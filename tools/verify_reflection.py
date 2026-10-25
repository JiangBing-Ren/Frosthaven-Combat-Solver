#!/usr/bin/env python3
"""
校验代码里的反射目标是否真的存在 —— 防止"反射打错类"这类静默失败。

背景（两次实战教训）：
  1. v1.18：`typeof(CAbility).GetProperty("MoveRestrictionType")` 拿到 null
     —— 该属性只定义在 CAbilityMove 上。结果 ReadMoveRestriction 永远返回 "None"，
     所有落脚限制（含 MustEndOnShadow）形同虚设，而编译通过、离线测试全绿。
  2. v1.17：脚本 rep 之后忘了写回文件，功能静默丢失。

这类失败的共同点是**没有任何日志**，只能靠实战截图发现。
本脚本在构建期做静态校验：把代码里用到的反射目标（类 + 属性/字段名）
拿去反编译产物里核对，缺一个就报错。

用法： python3 tools/verify_reflection.py
"""

import io
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
DECOMP = os.path.join(ROOT, 'decompiled')
SRC = os.path.join(ROOT, 'src')

# (声明该成员的类, 成员名, 成员种类) —— 与 src/ 里的反射调用一一对应
# 新增反射调用时，务必同步加到这里。
EXPECTED = [
    # 落脚限制（v1.18 修掉的 bug 就是这条打错了类）
    ('CAbilityMove', 'MoveRestrictionType', 'property'),
    # 强度随场上条件变化
    ('CAbility', 'StatIsBasedOnXEntries', 'property'),
    # 忽略视线
    ('CAbility', 'MiscAbilityData', 'property'),
    ('MiscAbilityData', 'IgnoreLOS', 'property'),   # AbilityData 的嵌套类，按类名索引
    ('CAbilityInfuse', 'ElementsToInfuse', 'property'),
    ('MiscAbilityData', 'CanTargetInvisible', 'property'),
    # 范围攻击形状
    ('CAbility', 'AreaEffectYMLString', 'property'),
    # 阴影
    ('CAbilityPerformFromShadow', 'm_ShadowsToUse', 'field'),
    ('CAbilityPerformFromShadow', 'm_RemoveWhenDone', 'field'),
    ('ScenarioState', 'ShadowProps', 'property'),
    ('CObjectShadow', 'WillBeRemoved', 'field'),
    # 条件
    ('CActor', 'm_Tokens', 'field'),
    ('CTokens', 'HasKey', 'method'),
    # 牌堆
    ('AttackModifierDeckState', 'AttackModifierCardsAvailable', 'property'),

    # 常驻加成（漂泊者的使用槽、各职业 persistent）
    ('CAbility', 'ActiveBonusData', 'property'),
    # ⚠ Behaviours / Duration 在 ActiveBonusData 上（代码里用 GetField），不是 CActiveBonus
    ('ActiveBonusData', 'Duration', 'field'),
    ('ActiveBonusData', 'Behaviours', 'field'),
    ('CActiveBonus', 'Remaining', 'property'),

    # StatIsBasedOnX 求值（v1.11 起用，算出「深渊力量」的真实攻击力）
    ('CAbility', 'GetStatIsBasedOnXValue', 'method'),
    ('StatIsBasedOnXData', 'AbilityStatType', 'property'),
    ('StatIsBasedOnXData', 'AddTo', 'property'),

    # 范围攻击
    ('CAbility', 'AreaEffect', 'property'),

    # ★ 这一击会**施加**给目标的条件（Dictionary<条件枚举, CAbility>）
    #   敌人先手给我上缴械就靠它
    ('CAbility', 'NegativeConditions', 'property'),

    # ★ 原生驱动：目标写在 CAbility 实例上（protected → 必须反射）
    ('CAbility', 'm_ActorsToTarget', 'field'),
    ('CAbility', 'm_TargetedActor', 'field'),

    ('CAbility', 'PositiveConditions', 'property'),

    # 牌堆预测补丁（M1.5 对拍验证）
    ('CActor', 'DrawAttackModifierCard', 'method'),

    # 战利品牌堆（P2）
    ('CLootDeck', 'AllCards', 'field'),
]

# 框架/引擎 API，不需要在反编译产物里核对
FRAMEWORK_MEMBERS = {
    'ConsoleVar_ShowTileIndex',   # Unity 引擎
    'Controls',                   # Unity Input System
    'ReadValue',                  # Unity Input System
    'Update',                     # Unity MonoBehaviour
}


def load_sources():
    """读所有反编译产物，按类名索引源码文本。"""
    by_class = {}
    for dp, dn, fn in os.walk(DECOMP):
        for f in fn:
            if not f.endswith('.cs'):
                continue
            path = os.path.join(dp, f)
            try:
                text = io.open(path, encoding='utf-8-sig', errors='replace').read()
            except Exception:
                continue
            # 文件内可能有多个类；按 "class X" 切分索引
            for m in re.finditer(r'\b(?:class|struct|enum)\s+(\w+)', text):
                by_class.setdefault(m.group(1), []).append(text)
            # 嵌套类用 A.B 形式
            for m in re.finditer(r'\bclass\s+(\w+)\s*\{', text):
                by_class.setdefault(m.group(1), []).append(text)
    return by_class


def find_member(by_class, cls, member, kind):
    """在指定类的源码里找成员定义。"""
    texts = by_class.get(cls)
    if not texts:
        return False, '类不存在'
    for text in texts:
        # 属性 / 字段
        if kind in ('property', 'field'):
            pat = (r'\bpublic\s+[\w<>\[\],\.\?\s]+?\s+' + re.escape(member)
                   + r'\s*(?:\{\s*get|\{|=>|;)')
            if re.search(pat, text):
                return True, ''
            # 简化再试一次（有些声明跨行）
            if re.search(r'\b' + re.escape(member) + r'\b', text) and kind == 'field':
                # 字段：m_Xxx 形式
                if re.search(r'\b(?:public|private|protected|internal)\s+[\w<>\[\],\.\?\s]+?\s+'
                             + re.escape(member) + r'\s*[;=]', text):
                    return True, ''
        elif kind == 'method':
            if re.search(r'\b' + re.escape(member) + r'\s*\(', text):
                return True, ''
    return False, '在 %s 里找不到该%s' % (cls, '属性' if kind == 'property' else
                                          '字段' if kind == 'field' else '方法')


def scan_source_reflections():
    """扫描 src/ 里所有反射取成员名的调用，返回 {成员名: [出现文件]}。"""
    found = {}
    for dp, dn, fn in os.walk(SRC):
        if 'obj' in dp or 'bin' in dp:
            continue
        for f in fn:
            if not f.endswith('.cs'):
                continue
            path = os.path.join(dp, f)
            try:
                text = io.open(path, encoding='utf-8-sig', errors='replace').read()
            except Exception:
                continue
            for m in re.finditer(r'\b(?:GetProperty|GetField|GetMethod)\s*\(\s*"([\w\.]+)"', text):
                found.setdefault(m.group(1), []).append(os.path.basename(path))
    return found


def main():
    by_class = load_sources()
    if not by_class:
        print('!! 没读到任何反编译产物（decompiled/ 为空？），跳过校验')
        return 0

    ok = 0
    bad = []
    for cls, member, kind in EXPECTED:
        found, why = find_member(by_class, cls, member, kind)
        if found:
            ok += 1
            print('  [OK] %s.%s (%s)' % (cls, member, kind))
        else:
            bad.append((cls, member, kind, why))
            print('  [XX] %s.%s (%s) —— %s' % (cls, member, kind, why))

    # ---- 反向检查：源码里用到的反射成员，是否都登记在 EXPECTED 里 ----
    used = scan_source_reflections()
    registered = {m for _, m, _ in EXPECTED}
    unregistered = sorted(n for n in used
                          if n not in registered and n not in FRAMEWORK_MEMBERS)
    if unregistered:
        print()
        print('!! 以下反射成员在源码里用到，但没登记到 EXPECTED 清单：')
        for n in unregistered:
            print('   - %s   (%s)' % (n, ', '.join(sorted(set(used[n])))))
        print('   → 新加反射调用时，请同步在 EXPECTED 里登记（类名 + 成员名 + 种类），')
        print('     否则它打错类时没人发现。')
        return 1
    else:
        print('  源码反射调用 %d 个，全部已登记' % len(used))

    print()
    print('反射目标校验：%d 通过，%d 失败' % (ok, len(bad)))

    if bad:
        print()
        print('!! 这些反射目标在反编译产物里找不到。常见原因：')
        print('   1. 属性/字段定义在**子类**上，代码却 typeof(基类) 去取 —— 正是 v1.18 的 bug')
        print('   2. 游戏版本升级后改名/移除')
        print('   3. 拼写错')
        print('   → 修法：按运行时实际类型反射（ab.GetType().GetProperty(...)），')
        print('     并给"读不到"加日志，别静默返回默认值。')
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())

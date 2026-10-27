#!/usr/bin/env python3
"""
对比**原版战斗战报**（实际）与我方**预测战报**，逐条找差异。

用法：
    python3 tools/compare_report.py [战报文件]

战报文件默认 D:/1Mods/FH/combat_report.tsv（插件每回合自动追加）。

战报格式（TSV）：
    类型(实际/预测)  回合  攻击者  目标  基础攻击  修正牌  伤害  备注

★ 前提：玩家**照着推荐打**，两条序列才应该一一对应。
  对不上的地方 = 模型误差的精确位置（比"感觉不智能"有用得多）。

输出：
    · 总览：实际 N 条 / 预测 M 条
    · 逐条差异：伤害不一致 / 修正牌不一致 / 基础攻击不一致 / 目标不一致
    · 按类型归纳：哪一类差异最多
"""

import io
import os
import re
import sys
from collections import defaultdict

DEFAULT = r'D:\1Mods\FH\combat_report.tsv'


def newest_report(directory, prefix='combat_report'):
    """v1.33 起战报按版本分文件（combat_report_v1.35.0.tsv）。
    没显式给路径时，优先用最新的版本化战报，避免误读旧文件。"""
    try:
        names = [n for n in os.listdir(directory)
                 if n.startswith(prefix) and n.endswith('.tsv')]
    except OSError:
        return None
    if not names:
        return None
    names.sort(key=lambda n: os.path.getmtime(os.path.join(directory, n)), reverse=True)
    return os.path.join(directory, names[0])


def load(path):
    rows = []
    if not os.path.exists(path):
        return rows
    version = ''
    with io.open(path, encoding='utf-8-sig', errors='replace') as f:
        for line in f:
            # ★ 战报分段头带插件版本号：混版时能分辨哪些行是旧版记的
            if line.startswith('# ==== 回合'):
                m = re.search(r'插件 v([\d.]+)', line)
                if m:
                    version = m.group(1)
                continue
            if line.startswith('#'):
                continue
            line = line.rstrip('\n')
            if not line.strip():
                continue
            parts = line.split('\t')
            if len(parts) < 7:
                continue
            rows.append({
                'version': version,
                'kind': parts[0],
                'round': parts[1],
                'attacker': parts[2],
                'target': parts[3],
                'base': parts[4],
                'mods': parts[5],
                'damage': parts[6],
                'note': parts[7] if len(parts) > 7 else '',
            })
    return rows


def norm_mods(s):
    """修正牌规范化：只留算式（+1 / *0 / -2），牌名（AMC_Basic_P0）无法映射 → 返回空。"""
    if not s:
        return ''
    out = []
    for tok in s.replace(',', ' ').split():
        t = tok.strip()
        if not t:
            continue
        if t.startswith('AMC_') or t.startswith('C'):   # 牌名，不是算式
            continue
        for op in ('+', '-', '*', '/'):
            if t.startswith(op) and t[1:].replace('.', '').isdigit():
                out.append(t)
                break
        else:
            if t.replace('.', '').isdigit():
                out.append(t)
    return ' '.join(out)


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else (
        newest_report(os.path.dirname(DEFAULT)) or DEFAULT)
    rows = load(path)
    if not rows:
        print('没有战报：%s' % path)
        print('（插件每回合会自动追加；跑一次战斗后重试）')
        return 0

    actual = [r for r in rows if r['kind'] == '实际']
    pred = [r for r in rows if r['kind'] == '预测']

    # ★ 版本混用检查：插件是启动时加载的，不重启就还是旧版
    versions = sorted(set(r['version'] for r in rows if r['version']))
    if len(versions) > 1:
        print('\n⚠⚠ 数据跨了 %d 个插件版本：%s' % (len(versions), '、'.join(versions)))
        print('   插件是**启动时加载**的 —— 部署新版后**必须重启游戏**才生效。')
        print('   混版数据不能用来判断模型对错，请用同一版本重打一局。')
    else:
        print('插件版本：%s' % (versions[0] if versions else '(无)'))

    print('战报文件：%s' % path)
    print('总条目 %d  →  实际 %d / 预测 %d' % (len(rows), len(actual), len(pred)))
    if not actual or not pred:
        print('\n（只有一侧有数据 —— 预测侧要按 Insert 求解才会有）')
        return 0

    # ---- 配对：先按**目标**匹配，匹配不上的再按顺序 ----
    #   按顺序配对在"玩家中途改了操作"时会全错位（用户实机就遇到过）。
    #   按目标配对更稳：同一次攻击的目标名应该一致。
    pairs = []
    used_pred = set()
    for a in actual:
        hit = -1
        for j, p in enumerate(pred):
            if j in used_pred:
                continue
            if a['target'] and p['target'] and a['target'] == p['target']:
                hit = j
                break
        if hit < 0:
            # 目标没记上（实际侧早期没记目标）→ 退化成按顺序
            hit = len(pairs)
            if hit < len(pred) and hit not in used_pred:
                used_pred.add(hit)
        else:
            used_pred.add(hit)
        pairs.append((a, pred[hit] if hit < len(pred) else None))

    n = len(pairs)
    diffs = []
    kinds = defaultdict(int)

    for i in range(n):
        a, p = pairs[i]
        if p is None:
            kinds['没有对应预测'] += 1
            diffs.append((i, a, None, ['实际侧多出（没有对应的预测）']))
            continue
        d = []
        if a['damage'] != p['damage']:
            d.append('伤害 实际%s vs 预测%s' % (a['damage'], p['damage']))
            kinds['伤害不一致'] += 1
        am, pm = norm_mods(a['mods']), norm_mods(p['mods'])
        if am != pm:
            d.append('修正牌 实际[%s] vs 预测[%s]' % (am, pm))
            kinds['修正牌不一致'] += 1
        if a['base'] != p['base']:
            d.append('基础攻击 实际%s vs 预测%s' % (a['base'], p['base']))
            kinds['基础攻击不一致'] += 1
        # 目标：**双方都有**才比 —— 实际侧在早期版本里没记上目标（时机太早），
        # 那时不该报"不一致"，而该报"实际侧缺目标"。
        if a['target'] and p['target']:
            if a['target'] != p['target']:
                d.append('目标 实际[%s] vs 预测[%s]' % (a['target'], p['target']))
                kinds['目标不一致'] += 1
        elif not a['target'] and p['target']:
            kinds['实际侧未记目标'] += 1
        if a['round'] != p['round']:
            d.append('回合 实际%s vs 预测%s' % (a['round'], p['round']))
            kinds['回合错位'] += 1
        if d:
            diffs.append((i, a, p, d))

    print('\n配对 %d 条，差异 %d 条' % (n, len(diffs)))
    if not diffs:
        print('  ★ 完全一致 —— 模型与原版结算吻合')
    else:
        print('\n按类型：')
        for k, v in sorted(kinds.items(), key=lambda kv: -kv[1]):
            print('  %-16s %d 次' % (k, v))
        print('\n前 20 条差异：')
        for i, a, p, d in diffs[:20]:
            print('  #%d  实际: %s→%s 基攻%s 牌[%s] 伤%s' %
                  (i + 1, a['attacker'], a['target'], a['base'], a['mods'], a['damage']))
            if p is not None:
                print('        预测: %s→%s 基攻%s 牌[%s] 伤%s' %
                      (p['attacker'], p['target'], p['base'], p['mods'], p['damage']))
            for x in d:
                print('        → ' + x)

    # 多出来的条目
    if len(actual) > n:
        print('\n实际多出 %d 条（预测侧没覆盖到的攻击）：' % (len(actual) - n))
        for r in actual[n:n + 10]:
            print('  %s→%s 基攻%s 牌[%s] 伤%s' %
                  (r['attacker'], r['target'], r['base'], r['mods'], r['damage']))
    if len(pred) > n:
        print('\n预测多出 %d 条（玩家没按推荐打这些）：' % (len(pred) - n))
        for r in pred[n:n + 10]:
            print('  %s→%s 基攻%s 牌[%s] 伤%s' %
                  (r['attacker'], r['target'], r['base'], r['mods'], r['damage']))
    return 0


if __name__ == '__main__':
    sys.exit(main())

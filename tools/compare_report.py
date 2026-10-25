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
import sys
from collections import defaultdict

DEFAULT = r'D:\1Mods\FH\combat_report.tsv'


def load(path):
    rows = []
    if not os.path.exists(path):
        return rows
    with io.open(path, encoding='utf-8-sig', errors='replace') as f:
        for line in f:
            if line.startswith('#'):
                continue
            line = line.rstrip('\n')
            if not line.strip():
                continue
            parts = line.split('\t')
            if len(parts) < 7:
                continue
            rows.append({
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
    """修正牌规范化：只留数字/符号，空格分隔，便于比较。"""
    if not s:
        return ''
    out = []
    for tok in s.replace(',', ' ').split():
        t = tok.strip()
        if not t:
            continue
        # 去掉牌名，只留算式部分（如 "+1" / "*0" / "-2"）
        for op in ('+', '-', '*', '/'):
            if t.startswith(op) and t[1:].replace('.', '').isdigit():
                out.append(t)
                break
        else:
            if t.replace('.', '').isdigit():
                out.append(t)
    return ' '.join(out)


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else DEFAULT
    rows = load(path)
    if not rows:
        print('没有战报：%s' % path)
        print('（插件每回合会自动追加；跑一次战斗后重试）')
        return 0

    actual = [r for r in rows if r['kind'] == '实际']
    pred = [r for r in rows if r['kind'] == '预测']

    print('战报文件：%s' % path)
    print('总条目 %d  →  实际 %d / 预测 %d' % (len(rows), len(actual), len(pred)))
    if not actual or not pred:
        print('\n（只有一侧有数据 —— 预测侧要按 Insert 求解才会有）')
        return 0

    # 按顺序配对（玩家照推荐打时应当一一对应）
    n = min(len(actual), len(pred))
    diffs = []
    kinds = defaultdict(int)

    for i in range(n):
        a, p = actual[i], pred[i]
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
        if a['target'] != p['target']:
            d.append('目标 实际[%s] vs 预测[%s]' % (a['target'], p['target']))
            kinds['目标不一致'] += 1
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

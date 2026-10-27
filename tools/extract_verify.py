#!/usr/bin/env python3
"""
阶段 2 附属：**A1 验证用扩展集**提取（ChaosDemon）。

★ 定位（用户裁决）：
    ChaosDemon **只进验证用扩展集，不进阶段 1 的 Data/ 主集**。
    产出落在 Data/verify/ 下，manifest.json 标注 purpose = "A1 verification material"。
    这样阶段 3 的最小闭环仍只用原 4 个怪物（AlgoxScout×2 + AbaelHerder×2），不被扩展集污染；
    阶段 4 做 hook 验证时再引用本扩展集。

为什么选 ChaosDemon（材料选择依据）：
    · ChaosDemonID 的 Attack：L0 = 2，L1 = 3 —— **相邻等级即可区分**，
      远优于 AlgoxScout（L0=L1=L2=2，只有 L3 才变 3，需跳场景等级，变量更多）。
    · 其 8 张卡中 6 张含哨兵，且**同一字段出现了 -1 与 -2 两个值**（见 sentinel_survey），
      这本身就是"同语义多值"的直接证据，可构成一条**不依赖跨等级**的干净对照。

产出：
    Data/verify/chaosdemon.json        怪物定义（普通/精英，含等级表）
    Data/verify/chaosdemon_cards.json  8 张行动卡（结构 + raw/resolved 双字段）
    Data/verify/verify_manifest.json   本扩展集的元信息（purpose、哨兵分布、验证用例建议）
"""

import io
import json
import os
import glob
import datetime
import yaml

# 复用主提取脚本的解析逻辑，保证结构同构
from extract_rules import (
    extract_ability, extract_abilities_dict, flat, num, rel, find_line,
    walk_abilities, resolve_value, SENTINEL_TO_STAT,
)

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, 'extracted', 'Base_ruleset')
OUT = os.path.join(ROOT, 'Data', 'verify')

MONSTER = 'ChaosDemon'
CARD_IDS = list(range(538, 546))   # 538..545


def extract_monster():
    out = []
    d = os.path.join(SRC, 'Campaign', 'Enemies', 'Monsters', MONSTER)
    for path in sorted(glob.glob(os.path.join(d, '*.yml'))):
        y = yaml.safe_load(io.open(path, encoding='utf-8').read())
        if not isinstance(y, dict) or y.get('Parser') != 'Enemy':
            continue
        levels = {}
        for lvl in range(0, 8):
            node = y.get('Level%d' % lvl)
            if not isinstance(node, dict):
                continue
            levels[str(lvl)] = {
                'Health': num(node.get('Health')),
                'Move': num(node.get('Move')),
                'Attack': num(node.get('Attack')),
                'Range': num(node.get('Range')),
                'Target': num(node.get('Target')),
                'Conditions': node.get('Conditions') or [],
            }
        out.append({
            '_source': {'file': rel(path), 'line': find_line(path, 'ID')},
            'Id': y.get('ID'),
            'LocKey': y.get('LocKey'),
            'MonsterType': y.get('MonsterType'),
            'StandeeLimit': num(y.get('StandeeLimit')),
            'AbilityCards': y.get('AbilityCards') or [],
            'Levels': levels,
        })
    return out


def survey_sentinels(cards_raw):
    """统计各卡各字段的哨兵值分布 —— A1 的"同语义多值"证据链。"""
    by_field = {}
    per_card = []
    for c in cards_raw:
        entries = []
        for fpath, field, raw in walk_abilities(c.get('Data') or {}):
            r = num(raw, default=None)
            if r is None or r >= 0:
                continue
            entries.append({'path': fpath, 'field': field, 'raw': r})
            by_field.setdefault(field, {}).setdefault(str(r), []).append(c.get('ID'))
        if entries:
            per_card.append({'cardId': c.get('ID'), 'sentinels': entries})
    return by_field, per_card


def main():
    os.makedirs(OUT, exist_ok=True)

    monsters = extract_monster()

    # ---- 行动卡 ----
    base = os.path.join(SRC, 'Campaign', 'Enemies', 'Monsters', MONSTER, 'Cards')
    raw_by_id = {}
    paths_by_id = {}
    for path in sorted(glob.glob(os.path.join(base, '*.yml'))):
        y = yaml.safe_load(io.open(path, encoding='utf-8').read())
        if not isinstance(y, dict):
            continue
        cid = num(y.get('ID'))
        if cid not in CARD_IDS:
            continue
        raw_by_id[cid] = y
        paths_by_id[cid] = path

    stat_idx = {}
    for m in monsters:
        stat_idx[m['Id']] = m['Levels']

    cards = []
    for cid in sorted(raw_by_id):
        y = raw_by_id[cid]
        path = paths_by_id[cid]
        raw_values = []
        resolved = {}
        for m_id, lv_table in stat_idx.items():
            by_level = {}
            for level in range(0, 8):
                if str(level) not in lv_table:
                    continue
                per_field = {}
                for fpath, field, raw in walk_abilities(y.get('Data') or {}):
                    per_field[fpath] = resolve_value(field, raw, lv_table, level)
                by_level[str(level)] = per_field
            resolved[m_id] = by_level
        for fpath, field, raw in walk_abilities(y.get('Data') or {}):
            raw_values.append({'path': fpath, 'field': field,
                               'raw': num(raw, default=None)})
        cards.append({
            '_source': {'file': rel(path), 'line': find_line(path, 'ID')},
            'Id': num(y.get('ID')),
            'LocKey': y.get('Name'),
            'Initiative': num(y.get('Initiative')),
            'Shuffle': bool(y.get('Shuffle')),
            'UsedBy': sorted(stat_idx.keys()),
            'RawValues': raw_values,
            'ResolvedValues': resolved,
            'Abilities': extract_abilities_dict((y.get('Data') or {}).get('Abilities')),
        })

    by_field, per_card = survey_sentinels([raw_by_id[c] for c in sorted(raw_by_id)])

    def dump(name, obj):
        p = os.path.join(OUT, name)
        with io.open(p, 'w', encoding='utf-8', newline='\n') as f:
            json.dump(obj, f, ensure_ascii=False, indent=2)
        return rel(p)

    files = [
        dump('chaosdemon.json', monsters),
        dump('chaosdemon_cards.json', cards),
    ]

    # 相邻等级属性差（用于判定"能否区分"）
    diffs = []
    for m in monsters:
        lv = m['Levels']
        if '0' in lv and '1' in lv:
            for f in ('Health', 'Move', 'Attack', 'Range', 'Target'):
                a, b = lv['0'].get(f), lv['1'].get(f)
                diffs.append({'monster': m['Id'], 'field': f, 'L0': a, 'L1': b,
                              'distinguishable': a != b})

    manifest = {
        'purpose': 'A1 verification material',
        'note': '本扩展集**仅供阶段 4 的 hook 验证引用**，不参与阶段 3 最小闭环。'
                '阶段 3 只用 Data/ 主集的 4 个怪物（AlgoxScout×2 + AbaelHerder×2）。',
        'generated_at': datetime.datetime.now().isoformat(timespec='seconds'),
        'monster': MONSTER,
        'card_ids': CARD_IDS,
        'counts': {'monsters': len(monsters), 'cards': len(cards)},
        'files': files,
        'L0_vs_L1_distinguishable': diffs,
        'sentinel_survey': {
            'by_field': by_field,
            'per_card': per_card,
            'interpretation': '若同一 field 出现了多个不同的 raw 值（例如 Move 同时有 -1 与 -2），'
                              '则这些值在本体语义上应当等价（都表示"取怪物自身属性"）；'
                              '这构成一条**不依赖跨等级对比**的验证路径。',
        },
        'verification_cases': [
            {
                'id': 'A1-C1',
                'title': '跨等级对比（Attack 哨兵）',
                'card': 539, 'field': 'AAttack.Attack', 'sentinel': -1,
                'levels': [0, 1],
                'expected_if_A': 'L0 最终攻击 = 2，L1 = 3（随 ChaosDemon 等级表的 Attack 变化）',
                'falsifies_A_if': '两级攻击恒为某个硬编码值、不随等级表变化',
            },
            {
                'id': 'A1-C2',
                'title': '同等级同义多值对比（Move 哨兵 -1 vs -2）',
                'cards': [538, 540], 'field': 'AMove.Move', 'sentinels': [-1, -2],
                'levels': [0],
                'expected_if_A': '两张卡在同一 Level 下实际移动力**相同**，均 = ChaosDemon 的 Move(3)',
                'falsifies_A_if': '两张卡移动力不同（说明 -1 与 -2 语义不同）',
                'why_cleaner': '不需要改场景等级，变量更少；直接在同等条件下比较两个哨兵值',
            },
        ],
    }
    files.append(dump('verify_manifest.json', manifest))

    print('=== A1 验证扩展集（ChaosDemon）提取完成 ===')
    print('  monsters: %d    cards: %d' % (len(monsters), len(cards)))
    print('\nL0 vs L1 是否可区分：')
    for d in diffs:
        print('  %-22s %-7s L0=%-3s L1=%-3s -> %s'
              % (d['monster'], d['field'], d['L0'], d['L1'],
                 '可区分' if d['distinguishable'] else '不可区分'))
    print('\n哨兵值分布（按字段）：')
    for f, vals in by_field.items():
        print('  %-8s %s' % (f, ', '.join('%s→卡%s' % (v, c) for v, c in sorted(vals.items()))))
    print('\n产出：')
    for f in files:
        print('  ' + f)


if __name__ == '__main__':
    main()

#!/usr/bin/env python3
"""
阶段 1：把明文规则数据提取为结构化 JSON（一次性清洗脚本，**不进主引擎**）。

事实源（只读）：
    D:\\1Mods\\FH\\extracted\\Base_ruleset\\**\\*.yml
    （该目录由 tools/ruleset.py 从 Base.ruleset 解出，字节级无损）

产出（写入 D:\\1Mods\\FH\\Data\\）：
    cards.json             角色能力卡（子集：BannerSpear + Drifter）
    monsters.json          怪物（子集：AlgoxScout + AbaelHerder，含等级表）
    monster_cards.json     怪物行动卡（子集引用的卡，含 rawValue / resolvedValue 双字段）
    attack_modifiers.json  攻击修正牌（140 全量）
    deck_membership.json   牌组 → 引用的 AMC（名称 + 数量）
    manifest.json          提取元信息（条数、来源路径、时间戳）

设计要点：
  · 每条记录都带 _source = { "file": 相对路径, "line": 关键字段行号 }，可逐字段溯源。
  · monster_cards 的数值字段输出 raw / resolved 双份：
      raw      = yml 原值（可能是 -1 / -2 哨兵）
      resolved = 已代入怪物属性的最终值（按怪物 × 等级展开）
    下游**只用 resolved**；raw 仅用于溯源。
    ⚠ resolved 的哨兵语义尚未在本体反编译产物中定位到处理点，见 manifest 的 unresolved。
  · 不提取本地化文本（按项目约束），统一用 ID / locKey 作标识。
"""

import io
import json
import os
import glob
import datetime
import yaml

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, 'extracted', 'Base_ruleset')
OUT = os.path.join(ROOT, 'Data')

# ---- 阶段 1 子集范围（由用户指定）----
CHARACTERS = ['BannerSpear', 'Drifter']
MONSTERS = ['AlgoxScout', 'AbaelHerder']
DECKS = ['Standard', 'ReducedRandomness', 'MonsterDeck', 'MonsterReducedRandomness']


def rel(p):
    """转成相对仓库根的路径（统一用 /）。"""
    return os.path.relpath(p, ROOT).replace('\\', '/')


def read_yaml(path):
    with io.open(path, encoding='utf-8') as f:
        return yaml.safe_load(f.read())


def find_line(path, key):
    """返回 key 在文件中首次出现的行号（1-based）；找不到返回 -1。"""
    try:
        with io.open(path, encoding='utf-8') as f:
            for i, line in enumerate(f, 1):
                stripped = line.strip()
                if stripped.startswith(key + ':') or stripped.startswith('- ' + key + ':'):
                    return i
    except OSError:
        pass
    return -1


def num(v, default=0):
    """把 yml 里的值转成 int（可能带引号或 None）。"""
    if v is None:
        return default
    if isinstance(v, bool):
        return default
    if isinstance(v, (int, float)):
        return int(v)
    try:
        return int(str(v).strip())
    except (ValueError, TypeError):
        return default


# ============================================================
# 1. 角色能力卡
# ============================================================
def flat(v):
    """
    把 yml 里的**异构值**统一成字符串。

    为什么必须做：Filter / Conditions / Targeting 这类字段在本体 yml 里
    「有时是标量、有时是数组、有时是嵌套 dict」—— 下游若用通用 JSON 反序列化，
    会在类型不固定时崩溃（实测 JavaScriptSerializer 报
    “数组的反序列化不支持类型 System.String”）。
    在提取层统一类型，是数据清洗的本职，也让 C# 侧模型保持简单。
      · 标量 → str
      · 数组 → 用 | 连接
      · dict → JSON 文本
      · None → ""
    """
    if v is None:
        return ""
    if isinstance(v, bool):
        return "true" if v else "false"
    if isinstance(v, (int, float)):
        return str(v)
    if isinstance(v, str):
        return v
    if isinstance(v, list):
        return "|".join(flat(x) for x in v)
    if isinstance(v, dict):
        return json.dumps(v, ensure_ascii=False, sort_keys=True)
    return str(v)


def extract_ability(node):
    """把一个能力节点（Attack/Move/Heal/Summon/...）抽成结构化 dict。"""
    if not isinstance(node, dict):
        return None
    a = {}
    for k in ('Attack', 'Move', 'Heal', 'Damage', 'Range', 'Target',
              'Shield', 'Retaliate', 'Summon', 'Conditions', 'AreaEffect',
              'AreaEffectLayoutOverride', 'Filter', 'Targeting',
              'MultiPassAttack', 'AnimOverload', 'XP'):
        if k in ('Move', 'Heal', 'Damage', 'Range', 'Target',
                 'Shield', 'Retaliate', 'Attack'):
            continue  # 这些已在上面按数值语义处理
        if k in node:
            a[k] = flat(node[k])
    # Attack 可能是 dict {Amount: N} 或标量 → 统一成数值
    if isinstance(node.get('Attack'), dict):
        a['AttackAmount'] = num(node['Attack'].get('Amount'))
    elif 'Attack' in node:
        a['AttackAmount'] = num(node['Attack'])
    else:
        a['AttackAmount'] = None
    # 数值字段保留数值语义（这些在 yml 里是标量；缺项写 None）
    for f in ('Move', 'Heal', 'Damage', 'Range', 'Target', 'Shield', 'Retaliate'):
        a[f] = num(node[f]) if f in node else None
    # SubAbilities 递归
    subs = node.get('SubAbilities')
    if isinstance(subs, dict):
        a['SubAbilities'] = {}
        for sk, sv in subs.items():
            got = extract_ability(sv)
            if got:
                a['SubAbilities'][sk] = got

    # ★ 嵌套能力组递归 —— 例如 ControlActor: { Abilities: { AllyMove, AllyAttack, ... } }。
    #   这类嵌套**不走 SubAbilities 键**，而是挂在功能键（ControlActor）内部。
    #   阶段 3 的 AI 需要知道"ControlActor 让被控单位做什么"，必须递归到这一层。
    for k, v in node.items():
        if not isinstance(v, dict) or k == 'SubAbilities':
            continue
        inner = v.get('Abilities')
        if isinstance(inner, dict) and inner:
            a.setdefault('Nested', {})[k] = extract_abilities_dict(inner)
    # 只关心"做什么"的能力类型名
    for type_key in ('Move', 'Attack', 'Heal', 'Summon', 'Damage', 'Shield',
                     'Retaliate', 'ControlActor', 'Push', 'Pull', 'Pierce',
                     'Curse', 'Bless', 'Strengthen', 'Muddle', 'Impair',
                     'Wound', 'Poison', 'Regenerate', 'Ward', 'Brittle',
                     'InfuseElement', 'ConsumeElement', 'Loot', 'MoveShadow',
                     'CreateShadow', 'RemoveShadow', 'Summon', 'Teleport'):
        if type_key in node:
            a.setdefault('Types', []).append(type_key)
    return a or None


def extract_abilities_dict(abilities):
    """
    把 yml 的 Abilities 字典转成与 **cards.json 同构**的递归结构
    { 能力名: Ability节点 }。

    ⚠ 为什么不能用 flat()：阶段 3 的怪物 AI 纯函数需要判断
      “这张卡有没有移动图标 / 有没有攻击图标 / 有几个能力节点 /
       ControlActor 嵌套了什么”，这类**布尔与结构信息**只能从能力树读；
      扁平化成字符串后，下游要么二次解析（脆），要么根本拿不到。
    """
    out = {}
    if not isinstance(abilities, dict):
        return out
    for name, node in abilities.items():
        got = extract_ability(node)
        if got:
            out[name] = got
    return out


def extract_half(half_node):
    """抽取一个半边（Top / Bottom）。"""
    if not isinstance(half_node, dict):
        return {'Abilities': {}, 'XP': 0}
    out = {'Abilities': {}, 'XP': num(half_node.get('XP'))}
    abilities = half_node.get('Abilities') or {}
    if isinstance(abilities, dict):
        for name, node in abilities.items():
            got = extract_ability(node)
            if got:
                out['Abilities'][name] = got
    # 常驻加成（ActiveBonus）单独标记
    bonus = half_node.get('ActiveBonus')
    if bonus:
        out['ActiveBonus'] = bonus
    return out


def extract_cards():
    cards = []
    for ch in CHARACTERS:
        d = os.path.join(SRC, 'Global', 'Characters', ch, 'AbilityCards')
        for path in sorted(glob.glob(os.path.join(d, '*.yml'))):
            y = read_yaml(path)
            if not isinstance(y, dict):
                continue
            data = y.get('Data') or {}
            cards.append({
                '_source': {'file': rel(path),
                            'line': find_line(path, 'ID'),
                            'line_initiative': find_line(path, 'Initiative')},
                'Id': num(y.get('ID')),
                'LocKey': y.get('Name'),
                'Character': y.get('Character'),
                'Level': num(y.get('Level')),
                'Initiative': num(y.get('Initiative')),
                'Discard': y.get('Discard'),          # [上半去向, 下半去向]
                'Top': extract_half(data.get('Top')),
                'Bottom': extract_half(data.get('Bottom')),
            })
    return cards


# ============================================================
# 2. 怪物（含等级表）
# ============================================================
def extract_monsters():
    monsters = []
    for name in MONSTERS:
        d = os.path.join(SRC, 'Campaign', 'Enemies', 'Monsters', name)
        for path in sorted(glob.glob(os.path.join(d, '*.yml'))):
            y = read_yaml(path)
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
            monsters.append({
                '_source': {'file': rel(path),
                            'line': find_line(path, 'ID'),
                            'line_abilitycards': find_line(path, 'AbilityCards')},
                'Id': y.get('ID'),
                'LocKey': y.get('LocKey'),
                'MonsterType': y.get('MonsterType'),
                'StandeeLimit': num(y.get('StandeeLimit')),
                'PredominantlyMelee': bool(y.get('PredominantlyMelee')),
                'NonEliteVariant': y.get('NonEliteVariant'),
                'AbilityCards': y.get('AbilityCards') or [],
                'Levels': levels,
            })
    return monsters


# ============================================================
# 3. 怪物行动卡（含 raw / resolved 双字段）
# ============================================================
# 哨兵值：yml 里写负数时，语义为"取怪物自身属性"。
#   ⚠ 具体映射（-1 → ? / -2 → ?）尚未在反编译产物中定位到处理点，
#     见 manifest.json 的 unresolved.sentinel_values。
#   本脚本按"字段名对应属性名"给出 resolved，并同时保留 raw 供修正。
SENTINEL_TO_STAT = {'Move': 'Move', 'Attack': 'Attack', 'Range': 'Range',
                    'Target': 'Target', 'Damage': 'Attack'}


def build_monster_stat_index(monsters):
    """monsterId → level → stat → value"""
    idx = {}
    for m in monsters:
        idx[m['Id']] = m['Levels']
    return idx


def resolve_value(field, raw, monster_levels, level):
    """把 raw 解析为该怪物该等级下的最终值。"""
    if raw is None:
        return None
    r = num(raw, default=None)
    if r is None:
        return None
    if r >= 0:
        return r
    stat = SENTINEL_TO_STAT.get(field)
    if not stat or not monster_levels:
        return r
    lv = monster_levels.get(str(level), {})
    return num(lv.get(stat), default=r)


def walk_abilities(node, path=''):
    """遍历能力树，产出 (字段路径, 字段名, raw值) 三元组。"""
    out = []
    if not isinstance(node, dict):
        return out
    for k, v in node.items():
        p = ('%s.%s' % (path, k)) if path else k
        if k in SENTINEL_TO_STAT and isinstance(v, (int, float, str)):
            out.append((p, k, v))
        if isinstance(v, dict):
            out.extend(walk_abilities(v, p))
        if isinstance(v, list):
            for i, item in enumerate(v):
                if isinstance(item, dict):
                    out.extend(walk_abilities(item, '%s[%d]' % (p, i)))
    return out


def extract_monster_cards(monsters):
    """按怪物引用的卡 ID 提取（卡是共享的，按 ID 而非目录）。"""
    stat_idx = build_monster_stat_index(monsters)
    # 卡 ID → 文件
    card_files = {}
    base = os.path.join(SRC, 'Campaign', 'Enemies', 'Monsters')
    for path in glob.glob(os.path.join(base, '*', 'Cards', '*.yml')):
        fname = os.path.basename(path)
        m = fname.split('_')[0]
        if m.isdigit():
            card_files.setdefault(int(m), []).append(path)

    # 该卡被哪些怪物引用（子集内）
    want_ids = {}
    for m in monsters:
        for cid in m['AbilityCards']:
            want_ids.setdefault(num(cid), []).append(m['Id'])

    cards = []
    for cid in sorted(want_ids):
        paths = card_files.get(cid)
        if not paths:
            continue
        path = paths[0]
        y = read_yaml(path)
        if not isinstance(y, dict):
            continue
        raw_values = []
        resolved_values = {}
        for m_id in want_ids[cid]:
            lv_table = stat_idx.get(m_id, {})
            by_level = {}
            for level in range(0, 8):
                if str(level) not in lv_table:
                    continue
                per_field = {}
                for fpath, field, raw in walk_abilities(y.get('Data') or {}):
                    per_field[fpath] = resolve_value(field, raw, lv_table, level)
                by_level[str(level)] = per_field
            resolved_values[m_id] = by_level
        for fpath, field, raw in walk_abilities(y.get('Data') or {}):
            raw_values.append({'path': fpath, 'field': field, 'raw': num(raw, default=None)})

        cards.append({
            '_source': {'file': rel(path),
                        'line': find_line(path, 'ID'),
                        'line_initiative': find_line(path, 'Initiative')},
            'Id': num(y.get('ID')),
            'LocKey': y.get('Name'),
            'Initiative': num(y.get('Initiative')),
            'Shuffle': bool(y.get('Shuffle')),
            'UsedBy': sorted(set(want_ids[cid])),
            'RawValues': raw_values,          # 溯源用
            'ResolvedValues': resolved_values,  # 下游**只用这个** {monsterId: {level: {path: value}}}
            'Abilities': extract_abilities_dict((y.get('Data') or {}).get('Abilities')),
        })
    return cards


# ============================================================
# 4. 攻击修正牌 + 牌组归属
# ============================================================
def extract_modifiers_and_decks():
    mods = []
    for path in sorted(glob.glob(os.path.join(SRC, 'Global', 'AttackModifiers', '*.yml'))):
        y = read_yaml(path)
        if not isinstance(y, dict):
            continue
        mods.append({
            '_source': {'file': rel(path),
                        'line': find_line(path, 'Name'),
                        'line_math': find_line(path, 'MathModifier')},
            'Name': y.get('Name'),
            'MathModifier': y.get('MathModifier'),
            'Rolling': bool(y.get('Rolling')),
            'Shuffle': bool(y.get('Shuffle')),
            'IsCurse': bool(y.get('IsCurse')),
            'IsBless': bool(y.get('IsBless')),
        })

    decks = []
    membership = {}
    for name in DECKS:
        path = os.path.join(SRC, 'Global', 'AttackModifierDecks', name + '.yml')
        if not os.path.exists(path):
            continue
        y = read_yaml(path) or {}
        entries = []
        for item in (y.get('AttackModifierCards') or []):
            if isinstance(item, list) and len(item) >= 2:
                entries.append({'Name': item[0], 'Count': num(item[1])})
        decks.append({
            '_source': {'file': rel(path), 'line': find_line(path, 'AttackModifierCards')},
            'Name': y.get('Name'),
            'IsMonsterDeck': bool(y.get('IsMonsterDeck')),
            'Cards': entries,
            'TotalCards': sum(e['Count'] for e in entries),
        })
        membership[y.get('Name') or name] = {
            'IsMonsterDeck': bool(y.get('IsMonsterDeck')),
            'Cards': entries,
            'TotalCards': sum(e['Count'] for e in entries),
            '_source': rel(path),
        }

    # 统计：140 个 AMC 里有多少进了牌组
    all_names = set(m['Name'] for m in mods)
    used = set()
    for d in decks:
        for e in d['Cards']:
            used.add(e['Name'])
    return mods, decks, membership, sorted(all_names), sorted(used)


# ============================================================
# main
# ============================================================
def main():
    os.makedirs(OUT, exist_ok=True)

    cards = extract_cards()
    monsters = extract_monsters()
    monster_cards = extract_monster_cards(monsters)
    mods, decks, membership, all_amc, used_amc = extract_modifiers_and_decks()

    def dump(name, obj):
        p = os.path.join(OUT, name)
        with io.open(p, 'w', encoding='utf-8', newline='\n') as f:
            json.dump(obj, f, ensure_ascii=False, indent=2)
        return os.path.relpath(p, ROOT).replace('\\', '/')

    written = [
        dump('cards.json', cards),
        dump('monsters.json', monsters),
        dump('monster_cards.json', monster_cards),
        dump('attack_modifiers.json', mods),
        dump('modifier_decks.json', decks),
        dump('deck_membership.json', membership),
    ]

    manifest = {
        'phase': 1,
        'generated_at': datetime.datetime.now().isoformat(timespec='seconds'),
        'source_root': 'extracted/Base_ruleset',
        'original_archive': 'D:/Steam/steamapps/common/Frosthaven/Frosthaven_Data/StreamingAssets/Rulebase/Base.ruleset',
        'archive_format': {
            'magic': '02392fe1',
            'magic_encoding': 'ASCII (8 bytes)',
            'bytes_8_11': 'u32 = 0 (reserved, meaning unconfirmed)',
            'bytes_12_15': 'u32 entryCount (LE) = 3397',
            'toc_entry_bytes': 144,
            'toc_layout': '128B path(UTF-8,space-padded) + u32 meta1 + u32 meta2 + u32 offset + u32 size',
            'verified': 'dataOffset 489184 = 16 + 3397*144',
        },
        'subset': {'characters': CHARACTERS, 'monsters': MONSTERS, 'decks': DECKS},
        'counts': {
            'cards': len(cards),
            'monsters': len(monsters),
            'monster_cards': len(monster_cards),
            'attack_modifiers': len(mods),
            'modifier_decks': len(decks),
            'amc_in_any_deck': len(used_amc),
            'amc_not_in_any_deck': len(set(all_amc) - set(used_amc)),
        },
        'files': written,
        'characters_full_enumeration': {
            'total': 17,
            'note': '17 个职业目录全部含 AbilityCards（27–34 张），均判定为可玩；未发现 NPC / 未启用职业。',
            'list': sorted(os.listdir(os.path.join(SRC, 'Global', 'Characters'))),
        },
        'unresolved': {
            'sentinel_values': {
                'evidence': [
                    'extracted/.../Scout/Cards/770_Scout_CruelBow.yml 等处出现 Move: -1',
                    'extracted/.../AbaelHerder/Cards/779_*.yml 出现 Attack: -2',
                    'decompiled/SRL/ScenarioRuleLibrary/CMonsterClass.cs:233-234 有 '
                    'CreateDefaultAttack(m_CurrentMonsterStat.Attack, ...) / CreateDefaultMove(m_CurrentMonsterStat.Move, ...)',
                ],
                'candidate_a': '负数 = 取怪物自身同名属性（Move:-1 → 怪物 Move；Attack:-2 → 怪物 Attack）',
                'candidate_b': '负数 = 未指定，运行时由 MonsterClass 默认能力填充（CMonsterClass.cs:233-234）',
                'candidate_c': '负数 = 相对修正（如怪物 Attack 上 -2），实测会导致 0 伤害，与观察不符',
                'current_choice': '本脚本按 candidate_a 生成 resolved，raw 同时保留以便修正',
                'affects_tests': ['怪物移动力', '怪物攻击力', '伤害计算', 'AI 纯函数输出'],
            },
            'lvldat_parsing': '关卡 .lvldat 为 .NET BinaryFormatter，需要 C# + 游戏程序集才能解析；阶段 1 未产出。',
            'baseline_scenario': '“烈焰焚城”使命的内部 ID 未能从明文数据确定（场景为 .lvldat 二进制且本地化不提取）。',
        },
    }
    written.append(dump('manifest.json', manifest))

    print('=== 阶段 1 提取完成 ===')
    for k, v in manifest['counts'].items():
        print('  %-22s %d' % (k, v))
    print('\n产出文件：')
    for f in written:
        print('  ' + f)
    print('\n未决问题：')
    for k in manifest['unresolved']:
        print('  - ' + k)


if __name__ == '__main__':
    main()

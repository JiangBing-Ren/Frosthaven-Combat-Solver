#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
遍历 Global/AttackModifiers 下的全部修正牌定义，输出分类汇总。

用法：
    python tools/modifier_catalog.py            # 汇总
    python tools/modifier_catalog.py -v         # 逐张列出
"""
import os
import re
import sys
from collections import Counter, defaultdict

D = r"D:/1Mods/FH/extracted/Base_ruleset/Global/AttackModifiers"

SCALAR_KEYS = [
    "Name", "MathModifier", "AlternateMathModifier", "Shuffle", "Rolling",
    "AddTarget", "IsCurse", "IsBless", "IsConditionalModifier",
    "DamageIfTargetMoves", "TryToPerformAsSummoner", "ControlTargetingActor",
    "UseGenericIcon", "OriginalModifier",
]


def parse(path):
    txt = open(path, encoding="utf-8", errors="replace").read().replace("\r\n", "\n")
    row = {k: None for k in SCALAR_KEYS}
    for k in SCALAR_KEYS:
        m = re.search(r"^%s:\s*(.*)$" % k, txt, re.M)
        if m:
            row[k] = m.group(1).strip()

    # Abilities 下的能力类型（4 空格缩进的顶层键）
    abilities = []
    m = re.search(r"^Abilities:\s*$(.*?)(?=^[A-Za-z_]+:|\Z)", txt, re.M | re.S)
    if m:
        abilities = re.findall(r"^    ([A-Za-z]\w*):", m.group(1), re.M)

    # Overrides 里的条件名
    overrides = []
    m2 = re.search(r"^AbilityOverrides:\s*$(.*?)(?=^[A-Za-z_]+:|\Z)", txt, re.M | re.S)
    if m2:
        overrides = re.findall(r"^    ([A-Za-z]\w*):", m2.group(1), re.M)
    # Overrides: 之后紧跟的 Conditions
    conditions = re.findall(r"Conditions:\s*\[([^\]]*)\]", txt)

    row["Abilities"] = abilities
    row["Overrides"] = overrides
    row["Conditions"] = [c.strip() for c in conditions]
    row["File"] = os.path.basename(path)
    return row


def main():
    files = sorted(f for f in os.listdir(D) if f.endswith(".yml"))
    rows = [parse(os.path.join(D, f)) for f in files]

    verbose = "-v" in sys.argv

    print("修正牌总数: %d\n" % len(rows))

    # 1) 按数学修正归类
    print("=== 按 MathModifier 分布 ===")
    for k, v in sorted(Counter(r["MathModifier"] for r in rows).items(),
                       key=lambda x: -x[1]):
        print("  %-6s %d" % (k, v))

    # 2) 标志位统计
    print("\n=== 标志位 ===")
    for flag in ["Shuffle", "Rolling", "AddTarget", "IsCurse", "IsBless",
                 "IsConditionalModifier"]:
        n = sum(1 for r in rows if (r[flag] or "").lower() == "true")
        print("  %-24s %d" % (flag, n))

    # 3) 附加能力分布
    print("\n=== 附加能力（Abilities）分布 ===")
    c = Counter()
    for r in rows:
        for a in r["Abilities"]:
            c[a] += 1
    for k, v in sorted(c.items(), key=lambda x: -x[1]):
        print("  %-28s %d" % (k, v))

    # 4) 条件覆盖
    print("\n=== 条件覆盖（Conditions）分布 ===")
    c2 = Counter()
    for r in rows:
        for cond in r["Conditions"]:
            for part in cond.split(","):
                part = part.strip()
                if part:
                    c2[part] += 1
    for k, v in sorted(c2.items(), key=lambda x: -x[1]):
        print("  %-28s %d" % (k, v))

    # 5) 按名字前缀族分类
    print("\n=== 名字族（前缀）分布 ===")
    fam = Counter()
    for r in rows:
        name = r["Name"] or r["File"][:-4]
        fam[name.split("_")[0]] += 1
    for k, v in sorted(fam.items(), key=lambda x: -x[1]):
        print("  %-20s %d" % (k, v))

    if verbose:
        print("\n=== 逐张明细 ===")
        for r in rows:
            flags = []
            for f in ["Shuffle", "Rolling", "AddTarget", "IsCurse", "IsBless",
                      "IsConditionalModifier"]:
                if (r[f] or "").lower() == "true":
                    flags.append(f)
            abil = ",".join(r["Abilities"]) if r["Abilities"] else ""
            cond = ",".join(r["Conditions"]) if r["Conditions"] else ""
            print("  %-38s %-5s %-22s %-18s %s" % (
                r["Name"], r["MathModifier"], ",".join(flags), abil, cond))


if __name__ == "__main__":
    main()

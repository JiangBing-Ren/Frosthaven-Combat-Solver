#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
霜港迷城 — 攻击修正牌日志溯源工具

从 Player.log / Player-prev.log 还原真实的抽牌序列，用于验证抽牌是否确定性。

用法:
    python tools/modifier_trace.py [日志文件路径]

不带参数时默认读取 Player-prev.log（游戏重启会把旧日志轮转为 Player-prev.log，
新 Player.log 往往很小，分析前先确认用哪个文件）。

输出:
    1. 抽牌事件数与表达式日志数
    2. 修正牌面序列（按每 10 张分组）
    3. 撤销点前后的牌面对比
    4. 重复连续块检测（相邻重复 = 撤销后重来得到同一张牌）

已知陷阱:
    - 一次抽牌会把同一条表达式打印约 27 次，必须先用
      "Drawing attack modifiers. OverallAdvantage" 事件切分，再取该事件后第一条
    - 表达式尾部的 *2 不都是 x2 牌，Brittle(易碎) 状态也会追加 *2
      （CAttackSummary.CalcExpressionString: if (Brittle) text += "*2";）
    - 表达式里的加项包含攻击强化等非修正牌来源，本脚本只取括号内的修正项
"""
import os
import re
import sys

DEFAULT_LOG = (
    r"C:\Users\hanf2\AppData\LocalLow\Snapshot Games Inc\Frosthaven\Player-prev.log"
)

DRAW_MARK = "Drawing attack modifiers. OverallAdvantage"
EXPR_RE = re.compile(r"Attack modifier Expression String: (\S+)\s+Result: (\S+)")
UNDO_MARK = "Fast Undo at"
COPY_MARK = "CopyScenarioState took"


def parse_face(expr):
    """把伤害表达式还原成修正牌牌面。

    表达式形如 ((base)+1-2)*2 或 (base)+1
    返回 '+1' / '-2' / '*2' / '*0' / '+0' / '?<原串>'
    """
    m = re.fullmatch(r"\(\((\d+)\)(.*)\)\*(\d+)", expr)
    if m:
        mul, adds = m.group(3), m.group(2)
        if mul == "2":
            return "*2" + (adds or "")
        if mul == "0":
            return "*0"
        return adds or "+0"
    m = re.fullmatch(r"\(\((\d+)\)(.*)\)", expr)
    if m:
        return m.group(2) or "+0"
    m = re.fullmatch(r"\((\d+)\)(.*)", expr)
    if m:
        return m.group(2) or "+0"
    return "?" + expr


def load(log_path):
    with open(log_path, "r", encoding="utf-8", errors="ignore") as f:
        return f.read().split("\n")


def extract_draws(lines):
    """返回 [(行号, 牌面), ...]，按抽牌事件去重。"""
    draw_idx = [i for i, l in enumerate(lines) if DRAW_MARK in l]
    exprs = []
    for i, l in enumerate(lines):
        m = EXPR_RE.search(l)
        if m:
            exprs.append((i, m.group(1)))
    out = []
    for d in draw_idx:
        for i, e in exprs:
            if i > d:
                out.append((d, parse_face(e)))
                break
    return out


def show_sequence(cards):
    print("=== 修正牌面序列 ===")
    for j in range(0, len(cards), 10):
        row = " ".join(f"{c:>5}" for c in cards[j:j + 10])
        print(f"  {j + 1:>4}-{j + len(cards[j:j + 10]):<4}: {row}")


def show_undo_compare(lines, draws):
    undo = [(i, l) for i, l in enumerate(lines) if UNDO_MARK in l]
    print("\n=== 撤销点前后对比（各 4 张）===")
    hits = 0
    for ui, ul in undo:
        tag = ul[ul.find(UNDO_MARK):].strip()
        before = [c for d, c in draws if d < ui][-4:]
        after = [c for d, c in draws if d > ui][:4]
        ok = bool(before) and before == after
        hits += ok
        flag = "  <== 一致" if ok else ""
        print(f"  {tag[:48]:<48} 前{str(before):<26} 后{str(after):<26}{flag}")
    print(f"\n  完全一致 {hits}/{len(undo)}"
          f"（连续撤销链会把本次测量搅混，仅作参考）")


def show_adjacent_repeats(cards):
    """相邻重复 = 撤销后立刻重打，翻出同一串牌。"""
    n = len(cards)
    print("\n=== 相邻重复块（撤销后立刻重来）===")
    for L in range(10, 2, -1):
        hits = [(a, cards[a:a + L]) for a in range(n - 2 * L + 1)
                if cards[a:a + L] == cards[a + L:a + 2 * L]]
        if hits:
            print(f"  长度 {L}: {len(hits)} 处")
            for a, blk in hits[:8]:
                print(f"    第 {a + 1} 张起: {blk}")
            return
    print("  无")


def show_far_repeats(cards):
    n = len(cards)
    print("\n=== 长距离重复块（牌序可长期复现）===")
    for L in range(8, 2, -1):
        found = []
        for a in range(n - L + 1):
            blk = tuple(cards[a:a + L])
            for b in range(a + L, n - L + 1):
                if tuple(cards[b:b + L]) == blk:
                    found.append((a, b, list(blk)))
        if found:
            print(f"  长度 {L}: {len(found)} 处")
            seen = set()
            for a, b, blk in found:
                if blk in seen:
                    continue
                seen.add(tuple(blk))
                print(f"    第 {a + 1} 张 == 第 {b + 1} 张（相隔 {b - a} 次抽牌）: {blk}")
            return
    print("  无")


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_LOG
    if not os.path.exists(path):
        print(f"日志不存在: {path}")
        return 1

    lines = load(path)
    draws = extract_draws(lines)
    cards = [c for _, c in draws]

    n_copy = sum(1 for l in lines if COPY_MARK in l)
    n_rng = sum(1 for l in lines if "RNG changed from" in l)

    print(f"日志文件        : {path}")
    print(f"文件行数        : {len(lines)}")
    print(f"抽牌事件        : {len(draws)}")
    print(f"状态深拷贝次数  : {n_copy}  (FastUndo 走 CopyScenarioState 全量回滚)")
    print(f"RNG 状态变更    : {n_rng}")
    if draws and n_rng:
        print(f"平均 {len(draws) / n_rng:.1f} 次抽牌才消耗 1 次 RNG")
    print()

    show_sequence(cards)
    show_undo_compare(lines, draws)
    show_adjacent_repeats(cards)
    show_far_repeats(cards)
    return 0


if __name__ == "__main__":
    sys.exit(main())

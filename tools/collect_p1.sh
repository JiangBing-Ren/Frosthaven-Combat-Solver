#!/usr/bin/env bash
# P1 收尾脚本：不启动游戏，只从已有日志里抽出 P1 需要的一切。
#
# 用法：
#   bash tools/collect_p1.sh
#
# 典型流程：玩家自己开游戏 → 载入战斗 → 轮到自己时按 F2 出推荐 → 照着打完 → 跑本脚本收尾。

LOG="D:/Steam/steamapps/common/Frosthaven/BepInEx/LogOutput.log"

if [ ! -f "$LOG" ]; then
    echo "找不到日志: $LOG"
    exit 1
fi

echo "==================== 概览 ===================="
echo "插件版本   : $(grep -ao '插件已加载 v[0-9.]*' "$LOG" | tail -1)"
echo "挂载入口数 : $(grep -ao '轮询入口挂载完成: [0-9]* 个' "$LOG" | tail -1)"
echo "伤害对拍   : $(grep -ao '已挂载伤害对拍.*' "$LOG" | tail -1)"
echo "心跳次数   : $(grep -ac '心跳#' "$LOG")"
echo "快照次数   : $(grep -ac '===== 战局快照' "$LOG")"
echo "P1 运行次数: $(grep -ac '===== P1 单回合穷举' "$LOG")"
echo "异常       : $(grep -ac 'P1 运行异常' "$LOG") 次 / TickPump $(grep -ac 'TickPump 异常' "$LOG") 次"
echo "读取失败   : $(grep -ac '读取战局快照失败' "$LOG") 次"

echo
echo "==================== 伤害对拍（预测 vs 游戏实际）===================="
echo "总条数     : $(grep -ac '对拍\[' "$LOG")"
echo "OK         : $(grep -ac '对拍\[OK\]' "$LOG")"
echo "XX         : $(grep -ac '对拍\[XX\]' "$LOG")"
echo "末次统计   : $(grep -ao '对拍 伤害对拍[^|]*' "$LOG" | tail -1)"
echo "--- 失配明细（如有，最多 15 条）---"
grep -ao '对拍\[XX\].*' "$LOG" | head -15
echo "--- 前 8 条对拍 ---"
grep -ao '对拍\[..*\].*' "$LOG" | head -8

echo
echo "==================== 基础攻击力校准（卡面 vs 游戏实际）===================="
echo "看「差=」这一列：恒为 0 说明卡面 Strength 就是最终基础攻；"
echo "若某张牌恒为 +N，说明该角色/该牌带 N 点常驻 buff，需要补进模型。"
grep -ao '差=[-0-9]*' "$LOG" | sort | uniq -c | sort -rn

echo
echo "==================== P1 推荐输出 ===================="
total=$(grep -ac '===== P1 单回合推荐' "$LOG")
if [ "$total" -gt 0 ]; then
    # 只打最后一次 P1 运行的完整块（到下一个心跳行为止）
    awk -v s="$total" '
      /===== P1 单回合推荐/ { c++ }
      c >= s { print }
      c >= s && /^.*心跳#/ { exit }
    ' "$LOG"
else
    echo "(本次日志没有 P1 运行记录 —— 战斗中按过 Ctrl+F2 吗？)"
fi

echo
echo "==================== 本地化命中情况 ===================="
echo "本地化初始化 : $(grep -ao '本地化: .*' "$LOG" | tail -1)"
echo "取不到的键   : $(grep -ac '未本地化' "$LOG") 处   <-- 大于0说明某些卡/怪的本地化键名对不上"
grep -ao '未本地化' "$LOG" | head -5

echo
echo "==================== 心跳尾部 ===================="
grep -ao '心跳#.*' "$LOG" | tail -3

echo
echo "=============================================="

#!/usr/bin/env bash
# M1 收尾脚本：不启动游戏，只从已有日志里抽出 M1 需要的一切。
#
# 用法：
#   bash tools/collect_m1.sh
#
# 典型流程：玩家自己开游戏 → 载入战斗 → 打完一回合 → 跑本脚本收尾。

LOG="D:/Steam/steamapps/common/Frosthaven/BepInEx/LogOutput.log"

if [ ! -f "$LOG" ]; then
    echo "找不到日志: $LOG"
    exit 1
fi

echo "==================== 概览 ===================="
echo "插件版本   : $(grep -ao '插件已加载 v[0-9.]*' "$LOG" | tail -1)"
echo "挂载入口数 : $(grep -ao '轮询入口挂载完成: [0-9]* 个' "$LOG" | tail -1)"
echo "心跳次数   : $(grep -ac '心跳#' "$LOG")"
echo "快照次数   : $(grep -ac '===== 战局快照' "$LOG")"
echo "首次心跳   : $(grep -ao '首次收到游戏心跳.*' "$LOG" | head -4)"
echo "异常       : $(grep -ac 'TickPump 异常' "$LOG") 次"
echo "读取失败   : $(grep -ac '读取战局快照失败' "$LOG") 次"
echo "owner=NULL : $(grep -ac 'owner=NULL' "$LOG") 次   <-- 大于0说明插件实例引用丢失"
echo "latch=True : $(grep -ac 'latch=True' "$LOG") 次   <-- 大于0说明进入战斗判定生效"

echo
echo "==================== M1.5 抽牌预测验证 ===================="
echo "抽牌总次数 : $(grep -ac '抽牌#' "$LOG")"
echo "命中       : $(grep -ac '抽牌#.* ✔' "$LOG")"
echo "不符       : $(grep -ac '抽牌#.* ✘' "$LOG")"
echo "末次统计   : $(grep -ao '预测 命中[^|]*' "$LOG" | tail -1)"
echo "--- 前 10 次断言 ---"
grep -ao '抽牌#.*' "$LOG" | head -10
echo "--- 失配明细（如有）---"
grep -ao '抽牌#.*✘.*' "$LOG" | head -10

echo
echo "==================== 心跳尾部（看轮询是否持续） ===================="
grep -ao '心跳#.*' "$LOG" | tail -5

echo
echo "==================== 快照触发点 ===================="
grep -ao '===== 战局快照 (.*) =====' "$LOG"

echo
echo "==================== 最后两次完整快照 ===================="
# 取最后两个快照块（从"战局快照"到"快照结束"）
awk '/===== 战局快照/{n++} n>=0' "$LOG" > /dev/null 2>&1
total=$(grep -ac '===== 战局快照' "$LOG")
if [ "$total" -gt 1 ]; then
    start=$((total - 1))
else
    start=1
fi
awk -v s="$start" '
  /===== 战局快照/ { c++ }
  c >= s { print }
  /===== 快照结束/ { if (c >= s + 1) exit }
' "$LOG"

echo
echo "=============================================="

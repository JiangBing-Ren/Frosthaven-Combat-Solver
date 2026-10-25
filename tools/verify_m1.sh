#!/usr/bin/env bash
# M1 验证脚本
#   boot   : 只验证插件加载 + Update 不报错（约 25 秒，不需要人操作）
#   battle : 启动后等待你载入战斗场景，命中快照后自动收尾（默认 6 分钟）
#
# 用法：
#   bash tools/verify_m1.sh boot
#   bash tools/verify_m1.sh battle [最长等待秒数]

MODE="${1:-boot}"
MAX_WAIT="${2:-360}"

GAME_DIR="D:/Steam/steamapps/common/Frosthaven"
GAME_EXE="$GAME_DIR/Frosthaven.exe"
LOG="$GAME_DIR/BepInEx/LogOutput.log"

if [ ! -f "$GAME_EXE" ]; then
    echo "找不到游戏: $GAME_EXE"
    exit 1
fi

[ -f "$LOG" ] && rm -f "$LOG"
cd "$GAME_DIR" || exit 1

echo "[verify] 模式=$MODE  最长等待=${MAX_WAIT}s"
echo "[verify] 启动游戏..."
"./Frosthaven.exe" &

if [ "$MODE" = "boot" ]; then
    sleep 22
    taskkill //F //IM Frosthaven.exe //T > /dev/null 2>&1
    sleep 2
    echo
    echo "================ LogOutput.log ================"
    cat "$LOG" 2>/dev/null || echo "(无日志)"
    echo "==============================================="
    if grep -qa "TickPump 异常" "$LOG" 2>/dev/null; then
        echo "M1 boot 结果: 未通过（轮询有异常）"
    elif ! grep -qa "轮询入口挂载完成" "$LOG" 2>/dev/null; then
        echo "M1 boot 结果: 未通过（轮询入口未挂载成功）"
    elif grep -qa "首次收到游戏心跳" "$LOG" 2>/dev/null; then
        echo "M1 boot 结果: 通过（轮询入口已生效）"
    elif grep -qa "插件已加载" "$LOG" 2>/dev/null; then
        echo "M1 boot 结果: 部分通过（插件已加载，但尚未收到游戏心跳）"
    else
        echo "M1 boot 结果: 未通过（插件未加载）"
    fi
    echo "心跳次数: $(grep -ac '心跳#' "$LOG" 2>/dev/null)"
    exit 0
fi

# battle 模式：等玩家载入战斗
elapsed=0
found=0
while [ "$elapsed" -lt "$MAX_WAIT" ]; do
    sleep 5
    elapsed=$((elapsed + 5))
    if grep -qa "战局快照" "$LOG" 2>/dev/null; then
        found=1
        echo "[verify] 命中！第 ${elapsed}s 捕获到战局快照"
        break
    fi
    if [ $((elapsed % 30)) -eq 0 ]; then
        echo "[verify] 等待载入战斗 ${elapsed}s ..."
    fi
done

sleep 6
taskkill //F //IM Frosthaven.exe //T > /dev/null 2>&1
sleep 2

echo
echo "================ 快照部分 ================"
if [ -f "$LOG" ]; then
    grep -a -A 40 "战局快照" "$LOG" | head -80
else
    echo "(无日志)"
fi
echo "=========================================="

if [ "$found" -eq 1 ]; then
    echo "M1 battle 结果: 通过"
else
    echo "M1 battle 结果: 未通过（未捕获到快照）"
fi

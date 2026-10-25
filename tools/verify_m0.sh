#!/usr/bin/env bash
# M0 验证：启动霜港迷城，等 BepInEx 日志出现 FHSolver，然后结束进程并汇报。
# 用法（Git Bash）：bash tools/verify_m0.sh
#
# 全过程自包含：启动 -> 轮询 -> 杀进程 -> 打印结果。

GAME_DIR="D:/Steam/steamapps/common/Frosthaven"
GAME_EXE="$GAME_DIR/Frosthaven.exe"
LOG="$GAME_DIR/BepInEx/LogOutput.log"

if [ ! -f "$GAME_EXE" ]; then
    echo "找不到游戏: $GAME_EXE"
    exit 1
fi

# 先清掉旧日志，确保本次结果是新的
[ -f "$LOG" ] && rm -f "$LOG"

echo "[verify] 启动游戏: $GAME_EXE"
cd "$GAME_DIR" || exit 1
"./Frosthaven.exe" &
GAME_PID=$!
echo "[verify] 游戏进程 pid=$GAME_PID"

MAX_WAIT=180
STEP=5
elapsed=0
found=0
while [ "$elapsed" -lt "$MAX_WAIT" ]; do
    sleep "$STEP"
    elapsed=$((elapsed + STEP))
    if [ -f "$LOG" ] && grep -qa "FHSolver" "$LOG"; then
        found=1
        echo "[verify] 命中！第 ${elapsed}s 日志中出现 FHSolver"
        break
    fi
    echo "[verify] 等待中 ${elapsed}s ... (日志: $([ -f "$LOG" ] && wc -l < "$LOG" || echo 0) 行)"
done

if [ "$found" -eq 0 ]; then
    echo "[verify] 超时：${MAX_WAIT}s 内未出现 FHSolver"
fi

# 再多给一点时间让日志写全
sleep 8

echo "[verify] 结束游戏进程"
taskkill //F //IM Frosthaven.exe //T > /dev/null 2>&1
sleep 2

echo
echo "================ LogOutput.log ================"
if [ -f "$LOG" ]; then
    cat "$LOG"
else
    echo "(无 LogOutput.log —— BepInEx 未初始化)"
fi
echo "==============================================="

if [ "$found" -eq 1 ]; then
    echo "M0 结果: 通过"
else
    echo "M0 结果: 未通过"
fi

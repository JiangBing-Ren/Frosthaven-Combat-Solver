#!/usr/bin/env bash
# 从霜港迷城移除 BepInEx（只删除 BepInEx 自己添加的文件，不碰任何游戏原文件）
# 用法（Git Bash）：bash tools/uninstall_bepinex.sh
#
# 为什么需要它：Steam 的「验证游戏文件完整性」也会清掉这些文件，
# 但那样会连带把你自己改过的东西一起还原。用这个脚本可以精确、定点卸载。

GAME_DIR="D:/Steam/steamapps/common/Frosthaven"

if [ ! -d "$GAME_DIR" ]; then
    echo "游戏目录不存在: $GAME_DIR"
    exit 1
fi

TARGETS=(
    "$GAME_DIR/winhttp.dll"
    "$GAME_DIR/doorstop_config.ini"
    "$GAME_DIR/.doorstop_version"
    "$GAME_DIR/BepInEx"
)

echo "即将从游戏目录移除以下 BepInEx 文件："
echo "  $GAME_DIR"
for t in "${TARGETS[@]}"; do
    if [ -e "$t" ]; then
        echo "    - $(basename "$t")"
    fi
done
echo
echo "不会删除任何游戏原文件（Frosthaven.exe / UnityPlayer.dll / Frosthaven_Data 等均保留）。"
echo
read -r -p "确认移除？[y/N] " ans
if [ "$ans" != "y" ] && [ "$ans" != "Y" ]; then
    echo "已取消。"
    exit 0
fi

for t in "${TARGETS[@]}"; do
    if [ -e "$t" ]; then
        rm -rf "$t"
        echo "已移除: $(basename "$t")"
    fi
done

echo
echo "BepInEx 已卸载完毕。插件本体仍在 D:/1Mods/FH/src/FHSolver，可随时重建部署。"

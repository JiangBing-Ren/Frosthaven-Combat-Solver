#!/usr/bin/env bash
# 构建 FHSolver 插件 + 跑离线测试台。
#
# 为什么用 no-restore：
#   当前宿主环境（WorkBuddy 启动的 shell）缺 LOCALAPPDATA 等环境变量，
#   NuGet 的 NuGetEnvironment.CalculateFolderPath 会 Path.Combine(null, ...)
#   抛 "Value cannot be null (Parameter 'path1')"，
#   导致任何 dotnet build 的**隐式 restore** 都崩（SDK 6/8/10 全试过）。
#   本项目两个工程都**不引用任何 NuGet 包**，所以用 no-restore 即可。
#   根治办法：给用户环境变量补上 LOCALAPPDATA（见 MEMORY 记录）。
set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DOTNET="/c/Program Files/dotnet/dotnet.exe"

python3 "$ROOT/tools/make_assets.py" "$ROOT/src/FHSolver"      FHSolver      net472
python3 "$ROOT/tools/make_assets.py" "$ROOT/src/FHSolver.Tests" FHSolver.Tests net8.0

echo "=== 反射目标校验（防\"反射打错类\"静默失败）==="
python3 "$ROOT/tools/verify_reflection.py"

echo "=== 构建插件 ==="
cd "$ROOT/src/FHSolver" && "$DOTNET" build -v q --nologo --no-restore

echo "=== 离线测试台 ==="
cd "$ROOT/src/FHSolver.Tests" && "$DOTNET" run --no-restore

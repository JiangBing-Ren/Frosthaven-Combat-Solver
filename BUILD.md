# FHSolver 构建与运行环境

状态：**M0 已通过**（2026-10-22）

---

## 1. 环境事实（全部实测，非推断）

| 项 | 值 | 来源 |
|---|---|---|
| 游戏路径 | `D:\Steam\steamapps\common\Frosthaven` | — |
| 游戏版本 | Frosthaven v1.0.34121 | BepInEx 日志 |
| Unity | **2022.3.71.13221651** | BepInEx 日志 |
| 后端 | **Mono**（有 `MonoBleedingEdge`，无 `GameAssembly.dll`） | 目录结构 |
| CLR 运行时 | `.NET Framework 4.x`（`4.0.30319.42000`） | BepInEx 日志 |
| 反射发射 | **Supports SRE: True** | BepInEx 日志 |
| BepInEx | **5.4.23.5**，Doorstop **4.3.0** | 实测 |
| Harmony | **2.9.0.0**（`0Harmony.dll`，命名空间 `HarmonyLib`） | 实测 |
| 反作弊 | 无 EAC | 目录结构 |

---

## 2. 为什么选 BepInEx 5.4 而不是 6.0

| 版本 | 发布时间 | 结论 |
|---|---|---|
| **BepInEx 5.4.23.5** | **2026-02-08** | 活跃维护的稳定线 → **采用** |
| BepInEx 6.0.0-pre.2 | 2024-08-27 | 两年前的陈旧预发布版 |

> v1 设计稿写的「必须 6.x，5.4.x 仅到 2021」是错的。判断依据是发布时间而非版本号大小。
> 两者 core 都编译于 **net35**（引用 `mscorlib 2.0.0.0` + `System.Core 3.5.0.0`）。

**Harmony 程序集辨识**（易踩坑）：`BepInEx/core` 下有两个
- `0Harmony.dll` → **HarmonyX 2.9.0.0**，命名空间 `HarmonyLib`，**引用这个**
- `0Harmony20.dll` → 2.0.0.0 兼容 shim，不要引用

---

## 3. 工程

```
D:\1Mods\FH\src\FHSolver\
├── FHSolver.csproj     目标 net472，路径集中在 PropertyGroup，构建后自动部署
├── Plugin.cs           BepInEx 入口 + 环境/类型自检
└── bin\FHSolver.dll
```

### 关键配置

```xml
<TargetFramework>net472</TargetFramework>          <!-- CLR 4.0.30319 → net472 正合适 -->
<PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.3" />
```

引用（全部 `Private=false`，运行时由 BepInEx/Unity 提供）：
`BepInEx.dll`、`0Harmony.dll`、`UnityEngine.dll`、`UnityEngine.CoreModule.dll`

**暂不添加游戏业务程序集的编译期引用**：`Assembly-CSharp` 依赖图极深，
贸然引用会引入大量无关解析错误。改用 `Type.GetType("<全称>, <程序集名>")` 运行时探测，
待确定要用哪些类型后，再有针对性地加引用。

### 命令

```bash
cd /d/1Mods/FH/src/FHSolver
dotnet build                 # 构建 + 自动部署到 BepInEx/plugins
bash tools/verify_m0.sh      # 启动游戏 -> 等日志 -> 杀进程 -> 打印结果
```

---

## 4. 对游戏目录的改动（全部为新增，未改动任何游戏原文件）

```
Frosthaven/
├── winhttp.dll            ← Doorstop 4.3.0 代理（新增）
├── doorstop_config.ini    ← 新增
├── .doorstop_version      ← 新增
└── BepInEx/               ← 新增
    ├── core/
    ├── config/            ← 首次启动后生成
    ├── plugins/FHSolver.dll
    └── LogOutput.log      ← 验证依据
```

`Frosthaven.exe` / `UnityPlayer.dll` / `Frosthaven_Data/` 时间戳未变，未被改动。

### 卸载

```bash
bash tools/uninstall_bepinex.sh
```

只删除上述新增项，不碰游戏原文件。

### ⚠️ Steam 验证风险

游戏目录内的文件会被 **Steam「验证游戏文件完整性」还原**，届时 BepInEx 会被清掉。
重装只需重跑一次 `dotnet build`（会自动复制 winhttp.dll 等吗？——不会，
winhttp.dll 需从 `build/be5/` 重新拷一次，见卸载脚本的反向操作）。
建议把 `D:\1Mods\FH\build\be5\` 留着当本地源。

---

## 5. 验证脚本

`tools/verify_m0.sh` 流程：清旧日志 → 启动游戏 → 每 5 秒轮询 `LogOutput.log`
是否含 `FHSolver` → 命中或超时 → `taskkill /F /IM Frosthaven.exe /T` → 打印日志。

实测：**5 秒命中**，远快于等到主菜单（BepInEx 在 preloader/chainloader 阶段就完成加载，
不需要等游戏进主菜单）。整套验证约 15-20 秒。

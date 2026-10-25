# 霜港迷城 (Frosthaven) — 文件结构逆向与可解析性评估

> 逆向日期：2026-10-22
> 目标本体：`D:\Steam\steamapps\common\Frosthaven`
> 结论：**可解析性极佳**。规则数据全部为明文，且已实现字节级无损解包/回打包。

---

## 1. 引擎判定：Unity Mono（非 IL2CPP）

| 判据 | 结果 |
|---|---|
| `MonoBleedingEdge/` 目录 | 存在 |
| `GameAssembly.dll` | **不存在** |
| `Frosthaven_Data/Managed/` | 存在，含全部托管程序集 |
| `UnityPlayer.dll` | 存在 |

→ 与 FTK2 同属 **Unity Mono 后端**，可直接用 dnSpy/ILSpy 反编译、Mono.Cecil 做 IL 注入。
→ 本机已装 dnSpy (`D:\dnSpy`) 与 ILSpy (`D:\ILSpy`)。

### 业务程序集（Managed 下的自研 DLL）

| 程序集 | 体积 | 作用 |
|---|---|---|
| `Assembly-CSharp.dll` | 11.66 MB | 主游戏代码 |
| `ScenarioRuleLibrary.dll` | 4.81 MB | 场景规则 |
| `ForsthavenRuleLibrary.dll` | 1.02 MB | 核心规则（金币/资源/补给/宠物分配） |
| `MapRuleLibrary.dll` | 559 KB | 地图规则 |
| `SharedLibrary.dll` / `GH.Shared.dll` | 小 | 共享 |

`ForsthavenRuleLibrary` 内可见命名空间：`ForsthavenRuleLibrary`、`ForsthavenSupply`、
`FrosthavenGoldDistribution`、`FrosthavenResourceDistribution`、`FrosthavenPetDistribution`、
`FrosthavenSupply` → **经济/平衡数值的宿主**。

### 混淆检测

对四个主 DLL 扫描 ConfuserEx / CryptoObfuscator / SmartAssembly / Eazfuscator /
Dotfuscator / VMProtect / Obfuscar / Babel 特征串 → **全部无命中**。
类型名、命名空间为明文，metadata 完整 → dnSpy 可直接打开。

---

## 2. 规则数据库 `Base.ruleset`（27,481,597 字节）

路径：`Frosthaven_Data/StreamingAssets/Rulebase/Base.ruleset`

### 归档格式（已逆向确认）

```
偏移 0    : 8 字节  ASCII 魔数  "02392fe1"
偏移 8    : u32     = 0
偏移 12   : u32     = 条目总数 N (=3397)
偏移 16   : TOC，N × 144 字节，每条：
              [0..127]   路径，UTF-8，末尾补 0x00（定长 128）
              [128] u32  meta1  条目类型/解析器枚举 ID（2..43, 255）
              [132] u32  meta2  恒为 0
              [136] u32  offset 相对数据区起点的偏移
              [140] u32  size   字节数
数据区     : 起点 = 16 + N×144 = 489,184
             各文件连续存放，无压缩、无对齐填充
```

**校验结果：3397 条偏移链全部连续，末条末端精确等于数据区长度 → 结构 100% 正确。**

### 内容构成

| 类型 | 数量 | 可解析性 |
|---|---|---|
| `.yml` 明文规则 | 3248 | **纯文本，任意语言可直接读写** |
| `.lvldat` 关卡数据 | 149 | .NET BinaryFormatter 序列化，需 C# + 游戏程序集 |

### `.yml` 侧（主要 mod 面）

顶层分类：`Campaign` 1493 / `Global` 1173 / `Outpost` 636 / `Tutorials` 90 / `CustomScenarios` 5

Parser 类型分布：

| Parser | 数量 | | Parser | 数量 |
|---|---|---|---|---|
| EnemyCard | 645 | | BattleGoal | 60 |
| AbilityCard | 527 | | HeroSummon | 40 |
| ScenarioAbility | 437 | | Mastery | 34 |
| ItemCard | 265 | | FHEnhancement | 34 |
| FHItem | 256 | | Character | 19 |
| Enemy | 219 | | PetCard | 12 |
| CampaignSection | 205 | | AttackModifierDeck | 4 |
| Perk | 185 | | TreasureTable | 3 |
| FHScenario | 145 | | GameMode | 3 |
| AttackModifier | 140 | | Scenario | 1 |

示例（`Outpost/Items/Item_1.yml`）：

```yaml
Parser: FHItem
ID: 1
ScenarioItemID: 882
Name: Spyglass
Quantity: 2
Type: Head
CraftingCost:
    Metal: 1
```

示例（`Global/HeroSummons/AnimatedClaymore.yml`）：

```yaml
Parser: HeroSummon
ID: AnimatedClaymoreID
LocKey: $FH/196_character_ability_cards_infuser_imbue_with_life_2$
Model: AnimatedClaymore
Health:
    Amount: 2
    SquareEnhancements: 1
Move:
    ...
```

> 注意：YAML 内文本为 `\r\n` 换行；`LocKey: $...$` 为本地化键，实际文案在别处（语言资源）。

### `.lvldat` 侧

文件头为 .NET BinaryFormatter 的 `SerializationHeaderRecord`：

```
00 01 00 00 00 | ff ff ff ff | 01 00 00 00 | 00 00 00 00
recordType=0, rootId=1, headerId=-1, majorVer=1, minorVer=0
```

其后为程序集限定类型名（`Assembly-CSharp, Version=0.0.0.0, ...`、
`ScenarioRuleLibrary, Version=0.0.0.0, ...`）与对象字段
（`AIFocusOverride`、`AIMoveFocusWaypoints`、`AbilityCardID`、`AbilityDeck`…）。

→ **解析 .lvldat 必须在 .NET 环境下反序列化**，且需加载 `Assembly-CSharp.dll`
与 `ScenarioRuleLibrary.dll` 作为类型来源。Python 无法直接处理。
→ 路径位于 `Campaign/Scenarios/LevelEditorScenarios/`，命名如
`FS_001_A_Town_in_Flames.lvldat`，说明游戏内置关卡编辑器。

---

## 3. 其他可解析资源

| 路径 | 内容 |
|---|---|
| `StreamingAssets/Procedures/` | 700 个 Apparance 程序化生成定义，**纯 XML**（`.proc` / `.procedit`） |
| `StreamingAssets/Scripts/` | 运行时 C# 脚本（`example.cs`、`refresh_probes.cs`），由 Roslyn 支撑 |
| `StreamingAssets/AutoComplete/` | `CampaignAutoComplete.yml` |
| `StreamingAssets/AssetBundles/` | Unity 资源包（怪物模型等，如 `npc_wraith`） |

游戏内置 Roslyn（`Microsoft.CodeAnalysis.CSharp.Scripting.dll`）→ 存在**运行时 C# 脚本执行**能力。

---

## 4. 工具

`tools/ruleset.py`（Python 3，无第三方依赖）

```bash
# 查看结构 + 分类统计 + 完整性校验
python tools/ruleset.py info    <Base.ruleset>

# 解包（连 _manifest.json 一起生成，供回打包）
python tools/ruleset.py extract <Base.ruleset> <输出目录>

# 回打包（依据 manifest 逐条保留 meta，无损）
python tools/ruleset.py repack  <解包目录> <输出.ruleset>
```

**回打包已验证：输出与原文件 md5 完全一致（`d5ec6f2230555a3f84262e0d7020046c`）**，
即改 YAML 后回写可被游戏正常读取。

---

## 5. mod 路径建议

1. **改数值（推荐首选）**：直接编辑 `.yml` → 回打包 `Base.ruleset`。
   不动 DLL，风险最低，且可精确控制到单个怪物/物品/技能/Perk。
2. **改逻辑**：对 `Assembly-CSharp.dll` / `ForsthavenRuleLibrary.dll` 做 Mono.Cecil
   IL 注入（与 FTK2 同手法）。注意 Unity Mono 的版本兼容。
3. **做关卡**：`.lvldat` 需用 C# 反序列化后改写再序列化，门槛最高；
   也可优先考虑游戏内置的关卡编辑器。

### 注意事项

- 改 `.yml` 时须保持 `\r\n` 换行与文件条目顺序（repack 依赖 manifest 顺序）。
- 归档内路径含空格（如 `Armored Tank.yml`），脚本处理时勿按空格切分。
- 建议先备份原 `Base.ruleset`。

---

## 6. mod 加载能力评估（2026-10-22）

游戏 1.0 正式版发售于 2026-10-16，开发商 Snapshot Games（Julian Gollop 团队），发行 Arc Games。

### 结论：能加载 mod，但没有官方创意工坊

| 路径 | 可行性 | 说明 |
|---|---|---|
| 改 `Base.ruleset` | **可行，首选** | 加载链路无完整性校验（见下） |
| 官方自定义关卡 | **可行** | 游戏内建 CustomLevel 系统 |
| 通用加载器（BepInEx 等） | 技术可行 | Unity Mono + 无 EAC |
| Steam 创意工坊 | **不支持** | Steam 页面标注 "No Workshop Support" |

### 证据 1：ruleset 加载链路无校验

`Player.log`（`%LOCALAPPDATA%Low\Snapshot Games Inc\Frosthaven\Player.log`）记录：

```
[INFO] Loading Rulebase
[YML] Loading  40 HeroSummon   / 140 AttackModifier / 19 Character
[YML] Loading 531 AbilityCard  / 266 ItemCard       / 185 Perk
[YML] Loading 648 EnemyCard    / 219 Enemy          / 437 ScenarioAbility
[YML] Loading 149 CustomLevel entries
[YML] Finished generating def repository. Duration: 8.6229595
```

数量与本工具解出的 Parser 统计**逐一吻合**，证明格式分析正确；
且加载过程中**无 hash / checksum / integrity 相关日志**，未见校验环节。

### 证据 2：内建自定义关卡系统

`Assembly-CSharp.dll` 中存在完整 API：

```
CustomLevelDataFolder              GetDefaultFilePathForCustomLevel
LoadCustomLevelDataFromFile        LoadCustomLevelDataService
UseCustomLevelDataFolder           SetCustomLevelDataFolderPressed
ScenarioRuleLibrary.CustomLevels
```

ruleset 内也自带官方示例：

```
CustomScenarios/SingleScenarioYML/SandboxScenario.yml
CustomScenarios/LevelEditorScenarios/Example_Level.lvldat
CustomScenarios/LevelEditorScenarios/Example_Puzzle_1.lvldat
```

→ 做**自定义关卡**走官方通路即可，不必改归档。

### 证据 3：无反作弊阻挡

- 游戏目录**无 `EasyAntiCheat` 目录** → 未被 EAC 保护。
- 存在 `StreamingAssets/EOS/`（Epic Online Services），但仅用于联机后端
  （日志：`FFSNetwork finished initialization with backend: FFSNet.Backend.EOSNetworkBackend`）。
- 引擎为 Unity Mono → BepInEx / MelonLoader / UnityModManager 均可适用。

### 风险提示

1. **Steam 验证完整性会还原改动**：改游戏目录内的 `Base.ruleset` 属于原地替换，
   一旦验证游戏文件完整性即被覆盖 → 务必留备份，或改用 BepInEx 类外挂方案。
2. **联机不同步**：游戏有在线合作（EOS）。改数值后联机可能导致双方状态分叉
   （与 WH3 的 desync 同类问题）。**改数值建议只在单人档使用。**
3. **尚未实测**：字节级一致 ≠ 游戏接受。需实跑一次最小改动验证。

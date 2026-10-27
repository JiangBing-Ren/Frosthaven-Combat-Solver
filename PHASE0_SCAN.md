# 阶段 0：只读扫描报告

> 目标：镜像复刻《霜港迷城》战斗机制，产出可运行 / 可回放 / 可自动对拍的战斗规则引擎。
> 本阶段**只扫描、不写实现代码**。所有结论来自本地明文数据，可追溯到具体文件路径。

- 扫描时间：2026-10-28
- 原始目录（只读）：`D:\Steam\steamapps\common\Frosthaven`
- 工作目录（产出）：`D:\1Mods\FH`

---

## 1. 目录结构摘要

```
Frosthaven\                          总 39 GB（绝大部分是美术/音频资产，本项目不碰）
├─ Frosthaven.exe                    666 KB
├─ UnityPlayer.dll                   31 MB
├─ MonoBleedingEdge\                 ★ Mono 运行时的存在 = 托管程序集以 IL 形式发布
├─ winhttp.dll / doorstop_config.ini （注：这两项是此前 BepInEx 注入留下的，非官方发行内容）
└─ Frosthaven_Data\
   ├─ Managed\                       ★ 托管程序集（IL，可反编译）
   │  ├─ Assembly-CSharp.dll         11.6 MB   ← 游戏主逻辑
   │  ├─ ScenarioRuleLibrary.dll     4.8 MB    ← 战斗规则库（核心）
   │  ├─ ForsthavenRuleLibrary.dll   1.0 MB    ← 本作专属规则/经济
   │  ├─ MapRuleLibrary.dll          0.56 MB   ← 地图规则
   │  ├─ Snapshot.Libs.Localization.dll        ← 本地化
   │  └─ （Unity 引擎与第三方库若干）
   ├─ StreamingAssets\
   │  ├─ Rulebase\Base.ruleset       27,481,597 B  ★★ 唯一规则数据归档
   │  ├─ AssetBundles\               25 GB（美术资产，不碰）
   │  ├─ aa\                         98 MB（Addressables，含本地化表）
   │  ├─ Procedures\ / Movies\       （表现层资产）
   │  └─ Scripts\                    2 KB（运行时 C# 脚本占位）
   ├─ globalgamemanagers / level*.resS   （Unity 序列化资产）
   └─ ScriptingAssemblies.json       （程序集清单）
```

**关键判断**：规则逻辑分布在两处——**IL 程序集**（行为/流程）与 **Base.ruleset**（数值/配置）。二者互为补充，缺一不可。

---

## 2. 识别到的引擎与数据格式

| 项 | 结论 | 证据 |
|---|---|---|
| 引擎 | Unity（含 `MonoBleedingEdge` → **Mono**，非 IL2CPP） | 目录项 `MonoBleedingEdge\` |
| 托管代码形态 | IL，**未混淆**（类名/字段名可读） | 已有反编译产物，见 §3.4 |
| 规则数据归档 | 自研二进制归档 `Base.ruleset` | 魔数与 TOC 见下 |
| 归档内容 | 明文 `.yml`（3248）+ `.lvldat`（149） | `tools/ruleset.py info` 实测 |
| 序列化 | `.lvldat` = .NET `BinaryFormatter` | 既有工作结论（FORMAT.md） |
| 本地化 | Unity Localization 包，表在 `aa/`（Addressables，二进制） | `Snapshot.Libs.Localization.dll` + `aa\StandaloneWindows64\` |

### 2.1 Base.ruleset 归档格式（可复现）

运行 `python3 tools/ruleset.py info <Base.ruleset>` 实测输出：

```
体积: 27481597 字节
条目: 3397
数据区: 起点 489184, 长度 26992413
```

结构（既有工具已实现，见 `tools/ruleset.py`、`FORMAT.md`）：

```
偏移 0   : 8 字节 ASCII 标识 "02392fe1"   ← 是**可打印 ASCII 字符串**，不是 u32
偏移 8   : u32 = 0                        （含义未确认，疑似保留位）
偏移 12  : u32 = 条目总数 N = 3397  (LE)
偏移 16  : TOC，N × 144 字节，每条：
            [0..127]  路径，UTF-8，末尾补 0x00（定长 128）
            [128] u32 meta1   条目类型/解析器枚举 ID（2..43, 255）
            [132] u32 meta2   恒为 0
            [136] u32 offset  相对数据区起点的偏移
            [140] u32 size    字节数
数据区   : 起点 = 16 + N×144 = 489,184（实测吻合）
           各文件连续存放，无压缩、无对齐填充
```

> ⚠ 措辞更正：原先写作“魔数 + u32 条目数”，**漏了偏移 8 处那个 u32=0**。
> 阶段 1 已用 `od -A d -t x1z -N 32` 复核确认，见 `FORMAT.md` 第 47–60 行。

条目类型分布（`meta1`）：`type11=648`、`type6=531`、`type16=437`、`type7=266`、`type39=256`、`type12=219`、`type8=185`、`type255=149`（= `.lvldat`）、`type40=205`、`type38=145` …

> ⚠ `meta1` 的**语义映射尚未完全确认**（哪些数值对应哪种解析器）。阶段 1 需建立"meta1 → Parser"映射表；这在阶段 1 是可解的反推问题（因为路径后缀 `.yml`/`.lvldat` 已给出强约束）。

---

## 3. 可用于提取规则的数据文件清单

### 3.1 角色 / 职业（Global/Characters）

路径：`extracted/Base_ruleset/Global/Characters/`

17 个职业目录，每职业 29–35 张能力卡：

| 职业 | 卡数 | 职业 | 卡数 |
|---|---|---|---|
| Geminate | 35 | Shattersong | 30 |
| Drifter / CrashingTide / Boneshaper | 32 | Pyroclast / PainConduit / Deepwraith / Blinkblade / BannerSpear | 30 |
| Snowdancer / Infuser / HIVE / Deathwalker | 31 | Trapper | 29 |

（另有 MetalMosaic、FrozenFist、Fused? 等未逐一枚举；`CharacterResources.yml` 为职业公共资源）

**单卡字段样本** — `Global/Characters/BannerSpear/AbilityCards/62_BannerSpear_RallyingCry.yml`：

```yaml
Parser: AbilityCard
Name: $FH/062_character_ability_cards_banner_spear_rallying_cry_1$
Character: BannerSpearID
ID: 62
Level: 1
Initiative: 71
Discard: [Discard,Lost]        # 上半=弃 / 下半=永久失去
Data:
    Top:
        Abilities:
            TAttack:
                Attack: { Amount: 3 }
                Conditions: Disarm
                AreaEffect: "0,0,G|2,0,R|3,1,B|4,0,E"
                MultiPassAttack: False
        XP: 1
    Bottom:
        Abilities:
            BSummon: { Summon: BannerOfStrengthID }
        XP: 2
```

**可直接提取的字段**：`ID`、`Initiative`、`Discard`（上下半去向）、半边能力列表（Attack/Move/Heal/Summon/Conditions/AreaEffect/Target/Range/Filter/SubAbilities）、`XP`。

### 3.2 攻击修正牌与牌组

| 路径 | 内容 |
|---|---|
| `Global/AttackModifiers/*.yml` | **140** 个修正牌定义（`AMC_*`） |
| `Global/AttackModifierDecks/` | **4** 副牌组：`Standard.yml`、`ReducedRandomness.yml`、`MonsterDeck.yml`、`MonsterReducedRandomness.yml` |

> ★ 此前在运行时观测到的"ReducedRandomness 房规"在此得到明文确认（`ReducedRandomness.yml`）。

### 3.3 怪物与行动卡

| 路径 | 内容 |
|---|---|
| `Campaign/Enemies/Monsters/<Name>/` | **86** 个怪物类，各含 `<Name>.yml` 与 `<Name>Elite.yml` |
| `Campaign/Enemies/Monsters/*/Cards/*.yml` | **384** 张怪物行动卡 |

**怪物基数据表** — `Campaign/Enemies/Monsters/AlgoxScout/AlgoxScoutElite.yml`：

```yaml
Parser: Enemy
ID: AlgoxScoutEliteID
LocKey: $FH/AlgoxScoutElite$
MonsterType: Monster
StandeeLimit: 6
NonEliteVariant: AlgoxScoutID
PredominantlyMelee: True
AbilityCards: [770,771,772,773,774,775,776,777]
Level1: { Health: 12, Move: 4, Attack: 3, Range: 1, Target: 1, Conditions: [Impair] }
```

→ 每级（Level0–7）的 **Health / Move / Attack / Range / Target / Conditions** 均为明文。
★ `Conditions: [Impair]` 直接证明"怪物攻击会附加状态"，可用于阶段 3 的状态施加规则。

**行动卡样本** — `.../AbaelHerder/Cards/779_AbaelHerder_DesperateHerd.yml`：

```yaml
Parser: EnemyCard
ID: 779
Initiative: 72
Shuffle: False
Data:
    Abilities:
        Move1: { Move: -1 }                  # -1 = 使用怪物自身 Move 属性
        ControlPig1:
            ControlActor:
                UseControllingActorType: True
                Abilities:
                    AllyAttack: { Attack: -2 }   # -2 = 使用自身 Attack 属性
                    SelfDamage: { Damage: 1, Filter: Self }
```

★ `Move: -1 / Attack: -2` 这类**哨兵值**表示"取怪物自身属性"——必须在提取层显式处理，否则会得到 -1/-2 的实际数值。这是高风险点。

### 3.4 反编译的 IL 源码（行为/流程事实源）

| 目录 | 文件数 | 说明 |
|---|---|---|
| `decompiled/ASharp/` | 4537 | Assembly-CSharp（含 UI/流程/客户端） |
| `decompiled/SRL/` | 1285 | ScenarioRuleLibrary + AStar + YML 等 |

关键类（均已确认存在）：`ScenarioState.cs`、`ScenarioRuleClient.cs`、`ScenarioManager.cs`、`CAbility.cs`、`CAbilityAttack.cs`、`CActor.cs`、`CPlayerActor.cs`、`AttackModifierDeckState.cs`、`Random.cs`、`AStar/`。

事件/日志相关类（**阶段 4 对拍的候选钩子**）：
`CCombatLogMessage_MessageData.cs`、`CAbilityEndUpdateCombatLog_MessageData.cs`、`CAbilityStartUpdateCombatLog_MessageData.cs`、`CAbilityTargetingCombatLog_MessageData.cs`、`CActionAbilityHasHappened_MessageData.cs`、`CAIFindingPath_MessageData.cs`。

### 3.5 全局规则参数

| 文件 | 内容 |
|---|---|
| `Global/Difficulties.yml` | 难度 → `ScenarioLevelModifier`、`HeroHealth`、`SummonHealth`、`FreeLongRest` 等 |
| `Global/LongRest.yml` | 长休卡（`ID: -1, Initiative: 99`） |
| `Global/Perks/*.yml` | 185 个 perk |
| `Global/ItemCards/*.yml` | 266 张物品卡 |
| `Global/HeroSummons/`、`Global/PetCards/`、`Global/IconGlossary.yml` | 召唤物 / 宠物 / 图标 |
| `Campaign/Scenarios/*.yml`(142) + `.lvldat`(149) | 场景定义与关卡布局 |
| `Campaign/ScenarioAbilities/`(432) | 场景能力 |
| `Campaign/BattleGoals/`(60) | 战斗目标 |

---

## 4. 提取与建模计划（草案，待确认）

### 阶段 1：结构化提取（产出 JSON/CSV + 字段说明 + 来源路径）

| # | 提取项 | 来源 | 产出 |
|---|---|---|---|
| 1.1 | 角色能力卡 | `Global/Characters/**/AbilityCards/*.yml` | `cards.json`（ID/Initiative/Discard/半边能力树/XP） |
| 1.2 | 攻击修正牌 | `Global/AttackModifiers/*.yml`（140） | `attack_modifiers.json` |
| 1.3 | 修正牌组 | `Global/AttackModifierDecks/*.yml`（4） | `modifier_decks.json`（含初始牌序） |
| 1.4 | 怪物基数据 | `Campaign/Enemies/Monsters/*.yml`（86×2） | `monsters.json`（等级表） |
| 1.5 | 怪物行动卡 | `.../Cards/*.yml`（384） | `monster_cards.json`（含 -1/-2 哨兵解析） |
| 1.6 | 全局参数 | `Difficulties.yml` 等 | `globals.json` |
| 1.7 | 关卡 | `*.lvldat`（149） | 地形/出生点/障碍（需 BinaryFormatter 解析） |

每项产出需附：**来源文件相对路径 + 关键行号 + 字段说明**。

### 阶段 2：模型文档（状态 / 事件 / 决策挂起点）

按你给定的架构：
- 状态机：`ROUND_START → CARD_SELECTION → INITIATIVE_SORT → TURN_LOOP → ROUND_END`
- 事件：`CARD_SELECTED / INITIATIVE_REVEALED / TURN_STARTED / ABILITY_RESOLVED / ATTACK_MODIFIER_DRAWN / DAMAGE_APPLIED / ELEMENT_INFUSED / ELEMENT_CONSUMED / TURN_ENDED / ROUND_ENDED`
- 决策挂起：优势/劣势选择、燃烧卡牌、多重选择 → 全部做成**可注入决策接口**，引擎不自动判定
- 怪物 AI：纯函数 `(monsterState, map, actors, drawnCard) → (path, targets)`
- 元素灌注：挂在 `END_OF_TURN`，能力结算中途不改元素表

### 阶段 3：最小战斗循环实现
### 阶段 4：对拍校准（见 §5 待确认项 1）
### 阶段 5：测试 / 回放 / 说明

---

## 5. 风险与需要你确认的问题

### 5.1 必须你决策（阻塞后续）

**Q1. 引擎实现语言 / 技术栈？**
- (A) **C#（.NET）**：与游戏同源，类型/命名可与反编译源码对齐，便于逐行对照；工作区已有 `src/FHSolver`（net472）与 237 项离线断言的测试台可复用。**推荐**
- (B) Python：原型快，但与游戏类型不对齐，逐行对照成本高
- (C) 其他

**Q2. 阶段 4 的"本体事件级输出"如何获取？**（这是验收标准的关键）
- (A) **运行时 hook**：沿用现有 BepInEx 插件，挂 `GameState.DrawAndApplyAttackModifierCards`、`ActorBeenDamaged`、`CCombatLogMessage_MessageData` 等，导出事件流。**已在实战中验证可行**（牌堆预测 66/66、伤害对拍 56/56 零不符）。**推荐**
- (B) 解析存档 `.dat`（BinaryFormatter）：只有**状态快照**，无事件序列 → 只能对拍"终态"，无法逐事件 diff
- (C) `Player.log`：实测**几乎不含战斗细节**，不足以对拍
- ⚠ 注意：(A) 需要向游戏进程注入插件（BepInEx 已在你的目录中）。这**不修改原始游戏文件**（注入在运行时），但需你确认可接受。

**Q3. 本地化文本是否提取？**
按你的约束"不复制文本资产"，我倾向**不提取**——引擎内部统一用 `locKey`（如 `$FH/062_...$` 或 `ID:62`）作标识，输出用 ID。请确认。

### 5.2 已识别的技术风险（我会处理，但需你知晓）

| # | 风险 | 影响 | 缓解 |
|---|---|---|---|
| R1 | 怪物卡 `Move: -1 / Attack: -2` 哨兵值 | 误读为负数 → 移动力/攻击力全错 | **结构性消除**：`monster_cards.json` 输出 `RawValues`（原值，仅溯源）与 `ResolvedValues`（已代入怪物属性）**双字段**，下游只用 `ResolvedValues` → 不可能误用（阶段 1 已落地并测试） |
| R2 | `meta1` 类型标签语义未完全确认 | 提取时可能漏分类 | 用路径后缀（`.yml`/`.lvldat`）反推，阶段 1 建立映射表 |
| R3 | `.lvldat` 需 BinaryFormatter 解析 | 关卡地形提取受阻 | 既有格式文档（`FORMAT.md`）；必要时可只做子集 |
| R4 | 元素/状态/地形/召唤等机制覆盖面大 | 阶段 3 可能过宽 | 阶段 3 先做**最小闭环**（移动+单体攻击+修正牌+先攻+元素时序），其余按阶段 5 补齐 |
| R5 | 引擎"决策挂起"与 AI 纯函数的设计边界 | 若 AI 内含随机/隐藏状态，则不可回放 | 强制：AI 为纯函数，随机源（修正牌序）显式入参 |

### 5.3 需要你补充的信息

1. 阶段 1 是否**全量提取**（3248 yml），还是先做一个**子集**（例如只 BannerSpear + Drifter + 2 个怪物类 + Standard 牌组）以快速打通链路？我倾向**先子集、后全量**。
2. 对拍的**基准场景**：是否有你常用的固定关卡/存档可作为回归基准？（例如 `AutoSave_102.dat`）
3. 引擎输出的**事件日志格式**：JSON Lines？还是结构化文本？

---

## 6. 本阶段变更

- **未编写任何实现代码**（按阶段 0 要求）
- 未修改原始游戏目录（全部为只读读取）
- 新增文档：`PHASE0_SCAN.md`（本文件，写入工作目录 `D:\1Mods\FH`）

**请确认 §5.1 的 Q1–Q3 与 §5.3 后，我再进入阶段 1。**

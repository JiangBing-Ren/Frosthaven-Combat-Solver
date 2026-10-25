# 霜港迷城 — 战斗求解器（Combat Solver）设计方案 v2

> 日期：2026-10-22
> 状态：**待确认**（确认后进入实现）
> v2 变更：修正牌机制已完全逆向（见 `MODIFIER.md`），
> **修正牌是确定量而非随机变量** → 功能 2 从"算期望"升级为"精确预知"。

---

## 1. 目标与范围

做一个**实时战斗辅助插件**，只读游戏状态、计算最优出牌与站位，以叠加 UI 呈现。
**不做任何数值修改**（不改 Base.ruleset、不改游戏状态）。

### 功能清单（用户确认）

| # | 功能 | 说明 | v2 变化 |
|---|---|---|---|
| 1 | 出牌组合推荐 | 枚举手牌「上半 + 下半」全部组合并排序 | — |
| 2 | 伤害预测 | 直接报出**本次翻出的确定修正牌**与确切伤害 | **改为精确值**（期望降为兜底） |
| 3 | 目标选择建议 | 建议集火目标 | — |
| 4 | 怪物意图预测 | 预测怪物下回合行动与伤害 | 同样可精确到修正牌 |
| 5 | 移动建议 | 建议站哪一格 | — |

求解深度：**单回合最优**（不推演多回合）。

---

## 1.5 核心前提：修正牌是确定量

> 完整论证见 `MODIFIER.md`。

**抽牌不消耗随机数**：`CActor.DrawAttackModifierCard` 直接取 `current[0]`，
全路径无一次 `Random.Next()`。牌堆顺序存在 `AttackModifierDeckState.AttackModifierCardsAvailable`
里，随存档持久化。

因此求解器**不需要算概率分布**，可以直接读出确切会翻出哪张牌。

| 用途 | API |
|---|---|
| 玩家/召唤物牌堆 | `CCharacterClass.m_AttackModifierCards` |
| 怪物牌堆 | `CMonsterAttackModifierDeck.m_AttackModifierCards` |
| 待洗标志 | `CCharacterClass.m_ShuffleAttackModifierCards` |
| RNG 当前状态 | `ScenarioManager.CurrentScenarioState.ScenarioRNG` |
| 克隆 RNG（模拟洗牌） | `Extensions.Save()` / `Extensions.Restore()` |

> ⚠️ 前三个是 **private 字段 / internal 成员**，Harmony 中要用
> `Traverse` 或反射（`AccessTools.Field`）读取，不要直接编译期引用。

**可精确预测的内容**：

- 本次攻击翻出的牌 = `m_AttackModifierCards[0]`
- 本次攻击后的连续翻牌 = `[1]`、`[2]`… → **AoE / 多目标攻击可整串预知**
- 翻到 ×0 / ×2 后本回合内**不会立即洗牌**（回合末才洗）→ 后续攻击继续可预测
- 需模拟洗牌时：克隆 RNG 后在克隆体上跑 `ShuffleAttackModifierCards` 算法，不污染真实状态

**顺带发现的现成后门**：`CAbilityAttack.ConsoleVar_DebugAttackModifierCard`
可直接指定翻出的修正牌，调试阶段极其好用。

---

## 2. 技术选型

> v2 全部条目均已实测确认（M0 已通过），不再是推断。

| 项 | 结论 | 依据（实测） |
|---|---|---|
| 引擎/后端 | Unity **2022.3.71.13221651** + **Mono** | BepInEx 日志 `Running under Unity v2022.3.71.13221651`；有 `MonoBleedingEdge`，无 `GameAssembly.dll` |
| CLR 运行时 | **.NET Framework 4.x**（`4.0.30319.42000`） | BepInEx 日志 `CLR runtime version: 4.0.30319.42000` → 插件用 `net472` 正合适 |
| 反射发射 | **Supports SRE: True** | BepInEx 日志 → Harmony 的动态方法可用，不受限 |
| 插件框架 | **BepInEx 5.4.23.5**（Doorstop 4.3.0） | 见下方版本抉择 |
| 注入方式 | Harmony **2.9.0.0**（`0Harmony.dll`，命名空间 `HarmonyLib`） | BepInEx 日志 `Harmony : 2.9.0.0` |
| 反作弊 | 无 EAC | 游戏目录无 `EasyAntiCheat` |
| 本地工具链 | dotnet SDK 10.0.401 + ilspycmd 11.1 | 已装 |

### BepInEx 版本抉择（推翻 v1 的推断）

v1 写的是「BepInEx 6.x Mono 版，5.4.x 仅到 2021，选错会静默失败」。**这条是错的**，
按发布时间与实测重新判定：

| 版本 | 发布时间 | 状态 |
|---|---|---|
| **BepInEx 5.4.23.5** | **2026-02-08** | **活跃维护的稳定线** → 采用 |
| BepInEx 6.0.0-pre.2 | 2024-08-27 | 两年前的陈旧预发布版 |

两者 core 都编译于 **net35**（引用 `mscorlib 2.0.0.0` + `System.Core 3.5.0.0`），
运行时由 Unity Mono 统一解析。5.4.23.5 自带的 Doorstop 是 **4.3.0**，比 6.0.0-pre.2 的更新。

**Harmony 程序集辨识**：`BepInEx/core` 下有两个——
`0Harmony.dll`（**HarmonyX 2.9.0.0**，命名空间 `HarmonyLib`，真正的实现，引用它）与
`0Harmony20.dll`（2.0.0.0 兼容 shim，不要引用）。

---

## 3. 已确认的游戏 API（ILSpy 反编译实证）

| 用途 | API |
|---|---|
| 当前场景状态 | `ScenarioManager.CurrentScenarioState` |
| 是否在战术（战斗）中 | `ScenarioManager.IsInTactical` |
| 玩家状态列表 | `ScenarioState.Players` → `List<PlayerState>` |
| 全部玩家已选卡 | `ScenarioState.GetAllPlayersSelectedAbilityCards()` |
| **手牌** | `CPlayerActor.CharacterClass.HandAbilityCards` |
| 本回合牌 | `CPlayerActor.CharacterClass.RoundAbilityCards` |
| 本回合已选牌 | `CPlayerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection` |
| **血量** | `CActor.Health` / `CActor.MaxHealth` |
| **位置** | `CActor.ArrayIndex`（`Point`，六边形坐标） |
| 多格怪物额外格 | `CActor.ExtraArrayIndexs` |
| 范围内单位 | `ScenarioRuleClient.GetActorsInRange(...)` |
| 相邻单位 | `ScenarioManager.GetAllAdjacentActors(actor)` |
| 卡牌类型 | `CAbilityCard`；攻击能力 `CAbilityAttack` |
| **玩家修正牌堆** | `CCharacterClass.m_AttackModifierCards`（private，反射读） |
| **怪物修正牌堆** | `CMonsterAttackModifierDeck.m_AttackModifierCards`（private） |
| 待洗标志 | `CCharacterClass.m_ShuffleAttackModifierCards`（public） |
| RNG 克隆 | `Extensions.Save()` / `Extensions.Restore()` |
| 调试后门 | `CAbilityAttack.ConsoleVar_DebugAttackModifierCard` |

> API 来源：`decompiled/CActor.cs`、`CPlayerActor.cs`、`ScenarioManager.cs`、
> `ScenarioState.cs`、`ScenarioRuleClient.cs`（已反编译留存）。
> 修正牌部分另见 `MODIFIER.md`，反编译产物在 `decompiled/SRL/ScenarioRuleLibrary/`
> 与 `decompiled/ASharp/`（全量，1501 文件）。

---

## 4. 架构

```
FHSolver/                          BepInEx 6 插件
├── Plugin.cs                      入口：Harmony 挂载、快捷键、配置、生命周期
├── Bridge/
│   ├── StateReader.cs             只读读游戏状态 → 快照（唯一接触游戏类型的地方）
│   └── DeckReader.cs              反射读修正牌牌堆顺序 + 克隆 RNG
├── Model/                         纯 POCO，不引用游戏类型
│   ├── BattleSnapshot.cs          战局快照
│   ├── UnitSnapshot.cs            单位快照（血量/位置/先攻/类型）
│   └── CardSnapshot.cs            卡牌快照（上下半能力/攻击值/射程/目标数）
├── Solver/                        纯算法，可脱离游戏单测
│   ├── CardComboEnumerator.cs     出牌组合枚举
│   ├── DamagePredictor.cs         精确伤害预测（读牌堆）+ 期望兜底
│   ├── ShuffleSimulator.cs        在 RNG 克隆体上模拟洗牌
│   ├── EnemyIntentPredictor.cs    怪物意图预测
│   ├── TargetAdvisor.cs           集火目标建议
│   └── MoveAdvisor.cs             移动建议
└── UI/
    └── Overlay.cs                 Unity UI 叠加呈现
```

**依赖方向单向**：`Bridge → Model ← Solver ← UI`，Solver 绝不反向依赖游戏。

---

## 5. 四条设计原则

1. **严格只读**：只用只读访问与 Harmony Postfix，**绝不 Prefix 改写、绝不改游戏状态**。
   这是不引发联机状态分叉的前提。
2. **快照隔离**：Solver 只吃 POCO 快照，不碰 `CActor` 等游戏对象。
   好处：可在游戏外单测、杜绝副作用、便于复制快照做假设推演。
3. **事件驱动**：不在 `Update` 每帧重算，只在状态实际变化时重算（回合开始、卡牌选中、移动后）。
4. **可开关**：快捷键（默认 F1）开关 UI，默认在联机时提示风险。

---

## 6. 核心算法

### 6.1 伤害预测（DamagePredictor）— v2 改为精确值

**首选路径：精确预测**（v1 的期望计算降级为兜底）

```
已知：牌堆 = [c0, c1, c2, ... ]        ← 从 m_AttackModifierCards 读出
本次攻击翻出的牌 = c0                    ← 确定，非随机
```

单次伤害（沿用 ATTACK.md 的三条铁律）：
```
伤害 = clamp( ((基础 + Σ加成 + c0的加值) × [×2|×0]) - 护盾 , 0, ∞)
```

多目标 / AoE / Rolling 修正牌会连翻多张，直接按 `[0]` `[1]` `[2]`… 顺序取用即可，
每翻一张都精确可知。

优势 / 劣势：
- `None`：抽 1 张 → 取 `c0`
- `Advantage`：抽 `c0`、`c1` → 按 `ChooseCardBasedOnAdvantage` 规则取较优者
- `Disadvantage`：抽 `c0`、`c1` → 取较差者

> 注意：`ChooseCardBasedOnAdvantage` 的排序不是纯数值比较，
> 还要看 `HasNegativeEffect` / `HasAdditionalEffect`，应直接复用该规则而非自造比较器。

**牌的优劣序**（取好/取坏时排序用）：
```
×2  >  +2 > +1 > +0 > -1 > -2  >  ×0
```

**兜底路径：期望计算**（仅在读不到牌堆时启用）

标准玩家牌组（20 张）：`+0×6, +1×5, +2×1, -1×5, -2×1, ×0×1, ×2×1`

- 无优势：`E = Σ P(牌) × dmg(牌)`
- 优势：按「两张中的较优者」分布
- 劣势：按「两张中的较差者」分布
- ReducedRandomness 牌组（若启用）：`+2_Shuffle` / `-2_Shuffle` 替代 ×2/×0，
  需按启用状态切换分布表。

---

### 6.1.1 洗牌模拟（ShuffleSimulator）

需要预测"跨过回合末"或"牌堆抽空"之后的牌序时才用：

```csharp
// 克隆 RNG，绝不碰真实状态（游戏自己的 PeekScenarioRNG 就是这个套路）
var rngClone = ScenarioManager.CurrentScenarioState.ScenarioRNG.Save().Restore();
// 在克隆体上跑与游戏一致的洗牌算法
// 消耗量 = 弃牌数 + (牌堆数 - 1)
```

RNG 本体是 `SharedLibrary.Random`（.NET Framework `System.Random` 的逐行拷贝，
Knuth 减法生成器）——**可离线复刻**，M2 阶段能纯离线单测。

### 6.2 出牌组合枚举（CardComboEnumerator）

Frosthaven 每回合从手牌选 2 张，一张取上半、另一张取下半：

```
组合数 = n × (n-1)     n = 手牌数
n=10 → 90 种（有序，因上下半不同）
```

每个组合输出：可造成的总期望伤害、涉及目标、是否需要移动。
按期望收益排序，取 Top-K 呈现。

### 6.3 怪物意图预测（EnemyIntentPredictor）

复用 ENEMY_AI.md 已逆出的规则：

1. **焦点目标**：距离最近 → 同距比先攻
2. **移动**：走到"能攻击到焦点目标"的格子（先移动再攻击）
3. 卡牌决定行为：`Target`（打几个）、`Range`（射程）、`PredominantlyMelee`（近战倾向）
4. 若当前位置已能攻击 → 不移动，直接攻击

输出：预测目标 + 预测落点 + 预测伤害区间。

### 6.4 移动建议（MoveAdvisor）

1. 取当前最优攻击组合，确定目标
2. 枚举"能攻击到该目标"的候选格
3. 对每个候选格评估**暴露度**（下回合有多少怪能打到你）
4. 综合「能打到目标」+「暴露度最低」排序推荐

---

## 7. 风险与缓解

| 风险 | 影响 | 缓解 |
|---|---|---|
| BepInEx 6 与 Unity 2022.3 实际兼容性 | 插件加载失败 | **M0 先用空白插件验证**，再写业务逻辑 |
| 游戏更新改 API | 插件失效 | 快照隔离 + 反射兜底 + 失败时优雅降级（不崩溃） |
| 联机状态分叉 | 联机异常 | 严格只读；联机时提示，建议仅单人使用 |
| 计算卡顿 | 帧率下降 | 事件驱动 + 组合数上限保护 |
| 位置/距离算法不正确 | 建议错误 | 复用游戏自身 API（`GetActorsInRange` 等），不自行实现距离 |
| 牌堆字段是 private | 读不到牌序 | 反射 + `AccessTools.Field`，读取失败自动回退到期望模式（仍可用） |
| 玩家撤消行为 | 预测瞬间失准 | 监听 `CUndo_MessageData` 与 `CopyScenarioState` 后主动重算 |

**关键缓解手段**：距离、可达性、范围判定**一律调用游戏自己的 API**，
不自己写 A*/六边形距离，避免与游戏判定不一致。

---

## 8. 里程碑

| 阶段 | 目标 | 验证方式 | 状态 |
|---|---|---|---|
| **M0** | 空白 BepInEx 插件能被游戏加载 | `BepInEx/LogOutput.log` 出现插件名 | **通过**（2026-10-22，类型探测 17/17） |
| **M1** | 只读拿到战局快照 + 牌堆顺序 | 插件日志输出：血量位置、手牌数、**当前牌堆前 5 张** | **通过**（2026-10-23，快照 195 次，异常 0） |
| **M1.5** | 牌堆预测正确性验证 | 预测下一张 vs 实际翻出的牌，连续 20 次全中 | **通过**（2026-10-24，**354/354 全中，零失配**） |
| **P1** | 单角色 × 单回合穷举（出牌 / 站位 / 目标 / 伤害） | 对局内「预测伤害 vs 游戏实际伤害」连续 20 次全中 | **代码完成**（2026-10-24，v0.6.0，boot 自检通过），**待实战校验** |
| **P2** | 加战役目标评分（使命 / 装备 / 专长 / 战绩 / 经验 / 战利品） | 回归断言「宝箱 120 > 材料 15 > 纯伤害」 | **完成**（2026-10-24，v1.6.0） |
| **P3** | 第二个角色纳入（联合出牌） | 同上 | **完成**（v1.3.0 团队联合，实际早于计划） |
| **P4** | 召唤物纳入 | 同上 | 部分完成（召唤物参与占位 + 敌人预测；主动召唤建模待做） |
| **P5** | 多回合滚动推演 | 回归断言：深度/位置/伤害/手牌递减 | **完成**（2026-10-24，v1.12.0，束搜索） |
| **M4** | UI 叠加 | 实机可用 | **完成**（2026-10-24，独立可拖拽窗口，Insert 开关） |

### P1 落地说明（2026-10-24，v0.6.0）

**两块前置逆向**（决定了 P1 能不能做得准）：

1. **手牌半边结构**：`CAbilityCard.TopAction` / `BottomAction` 各是一个 `CAction`，
   里面是 `List<CAbility>`，基类提供 `Strength` / `Range` / `NumberTargets` / `AbilityType`。
   出牌 = **一张牌出上半边 + 另一张牌出下半边**（Gloomhaven 规则），所以是 **n(n−1) 有序对**。
   先攻取"出上半边那张牌"的 `Initiative`。
2. **移动可达集不用自己复刻**：`ScenarioManager.PathFinder` 是 **public static**，
   `FloodFill_ToRange(actor, 起点, 移动力, ...)` 的**起点可以填任意假想坐标**。
   地形 / 占用 / 困难地形 / 危险地形 / 门 / 冰面滑动全交给游戏算，避免推荐走不到的格子。

**新增文件**：

| 文件 | 职责 |
|---|---|
| `Solver/DamageModel.cs` | 伤害模型。加在括号内、乘除在括号外、护盾最后减、截断到 0 |
| `Solver/ModifierDraw.cs` | 模拟一次攻击的抽牌（rolling 链 + 优劣势二选一） |
| `Solver/P1Planner.cs` | 出牌组合 × 执行顺序 × 站位 × 目标穷举 + 评分 |
| `Solver/TurnPlan.cs` | 方案 POCO |
| `Bridge/Geometry.cs` | 可达格 / 射程 / 视线，全部转调游戏自己的实现 |
| `Bridge/P1Runner.cs` | 驱动层：F2 → 建 context → 跑穷举 → 打可读推荐 |
| `Bridge/DamageVerifyPatch.cs` | **对拍钩子**：挂 `GameState.DrawAndApplyAttackModifierCards` |

**验证策略（不靠"感觉准"）**：
`GameState.DrawAndApplyAttackModifierCards` 是伤害计算的咽喉 ——
进它之前牌堆没动（能拿到抽牌前的完整牌序），返回值是**护盾前**伤害，
out 参数给出本次**真正用到**的牌。于是 Prefix/Postfix 一对，就能把
「牌序预测 / rolling 处理 / 优劣势处理 / 加减乘顺序」四件事一次性验掉。
它**不验**基础攻击力（那是游戏直接传进来的），所以另外打了
`基础攻(卡)=X 实际基础=Y 差=Y-X` 一列，用来观察常驻 buff 的偏移量。

**操作**：战斗中按 **Ctrl+F2** 出推荐（`TopN` 可在 config 调，默认 5）；
默认给**全队每个角色**各出一份（`PlanAllPlayers`，这游戏强制两人开局，单人档也是多角色），
收尾跑 `bash tools/collect_p1.sh`。

**地块坐标显示**（原版自带，插件只负责打开）：
`DebugMenu.ConsoleVar_ShowTileIndex` 打开后每块地头顶显示 `X:Y(通行标记)`。
标签在 `ClientScenarioManager` 建图时就为每块地创建好了（`TileBehaviour.CreateWorldspaceTileBehaviourUI` →
`WorldspaceTileBehaviourUI.ArrayIndexText`，TextMeshProUGUI），所以**不用自己造 UI**。
默认「按住才显示」，且**跟着控制走不跟着按键走**：从 FHControls 的输入绑定 JSON 里查实，
Tab 绑的是 `ScenarioWorldspace/HighlightInteractablesOverlay`，读的是 `InputAction.IsPressed()`，
玩家改键后自动跟着变。想常亮就把 config 的 `HoldToShow` 关掉。

**P1 明确未建模（不装作算准）**：
- 多目标 / 范围攻击（`AreaEffect != null` 或 `NumberTargets != 1`）→ 方案标 ⚠ 并沉底
- 半边内的治疗 / 护盾 / 元素注入 / 推拉 → 只记 XP，不进评分
- 半边内统一按「先移动后攻击」
- 敌人**会移动到哪**没有预测（只展示它本回合行动卡的意图），见 P4

---

### v1.3 修正：卡面语义 + 基础动作 + 团队联合规划（2026-10-24）

用户实测反馈四个问题，逐个落地修复。这一版的主题是
**"先把规则理解对，再谈最优"** —— 之前求解器把卡面半边一律当成"移动/攻击动作"，
于是推荐出游戏里根本不存在的操作。

#### ① 常驻加成 ≠ 动作

`CAbility.ActiveBonusData`（`AbilityData.ActiveBonusData`）非空且
`Duration != EActiveBonusDurationType.NA` 时，这一半是**常驻加成**，不是本回合的动作。

实证（死行「深渊力量」`100_Deathwalker_StrengthOfTheAbyss.yml`）：

```yaml
Bottom:
    BBuffMove:   { Move: 2,  ActiveBonus: { Behaviour: BuffMove,       Duration: Persistent,
                                            ValidAbilityType: Move,   IsToggleBonus: True } }
    BBuffAttack: { Attack: 2, ActiveBonus: { Behaviour: BuffSingleAttack, Duration: Persistent,
                                            ValidAbilityType: Attack, Hidden: True,
                                            IsToggleBonus: True, IsSingleTargetBonus: True } }
```

`Move: 2` / `Attack: 2` 是**加成幅度**，表示"以后每次移动 +2""每次攻击 +2"，
**不是**这一回合能移动 2 格 / 攻击 2 点。之前求解器直接拿它当动作，
于是推荐「用深渊力量下半移动 2」这种根本执行不了的方案。

现在 `ActionSnapshot.Kind` 分三档：`Empty` / `Action` / `PersistentBonus`，
只有 `Action` 才进入枚举；常驻加成与未建模能力都记进 `UnmodeledTypes`，
在诊断里明确告知玩家。

#### ② 基础攻击 2 / 基础移动 2

任何一张牌都能不当卡面能力用、改当通用的「基础攻击 2」（攻 2 / 射程 1 / 单体）
或「基础移动 2」（走 2 格）。数值从 `card.DefaultAttackAction` /
`DefaultMoveAction` 里读（不写死 2/1 —— 增益和道具会改它）。

组合数会炸，所以**只在必要时**生成基础动作候选：
该卡至少有一半不是我们建模得出的动作（是空的、或是常驻加成）时。
像「深渊力量」这种下半是常驻加成的牌，它唯一能做的事就是基础动作；
两半都是正常动作的牌，玩家几乎不会想拿它当 Attack2 用。

输出里每一步都写清来源：`打出「X」的【上半】` / `把「X」当【基础攻击 2】打`。

#### ③ 跨角色占位协同 + AOE 站位限制

**占位按"我行动那一刻"算，不是所有人当前位置**：

| 单位 | 我行动时它在哪 |
|---|---|
| 先攻更低的队友（先动） | 它方案的**终点** |
| 先攻更高的队友（后动） | **当前位置**（还没动） |
| 敌人 | **当前位置**（动到哪未知，保守全占） |
| 盟友 / 中立 / 召唤物 / 倒地 | 当前位置 |

这一条直接解决用户说的
「旗矛没考虑 (14,4) 已被先攻更快方案的大棒占据」——
大棒「义无反顾」是"移动+攻击"合并、下半还能再走两步，
他根本不会停在攻击位上，那一格旗矛本来是能用的。

**范围攻击（AreaEffect）**：像旗矛「绞肉机」上半这种带
`AreaEffect: "0,0,G|1,1,R|2,0,R|3,1,B"` 的攻击，
要先选焦点格、按固定形状铺开，一次能打好几个 ——
单体伤害模型算不准，所以**识别出来就不建模**，
诊断文本里把形状原样给玩家，让他自己摆站位
（**G=灰格，你自己站这儿；B=蓝格，必须每个都有盟友站着；R=红格，敌人格，至少 1 个有效目标即可**）。

#### ④ 先攻顺序纳入队友方案

之前【先攻顺序】里三个队友全显示 0，因为
`CPlayerActor.Initiative()` 取的是
`CharacterClass.InitiativeAbilityCard`，而这张"领头牌"
要等玩家出牌才定 —— 选牌阶段读出来就是 0。

现在：先跑第一遍各自独立算，再用**每人方案里先打出的那张牌**
的先攻作为该角色的先攻，据此排序。输出里标「取自该角色的推荐方案」；
怪物标「预测，取自下一张行动牌」。

**串行两遍法**（把三份独立路径咬合成一份团队方案）：
1. 第一遍：各自独立算（占用 = 其他所有人当前位置）
2. 按先攻升序逐个重算：
   - 把**先动队友的终点**算进占用集（不是当前位置）
   - 把**先动队友预计造成的伤害**从目标有效血量里扣掉
   - 重算完把自己的终点和伤害累积进去，供后动者使用

**已知近似**：每个人仍是在"给定队友预测"的前提下最大化自己的评分，
不是全局联合最优。真正的联合最优要等 P5（多人 × 多回合搜索），
到那时组合空间才值得上更强的方法。

---

### 敌人位置到底可不可以预测？（2026-10-24 查证）

用户问：「敌人的位置为什么是未知的？敌人 AI 有随机性吗？」

**结论：敌人 AI 没有任何随机性。上一版把「位置未知」写进报告是措辞错误，
它掩盖了一个真实的建模缺陷。**

#### 硬证据

| 文件 | 行数 | `Random` 出现次数 |
|---|---|---|
| `CAIMoveNew.cs`（怪物移动/攻击决策核心） | 1975 | **0** |
| `CMonsterClass.cs` | 1330 | **0** |
| `CEnemyActor.cs` | 751 | **0** |

全库 `Random` 只出现在这些地方，全都**不进入 AI 决策**：

- `CObjectChest.cs`(43) / `LootState.cs`(35) / `CActor.cs`(15) — 宝箱与战利品的随机内容
- `ScenarioState.cs`(40) — RNG 通道的状态本身（`EnemyAbilityCardRNG` 等）
- `CMonsterAttackModifierDeck.cs`(5) — 怪物修正牌堆洗牌
- 各 `ScenarioScript_FH*.cs` — 特定关卡脚本

怪物决策链路 `CAIMoveNew.Process()` 全程是**确定性计算**：
`ResolveFocusOverride` → `ResolveAttackAbility` → `InitPathfinderContext`
→ `AddActorsToPathFinderAndEnemyEvals` → `FloodFill`
→ `CalculateValidAttackTilesForAllEnemies` → `SortEnemyEvals`
→ `FindBestAttackPositionForTheFocus` → `FindPathToFocus` → `CalcWaypoints`。

焦点排序 `CompareEnemies` 也是纯比较、无随机 tie-break：
`isFocusOverride` 降序 → `lowestMoveCostToAttack` 升序 → `hexDist` 升序
→ `GameState.s_ActorAdjustedInitiativeComparer`（先攻）。

#### 那"未知"是怎么回事

敌人去哪**不是随机的，但也不是独立可知的** —— 它取决于**玩家当时站在哪**。

关键时序：玩家在选牌阶段，怪物还没动。等玩家出牌、进入执行阶段后，
是按先攻顺序逐个执行。所以：

- **怪物先攻 < 我** → 它在我之前就执行完了自己的「移动 + 攻击」→
  它**原来的格子一定会空出来**（只要它本回合带移动）
- **怪物先攻 > 我** → 轮到我时它还没动 → 它还在原位，占位

这个判断是**必然成立**的，不需要掷骰子。所以之前"一律按当前位置占位"
是过度保守：明明敌人会让开，我却把那格锁死，
玩家白白少了一个可选站位——大棒/旗矛那局就是这么错的。

#### 唯一真正不可知的场景

怪物行动牌堆**空了要洗牌**时：

```csharp
m_AbilityCards.Shuffle(ScenarioManager.CurrentScenarioState.EnemyAbilityRNG);
```

这时下一张牌是什么确实现在读不到。但按 MODIFIER.md 的同一套结论，
`EnemyAbilityRNG` 状态随存档持久化、且 `SharedLibrary.Random`
是 .NET `System.Random` 的逐行拷贝（56 槽 + 双指针），**可以离线复刻**。
这一条留到 P4。

#### 本次落地

- 新增 `Bridge/EnemyPredictor.cs`：**只读**预测敌人落点，全程零 Random。
- 占用集改为按**先攻**判断：先动且会移动的敌人，释放它的原格、占用它的落点。
- 报告里把「位置未知」改成准确措辞，并新增【敌人动向】段落。
- 敌人行动意图（移动 X / 攻击 Y 射程 Z / 目标数 N）在【场上单位】里列出。

#### 落点预测怎么算的

```
① 焦点（追谁）
   = 与它敌对的存活单位中，「走到能攻击到它的位置所需代价」最小者；
     平局依次比 六边形距离 → 调整后先攻
   （代价与距离都用 Geometry.RangeDistance = 寻路长度，不是直线 HexDist）

② 落点（走到哪）
   = 可达格里「能打到焦点」且「离自己最近」的那格

③ 若现在站的格子就已经能打到焦点 → 它不动
```

按先攻升序遍历敌人，已移动的怪物会把落点计入占用，
所以后动的怪物不会撞到先动怪物的新位置。

**近似**：不建模怪物之间的互相阻塞的完整顺序推进（P5 再补）。
**真正随机只剩**：怪物行动牌堆空了要洗牌
（`Shuffle(CurrentScenarioState.EnemyAbilityRNG)`）——
这个 RNG 状态随存档持久化、且可离线复刻，见 MODIFIER.md。

---

### v1.4 修正：出牌规则 + 移动落脚限制（2026-10-24，用户实战再反馈）

#### ① 出牌规则：两张牌必须**一张上半、一张下半**

上一版我把枚举"一般化"成每张牌可任选上下，理由是"漏掉了 (上,上)、(下,下)
这些合法组合"。**这个判断是错的** —— 它们根本不合法。

Gloomhaven / Frosthaven 规则：一回合打出两张牌，**一张执行其上半、
另一张执行其下半**。用了某张牌的上半，这张牌的另一半也不能再用了，
另一张牌的上半同样用不了。

现在按**两个槽位**枚举：

```
上半位候选 = { 该牌的上半能力, 通用基础攻击 2 }
下半位候选 = { 该牌的下半能力, 通用基础移动 2 }

枚举 = 有序对 (i 出上半位, j 出下半位), i≠j
     × 上半位候选
     × 下半位候选
     × 执行顺序（先做上半位 / 先做下半位）
```

组合数 n(n−1)×2×2×2（12 张手牌约 1056），比之前的错误版本小，且**全合法**。
回归用例：`PlayRuleTests()` 断言「两张都出上半」「两张都出下半」的方案数均为 0，
且没有方案把同一张牌用两次。

#### ② 移动落脚限制（`CAbilityMove.MoveRestrictionType`）

```csharp
public enum EMoveRestrictionType
{
    None, StraightLineOnly, MustEndNextToObstacle,
    MustEndOnShadow, MustEndOnSpecificTiles, MustEndNextToEnemy
}
```

死行「暗影步法」上半 `99_Deathwalker_ShadowStep.yml`：

```yaml
Top:
    TMove:   { Move: { Amount: 3 }, MoveRestrictionType: MustEndOnShadow }
    TAttack: { Attack: { Amount: 3 } }
```

即"先移动 3 再攻击 3"，但**移动必须停在阴影格上**。不认这条就会推荐
根本站不住的落点。游戏自己的校验在 `CAbilityMove.cs`：

```csharp
if (MoveRestrictionType.Equals(EMoveRestrictionType.MustEndOnShadow)
    && !GameState.GetTilesInRange(...).Any(x => x.FindPropOfType<CObjectShadow>() != null))
{
    SetCancelAbilityFlag(cancel: true);   // 没有可停的阴影格 → 整个能力取消
}
```

处理策略（宁可保守，也不给错的落点）：

| 限制 | 做法 |
|---|---|
| `None` | 正常枚举可达格 |
| `MustEndOnShadow` | 可达格 ∩ 阴影格（判定照抄 `CTile.FindPropOfType<CObjectShadow>() != null`） |
| 其余四种 | **只保留原地不动**，诊断里写明限制与"未建模" |

回归用例：`MoveRestrictionTests()` 断言用了「暗影步法」上半的方案落点全在阴影格上。

---

### 社区攻略核对（2026-10-24，来自 `.workbuddy/Documentation`）

用户提供了 20 份社区攻略，本轮先精读了**旗矛**与**死行**两份
（`.workbuddy/Documentation/旗矛Bannerspear Guide.docx` 约 10.3 万字符、
`死亡行者Deathwalker Guide.docx` 约 7.6 万字符）。
攻略用直接解析 docx 提取：`build/guides/{deathwalker,bannerspear}.txt`。

#### ★ 修正一处我之前的错误理解：形状里的 **B 不是"空/阻挡格"**

我原先注释写成 `B=蓝/空或阻挡格`，**错的**。攻略（旗矛 line 5）原文：

> "Each formation describes an AOE hex pattern including a space **where you need to stand**
> and one or two spaces **where allies need to stand**. You can only perform these abilities
> **if each blue hex is occupied by one of your allies**. While each blue hex does need to be
> occupied, **you don't have to fill every red hex**; like with standard AOE attacks,
> you simply need **at least one valid target**."

正确含义：

| 字母 | 含义 |
|---|---|
| **G** | 灰格，**你自己必须站这儿** |
| **B** | 蓝格，**每个蓝格都必须有盟友站着**，缺一个这个能力就发动不了 |
| **R** | 红格，敌人格；**不必填满**，只要有 ≥1 个有效目标即可 |

所以「绞肉机」`0,0,G|1,1,R|2,0,R|3,1,B` = 你站 G，要 **1 个盟友站 B**，
形状最多覆盖 2 个敌人格。这正是用户说的"绞肉机上半攻击有特定站位限制"。
已加入 `P1Planner.FormationRequirement()`，诊断里直接翻译成人话。

#### ★ 带形状 ≠ 一定是 AOE：「阵型单体」

旗矛 line 72（Boldening Blow）：

> "Boldening Blow is **not** an attack 4, disarm on four targets... it's an attack 4, disarm on
> **one adjacent target** within the red hexes. **The attack is not AOE at all**,
> it's just written as such because that's how formations look, and you need some allies
> lined up to validate it."

所以我之前把 `AreaEffect != null` 一律当"范围攻击"是**过度归类**。现在按目标数区分：

- `NumberTargets != 1`（含 0 = 打形状内所有）→ **真 AOE**
- `NumberTargets == 1` 且有形状 → **阵型单体**（形状只是站位要求，实际只打 1 个）

⚠ 注意「绞肉机」的真实 YAML **没有 `Target` 字段** → `NumberTargets = 0`
→ 属于**真 AOE**（形状里 2 个 R 格，最多 2 个目标，与攻略描述一致）。
这条也写进回归，避免再把 0 误判成单体。

#### 死行的阴影机制（攻略 line 5）

- **上限 5 个**："You can only have five shadows at once; if you would place a sixth,
  you must first remove an existing shadow."
- 阴影的放置/移动**忽略除墙和其他阴影外的一切**
- **相邻判定是特例**："Shadows are considered adjacent to figures both in or adjacent to
  their own hex."（对自己格内**或相邻格**的图形都算相邻）
- **阴影不是负面格**："Shadows aren't negative hexes, so enemies won't avoid them."
  → 敌人移动预测**不需要**绕开阴影，现有 `EnemyPredictor` 行为正确。

#### 变量强度（`StatIsBasedOnX`）—— 卡面写的值可能是下限

攻略 line 32（深渊力量）："If we have all of our shadows out, this will be an **attack six**
as a nonloss action." 而卡面只写 `Attack: 1`（= 1 + 阴影数）。
不标注就会按 1 去算，严重低估。已加 `ActionSnapshot.StrengthIsVariable`
（反射读 `CAbility.StatIsBasedOnXEntries`），诊断里提示"卡面值是下限"。
精确求值要跟着 `StatIsBasedOnX` 的表达式走，留到 P2。

#### 旗帜（Banner）规则 —— 旗矛核心，影响占位与 Lost

攻略 line 6 / 33 / 342：

- 旗帜是**由卡牌下半创建的 lost 召唤物**，死了无法恢复
- **不移动、不攻击**，但可被打断/推/拉/授予移动
- 有血量，会被敌人攻击；**敌人会在有选择时优先打旗帜**
  （"the monster will focus your banner given the choice"）
- 光环加成对自己也生效（"since you are an ally to your banner"）
- **Banner of Strength 的加成是 `once per turn`，不是 `once per round`**
  → 召唤物在自己回合被授予攻击时，能再多拿一次加成

对求解器：旗帜是**我方单位但要占格、且是敌人的优先目标**，
后续 P2 建模站位/敌人焦点时要单独考虑这一类。

#### 先攻编织（initiative weave）—— P2+ 的评分维度

攻略多次强调（死行 line 50 / 118）：

> "I typically try to **initiative weave** with this one, pairing it with a very fast or slow
> initiative to ensure that I don't end up being punished for the aggressive positioning
> that this action requires."
> "they will need to **initiative weave** in order to have the highest success"

即先攻不只是"谁先动"，还是**生存手段**：激进站位要配快先攻（先打完就走）
或慢先攻（等敌人走完再进）。**当前 P1 评分只有伤害 / 击杀 / XP，没有先攻安全性**，
这是 P2 明确的改进方向。

### M0 实测记录

```
[Message:   BepInEx] BepInEx 5.4.23.5 - Frosthaven (2026/10/21 22:31:13)
[Info   :   BepInEx] Running under Unity v2022.3.71.13221651
[Info   :   BepInEx] CLR runtime version: 4.0.30319.42000
[Info   :   BepInEx] Supports SRE: True
[Info   :   BepInEx] System platform: Bits64, Windows
[Message:   BepInEx] Chainloader started
[Info   :   BepInEx] 1 plugin to load
[Info   :   BepInEx] Loading [FHSolver 0.1.0]
[Info   :  FHSolver] [FHSolver] Harmony   : 2.9.0.0
[Info   :  FHSolver] [FHSolver] Unity     : 2022.3.71f1
[Info   :  FHSolver] [FHSolver] 产品名     : Frosthaven v1.0.34121
[Info   :  FHSolver] [FHSolver] 类型探测结果: 17/17 命中
[Message:   BepInEx] Chainloader startup complete
```

**启动后 5 秒内即完成链加载**，无需等到主菜单，验证很快。
`Supports SRE: True` 意味着 Harmony 的动态方法生成不受限制，M1+ 的 Hook 通路无障碍。

M2 的算法可**完全离线单测**（不启动游戏），用 ATTACK.md 里的
4996 条真实表达式做回归校验——这是保证算得对的关键。

**M1.5 是本方案独有的强验证点**：因为修正牌是确定量，
可以直接用 `ConsoleVar_DebugAttackModifierCard` 之外的纯只读方式，
逐次断言「预测 == 实际」，无需统计、一次即证伪。

### M1.5 实测结果（2026-10-24，v0.4.0）

**354 次抽牌，354 次命中，0 失配，洗牌跳过 0**（判定标准只要求连续 20 次）。

```
抽牌#1:  预测=AMC_Basic_M1  实际=AMC_Basic_M1  ✔ 命中1/1   连续1
抽牌#2:  预测=AMC_Basic_P1  实际=AMC_Basic_P1  ✔ 命中2/2   连续2
...
抽牌#354: 预测=...          实际=...           ✔ 命中354/354 连续354
```

同轮统计：快照 1075 次、心跳 3987 次、异常 0、`owner=NULL` 0、`latch=True` 3143 次。

**含义**：修正牌不是"概率上可预测"，而是**确定可读**。
求解器不需要算期望，也不需要蒙特卡洛——直接读序列即可。
这是整个方案区别于通用战斗求解器的根基，现已由实战数据坐实。

> 注：354 次全部落在非洗牌局（洗牌跳过 0）。
> 洗牌后的顺序需复刻 `ScenarioRNG` 或重新读取（见 `MODIFIER_TYPES.md` §7）。

### M1 快照样例（同轮，`RFS_06_On_Thin_Ice` 回合 4）

```
-- 玩家 3 名 --
  秉姜[DrifterID] 8/12 @(13,6) 先攻0  #1c9dbcdb 手牌5
    剩19 弃1 待洗=False 下一张=+0
    牌序(前5): +0 +1 *0[洗] -1 +1 …(+14)
  列阵旗矛[BannerSpearID] 12/12 @(4,3) 先攻67 #f1b5f84a
    剩15 弃4 下一张=+0   牌序: +0 +2 +1 -1 -1 …(+10)
  死行[DeathwalkerID] 7/7 @(12,5) 先攻24 #3eea5d91
    剩18 弃0 下一张=+0   牌序: +0 +1 -1 -1 -2 …(+13)
-- 怪物 7 只（另有已死 6 只）--
  VermlingScoutID#1 3/3 @(11,5) 先攻0 #94111ee3
  PolarBearID#4     10/10 @(12,4) 先攻0 #ca14d8bb   …
-- 怪物牌堆 --
  怪物-敌: 剩13 弃9  牌序: +2 -1 +1 +1 *2[祝] …   ← ★ 出现祝福牌
  ...
```

两点确认：
1. `AllAliveMonsters` 修正生效——死亡单位不再混入（此前会出现 `-9/3` 这类负血条目）。
2. 怪物牌堆出现 `*2[祝]`，印证了"怪物牌堆总数可超过 20"的观察：
   那是 **Bless 被置入**（`MonsterClassManager.AddBlessCardToEnemies`），与代码分析一致。

### M1 实测记录（2026-10-23，v0.3.2，实战打到第 13 回合）

场景 `RFS_07_Assault_On_The_Warchief`，玩家 2 名、怪物 18 只：

```
===== 战局快照 (回合 8 → 9) =====
场景 RFS_07_Assault_On_The_Warchief | 回合 9
-- 玩家 2 名 --
  秉姜[DrifterID] 5/12 @(4,9) 先攻27 手牌0 已选0
    秉姜: 剩12 弃8 待洗=False 下一张=+1
    牌序(前5): +1 +0 +0 -1 +1 …(+7)
  食人魔魔法师[GeminateID] 8/9 @(8,10) 先攻36 手牌7 已选0
    食人魔魔法师: 剩20 弃0 待洗=False 下一张=+1
    牌序(前5): +1 +0[滚] +1 +1 *2[洗] …(+15)
-- 怪物 18 只 --
  VermlingWarchiefID#1[VermlingWarchiefID] 14/24 @(5,9) 先攻0
  VermlingPriestID#5[VermlingPriestID] -9/3 @(4,9) 先攻0
  ...（略）
-- 怪物牌堆 --
  怪物-敌: 剩20 弃1 待洗=False 下一张=+0
    牌序(前5): +0 +1 +0 +0 +0 …(+15)
  怪物-友: 剩20 弃0 待洗=False 下一张=-1
    牌序(前5): -1 -2 -1 *0[洗] *2[洗] …(+15)
  怪物-敌2 / 怪物-中立 同样完整
```

**关键结论**：两副玩家牌堆合计均为 20 张，与 MODIFIER.md 的标准牌组规模一致；
牌序、`[滚]`（Rolling）、`[洗]`（Shuffle）属性全部正确读出。
**这就是"修正牌可精确预知"的直接证据——剩余牌序即未来翻牌序列。**

统计：心跳 599、快照 195、`latch=True` 537、`owner=NULL` 0、异常 0、读取失败 0。

### M1 发现的两个数据质量问题（M3 待办）

1. **怪物先攻恒为 0**：`CActor.Initiative()` 基类默认返回 0，
   `CEnemyActor.Initiative()` 走 `MonsterClass.Initiative(this, includeOverrideCard: true)`，
   依赖本回合行动卡 `CMonsterClass.RoundAbilityCard`。回合开始时怪物尚未选牌，故为 0。
   ⇒ **M3 的怪物意图入口已定位**：`CMonsterClass.RoundAbilityCard`（本回合）/ `NextAbilityCard`（下一张）。
2. **死亡怪物未过滤**：`CActor.IsDead => CauseOfDeath != ECauseOfDeath.StillAlive`，
   快照中出现 `-9/3` 之类负血条目，说明 `AllMonsters` 里仍保留已死单位，需按 `IsDead` 过滤。
3. 次要：怪物牌堆出现「剩20 弃1 = 21」超出标准 20 张，怀疑与 Bless/Curse 置入有关，待核。

---

## 9. 待决定事项 —— **已由用户决定（2026-10-24）**

| # | 议题 | 决定 | 落地 |
|---|---|---|---|
| 1 | UI 呈现形式 | **独立可拖拽窗口** | `Bridge/SolverUI.cs`（Unity IMGUI，`GUI.DragWindow`），标题栏整条可拖，右上角 x 关闭，位置自动夹在屏幕内 |
| 2 | 快捷键 | **Insert** | 已从游戏输入绑定实测确认空闲；`Plugin.SolveKey` 默认 `KeyCode.Insert` |
| 3 | 信息粒度 | **参考杀戮尖塔 2 的 Combat Solver 模组** | 见下方「信息粒度」 |

### 2 的实测依据（重要）

从 `FHControls.cs` 里的 `InputActionAsset.FromJson(...)` 解析出游戏真实绑定，
**游戏实际只占 39 个键盘键**：

```
1 2 3 4 a alt b c comma ctrl d downarrow e end enter escape f5 f9 l leftarrow
leftbracket numpadenter numpadminus numpadplus pagedown pageup period q r
rightarrow rightbracket s space t tab uparrow v w x
```

候选键位实测：

| 键 | 状态 |
|---|---|
| **Insert** | **free** ✅ |
| F10 / F11 / F12 | free ✅（备选） |
| Home | free ✅（备选） |
| `~` backquote | free ✅（备选） |
| **End** | **USED** ❌ `ScenarioWorldspace:ToggleSpeedUp` |
| Tab | USED（已在用于"按住显示地块坐标"） |

⚠ 顺带更正一条旧结论：v1.x 曾记录「游戏自带 F2 = 隐藏 UI，所以求解键改成 Ctrl+F2」。
按绑定实测 **F2 其实是空闲的**，当初那次观察到的现象另有原因。现已统一改为 Insert。

⚠ BepInEx 会把 config 落盘，**改默认键/加新配置项后必须删除**
`BepInEx/config/hanf2.Frosthaven.FHSolver.cfg` 才会生效（已备份并删除）。

### 3 信息粒度（参考 STS2 Combat Solver）

`P1Runner.ToUiLines()` 把同一份报告结构化成 `UiLine{Text, Style}`，
UI 只按样式渲染。分四层：

1. **总览** —— 先攻顺序前 6 名（敌/友分色）
2. **使命目标** —— 逐个列出，标明 已完成 / 可选 / 单独即可通关 / 力竭仍赢
3. **可捡拾 + 敌人动向** —— 宝箱与战利品袋位置、下一张战利品是不是装备；会先动的敌人与其预测落点
4. **推荐** —— 每个角色的方案，Top-1 用 `Good`（绿）高亮，警告类用 `Warn`（橙），调试信息用 `Dim`（灰）

与 STS2 Combat Solver 一致的原则：**结论在上、细节在下、不确定的单独标出来**，
不把"未建模"和"确定结论"混在一起显示。

### v1.7 修一批用户反馈（2026-10-24 实战后）

| # | 反馈 | 处理 |
|---|---|---|
| 1 | 鼠标移入/左键/滚轮仍影响游戏；要加高标题栏、加折叠+重算按钮；Insert 要能关窗口 | 见下 |
| 2 | 要分类：\|漂泊者\| \|旗矛手\| \|死亡行者\| \|敌人\| \|战利品\| \|日志\| \|报错\| | 见下 |
| 3 | 死行「腐烂萦绕」下半的**中毒**没提到 | **是真 bug**，已修并加回归 |
| 4 | 血量要显示「当前/最大」，后续"先动单位预计打…"保留 | 已修 |
| 5 | 出牌顺序与先攻的关系 | **已定并落地**（先攻来源独立枚举 + 按先攻升序展示） |
| 6 | Ctrl+F2 仍在生成方案、且是旧方案；要清理遗留 | 根因已找到并修 |

#### 1 的具体实现

- **输入不透传**：鼠标位于窗口矩形内时 `InputSystem.DisableDevice(Mouse.current)`。
  这是**只禁鼠标设备**——键盘不受影响，所以 Insert 仍能开关窗口。
  IMGUI 的 Event 走 legacy 通道，独立于 Input System，窗口自己照常收事件。
  （在 `Update` 里判定而不是 `OnGUI`，因为 `OnGUI` 一帧会调多次。）
  ⚠ 鼠标位置也只能用 `Mouse.current.position.ReadValue()`，
  不能碰 legacy `Input.mousePosition` —— 项目没引用 `InputLegacyModule`，
  且 Input System 原点在左下、GUI 原点在左上，需要翻转 y。
- 布局：标题栏加高到 26px（整条可拖）→ 按钮栏「重新计算」→ 分类页签 → 内容。
- 右上角 `-`/`+` 收起/展开，`x` 关闭。
- **Insert = 真开关**：开着→关掉；关着→跑求解并打开。

#### 2 的具体实现

`Model/UiSection.cs` + `SolverUI.SetSections()` + `P1Runner.ToUiSections()`：

- 每个角色一页：从报告分节标题 `===== [N] 角色名 | 先攻 X =====` 里正则抽出角色名做页签名
- **敌人**：先攻顺序 + 血量 + 意图；再单列"会先动、会让出原格"的预测
- **战利品**：使命目标 + 地上可捡拾 + 下一张战利品是不是装备
- **日志**：报告原文（灰字，方便排查）
- **报错**：汇总所有 ⚠/未建模 行（没有就显示"无"）

#### 3 的根因（重要教训）

`DescribeHalf` 里那段「⚠ 本半边还有未建模的效果」**在一次重构中被删掉**了，
而 `!half.HasAttack` 分支又直接 `return`，于是死行「腐烂萦绕」下半
（`BMove: Move 3` + `BPoison: Poison, Range 1, Target 1`）只剩一句光秃秃的
「只有移动3，无攻击」，**中毒完全没提**。
`P1Planner.Diagnose` 的"无攻击"分支同样漏了。

→ 教训：**未建模能力必须提示**这条规则没有回归保护，重构时静默丢失。
现在加了 `UnmodeledEffectTests()` 锁住：断言 `Poison` 进 `UnmodeledTypes`
且诊断里点名了它。

#### 5 的具体实现：先攻来源与执行顺序解耦（v1.8.0）

用户的规则澄清（关键）：

> "在选好两张牌后，这两张牌的先攻，玩家是**直接选择用哪个**的，
>   不一定需要因为使用哪个先攻而先选哪个牌。"
> "先攻影响的主要是出手时机，执行顺序影响的是过程和结果，
>   **是有关联的前中后，但不是完全一体**。"

代码实证：`CCharacterClass.SetInitiativeAbilityCard(CAbilityCard)` 是**显式设置**，
先攻牌与执行顺序无关。

于是枚举从 3 个维度扩成 4 个：

```
出牌组合 (i 上半位, j 下半位)   i≠j
  × 上半位候选 {卡面上半, 基础攻击 2}
  × 下半位候选 {卡面下半, 基础移动 2}
  × 执行顺序   (先做上半位 / 先做下半位)
  × 先攻来源   (由牌 i 提供 / 由牌 j 提供)      ← 新增
```

12 张手牌约 2112 组合。

**输出约定**（按用户要求）：

- **只报先攻点数**，不写"取自哪张" —— 玩家看着点数自己对。
- **出牌行按先攻升序写**，每张牌各自标出点数：

```
出牌：第3张「辅助牌」(先攻20)的【上】 + 第7张「强力牌」(先攻64)的【下】
次序：先做第7张，再做第3张（本方案先攻 20）
```

  选牌界面本来就是越上面先攻越低，玩家靠点数快速定位卡牌，所以展示要对齐这个习惯。
  典型打法：**低先攻辅助牌 + 高先攻强力牌**，先攻挑哪个由玩家按需定。

⚠ 配套修复：`Dedupe` 的 key **原来不含先攻**，两个先攻不同的方案会被误合并。
现在 `p.Initiative` 已进 key。回归 `InitiativeDecouplingTests()` 锁住三条：
两种先攻都能枚举到、每张牌先攻都记下来、存在"先攻相同但执行顺序不同"的方案。

### v1.9 修「十几个重复『角色』页签」（实战截图暴露）

症状：窗口页签栏只有第一个是「秉姜」，后面跟着**十几个一模一样的「角色」**，
内容区每个页签只显示一行。

**根因：`blocks` 的语义写错了**

```csharp
var blocks = new List<string>();                       // ❌ 扁平
blocks.AddRange(Report(...));                          // Report 返回 List<string>
```

`Report()` 返回的是**一个角色的所有行**，`AddRange` 把它们**平铺**进 blocks，
于是 `blocks` 变成"所有角色的所有行"的一维列表。而 `ToUiSections` 里

```csharp
foreach (string b in blocks)      // ❌ 遍历的是每一行，不是每个角色
```

把**每一行**都当成一个角色块 → 每行建一个 `UiSection("角色")`，
只有第一行碰巧被正则改成了「秉姜」。完全吻合截图。

**修复**：`blocks` 改成 `List<List<string>>`（每个角色一块），
`ToUiSections` / 日志页 / 报错页 / 写文件四处同步改遍历方式。

**配套**：页签名加了去重兜底 `UiSection.UniqueName(existing, preferred, fallback)`——
抽不到角色名的块不会再堆出一堆同名「角色」，撞名则加序号。
抽成 Model 层的纯函数是为了**能在离线测试台回归**（Bridge 层测不到）。
回归 `UiSectionNameTests()` 锁住：连加 5 个「角色」也不能重名。

**顺带**：窗口铺了一层不透明深色底
（`GUI.DrawTexture(Texture2D.whiteTexture)` + `GUI.color` 染色）。
默认 skin 在该主题下偏透明，会透出游戏画面，文字很吃力。

### v1.10 修窗口两个交互回归（实战反馈）

| # | 症状 | 根因 |
|---|---|---|
| 1 | 点「-」收起后，再点「+」无法展开 | `_rect.height` 被收起时的 52 覆盖 |
| 2 | 只要窗口开着就完全不能用鼠标操作游戏 | **禁用鼠标后用该设备读位置 → 死锁** |

#### 1：高度被覆盖

```csharp
_rect = GUI.Window(WindowId, drawRect, DrawWindow, _title);   // ❌ 全量回填
```

`GUI.Window` 返回实际绘制的 rect。收起时高度是 `TitleH+ButtonBarH = 52`，
回填后 `_rect.height` 永久变成 52；再点「+」时 `h = _rect.height = 52`，
只画得出标题栏+按钮栏的空壳 —— 表现就是"无法展开"。

**修复**：单独用 `_expandedHeight` 记展开高度；`GUI.Window` 的返回值
**只回填 x/y/width**，高度一律不写回 `_rect`。展开时再把用户拖动调整后的
高度记进 `_expandedHeight`（右下角可拖）。

#### 2：鼠标拦截死锁（重要）

上一版这样写：

```csharp
InputSystem.DisableDevice(Mouse.current);        // 禁用鼠标设备
...
Vector2 p = Mouse.current.position.ReadValue();  // ❌ 又用它读位置
```

设备一旦被禁用就**不再上报新坐标**，于是位置冻结在"进入窗口那一刻"，
`IsMouseInside()` 永远返回 true → 鼠标再也没被放开。
用户原话："只要求解器窗口开着，就没法使用鼠标操作游戏。"

**修复**：
- 位置改从 **IMGUI 的 `Event.current.mousePosition`** 读（legacy 通道，
  独立于 Input System，`DisableDevice` 不影响它）。
- 判定逻辑从 `Update` 挪到 `OnGUI`（`Update` 里没有 `Event`），
  `Update` 只保留"窗口不可见就放行"这一条兜底。
- 加**看门狗**：拦截期间若鼠标位置连续 ~2 秒(120 帧)完全没动，强制放行 1 帧。
  万一 Event 通道也拿不到新位置，也不会永久锁死鼠标 —— 每 2 秒给一次重定位机会。
  正常情况（玩家把鼠标停在窗口上看方案）也会触发，但放行本身无害：
  不点击就不会误操作。

**设计原则**（用户明确）：
> "我们是要鼠标移入求解器窗口以后不误操作游戏……
>   方便玩家一边看方案一边执行操作才是我们做游戏内窗口的主要原因。"

即：**只挡"鼠标压在窗口上"的那一刻，移开立刻放行**；
不是接管鼠标，更不是开着窗口就禁掉游戏操作。

### v1.11 建模完整性审查：阴影求值 + 危险地形（用户提问触发）

用户问了两件事：①「暗影步法」要落在**阴影**上，阴影是死行职业机制叠在地块上的
特殊资源，方案有没有考虑阴影的**生成与销毁**？② 方案有没有考虑**地块问题**
（困难地形要更多移动力等）？

#### 查证结论

**阴影**
- 阴影是 `CObjectShadow`（`CObjectProp` 子类），叠在格子上，**不是**光照阴影。
  官方判定 `CTile.FindPropOfType<CObjectShadow>() != null`。
  ⚠ 它是**道具**，所以**不阻挡移动**（`CAbilityCreateShadow` 里只把
  `ObjectType == Shadow` 排除在"阻挡"之外）—— 怪物能走过阴影。
- 上限 5 格；放置时忽略除墙和其他阴影外的一切；**不是负面格，敌人不会避开**。
- 死行「深渊力量」上半：
  ```yaml
  TAttack:
      Attack: 1
      StatIsBasedOnX:
          Strength: { BasedOn: Vars, AddTo: True,
                      Vars: { Value: ScenarioManager.CurrentScenarioState.ShadowProps.Count } }
  ```
  ⇒ **实际强度 = 1 + 当前阴影数**（最高 6）。

**★ 修掉的严重低估**：之前只按卡面的 `Attack: 1` 算，等于把死行最强的牌
低估 6 倍。现在用**游戏自己的求值函数** `CAbility.GetStatIsBasedOnXValue`
（**纯函数**，不会写 `HasBeenAdded` 标记、不会改 Strength）算出实际值。

⚠ 必须**反射**调用：`AbilityData.StatIsBasedOnXData` 在 `ScenarioRuleLibrary.YML`
程序集里，而本项目**有意**只引用 `ScenarioRuleLibrary` + `SharedLibrary`。

**地块**
| 机制 | 状态 |
|---|---|
| 移动代价（困难地形多耗移动力） | ✅ 准 —— 直接用 `FindCharacterPath(ignoreDifficultTerrain:false)`，游戏算 |
| 占用（敌人/队友/召唤物/障碍/门） | ✅ 准 —— 同一套寻路 |
| 视线（墙阻挡） | ✅ 准 —— `CActor.HaveLOS` |
| 射程（含门路径） | ✅ 准 —— `FindPath` 同参数 |
| **危险地形伤害** | ❌→✅ **本次补上** |
| 陷阱伤害 | ❌ 未建模（已列入「报错」页清单） |
| 冰面滑动 | ❌ 未建模 |
| 开门是独立动作 | ❌ 未建模（当成移动的一部分） |
| 压力板 / 刷怪点 / 可摧毁物件 | ❌ 未建模 |

**★ 危险地形伤害**：进入危险地形扣血（`ScenarioManager.HazardousTerrainDamage`，
默认 1），**站在上面不动不重复扣** —— 所以要按**整条路径**累加，不能只看终点。
现在 `Geometry.Reachable` 返回 `(X, Y, Hazard)`，第三项就是这条路径的伤害，
在评分里作为独立负分项（`ScoringWeights.SelfHazardWeight`，默认 1.0）。

#### 「报错」页新增固定清单

`P1Runner.UnmodeledMechanisms` 列出本版本**明确没建模**的机制
（阴影的动态生成/销毁、阴影上限 5、深渊力量下半"3 格内有阴影"的前提、
陷阱、冰面、开门、压力板、召唤物主动召唤、多目标伤害、先攻编织安全性权衡）。

原则：**明确告诉玩家哪里要自己判断，比藏着不说是诚实得多** ——
藏着会让玩家按错误的推荐去操作。

### v1.12 P5：多回合滚动推演（最后一个里程碑空白）

**结构：宽度受限的束搜索（beam search）**

```
第 1 回合：完整枚举 P1 方案 → 取 Top-K
           ↓ 执行（StateAdvancer.Apply）
第 2 回合：按新状态重建 context → 再枚举 → 取 Top-K
           ↓
第 D 回合：…
           ↓
终局价值 = 累计伤害 + 击杀×8 + 剩余血量×0.4 + 战役目标分
           − 危险地形自伤×1.5 − 死亡惩罚(200) + 存活回合数
```

**为什么用束搜索**：单回合组合数就上千，3 回合全展开是百万级。
每层只保留 K 个最有希望的分支 → 深度 D 就是 D 次枚举，**秒级完成**。

**新增文件**

| 文件 | 职责 |
|---|---|
| `Solver/WorldState.cs` | 可变世界状态（位置/血量/手牌/牌堆/敌人）+ `ToContext()` 重建枚举上下文 |
| `Solver/StateAdvancer.cs` | 执行一个方案 → 新状态；敌人回合（简化） |
| `Solver/MultiTurnSearch.cs` | 束搜索本体 + 终局价值评估 |

**状态推进器做了什么（准确的部分）**
- 我的位移 = 方案终点
- 危险地形自伤扣血
- 每个攻击：目标掉血、护盾先扣、归零则**从战场上移除**（击杀）
- 战役目标分：终点踩到的东西加分，并从可捡列表移除
- **弃掉打出的两张牌**（`TopCardIndex` / `BottomCardIndex`）
  —— 与执行顺序无关（先攻已解耦）
- **修正牌堆推进**（本回合抽了几张）

**⚠ 简化点（窗口「多回合」页首会明说）**
- **敌人回合只按意图估算**：读 `UnitSnapshot.Intent` 的 `AttackStrength/Range`
  算它能否打到我、掉多少血 —— **不模拟它真实抽修正牌**。
- **队友回合不推进**（队友位置保持不变）。
- 终局价值是**相对比较**用的，不是真实评分。

**性能**：第 1 回合的方案复用 P1Runner 已算好的结果（`r.Plans`），
省掉一次全枚举 —— 深度越大省得越多。

**配置**：`MultiTurnEnabled`（默认开）、`MultiTurnDepth`（默认 2）、`MultiTurnBeam`（默认 3）。

**回归 `MultiTurnTests()`** 锁住：深度对应回合数、回合编号、位置真的动了、
累计伤害 > 0、手牌递减 2 张、血量合理、深度 1 退化、单张手牌不崩。

### v1.13 P5 精确化：敌人回合接上**真实修正牌抽牌**

上一版 P5 的敌人回合是"按意图估算"（伤害 = 意图上的 AttackStrength 固定值）。
本版改为**真实抽牌** —— 依据 MODIFIER.md 的实证结论：

> 修正牌堆是**确定序列**，`CActor.DrawAttackModifierCard` 直接取 `current[0]`，
> 零 RNG 消耗，实测 **354/354 全中**。

所以能算出敌人**确切会翻出哪张牌**，而不是期望值。

```
旧：伤害 = 意图.AttackStrength                       （固定值）
新：伤害 = DamageModel.ComputePreShield(意图.AttackStrength, 真实抽到的牌)
       可能 +2、可能 ×2、也可能 ×0 完全免伤
```

#### 数据通路（新增）

| 位置 | 内容 |
|---|---|
| `P1Context.MonsterDecks` | 怪物四副**共享**牌堆（0=敌 1=友 2=敌2 3=中立） |
| `UnitSnapshot.DeckIndex` | 该单位用哪副；`DeckIndexForFaction(FactionKind)` 映射 |
| `WorldState.EnemyDecks` | 束搜索各分支**深拷贝**，互不污染 |
| `Model/EnemyStrike.cs` | 一次打击的记录：谁、翻了什么牌、打多少、是否退化估算 |
| `RouteStep.EnemyStrikes` | 每回合挨了哪些打，供窗口展示 |

⚠ **同阵营敌人共用一副牌堆**是游戏规则，不是我的简化 ——
多个敌人一回合内的抽牌必须按**先攻升序**依次消耗同一副，顺序错了牌就翻错。
`RunEnemyTurn` 里先排序再逐个结算，就是为这个。

#### ★ 顺带修掉一个真 bug

`MultiTurnSearch.ApplyAndEnemy` **只在第一层之后被调用** ——
深度 ≥3 时，第 2 回合之后的敌人**完全没有动手**，等于白推。
现改为每一层都跑敌人回合（`MakeStep` + `RunEnemy` 统一入口），
顺带删掉了 `ApplyAndEnemy` 里的一次**重复 `Apply`**（它把第一个方案又执行了一遍）。

#### 仍保留的简化（窗口「多回合」页首会明说）

- **不模拟敌人寻路**：用 `攻击射程 + 移动力` 取上界粗判它够不够得着
  （怪物是先移动再攻击，所以取上界是对的，但**可能高估**挨打风险）
- **不模拟敌人对我方的优/劣势**
- **队友回合不推进**
- 只结算"打我"的那一下；打队友时只消耗牌、不计我掉血

#### 回归 `EnemyRealDrawTests()` 锁住 19 条

×0 完全免伤 / +1 加成 / 牌堆真的被消耗 / **同阵营两敌人依次抽同一副的第一、第二张** /
牌堆见底退化成估算并标记 / 先攻高于我不打我 / ×2 翻倍（乘算在括号外）。

**输出示例**（「多回合」页，可直接拿去跟游戏里实际翻出的牌对照）：

```
    第1回合：猛击【上】+走位【下】 → 站(14,4) 血17 累计伤5 杀0 挨打-3
       挨打：「冰角守卫#4」先攻10 翻 +1 → 对你 3 点
```

### v1.14 P5 精确化：敌人回合接上**真实寻路**

上一版的够不着判定是 `dist <= AttackRange + MoveStrength` —— 纯几何，**会高估**：
隔着一堵墙、或者压根没有路，也会被算成"它能打到你"，于是玩家被吓得不敢站。

本版改为**真的去寻路**，看是否存在一个可达格能打到目标。

```
旧：够不着判定 = dist <= 射程 + 移动力            （几何上界，会高估）
新：EnemyPathing.TryPlanMove → 遍历可达格，
    找一个「能打到焦点 且 移动代价最小」的落点；找不到就是打不到
```

考虑了三件事：**地形**（困难地形多耗移动力，寻路里已含）、
**占用**（别的单位挡路，它进不去）、**视线**（墙后面打不到）。

#### 为什么另写一个 `Solver/EnemyPathing.cs`

`Bridge/EnemyPredictor`（P4）依赖 `CScenario` / `CActor` / `Geometry` 等游戏类型，
而 `StateAdvancer` 是**纯计算层**（不引用游戏类型，测试台才能直接编译它）。
所以复刻同一套规则，输入改成**委托** —— `P1Context` 本来就有
`Reachable` / `RangeDistance` / `HasLOS`，现在 `WorldState` 也带上了。

复刻的选位规则（CAIMoveNew，反编译实证，无随机）：
```
① 焦点 = 敌对单位中「能打到它所需的移动代价」最小者
② 落点 = 可达格里「能打到焦点」且「移动代价最小」的格子
③ 现在站的格子就能打到焦点 → 它不动（代价 0 已最小）
```

#### ★ 修掉一个焦点 tie-break 的真 bug

游戏 `CompareEnemies` 的顺序是
`lowestMoveCostToAttack 升序 → hexDist 升序 → 先攻`。

我第一版把 tie-break 写成了"落点到目标的距离"，**比错了维度** ——
应该在**移动代价相同**时比的是**敌人自己到目标的距离**（hexDist），不是落点到目标的。

后果很实际：敌人站你和你队友中间、两个都不用移动就能打到时，
它会去打**更近的那个**；比错维度就可能选成打你，让你误判要不要躲。
回归用例 ⑤ 抓到了这条（队友在 (14,4)、我在 (14,3)、敌人在 (14,5) → 应该打队友）。

#### 配套行为变化

- **打不到任何人 → 它原地不动，也不抽牌**（真实规则：没出手就不消耗修正牌）
- **打队友时只消耗牌、不掉我血**（它确实出手了，牌堆要推进）
- 先动的敌人占住新格后，后动的敌人**不会踩上去**（占用集随移动动态更新）
- `EnemyStrike` 新增 `Moved` / `FromX` / `FromY`，输出会写"走到 (x,y) 后"

#### 回归 `EnemyPathingTests()` 新增 14 条

没有路 → 打不到（旧版会误判）/ 没视线 → 打不到（旧版会误判）/
有路 → 移动后打到你 / 原地能打到 → 不动 /
**焦点是更近的队友** / 两个敌人不会站同一格。

**输出示例**：

```
    第1回合：猛击【上】+走位【下】 → 站(14,4) 血17 累计伤5 杀0 挨打-3
       挨打：「冰角守卫#4」先攻10（走到(14,4)后） 翻 +1 → 对你 3 点
```

### v1.15 纠正：够不着时敌人**会逼近**，不是原地不动

用户要求"调查清楚真实的敌人 AI，不盲目确信打不到人就一定会原地不动" ——
**查证结果：我上一版的假设是错的。**

#### 反编译证据链

`CAIMoveNew.Process()`（1975 行）：

```csharp
CalculateValidAttackTilesForAllEnemies();
SortEnemyEvals();
if (enemyEvals.Count != 0)
{
    var source = enemyEvals.Where(x => x.lowestCostAttackTile != null);
    finalFocusEval = source.FirstOrDefault();
    if (finalFocusEval?.lowestCostAttackTile != null)
    {
        FindBestAttackPositionForTheFocus();
        FindPathToFocus();     // ← 关键在这里
        CalcWaypoints();
        ...
    }
}
```

⚠ `CalculateValidAttackTiles` 选 `lowestCostAttackTile` 时**不检查 reachable** ——
只要求该格在射程内、有视线、可站立（`Walkable && !CannotStop`）。
所以"有攻击位"几乎总成立（目标周围基本都有格子），`finalFocusEval` 很少为 null。

真正的 fallback 在 `FindPathToFocus()`：

```csharp
if (finalFocusEval.bestAttackTile == null || maxMoveCount == 0)
{
    actor.AIMoveFocusPath.Clear();
    return;                                   // ← 只有这两种情况才完全不动
}
pfContext.GetPathTo(bestAttackTile.position, actor.AIMoveFocusPath, maxMoveCount);
if (IsCurrentPathPerfect()) return;           // 走到了攻击位 → 完成

// 走不到：在**所有可达格**里挑一个"离攻击位最近"的走过去
int num = SortAttackTilesAndFindNumberOfBests();
InitReachableTiles();                          // 排除自己脚下
actor.AIMoveFocusPath.Clear();
foreach (TReachableTileEval reachableTile in reachableTiles)
{
    ...    // 比较到攻击位的剩余代价，取最优
    actor.AIMoveFocusPath.AddRange(reachableTile.path);
}
```

`IsCurrentPathPerfect` 判定"不完美"的条件之一：
终点不是攻击位 **且** 还有剩余移动力（`CostWithoutTraps(cost.movePoints) < maxMoveCount`）
→ 继续走贴近逻辑，把移动力用掉。

**结论**：这是 Gloomhaven/Frosthaven 的标准规则 ——
**够不着也会尽量靠近焦点**。只有这两种情况才真的不动：
① `bestAttackTile == null`（连"理论上的攻击位"都没有，例如目标被墙封死）
② `maxMoveCount == 0`（本回合移动力为 0）

#### 我的错误偏差方向：低估

旧实现"打不到 → 原地不动、不抽牌"，会让你**误以为它是安全的**，
实际上它已经挪到你旁边了，下回合就能打到你。这是**低估风险**，比高估更危险。

#### 修正

`EnemyPathing` 新增 `EnemyMovePlan` 三态：`Attack` / `Approach` / `None`。
够不着时走到"离焦点最近的可达格"（要求**必须比现在更近**才动，避免无意义抖动），
但这回合**不出手、也不抽修正牌**（真实规则：没出手就不消耗牌）。

`EnemyStrike` 新增 `ApproachOnly`，输出会明确提示：

```
「冰角守卫#10」先攻10 够不着你 → 逼近到(13,7)，这回合不会出手（下回合小心）
```

实测：距离 6、射程 1、移动 2 → 逼近到距离 4，牌堆**未消耗**。

#### 保留的近似

贴近目标是"离**焦点本人**最近的可达格"，真实代码是"离**最佳攻击位**最近"
（攻击位本来就在焦点旁边，绝大多数场景等价，且更好解释）。

### v1.16 实战截图抓出的严重 bug：两张牌的先攻被**对调**

用户发来两张截图——一张游戏内 ACTIVE 卡列表、一张求解器窗口：

| 来源 | 刚毅不屈 | 义无反顾 |
|---|---|---|
| 游戏 ACTIVE 列表 | **31** | **20** |
| 求解器显示 | 20 ✗ | 31 ✗ |

而且没按先攻升序排（因为先攻值本身就错了）。

#### 根因：v1.8 解耦留下的坑

`TopCardInitiative` / `BottomCardInitiative` 是按**上下半位**存的
（前者 = 上半位那张牌 `Hand[i]` 的先攻，后者 = 下半位 `Hand[j]` 的）。

但展示层却用**执行顺序** `h1`（先执行）/`h2` 去取：

```csharp
(h1.CardIndex + 1, ..., p.TopCardInitiative),     // ❌ h1 未必是上半位
(h2.CardIndex + 1, ..., p.BottomCardInitiative),  // ❌ h2 未必是下半位
```

这两个维度在 v1.8 刚解耦，先执行下半位时就会把两张牌的先攻**对调**。

**日志佐证**（决定性）——同一组牌（日蚀 + 暗影步法）在不同方案里先攻完全对调：

```
出牌：第8张「暗影步法」(先攻19) + 第6张「日蚀」(先攻86)
出牌：第6张「日蚀」(先攻19)    + 第8张「暗影步法」(先攻86)
```

两张牌不可能都是 19 和 86 —— 直接坐实是按执行顺序错取。

**修复**：加 `TurnPlan.InitiativeOf(cardIndex)`（按 CardIndex 匹配），
展示层改用它；排序加 `ThenBy(c => c.idx)` 保证同先攻时稳定。

**回归 `InitiativeLabelTests()`**（复现截图场景）：
先攻跟着卡走、`InitiativeOf` 按 CardIndex 取对、
**先执行下半位时不被对调**（实测 10 个这类方案，标注错误 0）、
ToOneLine 里先攻低的排前面。

#### 教训

解耦**两个维度**时，凡是旧代码按其中一个维度去索引另一个维度的字段，
全都要复查。这类 bug 编译无错、离线断言也发现不了，只能靠实战截图。

### v1.16 阴影机制建模（死行可用性的前提）

日志实测未建模能力分布：

```
Heal 401 · MoveActiveBonusTracker 309 · MoveShadow 181 · CreateShadow 146
PerformFromShadow 145 · ControlActor 114 · MoveShadow/Damage/RemoveShadow/GainXP 91
Teleport2 83 · Poison 63 · Pull 16 · Strengthen 6
```

阴影相关合计 **≈563 次** —— 死行有一半的卡求解器"看不懂"，方案基本没法用。

**反编译实证的四类**（`CAbility*.cs`）：

| 类型 | 类 | 关键字段 |
|---|---|---|
| 造阴影 | `CAbilityCreateShadow` | Strength = 个数；**上限 5**（`num + 选中 - 待移除 <= 5`） |
| 移阴影 | `CAbilityMoveShadow` | `FinalShadowPositions` |
| 除阴影 | `CAbilityRemoveShadow` | `RemovedShadows` |
| 从阴影发动 | `CAbilityPerformFromShadow` | `m_ShadowsToUse` / `m_RemoveWhenDone` |

阴影是道具 `CObjectShadow`（不是光照阴影），**不阻挡移动**，上限 5。

**实现**
- `ActionSnapshot` 加 6 个阴影字段 + `IsShadowAction`
- `HandReader.ClassifyShadowAbility()` 把它们从 `UnmodeledTypes` 里**移出**
- `Geometry.ShadowTiles()` / `ShadowCount()` 读 `ScenarioState.ShadowProps`
- `P1Context.ShadowCount` + `ScoringWeights.ShadowCreateWeight`（默认 2.0）
  —— 造阴影会抬高「深渊力量」（攻击 = 1 + 阴影数），所以它值分
- `P1Planner.ShadowText()` 诊断：造几个、当前 n/5、**满了会警告**、
  从阴影发动**不够会警告**

**回归 `ShadowMechanismTests()` 8 条**：造阴影文本 / 上限警告 /
从阴影发动不够警告 / 够时不警告 / 普通攻击无阴影文本 /
造阴影方案记 ShadowGain / 满 5 后不再记。

⚠ **已知限制**（已写进「报错」页）：
纯"造/移阴影"且**不带移动**的半边枚举不到（它不产生位移也不攻击）；
带移动的版本正常（如「日蚀」下半 = 移动 + 造阴影）。

### v1.16 日志核查与死代码清理

**日志**（`BepInEx/LogOutput.log` 5754 行 + `Player.log` 32973 行）：
- 插件**零错误、零异常**（Player.log 里的 NullReferenceException 是游戏自身的）
- 心跳行给出**强证据**：`预测 命中18/18 不符0`、`对拍 伤害对拍 14中/0不符`
  → P1 的修正牌预测与伤害对拍在实战中都是全中

**静默删除排查**：逐个核对 16 个关键实现点（未建模提示 / 阵型 /
落脚限制 / StatIsBasedOnX / 危险地形 / 阴影 / 先攻 / 页签去重…）**全部存活**。
另扫出"定义了从未调用"的方法，清掉真死代码
`SolverUI.Toggle()` + `HasContent`（Insert 开关实际在 TickPump 里实现，
这里曾有两套重复实现）。

**攻略补读**：剩余 16 份全部提取（93.8 万字符，`build/guides/`）。
新增一条建模修正——阵型蓝格**不限于角色**，旗帜、召唤物都算
（旗矛 Resolved Courage 攻略原文："another character, one of your banners,
or another summon"）。

### v1.17 阴影门槛：0 阴影时不能推荐有阴影限制的牌

用户实战反馈：**"怎么上来就又使用有阴影限制的牌？此使命开局并没有任何阴影。"**

这是两个叠加的 bug，都会导致推荐出**根本用不了**的牌。

#### bug 1：原地不动绕过了落脚限制

```csharp
// dests() 里：可达格按 MustEndOnShadow 过滤了，但……
if (!list.Contains((x, y, 0))) list.Insert(0, (x, y, 0));   // ❌ 无条件插入原地
```

开局 0 阴影时可达格全被过滤掉 → `list` 为空 → 但**原地被无条件塞回来** →
方案照样生成，落点就是当前站的那格（没有阴影）→ **非法方案**。

修复：原地也要过 `IsShadowTile(x, y)` 校验。

#### bug 2：压根没有"半边可用性"判定

之前只要半边是 Action 就进候选，从不检查它的**前提**是否满足。

新增 `ShadowRequirementMet(half, ctx)`：

| 前提 | 不满足的条件 |
|---|---|
| `MustEndOnShadow` | 当前格不是阴影 **且** 场上阴影数为 0（没地方可落脚） |
| `PerformFromShadow` | `ctx.ShadowCount < m_ShadowsToUse` |

⚠ **造阴影类半边不受此限** —— 它正是"没有阴影时"的解法，不该被自己挡住。

`UpperOptions` / `LowerOptions` 据此过滤候选；诊断里明确写出原因：

```
「暗影步法」上 → ⚠ **本回合无法使用**：移动后必须停在**阴影格**，
                但场上当前一个阴影都没有（0/5）
                （这张牌若要做事，只能当基础攻击 2 / 基础移动 2 用）
```

#### 配套：报告新增【阴影】段

开局是不是 0 个直接决定哪些牌能用，所以必须显示出来**对账**：

```
【阴影】场上当前 0/5 个
  ⚠ 一个阴影都没有 → 「暗影步法」这类**必须停在阴影**的牌本回合用不了，
     「从阴影中发动」类也用不了。先用能造阴影的牌铺开。
```

有阴影时会列出坐标，并提示「深渊力量」按 `1+阴影数` 算。
配置项 `ShadowDiagnostics`（默认开）。

#### 数据读取验证（`Geometry.ShadowCount`）

反编译核对：
- `ScenarioState.ShadowProps` = `GetPropsThreadSafe.Where(ObjectType == Shadow)`
- `CObjectProp.ArrayIndex` 是 `TileIndex`（有 `.X` / `.Y`）
- 游戏自己也是 `CurrentScenarioState.ShadowProps` + `shadowProp.PropTile`

⚠ 排除了 `WillBeRemoved` 的阴影（本回合将被移除的不算）。

#### 回归 `ShadowGatingTests()` 5 条

0 阴影时不推荐暗影步法上半 / 诊断说明原因 / 说明"0/5" /
有阴影时**可以**推荐（避免矫枉过正）/ 旧用例补 `ctx.ShadowCount` 保持一致。

### v1.18 状态机制建模 + 反射 bug 修复（实战反馈）

用户四条反馈：① 无阴影回合仍推荐阴影牌；② 造阴影没写位置；③ 没考虑敌人先手上缴械；
⑤ 要求建模状态机制（缴械/中毒/劣势/流血/优势/静足…）。

#### ★ 修掉一个让阴影过滤完全失效的反射 bug

```csharp
// ❌ 原来这样写
private static readonly PropertyInfo PMoveRestriction =
    typeof(CAbility).GetProperty("MoveRestrictionType", ...);   // ← 拿到 null
```

**`MoveRestrictionType` 只定义在 `CAbilityMove` 上**（`CAbilityMove.cs:292`），
`CAbility` 基类没有这个属性 → 静态反射永远返回 null →
`ReadMoveRestriction()` 永远返回 `"None"` → **所有落脚限制形同虚设**
（v1.17 加的阴影门槛也因此不起作用）。

修复：按**运行时实际类型**反射 —— `ab.GetType().GetProperty(...)`。

**教训（第二次同类）**：反射读不到时**静默返回默认值**是最危险的失败模式 ——
编译通过、离线测试全绿、部署成功，但实战完全失效。
凡"读不到就 return 默认值"的地方，都应该考虑**记日志**。

#### 阴影前提（v1.17 埋的坑补完）

`ShadowRequirementMet(half, ctx)`：
· `MustEndOnShadow` 且当前格不是阴影 **且** 场上 0 个阴影 → 半边不可用
· `PerformFromShadow` 且 `ShadowCount < m_ShadowsToUse` → 半边不可用
⚠ 造阴影类不受限（它正是没阴影时的解法）。

配套修了 `dests()` 里"原地不动无条件插入"绕过过滤的问题。

#### 状态机制（本次新增）

`StateRequirementMet(half, ctx)` —— 依据 `CCondition.ENegativeCondition`：

| 条件 | 效果 |
|---|---|
| **Disarm** 缴械 | 攻击类半边用不了（**基础攻击也算** —— 它也是 attack action） |
| **Immobilize** 静足 | 移动类半边用不了（含基础移动） |
| **Stun** 眩晕 | 这半边整个用不了 |
| Disadvantage / Advantage | 已由官方 `GetAdvantageStatus` 覆盖（ModifierDraw 抽 2 取坏/好） |
| Strengthen / Bless / Ward / Regenerate | 只提示，未计入评分 |

数据通路：`CTokens.m_Tokens`（私有）反射 → 逐个 `HasKey(token)`。
`UnitSnapshot.NegativeConditions` / `PositiveConditions` → `P1Context.MyNegativeConditions`。

**报告新增【我方状态】段**，每个条件给出对应行动建议，例如：

```
【我方状态】Disarm
  ⚠ **缴械** —— 攻击类动作用不了；改用移动/治疗/辅助，或直接打基础移动 2
```

#### 造阴影写明位置（用户第 2 点）

之前只写"造 N 个阴影"，玩家得从第 2 步落点反推。现在明确说：

```
  ★ 本步会造 3 个阴影 —— **位置由你自己选**，当前场上 0/5。
     选址建议：放在你**下一步要站的格子**或**下一步要打到的敌人旁**
     （「暗影步法」这类牌必须先踩在阴影上才能用）。
```

#### 回归 `ConditionGatingTests()` 5 条

缴械后无任何攻击动作（含基础攻击）/ 诊断说明原因 / 静足后无位移 /
眩晕后无方案 / **无状态时正常推荐攻击**（避免矫枉过正）。

### v1.19 疑点系统排查 + 反射静态校验

#### REA 实测结论：对 .NET Mono 后端不适用（有证据）

用 MCP 接入的 REA 打开 `Assembly-CSharp.dll`，REA 自己判定：

```json
{"format":"pe","architecture":"x86",
 "provider_id":"ghidra","code":"managed_target_unsupported",
 "reason":"Windows Ghidra P0 accepts native PE applications and DLLs;
           managed or unclassified PE targets are unsupported.",
 "diagnostics":{"executable_role":"shared-library","managed":true}}
```

四个 provider 全部拒绝：

| provider | 拒绝原因 |
|---|---|
| ghidra | `managed_target_unsupported`（托管 PE 不支持）+ `not_configured` |
| hopper | `unsupported_host`（win32 不支持） |
| ida | `not_configured`（要 `REA_IDA_MCP_CONFIG`） |
| kuna | `target_format_unsupported`（只认 ELF / thin Mach-O） |

`"managed": true` 说明 REA 能识别这是 .NET 程序集，但它的反编译引擎
（Ghidra/Hopper/IDA/Kuna）**全部只做 native 代码**。
本项目是 Unity **Mono** 后端，业务逻辑全在托管 DLL 里 → REA 无从下手。
唯一 native 文件 `UnityPlayer.dll`（31MB）是引擎本体，不含业务逻辑。
已 `close_binary` 释放。

⇒ 继续用既有 C# 反编译产物（`decompiled/SRL` 等）作为规则真相来源。

#### 疑点排查结果

**① IgnoreLOS —— 查清并建模**

反编译实证（`SharedAbilityTargeting.GetValidActorsInRange`）：

```csharp
GameState.GetTilesInRange(..., ignoreLOS: ability.MiscAbilityData?.IgnoreLOS ?? false, ...)
```

即**默认要视线**，只有个别能力显式关掉。社区攻略说的
"非目标型攻击不用视线"指的就是这类卡。
之前我按"所有攻击都要视线"建模 → **低估**这类卡（漏掉墙后合法目标）。
已加 `ActionSnapshot.IgnoreLOS` + `HandReader.ReadIgnoreLOS`。

顺带澄清：`EAbilityTargeting.None` **只是标记**（"无目标类型"），
不是 LOS 豁免 —— 这个疑点可以关掉了。

**② 隐形目标 —— 查清并建模**

反编译实证（`CAbilityFilter.IsValidTarget`）：

```csharp
if ((!AreActorsAllied(...) & isTargetedAbility)
    && target.Tokens.HasKey(EPositiveCondition.Invisible)
    && flag != true)   // flag = GameState.CanTargetInvisible(self, canTargetInvisible)
    → 不是合法目标
```

即**默认不能打隐形单位**，只有 `MiscAbilityData.CanTargetInvisible` 的能力例外。
之前不排除 → **高估**（会把打不到的隐形敌人算成目标）。
已加 `ActionSnapshot.CanTargetInvisible` + 目标筛选排除。

**③ 仍不建模的（如实列入「报错」页）**

| 机制 | 规模 | 不建模的原因 |
|---|---|---|
| 元素（Element） | 27 个能力文件涉及，完整子系统 | 跨回合资源推演，复杂度高，价值待评估 |
| 陷阱伤害 / 冰面滑动 | `CObjectTrap.GetTrapDamage()` 已有接口 | 地块效果，需逐个算路径触发 |
| 召唤物主动召唤 | — | 需要建模召唤物自身行动 |

#### 新增 `tools/verify_reflection.py`（构建期静态校验）

针对 v1.17/v1.18 两次"静默失败"（脚本忘写回、反射打错类）——

这类失败编译通过、离线测试全绿、部署成功，但实战完全失效。

脚本做两件事：
1. **正向**：把 `EXPECTED` 清单里 23 个反射目标拿去反编译产物核对，缺一个就报错
2. **反向**：扫描 `src/` 里所有 `GetProperty/GetField/GetMethod("X")`，
   未登记到清单的**一律报错** —— 新增反射调用必须同步登记

已接进 `tools/build.sh`，构建时自动跑。

清单登记时又抓出一个错：`Behaviours` / `Duration` 原以为在 `CActiveBonus` 上，
实际在 `ActiveBonusData` 上（代码里用 `GetField` 取的是 ActiveBonusData）。

#### 回归新增 4 条

`IgnoreLosTests()`（普通攻击被墙挡 / IgnoreLOS 能穿墙）+
`InvisibleTargetTests()`（普通攻击打不到隐形 / CanTargetInvisible 可以）。

### v1.30 修三处：移动点 / 战报落盘 / 回合页归属

用户实机反馈：① 敌人攻击力对了但**移动点变 0**；② 多回合详细方案**只显示几行**且
**错误变成独立分页**（应该是"对应分页中的下一页"）；③ compare_report 实际侧 0 条。

#### ① 移动点变 0

`CMonsterClass` 的属性全来自 `m_CurrentMonsterStat`：
`Attack` / `Range` / `Target` / **`Move`** —— 都不在能力上。
v1.28 只补了 Attack/Range/Target，漏了 Move → 意图显示"移动0"。

已补 `MoveStrength`（`mc.Move`）。另外射程为 0 时**兜底成 1**：
射程 0 = 敌人打不到任何格子 → 生存维度整个失效（v1.28 的"射程0"就是这么毁掉它的）。

#### ② 战报实际侧 0 条

`CombatReport.Flush()` 只在按 Insert 时调用 → 玩家打完不再按 Insert，
**实际战报还留在内存里没落盘**。

改法：
- `Flush()` 改成**增量写**（只写 `_written` 之后的行），不再 `Rows.Clear()`
- 每次攻击后（`DamageVerifyPatch.Postfix`）**立刻 Flush**

#### ③ 回合页归属错了

原来 `BuildRoundSections` 把**所有角色**的回合页都加在最后，
页名是"第2回合方案·旗矛" → 变成独立页签，顺序也乱。

改成 `BuildRoundSectionsFor(roleName, ...)`：在**角色页循环内部**生成，
页名用**同一个角色名** → `UiSection.UniqueName` 自动加序号：

```
旗矛  →  旗矛1（第2回合） →  旗矛2（第3回合） →  漂泊者  →  漂泊者1 …
```

玩家点"下一页"就是这个角色的下一回合方案。

#### ★★ 工具链教训（本轮连续踩坑，最终根治）

`tools/patch.py` 经历了三次事故：
1. **stdin 模式 expand 把字面 `\n` 变成真换行** → C# 源码里字符串断行 → 编译失败
2. 目标文件 CRLF、调用方 LF → count=0
3. 修 2 时"写回时还原 CRLF"更糟 → expand 产生的 LF 混进 CRLF → **混合换行**

**最终版 patch.py**：
- **只提供文件模式**（`--old`/`--new` 文件），内容字面、真实换行
- **完全不做 expand**（源码里字面的 `\n` 原样保留）
- 统一 LF 比较与写回（不再还原 CRLF，避免混合换行）
- 原子写 + **先断言后写**（count≠1 时一个字节都不写）

用法：`python3 tools/patch.py <目标> --old build/_old.txt --new build/_new.txt`

### v1.29 原版战斗战报（用户提问"不是还有原版游戏的战斗日志可以读吗"）

**说得对** —— 游戏自己就记了权威战报：
- `CAttackSummary.TargetSummary` —— 每次攻击对每个目标的结算：
  护盾 / 穿透 / 实际承受伤害 / 中毒 / 脆弱 / 守护 / 免伤 / 伤害来源
- `GameState.ActorBeenDamaged` —— 实际扣血的那一刻

**之前只用了它的一小块**（伤害对拍：预测伤害 vs 实际护盾前伤害，56/56 全中），
没把它当成**可落盘、可比对的战报**。

#### 实现

新增 `Bridge/CombatReport.cs` —— 结构化 TSV（`D:/1Mods/FH/combat_report.tsv`）：

```
类型(实际/预测)  回合  攻击者  目标  基础攻击  修正牌  伤害  备注
```

- **实际侧**：`DamageVerifyPatch.Postfix`（已有钩子）→ 攻击者/基础攻击/修正牌/护盾前伤害/目标(含护盾与穿透)
- **预测侧**：`P1Runner` 生成方案时 → 每次攻击的目标/伤害/翻的牌/是否击杀/表达式
- 每回合 `Flush()` 落盘（追加），回合号分段

新增 `tools/compare_report.py` —— 逐条配对比对，输出：
伤害不一致 / 修正牌不一致 / 基础攻击不一致 / 目标不一致 / 回合错位，按类型归纳。

**★ 价值**：玩家若**照着推荐打**，两条序列应一一对应；
对不上的地方 = **模型误差的精确位置**。
这把"对账"从"我猜的预期"升级成"原版权威数据"。

#### 踩坑：heredoc 结束标记必须独占一行

`patch.py` 的 spec 通过 heredoc 传入时，若结束标记紧跟内容（同一行），
shell 不认 → **把后面的 shell 命令一起写进了源文件**（P1Runner.cs 被污染）。
用 `git checkout` 恢复（v1.28 已提交，安全）。
规则：`old` 与 `|||` 与 `new` 之间，结束标记 `XEOF` 必须**独占一行**。

### v1.28 差解分析与修正 + 多回合详细方案 + 翻页

用户实机通关一次使命，反馈三件事：
① 方案"在通关前提下几乎是最差解"；② 多回合只有一句话；③ 要翻页。

#### ① "几乎是最差解"的两个根因

**根因 A：怪物的射程/目标数读不到 → 生存维度整个失效**

日志里敌人意图是 `攻击1(射程0,目标0)` —— 射程 0 意味着**敌人打不到任何格子**
→ 威胁格全空 → 没有"会挨打"提示 → 推荐完全不考虑躲避。

反编译实证（`CMonsterClass.cs:233`）—— 怪物的攻击是**运行时**造的：

```csharp
cAction.AddAbility(CAbilityAttack.CreateDefaultAttack(
    m_CurrentMonsterStat.Attack,   // 攻击力
    m_CurrentMonsterStat.Range,    // 射程 —— 能力上是 0！
    m_CurrentMonsterStat.Target,   // 目标数 —— 能力上是 0！
    isMonster: true));
```

⇒ 射程与目标数**不在能力上**，要用 `MonsterClass.Range / .Target / .Attack` 补全。
已修（`StateReader`）。

**根因 B：评分顺序把"少挨打"排在"推进"之前 → 推荐苟着打**

原词典序：不会死 > **挨打少** > 击杀多 > 有效伤害多
→ "打 3 点不挨打" 赢过 "打 10 点但挨 2 刀" → 能赢但回合数爆炸。

新顺序（v1.28）：**不会死 > 击杀多 > 有效伤害多 > 挨打少 > 治疗 > 线性分**
（推进度优先，"能通关"的前提下推进比少挨打值钱）

并新增**效率提示**：`★ 清场预计还要 N 回合（剩余敌人 X 血）`
—— 回合数是最直观的指标，直接摆在方案描述里。

#### ② 多回合详细方案 + 翻页

原来第 2 回合只有一行：`第2回合：X【下】+Y【上】 → 站(a,b) 血10 累计伤5 杀1`

现在：
- `MultiTurnDetailedLines(RouteStep)` —— 展开成完整方案：
  出牌 → 落点/血量 → **每次攻击**（目标/伤害/翻的牌/是否击杀/表达式）
  → 治疗 → 危险地形 → 挨打 → 累计 → 敌人这一回合的真实行动
- `BuildRoundSections()` —— **一回合一个页签**（`第2回合方案·旗矛`），
  页名冲突由 `UiSection.UniqueName` 处理
- 角色页里的简述改成指向新页签

#### ③ 对账预期被覆盖的修法

玩家在出牌前会反复按 Insert —— 每按一次就覆盖预期的话，
预期会变成"最近一次按下时的状态"，执行后就没得可对账。
现在**只有回合号变了才更新预期**（`TurnLedger.LastRound`）。

### v1.27 【原生驱动 + 对账重规划】（学 StS2 第 8 节）

StS2 的 `PlayerTurnSetupCoordinator`：**原生 UI + 反射驱动 + 持续对账**。

#### ① 对账重规划（本轮完成）

FH 没有"自动重规划"的需求（我本来就每次按 Insert 重算），
所以对账的价值是**指出模型在哪一点上不准** —— 否则玩家只会觉得"这插件不靠谱"，
而不知道该改哪一块。

新增 `Bridge/TurnLedger.cs`：
- **状态指纹**（两个 u64，FNV-1a）：位置/血量/数量 + **修正牌堆顺序** + 手牌
  - ★ 必须含修正牌堆顺序：FH 的修正牌是**确定序列**，顺序变了后面翻出的牌就不同
- **战况对账**：上回合预期的敌人血量 / 我方血量 vs 这一回合的实际
- 报告新增【对账】段：

```
【对账】与上一回合的预测比对
  ⚠ 上回合预测与实际有 2 处不符：
    腐鼠侦察兵#3 预期剩 5 → 实际 1
  我方血量：预期 10 → 实际 6（比预期多挨了 4 点）
```

#### ② 原生驱动（**本轮只做只读探测**）

查清了 FH 的驱动链路：

```
GameState.CurrentAction          → 当前该出的牌（CurrentActorAction：CAction + CBaseCard）
PhaseManager.CurrentPhaseAction  → 玩家已选半边列表（m_PhaseAbilities，私有）
    每个 CPhaseAbility.m_Ability  ←←← 目标写在这个 ability 实例上
        m_ActorsToTarget / m_TargetedActor（protected → 必须反射）
```

⚠ **FH 没有 StS2 那种 `SetupPlayerTurn` 单一入口**，
"确认出牌"的程序化入口**还没找到** → 目前驱动的上限是**预选目标**，最后一步仍需玩家点。

因此本轮**只做只读探测**（`Bridge/NativeDriver.Probe`），**不改任何游戏状态**，
报告里新增【原生驱动探测】段：

```
【原生驱动探测】只读诊断，未改动任何游戏状态
  当前牌：XXX（2 个半边）
  可预选的半边：2 个
  目标字段可写：m_ActorsToTarget / m_TargetedActor ✓
  当前可选动作：Attack:...,Move:...
  ⚠ 尚未找到「确认出牌」的程序化入口
```

**为什么先探测不直接写**：没验证的写入不该上（v1.26.2 的文件事故 +
v1.18 的反射打错类，都是"没验证就上"的教训）。
拿到真机诊断后，再决定开自动写入。

类型修正备忘：`CurrentActorAction` 嵌套在 `GameState`，
`CPhaseAbility` 嵌套在 `CPhaseAction`，`m_PhaseAbilities` 私有
（公开只读的是 `RemainingPhaseAbilities`）。

### v1.26 【迭代 1/】多样性分桶（学 StS2 的 SearchRouteTraits 泳道）

按"对后续的影响程度"逐步迭代，本轮做**第 1 项：多样性分桶**。

#### 问题

只按排序取前 K 个 → **前几名往往是同一路子的变体**（同样的牌、同样的落点、
同样打谁），用户看到的 Top-5 就像"只给了一个方案"。

实测（离线测试）：纯 top-K 的**前 3 名只有 1 种打法**。

#### 实现

每个方案算三个签名：

| 签名 | 内容 |
|---|---|
| `OffenseSignature` | 这一轮打的是哪些目标（GUID 排序拼接） |
| `PositionSignature` | 最终站在哪 |
| `UtilitySignature` | 治疗/阴影/拾取/挨打（展示用，**不参与限流**） |

筛选（`SelectDiverse`）：
1. 遍历排序后的方案，**进攻**与**站位**两个维度**各自**限流（每桶最多 `perBucket` 个）
2. 超出桶容量的先存进 overflow
3. 剩余名额按排序补齐（保证方案数量不变、**第 1 名永远是最优**）

⚠ **踩坑**：一开始把三个签名**合起来**判桶（"所有签名都不满才算"），
而 `UtilitySignature` 对大多数方案都一样（`"util:"`）→ 第 3 个方案就全被挡进
overflow，补齐后等于**没分桶**。改成**每维度独立限流**才生效。

#### 效果（离线实测）

| | 纯 top-K | 分桶后 |
|---|---|---|
| 前 3 名的打法种类 | **1 种** | **3 种** |
| 整体打法种类 | 2 种 | 3 种 |
| 不同落点数 | 2 个 | 3 个 |

配置项 `Solver.DiversityPerBucket`（默认 2，0 = 关闭）。

**回归 `DiversityBucketTests()`**：方案数量不变 / 前排种类不少于纯 top-K /
整体种类更多 / 落点更多样 / **第 1 名与纯 top-K 相同**（多样性不影响榜首）。

### v1.25 对齐 StS2 Combat Solver 的方法论：词典序 Pareto 评估

依据：`COMBATSOLVER_TS2_完全解析.md`（StS2 战斗求解器的完整反编译解析）。

#### 核心差距：评估方式

| | StS2 Combat Solver | 我之前 |
|---|---|---|
| 最终评估 | **词典序 Pareto**（`RouteQuality`） | **线性加权** |
| 搜索引导 | `BeamRankScore` 线性启发（**与最终分分离**） | 和最终分混在一起 |

StS2 的词典序：赢 > 活 > 少用死亡救赎 > 少被偷资源 > 战略失血少 >
多成长 > 早结束 > 末位才比 Score。

**线性加权表达不了"生存优先"**：
"打 10 点但挨 8 刀"在加权下可能赢过"打 3 点不挨打"，
但按 StS2 的优先级后者更该排前面。

#### 现在的排序（词典序，前一项相等才看下一项）

```
① 会不会被打死      ← 生存的最高优先级（新增 TurnPlan.WouldDie）
② 挨打伤害少        ← 少失血
③ 击杀多
④ 有效伤害多
⑤ 治疗收益多
⑥ 线性 Score        ← 只做 tiebreak（= StS2 的"搜索引导分"）
⑦ Lost / WastedValue
```

⚠ 与 StS2 一致的关键点：**线性分不参与主要排序**，
它是"搜索往哪走"的引导分，不是"哪条路线最好"的判定。

#### 踩坑：bool 排序方向写反

```csharp
// ❌ 写反了：bool 里 true > false，但"会死"是更差 → 会死的排前面
int c = b.WouldDie.CompareTo(a.WouldDie);
// ✅ 活着在前 → 返回负数
int c = a.WouldDie.CompareTo(b.WouldDie);
```

**离线测试抓到的**：会死的方案被排到 #0，第一名挨打 5 点。
改成正确方向后，活的方案 #0-#15，会死的 #16-#19 ✓

**回归 `LexicographicOrderTests()`**：存在会死/不死两种方案 →
会死的全部排在后面 → 第一名不挨打（高先攻避开）。

#### 与 StS2 的剩余差距（按价值排序，还没做）

| StS2 部件 | 我的现状 | 价值 |
|---|---|---|
| **多样性束搜索分桶**（Control/Resource/Offense 泳道） | 只有 top-K | ★★★ Top-N 现在容易全是同一方案的变体 |
| **多宽度组合器**（BeamWidthPortfolio 共享预算） | 单一 beam 宽度 | ★★ 避免单宽度过早收敛 |
| **状态指纹去重**（dual u64，含 RNG 计数） | 方案级 Dedupe | ★★ 跨回合去重 |
| **线程让步**（每 16 单位检查取消、每次 4ms） | 主线程跑 | ★★★ **Unity 卡顿风险** |
| **GC 管控 / 内存预算** | 无 | ★★ |
| **原生驱动 + 对账重规划** | 只读建议，不自动执行 | ★ 用户未要求自动执行 |
| **Mirror 机制**（手写模拟核心，不调游戏运行时） | 已有等价物（`MODIFIER.md` 确认修正牌是确定量 + 离线复刻 RNG） | — 已具备 |

### v1.24 敌人建模补全：施加状态 + 先攻编织

用户要求"将敌人建模补全"，重点是**敌人给我方上状态**（精英冰角祭司先手缴械旗矛）。

#### ① 会施加什么条件（数据通路）

反编译实证：`CAbility.NegativeConditions` 是
`Dictionary<CCondition.ENegativeCondition, CAbility>` ——
即"**这个能力施加**的条件"，不是它自己身上的条件。

- `HandReader.ReadAppliedConditions()` 读它 → `ActionSnapshot.AppliesNegative/AppliesPositive`
- 玩家卡与**怪物卡走同一条路径**（`ReadAction` 通用）→ 敌人的缴械能被读到
- `EnemyPredictor.Prediction.AppliesNegative` 带出来

#### ② ★ 威胁按**方案先攻**分组（先攻编织终于有意义了）

之前 `ThreatenedCells` 是按角色先攻算的 → 换个先攻也没用。
现在 `P1Context.ThreatByInitiative`：我的先攻 → `ThreatInfo`。

`ThreatInfo` = 挨打格 + **会被上的负面条件**。

`BuildThreatByInitiative` 对每个可能用到的先攻分别算：
- 只算**先攻比我低**的敌人（它们这回合先动手）
- 命中格 = 敌人的预测落点 + 攻击范围，过视线

⇒ **换一张先攻更高的牌就能避开先手** —— 这正是先攻编织的价值。

#### ③ 状态导致动作执行不了 → 该方案作废

判定放在**方案构造之后**（要精确到动作发生的那一格）：

- 攻击发生在 `first` 的位置（first 有攻击时）或 `second` 的位置
- 移动同理
- 敌人得**真的打得到那一格**才会施加条件（打不到就没理由给我上缴械）

| 敌人上的状态 | 后果 |
|---|---|
| 缴械 Disarm | 攻击前被缴械 → **攻击方案作废** |
| 静足 Immobilize | 移动时被打中 → **移动方案作废** |
| 眩晕 Stun | **什么都做不了** |

**回归 `EnemyConditionTests()`（对照设计）**：
- 对照组（敌人不上缴械）：在威胁格攻击的方案 **2** 个
- 被缴械后：同样的方案 **0** 个 ✓
- 高先攻躲开缴械 → 攻击方案可用 ✓
- 静足：会挨打的移动方案 7 → 0 ✓
- 眩晕：威胁格上的动作方案全消失 ✓

`SurvivalWeightTests()` 也改成验证先攻编织：
低先攻站在威胁格 **0** 个 / 高先攻 **1** 个。

### v1.23 生存维度 + 后续回合并入角色页（学 StS2 的 Combat Solver）

用户反馈：方案不够智能、太多因素没考虑、角色页没有第二回合的方案。

#### 学到了什么（StS2 创意工坊 3790899961 = Combat Solver）

⚠ 那个链接**不是 Frosthaven 指南**，是《杀戮尖塔 2》的 Combat Solver 模组页。
但它的**设计框架**正是我缺的东西：

| StS2 Combat Solver | 本项目对应 |
|---|---|
| **跨回合搜索**，不只算当前回合 | 多回合束搜索（已有，但只在独立页） |
| 优先级：**生存 > 确认胜利 > 最小化总战损** | ★ 之前只有"伤害最大化" |
| 路线**按回合连续展示** | ★ 之前多回合在独立页，角色页看不到 |
| 敌人行动纳入模拟 | 敌人预测（已有） |
| 牌堆/洗牌/跨回合资源纳入模拟 | 修正牌确定序列（已有，实测 354/354） |
| 明确"预算内最佳，非全局最优" | 束搜索 + Top-K（已有） |

#### ① ★ 生存维度（当前回合）

**之前最大的缺陷**：评分只看伤害 → 会推荐"站到敌人面前挨一刀"的方案，
看着伤害高、实际是废招。

实现：
- `P1Context.ThreatenedCells`：格 → 预计挨打伤害
- `BuildThreatenedCells(enemyPreds, myInitiative)`：
  · 只算**先攻比我低**的敌人（它们这回合先动手）
    —— 先攻比我高的下回合才打我，不该影响当前决策
  · 按敌人的**预测落点 + 攻击范围**覆盖格子
  · 要过**视线**
  · 伤害用意图攻击力（敌人还没出手，修正牌未知）
- `ScoringWeights.SurvivalWeight` 默认 **2.0**（挨 3 刀 -6 分 > 多打 6 点伤害）
- 方案描述会写："⚠ **这回合会挨打 N 点**（先攻比你低的敌人能打到你，已计入评分）"

**回归证据**（`SurvivalWeightTests()`）：
同一局面，(14,4) 被标成会挨 5 点的威胁格：
- 默认权重 → **0** 个方案终点落在那里（避开了）
- 权重调 0 → **2** 个方案站上去（证明确实是生存维度挡掉的）

#### ② 后续回合并入角色页

以前多回合只在独立的「多回合」页，玩家要来回对照。
现在**第 2 回合及以后直接跟在角色方案后面**：

```
—— 后续回合推演（沿第 1 回合最佳方案继续）——
第2回合：驱策激励【上】+共同势力【下】 → 站(16,8) 血28 累计伤7 杀1 ⚠会挨打-3
    敌人：精英冰角祭司#3 伤3 翻牌[+0]
第3回合：… ☠ 阵亡
```

多回合结果**只算一次**（`_lastMultiTurn` 静态字段传给 Flush/ToUiSections），
角色页与多回合页共用，不会算两遍。

### v1.22 AOE 多目标伤害建模（收益最大的一次）

之前**真 AOE 完全不算伤害**（被标成"未建模"直接跳过），
旗矛/双子/火男这些 AOE 职业的方案收益全被低估。

#### 反编译实证

- `CAbilityAttack.m_TotalTargetSummaries`：**每个目标一个 summary** → 逐目标独立结算
- `m_ActorsToTargetModifiedStrength`：每个目标有各自的强度

⇒ AOE 攻击**每个目标独立抽修正牌**（不是共用一张）。

#### 两处改动

**① `IsUnsupportedAttack` 收窄**

```csharp
// 旧：任何带形状的都不算
public bool IsUnsupportedAttack => HasAttack && (HasAreaEffect || AttackTargets != 1);
// 新：只剩「阵型单体」
public bool IsUnsupportedAttack => HasAttack && HasAreaEffect && AttackTargets == 1;
```

阵型单体（旗矛 Boldening Blow）必须继续排除 ——
它的形状是**站位要求**不是伤害范围，按射程取目标会取错。

**② 枚举结构重构：多目标落进同一个方案**

旧写法是"每个目标一个方案" → AOE 的总伤害全丢了。
现在第一步的**所有**目标先算完（`step1Attacks` 收集），
再算第二步，一个 plan 装下全部攻击。`off1` 随目标累加 → 每目标独立抽牌。

#### 实测收益（复现实战场景）

「义无反照」（AOE）+「血流不止」（单体）：

| | 攻击次数 | 总伤 |
|---|---|---|
| v1.21 | 2 | 6 |
| **v1.22** | **3**（AOE 打 2 个 + 单体 1 个） | **10** |

⚠ 近似：真实覆盖范围由**玩家选的焦点格 + 形状**决定，这里只按 `AttackRange` 取目标
—— 保守（可能少打），不会多打。

### v1.21 陷阱伤害建模

反编译实证：
- 陷阱 = `ObjectImportType.Trap`
- `CObjectTrap.Damage == false` 的**不是**伤害陷阱（可能是治疗/增益），必须排除
- `CObjectTrap.GetTrapDamage()`：无主且未自定义 → 场景级默认 `ScenarioManager.TrapDamage`；否则 `DamageValue`

**实现**：`Geometry.TrapMap()` 一次建表（`(x,y) → 伤害`），
在 `Reachable` 入口构建、沿途所有 `CanWalk` 共用 —— 逐格遍历 props 会变成
O(格数 × 路径长 × props 数)。陷阱伤害并入路径 hazard，于是：

| 场景 | 行为 |
|---|---|
| 有干净格可走 | **自动绕开**（踩陷阱的自伤扣分） |
| 绕不开 | 记录 `SelfHazardDamage` 并在方案里标注"⚠ 移动会吃 N 点危险地形伤害" |
| 非伤害型陷阱（`Damage == false`） | 不计入 |

报告新增【陷阱】段，列出位置与伤害，并提示"有推/拉能力也可把敌人弄进去"。

**仍不建模**：把敌人推/拉进陷阱的收益（需要建模推/拉的落点）。

### v1.20 未建模项建模（治疗/强化/附加效果/元素提示）

日志实测的未建模能力分布（按频次）：

```
Heal 401 · MoveActiveBonusTracker 309 · MoveShadow 181 · CreateShadow 146
PerformFromShadow 145 · ControlActor 114 · MoveShadow/Damage/RemoveShadow/GainXP 91
Teleport2 83 · Poison 63 · Pull 16 · Strengthen 6
```

#### ① 治疗 Heal（401 次，最高频）—— **精确建模**

`Strength` = 治疗量、`Range`/`NumberTargets` 同攻击。

收益 = 友方（含我自己）按缺失血量降序取前 N 个，每个只算
`min(治疗量, 缺失血量)` —— **过量治疗不计**（奶满血队友等于浪费，
与有效伤害同理）。权重 `HealWeight` 默认 0.8（治疗不推进击杀，价值低于等量伤害）。

⚠ 不枚举具体治疗谁：治疗通常是"奶最残的那个"，枚举目标会让组合数翻倍而收益差异很小。

#### ② 强化 Strengthen —— **接入伤害**

自己有 Strengthen → 攻击 +1（`EffectiveAttack`）。
依据 GH/FH 标准规则与攻略一致；精确的代码应用点**没在反编译里定位到**
（攻击力计算链路较深），已在注释里标明，实测不符再回来查。

#### ③ 中毒 / 拉拽 / 控制 / 传送 —— **识别 + 提示**

这些不改变本回合的位移与伤害模型，但影响后续回合，所以必须说出来：

```
「毒刃」上 攻2射程1 从(14,3)：… → 最佳 冰角守卫#4 伤2
「毒刃」上 附加效果：给目标上**中毒 3**（目标回合末持续掉血，直到被治疗移除）。
```

`ExtraEffectText()` 统一生成，诊断与方案输出都会带。

#### ④ 元素 Element —— **识别 + 提示**（不推演）

`ElementsToInfuse` 只在 **`CAbilityInfuse`** 上（基类 CAbility 没有）——
反射校验当场抓出这个错，和 v1.18 的 `MoveRestrictionType` 同一类坑。
诊断会写"需要 **Fire** 元素 —— 本版本不校验你是否供得起，自己确认"。

元素是跨回合资源（注入/消耗/旺盛衰弱），涉及 27 个能力文件的完整子系统，
精确推演的性价比太低，所以定位为提示级。

#### ⑤ 仍不建模的（如实列入「报错」页）

陷阱伤害、冰面滑动、开门是独立动作、压力板/刷怪点、
召唤物主动召唤、范围多目标伤害、先攻编织安全性权衡。

### v1.19 疑点系统排查 + 反射静态校验

优先级链（用户定）：**使命 > 装备/宝箱 > 专长 > 战绩 > 经验 > 战利品袋(材料)**

自动计分的部分（`ScoringWeights.GoalWeight`）：

| 目标 | 价值 | 依据 |
|---|---|---|
| 宝箱（Chest / GoalChest） | 120 | 优先级第 2 档 |
| 战利品袋开出**装备** | 120 | `LootDeck[0] == RandomItem` → 等价于装备，升到第 2 档 |
| 战利品袋开出材料 | 15 | 优先级第 6 档 |
| 资源 | 15 | 优先级第 6 档 |

只按**方案终点**判定（`FinalX/FinalY`）—— 游戏是回合结束才结算 loot，中途站一下不算。

**剪枝冲突（踩过并修好）**：原本"这一步打不到东西就剪掉落点"的剪枝，
会把"走到宝箱上但打不到敌人"的方案误杀。已加 `HasSpotValue()` 放宽：
落点能捡到东西就保留，按"无攻击"建方案。回归用例 `CampaignGoalTests()` 锁住了这条。

**只提示、不自动计分的部分**：22 种使命目标类型里，
只有歼灭/伤害/拾取/位置几类能自动判定，其余（压力板、刷怪点、脚本触发…）
在窗口里如实列出，不假装替玩家决策。

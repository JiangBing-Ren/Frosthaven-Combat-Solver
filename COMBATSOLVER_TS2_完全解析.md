# STS2《战斗路线求解器 / Combat Solver》完全解析

> 目标：完整理解 Torch123 的 STS2 单人战斗求解器（Steam 工坊 3790899961，v0.50.1）的**思路与算法**，达到可复刻其方法论的程度。
> 本文是给「霜港迷城战斗求解器」做的方法论参考。结论先行：**字面移植不可能（游戏机制完全不同），但其"手写模拟核心 + 束搜索规划器 + 词典序多目标评估 + 原生驱动"的方法论可完整复刻**。
> 反编译产物：`D:/1Mods/_tmp/cs_decompiled/`（969 个 .cs，约 20.9 万行）。工具：`ilspycmd 11.1.0`。

---

## 0. 一句话结论

Combat Solver 是一个**基于搜索的战斗规划器**：它**不调用游戏运行时去"试玩"**，而是把 STS2 的战斗机制**手工重写**成一组"镜像（Mirror）"对象，在一个确定性的模拟世界里后台跑**多样性束搜索（diverse beam search）+ 多宽度组合搜索（beam portfolio）**，按**词典序多目标（胜 > 活 > 少失血 > 多成长 > 早结束）**选出整场战斗的最优路线，再通过游戏原生 UI 把路线"演"出来。

它的全部复杂度都来自两件事：
1. **手写的战斗模拟核心**（~200 个 Mirror 文件，31K 行的 `CombatBeamSolver` 里大半是卡牌/药水/选择的动作枚举与评分）——这部分是 STS2 专属的，霜港要重写。
2. **搜索与评估的工程化**（束搜索组合器、置换去重、anytime、多线程、GC 管控、路线复用）——这部分是**纯算法**，可直接迁移。

---

## 1. 基本信息

| 项 | 值 |
|---|---|
| 物品 ID | 3790899961（注意：在 `workshop/content/2868840/` 下，2868840 = 杀戮尖塔2 的 app id，**不是**霜港迷城 1976580） |
| 名称 | 战斗路线求解器 / Combat Solver |
| 作者 | Torch1230（Steam: 76561198950654908） |
| 版本 | 0.50.1 |
| 依赖 | RitsuLib ≥ 0.6.0（同我们 STS2 mod 的框架）；STS2 0.111.0；仅单人；不改游戏 RNG |
| 体积 | CombatSolver.dll 7.4 MB + CombatSolver.MemoryCleaner.exe（搜索时释放内存） |
| 许可 | MIT；模拟核心改造自 **Random Foreseer**（hotwords123，MIT），已获授权 |
| 反编译 | 969 文件 / 209,534 行；未混淆，命名清晰 |

**关键关系（来自 THIRD_PARTY_NOTICES.md）**：内置模拟核心 `uses and modifies portions of the Random Foreseer implementation`，涉及 `combat state / deck / RNG / Fork / History / Mirror` 基础逻辑。`CombatSolver 不加载或分发 Random Foreseer 程序集作为运行时依赖`（运行期分离）。即：它把 Random Foreseer 的代码**抄进自己 DLL 并重写**了，不是动态引用。

---

## 2. 整体架构（模块地图）

```
CombatSolver/                         ← 根：入口、编排、搜索、评估、执行、UI、诊断
  Entry.cs                            ← [ModInitializer] 入口，订阅生命周期事件，注册 ~40 个 patch
  CombatSearchCoordinator.cs (4541)   ← 分阶段搜索编排（主搜索 + 补充审计）
  CombatBeamSolver.cs (31141)         ← ★ 束搜索 + 卡牌/药水/选择动作枚举 + 评分（最大文件）
  BeamWidthPortfolio.cs (269)         ← ★ 多宽度束搜索组合器
  SolverController.cs (3609)           ← 搜索会话生命周期、anytime、路线复用、UI
  PlayerTurnSetupCoordinator.cs(1310) ← ★ 游戏驱动/执行（原生选牌 UI + 反射调 SetupPlayerTurn）
  SolverResult.cs / PlanAction.cs / SimulationSnapshot.cs / SearchNode.cs / CombatRootSnapshot.cs
  SearchPolicy / SolverPotionPolicy / SolverSettings / *ValuationModel (大量)
CombatSolver.Api/                     ← 与游戏/RitsuLib 的接口层
CombatSolver.Engine.Common/           ← 模拟基础设施
  PredictionStateStore.cs  PredictionRngState.cs  PredictionTrace.cs
  Mirrors/MethodMirrorRegistry.cs     ← ★ Mirror 派发核心（Random Foreseer 遗产）
CombatSolver.Engine.InCombat.Mirrors/ ← ★ 镜像实现（~203 文件）
  Cards.OnPlay/  Enchantments.OnPlay/  Potions.OnUse/  Afflictions.OnPlay/
  Hooks.{Attack,Block,Card,Damage,Death,Orb,Resources,TurnEnd,TurnStart}/
CombatSolver.Engine.InCombat.Simulation/ ← ★ 可变世界状态 + 模拟器
  CombatPredictionState.cs  CombatPredictionSimulator.cs
  SimCreatureState.cs  SimPlayerCombatState.cs
CombatSolver.Replay/                  ← 重放/无头测试
GodotPlugins.Game/Main.cs             ← 注入入口
```

**可移植性判断**：
- **纯算法（可直接迁移）**：`BeamWidthPortfolio`、`SearchNode`/`SimulationSnapshot` 的状态表示思想、`SolverResult` 的词典序评估、`Entry` 的生命周期骨架、`PlayerTurnSetupCoordinator` 的"原生驱动"模式。
- **STS2 专属（必须重写）**：`Engine.InCombat.Mirrors.*` 全部（每张卡/机制一份手写模拟）、`CombatBeamSolver` 里针对 STS2 卡牌的动作枚举、`CombatRootSnapshot` 对 STS2 `CombatState` 的抽取。

---

## 3. 核心思想：Mirror 模拟机制（最值得复刻的部分）

这是整个 mod 的基石，也是 Random Foreseer 的精髓。

### 3.1 为什么不用游戏运行时

STS2 的卡牌效果是通过游戏对象的方法调用实现的（如 `card.OnPlay()`、`creature.TakeDamage()`）。如果在模拟时真去调这些方法，会触发动画、事件、RNG、UI，既慢又不确定。

Combat Solver 的做法：**为每一个会被模拟调用的游戏方法，手写一个"镜像"方法**，镜像方法不碰游戏对象，而是把等效效果作用到自己维护的 `CombatPredictionState` 上。

### 3.2 派发机制（`MethodMirrorRegistry<TBase,TContext>`）

`Engine.Common.Mirrors/MethodMirrorRegistry.cs`：
- 针对某个"被镜像的方法"（如卡牌的 OnPlay），按**具体类型** `TModel` 注册 handler：`Register<TModel>(Action<TModel,TContext> handler)`。
- 派发结果 `MirrorDispatchKind`：`Handled`（有镜像）/ `Ignored`（显式忽略）/ `Inferred`（推断，如基类行为）/ `NotOverridden`（没镜像）。
- `Invoke(receiver, context)`：查 `receiver.GetType()` 的注册，调用对应 handler，handler 内 `context.PushDispatchSource(...)` 把效果作用到预测状态。
- **风险追踪**：未镜像的方法调用会触发 `History.RecordRisk(PredictionRiskReason.MethodNotMirrored)`——求解器知道自己的预测在哪一刻可能不准。这是它"诚实"的地方：遇到没实现的机制就标记，而不是静默错算。

`CombatMirrorContext<TBase>`（`Engine.InCombat.Mirrors/CombatMirrorContext.cs`，仅 60 行）是镜像的基类，给每个镜像方法提供统一入口：
```
Simulator (CombatPredictionSimulator)  → 可变世界状态
State     (CombatPredictionState)      → 同义
Rng       (CombatPredictionRngSet)     → 模拟用 RNG（与游戏 RNG 分离、确定）
StateStore(PredictionStateStore)       → 分叉/去重存储
History   (CombatPredictionHistory)    → 风险/已发生事件流水
CombatState(ICombatState)              → 被捕获的实时战斗（只读快照）
```

### 3.3 落地示例（具体卡牌镜像）

`Engine.InCombat.Mirrors.Cards.OnPlay/RandomTargetAttackCardMirrors.cs` —— 随机目标攻击牌的镜像：
```csharp
public static void FlakCannonOnPlay(FlakCannon card, CardOnPlayMirrorContext context)
{
    // 1) 把手牌里所有"类型=4(???)"且未消耗的牌耗尽
    List<PredictedCard> list = context.OwnerState.AllCards
        .Where(pc => (int)pc.Preview.Type == 4 && !context.OwnerState.ExhaustPile.Cards.Contains(pc))
        .ToList();
    foreach (var item in list) { context.Simulator.Exhaust(item); if (context.Simulator.HasPendingChoice) return; }
    // 2) 对随机敌人造成 N 次攻击
    context.AttackRandomOpponents(list.Count);
}
public static void RicochetOnPlay(Ricochet card, CardOnPlayMirrorContext context)
    => context.AttackRandomOpponents(((DynamicVar)card.DynamicVars.Repeat).IntValue);
// SweepingGaze 用 DamageCmd.Attack(...).FromOsty(...).TargetingRandomOpponents(...).Simulate(context.Simulator);
```

要点：
- 镜像**直接调用模拟器的 API**（`Exhaust`、`AttackRandomOpponents`、`DamageCmd...Simulate`）——这些 API 才是"战斗规则"的唯一真相来源，被所有卡牌镜像共享。
- 卡牌的 `DynamicVars`（X 费用/重复次数等）从预测状态里解析，保证跨回合一致。
- 攻击是**确定性**的：随机目标攻击会消费模拟 RNG 的一个计数（见 4.4 的 `rng_counters_in_state_key`），所以同一个快照分叉出的两条路线若攻击次数相同，命中序列完全可复现——这正是它能"精确预知"的根因（与你 FH 的 `MODIFIER.md` 结论同源：攻击修正牌是确定量，不是随机变量）。

### 3.4 这就是"200+ 镜像文件"的来源

每张卡、每个遗物钩子（OnPlay/OnUse/OnAttack/OnDamage/OnDeath…）、每个敌人意图，都有一份手写镜像。这是体力活，也是复刻成本的大头。**对霜港迷城，等价工作 = 手写每个 FH 机制的模拟**（攻击修正牌抽取、元素增减、initiative、summon、地形、状态 effect 等），共用一套 `ScenarioState` 模拟 API。

---

## 4. 世界状态表示

### 4.1 可变世界 + 不可变快照（分叉模式）

- `CombatPredictionState`（`Engine.InCombat.Simulation/CombatPredictionState.cs`，`internal sealed class`）：搜索过程中被**原地修改**的世界状态。包含玩家/敌人 `Sim*CombatState`、`SimCreatureState`、牌堆、RNG 计数、hooks 监听器等。
- `SimulationSnapshot`（`SimulationSnapshot.cs`）：搜索节点用的**不可变、扁平、约 80 个标量字段**的快照，由 `CombatPredictionState` 在某时刻冻结而成。搜索只比较/排序快照，分叉时再从父快照重建可变状态再改。

> 这是经典的高性能搜索模式：可变世界负责"走一步"，不可变快照负责"存档/比较/去重"。霜港可直接沿用。

### 4.2 SimulationSnapshot 的字段（搜索能"看到"的全部信息）

节选自 `SimulationSnapshot.cs` 构造签名（约 80 字段，节选关键的）：
```
玩家：PlayerHp, PlayerMaxHp, ProjectedPlayerHp, PlayerBlock, CumulativePlayerHpLost,
      RecoveredPlayerHp, DeathSaveUseCount/ProjectedDeathSaveUseCount, Energy, Stars
敌人：EnemyHp, EnemyBlock, AliveEnemyCount, AliveEnemyMask(ulong), RawEnemyHp, MaxCurrentEnemyHp,
      RevivingEnemyCount, EnemyCombatDistributionKey, EnemyControlDistributionKey
增益/减益：PersistentBuffValue, StrategicEffects, EnemyStrengthSuppression, EnemyWeakTurns,
      EnemyVulnerableTurns, FocusTarget*（焦点目标压力/剩余血/威胁/易伤回合）
资源/成长：LongTermResourceValue, GrowthHpCredit, GrowthRewards, StrategyGoalHpCredit/Count,
      OutstandingStolenResource, FutureResourceValue, FutureHealPotential
牌堆：HandCount, ReachableHandValue, ZeroCostPlayableCount, LiveDeckSize, LiveDeckClutter,
      ShufflesCrossed, ProjectedShuffleOrderValue/Key, UnorderedPileKey
药水：PotionUseCount, PotionStrategicCost, AutomaticPotionUseCount
进度/评估：Score, OffensiveProgressValue, CombatEndedTurn, DeathTurn, BoundaryReason,
      HasRisk, PredictionGaps
指纹（去重键）：StateKey, UnorderedPileKey, CycleShapeKey, ProjectedShuffleOrderKey,
      EnemyCombatDistributionKey（均为 StateFingerprint = dual u64）
```
`StateFingerprint` 是 `search_state_key=dual_u64` 的实现：状态 → 两个 u64 的哈希指纹，用于置换表去重与等价分桶。多种指纹服务于不同等价判定（有序 vs 无序牌堆、循环形状、预测洗牌顺序）。

---

## 5. 搜索引擎（CombatBeamSolver + BeamWidthPortfolio）

### 5.1 总体：多宽度束搜索组合器（BeamWidthPortfolio）

`BeamWidthPortfolio.cs` 的 `Run(...)`：
- 输入一组 `BeamWidthPortfolioMemberSpec`（每个含 `BeamWidth`、是否 `SecondRankBand`/`BaseScoreOnly`/`AggressivePowerCommitment`/`OffensiveRefinement`/`BoundedRefinement`）。
- **共享一个总节点预算** `sharedMaxExpandedNodes`：逐个成员跑束搜索，扣减已用节点；某成员跑完若已"可终止"（赢得战斗/无风险）可作为候选；任一成员 `StopPortfolio` 则立即停整个组合器。
- 最终从所有成员结果里按 `isBetter` 选最优，或在都没终止时 fallback 到 baseline。

`ProductionMembers(...)` 默认生成的成员组合（对应日志 `beam=60`）：
```
基线宽度 W=60
W * 2/3   （NarrowRefinementRatio，窄精炼）
W * 1.5   （WideRefinementRatio，宽精炼）
W * 0.4   （OffensiveRefinement，进攻精炼，权重 perturbation 把 EnemyHp 项 ×1.5 负向加权）
W + SecondRankBand      （第二排名带：保留被主排名压制的次优解）
W + BaseScoreOnly       （仅用基础分，不叠加战术项）
+ 可选 AggressivePowerCommitment（专注能量承诺，预留 1/5 节点预算）
```
这就是日志里 `beam_partition=unified` + `beam_lanes=balanced_defense_offense_utility_pareto_delayed` 的工程落地：**用多个不同宽度/评分策略的束搜索并行探索，共享预算，避免单宽度束搜索过早收敛**。

### 5.2 束内：多样性束搜索（Diverse Beam Search）

`CombatBeamSolver` 的 `SortByBeamRank` + 一系列 `Diversify*` / 分桶逻辑（grep 可见）：
- 先按 `BeamRankScore`（见 5.4）排序。
- 再按多维签名**分桶保留多样性代表**，而非只留 top-K：
  - `StateFingerprint`（置换去重：同状态只留 best）
  - `PocketwatchCadenceSignature` / `ProjectedShuffleOrderKey`（洗牌顺序等价类）
  - **`SearchRouteTraits` 泳道**：`Control` / `Resource` / `Offense`（攻防辅三路 Pareto）——每路独立保留高分代表（`LaneValue(snapshot, trait)`）。
- 最终 `ranked` 在保留 top-`limit`（beam width）的同时，强制纳入各桶的多样代表 + 必选节点（`AddRequired`）。

> 这正是"balanced defense/offense/utility pareto delayed"：搜索前沿是**多目标 Pareto 前沿**，不是单目标贪心。霜港可直接复用这套分桶多样性逻辑（把 trait 换成 FH 的"输出/生存/资源/控制"等维度）。

### 5.3 并行扩展

`ParallelExpansionExecutor`（degreeOfParallelism 多线程）：
- `AdmittedParent` → `AdmittedExpansionJob` → 多线程 `RunPhase(SerialJobPhase.{Prepare,Card,...})` 生成子节点。
- 卡牌**选择（choice）**用 `PrimaryChoiceReplayFrontier` / `DeferredCardActionProbe` 延迟处理：先枚举、再在确定性重放下解析选择分支（保证选择分支也被确定模拟，可复用）。
- 主循环：`wave.TryTake(outcome)` 取扩展结果 → 合并 → 排序/分桶/剪枝 → 下一波。

### 5.4 搜索引导评分（BeamRankScore，非最终质量）

`BeamRankScore`（line 6175）是**搜索时的启发式**，用于决定"先扩展哪个节点"，**不是最终胜负判定**：
```
num5 = node.Score
     + min(6, Energy) * 60000          // 留能量有价值
     + PersistentBuffDelta * (50000|150000)   // 叠持久增益
     + LatentSetupValue * 12000
     + FutureResourceValue * 10000
     + ReplayPotentialValue * 10000
     + RetainedAttackGrowth * 20000
     + DelayedDamageValue * 10000
     + SandpitRemaining * 30000
     + EnemyStrengthSuppression * 30000
     + EnemyWeakTurns * 30000
     + (actEndingBoss ? 额外项)
// BeamWeightPerturbation：进攻精炼时把 EnemyHp 项 ×(scale-1) 负向加权，引导更激进
// ContextualRankingModel.Adjustment：上下文微调
```
`CompareBeamRankOrder`：先比 BeamRankScore，平局比**更少动作数**（鼓励短路线），再比进攻进度。

> 注意分离：**搜索引导分（线性加权启发）≠ 最终选择分（词典序 Pareto）**。前者负责"往哪走"，后者负责"哪条路线最好"。霜港建议同样分离。

### 5.5 置换去重 / 状态键

`StateFingerprint`(dual u64) 作为 `SearchNode.StateKey`；`search_state_key=dual_u64`、`rng_counters_in_state_key=true`。同指纹节点只保留最优（5.2 的分桶去重），避免重复探索等价状态。霜港的 `ScenarioState` 同样需要可哈希成指纹（含 RNG 计数，保证确定性）。

### 5.6 分阶段搜索（CombatSearchCoordinator）

主搜索之外还有**补充审计**（`RunSupplementalAudits`）：
- 开局药水路线（`BuildOpeningPotionActions`）、开局能量路线（`BuildPowerActionsAfterPrefix`）、开局资源防守后续（`BuildOpeningDefensiveFollowUp`）、药水资源/能量后验（`PotionResourcePosterior` / `PotionPowerPosterior`）等。
- 这些是"主束搜索找到一条能赢的路线后，再专门在开局阶段做更精细的多方案探索"，提升开局决策质量。

### 5.7 Anytime / 路线复用 / 重规划

来自入口日志（Entry.cs:115）与 `SolverController`：
- `search_session=single_anytime`：搜索可随时停/续，UI 实时显示进度（`progress_ui=fixed_columns_seconds`，`progress_ui_ms=200`）。
- `cross_turn_reuse=exact_state_text`：跨回合复用——实机状态与预测一致时直接沿用既有路线，仅状态真正变化才重搜（`deployment_drift=replan`：实机偏离预测则重规划）。
- `SolvedRouteCache`（`PlayerTurnSetupCoordinator`）：缓存已解路线。
- 用户可"停止计算并暂停本场后续自动搜索，直到点重新计算"（`AutomaticSearchPaused`）。

---

## 6. 评估函数（最终选择：词典序多目标 Pareto）

`CombatBeamSolver` 末尾的结果排序（`list2.OrderByDescending(...)` 链，line 7667+）是**最终质量排序**，决定哪条路线被采用/展示。`RouteQuality` 承载：
```
Won, Survives, DeathSaveUseCount, OutstandingStolenResource,
StrategicHpDeficit, StrategyGoalHpCredit, StrategyGoalCount,
CombatEndedTurn, CumulativePlayerHpLost, PotionStrategicCost, EnemyHp, Score
```
排序优先级（词典序，前一项相等才看下一个）：
1. **Won**（赢） —— 首要
2. **Survives**（没死）—— 赢不了时优先活
3. **DeathSaveUseCount 少**（少用死亡救赎）
4. **OutstandingStolenResource 少**（少被偷资源，PreserveResources 策略时）
5. **StrategicHpDeficit 低**（战略失血少）
6. **GrowthHpCredit / GrowthRewardCount 高**（多成长）
7. **CombatEndedTurn 早**（早结束，`?? int.MaxValue` 保证无值排最后）
8. **PolicyHpDeficit 低** / **HealthResourceCost 低**（药水战略成本低）
9. 末位才比 `Score`（搜索引导分）

对应工坊描述"优先比较**生存、确认胜利和整场生命损失**，而不是只追求当前回合伤害"。
`sold_hp_basis=route_loss_minus_minimum_reachable_loss` = **卖血 = 路线总失血 − 可达最小失血**（即"超额失血"），正是 `CumulativePlayerHpLost` 与 `StrategicHpDeficit` 的关系。

> 霜港可直接套用此词典序框架，把维度换成 FH 的关切（击杀优先级、失血、资源/卡牌损耗、元素积累、回合效率等）。

---

## 7. 状态抽取（实时 → 模拟）

`CombatRootSnapshot`（`CombatRootSnapshot.cs`）：捕获实时 `CombatState` 进入模拟世界的根。
- 构造参数极多（玩家身份、敌人列表、意图预测 `IntentForecast`、牌堆快照、药水槽 `SearchablePotionSlotSnapshot`、初始血量/最大血、alive enemy mask、房间类型、Boss 回血策略、治疗上限评估、hook 监听数、run/combat mod 订阅数、BaseLib 卡修饰符…）。
- 实现 `ICombatPredictionRunSnapshot`：`CardMultiplayerConstraint` 等从实时战斗只读取。
- 抽取发生在 `CombatStartingEvent` / `TurnStarted` 时（`Entry.cs` 的 `SolverController.BeginCombat` / `PrepareAutomaticSearchForTurn`）。

> 霜港等价物 = 从 FH 运行时 `ScenarioState` 抽取一份 `FHScenarioSnapshot`（玩家/怪物站位、HP、状态、攻击修正牌堆、元素、initiative、手牌/牌库）。这是"读取实时状态"的环节，与 STS2 的 `CombatRootSnapshot` 一一对应。

---

## 8. 游戏驱动 / 执行（模拟 → 实时）

`PlayerTurnSetupCoordinator.cs`（1310 行）：
- 用 **`NativeChoiceSession`**（游戏的**原生选牌 UI**）——开局选牌、回合选牌、抽弃牌仍走游戏原生页面与动画（对应"原生选牌流程"）。
- 反射调用私有方法 `CombatManager.SetupPlayerTurn`（line 104：`GetMethod("SetupPlayerTurn", NonPublic)`）来驱动回合推进。
- `SolvedRouteCache` 复用路线；`ActivePlan` 持有 `CombatState/Player/NativeChoiceSession/PlannedChoices`。
- 三种使用方式落地：`仅查看`、`执行本回合`（`DeployAfterSetup`）、`连续全自动`（`FullAutoEnabled` / `WaitForTurnStartDeploymentDelayAsync`）。
- 执行时**持续与实时状态对账**：若实时偏离预测（`deployment_drift=replan` / `unplanned_combat_choice=fail_fast` / `deployment_unplayable=live_reason_log+root_replan`），则本地重规划或停手。

> 霜港的"驱动"应同样走 FH 原生 API（你 `MODIFIER.md`/`GUIDE_NOTES.md` 里已在研究的 ScenarioState 操作），保留原生动画，且必须带"实时对账 + 偏离重规划"回路，否则自动执行会跑飞。

---

## 9. 性能工程（值得抄的作业）

来自 Entry.cs:115 日志 + 代码：
- **GC 管控**：`gc_latency=combat_scoped_no_gc_region_or_clr_default`；`gc_budget=independent_configurable_default_16gb_retained_when_disabled`；`gc_partition=soh_five_sixths+loh_one_sixth`（小对象堆占 5/6、大对象堆 1/6）；`gc_manual_reclaim=both_modes_lifecycle_safe`。搜索期尽量不产生新分配（`lazy_simulation_collections=true`、`fork_context=pooled`、`state_store_fork=eager`、`history_cards=compact`）。
- **释放内存**：`CombatSolver.MemoryCleaner.exe` 是独立进程，搜索结束后主动释放被 CLR 滞留的内存（16GB 预算场景必需）。
- **后台让步**：`background_slice_ms=4`、`yield_check_interval=16`、`background_yield=adaptive_frame_recovery+thread_yield`——每 16 单位检查取消、每次最多跑 4ms 再让出，降低对游戏操作/动画影响。
- **分配遥测**：`allocation_telemetry=true`、`frame_percentiles=true`。
- **多线程**：`ParallelExpansionExecutor` 并行扩展；`PreCombatForecastWorker.PinMainProcessModSources()` 把 mod 源钉到主进程（无头预测 worker）。
- **搜索档位**：低 60s / 中 120s / 高 180s / 极高 300s / 自定义（`presets=low_60s+medium_120s+high_180s+very_high_300s+custom`），分别配时间/节点/内存预算。

> 霜港若用 BepInEx + 后台线程做搜索，同样需要：线程让步（不卡 Unity 主线程）、受控 GC/大内存预算、可选独立释放进程。你 `SOLVER_ENGINE.md` 已有相关讨论，可与此对齐。

---

## 10. 配置清单（从 Entry.cs:115 日志完整提取）

以下参数定义了它的全部行为开关（移植时即"设计空间"）：
```
simulation_engine=embedded rf_dependency=false
async=true auto_turn_search=true full_auto=true
search_session=single_anytime
default_performance_preset=medium
presets=low_60s+medium_120s+high_180s+very_high_300s+custom
search_budget=120s search_beam=60 beam_partition=unified
potion_beam_opportunity_cost_hp=18 potion_safety_candidate=1
beam_lanes=balanced_defense_offense_utility_pareto_delayed
auto_search_min_frames=3 auto_search_wait_actions=true
live_progress=true progress_ui=fixed_columns_seconds progress_ui_ms=200
background_slice_ms=4 background_yield=adaptive_frame_recovery+thread_yield yield_check_interval=16
gc_latency=combat_scoped_no_gc_region_or_clr_default gc_budget=independent_configurable_default_16gb
result_snapshot=scalar_only history_cards=compact state_store_fork=eager fork_context=pooled
risk_cache=signature_interned lazy_simulation_collections=true
search_state_key=dual_u64 cross_turn_reuse=exact_state_text horizon=time_or_node_budget
predicted_shuffles=unbounded empty_deck_can_advance=true
potion_min_hp_saved=9 rng_counters_in_state_key=true choices=true
turn_start_choices=tools_tyranny_entropy_toasty turn_start_choice_auto_deploy=true
sold_hp_basis=route_loss_minus_minimum_reachable_loss sold_hp_tracking=cumulative_excess_loss
live_end_turn_risk_recheck=true full_auto_combat_end_stop=true
deployment_drift=replan unplanned_combat_choice=fail_fast
deployment_unplayable=live_reason_log+root_replan headbutt_pre_discard=true ...
```

---

## 11. 对霜港迷城的迁移映射（复刻路线图）

| Combat Solver 部件 | 霜港等价物 | 复用级别 |
|---|---|---|
| `MethodMirrorRegistry` + 200 镜像 | 手写 FH 机制镜像（攻击修正牌抽取、元素、initiative、summon、地形、状态 effect），共用 `ScenarioState` 模拟 API | **重写镜像，复用机制** |
| `CombatPredictionState`（可变）+ `SimulationSnapshot`（不可变 ~80 字段） | `FHScenarioState`（可变）+ `FHScenarioSnapshot`（不可变哈希可比较） | **直接复用模式** |
| `StateFingerprint` dual_u64 去重 | `FHStateFingerprint`（含 RNG 计数） | **直接复用** |
| `CombatBeamSolver` 多样性束搜索 + 分桶（Control/Resource/Offense） | 同算法，trait 换成 FH 维度 | **直接复用算法** |
| `BeamWidthPortfolio` 多宽度共享预算 | 同 | **直接复用** |
| `BeamRankScore` 线性启发 | FH 版线性启发（输出/生存/资源/控制加权） | **重写权重** |
| `RouteQuality` 词典序 Pareto | FH 版词典序（击杀/失血/资源/回合） | **复用框架，换维度** |
| `CombatRootSnapshot` 状态抽取 | 从 FH `ScenarioState` 抽取 `FHScenarioSnapshot` | **重写抽取** |
| `PlayerTurnSetupCoordinator` 原生驱动 + 对账重规划 | FH 原生 API 驱动 + 实时对账 | **复用模式** |
| GC/线程/内存工程 | BepInEx 后台线程 + 让步 + 大内存预算 | **复用模式** |

### 复刻步骤建议（与你现有 FH 求解器对齐）

1. **先有确定性模拟核心**（你 `SOLVER_DESIGN.md` 已在做）：把 FH 战斗规则实现为纯函数式的 `ApplyAction(state) -> state`，状态可 `Clone`/`Fork`。这是一切前提。
2. **状态可哈希**：给 `FHScenarioState` 实现 `GetFingerprint()`（含攻击修正牌堆 RNG 计数），供置换去重。
3. **束搜索骨架**：实现 `SortByBeamRank` + 分桶多样性 + 多宽度组合器（直接搬 5.1–5.3 的结构）。
4. **评估分离**：搜索引导分（线性）与最终质量分（词典序 Pareto）分开，先上"赢 > 活 > 少失血"三档，再补资源/回合维度。
5. **状态抽取**：从 FH 运行时读 `ScenarioState` 建初始快照（注意你已发现"攻击修正牌是确定量"，抽取时要连修正牌堆顺序一起读）。
6. **原生驱动 + 对账**：自动执行走 FH 原生 API，每步对账，偏离即重规划（你 `ENEMY_AI.md`/`GUIDE_NOTES.md` 的实时回路）。
7. **性能**：后台线程 + 每帧让步 + 受控 GC；先小预算验证正确性，再上大内存/多线程。

---

## 12. 关键文件索引（继续深读用）

| 想深入 | 文件 |
|---|---|
| 入口/生命周期/全部参数 | `CombatSolver/Entry.cs` |
| 搜索组合器 | `CombatSolver/BeamWidthPortfolio.cs` |
| 束搜索主体（动作枚举+评分+剪枝） | `CombatSolver/CombatBeamSolver.cs`（31141 行，重点看 `BeamRankScore` 6175、`SortByBeamRank` 2315、`CompareBeamRankOrder` 9505、结果排序 7667+） |
| 节点/快照/状态 | `SearchNode.cs`、`SimulationSnapshot.cs`、`Engine.InCombat.Simulation/CombatPredictionState.cs`、`CombatPredictionSimulator.cs` |
| Mirror 机制 | `Engine.Common.Mirrors/MethodMirrorRegistry.cs`、`Engine.InCombat.Mirrors/CombatMirrorContext.cs`、具体示例 `Engine.InCombat.Mirrors.Cards.OnPlay/RandomTargetAttackCardMirrors.cs` |
| 分阶段编排 | `CombatSearchCoordinator.cs` |
| 状态抽取 | `CombatRootSnapshot.cs` |
| 游戏驱动 | `PlayerTurnSetupCoordinator.cs` |
| 评估/结果 | `SolverResult.cs`、`RouteQuality`（在 SimulationSnapshot 附近） |

---

## 13. 风险与提醒

- **Mock 风险即功能真相**：它的"预测可能不准"通过 `RecordMethodNotMirroredRisk` 显式记录——复刻时**每写一个镜像就要想"我漏了游戏的哪个分支"**，否则静默错算比崩溃更危险。
- **深度优先于广度**：先让模拟核心对 1~2 种怪、少量卡 100% 确定正确，再扩 Mirror 覆盖面。STS2 版也是逐步长出来的（v0.50.1 已很大）。
- **不改游戏 RNG**：模拟用独立 RNG 计数器，与游戏运行期分离——霜港同理（你已确认攻击修正牌确定，抽取时读当前牌堆顺序即可，不要去"预测"未发生的洗牌，除非要做 `predicted_shuffles`）。
- **字面代码不可移植**：31K 行的 `CombatBeamSolver` 与 200 镜像是 STS2 专属；可移植的是第 2/4/5/6/8/9 节的**结构与算法**。

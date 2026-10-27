# 阶段 3：实现计划（**先给计划，确认后再写代码**）

> 目标：实现**最小战斗闭环** —— 移动 + 单体攻击 + 修正牌 + 先攻 + 元素时序。
> 事实源：`Data/*.json`（主集）。**`Data/verify/`（ChaosDemon）不进阶段 3**，只在阶段 4 hook 验证时引用。
> 依据：`PHASE2_MODEL.md` v4（事件 / 状态 / 决策挂起点 / 伪代码 / 假设清单）。

---

## a) 文件结构与职责

依赖方向严格单向：`Model ← Engine/State ← Engine/Events ← Rules ← RoundRunner ← Tests`

| 文件（新增） | 职责 | 依赖 |
|---|---|---|
| `src/FHEngine/Rules/InitiativeSorter.cs` | 输入单位列表 → 输出排序后的 `List<TurnUnit>`（先攻升序；怪物组聚合为一个单元） | `Engine/State.cs` |
| `src/FHEngine/Rules/DamageResolver.cs` | 输入 (攻击值, 修正牌, 护盾, 穿透) → 输出 `ATTACK_RESOLVED` + `DAMAGE_APPLIED` 事件序列 | `State.cs`, `Events.cs`, `Model/` |
| `src/FHEngine/Rules/MonsterPlanner.cs` | `IMonsterAI` 的实现：找焦点 → 落点优化 → 只执行卡面所示 | `State.cs`, `IMonsterAI.cs`, `Model/` |
| `src/FHEngine/Rules/ElementTracker.cs` | 元素灌注（TURN_ENDED）与衰减（ROUND_END，全量快照） | `State.cs`, `Events.cs` |
| `src/FHEngine/Rules/ModifierDeck.cs` | 修正牌抽取（支持**预置序列**以消除随机性）与洗牌（TURN_ENDED） | `State.cs`, `Model/` |
| `src/FHEngine/Engine/RoundRunner.cs` | 串起上面组件 + 发射事件 + 决策挂起（实现 `ICombatEngine`） | 全部 |
| `tests/FHEngine.Tests/Rules/*.cs` | 各组件单测（**先写测试再写实现**） | — |
| `tests/FHEngine.Tests/SmokeTest.cs` | smoke test（规格见 §b） | 全部 |

**不改动**：`Engine/State.cs`、`Events.cs`、`Decisions.cs`、`IMonsterAI.cs`（阶段 2 已定稿）。

---

## b) smoke test 精确规格（**验收标准**）

### b.1 初始状态

| 项 | 值 | 来源 |
|---|---|---|
| 地图 | 10×10，全部 `Normal`，除单位外无 Occupied | 手工构造（阶段 1–3 不碰 .lvldat） |
| BannerSpear | **@(5,5)**，HP 10/10，盾 0 | — |
| AlgoxScout | **@(5,6)**，Level 0：HP 7, Move 3, Attack 2, Range 1, Target 1 | `Data/monsters.json` |
| 玩家修正牌堆 | `Standard`（20 张），**预置顺序**：`P0` 打头 | `Data/deck_membership.json` |
| 怪物修正牌堆 | `MonsterDeck`（20 张），**预置顺序**：`P0` 打头 | 同上 |
| 元素表 | 6 元素全 `Inert` | — |

### b.2 决策（全部预置，非随机）

| 项 | 预置值 | 说明 |
|---|---|---|
| BannerSpear 手牌 | **只有 2 张**：卡 **71**、卡 **68** | 限制手牌避免组合爆炸 |
| 选牌 | 卡 71 用**上半**（`TAttack` atk=**3**，cond=Immobilize）；卡 68 用**下半**（`BMove` mv=**0**） | 卡 71 上半来源 `Data/cards.json` |
| 出牌顺序 | **先打 71 上半，再打 68 下半** | 决定先攻 |
| BannerSpear 先攻 | **10**（= 先打的卡 71 的 Initiative） | 卡 71 `Initiative=10` |
| 怪物行动卡 | **卡 771**（`AbilityCardDeck` 预置为 `[771]`） | ini=**40**，`AMove` mv=1、`AAttack` atk=**-1**（哨兵） |
| 怪物哨兵解析 | `AAttack.atk=-1` → 按**假设 A** 取怪物 Attack = **2**（L0） | 假设 A1，**待 hook 验证** |
| 修正牌抽取 | **预置**：玩家抽 `P0(+0)`、怪物抽 `P0(+0)` | 消除随机性（见 A1-C1 的同样要求） |

### b.3 排序结果

```
TurnUnit[0] = { Initiative=10, ActorIds=["BannerSpear"], IsMonsterGroup=false }
TurnUnit[1] = { Initiative=40, ActorIds=["AlgoxScout"], IsMonsterGroup=true  }
```
→ **BannerSpear 先动**（10 < 40）。

### b.4 预期事件序列（seq 0 → N）

| seq | type | payload 关键字段（预期值） |
|---|---|---|
| 0 | `CARD_SELECTED` | actorId=BannerSpear, cardIds=[71,68], halves=[Top,Bottom], initiative=10 |
| 1 | `INITIATIVE_REVEALED` | actorId=AlgoxScout, initiative=40, source="card" |
| 2 | `TURN_STARTED` | kind="player", unitActorIds=[BannerSpear] |
| 3 | `ABILITY_RESOLVED` | abilityName="TAttack", half="Top", cardId=71 |
| 4 | `ATTACK_MODIFIER_DRAWN` | modifierName="AMC_Basic_P0", mathModifier="+0", drawnCount=1 |
| 5 | `ATTACK_RESOLVED` | baseAttack=3, modifierMath="+0", finalAttack=**3**, targetId=AlgoxScout |
| 6 | `DAMAGE_APPLIED` | targetId=AlgoxScout, preShieldAmount=3, shieldAbsorbed=0, amount=**3**, killed=false |
| 7 | `ABILITY_RESOLVED` | abilityName="BMove", half="Bottom", cardId=68 |
|   |  *(卡 68 下半 mv=0 → 不移动，无 DAMAGE 事件)* | |
| 8 | `TURN_ENDED` | actorId=BannerSpear |
|   |  *(本回合无元素灌注指令 → 无 ELEMENT_INFUSED)* | |
| 9 | `TURN_STARTED` | kind="monsterGroup", unitActorIds=[AlgoxScout] |
| 10 | `ABILITY_RESOLVED` | abilityName="AAttack", half="Top"(怪物卡无半边，记为 Top), cardId=771 |
|   |  *(怪物 @(5,6) 与 BannerSpear @(5,5) 距离 1 ≤ Range 1 → **够得着，不移动**，故无 Move 的 ABILITY_RESOLVED)* | |
| 11 | `ATTACK_MODIFIER_DRAWN` | modifierName="AMC_Basic_P0", mathModifier="+0", deckOwner="Monster", drawnCount=1 |
| 12 | `ATTACK_RESOLVED` | baseAttack=**2**, modifierMath="+0", finalAttack=**2**, targetId=BannerSpear |
| 13 | `DAMAGE_APPLIED` | targetId=BannerSpear, preShieldAmount=2, shieldAbsorbed=0, amount=**2**, killed=false |
| 14 | `TURN_ENDED` | actorId=AlgoxScout |
| 15 | `ELEMENT_DECAYED` | before={6 元素全 Inert}, after={6 元素全 Inert}（**全量**快照，本轮无变化） |
| 16 | `ROUND_ENDED` | round=1 |

**共 17 个事件（seq 0–16）**。

### b.5 断言清单

| # | 断言 | 预期 |
|---|---|---|
| 1 | 事件**总数** | 17 |
| 2 | 事件**type 序列**逐个比对 | 见 §b.4 表格 |
| 3 | AlgoxScout 最终 HP | **4**（7 − 3） |
| 4 | BannerSpear 最终 HP | **8**（10 − 2） |
| 5 | 元素表状态 | 6 元素**全 Inert**（本轮无灌注、无消耗、衰减无变化） |
| 6 | 玩家修正牌堆剩余 | **19**（20 − 1，抽了 P0） |
| 7 | 怪物修正牌堆剩余 | **19** |
| 8 | `TurnUnit[0].AllActed` / `TurnUnit[1].AllActed` | 均 **true** |
| 9 | `state.Round` | **2**（轮末 +1） |
| 10 | 无 `DecisionRequest` 残留 | null（本场景无挂起点触发） |

> ★ 断言 5 的"全 Inert"同时验证了 **ELEMENT_DECAYED 的全量 payload**：
> 即便无变化，6 个元素的档位也必须出现在 `after` 里。

---

## c) 分步实现顺序（**每步：先写测试 → 再写实现 → 跑通再进下一步**）

| 步 | 组件 | 单测用例 | 完成判据 |
|---|---|---|---|
| **1** | `InitiativeSorter` | ① 两个单位不同先攻 → 升序；② 两个单位**同先攻**（平局）；③ 怪物组（3 只共享先攻 → 聚合为 1 个 TurnUnit）；④ 玩家与怪物组混合排序 | 4 个用例全过 |
| **2** | `DamageResolver` | ① 无修正（atk=3 → 3）；② `+1`（3→4）；③ `×2`（3→6）；④ `×0`（3→0）；⑤ 护盾吸收（atk=3，盾 2 → amount=1）；⑥ 穿透（atk=3，盾 2，穿透 2 → amount=3）；⑦ 致死标记 | 7 个用例全过 |
| **3** | `MonsterPlanner` | ① **够得着**（不移动，直接攻击）；② **够不着**（逼近，IsApproachOnly=true）；③ **卡面无 Move 节点**（用卡 775，只攻击不移动）；④ **卡面无 Attack 节点**（只移动）；⑤ 平局打破（两个等距目标 → 按先攻） | 5 个用例全过 |
| **4** | `RoundRunner` | 即 **smoke test**（§b） | 10 条断言全过 |

> 每步失败时可定位到具体组件 —— 这是拆分的唯一目的。

---

## d) 决策挂起点的测试策略

**`ScriptedDecisionProvider`**（阶段 2 已声明，阶段 3 落地）：
- 构造时传入一个 `List<DecisionResponse>`（**录制序列**）
- `Request(req)` 时按游标顺序返回，并**校验 `Seq` 相等**；不等或耗尽 → 抛异常（附诊断信息）

**录制序列格式**（建议 JSON Lines，与事件日志一致便于交叉引用）：
```json
{ "seq": 4, "kind": "AdvantageDisadvantage", "chosenOptionId": "First" }
{ "seq": 9, "kind": "ElementConsumption",    "chosenOptionId": "Decline" }
```

**阶段 3 覆盖**：
| 挂起点 | 阶段 3 |
|---|---|
| #1 `AdvantageDisadvantage` | **实现 + 测试**（用卡 65，其 `TDisadvantage` 节点触发；抽 2 张后挂起选择） |
| #4 `ElementConsumption` | **实现 + 测试**（构造一个可消耗元素的能力场景） |
| #2 `BurnCard` | 接口在，**实现留空**（`NotImplementedException`） |
| #3 `TargetSelection` | 接口在，实现留空 |
| #5 `LongRest` | 接口在，实现留空 |
| #6 `DamageAssignment` | 接口在，实现留空 |

**待确认缺口**：玩家出两张牌时，**先攻取哪张**在 FH 中由玩家决定（先打哪张就用哪张的先攻）。这不在这 6 个挂起点里 —— 阶段 3 先**固定为"先打的那张"**，并在计划里标注：**是否需新增挂起点 #7「先攻/出牌顺序选择」，请你裁决**。

---

## e) 已知缺口声明（阶段 3 **不做**）

| 不做 | 计划补在 |
|---|---|
| 多角色、多怪物组 | 阶段 5 |
| AOE / 多目标攻击 | 阶段 5 |
| 召唤物 | 阶段 5 |
| 长休 | 阶段 5 |
| 燃烧卡牌（挂起点 #2） | 阶段 5 |
| 轮末持续效果（Wound 每轮伤害 / Regenerate 每轮治疗等） | 阶段 5 —— `PHASE2_MODEL.md` §5 已注明"阶段 3 为空操作" |
| `.lvldat` 关卡加载 | 阶段 1–3 均不碰；地图用手工构造的平坦 `MapState` |
| 元素消耗的询问时机（A6） | 待 hook；阶段 3 按"结算前挂起"实现 |
| ChaosDemon 扩展集 | **不进阶段 3**；仅阶段 4 hook 验证引用 |

---

## f) 本阶段变更（计划阶段）

- 新增：`PHASE3_PLAN.md`（本文件）
- 修改：`PHASE2_MODEL.md`（v4：A1-C2 三种判据、A1-C1 修正牌控制、ELEMENT_DECAYED 全量、轮末持续效果注明）
- 修改：`src/FHEngine/Engine/Events.cs`（`ELEMENT_DECAYED` payload 全量约定的注释）

**请确认后我再写实现代码。重点确认：**
1. §b 的 smoke test 规格（尤其**选牌与先攻取法**）
2. §d 末尾的**挂起点 #7（先攻/出牌顺序）是否需要新增**
3. §e 的缺口清单是否符合你对"最小闭环"的界定

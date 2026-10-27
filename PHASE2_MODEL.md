# 阶段 2：状态 / 事件 / 决策挂起点模型

> 按约定：本阶段**不写实现代码**，只产出文档 + 空壳类型定义（方法体 `throw new NotImplementedException()`）。
> 事实源：`Data/*.json`（阶段 1 提取）与 `decompiled/`（行为语义确认）。
> 所有未确认之处一律进 §6「待 hook 验证假设清单」，**不硬编码进引擎**。

**修订记录**

| 日期 | 修订 | 依据 |
|---|---|---|
| 2026-10-28 v2 | **元素灌注位置由 ROUND_END 改到 TURN_ENDED**；补 ROUND_END 的元素衰减 | 阶段 0 报告已记录"灌注发生在角色回合结束时"；v1 写成轮末属自相矛盾 |
| 2026-10-28 v2 | 补 `ATTACK_RESOLVED` 事件（事件 11 种） | 抽牌与攻击力结算是两个语义步骤，合并会让对拍粒度变粗 |
| 2026-10-28 v2 | 决策挂起点 #1 的 Trigger 改为 `after_draw_before_select` | 优势/劣势是**既定**的，需要玩家意图的是"抽完两张后选哪张" |
| 2026-10-28 v2 | `TurnOrder` 元素定义为**行动单元** | 怪物组共享先攻、作为一个整体行动 |
| 2026-10-28 v2 | A1 验证方案重做：双 hook + 可区分材料 | 原 hook 点可能在解析之后，看不到"替换动作"；且原测试材料 L0/L1 Attack 相同，无法否证 |
| 2026-10-28 v2 | 新增假设 A7（元素衰减）、A8（选牌与翻卡顺序）、A9（洗牌触发点） | — |

---

## 1. 事件 Schema

### 1.1 通用字段（每个事件都有）

日志格式：**JSON Lines**，一行一个事件。

```json
{ "seq": 12, "round": 1, "turn": 3, "actorId": "AlgoxScoutID#2", "type": "DAMAGE_APPLIED", "payload": { ... } }
```

| 字段 | 类型 | 约束 |
|---|---|---|
| `seq` | int64 | **全局单调递增**，从 0 开始，不可重复、不可回退。阶段 4 diff 按此对齐。 |
| `round` | int | 从 1 开始。 |
| `turn` | int | 本回合内行动序号（= `TurnIndex`）；非 TURN_* 事件沿用当前值。 |
| `actorId` | string | 发起/关联单位 Id；**全局事件用 `"*"`**（如 ROUND_ENDED）。取值域 = 场上单位 Id ∪ `"*"`。 |
| `type` | string | 见 §1.2 枚举。 |
| `payload` | object | 见 §1.3 各类型键约定。**弱类型字典**：新增字段不破坏对拍工具。 |

### 1.2 事件类型（12 种）

| # | Type | 语义 |
|---|---|---|
| 1 | `CARD_SELECTED` | 单位选定了本回合的牌与半边 |
| 2 | `INITIATIVE_REVEALED` | 先攻公开（含怪物翻出的行动卡先攻） |
| 3 | `TURN_STARTED` | 某**行动单元**开始行动 |
| 4 | `ABILITY_RESOLVED` | 一个能力（半边动作）结算完成 |
| 5 | `ATTACK_MODIFIER_DRAWN` | 一次攻击抽到修正牌 |
| 6 | **`ATTACK_RESOLVED`** | **攻击力结算：基础攻击 + 修正 = 最终攻击值**（v2 新增） |
| 7 | `DAMAGE_APPLIED` | 伤害落地（护盾/穿透结算后的最终值） |
| 8 | `ELEMENT_INFUSED` | 元素灌注（**TURN_ENDED 时**，不是轮末） |
| 9 | `ELEMENT_CONSUMED` | 元素被消耗 |
| 10 | **`ELEMENT_DECAYED`** | **元素衰减**（ROUND_END 内，Strong→Waning→Inert）。必须有事件承载，否则回放时元素表无法从事件日志重建 |
| 11 | `TURN_ENDED` | 某行动单元结束行动 |
| 12 | `ROUND_ENDED` | 轮结束（**衰减发生在它之前**） |

### 1.3 payload 键约定

| Type | 键 | 类型 | 说明 |
|---|---|---|---|
| CARD_SELECTED | `cardIds` / `halves` / `initiative` | string[] / string[] / int | 半边取值 `"Top"` / `"Bottom"` |
| INITIATIVE_REVEALED | `initiative` / `source` | int / string | `source` = `"card"` 或 `"default"` |
| TURN_STARTED | `kind` / `unitActorIds` | string / string[] | `kind` = `"player"` / `"monsterGroup"` |
| ABILITY_RESOLVED | `abilityName` / `half` / `cardId` / `nested` | string / string / string / string[] | `nested` 列出被触发的内层能力名（如 ControlActor 内） |
| ATTACK_MODIFIER_DRAWN | `modifierName` / `mathModifier` / `isRolling` / `deckOwner` / `drawnCount` | string / string / bool / string / int | `drawnCount` 优势/劣势时为 2 |
| **ATTACK_RESOLVED** | `baseAttack` / `modifierMath` / `finalAttack` / `targetId` | int / string / int / string | `modifierMath` 如 `"+1"` `"*2"` |
| DAMAGE_APPLIED | `targetId` / `amount` / `preShieldAmount` / `shieldAbsorbed` / `pierce` / `killed` | string / int×4 / bool | `amount` 为**最终**伤害 |
| ELEMENT_INFUSED | `elements` / `byAbilityId` / `byActorId` | string[] / string / string | 触发点为该单位的 TURN_ENDED |
| ELEMENT_CONSUMED | `elements` / `byActorId` / `forAbility` | string[] / string / string | — |
| **ELEMENT_DECAYED** | `before` / `after` | object / object | 元素名→档位，衰减**前后**两份快照 |
| TURN_ENDED | `actorId` / `unitActorIds` | string / string[] | — |
| ROUND_ENDED | `round` / `decayedElements` | int / object | `decayedElements` 记录衰减前后档位 |

### 1.4 状态折叠

```csharp
BattleState Apply(BattleState state, EngineEvent e)   // 纯函数，不改入参
```

**唯一**允许修改 `BattleState` 的地方。事件日志可完整重放 → 回放 = 从初始状态依次 apply 事件序列。

---

## 2. 状态 Schema

| 类型 | 关键字段 | 说明 |
|---|---|---|
| `HexCoord` | X, Y | 六边形坐标 |
| `ActorState` | Id, LocKey, Hp, MaxHp, Shield, Pos, Initiative, SelectedCardIds, PlayedHalves, Conditions, IsDead | 玩家/怪物/召唤物/盟友通用 |
| `MonsterState` | MonsterClassId, Level, Move, Attack, Range, Target, CurrentCardId, AbilityCardDeck, HasActed | 怪物专有（属性已按等级表解析，见假设 A1） |
| `MapState` | Width, Height, Tiles[], Occupied[] | **手工构造的平坦地图**（阶段 1–3 不碰 .lvldat） |
| **`ElementLevel`** | `Inert` / `Waning` / `Strong` | **三档**（v2） |
| `ElementBoard` | `Levels[6]`（`ElementLevel[]`）, `PendingInfusion{}` | 顺序：Fire, Ice, Air, Earth, Light, Dark |
| `ModifierDeckState` | DeckName, Remaining[], Discarded[], ShufflePending | — |
| **`TurnUnit`** | Initiative, ActorIds[], IsMonsterGroup, AllActed | **行动单元**（v2） |
| `BattleState` | Round, Phase, TurnIndex, `TurnOrder[]`（TurnUnit）, Actors[], Monsters[], Map, Elements, ModifierDecks{} | 聚合根 |

**回合状态机**：`ROUND_START → CARD_SELECTION → INITIATIVE_SORT → TURN_LOOP → ROUND_END`

### 2.1 ★ 元素时序（v2 修正）

| 动作 | 时点 | 说明 |
|---|---|---|
| 元素消耗 | 能力结算中（受挂起点 #4 约束） | — |
| **元素灌注** | **TURN_ENDED**（单位回合结束） | 怪物组特例：**组内最后一个**怪物的 TURN_ENDED 才灌注 |
| **元素衰减** | **ROUND_END** | `Strong → Waning → Inert`（档位数与影响因素见假设 A7） |

> v1 把灌注写在 ROUND_END 是**错的**。具体后果：A 单位回合末灌注的火元素，
> 同轮后续的 B 单位本可消耗——若等到轮末才灌注，B 就消耗不到，与本体行为不一致。

### 2.2 TurnOrder 的元素是"行动单元"

怪物组的多个怪物**共享一个先攻值、作为一个整体行动**。因此排序的基本单位不是单只怪物。
`TurnUnit.IsMonsterGroup = true` 时，`ActorIds` 为组内全部怪物 Id；`AllActed` 用于判断"最后一个怪物"以触发灌注。

---

## 3. 决策挂起点

设计原则（**硬性**）：引擎**绝不自动判定**这些节点——一律挂起，等待外部注入。决策序列是显式输入，这是可回放与可测试的前提。

| # | Kind | 触发时机（Trigger） | 可选动作集合 | 暂停/恢复协议 |
|---|---|---|---|---|
| 1 | `AdvantageDisadvantage` | **`after_draw_before_select`**（抽牌**之后**） | `{First, Second}`（选第 1 张 / 第 2 张） | 抽完两张后挂起；**无优势/劣势时抽一张直接用，不挂起** |
| 2 | `BurnCard` | `on_damage_taken` | 手牌 Id 列表 + `{Decline}` | 选 Decline 则正常结算伤害 |
| 3 | `TargetSelection` | `before_attack_targets` | 合法目标组合（或焦点格）列表 | — |
| 4 | `ElementConsumption` | `before_ability_resolve` | `{Consume(element), Decline}` | 询问时机本体待确认（假设 A6） |
| 5 | `LongRest` | `on_long_rest` | 已失去的牌 Id 列表 | — |
| 6 | `DamageAssignment` | `before_damage_assignment` | 分配方案列表 | — |

> **挂起点 #1 的 v1 错误**：写成"抽牌前决定有没有优势"。
> 实际上优势/劣势是**明确给定**的（卡面写了 Advantage，或状态给了 Disadvantage），
> 玩家不需要决定"这次有没有优势"。需要玩家意图的是**抽完两张后选哪一张**——这才是"优势模糊性"。

**协议**：
```
引擎: Step(provider)
  → 推进直到需要决策
  → 生成 DecisionRequest{ Seq = 下一个事件 seq, Kind, ActorId, Trigger, Options }
  → 调用 provider.Request(req)
  → 收到 DecisionResponse，校验 Seq 相等（不等则抛异常）
  → 恢复执行
```

测试用 `ScriptedDecisionProvider`（按录制序列顺序返回，seq 错位即报错）。

---

## 4. AI 纯函数签名

```csharp
public interface IMonsterAI
{
    MonsterPlan Plan(
        MonsterState monster,
        MapState map,
        IReadOnlyList<ActorState> actors,
        MonsterCard card);          // 结构化能力树（来自 monster_cards.json）
}

public sealed class MonsterPlan
{
    IReadOnlyList<HexCoord> Path;   // 移动路径；空 = 不移动
    IReadOnlyList<string> Targets;  // 攻击目标 Id 列表；空 = 不攻击
    string FocusActorId;            // 焦点（展示"它在追谁"）
    bool IsApproachOnly;            // 是否"够不着 → 逼近"
    string Reason;                  // 判定依据（对拍用）
}
```

**契约**：
1. **纯函数**：只读入参，不改状态，不产生随机。随机源（行动卡、修正牌序）全部显式入参。
2. 焦点 = 距离最近者；同距按先攻打破平局（**待验证 → A3**）。
3. 落点 = 焦点下"能打到 + 移动代价最小"的格子。
4. **只执行卡面所示**：卡面有 Move 节点才移动，有 Attack 节点才攻击。卡面没有的节点**不臆造**——不能因为"怪物有 Attack 属性"就凭空生成一次攻击（这正是 A1 要验证的分歧点）。

---

## 5. 最小战斗循环伪代码（v2）

```
RunRound(state, provider):
    # ---- ROUND_START ----
    state.Phase = ROUND_START
    重置本回合标记（HasActed / PlayedHalves / PendingInfusion）

    # ---- CARD_SELECTION ----
    state.Phase = CARD_SELECTION
    # ★ 顺序（规则层已确认，直接写死；A8 只验证"本体实现是否一致"）：
    #     玩家先选牌 → 怪物翻卡 → 公开先攻 → 排序
    for each playerActor:
        emit(CARD_SELECTED{ actorId, cardIds, halves, initiative })
    # 怪物翻行动卡（不询问，按卡堆顺序）—— 在玩家选牌**之后**
    for each monsterGroup:
        group.CurrentCardId = group.AbilityCardDeck.Pop()
        card = monster_cards[group.CurrentCardId]
        emit(INITIATIVE_REVEALED{ actorId=group, initiative=card.Initiative, source="card" })

    # ---- INITIATIVE_SORT ----
    state.Phase = INITIATIVE_SORT
    state.TurnOrder = 所有**行动单元**按 Initiative 升序     # 单元 = 1 玩家 / 1 怪物组
        # 同先攻的打破规则 → A3

    # ---- TURN_LOOP ----
    state.Phase = TURN_LOOP
    for turnIdx, unit in enumerate(state.TurnOrder):
        state.TurnIndex = turnIdx
        emit(TURN_STARTED{ kind, unitActorIds = unit.ActorIds })

        for actorId in unit.ActorIds:
            if actor 是怪物:
                plan = ai.Plan(monster, map, actors, card)         # 纯函数
                if plan.Path 非空:  移动; emit(ABILITY_RESOLVED{ Move节点 })
                if plan.Targets 非空:
                    emit(ATTACK_MODIFIER_DRAWN{ drawnCount })      # 优势/劣势时 = 2
                    if drawnCount == 2:
                        挂起 DecisionRequest{AdvantageDisadvantage, Trigger="after_draw_before_select"}
                        # ↑ 抽牌**之后**才挂起；无优劣势时不挂起
                    emit(ATTACK_RESOLVED{ baseAttack, modifierMath, finalAttack, targetId })   # v2 新增
                    emit(DAMAGE_APPLIED{ targetId, preShieldAmount, shieldAbsorbed, amount, killed })
            else:
                按已选半边结算
                    # 元素消耗（挂起点 #4）、燃烧卡牌（挂起点 #2）在此挂起

        # ★★ 元素灌注在**行动单元结束时**（v2 由轮末改到这里）
        emit(TURN_ENDED{ unitActorIds })
        if unit 本回合有灌注指令:                             # 怪物组：组内最后一个才置位
            emit(ELEMENT_INFUSED{ elements, byAbilityId, byActorId })
            Elements.Levels[对应元素] = Strong
        # 洗牌：若本行动单元抽到 Shuffle 牌 → 在**本行动单元 TURN_ENDED** 洗对应牌堆
        #   ★ 规则层已确认（桌游规则：抽到 Shuffle 的单位在自己回合结束时洗牌），直接写死；
        #     A9 只验证"本体实现是否一致、有无例外"。

    # ---- ROUND_END ----
    state.Phase = ROUND_END
    # ★ ROUND_END **不是原子步骤**，内部顺序定死（否则阶段 4 事件序列对不齐）
    #   1) 处理轮末持续效果（若有）
    结算轮末持续效果
    #   2) 元素衰减：Strong → Waning → Inert
    before = Elements.Levels 的快照
    for each element:
        Strong → Waning ; Waning → Inert
    #   3) 衰减本身产生事件（状态修改必须有事件承载）
    emit(ELEMENT_DECAYED{ before, after = Elements.Levels 快照 })
    #   4) 轮结束
    emit(ROUND_ENDED{ round })
    #   5) 轮号推进
    state.Round += 1
```

---

## 6. 待 hook 验证假设清单

> 每条均为**工作假设**，未进引擎硬编码。阶段 4 用运行时 hook 逐条验证；被否证则立即修正并重跑提取/测试。

### A1 ★ 哨兵值 `-1 / -2` 的语义（**排第一**，v3 定稿）

**假设**：卡面数值为负时，取怪物自身同名属性（`Move:-1/-2 → 怪物 Move`，`Attack:-1/-2 → 怪物 Attack`）。

**反编译证据（未找到该分支）** — 搜索记录见
`Data/manifest.json → unresolved.sentinel_values.search_evidence`：

| 搜索符号 | 命中 |
|---|---|
| `EnemyCard`（CSRLYML.cs） / EnemyCard 解析类 | 0 / 0 |
| `Sentinel` / `ResolveAmount` / `ParseAmount` | 各 0 |
| `Attack < 0` / `Strength < 0` | 1 处无关（CAbilityMoveActiveBonusTracker.cs:119，语义为 TrackerIndex） |
| `CreateDefaultAttack` 调用点 | 除 `CMonsterClass.cs:233` 外**全是硬编码**（`2,1,1` / `1,Range,1`） |

→ `CMonsterClass.cs:233-234` 的语义是"卡面没有该能力时补默认"，对应 candidate B，**不是** A。
→ 已排除"解析层有 `amount < 0` 分支"，故替换点只可能在 **CAbilityAttack 构造** 或 **CMonsterClass 默认能力填充**。

**★ 测试材料：`ChaosDemon` 扩展集（v3 定稿）**

用户裁决：ChaosDemon 只进 **`Data/verify/` 验证用扩展集**，
**不进**阶段 1 的 `Data/` 主集；阶段 3 最小闭环仍只用原 4 个怪物，不被污染。

选它的理由：`ChaosDemonID` 的 **Attack L0=2 / L1=3 —— 相邻等级即可区分**，
远优于 AlgoxScout（`L0=L1=L2=2`，只有 L3 变 3，需跳场景等级，变量更多）。

产出（`Data/verify/verify_manifest.json` 标注 `purpose: "A1 verification material"`）：
`chaosdemon.json`（2 个怪物）、`chaosdemon_cards.json`（8 张卡）。

**★ 新增证据：同语义多值（A1 证据链的关键一环）**

哨兵值分布实测（ChaosDemon 8 张卡）：

| 字段 | 哨兵值 → 出现的卡 |
|---|---|
| `Move` | **-1 → 卡 538 / 543 / 545**，**-2 → 卡 540** |
| `Attack` | -1 → 卡 539 / 541 / 545 |

→ **同一字段 `Move` 在同一怪物的卡组里用了 -1 和 -2 两个值**。
若本体语义不是"都取怪物属性"，这种做法无法解释。这构成一条
**不依赖跨等级对比**的验证路径。

**验证用例（两条，互为交叉）**

| 用例 | 做法 | 若 A 成立 | 否证条件 |
|---|---|---|---|
| **A1-C1** 跨等级（Attack） | 卡 **539**（`AAttack.Attack = -1`），ChaosDemon 在 **L0 与 L1** 各执行一次 | L0 攻击 = **2**、L1 = **3**（随等级表 Attack 变化） | 两级攻击恒为某硬编码值、不随等级变化 |
| **A1-C2** 同义多值（Move） | 卡 **538**（`Move:-1`）与卡 **540**（`Move:-2`），**同一 Level** 各执行一次 | 两者实际移动力**相同**，均 = ChaosDemon Move(**3**) | 两者移动力不同 → -1 与 -2 语义不同 |

`A1-C2` 更干净：不改场景等级，变量最少，直接在同等条件下比较两个哨兵值。

**hook 方案（双 hook）**
1. **主 hook（抓替换动作）**：`CAbilityAttack` 构造函数（看 `Attack` 赋值前后）
   或 `CMonsterClass.CreateDefaultAttack`（看入参是 `m_CurrentMonsterStat.Attack` 还是卡面值）
2. **辅 hook（交叉验证）**：`ActorBeenDamaged` / `CCombatLogMessage_MessageData` 拿最终值
3. 两条 hook 结论一致才下判断；若冲突，以主 hook 为准并重新设计材料

**影响**：怪物移动力、攻击力、伤害计算、AI 纯函数输出（4 条主线）。


### A2 `Target: 99` 的语义

- **假设**：`Target: 99` + 同节点 `Targeting: All` → "全部目标"
- **证据**：`61_BannerSpear_AtAllCosts.yml`（THeal.Target=99，同节点 Targeting: All）；`779` 的 ControlPig1.Target=99
- **验证**：实战 61 卡，hook 治疗事件看是否作用于全部友方；实战 779 看 ControlPig1 是否对全场生效

### A3 AI 焦点选择与平局打破

- **假设**：焦点 = 距离最近者；同距按先攻（小者先）
- **验证**：构造"两个单位与怪物等距"的局面，hook `CAIMoveNew` 的焦点输出

### A4 元素衰减（v3：规则层定死实现层待验）

- **规则层（已确认，直接写进伪代码）**：
  - 衰减位置 = **ROUND_END**，且 ROUND_END 内部顺序定死：
    `轮末持续效果 → 元素衰减 → emit(ELEMENT_DECAYED) → emit(ROUND_ENDED) → round += 1`
  - **衰减本身产生 `ELEMENT_DECAYED` 事件**（第 12 种），payload 含 `before` / `after` 两份档位快照。
    理由：元素表是战斗状态的一部分，衰减是对状态的修改；按事件溯源原则必须有事件承载，
    否则**回放时元素表无法从事件日志重建**。
- **实现层（待 hook）**：档位是三档（Strong/Waning/Inert）还是更多；是否受某些效果影响
- **验证**：hook 元素表在 ROUND_END 前后的档位变化，核对 `ELEMENT_DECAYED` 的 before/after

### A4b 洗牌触发点（v3 拆两段）

- **规则层（已确认，直接写进伪代码）**：洗牌触发点 = **该单位自己的 TURN_ENDED**
  （桌游规则：抽到 Shuffle 牌的单位在自己回合结束时洗牌），**不是轮末**
- **实现层（待 hook）**：本体是否严格按此实现；有无例外（如**多目标攻击中途**抽到 Shuffle 是否立即洗）
- **验证**：hook `AttackModifierDeckState` 的 `ShufflePending` 置位与洗牌动作的时间点


### A5 ReducedRandomness 的 `*_Shuffle` 实际行为

- **假设**：`M2_Shuffle`/`P2_Shuffle` 效果同 M2/P2 且带 Shuffle 标记，用于替代 ×0/×2
- **验证**：hook 抽牌，看 `MathModifier` 与 `Shuffle` 字段实际值及是否真的触发洗牌

### A6 元素消耗的询问时机

- **假设**：消耗是可选项，引擎在能力结算**前**挂起询问
- **验证**：hook 元素消耗事件，看其发生在 `ABILITY_RESOLVED` 之前还是之中

### A7 选牌与翻卡的先后顺序（v3 拆两段）

- **规则层（已确认，直接写进伪代码）**：
  **玩家先选牌 → 怪物翻卡 → 公开先攻 → 排序**
  （先攻排序需要所有单位先攻值已知，故翻卡必在排序前；玩家选牌时怪物卡**尚未翻**）
- **实现层（待 hook）**：本体是否严格按此顺序
- **验证**：hook `CARD_SELECTED` 与怪物翻卡动作的时间戳顺序


## 7. 本阶段变更

- 文档：`PHASE2_MODEL.md`（本文件，v2）
- 空壳类型（**无实现，方法体均 `NotImplementedException`**）：
  - `Engine/State.cs` — 新增 `ElementLevel`（三档）、`TurnUnit`（行动单元）；`ElementBoard` 与 `BattleState.TurnOrder` 相应改写
  - `Engine/Events.cs` — 新增 `ATTACK_RESOLVED`（第 11 种事件）
  - `Engine/Decisions.cs` — 挂起点 #1 的 Trigger 改为 `after_draw_before_select`
  - `Engine/IMonsterAI.cs`
- 阶段 1 数据校验：**39 通过 / 0 失败**（阶段 2 改动未破坏）

**下一步：阶段 3（最小战斗循环实现）。请确认 §5 伪代码 v2 与 §6 假设清单后开始。**

# 阶段 2：状态 / 事件 / 决策挂起点模型

> 按约定：本阶段**不写实现代码**，只产出文档 + 空壳类型定义（方法体 `throw new NotImplementedException()`）。
> 事实源：`Data/*.json`（阶段 1 提取）与 `decompiled/`（行为语义确认）。
> 所有未确认之处一律进 §5「待 hook 验证假设清单」，**不硬编码进引擎**。

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

### 1.2 事件类型（10 种）

| # | Type | 语义 |
|---|---|---|
| 1 | `CARD_SELECTED` | 单位选定了本回合的牌与半边 |
| 2 | `INITIATIVE_REVEALED` | 先攻公开（含怪物翻出的行动卡先攻） |
| 3 | `TURN_STARTED` | 某单位开始行动 |
| 4 | `ABILITY_RESOLVED` | 一个能力（半边动作）结算完成 |
| 5 | `ATTACK_MODIFIER_DRAWN` | 一次攻击抽到修正牌 |
| 6 | `DAMAGE_APPLIED` | 伤害落地（**护盾/穿透结算后的最终值**） |
| 7 | `ELEMENT_INFUSED` | 元素灌注（**只在回合结束**） |
| 8 | `ELEMENT_CONSUMED` | 元素被消耗 |
| 9 | `TURN_ENDED` | 某单位结束行动 |
| 10 | `ROUND_ENDED` | 回合结束 |

### 1.3 payload 键约定

| Type | 键 | 类型 | 说明 |
|---|---|---|---|
| CARD_SELECTED | `cardIds` | string[] | 选中的卡 Id |
| | `halves` | string[] | 对应半边 `"Top"` / `"Bottom"` |
| | `initiative` | int | 本次选择决定的先攻 |
| INITIATIVE_REVEALED | `initiative` | int | — |
| | `source` | string | `"card"`（来自卡） / `"default"`（无卡时的默认） |
| TURN_STARTED | `kind` | string | `"player"` / `"monster"` / `"summon"` |
| ABILITY_RESOLVED | `abilityName` | string | 能力节点名（如 `AAttack`） |
| | `half` | string | `"Top"` / `"Bottom"` |
| | `cardId` | string | 来源卡 Id |
| | `nested` | string[] | 若含嵌套（如 ControlActor），列出被触发的内层能力名 |
| ATTACK_MODIFIER_DRAWN | `modifierName` | string | 如 `AMC_Basic_P1` |
| | `mathModifier` | string | 如 `"+1"` / `"*2"` / `"*0"` |
| | `isRolling` | bool | — |
| | `deckOwner` | string | 牌堆归属（角色 Id 或 `"Monster"`） |
| | `drawnCount` | int | 本次抽了几张（优势/劣势为 2） |
| DAMAGE_APPLIED | `targetId` | string | — |
| | `amount` | int | **最终**伤害（护盾后） |
| | `preShieldAmount` | int | 护盾前伤害 |
| | `shieldAbsorbed` | int | 被护盾吸收量 |
| | `pierce` | int | 穿透 |
| | `killed` | bool | 是否致死 |
| ELEMENT_INFUSED | `elements` | string[] | 灌注的元素名 |
| | `byAbilityId` | string | 来源能力 |
| ELEMENT_CONSUMED | `elements` | string[] | — |
| | `byActorId` | string | — |
| | `forAbility` | string | — |
| TURN_ENDED | `actorId` | string | 冗余但便于对拍 |
| ROUND_ENDED | `round` | int | — |

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
| `MonsterState` | MonsterClassId, Level, Move, Attack, Range, Target, CurrentCardId, AbilityCardDeck, HasActed | 怪物专有（属性**已按等级表解析**，见假设 A1） |
| `MapState` | Width, Height, Tiles[], Occupied[] | **手工构造的平坦地图**（阶段 1–3 不碰 .lvldat） |
| `ElementBoard` | Levels[6], InfusedThisRound | 顺序：Fire, Ice, Air, Earth, Light, Dark |
| `ModifierDeckState` | DeckName, Remaining[], Discarded[], ShufflePending | — |
| `BattleState` | Round, Phase, TurnIndex, TurnOrder[], Actors[], Monsters[], Map, Elements, ModifierDecks{} | 聚合根 |

**回合状态机**：`ROUND_START → CARD_SELECTION → INITIATIVE_SORT → TURN_LOOP → ROUND_END`

---

## 3. 决策挂起点

设计原则（**硬性**）：引擎**绝不自动判定**这些节点——一律挂起，等待外部注入。决策序列是显式输入，这是可回放与可测试的前提。

| # | Kind | 触发时机 | 可选动作集合 | 暂停/恢复协议 |
|---|---|---|---|---|
| 1 | `AdvantageDisadvantage` | 抽修正牌**之前** | `{None, Advantage, Disadvantage}` | 引擎发 `DecisionRequest(Trigger="before_draw_modifier")` 后暂停；收到响应后继续抽牌 |
| 2 | `BurnCard` | 受到伤害时（抵消伤害） | 手牌 Id 列表 + `{Decline}` | `Trigger="on_damage_taken"`；选 Decline 则正常结算伤害 |
| 3 | `TargetSelection` | 多目标 / AOE 时 | 合法目标组合（或焦点格）列表 | `Trigger="before_attack_targets"` |
| 4 | `ElementConsumption` | 能力**可**消耗元素时 | `{Consume(element), Decline}` | `Trigger="before_ability_resolve"`；**注意**：是否在结算前询问待 A6 确认 |
| 5 | `LongRest` | 长休时 | 已失去的牌 Id 列表 | `Trigger="on_long_rest"` |
| 6 | `DamageAssignment` | 伤害可分配到多个目标时 | 分配方案列表 | `Trigger="before_damage_assignment"` |

**协议**：
```
引擎: Step(provider)
  → 推进直到需要决策
  → 生成 DecisionRequest{Seq = 下一个事件 seq, Kind, ActorId, Trigger, Options}
  → 调用 provider.Request(req)
  → 收到 DecisionResponse
  → 校验 response.Seq == request.Seq（不等则抛异常）
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

## 5. 最小战斗循环伪代码

```
RunRound(state, provider):
    # ---- ROUND_START ----
    emit(ROUND_START 相关: 重置本回合标记, Elements.InfusedThisRound = false)
    state.Phase = ROUND_START
    # 注：回合起始效果（如元素可消耗性刷新）在此结算

    # ---- CARD_SELECTION ----
    state.Phase = CARD_SELECTION
    for each playerActor in state.Actors(玩家侧):
        # 挂起：选牌本身也是一个决策（本阶段先用 CARD_SELECTED 事件直接注入）
        emit(CARD_SELECTED{ actorId, cardIds, halves, initiative })

    # 怪物翻行动卡（不询问，按卡堆顺序）
    for each monster in state.Monsters:
        monster.CurrentCardId = monster.AbilityCardDeck.Pop()
        card = monster_cards[monster.CurrentCardId]
        emit(INITIATIVE_REVEALED{ actorId, initiative = card.Initiative, source = "card" })

    # ---- INITIATIVE_SORT ----
    state.Phase = INITIATIVE_SORT
    state.TurnOrder = 所有单位按 Initiative 升序
        # 平局规则待验证 → A3

    # ---- TURN_LOOP ----
    state.Phase = TURN_LOOP
    for turnIdx, actorId in enumerate(state.TurnOrder):
        state.TurnIndex = turnIdx
        emit(TURN_STARTED{ actorId, kind })

        if actor 是怪物:
            plan = ai.Plan(monster, map, actors, card)      # 纯函数
            if plan.Path 非空:
                移动; emit(ABILITY_RESOLVED{ abilityName = Move节点名, ... })
            if plan.Targets 非空:
                emit(ATTACK_MODIFIER_DRAWN{ ... })          # 抽牌前挂起 A/D 决策
                emit(DAMAGE_APPLIED{ targetId, amount, ... })
                if 目标死亡: 标记 IsDead
        else:
            按已选半边依次结算
                # 元素消耗决策点（ElementConsumption）在此挂起
                # 燃烧卡牌决策点（BurnCard）在受伤时挂起

        emit(TURN_ENDED{ actorId })

    # ---- ROUND_END ----
    state.Phase = ROUND_END
    # ★ 元素灌注**只在这里**发生
    for each 本回合未消耗的元素:
        emit(ELEMENT_INFUSED{ elements, byAbilityId })
    # 洗牌：若本回合抽到 Shuffle 牌 → 洗对应牌堆（时机待验证 → A4）
    emit(ROUND_ENDED{ round })
    state.Round += 1
```

---

## 6. 待 hook 验证假设清单

> 每条均为**工作假设**，未进引擎硬编码。阶段 4 用运行时 hook 逐条验证；被否证则立即修正并重跑提取/测试。

### A1 ★ 哨兵值 `-1 / -2` 的语义（**排第一**）

- **假设**：卡面数值为负时，取怪物自身同名属性（`Move:-1 → 怪物 Move`，`Attack:-1/-2 → 怪物 Attack`）
- **反编译证据**：**未找到**该分支。搜索记录（见 `Data/manifest.json → unresolved.sentinel_values.search_evidence`）：
  - `EnemyCard`（CSRLYML.cs）：0 处；EnemyCard 解析类文件：0
  - `Sentinel` / `ResolveAmount` / `ParseAmount`：各 0 处
  - `Attack < 0` / `Strength < 0`：仅 1 处无关命中（CAbilityMoveActiveBonusTracker.cs:119，语义为 TrackerIndex）
  - `CreateDefaultAttack` 调用点：除 `CMonsterClass.cs:233` 外均为**硬编码值**（`2,1,1` / `1,Range,1`）
  - → `CMonsterClass.cs:233-234` 的语义是"卡面没有该能力时补默认"，对应 candidate B，**不是** A
- **观测**：770_Scout_CruelBow 用 `Attack: -1`，779_AbaelHerder 用 `Attack: -2` —— 同语义两值，不能猜
- **验证方法**：
  - hook：`GameState.DrawAndApplyAttackModifierCards`（Prefix 取 `attackStrength` 入参）
  - 场景：取 **770_Scout_CruelBow**，让 AlgoxScout 在 **Level 0 与 Level 1 各执行一次**
  - 看：`attackStrength` 实际值
  - 预期（若 A 成立）：Level0 → 2（该级 Attack），Level1 → 2（若两级 Attack 相同）→ 应随等级表变化
  - 否证条件：若 `attackStrength` 恒为硬编码（如 2 或 1）而不随等级变化 → A 错，改按 B 重算 `resolved`
- **影响**：怪物移动力、攻击力、伤害计算、AI 纯函数输出（4 条主线）

### A2 `Target: 99` 的语义

- **假设**：`Target: 99` + 同节点 `Targeting: All` → 语义为"全部目标"
- **证据**：`61_BannerSpear_AtAllCosts.yml` 的 `THeal.Target = 99` 且同节点 `Targeting: All`；`779` 的 `ControlPig1.Target = 99`
- **验证方法**：
  - 实战打 `61_BannerSpear_AtAllCosts`，hook 治疗事件，看是否作用于**全部**友方
  - 实战触发 `779_AbaelHerder_DesperateHerd`，看 `ControlPig1` 是否对全场生效
- **预期**：是 → A2 成立；否 → 按实际范围修正

### A3 AI 焦点选择的平局打破规则

- **假设**：焦点 = **距离最近**者；同距时按**先攻**打破（小者先）
- **证据**：`CAIMoveNew.ChooseClosestActor` + `ActorInitiativeComparer`（此前工作的既有结论，需 hook 复核）
- **验证方法**：构造"两个单位与怪物距离相同"的局面，hook `CAIMoveNew` 的焦点输出
- **预期**：选先攻较小的那个
- **否证处理**：改平局规则并重跑 AI 测试

### A4 `Shuffle: True` 的洗牌时机

- **假设**：抽到带 Shuffle 的牌后**置位**，在**回合末**洗牌（而非立即洗）
- **证据**：此前工作记录——"×0/×2 抽到只置位，回合末才洗"（`MODIFIER.md`）
- **验证方法**：hook `AttackModifierDeckState` 的 `ShufflePending` 与洗牌动作，观测置位→洗牌的时间点
- **预期**：置位与洗牌分属两个时刻，洗牌在回合结束
- **影响**：多目标攻击的后续抽牌序列

### A5 ReducedRandomness 房规的 `*_Shuffle` 实际行为

- **假设**：`M2_Shuffle` / `P2_Shuffle` 是"效果同 M2/P2，且带 Shuffle 标记"，用于替代 ×0/×2
- **证据**：`ReducedRandomness.yml` 用 `M2_Shuffle`+`P2_Shuffle` 替换了 `T0(×0)`+`T2(×2)`（阶段 1 明文确认）
- **验证方法**：hook 抽牌，看 `M2_Shuffle` 的 `MathModifier` 与 `Shuffle` 字段实际值，以及是否真的触发洗牌
- **预期**：`MathModifier` = `"-2"` / `"+2"`，`Shuffle` = true

### A6 元素消耗的询问时机

- **假设**：元素消耗是**可选项**，引擎需在能力结算**前**挂起询问
- **证据**：阶段 1 未提取到相关明文；属流程语义，需 hook 确认
- **验证方法**：hook 元素消耗事件，观测其发生在 `ABILITY_RESOLVED` 之前还是之中
- **影响**：决策挂起点 #4 的触发时机定义

---

## 7. 本阶段变更

- 新增文档：`PHASE2_MODEL.md`
- 新增空壳类型（**无实现，方法体均 `NotImplementedException`**）：
  - `src/FHEngine/Engine/State.cs`
  - `src/FHEngine/Engine/Events.cs`
  - `src/FHEngine/Engine/Decisions.cs`
  - `src/FHEngine/Engine/IMonsterAI.cs`
- 阶段 1 修正（承你指示）：`monster_cards.json` 的 `Abilities` 改为递归结构；断言 30 → **39**

**下一步：阶段 3（最小战斗循环实现）。请确认 §5 伪代码与 §6 假设清单后开始。**

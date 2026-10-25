# 霜港迷城：修正牌全集与状态效果

> 配套文档：`MODIFIER.md`（修正牌为什么是确定量 / RNG 架构）、`ATTACK.md`（伤害公式）
> 数据来源：反编译代码 + `Base.ruleset` 解包后的原始 YAML，非推测。

## 0. 修正组从哪来：三个来源

一副修正牌堆在任何时刻都由三部分叠加决定：

```
① 基础牌组（Standard / MonsterDeck / ReducedRandomness 房规版）
   ② Perk 改造（玩家用专注点勾选，进入战斗前可变）
      ③ 战斗中的动态增删（Curse / Bless / 移除类能力 / 场景修饰符）
```

**这三层都是可变的**——求解器不能假定牌堆恒为 20 张，也不能假定构成恒定。

---

## 1. 修正牌数据结构

`ScenarioRuleLibrary.YML.AttackModifierYMLData`（`AttackModifierYMLData.cs`）

| 字段 | 类型 | 含义 |
|---|---|---|
| `Name` | string | 唯一名，如 `AMC_Basic_P1`、`Curse`、`Bless` |
| `MathModifier` | string | 数学修正：`"+1"` `"-2"` `"*2"` `"*0"` |
| `AlternateMathModifier` | string | 可切换的第二修正（玩家可手动开关，如元素牌） |
| `Shuffle` | bool | 抽到后置洗牌标记，**回合末才洗**（不是立即洗） |
| `Rolling` | bool | 滚动态：翻出后**继续翻**下一张，直到非 Rolling |
| `AddTarget` | bool | 追加一个目标（AoE 扩展） |
| `IsCurse` | bool | 诅咒牌；抽到即**永久移出牌堆**，不进弃牌堆 |
| `IsBless` | bool | 祝福牌；同上 |
| `IsConditionalModifier` | bool | 条件修正（代码动态生成的牌恒为 true） |
| `IsPersistent` | bool（只读） | `Abilities` 中任一能力的 `Duration == Persistent` |
| `Abilities` | List\<CAbility\> | 附带的实际效果（元素注入、治疗、推击、护盾…） |
| `Overrides` | List\<CAbilityOverride\> | 条件覆盖：在特定状态下替换数学修正 |
| `DamageIfTargetMoves` | int? | 目标移动则造成伤害 |
| `TryToPerformAsSummoner` / `ControlTargetingActor` | bool | 召唤物相关 |

> `ID` 由 `Name` 的确定性哈希得出，不是序号。
> 校验规则：`IsBless` 的牌只能叫 `Bless` / `Reduced_Randomness_Bless`；`IsCurse` 同理。

---

## 2. 标准牌组构成

| 牌组 | 构成 | 张数 |
|---|---|---|
| **Standard**（玩家默认） | +0×6, +1×5, +2×1, −1×5, −2×1, ×0×1, ×2×1 | 20 |
| **MonsterDeck**（怪物默认） | 同上 | 20 |
| **ReducedRandomness**（房规，玩家） | +0×6, +1×5, +2×1, −1×5, −2×1, `−2_Shuffle`×1, `+2_Shuffle`×1 | 20 |
| **MonsterReducedRandomness** | 同上 | 20 |

房规 `ReducedRandomness` 的作用：**把 ×0 与 ×2 换成 ±2_Shuffle**，即抹掉极端倍率。
（数字版特有房规，对应桌游没有的规则。）

---

## 3. 修正牌全集：140 张

`Global/AttackModifiers/` 下共 **140** 张（含 Perk 可添加的牌、元素牌、诅咒/祝福牌）。
可用 `python tools/modifier_catalog.py -v` 复现全量明细。

### 3.1 按数学修正分布

| 修正 | 张数 | | 修正 | 张数 |
|---|---|---|---|---|
| +0 | 69 | | +3 | 3 |
| +1 | 33 | | ×2 | 3 |
| +2 | 17 | | +4 | 2 |
| −1 | 5 | | ×0 | 2 |
| −2 | 4 | | −3 / −4 | 各 1 |

注意：**+0 占了整整一半**。因为绝大多数特殊牌（元素注入、治疗、推击…）都是 +0 伤害附带效果。

### 3.2 标志位

| 标志 | 张数 | 效果 |
|---|---|---|
| `Rolling` | **43** | 翻出后继续翻，一次攻击可消耗多张 |
| `Shuffle` | 4 | 抽到后标记，回合末洗牌 |
| `AddTarget` | 2 | 追加目标 |
| `IsCurse` | 2 | `Curse`（×0）、`Reduced_Randomness_Curse`（−2） |
| `IsBless` | 2 | `Bless`（×2）、`Reduced_Randomness_Bless`（+2） |

### 3.3 附带能力（Abilities）分布

| 能力 | 张数 | 说明 |
|---|---|---|
| `InfuseAbility` | 30 | 注入元素（火/冰/风/土/光/暗） |
| `AHeal` / `HealAbility` | 12 | 治疗 |
| `PushAbility` / `APush` | 5 | 推击 |
| `AShield` / `ShieldAbility` | 5 | 护盾 |
| `TrapAbility` | 2 | 生成陷阱 |
| 其他各 1 | — | `BrittleAbility`、`CreateIceAbility`、`RetaliateAbility`、`DamageAbility`、`PullAbility`、`Disarm`、`ConsumeAbility`、`AddModifierBackToDeck`、`TimeTokenAbility`、`ChooseAbility`、`RefreshItemsAbility`、`AResonance`、`TRevealCard`、`ModeTransferAbility`、`TPressureChange`、`ACreateHazardousTerrain`、`ImproveAbility`、`ControlSummoner`、`ControlAbility`、`AMoveTracker`、`RollingAbility`、`AHealBless` |

### 3.4 条件覆盖（Overrides / Conditions）

部分牌在**特定状态下数学修正会被替换**。出现的条件词频：

`Curse`(6), `Immobilize`(5), `Disarm`(4), `Muddle`(4), `Poison`(4), `Stun`(4), `Wound`(4),
`Invisible`(3), `Regenerate`(2), `Brittle`(1)

示例：`AMC_Curse_P0`（+0）在 `Curse` 条件下替换成 ×0 —— 这正是诅咒牌的实现方式。

---

## 4. 状态效果全集

状态由 `CActor.Tokens`（`CTokens`）持有，枚举定义在 `CCondition.cs`，均为**位标志**。

### 4.1 负面状态 `ENegativeCondition`

| 状态 | 值 | 说明 |
|---|---|---|
| Poison | 1 | 中毒 |
| Wound | 2 | 创伤 |
| Immobilize | 4 | 定身 |
| Disarm | 8 | 缴械 |
| Stun | 0x10 | 眩晕 |
| **Muddle** | 0x20 | 混乱 → **导致 Disadvantage** |
| **Curse** | 0x40 | 诅咒 → **向牌堆塞诅咒牌** |
| Disadvantage | 0x80 | 劣势（直接状态） |
| StopFlying | 0x100 | 禁止飞行 |
| Sleep | 0x200 | 睡眠 |
| Bane | 0x400 | 灾厄 |
| Brittle | 0x800 | 易碎 |
| Impair | 0x1000 | 削弱 |

### 4.2 正面状态 `EPositiveCondition`

| 状态 | 值 | 说明 |
|---|---|---|
| Invisible | 1 | 隐身 |
| **Strengthen** | 2 | 强化 → **导致 Advantage** |
| **Bless** | 4 | 祝福 → **向牌堆塞祝福牌** |
| Advantage | 8 | 优势（直接状态） |
| Immovable | 0x10 | 不可移动 |
| Regenerate | 0x20 | 再生 |
| Ward | 0x40 | 结界 |
| Mutate | 0x80 | 变异 |

---

## 5. 状态如何改变修正组（核心因果链）

### 5.1 Strengthen / Muddle → 改变抽牌张数

`CTokens.GetAdvantageStatus`（CTokens.cs:328）：

```csharp
bool flag  = addAdvantage  || 有 Advantage token || 有 Strengthen;
bool flag2 = addDisadvantage || 有 Muddle || 有 Disadvantage;
if (flag & flag2) return None;      // ★ 同时存在 → 互相抵消，不是比数量
if (flag)  return Advantage;        // 抽 2 张取优
if (flag2) return Disadvantage;     // 抽 2 张取劣
return None;                        // 抽 1 张
```

**要点**：
- Strengthen 与 Muddle **同时存在时结果为 None**（完全抵消），不是比较层数。
- Advantage/Disadvantage 会让**一次攻击消耗 2 张牌**（详见 MODIFIER.md 的抽牌流程）。
- 因此这两个状态不改变牌堆构成，但**改变消耗速率**。

### 5.2 Curse / Bless → 真的往牌堆里塞牌

`CCharacterClass.AddCurseCard` / `AddBlessCard`（CCharacterClass.cs:1560）：

```csharp
// 上限：全队诅咒牌 < 10（玩家）；祝福牌 < 10（玩家 bless 数 + MonsterBlessCount）
CClass.AddCurseCardToDeck(m_AttackModifierCards);
    → deck.Add(CreateCurse());
    → deck.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);   // ★ 立即洗牌
```

| 牌 | 数学修正 | 抽到后 |
|---|---|---|
| `Curse` | ×0 | 永久移出牌堆 |
| `Bless` | ×2 | 永久移出牌堆 |
| `Reduced_Randomness_Curse` | −2 | 同上 |
| `Reduced_Randomness_Bless` | +2 | 同上 |

**三个关键后果**：
1. **牌堆总数会增加**（最多各 10 张）→ 不是恒定量。
2. **加入时立即 `Shuffle(ScenarioRNG)`，消耗随机数** → 这是少数几个会打乱牌序的操作之一。
3. 抽到后**永久移出**（不进弃牌堆）→ 总数又降回来。

`RemoveCurses` / `RemoveBlesses` 可整批移除。

### 5.3 战斗中的动态增删

- `CAbilityRemoveAttackModifier`：一种能力，可在战斗中**按名字移除**修正牌
  （`CCharacterClass.RemoveModifierCards` / `MonsterClassManager.RemoveModifierCards`）。
- `CScenarioModifierAddModifierCards`：场景修饰符，可给牌堆加牌。

→ 修正组在**战斗中也可能变**，不只是进战斗前。

---

## 6. Perk（专注点）改造牌组

`Global/Perks/<职业>/`，**17 个职业共约 180 个 Perk**。

```
Parser: Perk
ID: Drifter01
Name: FH/PERK_Drifter_01_Name
CharacterID: DrifterID
Available: 3                        ← 可勾选次数
CardsToRemove: [AMC_Basic_M1]       ← 从牌堆移除
CardsToAdd:    [AMC_Basic_P1]       ← 加入牌堆
```

各职业 Perk 数：Drifter 13、Deepwraith/PainConduit/Pyroclast 12、Blinkblade/Deathwalker/Frozenfist/Shattersong 11、
BannerSpear/Boneshaper/CrashingTide/HIVE/MetalMosaic/Trapper 10、Snowdancer 9。

运行时入口：`CCharacterClass.ApplyPerk(perk)` / `ApplyPerksToList(...)`。

> 术语说明：这里的 Perk 即你所说的"专注点"。Perk 改造在进入战斗前生效，
> 求解器读到的 `AttackModifierCards` 已是改造后的结果，无需额外处理。

---

## 7. 对求解器的影响（边界条件）

| 现象 | 对预测的影响 |
|---|---|
| 抽牌取 `current[0]`，零 RNG | ✅ 可精确预知（M1 已实证） |
| Rolling 连抽 | 一次攻击消耗多张，预测要按「次」推进 |
| Advantage / Disadvantage | 一次攻击消耗 2 张 |
| Strengthen + Muddle 同时存在 | 抵消为 None，只抽 1 张 |
| 抽到 ×0 / ×2 | 只置洗牌标记，**回合末才洗**（不是立即洗） |
| **加 Curse / Bless** | ❌ **立即 `Shuffle(ScenarioRNG)`，牌序失效**，此后无法静态预知 |
| Curse / Bless 抽到 | 永久移出，牌堆总数下降 |
| 撤销 | 整份 ScenarioState 深拷贝回滚，牌序一并回滚（可复现） |

**结论**：只要不发生"加入 Curse/Bless"，牌序完全可精确预知；
一旦发生，必须把该事件作为预测失效的分界点，之后重新读取牌序。

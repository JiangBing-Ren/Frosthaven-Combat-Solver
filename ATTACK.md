# 霜港迷城 — 攻击（Attack）机制逆向

> 逆向日期：2026-10-22
> 证据来源三层交叉验证：
> - **数据层**：`Base.ruleset` 解包后的 YAML（卡牌 / 修正牌 / 牌组）
> - **代码层**：`Assembly-CSharp.dll`、`ScenarioRuleLibrary.dll`、`ForsthavenRuleLibrary.dll` 标识符
> - **运行层**：`Player.log` 中真实对局的攻击时序与伤害表达式（本次取样 4996 条表达式）

---

## 1. 攻击的生命周期

真实日志时序（取自 `Player.log`，Geminate 对 VermlingScout 的一次攻击）：

| # | 阶段 | 关键消息 |
|---|---|---|
| 1 | 选择焦点目标 | `ActorIsSelectingAttackFocusTargets` |
| 2 | 确认目标 | `ActorSelectedAttackFocus` |
| 3 | 锁定攻防双方 | `attacker: GeminateID` / `defender: VermlingScoutID` / `ability: CAbilityAttack` |
| 4 | 抽修正牌 | `Drawing attack modifiers. OverallAdvantage status is: Disadvantage` |
| 5 | **求值（先算）** | `Attack modifier Expression String: (2)-1  Result: 1` |
| 6 | 卡牌日志 | `AbilityStartUpdateCombatLog`：双生体使用了「心灵尖刺」 |
| 7 | 清场 | `ResetAttackEvents` / `ClearActorsTargetedForVFX` |
| 8 | 进入攻击态 | `ActorIsAttacking` |
| 9 | **抽牌演出** | `StartedDrawingModifier` ×2 → `FinishedDrawingModifier` ×2 → `ActorFinishedDrawingModifiers` |
| 10 | 记录修正 | `LogAttackModifier`：双生体抽到了 **-1** 与 **+2** 攻击修正 |
| 11 | 伤害表现 | `DisplayAttackModifierDamageFlow` → `ShowAttackModifDamage` → `FinalizeFlow` |
| 12 | 结算收尾 | 护盾/穿透/反击结算 → `End Ability` |

**关键结论：伤害在阶段 5 就已算完，阶段 9–11 的抽牌与动画只是表现。**
这是典型的"逻辑先行、表现后补"设计，服务于联机（EOS）同步——
也意味着**改动画不影响伤害结果，改表达式才影响**。

---

## 2. 伤害公式

游戏内部用一条**表达式字符串**表示，并打印到日志。结构为：

```
( 基础攻击值  [+/- 修正 …] )  [ *2 或 *0 ]
```

### 实证表达式（全部取自真实日志）

| 表达式 | 结果 | 说明 |
|---|---|---|
| `((2))` | 2 | 无修正，基础攻击 2 |
| `((2)+1)` | 3 | 加 +1 |
| `((2)-1)` | 1 | 减 -1 |
| `((1)+1)*2` | **4** | **暴击翻倍「含 +1 加成」** |
| `((2))*0` | 0 | 落空归零 |
| `((1))*0-3` | **0** | 落空后 -3，仍为 0（截断） |
| `((1)-1)` | **0** | 减法结果被截断到 0 |
| `((1)-1-3)*2` | **0** | 负结果 ×2 仍为 0 |
| `(((2))*2)*2` | 8 | 多层乘算叠加 |
| `((10))*2` | 20 | 高基础攻击的暴击 |

### 三条铁律

1. **乘算最后应用**：`((1)+1)*2 = 4` —— ×2 会**连同所有加法加成一起翻倍**（含物品、buff、修正牌）。
2. **×0 直接归零**：`((1))*0-3 = 0` —— 落空后不再产生有效伤害。
3. **伤害下限为 0**：任何负结果都被截断到 0，`((1)-1-3)*2 = 0`。

### 2.1 表达式是怎么拼出来的（精确构造顺序）

> 逆向日期：2026-10-24，为 P1 求解器落地而精确化。
> 出处：`GameState.DrawAndApplyAttackModifierCards` 末尾 + `CAttackSummary.CalcExpressionString`。

抽到的每张牌按 `MathModifier` **是否含 `*` 或 `/`** 分成两类，然后按下面顺序拼串：

```csharp
// GameState.cs（DrawAndApplyAttackModifierCards 末尾）
foreach (牌 in 本次抽到的牌)
{
    if (牌.MathModifier.Contains("*") || 牌.MathModifier.Contains("/"))
        summary.PreShieldScalarModifiers.Add(牌.MathModifier);   // 乘除类
    else
        summary.NonScalarMathModifiers.Add(牌.MathModifier);     // 加减类
}
```

```csharp
// CAttackSummary.CalcExpressionString
text  = "(" + 基础攻击力 + OverrideMathModifier + ")"
text += 每个 NonScalar（加减，按抽牌顺序）
if (PreShieldScalar 非空) { text = "(" + text + ")"; text += 每个 Scalar（乘除，按抽牌顺序） }
if (护盾 != 0)            text += "-" + 护盾
result = Max(0, 解析(text))
```

⇒ **可执行的等价算法**（求解器就是这么算的）：

```
v = 基础攻击力
v += 每张加减牌的带符号值        // 按抽牌顺序
for 每张乘除牌（按抽牌顺序）: v = v OP val
v -= 护盾
v = max(0, v)
```

**关键且反直觉的一点：护盾在乘算之后才减。**
`((3))*2` 打护盾 1 的目标是 `6-1=5`，不是 `(3-1)*2=4`。

**乘除牌是左结合、按抽牌顺序依次作用**：两张 ×2 得 `(((2))*2)*2 = 8`。

### 2.2 一次攻击到底抽几张牌

> 出处：`CActor.DrawAttackModifierCards`。

```
while (牌 == null || 牌.Rolling)  牌 = Draw();      // 滚动态进 list3，继续翻
None         : 结果 = list3 + 终止牌
Advantage    : 再 Draw() 一张 → 二选一 → 结果 = list3 + 选中者
Disadvantage : 再 Draw() 一张 → 二选一 → 结果 = [选中者]（**滚动态被丢弃**）
```

- 优劣势的第二张**只翻一张**，不再走 rolling 循环。
- 二选一用 `ChooseCardBasedOnAdvantage`：
  `Parse(100 + 牌.MathModifier)` 与 `Parse(攻击力 + 牌.MathModifier)` 两组值比较，
  优势取大、劣势取小；平局时再看 `HasNegativeEffect` / `HasAdditionalEffect`。
  劣势分支有个特例：`if ((num2 < num4 && num3 != 0) || num == 0) return first;`
  —— **第一张是 ×0 时劣势必定取它**。

---

## 3. 攻击修正牌系统

### 标准玩家牌组（Standard，20 张）

`Global/AttackModifierDecks/Standard.yml`

| 牌 | 数量 | 效果 |
|---|---|---|
| AMC_Basic_P0 | 6 | +0 |
| AMC_Basic_P1 | 5 | +1 |
| AMC_Basic_P2 | 1 | +2 |
| AMC_Basic_M1 | 5 | -1 |
| AMC_Basic_M2 | 1 | -2 |
| **AMC_Basic_T0** | 1 | **×0（落空），Shuffle: True** |
| **AMC_Basic_T2** | 1 | **×2（暴击），Shuffle: True** |

怪物牌组 `MonsterDeck.yml` 构成**完全相同**（仅 `IsMonsterDeck: True`）。

### 洗牌时机

`Shuffle: True` 挂在牌本身。**注意：不是"抽到就洗"。**

抽到 ×0 / ×2 时只**置位** `m_ShuffleAttackModifierCards`
（`CCharacterClass.cs:602` `m_ShuffleAttackModifierCards |= shuffle;`），
**延到回合末**由 `GameState.cs:4929` 的 `CheckAttackModifierCardShuffle()` 才真正洗牌
（前提是 `GameState.ShuffleAttackModsEnabledForPlayers` 打开）。

另有「牌堆抽空」这一条即时洗牌路径：`CActor.DrawAttackModifierCard` 开头
`if (current.Count == 0 && ...) ShuffleAttackModifierCards(...)`。

> ⚠️ 修正记录：本文件原先写"抽到极端牌**立即**洗牌"，经
> `CActor.cs` / `CCharacterClass.cs` / `GameState.cs` 三处代码对证，实为**回合末才洗**。
> 详见 `MODIFIER.md`。

### 优势 / 劣势

| 状态 | 行为 |
|---|---|
| `None` | 抽 1 张 |
| `Advantage` | 抽 2 张，**取较好的一张** |
| `Disadvantage` | 抽 2 张，**取较差的一张** |

**直接证据**：日志 `双生体抽到了 -1 与 +2 攻击修正`，最终表达式为 `(2)-1 = 1`
→ 两张中取了 **-1**（劣势取坏）。

本次日志取样：None 144 次、Disadvantage 39 次（未出现 Advantage 局）。

> 注：Advantage 取好为基于 `AttackAdvantage` / `AttackDisadvantage` 对称实现的推断，
> 与劣势侧的直接证据互为镜像。

### 元素与特殊修正

修正牌命名含元素变体：`AMC_Air_*`、`AMC_Earth_*`、`AMC_Dark_*`、`AMC_Light_*`，
可携带 `AbilityOverrides`（如 `AMC_AddAttack1ForEachAllyAdjToTarget_P1` 会
按"目标 adjacent 的友军数"动态加攻击）。

### 祝福 / 诅咒

代码侧存在 `AddBlessCard` / `AddCurseCard` / `AddBlessCardToEnemies`，
可动态向牌组注入祝福（×2）与诅咒（×0）牌。

---

## 4. 目标选择与判定

- **焦点（Focus）选择**：`ActorIsSelectingAttackFocusTargets` → `ActorSelectedAttackFocus`
- **AI 焦点覆盖**：`AIFocusOverride` / `AIFocusOverridePanel` / `AddFocusOverrideToEnemyEvals`
  → 怪物选目标的行为可被覆盖（房规/AI 设置入口）
- **范围攻击焦点**：`WaitingForAreaAttackFocusSelection`（范围技单独一套选目标流程）
- **路径**：`AllTargetsOnAttackPath`（穿透路径上的所有目标）

### 4.1 射程与视线的确切判据（对 modder 很重要）

> 逆向日期：2026-10-24。`CAbilityAttack.IsActorWithinRangeAndLoS`。

```csharp
public bool IsActorWithinRangeAndLoS(CActor target, int range)
{
    if (CanTargetFromAnywhere(target)) return true;   // 少数能力可无视距离
    CTile from = ScenarioManager.Tiles[施术者.X, 施术者.Y];
    return CActor.HaveLOS(from, targetTile) && IsTileWithinRange(from, targetTile, range);
}
```

⚠️ **`IsTileWithinRange` 走的不是六边形距离，而是寻路长度**：

```csharp
// CAbilityAttack.IsTileWithinRange
path = PathFinder.FindPath(null, 目标格, 施术者格,
           ignoreBlocked: true,  ignoreMoveCost: true,
           ignoreDifficultTerrain: false, ignoreHazardousTerrain: false,
           waterTilesThatCostZero: 0, out found, ignoreDoorLocks);
return path.Count <= range;
```

并且跨 hex map 时会有 `DoorPath` 拼接（两段的 `Count` 相加）。

⇒ **求解器要复刻射程，就该照抄上面这组 `FindPath` 参数，不要用 `Point.HexDist` 顶替。**
（`ignoreBlocked:true` 时两者通常相等，但门/多地图场景会分叉。）

视线是纯几何查询：`CActor.HaveLOS(CTile a, CTile b)` 是 **public static**，
且只依赖两格的位置，与"谁站在那"无关 —— 可以直接拿来计算**假想站位**的视线。

移动可达格同理：`ScenarioManager.PathFinder` 是 **public static**，
`FloodFill_ToRange(actor, 起点, 移动力, ...)` 的起点可以是任意假想坐标。
⇒ 地形/占用/困难地形/危险地形/门/冰面**全部不用自己复刻**，交给游戏算。

---

## 5. 护盾、穿透与反击

| 机制 | 代码证据 |
|---|---|
| 护盾 | `CalculateShield` / `BaseShield` / `ActorCalculateShield` |
| **护盾快照** | **`CachedShieldValue` / `CachedShieldNeutralized`** |
| 穿透 | `AttackEffectPierce` / `CAbilityPierceInvulnerability` |
| **穿透+护盾同算** | **`CalcShieldPierce` / `CalcShieldPierceResult`** |
| 反击 | `ApplyRetaliateToAttack` / `ApplyRetaliateToParentAttack` |
| 中和护盾 | `CachedShieldNeutralized` / `CAbilityNeutralizeShield` |

护盾分多种类型：

```
CShieldActiveBonus_ShieldIncomingAttacks   所有来袭攻击
CShieldActiveBonus_ShieldTargetedAttacks  针对该目标的攻击
CShieldActiveBonus_ShieldAndRetaliate     护盾 + 反击
CShieldActiveBonus_ShieldAndDisadvantage  护盾 + 使攻击者劣势
```

→ 判定护盾是否生效时，要**先匹配护盾类型**，再取值。

---

## 6. 隐藏规则（数字版特有 / 不易察觉）

### 6.1 降低随机性牌组（ReducedRandomness）

`Global/AttackModifierDecks/` 下有一组**标准牌组之外的平行牌组**：

| 牌组 | 差异 |
|---|---|
| `Standard` | 含 ×0 与 ×2 |
| `ReducedRandomness` | **去掉 ×0/×2**，换成 `AMC_Basic_P2_Shuffle`(+2) 与 `AMC_Basic_M2_Shuffle`(-2) |
| `MonsterDeck` | 怪物标准牌组 |
| `MonsterReducedRandomness` | 怪物降低随机性牌组 |

两牌组均标记 `IsReducedRandomnessDeck: True`，且替换牌都带 `Shuffle: True`。

**效果**：彻底消灭"完全落空"与"暴击"两种极端结果，极端值降为 ±2 且抽后立即洗牌
→ 保留修正幅度，但**抹平暴击/落空造成的方差**。

本次取样的日志中**未出现** ReducedRandomness（用户未开启）。它是可选项，
对应 1.0 版加入的「可自定义房规」。

### 6.2 暴击加成翻倍（易被误解）

`((1)+1)*2 = 4` 而非 3 —— **×2 作用于"加成之后"的总值**。
桌上玩家常误以为 ×2 只翻倍基础攻击，数字版明确是**先加后乘**。

### 6.3 伤害下限截断

任何计算为负的结果一律归 0，包括 `((1)-1-3)*2`。

### 6.4 逻辑先行、动画后演

表达式在抽牌动画之前已求值 → 联机同步设计。
对 mod 的含义：**改动画/UI 不影响伤害；只有改表达式或牌组才影响结果**。

### 6.5 护盾是快照

`CachedShieldValue` 表明护盾在一次攻击结算内取快照计算，
而非每次实时求值 → 多重护盾叠加时按快照处理。

### 6.6 落空仍走完整流程

`((5))*0 Result: 0` 的攻击依然完整执行了卡牌日志、抽牌演出、清场流程
→ 落空不影响卡牌消耗与流程结算（符合桌面版：卡牌照常打出）。

---

## 7. 对 mod 的落点

改攻击相关的可操作目标（均在 `.yml`，改后回打包即可）：

| 想改什么 | 文件 |
|---|---|
| 卡牌攻击值 | `Global/Characters/<职业>/AbilityCards/*.yml` |
| 修正牌效果 | `Global/AttackModifiers/AMC_*.yml`（改 `MathModifier`、`Shuffle`） |
| 牌组构成 | `Global/AttackModifierDecks/*.yml`（改各牌数量） |
| 启用降低随机性 | 换用 `ReducedRandomness` 牌组（需确认房规开关位置） |
| 怪物攻击 | `Campaign/Enemies/*.yml` |
| 房规/难度 | `Outpost/GameMode/GameMode_*.yml` |

> 注意：房间规则（GameMode）里 `ScenarioLevels` 控制陷阱/危险地形伤害与经验金币，
> 但**降低随机性开关未出现在 GameMode 的 YAML 字面量中**，推测在设置/房规选项里
> 由 `IsReducedRandomnessDeck` 运行时切换。

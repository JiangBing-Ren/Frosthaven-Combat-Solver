# 霜港迷城 — 攻击修正牌（Attack Modifier）机制深挖

> 目标问题：**为什么实际游玩中修正牌像是"固定"的？哪怕复原到回合开始重新攻击、
> 或者换一张能力牌攻击，翻出来的修正还是同一张。**

---

## 结论（一句话）

**修正牌不是"随机抽"，而是从一张预先排好序的牌堆里按顺序翻牌。翻牌本身不消耗任何随机数。**

牌堆顺序被写进存档；撤销时整份场景状态（牌序 + 全部 RNG 状态）被原样深拷贝回滚。
所以重来一次，翻出的必定是同一张牌。

---

## 证据链

### 证据 1：抽牌代码里没有任何 RNG 调用

`ScenarioRuleLibrary.CActor.DrawAttackModifierCard`（CActor.cs:4639）

```csharp
private static AttackModifierYMLData DrawAttackModifierCard(...)
{
    // 只有牌堆抽空时才洗 —— 这是唯一消耗 RNG 的分支
    if (current.Count == 0 && ((actor is CPlayerActor && GameState.ShuffleAttackModsEnabledForPlayers) || ...))
    {
        ShuffleAttackModifierCards(current, discarded, cardsToUse);
    }

    AttackModifierYMLData card = current[0];   // ← 直接取牌堆顶，零 RNG 消耗
    ...
    current.Remove(card);
    discarded.Add(card);
    return card;
}
```

**抽牌 = 取 `current[0]`。整条路径上没有一次 `Random.Next()`。**

### 证据 2：牌堆顺序是存档里的一等公民

`ScenarioRuleLibrary.AttackModifierDeckState : ISerializable`

| 字段 | 含义 |
|---|---|
| `AttackModifierCardsPool` | 全部卡牌（牌名列表） |
| **`AttackModifierCardsAvailable`** | **牌堆剩余，按翻牌顺序排列** ← 这就是"牌序" |
| `AttackModifierCardsDiscarded` | 弃牌堆 |
| `ShuffleAttackModifierCards` | 待洗标志 |

三个列表都以**明文字符串列表**序列化，牌序因此被完整持久化。

### 证据 3：撤销 = 整份场景状态深拷贝回滚

玩家日志实证：

```
[Host #39] Button Click: 'FastUndo' (Gameplay UI / FH_UI_Button_FastUndo)
Fast Undo at R2_DrifterID_0 11 / 11 to R2_GeminateID_2
CopyScenarioState took 38ms
```

`GloomUtility.CopyScenarioState`：

```csharp
public static async Task<ScenarioState> CopyScenarioState(this ScenarioState source)
{
    ScenarioState copy = await source.DeepCopySerializableObject<ScenarioState>();
    // BinaryFormatter 全量序列化 → 反序列化
}
```

因为 `ScenarioState` 是可序列化的完整对象图，**牌堆顺序随整份状态一起回滚**。

### 证据 4：RNG 状态也一起回滚

`ScenarioState.GetObjectData` 序列化四个独立 RNG：

```csharp
info.AddValue("ScenarioRNGState", ScenarioRNGState);
info.AddValue("EnemyIDRNGState", EnemyIDRNGState);
info.AddValue("EnemyAbilityCardRNGState", EnemyAbilityCardRNGState);
info.AddValue("GuidRNGState", GuidRNGState);
```

`Extensions.RandomState` 存的是 `SharedLibrary.Random` 的 BinaryFormatter 全量字节
（55 槽种子数组 + `inext` + `inextp`），**连"已经消耗到第几个随机数"都存下来了**。

还有一条兜底路径：

```csharp
public void RestoreRNGStates()
{
    m_ScenarioRNG        = new SharedLibrary.Random(Seed);
    m_EnemyIDRNG         = new SharedLibrary.Random(Seed);
    m_EnemyAbilityCardRNG = new SharedLibrary.Random(Seed);
    m_GuidRNG            = new SharedLibrary.Random(Seed);
}
```

四个 RNG 全部由**同一个 int 种子**重建 —— 从同一点回退，洗出来的牌序必定一致。

### 证据 5：玩家日志实证

| 指标 | 数值 | 说明 |
|---|---|---|
| `RNG changed from X to Y` | **10 次 / 4.5 小时** | 全部发生在场景加载时刻 |
| 真实抽牌次数 | 183 次 | 按 `DrawAttackModifiers` 事件计数 |
| 撤销后立刻重打的**紧邻 4 张完全重复** | 3 处 | 直接证明"撤销重来 = 同一张" |
| 跨越 146 次抽牌的 5 张重复块 | 1 处 | 牌序可长期复现 |

紧邻重复实例（第 4 张起，对应 R2 撤销链）：

```
['-1', '+1', '+1', '+1']   →  ['-1', '+1', '+1', '+1']
```

**抽牌 183 次，RNG 只变了 10 次 → 平均 18 次抽牌才消耗 1 次 RNG。**

---

## 完整生命周期

```
进入场景
  └─ ResetAttackModifierDeck()          【消耗 RNG】用 ScenarioRNG 洗一次，牌序就此固定
     └─ 后续所有翻牌都是顺序取用

每次攻击
  ├─ 若牌堆空 → CheckAttackModifierCardShuffle(force:true)   【消耗 RNG】
  ├─ DrawAttackModifierCard → 取 current[0]                  【零 RNG】
  ├─ Rolling 牌：继续抽下一张，直到抽到非 Rolling（仍是顺序取）【零 RNG】
  ├─ Advantage/Disadvantage：抽 2 张按规则取好/取坏          【零 RNG】
  └─ 若翻到 ×0 / ×2 → 置位 m_ShuffleAttackModifierCards（**不立刻洗**）

回合末（GameState.cs:4929）
  └─ CheckAttackModifierCardShuffle()   【仅当标志置位才消耗 RNG】
```

关键点：**×0 / ×2 不是抽到就洗，而是标记后延到回合末才洗。**
这让同一回合内的后续抽牌继续沿用同一份牌序。

### 洗牌真正的实现

```csharp
public static void ShuffleAttackModifierCards(List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, List<AttackModifierYMLData> inUse = null)
{
    // 1) 弃牌逐张随机插回（消耗 RNG）
    while (discarded.Count > 0)
    {
        var item = discarded[0];
        discarded.Remove(item);
        current.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, current.Count), item);
    }
    // 2) 再整体 Fisher-Yates 一遍（消耗 RNG）
    current.Shuffle();
}
```

RNG 消耗量 = `弃牌数 + (牌堆数 - 1)`。

---

## 三条可直接验证的推论

1. **撤销到回合起点后再攻击 → 翻出同一张。**
   牌序与 RNG 状态都回滚了。

2. **撤销后换一张能力牌攻击 → 还是同一张。**
   牌堆位置回到 `current[0]`，跟你用哪张能力牌无关。

3. **不撤销、在同一回合里连着打第二下 → 会翻到下一张（不一样）。**
   因为不再洗牌，只是顺序推进。

---

## 对战斗求解器的意义（重大）

### 修正牌是**确定量**，不是随机变量

求解器**不需要算伤害期望**，可以直接**精确预知**这次攻击会翻出哪张牌。

| 求解器要读的东西 | 位置 |
|---|---|
| 玩家/召唤物牌堆 | `CCharacterClass.m_AttackModifierCards` |
| 怪物牌堆 | `CMonsterAttackModifierDeck.m_AttackModifierCards` |
| 待洗标志 | `CCharacterClass.m_ShuffleAttackModifierCards` |
| RNG 当前状态 | `ScenarioManager.CurrentScenarioState.ScenarioRNG` |

- **本次攻击翻出的牌** = `m_AttackModifierCards[0]`，精确、无概率。
- **本次攻击后的牌** = `[1]`、`[2]`……可直接推演多段攻击（AoE / 多目标）的整串修正。
- **翻到 ×0/×2 后**本回合后续攻击不受影响，回合末才洗。
- **需要模拟洗牌时**：用 `Extensions.Save()/Restore()` 克隆一份 RNG（游戏自己的 `PeekScenarioRNG` 就是这么干的），
  在克隆体上跑洗牌算法，不污染真实状态。

### 顺带发现的两个现成后门

| 后门 | 用途 |
|---|---|
| `CAbilityAttack.ConsoleVar_DebugAttackModifierCard` | 控制台变量，**直接指定本次翻出哪张修正牌**，调试神器 |
| `CAbilityShowNextAttackModifier` / `CAbilitySetNextAttackModifiers` | 游戏内已实现"显示下一张修正牌"能力，可参考其数据通路 |

---

## 隐藏规则补充

1. **回合末才洗牌**：抽到 ×0 / ×2 只置位不洗牌，延到回合末统一处理。
   这是玩家能在一个回合内"心算后续修正"的基础。
2. **全场只有 2 张牌会触发洗牌**：Standard 牌组里就是 `AMC_Basic_T0`(×0) 与 `AMC_Basic_T2`(×2)，
   二者 YAML 里都标了 `Shuffle: True`。
3. **ReducedRandomness 牌组不是"不洗"，而是"洗得温和"**：
   把 ×0 / ×2 换成了 `AMC_Basic_M2_Shuffle` / `AMC_Basic_P2_Shuffle`（±2 且洗牌），
   保留 2 张洗牌触发器，只是去掉了极端结果。
4. **初始建堆用的是有偏插入洗牌**：
   `Insert(RNG.Next(0, count), item)` 的位置区间是 `[0, count)`，**永远排除"追加到末尾"**，
   不是无偏 Fisher-Yates。建堆后若要重洗则走 `Extensions.Shuffle()`（正牌 Fisher-Yates）。
5. **日志里的 `*2` 不都是 ×2 牌**：`CalcExpressionString` 中 `if (Brittle) text += "*2";`
   —— **易碎状态**也会在表达式尾部追加 `*2`，与 ×2 牌同形，读日志务必注意。
   另有 `Ward` → `floor(...)/2`。
6. **Rolling 修正牌会连抽**：`while (card == null || card.Rolling)` 循环，
   上限 200 张；`TreatFirstAttackModifierAsRolling` 可把首翻强制造为 Rolling。
7. **RNG 本体是 .NET Framework `System.Random` 的逐行拷贝**：
   Knuth 减法生成器，56 槽状态 + 双指针，见 `SharedLibrary.Random`。
   序列化时由 `RandomSerializationBinding` 把 `System.Random` 重定向到 `SharedLibrary.Random`。

---

## 牌组定义（Global/AttackModifierDecks）

| 牌组 | 构成 | 张数 |
|---|---|---|
| **Standard**（玩家默认） | +0×6, +1×5, +2×1, −1×5, −2×1, ×0×1, ×2×1 | 20 |
| **MonsterDeck**（怪物默认） | 同上 | 20 |
| **ReducedRandomness**（房规） | +0×6, +1×5, +2×1, −1×5, −2×1, −2_Shuffle×1, +2_Shuffle×1 | 20 |
| **MonsterReducedRandomness** | 同上 | 20 |

---

## 附：RNG 架构速查

```
SharedLibrary.Random            ← .NET Framework System.Random 拷贝（55 槽 + inext/inextp）
  ├─ Extensions.Save()          → RandomState(byte[])   BinaryFormatter 全量序列化
  ├─ Extensions.Restore()       → 反序列化出新实例（克隆，不消耗原状态）
  └─ PeekScenarioRNG            = Save().Restore().Next()   ← 克隆后再取，不污染本体

ScenarioState
  ├─ ScenarioRNGState / EnemyIDRNGState / EnemyAbilityCardRNGState / GuidRNGState
  ├─ Seed（单一 int，来自地图或 SharedClient.GlobalRNG）
  ├─ RestoreRNGStates()         四 RNG 由同一 Seed 重建
  └─ RandomiseRNGOnLoad()       "载入时重随"房规开关，开启则读档即换种子
```

> **注意**：`PeekScenarioRNG` 是先克隆再取数，所以日志里的 `[ScenarioRNG] - X`
> 不代表 RNG 被消耗。读日志时不要被它误导。

---

## 附：抽牌流程实证（2026-10-23 代码核对 + 实机验证）

`CActor.DrawAttackModifierCard`（CActor.cs:4630）单次抽牌的完整动作：

```csharp
if (current.Count == 0 && 允许洗牌) ShuffleAttackModifierCards(current, discarded, cardsToUse);
AttackModifierYMLData card = current[0];      // ← 取首张，零 RNG 消耗
if (card.IsCurse || card.IsBless || card.IsPersistent) {
    /* 只打日志，不进弃牌堆 → 该牌被永久移出牌堆，总数 -1 */
} else {
    discarded.Add(card);
}
current.Remove(card);                          // ← current → discard，总数守恒
```

### 一次攻击可能消耗多张牌

外层 `DrawAttackModifierCards`（CActor.cs:4588）：

1. **Rolling 连抽**：`while ((card == null || card.Rolling) && list.Count < 200)`
   —— 翻出 Rolling 牌会继续翻，直到非 Rolling 为止。带 Rolling 修饰的攻击一次可消耗多张。
2. **Advantage 抽 2 张**：`EAdvantageStatuses.Advantage` 分支再调一次 `DrawAttackModifierCard`，
   两次结果交给 `ChooseCardBasedOnAdvantage` 择优（受 `GameState.ChooseAdvantageModifiers` 控制是否让玩家手选）。
   Disadvantage 同理抽 2 张取劣。
3. **Advantage 第二张的落点不同**：第一次调用传 `cardsToUse=cardsDrawn=list2`，
   Advantage 第二次传 `cardsToUse=list, cardsDrawn=list2`；未被采用的那张进 `notUsed`。

### 对求解器的三条影响

- **预测必须按「次」而非按「攻击」推进**：Rolling 与 Advantage 会让一次攻击消耗 2 张以上。
- **牌堆总数不是恒定量**：Curse / Bless / Persistent 抽到即永久移出，总数会下降；
  洗牌时 `discarded` 合并回 `current`，总数恢复。
- **预测与比对要用 Name，不能用引用**：若 `actor.TreatFirstAttackModifierAsRolling()` 为真，
  方法内会 `CreateRollingAttackModifierCopy` 返回副本，引用不等但 Name 相同。

---

## 附：单位集合的正确用法（易错）

| 属性 | 含义 | 求解器该用哪个 |
|---|---|---|
| `Scenario.AllMonsters` | 存活 **+ 已死**（`Enemies + DeadEnemies + Ally + DeadAlly + Enemy2 + DeadEnemy2 + Neutral + DeadNeutral`） | ✘ 会混入已死单位（表现为负血条目） |
| `Scenario.AllAliveMonsters` | 只含存活 | ✔ 目标选择、威胁评估一律用它 |
| `CActor.IsDead` | `CauseOfDeath != ECauseOfDeath.StillAlive` | 需要逐个判断时用 |

**唯一标识是 `CActor.ActorGuid`，不是 `StandeeID`。**
同一怪物类的多个 standee 会重号（实测快照里 `VermlingScoutID#3` 同时出现两次，坐标不同），
按 `ClassID + StandeeID` 定位目标会错位。

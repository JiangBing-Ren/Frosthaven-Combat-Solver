# 霜港迷城：NPC 行动牌机制与可预测性

> 适用范围：**敌人（Enemy）、盟友（Ally）、中立（Neutral）**——三者都是 `CEnemyActor`，
> 共用同一套 `CMonsterClass` 行动牌机制，规则完全一致。
> 配套：`ENEMY_AI.md`（选敌 / 移动 / 攻击）、`MODIFIER.md`（修正牌确定量论证）。

## 0. 结论先行

**NPC 行动牌与玩家修正牌是同构机制：牌序明文存储、取 `[0]`、零 RNG。**

因此怪物意图**完全可预测**，且不止能预测下一回合——能一路预测到下一次洗牌为止，
甚至连"还剩几回合会洗牌"都可以提前算出。

| 问题 | 答案 |
|---|---|
| 行动牌固定吗？ | **在一次洗牌周期内完全固定**（顺序即 `AbilityCards` 列表顺序） |
| 遵循什么规则？ | 顺序抽取；牌堆空或抽到 `Shuffle` 标记牌时重洗 |
| 可预测吗？ | **是**。可预测到下次洗牌；洗牌点也可提前定位 |
| 洗牌后可预测吗？ | 需复刻 `EnemyAbilityCardRNG`（可行，见 §7）；且游戏会把洗牌后牌序打进日志 |

---

## 1. 数据结构

`CMonsterClass.cs` 中四层状态：

| 成员 | 含义 |
|---|---|
| `m_AbilityCardsPool` | 牌池（YML 定义的完整牌组，恒定） |
| `m_AbilityCards` | **当前牌堆**，`[0]` 即下一张 |
| `m_DiscardedAbilityCards` | 弃牌堆 |
| `m_RoundAbilityCard` | 本回合已选出的牌 |
| `NextAbilityCard` | `AbilityCards.Count > 0 ? AbilityCards[0] : null` |
| `NextAbilityCardKnown` | 玩家是否**被允许看到**下一张（需 `ShowNextCard` 能力） |

> `NextAbilityCardKnown` 是 UI 展示权限（数字版便利功能），
> 与"求解器能否读出"无关——求解器始终能读到。

---

## 2. 选牌流程（零 RNG）

`CMonsterClass.SelectRoundAbilityCard()`（CMonsterClass.cs:646）：

```csharp
if (m_AbilityCards.Count == 0 && m_RoundAbilityCard == null)
    ResetAbilityDeck();                              // 牌堆空 → 重洗
if (m_AbilityCards.Count > 0 && m_RoundAbilityCard == null)
    SetRoundCard(m_AbilityCards[0]);                 // ★ 取首张，无 RNG
```

`SetRoundCard`（:667）：

```csharp
m_RoundAbilityCard = card;
m_AbilityCards.Remove(card);
m_AbilityCards.RemoveAll(同 ID 的牌);                // 连同名重复牌一起移走
```

回合结束时 `DiscardRoundAbilityCard()`（:679）：

```csharp
m_DiscardedAbilityCards.Add(m_RoundAbilityCard);
if (m_RoundAbilityCard.Shuffle) ResetAbilityDeck();  // ★ Shuffle 牌 → 立即重洗
m_RoundAbilityCard = null;
```

**注意**：`SetRoundCard` 会把**所有同 ID 的牌**移出牌堆。所以牌堆里同一张牌不会连续出现两次。

---

## 3. 洗牌规则

`ResetAbilityDeck()`（CMonsterClass.cs:925）——仅此一处消耗 RNG：

```csharp
m_RoundAbilityCard = null;
while (m_DiscardedAbilityCards.Count > 0) { ...; m_AbilityCards.Add(card); }   // 弃牌全部收回
if (AbilityCards.Count == 0) m_AbilityCards.AddRange(AbilityCardsPool);
m_AbilityCards.Shuffle(ScenarioManager.CurrentScenarioState.EnemyAbilityCardRNG);
Log.SimpleInfo("[Round N] ENEMY CARD:<ID> ability deck shuffled:\nAbility Cards:\n<逐张名字>");
```

**两个触发条件**：

1. 牌堆为空且无回合牌（`SelectRoundAbilityCard` 开头）
2. 弃掉一张 `Shuffle: True` 的牌（`DiscardRoundAbilityCard` 内）

**洗牌用的 RNG 是 `EnemyAbilityCardRNG`** —— 即 `MODIFIER.md` 记录的四个 RNG 之一。
**所有怪物类型共用这一个 RNG**，因此 A 类怪物洗牌会推进共享 RNG，进而影响 B 类怪物之后的洗牌结果。
（但由于洗牌后我们可直接读 `AbilityCards` 顺序，通常**不需要**模拟它。）

> 游戏自己会把洗牌后的完整牌序打到日志里——这是验证预测的现成真值来源。

---

## 4. 共享牌池：预测粒度是「类型」而非「个体」

`CMonsterClass` 是**类型级**对象，不是单体。同类型的所有 standee 共享同一张 `RoundAbilityCard`——
与桌游规则一致（同类怪物同回合执行同一张行动牌，先攻相同）。

进一步，`CardsPoolClass`（CMonsterClass.cs:52）允许**跨类型共享**：

```csharp
if (!string.IsNullOrEmpty(m_MonstersYML.CardsPoolClass))
    return MonsterClassManager.Find(m_MonstersYML.CardsPoolClass);   // 转发到别的类
```

且 `AbilityCards` / `DiscardedAbilityCards` / `RoundAbilityCard` 三个属性**都转发到 `CardsPoolClass`**。

实例：`LadyInWhiteElite.yml` 里 `CardsPoolClass: IceWraithID`——精英变体用的是 IceWraith 的牌池。

→ **求解器按「怪物类型」预测意图即可，不必逐个体预测**，预测单元数量大幅下降。

---

## 5. YML 结构

### 怪物类（`Campaign/Enemies/.../*.yml`）

```yaml
Parser: Enemy
ID: VermlingScoutID
MonsterType: Monster
StandeeLimit: 8                       # 最多 8 个 standee（实测快照 Scout#1~#8 吻合）
AbilityCards: [770,771,772,773,774,775,776,777]     # 8 张行动卡
Level0: { Health: 2, Move: 3, Attack: 1, Range: 1, Target: 1 }
Level1: { Health: 3, Move: 3, Attack: 1, ... }      # 属性按等级缩放
```

### 行动卡（`Cards/*.yml`）

```yaml
Parser: EnemyCard
ID: 99996
Name: $Monster_Ability_GuidedAdvance$
Initiative: 99        # 先攻值写在牌上 → 选牌前先攻不可知
Shuffle: True         # 抽到后重洗牌堆
Data:
    Abilities: ...
```

---

## 5.1 先攻：可提前读出，不是「不可知」

`CEnemyActor.Initiative()` → `CMonsterClass.Initiative(actor, includeOverrideCard)`（:1304）：

```csharp
int initiativeBonus = actor.GetInitiativeBonus();
CMonsterAbilityCard card = RoundAbilityCard;
if (includeOverrideCard && actor.OverrideRoundAbilityCard != null)
    card = actor.OverrideRoundAbilityCard;
if (!flag && card != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
    num = card.Initiative;                       // 先攻来自行动卡
return num + initiativeBonus;
```

**快照里先攻恒为 0 的原因是第一行那个阶段门控**：不到
`CPhase.PhaseType.MonsterClassesSelectAbilityCards` 阶段就返回 `0 + bonus`。
这是"当前回合先攻尚未生效"，**不是"不可知"**。

正确的读法是绕开 `Initiative()`，直接读牌：

```
AbilityCards[0].Initiative      ← 下一回合的先攻，选牌阶段前就可读
```

**先攻 = 行动卡 `Initiative` + `GetInitiativeBonus()`**，其中 bonus 来自
`Class.TempInitiativeBonus` 与所有 `AdjustInitiative` 类 ActiveBonus 之和。

### 两个会让先攻偏离牌堆预测的例外

1. **`OverrideRoundAbilityCard`** —— 可被脚本强制指定一张行动卡。
   实例：`BossHasMonsterCardExtraTurnScenarioScript`（Boss 额外回合）会临时把 Boss 的行动卡
   换成 `_bossCard`，回合结束再置 null。此时实际先攻 ≠ `AbilityCards[0].Initiative`。
2. **`MonsterClassIDToActImmediatelyBefore`** —— 配置了该字段的怪物类型会
   **跟随另一个类型的先攻**（取那个类型 `RoundAbilityCard.Initiative`），而不是自己的牌。

求解器在读先攻时应同时检查这两个字段，否则会对 Boss 与联动型怪物给出错误排序。

---

## 6. 可预测性边界

| 能预测 | 不能（或需要额外工作） |
|---|---|
| 剩余牌堆的完整顺序 | 洗牌**之后**的新顺序（需复刻 RNG） |
| 下一次洗牌发生在第几回合 | — |
| **先攻**（读 `AbilityCards[0].Initiative`，选牌前即可知） | 被 `OverrideRoundAbilityCard` 覆盖时的先攻（Boss 额外回合） |
| 每次行动的具体内容（移动/攻击/数值） | — |

**"还剩几回合洗牌"怎么算**：扫描当前 `AbilityCards`，找到第一张 `Shuffle == true` 的牌的下标 `i`
→ 还有 `i + 1` 次选牌后洗牌（若整堆都无 Shuffle 牌，则抽空时洗）。
这让求解器能明确标注"本预测有效期到第 N 回合"。

---

## 7. 洗牌后顺序为何仍可能预测

`EnemyAbilityCardRNG` 的类型是 `SharedLibrary.Random`——
按 `MODIFIER.md` 的论证，它是 .NET Framework `System.Random` 的逐行拷贝（Knuth 减法生成器，56 槽 + 双指针），
**可离线复刻**。且它有 `Save()/Restore()`（`RandomState(byte[])`），状态随存档持久化。

因此理论路径成立：读取当前 RNG 状态 → 按 `List.Shuffle` 的实现重放洗牌 → 得到洗牌后顺序。
代价是要精确复刻 `Shuffle` 扩展方法的实现（大概率是 Fisher-Yates）。

**但实践中不必如此**：洗牌时刻与洗牌后的顺序都能直接观测（读 `AbilityCards`、读日志）。
只有在"提前很久预测洗牌后顺序"的场景才需要复刻 RNG——而该场景对单回合最优求解并无必要。

---

## 8. 对求解器（M3）的意义

1. **意图读取入口**：`CMonsterClass.RoundAbilityCard`（本回合）、`NextAbilityCard`（下一张）。
   同类共享，按类型读取即可。
2. **先攻**：不要调用 `Initiative()`（受阶段门控，选牌前恒返回 0），
   应直接读 `NextAbilityCard.Initiative + actor.GetInitiativeBonus()`，
   并额外检查 `OverrideRoundAbilityCard` 与 `MonsterClassIDToActImmediatelyBefore` 两个例外。
3. **多回合推演**：可直接把 `AbilityCards` 顺序展开成未来 N 回合的行动序列（N 由 Shuffle 牌位置决定）。
4. **验证成本极低**：游戏把洗牌后牌序写进日志，可与插件读数直接对照。

### 待实测验证

- 一次实战中连续记录 `NextAbilityCard` 与实际 `RoundAbilityCard`，断言逐次一致（类比 M1.5）。
- 确认 `StandeeLimit` 与场上实际 standee 数的对应关系。
- 确认中立怪物是否走同一选牌流程（代码层面一致，实战待观察）。

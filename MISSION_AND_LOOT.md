# 霜港迷城：使命（关卡）表、装备表、药水表、战利品可预测性

> 用户确定的优先级链：
> **使命目标 > 捡拾装备/宝箱 > 专长任务(Mastery) > 巅峰战绩(BattleGoal) > 获取经验 > 捡拾战利品袋(材料)**
>
> 本文调查该优先级链涉及的全部数据表，并回答「地上的战利品袋在捡拾前是否可知」。

---

## 1. 使命（关卡）表

### 1.1 载体

- 关卡数据：**`Campaign/Scenarios/LevelEditorScenarios/*.lvldat`，共 149 个**（.NET BinaryFormatter）
- 实测场景 ID 形如 `RFS_07_Assault_On_The_Warchief`（M1 快照中确认）
- 场景级 YML 模板另见 `CustomScenarios/SingleScenarioYML/SandboxScenario.yml`

### 1.2 使命目标类型：22 种

目标类型以 `CObjective_*` 类实现，`SRL/ScenarioRuleLibrary/` 下共 22 个：

| 类别 | 目标类型 | 说明 |
|---|---|---|
| **歼灭** | `KillAllEnemies` | 消灭全部敌人 |
| | `KillAllBosses` | 消灭全部 Boss |
| | `DealXDamage` | 累计造成 X 伤害 |
| | `DamageQuota` | 达成伤害额度 |
| | `XCharactersDie` | X 个角色死亡（**负面目标**，通常是失败条件） |
| **拾取** | `LootX` | 拾取 X 个战利品 |
| | `AllCharactersMustLoot` | 所有角色都必须拾取过 |
| | `BringItemsToArea` | 把物品带到指定区域 |
| **位置** | `ActorReachPosition` | 指定角色到达某格 |
| | `AnyActorReachPosition` | 任一角色到达某格 |
| | `ActorsEscaped` | 角色撤离 |
| | `ActorsNotInAllRooms` | 角色不得进入全部房间 |
| | `RevealAllRooms` | 揭示全部房间 |
| **交互** | `ActivatePressurePlateX` | 激活 X 个压力板 |
| | `ActivateXProps` | 激活 X 个物件 |
| | `ActivateXSpawners` | 激活 X 个刷怪点 |
| | `DeactivateXSpawners` | 停用 X 个刷怪点 |
| | `DestroyXObjects` | 摧毁 X 个物件 |
| **条件** | `ReachRound` | 坚持到第 X 回合 |
| | `XActorsHealToMax` | X 个角色治疗至满血 |
| **特殊** | `CustomTrigger` | 自定义触发 |
| | `Scripted` | 脚本控制 |

### 1.3 目标对象的关键字段（`CObjective.cs`）

这些字段直接决定目标函数怎么算：

| 字段 | 含义 | 对求解器的意义 |
|---|---|---|
| `ObjectiveType` | 类型（22 种） | 决定评估谓词 |
| `IsComplete` | 是否完成 | 主目标判定 |
| `UnableToComplete` | 是否已无法完成 | 可提前剪枝 |
| **IsOptional** | **是否可选目标** | 区分主/次目标 |
| **EnoughToWinAlone** | **单独完成即算通关** | 存在多个目标时，只需达成其一 |
| **WinsDespiteExhaustion** | **力竭仍算赢** | 可放心耗尽手牌 |
| `ActivateOnRound` / `IsActiveFromStart` | 生效回合 | 时序规划 |
| `RequiredObjectiveStates` | 依赖的其他目标状态 | 目标间有先后顺序 |
| `IsHidden` | 是否隐藏 | 隐藏目标不影响当前决策 |
| `ObjectiveFilter` | 目标过滤条件（怪物类型/角色等） | 决定哪些单位计入 |

> 注意 `EnoughToWinAlone` 与 `RequiredObjectiveStates`：
> 多个目标之间可能是"任一即可"或"有依赖"的关系，
> 求解器不能简单假设"必须完成所有非可选目标"。

---

## 2. 装备表

`Global/ItemCards/*.yml`，共 **266 张**。

### 2.1 按部位（Slot）分布

| Slot | 数量 |
|---|---|
| SmallItem | 74 |
| OneHand | 62 |
| Head | 38 |
| Body | 32 |
| Legs | 30 |
| TwoHand | 27 |
| QuestItem | 3 |

### 2.2 稀有度

| Rarity | 数量 |
|---|---|
| Rare | 253 |
| Relic | 13 |

### 2.3 关键字段

```yaml
Parser: ItemCard
ID: 1001
StringID: ScaledArmorID
Name: $FH/1001_item_cards_scaled_armor_1$
TotalInGame: 2          # 全局数量上限
Cost: 10                # 购买价格
Slot: Body
Rarity: Rare
Usage: Consumed         # 消耗方式
Trigger: PassiveEffect  # 触发时机（PassiveEffect / DuringOwnTurn）
Data:
    Abilities:
        AShield: { ... }
```

求解器关注：`Slot`（能否装备）、`Trigger`（何时生效）、`Usage`（一次性/持续）、`TotalInGame`（稀缺度）。

---

## 3. 药水表

**27 个**，均属于 `Slot: SmallItem`，命名带 `Potion`。

已知样例：`HealingPotion`(1031)、`StaminaPotion`(1033)、`PowerPotion`(1035)、`ElementPotion`(1037)、
`CurePotion`(1039)、`FireshieldPotion`(1041)、`StoneskinPotion`(1043)、`MusclePotion`(1045)、
`RenewingPotion`(1049)、`GlancingPotion`(1051) …

```yaml
ID: 1031
Slot: SmallItem
Cost: 0
Usage: Consumed
Trigger: DuringOwnTurn      # 自己回合内主动使用
Data:
    Abilities:
        AHeal: { Heal: 3, Filter: Self }
```

药水在数值上是**小物品**，但战术价值（救命/续航）远高于同等材料，
建议在评分里单独加权，而不是按 `Slot` 一视同仁。

---

## 4. 地上的战利品袋：捡拾前**完全可知**

这是本次调查最有价值的一条。

### 4.1 战利品牌堆是明文索引序列

捡拾走 `CActor.TryLootFromDeck` → `CLootDeck.DrawNext`（CLootDeck.cs:467）：

```csharp
public static bool DrawNext(List<byte> deck, out LootCard lc)
{
    if (deck.Count == 0 || deck[0] == 0) { lc = default; return false; }
    lc = AllCards[deck[0]];      // ★ deck[0] 是 AllCards 的下标
    deck.Add(deck[0]);           // ★ 抽到的牌放回末尾（循环，不是进弃牌堆）
    deck.RemoveAt(0);
    return true;
}
```

- 牌堆 = `ScenarioState.LootDeck`，类型是 **`List<byte>`**
- `deck[0]` 就是下一次捡拾会抽到的牌
- `0` 是分隔符（delimiter），`GetDelimiterID` 即 `IndexOf(0)`
- **抽完放回末尾** → 牌堆循环使用，不会耗尽

⇒ **求解器只要读 `LootDeck[0]`（乃至 `[0..n]`），就能在捡拾前知道会拿到什么**，
并且能预判接下来若干次捡拾的收益。

### 4.1.1 内容何时决定 —— 袋子本身不存内容

`CObjectGoldPile`（地上金币堆）是 `CObjectProp` 的**空壳子类**，
没有任何携带战利品的字段，`AutomaticActivate` 恒返回 false。
地上物件只有 `ObjectType`（`MoneyToken` / `Chest` / `GoalChest` / `CarryableQuestItem` / `Resource`）。

⇒ **内容是"捡起来那一刻"才决定的**（走 `DrawNext` 抽全局牌堆）。
但牌堆顺序由 `CLootDeck.Generate` 在**进关卡时**就洗定并持久化：

```csharp
// CLootDeck.Generate：按类型分组 → 每组 Shuffle(rng) → 按 scenarioLootTable 取量 → 组牌
// RandomScenarioLootTable(rng)：决定「每种类型各放几张」
```

⇒ **第 n 次捡拾会得到什么，在进关卡时就已经确定**，且随存档走。

### 4.1.2 捡拾是逐格顺序进行的

`CAbilityLoot` 遍历 `m_TilesSelected`（格子选择顺序），每格取全部 token，
逐个走 `LootTile` → `TryLootFromDeck`：

```csharp
for (int i = 0; i < m_TilesSelected.Count; i++)
{
    List<CObjectProp> list = cTile.m_Props.FindAll(x => x.ObjectType == MoneyToken | Chest | ...);
    LootCount += list.Count;
    if (!m_AllTargets && LootCount >= NumberTargets) num = i;   // 达到上限即停
    if (targetingActor.LootTile(cTile, ...)) m_PropsLooted.AddRange(list);
}
```

⇒ 拿到的依次是 `deck[0]`, `deck[1]`, `deck[2]`…，顺序由**捡拾顺序**决定。

### 4.1.3 唯二的"挑"机制

**(a) `LootCardAdvantage`：抽 2 选 1**

```csharp
if (flag && !endOfScenario)                       // flag = HasLootCardAdvantage
{
    if (CLootDeck.DrawNext(LootDeck, out var lc2))
    {
        List<LootCard> list2 = { lc, lc2 };
        if (GameState.ShowLootCardSelection(this, list2, out var selectedLootCard))
        {
            foreach (LootCard item3 in list2.Where(x => x != selectedLootCard))
            {
                CLootDeck.RemoveCardFromDeck(LootDeck, item3);
                CLootDeck.AddCardBeforeDelimiter(LootDeck, item3);   // ★ 未选的放回分隔符前
            }
            lc = selectedLootCard;
        }
    }
    foreach (PlayerState p in CurrentScenarioState.Players) p.SetLootCardAdvantage(false);
}
```

抽两张、玩家选一张，**未选的放回牌堆分隔符之前**（下次还会抽到）。
来源：`PlayerState.HasLootCardAdvantage`，部分道具/能力可给予。

**(b) `RandomItem` 兑现的是关卡预设的装备，不是当场随机**

```csharp
if (type == LootCard.EType.RandomItem)
{
    if (!scenarioSetup.RandomItemLooted)
    {
        if (scenarioSetup.NextRandomItemID > 0)
            TryLootItem(scenarioSetup.NextRandomItemID);      // ★ 用预设 ID
        else { drawnLoot.NumEmptyRandomItemDraws++; ... }     // 已发光则退化为金币
    }
}
```

→ 具体哪件装备由 `ScenarioSetup.NextRandomItemID` 决定，**可读、可预知**；
且每关只有**第一次**抽到 `RandomItem` 才给装备，之后退化为金币。

### 4.2 战利品类型（`LootCard.EType`）

`None, Money, Lumber, Metal, Hide, Arrowvine, Axenut, Corpsecap, Flamefruit, Rockroot, Snowthistle, **RandomItem**`

- 前 4 类是建造资源（木/金属/皮）+ 钱
- 中间 6 类是炼金草药
- **`RandomItem`：会开出随机装备** ← 这正是"捡拾战利品袋也可能拿到装备"的来源

数量由 `EAmount` 决定（`Plus1`/`Plus2`/`Plus3`/条件型如 `Plus1if4_Plus2if23`）。
条件型依赖玩家人数，求解器需要一并处理。

### 4.3 对优先级链的直接结论

因为牌序可知，求解器可以**精确判断"绕路去捡这个袋子值不值"**：

- 若 `LootDeck[0]` 是 `RandomItem` → 等价于装备，按你的优先级应**升到第二档**
- 若是 `Money`/材料 → 保持最末档，除非顺路
- 可预先算出"再捡 N 次能拿到什么"，从而规划捡拾顺序

### 4.4 能否「从多个袋子中精准捡到想要的」

**结论：不能跳序挑，但可以精准规划。**

| 问题 | 答案 |
|---|---|
| 袋子里有啥何时定？ | **捡起来的那一刻**（抽全局牌堆）；袋子本身不存内容 |
| 但牌序何时定？ | **进关卡时就洗定**并持久化 → 第 n 次捡拾的结果早已确定 |
| 挑袋子有用吗？ | **没用**。所有袋子共享同一牌堆，捡哪个袋子不影响抽到的内容，只影响顺序 |
| 能跳序拿吗？ | **不能**。只能按 `deck[0], deck[1], deck[2]…` 顺序拿 |
| 那还能"精准"吗？ | **能**——靠预知：读 `LootDeck[0..n]`，决定**捡几个**、**让谁捡**，从而让目标物品落在想要的角色手里 |
| 唯一的"挑" | `LootCardAdvantage`：抽 2 选 1，未选的**放回分隔符前**（下次还能抽到） |

**实操含义**：假设你只想要第 4 张（一件 `RandomItem` 装备），
那么必须先消耗掉前 3 张（捡掉 3 个袋子）才能轮到它——
这 3 次捡拾是否值得、由谁去捡、顺不顺路，全都可以提前算清楚。

---

## 5. 宝箱

- 类 `CObjectChest`，字段 `ChestTreasureTablesID`（战利品表 ID 列表）
- 内容按箱子**序号预设**（枚举形如 `GainRandomItemBlueprint_03`、`UnlockRandomSideScenario_09`）
- 部分宝箱给"随机物品蓝图"——**类型可知（会给装备），具体哪件不可知**
- 另有相当一部分宝箱的收益是**解锁支线场景**（`UnlockRandomSideScenario_*`），属于战役层价值

⇒ 对求解器：宝箱**恒定属于"装备/宝箱"这一档**（第二优先级），
因为无论哪种，收益都高于材料。这一点无需预测具体内容即可决策。

---

## 6. 优先级链 → 代码映射

| 优先级 | 数据来源 | 可读性 |
|---|---|---|
| 1 使命目标 | `ScenarioState.Objectives`（`CObjective`） | ✅ 可读 `IsComplete` / `UnableToComplete` / `IsOptional` |
| 2 捡拾装备/宝箱 | 场上 `CObjectChest` 与道具；`LootDeck[0]` 若为 `RandomItem` | ✅ 位置可知；宝箱收益类型可知 |
| 3 专长任务 Mastery | `Campaign/Masteries/` 34 个，声明式条件 | ✅ 条件 DSL 可解析 |
| 4 巅峰战绩 BattleGoal | `Campaign/BattleGoals/` 60 个，同上 | ✅ 同上（含失败条件） |
| 5 经验值 | `CPlayerActor.SetXP` / `ScenarioXP` | ✅ 可读 |
| 6 战利品袋（材料） | `ScenarioState.LootDeck`（`List<byte>`） | ✅ **捡拾前可知** |

**值得注意的推论**：第 2 档与第 6 档共用同一个 `LootDeck`。
因此"这个袋子该不该捡"不是固定结论——取决于 `LootDeck[0]` 是 `RandomItem` 还是材料。
这正好解释了你优先级里"除非是宝箱或可捡到装备"的例外条款，**且该例外可被精确判定**。

# 霜港迷城 — 敌人攻击倾向（AI 选敌）逆向

> 逆向日期：2026-10-22
> 数据来源：`Player-prev.log`（32MB，含 193 个怪物回合、15945 条怪物侧记录）
> ⚠️ 日志会轮转：会话重启后旧日志变为 `Player-prev.log`，新 `Player.log` 很小。
>
> 本文讲**选敌 / 移动 / 攻击**；行动牌本身的机制与可预测性见 **`MONSTER_CARDS.md`**。

---

## 1. 怪物回合的完整时序

追踪 VermlingWarchief（腐鼠战酋）一个回合的**真实消息序列**：

```
StartTurn                       回合开始
UpdateBattleGoals
UpdateWorldspaceConditionsUI
ActionSelectionPhaseStart
ActionSelection                 决定本回合行动
WaitForMultiplayerSyncPoint
ClearAllActorEvents
StartedAbility
StartAbility                    开始执行卡牌能力
ActorIsSelectingMoveTile        ← 选择「移到哪一格」
ActorHasMoved                   ← 实际移动
PlayerWaitForIdle
AbilityEndUpdateCombatLog
ActionAbilityHasHappened
EndAbilityAnimSync
InvalidAttack                   ← 本次攻击未生效
EndAction
EndTurn                         回合结束
```

**核心结论：怪物「先移动，再攻击」。**
`ActorIsSelectingMoveTile` 排在 `StartAbility` 之后、攻击结算之前——
怪物先算出"移到哪里才能打到人"，再移动，最后攻击。

### 全样本统计（193 个怪物回合）

| 动作 | 次数 | 说明 |
|---|---|---|
| StartTurn / EndTurn | 193 | 怪物回合数 |
| StartAbility | 449 | 执行的能力动作 |
| **ActorIsSelectingMoveTile** | 306 | 选移动格 |
| ActorHasMoved | 229 | 实际发生移动 |
| 抽修正牌（StartedDrawingModifier） | 97 | **真正形成攻击的次数** |

→ **449 次能力动作中只有 97 次抽了修正牌**，即约 78% 的怪物能力动作
是移动/增益/召唤等**非攻击**行为，攻击只占约 22%。

---

## 2. 选敌规则：距离优先，同距比先攻

代码层证据（经典 Gloomhaven/Frosthaven 规则）：

| 标识符 | 作用 |
|---|---|
| `ChooseClosestActor` | 选出**最近**的可攻击目标 |
| `ClosestActorToDamage` | 受伤时的最近判定 |
| `ActorInitiativeComparer` | **先攻比较器**（距离相同时排序用） |
| `AxialDistance` | 六边形轴向距离 |
| `CrowFlyDistance` / `CalculateCrowFlyDistance` | 直线距离 |
| `AproximatedFlyDistance` | 飞行距离估算 |
| `CachePositionsAndDistances` | 缓存位置与距离 |
| `AllTargetsInRangeAlongPath` | 路径上的全部目标 |
| `FindPathToFocus` / `CalcAIMoveFocusPath` | 走向焦点的路径 |
| `InitReachableTiles` | **够不着时的贴近候选**（所有可达格） |

**判定顺序**：
1. 找出所有能攻击到的玩家角色
2. 按**距离**取最近者
3. 距离相同时，按**先攻值（Initiative）**取小者

**★ 够不着时会逼近（重要，容易被想当然搞错）**

`FindPathToFocus()` 有明确的 fallback：走不到最佳攻击位时，
会在 `InitReachableTiles()` 给出的**所有可达格**里挑一个"离攻击位最近"的走过去，
把移动力用掉 —— 即标准规则「怪物会尽可能靠近它的焦点」。

只有两种情况它才真的不动：
1. `bestAttackTile == null` —— 连理论上的攻击位都没有（目标被墙封死等）
2. `maxMoveCount == 0` —— 本回合移动力为 0

⚠ 注意 `CalculateValidAttackTiles` 选 `lowestCostAttackTile` 时**不检查 reachable**，
  只要求在射程内 + 有视线 + 可站立。所以情况 1 很少发生 —— **怪物几乎总会移动**。

逼近的那回合它**不出手，也不抽修正牌**（没出手就不消耗牌）。

---

## 3. 目标数量：由卡牌的 Target 字段决定

统计 `Campaign/Enemies/**` 全部卡牌：

| Target 值 | 出现次数 | 含义 |
|---|---|---|
| `Target: 1` | 1307 | **单体（主流）** |
| `Target: 2` | 141 | 双目标 |
| `Target: 3` | 45 | 三目标 |
| **`Target: -1`** | 89 | **范围/无限制目标（AOE）** |
| `Target: 0` | 24 | 自身 |

配合 `Range` 字段：

| Range 值 | 出现次数 |
|---|---|
| `Range: 1` | 1520（近战主导） |
| `Range: 3` | 105 |
| `Range: 4` | 46 |
| `Range: 5` | 33 |
| `Range: 0` | 56 |

→ **绝大多数怪物是 Range 1 的单体近战**，远程与多目标属于少数设计。

### 怪物实体上的近战/远程标记

`Campaign/Enemies/**/*.yml`（Parser: Enemy）中：

```yaml
ID: AlgoxFrozenFistID
MonsterType: Boss
PredominantlyMelee: True      ← 近战倾向标记
StandeeLimit: 1
Level0:
    Move: 3
    Attack: 0
    Range: 1
    Target: 1
    StatIsBasedOnX:
        Health:
            BasedOn: InitialPlayerCharacterCount
            Multiplier: 13
        Attack:
            BasedOn: InitialPlayerCharacterCount
            Multiplier: 2
            AddTo: True
```

**`PredominantlyMelee`** 显式声明该怪物为近战倾向，驱动 AI 的接敌行为。
另注意 **怪物属性按玩家人数动态缩放**（English：Health = 人数 × 13）。

---

## 4. 实战目标分布（真实对局）

本次对局为 2 人小队（双生体 Geminate + 漂泊者 Drifter），
统计战斗日志中"X 对 Y 发动了攻击"：

| 攻击对 | 次数 |
|---|---|
| 腐鼠战酋 → 双生体 | 20 |
| (3) → 双生体 | 20 |
| 腐鼠战酋 → 漂泊者 | 18 |
| (3) → 漂泊者 | 16 |
| (4) → 双生体 | 14 |
| (5) → 双生体 | 12 |
| (4) → 漂泊者 | 12 |
| (1) → 漂泊者 | 12 |

**合计：双生体被攻击 90 次、漂泊者 82 次。**

分布接近但双生体略高——与两人站位远近一致，
符合"距离优先"而非"职业优先"的判定（无针对职业的仇恨机制）。

---

## 5. 隐藏规则

### 5.1 AIFocusOverride：可强制指定怪物打谁

一套完整的焦点覆盖系统：

```
AIFocusOverride / AIFocusOverrideDetails / AIFocusOverridePanel
SetAIFocusOverride / ClearAIFocusOverride / ResetAIFocusOverride
SetAIFocusOverrideOnTileOrNearestEmpty
LevelEditorAIFocusOverridePanel      ← 关卡编辑器内的面板
```

→ **可以覆盖怪物的选敌结果**，且关卡编辑器里就能设。
做自定义关卡时，这是直接操控敌人攻击倾向的官方接口。

### 5.2 攻击可能「无效」：InvalidAttack

时序里出现 `InvalidAttack` —— 攻击未生效的独立状态。
结合攻击侧的 ×0 落空（Attack.md 的 `((5))*0 = 0`），
**怪物同样会落空**，且走独立的无效分支。

### 5.3 威胁值（Threat）只用于场景生成，不是战斗选敌

`Campaign/Monsters.yml` 结构：

| 段 | 用途 |
|---|---|
| `Threats:` | 怪物威胁值 `[普通, 精英]`，如 `CaveBearID: [50,100]` |
| `MonsterFamilies:` | 怪物家族（City/Undead/Vermling/Inox/Demon…）与配比 |
| `MonsterGroups:` | 怪物编组 |

但文件头明写：

```
# Commented out as this is used by the Guildmasters mode while generating
# scenarios which we won't be supporting.
```

且**绝大多数 Threats 条目已被注释掉**（仅 CityGuardID / CityArcherID 启用）。

→ **Threats 服务于「场景生成时放什么怪」，该模式不被支持；
战斗中的选敌走的是距离 + 先攻，不是威胁值。**

（DLL 中另有 `AddThreat` / `ReduceThreatByEachDefense` / `MonsterThreatValues`
等运行期标识符，推测为同一系统的残留或 TownGuard 相关用途，
但在当前战斗选敌链路中未见其参与焦点判定。）

### 5.4 怪物属性随人数缩放

`StatIsBasedOnX: BasedOn: InitialPlayerCharacterCount`
→ 人数越多，怪物血量与攻击越高（血量 ×13/人，攻击 +2/人）。
**这是"人越多怪越硬"的隐藏缩放**，直接影响难度感知。

---

## 6. 对 mod 的落点

| 想改什么 | 文件 / 手段 |
|---|---|
| 单体改多目标 | 改怪物卡 `Target: 1` → `2`、`3`、`-1` |
| 近战改远程 | 改 `Range: 1` → `3/4/5` |
| 接敌倾向 | 改实体 `PredominantlyMelee` |
| 攻击强度 | 改卡牌 `Attack:` 或 `StatIsBasedOnX` 的 `Multiplier` |
| 指定怪物打谁 | 用关卡编辑器的 `AIFocusOverride` |
| 场景出什么怪 | `Campaign/Monsters.yml` 的 `MonsterFamilies` / `MonsterGroups` |

> 注意：`Monsters.yml` 里大量内容被注释，改之前先确认目标段是否处于启用状态。

# 阶段 1：规则数据提取（子集）

> 执行依据：阶段 0 报告 + 你的决策（Q1 C# / Q2 运行时 hook / Q3 子集 + 本地化不提取 + JSON Lines）。
> 事实源：`extracted/Base_ruleset/**/*.yml`（由 `Base.ruleset` 解出，字节级无损）。
> 原始游戏目录全程只读。

---

## 1. 文件清单

### 1.1 新增 / 变更文件

| 路径 | 作用 |
|---|---|
| `tools/extract_rules.py` | 阶段 1 提取脚本（**一次性清洗，不进主引擎**） |
| `Data/cards.json` | 角色能力卡（BannerSpear + Drifter） |
| `Data/monsters.json` | 怪物（AlgoxScout + AbaelHerder，含 Level0–7 属性表） |
| `Data/monster_cards.json` | 怪物行动卡 16 张，**含 rawValue / resolvedValue 双字段** |
| `Data/attack_modifiers.json` | 攻击修正牌 140 个（全量定义） |
| `Data/modifier_decks.json` | 4 副牌组 |
| `Data/deck_membership.json` | **牌组 → 引用的 AMC（名称 + 张数）** |
| `Data/manifest.json` | 提取元信息：条数、归档格式、未决问题 |
| `src/FHEngine/FHEngine.csproj` | 引擎工程（**netstandard2.0**，零 NuGet 依赖） |
| `src/FHEngine/Model/RuleData.cs` | 数据模型，与 JSON 逐字段对应 |
| `tests/FHEngine.Tests/FHEngine.Tests.csproj` | 测试台（**net472**，复用现有断言风格） |
| `tests/FHEngine.Tests/Program.cs` | 数据校验台（30 项断言） |
| `PHASE0_SCAN.md` | 已按你的意见修正两处措辞（§2.1 魔数、§5.2 R1） |

### 1.2 提取条数

| 项 | 条数 | 来源 |
|---|---|---|
| cards | **60**（BannerSpear 29 + Drifter 31） | `Global/Characters/{BannerSpear,Drifter}/AbilityCards/*.yml` |
| monsters | **4**（AlgoxScout / AlgoxScoutElite / AbaelHerder / AbaelHerderElite） | `Campaign/Enemies/Monsters/{AlgoxScout,AbaelHerder}/*.yml` |
| monster_cards | **16**（770–777 Scout 卡 + 778–785 AbaelHerder 卡） | 按怪物 `AbilityCards` 的 **ID 引用**解析 |
| attack_modifiers | **140** | `Global/AttackModifiers/*.yml` |
| modifier_decks | **4** | `Global/AttackModifierDecks/*.yml` |
| **入组 AMC** | **9** / 140 | 由 `deck_membership.json` 统计 |
| 未入组 AMC | **131** | 同上 |

---

## 2. 抽样 3 条：与源文件逐字段对照

### 抽样 1 — `Data/cards.json` Id=62

源文件：`extracted/Base_ruleset/Global/Characters/BannerSpear/AbilityCards/62_BannerSpear_RallyingCry.yml`

| 字段 | 源文件（行号） | JSON / 引擎读出 |
|---|---|---|
| ID | 第 5 行 `ID: 62` | 62 ✓ |
| Initiative | 第 7 行 `Initiative: 71` | 71 ✓ |
| Level | 第 6 行 `Level: 1` | 1 ✓ |
| Discard | 第 8 行 `Discard: [Discard,Lost]` | `["Discard","Lost"]` ✓ |
| Top.Abilities.TAttack.Attack | `Attack: {Amount: 3}` | AttackAmount = 3 ✓ |
| Top.Abilities.TAttack.Conditions | `Conditions: Disarm` | `"Disarm"` ✓ |
| Top.Abilities.TAttack.AreaEffect | `"0,0,G\|2,0,R\|3,1,B\|4,0,E"` | 原样 ✓ |
| Top.XP | `XP: 1` | 1 ✓ |
| Bottom.Abilities.BSummon.Summon | `Summon: BannerOfStrengthID` | ✓ |
| Bottom.XP | `XP: 2` | 2 ✓ |

### 抽样 2 — `Data/monster_cards.json` Id=779（AbaelHerder_DesperateHerd）

源文件：`.../AbaelHerder/Cards/779_AbaelHerder_DesperateHerd.yml`

| 字段路径 | raw（yml 原值） | resolved（AbaelHerderID L0） | resolved（Elite L0） |
|---|---|---|---|
| `Abilities.Move1.Move` | **-1** | **3** ✓ | 3 |
| `...AllyMove.Move` | **-1** | **3** ✓ | 3 |
| `...AllyAttack.Attack` | **-2** | **2** ✓ | **3** ✓ |
| `...SelfDamage.Damage` | 1 | 1 ✓ | 1 |
| `Abilities.ControlPig1.Range` | 3 | 3 ✓ | 3 |
| `Abilities.ControlPig1.Target` | **99** | 99（未解释，见未决问题） | 99 |

→ 哨兵值 `-1/-2` 已按"取怪物同名属性"解析，且**精英与普通得到不同攻击力**（2 vs 3），与源文件 `Level0: {Attack: 2}` / 精英 `Attack: 3` 一致。

### 抽样 3 — `Data/deck_membership.json` Standard

| 项 | 值 | 源文件 |
|---|---|---|
| TotalCards | **20** | `Global/AttackModifierDecks/Standard.yml` |
| 牌种 | **7** | 同上 |
| 构成 | P0×6, P1×5, P2×1, M1×5, M2×1, T0×1, T2×1 | 同上 |
| IsMonsterDeck | False | 同上 |

**四副牌组全表**：

| 牌组 | 怪物牌组 | 张数 | 构成 |
|---|---|---|---|
| Standard | False | 20 | P0×6 P1×5 P2×1 M1×5 M2×1 T0×1 T2×1 |
| ReducedRandomness | False | 20 | 同上但把 ×0/×2 换为 `M2_Shuffle` / `P2_Shuffle` |
| MonsterDeck | True | 20 | 同 Standard |
| MonsterReducedRandomnessDeck | True | 20 | 同 ReducedRandomness |

★ ReducedRandomness 用 `*_Shuffle` 替代 `T0(×0)` / `T2(×2)`——这正是该房规的机制证据。

---

## 3. 运行命令与结果

```bash
# 提取（一次性清洗脚本）
python3 tools/extract_rules.py

# 数据校验（C#，net472）
cd tests/FHEngine.Tests
dotnet build -v q --nologo
dotnet run --no-build
```

**测试结果：`30 通过 / 0 失败`**

覆盖：条数（5 项）、抽样 1 逐字段（10 项）、抽样 2 raw/resolved 一致性（8 项）、抽样 3 牌组（5 项）、引用完整性（2 项）。

其中关键断言：
- `存在哨兵 raw 值（负数，需解析）: 3` ✓
- `resolved 中仍为负数的字段数（应为 0）` = 0 ✓
- `AbaelHerderID L0 的 AllyAttack.Attack（= 怪物 Attack 2）` = 2 ✓
- `入组的 AMC 数 < 140` = 9 ✓
- `怪物引用的卡中缺失的条数（应为 0）` = 0 ✓

---

## 4. 你要求的三个附带确认

### 4.1 魔数 `02392fe1` —— **确认为 8 字节 ASCII 字符串**

```
$ od -A d -t x1z -N 32 Base.ruleset
0000000 30 32 33 39 32 66 65 31 00 00 00 00 45 0d 00 00  >02392fe1....E...<
```

`30 32 33 39 32 66 65 31` = ASCII `"02392fe1"`。
**你的判断正确**：是可打印 ASCII，不是 u32。

同时发现：偏移 8 处还有一个 `u32 = 0`（保留位），偏移 12 才是条目数 3397。
`FORMAT.md` 第 47–60 行原本已正确；错的是**阶段 0 报告 §2.1**（写成"魔数 + u32 条目数"，漏了那 4 字节）——已按你的意见改写为"8 字节 ASCII 标识"并补全偏移表。

### 4.2 17 个职业 **全部可玩**

逐目录核实 `AbilityCards/` 存在与卡数：

| 职业 | 卡数 | 职业 | 卡数 |
|---|---|---|---|
| Geminate | 34 | BannerSpear / Blinkblade / Deepwraith / PainConduit / Pyroclast / Shattersong | 29 |
| Boneshaper / CrashingTide / Drifter | 31 | FrozenFist / MetalMosaic / Trapper | 28 |
| Deathwalker / HIVE / Infuser / Snowdancer | 30 | | |

**结论：17 个目录均含 AbilityCards（28–34 张），无 NPC / 未启用职业。** 我阶段 0 报告里写的"Fused?"是臆测——**该目录不存在**，已更正。

### 4.3 140 个 AMC 与 4 副牌组的关系

- 140 = **全部修正牌定义**（含祝福/诅咒/元素/职业专属/物品触发等）
- 入组 = **仅 9 个**；未入组 **131 个**
- 已产出 `deck_membership.json`（牌组 → AMC 名 + 张数），阶段 3 抽牌**只从这里取**，不会混入未入组的牌

---

## 5. 未决问题（需你决策或补充）

### Q-A 哨兵值 `-1 / -2` 的**精确语义未能在本体反编译产物中定位**

- **本地证据**
  - `Scout/Cards/770_Scout_CruelBow.yml` 等：`Move: -1`
  - `AbaelHerder/Cards/779_*.yml`：`Attack: -2`
  - `decompiled/SRL/ScenarioRuleLibrary/CMonsterClass.cs:233-234`：
    `CreateDefaultAttack(m_CurrentMonsterStat.Attack, ...)` / `CreateDefaultMove(m_CurrentMonsterStat.Move, ...)`
  - 未能找到"卡面值为负时读取怪物属性"的那一行代码
- **候选解释**
  - A：负数 = 取怪物自身同名属性（Move→Move，Attack→Attack）
  - B：负数 = 未指定，运行时由 MonsterClass 默认能力填充（CMonsterClass.cs:233-234）
  - C：负数 = 相对修正（怪物 Attack 上 -2）→ 实测会导致 0 伤害，**与观察不符**
- **当前选择**：按 **A** 生成 `resolved`；`raw` 完整保留，若你否定 A 可立即重算
- **影响测试**：怪物移动力、怪物攻击力、伤害计算、AI 纯函数输出

### Q-B 新增哨兵 `Target: 99`（提取时发现，需确认）

- 证据：`779_*.yml` 的 `ControlPig1.Target = 99`；`61_BannerSpear_AtAllCosts.yml` 的 `THeal.Target = 99` 且同节点有 `Targeting: All`
- 候选：A = "全部/无上限"；B = 引擎侧占位（实际由 `Targeting` 语义决定）
- 当前：原样保留 99，不做解释

### Q-C 关卡与基准场景

- `Campaign/Scenarios/` **无 .yml**，只有 142 个 `.lvldat`（.NET BinaryFormatter）
- "烈焰焚城"使命的**内部 ID 无法从明文确定**（二进制 + 本地化不提取）
- 校准档已确认存在：`.../Campaign/规则校准档/`（`A001.dat` 62KB / `AutoSave_000.dat` / `AutoSave_001.dat`）
- **需要你提供**：该使命的内部 ID，或允许我用 C# + 游戏程序集解析 `.dat`/`.lvldat` 来定位

### Q-D 工程侧的一个实现取舍（供你知晓，非阻塞）

- `DataContractJsonSerializer` 按成员**字母序**期望 JSON 键，顺序不符时后续成员静默为 null（已踩）
- 本体 yml 存在**异构字段**（`Filter`/`Conditions` 有时标量、有时数组）→ 通用反序列化会崩
- 处理：在**提取层**（Python）统一类型（标量→str、数组→`|` 连接、dict→JSON 文本），C# 侧模型保持简单
- 引擎侧（netstandard2.0）**不引入任何 JSON 库**，符合零 NuGet 约束

---

## 6. 下一步建议

我建议阶段 2 的产出顺序：
1. 状态 / 事件 / 决策挂起点的模型文档（含事件字段定义与 JSON Lines 格式 `{seq, round, turn, actorId, type, payload}`）
2. 最小战斗循环实现计划（ROUND_START → … → ROUND_END 的伪代码 + 涉及的数据）

**在动手阶段 2 之前，请裁决 Q-A、Q-B，并补充 Q-C 的关卡定位方式。**

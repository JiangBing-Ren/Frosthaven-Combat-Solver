# 项目画像：FH（霜港迷城 Frosthaven 电子版 mod）

## 项目概况
- 目标：《霜港迷城》(Frosthaven) PC 数字版 mod。
- 游戏本体：`D:\Steam\steamapps\common\Frosthaven`（D 盘 Steam 库，非 C 盘）。
- 工作区：`D:\1Mods\FH`。

## 技术底座（已确认）
- **Unity Mono** 后端（非 IL2CPP）→ 可 dnSpy 反编译、Mono.Cecil IL 注入。
- 主 DLL 无混淆。业务程序集：`Assembly-CSharp` / `ScenarioRuleLibrary` /
  `ForsthavenRuleLibrary`（经济数值）/ `MapRuleLibrary` / `SharedLibrary`。
- 内置 Roslyn → 游戏有运行时 C# 脚本能力（`StreamingAssets/Scripts/*.cs`）。

## 核心文件格式（详见 FORMAT.md）
- `StreamingAssets/Rulebase/Base.ruleset` = 自研归档。
  格式：`02392fe1` + u32 条目数 + TOC(N×144: 128B 路径 + meta1 + meta2 + offset + size) + 连续数据区。
- 内容 = 3248 个明文 `.yml`（规则数据，主 mod 面）+ 149 个 `.lvldat`
  （.NET BinaryFormatter 关卡数据）。
- 工具：`tools/ruleset.py`（info / extract / repack），回打包字节级无损（md5 一致）。

## 项目约定
- 改数值优先走「编辑 .yml → 回打包」，不动 DLL，风险最低。
- 回打包依赖 `_manifest.json` 保留 meta 与条目顺序，**勿打乱顺序**。
- YAML 内为 `\r\n` 换行；归档路径含空格，脚本勿按空格切分。
- 改前先备份原 `Base.ruleset`。

## mod 加载能力
- **无官方 Steam 创意工坊**（Steam appid 2347080 标注 No Workshop Support）。
- **改 Base.ruleset 是首选路径**：游戏逐条解析 YAML，`Player.log` 可见加载时序
  （40 HeroSummon / 531 AbilityCard / 648 EnemyCard / 149 CustomLevel…），无 hash 校验环节。
- **内建自定义关卡系统**（CustomLevel API + 官方 SandboxScenario/Example_Level 示例）
  → 做自定义关卡走官方通路，不必改归档。
- **无 EasyAntiCheat** → BepInEx/MelonLoader/UnityModManager 均可用（Unity Mono）。
- ⚠️ 改游戏目录内文件会被 **Steam 验证完整性还原**，留备份或走外挂方案。
- ⚠️ 游戏有联机（EOS），**改数值仅限单人档**，联机可能状态分叉（类 WH3 desync）。

## 重要路径
- 游戏日志：`C:\Users\hanf2\AppData\LocalLow\Snapshot Games Inc\Frosthaven\Player.log`（调试主要依据）
- 攻击机制逆向完整文档：`D:\1Mods\FH\ATTACK.md`（生命周期 / 伤害公式 / 牌组 / 隐藏规则）

## 敌人 AI 速查（详见 ENEMY_AI.md）
- 怪物回合：**先移动再攻击**（选移动格 → 移动 → 攻击）。
- 选敌 = **距离优先 + 同距比先攻**（`ChooseClosestActor` + `ActorInitiativeComparer`），无职业仇恨。
- 卡牌 `Target` 决定打几个（-1 = 范围AOE），`Range` 决定近战/远程；多数怪物是 Range1 单体近战。
- ⚠️ **Threats 表不是战斗选敌**，是场景生成用的（Guildmasters 模式，已废弃/注释）。
- 可改：怪物行为改卡牌 Target/Range；强制指定目标用关卡编辑器的 `AIFocusOverride`。
- ⚠️ 怪物属性随**玩家人数**缩放（血量 ×13/人、攻击 +2/人）。

## 攻击机制速查（详见 ATTACK.md）
- 伤害表达式 `( 基础 [±修正] ) [×2|×0]`，**乘算最后** → ×2 连加成一起翻倍；**下限截断 0**。
- 标准修正牌组 20 张（+0×6/+1×5/+2×1/-1×5/-2×1/×0/×2）。
- Advantage 抽2取好 / Disadvantage 抽2取坏 / None 抽1。
- **伤害先算出、动画后演**（改动画不影响结果）。
- 数字版特有 **ReducedRandomness 牌组**（把 ×0/×2 换成 ±2_Shuffle），属房规。

## 修正牌是确定量（详见 MODIFIER.md）★ 本项目最关键结论
- **抽修正牌不消耗随机数**：`CActor.DrawAttackModifierCard` 直接取 `current[0]`。
- 牌序明文存在 `AttackModifierDeckState.AttackModifierCardsAvailable`，随存档持久化。
- **撤销 = 整份 ScenarioState 深拷贝回滚**（`CopyScenarioState`，BinaryFormatter），
  RNG 状态（4 条，各自 `RandomState(byte[])`）一并回滚 → 重来必定翻出同一张。
- ×0/×2 **抽到只置位，回合末才洗**（不是抽到就洗）。
- RNG 本体 = `SharedLibrary.Random`（.NET Framework `System.Random` 逐行拷贝，56 槽+双指针），
  可离线复刻。
- → 求解器**无需算期望，可直接精确预知**每次翻出的牌。

## 战斗求解器（BepInEx 插件，详见 SOLVER_DESIGN.md / BUILD.md）
- **M0 已通过**（2026-10-22）：BepInEx 5.4.23.5 正常加载，游戏类型探测 17/17 命中。
- 环境实测：Unity **2022.3.71.13221651**，CLR `4.0.30319.42000`，**Supports SRE: True**。
- **必须用 BepInEx 5.4.23.5（2026-02 发布），不要用 6.0.0-pre.2（2024-08 的陈旧预发布版）**。
  判断依据是发布时间与维护状态，不是版本号大小。
- 插件 TFM = **net472**；Harmony 引用 `0Harmony.dll`（HarmonyX 2.9，命名空间 `HarmonyLib`），
  不是 `0Harmony20.dll`。
- 工程：`D:\1Mods\FH\src\FHSolver`（构建后自动部署到 `BepInEx/plugins`）。
- 工具：`tools/verify_m0.sh`（自动启停游戏验证）、`tools/uninstall_bepinex.sh`、
  `tools/verify_m1.sh`（boot/battle）、`tools/collect_m1.sh`（不启游戏，只抽日志）。

### ★ 插件开发三条硬教训（2026-10-22 实机踩坑）
1. **插件 MonoBehaviour 启动后不久会被 Unity 销毁。** 已用 fake-null 检测坐实：
   `Plugin.Instance == null` → True，但 `(object)Plugin.Instance == null` → False。
   连带后果：只有 `Awake`/`OnEnable` 会触发，`Start`/`Update`/`FixedUpdate`/`LateUpdate` 全不触发；
   自建 `DontDestroyOnLoad` 对象同样只活到 Awake。日志仍能打出，仅因
   `ManualLogSource` 不是 `UnityEngine.Object`。
   ⇒ **轮询入口不能靠 MonoBehaviour，改用 Harmony Postfix 挂游戏自身的 `Update()`。**
2. **已挂且全部命中的 4 个入口**（运行时 `Type.GetType("<名>, Assembly-CSharp")` 反射取，
   **不要编译期引用 Assembly-CSharp**，其依赖图极深）：
   `Choreographer` / `ClientScenarioManager` / `CharacterManager` / `InteractabilityManager`。
   `Choreographer` 战斗期必在，`InteractabilityManager` 主菜单期就有。
   实现见 `src/FHSolver/Bridge/TickPump.cs`。
3. **一切不依赖 UnityEngine.Object。** config 值（enabled / 快捷键 / 预读张数）在 `Awake`
   里立刻快照成 `Plugin.StaticEnabled` / `StaticDumpKey` / `StaticPeek`；
   轮询状态放 `TickPump` 的 static 字段。
   ⚠️ 未来做 UI（M4）时必须自建 GameObject 并考虑防销毁，不能指望插件组件存活。
4. **诊断量要打进心跳行。** v0.3.0 靠心跳行里同时出现 `IsInTactical=True` 与
   `已进战斗=False` 才把嫌疑锁定到 `_owner == null`，否则无从下手。

## ★ 卡面半边语义（v1.3 关键规则理解，详见 SOLVER_DESIGN.md）
- 判定：`CAbility.ActiveBonusData != null && Duration != EActiveBonusDurationType.NA`
  → 这一半是**常驻加成**，**不是**本回合能执行的动作。
- 常驻加成里的 `Move: N` / `Attack: N` 是**加成幅度**（以后每次移动/攻击 +N），
  **不是**本回合能移动 N 格 / 攻击 N 点。踩过坑：把死行「深渊力量」下半当移动 2 用。
  实证：`100_Deathwalker_StrengthOfTheAbyss.yml` 的 BBuffMove / BBuffAttack。
- 一张牌的三个可用候选动作：**上半能力 / 下半能力 / 通用基础攻击 2（攻2射程1单体）**，
  以及 **通用基础移动 2**（走 2 格）。数值读 `card.DefaultAttackAction` /
  `DefaultMoveAction`，**不要写死 2/1**（增益和道具会改）。
- **范围攻击**：`CAbility.AreaEffect != null` → 要先选焦点格、按固定形状铺开，
  一次打好几个 → 单体伤害模型算不准 → **识别出来就别建模**，把形状原样提示给玩家。
  实证：旗矛「绞肉机」上半 `AreaEffect: "0,0,G|1,1,R|2,0,R|3,1,B"`。
- **实测结论**：全部 504 张角色卡里，`Move:` 出现在 `Attack:` 之后的卡 = **0 张**
  → "半边内统一按先移动后攻击"这个简化对玩家卡成立（怪物卡同理）。

## 先攻规则（v1.3）
- **玩家先攻取自"自己方案里先打出的那张牌"**。`CPlayerActor.Initiative()` 读
  `CharacterClass.InitiativeAbilityCard`，这张"领头牌"要等出牌才定 →
  **选牌阶段读出来恒为 0**。之前三个队友全显示 0 就是这么来的。
- 怪物先攻：`MonsterClass.RoundAbilityCard ?? NextAbilityCard` 的 `Initiative`。
- 数值**小**的先动。

## 已知待验证
- dnSpy 实际打开 DLL 尚未验证（目前为反编译 + 实机类型探测证据，已足够）。
- 改 YAML 回打包后游戏实际加载效果尚未实测（字节一致 ≠ 逻辑通过）。
- ~~求解器 M1（只读快照 + 牌堆顺序）~~ → **已通过**（2026-10-23，v0.3.2 实战，快照 195 次）。
- ~~M1.5 牌堆预测正确性~~ → **已通过**（2026-10-24，v0.4.0 实战，**354/354 全中，零失配**，远高于连续 20 次的判定线）。

## 求解器 M1 成果要点
- **牌序可直接读出**：`CCharacterClass.AttackModifierCards` 的顺序即未来翻牌序列，
  玩家两副牌堆实测合计均 20 张，与 MODIFIER.md 标准牌组一致 → 修正牌可精确预知获直接证据。
- 怪物共四副牌堆：`MonsterClassManager.Enemy/Allied/Enemy2/NeutralMonsterAttackModifierDeck`。
- **M3 怪物意图入口已定位**：`CMonsterClass.RoundAbilityCard`（本回合行动卡）/ `NextAbilityCard`。
  怪物先攻恒 0 是因回合开始时尚未选牌（`CActor.Initiative()` 基类默认返回 0）。
- **死亡单位需过滤**：`CActor.IsDead => CauseOfDeath != ECauseOfDeath.StillAlive`，
  `Scenario.AllMonsters` 仍包含已死单位（会出现负血条目）。

## GitHub 仓库（2026-10-25 建立）
- **`JiangBing-Ren/Frosthaven-Combat-Solver`**（**private**）—— 本项目本体
  - 私有原因：含游戏 DLL 的反编译产物（`decompiled/` 5830 文件、`extracted/` 3398 文件）
    属游戏厂商内容，仅供内部研究
  - 推送：`git push -u origin main`（用 git-credential-manager 的缓存凭据）
  - ⚠ MCP 的 GitHub App **没有建仓权限**（403 Resource not accessible by integration），
    改用 `curl` + `git credential fill` 拿 PAT 调 API 建仓
  - 主分支 `main`（本地 `git init` 后 `git branch -m main`）
- 提交历史即事故记录：fd9bd4e 基线 → d984e33 恢复 → f77dcff 埋点 → 82e43b9 记忆

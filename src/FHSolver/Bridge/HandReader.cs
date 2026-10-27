using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AStar;
using FHSolver.Model;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 读取手牌详情与移动可达集 —— P1（单角色×单回合穷举）的两块输入。
    ///
    /// 这里最要紧的一件事：**分清卡面半边的语义**。
    /// 一个半边可能是：
    ///   ① 本回合执行的一次性动作（移动 / 攻击 / 治疗 …）；
    ///   ② 常驻加成（ActiveBonus，Duration != NA）—— 打出这张牌时自动挂上的增益，
    ///      **不是动作**，不能拿去"移动 2 / 攻击 2"；
    /// 还有一条路是 ③ 通用「基础攻击 2」/「基础移动 2」，任何一张牌都能当它用。
    ///
    /// 修这个之前，求解器把死行「深渊力量」的下半（BBuffMove/BBuffAttack，
    /// Duration: Persistent）当成了「移动 2 / 攻击 2」的动作，
    /// 于是推荐出游戏里根本执行不了的操作。
    ///
    /// 移动可达集**不自己复刻**，直接调用游戏自己的
    /// `ScenarioManager.PathFinder.FloodFill_ToRange`（见 Geometry）。
    /// </summary>
    internal static class HandReader
    {
        // ---- 反射：AbilityData.ActiveBonusData ----
        // 走反射而不是编译期引用，理由和 Loc.cs 一样：跨版本拿不到就静默退化，
        // 好过整个插件在 JIT 阶段炸掉。
        private static readonly PropertyInfo PActiveBonusData =
            typeof(CAbility).GetProperty("ActiveBonusData", BindingFlags.Public | BindingFlags.Instance);

        private static readonly PropertyInfo PAreaEffect =
            typeof(CAbility).GetProperty("AreaEffect", BindingFlags.Public | BindingFlags.Instance);

        private static readonly PropertyInfo PAreaEffectYml =
            typeof(CAbility).GetProperty("AreaEffectYMLString", BindingFlags.Public | BindingFlags.Instance);

        // ⚠ 不要 typeof(CAbility).GetProperty("MoveRestrictionType") —— 那个属性
        //   **只定义在 CAbilityMove 上**，CAbility 基类没有它，所以静态反射拿到的是 null，
        //   ReadMoveRestriction 永远返回 "None" → 落脚限制（含 MustEndOnShadow）形同虚设。
        //   实战里因此把一个"没有阴影的回合"推荐成了「暗影步法」。
        //   必须按**运行时实际类型**（CAbilityMove 实例）反射，且拿不到要大声报错。

        private static readonly PropertyInfo PStatIsBasedOnX =
            typeof(CAbility).GetProperty("StatIsBasedOnXEntries", BindingFlags.Public | BindingFlags.Instance);

        // CAbility.NegativeConditions / PositiveConditions ——
        //   "这个能力施加的条件"（Dictionary<条件枚举, CAbility>）
        private static readonly PropertyInfo PNegativeConditions =
            typeof(CAbility).GetProperty("NegativeConditions", BindingFlags.Public | BindingFlags.Instance);
        private static readonly PropertyInfo PPositiveConditions =
            typeof(CAbility).GetProperty("PositiveConditions", BindingFlags.Public | BindingFlags.Instance);

        // MiscAbilityData.IgnoreLOS —— 逐能力的"忽略视线"开关（见 ActionSnapshot.IgnoreLOS）
        // ⚠ ElementsToInfuse 只定义在 CAbilityInfuse 上，CAbility 基类没有 ——
        //   静态 typeof(CAbility).GetProperty 会拿到 null（和 MoveRestrictionType 同一类坑）。
        //   所以不缓存静态字段，改为按运行时类型现找（见 ReadElementNeed）。

        private static readonly PropertyInfo PMiscAbilityData =
            typeof(CAbility).GetProperty("MiscAbilityData", BindingFlags.Public | BindingFlags.Instance);

        /// <summary>读一个角色的手牌详情。</summary>
        public static List<AbilityCardSnapshot> ReadHand(CCharacterClass cls)
        {
            var list = new List<AbilityCardSnapshot>();
            if (cls == null) return list;

            try
            {
                List<CAbilityCard> hand = cls.HandAbilityCards?.ToList() ?? new List<CAbilityCard>();
                foreach (CAbilityCard card in hand)
                {
                    if (card == null) continue;
                    list.Add(ReadCard(card));
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取手牌详情失败: {e.Message}");
            }
            return list;
        }

        private static AbilityCardSnapshot ReadCard(CAbilityCard card)
        {
            var snap = new AbilityCardSnapshot();
            try
            {
                snap.Id = card.ID;
                snap.Name = card.Name ?? card.ID.ToString();
                // 本地化键就是 Name（AbilityCardUI.MakeAbilityCard: CardName = cardData.Name）
                snap.DisplayName = Loc.T(snap.Name);
                snap.Initiative = card.Initiative;

                try
                {
                    var yml = card.GetAbilityCardYML;
                    if (yml != null)
                    {
                        snap.TopDiscard = yml.TopDiscardType.ToString();
                        snap.BottomDiscard = yml.BottomDiscardType.ToString();
                    }
                }
                catch { /* YML 缺失时忽略 */ }

                snap.Top = ReadAction(card.TopAction, "上");
                snap.Bottom = ReadAction(card.BottomAction, "下");

                ReadDefaultActions(card, snap);

                // 基础动作快照在 Bridge 层合成好（Solver 层不碰游戏类型）。
                snap.DefaultAttack = MakeDefaultAttack(snap);
                snap.DefaultMove = MakeDefaultMove(snap);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取单卡失败: {e.Message}");
            }
            return snap;
        }

        /// <summary>
        /// 移动类能力。必须精确匹配，不能用 IndexOf("Move") ——
        /// MoveShadow / MoveObstacle / MoveActiveBonusTracker / MergedMoveAttack 都含 "Move" 子串，
        /// 但语义完全不同（按子串匹配会把"移动影子"当成"移动0"，实测就这么错过）。
        /// </summary>
        private static readonly HashSet<string> MoveTypes = new HashSet<string>
        {
            "Move",
            "MergedMoveAttack",      // 合并能力：移动 + 攻击（先移动后攻击）
        };

        /// <summary>攻击类能力（含 Frosthaven 的合并能力）。</summary>
        private static readonly HashSet<string> AttackTypes = new HashSet<string>
        {
            "Attack",
            "MergedCreateAttack",
            "MergedDestroyObstacleAttack",
            "MergedMoveAttack",
            "MergedMoveObstacleAttack",
            "MergedKillCreate",
            "MergedDisarmTrapDestroyObstacles",
        };

        /// <summary>
        /// 读一个 CAction 的内容（卡半边 / 怪物行动卡共用同一套能力结构）。
        /// 怪物行动卡的意图也是 CAction，所以直接复用，玩家能提前看到它要干什么。
        /// </summary>
        /// <summary>
        /// 诊断用：把一个 CAction 里的每个能力列出来（类型/强度/射程）。
        /// 用来查"意图读到了什么"这类问题。
        /// </summary>
        public static string DescribeAbilities(CAction action)
        {
            try
            {
                if (action == null || action.Abilities == null) return "(空)";
                var parts = new List<string>();
                foreach (CAbility ab in action.Abilities)
                {
                    if (ab == null) continue;
                    parts.Add($"{ab.AbilityType}:S{ab.Strength}/R{ab.Range}/N{ab.NumberTargets}");
                }
                return parts.Count == 0 ? "(无能力)" : string.Join(", ", parts);
            }
            catch (Exception e)
            {
                return "(读取失败:" + e.Message + ")";
            }
        }

        internal static ActionSnapshot ReadAction(CAction action, string side)

        {
            var a = new ActionSnapshot { Side = side };
            if (action == null) return a;   // Kind 保持 Empty

            int abilities = 0;
            int persistent = 0;

            try
            {
                foreach (CAbility ab in action.Abilities ?? new List<CAbility>())
                {
                    if (ab == null) continue;
                    abilities++;
                    string typeName = ab.AbilityType.ToString();
                    a.AbilityTypes.Add(typeName);

                    // ---- ① 常驻加成：不是本回合动作 ----
                    string behaviour;
                    if (TryReadPersistentBonus(ab, out behaviour))
                    {
                        persistent++;
                        a.PersistentBehaviours.Add(behaviour);
                        DescribeBonus(behaviour, ab, a);
                        continue;
                    }

                    bool isMove = MoveTypes.Contains(typeName);
                    bool isAttack = AttackTypes.Contains(typeName);
                    bool isHeal = typeName == "Heal";
                    bool isPoison = typeName == "Poison";
                    bool isPull = typeName == "Pull";
                    bool isControl = typeName == "ControlActor";
                    bool isTeleport = typeName == "Teleport2" || typeName == "Teleport";

                    // 强度是否随场上条件变化（死行的阴影数、旗矛的旗帜数 …）
                    if (HasStatIsBasedOnX(ab)) a.StrengthIsVariable = true;

                    if (isMove)
                    {
                        a.HasMove = true;
                        if (ab.Strength > a.MoveStrength) a.MoveStrength = ab.Strength;

                        // 落脚限制：CAbilityMove.MoveRestrictionType
                        // 死行「暗影步法」上半 = MustEndOnShadow（必须停在阴影格）
                        string restriction = ReadMoveRestriction(ab);
                        if (restriction != "None") a.MoveRestriction = restriction;
                    }
                    if (isAttack)
                    {
                        a.HasAttack = true;

                        // 忽略视线的能力：墙后目标也合法（默认要视线）
                        a.IgnoreLOS |= ReadIgnoreLOS(ab);
                        // 能打隐形目标的能力（默认不能）
                        a.CanTargetInvisible |= ReadCanTargetInvisible(ab);
                        // 「优先攻击最远目标」的怪（CAIMoveNew.useFurthestFocus）
                        a.AttackFurthestTarget |= ReadAttackFurthestTarget(ab);
                        // 元素需求（只提示）
                        string els = ReadElementNeed(ab);
                        if (els.Length > 0 && !a.ElementNeed.Contains(els))
                        {
                            a.ElementNeed = a.ElementNeed.Length == 0 ? els : a.ElementNeed + "+" + els;
                        }
                        a.AttackStrengthBase = ab.Strength;
                        a.AttackStrengthActual = ab.Strength;
                        if (ab.Strength > a.AttackStrength) a.AttackStrength = ab.Strength;
                        if (ab.Range > a.AttackRange) a.AttackRange = ab.Range;
                        if (ab.NumberTargets > a.AttackTargets) a.AttackTargets = ab.NumberTargets;

                        // 穿透：CAbilityAttack.Pierce（护盾先减穿透）
                        CAbilityAttack atkAb = ab as CAbilityAttack;
                        if (atkAb != null && atkAb.Pierce > a.Pierce) a.Pierce = atkAb.Pierce;

                        // ★ 强度随场上条件变化时（StatIsBasedOnX），求出**实际**强度。
                        //   死行「深渊力量」= 1 + 阴影数（上限 5 → 最高攻击 6）。
                        //   不求值就按卡面的 1 算，这张死行最强的牌会被低估 6 倍。
                        if (a.StrengthIsVariable)
                        {
                            int actual = ResolveStatBasedOnXStrength(ab);
                            if (actual > 0)
                            {
                                a.AttackStrengthActual = actual;
                                if (actual > a.AttackStrength) a.AttackStrength = actual;
                            }
                        }

                        // 范围攻击：要按形状摆站位，且一次打好几个 —— 单体伤害模型算不准
                        if (HasAreaEffect(ab))
                        {
                            a.HasAreaEffect = true;
                            string shape = ReadAreaEffectShape(ab);
                            if (!string.IsNullOrEmpty(shape) &&
                                !a.AreaEffectShape.Contains(shape))
                            {
                                if (a.AreaEffectShape.Length > 0) a.AreaEffectShape += " + ";
                                a.AreaEffectShape += shape;
                            }
                        }
                    }

                    // ---- ★ 这一击会施加什么条件（缴械/中毒/静足…）----
                    //   玩家卡和怪物卡通用：怪物的行动卡也走这里，
                    //   所以敌人给我方上缴械这类情况也能被读到。
                    ReadAppliedConditions(ab, a);

                    // ---- ② 阴影机制（死行核心）—— 单独识别，不能一律丢进"未建模" ----
                    if (ClassifyShadowAbility(ab, typeName, a)) { /* 已识别 */ }
                    else if (isPoison || isPull || isControl || isTeleport)
                    {
                        // ---- ④ 附加效果：识别 + 提示（不精确计算后续回合）----
                        ClassifyExtraEffect(ab, typeName, a);
                    }
                    else if (isHeal)
                    {
                        // ---- ③ 治疗：支援职业的核心，之前一律算未建模 ----
                        // 日志实测 Heal 出现 401 次，是最高频未建模项。
                        a.HasHeal = true;
                        if (ab.Strength > a.HealStrength) a.HealStrength = ab.Strength;
                        if (ab.Range > a.HealRange) a.HealRange = ab.Range;
                        if (ab.NumberTargets > a.HealTargets) a.HealTargets = ab.NumberTargets;
                    }
                    else if (!isMove && !isAttack)
                    {
                        // 没建模的能力：记下来，推荐里必须告知玩家
                        if (!a.UnmodeledTypes.Contains(typeName)) a.UnmodeledTypes.Add(typeName);
                    }
                }

                a.XP = action.ActionXP;

                foreach (ElementInfusionBoardManager.EElement el in action.Infusions ?? new List<ElementInfusionBoardManager.EElement>())
                {
                    a.Extras.Add("注入" + el);
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取半边失败: {e.Message}");
            }

            // ---- 语义定档 ----
            if (abilities == 0) a.Kind = HalfKind.Empty;
            else if (persistent > 0 && persistent == abilities) a.Kind = HalfKind.PersistentBonus;
            else a.Kind = HalfKind.Action;

            return a;
        }

        /// <summary>
        /// 这个 ability 是不是"常驻加成"？
        /// 判据完全走游戏自己的数据：`ActiveBonusData != null && Duration != NA`。
        /// （实测「深渊力量」下半的两个 ability 都是 Duration: Persistent，
        ///   Behaviour 分别是 BuffMove / BuffSingleAttack —— 加的是"移动 +2""攻击 +2"，
        ///   Strength 字段是**加成幅度**，不是这一回合能移动/攻击的量。）
        /// </summary>
        private static bool TryReadPersistentBonus(CAbility ab, out string behaviour)
        {
            behaviour = null;
            try
            {
                if (PActiveBonusData == null) return false;
                object data = PActiveBonusData.GetValue(ab, null);
                if (data == null) return false;

                Type dt = data.GetType();

                FieldInfo fDur = dt.GetField("Duration", BindingFlags.Public | BindingFlags.Instance);
                object dur = fDur != null ? fDur.GetValue(data) : null;
                if (dur == null) return false;
                if (string.Equals(dur.ToString(), "NA", StringComparison.OrdinalIgnoreCase)) return false;

                FieldInfo fBeh = dt.GetField("Behaviours", BindingFlags.Public | BindingFlags.Instance);
                object behs = fBeh != null ? fBeh.GetValue(data) : null;

                var names = new List<string>();
                if (behs is IEnumerable seq && !(behs is string))
                {
                    foreach (object b in seq)
                    {
                        if (b != null) names.Add(b.ToString());
                    }
                }
                behaviour = names.Count > 0 ? string.Join("/", names) : "(未命名)";
                return true;
            }
            catch
            {
                behaviour = null;
                return false;
            }
        }

        /// <summary>常驻加成 → 人话。</summary>
        private static void DescribeBonus(string behaviour, CAbility ab, ActionSnapshot a)
        {
            string kind = behaviour.Contains("BuffMove") ? "移动"
                        : behaviour.Contains("BuffRange") ? "射程"
                        : behaviour.Contains("BuffTarget") ? "目标数"
                        : behaviour.Contains("BuffAttack") ? "攻击"
                        : behaviour.Contains("Shield") ? "护盾"
                        : behaviour.Contains("Retaliate") ? "反击"
                        : null;

            string text = kind != null
                ? $"常驻增益：{kind} +{ab.Strength}"
                : $"常驻增益：{behaviour}";

            if (!a.BonusTexts.Contains(text)) a.BonusTexts.Add(text);
        }

        private static bool HasAreaEffect(CAbility ab)
        {
            try
            {
                if (PAreaEffect == null) return false;
                return PAreaEffect.GetValue(ab, null) != null;
            }
            catch { return false; }
        }

        /// <summary>
        /// 求值「强度随场上条件变化」的能力，算出**实际**数值。
        ///
        /// 死行「深渊力量」上半：
        /// ```yaml
        /// TAttack:
        ///     Attack: 1
        ///     StatIsBasedOnX:
        ///         Strength: { BasedOn: Vars, AddTo: True,
        ///                     Vars: { Value: ScenarioManager.CurrentScenarioState.ShadowProps.Count } }
        /// ```
        /// ⇒ 实际 = 1 + 当前阴影数（阴影上限 5，所以最高是**攻击 6**）。
        /// 不求值就按卡面的 1 算，等于把这张死行最强的牌低估 6 倍。
        ///
        /// ⚠ 必须用**反射**：`AbilityData.StatIsBasedOnXData` 在
        ///   `ScenarioRuleLibrary.YML` 程序集里，而本项目**有意**只引用
        ///   `ScenarioRuleLibrary` + `SharedLibrary`（编译期引用规则库过深容易踩坑）。
        ///
        /// ⚠ 只能调 `GetStatIsBasedOnXValue`（**纯函数**），
        ///   绝不能调 `SetStatBasedOnX` —— 后者会写 `HasBeenAdded` 标记并改 Strength，
        ///   是有副作用的，会污染游戏状态。
        /// </summary>
        private static int ResolveStatBasedOnXStrength(CAbility ab)
        {
            try
            {
                object entriesObj = PStatIsBasedOnX?.GetValue(ab, null);
                var entries = entriesObj as System.Collections.IEnumerable;
                if (entries == null) return 0;

                int value = ab.Strength;
                bool changed = false;

                foreach (object entry in entries)
                {
                    if (entry == null) continue;

                    // 只关心 Strength（Range / RetaliateRange 等另说）
                    PropertyInfo statTypeProp = entry.GetType().GetProperty("AbilityStatType");
                    if (statTypeProp == null) continue;
                    object statType = statTypeProp.GetValue(entry, null);
                    if (statType == null || statType.ToString() != "Strength") continue;

                    PropertyInfo addToProp = entry.GetType().GetProperty("AddTo");
                    bool addTo = false;
                    if (addToProp != null)
                    {
                        object av = addToProp.GetValue(entry, null);
                        addTo = av is bool b && b;
                    }

                    int v = InvokeGetStatBasedOnX(ab, entry);
                    if (v == 0) continue;

                    value = addTo ? value + v : v;
                    changed = true;
                }

                return changed ? value : 0;
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] StatIsBasedOnX 求值失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>反射调 `CAbility.GetStatIsBasedOnXValue`（纯函数）。</summary>
        private static int InvokeGetStatBasedOnX(CAbility ab, object entry)
        {
            try
            {
                MethodInfo mi = typeof(CAbility).GetMethod("GetStatIsBasedOnXValue",
                    BindingFlags.Public | BindingFlags.Instance);
                if (mi == null) return 0;

                ParameterInfo[] ps = mi.GetParameters();
                if (ps.Length < 7) return 0;

                // 第 4 个参数是枚举值类型（basedOnOverride），传 0 = None
                object basedOnOverride = Enum.ToObject(ps[3].ParameterType, 0);
                object[] args =
                {
                    null,                 // targetingActor：表达式不依赖角色时够用
                    entry,
                    ab.AbilityFilter,
                    basedOnOverride,
                    1f, 1f,
                    null,                 // targetActor
                };

                object r = mi.Invoke(ab, args);
                return r is int i ? i : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 识别四类阴影能力（死行 / Deathwalker 的职业机制）。
        ///
        /// ⚠ 阴影**不是**光照阴影，是叠在地块上的道具 `CObjectShadow`。
        ///   之前这四类全被丢进 UnmodeledTypes —— 日志实测死行手牌里出现 560+ 次，
        ///   等于死行有一半的卡都"看不懂"，方案基本没法用。
        ///
        /// 四类（反编译实证）：
        ///   · CreateShadow  —— 造阴影（Strength = 个数；**上限 5**，见 CAbilityCreateShadow）
        ///   · MoveShadow    —— 挪动已有阴影（自己不动）
        ///   · RemoveShadow  —— 移除阴影
        ///   · PerformFromShadow —— 从阴影中发动（要先用掉 m_ShadowsToUse 个阴影）
        ///
        /// 造阴影的现实价值：**深渊力量 = 攻击 1 + 阴影数**（StatIsBasedOnX 已求值），
        /// 所以造一个阴影 ≈ 让后续攻击 +1。这一点在评分里给了可配权重。
        /// </summary>
        /// <returns>是否识别为阴影能力（true 就不该再算作"未建模"）</returns>
        private static bool ClassifyShadowAbility(CAbility ab, string typeName, ActionSnapshot a)
        {
            try
            {
                switch (typeName)
                {
                    case "CreateShadow":
                        a.HasCreateShadow = true;
                        if (ab.Strength > a.CreateShadowCount) a.CreateShadowCount = ab.Strength;
                        return true;

                    case "MoveShadow":
                        a.HasMoveShadow = true;
                        if (ab.Strength > a.MoveShadowCount) a.MoveShadowCount = ab.Strength;
                        return true;

                    case "RemoveShadow":
                        a.HasRemoveShadow = true;
                        if (ab.Strength > a.RemoveShadowCount) a.RemoveShadowCount = ab.Strength;
                        return true;

                    case "PerformFromShadow":
                        a.HasPerformFromShadow = true;
                        int need = ReadIntField(ab, "m_ShadowsToUse");
                        if (need > a.ShadowsToUse) a.ShadowsToUse = need;
                        if (a.ShadowsToUse <= 0) a.ShadowsToUse = 1;   // 至少要 1 个
                        a.ShadowRemoveWhenDone |= ReadBoolField(ab, "m_RemoveWhenDone");
                        return true;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static int ReadIntField(object obj, string field)
        {
            try
            {
                FieldInfo fi = obj.GetType().GetField(field,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null) return 0;
                object v = fi.GetValue(obj);
                return v is int i ? i : 0;
            }
            catch { return 0; }
        }

        private static bool ReadBoolField(object obj, string field)
        {
            try
            {
                FieldInfo fi = obj.GetType().GetField(field,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null) return false;
                object v = fi.GetValue(obj);
                return v is bool b && b;
            }
            catch { return false; }
        }

        /// <summary>
        /// 识别\"附加效果\"类能力：中毒 / 拉拽 / 控制 / 传送。
        /// 它们不改变本回合的位移与伤害模型，但会**影响后续回合**，
        /// 所以必须识别出来提示玩家，不能默默算进\"未建模\"就完了。
        /// </summary>
        private static void ClassifyExtraEffect(CAbility ab, string typeName, ActionSnapshot a)
        {
            switch (typeName)
            {
                case "Poison":
                    a.HasPoison = true;
                    if (ab.Strength > a.PoisonStrength) a.PoisonStrength = ab.Strength;
                    break;
                case "Pull":
                    a.HasPull = true;
                    if (ab.Strength > a.PullStrength) a.PullStrength = ab.Strength;
                    break;
                case "ControlActor":
                    a.HasControlActor = true;
                    break;
                case "Teleport":
                case "Teleport2":
                    a.HasTeleport = true;
                    break;
            }
        }

        /// <summary>
        /// 读这个能力**会施加**给目标的条件。
        ///
        /// 反编译实证：`CAbility.NegativeConditions` 是
        /// `Dictionary&lt;CCondition.ENegativeCondition, CAbility&gt;` ——
        /// 键就是它施加的条件类型。
        ///
        /// ★ 怪物卡也走这条路径（HandReader.ReadAction 同时用于玩家与怪物），
        ///   所以"敌人先手给我上缴械"能被读到。
        /// </summary>
        private static void ReadAppliedConditions(CAbility ab, ActionSnapshot a)
        {
            try
            {
                if (ab == null || a == null) return;

                // 负面条件
                if (PNegativeConditions != null)
                {
                    object v = PNegativeConditions.GetValue(ab, null);
                    if (v is System.Collections.IDictionary dict)
                    {
                        foreach (object key in dict.Keys)
                        {
                            if (key == null) continue;
                            string name = key.ToString();
                            if (name.Length > 0 && !a.AppliesNegative.Contains(name))
                                a.AppliesNegative.Add(name);
                        }
                    }
                }

                // 正向条件
                if (PPositiveConditions != null)
                {
                    object v = PPositiveConditions.GetValue(ab, null);
                    if (v is System.Collections.IDictionary dict)
                    {
                        foreach (object key in dict.Keys)
                        {
                            if (key == null) continue;
                            string name = key.ToString();
                            if (name.Length > 0 && !a.AppliesPositive.Contains(name))
                                a.AppliesPositive.Add(name);
                        }
                    }
                }
            }
            catch
            {
                // 读不到就不记，不影响主流程
            }
        }

        /// <summary>读 `MiscAbilityData.CanTargetInvisible`（bool? 字段，读不到当 false）。</summary>
        private static bool ReadCanTargetInvisible(CAbility ab)
        {
            return ReadMiscBool(ab, "CanTargetInvisible");
        }

        /// <summary>读 `ElementsToInfuse`，返回如 "Fire" / "Fire+Water"；无需求返回空。</summary>
        private static string ReadElementNeed(CAbility ab)
        {
            try
            {
                if (ab == null) return "";

                // 按运行时类型找 —— ElementsToInfuse 在 CAbilityInfuse 上
                PropertyInfo pi = ab.GetType().GetProperty("ElementsToInfuse",
                    BindingFlags.Public | BindingFlags.Instance);
                if (pi == null) return "";      // 不是注入类能力，正常

                object v = pi.GetValue(ab, null);
                if (v == null) return "";

                var names = new List<string>();
                foreach (object e in (System.Collections.IEnumerable)v)
                {
                    if (e == null) continue;
                    string n = e.ToString();
                    if (n.Length > 0 && !names.Contains(n)) names.Add(n);
                }
                return names.Count == 0 ? "" : string.Join("+", names);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>读 `MiscAbilityData.AttackFurthestTarget`（"优先打最远目标"，读不到当 false）。</summary>
        private static bool ReadAttackFurthestTarget(CAbility ab)
        {
            return ReadMiscBool(ab, "AttackFurthestTarget");
        }

        /// <summary>读 `MiscAbilityData.IgnoreLOS`（读不到当 false）。</summary>
        private static bool ReadIgnoreLOS(CAbility ab)
        {
            return ReadMiscBool(ab, "IgnoreLOS");
        }

        /// <summary>
        /// 读 `MiscAbilityData` 上的一个布尔开关 —— **字段或属性都认**，`bool` / `bool?` 都认。
        ///
        /// ⚠ v1.35 修掉的静默 bug：这三个开关在反编译里都是**公开字段**
        ///   （`AttackFurthestTarget` / `IgnoreLOS` 是 bool，`CanTargetInvisible` 是 bool?），
        ///   而旧代码用 `GetProperty` 取 → 永远 null → 返回 false。
        ///   于是「忽略视线」「可打隐形」「打最远目标」三条从 v1.19 起一直**没生效**，
        ///   离线测试却全绿（测试直接构造 ActionSnapshot，不经过反射）。
        /// </summary>
        private static bool ReadMiscBool(CAbility ab, string name)
        {
            try
            {
                if (ab == null || PMiscAbilityData == null) return false;
                object misc = PMiscAbilityData.GetValue(ab, null);
                if (misc == null) return false;

                Type t = misc.GetType();
                FieldInfo fi = t.GetField(name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    object fv = fi.GetValue(misc);
                    return fv is bool b && b;      // bool? 的 null → false
                }

                PropertyInfo pi = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (pi != null)
                {
                    object pv = pi.GetValue(misc, null);
                    return pv is bool b2 && b2;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 这个 ability 的强度/距离是否随场上条件变化（`CAbility.StatIsBasedOnXEntries`）。
        /// YAML 里的 `StatIsBasedOnX` —— 如死行「深渊力量」上半
        /// `Attack: 1 + ScenarioManager.CurrentScenarioState.ShadowProps.Count`。
        /// 卡面写 1，满 5 个阴影时实际是 6。
        /// </summary>
        private static bool HasStatIsBasedOnX(CAbility ab)
        {
            try
            {
                if (PStatIsBasedOnX == null) return false;
                object v = PStatIsBasedOnX.GetValue(ab, null);
                if (v == null) return false;
                var seq = v as System.Collections.IEnumerable;
                if (seq == null) return false;
                foreach (object o in seq)
                {
                    if (o != null) return true;   // 有一条就算
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读「移动落脚限制」：`CAbilityMove.MoveRestrictionType`。
        /// 用反射而不是编译期转型，拿不到就当 None（保守：按无限制处理会误推荐，
        /// 所以这里拿不到时**也按受限**处理，宁可不枚举落点）。
        /// </summary>
        private static string ReadMoveRestriction(CAbility ab)
        {
            try
            {
                if (ab == null) return "None";

                // 按运行时类型找 —— MoveRestrictionType 是 CAbilityMove 的属性
                PropertyInfo pi = ab.GetType().GetProperty("MoveRestrictionType",
                    BindingFlags.Public | BindingFlags.Instance);
                if (pi == null) return "None";      // 不是移动类能力，正常

                object v = pi.GetValue(ab, null);
                return v != null ? v.ToString() : "None";
            }
            catch
            {
                return "None";
            }
        }

        private static string ReadAreaEffectShape(CAbility ab)
        {
            try
            {
                if (PAreaEffectYml == null) return null;
                return PAreaEffectYml.GetValue(ab, null) as string;
            }
            catch { return null; }
        }

        // ---------- 通用基础动作（Attack2 / Move2）----------

        /// <summary>
        /// 通用「基础攻击 2」—— 任何一张牌都能不当卡面能力用、改当基础攻击。
        /// 数值从 `card.DefaultAttackAction` 里读（正常是 攻2/射程1/单体），
        /// 不写死：增益 / 道具改了这个数值时，读出来的才是对的。
        /// </summary>
        private static void ReadDefaultActions(CAbilityCard card, AbilityCardSnapshot snap)
        {
            try
            {
                CAction atk = card.DefaultAttackAction;
                if (atk != null)
                {
                    snap.HasDefaultAttack = true;
                    snap.DefaultAttackXP = atk.ActionXP;
                    foreach (CAbility ab in atk.Abilities ?? new List<CAbility>())
                    {
                        if (ab == null) continue;
                        if (ab.AbilityType == CAbility.EAbilityType.Attack)
                        {
                            if (ab.Strength > 0) snap.DefaultAttackStrength = ab.Strength;
                            if (ab.Range > 0) snap.DefaultAttackRange = ab.Range;
                        }
                    }
                }
            }
            catch { /* 读不到就用默认值 2/1 */ }

            try
            {
                CAction mv = card.DefaultMoveAction;
                if (mv != null)
                {
                    snap.HasDefaultMove = true;
                    snap.DefaultMoveXP = mv.ActionXP;
                    foreach (CAbility ab in mv.Abilities ?? new List<CAbility>())
                    {
                        if (ab == null) continue;
                        if (ab.AbilityType == CAbility.EAbilityType.Move && ab.Strength > 0)
                    {
                            snap.DefaultMoveStrength = ab.Strength;
                    }
                }
                }
            }
            catch { /* 读不到就用默认值 2 */ }
        }

        /// <summary>合成一个「基础攻击」动作快照（不引用游戏类型）。</summary>
        public static ActionSnapshot MakeDefaultAttack(AbilityCardSnapshot card)
        {
            var a = new ActionSnapshot
            {
                Side = "基础攻击",
                Kind = card != null && card.HasDefaultAttack ? HalfKind.Action : HalfKind.Empty,
                IsDefaultAction = true,
                DefaultLabel = "基础攻击",
                HasAttack = card != null && card.HasDefaultAttack,
                AttackStrength = card != null ? card.DefaultAttackStrength : 2,
                AttackRange = card != null ? card.DefaultAttackRange : 1,
                AttackTargets = 1,
                XP = card != null ? card.DefaultAttackXP : 0,
            };
            a.AbilityTypes.Add("DefaultAttack");
            return a;
        }

        /// <summary>合成一个「基础移动」动作快照（不引用游戏类型）。</summary>
        public static ActionSnapshot MakeDefaultMove(AbilityCardSnapshot card)
        {
            var a = new ActionSnapshot
            {
                Side = "基础移动",
                Kind = card != null && card.HasDefaultMove ? HalfKind.Action : HalfKind.Empty,
                IsDefaultAction = true,
                DefaultLabel = "基础移动",
                HasMove = card != null && card.HasDefaultMove,
                MoveStrength = card != null ? card.DefaultMoveStrength : 2,
                XP = card != null ? card.DefaultMoveXP : 0,
            };
            a.AbilityTypes.Add("DefaultMove");
            return a;
        }

        /// <summary>
        /// 计算某单位的移动可达格。直接复用游戏的 A* 洪水填充（见 Geometry.Reachable）。
        /// 返回可达格坐标列表；失败返回空列表（不会抛到调用方）。
        /// </summary>
        public static List<(int X, int Y, int Hazard)> ReachableTiles(CActor actor, int moveRange)
        {
            if (actor == null || actor.ArrayIndex == null) return new List<(int, int, int)>();
            return Geometry.Reachable(actor, actor.ArrayIndex.X, actor.ArrayIndex.Y, moveRange);
        }

        /// <summary>当前地图的寻路器是否已就绪（不在战斗地图时为 null）。</summary>
        public static bool PathFinderReady
        {
            get
            {
                try { return ScenarioManager.PathFinder != null; }
                catch { return false; }
            }
        }
    }
}

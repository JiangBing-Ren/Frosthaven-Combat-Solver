using System;
using System.Collections.Generic;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>P1 评分权重。数值是"量纲换算"，不是魔法数字，可随实测调。</summary>
    public sealed class ScoringWeights
    {
        /// <summary>一次击杀的额外价值（清掉一个行动位，收益远大于等额伤害）。</summary>
        public double KillBonus = 25.0;

        /// <summary>
        /// 1 点经验的权重。**刻意压低**：XP 只来自"打出的牌"，
        /// 而一张啥都不干的牌也可能给 XP —— 权重一高就会出现
        /// 「0 伤害 1 经验」打赢「3 伤害」的荒谬排名（实测踩过）。
        /// </summary>
        public double XpWeight = 1.0;

        /// <summary>有效伤害的权重（溢出伤害不计，见 EffectiveDamage）。</summary>
        public double DamageWeight = 1.0;

        /// <summary>
        /// 自伤（危险地形）的权重。1.0 = 吃 1 点血扣 1 分。
        /// 调到 0 就等于忽略地形伤害（不建议）。
        /// </summary>
        public double SelfHazardWeight = 1.0;

        /// <summary>
        /// 多样性分桶：同一个"打法签名"最多保留几个方案。
        /// 0 = 关闭（纯按分数取前 N）。默认 2。
        /// </summary>
        public int DiversityPerBucket = 2;

        /// <summary>
        /// **生存权重**：站到会被打的格子上，每点预计伤害扣多少分。
        ///
        /// 默认 2.0 —— 挨 3 刀（-6 分）比多打 6 点伤害（+6 分）更糟。
        /// 这个权重体现 StS2 求解器的优先级：生存 &gt; 伤害。
        /// 调到 0 就退回"只看伤害"的旧行为。
        /// </summary>
        public double SurvivalWeight = 2.0;

        /// <summary>
        /// 治疗收益的权重。1.0 = 1 点治疗 = 1 分。
        /// 治疗的价值低于等量伤害（不推进击杀、不解威胁），默认 0.8。
        /// </summary>
        public double HealWeight = 0.8;

        /// <summary>
        /// 造一个阴影的价值（死行）。默认 2.0 ≈ 相当于 2 点伤害。
        /// 理由：阴影让「深渊力量」的攻击 +1（上限 5），是持续收益；
        /// 调大 = 更倾向先铺阴影，调 0 = 完全不管阴影。
        /// </summary>
        public double ShadowCreateWeight = 2.0;

        /// <summary>
        /// 战役目标价值的权重（P2）。
        /// LootSpot.Value 本身已经按用户的优先级链换算过了（宝箱/装备 120、材料 15），
        /// 这里就是 1.0 —— 想让"捡东西"压过战斗价值就调大它。
        /// 使命目标目前不自动计分（22 种类型，只做提示），见 SOLVER_DESIGN.md P2 说明。
        /// </summary>
        public double GoalWeight = 1.0;

        // 注意：**没有** LostPenalty。
        // 曾经设成线性惩罚（14，后来 5），两次都踩同一个坑：
        //   「4 伤害 + 失去一半张牌」得分 4-5 = -1，输给「什么都不做」的 0 分。
        // 也就是说线性惩罚会让"打但亏"排在"不打"前面 —— 这不可能对。
        // Lost 是**本回合之后**的战役级代价（牌组资源），属于 P2 的战役目标评分，
        // P1 只优化本回合战斗价值，Lost 降级成排序时的次级条件（见 Enumerate 的 Sort），
        // 同时在方案文本里用 ⚠ 显式告知，玩家想省卡就自己挑 #2。
    }

    /// <summary>
    /// 一张牌能提供的**一个**候选动作。
    ///
    /// 这是 v1.3 的关键改动：以前求解器写死"一张牌出上半、另一张出下半"，
    /// 于是 (上,上)、(下,下) 这些合法组合根本枚举不到 —— 方案自然不是最优。
    /// 现在一张牌的候选是：上半能力 / 下半能力 / 基础攻击 2 / 基础移动 2。
    /// </summary>
    public sealed class ActionOption
    {
        /// <summary>出了手牌里的第几张（基础动作也记，表示弃的是哪张）。</summary>
        public int CardIndex = -1;

        /// <summary>牌的本地化显示名。</summary>
        public string CardName = "?";

        /// <summary>牌的内部名（调试用）。</summary>
        public string CardInternalName = "?";

        /// <summary>用的是上半 / 下半 / 基础攻击 / 基础移动。</summary>
        public HalfSource Source = HalfSource.Top;

        /// <summary>归一化后的可执行动作内容（基础动作这里是合成的）。</summary>
        public ActionSnapshot Half;

        /// <summary>用这一半会不会让这张牌永久失去（基础动作不会，只是进弃牌堆）。</summary>
        public bool IsLost;

        /// <summary>这是通用基础动作（Attack2 / Move2），不是卡面能力。</summary>
        public bool IsDefaultAction => Source == HalfSource.DefaultAttack
                                    || Source == HalfSource.DefaultMove;

        /// <summary>缓存/去重用的键：牌下标 + 动作来源。</summary>
        public string Key => CardIndex + ":" + (int)Source;
    }

    /// <summary>
    /// P1 的输入。全部是 POCO + 三个委托 —— Solver 层仍然不引用任何游戏类型，
    /// 地图查询由 Bridge 层注入（内部直接调游戏自己的 PathFinder / HaveLOS）。
    /// </summary>
    public sealed class P1Context
    {
        public string ActorName = "?";
        public int StartX;
        public int StartY;

        /// <summary>我方当前血量 / 最大血量（多回合推演 P5 要用）。</summary>
        public int StartHealth;
        public int StartMaxHealth;

        /// <summary>手牌（快照）。下标即输出里的 CardIndex。</summary>
        public List<AbilityCardSnapshot> Hand = new List<AbilityCardSnapshot>();

        /// <summary>该角色的修正牌堆（Remaining 顺序即未来翻牌顺序）。</summary>
        public ModifierDeckSnapshot Deck;

        /// <summary>
        /// 怪物的四副**共享**修正牌堆（P5 敌人真实抽牌要用）。
        /// 顺序固定：0=敌方 1=友方 2=敌方2 3=中立，与 FactionKind 的 DeckIndex 对应。
        /// 同阵营的怪物**共用一副**，所以多个敌人一回合内的抽牌会按先攻顺序依次消耗同一副。
        /// </summary>
        public List<ModifierDeckSnapshot> MonsterDecks = new List<ModifierDeckSnapshot>();

        /// <summary>
        /// **按我的先攻分组**的威胁信息 → 我的先攻 → 威胁。
        ///
        /// ★ 为什么按先攻分组：方案可以**换先攻**（用另一张牌的高先攻），
        ///   换了先攻就可能避开敌人的先手 —— 这正是"先攻编织"的价值。
        ///   所以威胁必须按**方案的先攻**查，而不是按角色的先攻。
        ///
        /// ★ 这是"生存维度"的核心：之前评分只看伤害，导致推荐出
        ///   "站到敌人面前挨一刀"的方案 —— 看着伤害高，实际是废招。
        ///
        /// 参考 StS2 的 Combat Solver：优先级是 **生存 &gt; 确认胜利 &gt; 最小化总战损**，
        /// 生存排第一。本项目的等价物就是"别站到会被打的地方"。
        /// </summary>
        public Dictionary<int, ThreatInfo> ThreatByInitiative;

        /// <summary>某个先攻下的威胁详情。</summary>
        public sealed class ThreatInfo
        {
            /// <summary>会挨打的格子 → 预计伤害。</summary>
            public Dictionary<(int X, int Y), int> DamagedCells =
                new Dictionary<(int X, int Y), int>();

            /// <summary>敌人会给我上的**负面条件**（Disarm 缴械 / Immobilize 静足 / Stun 眩晕…）。</summary>
            public List<string> AppliedConditions = new List<string>();

            /// <summary>对这个先攻来说，我是不是完全没有安全格（到处都在打）。</summary>
            public bool EverywhereDangerous;
        }

        /// <summary>取某个先攻下的威胁信息（没有就返回 null）。</summary>
        public ThreatInfo ThreatFor(int initiative)
        {
            if (ThreatByInitiative == null) return null;
            ThreatInfo t;
            return ThreatByInitiative.TryGetValue(initiative, out t) ? t : null;
        }

        /// <summary>
        /// 我方当前带有的**负面条件**（Disarm 缴械 / Immobilize 静足 / Stun 眩晕 …）。
        /// 直接决定哪些半边的动作能不能用 —— 详见 StateRequirementMet。
        /// </summary>
        public List<string> MyNegativeConditions = new List<string>();

        /// <summary>我方当前带有的正面条件（Strengthen / Ward / Regenerate …）。</summary>
        public List<string> MyPositiveConditions = new List<string>();

        /// <summary>
        /// 场上**当前**的阴影数量（死行机制，上限 5）。
        /// 「深渊力量」攻击 = 1 + 阴影数，所以造阴影 = 抬后续攻击力。
        /// </summary>
        public int ShadowCount;

        /// <summary>
        /// 地上可捡拾的东西（宝箱 / 战利品袋 / 资源）与它们的价值 —— P2 战役目标。
        /// 方案的**终点**踩上去就能捡，所以直接按位置匹配给分。
        /// 价值已按优先级链换算（宝箱/装备 120，材料 15），见 MISSION_AND_LOOT.md §6。
        /// </summary>
        public List<LootSpot> LootSpots = new List<LootSpot>();

        /// <summary>存活怪物（敌对，可作攻击目标）。</summary>
        public List<UnitSnapshot> Monsters = new List<UnitSnapshot>();

        /// <summary>
        /// 场上其他单位（盟友 / 中立 / 召唤物 / 队友）—— 只用于展示和占位，**绝不是攻击目标**。
        /// 曾把 AllAliveMonsters 直接当敌人列表，而它含 AllyMonsters / NeutralMonsters，
        /// 导致把盟友当敌人打。现在按 CActor.AreActorsAllied 严格分开。
        /// </summary>
        public List<UnitSnapshot> Others = new List<UnitSnapshot>();

        /// <summary>
        /// 被占用的格子（敌人 / 后动的队友 / 盟友 / 召唤物 / 倒地单位）。
        /// 移动落点必须排除 —— 走到敌人占的那格这种方案根本执行不了（实测出现过）。
        ///
        /// ⚠ 这里**不是**"所有人当前位置"：先攻更低、会先动的队友，
        ///   占用的是他们方案跑完之后站的位置（由 Bridge 层算好传进来）。
        ///   之前一律按当前位置占，于是旗矛被"大棒会停在 (14,4)"挡住 ——
        ///   可实际上大棒「义无反顾」打完还能再走两步，根本不会停那儿。
        /// </summary>
        public HashSet<(int X, int Y)> Occupied = new HashSet<(int, int)>();

        public AdvStatus Adv = AdvStatus.None;

        /// <summary>从 (x,y) 出发、移动力 range 的可达格（含自身）。</summary>
        public Func<int, int, int, List<(int X, int Y, int Hazard)>> Reachable;

        /// <summary>射程距离（复刻 IsTileWithinRange）。</summary>
        public Func<int, int, int, int, int> RangeDistance;

        /// <summary>视线（游戏 CActor.HaveLOS）。</summary>
        public Func<int, int, int, int, bool> HasLOS;

        /// <summary>
        /// 该格是不是阴影格（游戏 `CTile.FindPropOfType<CObjectShadow>() != null`）。
        /// 死行「暗影步法」上半 `MustEndOnShadow` 要用它过滤落点。
        /// 为 null 表示当前拿不到，受限移动会退化成"原地不动"。
        /// </summary>
        public Func<int, int, bool> IsShadowTile;

        public ScoringWeights W = new ScoringWeights();
    }

    /// <summary>
    /// P1：单角色 × 单回合穷举。
    ///
    /// 枚举空间：
    ///   出牌 = 有序对 (第 i 张先出, 第 j 张后出)，i≠j               → n(n−1)
    ///   × 第 i 张用哪个候选动作（上 / 下 / 基础攻击 / 基础移动）        → ≤4
    ///   × 第 j 张用哪个候选动作                                        → ≤4
    ///   × 第一个动作的落点（含原地不动）                                → ≤60
    ///   × 第一个动作的目标                                              → ≤4
    ///   × 第二个动作的落点                                              → ≤60
    ///   × 第二个动作的目标                                              → ≤4
    ///
    /// 靠三处收敛把 10^8 压到 10^5：
    ///   ① 可达格 / 射程 / 视线结果缓存（跨出牌组合复用）
    ///   ② 「某动作从某格打某目标」的评估缓存（与出牌组合无关）
    ///   ③ **落点剪枝**：攻击动作只考虑「能打到东西」的落点 + 原地不动；
    ///      无攻击的动作若排在攻击动作之前，也只保留「能从那里走到可攻击位置」的落点
    ///
    /// 未建模（明确标注，不假装算出来）：
    ///   - 多目标 / 范围攻击（AreaEffect 或 NumberTargets != 1）
    ///   - 常驻加成（ActiveBonus，Duration != NA）只展示，不进评分
    ///   - 半边内的非攻击效果（治疗、护盾、元素注入、推拉）只记 XP，不进评分
    ///   - 半边内「先攻击后移动」的次序（统一按先移动后攻击）
    /// </summary>
    public static class P1Planner
    {
        /// <summary>某个半边、从某个位置、打某个目标的一次攻击评估。</summary>
        private sealed class AttackOnTarget
        {
            public AttackChoice Choice;
            public int Consumed;
            public double LocalValue;
        }


        /// <summary>
        /// 一张牌能填「上半位」的候选动作。
        ///
        /// ⚠ 规则：一回合打出的两张牌**必须一张用上半、一张用下半**。
        ///   上一版我把枚举"一般化"成每张牌可任选上下，那是**错的** ——
        ///   它会产生 (上,上)、(下,下) 这类游戏里根本不存在的组合。
        ///   所以这里按槽位分开：上半位只能取「卡面上半」或「基础攻击 2」。
        /// </summary>
        private static List<ActionOption> UpperOptions(P1Context ctx, int cardIndex, int atkCarrier)
        {
            var list = new List<ActionOption>();
            AbilityCardSnapshot c = ctx.Hand[cardIndex];
            if (c == null) return list;

            if (IsUsableHalf(c.Top) && ShadowRequirementMet(c.Top, ctx)
                && StateRequirementMet(c.Top, ctx))
            {
                list.Add(new ActionOption
                {
                    CardIndex = cardIndex,
                    CardName = c.DisplayName,
                    CardInternalName = c.Name,
                    Source = HalfSource.Top,
                    Half = c.Top,
                    IsLost = c.TopLost,
                });
            }
            // ⚠ 基础攻击也是 attack action，缴械时同样不能用 —— 不过滤就漏了
            if (cardIndex == atkCarrier && c.DefaultAttack != null && c.DefaultAttack.IsAction
                && StateRequirementMet(c.DefaultAttack, ctx))
            {
                list.Add(new ActionOption
                {
                    CardIndex = cardIndex,
                    CardName = c.DisplayName,
                    CardInternalName = c.Name,
                    Source = HalfSource.DefaultAttack,
                    Half = c.DefaultAttack,
                    IsLost = false,
                });
            }
            return list;
        }

        /// <summary>
        /// 一张牌能填「下半位」的候选动作。
        /// 上半位取「卡面上半」或「基础攻击 2」，下半位取「卡面下半」或「基础移动 2」。
        /// </summary>
        private static List<ActionOption> LowerOptions(P1Context ctx, int cardIndex, int mvCarrier)
        {
            var list = new List<ActionOption>();
            AbilityCardSnapshot c = ctx.Hand[cardIndex];
            if (c == null) return list;

            if (IsUsableHalf(c.Bottom) && ShadowRequirementMet(c.Bottom, ctx)
                && StateRequirementMet(c.Bottom, ctx))
            {
                list.Add(new ActionOption
                {
                    CardIndex = cardIndex,
                    CardName = c.DisplayName,
                    CardInternalName = c.Name,
                    Source = HalfSource.Bottom,
                    Half = c.Bottom,
                    IsLost = c.BottomLost,
                });
            }
            if (cardIndex == mvCarrier && c.DefaultMove != null && c.DefaultMove.IsAction
                && StateRequirementMet(c.DefaultMove, ctx))
            {
                list.Add(new ActionOption
                {
                    CardIndex = cardIndex,
                    CardName = c.DisplayName,
                    CardInternalName = c.Name,
                    Source = HalfSource.DefaultMove,
                    Half = c.DefaultMove,
                    IsLost = false,
                });
            }
            return list;
        }

        /// <summary>这一半能不能被"执行"（不是空的、不是常驻加成）。</summary>
        private static bool IsUsableHalf(ActionSnapshot h)
        {
            return h != null && h.Kind == HalfKind.Action;
        }

        /// <summary>
        /// 这一半的**状态前提**当前是否满足。
        ///
        /// 实战问题（用户反馈）：敌人精英冰角祭司**先于旗矛出手**并施加缴械，
        /// 求解器却仍推荐攻击 —— 整个方案直接失效。
        ///
        /// 三条核心规则（`CCondition.ENegativeCondition`）：
        ///   · Disarm     缴械 → 攻击类半边用不了
        ///   · Immobilize 静足 → 移动类半边用不了（不产生位移）
        ///   · Stun       眩晕 → 这半边整个用不了
        ///
        /// ⚠ 反过来，**纯治疗 / 纯上状态**的半边不受缴械影响 ——
        ///   缴械只禁攻击，不禁其他动作。
        /// </summary>
        private static bool StateRequirementMet(ActionSnapshot h, P1Context ctx)
        {
            if (h == null || ctx == null) return true;

            var neg = ctx.MyNegativeConditions;
            if (neg == null || neg.Count == 0) return true;

            bool disarmed = neg.Contains("Disarm");
            bool immobilized = neg.Contains("Immobilize");
            bool stunned = neg.Contains("Stun");

            if (stunned) return false;                       // 眩晕：什么都干不了

            if (disarmed && h.HasAttack) return false;       // 缴械：不能攻击
            if (immobilized && h.HasMove) return false;      // 静足：不能移动

            return true;
        }

        /// <summary>
        /// 这一半的**阴影前提**当前是否满足（死行）。
        ///
        /// 实战问题：使命开局场上一个阴影都没有，求解器却照样推荐
        /// 「暗影步法」（MustEndOnShadow）和「从阴影中发动」类的牌 —— 根本用不了。
        ///
        /// 两条前提（反编译实证）：
        ///   · MustEndOnShadow：移动必须停在阴影格 → 得**有阴影可落脚**
        ///   · PerformFromShadow：需要先有 m_ShadowsToUse 个阴影
        ///
        /// ⚠ 反过来，「造阴影」类半边**不受此限** —— 它正是没有阴影时的解法。
        /// </summary>
        private static bool ShadowRequirementMet(ActionSnapshot h, P1Context ctx)
        {
            if (h == null) return true;

            bool mustEndOnShadow = h.HasMove && h.MoveRestriction == "MustEndOnShadow";
            bool performFromShadow = h.HasPerformFromShadow;

            if (!mustEndOnShadow && !performFromShadow) return true;

            int cur = ctx != null ? ctx.ShadowCount : 0;

            // 站在阴影上时，MustEndOnShadow 不需要移动就满足
            if (mustEndOnShadow)
            {
                bool onShadow = ctx != null && ctx.IsShadowTile != null
                                && ctx.IsShadowTile(ctx.StartX, ctx.StartY);
                if (!onShadow && cur <= 0) return false;       // 没阴影可落脚
            }

            if (performFromShadow && cur < h.ShadowsToUse) return false;

            return true;
        }

        /// <summary>
        /// 这一格有没有可捡拾的东西（P2 战役目标）。
        /// 剪枝时用它放行：打不到人没关系，能捡到宝箱就值得走过去。
        /// </summary>
        private static bool HasSpotValue(P1Context ctx, int x, int y)
        {
            if (ctx == null || ctx.LootSpots == null) return false;
            foreach (LootSpot sp in ctx.LootSpots)
            {
                if (sp != null && sp.X == x && sp.Y == y && sp.Value > 0) return true;
            }
            return false;
        }

        /// <summary>
        /// 把 AOE 形状串翻译成"要几个人站哪"的人话（Frosthaven 官方字母）：
        ///   G = 灰格，自己必须站这儿
        ///   B = 蓝格，**每个蓝格都必须有盟友站着**，缺一个这个能力就发动不了
        ///   R = 红格，敌人格；**不必填满**，只要有 ≥1 个有效目标即可
        /// </summary>
        public static string FormationRequirement(string shape)
        {
            if (string.IsNullOrEmpty(shape)) return "";

            int g = 0, b = 0, r = 0;
            foreach (string part in shape.Split('|'))
            {
                string t = part.Trim();
                if (t.Length == 0) continue;
                char c = t[t.Length - 1];
                if (c == 'G') g++;
                else if (c == 'B') b++;
                else if (c == 'R') r++;
            }
            if (g == 0 && b == 0 && r == 0) return "";

            var bits = new List<string>();
            if (g > 0) bits.Add("你站灰格(G)");
                            if (g > 0) bits.Add("你站灰格(G)");
                // 攻略实证（旗矛 Resolved Courage）：蓝格**不限于角色** ——
                //   "This can be another character, one of your banners, or another summon."
                //   旗帜、其他召唤物都算。
                if (b > 0) bits.Add($"**每个蓝格(B)都要有单位**共 {b} 个 —— 盟友/旗帜/召唤物都算，缺一个就发动不了");
            if (r > 0) bits.Add($"最多覆盖 {r} 个敌人格(R)，至少 1 个目标即可");
            return "阵型要求：" + string.Join("；", bits) + "。";
        }

        /// <summary>
        /// 阴影操作的诊断文本（死行）。
        /// 上限 5，造阴影会抬高「深渊力量」的攻击力（1 + 阴影数）。
        /// </summary>
        public static string ShadowText(ActionSnapshot half, P1Context ctx)
        {
            if (half == null || !half.IsShadowAction) return "";

            var bits = new List<string>();
            int cur = ctx != null ? ctx.ShadowCount : 0;

            if (half.HasCreateShadow)
            {
                int room = 5 - cur;
                if (room <= 0)
                {
                    bits.Add($"**造阴影 {half.CreateShadowCount} 个** → ⚠ 已达上限 5，造不了");
                }
                else
                {
                    int n = Math.Min(half.CreateShadowCount, room);
                    bits.Add($"**造 {n} 个阴影**（当前 {cur}/5）→ 后续「深渊力量」攻击 +{n}");
                }
            }
            if (half.HasMoveShadow)
            {
                bits.Add($"移动 {half.MoveShadowCount} 个阴影（自己不动）");
            }
            if (half.HasRemoveShadow)
            {
                bits.Add($"移除 {half.RemoveShadowCount} 个阴影");
            }
            if (half.HasPerformFromShadow)
            {
                string need = $"需要先有 {half.ShadowsToUse} 个阴影";
                string rm = half.ShadowRemoveWhenDone ? "，用完会消耗掉" : "";
                if (cur < half.ShadowsToUse)
                {
                    bits.Add($"**从阴影中发动** → ⚠ 当前只有 {cur} 个，不够 {need}");
                }
                else
                {
                    bits.Add($"**从阴影中发动**（{need}{rm}）");
                }
            }

            return bits.Count == 0 ? "" : "阴影：" + string.Join("；", bits) + "。";
        }

        /// <summary>这一半因为什么被状态挡住（诊断用）。</summary>
        private static string StateBlockReason(ActionSnapshot h, P1Context ctx)
        {
            var neg = ctx?.MyNegativeConditions;
            if (neg == null) return "状态前提不满足";

            if (neg.Contains("Stun")) return "处于**眩晕**，什么都做不了";
            if (neg.Contains("Disarm") && h.HasAttack)
            {
                return "处于**缴械**，不能攻击（治疗/辅助类动作仍可用）";
            }
            if (neg.Contains("Immobilize") && h.HasMove)
            {
                return "处于**静足**，不能移动";
            }
            return "状态前提不满足";
        }

        /// <summary>
        /// 这一半最多能奶回多少血。
        ///
        /// 目标 = 友方（含我自己），按"缺失血量"降序取前 HealTargets 个，
        /// 每个只算 min(治疗量, 缺失血量) —— **过量治疗不计**
        /// （奶满血队友等于浪费，和有效伤害同理）。
        ///
        /// ⚠ 不枚举具体治疗谁：治疗通常是"奶最残的那个"，
        ///   枚举目标会让组合数翻倍，而收益差异很小。
        /// </summary>
        private static int BestHealValue(P1Context ctx, ActionSnapshot half)
        {
            if (ctx == null || half == null || !half.HasHeal) return 0;
            if (half.HealStrength <= 0) return 0;

            int need = half.HealTargets > 0 ? half.HealTargets : 1;

            // 候选：我自己 + 所有非敌对单位
            var cands = new List<int>();
            int myMissing = ctx.StartMaxHealth - ctx.StartHealth;
            if (myMissing > 0) cands.Add(myMissing);

            if (ctx.Others != null)
            {
                foreach (UnitSnapshot o in ctx.Others)
                {
                    if (o == null || o.IsHostileToPlayer) continue;
                    int miss = o.MaxHealth - o.Health;
                    if (miss > 0) cands.Add(miss);
                }
            }
            if (cands.Count == 0) return 0;      // 大家都满血，治疗没收益

            cands.Sort((a, b) => b.CompareTo(a));   // 缺失多的优先

            int total = 0;
            for (int i = 0; i < cands.Count && i < need; i++)
            {
                total += Math.Min(half.HealStrength, cands[i]);
            }
            return total;
        }

        /// <summary>这一半因为什么被阴影前提挡住（诊断用）。</summary>
        private static string ShadowBlockReason(ActionSnapshot h, P1Context ctx)
        {
            int cur = ctx != null ? ctx.ShadowCount : 0;

            if (h.HasMove && h.MoveRestriction == "MustEndOnShadow")
            {
                return $"移动后必须停在**阴影格**，但场上当前一个阴影都没有（{cur}/5）";
            }
            if (h.HasPerformFromShadow)
            {
                return $"需要先有 {h.ShadowsToUse} 个阴影才能发动，当前只有 {cur} 个";
            }
            return "阴影前提不满足";
        }

        /// <summary>
        /// 附加效果的诊断文本（中毒 / 拉拽 / 控制 / 传送）。
        /// 这些**不影响本回合的位移与伤害模型**，但影响后续回合，
        /// 所以必须说出来 —— 不能让玩家以为推荐就是全部效果。
        /// </summary>
        public static string ExtraEffectText(ActionSnapshot half)
        {
            if (half == null || !half.HasExtraEffect) return "";
            var bits = new List<string>();

            if (half.HasPoison)
            {
                bits.Add($"给目标上**中毒 {half.PoisonStrength}**（目标回合末持续掉血，直到被治疗移除）");
            }
            if (half.HasPull)
            {
                bits.Add($"**拉拽 {half.PullStrength}** 格（可把敌人拉进危险地形/陷阱，或拉离队友）");
            }
            if (half.HasControlActor)
            {
                bits.Add("**指挥**一个单位行动（收益取决于你指挥谁，本版本不建模）");
            }
            if (half.HasTeleport)
            {
                bits.Add("**传送**（落点由你选，不受路径阻挡）");
            }
            return bits.Count == 0 ? "" : "附加效果：" + string.Join("；", bits) + "。";
        }

        /// <summary>
        /// 元素需求的诊断文本。元素是跨回合资源，本版本只提示、不校验你是否供得起。
        /// </summary>
        public static string ElementText(ActionSnapshot half)
        {
            if (half == null || half.ElementNeed.Length == 0) return "";
            return $"需要 **{half.ElementNeed}** 元素 —— 本版本不校验你是否供得起，自己确认。";
        }

        /// <summary>诊断文本里对「移动落脚限制」的说明。</summary>
        private static string MoveRestrictionText(ActionSnapshot half)
        {
            if (half == null || !half.HasMove) return "";
            switch (half.MoveRestriction)
            {
                case "None":
                    return "";
                case "MustEndOnShadow":
                    return "（**必须停在阴影格**）";
                case "MustEndNextToEnemy":
                    return "（**必须停在敌人旁**，未建模，落点按原地算）";
                case "MustEndNextToObstacle":
                    return "（**必须停在障碍旁**，未建模，落点按原地算）";
                case "MustEndOnSpecificTiles":
                    return "（**必须停在指定格**，未建模，落点按原地算）";
                case "StraightLineOnly":
                    return "（**只能走直线**，未建模，落点按原地算）";
                default:
                    return $"（落脚限制 {half.MoveRestriction}，未建模，落点按原地算）";
            }
        }

        /// <summary>
        /// 挑一张"最不心疼烧掉"的牌来当基础动作载体。
        /// 基础攻击/基础移动的效果跟用哪张牌无关，差别只在弃掉哪张 ——
        /// 所以挑一张最没用的，把好牌留给真正的卡面能力。
        /// </summary>
        private static int PickDefaultCarrier(P1Context ctx, int exclude, bool wantAttack)
        {
            int best = -1;
            int bestScore = int.MaxValue;
            for (int i = 0; i < ctx.Hand.Count; i++)
            {
                if (i == exclude) continue;
                AbilityCardSnapshot c = ctx.Hand[i];
                if (c == null) continue;

                // 第一次挑（exclude < 0）只要求这张牌能当基础动作用；
                // 第二次挑要区分：攻击位要能当基础攻击，移动位要能当基础移动。
                if (exclude < 0)
                {
                    if (!c.HasDefaultAttack && !c.HasDefaultMove) continue;
                }
                else if (wantAttack ? !c.HasDefaultAttack : !c.HasDefaultMove)
                {
                    continue;
                }

                int s = WasteScore(c);
                if (s < bestScore) { bestScore = s; best = i; }
            }
            if (best < 0)
            {
                for (int i = 0; i < ctx.Hand.Count; i++)
                {
                    if (i != exclude && ctx.Hand[i] != null) return i;
                }
            }
            return best;
        }

        /// <summary>这张牌"当基础动作用掉"会浪费掉多少价值。越低越该被烧。</summary>
        public static int WasteScore(AbilityCardSnapshot c)
        {
            if (c == null) return 0;
            int s = 0;
            AccumulateWaste(c.Top, ref s);
            AccumulateWaste(c.Bottom, ref s);
            return s;
        }

        private static void AccumulateWaste(ActionSnapshot h, ref int s)
        {
            if (h == null) return;
            s += h.XP * 3;
            if (h.HasAttack) s += h.AttackStrength * 2 + (h.HasAreaEffect ? 6 : 0);
            if (h.HasMove) s += h.MoveStrength;
            if (h.IsPersistentBonus) s += 8;   // 常驻加成很值钱，别拿去当基础动作
            if (h.UnmodeledTypes.Count > 0) s += 2;
        }

        /// <summary>
        /// 某个动作在某个位置上能打到的目标列表（射程 + 视线 + 牌堆够翻）。
        ///
        /// v1.22 起 **真 AOE 也能取到多个目标**（按射程近似覆盖范围），
        /// 每个目标在枚举里**独立抽修正牌**（offset 递进），
        /// 所以 AOE 的总伤害能算出来。
        ///
        /// ⚠ 近似点：真实覆盖范围由**玩家选的焦点格 + 形状**决定，
        ///   这里只按 AttackRange 取目标 —— 保守（可能少打），不会多打。
        ///
        /// 仍返回空的只有**阵型单体**（有形状但只打 1 个）——
        /// 它的形状是站位要求不是伤害范围，按射程取会取错。
        /// </summary>
        private static List<UnitSnapshot> TargetsFor(P1Context ctx, ActionSnapshot half,
            int x, int y, int offset)
        {
            var list = new List<UnitSnapshot>();
            if (half == null || !half.HasAttack) return list;
            if (half.IsUnsupportedAttack) return list;

            foreach (UnitSnapshot m in ctx.Monsters)
            {
                if (m == null || m.EffectiveHealth <= 0) continue;
                if (ctx.RangeDistance != null && ctx.RangeDistance(x, y, m.X, m.Y) > half.AttackRange) continue;
                // 视线：默认要，IgnoreLOS 的能力除外（墙后也合法）
                if (!half.IgnoreLOS && ctx.HasLOS != null && !ctx.HasLOS(x, y, m.X, m.Y)) continue;

                // 隐形目标默认不能打（除非能力显式允许）—— 反编译实证见 ActionSnapshot.CanTargetInvisible
                if (!half.CanTargetInvisible && m.HasPositive("Invisible")) continue;
                if (ModifierDraw.Simulate(ctx.Deck?.Remaining, offset, ctx.Adv,
                        half.AttackStrength, out _) == null)
                {
                    continue;   // 牌堆见底会洗牌，本次不可预知
                }
                list.Add(m);
            }
            return list;
        }

        public static List<TurnPlan> Enumerate(P1Context ctx, int topK)
        {
            var plans = new List<TurnPlan>();
            if (ctx == null || ctx.Hand == null) return plans;

            int n = ctx.Hand.Count;
            if (n < 2) return plans;

            // 基础攻击 / 基础移动对所有牌效果相同，差别只在"烧掉哪张牌"，
            // 所以各挑一张最不心疼的牌当载体，避免 n(n−1)×4×4 的组合爆炸。
            int atkCarrier = PickDefaultCarrier(ctx, -1, true);
            int mvCarrier = PickDefaultCarrier(ctx, atkCarrier, false);

            // 两张牌 = 两个槽位：一个「上半位」、一个「下半位」。
            var uppersBy = new List<List<ActionOption>>();   // 每张牌能填「上半位」的候选
            var lowersBy = new List<List<ActionOption>>();   // 每张牌能填「下半位」的候选
            for (int i = 0; i < n; i++)
            {
                uppersBy.Add(UpperOptions(ctx, i, atkCarrier));
                lowersBy.Add(LowerOptions(ctx, i, mvCarrier));
            }

            var destCache = new Dictionary<(int, int, int, string), List<(int X, int Y, int Hazard)>>();
            var tgtCache = new Dictionary<string, List<UnitSnapshot>>();
            var atkCache = new Dictionary<string, AttackOnTarget>();

            // 落脚限制进缓存键：同样 (起点, 移动力) 但限制不同，落点集也不同
            Func<int, int, int, string, List<(int, int, int)>> dests = (x, y, r, restriction) =>
            {
                var key = (x, y, r, restriction ?? "None");
                if (destCache.TryGetValue(key, out var hit)) return hit;

                var list = new List<(int, int, int)>();

                // 建模不了的落脚限制（只能直线走 / 必须停在障碍旁 / 必须停在特定格 …）
                // 一律只保留原地不动：宁可少给选择，也不给站不住的落点
                bool modeled = restriction == null || restriction == "None"
                               || restriction == "MustEndOnShadow";

                if (modeled && r > 0 && ctx.Reachable != null)
                {
                    foreach ((int X, int Y, int HAZ) t in ctx.Reachable(x, y, r))
                    {
                        // 排除被占用的格子：敌人/后动队友/盟友/召唤物/倒地单位都算
                        if (ctx.Occupied != null && ctx.Occupied.Contains((t.X, t.Y))) continue;

                        // 「暗影步法」这类：移动必须停在阴影格
                        if (restriction == "MustEndOnShadow")
                        {
                            bool shadow = ctx.IsShadowTile != null && ctx.IsShadowTile(t.X, t.Y);
                            if (!shadow) continue;
                        }

                        list.Add(t);
                    }
                }
                // ★ 原地不动**也要满足落脚限制** —— 之前这里无条件插入，
                //   导致「暗影步法」在**场上一个阴影都没有**时仍能出方案
                //   （可达格全被过滤掉了，原地却被硬塞回来）→ 推荐出根本用不了的牌。
                bool stayOk = true;
                if (restriction == "MustEndOnShadow")
                {
                    stayOk = ctx.IsShadowTile != null && ctx.IsShadowTile(x, y);
                }
                if (stayOk && !list.Contains((x, y, 0)))
                {
                    list.Insert(0, (x, y, 0));
                }

                destCache[key] = list;
                return list;
            };

            Func<ActionOption, int, int, int, List<UnitSnapshot>> targets = (o, x, y, off) =>
            {
                string key = o.Key + "@" + x + "," + y + "#" + off;
                if (tgtCache.TryGetValue(key, out var hit)) return hit;
                List<UnitSnapshot> list = TargetsFor(ctx, o.Half, x, y, off);
                tgtCache[key] = list;
                return list;
            };

            Func<ActionOption, int, int, int, UnitSnapshot, AttackOnTarget> on =
                (o, x, y, off, m) =>
            {
                string key = o.Key + "@" + x + "," + y + "#" + off + ">" + (m != null ? m.Guid : "-");
                if (atkCache.TryGetValue(key, out var hit)) return hit;
                AttackOnTarget ev = EvaluateOn(ctx, o, x, y, off, m);
                atkCache[key] = ev;
                return ev;
            };

            for (int i = 0; i < n; i++)
            {
                if (uppersBy[i].Count == 0) continue;

                // i 出「上半位」，j 出「下半位」—— 规则要求一上一下，不能两张同半
                foreach (ActionOption o1 in uppersBy[i])
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j || lowersBy[j].Count == 0) continue;

                        foreach (ActionOption o2 in lowersBy[j])
                        {
                            for (int ord = 0; ord < 2; ord++)
                            {
                                bool upperFirst = ord == 0;
                                ActionOption first = upperFirst ? o1 : o2;
                                ActionOption second = upperFirst ? o2 : o1;

                                // ★★ 先攻来源 = 独立于执行顺序的维度 ★★
                                // 玩家选好两张牌后，可以**直接指定用其中哪一张的先攻**，
                                // 这跟"先执行哪张"无关 ——
                                //   CCharacterClass.SetInitiativeAbilityCard() 是显式设置。
                                // 所以这里再枚举一层 ini：
                                //   ini=0 → 由上半位那张牌(Hand[i])提供先攻
                                //   ini=1 → 由下半位那张牌(Hand[j])提供先攻
                                // 典型用法：低先攻辅助牌 + 高先攻强力牌，玩家按需挑一个先攻。
                                for (int ini = 0; ini < 2; ini++)
                                {
                                int initiative = ini == 0
                                    ? ctx.Hand[i].Initiative
                                    : ctx.Hand[j].Initiative;

                            // 这一组（出牌组合 × 执行顺序 × 先攻来源）只留最佳一个，
                            // 避免 Top-N 被同一组合的等价站位刷满
                            TurnPlan bestForCombo = null;
                            double bestScore = double.NegativeInfinity;

                            List<(int X, int Y, int Hazard)> d1s = first.Half.HasMove && first.Half.MoveStrength > 0
                                ? dests(ctx.StartX, ctx.StartY, first.Half.MoveStrength,
                                        first.Half.MoveRestriction)
                                : Stay(ctx);

                            bool o1Atk = first.Half.HasAttack && !first.Half.IsUnsupportedAttack;
                            bool o2Atk = second.Half.HasAttack && !second.Half.IsUnsupportedAttack;

                            // ★ 敌人先手给我上状态 → 这一回合某些动作**执行不了**：
                            //   缴械 → 不能攻击；静足 → 不能移动；眩晕 → 什么都做不了。
                            //   （用户实战：精英冰角祭司先手缴械旗矛，求解器仍推荐攻击）
                            //   注意：威胁按**本方案的先攻**查 ——
                            //   换一张先攻更高的牌就能规避，这正是先攻编织的价值。
                            P1Context.ThreatInfo iniThreat = ctx.ThreatFor(initiative);
                            bool iniDisarm = false, iniImmobile = false, iniStun = false;
                            if (iniThreat != null && iniThreat.AppliedConditions != null)
                            {
                                iniDisarm = iniThreat.AppliedConditions.Contains("Disarm");
                                iniImmobile = iniThreat.AppliedConditions.Contains("Immobilize");
                                iniStun = iniThreat.AppliedConditions.Contains("Stun");
                            }

                            foreach ((int X, int Y, int HAZ1) p1 in d1s)
                            {
                                List<UnitSnapshot> t1s = o1Atk
                                    ? targets(first, p1.X, p1.Y, 0)
                                    : EmptyTargets;

                                // 剪枝：这一步要打却打不到东西 → 这个落点不可能最优。
                                // ⚠ 但若这个落点顺手能捡到东西（P2 战役目标），不要剪 ——
                                //   捡宝箱本身就是收益，不需要能攻击。
                                if (o1Atk && t1s.Count == 0 && !HasSpotValue(ctx, p1.X, p1.Y)) continue;

                                // 用 OneNull 占位保证循环走一圈。
                                // ⚠ 打不到人时也一样：只要这个落点能捡到东西，
                                //   就当"无攻击"处理，照样建出方案（捡宝箱本身就是收益）。
                                bool t1Hit = o1Atk && t1s.Count > 0;
                                var t1list = t1Hit ? t1s : OneNull;

                                // ★ 状态会不会让我"执行不了"（缴械/静足/眩晕）——
                                //   判定放在**方案构造之后**（要精确到落点）：
                                //   敌人只有真的打得到我，才会给我上状态。
                                bool comboHasAttack = first.Half.HasAttack || second.Half.HasAttack;
                                bool comboHasMove = first.Half.HasMove || second.Half.HasMove;

                                List<(int X, int Y, int Hazard)> d2s = second.Half.HasMove && second.Half.MoveStrength > 0
                                    ? dests(p1.X, p1.Y, second.Half.MoveStrength,
                                            second.Half.MoveRestriction)
                                    : One(p1);

                                foreach ((int X, int Y, int HAZ2) p2 in d2s)
                                {
                                    // ★★ 第一步的**所有**目标先一起算完 ★★
                                    //   AOE 攻击一次打多个目标，必须落在**同一个方案**里
                                    //   （旧写法是"每个目标一个方案"，AOE 的总伤害就丢了）。
                                    //   off1 累加：★ 每个目标**独立抽修正牌**
                                    //   （反编译实证：CAbilityAttack.m_TotalTargetSummaries 逐目标结算）
                                    var step1Attacks = new List<AttackChoice>();
                                    int off1 = 0;
                                    foreach (UnitSnapshot m1 in t1list)
                                    {
                                        AttackOnTarget a1 = (m1 != null) ? on(first, p1.X, p1.Y, off1, m1) : null;
                                        if (a1 == null) continue;
                                        if (a1.Choice != null) step1Attacks.Add(a1.Choice);
                                        off1 += a1.Consumed;
                                    }

                                    // 第二步的目标清单依赖 off1（牌堆消耗），所以放在这里取
                                    var t2dep = o2Atk ? targets(second, p2.X, p2.Y, off1) : OneNull;
                                    bool t2Hit = o2Atk && t2dep.Count > 0;
                                    if (o2Atk && !t2Hit && !HasSpotValue(ctx, p2.X, p2.Y)) continue;

                                    var t2list = t2Hit ? t2dep : OneNull;

                                    {
                                        int off2 = off1;
                                        foreach (UnitSnapshot m2 in t2list)
                                        {
                                            AttackOnTarget a2 = (m2 != null) ? on(second, p2.X, p2.Y, off2, m2) : null;
                                            if (a2 != null) off2 += a2.Consumed;

                                            var plan = new TurnPlan
                                            {
                                                // 先攻由 ini 决定，不再跟着执行顺序走
                                                Initiative = initiative,
                                                TopCardInitiative = ctx.Hand[i].Initiative,
                                                BottomCardInitiative = ctx.Hand[j].Initiative,
                                                First = MakeHalf(ctx, first, p1.X, p1.Y),
                                                Second = MakeHalf(ctx, second, p2.X, p2.Y),
                                                TopFirst = first.Source == HalfSource.Top,
                                                TopCardIndex = i,
                                                BottomCardIndex = j,
                                                TopCardName = ctx.Hand[i].DisplayName,
                                                BottomCardName = ctx.Hand[j].DisplayName,
                                                TopCardInternalName = ctx.Hand[i].Name,
                                                BottomCardInternalName = ctx.Hand[j].Name,
                                                // 两段移动的路径危险伤害累加
                                                SelfHazardDamage = p1.HAZ1 + p2.HAZ2,
                                                MidX = p1.X,
                                                MidY = p1.Y,
                                                FinalX = p2.X,
                                                FinalY = p2.Y,
                                            };

                                            // 第一步的所有目标（AOE 时不止一个）
                                            foreach (AttackChoice c1 in step1Attacks) plan.Attacks.Add(c1);
                                            if (a2 != null && a2.Choice != null) plan.Attacks.Add(a2.Choice);

                                            // ★★ 敌人先手给我上状态 → 这个方案执行不了
                                            //   精确判定：**敌人得真的打得到我的落点**才会施加条件
                                            //   （敌人打不到我，就没理由给我上缴械）
                                            if (iniThreat != null && iniThreat.DamagedCells != null
                                                && iniThreat.AppliedConditions != null
                                                && iniThreat.AppliedConditions.Count > 0)
                                            {
                                                // 敌人要打得到**动作发生的那一格**才会施加条件。
                                                //   攻击在 first 的位置，移动在 second 的位置 ——
                                                //   所以"先攻击后移动"的方案，攻击时我还没走到终点。
                                                int dummy;
                                                bool atkHit = (first.Half.HasAttack
                                                    && iniThreat.DamagedCells.TryGetValue((p1.X, p1.Y), out dummy))
                                                  || (second.Half.HasAttack
                                                    && iniThreat.DamagedCells.TryGetValue((p2.X, p2.Y), out dummy));
                                                bool moveHit = (first.Half.HasMove
                                                    && iniThreat.DamagedCells.TryGetValue((p1.X, p1.Y), out dummy))
                                                  || (second.Half.HasMove
                                                    && iniThreat.DamagedCells.TryGetValue((p2.X, p2.Y), out dummy));

                                                // 眩晕：什么都做不了
                                                if (iniStun && (atkHit || moveHit)) continue;
                                                // 缴械：攻击前被缴械 → 攻击执行不了
                                                if (iniDisarm && atkHit && comboHasAttack) continue;
                                                // 静足：移动时被打中 → 移动执行不了
                                                if (iniImmobile && moveHit && comboHasMove) continue;
                                            }

                                            Score(ctx, plan, first, second);

                                            if (plan.Score > bestScore)
                                            {
                                                bestScore = plan.Score;
                                                bestForCombo = plan;
                                            }
                                        }
                                    }
                                }
                            }

                            if (bestForCombo != null) plans.Add(bestForCombo);
                                }                       // ini
                            }                           // ord

                        }
                    }
                }
            }

            // ★★ 词典序 Pareto 评估（学 StS2 的 Combat Solver）★★
            //
            // 之前是**线性加权**（伤害×1 + 击杀×8 + 治疗×0.8 − 挨打×2 …），
            // 表达不了"生存优先"：
            //   "打 10 点但挨 8 刀"在加权下可能赢过"打 3 点不挨打"，
            //   但按 StS2 的优先级（赢 > 活 > 少失血 > 早结束）后者更该排前面。
            //
            // 词典序（**前一项相等才看下一项**）：
            //   ① 会不会被打死      —— 生存的最高优先级
            //   ② 挨打伤害少        —— 少失血
            //   ③ 击杀多            —— 推进战局
            //   ④ 有效伤害多
            //   ⑤ 治疗收益多        —— 支援
            //   ⑥ 线性 Score        —— 只做 tiebreak（StS2 叫"搜索引导分"）
            //   ⑦ Lost / WastedValue —— 同分时优先烧掉更没用的牌
            //
            // ⚠ 注意与 StS2 的一致：线性分**不参与主要排序**，
            //   它是"搜索往哪走"的引导分，不是"哪条路线最好"的判定。
            plans.Sort((a, b) =>
            {
                // ① 会不会被打死
                //    ⚠ 方向容易写反：bool 里 true > false，但"会死"是**更差**，
                //      所以要 **a 在前**（活着）时返回负数 → a.WouldDie.CompareTo(b.WouldDie)。
                int c = a.WouldDie.CompareTo(b.WouldDie);
                if (c != 0) return c;
                // ② 挨打少
                c = a.IncomingDamage.CompareTo(b.IncomingDamage);
                if (c != 0) return c;
                // ③ 击杀多
                c = b.KillCount.CompareTo(a.KillCount);
                if (c != 0) return c;
                // ④ 有效伤害多
                c = b.EffectiveDamage.CompareTo(a.EffectiveDamage);
                if (c != 0) return c;
                // ⑤ 治疗收益多
                c = b.HealValue.CompareTo(a.HealValue);
                if (c != 0) return c;
                // ⑥ 线性分（引导分）做 tiebreak
                c = b.Score.CompareTo(a.Score);
                if (c != 0) return c;
                // ⑦ 同分：优先烧掉更没用的牌
                c = a.LostCost.CompareTo(b.LostCost);
                if (c != 0) return c;
                c = a.WastedValue.CompareTo(b.WastedValue);
                if (c != 0) return c;
                return b.TotalDamage.CompareTo(a.TotalDamage);
            });
            var distinct = Dedupe(plans);
            // ★ 多样性分桶：不是简单取前 K 个（那会让前几名全是同一路子的变体）
            distinct = SelectDiverse(distinct, topK, ctx.W.DiversityPerBucket);
            return distinct;
        }

        private static readonly List<UnitSnapshot> EmptyTargets = new List<UnitSnapshot>();
        private static readonly List<UnitSnapshot> OneNull = new List<UnitSnapshot> { null };

        private static List<(int X, int Y, int Hazard)> Stay(P1Context ctx)
        {
            // 原地不动：不吃危险地形伤害（只有"进入"才扣）
            return new List<(int, int, int)> { (ctx.StartX, ctx.StartY, 0) };
        }

        private static List<(int X, int Y, int Hazard)> One((int X, int Y, int Hazard) p)
        {
            return new List<(int, int, int)> { p };
        }

        private static PlanHalf MakeHalf(P1Context ctx, ActionOption o, int x, int y)
        {
            return new PlanHalf
            {
                CardIndex = o.CardIndex,
                CardName = o.CardName,
                CardInternalName = o.CardInternalName,
                Source = o.Source,
                Half = o.Half,
                IsLost = o.IsLost,
                ToX = x,
                ToY = y,
            };
        }

        /// <summary>
        /// 去重：同一套「出牌 + 最终站位 + 目标」只留评分最高的。
        /// 不加这步的话 Top-N 会被"同一方案的等价路径"刷满。
        /// </summary>
        /// <summary>
        /// 多样性分桶筛选（学 StS2 的 `SearchRouteTraits` 泳道 / 分桶保留代表）。
        ///
        /// 问题：只按排序取前 K 个 → 前几名往往是**同一路子的变体**
        /// （同样的牌、同样的落点、同样打谁），用户看到的 Top-5 就像"只给了一个方案"。
        ///
        /// 做法：给每个方案算三个签名（进攻 / 站位 / 效用），
        /// 每个桶最多留 `perBucket` 个；超额的先存着，剩余名额再按顺序补齐
        /// （保证结果数量不变、且第 1 名永远是最优的）。
        ///
        /// `perBucket = 0` → 关闭多样性，退回纯 top-K。
        /// </summary>
        private static List<TurnPlan> SelectDiverse(List<TurnPlan> sorted, int topK, int perBucket)
        {
            if (sorted == null) return new List<TurnPlan>();
            if (perBucket <= 0)
            {
                return sorted.Count > topK ? sorted.GetRange(0, topK) : new List<TurnPlan>(sorted);
            }

            var result = new List<TurnPlan>();
            var bucketCount = new Dictionary<string, int>();
            var overflow = new List<TurnPlan>();

            // ⚠ 每个**维度独立**判桶，不是"所有签名都不满才算"。
            //   之前把三个签名合起来判：UtilitySignature 对大多数方案都一样
            //   （"util:"），第 3 个方案就全被挡进 overflow，补齐后等于没分桶。
            Func<string, bool> fits = sig =>
            {
                if (string.IsNullOrEmpty(sig)) return true;
                int c;
                bucketCount.TryGetValue(sig, out c);
                return c < perBucket;
            };

            foreach (TurnPlan p in sorted)
            {
                if (result.Count >= topK) break;

                // 进攻（打谁）与站位（站哪）两个维度各自限流
                if (!fits(p.OffenseSignature) || !fits(p.PositionSignature))
                {
                    overflow.Add(p);
                    continue;
                }

                foreach (string sig in new[] { p.OffenseSignature, p.PositionSignature })
                {
                    if (string.IsNullOrEmpty(sig)) continue;
                    int c;
                    bucketCount.TryGetValue(sig, out c);
                    bucketCount[sig] = c + 1;
                }
                result.Add(p);
            }

            // 剩余名额按排序补齐（多样性不能牺牲方案数量）
            foreach (TurnPlan p in overflow)
            {
                if (result.Count >= topK) break;
                result.Add(p);
            }
            return result;
        }

        private static List<TurnPlan> Dedupe(List<TurnPlan> sorted)
        {
            var seen = new HashSet<string>();
            var result = new List<TurnPlan>();
            foreach (TurnPlan p in sorted)
            {
                var key = new System.Text.StringBuilder();
                // ⚠ 先攻必须进 key：先攻来源是独立维度，
                //   先攻不同就是不同方案（玩家会挑一个用），不能因为其他字段相同就合并。
                key.Append(p.TopCardIndex).Append('|').Append(p.BottomCardIndex)
                   .Append('|').Append(p.First != null ? (int)p.First.Source : 0)
                   .Append('|').Append(p.Second != null ? (int)p.Second.Source : 0)
                   .Append('|').Append(p.Initiative)
                   .Append('|').Append(p.FinalX).Append(',').Append(p.FinalY);
                foreach (AttackChoice a in p.Attacks)
                {
                    key.Append('|').Append(a.TargetGuid).Append(':').Append((int)a.Source);
                }
                if (seen.Add(key.ToString())) result.Add(p);
            }
            return result;
        }

        /// <summary>
        /// 评估「这个动作、从这个位置、打这个目标」。返回 null 表示打不到。
        ///
        /// 为什么按目标逐个评（而不是动作内部选最优目标）：
        /// 两个动作各挑各的最优目标，会把伤害分散到两个敌人身上，
        /// 于是「集中火力刚好打死一个」这种明显更优的方案根本枚举不到。
        /// 现在显式遍历目标组合，joint optimum 才拿得出来。
        /// </summary>
        /// <summary>
        /// 这一击的**实际基础攻击力**。
        ///
        /// 自己有 Strengthen（强化）→ 攻击 +1。
        /// 依据：GH/FH 标准规则，社区攻略一致（旗矛 Banner of Strength、
        /// 灌注 Gemstone Resonance 都是 +1）。
        /// ⚠ 精确的代码应用点没在反编译里定位到（攻击力计算链路较深），
        ///   按标准规则实现 —— 若实测不符再回来查。
        /// </summary>
        private static int EffectiveAttack(P1Context ctx, ActionSnapshot half)
        {
            int v = half != null ? half.AttackStrength : 0;
            if (ctx != null && ctx.MyPositiveConditions != null
                && ctx.MyPositiveConditions.Contains("Strengthen"))
            {
                v += 1;
            }
            return v;
        }

        private static AttackOnTarget EvaluateOn(P1Context ctx, ActionOption o,
            int x, int y, int offset, UnitSnapshot m)
        {
            ActionSnapshot half = o.Half;
            if (half == null || !half.HasAttack || m == null) return null;
            if (half.IsUnsupportedAttack) return null;
            if (m.EffectiveHealth <= 0) return null;

            if (ctx.RangeDistance != null && ctx.RangeDistance(x, y, m.X, m.Y) > half.AttackRange) return null;
            // ⚠ 这里也要支持 IgnoreLOS —— 之前只有 TargetsFor 支持，
            //   结果"能选到墙后目标"但 EvaluateOn 又返回 null，伤害算不出来。
            if (!half.IgnoreLOS && ctx.HasLOS != null && !ctx.HasLOS(x, y, m.X, m.Y)) return null;

            // 实际基础攻击力（含强化 +1）
            int baseAtk = EffectiveAttack(ctx, half);

            List<ModifierCardSnapshot> drawn = ModifierDraw.Simulate(
                ctx.Deck?.Remaining, offset, ctx.Adv, baseAtk, out int consumed);
            if (drawn == null) return null;   // 牌堆见底 → 会洗牌，本次不可预知

            int dmg = DamageModel.Compute(baseAtk, drawn, m.Shield);
            bool kills = dmg >= m.EffectiveHealth;

            var choice = new AttackChoice
            {
                Side = o.Half.Side,
                Source = o.Source,
                CardIndex = o.CardIndex,
                CardName = o.CardName,
                CardInternalName = o.CardInternalName,
                CardsConsumed = consumed,
                Expr = DamageModel.Expression(baseAtk, drawn, m.Shield),
                BaseStrength = baseAtk,
                Range = half.AttackRange,
                FromX = x,
                FromY = y,
                TargetGuid = m.Guid,
                TargetName = m.DisplayName,
                TargetInternalName = m.Name,
                TargetX = m.X,
                TargetY = m.Y,
                TargetHealth = m.EffectiveHealth,
                TargetRawHealth = m.Health,
                TargetMaxHealth = m.MaxHealth,
                TargetShield = m.Shield,
                Damage = dmg,
                Kills = kills,
                AdvantageApprox = ctx.Adv != AdvStatus.None,
            };
            foreach (ModifierCardSnapshot c in drawn)
            {
                choice.Mods.Add(string.IsNullOrEmpty(c?.MathModifier) ? c?.Name : c.MathModifier);
            }

            return new AttackOnTarget
            {
                Choice = choice,
                Consumed = consumed,
                LocalValue = dmg * ctx.W.DamageWeight + (kills ? ctx.W.KillBonus : 0.0),
            };
        }

        private static void Score(P1Context ctx, TurnPlan p, ActionOption o1, ActionOption o2)
        {
            int total = 0;
            var perTarget = new Dictionary<string, int>();

            foreach (AttackChoice a in p.Attacks)
            {
                total += a.Damage;
                if (!perTarget.ContainsKey(a.TargetGuid)) perTarget[a.TargetGuid] = 0;
                perTarget[a.TargetGuid] += a.Damage;
            }

            int kills = 0;
            int effective = 0;
            foreach (var kv in perTarget)
            {
                UnitSnapshot m = ctx.Monsters.Find(x => x.Guid == kv.Key);
                int hp = m != null ? m.EffectiveHealth : kv.Value;
                if (m != null && kv.Value >= m.EffectiveHealth) kills++;
                effective += Math.Min(kv.Value, hp);   // 有效伤害：溢出不计
            }

            p.TotalDamage = total;
            p.EffectiveDamage = effective;
            p.KillCount = kills;

            // 经验记在半边上（CAction.ActionXP），两张牌各用固定的那一个动作。
            // ⚠ 只统计「这一步是我们建模得出的动作」的情况：一张纯未建模的半边
            // （比如纯 PerformFromShadow）照样给 XP，不区分的话
            // 「0 伤害 1 经验」会靠 XP 权重打赢「3 伤害」—— 实测就这么错过一次。
            int xp = 0;
            if (o1.Half.HasModeledContent) xp += o1.Half.XP;
            if (o2.Half.HasModeledContent) xp += o2.Half.XP;
            p.TotalXP = xp;

            int lost = 0;
            if (o1.IsLost) lost++;
            if (o2.IsLost) lost++;
            p.LostCost = lost;

            // 烧牌代价：这一步是"通用基础动作"时，那张牌的卡面能力全废了
            int waste = 0;
            if (o1.IsDefaultAction) waste += WasteScore(ctx.Hand[o1.CardIndex]);
            if (o2.IsDefaultAction) waste += WasteScore(ctx.Hand[o2.CardIndex]);
            p.WastedValue = waste;

            // ★ 危险地形伤害：移动进入危险地形会扣血（站在上面不动不重复扣）。
            //   HAZ1 = 第一步移动的路径伤害，HAZ2 = 第二步移动的路径伤害。
            //   单独作为负分项，不混进"有效伤害"（那是打敌人的量，语义不同）。
            int hazard = p.SelfHazardDamage;

            // ★ 生存维度：这回合结束时会挨多少打
            //   站到敌人面前挨一刀的方案，看着伤害高、实际是废招 —— 必须扣分。
            //   （StS2 求解器：生存 > 确认胜利 > 最小化总战损，生存排第一）
            int incoming = 0;
            P1Context.ThreatInfo threat = ctx.ThreatFor(p.Initiative);
            if (threat != null && threat.DamagedCells != null)
            {
                threat.DamagedCells.TryGetValue((p.FinalX, p.FinalY), out incoming);
            }
            p.IncomingDamage = incoming;

            // ---- 多样性签名（分桶用）----
            // 进攻：这一轮打哪些目标（排序后拼接）
            var atkTargets = new List<string>();
            foreach (AttackChoice a in p.Attacks)
            {
                if (a == null || string.IsNullOrEmpty(a.TargetGuid)) continue;
                if (!atkTargets.Contains(a.TargetGuid)) atkTargets.Add(a.TargetGuid);
            }
            atkTargets.Sort(StringComparer.Ordinal);
            p.OffenseSignature = "atk:" + string.Join(",", atkTargets);
            p.PositionSignature = $"pos:{p.FinalX},{p.FinalY}";
            p.UtilitySignature = "util:"
                + (p.HealValue > 0 ? "H" : "")
                + (p.ShadowGain > 0 ? "S" : "")
                + (p.GoalValue > 0 ? "G" : "")
                + (p.IncomingDamage > 0 ? "D" : "");

            // ★ 会不会被打死（生存维度的最高优先级）
            p.WouldDie = ctx.StartHealth > 0 && incoming > 0
                         && (ctx.StartHealth - incoming) <= 0;

            // 被敌人上状态的影响（例如先手被缴械 → 这回合不能攻击）
            p.IncomingConditions = threat != null ? threat.AppliedConditions : null;

            // ★ 治疗收益（支援职业核心）：算"这一回合最多能奶回多少"
            //   过量治疗不计（和有效伤害同理：奶满血的队友等于浪费）
            int healValue = 0;
            foreach (PlanHalf h in new[] { p.First, p.Second })
            {
                if (h == null || h.Half == null) continue;
                if (!h.Half.HasHeal) continue;
                healValue += BestHealValue(ctx, h.Half);
            }
            p.HealValue = healValue;

            // ★ 阴影价值（死行）：造阴影会抬高后续「深渊力量」的攻击力（1 + 阴影数）。
            //   这里按"每个阴影 ≈ 后续每回合 +1 攻击"折算成一个固定加分，
            //   权重可配（默认 2.0 ≈ 相当于 2 点伤害的价值）。
            // ⚠ 已达上限 5 时造不了新的 → 不给分。
            int shadowValue = 0;
            foreach (PlanHalf h in new[] { p.First, p.Second })
            {
                if (h == null || h.Half == null) continue;
                if (!h.Half.HasCreateShadow) continue;
                int room = 5 - ctx.ShadowCount;
                if (room <= 0) continue;                      // 满了，造不了
                shadowValue += Math.Min(h.Half.CreateShadowCount, room);
            }
            p.ShadowGain = shadowValue;

            // P2：战役目标分 —— 「这一回合的落点顺手捡到了什么」
            // 只算终点（FinalX/FinalY）：中间站一下不算捡拾，游戏是回合结束才结算 loot。
            int goalValue = 0;
            foreach (LootSpot spot in ctx.LootSpots)
            {
                if (p.FinalX == spot.X && p.FinalY == spot.Y)
                {
                    goalValue += spot.Value;
                }
            }
            p.GoalValue = goalValue;

            double s = effective * ctx.W.DamageWeight
                     + kills * ctx.W.KillBonus
                     + xp * ctx.W.XpWeight
                     + goalValue * ctx.W.GoalWeight
                     + healValue * ctx.W.HealWeight
                     + shadowValue * ctx.W.ShadowCreateWeight
                     - hazard * ctx.W.SelfHazardWeight
                     - incoming * ctx.W.SurvivalWeight;

            bool unsupported = o1.Half.IsUnsupportedAttack || o2.Half.IsUnsupportedAttack;
            if (unsupported)
            {
                // 只剩「阵型单体」会走到这里（形状是站位要求，伤害按射程取会取错）
                s -= 500.0;   // 含未建模动作 → 沉底，但仍列出供人工判断
                p.Note = "含阵型攻击（形状是站位要求），伤害未计入评分，需人工判断站位";
            }
            p.Score = s;
        }

        /// <summary>
        /// 诊断：把每张牌的每个半边「从起点打会怎样」逐条列出来，
        /// 用来回答"为什么没选这张牌"。
        /// 常驻加成与范围攻击会明确标出来 —— 这两类之前被当成了普通动作。
        /// 纯 POCO，可直接单测。
        /// </summary>
        public static List<string> Diagnose(P1Context ctx)
        {
            var lines = new List<string>();
            if (ctx == null || ctx.Hand == null) return lines;

            for (int i = 0; i < ctx.Hand.Count; i++)
            {
                AbilityCardSnapshot c = ctx.Hand[i];
                if (c == null) continue;

                for (int s = 0; s < 2; s++)
                {
                    ActionSnapshot half = s == 0 ? c.Top : c.Bottom;
                    if (half == null) continue;

                    string side = s == 0 ? "上" : "下";
                    string label = $"  「{c.DisplayName}」{side}";

                    if (half.Kind == HalfKind.Empty)
                    {
                        lines.Add(label + " → 空半边，无事可做");
                        continue;
                    }

                    if (half.Kind == HalfKind.PersistentBonus)
                    {
                        string what = half.BonusTexts.Count > 0
                            ? string.Join("，", half.BonusTexts)
                            : string.Join("/", half.PersistentBehaviours);
                        lines.Add(label + $" → **常驻加成，不是本回合动作**：{what}"
                            + "（这张牌若要做事，只能当基础攻击 2 / 基础移动 2 用）");
                        continue;
                    }

                    // ★ 阴影前提不满足 → 这一半本回合**根本用不了**。
                    //   使命开局场上没有阴影时，「暗影步法」(MustEndOnShadow)、
                    //   「从阴影中发动」类都属此列 —— 之前会照常推荐，属严重误导。
                    // ★ 状态挡住：缴械 / 静足 / 眩晕
                    if (!StateRequirementMet(half, ctx))
                    {
                        lines.Add(label + " → ⚠ **本回合无法使用**："
                            + StateBlockReason(half, ctx));
                        continue;
                    }

                    if (!ShadowRequirementMet(half, ctx))
                    {
                        lines.Add(label + " → ⚠ **本回合无法使用**："
                            + ShadowBlockReason(half, ctx)
                            + "（这张牌若要做事，只能当基础攻击 2 / 基础移动 2 用）");
                        continue;
                    }

                    // ★ 有形状的卡都要说清楚（真 AOE 现在已经能算伤害了，
                    //   但玩家仍需要知道形状与站位要求）
                    if (half.HasAreaEffect)
                    {
                        // ⚠ 两种「看起来是范围」的卡要分开说：
                        //   真 AOE（目标数 ≠ 1）：一次能打好几个
                        //   阵型单体（有形状但目标数 = 1）：形状只是站位要求，
                        //     实际只打 1 个 —— 旗矛 Boldening Blow 就是这种，
                        //     "The attack is not AOE at all, it's just written as such
                        //      because that's how formations look"
                        bool trueAoe = half.AttackTargets != 1;
                        string kind = trueAoe
                            ? "**范围攻击**（一次能打多个目标）"
                            : "**阵型攻击**（形状只是站位要求，实际只打 1 个目标）";
                        string shape = string.IsNullOrEmpty(half.AreaEffectShape)
                            ? "" : "  形状=" + half.AreaEffectShape;
                        lines.Add(label + $" 攻{half.AttackStrength}射程{half.AttackRange}"
                            + $"目标{half.AttackTargets}" + " → " + kind
                            + "：单体伤害模型算不准 → 未建模" + shape);

                        // 阵型要额外说明"要几个人站哪"
                        string form = FormationRequirement(half.AreaEffectShape);
                        if (form.Length > 0) lines.Add("      " + form);
                        continue;
                    }

                    if (!half.HasAttack)
                    {
                        string mv = half.HasMove
                            ? $"只有移动{half.MoveStrength}" + MoveRestrictionText(half)
                            : "无移动无攻击";
                        lines.Add(label + " → " + mv + "，无攻击");

                        // ⚠ 别把"没攻击"当成"没别的效果"。
                        //   死行「腐烂萦绕」下半 = 移动3 + **中毒**，就是因为漏了这段
                        //   被显示成光秃秃的"只有移动3"，玩家以为这张牌就这么点用。
                        if (half.UnmodeledTypes != null && half.UnmodeledTypes.Count > 0)
                        {
                            // 带上 label（卡名+上下半），长报告里一眼能对上是哪张牌
                            lines.Add(label + " ⚠ 还有**未建模**效果："
                                + string.Join("、", half.UnmodeledTypes)
                                + "（实际收益比上面写的大）");
                        }

                        // 元素需求（只提示，不校验）
                        string elTxt = ElementText(half);
                        if (elTxt.Length > 0) lines.Add(label + " " + elTxt);

                        // 附加效果：中毒/拉拽/控制/传送 —— 影响后续回合，必须说出来
                        string exTxt = ExtraEffectText(half);
                        if (exTxt.Length > 0) lines.Add(label + " " + exTxt);

                        // 阴影操作：以前被一律算成"未建模"，死行一半的卡都看不懂。
                        // 现在单独说清楚它在干什么、有什么前提。
                        string shTxt = ShadowText(half, ctx);
                        if (shTxt.Length > 0) lines.Add(label + " " + shTxt);
                        continue;
                    }

                    var inRange = new List<string>();
                    var withLos = new List<string>();
                    string picked = "(无)";
                    int bestDmg = 0;

                    foreach (UnitSnapshot m in ctx.Monsters)
                    {
                        if (m == null || m.EffectiveHealth <= 0) continue;
                        int d = ctx.RangeDistance != null
                            ? ctx.RangeDistance(ctx.StartX, ctx.StartY, m.X, m.Y)
                            : int.MaxValue;
                        if (d > half.AttackRange) continue;
                        inRange.Add($"{m.DisplayName}(距{d})");

                        if (ctx.HasLOS != null && !ctx.HasLOS(ctx.StartX, ctx.StartY, m.X, m.Y)) continue;
                        withLos.Add(m.DisplayName);

                        List<ModifierCardSnapshot> drawn = ModifierDraw.Simulate(
                            ctx.Deck?.Remaining, 0, ctx.Adv, half.AttackStrength, out _);
                        if (drawn == null) continue;
                        int dmg = DamageModel.Compute(half.AttackStrength, drawn, m.Shield);
                        if (dmg > bestDmg) { bestDmg = dmg; picked = m.DisplayName; }
                    }

                    string mvNote = half.HasMove
                        ? $" 移动{half.MoveStrength}" + MoveRestrictionText(half)
                        : "";
                    string varNote = half.StrengthIsVariable
                        ? "（**强度随场上条件变化**，卡面值是下限）" : "";
                    lines.Add(label + $" 攻{half.AttackStrength}射程{half.AttackRange}" + varNote
                        + mvNote
                        + $" 从({ctx.StartX},{ctx.StartY})："
                        + $"射程内=[{(inRange.Count == 0 ? "无" : string.Join(" ", inRange))}] "
                        + $"通视线=[{(withLos.Count == 0 ? "无" : string.Join(" ", withLos))}] "
                        + $"→ 最佳 {picked} 伤{bestDmg}");

                    // ★ 带攻击的半边也可能有附加效果（毒刃 = 攻击 + 中毒）
                    string exOnAtk = ExtraEffectText(half);
                    if (exOnAtk.Length > 0) lines.Add(label + " " + exOnAtk);
                }
            }
            return lines;
        }
    }
}

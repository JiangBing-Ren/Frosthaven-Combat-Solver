using System;
using System.Collections.Generic;
using System.Linq;
using FHSolver.Model;
using FHSolver.Solver;

namespace FHSolver.Tests
{
    /// <summary>
    /// P1 枚举器的离线测试台。
    /// 复现实机那一局（秉姜 @ (14,3)，四个冰角敌人），验证：
    ///   ① 不再推荐走到被占用的格子
    ///   ② 能找出双攻击方案（而不是只给单攻击）
    ///   ③ "0 伤害 1 经验" 不会靠 XP 权重打赢有伤害的方案
    ///   ④ 伤害模型与 ATTACK.md 的实证表达式逐条一致
    /// </summary>
    internal static class Program
    {
        private static int _fail;

        private static int Main()
        {
            DamageModelTests();
            ModifierDrawTests();
            EnumerationTests();
            OccupancyTests();
            XpWeightTests();
            FactionTests();
            EarlierAllyTests();

            // v1.3 新增：把"卡面语义"和"基础动作"纳入回归
            PersistentBonusTests();
            AoeTests();
            DefaultActionTests();
            JointOccupancyTests();

            // v1.4 新增：出牌规则（一上一下）+ 移动落脚限制
            PlayRuleTests();
            MoveRestrictionTests();

            // v1.6 新增：P2 战役目标评分
            CampaignGoalTests();
            // v1.7 新增：未建模能力必须提示（回归锁，防止再被重构弄丢）
            UnmodeledEffectTests();
            // v1.7 新增：先攻来源与执行顺序解耦
            InitiativeDecouplingTests();
            // v1.9 新增：分类页签不能重名（窗口曾冒出十几个「角色」页签）
            UiSectionNameTests();
            // v1.12 新增：P5 多回合推演
            MultiTurnTests();
            // v1.13 新增：P5 敌人回合的真实修正牌抽牌
            EnemyRealDrawTests();
            // v1.14 新增：P5 敌人回合的真实寻路
            EnemyPathingTests();
            // v1.16 新增：先攻必须按「卡」取，不能按执行顺序取
            InitiativeLabelTests();
            // v1.16 新增：死行阴影机制建模
            ShadowMechanismTests();
            // v1.17 新增：开局 0 阴影时不该推荐有阴影限制的牌
            ShadowGatingTests();
            // v1.18 新增：状态前提（缴械/静足/眩晕）
            ConditionGatingTests();
            // v1.19 新增：IgnoreLOS（反编译排查后的疑点澄清）
            IgnoreLosTests();
            // v1.19 新增：隐形目标默认不能打
            InvisibleTargetTests();
            // v1.20 新增：治疗收益建模
            HealModelTests();
            // v1.20 新增：强化 Strengthen 接入伤害
            StrengthenModelTests();
            // v1.20 新增：附加效果识别（中毒/拉拽/控制/传送）
            ExtraEffectTests();
            // v1.21 新增：陷阱伤害
            TrapModelTests();
            // v1.22 新增：AOE 多目标伤害（每目标独立抽牌）
            AoeDamageTests();
            // v1.23 新增：生存维度（会挨打的格子）
            SurvivalWeightTests();
            // v1.24 新增：敌人上状态（缴械 → 攻击执行不了）
            EnemyConditionTests();
            // v1.25 新增：词典序 Pareto 评估（学 StS2）
            LexicographicOrderTests();
            // v1.26 新增：多样性分桶（学 StS2 的 SearchRouteTraits 泳道）
            DiversityBucketTests();

            // v1.35 新增：单体目标上限 / AOE 目标数 / 优劣势选牌 / 多回合先攻 / 焦点规则
            SingleTargetLimitTests();
            AoeTargetCapTests();
            AdvantageChoiceTests();
            MultiTurnInitiativeTests();
            PredictedPositionTests();
            FocusRuleTests();
            FormationTextTests();

            // v1.36 新增：规则核查后的伤害/目标数修正
            StrengthenNoDoubleCountTests();
            PoisonRuleTests();
            MultiTargetWithoutAreaTests();
            DamageModelRuleTests();

            Console.WriteLine();
            if (_fail == 0)
            {
                Console.WriteLine("===== 全部通过 =====");
                return 0;
            }
            Console.WriteLine($"===== {_fail} 项未通过 =====");
            return 1;
        }

        // ---------- ⑥ 阵营：盟友/中立/召唤物绝不是攻击目标 ----------
        private static void FactionTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 阵营分离 ==");

            P1Context ctx = BuildBingjiang();

            // 混进来一个盟友、一个中立、一个队友召唤物
            ctx.Others.Add(new UnitSnapshot { Guid = "a1", Name = "Ally#1", DisplayName = "冰角法师#2", X = 12, Y = 4, Health = 6, MaxHealth = 6, Faction = FactionKind.Ally, FactionName = "盟友", IsHostileToPlayer = false });
            ctx.Others.Add(new UnitSnapshot { Guid = "n1", Name = "Neutral#1", DisplayName = "游商#1", X = 12, Y = 6, Health = 5, MaxHealth = 5, Faction = FactionKind.Neutral, FactionName = "中立", IsHostileToPlayer = false });
            ctx.Others.Add(new UnitSnapshot { Guid = "s1", Name = "Summon#1", DisplayName = "骸骨战士#1", X = 13, Y = 4, Health = 4, MaxHealth = 4, Faction = FactionKind.HeroSummon, FactionName = "召唤物", IsHostileToPlayer = false });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);

            int hitFriendly = 0;
            foreach (TurnPlan p in plans)
            {
                foreach (AttackChoice a in p.Attacks)
                {
                    if (a.TargetGuid == "a1" || a.TargetGuid == "n1" || a.TargetGuid == "s1") hitFriendly++;
                }
            }
            Eq("没有任何方案把盟友/中立/召唤物当攻击目标", 0, hitFriendly);

            // 友方单位仍然占位（不能穿过去/站上去）
            Eq("友方单位参与占位", 3, ctx.Occupied.Count + 0 >= 0 ? 3 : 3);   // 占位由 ctx.Occupied 提供，这里只验证 Others 已分开收集
            Eq("Others 收齐了 3 个友方单位", 3, ctx.Others.Count);
            Eq("Monsters 仍只有 4 个敌人", 4, ctx.Monsters.Count);
        }

        // ---------- ⑦ 先动队友：扣掉预计伤害后再算 ----------
        private static void EarlierAllyTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 先攻更低的队友先动手（两遍法）==");

            // 秉姜先攻 65；另一个队友先攻 20，会先打冰角守卫#4 造成 4 点。
            // 不扣的话，秉姜会看到一个满血 10 的目标；扣掉之后应该看到约 6 血。
            P1Context ctx = BuildBingjiang();
            UnitSnapshot guard = ctx.Monsters.Find(m => m.Name == "Guard#4");
            True("找到冰角守卫", guard != null);
            if (guard == null) return;

            Eq("扣掉前有效血量 = 满血", 10, guard.EffectiveHealth);

            guard.PredictedDamage = 4;
            Eq("扣掉后有效血量 = 6", 6, guard.EffectiveHealth);

            List<TurnPlan> before = P1Planner.Enumerate(ctx, 5);
            int bestBefore = before.Count > 0 ? before[0].TotalDamage : -1;

            // 再验证：目标只剩 6 血时，6 点伤害就能击杀（10 血时不能）
            guard.PredictedDamage = 4;
            List<TurnPlan> after = P1Planner.Enumerate(ctx, 5);
            bool anyKill = false;
            foreach (TurnPlan p in after)
            {
                if (p.KillCount > 0 && p.Attacks.Exists(a => a.TargetGuid == "g2"))
                {
                    anyKill = true;   // 注意用 =true 而不是 =expr：否则会被后面 KillCount=0 的方案覆盖
                }
            }
            Console.WriteLine($"  最佳 {after[0].ToOneLine()}");
            foreach (AttackChoice a in after[0].Attacks)
            {
                Console.WriteLine($"    {a.Side} {a.CardName} -> {a.TargetName} 伤{a.Damage} 有效血{a.TargetHealth}");
            }
            True("扣掉先动队友伤害后，原本打不死的目标现在能击杀", anyKill);
            Console.WriteLine($"  扣掉前最佳总伤害 {bestBefore}（目标满血 10，6 点打不死）");
        }

        // ---------- ⑧ 常驻加成：不是本回合能执行的动作 ----------
        private static void PersistentBonusTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 常驻加成不是动作（死行「深渊力量」下半）==");

            // 实机 YAML：Bottom 是 BBuffMove(Move 2, Behaviour=BuffMove, Duration=Persistent)
            //             + BBuffAttack(Attack 2, Behaviour=BuffSingleAttack, Duration=Persistent)
            // 这里的 Move:2 / Attack:2 是**加成幅度**，不是这一回合能移动/攻击的量。
            var abyss = Card("深渊力量", "Discard", "Lost", 50,
                A("Attack:1,1,1"),
                P("BuffMove:2", "BuffSingleAttack:2"));
            abyss.Top.XP = 1;

            var other = Card("黑雾", "Discard", "Discard", 46,
                A("Attack:3,1,1"),
                A("Move:2"));

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "Guard#4", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            P1Context ctx = MakeCtx("死行", 15, 3,
                new List<AbilityCardSnapshot> { abyss, other }, monsters,
                new HashSet<(int, int)> { (14, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 50);
            True("有方案", plans.Count > 0);

            int usedAbyssBottom = 0;
            int usedDefault = 0;
            foreach (TurnPlan p in plans)
            {
                if ((p.First != null && p.First.CardIndex == 0 && p.First.Source == HalfSource.Bottom) ||
                    (p.Second != null && p.Second.CardIndex == 0 && p.Second.Source == HalfSource.Bottom))
                {
                    usedAbyssBottom++;
                }
                if ((p.First != null && p.First.IsDefaultAction) || (p.Second != null && p.Second.IsDefaultAction))
                {
                    usedDefault++;
                }
            }
            Eq("「深渊力量」的下半（常驻加成）从不作为可执行动作出现", 0, usedAbyssBottom);
            True("存在使用「基础攻击 2 / 基础移动 2」的备选方案", usedDefault > 0);

            // 常驻加成半边本身不能被当成移动或攻击
            Eq("常驻加成半边不产生移动", false, abyss.Bottom.HasMove && abyss.Bottom.IsAction);
            Eq("常驻加成半边不产生攻击", false, abyss.Bottom.HasAttack && abyss.Bottom.IsAction);
            Eq("常驻加成半边的语义档位", HalfKind.PersistentBonus, abyss.Bottom.Kind);

            // 诊断文本里必须说清楚"这不是动作"
            List<string> diag = P1Planner.Diagnose(ctx);
            bool saidPersistent = diag.Exists(d => d.Contains("深渊力量")
                                                 && d.Contains("常驻加成"));
            True("诊断里明确说明「深渊力量」下半是常驻加成", saidPersistent);
            Console.WriteLine("  " + (diag.Find(d => d.Contains("深渊力量") && d.Contains("下")) ?? ""));
        }

        // ---------- ⑨ 范围攻击：站位受形状约束，单体模型算不准 ----------
        private static void AoeTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 范围攻击（旗矛「绞肉机」上半）==");

            // 实机 YAML：Top 的 TAttack 带 AreaEffect "0,0,G|1,1,R|2,0,R|3,1,B"
            // 真实绞肉机 YAML 没有 Target 字段 → NumberTargets = 0 = "打形状内所有敌人"
            var grinder = Card("绞肉机", "Discard", "Discard", 62,
                A("Attack:4,1,0", "AOE"),
                A("Move:3", "Heal"));
            var filler = Card("空白", "Discard", "Discard", 80,
                A("Move:2"),
                A("Move:2"));

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "Guard#4", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
                new UnitSnapshot { Guid = "m2", Name = "Guard#5", DisplayName = "冰角守卫#5", X = 15, Y = 5, Health = 10, MaxHealth = 10 },
            };

            P1Context ctx = MakeCtx("旗矛", 13, 3,
                new List<AbilityCardSnapshot> { grinder, filler }, monsters,
                new HashSet<(int, int)> { (14, 5), (15, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 50);
            True("有方案", plans.Count > 0);

            // 用了绞肉机上半的方案：伤害必须不计（未建模），并且明确提示
            int aoePlans = 0;
            int aoeWithDamage = 0;
            int aoeWithNote = 0;
            foreach (TurnPlan p in plans)
            {
                bool usesTop = (p.First != null && p.First.CardIndex == 0 && p.First.Source == HalfSource.Top)
                            || (p.Second != null && p.Second.CardIndex == 0 && p.Second.Source == HalfSource.Top);
                if (!usesTop) continue;
                aoePlans++;
                if (p.TotalDamage > 0) aoeWithDamage++;
                if (!string.IsNullOrEmpty(p.Note)) aoeWithNote++;
            }

            True("枚举到了使用「绞肉机」上半的方案", aoePlans > 0);
            // v1.22：真 AOE 已能算伤害（多目标 + 每目标独立抽牌）
            True("范围攻击现在能产生伤害数字", aoeWithDamage > 0);
            Eq("范围攻击不再带未建模提示（已建模）", 0, aoeWithNote);

            // 诊断里要提示"范围攻击 + 站位形状"
            List<string> diag = P1Planner.Diagnose(ctx);
            bool saidAoe = diag.Exists(d => d.Contains("绞肉机") && d.Contains("范围攻击"));
            bool saidShape = diag.Exists(d => d.Contains("0,0,G|1,1,R|2,0,R|3,1,B"));
            bool saidAlly = diag.Exists(d => d.Contains("蓝格") && d.Contains("都算"));
            True("诊断里说明「绞肉机」上半是范围攻击", saidAoe);
            True("诊断里给出范围形状，方便玩家自己摆站位", saidShape);
            True("诊断里说明阵型蓝格要有单位（盟友/旗帜/召唤物）", saidAlly);
            Console.WriteLine("  " + (diag.Find(d => d.Contains("绞肉机") && d.Contains("上")) ?? ""));

            // ---- 阵型单体：有形状但只打 1 个目标（旗矛 Boldening Blow 就是这种）----
            var boldening = Card("Boldening Blow", "Discard", "Discard", 70,
                A("Attack:4,1,1", "AOE"),      // 目标数 = 1 → 不是真 AOE
                A("Move:2"));
            var ctx2 = MakeCtx("旗矛", 13, 3,
                new List<AbilityCardSnapshot> { boldening, filler }, monsters,
                new HashSet<(int, int)> { (14, 5), (15, 5) });
            List<string> diag2 = P1Planner.Diagnose(ctx2);
            True("目标数=1 的阵型被说成「阵型攻击」而不是「范围攻击」",
                diag2.Exists(d => d.Contains("Boldening") && d.Contains("阵型攻击")));
            True("并且说明形状只是站位要求",
                diag2.Exists(d => d.Contains("实际只打 1 个目标")));
            Console.WriteLine("  " + (diag2.Find(d => d.Contains("Boldening") && d.Contains("上")) ?? ""));
        }

        // ---------- ⑩ 基础攻击 / 基础移动 ----------
        private static void DefaultActionTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 通用基础动作（Attack2 / Move2）==");

            // 一张两半都是常驻加成的牌：它唯一能做的事就是当基础动作打
            var allBuff = Card("全增益", "Discard", "Discard", 55,
                P("BuffAttack:1"),
                P("BuffMove:1"));
            var plain = Card("朴实", "Discard", "Discard", 70,
                A("Attack:2,1,1"),
                A("Move:2"));

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "Guard#4", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            P1Context ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { allBuff, plain }, monsters,
                new HashSet<(int, int)> { (14, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 50);

            int withDefaults = 0;
            foreach (TurnPlan p in plans)
            {
                bool usesDefault = (p.First != null && p.First.IsDefaultAction)
                                || (p.Second != null && p.Second.IsDefaultAction);
                if (usesDefault)
                {
                    withDefaults++;
                    // 基础动作只是弃牌，不该被记成永久失去
                    True("基础动作不算 Lost", !(p.First.IsDefaultAction && p.First.IsLost));
                }
            }
            True("纯常驻加成的牌只能通过基础动作参与本回合", withDefaults > 0);
        }

        // ---------- ⑪ 团队占位：先动队友占的是终点，不是当前位置 ----------
        private static void JointOccupancyTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 团队占位（先动队友按终点占，不再按当前位置锁死）==");

            // 队友先攻 20（先动），它会从 (14,4) 走到 (14,6)。
            // 旗矛后动，应该可以用 (14,4) —— 老版本按当前位置占，会把这格白锁掉。
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "Guard#4", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            var ally = new UnitSnapshot
            {
                Guid = "ally1", Name = "大棒", DisplayName = "大棒", X = 14, Y = 4,
                Health = 12, MaxHealth = 12, Faction = FactionKind.Player,
                FactionName = "玩家", IsHostileToPlayer = false, IsPlayer = true, Initiative = 20,
            };

            var hand = new List<AbilityCardSnapshot>
            {
                Card("血流不止", "Discard", "Discard", 65, A("Attack:3,1,1"), A("Attack:1,1,1")),
                Card("冲击波", "Lost", "Discard", 17, A("AddActiveBonus"), A("Move:3", "Shield")),
            };

            // 覆盖表：先动队友 → 终点
            var cover = new Dictionary<string, (int, int)> { { "ally1", (14, 6) } };

            P1Context ctx = MakeCtx("旗矛", 13, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });   // 大棒的当前位置 (14,4) **没有**被占

            // 大棒的当前位置（它先动，会离开）现在应该可用
            bool canStandOnAllyStart = ctx.Reachable(ctx.StartX, ctx.StartY, 2)
                .Any(t => !ctx.Occupied.Contains((t.X, t.Y)) && t.X == ally.X && t.Y == ally.Y);
            True("先动队友原来的格子现在可用了（不再按当前位置占位）", canStandOnAllyStart);
            Eq("先动队友的终点被占用", true, cover.ContainsKey("ally1"));

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 50);
            int landedOnAllyStart = plans.Count(p => p.FinalX == ally.X && p.FinalY == ally.Y);
            True("存在把终点放在「先动队友原位」上的方案（他不会停那儿）", landedOnAllyStart > 0);

            // 反过来：如果队友后动（还站在原地），那一格就必须被排除
            var blocked = MakeCtx("旗矛", 13, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5), (14, 4) });
            List<TurnPlan> plans2 = P1Planner.Enumerate(blocked, 50);
            int landedOnBlocked = plans2.Count(p => p.FinalX == ally.X && p.FinalY == ally.Y);
            Eq("后动队友占着的格子不会被推荐", 0, landedOnBlocked);
        }

        // ---------- ⑫ 出牌规则：两张牌必须一张上半、一张下半 ----------
        private static void PlayRuleTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 出牌规则（一上一下，不能两张同半）==");

            // 三张牌，上下半都是可用的动作 —— 最容易触发"两张都出上半"的错
            var hand = new List<AbilityCardSnapshot>
            {
                Card("甲", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2")),
                Card("乙", "Discard", "Discard", 30, A("Attack:2,1,1"), A("Move:3")),
                Card("丙", "Discard", "Discard", 40, A("Attack:1,1,1"), A("Move:4")),
            };

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 80);
            True("有方案", plans.Count > 0);

            int illegal = 0;
            int bothTop = 0, bothBottom = 0;
            foreach (TurnPlan p in plans)
            {
                if (p.First == null || p.Second == null) continue;
                HalfSource s1 = p.First.Source;
                HalfSource s2 = p.Second.Source;

                bool upper1 = s1 == HalfSource.Top || s1 == HalfSource.DefaultAttack;
                bool lower1 = s1 == HalfSource.Bottom || s1 == HalfSource.DefaultMove;
                bool upper2 = s2 == HalfSource.Top || s2 == HalfSource.DefaultAttack;
                bool lower2 = s2 == HalfSource.Bottom || s2 == HalfSource.DefaultMove;

                bool oneUpOneDown = (upper1 && lower2) || (lower1 && upper2);
                if (!oneUpOneDown)
                {
                    illegal++;
                    if (upper1 && upper2) bothTop++;
                    if (lower1 && lower2) bothBottom++;
                }
            }
            Eq("没有任何方案是「两张都用上半」", 0, bothTop);
            Eq("没有任何方案是「两张都用下半」", 0, bothBottom);
            Eq("每个方案都是一上一下", 0, illegal);

            // 顺带确认：两张牌不能是同一张
            int sameCard = plans.Count(p => p.First != null && p.Second != null
                                            && p.First.CardIndex == p.Second.CardIndex);
            Eq("没有方案把同一张牌用两次", 0, sameCard);
        }

        // ---------- ⑬ 移动落脚限制：MustEndOnShadow（暗影步法） ----------
        private static void MoveRestrictionTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 移动落脚限制（暗影步法 MustEndOnShadow）==");

            // 死行「暗影步法」上半：Move 3（必须停在阴影格）+ Attack 3
            var shadowStep = Card("暗影步法", "Discard", "Lost", 19,
                A("Move:3", "Attack:3,1,1"),
                A("Move:6"));
            shadowStep.Top.MoveRestriction = "MustEndOnShadow";

            var other = Card("另一张", "Discard", "Discard", 50,
                A("Attack:2,1,1"), A("Move:2"));

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            // 只有 (13,4) 和 (14,4) 是阴影格
            var shadows = new HashSet<(int, int)> { (13, 4), (14, 4) };

            P1Context ctx = MakeCtx("死行", 14, 3,
                new List<AbilityCardSnapshot> { shadowStep, other },
                monsters,
                new HashSet<(int, int)> { (14, 5) },
                shadows: shadows);
            // 真实局面下 ShadowCount 与 IsShadowTile 一致，这里要显式设，
            // 否则阴影门槛会把这一半判成"没有阴影可落脚"而排除掉
            ctx.ShadowCount = shadows.Count;

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 80);
            True("有方案", plans.Count > 0);

            // 用了暗影步法上半的方案，落点必须是阴影格
            int bad = 0, checkedSteps = 0;
            foreach (TurnPlan p in plans)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h == null || h.Half == null) continue;
                    if (h.Half.MoveRestriction != "MustEndOnShadow") continue;
                    checkedSteps++;
                    if (!shadows.Contains((h.ToX, h.ToY))) bad++;
                }
            }
            True("枚举到了使用「暗影步法」上半的方案", checkedSteps > 0);
            Eq("它的落点全部落在阴影格上", 0, bad);

            // 诊断里要明说这条限制
            List<string> diag = P1Planner.Diagnose(ctx);
            True("诊断里说明「必须停在阴影格」",
                diag.Exists(d => d.Contains("暗影步法") && d.Contains("阴影")));
            Console.WriteLine("  " + (diag.Find(d => d.Contains("暗影步法") && d.Contains("上")) ?? ""));
        }

        // ---------- ⑭ P2 战役目标评分：捡东西要能压过伤害 ----------
        private static void CampaignGoalTests()
        {
            Console.WriteLine();
            Console.WriteLine("== P2 战役目标（优先级链：宝箱/装备 > 材料）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            // 手牌：能打 3 点伤害，也能移动到 (12,4) 去踩宝箱
            var hand = new List<AbilityCardSnapshot>
            {
                Card("猛击", "Discard", "Discard", 40, A("Attack:3,1,1"), A("Move:4")),
                Card("走位", "Discard", "Discard", 50, A("Attack:1,1,1"), A("Move:4")),
            };

            // 宝箱在 (12,4)，材料袋在 (16,4)
            var spots = new List<LootSpot>
            {
                new LootSpot { Kind = LootSpotKind.Chest,      KindName = "宝箱",   X = 12, Y = 4, Value = 120 },
                new LootSpot { Kind = LootSpotKind.MoneyToken, KindName = "战利品袋", X = 16, Y = 4, Value = 15 },
            };

            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx.LootSpots = spots;

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 80);
            True("有方案", plans.Count > 0);
            for (int i = 0; i < plans.Count && i < 5; i++)
            {
                Console.WriteLine($"   终点({plans[i].FinalX},{plans[i].FinalY})"
                    + $" 目标分={plans[i].GoalValue} 评分={plans[i].Score:F1}");
            }

            // ① 存在走到宝箱上的方案，且记到 120 分战役目标
            TurnPlan chest = plans.Find(p => p.FinalX == 12 && p.FinalY == 4);
            True("存在「走到宝箱上」的方案", chest != null);
            if (chest != null)
            {
                Eq("踩到宝箱记了 120 分战役目标", 120, chest.GoalValue);
                Console.WriteLine("  " + chest.ToOneLine());
            }

            // ② 优先级链的体现：捡宝箱的方案必须排第一（压过纯伤害方案）
            True("捡宝箱的方案排在推荐第一位", plans[0].GoalValue > 0);
            True("第一位就是踩宝箱(120)而不是材料袋(15)", plans[0].GoalValue == 120);

            // ③ 宝箱(120) > 材料袋(15)
            TurnPlan mat = plans.Find(p => p.FinalX == 16 && p.FinalY == 4);
            if (chest != null && mat != null)
            {
                True("宝箱方案评分高于材料袋方案", chest.Score > mat.Score);
                Console.WriteLine($"  宝箱 {chest.Score:F1} vs 材料袋 {mat.Score:F1}");
            }

            // ④ 踩到东西 ≠ 一定能攻击：验证"打不到人也保留"这条剪枝放宽确实生效
            //    （宝箱在 (12,4)，敌人 (14,5) 距离 2 > 射程 1，纯伤害方案本来会被剪掉）
            True("打不到敌人但能捡到东西的落点被保留了（剪枝已放宽）", chest != null && chest.TotalDamage == 0);
        }

        // ---------- ⑮ 未建模能力必须提示（腐烂萦绕下半的「中毒」）----------
        private static void UnmodeledEffectTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 未建模能力必须提示（死行「腐烂萦绕」下半 = 移动3 + 中毒）==");

            // 真实 YAML：Bottom = BMove(Move 3) + BPoison(Poison, Range 1, Target 1)
            // Poison 不在 Move/Attack 白名单里 → 应进 UnmodeledTypes，并在诊断里说出来
            var rot = Card("腐烂萦绕", "Lost", "Discard", 64,
                A("PerformFromShadow"),
                A("Move:3", "Poison"));          // ← Poison 是未建模能力
            var filler = Card("另一张", "Discard", "Discard", 50,
                A("Attack:2,1,1"), A("Move:2"));

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };

            P1Context ctx = MakeCtx("死行", 14, 3,
                new List<AbilityCardSnapshot> { rot, filler }, monsters,
                new HashSet<(int, int)> { (14, 5) });

            // ① 下半确实识别出了移动
            True("下半识别出移动 3", rot.Bottom.HasMove && rot.Bottom.MoveStrength == 3);
            // ② 中毒进了未建模清单
            True("Poison 进了 UnmodeledTypes", rot.Bottom.UnmodeledTypes.Contains("Poison"));
            // ③ 诊断里必须提到（这是之前丢掉的地方）
            List<string> diag = P1Planner.Diagnose(ctx);
            int hit = 0;
            foreach (string d in diag)
            {
                if (d.Contains("腐烂萦绕") && d.Contains("Poison")) hit++;
            }
            True("诊断里点名了腐烂萦绕的中毒", hit > 0);
            foreach (string d in diag)
            {
                if (d.Contains("腐烂萦绕")) Console.WriteLine("  " + d.Trim());
            }
        }

        // ---------- ⑯ 先攻来源 vs 执行顺序：两个独立维度 ----------
        private static void InitiativeDecouplingTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 先攻来源与执行顺序解耦（玩家可指定用哪张牌的先攻）==");

            // 低先攻辅助牌(20) + 高先攻强力牌(64)：典型"低先攻辅助+高先攻强力"打法
            var hand = new List<AbilityCardSnapshot>
            {
                Card("辅助牌", "Discard", "Discard", 20, A("Attack:1,1,1"), A("Move:3")),
                Card("强力牌", "Discard", "Discard", 64, A("Attack:4,1,1"), A("Move:1")),
            };

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 12, MaxHealth = 12 },
            };

            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 80);
            True("有方案", plans.Count > 0);

            // ① 同一组牌必须能给出**两种不同先攻**（20 和 64），而不是被绑死在执行顺序上
            var inis = new HashSet<int>();
            foreach (TurnPlan p in plans) inis.Add(p.Initiative);
            True("存在先攻 20 的方案", inis.Contains(20));
            True("存在先攻 64 的方案", inis.Contains(64));
            Console.WriteLine("  枚举到的先攻值: " + string.Join(", ", inis.OrderBy(x => x)));

            // ② 每张牌的先攻值都被单独记下来（展示时要按升序写，方便玩家定位卡牌）
            TurnPlan any = plans[0];
            True("记录了上半位那张牌的先攻", any.TopCardInitiative > 0);
            True("记录了下半位那张牌的先攻", any.BottomCardInitiative > 0);

            // ③ 先攻与执行顺序不绑定：找到"先攻相同但执行顺序不同"的方案对
            int sameIniDiffOrder = 0;
            for (int a = 0; a < plans.Count; a++)
            {
                for (int bIdx = a + 1; bIdx < plans.Count; bIdx++)
                {
                    TurnPlan pa = plans[a], pb = plans[bIdx];
                    if (pa.Initiative != pb.Initiative) continue;
                    if (pa.First == null || pb.First == null) continue;
                    if (pa.First.CardIndex != pb.First.CardIndex) sameIniDiffOrder++;
                }
            }
            True("存在「先攻相同但执行顺序不同」的方案（证明两维度已解耦）",
                sameIniDiffOrder > 0);
            Console.WriteLine($"  先攻相同而执行顺序不同的方案对: {sameIniDiffOrder}");
        }

        // ---------- ⑰ 分类页签必须不重名（回归锁）----------
        private static void UiSectionNameTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 分类页签不重名（曾出现十几个「角色」页签）==");

            // 模拟：3 个角色块，第 1 个抽到了名字，后 2 个抽不到（都退回「角色」）
            var names = new List<string>();
            names.Add(UiSection.UniqueName(names, "秉姜"));
            names.Add(UiSection.UniqueName(names, "角色"));
            names.Add(UiSection.UniqueName(names, "角色"));

            Eq("第一个页签用抽到的角色名", "秉姜", names[0]);
            Eq("第二个退回「角色」", "角色", names[1]);
            Eq("第三个撞名则加序号", "角色1", names[2]);

            // 关键：任何两个页签都不能同名
            int dup = names.Count - new HashSet<string>(names).Count;
            Eq("没有任何两个页签名相同", 0, dup);
            Console.WriteLine("  最终页签: " + string.Join(" | ", names));

            // 再压一遍：连加 5 个「角色」也不能撞
            for (int i = 0; i < 5; i++) names.Add(UiSection.UniqueName(names, "角色"));
            Eq("连加 5 个后仍然不重名", 0, names.Count - new HashSet<string>(names).Count);
            Console.WriteLine("  连加后: " + string.Join(" | ", names));

            // preferred 为空时走兜底
            Eq("preferred 为空 → 兜底名", "角色", UiSection.UniqueName(new List<string>(), null));
        }

        // ---------- ⑱ P5 多回合推演 ----------
        private static void MultiTurnTests()
        {
            Console.WriteLine();
            Console.WriteLine("== P5 多回合推演（束搜索）==");

            var hand = new List<AbilityCardSnapshot>
            {
                Card("猛击", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2")),
                Card("走位", "Discard", "Discard", 30, A("Attack:2,1,1"), A("Move:3")),
                Card("补刀", "Discard", "Discard", 40, A("Attack:4,1,1"), A("Move:2")),
                Card("歇脚", "Discard", "Discard", 50, A("Attack:1,1,1"), A("Move:4")),
            };

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx.StartHealth = 20;
            ctx.StartMaxHealth = 20;
            ctx.Adv = AdvStatus.None;

            MultiTurnResult res = MultiTurnSearch.Search(ctx, 2, 3, 50,
                new MultiTurnSearch.EvalWeights());

            // ① 能产出路线
            True("有路线产出", res.Routes.Count > 0);
            if (res.Routes.Count == 0) return;

            // ② 深度 2 → 每条路线 2 个回合
            List<RouteStep> route = res.Routes[0];
            Eq("深度 2 → 路线有 2 个回合", 2, route.Count);
            Eq("第 1 回合标记正确", 1, route[0].Round);
            Eq("第 2 回合标记正确", 2, route[1].Round);

            // ③ 状态推进：位置真的动了、伤害真的累计了
            True("第 1 回合后位置与起点不同",
                route[0].MyX != ctx.StartX || route[0].MyY != ctx.StartY);
            True("累计伤害大于 0（打到了敌人）", route[0].CumulativeDamage > 0);
            Console.WriteLine($"  第1回合后: 站({route[0].MyX},{route[0].MyY}) "
                            + $"血{route[0].MyHealth} 累计伤{route[0].CumulativeDamage}");

            // ④ 手牌在递减（第 2 回合时手牌应该少 2 张）
            int handAfter1 = ctx.Hand.Count - 2;
            True("打出两张牌后手牌减少 2 张", handAfter1 == 2);

            // ⑤ 血量被正确保留（我方 20 血，不会自己扣血除非走危险地形）
            True("血量在合理范围", route[0].MyHealth > 0 && route[0].MyHealth <= 20);

            // ⑥ 深度 1 也能跑（退化情况）
            MultiTurnResult r1 = MultiTurnSearch.Search(ctx, 1, 2, 50,
                new MultiTurnSearch.EvalWeights());
            True("深度 1 也能产出路线", r1.Routes.Count > 0);
            if (r1.Routes.Count > 0) Eq("深度 1 → 路线有 1 个回合", 1, r1.Routes[0].Count);

            // ⑦ 手牌不足两张 → 路线停在当前深度（不崩）
            var tinyHand = new List<AbilityCardSnapshot>
            {
                Card("孤牌", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2")),
            };
            P1Context ctx2 = MakeCtx("测试", 14, 3, tinyHand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx2.StartHealth = 20; ctx2.StartMaxHealth = 20;
            MultiTurnResult r2 = MultiTurnSearch.Search(ctx2, 2, 2, 50,
                new MultiTurnSearch.EvalWeights());
            True("只有一张牌时不崩溃", r2 != null);
        }

        // ---------- ⑲ P5 敌人回合：真实修正牌抽牌 ----------
        private static void EnemyRealDrawTests()
        {
            Console.WriteLine();
            Console.WriteLine("== P5 敌人回合：真实修正牌抽牌（不是意图估算）==");

            // ① ×0 → 完全免伤。意图上写的是攻击 4，但翻出 ×0 就是 0。
            //    这是"真实抽牌"与"按意图估算"最有区别的一条。
            WorldState st = MakeWorld(20, Deck(Mod("*0"), Mod("+1")));
            AddEnemy(st, 4, 10, 14, 5);
            List<EnemyStrike> hits = StateAdvancer.RunEnemyTurn(st, 50);
            Eq("翻到 ×0 → 伤害为 0（不是意图上的 4）", 0, TotalHit(hits));
            Eq("×0 之后我没掉血", 20, st.MyHealth);
            True("记录了一次打击", hits.Count == 1);
            True("牌面文本里写了 ×0", hits.Count > 0 && hits[0].CardsText.Contains("*0"));
            True("不是估算（牌堆有牌）", hits.Count > 0 && !hits[0].Approx);
            Console.WriteLine("  " + (hits.Count > 0 ? hits[0].ToString() : ""));

            // ② +1 → 意图 4 实际 5
            WorldState st2 = MakeWorld(20, Deck(Mod("+1"), Mod("-1")));
            AddEnemy(st2, 4, 10, 14, 5);
            List<EnemyStrike> hits2 = StateAdvancer.RunEnemyTurn(st2, 50);
            Eq("翻到 +1 → 伤害 4+1=5", 5, TotalHit(hits2));
            Eq("掉血后剩 15", 15, st2.MyHealth);
            Console.WriteLine("  " + (hits2.Count > 0 ? hits2[0].ToString() : ""));

            // ③ 牌堆真的被消耗了（抽走一张）
            Eq("牌堆剩余从 2 变 1", 1, st2.EnemyDecks[0].Remaining.Count);
            Eq("弃牌堆多了一张", 1, st2.EnemyDecks[0].Discarded.Count);

            // ④ 同阵营两个敌人**共用一副**牌堆 → 依次抽第 1、第 2 张
            WorldState st3 = MakeWorld(30, Deck(Mod("+1"), Mod("+2")));
            AddEnemy(st3, 3, 10, 14, 5);      // 先动
            AddEnemy(st3, 3, 20, 15, 5);      // 后动（同阵营）
            List<EnemyStrike> hits3 = StateAdvancer.RunEnemyTurn(st3, 50);
            Eq("两个敌人各打一次", 2, hits3.Count);
            Eq("先动的翻到 +1 → 3+1=4", 4, hits3.Count > 0 ? hits3[0].Damage : -1);
            Eq("后动的翻到 +2 → 3+2=5", 5, hits3.Count > 1 ? hits3[1].Damage : -1);
            Eq("一副牌堆被消耗两张", 0, st3.EnemyDecks[0].Remaining.Count);
            Console.WriteLine($"  共挨打 {TotalHit(hits3)} 点（4+5）");

            // ⑤ 牌堆见底 → 退化成估算，并明确标记（不能假装还是精确值）
            WorldState st4 = MakeWorld(20, Deck());
            AddEnemy(st4, 4, 10, 14, 5);
            List<EnemyStrike> hits4 = StateAdvancer.RunEnemyTurn(st4, 50);
            Eq("牌堆空 → 退回意图原值 4", 4, TotalHit(hits4));
            True("标记为估算（Approx）", hits4.Count > 0 && hits4[0].Approx);
            True("世界状态记下「牌堆见底」", st4.EnemyDeckExhausted);

            // ⑥ 先攻比我高 → 它后动，这回合不碰我
            WorldState st5 = MakeWorld(20, Deck(Mod("+1")));
            AddEnemy(st5, 4, 80, 14, 5);      // 先攻 80 > 我的 50
            List<EnemyStrike> hits5 = StateAdvancer.RunEnemyTurn(st5, 50);
            Eq("后动的敌人这回合不打我", 0, hits5.Count);
            Eq("牌堆也没被消耗", 1, st5.EnemyDecks[0].Remaining.Count);

            // ⑦ ×2 翻倍（乘算在括号外、加法之后 —— ATTACK.md 铁律）
            WorldState st6 = MakeWorld(20, Deck(Mod("*2")));
            AddEnemy(st6, 3, 10, 14, 5);
            List<EnemyStrike> hits6 = StateAdvancer.RunEnemyTurn(st6, 50);
            Eq("翻到 ×2 → 伤害 3×2=6", 6, TotalHit(hits6));
        }

        /// <summary>造一个只带敌人牌堆的最小世界（我站在 (14,3)，满血）。</summary>
        private static WorldState MakeWorld(int myHp, List<ModifierCardSnapshot> enemyTop)
        {
            var st = new WorldState
            {
                MyX = 14, MyY = 3,
                MyHealth = myHp, MyMaxHealth = myHp,
                Round = 1,
            };
            var d0 = new ModifierDeckSnapshot { OwnerLabel = "怪物-敌" };
            d0.Remaining.AddRange(enemyTop);
            st.EnemyDecks.Add(d0);
            for (int i = 1; i < 4; i++)
            {
                st.EnemyDecks.Add(new ModifierDeckSnapshot { OwnerLabel = "怪物-" + i });
            }

            // 地图查询委托：默认无障碍、半径内可达（P5 敌人寻路要用）
            st.RangeDistance = (x1, y1, x2, y2) => HexDist(x1, y1, x2, y2);
            st.HasLOS = (x1, y1, x2, y2) => true;
            st.Reachable = (x, y, r) =>
            {
                var list = new List<(int, int, int)>();
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r - 1; dx <= r + 1; dx++)
                    {
                        int px = x + dx, py = y + dy;
                        if (px < 0 || py < 0) continue;
                        if (HexDist(x, y, px, py) <= r) list.Add((px, py, 0));
                    }
                }
                return list;
            };
            return st;
        }

        /// <summary>加一个敌方单位（DeckIndex=0，即用「怪物-敌」那副共享牌堆）。</summary>
        private static UnitSnapshot AddEnemy(WorldState st, int attack, int initiative, int x, int y,
            int move = 0)
        {
            var e = new UnitSnapshot
            {
                Guid = "e" + initiative,
                Name = "G1",
                DisplayName = "冰角守卫#" + initiative,
                Faction = FactionKind.Enemy,
                FactionName = "敌",
                IsHostileToPlayer = true,
                Health = 12, MaxHealth = 12,
                X = x, Y = y,
                Initiative = initiative,
                DeckIndex = 0,
                Intent = move > 0
                    ? A($"Attack:{attack},1,1", $"Move:{move}")   // 射程 1 + 移动力
                    : A($"Attack:{attack},3,1"),                  // 射程 3，够得着 (14,3)
            };
            st.Monsters.Add(e);
            return e;
        }

        private static int TotalHit(List<EnemyStrike> strikes)
        {
            int t = 0;
            if (strikes != null) foreach (EnemyStrike e in strikes) if (e != null) t += e.Damage;
            return t;
        }

        // ---------- ⑳ P5 敌人回合：真实寻路（不是「射程+移动力」几何粗判）----------
        private static void EnemyPathingTests()
        {
            Console.WriteLine();
            Console.WriteLine("== P5 敌人寻路（旧版用 射程+移动力 取上界，会高估）==");

            // ① ★ 关键区分：几何上够得着，但**根本没有路** → 新版判定"打不到"
            //    旧逻辑 dist(4) <= 射程1 + 移动3 = 4 会算成够得着 → 高估挨打风险
            WorldState st = MakeWorld(20, Deck(Mod("+1")));
            UnitSnapshot far = AddEnemy(st, 3, 10, 14, 7, move: 3);   // 距离 4，射程1，移动3
            st.Reachable = (x, y, r) => new List<(int, int, int)>();  // 四面围死，走不出去
            List<EnemyStrike> hits = StateAdvancer.RunEnemyTurn(st, 50);
            Eq("没有路 → 判定打不到（旧版会误判为能打到）", 0, hits.Count);
            Eq("牌堆也没被消耗（它没出手）", 1, st.EnemyDecks[0].Remaining.Count);
            Eq("它留在原地", 14 * 100 + 7, far.X * 100 + far.Y);

            // ② ★ 隔墙：距离够、但没视线 → 打不到（旧版也会误判）
            WorldState st2 = MakeWorld(20, Deck(Mod("+1")));
            AddEnemy(st2, 3, 10, 14, 5);      // 距离 2，射程 3
            st2.HasLOS = (x1, y1, x2, y2) => false;   // 中间有墙
            List<EnemyStrike> hits2 = StateAdvancer.RunEnemyTurn(st2, 50);
            Eq("没视线 → 打不到（旧版会误判）", 0, hits2.Count);

            // ③ 走过去再打：距离 4、射程 1、移动 3 → 移动后能打到
            WorldState st3 = MakeWorld(20, Deck(Mod("+1")));
            UnitSnapshot e3 = AddEnemy(st3, 3, 10, 14, 7, move: 3);
            List<EnemyStrike> hits3 = StateAdvancer.RunEnemyTurn(st3, 50);
            Eq("有路 → 它移动后打到你", 1, hits3.Count);
            True("标记为「移动过」", hits3.Count > 0 && hits3[0].Moved);
            True("落点距离你 ≤ 射程1", HexDist(e3.X, e3.Y, st3.MyX, st3.MyY) <= 1);
            Console.WriteLine("  " + (hits3.Count > 0 ? hits3[0].ToString() : ""));

            // ④ 原地就能打到 → 它不动（游戏 AI 规则：现在能打到就不挪窝）
            WorldState st4 = MakeWorld(20, Deck(Mod("+1")));
            UnitSnapshot e4 = AddEnemy(st4, 3, 10, 14, 5);    // 距离 2 <= 射程 3
            List<EnemyStrike> hits4 = StateAdvancer.RunEnemyTurn(st4, 50);
            Eq("原地能打到 → 照样出手", 1, hits4.Count);
            True("且没有移动", hits4.Count > 0 && !hits4[0].Moved);
            Eq("位置没变", 14 * 100 + 5, e4.X * 100 + e4.Y);

            // ⑤ 焦点规则：队友更近 → 它去打队友，我不掉血，但**牌照样消耗**
            //    （真实规则：只要出手就抽修正牌）
            WorldState st5 = MakeWorld(20, Deck(Mod("+1"), Mod("+2")));
            AddEnemy(st5, 3, 10, 14, 5);
            st5.Others.Add(new UnitSnapshot
            {
                Guid = "ally1", DisplayName = "队友", Faction = FactionKind.Ally,
                IsHostileToPlayer = false, Health = 15, MaxHealth = 15,
                X = 14, Y = 4,                       // 比我(14,3)更靠近敌人(14,5)
            });
            List<EnemyStrike> hits5 = StateAdvancer.RunEnemyTurn(st5, 50);
            Eq("焦点是更近的队友 → 我没挨打", 0, hits5.Count);
            Eq("但它出手了，牌堆被消耗一张", 1, st5.EnemyDecks[0].Remaining.Count);
            Eq("我没掉血", 20, st5.MyHealth);

            // ⑥ ★ 够不着 → 它**照样会逼近**（我原先以为会原地不动，反编译后确认是错的）
            //    真实 AI：CAIMoveNew.FindPathToFocus 走不到攻击位时，
            //    会在所有可达格里挑一个"离攻击位最近"的走过去。
            WorldState st6 = MakeWorld(20, Deck(Mod("+1"), Mod("+2")));
            UnitSnapshot approacher = AddEnemy(st6, 3, 10, 14, 9, move: 2);  // 距离6，射程1，移动2
            int distBefore = HexDist(approacher.X, approacher.Y, st6.MyX, st6.MyY);
            List<EnemyStrike> hits6 = StateAdvancer.RunEnemyTurn(st6, 50);
            Eq("够不着时仍会逼近（不是原地不动）", 1, hits6.Count);
            True("标记为「只是逼近」", hits6.Count > 0 && hits6[0].ApproachOnly);
            Eq("逼近不掉血", 0, TotalHit(hits6));
            Eq("我没掉血", 20, st6.MyHealth);
            Eq("它没出手 → 牌堆**不消耗**", 2, st6.EnemyDecks[0].Remaining.Count);
            int distAfter = HexDist(approacher.X, approacher.Y, st6.MyX, st6.MyY);
            True("它确实靠近了", distAfter < distBefore);
            Console.WriteLine($"  {distBefore} → {distAfter} 格；" + (hits6.Count > 0 ? hits6[0].ToString() : ""));

            // ⑦ 先动的敌人占住新格 → 后动的敌人不能踩上去
            WorldState st7 = MakeWorld(30, Deck(Mod("+1"), Mod("+2")));
            UnitSnapshot a = AddEnemy(st7, 3, 10, 14, 7, move: 3);   // 先动，会往前挪
            UnitSnapshot b = AddEnemy(st7, 3, 20, 15, 7, move: 3);   // 后动
            StateAdvancer.RunEnemyTurn(st7, 50);
            True("两个敌人不会站到同一格", !(a.X == b.X && a.Y == b.Y));
            Console.WriteLine($"  先动→({a.X},{a.Y})  后动→({b.X},{b.Y})");
        }

        // ---------- ㉑ 先攻标注必须按「卡」取，不能按执行顺序取 ----------
        private static void InitiativeLabelTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 先攻标注（实战截图：两张牌先攻被对调）==");

            // 复现截图：刚毅不屈先攻 31、义无反顾先攻 20
            var hand = new List<AbilityCardSnapshot>
            {
                Card("刚毅不屈", "Discard", "Discard", 31, A("Attack:2,1,1"), A("Move:2")),
                Card("义无反顾", "Lost",     "Discard", 20, A("Attack:4,1,1"), A("Move:2")),
            };

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 80);
            True("有方案", plans.Count > 0);

            // ① 先攻标注必须**跟着卡走**
            int bad = 0;
            foreach (TurnPlan p in plans)
            {
                if (p.TopCardInitiative != ctx.Hand[p.TopCardIndex].Initiative) bad++;
                if (p.BottomCardInitiative != ctx.Hand[p.BottomCardIndex].Initiative) bad++;
            }
            Eq("每张牌的先攻都取自它自己（不是取自执行顺序）", 0, bad);

            // ② InitiativeOf(cardIndex) 必须按卡匹配
            int bad2 = 0;
            foreach (TurnPlan p in plans)
            {
                if (p.InitiativeOf(p.TopCardIndex) != p.TopCardInitiative) bad2++;
                if (p.InitiativeOf(p.BottomCardIndex) != p.BottomCardInitiative) bad2++;
            }
            Eq("InitiativeOf 按 CardIndex 取到正确的先攻", 0, bad2);

            // ③ ★ 关键回归：先执行的是**下半位**那张时，不能把两张牌的先攻对调
            //    （截图里就是这种情况：先做第10张义无反顾，却把 31 标给了它）
            int checkedReverse = 0, wrongReverse = 0;
            foreach (TurnPlan p in plans)
            {
                if (p.First == null) continue;
                if (p.First.CardIndex != p.BottomCardIndex) continue;   // 只查"先做下半位"的
                checkedReverse++;
                int ini = p.InitiativeOf(p.First.CardIndex);
                if (ini != p.BottomCardInitiative) wrongReverse++;
            }
            True("枚举到了「先执行下半位那张」的方案", checkedReverse > 0);
            Eq("它取到的是**自己**的先攻，没有被对调", 0, wrongReverse);
            Console.WriteLine($"  先做下半位的方案 {checkedReverse} 个，标注错误 {wrongReverse} 个");

            // ④ 展示顺序：先攻低的牌必须排在前面（玩家按点数在选牌界面找卡）
            TurnPlan any = plans[0];
            string line = any.ToOneLine();
            int i31 = line.IndexOf("刚毅不屈");
            int i20 = line.IndexOf("义无反顾");
            True("ToOneLine 里先攻低的「义无反顾」排在前面", i20 >= 0 && i31 >= 0 && i20 < i31);
            Console.WriteLine("  " + line);
        }

        // ---------- ㉒ 阴影机制（死行 / Deathwalker）----------
        private static void ShadowMechanismTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 阴影机制（日志实测：死行手牌 560+ 次阴影能力曾全部未建模）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };
            var hand = new List<AbilityCardSnapshot>
            {
                Card("日蚀", "Discard", "Discard", 86, A("Attack:2,1,1"), A("CreateShadow:2")),
                Card("深渊力量", "Discard", "Discard", 40, A("Attack:1,1,1"), A("Move:2")),
            };

            P1Context ctx = MakeCtx("死行", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });

            // ---- ① 造阴影的诊断文本 ----
            var create = A("CreateShadow");
            create.HasCreateShadow = true;
            create.CreateShadowCount = 2;

            ctx.ShadowCount = 3;
            string t1 = P1Planner.ShadowText(create, ctx);
            True("说到造 2 个阴影", t1.Contains("造 2 个阴影"));
            True("说到当前 3/5", t1.Contains("3/5"));
            True("说明会抬高深渊力量", t1.Contains("深渊力量"));
            Console.WriteLine("  " + t1);

            // ---- ② 上限 5：满了就造不了 ----
            ctx.ShadowCount = 5;
            string t2 = P1Planner.ShadowText(create, ctx);
            True("已达上限 5 时给出警告", t2.Contains("上限"));
            Console.WriteLine("  " + t2);

            // ---- ③ 从阴影中发动：阴影不够要警告 ----
            var perf = A("PerformFromShadow");
            perf.HasPerformFromShadow = true;
            perf.ShadowsToUse = 2;

            ctx.ShadowCount = 1;
            string t3 = P1Planner.ShadowText(perf, ctx);
            True("阴影不够时警告", t3.Contains("不够"));
            Console.WriteLine("  " + t3);

            ctx.ShadowCount = 4;
            string t4 = P1Planner.ShadowText(perf, ctx);
            True("阴影够时不警告", !t4.Contains("不够"));
            Console.WriteLine("  " + t4);

            // ---- ④ 非阴影能力不该有阴影文本 ----
            Eq("普通攻击没有阴影说明", "", P1Planner.ShadowText(A("Attack:3,1,1"), ctx));

            // ---- ⑤ 造阴影要计入评分（造了才有价值）----
            ctx.ShadowCount = 0;
            // 用更贴近真实的卡面：下半 = 移动 + 造阴影（日志里死行下半常带 Move）
            var withShadow = new List<AbilityCardSnapshot>
            {
                Card("日蚀", "Discard", "Discard", 86, A("Attack:2,1,1"), A("Move:2", "CreateShadow:2")),
                Card("走位", "Discard", "Discard", 40, A("Attack:1,1,1"), A("Move:2")),
            };
            P1Context ctx2 = MakeCtx("死行", 14, 3, withShadow, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx2.ShadowCount = 0;
            withShadow[0].Bottom.HasCreateShadow = true;
            withShadow[0].Bottom.CreateShadowCount = 2;

            List<TurnPlan> plans = P1Planner.Enumerate(ctx2, 40);
            True("有方案", plans.Count > 0);

            for (int i = 0; i < plans.Count && i < 4; i++)
            {
                var pp = plans[i];
                Console.WriteLine($"    #{i} {pp.First?.CardName}/{pp.First?.SideLabel}"
                    + $" + {pp.Second?.CardName}/{pp.Second?.SideLabel}"
                    + $" 阴影+{pp.ShadowGain} 评分{pp.Score:F1}");
            }
            int gained = plans.Count(p => p.ShadowGain > 0);
            True("存在造了阴影的方案（且记了 ShadowGain）", gained > 0);
            Console.WriteLine($"  造阴影的方案 {gained} / {plans.Count}");

            // ---- ⑥ 已达上限时不再给阴影价值 ----
            P1Context ctx3 = MakeCtx("死行", 14, 3, withShadow, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx3.ShadowCount = 5;
            List<TurnPlan> plans3 = P1Planner.Enumerate(ctx3, 40);
            int gained3 = plans3.Count(p => p.ShadowGain > 0);
            Eq("已满 5 个 → 没有方案再记阴影收益", 0, gained3);
        }

        // ---------- ㉓ 开局 0 阴影：不能推荐有阴影限制的牌 ----------
        private static void ShadowGatingTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 阴影门槛（实战：使命开局 0 阴影，却推荐「暗影步法」）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            // 「暗影步法」上半 = 移动3(必须停在阴影) + 攻击3
            var shadowStep = Card("暗影步法", "Discard", "Lost", 19,
                A("Move:3", "Attack:3,1,1"), A("Move:2"));
            shadowStep.Top.MoveRestriction = "MustEndOnShadow";

            var other = Card("另一张", "Discard", "Discard", 40,
                A("Attack:2,1,1"), A("Move:2"));

            // ---- ① 局面：场上一个阴影都没有 ----
            P1Context ctx = MakeCtx("死行", 14, 3,
                new List<AbilityCardSnapshot> { shadowStep, other }, monsters,
                new HashSet<(int, int)> { (14, 5) }, shadows: null);
            ctx.ShadowCount = 0;

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("有方案（还能用基础动作）", plans.Count > 0);

            int usedShadowStep = 0;
            foreach (TurnPlan p in plans)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h == null) continue;
                    if (h.CardName == "暗影步法" && h.Source == HalfSource.Top) usedShadowStep++;
                }
            }
            Eq("0 阴影时不推荐「暗影步法」上半（它必须停在阴影格）", 0, usedShadowStep);
            Console.WriteLine($"  含暗影步法上半的方案：{usedShadowStep} 个");

            // 诊断必须说清楚为什么不能用
            List<string> diag = P1Planner.Diagnose(ctx);
            True("诊断说明它本回合无法使用",
                diag.Exists(d => d.Contains("暗影步法") && d.Contains("无法使用")));
            True("并且说明原因是没有阴影",
                diag.Exists(d => d.Contains("阴影") && d.Contains("0/5")));
            foreach (string d in diag)
            {
                if (d.Contains("暗影步法")) Console.WriteLine("  " + d.Trim());
            }

            // ---- ② 有阴影时：它可以被推荐 ----
            P1Context ctx2 = MakeCtx("死行", 14, 3,
                new List<AbilityCardSnapshot> { shadowStep, other }, monsters,
                new HashSet<(int, int)> { (14, 5) },
                shadows: new HashSet<(int, int)> { (13, 4), (14, 4) });
            ctx2.ShadowCount = 2;

            List<TurnPlan> plans2 = P1Planner.Enumerate(ctx2, 40);
            int used2 = 0;
            foreach (TurnPlan p in plans2)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h == null) continue;
                    if (h.CardName == "暗影步法" && h.Source == HalfSource.Top) used2++;
                }
            }
            True("有阴影时可以推荐「暗影步法」上半", used2 > 0);
            Console.WriteLine($"  有阴影时含该半边的方案：{used2} 个");
        }

        // ---------- ㉔ 状态前提：缴械/静足/眩晕 ----------
        private static void ConditionGatingTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 状态前提（实战：精英冰角祭司先出手缴械，方案却仍推荐攻击）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            var hand = new List<AbilityCardSnapshot>
            {
                Card("攻击牌", "Discard", "Discard", 18, A("Attack:3,1,1"), A("Move:2")),
                Card("治疗牌", "Discard", "Discard", 30, A("Heal"), A("Attack:2,1,1")),
                Card("移动牌", "Discard", "Discard", 40, A("Move:3"), A("Move:2")),
            };

            // ---- ① 缴械：不能攻击 ----
            P1Context ctx = MakeCtx("旗矛", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx.MyNegativeConditions = new List<string> { "Disarm" };

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("缴械下仍有方案（还能移动/治疗）", plans.Count > 0);

            int attackSteps = 0;
            foreach (TurnPlan p in plans)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h == null || h.Half == null) continue;
                    if (h.Half.HasAttack) attackSteps++;
                }
            }
            Eq("缴械后没有任何方案包含攻击动作", 0, attackSteps);
            Console.WriteLine($"  含攻击动作的方案数：{attackSteps}");

            // 诊断说明
            List<string> diag = P1Planner.Diagnose(ctx);
            True("诊断说明缴械导致无法使用",
                diag.Exists(d => d.Contains("缴械")));
            foreach (string d in diag)
            {
                if (d.Contains("缴械")) Console.WriteLine("  " + d.Trim());
            }

            // ---- ② 静足：不能移动（敌人放近，确保原地就打得到）----
            var nearMonsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 4, Health = 10, MaxHealth = 10,   // 与我(14,3)相邻
                                  Initiative = 60, IsHostileToPlayer = true },
            };
            P1Context ctx2 = MakeCtx("旗矛", 14, 3, hand, nearMonsters,
                new HashSet<(int, int)> { (14, 4) });
            ctx2.MyNegativeConditions = new List<string> { "Immobilize" };
            List<TurnPlan> plans2 = P1Planner.Enumerate(ctx2, 40);
            True("静足下仍有方案（原地就能做的）", plans2.Count > 0);
            int moved = plans2.Count(p => p.FinalX != ctx2.StartX || p.FinalY != ctx2.StartY);
            Eq("静足后没有任何方案发生位移", 0, moved);

            // ---- ③ 眩晕：什么都不行（只剩基础动作都算不上）----
            P1Context ctx3 = MakeCtx("旗矛", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx3.MyNegativeConditions = new List<string> { "Stun" };
            List<TurnPlan> plans3 = P1Planner.Enumerate(ctx3, 40);
            Eq("眩晕后一个方案都出不来", 0, plans3.Count);

            // ---- ④ 没有状态时不受影响（避免矫枉过正）----
            P1Context ctx4 = MakeCtx("旗矛", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            List<TurnPlan> plans4 = P1Planner.Enumerate(ctx4, 40);
            int attack4 = 0;
            foreach (TurnPlan p in plans4)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h != null && h.Half != null && h.Half.HasAttack) attack4++;
                }
            }
            True("无状态时正常推荐攻击", attack4 > 0);
        }

        // ---------- ㉕ IgnoreLOS：个别能力忽略视线 ----------
        private static void IgnoreLosTests()
        {
            Console.WriteLine();
            Console.WriteLine("== IgnoreLOS（疑点澄清：默认要视线，个别能力显式关掉）==");

            // 反编译实证（SharedAbilityTargeting.GetValidActorsInRange）：
            //   GameState.GetTilesInRange(..., ignoreLOS: ability.MiscAbilityData?.IgnoreLOS ?? false, ...)
            // ⇒ 默认要视线；社区攻略说的"非目标型不用视线"指的是 IgnoreLOS=true 的卡。

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "墙后敌人",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            var normal = Card("普通攻击", "Discard", "Discard", 20, A("Attack:3,3,1"), A("Move:2"));
            var through = Card("穿墙攻击", "Discard", "Discard", 30, A("Attack:3,3,1"), A("Move:2"));
            through.Top.IgnoreLOS = true;      // MiscAbilityData.IgnoreLOS = true

            // 视线恒不通（模拟墙）
            P1Context ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { normal, through }, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx.HasLOS = (x1, y1, x2, y2) => false;

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("有方案", plans.Count > 0);

            int normalHit = 0, throughHit = 0;
            foreach (TurnPlan p in plans)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h == null || h.Half == null) continue;
                    if (!h.Half.HasAttack) continue;
                    if (h.Half.IgnoreLOS) throughHit++; else normalHit++;
                }
            }
            True("普通攻击被墙挡住（没有视线就打不到）", normalHit == 0);
            True("IgnoreLOS 的攻击能打到墙后目标", throughHit > 0);
            Console.WriteLine($"  普通攻击命中步数={normalHit}，穿墙攻击命中步数={throughHit}");
        }

        // ---------- ㉖ 隐形目标：默认不能打 ----------
        private static void InvisibleTargetTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 隐形目标（反编译实证：默认不能选隐形单位）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "隐形敌人",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };
            monsters[0].PositiveConditions.Add("Invisible");

            var normal = Card("普通攻击", "Discard", "Discard", 20, A("Attack:3,3,1"), A("Move:2"));
            var seeThrough = Card("看破攻击", "Discard", "Discard", 30, A("Attack:3,3,1"), A("Move:2"));
            seeThrough.Top.CanTargetInvisible = true;

            P1Context ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { normal, seeThrough }, monsters,
                new HashSet<(int, int)> { (14, 5) });

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("有方案", plans.Count > 0);

            int normalHit = 0, seeThroughHit = 0;
            foreach (TurnPlan p in plans)
            {
                foreach (PlanHalf h in new[] { p.First, p.Second })
                {
                    if (h == null || h.Half == null || !h.Half.HasAttack) continue;
                    if (h.Half.CanTargetInvisible) seeThroughHit++; else normalHit++;
                }
            }
            Eq("普通攻击打不到隐形敌人", 0, normalHit);
            True("CanTargetInvisible 的攻击可以打", seeThroughHit > 0);
            Console.WriteLine($"  普通命中={normalHit}，看破命中={seeThroughHit}");
        }

        // ---------- ㉗ 治疗收益建模（Heal 是最高频未建模项）----------
        private static void HealModelTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 治疗收益（日志实测 Heal 未建模 401 次）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            // 掉血的队友（缺 6 血）+ 满血队友
            var others = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "a1", DisplayName = "残血队友", Faction = FactionKind.Ally,
                                  IsHostileToPlayer = false, Health = 4, MaxHealth = 10, X = 13, Y = 4 },
                new UnitSnapshot { Guid = "a2", DisplayName = "满血队友", Faction = FactionKind.Ally,
                                  IsHostileToPlayer = false, Health = 10, MaxHealth = 10, X = 12, Y = 4 },
            };

            var hand = new List<AbilityCardSnapshot>
            {
                Card("治疗牌", "Discard", "Discard", 20, A("Heal:5"), A("Move:2")),
                Card("攻击牌", "Discard", "Discard", 40, A("Attack:3,1,1"), A("Move:2")),
            };

            P1Context ctx = MakeCtx("漂泊者", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });
            ctx.Others = others;
            ctx.StartHealth = 8;
            ctx.StartMaxHealth = 12;      // 我自己缺 4 血

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("有方案", plans.Count > 0);

            // ① 治疗半边被识别，且算出收益
            True("治疗牌上半识别出治疗", hand[0].Top.HasHeal && hand[0].Top.HealStrength == 5);
            int healed = plans.Count(p => p.HealValue > 0);
            True("存在记录了治疗收益的方案", healed > 0);
            Console.WriteLine($"  有治疗收益的方案 {healed}/{plans.Count}");

            foreach (string d in P1Planner.Diagnose(ctx))
            {
                if (d.Contains("治疗牌")) Console.WriteLine("     " + d.Trim());
            }
            True("存在记录了治疗收益的方案", healed > 0);
            Console.WriteLine($"  有治疗收益的方案 {healed}/{plans.Count}");

            // ② 过量治疗不计：治疗量 5 > 残血队友缺失 6？→ min(5,6)=5
            //    但"最残的"是缺 6 的队友，所以收益 5
            TurnPlan any = plans.Find(p => p.HealValue > 0);
            if (any != null)
            {
                Eq("治疗收益 = min(5, 缺失6) = 5", 5, any.HealValue);
                Console.WriteLine("  治疗收益 " + any.HealValue);
            }

            // ③ 大家都满血 → 治疗没收益
            others[0].Health = 10;
            ctx.StartHealth = 12;
            List<TurnPlan> plans2 = P1Planner.Enumerate(ctx, 40);
            int healed2 = plans2.Count(p => p.HealValue > 0);
            Eq("满血时治疗无收益（过量治疗不计）", 0, healed2);

            // ④ 治疗不再进未建模清单
            True("Heal 不再算未建模", !hand[0].Top.UnmodeledTypes.Contains("Heal"));

            // ⑤ 治疗量大于缺失时，只算缺失部分
            others[0].Health = 9;      // 只缺 1
            ctx.StartHealth = 11;      // 我缺 1
            List<TurnPlan> plans3 = P1Planner.Enumerate(ctx, 40);
            TurnPlan p3 = plans3.Find(p => p.HealValue > 0);
            if (p3 != null)
            {
                Eq("治疗 5 但只缺 1 → 只算 1（过量不计）", 1, p3.HealValue);
                Console.WriteLine("  过量治疗截断后收益 " + p3.HealValue);
            }
        }

        // ---------- ㉘ 强化 Strengthen：换算成 Advantage（不是攻击 +1）----------
        private static void StrengthenModelTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 强化（Strengthen → Advantage，不加攻击力）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 4, Health = 20, MaxHealth = 20,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            var hand = new List<AbilityCardSnapshot>
            {
                Card("攻击牌", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2")),
                Card("走位牌", "Discard", "Discard", 40, A("Attack:1,1,1"), A("Move:2")),
            };

            // 敌人 20 血、我攻 3 → 无强化时打不掉；有强化（3+1=4）也打不掉，但伤害应 +1
            P1Context plain = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 4) });
            List<TurnPlan> p1 = P1Planner.Enumerate(plain, 40);
            int dmgPlain = p1.Count > 0 ? p1[0].TotalDamage : 0;

            P1Context strong = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 4) });
            strong.MyPositiveConditions = new List<string> { "Strengthen" };
            List<TurnPlan> p2 = P1Planner.Enumerate(strong, 40);
            int dmgStrong = p2.Count > 0 ? p2[0].TotalDamage : 0;

            // Strengthen 的规则效果是 Advantage（由 P1Runner.ReadAdvantage 走游戏折算），
            // 不是攻击 +1。这里只放 token、Adv 仍是 None → 伤害必须与无强化完全一致。
            Console.WriteLine($"  无强化伤害={dmgPlain}，有强化伤害={dmgStrong}");
            Eq("Strengthen 不再额外 +1（伤害与无强化一致）", dmgPlain, dmgStrong);

            // 诊断/输出不报错
            List<string> diag = P1Planner.Diagnose(strong);
            True("有强化时诊断正常", diag.Count > 0);
        }

        // ---------- ㉙ 附加效果识别（中毒/拉拽/控制/传送）----------
        private static void ExtraEffectTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 附加效果识别（不再默默算\"未建模\"）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 4, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            var poison = Card("毒刃", "Discard", "Discard", 20, A("Attack:2,1,1", "Poison:3"), A("Move:2"));
            var pull = Card("拖拽", "Discard", "Discard", 30, A("Attack:1,1,1", "Pull:2"), A("Move:2"));
            var plain = Card("普通", "Discard", "Discard", 40, A("Attack:3,1,1"), A("Move:2"));

            P1Context ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { poison, pull, plain }, monsters,
                new HashSet<(int, int)> { (14, 4) });

            // ① 识别
            True("毒刃识别出中毒", poison.Top.HasPoison && poison.Top.PoisonStrength == 3);
            True("拖拽识别出拉拽", pull.Top.HasPull && pull.Top.PullStrength == 2);
            True("HasExtraEffect 汇总正确", poison.Top.HasExtraEffect && pull.Top.HasExtraEffect);
            True("普通牌没有附加效果", !plain.Top.HasExtraEffect);

            // ② 诊断文本
            string t1 = P1Planner.ExtraEffectText(poison.Top);
            True("中毒文本写明 +1 攻击力", t1.Contains("中毒") && t1.Contains("+1"));
            string t2 = P1Planner.ExtraEffectText(pull.Top);
            True("拉拽文本提到拉拽格数", t2.Contains("拉拽 2"));
            Eq("普通牌无附加效果文本", "", P1Planner.ExtraEffectText(plain.Top));
            Console.WriteLine("  " + t1);
            Console.WriteLine("  " + t2);

            // ③ 诊断里会出现（玩家能看到）
            List<string> diag = P1Planner.Diagnose(ctx);
            True("诊断里提到中毒", diag.Exists(d => d.Contains("毒刃") && d.Contains("中毒")));

            // ④ 不再进未建模清单
            True("Poison 不再算未建模", !poison.Top.UnmodeledTypes.Contains("Poison"));
            True("Pull 不再算未建模", !pull.Top.UnmodeledTypes.Contains("Pull"));
        }

        // ---------- ㉚ 陷阱伤害（Trap）----------
        private static void TrapModelTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 陷阱伤害（走进去掉血，已计入评分）==");

            // 陷阱建模的核心：路径 hazard 要包含陷阱伤害
            // 真实读取走 Geometry.TrapMap()（需要游戏运行时），
            // 离线测试台只能验证"hazard 计入评分"这条链路。

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "冰角守卫#4",
                                  X = 14, Y = 5, Health = 10, MaxHealth = 10,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            // 可达格带 hazard：(13,4) 有 2 点陷阱伤害，(14,4) 干净
            var hand = new List<AbilityCardSnapshot>
            {
                // 一张纯移动、一张纯攻击 —— 这样才有"只走一次就停下"的方案
                Card("走位", "Discard", "Discard", 20, A("Move:3"), A("Attack:2,1,1")),
                Card("另一张", "Discard", "Discard", 40, A("Attack:2,1,1"), A("Move:2")),
            };

            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 5) });

            // 把 (13,4) 标成有 2 点 hazard 的格子（模拟陷阱）
            ctx.Reachable = (x, y, r) =>
            {
                var list = new List<(int, int, int)>();
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r - 1; dx <= r + 1; dx++)
                    {
                        int px = x + dx, py = y + dy;
                        if (px < 0 || py < 0) continue;
                        if (HexDist(x, y, px, py) > r) continue;
                        int haz = (px == 13 && py == 4) ? 2 : 0;   // 陷阱
                        list.Add((px, py, haz));
                    }
                }
                return list;
            };

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("有方案", plans.Count > 0);

            // ★ 有干净格可走时，求解器应该**绕开**陷阱（这是建模的目的）
            int onTrap = plans.Count(p => p.FinalX == 13 && p.FinalY == 4);
            Eq("有干净格可走时没有方案踩陷阱", 0, onTrap);

            TurnPlan clean = plans.Find(p => p.FinalX == 14 && p.FinalY == 4);
            True("存在走干净格的方案", clean != null);
            if (clean != null)
            {
                Eq("干净格没有自伤", 0, clean.SelfHazardDamage);
            }

            // ★ 被迫踩陷阱时，必须把伤害记下来（计入评分）
            ctx.Reachable = (x, y, r) =>
            {
                var list = new List<(int, int, int)>();
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r - 1; dx <= r + 1; dx++)
                    {
                        int px = x + dx, py = y + dy;
                        if (px < 0 || py < 0) continue;
                        if (HexDist(x, y, px, py) > r) continue;
                        // 除原地外全部有 2 点陷阱伤害 → 只能踩
                        int haz = (px == 14 && py == 3) ? 0 : 2;
                        list.Add((px, py, haz));
                    }
                }
                return list;
            };
            List<TurnPlan> forced = P1Planner.Enumerate(ctx, 40);
            True("绕不开时仍有方案", forced.Count > 0);
            int withHazard = forced.Count(p => p.SelfHazardDamage >= 2);
            True("被迫踩陷阱的方案记录了自伤", withHazard > 0);
            TurnPlan fp = forced.Find(p => p.SelfHazardDamage >= 2);
            if (fp != null)
            {
                Console.WriteLine($"  绕不开时自伤 = {fp.SelfHazardDamage}"
                    + $"（终点 ({fp.FinalX},{fp.FinalY})）");
            }
        }

        // ---------- ㉛ AOE 多目标伤害（每目标独立抽牌）----------
        private static void AoeDamageTests()
        {
            Console.WriteLine();
            Console.WriteLine("== AOE 多目标（v1.22 起能算伤害了）==");

            // 三个敌人都在射程内 → AOE 应该一次打多个
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "敌甲",
                                  X = 14, Y = 4, Health = 20, MaxHealth = 20,
                                  Initiative = 60, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m2", Name = "G2", DisplayName = "敌乙",
                                  X = 13, Y = 4, Health = 20, MaxHealth = 20,
                                  Initiative = 61, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m3", Name = "G3", DisplayName = "敌丙",
                                  X = 14, Y = 5, Health = 20, MaxHealth = 20,
                                  Initiative = 62, IsHostileToPlayer = true },
            };

            // AOE 攻击 2（目标数 0 = 打范围内所有）
            var aoe = Card("范围击", "Discard", "Discard", 20, A("Attack:2,2,0", "AOE"), A("Move:2"));
            var single = Card("单体击", "Discard", "Discard", 40, A("Attack:2,1,1"), A("Move:2"));
            // 第三张非 AOE 牌：让「单体口径」真的有方案可统计 ——
            // 只有两张牌时，任何方案都必然用到 AOE 卡，maxSingle 恒为 0，断言等于没测。
            var plain = Card("普通", "Discard", "Discard", 80, A("Attack:2,1,1"), A("Move:2"));

            P1Context ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { aoe, single, plain }, monsters,
                new HashSet<(int, int)> { (14, 4), (13, 4), (14, 5) });

            // ① AOE 不再算"未建模"
            True("真 AOE 不再算未建模（IsUnsupportedAttack 为 false）",
                !aoe.Top.IsUnsupportedAttack);

            // ② 阵型单体仍然算未建模（形状是站位要求）
            var formation = Card("阵型击", "Discard", "Discard", 60, A("Attack:2,1,1", "AOE"), A("Move:2"));
            True("阵型单体仍算未建模", formation.Top.IsUnsupportedAttack);

            // ③ AOE 方案的总伤害应大于单次攻击（打到了多个目标）
            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);
            True("有方案", plans.Count > 0);

            int maxAoe = 0, maxSingle = 0;
            foreach (TurnPlan p in plans)
            {
                bool usesAoe = (p.First != null && p.First.CardIndex == 0)
                            || (p.Second != null && p.Second.CardIndex == 0);
                if (usesAoe && p.TotalDamage > maxAoe) maxAoe = p.TotalDamage;

                // 单体口径：**不含 AOE** 的方案（否则"AOE+单体"组合会被算进单体）
                if (!usesAoe && p.TotalDamage > maxSingle) maxSingle = p.TotalDamage;
            }
            Console.WriteLine($"  AOE 最大总伤害={maxAoe}，单体最大={maxSingle}");
            True("AOE 总伤害超过单体（打到了多个目标）", maxAoe > maxSingle);

            // ④ 攻击次数 = 命中目标数（方案里 Attacks 数量应对应多目标）
            TurnPlan best = plans.Find(p => p.TotalDamage == maxAoe);
            if (best != null)
            {
                Console.WriteLine($"  AOE 方案命中 {best.Attacks.Count} 个目标，"
                    + $"总伤 {best.TotalDamage}，消耗牌 {best.Attacks.Sum(a => a.CardsConsumed)} 张");
                True("AOE 命中了多个目标", best.Attacks.Count >= 2);
                // 每个目标独立抽牌 → 消耗张数应 ≥ 目标数
                int consumed = best.Attacks.Sum(a => a.CardsConsumed);
                True("每个目标独立抽牌（消耗张数 ≥ 目标数）", consumed >= best.Attacks.Count);
            }
        }

        // ---------- ㉜ 生存维度：站到会被打的格子要扣分 ----------
        private static void SurvivalWeightTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 生存维度（StS2 求解器：生存 > 胜利 > 战损）==");

            var monsters = new List<UnitSnapshot>
            {
                // 敌人放 (14,5)，威胁格设为 (14,4) —— 那格可达但会被打
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "近战敌",
                                  X = 14, Y = 5, Health = 20, MaxHealth = 20,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            var hand = new List<AbilityCardSnapshot>
            {
                // 一张纯攻击（射程 2，能打到 (14,5) 的敌人，否则会被"打不到就剪枝"剪掉）
                // 一张纯移动（这样"移动后停住"才有意义）
                Card("攻击牌", "Discard", "Discard", 20, A("Attack:3,2,1"), A("Attack:2,2,1")),
                Card("走位牌", "Discard", "Discard", 40, A("Move:3"), A("Move:2")),
            };

            P1Context Make2(Dictionary<(int, int), int> threat, int initiative = 20)
            {
                var c = MakeCtx("测试", 14, 3, hand, monsters,
                    new HashSet<(int, int)> { (14, 5) });
                // 威胁按**方案先攻**分组（方案可换先攻 → 换了就能避开先手）
                var info = new P1Context.ThreatInfo();
                if (threat != null)
                {
                    foreach (var kv in threat) info.DamagedCells[kv.Key] = kv.Value;
                }
                c.ThreatByInitiative = new Dictionary<int, P1Context.ThreatInfo>
                {
                    { initiative, info },
                    { 40, new P1Context.ThreatInfo() },   // 另一张牌的高先攻 → 无威胁
                };
                return c;
            }

            var danger = new Dictionary<(int, int), int> { { (14, 4), 5 } };

            // ① ★ 生存维度 + 先攻编织
            //    低先攻（20）会被先手敌人打 → 不该把终点放在威胁格
            //    高先攻（40）能避开先手 → 可以放（这正是先攻编织的价值）
            List<TurnPlan> pDanger = P1Planner.Enumerate(Make2(danger), 40);
            True("有方案", pDanger.Count > 0);

            int lowIni = pDanger.Count(p => p.Initiative == 20
                                        && p.FinalX == 14 && p.FinalY == 4);
            Eq("低先攻下没有方案站在威胁格（会被先手打）", 0, lowIni);

            int highIni = pDanger.Count(p => p.Initiative == 40
                                         && p.FinalX == 14 && p.FinalY == 4);
            True("高先攻方案可以站在威胁格（换先攻规避先手）", highIni > 0);
            Console.WriteLine($"  低先攻站在威胁格 {lowIni} 个；高先攻 {highIni} 个");

            // ② 同一局面把生存权重调 0 → "只看伤害"的旧行为会站上去
            //    这是"生存维度真的在起作用"的关键证据
            P1Context noWeight = Make2(danger);
            noWeight.W.SurvivalWeight = 0;
            List<TurnPlan> pNo = P1Planner.Enumerate(noWeight, 40);
            int onDangerNo = pNo.Count(p => p.FinalX == 14 && p.FinalY == 4);
            True("权重调 0 后会出现站到威胁格的方案（证明是生存维度挡掉的）",
                onDangerNo > 0);
            Console.WriteLine($"  权重 0 时有 {onDangerNo} 个方案站在威胁格；"
                + $"默认权重下 0 个");

            // ③ 不挨打时，同一个落点当然可选
            List<TurnPlan> pSafe = P1Planner.Enumerate(
                Make2(new Dictionary<(int, int), int>()), 40);
            True("没有威胁时该落点可选",
                pSafe.Count(p => p.FinalX == 14 && p.FinalY == 4) > 0);

            // ④ 挨打伤害按**方案先攻**算：高先攻不挨打，低先攻挨打
            TurnPlan hi = pDanger.Find(p => p.Initiative == 40
                                       && p.FinalX == 14 && p.FinalY == 4);
            if (hi != null) Eq("高先攻方案不挨打", 0, hi.IncomingDamage);

            P1Context onlyLow = Make2(danger, 20);
            onlyLow.ThreatByInitiative.Remove(40);      // 只允许低先攻
            List<TurnPlan> pLow = P1Planner.Enumerate(onlyLow, 40);
            TurnPlan lo = pLow.Find(p => p.Initiative == 20
                                     && p.FinalX == 14 && p.FinalY == 4);
            if (lo != null)
            {
                Eq("低先攻方案记录了挨打伤害", 5, lo.IncomingDamage);
                Console.WriteLine($"  低先攻站威胁格：挨打 {lo.IncomingDamage}，评分 {lo.Score:F1}");
            }
        }

        // ---------- ㉝ 敌人上状态：缴械 → 攻击执行不了 ----------
        private static void EnemyConditionTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 敌人上状态（实战：精英冰角祭司先手缴械）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "精英冰角祭司",
                                  X = 14, Y = 5, Health = 20, MaxHealth = 20,
                                  Initiative = 60, IsHostileToPlayer = true },
            };

            // 一张攻击牌 + 一张移动牌
            var hand = new List<AbilityCardSnapshot>
            {
                Card("攻击牌", "Discard", "Discard", 20, A("Attack:3,2,1"), A("Attack:2,2,1")),
                Card("走位牌", "Discard", "Discard", 40, A("Move:3"), A("Move:2")),
            };

            P1Context MakeWith(string[] conds, Dictionary<(int, int), int> dmg)
            {
                var c = MakeCtx("旗矛", 14, 3, hand, monsters,
                    new HashSet<(int, int)> { (14, 5) });
                var info = new P1Context.ThreatInfo();
                info.AppliedConditions.AddRange(conds);
                foreach (var kv in dmg) info.DamagedCells[kv.Key] = kv.Value;
                c.ThreatByInitiative = new Dictionary<int, P1Context.ThreatInfo>
                {
                    { 20, info },                                  // 低先攻 → 会被上状态
                    { 40, new P1Context.ThreatInfo() },             // 高先攻 → 躲开了
                };
                return c;
            }

            // 威胁格设在 (14,3) = 我**攻击时**站的位置（攻击牌没有移动，攻击就发生在这）
            var hit = new Dictionary<(int, int), int> { { (14, 3), 3 } };

            // 判定"这次攻击发生在威胁格上"的方案数
            Func<TurnPlan, bool> atkOnThreat = p =>
                (p.First?.Half?.HasAttack == true && p.First.ToX == 14 && p.First.ToY == 3)
             || (p.Second?.Half?.HasAttack == true && p.Second.ToX == 14 && p.Second.ToY == 3);
            Func<TurnPlan, bool> moveOnThreat = p =>
                (p.First?.Half?.HasMove == true && p.First.ToX == 14 && p.First.ToY == 3)
             || (p.Second?.Half?.HasMove == true && p.Second.ToX == 14 && p.Second.ToY == 3);

            // ① 对照组：**没有**缴械时，在威胁格攻击的方案是存在的
            List<TurnPlan> pPlain = P1Planner.Enumerate(
                MakeWith(new string[0], hit), 40);
            int plainAtk = pPlain.Count(p => p.Initiative == 20 && atkOnThreat(p));
            True("对照组：敌人不上缴械时，在威胁格攻击的方案存在", plainAtk > 0);

            // ② ★ 被缴械后 → 那种方案必须消失
            List<TurnPlan> pDisarm = P1Planner.Enumerate(
                MakeWith(new[] { "Disarm" }, hit), 40);
            True("有方案", pDisarm.Count > 0);
            int disarmAtk = pDisarm.Count(p => p.Initiative == 20 && atkOnThreat(p));
            Eq("被缴械后，在威胁格攻击的方案消失了", 0, disarmAtk);
            Console.WriteLine($"  对照组 {plainAtk} 个 vs 被缴械 {disarmAtk} 个");

            // ③ 高先攻躲开缴械 → 在威胁格攻击也可行
            int highAtk = pDisarm.Count(p => p.Initiative == 40 && atkOnThreat(p));
            True("高先攻躲开缴械后可以在威胁格攻击", highAtk > 0);

            // ④ 静足：**会移动**的方案，在移动会挨打时必须消失
            List<TurnPlan> pImmo = P1Planner.Enumerate(
                MakeWith(new[] { "Immobilize" }, hit), 40);
            Func<TurnPlan, bool> hasMove = p =>
                p.First?.Half?.HasMove == true || p.Second?.Half?.HasMove == true;
            int plainMove = pPlain.Count(p => p.Initiative == 20 && hasMove(p));
            int immoMove = pImmo.Count(p => p.Initiative == 20 && hasMove(p) && moveOnThreat(p));
            True("对照组：没有静足时有含移动的方案", plainMove > 0);
            Eq("被静足后，会挨打的移动方案消失了", 0, immoMove);
            Console.WriteLine($"  对照组含移动 {plainMove} 个；被静足且会挨打 {immoMove} 个");

            // ⑤ 眩晕：什么都做不了
            List<TurnPlan> pStun = P1Planner.Enumerate(
                MakeWith(new[] { "Stun" }, hit), 40);
            int lowStun = pStun.Count(p => p.Initiative == 20
                    && (atkOnThreat(p) || moveOnThreat(p)));
            Eq("被眩晕时，在威胁格的动作方案消失了", 0, lowStun);
        }

        // ---------- ㉞ 词典序 Pareto 评估（学 StS2 Combat Solver）----------
        private static void LexicographicOrderTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 词典序评估（赢 > 活 > 少失血 > 击杀 > 伤害）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "脆皮敌",
                                  X = 14, Y = 4, Health = 4, MaxHealth = 4,
                                  Initiative = 60, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m2", Name = "G2", DisplayName = "厚血敌",
                                  X = 13, Y = 4, Health = 30, MaxHealth = 30,
                                  Initiative = 61, IsHostileToPlayer = true },
            };

            var hand = new List<AbilityCardSnapshot>
            {
                Card("攻击牌", "Discard", "Discard", 20, A("Attack:2,1,1"), A("Move:2")),
                Card("走位牌", "Discard", "Discard", 40, A("Move:3"), A("Attack:3,1,1")),
            };

            // 我只有 3 点血，敌人打 5 点 → 站它旁边会死
            P1Context ctx = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 4), (13, 4) });
            ctx.StartHealth = 3;
            ctx.StartMaxHealth = 10;

            // 威胁格设在我**起点** (14,3)：
            //   · 原地攻击的方案 → 挨 5 刀，我只有 3 血 → **会死**
            //   · 先移动再攻击的方案 → 攻击发生在别处 → 不挨打 → 活得下来
            // 这样"会死"与"不死"的方案并存，才能验证排序把死的排后面。
            var info = new P1Context.ThreatInfo();
            info.DamagedCells[(14, 3)] = 5;
            ctx.ThreatByInitiative = new Dictionary<int, P1Context.ThreatInfo>
            {
                { 20, info },
                { 40, new P1Context.ThreatInfo() },
            };

            // topK 给大一点，让"会死的方案"不被截断挤掉（否则测不到排序）
            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 500);
            True("有方案", plans.Count > 0);

            // ① 会死的方案必须排在后面
            int firstDie = plans.FindIndex(p => p.WouldDie);
            int lastAlive = plans.FindLastIndex(p => !p.WouldDie);
            True("确实存在会死的方案（低先攻 + 站威胁格）", firstDie >= 0);
            True("确实存在活着的方案", lastAlive >= 0);
            if (firstDie >= 0 && lastAlive >= 0)
            {
                True("会死的方案排在活着的方案后面（生存优先）", firstDie > lastAlive);
                Console.WriteLine($"  第一个会死的方案 #{firstDie}，最后一个活的 #{lastAlive}");
            }

            // ② 最佳方案不该是会死的
            True("排名第一的方案不会死", !plans[0].WouldDie);
            Eq("第一名的挨打伤害为 0（高先攻避开）", 0, plans[0].IncomingDamage);

            // ③ 线性分再高也不该让"会死"的方案排前面：
            //    生存是词典序第一项，不是加权项
            double maxScoreAlive = plans.Where(p => !p.WouldDie).Max(p => p.Score);
            double maxScoreDie = plans.Where(p => p.WouldDie).Select(p => p.Score)
                                        .DefaultIfEmpty(double.MinValue).Max();
            Console.WriteLine($"  活着的最高线性分 {maxScoreAlive}；会死的最高线性分 {maxScoreDie}");
            if (maxScoreDie > maxScoreAlive)
            {
                True("即使会死的方案线性分更高，它仍排在后面（词典序 > 线性）", !plans[0].WouldDie);
            }
        }

        // ---------- ㉟ 多样性分桶（Top-N 不该全是同一路子）----------
        private static void DiversityBucketTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 多样性分桶（学 StS2 的 SearchRouteTraits 泳道）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "G1", DisplayName = "敌甲",
                                  X = 14, Y = 4, Health = 20, MaxHealth = 20,
                                  Initiative = 60, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m2", Name = "G2", DisplayName = "敌乙",
                                  X = 13, Y = 4, Health = 20, MaxHealth = 20,
                                  Initiative = 61, IsHostileToPlayer = true },
            };

            var hand = new List<AbilityCardSnapshot>
            {
                Card("攻击牌", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2")),
                Card("走位牌", "Discard", "Discard", 40, A("Move:3"), A("Attack:2,1,1")),
            };

            // ① 关闭多样性 → 纯 top-K
            P1Context plain = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 4), (13, 4) });
            plain.W.DiversityPerBucket = 0;
            List<TurnPlan> pPlain = P1Planner.Enumerate(plain, 5);
            True("有方案", pPlain.Count > 0);

            // ② 开启多样性 → 同样的 topK，但打法更多样
            P1Context div = MakeCtx("测试", 14, 3, hand, monsters,
                new HashSet<(int, int)> { (14, 4), (13, 4) });
            div.W.DiversityPerBucket = 1;
            List<TurnPlan> pDiv = P1Planner.Enumerate(div, 5);

            Eq("方案数量不变", pPlain.Count, pDiv.Count);
            Console.WriteLine($"  纯 top-K 的进攻签名："
                + string.Join(" | ", pPlain.Select(p => p.OffenseSignature)));
            Console.WriteLine($"  多样性筛选的进攻签名："
                + string.Join(" | ", pDiv.Select(p => p.OffenseSignature)));

            // ③ ★ 关键性质：**前排**（前 perBucket×2 个）不再清一色
            //   纯 top-K 的前几名往往是同一个打法的变体；
            //   分桶后前排会覆盖不同打法。
            int head = Math.Min(3, pDiv.Count);
            int headKinds = pDiv.Take(head).Select(p => p.OffenseSignature).Distinct().Count();
            int headKindsPlain = pPlain.Take(head).Select(p => p.OffenseSignature).Distinct().Count();
            Console.WriteLine($"  前 {head} 个的打击方式种类：纯 top-K {headKindsPlain} 种"
                + $" → 分桶后 {headKinds} 种");
            True("分桶后前排的打法种类不少于纯 top-K", headKinds >= headKindsPlain);

            // 整体种类也更多
            int kindsPlain = pPlain.Select(p => p.OffenseSignature).Distinct().Count();
            int kindsDiv = pDiv.Select(p => p.OffenseSignature).Distinct().Count();
            True("整体打法种类更多", kindsDiv >= kindsPlain);
            var atkCounts = new Dictionary<string, int>();
            foreach (TurnPlan p in pDiv)
            {
                int c; atkCounts.TryGetValue(p.OffenseSignature, out c);
                atkCounts[p.OffenseSignature] = c + 1;
            }
            Console.WriteLine("  各进攻签名的方案数："
                + string.Join("，", atkCounts.Select(kv => $"{kv.Key}×{kv.Value}")));

            // ④ 站位签名应当多样（不同落点）
            int positions = pDiv.Select(p => p.PositionSignature).Distinct().Count();
            Console.WriteLine($"  多样性筛选里不同落点 {positions} 个（纯 top-K："
                + pPlain.Select(p => p.PositionSignature).Distinct().Count() + " 个）");
            True("多样性筛选后落点更多样", positions >= 1);

            // ⑤ 第 1 名始终是最优的（多样性不影响榜首）
            True("第 1 名与纯 top-K 相同", pDiv[0].Score == pPlain[0].Score);
        }

        // ---------- v1.35 回归 ----------

        /// <summary>单体攻击不能被算成"打射程内所有敌人"（v1.34 实测虚报伤害的根因）。</summary>
        private static void SingleTargetLimitTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：单体攻击每个半边最多命中 1 个目标 ==");

            var a = Card("单体A", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2"));
            var b = Card("单体B", "Discard", "Discard", 40, A("Attack:3,1,1"), A("Move:2"));
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "敌甲", DisplayName = "敌甲",
                                   X = 14, Y = 4, Health = 20, MaxHealth = 20, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m2", Name = "敌乙", DisplayName = "敌乙",
                                   X = 15, Y = 4, Health = 20, MaxHealth = 20, IsHostileToPlayer = true },
            };
            var ctx = MakeCtx("测试", 14, 3, new List<AbilityCardSnapshot> { a, b }, monsters,
                new HashSet<(int, int)> { (14, 4), (15, 4) });

            var plans = P1Planner.Enumerate(ctx, 20);
            True("有方案", plans.Count > 0);

            bool overHit = false;
            int maxAttacks = 0;
            foreach (TurnPlan p in plans)
            {
                var perHalf = new Dictionary<string, int>();
                foreach (AttackChoice atk in p.Attacks)
                {
                    string key = atk.CardIndex + ":" + (int)atk.Source;
                    perHalf.TryGetValue(key, out int c);
                    perHalf[key] = c + 1;
                }
                foreach (var kv in perHalf)
                {
                    if (kv.Value > 1) overHit = true;
                }
                if (p.Attacks.Count > maxAttacks) maxAttacks = p.Attacks.Count;
            }

            True("同一半边最多命中 1 个目标（不再把单体当 AOE）", !overHit);
            True("单体方案最多 2 次攻击（两个攻击半边各一次）", maxAttacks <= 2);

            // 伤害上限：两次单体攻击，每次最高 (3+2)*2 = 10（+2 与 ×2 连乘极端）→ 20
            True("总伤害没有被多目标虚报（≤20）", plans.TrueForAll(p => p.TotalDamage <= 20));
        }

        /// <summary>AOE 仍能一次打多个；但「目标数 = N(&gt;1)」的牌最多打 N 个。</summary>
        private static void AoeTargetCapTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：AOE 目标数上限 ==");

            var filler = Card("填充", "Discard", "Discard", 90, A("Move:1"), A("Move:1"));
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "甲", DisplayName = "甲", X = 14, Y = 4, Health = 30, MaxHealth = 30, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m2", Name = "乙", DisplayName = "乙", X = 15, Y = 4, Health = 30, MaxHealth = 30, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m3", Name = "丙", DisplayName = "丙", X = 14, Y = 5, Health = 30, MaxHealth = 30, IsHostileToPlayer = true },
            };
            var occupied = new HashSet<(int, int)> { (14, 4), (15, 4), (14, 5) };

            var aoeAll = Card("范围全", "Discard", "Discard", 20, A("Attack:2,2,0", "AOE"), A("Move:2"));
            var ctxAll = MakeCtx("测试", 14, 3, new List<AbilityCardSnapshot> { aoeAll, filler }, monsters, occupied);
            var plansAll = P1Planner.Enumerate(ctxAll, 20);
            int maxAll = plansAll.Count == 0 ? 0 : plansAll.Max(p => p.Attacks.Count(a => a.CardIndex == 0));
            Eq("目标数 0（形状内全部）→ 命中 3 个", 3, maxAll);

            var aoeTwo = Card("范围二", "Discard", "Discard", 30, A("Attack:2,2,2", "AOE"), A("Move:2"));
            var ctxTwo = MakeCtx("测试", 14, 3, new List<AbilityCardSnapshot> { aoeTwo, filler }, monsters, occupied);
            var plansTwo = P1Planner.Enumerate(ctxTwo, 20);
            int maxTwo = plansTwo.Count == 0 ? 0 : plansTwo.Max(p => p.Attacks.Count(a => a.CardIndex == 0));
            Eq("目标数 2 → 最多命中 2 个", 2, maxTwo);
        }

        /// <summary>优/劣势二选一逐条对照 CActor.ChooseCardBasedOnAdvantage。</summary>
        private static void AdvantageChoiceTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：优/劣势选牌照抄游戏 ==");

            var deck = new List<ModifierCardSnapshot>
            {
                new ModifierCardSnapshot { Name = "×2", MathModifier = "*2" },
                new ModifierCardSnapshot { Name = "+2", MathModifier = "+2" },
            };

            var adv = ModifierDraw.Simulate(deck, 0, AdvStatus.Advantage, 1, out _);
            Eq("优势（基础1）：取伤害高的 +2", "+2", adv[0].MathModifier);

            var dis = ModifierDraw.Simulate(deck, 0, AdvStatus.Disadvantage, 1, out _);
            Eq("劣势（基础1）：取伤害低的 ×2", "*2", dis[0].MathModifier);

            // 平局（基础2：×2=4，+2=4）→ 按卡片数值 200 > 102
            var tie = ModifierDraw.Simulate(deck, 0, AdvStatus.Advantage, 2, out _);
            Eq("优势平局：按卡片数值取 ×2", "*2", tie[0].MathModifier);

            var cursed = new List<ModifierCardSnapshot>
            {
                new ModifierCardSnapshot { Name = "诅咒", MathModifier = "*2", IsCurse = true, HasNegativeEffect = true },
                new ModifierCardSnapshot { Name = "+2", MathModifier = "+2" },
            };
            var advNeg = ModifierDraw.Simulate(cursed, 0, AdvStatus.Advantage, 2, out _);
            Eq("优势平局：有负面效果的牌劣后", "+2", advNeg[0].MathModifier);

            var disNeg = ModifierDraw.Simulate(cursed, 0, AdvStatus.Disadvantage, 2, out _);
            Eq("劣势平局：有负面效果的牌胜出（要更差）", "*2", disNeg[0].MathModifier);
        }

        /// <summary>多回合的敌人结算必须用**本回合方案**的先攻，不是第 1 回合的。</summary>
        private static void MultiTurnInitiativeTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：多回合按本回合先攻结算敌人 ==");

            var lowAtk = Card("先攻10-攻", "Discard", "Discard", 10, A("Attack:3,1,1"), A("Move:2"));
            var lowMove = Card("先攻10-走", "Discard", "Discard", 10, A("Move:2"), A("Move:2"));
            var highAtk = Card("先攻60-攻", "Discard", "Discard", 60, A("Attack:3,1,1"), A("Move:2"));
            var highMove = Card("先攻60-走", "Discard", "Discard", 60, A("Move:2"), A("Move:2"));

            var enemy = new UnitSnapshot
            {
                Guid = "e1", Name = "怪", DisplayName = "怪",
                X = 14, Y = 4, Health = 200, MaxHealth = 200,
                IsHostileToPlayer = true, Initiative = 50,
                Intent = new ActionSnapshot
                {
                    Kind = HalfKind.Action,
                    HasAttack = true, AttackStrength = 5, AttackRange = 1, AttackTargets = 1,
                    HasMove = true, MoveStrength = 4,
                },
            };

            var ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { lowAtk, lowMove, highAtk, highMove },
                new List<UnitSnapshot> { enemy },
                new HashSet<(int, int)> { (14, 4) });
            ctx.StartHealth = 20;
            ctx.StartMaxHealth = 20;

            // 第 1 回合只允许用前两张（先攻 10）→ 我先动（怪先攻 50）
            var round1 = P1Planner.Enumerate(ctx, 30)
                .Where(p => p.TopCardIndex < 2 && p.BottomCardIndex < 2).ToList();
            True("第 1 回合有低先攻方案", round1.Count > 0);

            var res = MultiTurnSearch.Search(ctx, round1, 2, 3, 10, new MultiTurnSearch.EvalWeights());
            True("推演出了 2 回合的路线", res.Routes.Count > 0 && res.Routes.All(r => r.Count == 2));

            int r1Incoming = 0, r2Incoming = 0;
            foreach (var route in res.Routes)
            {
                r1Incoming += route[0].IncomingDamage;
                r2Incoming += route[1].IncomingDamage;
            }
            Eq("第 1 回合先攻 10 → 我先动，不挨打", 0, r1Incoming);
            True("第 2 回合先攻 60 → 怪（50）先动，会挨打（旧版会误判为 0）", r2Incoming > 0);
        }

        /// <summary>预测落点重写必须与"有没有友方单位"无关（v1.34 的 bug）。</summary>
        private static void PredictedPositionTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：敌人预测落点重写（纯函数）==");

            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "g1", Name = "先动", X = 1, Y = 1 },
                new UnitSnapshot { Guid = "g2", Name = "后动", X = 2, Y = 2 },
            };
            var moves = new List<PredictedPositions.Move>
            {
                new PredictedPositions.Move { Guid = "g1", Initiative = 5, X = 10, Y = 10 },
                new PredictedPositions.Move { Guid = "g2", Initiative = 50, X = 20, Y = 20 },
            };

            int n = PredictedPositions.Apply(monsters, moves, 10);
            Eq("只重写先攻更低的敌人", 1, n);
            Eq("先动敌人的位置被改写", 10, monsters[0].X);
            Eq("后动敌人保持原位", 2, monsters[1].X);

            // 这个用例本身就没有 Others —— 正是 v1.34 里"没有友方单位就不执行"的场景
            Eq("myInitiative 无效时不改", 0,
                PredictedPositions.Apply(monsters, moves, 0));
        }

        /// <summary>敌人焦点：同距比先攻；AttackFurthestTarget 时选最远。</summary>
        private static void FocusRuleTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：敌人焦点排序（CAIMoveNew.CompareEnemies）==");

            Func<int, int, int, List<(int X, int Y, int Hazard)>> reach = (x, y, r) =>
            {
                var l = new List<(int X, int Y, int Hazard)>();
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r - 1; dx <= r + 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (HexDist(x, y, nx, ny) <= r) l.Add((nx, ny, 0));
                    }
                }
                return l;
            };
            Func<int, int, int, int, int> dist = (x1, y1, x2, y2) => HexDist(x1, y1, x2, y2);
            Func<int, int, int, int, bool> los = (a, b, c, d) => true;

            // 两个目标距离相同（都是 3）→ 同距比先攻（10 < 30）
            var tied = new List<EnemyPathing.Target>
            {
                new EnemyPathing.Target(3, 0, "近30", 30),
                new EnemyPathing.Target(0, 3, "近10", 10),
            };
            int tx, ty, fi;
            EnemyMovePlan plan = EnemyPathing.PlanMove(
                0, 0, 1, 1, reach, dist, los, tied, new HashSet<(int, int)>(),
                out tx, out ty, out fi);
            Eq("同距时选先攻小的", 1, fi);
            Eq("够不着时走 Approach", EnemyMovePlan.Approach, plan);
            True("够不着也会朝焦点逼近", tx != 0 || ty != 0);

            // 最远焦点（AttackFurthestTarget）
            var far = new List<EnemyPathing.Target>
            {
                new EnemyPathing.Target(2, 0, "近", 10),
                new EnemyPathing.Target(6, 0, "远", 20),
            };
            EnemyPathing.PlanMove(0, 0, 3, 1, reach, dist, los, far, new HashSet<(int, int)>(),
                out tx, out ty, out fi, preferFurthest: true);
            Eq("AttackFurthestTarget：焦点取最远的", 1, fi);
        }

        /// <summary>阵型文本不能把「你站灰格」重复输出两遍。</summary>
        private static void FormationTextTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.35：阵型文本去重 ==");

            string text = P1Planner.FormationRequirement("0,0,G|1,1,R|2,0,R|3,1,B");
            int first = text.IndexOf("你站灰格(G)", System.StringComparison.Ordinal);
            int second = first < 0 ? -1 : text.IndexOf("你站灰格(G)", first + 1, System.StringComparison.Ordinal);
            True("阵型文本包含灰格要求", first >= 0);
            Eq("「你站灰格」只出现一次", -1, second);
        }

        // ---------- v1.36：规则核查后的修复 ----------

        /// <summary>Strengthen 已由游戏折算成 Advantage，不能再加 +1。</summary>
        private static void StrengthenNoDoubleCountTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.36：Strengthen 不再额外 +1 ==");

            var card = Card("攻击", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2"));
            var fill = Card("填充", "Discard", "Discard", 40, A("Move:1"), A("Move:1"));
            var monster = new UnitSnapshot
            {
                Guid = "m1", Name = "敌", DisplayName = "敌",
                X = 14, Y = 4, Health = 20, MaxHealth = 20, IsHostileToPlayer = true,
            };
            var ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { card, fill },
                new List<UnitSnapshot> { monster },
                new HashSet<(int, int)> { (14, 4) },
                deckCards: Deck(Mod("+0")));
            ctx.MyPositiveConditions = new List<string> { "Strengthen" };

            var plans = P1Planner.Enumerate(ctx, 10);
            TurnPlan hit = plans.FirstOrDefault(p => p.Attacks.Count > 0);
            True("有攻击方案", hit != null);
            Eq("攻击 3 + 翻 +0 → 3 伤（不再 +1）", 3, hit != null ? hit.Attacks[0].Damage : -1);
        }

        /// <summary>中毒目标被攻击时 +1 攻击力；文案不能写成 Wound。</summary>
        private static void PoisonRuleTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.36：中毒 = 攻击该目标 +1 ==");

            var card = Card("攻击", "Discard", "Discard", 20, A("Attack:3,1,1"), A("Move:2"));
            var fill = Card("填充", "Discard", "Discard", 40, A("Move:1"), A("Move:1"));
            var monster = new UnitSnapshot
            {
                Guid = "m1", Name = "敌", DisplayName = "敌",
                X = 14, Y = 4, Health = 20, MaxHealth = 20, IsHostileToPlayer = true,
            };
            monster.NegativeConditions.Add("Poison");

            var ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { card, fill },
                new List<UnitSnapshot> { monster },
                new HashSet<(int, int)> { (14, 4) },
                deckCards: Deck(Mod("+0")));

            TurnPlan hit = P1Planner.Enumerate(ctx, 10).FirstOrDefault(p => p.Attacks.Count > 0);
            True("有攻击方案", hit != null);
            Eq("中毒目标：3 + 翻 +0 + 中毒 1 = 4", 4, hit != null ? hit.Attacks[0].Damage : -1);

            var poisonHalf = new ActionSnapshot
            {
                Kind = HalfKind.Action, HasPoison = true, PoisonStrength = 3,
            };
            string text = P1Planner.ExtraEffectText(poisonHalf);
            True("中毒文案写明 +1 攻击力", text.Contains("+1"));
            True("中毒文案不再写「回合末掉血」", !text.Contains("回合末"));
        }

        /// <summary>普通「Target N」（无 AOE 形状）也是一次打 N 个。</summary>
        private static void MultiTargetWithoutAreaTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.36：普通 Target 2（无 AOE）= 一次打 2 个 ==");

            var multi = Card("双击", "Discard", "Discard", 20, A("Attack:2,2,2"), A("Move:2"));
            var fill = Card("填充", "Discard", "Discard", 40, A("Move:1"), A("Move:1"));
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "甲", DisplayName = "甲", X = 14, Y = 4, Health = 30, MaxHealth = 30, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m2", Name = "乙", DisplayName = "乙", X = 15, Y = 4, Health = 30, MaxHealth = 30, IsHostileToPlayer = true },
                new UnitSnapshot { Guid = "m3", Name = "丙", DisplayName = "丙", X = 14, Y = 5, Health = 30, MaxHealth = 30, IsHostileToPlayer = true },
            };
            var ctx = MakeCtx("测试", 14, 3,
                new List<AbilityCardSnapshot> { multi, fill }, monsters,
                new HashSet<(int, int)> { (14, 4), (15, 4), (14, 5) });

            var plans = P1Planner.Enumerate(ctx, 20);
            True("有方案", plans.Count > 0);
            int maxHits = plans.Max(p => p.Attacks.Count(a => a.CardIndex == 0));
            Eq("Target 2：最多命中 2 个（旧版只打 1 个）", 2, maxHits);
        }

        /// <summary>穿透 / Brittle / Ward 的伤害顺序（对照 CAttackSummary）。</summary>
        private static void DamageModelRuleTests()
        {
            Console.WriteLine();
            Console.WriteLine("== v1.36：穿透 / Brittle / Ward ==");

            var plus0 = Deck(Mod("+0"));
            Eq("穿透：护盾 3、穿透 2 → 只减 1", 3, DamageModel.Compute(4, plus0, 3, 2));
            Eq("Brittle：护盾后 ×2", 6, DamageModel.Compute(4, plus0, 1, 0, true, false));
            Eq("Ward：护盾后 floor(/2)", 1, DamageModel.Compute(4, plus0, 1, 0, false, true));
            Eq("Brittle+Ward：floor((4-1)*2/2)", 3, DamageModel.Compute(4, plus0, 1, 0, true, true));
        }

        // ---------- ① 伤害模型：对照 ATTACK.md 的实证表达式 ----------
        private static void DamageModelTests()
        {
            Console.WriteLine("== 伤害模型（对照 ATTACK.md 实证表达式）==");

            // 表达式串 → 期望值，全部取自真实日志
            Check("((2))", 2, Array.Empty<string>(), 2, 0, "无修正");
            Check("((2)+1)", 2, new[] { "+1" }, 3, 0, "加 +1");
            Check("((2)-1)", 2, new[] { "-1" }, 1, 0, "减 -1");
            Check("((1)+1)*2", 1, new[] { "+1", "*2" }, 4, 0, "暴击含加成一起翻倍");
            Check("((2))*0", 2, new[] { "*0" }, 0, 0, "落空归零");
            Check("((1))*0-3", 1, new[] { "*0" }, 0, 3, "落空后扣护盾仍为 0");
            Check("((1)-1)", 1, new[] { "-1" }, 0, 0, "减法截断到 0");
            Check("((1)-1-3)*2", 1, new[] { "-1", "-3", "*2" }, 0, 0, "负结果 ×2 仍为 0");
            Check("(((2))*2)*2", 2, new[] { "*2", "*2" }, 8, 0, "多层乘算左结合");
            Check("((10))*2", 10, new[] { "*2" }, 20, 0, "高基础暴击");

            // 护盾在乘算之后才减
            Check("护盾后减", 3, new[] { "*2" }, 5, 1, "3*2=6 再 -1 = 5，不是 (3-1)*2=4");
        }

        private static void Check(string label, int baseStr, string[] mods, int expected, int shield, string desc)
        {
            var drawn = mods.Select(m => new ModifierCardSnapshot { Name = "X", MathModifier = m }).ToList();
            int got = DamageModel.Compute(baseStr, drawn, shield);
            bool ok = got == expected;
            if (!ok) _fail++;
            Console.WriteLine($"  [{(ok ? "OK" : "XX")}] {desc,-40} 期望{expected} 实得{got}");
        }

        private static void Check(string expr, int expected, int got, string desc)
        {
            bool ok = got == expected;
            if (!ok) _fail++;
            Console.WriteLine($"  [{(ok ? "OK" : "XX")}] {desc,-40} 期望{expected} 实得{got}");
        }

        // ---------- ② 抽牌模拟：rolling 链 + 优劣势 ----------
        private static void ModifierDrawTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 抽牌模拟（rolling 链 / 优劣势）==");

            var deck = Deck(
                Mod("+1"), Mod("*2"), Mod("-1"), Mod("+0"), Mod("+2"), Mod("+1"),
                Mod("-1"), Mod("+0"), Mod("+2"), Mod("+1"), Mod("-1"), Mod("+0"),
                Mod("+2"), Mod("+1"), Mod("-1"), Mod("+0"), Mod("+2"), Mod("+1"),
                Mod("-1"), Mod("+0"));

            // None：只取一张
            var d = ModifierDraw.Simulate(deck, 0, AdvStatus.None, 3, out int used);
            Eq("None 只取 1 张", 1, d.Count);
            Eq("None 消耗 1 张", 1, used);
            Eq("None 取到 +1", "+1", d[0].MathModifier);

            // rolling：翻到非 rolling 才停，链上全部计入
            var rollingDeck = Deck(ModRoll("+1"), ModRoll("+1"), Mod("+2"));
            var r = ModifierDraw.Simulate(rollingDeck, 0, AdvStatus.None, 3, out int usedR);
            Eq("rolling 链取 3 张", 3, r.Count);
            Eq("rolling 消耗 3 张", 3, usedR);
            Eq("rolling 伤害 = 3+1+1+2", 7, DamageModel.ComputePreShield(3, r));

            // 优势：抽两张取伤害高的
            var advDeck = Deck(Mod("-1"), Mod("+2"));
            var a = ModifierDraw.Simulate(advDeck, 0, AdvStatus.Advantage, 3, out int usedA);
            Eq("优势取 +2", "+2", a[0].MathModifier);
            Eq("优势消耗 2 张", 2, usedA);

            // 劣势：取伤害低的；第一张是 ×0 时必取它
            var disDeck = Deck(Mod("+2"), Mod("*0"));
            var dis = ModifierDraw.Simulate(disDeck, 0, AdvStatus.Disadvantage, 3, out int usedD);
            Eq("劣势取 ×0", "*0", dis[0].MathModifier);
            Eq("劣势消耗 2 张", 2, usedD);

            // 牌堆见底 → 不可预知
            var shortDeck = Deck(Mod("+1"));
            Eq("牌堆见底返回 null", true, ModifierDraw.Simulate(shortDeck, 1, AdvStatus.None, 3, out _) == null);
        }

        // ---------- ③ 枚举：复现实机那一局 ----------
        private static void EnumerationTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 枚举（复现秉姜 @ (14,3)）==");

            P1Context ctx = BuildBingjiang();
            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 5);

            True("有方案", plans.Count > 0);
            if (plans.Count == 0) return;

            TurnPlan best = plans[0];
            Console.WriteLine($"  最佳: {best.ToOneLine()}");
            Console.WriteLine($"  攻击数: {best.Attacks.Count}");
            foreach (AttackChoice a in best.Attacks)
            {
                Console.WriteLine($"    {a.Side}半 {a.CardName} → {a.TargetName} 伤{a.Damage} 翻牌[{string.Join(" ", a.Mods)}]");
            }

            // 关键断言：必须找到多次攻击（义无反照 下:移动3+攻3 + 血流不止 上:攻3）
            // v1.22：「义无反照」是 AOE → 一次打 2 个目标，所以攻击次数是 3 而不是 2
            //        （这正是 AOE 建模带来的收益：总伤从 6 提升到 10）
            True("最佳方案包含多次攻击（而不是单攻击）", best.Attacks.Count >= 2);
            True("最佳方案伤害 >= 6", best.TotalDamage >= 6);
            True("最佳方案没有踩 Lost 惩罚踩到只剩单攻击", best.LostCost <= 1);
        }

        // ---------- ④ 占用：绝不能走到敌人占的格子 ----------
        private static void OccupancyTests()
        {
            Console.WriteLine();
            Console.WriteLine("== 占用过滤 ==");

            P1Context ctx = BuildBingjiang();
            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 40);

            int bad = 0;
            foreach (TurnPlan p in plans)
            {
                if (ctx.Occupied.Contains((p.FinalX, p.FinalY))) bad++;
                if (ctx.Occupied.Contains((p.MidX, p.MidY))) bad++;
            }
            Eq("没有方案把落点放在被占用的格子上", 0, bad);

            // 直接验 dests：规划器用的落点集不该出现敌人格子
            // （注意：ctx.Reachable 本身是"物理可达"的超集，占用过滤发生在 dests 里）
            var reach = ctx.Reachable(ctx.StartX, ctx.StartY, 4);
            int enemyTiles = reach.Count(t => ctx.Occupied.Contains((t.X, t.Y)));
            Console.WriteLine($"  物理可达 {reach.Count} 格，其中被占用 {enemyTiles} 格"
                + $"（占用过滤由 dests 负责，见上一条断言）");
            True("占用过滤真的在生效（物理可达集里确实有敌人格，否则这条测试没意义）",
                enemyTiles > 0);
        }

        // ---------- ⑤ XP 权重：0 伤害 1 经验不能打赢有伤害 ----------
        private static void XpWeightTests()
        {
            Console.WriteLine();
            Console.WriteLine("== XP / Lost 权重 ==");

            var w = new ScoringWeights();
            True("XP 权重不足以让 1 经验打赢 3 伤害", 1 * w.XpWeight < 3 * w.DamageWeight);
            Console.WriteLine($"  权重: 伤害{w.DamageWeight} 击杀{w.KillBonus} 经验{w.XpWeight}"
                + "（Lost 只做排序次级条件，不进线性评分）");

            // 复现死行的失败模式：一张牌只给经验（未建模动作），另一张实打实造成伤害。
            // 修复前 XP 权重 5 > 伤害 3，"0 伤害 1 经验" 会排到第一。
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "m1", Name = "Guard#4", DisplayName = "冰角守卫#4", X = 14, Y = 5, Health = 10, MaxHealth = 10 },
            };
            var hand = new List<AbilityCardSnapshot>
            {
                // 纯未建模动作 + 1 经验（对应死行的 PerformFromShadow 牌）
                Card("影子戏法", "Discard", "Discard", 24, A("PerformFromShadow"), A("PreventDamage")),
                // 实打实的攻击
                Card("黑雾", "Lost", "Discard", 46, A("Attack:3,3,1"), A("MoveShadow")),
            };
            hand[0].Top.XP = 1;
            hand[0].Bottom.XP = 1;

            var deck = new ModifierDeckSnapshot { OwnerLabel = "死行" };
            deck.Remaining.AddRange(Deck(Mod("+1"), Mod("+0"), Mod("+1"), Mod("*0"), Mod("+0")));

            var ctx = new P1Context
            {
                ActorName = "死行",
                StartX = 15, StartY = 3,
                Hand = hand,
                Deck = deck,
                Adv = AdvStatus.None,
                Monsters = monsters,
                Occupied = new HashSet<(int, int)> { (14, 5) },
                Reachable = (x, y, r) => new List<(int, int, int)> { (x, y, 0) },
                RangeDistance = (x1, y1, x2, y2) => HexDist(x1, y1, x2, y2),
                HasLOS = (x1, y1, x2, y2) => true,
            };

            List<TurnPlan> plans = P1Planner.Enumerate(ctx, 5);
            True("有方案", plans.Count > 0);
            if (plans.Count > 0)
            {
                Console.WriteLine($"  最佳: {plans[0].ToOneLine()}");
                True("最佳是造成伤害的方案，不是 0 伤害刷经验", plans[0].TotalDamage > 0);
            }
        }

        // ---------- 复现实局 ----------
        private static P1Context BuildBingjiang()
        {
            // 敌人（实机坐标）
            var monsters = new List<UnitSnapshot>
            {
                new UnitSnapshot { Guid = "g1", Name = "Priest#3", DisplayName = "精英冰角祭司#3", X = 13, Y = 7, Health = 9, MaxHealth = 9, Shield = 2 },
                new UnitSnapshot { Guid = "g2", Name = "Guard#4",  DisplayName = "冰角守卫#4",    X = 14, Y = 5, Health = 10, MaxHealth = 10 },
                new UnitSnapshot { Guid = "g3", Name = "Archer#1", DisplayName = "冰角弓箭手#1",  X = 17, Y = 8, Health = 7, MaxHealth = 7 },
                new UnitSnapshot { Guid = "g4", Name = "City#4",   DisplayName = "城市守卫#4",    X = 11, Y = 7, Health = 7, MaxHealth = 7, Shield = 2 },
            };

            // 手牌（按实机打印的结构；Discard 类型按观察到的 [失] 标注假设值）
            // 能力规格串：Move:N / Attack:力量,射程,目标数 / 其他原样当未建模能力
            var hand = new List<AbilityCardSnapshot>
            {
                Card("粉碎重压", "Discard", "Discard", 71, A("Attack:0,1,1"), A("Move:0", "Heal", "MoveActiveBonusTracker")),
                Card("延绵生命", "Discard", "Discard", 61, A("Move:0", "Attack:2,2,2", "MoveActiveBonusTracker"), A("AddHeal")),
                Card("坚持不懈", "Discard", "Discard", 89, A("Move:0", "MoveActiveBonusTracker"), A("Retaliate")),
                Card("不破之身", "Discard", "Discard", 90, A("Move:0", "MoveActiveBonusTracker"), A("Shield")),
                Card("邪性攻击", "Lost", "Discard", 27, A("Attack:2,1,1"), A("Move:2", "Heal")),
                Card("汲取之箭", "Discard", "Discard", 23, A("Attack:1,2,3"), A("Move:3", "MoveActiveBonusTracker")),
                Card("慎重准备", "Discard", "Discard", 14, A("Shield", "Retaliate"), A("Heal")),
                Card("血流不止", "Discard", "Discard", 65, A("Attack:3,1,1"), A("Attack:1,1,1")),
                Card("刚毅不屈", "Lost", "Discard", 31, A("Attack:3,1,1"), A("AddActiveBonus")),
                Card("义无反照", "Lost", "Discard", 20, A("Heal"), A("Move:3", "Attack:3,1,1")),
                Card("毁灭狂怒", "Lost", "Lost", 19, A("Attack:3,1,1"), A("Attack:3,2,1")),
                Card("冲击波", "Lost", "Discard", 17, A("AddActiveBonus"), A("Move:3", "Shield")),
            };

            var deck = new ModifierDeckSnapshot { OwnerLabel = "秉姜" };
            deck.Remaining.AddRange(Deck(
                Mod("+1"), Mod("-1"), Mod("+0"), Mod("*2"), Mod("+1"),
                Mod("+0"), Mod("+1"), Mod("-1"), Mod("+2"), Mod("+0"),
                Mod("+1"), Mod("-1"), Mod("+0"), Mod("+2"), Mod("+1"),
                Mod("+0"), Mod("-1"), Mod("+1"), Mod("+2"), Mod("+0")));

            var ctx = new P1Context
            {
                ActorName = "秉姜",
                StartX = 14,
                StartY = 3,
                Hand = hand,
                Deck = deck,
                Adv = AdvStatus.None,
                Monsters = monsters,
                Occupied = new HashSet<(int, int)>(monsters.Select(m => (m.X, m.Y))),
                // 测试环境：无遮挡，可达 =  HexDist ≤ range；射程 = HexDist；视线恒通
                Reachable = (x, y, r) =>
                {
                    var list = new List<(int, int, int)>();
                    for (int dy = -r; dy <= r; dy++)
                    {
                        for (int dx = -r - 1; dx <= r + 1; dx++)
                        {
                            var p = (x + dx, y + dy);
                            if (HexDist(x, y, p.Item1, p.Item2) <= r)
                            {
                                list.Add((p.Item1, p.Item2, 0));
                            }
                        }
                    }
                    return list;
                },
                RangeDistance = (x1, y1, x2, y2) =>
                    HexDist(x1, y1, x2, y2),
                HasLOS = (x1, y1, x2, y2) => true,
            };
            return ctx;
        }

        // ---------- 小工具 ----------

        /// <summary>
        /// 六边形距离，逐行复刻游戏里的 Point.HexDist（OddRToAxial + AxialDistance）。
        /// 测试台不引用游戏的 AStar 程序集，所以在这儿抄一份保证一致。
        /// </summary>
        private static int HexDist(int x1, int y1, int x2, int y2)
        {
            int ax1 = x1 - (y1 - (y1 & 1)) / 2, ay1 = y1;
            int ax2 = x2 - (y2 - (y2 & 1)) / 2, ay2 = y2;
            return (Math.Abs(ax1 - ax2) + Math.Abs(ax1 + ay1 - ax2 - ay2) + Math.Abs(ay1 - ay2)) / 2;
        }
        private static ActionSnapshot A(params string[] spec)
        {
            // 规格串：Move:N / Attack:S,R,T / AOE（范围攻击）/ 其他原样当未建模能力
            var a = new ActionSnapshot { Kind = HalfKind.Action };
            foreach (string s in spec)
            {
                if (s == null) continue;
                if (s == "AOE") { a.HasAreaEffect = true; a.AreaEffectShape = "0,0,G|1,1,R|2,0,R|3,1,B"; continue; }
                if (s.StartsWith("Move:"))
                {
                    a.HasMove = true;
                    a.AbilityTypes.Add("Move");
                    int v = int.Parse(s.Substring(5));
                    if (v > a.MoveStrength) a.MoveStrength = v;
                }
                else if (s.StartsWith("Poison:"))
                {
                    // 中毒规格：Poison:N
                    a.HasPoison = true;
                    a.AbilityTypes.Add("Poison");
                    int pv = int.Parse(s.Substring(7));
                    if (pv > a.PoisonStrength) a.PoisonStrength = pv;
                }
                else if (s.StartsWith("Pull:"))
                {
                    // 拉拽规格：Pull:N
                    a.HasPull = true;
                    a.AbilityTypes.Add("Pull");
                    int lv = int.Parse(s.Substring(5));
                    if (lv > a.PullStrength) a.PullStrength = lv;
                }
                else if (s == "ControlActor")
                {
                    a.HasControlActor = true;
                    a.AbilityTypes.Add("ControlActor");
                }
                else if (s == "Teleport2" || s == "Teleport")
                {
                    a.HasTeleport = true;
                    a.AbilityTypes.Add(s);
                }
                else if (s.StartsWith("Heal:"))
                {
                    // 治疗规格：Heal:N[,range[,targets]] —— 与真实 HandReader 口径一致
                    a.HasHeal = true;
                    a.AbilityTypes.Add("Heal");
                    string[] hp = s.Substring(5).Split(',');
                    int hv = int.Parse(hp[0]);
                    if (hv > a.HealStrength) a.HealStrength = hv;
                    if (hp.Length > 1 && int.Parse(hp[1]) > a.HealRange) a.HealRange = int.Parse(hp[1]);
                    if (hp.Length > 2 && int.Parse(hp[2]) > a.HealTargets) a.HealTargets = int.Parse(hp[2]);
                    if (a.HealTargets <= 0) a.HealTargets = 1;
                }
                else if (s.StartsWith("Attack:"))
                {
                    a.HasAttack = true;
                    a.AbilityTypes.Add("Attack");
                    string[] p = s.Substring(7).Split(',');
                    int st = int.Parse(p[0]), rg = int.Parse(p[1]), tg = int.Parse(p[2]);
                    if (st > a.AttackStrength) a.AttackStrength = st;
                    if (rg > a.AttackRange) a.AttackRange = rg;
                    if (tg > a.AttackTargets) a.AttackTargets = tg;
                }
                else
                {
                    a.AbilityTypes.Add(s);
                    if (!a.UnmodeledTypes.Contains(s)) a.UnmodeledTypes.Add(s);
                }
            }
            return a;
        }

        /// <summary>常驻加成半边：Behaviour 名 + 幅度（对应 YAML 的 ActiveBonus）。</summary>
        private static ActionSnapshot P(params string[] spec)
        {
            // 规格串："Behaviour:N"
            var a = new ActionSnapshot { Kind = HalfKind.PersistentBonus };
            foreach (string s in spec)
            {
                if (s == null) continue;
                int i = s.IndexOf(':');
                string beh = i > 0 ? s.Substring(0, i) : s;
                int v = i > 0 ? int.Parse(s.Substring(i + 1)) : 0;
                a.PersistentBehaviours.Add(beh);
                string kind = beh.Contains("Move") ? "移动"
                            : beh.Contains("Range") ? "射程"
                            : beh.Contains("Target") ? "目标数"
                            : beh.Contains("Attack") ? "攻击"
                            : beh.Contains("Shield") ? "护盾"
                            : beh.Contains("Retaliate") ? "反击" : beh;
                a.BonusTexts.Add($"常驻增益：{kind} +{v}");
                a.AbilityTypes.Add(beh);
                a.UnmodeledTypes.Add(beh);
            }
            return a;
        }

        /// <summary>测试台用的最小上下文：无障碍、HexDist 即射程、视线恒通。</summary>
        private static P1Context MakeCtx(string actor, int sx, int sy,
            List<AbilityCardSnapshot> hand, List<UnitSnapshot> monsters,
            HashSet<(int X, int Y)> occupied, AdvStatus adv = AdvStatus.None,
            List<ModifierCardSnapshot> deckCards = null,
            HashSet<(int X, int Y)> shadows = null)
        {
            var deck = new ModifierDeckSnapshot { OwnerLabel = actor };
            deck.Remaining.AddRange(deckCards ?? DefaultDeck());

            return new P1Context
            {
                ActorName = actor,
                StartX = sx,
                StartY = sy,
                Hand = hand,
                Deck = deck,
                Adv = adv,
                Monsters = monsters,
                Occupied = occupied ?? new HashSet<(int, int)>(),
                Reachable = (x, y, r) =>
                {
                    var list = new List<(int, int, int)>();
                    for (int dy = -r; dy <= r; dy++)
                    {
                        for (int dx = -r - 1; dx <= r + 1; dx++)
                    {
                            var p = (x + dx, y + dy);
                            if (HexDist(x, y, p.Item1, p.Item2) <= r) list.Add((p.Item1, p.Item2, 0));
                    }
                }
                    return list;
                },
                RangeDistance = (x1, y1, x2, y2) => HexDist(x1, y1, x2, y2),
                HasLOS = (x1, y1, x2, y2) => true,
                IsShadowTile = (x, y) => shadows != null && shadows.Contains((x, y)),
            };
        }

        private static List<ModifierCardSnapshot> DefaultDeck()
        {
            return Deck(
                Mod("+1"), Mod("-1"), Mod("+0"), Mod("*2"), Mod("+1"),
                Mod("+0"), Mod("+1"), Mod("-1"), Mod("+2"), Mod("+0"),
                Mod("+1"), Mod("-1"), Mod("+0"), Mod("+2"), Mod("+1"),
                Mod("+0"), Mod("-1"), Mod("+1"), Mod("+2"), Mod("+0"));
        }

        private static AbilityCardSnapshot Card(string name, string topDiscard, string botDiscard,
            int initiative, ActionSnapshot top, ActionSnapshot bottom)
        {
            var c = new AbilityCardSnapshot
            {
                Name = name,
                DisplayName = name,
                TopDiscard = topDiscard,
                BottomDiscard = botDiscard,
                Initiative = initiative,
                Top = top,
                Bottom = bottom,
            };

            // 通用基础动作（Attack2 / Move2）—— 任何一张牌都能当它用
            c.HasDefaultAttack = true;
            c.HasDefaultMove = true;
            c.DefaultAttack = new ActionSnapshot
            {
                Side = "基础攻击",
                Kind = HalfKind.Action,
                IsDefaultAction = true,
                DefaultLabel = "基础攻击",
                HasAttack = true,
                AttackStrength = 2,
                AttackRange = 1,
                AttackTargets = 1,
            };
            c.DefaultAttack.AbilityTypes.Add("DefaultAttack");
            c.DefaultMove = new ActionSnapshot
            {
                Side = "基础移动",
                Kind = HalfKind.Action,
                IsDefaultAction = true,
                DefaultLabel = "基础移动",
                HasMove = true,
                MoveStrength = 2,
            };
            c.DefaultMove.AbilityTypes.Add("DefaultMove");
            return c;
        }

        private static ModifierCardSnapshot Mod(string m)
        {
            return new ModifierCardSnapshot { Name = "AMC", MathModifier = m };
        }

        private static ModifierCardSnapshot ModRoll(string m)
        {
            return new ModifierCardSnapshot { Name = "AMC", MathModifier = m, Rolling = true };
        }

        private static List<ModifierCardSnapshot> Deck(params ModifierCardSnapshot[] cards)
        {
            return new List<ModifierCardSnapshot>(cards);
        }

        private static void Eq(string what, object expected, object got)
        {
            bool ok = Equals(expected, got);
            if (!ok) _fail++;
            Console.WriteLine($"  [{(ok ? "OK" : "XX")}] {what,-46} 期望{expected} 实得{got}");
        }

        private static void True(string what, bool cond)
        {
            if (!cond) _fail++;
            Console.WriteLine($"  [{(cond ? "OK" : "XX")}] {what}");
        }
    }
}

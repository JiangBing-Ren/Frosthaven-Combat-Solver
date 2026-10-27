using System;
using System.Collections.Generic;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>一条多回合路线：每回合一个方案。</summary>
    public sealed class RouteStep
    {
        public int Round;
        public TurnPlan Plan;

        /// <summary>这一步之后我方状态摘要。</summary>
        public int MyX, MyY, MyHealth;
        public int CumulativeDamage, CumulativeKills;
        public bool Died;

        /// <summary>
        /// 这一回合里敌人对我造成的打击（**已代入真实修正牌**）。
        /// 空 = 这回合没人打到我。
        /// </summary>
        public List<EnemyStrike> EnemyStrikes = new List<EnemyStrike>();

        /// <summary>这一回合我被敌人打掉多少血。</summary>
        public int IncomingDamage
        {
            get
            {
                int t = 0;
                if (EnemyStrikes != null)
                {
                    foreach (EnemyStrike e in EnemyStrikes)
                    {
                        // 逼近不算挨打（它没出手）
                        if (e != null && !e.ApproachOnly) t += e.Damage;
                    }
                }
                return t;
            }
        }
    }

    /// <summary>多回合搜索的结果。</summary>
    public sealed class MultiTurnResult
    {
        public List<List<RouteStep>> Routes = new List<List<RouteStep>>();
        public double BestValue;
        public int Depth;
        public int Beam;

        /// <summary>推演是否因为"我方阵亡"提前结束。</summary>
        public bool DiedOnRoute;
    }

    /// <summary>
    /// 多回合滚动推演（P5）。
    ///
    /// 结构：宽度受限的**束搜索（beam search）**
    ///   第 1 回合：完整枚举 P1 方案 → 取 Top-K
    ///   第 2 回合：把第 1 回合的方案"执行"成新状态 → 再枚举 → 取 Top-K
    ///   ... 直到深度 D
    ///   终局价值 = 累计收益 − 死亡惩罚
    ///
    /// 为什么要束搜索：单回合组合数就上千，3 回合全展开是百万级。
    ///   每层只保留 K 个最有希望的分支，深度 D 就是 D 次枚举，秒级完成。
    ///
    /// ⚠ 简化点（窗口里会明说）：
    ///   · 敌人回合用意图估算，不模拟它真实抽修正牌
    ///   · 队友回合不推进
    ///   · 终局价值是**相对比较**用的，不是真实评分
    /// </summary>
    internal static class MultiTurnSearch
    {
        /// <summary>终局价值评估的权重。</summary>
        public sealed class EvalWeights
        {
            public double PerDamage = 1.0;
            public double PerKill = 8.0;
            public double PerRemainingHealth = 0.4;
            public double PerGoalValue = 1.0;
            public double PerHazard = 1.5;
            public double DeathPenalty = 200.0;
            public double PerRoundSurvived = 1.0;
        }

        /// <summary>跑一次多回合推演。</summary>
        /// <param name="template">第一回合的上下文模板（含寻路委托、权重等）。</param>
        /// <param name="depth">推演多少回合（不含第一回合）。</param>
        /// <param name="beam">每层保留几个分支。</param>
        /// <param name="myInitiative">我的先攻（用来判断敌人这回合会不会动手）。</param>
        public static MultiTurnResult Search(
            P1Context template, int depth, int beam, int myInitiative, EvalWeights w)
        {
            return Search(template, null, depth, beam, myInitiative, w);
        }

        /// <summary>
        /// 跑一次多回合推演。<paramref name="firstRound"/> 可以传入**已经算好的**第一回合方案
        /// （P1Runner 那边刚算过），省掉一次全量枚举 —— 深度越大省得越多。
        /// </summary>
        public static MultiTurnResult Search(
            P1Context template, List<TurnPlan> firstRound, int depth, int beam,
            int myInitiative, EvalWeights w)
        {
            var res = new MultiTurnResult { Depth = depth, Beam = beam };
            if (template == null) return res;
            if (w == null) w = new EvalWeights();
            if (depth <= 0) depth = 1;
            if (beam <= 0) beam = 3;

            // ---- 初始状态 ----
            var init = new WorldState
            {
                MyX = template.StartX,
                MyY = template.StartY,
                Hand = new List<AbilityCardSnapshot>(template.Hand),
                Deck = template.Deck,
                LootSpots = new List<LootSpot>(template.LootSpots),
                Round = 1,

                // 地图查询委托 —— P5 敌人寻路要用（可达格 / 距离 / 视线）
                Reachable = template.Reachable,
                RangeDistance = template.RangeDistance,
                HasLOS = template.HasLOS,
            };
            foreach (UnitSnapshot u in template.Monsters)
            {
                // 挂上它用的那副共享牌堆（P5 敌人真实抽牌）
                if (u != null && u.DeckIndex < 0)
                {
                    u.DeckIndex = UnitSnapshot.DeckIndexForFaction(u.Faction);
                }
                init.Monsters.Add(u);
            }
            foreach (UnitSnapshot u in template.Others) init.Others.Add(u);

            // 怪物四副共享牌堆 —— 深拷贝，束搜索各分支独立推进
            foreach (ModifierDeckSnapshot d in template.MonsterDecks)
            {
                init.EnemyDecks.Add(d == null ? null : new ModifierDeckSnapshot
                {
                    OwnerLabel = d.OwnerLabel,
                    ShufflePending = d.ShufflePending,
                    Remaining = new List<ModifierCardSnapshot>(d.Remaining),
                    Discarded = new List<ModifierCardSnapshot>(d.Discarded),
                });
            }

            // 我的血量从快照里拿不到（ctx 里没有），用怪物表反推不合适 ——
            // 让上层通过 StartHealth 传入，这里默认满血。
            init.MyHealth = template.StartHealth > 0 ? template.StartHealth : 20;
            init.MyMaxHealth = template.StartMaxHealth > 0 ? template.StartMaxHealth : init.MyHealth;

            // ---- 第一层：优先复用外部已算好的方案 ----
            List<TurnPlan> first = firstRound != null && firstRound.Count > 0
                ? firstRound
                : P1Planner.Enumerate(template, beam);
            if (first.Count == 0) return res;

            // ---- 第一层 ----
            var live = new List<(List<RouteStep> steps, WorldState st, double value)>();

            foreach (TurnPlan p in first)
            {
                WorldState st = StateAdvancer.Apply(init, p);
                RouteStep step = MakeStep(st, p, 1);
                // ★ v1.35：每回合用自己的先攻判断敌我先后手。
                //   之前所有层都传第 1 回合的 myInitiative ——
                //   第 2 回合明明选了高先攻，仍按第 1 回合的低先攻结算挨打。
                RunEnemy(st, step, RoundInitiative(p, myInitiative));
                live.Add((new List<RouteStep> { step }, st, Evaluate(st, 1, w)));
            }

            for (int d = 2; d <= depth; d++)
            {
                var next = new List<(List<RouteStep> steps, WorldState st, double value)>();

                foreach (var b in live)
                {
                    if (b.st.Dead) { next.Add(b); continue; }

                    P1Context ctx = b.st.ToContext(template, template.W);
                    if (ctx.Hand.Count < 2)
                    {
                        // 手牌不够两张 → 这回合没法出牌，状态原样保留
                        next.Add(b);
                        continue;
                    }

                    List<TurnPlan> plans = P1Planner.Enumerate(ctx, beam);
                    if (plans.Count == 0) { next.Add(b); continue; }

                    foreach (TurnPlan p in plans)
                    {
                        WorldState st = StateAdvancer.Apply(b.st, p);
                        RouteStep step = MakeStep(st, p, d);

                        // ★ 每一层都要跑敌人回合 —— 上一版只在第一层跑了，
                        //   深度 ≥3 时后面几回合的敌人完全没动手，等于白推。
                        //   v1.35：先攻取**本回合方案**的先攻，不是第 1 回合的。
                        RunEnemy(st, step, RoundInitiative(p, myInitiative));

                        var steps = new List<RouteStep>(b.steps) { step };
                        next.Add((steps, st, Evaluate(st, d, w)));
                    }
                }

                // 按价值排序，截断到 beam
                next.Sort((a, b) => b.value.CompareTo(a.value));
                if (next.Count > beam) next.RemoveRange(beam, next.Count - beam);
                live = next;

                if (live.Count == 0) break;
            }

            // ---- 收集结果 ----
            foreach (var b in live)
            {
                res.Routes.Add(b.steps);
                if (b.value > res.BestValue) res.BestValue = b.value;
                if (b.st.Dead) res.DiedOnRoute = true;
            }
            res.Routes.Sort((a, b) => RouteValue(b, w).CompareTo(RouteValue(a, w)));
            return res;
        }

        /// <summary>本回合结算用的先攻：优先用方案自己的先攻，缺失才退回外部值。</summary>
        private static int RoundInitiative(TurnPlan plan, int fallback)
        {
            return plan != null && plan.Initiative > 0 ? plan.Initiative : fallback;
        }

        /// <summary>造一个路线步骤（还没跑敌人回合时的我方状态）。</summary>
        private static RouteStep MakeStep(WorldState st, TurnPlan plan, int round)
        {
            return new RouteStep
            {
                Round = round,
                Plan = plan,
                MyX = st.MyX,
                MyY = st.MyY,
                MyHealth = st.MyHealth,
                CumulativeDamage = st.TotalDamage,
                CumulativeKills = st.TotalKills,
                Died = st.Dead,
            };
        }

        /// <summary>
        /// 跑敌人回合，并把结果记进这一步。
        /// 我方血量以**敌人打完之后**为准 —— 那才是这一回合真正结束时的状态。
        /// </summary>
        private static void RunEnemy(WorldState st, RouteStep step, int myInitiative)
        {
            step.EnemyStrikes = StateAdvancer.RunEnemyTurn(st, myInitiative);
            step.MyHealth = st.MyHealth;
            step.Died = st.Dead;
        }

        private static double Evaluate(WorldState st, int round, EvalWeights w)
        {
            if (st == null) return double.NegativeInfinity;
            if (st.Dead) return -w.DeathPenalty;

            double v = st.TotalDamage * w.PerDamage
                     + st.TotalKills * w.PerKill
                     + st.MyHealth * w.PerRemainingHealth
                     + st.GoalValue * w.PerGoalValue
                     - st.HazardTaken * w.PerHazard
                     + round * w.PerRoundSurvived;
            return v;
        }

        private static double RouteValue(List<RouteStep> steps, EvalWeights w)
        {
            if (steps == null || steps.Count == 0) return 0;
            RouteStep last = steps[steps.Count - 1];
            double v = last.CumulativeDamage * w.PerDamage
                     + last.CumulativeKills * w.PerKill
                     + last.MyHealth * w.PerRemainingHealth
                     + (last.Died ? -w.DeathPenalty : 0);
            return v;
        }
    }
}

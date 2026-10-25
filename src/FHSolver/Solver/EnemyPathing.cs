using System;
using System.Collections.Generic;

namespace FHSolver.Solver
{
    /// <summary>
    /// 敌人移动预测（P5 精确化）—— **纯函数**，不引用任何游戏类型。
    ///
    /// ═══════════════════════════════════════════════════════════════════
    /// 为什么还要再写一个，而不直接用 <c>Bridge/EnemyPredictor</c>：
    ///   那个类依赖 CScenario / CActor / Geometry 等**游戏类型**，
    ///   而 StateAdvancer 是纯计算层（不引用游戏类型，测试台才能直接编译它）。
    ///   所以这里复刻同一套规则，但输入改成**委托**（P1Context 本来就有）。
    ///
    /// 复刻的是游戏真实的 AI 选位规则（CAIMoveNew，反编译实证）：
    ///   ① 焦点 = 与它敌对的存活单位中，**能打到它所需的移动代价**最小者
    ///      （`lowestMoveCostToAttack` 升序 → hexDist 升序 → 先攻；无随机 tie-break）
    ///   ② 落点 = 可达格里「能打到焦点」且「移动代价最小」的那一格
    ///   ③ 现在站的格子就能打到焦点 → **它不动**（代价 0 已经最小）
    ///
    /// 与上一版「射程 + 移动力 取上界」的区别：
    ///   旧：dist <= AttackRange + MoveStrength 就算够得着 → 纯几何，**会高估**
    ///       （隔着一堵墙、或者根本没有路，也会算成能打到你）
    ///   新：真的去寻路，看**是否存在**一个可达格能打到目标 → 考虑了地形、占用、视线
    /// ═══════════════════════════════════════════════════════════════════
    /// </summary>
    /// <summary>
    /// 怪物这一回合的移动意图。
    ///
    /// ⚠ 这个枚举的存在本身就是一次**纠正**：
    ///   我原先以为"找不到攻击位 → 原地不动"，反编译后发现是错的 ——
    ///   CAIMoveNew.FindPathToFocus() 有明确的 fallback：
    ///   走不到最佳攻击位时，会在**所有可达格**里找一个"离攻击位最近"的格子走过去。
    ///   也就是 Gloomhaven/Frosthaven 的标准规则：**够不着也会尽量靠近焦点**。
    /// </summary>
    internal enum EnemyMovePlan
    {
        /// <summary>不动（移动力为 0，或没有任何可达格）。</summary>
        None,

        /// <summary>能走到一个「可以打到焦点」的格子 → 移动后攻击。</summary>
        Attack,

        /// <summary>走不到任何攻击位 → 朝焦点**靠近**，但这回合不攻击、不抽牌。</summary>
        Approach,
    }

    internal static class EnemyPathing
    {
        /// <summary>一个候选目标。Name 只用于输出。</summary>
        public struct Target
        {
            public int X;
            public int Y;
            public string Name;

            /// <summary>目标的先攻（同距时用来 tie-break，照抄 ActorInitiativeComparer）。</summary>
            public int Initiative;

            public Target(int x, int y, string name, int initiative = int.MaxValue)
            {
                X = x; Y = y; Name = name; Initiative = initiative;
            }
        }

        /// <summary>
        /// 给一个敌人规划「这一回合会走到哪、打谁」。
        /// </summary>
        /// <param name="fromX">它现在的位置</param>
        /// <param name="fromY">它现在的位置</param>
        /// <param name="movePoints">本回合移动力（意图上的 Move；负数已折算过）</param>
        /// <param name="attackRange">攻击射程</param>
        /// <param name="reachable">可达格查询（含危险地形伤害的第三项）</param>
        /// <param name="rangeDistance">格间距离（走寻路，含门/地形的真实距离）</param>
        /// <param name="hasLOS">视线判定</param>
        /// <param name="targets">候选目标（我方单位）</param>
        /// <param name="blocked">被占用的格子（其他单位；**不含它自己**）</param>
        /// <param name="toX">输出：落点</param>
        /// <param name="toY">输出：落点</param>
        /// <param name="focusIdx">输出：焦点在 targets 里的下标；-1 = 打不到任何人</param>
        /// <returns>移动意图（Attack / Approach / None）</returns>
        /// <returns>移动意图：能攻击 / 只能靠近 / 不动</returns>
        public static EnemyMovePlan PlanMove(
            int fromX, int fromY,
            int movePoints,
            int attackRange,
            Func<int, int, int, List<(int X, int Y, int Hazard)>> reachable,
            Func<int, int, int, int, int> rangeDistance,
            Func<int, int, int, int, bool> hasLOS,
            List<Target> targets,
            HashSet<(int, int)> blocked,
            out int toX, out int toY, out int focusIdx)
        {
            toX = fromX;
            toY = fromY;
            focusIdx = -1;

            if (targets == null || targets.Count == 0) return EnemyMovePlan.None;
            if (attackRange < 0) return EnemyMovePlan.None;

            // ---- 候选落点：可达格 + 原地不动 ----
            var spots = new List<(int X, int Y)> { (fromX, fromY) };

            if (movePoints > 0 && reachable != null)
            {
                List<(int X, int Y, int Hazard)> reach;
                try { reach = reachable(fromX, fromY, movePoints); }
                catch { reach = null; }

                if (reach != null)
                {
                    foreach ((int X, int Y, int HAZ) t in reach)
                    {
                        if (t.X == fromX && t.Y == fromY) continue;      // 已在 spots 里
                        if (blocked != null && blocked.Contains((t.X, t.Y))) continue;
                        spots.Add((t.X, t.Y));
                    }
                }
            }

            if (rangeDistance == null) return EnemyMovePlan.None;

            // ---- ① 焦点 = **距离最近**的敌对单位（同距比先攻）----
            //    ★ 严格照抄游戏：CAIMoveNew.ChooseClosestActor + ActorInitiativeComparer
            //      —— 先按 hexDist 选最近，同距再比先攻（小的先动）。
            //
            //    ⚠ v1.31 之前这里是"在**能打到的**目标里选移动代价最小" ——
            //      维度错了！后果：真实 AI 追的是"最近的人"（哪怕够不着），
            //      我却可能去追"移动代价最小的人" → 落点不同 →
            //      多回合推演里第 2 回合打在敌人已经不在这儿的格子上（打空气）。
            int bestFocusDist = int.MaxValue;
            int bestFocusIni = int.MaxValue;
            int bestFocus = -1;

            for (int ti = 0; ti < targets.Count; ti++)
            {
                Target tg = targets[ti];
                int d = rangeDistance(fromX, fromY, tg.X, tg.Y);
                if (d < 0) continue;
                int ini = tg.Initiative;
                if (d < bestFocusDist || (d == bestFocusDist && ini < bestFocusIni))
                {
                    bestFocusDist = d;
                    bestFocusIni = ini;
                    bestFocus = ti;
                }
            }

            if (bestFocus < 0) return EnemyMovePlan.None;
            Target focus = targets[bestFocus];

            // ---- ② 在这个焦点下找落点：能打到它 + 移动代价最小 ----
            int bestCost = int.MaxValue;
            int bestX = -1, bestY = -1;

            for (int si = 0; si < spots.Count; si++)
            {
                (int X, int Y) s = spots[si];

                int r = rangeDistance(s.X, s.Y, focus.X, focus.Y);
                if (r < 0 || r > attackRange) continue;

                // 视线：墙后面打不到
                if (hasLOS != null && !hasLOS(s.X, s.Y, focus.X, focus.Y)) continue;

                // 移动代价 = 从原地走到这一格的距离
                int cost = rangeDistance(fromX, fromY, s.X, s.Y);
                if (cost < 0) continue;

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestX = s.X;
                    bestY = s.Y;
                }
            }

            if (bestCost < int.MaxValue)
            {
                toX = bestX;
                toY = bestY;
                focusIdx = bestFocus;
                return EnemyMovePlan.Attack;
            }

            // ---- ② 打不到任何人 → **朝焦点靠近**（真实 AI 的 fallback，不是原地不动）----
            //    CAIMoveNew.FindPathToFocus：走不到 bestAttackTile 时，
            //    会在所有可达格里挑一个"离攻击位最近"的走过去（InitReachableTiles + 代价比较）。
            //    这里简化为「离**焦点本人**最近的可达格」—— 攻击位本来就是焦点旁边的格子，
            //    两者在绝大多数场景下等价，且更好解释。
            return PlanApproach(fromX, fromY, movePoints, reachable, rangeDistance,
                targets, blocked, spots, out toX, out toY, out focusIdx);
        }

        /// <summary>够不着时的贴近逻辑：走到离焦点最近的可达格（必须比现在更近才动）。</summary>
        private static EnemyMovePlan PlanApproach(
            int fromX, int fromY, int movePoints,
            Func<int, int, int, List<(int X, int Y, int Hazard)>> reachable,
            Func<int, int, int, int, int> rangeDistance,
            List<Target> targets,
            HashSet<(int, int)> blocked,
            List<(int X, int Y)> spots,
            out int toX, out int toY, out int focusIdx)
        {
            toX = fromX;
            toY = fromY;
            focusIdx = -1;

            if (movePoints <= 0) return EnemyMovePlan.None;

            // 焦点 = 离它最近的敌对单位（**同距比先攻**，照抄游戏）
            int bestFd = int.MaxValue;
            int bestIni = int.MaxValue;
            int fi = -1;
            for (int i = 0; i < targets.Count; i++)
            {
                int d = rangeDistance(fromX, fromY, targets[i].X, targets[i].Y);
                if (d < 0) continue;
                int ini = targets[i].Initiative;
                if (d < bestFd || (d == bestFd && ini < bestIni))
                {
                    bestFd = d; bestIni = ini; fi = i;
                }
            }
            if (fi < 0) return EnemyMovePlan.None;

            // 在可达格里找离焦点最近的
            int bestD = bestFd;              // 必须比"现在到焦点的距离"更近才值得动
            int bx = -1, by = -1;
            foreach ((int X, int Y) s in spots)
            {
                if (s.X == fromX && s.Y == fromY) continue;
                int d = rangeDistance(s.X, s.Y, targets[fi].X, targets[fi].Y);
                if (d >= 0 && d < bestD) { bestD = d; bx = s.X; by = s.Y; }
            }

            if (bx < 0) return EnemyMovePlan.None;

            toX = bx;
            toY = by;
            focusIdx = fi;
            return EnemyMovePlan.Approach;
        }
    }
}

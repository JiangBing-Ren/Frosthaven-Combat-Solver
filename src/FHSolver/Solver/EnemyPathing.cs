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
        /// <param name="preferFurthest">
        /// `MiscAbilityData.AttackFurthestTarget`（游戏里的「攻击最远目标」怪）。
        /// 为 true 时焦点先按 hexDist **降序**（CompareEnemiesUsingFurthesFocus）。
        /// </param>
        /// <returns>移动意图（Attack / Approach / None）</returns>
        public static EnemyMovePlan PlanMove(
            int fromX, int fromY,
            int movePoints,
            int attackRange,
            Func<int, int, int, List<(int X, int Y, int Hazard)>> reachable,
            Func<int, int, int, int, int> rangeDistance,
            Func<int, int, int, int, bool> hasLOS,
            List<Target> targets,
            HashSet<(int, int)> blocked,
            out int toX, out int toY, out int focusIdx,
            bool preferFurthest = false)
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

            // ---- ① 每个目标的「攻击代价 / 距离 / 先攻」----
            //   游戏 CAIMoveNew.CompareEnemies（反编译实证，无随机 tie-break）：
            //     isFocusOverride 降序 → lowestMoveCostToAttack 升序
            //     → hexDist 升序 → 调整后先攻
            //   其中 hexDist = 地形感知的**路径长度**（忽略单位），
            //   lowestMoveCostToAttack = 走到**任一攻击位**的最小移动代价。
            //
            //   ⚠ v1.31 把焦点简化成"路径距离最近"，维度不对：
            //     隔困难地形/绕墙时，"最近"和"打它最省移动力"会指向不同目标。
            //     这里按三级键照抄游戏；move cost 用 rangeDistance 近似（不额外计困难地形双倍）。
            int n = targets.Count;
            var dist = new int[n];
            var cost = new int[n];
            var spotX = new int[n];
            var spotY = new int[n];

            for (int ti = 0; ti < n; ti++)
            {
                dist[ti] = rangeDistance(fromX, fromY, targets[ti].X, targets[ti].Y);
                cost[ti] = int.MaxValue;
                spotX[ti] = -1;
                spotY[ti] = -1;
            }

            foreach ((int X, int Y) s in spots)
            {
                int costToSpot = rangeDistance(fromX, fromY, s.X, s.Y);
                if (costToSpot < 0) continue;

                for (int ti = 0; ti < n; ti++)
                {
                    if (cost[ti] <= costToSpot) continue;             // 已有更省的攻击位

                    int r = rangeDistance(s.X, s.Y, targets[ti].X, targets[ti].Y);
                    if (r < 0 || r > attackRange) continue;
                    if (hasLOS != null && !hasLOS(s.X, s.Y, targets[ti].X, targets[ti].Y)) continue;

                    cost[ti] = costToSpot;
                    spotX[ti] = s.X;
                    spotY[ti] = s.Y;
                }
            }

            // ---- ② 焦点（CompareEnemies / CompareEnemiesUsingFurthesFocus）----
            int bestFocus = -1;
            for (int ti = 0; ti < n; ti++)
            {
                if (dist[ti] < 0) continue;                            // 完全没有路径
                if (bestFocus < 0 || BetterFocus(ti, bestFocus, targets, dist, cost, preferFurthest))
                {
                    bestFocus = ti;
                }
            }
            if (bestFocus < 0) return EnemyMovePlan.None;
            Target focus = targets[bestFocus];

            // ---- ③ 能打到焦点 → 走到最省的攻击位（原地代价 0，自然不动）----
            if (cost[bestFocus] < int.MaxValue)
            {
                toX = spotX[bestFocus];
                toY = spotY[bestFocus];
                focusIdx = bestFocus;
                return EnemyMovePlan.Attack;
            }

            // ---- ④ 打不到任何人 → **朝焦点靠近**（真实 AI 的 fallback，不是原地不动）----
            //    CAIMoveNew.FindPathToFocus：走不到 bestAttackTile 时，
            //    会在所有可达格里挑一个"离攻击位最近"的走过去（InitReachableTiles + 代价比较）。
            //    这里简化为「离**焦点本人**最近的可达格」—— 攻击位本来就是焦点旁边的格子，
            //    两者在绝大多数场景下等价，且更好解释。
            if (movePoints <= 0) return EnemyMovePlan.None;

            int bestD = dist[bestFocus];          // 必须比"现在到焦点的距离"更近才值得动
            int bx = fromX, by = fromY;
            bool found = false;
            foreach ((int X, int Y) s in spots)
            {
                if (s.X == fromX && s.Y == fromY) continue;
                int d = rangeDistance(s.X, s.Y, focus.X, focus.Y);
                if (d >= 0 && d < bestD) { bestD = d; bx = s.X; by = s.Y; found = true; }
            }

            // ⚠ 不能用 x/y == -1 当"没找到"的哨兵：
            //   六边形地图的坐标可以是负的（实测 (-1,1) 就是合法格）。
            if (!found) return EnemyMovePlan.None;

            toX = bx;
            toY = by;
            focusIdx = bestFocus;
            return EnemyMovePlan.Approach;
        }

        /// <summary>
        /// 焦点排序：<c>dist</c> 是六边形路径距离，<c>cost</c> 是"走到攻击位的最小代价"。
        /// 与游戏 <c>CompareEnemies</c> 一致：cost 升序 → dist 升序 → 先攻升序；
        /// <c>preferFurthest</c>（AttackFurthestTarget）时先按 dist **降序**。
        /// </summary>
        private static bool BetterFocus(int i, int j, List<Target> targets,
            int[] dist, int[] cost, bool preferFurthest)
        {
            if (preferFurthest && dist[i] != dist[j]) return dist[i] > dist[j];

            if (cost[i] != cost[j]) return cost[i] < cost[j];
            if (dist[i] != dist[j]) return dist[i] < dist[j];
            return targets[i].Initiative < targets[j].Initiative;
        }
    }
}

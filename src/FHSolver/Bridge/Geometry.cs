using System;
using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 地图几何 —— 一律复用游戏自己的实现，不自己复刻。
    ///
    /// 三条通路：
    ///   可达格    → PathFinder.FloodFill_ToRange（取候选）+ CActor.FindCharacterPath（逐个确认真能走到）
    ///   射程距离  → PathFinder.FindPath（与 CAbilityAttack.IsTileWithinRange 同参数）
    ///   视线      → CActor.HaveLOS（与 CAbilityAttack.IsActorWithinRangeAndLoS 同一函数）
    ///
    /// 射程为什么不用 Point.HexDist：
    /// CAbilityAttack.IsTileWithinRange 实际走的是 FindPath(ignoreBlocked:true, ignoreMoveCost:true,
    /// ignoreDifficultTerrain:false, ignoreHazardousTerrain:false) 的路径长度，
    /// 并且有 DoorPath（跨 hex map 的门）拼接逻辑。直接照抄它的参数，行为才一致。
    ///
    /// 可达格为什么分两步：
    /// 游戏自己的移动判定（CAbilityMove.CanBePerformed）就是两步 ——
    ///   ① FloodFill_ToRange(..., ignoreMoveCost:true, ignoreDifficultTerrain:true, ignoreHazardousTerrain:true)
    ///      取一个宽松的超集；
    ///   ② 对每个候选调 CActor.FindCharacterPath(..., ignoreMoveCost:false, ignoreDifficultTerrain:false)
    ///      做严格确认。
    /// 只做①会漏掉占用/代价约束 —— 实测就出现过「推荐走到敌人占的那一格」这种根本执行不了的方案。
    /// FindPlayerPath 内部会把敌人、其他玩家、倒地单位、物体占的格子全部塞进 blockedList，
    /// 所以②天然解决占用问题，和游戏判定完全一致。
    /// </summary>
    internal static class Geometry
    {
        /// <summary>
        /// 从指定起点做移动可达计算。起点可以是「假想位置」。
        ///
        /// 返回 (X, Y, Hazard)：Hazard = 走到这一格**路径上**要吃的危险地形伤害。
        /// 危险地形是**进入时**扣血（站在上面不动不重复扣），
        /// 所以要按整条路径累加，不能只看终点。
        /// </summary>
        public static List<(int X, int Y, int Hazard)> Reachable(
            CActor actor, int startX, int startY, int moveRange)
        {
            var result = new List<(int, int, int)>();
            if (actor == null || moveRange <= 0) return result;

            // 陷阱映射表：一次建好，沿途所有 CanWalk 共用
            var traps = TrapMap();

            try
            {
                CPathFinder finder = ScenarioManager.PathFinder;
                if (finder == null) return result;

                var start = new Point(startX, startY);
                var flooded = new List<Point>();

                // 第一步：宽松洪水填充，与 CAbilityMove.CanBePerformed 同参数
                finder.FloodFill_ToRange(
                    actor,
                    start,
                    moveRange,
                    ignoreBlocked: false,
                    ignoreMoveCost: true,
                    ignoreDifficultTerrain: true,
                    ignoreHazardousTerrain: true,
                    waterTilesThatCostZero: 0,
                    blockedList: null,
                    superBlockedList: null,
                    outFloodedPoints: flooded,
                    transientBlockedList: null,
                    ignoreDoorLocks: false);

                // 第二步：严格确认。HexDist 先快速筛掉明显超范围的，减少 A* 次数
                foreach (Point p in flooded)
                {
                    if (p == null) continue;
                    if (Point.HexDist(start, p) > moveRange) continue;
                    if (!CanWalk(actor, start, p, moveRange, out int hazard, traps)) continue;
                    result.Add((p.X, p.Y, hazard));
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 可达格计算失败: {e.Message}");
            }
            return result;
        }

        /// <summary>
        /// 这个角色真的能从 from 走到 to 吗（且代价不超预算）。
        /// 用游戏自己的寻路，占用/困难地形/危险地形/门全部由它判定。
        /// </summary>
        private static bool CanWalk(CActor actor, Point from, Point to, int moveRange,
            out int hazardDamage, Dictionary<(int X, int Y), int> trapMap = null)
        {
            hazardDamage = 0;
            try
            {
                List<Point> path = CActor.FindCharacterPath(
                    actor,
                    from,
                    to,
                    ignoreBlocked: false,
                    ignoreMoveCost: false,
                    out bool foundPath,
                    ignoreDifficultTerrain: false,
                    ignoreNegativeTiles: false,
                    waterTilesThatCostZero: 0,
                    logFailure: false,
                    maxMovementPoints: moveRange);

                if (!foundPath || path == null || path.Count > moveRange) return false;

                // 路径上每进入一个危险地形格扣一次血
                int perTile = ScenarioManager.HazardousTerrainDamage;
                if (perTile > 0)
                {
                    foreach (Point p in path)
                    {
                        if (p != null && IsHazardTile(p.X, p.Y)) hazardDamage += perTile;
                    }
                }

                // 陷阱同理：**进入**陷阱格扣血（伤害型陷阱才扣，见 TrapMap）
                if (trapMap != null)
                {
                    foreach (Point p in path)
                    {
                        if (p == null) continue;
                        int td;
                        if (trapMap.TryGetValue((p.X, p.Y), out td) && td > 0)
                        {
                            hazardDamage += td;
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                // 拿不准就放行（洪水填充已经筛过一轮），但记一次，避免刷屏
                Plugin.Log?.LogWarning($"[FHSolver] FindCharacterPath 不可用，可达格退化为洪水填充结果: {e.Message}");
                return true;
            }
        }

        /// <summary>
        /// 射程距离 —— 复刻 CAbilityAttack.IsTileWithinRange 的取法。
        /// </summary>
        public static int RangeDistance(int fromX, int fromY, int toX, int toY)
        {
            try
            {
                CPathFinder finder = ScenarioManager.PathFinder;
                if (finder == null) return Point.HexDist(new Point(fromX, fromY), new Point(toX, toY));

                List<Point> path = finder.FindPath(
                    null,
                    new Point(toX, toY),      // 与游戏一致：target → attacker
                    new Point(fromX, fromY),
                    ignoreBlocked: true,
                    ignoreMoveCost: true,
                    ignoreDifficultTerrain: false,
                    ignoreHazardousTerrain: false,
                    waterTilesThatCostZero: 0,
                    out bool foundPath,
                    ignoreDoorLocks: false);

                if (foundPath) return path.Count;
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 射程距离计算失败: {e.Message}");
            }
            return Point.HexDist(new Point(fromX, fromY), new Point(toX, toY));
        }

        /// <summary>
        /// 该格是不是阴影格（死行的阴影机制）。
        ///
        /// 官方判定就是这一句，原样照抄：
        ///   `CAbilityMove.cs` 里 `CTile.FindPropOfType<CObjectShadow>() != null`
        ///
        /// 「暗影步法」上半的 `MoveRestrictionType: MustEndOnShadow` 靠它落地 ——
        /// 它可以"先移动 3 再攻击 3"，但**必须停在阴影格上**。
        /// </summary>
        public static bool IsShadowTile(int x, int y)
        {
            try
            {
                CTile[,] tiles = ScenarioManager.Tiles;
                if (tiles == null) return false;
                int w = tiles.GetLength(0);
                int h = tiles.GetLength(1);
                if (x < 0 || y < 0 || x >= w || y >= h) return false;

                CTile t = tiles[x, y];
                if (t == null) return false;
                return t.FindPropOfType<CObjectShadow>() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 场上所有阴影的位置（死行 / Deathwalker 的职业机制）。
        ///
        /// 阴影是叠在地块上的道具 `CObjectShadow`，**不是**光照阴影。
        /// 官方上限 **5** 个（CAbilityCreateShadow：`num + 选中格数 - 待移除数 <= 5`）。
        ///
        /// 用途：
        ///   · 「暗影步法」MustEndOnShadow 的落点判定（已有 IsShadowTile）
        ///   · 「深渊力量」攻击 = 1 + 阴影数（StatIsBasedOnX 已求值）
        ///   · 「从阴影中发动」类能力的前提判定
        ///   · 诊断里告诉玩家现在有几个、在哪
        /// </summary>
        public static List<(int X, int Y)> ShadowTiles()
        {
            var list = new List<(int, int)>();
            try
            {
                var props = ScenarioManager.CurrentScenarioState?.ShadowProps;
                if (props == null) return list;

                foreach (CObjectProp prop in props)
                {
                    if (prop == null) continue;
                    if (!(prop is CObjectShadow)) continue;      // 只算真阴影
                    var sh = (CObjectShadow)prop;
                    if (sh.WillBeRemoved) continue;              // 本回合将被移除的不算

                    TileIndex idx = prop.ArrayIndex;
                    if (idx != null) list.Add((idx.X, idx.Y));
                }
            }
            catch
            {
                // 读不到就是没有，不影响主流程
            }
            return list;
        }

        /// <summary>当前阴影数量（上限 5）。</summary>
        public static int ShadowCount()
        {
            try { return ShadowTiles().Count; }
            catch { return 0; }
        }

        /// <summary>
        /// 该格是不是危险地形（Hazardous Terrain）。
        /// 判定照抄游戏：`CTile.FindPropOfType&lt;CObjectHazardousTerrain&gt;() != null`。
        /// 伤害值是场景级的 `ScenarioManager.HazardousTerrainDamage`（默认 1）。
        /// </summary>
        public static bool IsHazardTile(int x, int y)
        {
            try
            {
                CTile[,] tiles = ScenarioManager.Tiles;
                if (tiles == null) return false;
                int w = tiles.GetLength(0);
                int h = tiles.GetLength(1);
                if (x < 0 || y < 0 || x >= w || y >= h) return false;

                CTile t = tiles[x, y];
                if (t == null) return false;
                return t.FindPropOfType<CObjectHazardousTerrain>() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 场上所有**伤害型陷阱**的位置与伤害值。
        ///
        /// 反编译实证：
        ///   · 陷阱 = `ObjectImportType.Trap`
        ///   · `CObjectTrap.Damage == false` 的**不是**伤害陷阱（可能是治疗/增益），要排除
        ///   · `CObjectTrap.GetTrapDamage()`：无主且未自定义 → 场景级默认 `ScenarioManager.TrapDamage`；
        ///     否则用 `DamageValue`
        ///
        /// 一次建表而不是逐格查询：Reachable 会对每个可达格各跑一次寻路，
        /// 逐格遍历 props 会变成 O(格数 x 路径长 x props 数)。
        /// </summary>
        public static Dictionary<(int X, int Y), int> TrapMap()
        {
            var map = new Dictionary<(int, int), int>();
            try
            {
                var props = ScenarioManager.CurrentScenarioState?.GetPropsThreadSafe;
                if (props == null) return map;

                foreach (CObjectProp prop in props)
                {
                    if (prop == null) continue;
                    if (prop.ObjectType != ScenarioManager.ObjectImportType.Trap) continue;

                    var trap = prop as CObjectTrap;
                    if (trap == null || !trap.Damage) continue;      // 只算伤害型

                    TileIndex idx = prop.ArrayIndex;
                    if (idx == null) continue;

                    int dmg = trap.GetTrapDamage();
                    if (dmg > 0) map[(idx.X, idx.Y)] = dmg;
                }
            }
            catch
            {
                // 读不到就当没有陷阱
            }
            return map;
        }

        /// <summary>该格的陷阱伤害（0 = 没有或不是伤害型）。</summary>
        public static int TrapDamageAt(int x, int y, Dictionary<(int X, int Y), int> map = null)
        {
            if (map == null) map = TrapMap();
            int v;
            return map.TryGetValue((x, y), out v) ? v : 0;
        }

        /// <summary>视线 —— 直接调游戏的 CActor.HaveLOS。</summary>
        public static bool HasLOS(int fromX, int fromY, int toX, int toY)
        {
            try
            {
                CTile[,] tiles = ScenarioManager.Tiles;
                if (tiles == null) return true;
                int w = tiles.GetLength(0);
                int h = tiles.GetLength(1);
                if (fromX < 0 || fromY < 0 || toX < 0 || toY < 0 || fromX >= w || fromY >= h || toX >= w || toY >= h)
                {
                    return false;
                }
                CTile a = tiles[fromX, fromY];
                CTile b = tiles[toX, toY];
                if (a == null || b == null) return false;
                return CActor.HaveLOS(a, b);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 视线判定失败: {e.Message}");
                return true;   // 判定失败时放行，避免整片候选被误杀
            }
        }
    }
}

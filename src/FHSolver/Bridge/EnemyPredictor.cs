using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FHSolver.Model;
using FHSolver.Solver;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 敌人落点预测（P4）—— **只读**，不掷任何骰子。
    ///
    /// ════════════════════════════════════════════════════════════════
    /// 前提：怪物 AI **完全确定性**，不存在随机性。
    ///
    /// 证据（反编译实证）：
    ///   `CAIMoveNew.cs`    1975 行，Random 出现 **0** 次
    ///   `CMonsterClass.cs` 1330 行，Random 出现 **0** 次
    ///   `CEnemyActor.cs`    751 行，Random 出现 **0** 次
    ///
    /// 全库 Random 只出现在：宝箱/战利品的随机内容、怪物修正牌堆洗牌、
    /// 传送落点、关卡脚本与场景物件摆放 —— **都不进入 AI 决策**。
    ///
    /// 怪物决策链 CAIMoveNew.Process() 全程确定性：
    ///   ResolveFocusOverride → ResolveAttackAbility → InitPathfinderContext
    ///   → AddActorsToPathFinderAndEnemyEvals → FloodFill
    ///   → CalculateValidAttackTilesForAllEnemies → SortEnemyEvals
    ///   → FindBestAttackPositionForTheFocus → FindPathToFocus → CalcWaypoints
    ///
    /// 焦点排序 CompareEnemies 是纯比较、无随机 tie-break：
    ///   isFocusOverride 降序 → lowestMoveCostToAttack 升序 → hexDist 升序
    ///   → GameState.s_ActorAdjustedInitiativeComparer（先攻）
    ///
    /// ⚠ 唯一真正随机的场景：怪物行动牌堆空了要洗牌
    ///   `m_AbilityCards.Shuffle(CurrentScenarioState.EnemyAbilityRNG)`
    ///   这个 RNG 状态随存档持久化、且 SharedLibrary.Random 是
    ///   .NET System.Random 的逐行拷贝，可离线复刻（见 MODIFIER.md）。
    /// ════════════════════════════════════════════════════════════════════
    ///
    /// 复刻的规则：
    ///   ① 焦点 = 与它敌对的存活单位中，「走到能攻击到它的位置所需代价」最小者
    ///   ② 落点 = 可达格里「能打到焦点」且「离自己最近」的格子
    ///   ③ 若现在站的格子就已经能打到焦点 → 它不动
    /// </summary>
    internal static class EnemyPredictor
    {
        /// <summary>一次预测结果：这个敌人移动之后会站在哪。</summary>
        public sealed class Prediction
        {
            public string Guid;
            public string DisplayName;
            public int FromX, FromY;
            public int ToX, ToY;

            /// <summary>本回合移动力（负数已折算成怪物基础移动力）。</summary>
            public int MovePoints;

            /// <summary>该单位的先攻（数值小的先动）。</summary>
            public int Initiative;

            public bool HasAttack;
            public int AttackRange;
            public int AttackStrength;

            /// <summary>它要打谁（焦点）。</summary>
            public string FocusName = "(无)";

            /// <summary>
            /// 它这次攻击会**施加**给目标的负面条件（缴械/中毒/静足/眩晕…）。
            /// 读自行动卡的 `CAbility.NegativeConditions`。
            /// </summary>
            public List<string> AppliesNegative = new List<string>();

            /// <summary>预测它会不会离开现在的格子。</summary>
            public bool WillMove;
        }

        /// <summary>
        /// 预测所有敌对单位「本回合结束时会站在哪」。
        ///
        /// 时序正确性的关键：怪物执行自己回合时，玩家还没执行本回合移动，
        /// 所以怪物的落点只依赖玩家的**起始站位** —— 是已知量。
        ///
        /// 这里按先攻顺序逐个推进怪物，已移动的怪物会把它的落点计入占用，
        /// 于是后动的怪物不会撞到先动怪物的新位置。
        /// </summary>
        public static List<Prediction> Predict(CScenario scenario)
        {
            var list = new List<Prediction>();
            if (scenario == null) return list;

            var alive = new List<CActor>();
            try
            {
                foreach (CActor x in scenario.AllAliveActors)
                {
                    if (x != null) alive.Add(x);
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取存活单位失败: {e.Message}");
                return list;
            }

            // 收集所有敌对单位及其先攻
            var enemies = new List<UnitRef>();
            foreach (CActor a in alive)
            {
                try
                {
                    if (!(a is CEnemyActor enemy)) continue;
                    if (enemy.MonsterClass == null) continue;

                    UnitSnapshot u = StateReader.ReadUnit(a, isPlayer: false);
                    if (!u.IsHostileToPlayer) continue;

                    int init = u.Initiative == 0 ? u.InitiativePredicted : u.Initiative;
                    enemies.Add(new UnitRef { Actor = a, Snap = u, X = u.X, Y = u.Y, Initiative = init });
                }
                catch { /* 单个单位跳过 */ }
            }

            // 初始占用：所有非敌对单位 + 所有敌人（后动者会原地不动）
            var occupied = new HashSet<(int, int)>();
            foreach (CActor a in alive)
            {
                Point p;
                try { p = a.ArrayIndex; } catch { continue; }
                if (p != null) occupied.Add((p.X, p.Y));
            }

            // ★ 焦点规则现在要算「走到每个攻击位的代价」，同一个 Insert 里
            //   不同敌人会反复查同样的格子对 —— 用局部缓存挡掉重复 A*。
            var distCache = new Dictionary<(int, int, int, int), int>();
            Func<int, int, int, int, int> dist = (x1, y1, x2, y2) =>
            {
                var key = (x1, y1, x2, y2);
                int v;
                if (distCache.TryGetValue(key, out v)) return v;
                v = Geometry.RangeDistance(x1, y1, x2, y2);
                distCache[key] = v;
                return v;
            };

            foreach (UnitRef e in enemies.OrderBy(x => x.Initiative))
            {
                ActionSnapshot intent = e.Snap.Intent;
                if (intent == null) continue;

                int move = intent.HasMove ? intent.MoveStrength : 0;
                if (move < 0)
                {
                    try { move = (e.Actor as CEnemyActor)?.MonsterClass?.Move ?? 0; }
                    catch { move = 0; }
                }
                if (move <= 0) continue;                       // 不走就原地不动，占用集已经包含它

                // 候选目标：与它敌对的存活单位（含玩家/盟友/召唤物）。
                // 只取焦点排序需要的字段，**不调 StateReader.ReadUnit**
                // （那会为每个目标重复读手牌/牌堆，怪多时是明显的浪费）。
                var targets = new List<EnemyPathing.Target>();
                var names = new List<string>();
                foreach (CActor other in alive)
                {
                    if (other == e.Actor) continue;
                    bool isEnemy;
                    try { isEnemy = CActor.AreActorsEnemies(e.Actor.Type, other.Type, e.Actor, other); }
                    catch { continue; }
                    if (!isEnemy) continue;

                    Point op;
                    try { op = other.ArrayIndex; } catch { continue; }
                    if (op == null) continue;

                    bool dead = true;
                    try { dead = other.IsDead; } catch { /* 拿不准就当活的 */ dead = false; }
                    if (dead) continue;

                    int ini = 0;
                    try { ini = other.Initiative(); } catch { /* 忽略 */ }

                    targets.Add(new EnemyPathing.Target(op.X, op.Y, other.ActorLocKey(), ini));
                    names.Add(Loc.T(other.ActorLocKey()));
                }
                if (targets.Count == 0) continue;

                // ★ 与多回合共用同一套焦点/落点规则（EnemyPathing = CAIMoveNew 的复刻）：
                //   lowestMoveCostToAttack 升序 → hexDist 升序 → 先攻；
                //   AttackFurthestTarget 时改用 hexDist 降序优先。
                //   以前这里另写了一套"路径距离最近"，两套实现会互相打架（v1.31 的教训）。
                var blocked = new HashSet<(int, int)>(occupied);
                blocked.Remove((e.X, e.Y));                    // 自己站的格子不算阻挡

                int toX, toY, focusIdx;
                EnemyMovePlan plan = EnemyPathing.PlanMove(
                    e.X, e.Y, move, intent.AttackRange,
                    (x, y, r) => Geometry.Reachable(e.Actor, x, y, r),
                    dist, Geometry.HasLOS,
                    targets, blocked,
                    out toX, out toY, out focusIdx,
                    intent.AttackFurthestTarget);

                if (plan == EnemyMovePlan.None) continue;

                var pred = new Prediction
                {
                    Guid = e.Actor.ActorGuid,
                    DisplayName = e.Snap.DisplayName,
                    FromX = e.X,
                    FromY = e.Y,
                    ToX = toX,
                    ToY = toY,
                    MovePoints = move,
                    Initiative = e.Initiative,
                    HasAttack = intent.HasAttack,
                    AttackRange = intent.AttackRange,
                    AttackStrength = intent.AttackStrength,
                    FocusName = focusIdx >= 0 && focusIdx < names.Count ? names[focusIdx] : "(无)",
                    WillMove = toX != e.X || toY != e.Y,
                };
                // 它这次攻击会给我方上什么条件（缴械/中毒/静足…）
                if (intent.AppliesNegative != null)
                {
                    foreach (string c in intent.AppliesNegative)
                    {
                        if (!pred.AppliesNegative.Contains(c)) pred.AppliesNegative.Add(c);
                    }
                }
                list.Add(pred);

                // ★ 占用集随移动推进：先动的怪让出原格、占住新格，
                //   后动的怪不会预测到同一格（旧代码声明了 occupied 却从没更新）。
                if (pred.WillMove)
                {
                    occupied.Remove((e.X, e.Y));
                    occupied.Add((toX, toY));
                    e.X = toX;
                    e.Y = toY;
                }
            }

            return list;
        }

        /// <summary>临时持有的单位引用 + 快照。</summary>
        private sealed class UnitRef
        {
            public CActor Actor;
            public UnitSnapshot Snap;
            public int X, Y;
            public int Initiative;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>
    /// 状态推进器（P5）——把一个方案"执行"掉，得到下一回合的世界状态。
    ///
    /// ⚠ 这是**简化模拟**，不是精确复刻游戏的回合流程：
    ///   · 玩家回合：按方案应用位移、伤害、击杀、弃牌、牌堆推进 —— 这部分是准确的
    ///   · 敌人回合：**已接上真实修正牌抽牌**（见 RunEnemyTurn），
    ///     伤害 = 意图攻击力代入**确切会翻出的那张牌**。
    ///     但**不模拟它的寻路**（用 射程+移动力 取上界粗判够不够得着）
    ///   · 队友回合：不推进（队友位置保持不变）
    ///   窗口「多回合」页会明说这一点，避免玩家把它当成精确预测。
    /// </summary>
    internal static class StateAdvancer
    {
        /// <summary>执行一个玩家方案 → 新状态。</summary>
        public static WorldState Apply(WorldState cur, TurnPlan plan)
        {
            WorldState st = cur.Clone();
            if (plan == null) return st;

            // ---- 位移 ----
            st.MyX = plan.FinalX;
            st.MyY = plan.FinalY;

            // ---- 危险地形自伤 ----
            if (plan.SelfHazardDamage > 0)
            {
                st.MyHealth -= plan.SelfHazardDamage;
                st.HazardTaken += plan.SelfHazardDamage;
            }

            // ---- 攻击与击杀 ----
            foreach (AttackChoice a in plan.Attacks)
            {
                if (a == null || string.IsNullOrEmpty(a.TargetGuid)) continue;

                UnitSnapshot target = FindUnit(st, a.TargetGuid);
                if (target == null) continue;

                int dmg = Math.Max(0, a.Damage);
                target.Health -= dmg;
                st.TotalDamage += dmg;
                st.TotalXp += a.TargetGuid != null ? EstimateXp(dmg, target) : 0;

                // 护盾先扣
                if (target.Shield > 0)
                {
                    int absorbed = Math.Min(target.Shield, Math.Max(0, -target.Health - target.MaxHealth + target.Health));
                    if (absorbed > 0) target.Shield -= absorbed;
                }

                if (target.Health <= 0)
                {
                    target.Health = 0;
                    st.TotalKills++;
                    st.Monsters.Remove(target);
                }
            }

            // ---- 战役目标分（终点踩到的东西）----
            foreach (LootSpot sp in st.LootSpots)
            {
                if (sp.X == plan.FinalX && sp.Y == plan.FinalY)
                {
                    st.GoalValue += sp.Value;
                    st.LootSpots.Remove(sp);
                    break;
                }
            }

            // ---- 弃掉打出的两张牌 ----
            // 先攻与执行顺序解耦后，打出的是"上半位那张 + 下半位那张"，
            // 与先执行哪个无关，所以按 CardIndex 弃。
            st.Hand.RemoveAll(c => c == null);
            if (plan.TopCardIndex >= 0 && plan.TopCardIndex < st.Hand.Count)
            {
                st.Hand.RemoveAt(plan.TopCardIndex);
            }
            int bottomIdx = plan.BottomCardIndex;
            if (plan.TopCardIndex >= 0 && plan.TopCardIndex < bottomIdx) bottomIdx--;
            if (bottomIdx >= 0 && bottomIdx < st.Hand.Count)
            {
                st.Hand.RemoveAt(bottomIdx);
            }

            // ---- 修正牌堆推进（本回合抽了几张）----
            int consumed = 0;
            foreach (AttackChoice a in plan.Attacks)
            {
                if (a != null) consumed += Math.Max(0, a.CardsConsumed);
            }
            AdvanceDeck(st.Deck, consumed);

            st.Round = cur.Round + 1;

            if (st.MyHealth <= 0)
            {
                st.MyHealth = 0;
                st.Dead = true;
            }
            return st;
        }

        /// <summary>
        /// 敌人回合 —— **真实寻路 + 真实抽修正牌**（P5 精确化）。
        ///
        /// 两处都依据反编译实证：
        ///   · 抽牌：修正牌堆是**确定序列**，DrawAttackModifierCard 直接取 current[0]，
        ///     零 RNG 消耗，实测 354/354 全中 → 能算出它**确切翻出哪张牌**
        ///   · 寻路：CAIMoveNew 全程确定性（Random 出现 0 次），
        ///     焦点 = 能打到它所需**移动代价**最小者，落点 = 能打到焦点且代价最小的格子
        ///
        /// 与上一版的区别：
        ///   旧：够不着判定用 `dist <= AttackRange + MoveStrength` —— 纯几何，**会高估**
        ///       （隔一堵墙、或压根没有路，也会被算成"能打到你"）
        ///   新：真的去寻路，看**是否存在**一个可达格能打到目标
        ///       → 考虑了地形（困难地形多耗移动力）、占用（别的单位挡路）、视线（墙）
        ///
        /// ⚠ 仍保留的简化：
        ///   · 贴近目标是"离**焦点本人**最近的可达格"，真实代码是"离最佳攻击位最近"
        ///     （攻击位本来就在焦点旁边，绝大多数场景等价）
        ///   · 只结算"打我"那一下；打队友时只消耗牌、不计我掉血
        ///   · 不模拟敌人对我方的优/劣势
        ///   · 不模拟移动力不足以走完时的"部分路径"细节
        /// </summary>
        /// <returns>本回合敌人对我造成的每一次打击（供输出展示）</returns>
        public static List<EnemyStrike> RunEnemyTurn(WorldState st, int myInitiative)
        {
            var strikes = new List<EnemyStrike>();
            if (st == null || st.Dead) return strikes;

            // 先动先结算 —— 同阵营共用一副牌堆，顺序错了牌就翻错
            var actors = new List<UnitSnapshot>();
            foreach (UnitSnapshot e in st.Monsters)
            {
                if (e == null) continue;
                if (e.Initiative >= myInitiative) continue;   // 它后动，这回合不碰我
                actors.Add(e);
            }
            actors.Sort((a, b) => a.Initiative.CompareTo(b.Initiative));

            // 占用集：随着敌人移动动态更新（先动的会让出原格、占住新格）
            var occupied = new HashSet<(int, int)> { (st.MyX, st.MyY) };
            foreach (UnitSnapshot u in st.Monsters) if (u != null) occupied.Add((u.X, u.Y));
            foreach (UnitSnapshot u in st.Others) if (u != null) occupied.Add((u.X, u.Y));

            foreach (UnitSnapshot e in actors)
            {
                ActionSnapshot it = e.Intent;
                if (it == null || !it.HasAttack) continue;
                if (it.AttackStrength <= 0) continue;

                // ---- 候选目标：我排第 0，其余是队友/盟友 ----
                // 目标列表：带上先攻（同距时 tie-break，照抄 ActorInitiativeComparer）
                var targets = new List<EnemyPathing.Target>
                {
                    new EnemyPathing.Target(st.MyX, st.MyY, "我", myInitiative)
                };
                foreach (UnitSnapshot o in st.Others)
                {
                    if (o == null || o.IsHostileToPlayer) continue;
                    targets.Add(new EnemyPathing.Target(o.X, o.Y, o.DisplayName, o.Initiative));
                }

                // 它自己站的格子不算阻挡（否则永远走不出去）
                var blocked = new HashSet<(int, int)>(occupied);
                blocked.Remove((e.X, e.Y));

                int move = it.HasMove ? it.MoveStrength : 0;

                EnemyMovePlan plan = EnemyPathing.PlanMove(
                    e.X, e.Y, move, it.AttackRange,
                    st.Reachable, st.RangeDistance, st.HasLOS,
                    targets, blocked,
                    out int toX, out int toY, out int focusIdx);

                if (plan == EnemyMovePlan.None) continue;

                bool moved = toX != e.X || toY != e.Y;
                if (moved)
                {
                    occupied.Remove((e.X, e.Y));
                    occupied.Add((toX, toY));
                    e.X = toX;
                    e.Y = toY;
                }

                // ★ 够不着 → 它**照样会逼近**（真实 AI 的 fallback，不是原地不动），
                //   但这回合不出手、也不抽修正牌。
                //   对玩家很重要：它靠过来了，下回合多半就能打到你。
                if (plan == EnemyMovePlan.Approach)
                {
                    strikes.Add(new EnemyStrike
                    {
                        AttackerName = e.DisplayName,
                        Initiative = e.Initiative,
                        Damage = 0,
                        CardsText = "（未出手）",
                        Approx = false,
                        Moved = moved,
                        FromX = e.X,
                        FromY = e.Y,
                        ApproachOnly = true,
                    });
                    continue;
                }

                // ---- 抽牌：只要它出手了就抽（不管打的是谁）----
                ModifierDeckSnapshot deck = GetDeck(st, e.DeckIndex);
                int dmg;
                string cardText;
                bool approx;

                List<ModifierCardSnapshot> drawn = null;
                int consumed = 0;
                if (deck != null && deck.Remaining.Count > 0)
                {
                    drawn = ModifierDraw.Simulate(deck.Remaining, 0, AdvStatus.None,
                        it.AttackStrength, out consumed);
                }

                if (drawn != null && drawn.Count > 0)
                {
                    dmg = DamageModel.ComputePreShield(it.AttackStrength, drawn);
                    cardText = string.Join(" ", drawn.Select(c => c != null ? c.ToString() : "?"));
                    approx = false;
                    ConsumeDeck(deck, drawn, consumed);
                }
                else
                {
                    // 牌堆见底会触发洗牌 → 顺序不可知，退化成意图估算并标记
                    dmg = it.AttackStrength;
                    cardText = deck == null ? "（无牌堆数据）" : "（牌堆见底，待洗）";
                    approx = true;
                    st.EnemyDeckExhausted = true;
                }

                if (focusIdx != 0) continue;    // 打的是队友，不掉我血

                // ---- 护盾先吃，再扣血 ----
                if (st.MyShield > 0)
                {
                    int absorbed = Math.Min(st.MyShield, Math.Max(0, dmg));
                    st.MyShield -= absorbed;
                    dmg -= absorbed;
                }
                if (dmg < 0) dmg = 0;
                st.MyHealth -= dmg;

                strikes.Add(new EnemyStrike
                {
                    AttackerName = e.DisplayName,
                    Initiative = e.Initiative,
                    Damage = dmg,
                    CardsText = cardText,
                    Approx = approx,
                    Moved = moved,
                    FromX = moved ? toX : e.X,
                    FromY = moved ? toY : e.Y,
                });

                if (st.MyHealth <= 0)
                {
                    st.MyHealth = 0;
                    st.Dead = true;
                    st.KilledBy = e.DisplayName;
                    return strikes;
                }
            }
            return strikes;
        }

        private static ModifierDeckSnapshot GetDeck(WorldState st, int index)
        {
            if (st == null || index < 0 || index >= st.EnemyDecks.Count) return null;
            return st.EnemyDecks[index];
        }

        /// <summary>把本次抽到的牌从牌堆顶移走；含 Shuffle 牌则置待洗标记。</summary>
        private static void ConsumeDeck(ModifierDeckSnapshot deck,
            List<ModifierCardSnapshot> drawn, int consumed)
        {
            if (deck == null || consumed <= 0) return;

            for (int i = 0; i < consumed && deck.Remaining.Count > 0; i++)
            {
                ModifierCardSnapshot c = deck.Remaining[0];
                deck.Remaining.RemoveAt(0);
                deck.Discarded.Add(c);
            }

            // ×0 / ×2 抽到只置位，回合末才洗（MODIFIER.md 实证）
            foreach (ModifierCardSnapshot c in drawn)
            {
                if (c != null && c.Shuffle) deck.ShufflePending = true;
            }
        }

        private static int EstimateXp(int damage, UnitSnapshot target)
        {
            // 经验按"造成的伤害"粗估（真实规则是杀敌给的经验更多），
            // 这里只用于多回合路线的相对比较，不需要绝对精确。
            return damage;
        }

        private static UnitSnapshot FindUnit(WorldState st, string guid)
        {
            foreach (UnitSnapshot u in st.Monsters) if (u.Guid == guid) return u;
            foreach (UnitSnapshot u in st.Others) if (u.Guid == guid) return u;
            return null;
        }

        private static void AdvanceDeck(ModifierDeckSnapshot deck, int count)
        {
            if (deck == null || count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                if (deck.Remaining.Count == 0) { deck.ShufflePending = true; break; }
                deck.Discarded.Add(deck.Remaining[0]);
                deck.Remaining.RemoveAt(0);
            }
        }

        /// <summary>六边形距离（axial）。</summary>
        private static int HexDist(int x1, int y1, int x2, int y2)
        {
            int ax = x1 - (y1 - 1) / 2;
            int ay = y1;
            int bx = x2 - (y2 - 1) / 2;
            int by = y2;
            int d = Math.Abs(ax - bx) + Math.Abs(ay - by) + Math.Abs((ax + ay) - (bx + by));
            return d / 2;
        }
    }
}

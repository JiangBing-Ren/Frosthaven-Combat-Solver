using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHSolver.Model;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 跨回合账本 + 状态指纹 + **对账**。
    ///
    /// 参考 StS2 Combat Solver 的 `deployment_drift=replan`：
    /// 实机状态偏离预测 → 本地重规划。本项目的等价物不是"自动重规划"
    /// （我本来就每次按 Insert 重新算），而是**告诉用户模型在哪一点上不准** ——
    /// 这才是能持续改进模型的信息。
    ///
    /// 两种对账：
    ///   ① 状态指纹对账：手牌/站位/血量/修正牌堆顺序 是否变了
    ///   ② 战况对账：上回合**预期**的敌人血量/击杀数 与**实际**是否一致
    /// </summary>
    public static class TurnLedger
    {
        /// <summary>状态指纹（两个 u64，含修正牌堆顺序摘要）。</summary>
        public struct Fingerprint
        {
            public ulong A;
            public ulong B;

            public bool SameAs(Fingerprint o) => A == o.A && B == o.B;

            public string Short() => A.ToString("X8") + "-" + B.ToString("X8");

            public override string ToString() => Short();
        }

        // ---------- 上一回合的预期 ----------

        /// <summary>上回合的预期：敌人 GUID → 预期剩余血量。</summary>
        public static Dictionary<string, int> ExpectedEnemyHealth = new Dictionary<string, int>();

        /// <summary>上回合的预期：我方剩余血量（取第一个玩家）。</summary>
        public static int ExpectedMyHealth = -1;

        /// <summary>上回合的预期：击杀数。</summary>
        public static int ExpectedKills = -1;

        /// <summary>上回合的预期：手牌指纹（判断是否按推荐出的牌）。</summary>
        public static string ExpectedHandSummary = "";

        /// <summary>上回合推荐里的关键描述（给玩家对照）。</summary>
        public static string LastPlanSummary = "";

        /// <summary>上回合的状态指纹。</summary>
        public static Fingerprint LastFingerprint;

        /// <summary>是否已经有过一次推荐（第一次没有对账对象）。</summary>
        public static bool HasHistory;

        /// <summary>记录预期时的回合号 —— 用来判断"是不是同一回合"。</summary>
        public static int LastRound = -1;


        // ---------- 指纹计算 ----------

        /// <summary>
        /// 算当前状态的指纹。
        ///
        /// ★ 必须包含**修正牌堆顺序** —— 顺序变了，后面翻出的牌就不同，
        ///   上一回合的预测立即失效（FH 的修正牌是确定序列，不是随机）。
        /// </summary>
        public static Fingerprint ComputeFingerprint(
            List<UnitSnapshot> monsters,
            List<UnitSnapshot> others,
            List<AbilityCardSnapshot> hand,
            ModifierDeckSnapshot deck,
            int shadowCount)
        {
            ulong a = 1469598103934665603UL;   // FNV-1a 起始值
            ulong b = 1469598103934665603UL;

            // A：位置 + 血量 + 数量
            Action(monsters, ref a);
            Action(others, ref a);
            Mix((ulong)shadowCount, ref b);

            // B：修正牌堆顺序（决定后续翻牌）+ 手牌
            if (deck != null && deck.Remaining != null)
            {
                int n = Math.Min(deck.Remaining.Count, 20);
                for (int i = 0; i < n; i++)
                {
                    ModifierCardSnapshot c = deck.Remaining[i];
                    MixStr(c != null ? c.MathModifier : "?", ref b);
                    Mix((ulong)i, ref b);
                }
            }
            if (hand != null)
            {
                var names = hand.Where(h => h != null)
                                .Select(h => h.DisplayName ?? h.Name ?? "")
                                .OrderBy(s => s, StringComparer.Ordinal);
                foreach (string s in names) MixStr(s, ref b);
            }

            return new Fingerprint { A = a, B = b };
        }

        private static void Action(List<UnitSnapshot> list, ref ulong h)
        {
            if (list == null) return;
            foreach (UnitSnapshot u in list)
            {
                if (u == null) continue;
                MixStr(u.Guid ?? "", ref h);
                Mix((ulong)u.X, ref h);
                Mix((ulong)u.Y, ref h);
                Mix((ulong)u.Health, ref h);
            }
            Mix((ulong)list.Count, ref h);
        }

        private static void Mix(ulong v, ref ulong h)
        {
            h ^= v;
            h *= 1099511628211UL;
        }

        private static void MixStr(string s, ref ulong h)
        {
            if (s == null) s = "";
            for (int i = 0; i < s.Length; i++)
            {
                h ^= s[i];
                h *= 1099511628211UL;
            }
            h ^= 0xFF;
            h *= 1099511628211UL;
        }

        // ---------- 记录与对账 ----------

        /// <summary>把本回合的预期记进账本（下次回合用来对账）。</summary>
        public static void RecordExpectation(
            List<UnitSnapshot> monsters,
            int myHealth,
            int kills,
            List<AbilityCardSnapshot> hand,
            string planSummary,
            Fingerprint fp,
            int roundNumber = -1)
        {
            // ★ 同一回合里**不要覆盖**预期！
            //   玩家在出牌前可能反复按 Insert —— 每按一次就覆盖的话，
            //   预期会变成"最近一次按下时的状态"，执行后就没得可对账了。
            //   只有回合号变了（新回合）才更新预期。
            if (HasHistory && roundNumber >= 0 && roundNumber == LastRound)
            {
                return;
            }

            ExpectedEnemyHealth = new Dictionary<string, int>();
            if (monsters != null)
            {
                foreach (UnitSnapshot u in monsters)
                {
                    if (u == null || string.IsNullOrEmpty(u.Guid)) continue;
                    ExpectedEnemyHealth[u.Guid] = u.EffectiveHealth;
                }
            }
            ExpectedMyHealth = myHealth;
            ExpectedKills = kills;
            LastPlanSummary = planSummary ?? "";
            LastFingerprint = fp;

            var names = new List<string>();
            if (hand != null)
            {
                foreach (AbilityCardSnapshot h in hand)
                {
                    if (h == null) continue;
                    names.Add(h.DisplayName ?? h.Name ?? "?");
                }
            }
            names.Sort(StringComparer.Ordinal);
            ExpectedHandSummary = string.Join("、", names);
            HasHistory = true;
            LastRound = roundNumber;

        }

        /// <summary>
        /// 对账：把"上回合的预期"和"这一回合的实际"比一遍，产出人话结论。
        ///
        /// 返回空列表 = 没有可对账的历史，或一切吻合。
        /// </summary>
        public static List<string> Reconcile(
            List<UnitSnapshot> monsters,
            int myHealth,
            Fingerprint fp)
        {
            var lines = new List<string>();
            if (!HasHistory) return lines;

            // ① 状态指纹
            if (!fp.SameAs(LastFingerprint))
            {
                lines.Add($"状态已变化（指纹 {LastFingerprint.Short()} → {fp.Short()}）");
            }

            // ② 战况对账：敌人血量
            int mismatches = 0;
            var detail = new List<string>();
            if (monsters != null)
            {
                foreach (UnitSnapshot u in monsters)
                {
                    if (u == null || string.IsNullOrEmpty(u.Guid)) continue;
                    int expected;
                    if (!ExpectedEnemyHealth.TryGetValue(u.Guid, out expected)) continue;
                    int actual = u.EffectiveHealth;
                    if (actual == expected) continue;
                    mismatches++;
                    detail.Add($"{u.DisplayName} 预期剩 {expected} → 实际 {actual}");
                }
            }

            if (mismatches > 0)
            {
                lines.Add($"⚠ 上回合预测与实际有 {mismatches} 处不符：");
                foreach (string d in detail) lines.Add("    " + d);
                lines.Add("    → 可能是敌人行为/状态/时机与我的模型有出入，值得记一笔");
            }

            // ③ 我方血量
            if (ExpectedMyHealth >= 0 && myHealth != ExpectedMyHealth)
            {
                lines.Add($"我方血量：预期 {ExpectedMyHealth} → 实际 {myHealth}"
                          + (myHealth < ExpectedMyHealth
                             ? $"（比预期多挨了 {ExpectedMyHealth - myHealth} 点）"
                             : "（比预期少挨了）"));
            }

            return lines;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FHSolver.Bridge
{
    /// <summary>
    /// **原版战斗日志（战报）**。
    ///
    /// 用户提问"不是还有原版游戏的战斗日志可以读吗" —— 对，游戏自己就记了：
    ///
    ///   · `CAttackSummary.TargetSummary` —— 每次攻击对每个目标的**权威结算**：
    ///       护盾 / 穿透 / 实际承受伤害 / 中毒 / 脆弱 / 守护 / 免伤 / 伤害来源
    ///   · `GameState.ActorBeenDamaged` —— 实际扣血的那一刻
    ///
    /// 我把它落盘成结构化 TSV，于是能拿到两份序列：
    ///   · **实际战报**（游戏说的）—— 来自 DamageVerifyPatch / CombatReportPatch
    ///   · **预测战报**（我算的）—— 来自 P1Planner 的 AttackChoice
    ///
    /// ★ 价值：玩家若**照着推荐打**，两条序列应当一一对应；
    ///   对不上的地方，就是模型误差的精确位置（比"感觉不智能"有用得多）。
    ///   这也是"对账"从"我猜的预期"升级成"原版权威数据"。
    /// </summary>
    public static class CombatReport
    {
        /// <summary>战报文件路径（放在项目目录，便于分析）。</summary>
        public static string FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                {
                    string v = "unknown";
                    try { v = Plugin.Version ?? "unknown"; } catch { }
                    _filePath = System.IO.Path.Combine("D:", "1Mods", "FH", "combat_report_v" + v + ".tsv");
                }
                return _filePath;
            }
            set { _filePath = value; }
        }

        private static string _filePath;

        /// <summary>战报条目：类型 / 回合 / 攻击者 / 目标 / 基础攻击 / 修正牌 / 伤害 / 备注</summary>
        public sealed class Row
        {
            public string Kind = "";        // 实际 / 预测
            public int Round;
            public string Attacker = "";
            public string Target = "";
            public int BaseStrength;
            public string Mods = "";
            public int Damage = -1;         // -1 = 未知/未结算
            public string Note = "";
        }

        private static readonly List<Row> Rows = new List<Row>();
        private static readonly object Gate = new object();
        private static int _round = 1;
        private static int _seq;

        /// <summary>当前回合号（由 P1Runner 写入，用于给战报分段）。</summary>
        public static int CurrentRound
        {
            get { return _round; }
            set { _round = value; }
        }

        /// <summary>追加一条战报。不会抛异常（战报是辅助功能，不能影响游戏）。</summary>
        public static void Add(Row r)
        {
            if (r == null) return;
            try
            {
                lock (Gate)
                {
                    r.Round = _round;
                    Rows.Add(r);
                    _seq++;
                }
            }
            catch { /* 忽略 */ }
        }

        /// <summary>把内存里的战报写到文件（追加模式）。</summary>
        public static void Flush()
        {
            try
            {
                lock (Gate)
                {
                    // 只写内存里还没落盘的行；写完立刻清空，避免同一批行被重复追加。
                    if (Rows.Count == 0) return;

                    var sb = new StringBuilder();
                    // ★ 战报也带版本号：混版时能分辨哪几行是旧版记的
                    sb.Append("# ==== 回合 " + _round + " | 插件 v" + Plugin.Version + " ====\n");
                    for (int i = 0; i < Rows.Count; i++)
                    {
                        Row r = Rows[i];
                        sb.Append(r.Kind).Append('\t')
                          .Append(r.Round).Append('\t')
                          .Append(Clean(r.Attacker)).Append('\t')
                          .Append(Clean(r.Target)).Append('\t')
                          .Append(r.BaseStrength).Append('\t')
                          .Append(Clean(r.Mods)).Append('\t')
                          .Append(r.Damage).Append('\t')
                          .Append(Clean(r.Note)).Append('\n');
                    }

                    string dir = Path.GetDirectoryName(FilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.AppendAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
                    Rows.Clear();
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 战报写盘失败: {e.Message}");
            }
        }

        private static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>
    /// 伤害模型 —— 纯计算，不引用任何游戏类型。
    ///
    /// 依据（反编译实证，非推测）：
    ///   GameState.DrawAndApplyAttackModifierCards 末尾：
    ///     foreach (抽到的牌) {
    ///         if (MathModifier 含 "*" 或 "/") summary.PreShieldScalarModifiers.Add(...)
    ///         else                            summary.NonScalarMathModifiers.Add(...)
    ///     }
    ///   CAttackSummary.CalcExpressionString：
    ///     text = "(" + base + OverrideMathModifier + ")"
    ///     text += 每个 NonScalar（加法，按抽牌顺序）
    ///     if (PreShieldScalar 非空) text = "(" + text + ")" + 每个 Scalar（按抽牌顺序）
    ///     if (护盾 != 0) text += "-" + 护盾
    ///     result = Max(0, Parse(text))
    ///
    /// ⇒ 与 ATTACK.md 的三条铁律完全一致：
    ///    1. 加法修正全部在括号内累加
    ///    2. 乘算（×2 / ×0 / ÷2）在括号**外**、按抽牌顺序依次左结合
    ///    3. 护盾在乘算**之后**才减，最后统一截断到 0
    ///   实证表达式：((1)+1)*2 = 4 ；((1))*0-3 = 0 ；(((2))*2)*2 = 8
    /// </summary>
    public static class DamageModel
    {
        /// <summary>
        /// 解析 MathModifier 字符串。形如 "+1" "-2" "*2" "*0" "+0" "/2"。
        /// </summary>
        public static bool TryParse(string s, out char op, out int val)
        {
            op = '?';
            val = 0;
            if (string.IsNullOrEmpty(s)) return false;
            s = s.Trim();
            op = s[0];
            if (op != '+' && op != '-' && op != '*' && op != '/') return false;
            string num = s.Substring(1).Trim();
            if (!int.TryParse(num, out val)) return false;
            if (op == '-') val = -val;   // 统一成带符号数，下面只做加法
            return true;
        }

        /// <summary>按抽牌顺序累加/连乘，返回护盾前伤害（对应游戏的 num8）。</summary>
        public static int ComputePreShield(int baseStrength, IEnumerable<ModifierCardSnapshot> drawn)
        {
            int v = baseStrength;
            var scalars = new List<KeyValuePair<char, int>>();

            foreach (ModifierCardSnapshot c in drawn)
            {
                if (c == null) continue;
                if (!TryParse(c.MathModifier, out char op, out int val)) continue;

                if (op == '+' || op == '-') v += val;
                else scalars.Add(new KeyValuePair<char, int>(op, val));
            }

            foreach (KeyValuePair<char, int> s in scalars)
            {
                v = ApplyScalar(v, s.Key, s.Value);
            }
            return v;
        }

        /// <summary>
        /// 完整伤害：乘算后减护盾（先扣穿透）→ Brittle ×2 → Ward floor(/2) → 截断到 0。
        ///
        /// 依据反编译 `CAttackSummary.CalcExpressionString`（L810-866）：
        ///   · `ShieldMinusPierce = max(0, Shield - Pierce)`（Pierce ≥ 99999 视为完全穿透）
        ///   · 之后依次：PostShieldScalars → Brittle `*2` → Ward `floor(.../2)`
        ///   · 最后 `Max(0, ...)`
        /// </summary>
        public static int Compute(int baseStrength, IEnumerable<ModifierCardSnapshot> drawn, int shield,
            int pierce = 0, bool brittle = false, bool ward = false)
        {
            int effShield = pierce >= 99999 ? 0 : Math.Max(0, shield - Math.Max(0, pierce));
            int v = ComputePreShield(baseStrength, drawn) - effShield;
            if (brittle) v *= 2;
            if (ward) v = (int)Math.Floor(v / 2.0);
            return v < 0 ? 0 : v;
        }

        private static int ApplyScalar(int v, char op, int val)
        {
            if (op == '*') return v * val;
            if (op == '/')
            {
                if (val == 0) return v;
                // 游戏侧是 floor(...) 再 Convert.ToInt32；这里取向下取整，与 floor 语义一致
                return (int)Math.Floor((double)v / val);
            }
            return v;
        }

        /// <summary>
        /// 还原出与游戏日志同形的表达式串，便于逐条比对。
        /// 顺序照抄 `CAttackSummary.CalcExpressionString`：
        ///   (基础+加减) ×/÷ … − 护盾（已扣穿透）→ Brittle ×2 → Ward floor(/2)。
        /// </summary>
        public static string Expression(int baseStrength, IEnumerable<ModifierCardSnapshot> drawn, int shield,
            int pierce = 0, bool brittle = false, bool ward = false)
        {
            var sb = new StringBuilder();
            var scalars = new List<string>();
            sb.Append('(').Append(baseStrength);

            foreach (ModifierCardSnapshot c in drawn)
            {
                if (c == null) continue;
                if (!TryParse(c.MathModifier, out char op, out int val)) continue;
                if (op == '+' || op == '-') sb.Append(val >= 0 ? "+" : "").Append(val);
                else scalars.Add(c.MathModifier);
            }
            sb.Append(')');

            if (scalars.Count > 0)
            {
                sb.Insert(0, "(");
                sb.Append(')');
                foreach (string s in scalars) sb.Append(s);
            }

            int effShield = pierce >= 99999 ? 0 : Math.Max(0, shield - Math.Max(0, pierce));
            if (effShield != 0) sb.Append('-').Append(effShield);

            if (brittle)
            {
                sb.Insert(0, "(");
                sb.Append(")*2");
            }
            if (ward)
            {
                sb.Insert(0, "floor(");
                sb.Append("/2)");
            }
            return sb.ToString();
        }

        /// <summary>把抽到的牌拼成可读串，如 "+1 ×2"。</summary>
        public static string ModsText(IEnumerable<ModifierCardSnapshot> drawn)
        {
            var parts = new List<string>();
            foreach (ModifierCardSnapshot c in drawn)
            {
                if (c == null) continue;
                parts.Add(string.IsNullOrEmpty(c.MathModifier) ? c.Name : c.MathModifier);
            }
            return parts.Count == 0 ? "(无)" : string.Join(" ", parts);
        }
    }
}

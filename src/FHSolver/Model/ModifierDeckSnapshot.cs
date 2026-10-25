using System.Collections.Generic;
using System.Linq;

namespace FHSolver.Model
{
    /// <summary>
    /// 一副攻击修正牌堆的只读快照。
    /// Remaining 的顺序就是翻牌顺序：Remaining[0] 即下一次攻击会翻出的牌。
    /// </summary>
    public sealed class ModifierDeckSnapshot
    {
        public string OwnerLabel;
        public List<ModifierCardSnapshot> Remaining = new List<ModifierCardSnapshot>();
        public List<ModifierCardSnapshot> Discarded = new List<ModifierCardSnapshot>();
        public bool ShufflePending;

        /// <summary>下一次攻击会翻出的牌；牌堆为空时返回 null。</summary>
        public ModifierCardSnapshot NextCard => Remaining.Count > 0 ? Remaining[0] : null;

        /// <summary>接下来 n 次攻击会依次翻出的牌（用于 AoE / 多目标推演）。</summary>
        public List<ModifierCardSnapshot> Peek(int n)
        {
            return Remaining.Take(n).ToList();
        }

        public string ToDeckString(int maxCards = 20)
        {
            var parts = Remaining.Take(maxCards).Select(c => c.ToString());
            string s = string.Join(" ", parts);
            if (Remaining.Count > maxCards) s += $" …(+{Remaining.Count - maxCards})";
            return s;
        }

        public string ToSummaryString()
        {
            return $"{OwnerLabel}: 剩{Remaining.Count} 弃{Discarded.Count}"
                 + $" 待洗={ShufflePending}"
                 + $" 下一张={(NextCard != null ? NextCard.ToString() : "无")}";
        }
    }
}

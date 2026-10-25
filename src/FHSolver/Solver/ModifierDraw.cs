using System.Collections.Generic;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>与 ScenarioRuleLibrary 的 EAdvantageStatuses 对齐。</summary>
    public enum AdvStatus
    {
        None = 0,
        Advantage = 1,
        Disadvantage = 2,
    }

    /// <summary>
    /// 模拟一次攻击的抽牌过程 —— 纯计算，不引用游戏类型。
    ///
    /// 依据 CActor.DrawAttackModifierCards：
    ///   while (card == null || card.Rolling) card = Draw();   // Rolling 的牌进 list3，继续翻
    ///   Advantage    : 再 Draw() 一张 → ChooseCardBasedOnAdvantage → 结果 = list3 + 选中者
    ///   Disadvantage : 再 Draw() 一张 → ChooseCardBasedOnAdvantage → 结果 = [选中者]（list3 丢弃）
    ///   None         : 结果 = list3 + 终止牌
    ///
    /// 注意：牌堆本身是确定序列（MODIFIER.md 已实证 354/354），
    /// 所以这里能算出「本次攻击确切会用到哪几张牌」，而不是期望值。
    /// </summary>
    public static class ModifierDraw
    {
        /// <summary>
        /// 从 remaining 的 offset 位置开始模拟一次攻击抽牌。
        /// </summary>
        /// <param name="remaining">牌堆剩余序列（顺序即未来翻牌顺序）</param>
        /// <param name="offset">本次攻击开始前已消耗的张数</param>
        /// <param name="adv">优/劣势</param>
        /// <param name="baseStrength">基础攻击力（优劣势二选一时按实际伤害比较）</param>
        /// <param name="consumed">本次攻击实际从牌堆顶部拿走的张数</param>
        /// <returns>参与伤害计算的牌；牌堆不足（触发洗牌）时返回 null</returns>
        public static List<ModifierCardSnapshot> Simulate(
            List<ModifierCardSnapshot> remaining,
            int offset,
            AdvStatus adv,
            int baseStrength,
            out int consumed)
        {
            consumed = 0;
            if (remaining == null) return null;

            int i = offset;
            var rolling = new List<ModifierCardSnapshot>();
            ModifierCardSnapshot terminator = null;

            // 主链：一直翻到非 Rolling
            while (i < remaining.Count)
            {
                ModifierCardSnapshot c = remaining[i++];
                if (c != null && c.Rolling) rolling.Add(c);
                else { terminator = c; break; }
            }
            if (terminator == null) return null;   // 牌堆见底 → 会触发洗牌，顺序不可知

            if (adv == AdvStatus.None)
            {
                consumed = i - offset;
                var result = new List<ModifierCardSnapshot>(rolling);
                result.Add(terminator);
                return result;
            }

            // 优/劣势：再翻一张（只翻一张，不再 rolling 循环）
            if (i >= remaining.Count) return null;
            ModifierCardSnapshot second = remaining[i++];
            consumed = i - offset;

            bool takeFirst = IsBetter(terminator, second, baseStrength, adv);

            if (adv == AdvStatus.Advantage)
            {
                // 结果 = 滚动态 + 选中者
                var result = new List<ModifierCardSnapshot>(rolling);
                result.Add(takeFirst ? terminator : second);
                return result;
            }
            // 劣势：滚动态被丢弃，只留选中者
            return new List<ModifierCardSnapshot> { takeFirst ? terminator : second };
        }

        /// <summary>
        /// 优劣势二选一。优势取伤害高者，劣势取伤害低者。
        /// 平局时的游戏侧 tie-break 会看 HasNegativeEffect / HasAdditionalEffect，
        /// 这里没有那两个字段，按「取第一张」近似 —— 已在调用处标注为近似。
        /// </summary>
        private static bool IsBetter(ModifierCardSnapshot first, ModifierCardSnapshot second,
            int baseStrength, AdvStatus adv)
        {
            int d1 = DamageModel.ComputePreShield(baseStrength, new List<ModifierCardSnapshot> { first });
            int d2 = DamageModel.ComputePreShield(baseStrength, new List<ModifierCardSnapshot> { second });

            if (d1 == d2) return true;
            return adv == AdvStatus.Advantage ? d1 > d2 : d1 < d2;
        }
    }
}

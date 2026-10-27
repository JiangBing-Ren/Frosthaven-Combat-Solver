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
        /// 优/劣势二选一 —— **逐条复刻游戏 `CActor.ChooseCardBasedOnAdvantage`**（v1.35）。
        ///
        /// 反编译原文（decompiled/SRL/ScenarioRuleLibrary/CActor.cs L4730）：
        ///   num  = Parse("100" + MathModifier)          ← 卡片"数值"（×2=200 / +2=102 / ×0=0）
        ///   num2 = Parse(attackStrength + MathModifier) ← 实际伤害
        ///   优势：伤害不同 → 取高；相同 → 先比负面效果（有负面者劣），
        ///         再比附加效果（有附加者优），最后比卡片数值，再相同取第一张。
        ///   劣势：`(实际伤害更低 && 第二张数值 != 0) || 第一张数值 == 0` → 取第一张；
        ///         伤害相同 → 先比负面效果（有负面者**优**——劣势要更差），
        ///         再比附加效果（有附加者劣），最后取第一张。
        ///
        /// ⚠ 之前只按伤害比较、平局取第一张 —— 平局（如 ×2 vs +2 在基础 2 时）
        ///   会选错牌，预测伤害与翻牌一起错。
        /// </summary>
        private static bool IsBetter(ModifierCardSnapshot first, ModifierCardSnapshot second,
            int baseStrength, AdvStatus adv)
        {
            int d1 = DamageModel.ComputePreShield(baseStrength, new List<ModifierCardSnapshot> { first });
            int d2 = DamageModel.ComputePreShield(baseStrength, new List<ModifierCardSnapshot> { second });

            int v1 = CardValue(first);
            int v2 = CardValue(second);

            bool neg1 = first != null && first.HasNegativeEffect;
            bool neg2 = second != null && second.HasNegativeEffect;
            bool add1 = first != null && first.HasAdditionalEffect;
            bool add2 = second != null && second.HasAdditionalEffect;

            if (adv == AdvStatus.Advantage)
            {
                if (d1 == d2)
                {
                    if (neg1 && neg2) return true;
                    if (neg1) return false;
                    if (neg2) return true;
                    if (add1 && add2) return true;
                    if (add1) return true;
                    if (add2) return false;
                    if (v1 == v2) return true;
                    return v1 > v2;
                }
                return d1 > d2;
            }

            // 劣势
            if ((d1 < d2 && v2 != 0) || v1 == 0) return true;
            if (d1 == d2)
            {
                if (neg1 && neg2) return true;
                if (neg1) return true;
                if (neg2) return false;
                if (add1 && add2) return true;
                if (add1) return false;
                return true;      // 游戏原文：第二张有没有附加效果都不改变结果
            }
            return false;
        }

        /// <summary>卡片"数值" = Parse("100" + MathModifier)，用于优势平局时的最终 tie-break。</summary>
        private static int CardValue(ModifierCardSnapshot c)
        {
            if (c == null) return 0;
            if (!DamageModel.TryParse(c.MathModifier, out char op, out int val)) return 0;
            switch (op)
            {
                case '+':
                case '-':
                    return 100 + val;          // '-' 已在 TryParse 里折成负数
                case '*':
                    return 100 * val;
                case '/':
                    return val == 0 ? 0 : 100 / val;
                default:
                    return 0;
            }
        }
    }
}

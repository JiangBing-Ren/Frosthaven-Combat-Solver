using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHSolver.Bridge
{
    /// <summary>
    /// M1.5：牌堆预测正确性验证。
    ///
    /// 挂 CActor.DrawAttackModifierCard（private static）：
    ///   - Prefix  记录 current[0] 作为「预测」
    ///   - Postfix 拿返回值作为「实际」，逐次断言两者一致
    ///
    /// 判定标准：连续 20 次全中即 M1.5 通过。
    ///
    /// 两种必须排除的局：
    /// 1. current.Count == 0 时会先洗牌再取 [0]，洗牌后顺序静态不可知 → 记 ShuffleSkipped。
    /// 2. 返回值可能是副本：若 actor.TreatFirstAttackModifierAsRolling()，
    ///    方法内会 CreateRollingAttackModifierCopy 替换返回值，引用不等但 Name 相同
    ///    → 因此按 Name 比对，不按引用比对。
    ///
    /// 实测机制（CActor.cs:4630 起）：
    ///   card = current[0] → discarded.Add(card) → current.Remove(card)
    ///   Curse / Bless / Persistent 三类的牌抽到后不进弃牌堆，而是永久移出牌堆。
    /// </summary>
    internal static class ModifierPredictPatch
    {
        internal static int Total;
        internal static int Hit;
        internal static int Miss;
        internal static int ShuffleSkipped;

        /// <summary>连续命中计数，任一失配清零。达到 20 即判定通过。</summary>
        internal static int Streak;

        private static string _predictedName;
        private static bool _shuffleCase;

        internal static string Summary =>
            $"命中{Hit}/{Total} 不符{Miss} 洗牌跳过{ShuffleSkipped} 连续{Streak}";

        public static void Prefix(List<AttackModifierYMLData> current)
        {
            try
            {
                if (current != null && current.Count > 0)
                {
                    _predictedName = current[0]?.Name;
                    _shuffleCase = false;
                }
                else
                {
                    _predictedName = null;
                    _shuffleCase = true;
                }
            }
            catch
            {
                _predictedName = null;
                _shuffleCase = true;
            }
        }

        public static void Postfix(AttackModifierYMLData __result)
        {
            try
            {
                if (_shuffleCase)
                {
                    ShuffleSkipped++;
                    return;
                }

                string actual = __result?.Name;
                Total++;

                if (_predictedName == actual)
                {
                    Hit++;
                    Streak++;
                    Plugin.Log?.LogInfo($"[FHSolver] 抽牌#{Total}: 预测={_predictedName} 实际={actual}"
                        + $" ✔ 命中{Hit}/{Total} 连续{Streak}");
                }
                else
                {
                    Miss++;
                    Streak = 0;
                    Plugin.Log?.LogWarning($"[FHSolver] 抽牌#{Total}: 预测={_predictedName}"
                        + $" 实际={actual} ✘ 不符（累计不符 {Miss}）");
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 预测断言异常: {e.Message}");
            }
        }

        public static void Install(Harmony harmony)
        {
            MethodInfo m = typeof(CActor).GetMethod("DrawAttackModifierCard",
                BindingFlags.Static | BindingFlags.NonPublic);

            if (m == null)
            {
                Plugin.Log?.LogError("[FHSolver] 找不到 CActor.DrawAttackModifierCard，M1.5 无法验证");
                return;
            }

            harmony.Patch(m,
                prefix: new HarmonyMethod(typeof(ModifierPredictPatch).GetMethod(nameof(Prefix),
                    BindingFlags.Static | BindingFlags.Public)),
                postfix: new HarmonyMethod(typeof(ModifierPredictPatch).GetMethod(nameof(Postfix),
                    BindingFlags.Static | BindingFlags.Public)));

            Plugin.Log?.LogInfo($"[FHSolver] 已挂载抽牌预测断言: CActor.DrawAttackModifierCard"
                + $"（参数 {m.GetParameters().Length} 个）");
        }
    }
}

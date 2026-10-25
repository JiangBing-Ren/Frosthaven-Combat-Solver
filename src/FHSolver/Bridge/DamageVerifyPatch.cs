using System;
using System.Collections.Generic;
using System.Reflection;
using FHSolver.Model;
using FHSolver.Solver;
using HarmonyLib;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHSolver.Bridge
{
    /// <summary>
    /// P1 的正确性验证钩子（v0.6.0）。
    ///
    /// 挂 GameState.DrawAndApplyAttackModifierCards —— 这是伤害计算的咽喉：
    ///   - 进它之前牌堆还没动 → Prefix 能拿到"抽牌前"的完整牌序
    ///   - 它的返回值 num8 = CalcExpressionString(calcShield:false) = **护盾前**伤害
    ///   - 它的 out 参数 outAttackModifierCards = 本次攻击真正用到的牌
    ///
    /// 于是可以在这里做一次无死角的对拍：
    ///   我方预测 = DamageModel.ComputePreShield(游戏给的真实 attackStrength, 我 peek 到的牌)
    ///   实际结果 = 游戏返回值 __result
    /// 两侧一比，就把"牌序预测""rolling 处理""优劣势处理""加减乘顺序"四件事一次性验掉。
    ///
    /// 注意它**不验证**基础攻击力 —— attackStrength 是游戏直接给的。
    /// 基础攻击力那条线单独用下面的 `基础攻=${ability.Strength} 实际=${attackStrength}` 观察 buff 差。
    /// </summary>
    internal static class DamageVerifyPatch
    {
        private static int _attackStrength;
        private static AdvStatus _adv;
        private static List<ModifierCardSnapshot> _deckBefore;
        private static int _abilityStrength;
        private static bool _armed;
        private static CAbilityAttack attackAbilityRef;
        private static CActor _actor;



        public static int Hit;
        public static int Miss;
        public static int Streak;
        public static int BestStreak;
        public static int Skipped;      // 牌堆见底（会洗牌）/ 无法取到牌堆
        public static int NonPlayer;    // 非玩家单位（怪物），不计入统计

        public static string Summary =>
            $"伤害对拍 {Hit}中/{Miss}不符 连续{Streak} 最长{BestStreak} 跳过{Skipped} 非玩家{NonPlayer}";

        public static void Install(Harmony harmony)
        {
            try
            {
                Type gs = typeof(GameState);
                MethodInfo m = AccessTools.Method(gs, "DrawAndApplyAttackModifierCards");
                if (m == null)
                {
                    Plugin.Log?.LogError("[FHSolver] 找不到 GameState.DrawAndApplyAttackModifierCards");
                    return;
                }

                harmony.Patch(m,
                    prefix: new HarmonyMethod(typeof(DamageVerifyPatch).GetMethod(nameof(Prefix),
                        BindingFlags.Static | BindingFlags.Public)),
                    postfix: new HarmonyMethod(typeof(DamageVerifyPatch).GetMethod(nameof(Postfix),
                        BindingFlags.Static | BindingFlags.Public)));

                Plugin.Log?.LogInfo($"[FHSolver] 已挂载伤害对拍: GameState.DrawAndApplyAttackModifierCards"
                    + $"（参数 {m.GetParameters().Length} 个，返回 {m.ReturnType.Name}）");
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"[FHSolver] 挂载伤害对拍失败: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void Prefix(CAbilityAttack attackAbility, CActor actor,
            int attackStrength, EAdvantageStatuses advantageStatus)
        {
            try
            {
                _armed = false;
                _attackStrength = attackStrength;
                _adv = ToAdv(advantageStatus);
                _abilityStrength = attackAbility != null ? attackAbility.Strength : -1;
                attackAbilityRef = attackAbility;
                _actor = actor;



                _deckBefore = SnapshotDeck(actor);
                _armed = _deckBefore != null;
            }
            catch (Exception e)
            {
                _armed = false;
                Plugin.Log?.LogWarning($"[FHSolver] 伤害对拍 Prefix 异常: {e.Message}");
            }
        }

        public static void Postfix(int __result, List<AttackModifierYMLData> outAttackModifierCards)
        {
            try
            {
                if (!_armed) return;

                List<ModifierCardSnapshot> drawn = ModifierDraw.Simulate(
                    _deckBefore, 0, _adv, _attackStrength, out int consumed);

                if (drawn == null || outAttackModifierCards == null)
                {
                    Skipped++;
                    return;
                }

                int predicted = DamageModel.ComputePreShield(_attackStrength, drawn);

                var actualNames = new List<string>();
                foreach (AttackModifierYMLData c in outAttackModifierCards)
                {
                    if (c != null) actualNames.Add(c.Name);
                }
                var predNames = new List<string>();
                foreach (ModifierCardSnapshot c in drawn) predNames.Add(c.Name);

                bool cardsMatch = actualNames.Count == predNames.Count;
                if (cardsMatch)
                {
                    for (int i = 0; i < actualNames.Count; i++)
                    {
                        if (actualNames[i] != predNames[i]) { cardsMatch = false; break; }
                    }
                }

                bool ok = cardsMatch && predicted == __result;

                if (ok)
                {
                    Hit++;
                    Streak++;
                    if (Streak > BestStreak) BestStreak = Streak;
                }
                else
                {
                    Miss++;
                    Streak = 0;
                }

                // ★ 记录到原版战报（实际侧）：攻击者/基础攻击/修正牌/护盾前伤害/目标
                try
                {
                    var rep = new CombatReport.Row
                    {
                        Kind = "实际",
                        Attacker = _actor != null ? _actor.ActorLocKey() : "?",

                        BaseStrength = _attackStrength,
                        Mods = string.Join(" ", actualNames),
                        Damage = __result,
                        Note = "护盾前",
                    };
                    if (attackAbilityRef != null)
                    {
                        var sums = attackAbilityRef.TotalTargetSummaries;
                        if (sums != null && sums.Count > 0)
                        {
                            var names = new List<string>();
                            foreach (var ts in sums)
                            {
                                if (ts == null || ts.Actor == null) continue;
                                names.Add(ts.Actor.ActorLocKey()
                                    + (ts.Shield > 0 ? $"(盾{ts.Shield})" : "")
                                    + (ts.Pierce > 0 ? $"(穿{ts.Pierce})" : ""));
                            }
                            if (names.Count > 0) rep.Target = string.Join("+", names);
                        }
                    }
                    CombatReport.Add(rep);
                    // ★ 立刻落盘：战斗结束（玩家不再按 Insert）时实际战报才不丢
                    CombatReport.Flush();
                }
                catch { /* 战报失败不影响游戏 */ }

                // 每次都打，便于事后逐条核对；前面带 OK / XX
                Plugin.Log?.LogInfo($"[FHSolver] 对拍[{(ok ? "OK" : "XX")}]"

                    + $" 基础攻(卡)={_abilityStrength} 实际基础={_attackStrength} 差={_attackStrength - _abilityStrength}"
                    + $" 优劣势={_adv}"
                    + $" 预测牌=[{string.Join(" ", predNames)}]"
                    + $" 实际牌=[{string.Join(" ", actualNames)}]"
                    + $" 预测伤害={predicted} 实际(护盾前)={__result}");
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 伤害对拍 Postfix 异常: {e.Message}");
            }
        }

        private static AdvStatus ToAdv(EAdvantageStatuses s)
        {
            switch (s)
            {
                case EAdvantageStatuses.Advantage: return AdvStatus.Advantage;
                case EAdvantageStatuses.Disadvantage: return AdvStatus.Disadvantage;
                default: return AdvStatus.None;
            }
        }

        /// <summary>取该单位当前的修正牌堆（抽牌前）。取不到返回 null（该次跳过，不计入统计）。</summary>
        private static List<ModifierCardSnapshot> SnapshotDeck(CActor actor)
        {
            if (actor == null) return null;

            List<AttackModifierYMLData> src = null;

            try
            {
                var p = actor as CPlayerActor;
                if (p != null && p.CharacterClass != null)
                {
                    src = p.CharacterClass.AttackModifierCards;
                }
                else if (actor.Type == CActor.EType.Enemy)
                {
                    NonPlayer++;
                    src = MonsterClassManager.EnemyMonsterAttackModifierDeck?.AttackModifierCards;
                }
                else
                {
                    NonPlayer++;
                }
            }
            catch
            {
                return null;
            }

            if (src == null) return null;

            var list = new List<ModifierCardSnapshot>();
            foreach (AttackModifierYMLData c in src)
            {
                if (c == null) continue;
                list.Add(new ModifierCardSnapshot
                {
                    Name = c.Name,
                    MathModifier = c.MathModifier,
                    Shuffle = c.Shuffle,
                    Rolling = c.Rolling,
                    IsCurse = c.IsCurse,
                    IsBless = c.IsBless,
                });
            }
            return list;
        }
    }
}

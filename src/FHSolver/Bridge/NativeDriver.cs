using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge
{
    /// <summary>
    /// **原生驱动**（学 StS2 的 `PlayerTurnSetupCoordinator`：原生 UI + 反射驱动）。
    ///
    /// FH 没有 StS2 那种"一个 `SetupPlayerTurn` 就能推进"的入口，
    /// 驱动要落在玩家选牌阶段（`CPhaseAction`）上：
    ///
    /// ```
    /// GameState.CurrentAction          → 当前该出的牌（CAction + CBaseCard）
    /// PhaseManager.CurrentPhaseAction  → 玩家已选的半边列表（m_PhaseAbilities）
    ///     每个 CPhaseAbility.m_Ability  ←←← 目标就写在这个 ability 实例上
    ///         m_ActorsToTarget / m_TargetedActor（protected → 必须反射）
    /// ```
    ///
    /// ⚠ 本类目前只做**只读探测**（Probe）：
    ///   不改任何游戏状态。目的是先搞清楚"驱动链路的哪一环缺什么"，
    ///   拿到真机诊断后再决定要不要开自动写入。
    ///   —— v1.26.2 就是因为盲写游戏内部状态吃过亏（那次是文件事故，
    ///      但教训一样：没验证的写入不该上）。
    /// </summary>
    public static class NativeDriver
    {
        private static FieldInfo _fActorsToTarget;
        private static FieldInfo _fTargetedActor;

        private static FieldInfo ActorsToTargetField
        {
            get
            {
                if (_fActorsToTarget == null)
                {
                    _fActorsToTarget = typeof(CAbility).GetField("m_ActorsToTarget",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }
                return _fActorsToTarget;
            }
        }

        private static FieldInfo TargetedActorField
        {
            get
            {
                if (_fTargetedActor == null)
                {
                    _fTargetedActor = typeof(CAbility).GetField("m_TargetedActor",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }
                return _fTargetedActor;
            }
        }

        /// <summary>
        /// 探测当前能不能"原生预选"：返回人话诊断（空列表 = 现在不在可驱动状态）。
        /// </summary>
        public static List<string> Probe(List<string> recommendedCardNames)
        {
            var lines = new List<string>();
            try
            {
                if (!ScenarioManager.IsInTactical)
                {
                    lines.Add("不在战斗中，无法驱动");
                    return lines;
                }

                // ① 当前 action（玩家正在出哪张牌）
                GameState.CurrentActorAction cur = GameState.CurrentAction;

                if (cur == null)
                {
                    lines.Add("GameState.CurrentAction 为空 —— 玩家还没进选牌阶段");
                    return lines;
                }
                CBaseCard baseCard = cur.BaseCard;
                lines.Add($"当前牌：{baseCard?.Name ?? "(无)"}"
                          + $"（{(cur.Action != null ? cur.Action.Abilities?.Count ?? 0 : 0)} 个半边）");

                // ② 选牌阶段的半边列表
                CPhaseAction phase = PhaseManager.CurrentPhaseAction;
                if (phase == null)
                {
                    lines.Add("PhaseManager.CurrentPhaseAction 为空 —— 不在 CPhaseAction 阶段（无法预选半边）");
                    return lines;
                }
                // ⚠ m_PhaseAbilities 是私有的；公开的是 RemainingPhaseAbilities（只读）
                List<CPhaseAction.CPhaseAbility> pabs = phase.RemainingPhaseAbilities;

                lines.Add($"可预选的半边：{(pabs != null ? pabs.Count : 0)} 个");

                // ③ 目标字段可写性（反射目标是否存在）
                bool canTarget = ActorsToTargetField != null || TargetedActorField != null;
                lines.Add(canTarget
                    ? "目标字段可写：m_ActorsToTarget / m_TargetedActor ✓"
                    : "⚠ 找不到目标字段（m_ActorsToTarget / m_TargetedActor）→ 目标预选做不了");
                if (!canTarget)
                {
                    lines.Add("  → 实际可用字段：" + string.Join(", ",
                        typeof(CAbility).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                            .Select(f => f.Name)
                            .Where(n => n.ToLower().Contains("target"))
                            .Take(6)));
                }

                // ④ 推荐能否对上号
                if (recommendedCardNames != null && recommendedCardNames.Count > 0)
                {
                    var have = new List<string>();
                    if (pabs != null)
                    {
                        foreach (CPhaseAction.CPhaseAbility pa in pabs)

                        {
                            CAbility ab = pa?.m_Ability;
                            if (ab == null) continue;
                            have.Add(ab.AbilityType + ":" + (ab.Name ?? ab.GetType().Name));
                        }
                    }
                    lines.Add("当前可选动作：" + string.Join(", ", have.Take(8)));
                    lines.Add("推荐涉及的牌：" + string.Join("、", recommendedCardNames));
                }

                // ⑤ 确认入口：还没找到"玩家点了确认"的程序化入口
                lines.Add("⚠ 尚未找到「确认出牌」的程序化入口（FH 没有 StS2 的 SetupPlayerTurn）");
                lines.Add("   → 目前的原生驱动上限：只能**预选目标**，最后一步仍需玩家点确认");
            }
            catch (Exception e)
            {
                lines.Add("探测异常：" + e.Message);
            }
            return lines;
        }
    }
}

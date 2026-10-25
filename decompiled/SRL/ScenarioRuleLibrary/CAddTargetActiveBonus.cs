using System;

namespace ScenarioRuleLibrary;

public class CAddTargetActiveBonus : CActiveBonus
{
	public CAddTargetActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.BuffTarget)
			{
				if (!(ability is CAbilityAddTarget ability2))
				{
					throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Target active bonus");
				}
				AddBespokeBehaviour(new CAddTargetActiveBonus_BuffTarget(actor, ability2, this));
			}
		}
	}

	public override void Finish()
	{
		base.Finish();
		if (PhaseManager.Phase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Attack && cPhaseAction.CurrentPhaseAbility.m_Ability.CanClearTargets())
		{
			cPhaseAction.CurrentPhaseAbility.m_Ability.ClearTargets();
			cPhaseAction.CurrentPhaseAbility.m_Ability.Restart();
			if (cPhaseAction.CurrentPhaseAbility.m_Ability.CanReceiveTileSelection())
			{
				cPhaseAction.CurrentPhaseAbility.m_Ability.Perform();
			}
		}
	}
}

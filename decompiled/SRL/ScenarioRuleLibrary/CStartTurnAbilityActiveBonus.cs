namespace ScenarioRuleLibrary;

public class CStartTurnAbilityActiveBonus : CActiveBonus
{
	[StateDump(Skip = true)]
	public CAbility AddAbility;

	public CStartTurnAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public bool CanTriggerAbility()
	{
		if (!IsActiveBonusToggledAndNotRestricted(Actor))
		{
			return false;
		}
		bool flag = RequirementsMet(Actor);
		if (Ability.ActiveBonusData.HasBehaviour(EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns) && !flag)
		{
			TriggerAbility();
		}
		return flag;
	}

	public void TriggerAbility()
	{
		if (!IsActiveBonusToggledAndNotRestricted(Actor))
		{
			return;
		}
		if (PhaseManager.CurrentPhase is CPhaseStartTurn || PhaseManager.CurrentPhase is CPhaseAction)
		{
			if (CheckAddAbilityValidity(AddAbility))
			{
				RestrictActiveBonus(Actor);
				if (HasTracker)
				{
					UpdateXPTracker();
					FinishIfNoRemaining();
				}
			}
		}
		else
		{
			Log.ErrorWithReport("Invalid phase for StartTurn Ability");
		}
	}
}

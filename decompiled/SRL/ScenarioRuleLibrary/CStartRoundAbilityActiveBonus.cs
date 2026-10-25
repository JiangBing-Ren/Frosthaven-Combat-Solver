namespace ScenarioRuleLibrary;

public class CStartRoundAbilityActiveBonus : CActiveBonus
{
	[StateDump(Skip = true)]
	public CAbility AddAbility;

	public CStartRoundAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public bool CanTriggerAbility()
	{
		return RequirementsMet();
	}

	public void TriggerAbility()
	{
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
				ToggledBonus = false;
				ToggleLocked = false;
			}
		}
		else
		{
			Log.ErrorWithReport("Invalid phase for StartRound Ability");
		}
	}
}

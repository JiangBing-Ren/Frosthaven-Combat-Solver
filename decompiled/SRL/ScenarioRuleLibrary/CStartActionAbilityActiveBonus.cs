namespace ScenarioRuleLibrary;

public class CStartActionAbilityActiveBonus : CActiveBonus
{
	[StateDump(Skip = true)]
	public CAbility AddAbility;

	public CStartActionAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseAction)
		{
			if (CheckAddAbilityValidity(AddAbility))
			{
				RestrictActiveBonus(Actor);
				if (HasTracker && RequirementsMet(Actor))
				{
					UpdateXPTracker();
					FinishIfNoRemaining();
				}
			}
		}
		else
		{
			Log.ErrorWithReport("Invalid phase for StartAction Ability");
		}
	}
}

namespace ScenarioRuleLibrary;

public class CEndTurnAbilityActiveBonus : CActiveBonus
{
	[StateDump(Skip = true)]
	public CAbility AddAbility;

	public CEndTurnAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseEndTurn || PhaseManager.CurrentPhase is CPhaseAction)
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
			Log.ErrorWithReport("Invalid phase for EndTurn Ability");
		}
	}
}

namespace ScenarioRuleLibrary;

public class CDuringTurnAbilityActiveBonus : CActiveBonus
{
	[StateDump(Skip = true)]
	public CAbility AddAbility;

	public CDuringTurnAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.DuringTurnAbility || behaviour == EActiveBonusBehaviourType.BetweenActionAbilities)
			{
				AddBespokeBehaviour(new CDuringTurnAbilityActiveBonus_TriggerAbility(actor, ability, this, behaviour));
			}
		}
	}

	public void TriggerAbility()
	{
		RestrictActiveBonus(Actor);
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.OnBehaviourTriggered();
		}
		if (HasTracker)
		{
			UpdateXPTracker();
			FinishIfNoRemaining();
		}
	}

	public override bool ShouldRestartAbilityOnToggle(CAbility currentAbility)
	{
		if (currentAbility is CAbilityMove && ToggledBonus)
		{
			return false;
		}
		return base.ShouldRestartAbilityOnToggle(currentAbility);
	}
}

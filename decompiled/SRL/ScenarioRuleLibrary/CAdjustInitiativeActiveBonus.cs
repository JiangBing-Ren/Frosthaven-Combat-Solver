namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus : CActiveBonus
{
	public CAdjustInitiativeActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.AdjustInitiative:
				AddBespokeBehaviour(new CAdjustInitiativeActiveBonus_AdjustInitiative(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.FocusInitiative:
				AddBespokeBehaviour(new CAdjustInitiativeActiveBonus_FocusInitiative(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.SetInitiative:
				AddBespokeBehaviour(new CAdjustInitiativeActiveBonus_SetInitiative(actor, ability, this));
				break;
			}
		}
	}

	public void InitiativeAdjusted()
	{
		if (!IsActiveBonusToggledAndNotRestricted(Actor))
		{
			return;
		}
		RestrictActiveBonus(Actor);
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.OnBehaviourTriggered();
		}
		ToggledBonus = true;
		if (HasTracker)
		{
			UpdateXPTracker();
			FinishIfNoRemaining();
		}
	}

	public override void FinishIfNoRemaining()
	{
		if (Remaining <= 0)
		{
			ToggledBonus = false;
		}
		base.FinishIfNoRemaining();
	}
}

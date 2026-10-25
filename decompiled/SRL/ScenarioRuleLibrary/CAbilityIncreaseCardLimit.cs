namespace ScenarioRuleLibrary;

public class CAbilityIncreaseCardLimit : CAbilityTargeting
{
	public CAbilityIncreaseCardLimit()
		: base(EAbilityType.IncreaseCardLimit)
	{
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			TargetingActor.FindCardWithAbility(this);
			_ = (CPlayerActor)actor;
			CSelectIncreasedCardLimit_MessageData cSelectIncreasedCardLimit_MessageData = new CSelectIncreasedCardLimit_MessageData(AnimOverload, TargetingActor);
			cSelectIncreasedCardLimit_MessageData.m_ActorIncreasingCardLimit = actor;
			cSelectIncreasedCardLimit_MessageData.m_Ability = this;
			ScenarioRuleClient.MessageHandler(cSelectIncreasedCardLimit_MessageData);
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}
}

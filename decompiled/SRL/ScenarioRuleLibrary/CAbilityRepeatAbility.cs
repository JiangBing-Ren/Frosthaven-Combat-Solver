namespace ScenarioRuleLibrary;

public class CAbilityRepeatAbility : CAbilityTargeting
{
	public CAbilityRepeatAbility(EAbilityType abilityType)
		: base(abilityType)
	{
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, TargetingActor);
			}
			else if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find base ability card for ability " + Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
			CActorIsRepeatingAbilities message = new CActorIsRepeatingAbilities(AnimOverload, actor)
			{
				m_RepeatAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		return true;
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityAddAugment : CAbilityTargeting
{
	public CAbilityAddAugment()
		: base(EAbilityType.AddAugment)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingAddAugment_MessageData message = new CActorIsApplyingAddAugment_MessageData(AnimOverload, actorApplying)
		{
			m_AddAugmentAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CApplyToActorAddAugment_MessageData message = new CApplyToActorAddAugment_MessageData(TargetingActor)
			{
				m_AddAugmentAbility = this,
				m_Target = actor
			};
			ScenarioRuleClient.MessageHandler(message);
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard != null)
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
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}
}

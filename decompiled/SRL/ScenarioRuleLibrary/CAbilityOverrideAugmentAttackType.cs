using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityOverrideAugmentAttackType : CAbilityTargeting
{
	[JsonProperty]
	public EAttackType OverrideAttackType;

	public CAbilityOverrideAugmentAttackType(EAttackType overrideAttackType)
		: base(EAbilityType.OverrideAugmentAttackType)
	{
		OverrideAttackType = overrideAttackType;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingOverrideAugmentAttackType_MessageData message = new CActorIsApplyingOverrideAugmentAttackType_MessageData(AnimOverload, actorApplying)
		{
			m_OverrideAugmentAttackTypeAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CApplyToActorOverrideAugmentAttackType_MessageData message = new CApplyToActorOverrideAugmentAttackType_MessageData(TargetingActor)
			{
				m_OverrideAugmentAttackTypeAbility = this,
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

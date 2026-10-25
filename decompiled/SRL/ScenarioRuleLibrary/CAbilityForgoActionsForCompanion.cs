using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityForgoActionsForCompanion : CAbilityTargeting
{
	[JsonProperty]
	public CAbility ForgoTopActionAbility { get; set; }

	[JsonProperty]
	public CAbility ForgoBottomActionAbility { get; set; }

	public CAbilityForgoActionsForCompanion(CAbility forgoTopActionAbility, CAbility forgoBottomActionAbility)
		: base(EAbilityType.ForgoActionsForCompanion)
	{
		ForgoTopActionAbility = forgoTopActionAbility;
		ForgoBottomActionAbility = forgoBottomActionAbility;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Error: Unable to find base ability card for ability " + Name);
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

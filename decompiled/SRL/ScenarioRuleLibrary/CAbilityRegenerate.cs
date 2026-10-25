using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityRegenerate : CAbilityCondition
{
	public CAbilityRegenerate(EConditionDecTrigger decrementTrigger)
		: base(EAbilityType.Regenerate, int.MaxValue, decrementTrigger)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		LogIsApplying(actorsAppliedTo);
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
		bool result = false;
		if (base.ApplyToActor(actor))
		{
			LogApplyToActor(actor);
			AbilityHasHappened = true;
			if (!CAbility.ImmuneToAbility(actor, this))
			{
				actor.ApplyCondition(TargetingActor, CCondition.EPositiveCondition.Regenerate, 0, DecrementTrigger, "", IsMapCondition);
			}
			result = true;
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		return result;
	}

	public override bool IsPositive()
	{
		return true;
	}
}

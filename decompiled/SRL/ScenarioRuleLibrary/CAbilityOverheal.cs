using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityOverheal(int overhealAmount, int duration, EConditionDecTrigger decTrigger) : CAbilityCondition(EAbilityType.Overheal, duration, decTrigger)
{
	public int OverhealAmount = 35;

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
		if (base.ApplyToActor(actor))
		{
			LogApplyToActor(actor);
			AbilityHasHappened = true;
			CActiveBonus cActiveBonus = GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
			if (cActiveBonus != null && cActiveBonus.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				CLogActiveBonus_MessageData message = new CLogActiveBonus_MessageData(AnimOverload, TargetingActor)
				{
					m_ActiveBonus = cActiveBonus,
					m_ActorAppliedTo = actor
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
			return true;
		}
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityBrittle : CAbilityCondition
{
	public CAbilityBrittle()
		: base(EAbilityType.Brittle, int.MaxValue, EConditionDecTrigger.Never)
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
		if (base.ApplyToActor(actor))
		{
			LogApplyToActor(actor);
			AbilityHasHappened = true;
			if (!CAbility.ImmuneToAbility(actor, this) && !actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
			{
				actor.ApplyCondition(TargetingActor, CCondition.ENegativeCondition.Brittle, 0, EConditionDecTrigger.Never, "", IsMapCondition, canGoOverCurseLimit: false, isCauseByAnotherCondition: false, -1, TempDisabledConditions);
				if (m_NegativeConditions.Count > 0)
				{
					ProcessNegativeStatusEffects(actor);
				}
			}
			return true;
		}
		return false;
	}

	public override bool IsPositive()
	{
		return false;
	}
}

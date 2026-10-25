using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityBless : CAbilityCondition
{
	private bool canGoOverLimit;

	public CAbilityBless()
		: base(EAbilityType.Bless, int.MaxValue, EConditionDecTrigger.Never)
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

	public bool ApplyToActor(CActor actor, bool canGoOverBlessLimit)
	{
		canGoOverLimit = canGoOverBlessLimit;
		bool result = ApplyToActor(actor);
		canGoOverLimit = false;
		return result;
	}

	public override bool ApplyToActor(CActor actor)
	{
		bool result = false;
		if (base.ApplyToActor(actor))
		{
			LogApplyToActor(actor);
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.AlreadyHasConditionDamageInstead.HasValue && actor.Tokens.HasKey(CCondition.EPositiveCondition.Bless))
			{
				int health = actor.Health;
				bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(actor, MiscAbilityData.AlreadyHasConditionDamageInstead.Value, checkIfPlayerCanAvoidDamage: true, TargetingActor, this, EAbilityType.Damage);
				if ((actor.Type != CActor.EType.Player || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(actor)
					{
						m_ActorBeingDamaged = actor,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else
			{
				actor.ApplyCondition(TargetingActor, CCondition.EPositiveCondition.Bless, 0, EConditionDecTrigger.Never, "", isMapCondition: false, canGoOverLimit);
				for (int i = 1; i < Strength; i++)
				{
					actor.ApplyCondition(TargetingActor, CCondition.EPositiveCondition.Bless, 0, EConditionDecTrigger.Never, "", isMapCondition: false, canGoOverLimit);
				}
			}
			result = true;
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		if (m_NegativeConditions.Count > 0)
		{
			ProcessNegativeStatusEffects(actor);
		}
		return result;
	}

	public override bool IsPositive()
	{
		return true;
	}
}

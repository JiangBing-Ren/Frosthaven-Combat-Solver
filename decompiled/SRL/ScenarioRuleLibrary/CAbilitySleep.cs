using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilitySleep : CAbilityCondition
{
	public CAbilitySleep()
		: base(EAbilityType.Sleep, int.MaxValue, EConditionDecTrigger.Never)
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
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.AlreadyHasConditionDamageInstead.HasValue && actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
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
				actor.ApplyCondition(TargetingActor, CCondition.ENegativeCondition.Sleep, Duration, DecrementTrigger, "", IsMapCondition, canGoOverCurseLimit: false, isCauseByAnotherCondition: false, -1, TempDisabledConditions);
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

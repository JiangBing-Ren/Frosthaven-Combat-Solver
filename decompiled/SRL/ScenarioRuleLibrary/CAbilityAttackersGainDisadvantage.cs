using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityAttackersGainDisadvantage : CAbilityTargeting
{
	public CAbilityAttackersGainDisadvantage()
		: base(EAbilityType.AttackersGainDisadvantage)
	{
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
			CAttackersGainDisadvantage_MessageData cAttackersGainDisadvantage_MessageData = new CAttackersGainDisadvantage_MessageData(AnimOverload, TargetingActor);
			cAttackersGainDisadvantage_MessageData.m_AttackersGainDisadvantageAbility = this;
			cAttackersGainDisadvantage_MessageData.m_ActorAppliedTo = actor;
			ScenarioRuleClient.MessageHandler(cAttackersGainDisadvantage_MessageData);
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

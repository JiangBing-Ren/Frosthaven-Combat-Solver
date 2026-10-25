using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityPreventDamage : CAbilityTargeting
{
	[JsonProperty]
	private bool m_AttackSourcesOnly;

	public bool AttackSourcesOnly
	{
		get
		{
			return m_AttackSourcesOnly;
		}
		set
		{
			m_AttackSourcesOnly = value;
		}
	}

	public CAbilityPreventDamage(bool attackSourceOnly)
		: base(EAbilityType.PreventDamage)
	{
		m_AttackSourcesOnly = attackSourceOnly;
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
			GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
			CPreventDamage_MessageData message = new CPreventDamage_MessageData(AnimOverload, TargetingActor)
			{
				m_PreventDamageAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
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

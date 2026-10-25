using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityRedirect : CAbilityTargeting
{
	public CAbilityFilterContainer Filter;

	public bool IgnoreLOS;

	public int RedirectRange = 1;

	public CAbilityRedirect()
		: base(EAbilityType.Redirect)
	{
	}

	public void CalcRedirectVars()
	{
		if (MiscVars != null)
		{
			TValue miscVar = GetMiscVar("RedirectFilter");
			if (!miscVar.isUnknown)
			{
				CAbilityFilter item = miscVar.GetObject<CAbilityFilter>();
				Filter = new CAbilityFilterContainer(new List<CAbilityFilter> { item });
			}
			TValue miscVar2 = GetMiscVar("RedirectRange");
			if (!miscVar2.isUnknown)
			{
				RedirectRange = miscVar2.GetInt(null, RedirectRange);
			}
			TValue miscVar3 = GetMiscVar("RedirectIgnoreLOS");
			if (!miscVar3.isUnknown)
			{
				IgnoreLOS = miscVar3.GetBool(null, IgnoreLOS);
			}
		}
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
			CRedirect_MessageData cRedirect_MessageData = new CRedirect_MessageData(AnimOverload, null);
			cRedirect_MessageData.m_RedirectAbility = this;
			cRedirect_MessageData.m_ActorAppliedTo = actor;
			ScenarioRuleClient.MessageHandler(cRedirect_MessageData);
			GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
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

using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAbilityAddCondition : CAbilityTargeting
{
	public CAbilityAddCondition()
		: base(EAbilityType.AddCondition)
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
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Error: Unable to find base ability card for ability " + Name);
			}
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		if (m_NegativeConditions.Count > 0)
		{
			ProcessNegativeStatusEffects(actor);
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool ShouldSortValidActorsInRangeByInitiative()
	{
		return true;
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList, bool onlyCheck = true)
	{
		if (ActiveBonusData != null && ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AddConditionUntilDamaged))
		{
			actorList.RemoveAll((CActor x) => ActiveBonusData.AbilityData.PositiveConditions.Values.Any((CAbility y) => CAbility.ImmuneToAbility(x, y)));
			actorList.RemoveAll((CActor x) => ActiveBonusData.AbilityData.NegativeConditions.Values.Any((CAbility y) => CAbility.ImmuneToAbility(x, y)));
		}
		base.RemoveImmuneActorsFromList(ref actorList, onlyCheck);
	}
}

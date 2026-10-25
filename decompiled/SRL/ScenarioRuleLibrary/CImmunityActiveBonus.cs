using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CImmunityActiveBonus : CActiveBonus
{
	public CImmunityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (ability is CAbilityImmunityTo ability2)
		{
			{
				foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
				{
					switch (behaviour)
					{
					case EActiveBonusBehaviourType.ImmunityToConditionAfterAttackDamage:
						AddBespokeBehaviour(new CImmunityActiveBonus_ImmunityToConditionAfterAttackDamage(actor, ability2, this));
						break;
					case EActiveBonusBehaviourType.ChooseCondition:
						AddBespokeBehaviour(new CImmunityActiveBonus_ChooseCondition(actor, ability2, this));
						break;
					}
				}
				return;
			}
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Immunity active bonus");
	}

	public void RemoveConditionsFromAffectedActors()
	{
		List<CActor> list = new List<CActor>();
		list.AddRange(ValidActorsInRangeOfAura);
		if (!list.Contains(Actor) && (!IsAura || (AuraFilter != null && AuraFilter.IsValidTarget(Actor, Caster, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			list.Add(Actor);
		}
		foreach (CActor item in list)
		{
			List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>(Ability.NegativeConditions.Keys);
			List<CCondition.EPositiveCondition> list3 = new List<CCondition.EPositiveCondition>(Ability.PositiveConditions.Keys);
			if (Ability is CAbilityImmunityTo cAbilityImmunityTo)
			{
				foreach (CAbility.EAbilityType immuneToAbilityType in cAbilityImmunityTo.ImmuneToAbilityTypes)
				{
					if (Enum.TryParse<CCondition.ENegativeCondition>(immuneToAbilityType.ToString(), ignoreCase: false, out var result) && !list2.Contains(result))
					{
						list2.Add(result);
					}
					if (Enum.TryParse<CCondition.EPositiveCondition>(immuneToAbilityType.ToString(), ignoreCase: false, out var result2) && !list3.Contains(result2))
					{
						list3.Add(result2);
					}
				}
			}
			foreach (CCondition.ENegativeCondition item2 in list2)
			{
				if (item2 != CCondition.ENegativeCondition.Curse && item.RemoveNegativeConditionToken(item2, EConditionDecTrigger.None, Caster))
				{
					CActorIsRemovingCondition_MessageData message = new CActorIsRemovingCondition_MessageData("", item)
					{
						m_Ability = Ability,
						m_ActorAppliedTo = item,
						m_NegativeCondition = item2
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			foreach (CCondition.EPositiveCondition item3 in list3)
			{
				if (item3 != CCondition.EPositiveCondition.Bless)
				{
					item.RemovePositiveConditionToken(item3);
				}
			}
		}
	}
}

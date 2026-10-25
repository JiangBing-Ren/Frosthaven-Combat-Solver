using System;

namespace ScenarioRuleLibrary;

public class CAddHealActiveBonus : CActiveBonus
{
	public CAddHealActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.BuffHealPerActor:
			case EActiveBonusBehaviourType.BuffHealPerAction:
			case EActiveBonusBehaviourType.BuffIncomingHeal:
				if (ability is CAbilityAddHeal ability2)
				{
					AddBespokeBehaviour(new CAddHealActiveBonus_BuffHeal(actor, ability2, this, behaviour));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Heal active bonus");
			case EActiveBonusBehaviourType.HealOnHealingReceived:
				AddBespokeBehaviour(new CHealActiveBonus_HealOnHealReceived(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.HealOnDamageReceived:
				AddBespokeBehaviour(new CHealActiveBonus_HealOnDamageReceived(actor, ability, this));
				break;
			}
		}
	}
}

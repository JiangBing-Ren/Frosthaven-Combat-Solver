using System;

namespace ScenarioRuleLibrary;

public class CAttackActiveBonus : CActiveBonus
{
	public CAttackActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.BuffAttack:
			case EActiveBonusBehaviourType.BuffSingleAttack:
			case EActiveBonusBehaviourType.ShadowBuffAttack:
				if (ability is CAbilityAttack ability3)
				{
					AddBespokeBehaviour(new CAttackActiveBonus_BuffAttack(actor, ability3, this, behaviour));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus " + behaviour);
			case EActiveBonusBehaviourType.BuffIncomingAttacks:
				if (ability is CAbilityAttack ability2)
				{
					AddBespokeBehaviour(new CAttackActiveBonus_BuffIncomingAttacks(actor, ability2, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus " + behaviour);
			}
		}
	}
}

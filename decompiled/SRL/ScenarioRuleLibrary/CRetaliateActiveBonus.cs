using System;

namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus : CActiveBonus
{
	public CRetaliateActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.Retaliate:
				if (ability is CAbilityRetaliate ability3)
				{
					AddBespokeBehaviour(new CRetaliateActiveBonus_Retaliate(actor, ability3, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Retaliate active bonus");
			case EActiveBonusBehaviourType.SingleShotRetaliate:
				if (ability is CAbilityRetaliate ability4)
				{
					AddBespokeBehaviour(new CRetaliateActiveBonus_SingleShotRetaliate(actor, ability4, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Retaliate active bonus");
			case EActiveBonusBehaviourType.BuffRetaliate:
				if (ability is CAbilityRetaliate ability2)
				{
					AddBespokeBehaviour(new CRetaliateActiveBonus_BuffRetaliate(actor, ability2, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Retaliate active bonus");
			}
		}
	}
}

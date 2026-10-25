using System;

namespace ScenarioRuleLibrary;

public class CLootActiveBonus : CActiveBonus
{
	public CLootActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (ability is CAbilityLoot ability2)
		{
			{
				foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
				{
					switch (behaviour)
					{
					case EActiveBonusBehaviourType.BuffLoot:
						AddBespokeBehaviour(new CLootActiveBonus_BuffLoot(actor, ability2, this));
						break;
					case EActiveBonusBehaviourType.LimitLoot:
						AddBespokeBehaviour(new CLootActiveBonus_LimitLoot(actor, ability2, this));
						break;
					}
				}
				return;
			}
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus LootBuff");
	}
}

using System;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus : CActiveBonus
{
	public CAddConditionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (ability is CAbilityAddCondition ability2)
		{
			{
				foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
				{
					switch (behaviour)
					{
					case EActiveBonusBehaviourType.MultiplyAddedConditions:
						AddBespokeBehaviour(new CAddConditionActiveBonus_MultiplyAddedConditions(actor, ability2, this));
						break;
					case EActiveBonusBehaviourType.ApplyConditionalConditions:
						AddBespokeBehaviour(new CAddConditionActiveBonus_ApplyConditionalConditions(actor, ability2, this));
						break;
					case EActiveBonusBehaviourType.ApplyConditionOnLoseCondition:
						AddBespokeBehaviour(new CAddConditionActiveBonus_ApplyConditionOnLoseCondition(actor, ability2, this));
						break;
					case EActiveBonusBehaviourType.AddConditionUntilDamaged:
						AddBespokeBehaviour(new CAddConditionActiveBonus_AddConditionUntilDamaged(actor, ability2, this));
						break;
					}
				}
				return;
			}
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Condition active bonus");
	}
}

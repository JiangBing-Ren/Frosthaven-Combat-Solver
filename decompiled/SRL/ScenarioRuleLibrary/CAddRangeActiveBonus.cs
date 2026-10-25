using System;

namespace ScenarioRuleLibrary;

public class CAddRangeActiveBonus : CActiveBonus
{
	public CAddRangeActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.BuffRange)
			{
				if (!(ability is CAbilityAddRange ability2))
				{
					throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Range active bonus");
				}
				AddBespokeBehaviour(new CAddRangeActiveBonus_BuffRange(actor, ability2, this));
			}
		}
	}
}

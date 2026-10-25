using System;

namespace ScenarioRuleLibrary;

public class CChangeModifierActiveBonus : CActiveBonus
{
	public CChangeModifierActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.AdjustWhenAttacked:
				if (ability is CAbilityChangeModifier ability3)
				{
					AddBespokeBehaviour(new CChangeModifierActiveBonus_AdjustWhenAttacked(actor, baseCard, ability3, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus Adjust Attack Modifier");
			case EActiveBonusBehaviourType.AdjustWhenAttacking:
				if (ability is CAbilityChangeModifier ability2)
				{
					AddBespokeBehaviour(new CChangeModifierActiveBonus_AdjustWhenAttacking(actor, baseCard, ability2, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus Adjust Attacking Modifier");
			}
		}
	}
}

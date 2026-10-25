using System;

namespace ScenarioRuleLibrary;

public class CDamageActiveBonus : CActiveBonus
{
	public CDamageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.DamageOnCurseModApplied:
				if (ability is CAbilityDamage ability2)
				{
					AddBespokeBehaviour(new CDamageActiveBonus_DamageOnCurseModApplied(actor, ability2, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus DamageOnCurseModApplied");
			case EActiveBonusBehaviourType.DamageOnReceivedDamage:
				AddBespokeBehaviour(new CDamageActiveBonus_DamageOnReceivedDamage(caster, ability, this));
				break;
			}
		}
	}
}

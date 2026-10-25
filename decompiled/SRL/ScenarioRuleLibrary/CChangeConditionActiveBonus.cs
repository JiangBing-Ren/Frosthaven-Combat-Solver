using System.Linq;

namespace ScenarioRuleLibrary;

public class CChangeConditionActiveBonus : CActiveBonus
{
	public CChangeConditionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public bool ChangeCondition(CTokens token, CCondition.EPositiveCondition condition, int duration, EConditionDecTrigger decTrigger, CActor actor, int strength)
	{
		if (Ability.MiscAbilityData.ReplacePositiveConditions != null && Ability.MiscAbilityData.ReplacePositiveConditions.SingleOrDefault((CCondition.EPositiveCondition s) => s == condition) != CCondition.EPositiveCondition.NA)
		{
			if (Ability.MiscAbilityData.ReplaceWithPositiveConditions != null)
			{
				token.AddPositiveToken(Ability.MiscAbilityData.ReplaceWithPositiveConditions[0], duration, decTrigger, actor, Caster, recall: true);
				token.ModifiedPositiveCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
			if (Ability.MiscAbilityData.ReplaceWithNegativeConditions != null)
			{
				token.AddNegativeToken(Ability.MiscAbilityData.ReplaceWithNegativeConditions[0], duration, decTrigger, actor, strength, recall: true);
				token.ModifiedPositiveCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
		}
		return true;
	}

	public bool ChangeCondition(CTokens token, CCondition.ENegativeCondition condition, int duration, EConditionDecTrigger decTrigger, CActor actor, int strength)
	{
		if (Ability.MiscAbilityData.ReplaceNegativeConditions != null && Ability.MiscAbilityData.ReplaceNegativeConditions.SingleOrDefault((CCondition.ENegativeCondition s) => s == condition) != CCondition.ENegativeCondition.NA)
		{
			if (Ability.MiscAbilityData.ReplaceWithPositiveConditions != null)
			{
				token.AddPositiveToken(Ability.MiscAbilityData.ReplaceWithPositiveConditions[0], duration, decTrigger, actor, Caster, recall: true);
				token.ModifiedNegativeCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
			if (Ability.MiscAbilityData.ReplaceWithNegativeConditions != null)
			{
				token.AddNegativeToken(Ability.MiscAbilityData.ReplaceWithNegativeConditions[0], duration, decTrigger, actor, strength, recall: true);
				token.ModifiedNegativeCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
		}
		return true;
	}
}

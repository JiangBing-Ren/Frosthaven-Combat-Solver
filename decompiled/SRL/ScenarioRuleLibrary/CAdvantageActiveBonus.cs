namespace ScenarioRuleLibrary;

public class CAdvantageActiveBonus : CActiveBonus
{
	public CAdvantageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}
}

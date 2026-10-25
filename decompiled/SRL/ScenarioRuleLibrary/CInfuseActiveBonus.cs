namespace ScenarioRuleLibrary;

public class CInfuseActiveBonus : CActiveBonus
{
	public CInfuseActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}
}

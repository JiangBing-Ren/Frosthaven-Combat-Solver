namespace ScenarioRuleLibrary;

public class CPreventLoseCardActiveBonus : CActiveBonus
{
	public CPreventLoseCardActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}
}

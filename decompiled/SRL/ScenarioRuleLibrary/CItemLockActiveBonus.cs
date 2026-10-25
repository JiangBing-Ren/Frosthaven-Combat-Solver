namespace ScenarioRuleLibrary;

public class CItemLockActiveBonus : CActiveBonus
{
	public CItemLockActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}

	public override void Finish()
	{
		base.Finish();
		Actor?.ActivatePassiveItems(firstLoad: true);
	}
}

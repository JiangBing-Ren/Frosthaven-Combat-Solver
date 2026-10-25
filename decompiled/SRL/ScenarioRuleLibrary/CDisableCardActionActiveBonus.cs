namespace ScenarioRuleLibrary;

public class CDisableCardActionActiveBonus : CActiveBonus
{
	private CAbilityDisableCardAction DisableCardActionAbility;

	public CAbilityDisableCardAction.DisableCardActionData DisableCardActionData => DisableCardActionAbility.DisableCardActionAbilityData;

	public CDisableCardActionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		DisableCardActionAbility = (CAbilityDisableCardAction)ability;
	}
}

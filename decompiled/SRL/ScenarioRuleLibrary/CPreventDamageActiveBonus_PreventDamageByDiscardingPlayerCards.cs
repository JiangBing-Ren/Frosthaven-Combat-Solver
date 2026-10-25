namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards;

	public CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus == ActiveBonus)
		{
			OnBehaviourTriggered();
		}
	}
}

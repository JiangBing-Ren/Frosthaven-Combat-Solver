namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventFirstDamage : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventFirstDamage;

	public CPreventDamageActiveBonus_PreventFirstDamage(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus == ActiveBonus)
		{
			base.OnPreventDamageTriggered(preventedDamage, damageSource, actorDamaged, damagingAbility, preventedByBonus);
			m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damagingAbility?.TargetingActor, actorDamaged, ("PreventedDamage", preventedDamage));
		}
	}
}

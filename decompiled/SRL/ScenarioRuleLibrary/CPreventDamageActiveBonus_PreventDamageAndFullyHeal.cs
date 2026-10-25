namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventDamageAndFullyHeal : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventAndFullyHeal;

	public CPreventDamageActiveBonus_PreventDamageAndFullyHeal(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus == ActiveBonus)
		{
			base.OnPreventDamageTriggered(preventedDamage, damageSource, actorDamaged, damagingAbility, preventedByBonus);
			if (m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, actorDamaged, ("PreventedDamage", preventedDamage)))
			{
				actorDamaged.Healed(actorDamaged.MaxHealth);
				OnBehaviourTriggered();
			}
		}
	}
}

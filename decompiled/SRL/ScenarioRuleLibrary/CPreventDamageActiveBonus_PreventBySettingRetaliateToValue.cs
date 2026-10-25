namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventBySettingRetaliateToValue : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventBySettingRetaliateToValue;

	public CPreventDamageActiveBonus_PreventBySettingRetaliateToValue(CActor actor, CAbility ability, CActiveBonus activeBonus)
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
				OnBehaviourTriggered();
			}
		}
	}
}

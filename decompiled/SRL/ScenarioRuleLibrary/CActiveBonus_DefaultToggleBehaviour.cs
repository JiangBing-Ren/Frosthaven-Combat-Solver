namespace ScenarioRuleLibrary;

internal class CActiveBonus_DefaultToggleBehaviour : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public CActiveBonus_DefaultToggleBehaviour(CActor actor, CAbility ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType behaviourType)
		: base(actor, ability, activeBonus)
	{
		m_type = behaviourType;
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		if (attackAbility != null && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor) && m_ActiveBonus.SetupTriggerContext(this, null, attackAbility, null, target))
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnConditionApplyToActor(CAbility conditionAbility, CActor target)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(conditionAbility.TargetingActor) && m_ActiveBonus.SetupTriggerContext(this, null, conditionAbility, null, null))
		{
			m_ActiveBonus.RestrictActiveBonus(conditionAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnHealApplyToAction(CAbilityHeal healAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(healAbility.TargetingActor) && m_ActiveBonus.SetupTriggerContext(this, null, healAbility, null, null))
		{
			m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnHealApplyToActor(CAbilityHeal healAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(healAbility.TargetingActor) && m_ActiveBonus.SetupTriggerContext(this, null, healAbility, null, null))
		{
			m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus == ActiveBonus && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(damagedActor) && m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, damagedActor, ("PreventedDamage", preventedDamage)))
		{
			m_ActiveBonus.RestrictActiveBonus(damagedActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnRetaliate()
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(m_Actor) && m_ActiveBonus.SetupTriggerContext(this, null, null, m_Actor, null))
		{
			m_ActiveBonus.RestrictActiveBonus(m_Actor);
			OnBehaviourTriggered();
		}
	}
}

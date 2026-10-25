namespace ScenarioRuleLibrary;

public class CShieldActiveBonus_TargetedShield : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ShieldTargetedAttacks;

	public CShieldActiveBonus_TargetedShield(CActor actor, CAbilityShield ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (IsValidTarget(ability, m_Actor) && !m_ActiveBonusData.IsToggleBonus && m_ActiveBonus.SetupTriggerContext(this, null, ability, null, m_Actor, ("ModifiedStrength", modifiedStrength)))
		{
			m_ActiveBonus.RestrictActiveBonus(m_Actor);
			if (m_ActiveBonus.HasTracker && modifiedStrength > 0)
			{
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus)
		{
			return;
		}
		base.OnPreventDamageTriggered(damagePrevented, damageSource, damagedActor, damagingAbility, preventedByBonus);
		if (IsValidTarget(damagingAbility, damagedActor) && m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, damagedActor, ("PreventedDamage", damagePrevented)))
		{
			m_ActiveBonus.RestrictActiveBonus(damagedActor);
			if (m_ActiveBonus.HasTracker)
			{
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (m_ActiveBonusData.StrengthIsScalar)
		{
			return 0;
		}
		return m_Strength;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (!m_ActiveBonusData.StrengthIsScalar)
		{
			return 1;
		}
		return m_Strength;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		return m_ActiveBonusData.ProcXP;
	}
}

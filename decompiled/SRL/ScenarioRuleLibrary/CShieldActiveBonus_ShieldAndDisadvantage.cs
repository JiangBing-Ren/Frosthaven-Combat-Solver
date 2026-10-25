namespace ScenarioRuleLibrary;

public class CShieldActiveBonus_ShieldAndDisadvantage : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ShieldAndDisadvantage;

	public CShieldActiveBonus_ShieldAndDisadvantage(CActor actor, CAbilityShield ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (IsValidTarget(ability, m_Actor) && m_ActiveBonus.SetupTriggerContext(this, null, ability, null, m_Actor, ("ModifiedStrength", modifiedStrength)))
		{
			m_ActiveBonus.RestrictActiveBonus(ability.TargetingActor);
			if (m_ActiveBonus.HasTracker && modifiedStrength < 1)
			{
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus)
		{
			return;
		}
		base.OnPreventDamageTriggered(damagePrevented, damageSource, actorDamaged, damagingAbility, preventedByBonus);
		if (IsValidTarget(damagingAbility, m_Actor) && m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, actorDamaged, ("PreventedDamage", damagePrevented)))
		{
			m_ActiveBonus.RestrictActiveBonus(m_Actor);
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

namespace ScenarioRuleLibrary;

public class CAddRangeActiveBonus_BuffRange : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffRange;

	public CAddRangeActiveBonus_BuffRange(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnAttackStart(CAbilityAttack attackAbility)
	{
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		if (m_ActiveBonus.IsValidAttackType(attackAbility) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor) && (m_ActiveBonusData.TargetCount == 0 || (m_ActiveBonusData.TargetCount == 1 && attackAbility.AreaEffect == null && attackAbility.NumberTargets == 1 && (attackAbility.Targeting == CAbility.EAbilityTargeting.None || attackAbility.Targeting == CAbility.EAbilityTargeting.Range)) || (m_ActiveBonusData.TargetCount != 1 && m_ActiveBonusData.TargetCount == attackAbility.OriginalTargetCount)) && m_ActiveBonus.SetupTriggerContext(this, null, attackAbility, null, target))
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			m_ActiveBonus.RestrictActiveBonus(m_Actor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		int result = 0;
		if (ability is CAbilityAttack cAbilityAttack && !m_ActiveBonusData.StrengthIsScalar && m_ActiveBonus.IsValidAttackType(cAbilityAttack) && (m_ActiveBonusData.TargetCount == 0 || (m_ActiveBonusData.TargetCount == 1 && cAbilityAttack.AreaEffect == null && cAbilityAttack.NumberTargets == 1 && (cAbilityAttack.Targeting == CAbility.EAbilityTargeting.None || cAbilityAttack.Targeting == CAbility.EAbilityTargeting.Range)) || (m_ActiveBonusData.TargetCount != 1 && m_ActiveBonusData.TargetCount == cAbilityAttack.OriginalTargetCount)))
		{
			result = CheckStatIsBasedOnXType();
		}
		return result;
	}
}

namespace ScenarioRuleLibrary;

public class CAddTargetActiveBonus_BuffTarget : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffTarget;

	public CAddTargetActiveBonus_BuffTarget(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentAbility, activeBonus, toggledOn);
		if (currentAbility is CAbilityAttack cAbilityAttack)
		{
			if (cAbilityAttack.AttackSummary != null)
			{
				cAbilityAttack.AttackSummary.UpdateTargetData(cAbilityAttack, cAbilityAttack.ActiveSingleTargetItems, cAbilityAttack.ActiveSingleTargetActiveBonuses);
			}
			CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(m_Actor);
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = m_Actor;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = cAbilityAttack;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = cAbilityAttack.AttackSummary?.Copy();
			ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
		}
	}

	public override void OnPreActorIsAttacking(CAbilityAttack attackAbility)
	{
		if (m_ActiveBonus.IsValidAttackType(attackAbility))
		{
			HandleTrigger(attackAbility);
		}
	}

	public override void OnHealApplyToAction(CAbilityHeal healAbility)
	{
		if (IsValidAbilityType(healAbility))
		{
			HandleTrigger(healAbility);
		}
	}

	private void HandleTrigger(CAbility ability)
	{
		if (m_ActiveBonus.SetupTriggerContext(this, null, ability, null, null))
		{
			m_ActiveBonus.RestrictActiveBonus(ability.TargetingActor);
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
		if (!m_ActiveBonusData.StrengthIsScalar && IsValidAbilityType(ability))
		{
			if (ability is CAbilityAttack attackAbility && !m_ActiveBonus.IsValidAttackType(attackAbility))
			{
				return result;
			}
			result = CheckStatIsBasedOnXType();
		}
		return result;
	}
}

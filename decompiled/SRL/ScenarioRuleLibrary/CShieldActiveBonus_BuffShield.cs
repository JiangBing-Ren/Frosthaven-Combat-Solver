using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CShieldActiveBonus_BuffShield : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public CShieldActiveBonus_BuffShield(CActor actor, CAbilityShield ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType type)
		: base(actor, ability, activeBonus)
	{
		m_type = type;
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (IsValidTarget(ability, m_Actor) && !m_ActiveBonusData.IsToggleBonus && m_ActiveBonus.SetupTriggerContext(this, null, ability, null, m_Actor, ("ModifiedStrength", modifiedStrength)) && modifiedStrength > 0)
		{
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	protected override bool IsValidTarget(CAbility ability, CActor target, bool useTargetOriginalType = false)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			bool flag = m_ActiveBonusData.Filter.IsValidTarget(cAbilityAttack.TargetingActor, target, cAbilityAttack.IsTargetedAbility, useTargetOriginalType: false, true, skipUntargetableCheck: true);
			if (((target != null && cAbilityAttack != null) & flag) && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonus.IsValidAttackType(cAbilityAttack))
			{
				return true;
			}
			return false;
		}
		return m_ActiveBonusData.Filter.IsValidTarget(target, target, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true);
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus)
		{
			return;
		}
		base.OnPreventDamageTriggered(damagePrevented, damageSource, damagedActor, damagingAbility, preventedByBonus);
		if (IsValidTarget(damagingAbility, damagedActor) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(damagedActor) && m_ActiveBonusData.IsToggleBonus && m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, damagedActor, ("PreventedDamage", damagePrevented)))
		{
			m_ActiveBonus.RestrictActiveBonus(damagedActor);
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
		List<CConditionalOverride> source = m_Ability.ConditionalOverrides.Where((CConditionalOverride co) => co.Requirements.MeetsAbilityRequirements(target, m_Ability)).ToList();
		int result = CheckStatIsBasedOnXType(ability) + source.Sum((CConditionalOverride o) => o.AbilityOverrides.Sum((CAbilityOverride ao) => ao.Strength)).Value;
		if (!IsValidTarget(ability, m_Actor) || m_ActiveBonusData.StrengthIsScalar)
		{
			return 0;
		}
		return result;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		List<CConditionalOverride> source = m_Ability.ConditionalOverrides.Where((CConditionalOverride co) => co.Requirements.MeetsAbilityRequirements(target, m_Ability)).ToList();
		int result = CheckStatIsBasedOnXType(ability) + source.Sum((CConditionalOverride o) => o.AbilityOverrides.Sum((CAbilityOverride ao) => ao.Strength)).Value;
		if (!IsValidTarget(ability, m_Actor) || !m_ActiveBonusData.StrengthIsScalar)
		{
			return 1;
		}
		return result;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		return m_ActiveBonusData.ProcXP;
	}

	public override void OnListenersRefreshed(CActor actor)
	{
		base.OnListenersRefreshed(actor);
		actor.CalculateShield();
	}

	public override void OnRestrictionReset()
	{
		base.OnRestrictionReset();
		m_Actor?.CalculateShield();
	}

	public override void OnBeingAttackedPostDamage(CAbilityAttack attackAbility, CAttackSummary.TargetSummary targetSummary)
	{
		base.OnBeingAttackedPostDamage(attackAbility, targetSummary);
		if (ActiveBonus.IsActiveBonusToggledAndNotRestricted(targetSummary.ActorToAttack) && targetSummary.CalcExpressionString(calcPostShieldScalars: true, calcShield: false) > 0)
		{
			ActiveBonus.RestrictActiveBonus(targetSummary.ActorToAttack);
		}
	}
}

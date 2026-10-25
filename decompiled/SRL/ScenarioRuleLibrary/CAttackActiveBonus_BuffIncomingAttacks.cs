using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAttackActiveBonus_BuffIncomingAttacks : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffIncomingAttacks;

	public CAttackActiveBonus_BuffIncomingAttacks(CActor actor, CAbilityAttack ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttackedPreDamage(CAbilityAttack attackAbility)
	{
		if (!IsValidTarget(attackAbility, attackAbility.TargetingActor) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor))
		{
			return;
		}
		bool flag = CalculateBuffs(attackAbility, attackAbility.TargetingActor, out var _, out var _, out var _, out var _, out var _, out var _, out var _);
		flag |= m_ActiveBonusData.StrengthIsScalar;
		flag |= m_ActiveBonusData.AbilityStrengthIsScalar;
		flag |= m_ActiveBonusData.AlwaysBuff;
		if (m_Ability.NegativeConditions != null && m_Ability.NegativeConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition negCondition in m_Ability.NegativeConditions.Keys)
			{
				if (!attackAbility.NegativeConditions.ContainsKey(negCondition) && negCondition != CCondition.ENegativeCondition.NA)
				{
					CAbility.EAbilityType abilityType = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCondition.ToString());
					attackAbility.NegativeConditions.Add(negCondition, CAbility.CreateAbility(abilityType, attackAbility.AbilityFilter, isMonster: false, attackAbility.IsTargetedAbility));
					flag = true;
				}
			}
		}
		if (m_Ability.PositiveConditions != null && m_Ability.PositiveConditions.Count > 0)
		{
			foreach (CCondition.EPositiveCondition posCondition in m_Ability.PositiveConditions.Keys)
			{
				if (!attackAbility.PositiveConditions.ContainsKey(posCondition) && posCondition != CCondition.EPositiveCondition.NA)
				{
					CAbility.EAbilityType abilityType2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCondition.ToString());
					attackAbility.PositiveConditions.Add(posCondition, CAbility.CreateAbility(abilityType2, attackAbility.AbilityFilter, isMonster: false, attackAbility.IsTargetedAbility));
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		if (m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
		{
			foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
			{
				attackEffect.ResetStackedSubAbilityForBuffOnAttack();
			}
		}
		m_ActiveBonus.UpdateXPTracker();
		m_ActiveBonus.FinishIfNoRemaining();
	}

	private bool CalculateBuffs(CAbility ability, CActor target, out int strength, out int xp, out int pierce, out int range, out int targetsCount, out List<CCondition.EPositiveCondition> positiveConditions, out List<CCondition.ENegativeCondition> negativeConditions)
	{
		strength = 0;
		xp = 0;
		pierce = 0;
		range = 0;
		targetsCount = 0;
		positiveConditions = null;
		negativeConditions = null;
		CAbilityAttack cAbilityAttack = ability as CAbilityAttack;
		if (target != null && m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack2 && cAbilityAttack != null && cAbilityAttack.TargetingActor != m_Actor)
		{
			strength = CheckStatIsBasedOnXType(cAbilityAttack);
			xp = m_XP;
			foreach (CAttackEffect attackEffect in cAbilityAttack2.AttackEffects)
			{
				attackEffect.GetBonus(target, cAbilityAttack, out var attackBuff, out var xpBuff, out var rangeBuff, out var targetsCountBuff, out var pierceBuff, out var _, out var positiveConditionsBuff, out var negativeConditionsBuff);
				strength += attackBuff;
				xp += xpBuff;
				range += rangeBuff;
				targetsCount += targetsCountBuff;
				pierce += pierceBuff;
				positiveConditions = positiveConditionsBuff;
				negativeConditions = negativeConditionsBuff;
			}
		}
		if (strength == 0 && xp == 0 && pierce == 0 && range == 0 && targetsCount == 0 && positiveConditions == null)
		{
			return negativeConditions != null;
		}
		return true;
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (!m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _, out var _, out var _, out var _, out var _);
			return strength;
		}
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _, out var _, out var _, out var _, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		if (IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var _, out var xp, out var _, out var _, out var _, out var _, out var _);
			return xp;
		}
		return 0;
	}

	public int ReferencePierce(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, ability.TargetingActor))
		{
			CalculateBuffs(ability, target, out var _, out var _, out var pierce, out var _, out var _, out var _, out var _);
			return pierce;
		}
		return 0;
	}

	public int ReferenceRange(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var range, out var _, out var _, out var _);
			return range;
		}
		return 0;
	}

	public int ReferenceTargetsCount(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var _, out var targetsCount, out var _, out var _);
			return targetsCount;
		}
		return 0;
	}

	public List<CCondition.EPositiveCondition> ReferencePositiveConditions(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var _, out var _, out var positiveConditions, out var _);
			return positiveConditions;
		}
		return null;
	}

	public List<CCondition.ENegativeCondition> ReferenceNegativeConditions(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var _, out var _, out var _, out var negativeConditions);
			return negativeConditions;
		}
		return null;
	}

	protected override bool IsValidTarget(CAbility ability, CActor target, bool useTargetOriginalType = false)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			if (target != null && cAbilityAttack != null && m_ActiveBonusData.Filter.IsValidTarget(target, m_ActiveBonus.Caster, cAbilityAttack.IsTargetedAbility, useTargetOriginalType: false, cAbilityAttack.MiscAbilityData?.CanTargetInvisible) && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonus.IsValidAttackType(cAbilityAttack))
			{
				return true;
			}
		}
		else if (target != null && ability != null && ability.TargetingActor != null && m_ActiveBonusData.Filter.IsValidTarget(target, m_ActiveBonus.Caster, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished())
		{
			return true;
		}
		return false;
	}
}

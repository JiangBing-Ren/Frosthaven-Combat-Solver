using System.Linq;

namespace ScenarioRuleLibrary;

public class CLootActiveBonus_BuffLoot : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffLoot;

	public CLootActiveBonus_BuffLoot(CActor actor, CAbilityLoot ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		ability.Strength = ability.Range;
	}

	public override void OnLoot(CAbilityLoot lootAbility)
	{
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(lootAbility.TargetingActor))
		{
			return;
		}
		bool flag = CalculateBuffs(out var _, out var _);
		flag |= m_ActiveBonusData.StrengthIsScalar;
		if (m_Ability.NegativeConditions != null && m_Ability.NegativeConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition negCondition in m_Ability.NegativeConditions.Keys)
			{
				if (!lootAbility.NegativeConditions.ContainsKey(negCondition) && negCondition != CCondition.ENegativeCondition.NA)
				{
					CAbility.EAbilityType abilityType = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCondition.ToString());
					lootAbility.NegativeConditions.Add(negCondition, CAbility.CreateAbility(abilityType, lootAbility.AbilityFilter, isMonster: false, lootAbility.IsTargetedAbility));
					flag = true;
				}
			}
		}
		if (m_Ability.PositiveConditions != null && m_Ability.PositiveConditions.Count > 0)
		{
			foreach (CCondition.EPositiveCondition posCondition in m_Ability.PositiveConditions.Keys)
			{
				if (!lootAbility.PositiveConditions.ContainsKey(posCondition) && posCondition != CCondition.EPositiveCondition.NA)
				{
					CAbility.EAbilityType abilityType2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCondition.ToString());
					lootAbility.PositiveConditions.Add(posCondition, CAbility.CreateAbility(abilityType2, lootAbility.AbilityFilter, isMonster: false, lootAbility.IsTargetedAbility));
					flag = true;
				}
			}
		}
		if (flag)
		{
			m_ActiveBonus.RestrictActiveBonus(lootAbility.TargetingActor);
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (ability is CAbilityLoot { Range: 1 } && !m_ActiveBonusData.StrengthIsScalar)
		{
			CalculateBuffs(out var strength, out var _);
			return strength;
		}
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (ability is CAbilityLoot { Range: 1 } && m_ActiveBonusData.StrengthIsScalar)
		{
			CalculateBuffs(out var strength, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		if (ability is CAbilityLoot)
		{
			CalculateBuffs(out var _, out var xp);
			return xp;
		}
		return 0;
	}

	public bool CalculateBuffs(out int strength, out int xp)
	{
		strength = CheckStatIsBasedOnXType();
		xp = m_XP;
		if (strength == 0)
		{
			return xp != 0;
		}
		return true;
	}
}

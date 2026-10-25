using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class COverhealActiveBonus_SetMinimumOverheal : CBespokeBehaviour
{
	public int CurrentMinimumOverheal;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.SetMinimumOverheal;

	public COverhealActiveBonus_SetMinimumOverheal(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		UpdateCurrentOverhealStrength();
	}

	public void UpdateCurrentOverhealStrength()
	{
		CalculateBuffs(m_Ability, m_Actor, out var strength);
		CurrentMinimumOverheal = strength;
		CheckForAllMinimumOverhealsAndUpdate();
	}

	public void CheckForAllMinimumOverhealsAndUpdate(bool excludeSelf = false)
	{
		int num = 0;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(m_Actor, m_Ability.AbilityType, BehaviourType);
		list.RemoveAll((CActiveBonus b) => b.Finishing() || b.Finished() || (excludeSelf && b == m_ActiveBonus));
		List<COverhealActiveBonus_SetMinimumOverheal> list2 = list.Select((CActiveBonus a) => a.BespokeBehaviour as COverhealActiveBonus_SetMinimumOverheal).ToList();
		if (!excludeSelf && !list2.Contains(this))
		{
			list2.Add(this);
		}
		if (list2.Count > 0)
		{
			num = list2.Aggregate((COverhealActiveBonus_SetMinimumOverheal currentMin, COverhealActiveBonus_SetMinimumOverheal next) => (currentMin != null && next.CurrentMinimumOverheal <= currentMin.CurrentMinimumOverheal) ? currentMin : next).CurrentMinimumOverheal;
		}
		if (m_Actor.MinimumOverheal != num)
		{
			m_Actor.MinimumOverheal = num;
			m_Actor.Health = Math.Min(m_Actor.Health, m_Actor.MaxHealth);
			CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(m_Actor, m_Actor, m_Actor.Health);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Overheal && !m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength);
			return strength;
		}
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Overheal && m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength);
			return strength;
		}
		return 1;
	}

	public override int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Overheal && m_ActiveBonusData.AbilityStrengthIsScalar && !m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength);
			return strength;
		}
		return 1;
	}

	private bool CalculateBuffs(CAbility ability, CActor target, out int strength)
	{
		strength = CheckStatIsBasedOnXType();
		return strength != 0;
	}
}

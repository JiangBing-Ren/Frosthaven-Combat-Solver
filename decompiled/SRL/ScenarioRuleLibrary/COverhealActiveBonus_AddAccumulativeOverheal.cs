using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class COverhealActiveBonus_AddAccumulativeOverheal : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	public int CurrentAddedOverheal;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public COverhealActiveBonus_AddAccumulativeOverheal(CActor actor, CAbility ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType type)
		: base(actor, ability, activeBonus)
	{
		UpdateCurrentOverhealStrength(actor);
		m_type = type;
	}

	public void UpdateCurrentOverhealStrength(CActor target)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			CalculateBuffs(m_Ability, target, out var strength);
			CurrentAddedOverheal = strength;
			GetAccumulativeOverhealAndUpdate(target);
		}
	}

	public void GetAccumulativeOverhealAndUpdate(CActor m_Actor, bool excludeSelf = false)
	{
		int num = 0;
		List<CActiveBonus> list = (from b in CActiveBonus.FindApplicableActiveBonuses(m_Actor, m_Ability.AbilityType)
			where b.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AddAccumulativeOverheal, CActiveBonus.EActiveBonusBehaviourType.AddAccumulativeOverhealAndHeal, CActiveBonus.EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax)
			select b).ToList();
		list.RemoveAll((CActiveBonus b) => b.Finishing() || b.Finished() || (excludeSelf && b == m_ActiveBonus) || (m_ActiveBonus.Ability.IsItemAbility && b.BaseCard == m_ActiveBonus.BaseCard));
		List<COverhealActiveBonus_AddAccumulativeOverheal> list2 = list.Select((CActiveBonus a) => a.GetBehaviour<COverhealActiveBonus_AddAccumulativeOverheal>()).ToList();
		if (!excludeSelf && !list2.Contains(this))
		{
			list2.Add(this);
		}
		list2.RemoveAll((COverhealActiveBonus_AddAccumulativeOverheal b) => b.ActiveBonus.IsAura && !b.ActiveBonus.ValidActorsInRangeOfAura.Contains(m_Actor));
		num = list2.Sum((COverhealActiveBonus_AddAccumulativeOverheal b) => b.CurrentAddedOverheal);
		if (m_Actor.AccumulativeOverheal == num)
		{
			return;
		}
		bool flag = m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax) && m_Actor.AccumulativeOverheal < num && m_Actor.Health == m_Actor.MaxHealth && (!ScenarioManager.IsLoading || ScenarioManager.CurrentScenarioState.IsFirstLoad);
		int num2 = 0;
		if (m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AddAccumulativeOverhealAndHeal) && m_Actor.AccumulativeOverheal != num)
		{
			num2 = num - m_Actor.AccumulativeOverheal;
		}
		m_Actor.AccumulativeOverheal = num;
		if (!GameState.IsLoadingClassActiveBonuses)
		{
			if (flag)
			{
				m_Actor.Health = m_Actor.MaxHealth;
			}
			else
			{
				m_Actor.Health = Math.Min(m_Actor.Health + num2, m_Actor.MaxHealth);
				GameState.ActorHealthCheck(m_Actor, m_Actor);
			}
		}
		CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(m_Actor, m_Actor, m_Actor.Health);
		ScenarioRuleClient.MessageHandler(message);
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
		if (!m_ActiveBonus.AuraFilter.IsValidTarget(target, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false, skipUntargetableCheck: false, null, doesntWorkOnDeactivated: false))
		{
			strength = 0;
		}
		else
		{
			strength = CheckStatIsBasedOnXType();
		}
		return strength != 0;
	}

	public override void OnListenersRefreshed(CActor actor)
	{
		base.OnListenersRefreshed(actor);
		UpdateCurrentOverhealStrength(actor);
	}
}

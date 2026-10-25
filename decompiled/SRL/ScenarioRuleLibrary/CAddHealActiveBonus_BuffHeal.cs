namespace ScenarioRuleLibrary;

public class CAddHealActiveBonus_BuffHeal : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public CAddHealActiveBonus_BuffHeal(CActor actor, CAbilityAddHeal ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType type)
		: base(actor, ability, activeBonus)
	{
		m_type = type;
	}

	public override void OnHealApplyToActor(CAbilityHeal healAbility)
	{
		if (!m_ActiveBonus.SetupTriggerContext(this, null, healAbility, null, null))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
		if (m_Ability.MiscAbilityData?.ConditionsToRemoveFirst != null)
		{
			foreach (CActor item in healAbility.ActorsTargeted)
			{
				CAbilityRemoveConditions.RemoveConditions(healAbility.TargetingActor, item, m_Ability.MiscAbilityData.ConditionsToRemoveFirst, null, m_Ability);
			}
		}
		if (!m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffHealPerActor) || !m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item2 in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item2, healAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		if (m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public override void OnHealApplyToAction(CAbilityHeal healAbility)
	{
		if (!m_ActiveBonus.SetupTriggerContext(this, null, healAbility, null, null))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
		if (!m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffHealPerAction) || !m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, healAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		if (!m_ActiveBonus.ApplyToAction)
		{
			m_ActiveBonus.ApplyToAction = true;
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}

	public override void OnBeingHealed(CAbilityHeal healAbility)
	{
		if (!m_ActiveBonus.SetupTriggerContext(this, null, healAbility, null, null))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
		if (!m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal) || !m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, healAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		m_ActiveBonus.FinishIfNoRemaining();
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		int result = 0;
		bool flag = true;
		if (ability is CAbilityHeal ability2)
		{
			flag = IsValidTarget(ability2, target);
		}
		if (flag && !m_ActiveBonusData.StrengthIsScalar && (!m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal) || (m_ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal) && target == m_Actor)))
		{
			result = CheckStatIsBasedOnXType();
		}
		return result;
	}
}

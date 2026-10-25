namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus_MultiplyAddedConditions : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.MultiplyAddedConditions;

	public CAddConditionActiveBonus_MultiplyAddedConditions(CActor actor, CAbilityAddCondition ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnConditionApplyToActor(CAbility conditionAbility, CActor target)
	{
		if (!IsValidAbilityType(conditionAbility) || !IsValidTarget(conditionAbility, target) || !m_ActiveBonus.SetupTriggerContext(this, null, conditionAbility, null, target))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(conditionAbility.TargetingActor);
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, conditionAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		m_ActiveBonus.FinishIfNoRemaining();
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		int num = 0;
		if (ability is CAbilityHeal ability2 && !m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability2, target))
		{
			num = CheckStatIsBasedOnXType();
		}
		else if (IsValidAbilityType(ability))
		{
			num = CheckStatIsBasedOnXType();
			if (m_ActiveBonusData.StrengthIsScalar)
			{
				num = ability.Strength * num - ability.Strength;
			}
			if (!m_ActiveBonus.SetupTriggerContext(this, null, ability, null, target, ("Strength", num)))
			{
				return 0;
			}
			m_ActiveBonus.RestrictActiveBonus(ability.TargetingActor);
			if (m_ActiveBonus.HasTracker)
			{
				foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
				{
					ElementInfusionBoardManager.Consume(item, ability.TargetingActor);
				}
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
		return num;
	}
}

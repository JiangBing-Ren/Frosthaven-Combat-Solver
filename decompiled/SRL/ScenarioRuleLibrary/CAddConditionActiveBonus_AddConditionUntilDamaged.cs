using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus_AddConditionUntilDamaged : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.AddConditionUntilDamaged;

	public CAddConditionActiveBonus_AddConditionUntilDamaged(CActor actor, CAbilityAddCondition ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
		{
			m_Actor.ApplyCondition(m_ActiveBonus.Caster, positiveCondition.Key, 1, EConditionDecTrigger.ConditionalCondition);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
		{
			m_Actor.ApplyCondition(m_ActiveBonus.Caster, negativeCondition.Key, 1, EConditionDecTrigger.ConditionalCondition);
		}
	}

	private void RemoveConditions()
	{
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
		{
			m_Actor.RemovePositiveConditionToken(positiveCondition.Key, EConditionDecTrigger.ConditionalCondition);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
		{
			m_Actor.RemoveNegativeConditionToken(negativeCondition.Key, EConditionDecTrigger.ConditionalCondition, m_Actor);
		}
	}

	public override void OnDamaged(GameState.DamageData damageData)
	{
		base.OnDamaged(damageData);
		if (!m_ActiveBonus.SetupTriggerContext(this, null, null, damageData.ActorDamaged, null))
		{
			return;
		}
		RemoveConditions();
		if (m_ActiveBonus.HasTracker)
		{
			foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
			{
				ElementInfusionBoardManager.Consume(item, m_Actor);
			}
			OnBehaviourTriggered();
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
		else
		{
			Finish();
		}
	}

	public override void OnFinished()
	{
		base.OnFinished();
		RemoveConditions();
	}
}

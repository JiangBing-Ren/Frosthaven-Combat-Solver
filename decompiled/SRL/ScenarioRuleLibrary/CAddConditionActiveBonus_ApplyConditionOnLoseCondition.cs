using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus_ApplyConditionOnLoseCondition : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ApplyConditionOnLoseCondition;

	public CAddConditionActiveBonus_ApplyConditionOnLoseCondition(CActor actor, CAbilityAddCondition ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnNegativeConditionEndedOnActor(CCondition.ENegativeCondition negativeConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
		if (!IsValidNegativeConditionType(negativeConditionType) || !m_ActiveBonus.SetupTriggerContext(this, null, null, m_Actor, target, ("NegativeConditionType", TValue.Enum(negativeConditionType))))
		{
			return;
		}
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> item in m_Ability.ActiveBonusData.AbilityData?.PositiveConditions)
		{
			target.ApplyCondition(m_Actor, item.Key, 1, EConditionDecTrigger.Turns);
			CConditionCombatLog_MessageData message = new CConditionCombatLog_MessageData(m_Actor, item.Key, CCondition.ENegativeCondition.NA, target);
			ScenarioRuleClient.MessageHandler(message);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> item2 in m_Ability.ActiveBonusData.AbilityData?.NegativeConditions)
		{
			target.ApplyCondition(m_Actor, item2.Key, 1, EConditionDecTrigger.Turns);
			CConditionCombatLog_MessageData message2 = new CConditionCombatLog_MessageData(m_Actor, CCondition.EPositiveCondition.NA, item2.Key, target);
			ScenarioRuleClient.MessageHandler(message2);
		}
		m_ActiveBonus.RestrictActiveBonus(target);
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item3 in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item3, m_Actor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		m_ActiveBonus.FinishIfNoRemaining();
	}

	public override void OnPositiveConditionEndedOnActor(CCondition.EPositiveCondition positiveConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
		if (!IsValidPositiveConditionType(positiveConditionType) || !m_ActiveBonus.SetupTriggerContext(this, null, null, m_Actor, target, ("PositiveConditionType", TValue.Enum(positiveConditionType))))
		{
			return;
		}
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> item in m_Ability.ActiveBonusData.AbilityData?.PositiveConditions)
		{
			target.ApplyCondition(m_Actor, item.Key, 1, EConditionDecTrigger.Turns);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> item2 in m_Ability.ActiveBonusData.AbilityData?.NegativeConditions)
		{
			target.ApplyCondition(m_Actor, item2.Key, 1, EConditionDecTrigger.Turns);
		}
		m_ActiveBonus.RestrictActiveBonus(target);
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item3 in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item3, m_Actor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		m_ActiveBonus.FinishIfNoRemaining();
	}
}

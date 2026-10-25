namespace ScenarioRuleLibrary;

public class CConditionCombatLog_MessageData : CMessageData
{
	public CActor m_ActorGettingCondition;

	public CCondition.ENegativeCondition m_ConditionNegative;

	public CCondition.EPositiveCondition m_ConditionPositive;

	public CConditionCombatLog_MessageData(CActor actorSpawningMessage, CCondition.EPositiveCondition conditionP, CCondition.ENegativeCondition conditionN, CActor actorGettingCondition)
		: base(MessageType.ConditionCombatLog, actorSpawningMessage)
	{
		m_ConditionPositive = conditionP;
		m_ConditionNegative = conditionN;
		m_ActorGettingCondition = actorGettingCondition;
	}
}

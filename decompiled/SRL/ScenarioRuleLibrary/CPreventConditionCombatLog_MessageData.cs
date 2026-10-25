namespace ScenarioRuleLibrary;

public class CPreventConditionCombatLog_MessageData : CMessageData
{
	public CActor m_ActorGettingCondition;

	public CCondition.ENegativeCondition m_ConditionNegative;

	public CCondition.EPositiveCondition m_ConditionPositive;

	public CAbility.EAbilityType m_AbilityType;

	public CPreventConditionCombatLog_MessageData(CActor actorSpawningMessage, CAbility.EAbilityType type, CActor actorGettingCondition)
		: base(MessageType.PreventConditionCombatLog, actorSpawningMessage)
	{
		m_AbilityType = type;
		m_ActorGettingCondition = actorGettingCondition;
	}
}

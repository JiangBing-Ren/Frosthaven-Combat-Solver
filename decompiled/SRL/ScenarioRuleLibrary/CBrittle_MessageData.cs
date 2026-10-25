namespace ScenarioRuleLibrary;

public class CBrittle_MessageData : CMessageData
{
	public CActor m_Actor;

	public bool m_ConditionAlreadyApplied;

	public CBrittle_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Brittle, actorSpawningMessage, animOverload)
	{
	}
}

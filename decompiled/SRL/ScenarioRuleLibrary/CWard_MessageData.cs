namespace ScenarioRuleLibrary;

public class CWard_MessageData : CMessageData
{
	public CActor m_ActorToWard;

	public bool m_ConditionAlreadyApplied;

	public CWard_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Ward, actorSpawningMessage, animOverload)
	{
	}
}

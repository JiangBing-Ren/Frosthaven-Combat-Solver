namespace ScenarioRuleLibrary;

public class CSleep_MessageData : CMessageData
{
	public CActor m_ActorToSleep;

	public bool m_ConditionAlreadyApplied;

	public CSleep_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Sleep, actorSpawningMessage, animOverload)
	{
	}
}

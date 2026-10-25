namespace ScenarioRuleLibrary;

public class CWardTriggered_MessageData : CMessageData
{
	public CActor m_WardedActor;

	public bool m_ActorWasAsleep;

	public CWardTriggered_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.WardTriggered, actorSpawningMessage, animOverload)
	{
	}
}

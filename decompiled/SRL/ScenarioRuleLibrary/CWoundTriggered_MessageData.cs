namespace ScenarioRuleLibrary;

public class CWoundTriggered_MessageData : CMessageData
{
	public CActor m_WoundedActor;

	public int m_ActorOriginalHealth;

	public bool m_ActorWasAsleep;

	public CWoundTriggered_MessageData(CActor actorSpawningMessage)
		: base(MessageType.WoundTriggered, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CRegenerateTriggered_MessageData : CMessageData
{
	public CActor m_RegenActor;

	public int m_ActorOriginalHealth;

	public bool m_ActorWasAsleep;

	public CRegenerateTriggered_MessageData(CActor actorSpawningMessage)
		: base(MessageType.RegenerateTriggered, actorSpawningMessage)
	{
	}
}

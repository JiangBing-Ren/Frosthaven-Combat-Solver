namespace ScenarioRuleLibrary;

public class CBaneTriggered_MessageData : CMessageData
{
	public CActor m_BanedActor;

	public int m_ActorOriginalHealth;

	public bool m_ActorWasAsleep;

	public CBaneTriggered_MessageData(CActor actorSpawningMessage)
		: base(MessageType.BaneTriggered, actorSpawningMessage)
	{
	}
}

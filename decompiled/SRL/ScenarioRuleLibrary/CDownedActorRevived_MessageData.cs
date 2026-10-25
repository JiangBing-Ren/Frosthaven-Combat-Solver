namespace ScenarioRuleLibrary;

public class CDownedActorRevived_MessageData : CMessageData
{
	public CActor m_Actor;

	public CDownedActorRevived_MessageData(CActor actorSpawningMessage)
		: base(MessageType.DownedActorRevived, actorSpawningMessage)
	{
		m_Actor = actorSpawningMessage;
	}
}

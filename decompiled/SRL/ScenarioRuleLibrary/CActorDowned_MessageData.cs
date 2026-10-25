namespace ScenarioRuleLibrary;

public class CActorDowned_MessageData : CMessageData
{
	public CActor m_Actor;

	public CActorDowned_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorDowned, actorSpawningMessage)
	{
		m_Actor = actorSpawningMessage;
	}
}

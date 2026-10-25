namespace ScenarioRuleLibrary;

public class CActorAwakened_MessageData : CMessageData
{
	public CActor m_ActorAwakened;

	public CActorAwakened_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorAwakened, actorSpawningMessage)
	{
		m_ActorAwakened = actorSpawningMessage;
	}
}

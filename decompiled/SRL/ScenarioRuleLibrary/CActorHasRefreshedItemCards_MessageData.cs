namespace ScenarioRuleLibrary;

public class CActorHasRefreshedItemCards_MessageData : CMessageData
{
	public CActor m_ActorBeenRefreshed;

	public CActorHasRefreshedItemCards_MessageData(CActor actorSpawningMessage, CActor targetActor)
		: base(MessageType.ActorHasRefreshedItemCards, actorSpawningMessage)
	{
		m_ActorBeenRefreshed = targetActor;
	}
}

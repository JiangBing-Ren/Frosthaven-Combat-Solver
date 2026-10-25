namespace ScenarioRuleLibrary;

public class CRefreshActorHealth_MessageData : CMessageData
{
	public CActor m_ActorBeingRefreshed;

	public int m_ActorOriginalHealth;

	public CRefreshActorHealth_MessageData(CActor actorSpawningMessage, CActor targetActor, int actorOriginalHealth)
		: base(MessageType.RefreshActorHealth, actorSpawningMessage)
	{
		m_ActorBeingRefreshed = targetActor;
		m_ActorOriginalHealth = actorOriginalHealth;
	}
}

namespace ScenarioRuleLibrary;

public class CActorLookAt_MessageData : CMessageData
{
	public CTile m_TileToLookAt;

	public CActorLookAt_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorLookAt, actorSpawningMessage)
	{
	}
}

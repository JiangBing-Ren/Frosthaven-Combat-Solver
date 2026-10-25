namespace ScenarioRuleLibrary;

public class CPlayerFinishedBurningCardForAlly_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public CPlayerFinishedBurningCardForAlly_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerFinishedBurningCardForAlly, actorSpawningMessage)
	{
	}
}

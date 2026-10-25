namespace ScenarioRuleLibrary;

public class CPlayerToShortRest_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.PlayerToShortRest, actorSpawningMessage)
{
	public bool m_SyncMultiplayer = true;
}

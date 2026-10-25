namespace ScenarioRuleLibrary;

public class CActionSelection_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActionSelection, actorSpawningMessage)
{
	public bool m_SyncMultiplayer = true;
}

namespace ScenarioRuleLibrary;

public class CEndRound_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.EndRound, actorSpawningMessage)
{
	public bool m_SyncMultiplayer = true;
}

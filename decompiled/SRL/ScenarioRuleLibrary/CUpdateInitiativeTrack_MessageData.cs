namespace ScenarioRuleLibrary;

public class CUpdateInitiativeTrack_MessageData : CMessageData
{
	public CUpdateInitiativeTrack_MessageData(CActor actorSpawningMessage)
		: base(MessageType.UpdateInitiativeTrack, actorSpawningMessage)
	{
	}
}

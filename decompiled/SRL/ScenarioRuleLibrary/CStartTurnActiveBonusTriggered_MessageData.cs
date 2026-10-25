namespace ScenarioRuleLibrary;

public class CStartTurnActiveBonusTriggered_MessageData : CMessageData
{
	public CStartTurnActiveBonusTriggered_MessageData(CActor actorSpawningMessage)
		: base(MessageType.StartTurnActiveBonusTriggered, actorSpawningMessage)
	{
	}
}

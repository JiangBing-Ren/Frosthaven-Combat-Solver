namespace ScenarioRuleLibrary;

public class CEndTurn_MessageData : CMessageData
{
	public CEndTurn_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndTurn, actorSpawningMessage)
	{
	}
}

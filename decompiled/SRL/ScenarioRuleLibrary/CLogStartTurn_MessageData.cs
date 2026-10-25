namespace ScenarioRuleLibrary;

public class CLogStartTurn_MessageData : CMessageData
{
	public CLogStartTurn_MessageData(CActor actorSpawningMessage, bool skipNextPhase = false)
		: base(MessageType.LogStartTurn, actorSpawningMessage)
	{
	}
}

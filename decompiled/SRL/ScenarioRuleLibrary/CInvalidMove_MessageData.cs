namespace ScenarioRuleLibrary;

public class CInvalidMove_MessageData : CMessageData
{
	public CInvalidMove_MessageData(CActor actorSpawningMessage)
		: base(MessageType.InvalidMove, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CWaitForMovementComplete_MessageData : CMessageData
{
	public CWaitForMovementComplete_MessageData(CActor actorSpawningMessage)
		: base(MessageType.WaitForMovementComplete, actorSpawningMessage)
	{
	}
}

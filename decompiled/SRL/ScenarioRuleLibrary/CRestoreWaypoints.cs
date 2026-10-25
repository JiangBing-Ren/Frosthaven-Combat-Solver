namespace ScenarioRuleLibrary;

public class CRestoreWaypoints : CMessageData
{
	public CRestoreWaypoints(CActor actorSpawningMessage)
		: base(MessageType.RestoreWaypoints, actorSpawningMessage)
	{
	}
}

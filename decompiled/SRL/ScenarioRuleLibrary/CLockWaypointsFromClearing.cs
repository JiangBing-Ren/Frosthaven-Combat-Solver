namespace ScenarioRuleLibrary;

public class CLockWaypointsFromClearing : CMessageData
{
	public CLockWaypointsFromClearing(CActor actorSpawningMessage)
		: base(MessageType.LockWaypointsFromClearing, actorSpawningMessage)
	{
	}
}

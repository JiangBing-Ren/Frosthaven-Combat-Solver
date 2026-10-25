namespace ScenarioRuleLibrary;

public class CUnlockWaypointsForClearing : CMessageData
{
	public CUnlockWaypointsForClearing(CActor actorSpawningMessage)
		: base(MessageType.UnlockWaypointsForClearing, actorSpawningMessage)
	{
	}
}

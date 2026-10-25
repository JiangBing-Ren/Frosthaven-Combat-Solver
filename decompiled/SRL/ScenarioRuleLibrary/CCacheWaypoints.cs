namespace ScenarioRuleLibrary;

public class CCacheWaypoints : CMessageData
{
	public CCacheWaypoints(CActor actorSpawningMessage)
		: base(MessageType.CacheWaypoints, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CInvalidSpawnPosition_MessageData : CMessageData
{
	public CInvalidSpawnPosition_MessageData(CActor actorSpawningMessage)
		: base(MessageType.InvalidSpawnPosition, actorSpawningMessage)
	{
	}
}

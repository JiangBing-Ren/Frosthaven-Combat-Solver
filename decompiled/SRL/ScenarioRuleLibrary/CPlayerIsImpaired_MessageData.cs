namespace ScenarioRuleLibrary;

public class CPlayerIsImpaired_MessageData : CMessageData
{
	public CPlayerIsImpaired_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerIsImpaired, actorSpawningMessage)
	{
	}
}

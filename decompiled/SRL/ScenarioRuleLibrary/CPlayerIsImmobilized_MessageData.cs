namespace ScenarioRuleLibrary;

public class CPlayerIsImmobilized_MessageData : CMessageData
{
	public CPlayerIsImmobilized_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerIsImmobilized, actorSpawningMessage)
	{
	}
}

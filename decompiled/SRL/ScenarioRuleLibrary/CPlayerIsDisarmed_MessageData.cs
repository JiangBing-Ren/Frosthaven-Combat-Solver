namespace ScenarioRuleLibrary;

public class CPlayerIsDisarmed_MessageData : CMessageData
{
	public CPlayerIsDisarmed_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerIsDisarmed, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CPlayerFinishedShortRestWithNoEffects_MessageData : CMessageData
{
	public CPlayerFinishedShortRestWithNoEffects_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerFinishedShortRestWithNoEffects, actorSpawningMessage)
	{
	}
}

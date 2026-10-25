namespace ScenarioRuleLibrary;

public class CPlayerFinishedLongRestWithNoEffects_MessageData : CMessageData
{
	public CPlayerFinishedLongRestWithNoEffects_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerFinishedLongRestWithNoEffects, actorSpawningMessage)
	{
	}
}

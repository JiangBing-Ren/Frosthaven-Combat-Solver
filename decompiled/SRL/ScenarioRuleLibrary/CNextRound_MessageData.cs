namespace ScenarioRuleLibrary;

public class CNextRound_MessageData : CMessageData
{
	public CNextRound_MessageData(CActor actorSpawningMessage)
		: base(MessageType.NextRound, actorSpawningMessage)
	{
	}
}

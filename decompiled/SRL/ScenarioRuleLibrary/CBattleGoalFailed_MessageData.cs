namespace ScenarioRuleLibrary;

public class CBattleGoalFailed_MessageData : CMessageData
{
	public string battleGoalID;

	public string characterID;

	public CBattleGoalFailed_MessageData(CActor actorSpawningMessage)
		: base(MessageType.BattleGoalFailed, actorSpawningMessage)
	{
	}
}

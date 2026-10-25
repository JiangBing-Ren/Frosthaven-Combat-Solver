namespace ScenarioRuleLibrary;

public class CUpdateBattleGoalsMessage : CMessageData
{
	public CUpdateBattleGoalsMessage(CActor actorSpawningMessage)
		: base(MessageType.UpdateBattleGoals, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CPlayerCannotUseMoveAbilities_MessageData : CMessageData
{
	public CPlayerCannotUseMoveAbilities_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerCannotUseMoveAbilities, actorSpawningMessage)
	{
	}
}

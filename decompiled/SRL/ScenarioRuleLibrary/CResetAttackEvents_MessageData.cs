namespace ScenarioRuleLibrary;

public class CResetAttackEvents_MessageData : CMessageData
{
	public CResetAttackEvents_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ResetAttackEvents, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CMoveBuff_MessageData : CMessageData
{
	public CAbilityMove m_MoveAbility;

	public CMoveBuff_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.MoveBuff, actorSpawningMessage, animOverload)
	{
	}
}

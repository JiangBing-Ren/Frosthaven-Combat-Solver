namespace ScenarioRuleLibrary;

public class CSelectExtraTurnCards_MessageData : CMessageData
{
	public CActor m_ActorTakingExtraTurn;

	public CAbilityExtraTurn m_ExtraTurnAbility;

	public CSelectExtraTurnCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectExtraTurnCards, actorSpawningMessage, animOverload)
	{
	}
}

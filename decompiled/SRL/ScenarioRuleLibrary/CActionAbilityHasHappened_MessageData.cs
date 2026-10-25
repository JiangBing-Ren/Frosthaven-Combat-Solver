namespace ScenarioRuleLibrary;

public class CActionAbilityHasHappened_MessageData : CMessageData
{
	public bool PendingElementsToInfuse;

	public CAbility m_Ability;

	public CActionAbilityHasHappened_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActionAbilityHasHappened, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CEndActorAbilityAnimSync_MessageData : CMessageData
{
	public bool m_IsLastAbility;

	public CAbility m_Ability;

	public bool m_AdvanceGameState;

	public CEndActorAbilityAnimSync_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndAbilityAnimSync, actorSpawningMessage)
	{
	}
}

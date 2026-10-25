namespace ScenarioRuleLibrary;

public class CEndActorAbility_MessageData : CMessageData
{
	public bool m_IsLastAbility;

	public bool m_AdvanceGameState;

	public CEndActorAbility_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndAbility, actorSpawningMessage)
	{
	}
}

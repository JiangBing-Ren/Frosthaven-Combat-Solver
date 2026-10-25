namespace ScenarioRuleLibrary;

public class CRecoverDiscardedCards_MessageData : CMessageData
{
	public CActor m_ActorRecoveringLostCards;

	public CAbility m_Ability;

	public CRecoverDiscardedCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.RecoverDiscardedCards, actorSpawningMessage, animOverload)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CRecoverLostCards_MessageData : CMessageData
{
	public CActor m_ActorRecoveringLostCards;

	public CAbility m_Ability;

	public CRecoverLostCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.RecoverLostCards, actorSpawningMessage, animOverload)
	{
	}
}

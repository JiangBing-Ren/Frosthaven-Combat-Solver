namespace ScenarioRuleLibrary;

public class CLoseCards_MessageData : CMessageData
{
	public CActor m_ActorLosingCards;

	public CAbility m_Ability;

	public CLoseCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.LoseCards, actorSpawningMessage, animOverload)
	{
	}
}

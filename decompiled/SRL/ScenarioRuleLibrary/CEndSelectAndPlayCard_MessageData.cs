namespace ScenarioRuleLibrary;

public class CEndSelectAndPlayCard_MessageData : CMessageData
{
	public CActor m_ActorReplayingCard;

	public CAbility m_Ability;

	public CEndSelectAndPlayCard_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.EndSelectAndPlayCard, actorSpawningMessage, animOverload)
	{
	}
}

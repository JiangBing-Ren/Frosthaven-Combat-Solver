namespace ScenarioRuleLibrary;

public class CSelectRecoverCards_MessageData : CMessageData
{
	public CActor m_ActorRecoveringLostCards;

	public CAbility m_Ability;

	public int m_MaxCardLevel;

	public CSelectRecoverCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectRecoverCards, actorSpawningMessage, animOverload)
	{
	}
}

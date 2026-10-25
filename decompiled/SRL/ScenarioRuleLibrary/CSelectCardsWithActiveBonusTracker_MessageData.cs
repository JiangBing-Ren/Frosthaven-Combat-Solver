namespace ScenarioRuleLibrary;

public class CSelectCardsWithActiveBonusTracker_MessageData : CMessageData
{
	public CActor m_Actor;

	public CAbility m_Ability;

	public bool m_FirstAbility;

	public CSelectCardsWithActiveBonusTracker_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectCardsWithActiveBonusTracker, actorSpawningMessage, animOverload)
	{
	}
}

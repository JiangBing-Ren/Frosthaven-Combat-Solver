namespace ScenarioRuleLibrary;

public class CNoValidCardsForAbility_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CNoValidCardsForAbility_MessageData(CActor actorSpawningMessage, CAbility ability)
		: base(MessageType.NoValidCardsForAbility, actorSpawningMessage)
	{
		m_Ability = ability;
	}
}

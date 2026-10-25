namespace ScenarioRuleLibrary;

public class CShowToggleBonusesForAI_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CShowToggleBonusesForAI_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowToggleBonusesForAI, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CShowForgoActiveBonusBar_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CShowForgoActiveBonusBar_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowForgoActiveBonus, actorSpawningMessage)
	{
	}
}

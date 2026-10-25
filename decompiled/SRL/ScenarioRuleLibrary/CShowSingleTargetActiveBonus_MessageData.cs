namespace ScenarioRuleLibrary;

public class CShowSingleTargetActiveBonus_MessageData : CMessageData
{
	public bool m_ShowSingleTargetActiveBonus;

	public CAbility m_Ability;

	public CShowSingleTargetActiveBonus_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowSingleTargetActiveBonus, actorSpawningMessage)
	{
	}
}

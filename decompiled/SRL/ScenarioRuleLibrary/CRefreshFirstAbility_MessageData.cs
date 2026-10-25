namespace ScenarioRuleLibrary;

public class CRefreshFirstAbility_MessageData : CMessageData
{
	public bool m_IsFirstAbility;

	public CAbility m_Ability;

	public CRefreshFirstAbility_MessageData(CActor actorSpawningMessage)
		: base(MessageType.RefreshFirstAbility, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CShowSelectAbility_MessageData : CMessageData
{
	public CAbilityChooseAbility m_ChooseAbility;

	public bool m_IsFirstAbility;

	public CShowSelectAbility_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowSelectAbility, actorSpawningMessage, string.Empty)
	{
	}
}

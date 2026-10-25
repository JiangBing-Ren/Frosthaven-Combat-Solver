namespace ScenarioRuleLibrary;

public class CStartedAbility_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CStartedAbility_MessageData()
		: base(MessageType.StartedAbility, null)
	{
	}
}

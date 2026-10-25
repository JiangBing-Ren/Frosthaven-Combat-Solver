namespace ScenarioRuleLibrary;

public class CStackedInlineSubAbilities_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CStackedInlineSubAbilities_MessageData()
		: base(MessageType.StackedInlineSubAbilities, null)
	{
	}
}

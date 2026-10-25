namespace ScenarioRuleLibrary;

public class CToggledActionAugmentation_MessageData : CMessageData
{
	public CAbility m_CurrentAbilityAfterToggling;

	public CActionAugmentation m_CurrentAugment;

	public CToggledActionAugmentation_MessageData()
		: base(MessageType.ToggledActionAugmentation, null)
	{
	}
}

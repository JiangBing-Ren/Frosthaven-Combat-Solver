namespace ScenarioRuleLibrary;

public class CFinishedProcessingItemToggle_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CFinishedProcessingItemToggle_MessageData()
		: base(MessageType.FinishedProcessingItemToggle, null)
	{
	}
}

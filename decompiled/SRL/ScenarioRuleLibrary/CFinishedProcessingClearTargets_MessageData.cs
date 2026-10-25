namespace ScenarioRuleLibrary;

public class CFinishedProcessingClearTargets_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CFinishedProcessingClearTargets_MessageData()
		: base(MessageType.FinishedProcessingClearTargets, null)
	{
	}
}

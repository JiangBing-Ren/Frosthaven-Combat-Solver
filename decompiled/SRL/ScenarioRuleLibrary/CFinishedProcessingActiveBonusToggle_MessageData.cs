namespace ScenarioRuleLibrary;

public class CFinishedProcessingActiveBonusToggle_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public int m_PhaseWhenClickedInt;

	public CFinishedProcessingActiveBonusToggle_MessageData()
		: base(MessageType.FinishedProcessingActiveBonusToggle, null)
	{
	}
}

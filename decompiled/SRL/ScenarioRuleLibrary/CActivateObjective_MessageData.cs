namespace ScenarioRuleLibrary;

public class CActivateObjective_MessageData : CMessageData
{
	public CObjective m_ActivatedObjective;

	public CActivateObjective_MessageData()
		: base(MessageType.ActivateObjective, null)
	{
	}
}

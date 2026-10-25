namespace ScenarioRuleLibrary;

public class CDeactivateObjective_MessageData : CMessageData
{
	public CObjective m_DeactivatedObjective;

	public CDeactivateObjective_MessageData()
		: base(MessageType.DeactivateObjective, null)
	{
	}
}

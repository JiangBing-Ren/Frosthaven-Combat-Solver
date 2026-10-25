namespace ScenarioRuleLibrary;

public class CSRLWrongPhaseException_MessageData : CMessageData
{
	public string m_CurrentPhaseType;

	public ScenarioRuleClient.EMessageType m_MessageType;

	public string m_Stack;

	public CSRLWrongPhaseException_MessageData(string currentPhaseType, ScenarioRuleClient.EMessageType messageType, string stack)
		: base(MessageType.SRLWrongPhaseExceptionMessage, null)
	{
		m_CurrentPhaseType = currentPhaseType;
		m_MessageType = messageType;
		m_Stack = stack;
	}
}

namespace ScenarioRuleLibrary;

public class CSRLException_MessageData : CMessageData
{
	public string m_Message;

	public string m_Stack;

	public CSRLException_MessageData(string message, string stack)
		: base(MessageType.SRLExceptionMessage, null)
	{
		m_Message = message;
		m_Stack = stack;
	}
}

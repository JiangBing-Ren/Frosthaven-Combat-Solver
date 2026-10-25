namespace ScenarioRuleLibrary;

public class CSRLQueueStatusLog_MessageData : CMessageData
{
	public ScenarioRuleClient.CSRLMessage m_SRLMessage;

	public CSRLQueueStatusLog_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SRLQueueDebugLog, actorSpawningMessage)
	{
	}
}

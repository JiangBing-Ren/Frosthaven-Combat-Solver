namespace ScenarioRuleLibrary;

public class CAIFindingPath_MessageData : CMessageData
{
	public CActor m_TargetActor;

	public string m_Reason;

	public CAIFindingPath_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AIFindingPath, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CPauseLoco_MessageData : CMessageData
{
	public bool m_Pause;

	public CPauseLoco_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PauseLoco, actorSpawningMessage)
	{
	}
}

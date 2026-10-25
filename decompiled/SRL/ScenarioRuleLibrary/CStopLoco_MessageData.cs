namespace ScenarioRuleLibrary;

public class CStopLoco_MessageData : CMessageData
{
	public bool m_Pause;

	public CStopLoco_MessageData(CActor actorSpawningMessage)
		: base(MessageType.StopLoco, actorSpawningMessage)
	{
	}
}

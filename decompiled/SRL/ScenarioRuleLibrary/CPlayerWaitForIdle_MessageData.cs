namespace ScenarioRuleLibrary;

public class CPlayerWaitForIdle_MessageData : CMessageData
{
	public CActor m_Actor;

	public CPlayerWaitForIdle_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerWaitForIdle, actorSpawningMessage)
	{
	}
}

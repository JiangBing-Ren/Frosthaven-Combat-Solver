namespace ScenarioRuleLibrary;

public class CStartTurn_MessageData : CMessageData
{
	public bool m_SkipNextPhase;

	public CStartTurn_MessageData(CActor actorSpawningMessage, bool skipNextPhase = false)
		: base(MessageType.StartTurn, actorSpawningMessage)
	{
		m_SkipNextPhase = skipNextPhase;
	}
}

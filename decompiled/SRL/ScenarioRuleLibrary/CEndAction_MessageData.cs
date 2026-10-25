namespace ScenarioRuleLibrary;

public class CEndAction_MessageData : CMessageData
{
	public bool m_ActionHappened;

	public string m_ActionName;

	public CBaseCard m_Card;

	public CEndAction_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndAction, actorSpawningMessage)
	{
	}
}

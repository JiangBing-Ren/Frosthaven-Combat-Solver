namespace ScenarioRuleLibrary;

public class CUnpauseAura_MessageData : CMessageData
{
	public int m_AuraBaseCardID;

	public string m_AuraBaseCardName;

	public CUnpauseAura_MessageData(CActor actorSpawningMessage)
		: base(MessageType.UnpauseAura, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CFinishAura_MessageData : CMessageData
{
	public int m_AuraAbilityID;

	public CFinishAura_MessageData(CActor actorSpawningMessage)
		: base(MessageType.FinishAura, actorSpawningMessage)
	{
	}
}

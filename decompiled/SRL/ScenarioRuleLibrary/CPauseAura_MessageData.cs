namespace ScenarioRuleLibrary;

public class CPauseAura_MessageData : CMessageData
{
	public int m_AuraAbilityID;

	public CPauseAura_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PauseAura, actorSpawningMessage)
	{
	}
}

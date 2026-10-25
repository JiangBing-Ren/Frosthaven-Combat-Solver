namespace ScenarioRuleLibrary;

public class CModeCardChanged_MessageData : CMessageData
{
	public CAbilityCard m_NewModeCard;

	public CModeCardChanged_MessageData(CAbilityCard newModeCard, CActor actorSpawningMessage)
		: base(MessageType.ModeCardChanged, actorSpawningMessage)
	{
		m_NewModeCard = newModeCard;
	}
}

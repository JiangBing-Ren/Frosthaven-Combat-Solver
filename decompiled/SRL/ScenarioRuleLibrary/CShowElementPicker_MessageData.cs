namespace ScenarioRuleLibrary;

public class CShowElementPicker_MessageData : CMessageData
{
	public CAbilityInfuse m_InfuseAbility;

	public CAbilityConsumeElement m_ConsumeAbility;

	public CShowElementPicker_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowElementPicker, actorSpawningMessage, string.Empty)
	{
	}
}

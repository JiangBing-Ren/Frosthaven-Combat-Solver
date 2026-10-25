namespace ScenarioRuleLibrary;

public class CChosenAbilityChanged_MessageData : CMessageData
{
	public CAbilityChooseAbility m_ChooseAbility;

	public CChosenAbilityChanged_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ChosenAbilityChanged, actorSpawningMessage, string.Empty)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CGenericAbilityInvalidHelpText_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CGenericAbilityInvalidHelpText_MessageData(CActor actorSpawningMessage, CAbility ability)
		: base(MessageType.GenericAbilityInvalidHelpText, actorSpawningMessage)
	{
		m_Ability = ability;
	}
}

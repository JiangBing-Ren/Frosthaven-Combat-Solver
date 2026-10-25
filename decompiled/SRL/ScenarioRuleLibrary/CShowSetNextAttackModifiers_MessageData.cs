namespace ScenarioRuleLibrary;

public class CShowSetNextAttackModifiers_MessageData : CMessageData
{
	public CAbilitySetNextAttackModifiers m_ChooseAbility;

	public bool m_IsFirstAbility;

	public CShowSetNextAttackModifiers_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowSetNextAttackModifiers, actorSpawningMessage, string.Empty)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CAbilityEndUpdateCombatLog_MessageData : CMessageData
{
	public CAbility.EAbilityType m_AbilityType;

	public CAbility m_Ability;

	public CAbilityEndUpdateCombatLog_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AbilityEndUpdateCombatLog, actorSpawningMessage)
	{
	}
}

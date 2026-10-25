namespace ScenarioRuleLibrary;

public class CAbilityStartUpdateCombatLog_MessageData : CMessageData
{
	public CAbility.EAbilityType m_AbilityType;

	public CAbility m_Ability;

	public SEventAction m_Event;

	public CAbilityStartUpdateCombatLog_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AbilityStartUpdateCombatLog, actorSpawningMessage)
	{
	}
}

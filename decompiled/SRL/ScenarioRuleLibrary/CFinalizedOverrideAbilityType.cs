namespace ScenarioRuleLibrary;

public class CFinalizedOverrideAbilityType : CMessageData
{
	public CAbility m_Ability;

	public CFinalizedOverrideAbilityType(CActor actor)
		: base(MessageType.FinalizedOverrideAbilityType, actor)
	{
	}
}

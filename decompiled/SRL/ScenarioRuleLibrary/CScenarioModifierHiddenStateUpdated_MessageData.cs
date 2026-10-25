namespace ScenarioRuleLibrary;

public class CScenarioModifierHiddenStateUpdated_MessageData : CMessageData
{
	public CScenarioModifier m_UpdatedModifier;

	public CScenarioModifierHiddenStateUpdated_MessageData()
		: base(MessageType.ScenarioModifierHiddenStateUpdated, null)
	{
	}
}

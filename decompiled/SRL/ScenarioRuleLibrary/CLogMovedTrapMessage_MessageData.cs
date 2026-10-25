namespace ScenarioRuleLibrary;

public class CLogMovedTrapMessage_MessageData : CMessageData
{
	public SEventAbilityTrap m_Event;

	public CLogMovedTrapMessage_MessageData(CActor actor)
		: base(MessageType.LogMovedTrap, actor)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CUpdateTrapVisualState_MessageData : CMessageData
{
	public CObjectTrap m_trap;

	public float m_delay;

	public CUpdateTrapVisualState_MessageData()
		: base(MessageType.UpdateTrapVisualState, null, string.Empty)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CChangeActorVisualState_MessageData : CMessageData
{
	public string m_stateName;

	public CActor m_actor;

	public float m_delay;

	public string m_DestructionVFXName = string.Empty;

	public float m_DestructionVFXDelay;

	public CChangeActorVisualState_MessageData()
		: base(MessageType.ChangeActorVisualState, null, string.Empty)
	{
	}
}

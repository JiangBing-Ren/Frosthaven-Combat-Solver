namespace ScenarioRuleLibrary;

public class CChangePropVisualState_MessageData : CMessageData
{
	public string m_stateName;

	public CObjectProp m_prop;

	public float m_delay;

	public string m_DestructionVFXName = string.Empty;

	public float m_DestructionVFXDelay;

	public CChangePropVisualState_MessageData()
		: base(MessageType.ChangePropVisualState, null, string.Empty)
	{
	}
}

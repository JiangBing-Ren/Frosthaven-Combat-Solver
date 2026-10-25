namespace ScenarioRuleLibrary;

public class CSetPropChildActive_MessageData : CMessageData
{
	public CObjectProp m_prop;

	public string m_childName;

	public bool m_active;

	public CSetPropChildActive_MessageData()
		: base(MessageType.SetPropChildActive, null, string.Empty)
	{
	}
}

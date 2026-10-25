namespace ScenarioRuleLibrary;

public class CHidePropChild_MessageData : CMessageData
{
	public string m_childName;

	public bool m_hidden;

	public CObjectProp m_prop;

	public float m_delay;

	public CHidePropChild_MessageData()
		: base(MessageType.HidePropChild, null, string.Empty)
	{
	}
}

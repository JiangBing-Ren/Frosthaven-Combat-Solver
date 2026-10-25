namespace ScenarioRuleLibrary;

public class CPlaySound_MessageData : CMessageData
{
	public string m_AudioItem;

	public CPlaySound_MessageData(string audioItem)
		: base(MessageType.PlaySound, null)
	{
		m_AudioItem = audioItem;
	}
}

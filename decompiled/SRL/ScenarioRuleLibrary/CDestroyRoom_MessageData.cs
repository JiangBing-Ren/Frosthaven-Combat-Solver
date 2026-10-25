namespace ScenarioRuleLibrary;

public class CDestroyRoom_MessageData : CMessageData
{
	public CMap m_MapToDestroy;

	public bool m_RevealMonsters = true;

	public CDestroyRoom_MessageData()
		: base(MessageType.DestroyRoom, null)
	{
	}
}

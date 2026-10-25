namespace ScenarioRuleLibrary;

public class HideShowChildObject_MessageData : CMessageData
{
	public string m_GOName;

	public bool m_Show;

	public bool m_SilentFail;

	public bool m_waitForActorCreation;

	public HideShowChildObject_MessageData(CActor actorSpawningMessage)
		: base(MessageType.HideShowChildObject, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CLockUnlockedDoor_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public bool m_InitialLoad;

	public CLockUnlockedDoor_MessageData(CActor actorSpawningMessage)
		: base(MessageType.LockUnlockedDoor, actorSpawningMessage)
	{
	}
}

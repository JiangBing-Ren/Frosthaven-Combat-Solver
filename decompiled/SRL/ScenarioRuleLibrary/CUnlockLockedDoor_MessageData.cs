namespace ScenarioRuleLibrary;

public class CUnlockLockedDoor_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public bool m_InitialLoad;

	public CUnlockLockedDoor_MessageData(CActor actorSpawningMessage)
		: base(MessageType.UnlockLockedDoor, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CActiveBonusDoomSlotChoice_MessageData : CMessageData
{
	public CActor m_DoomTargetActor;

	public CDoom m_NewDoom;

	public CActiveBonusDoomSlotChoice_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusDoomSlotChoice, actorSpawningMessage)
	{
	}
}

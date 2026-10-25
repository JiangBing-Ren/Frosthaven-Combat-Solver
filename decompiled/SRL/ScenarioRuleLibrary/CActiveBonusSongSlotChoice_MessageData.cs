namespace ScenarioRuleLibrary;

public class CActiveBonusSongSlotChoice_MessageData : CMessageData
{
	public CActor m_Actor;

	public CAbility m_Ability;

	public CActiveBonusSongSlotChoice_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusSongSlotChoice, actorSpawningMessage)
	{
	}
}

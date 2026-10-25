namespace ScenarioRuleLibrary;

public class CActiveBonusSongAdded_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public CActor m_Actor;

	public CActiveBonusSongAdded_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusSongAdded, actorSpawningMessage)
	{
	}
}

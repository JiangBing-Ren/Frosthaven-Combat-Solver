namespace ScenarioRuleLibrary;

public class CActiveBonusAugmentAdded_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public CActor m_Actor;

	public CActiveBonusAugmentAdded_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusAugmentAdded, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CActiveBonusTrackerIncremented_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public CActor m_Actor;

	public CActiveBonusTrackerIncremented_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusTrackerIncremented, actorSpawningMessage)
	{
	}
}

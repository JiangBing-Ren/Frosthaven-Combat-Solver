namespace ScenarioRuleLibrary;

public class CActiveBonusAugmentSlotChoice_MessageData : CMessageData
{
	public CActor m_Actor;

	public CAbility m_Ability;

	public CActiveBonusAugmentSlotChoice_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActiveBonusAugmentSlotChoice, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CAdvantage_MessageData : CMessageData
{
	public CAbilityAdvantage m_AdvantageAbility;

	public CActor m_AdvantagedActor;

	public bool m_ConditionAlreadyApplied;

	public CAdvantage_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Advantage, actorSpawningMessage, animOverload)
	{
	}
}

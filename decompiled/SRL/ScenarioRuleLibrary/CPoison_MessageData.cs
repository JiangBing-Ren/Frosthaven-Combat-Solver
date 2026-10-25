namespace ScenarioRuleLibrary;

public class CPoison_MessageData : CMessageData
{
	public CActor m_PoisonedActor;

	public bool m_ConditionAlreadyApplied;

	public CPoison_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Poison, actorSpawningMessage, animOverload)
	{
	}
}

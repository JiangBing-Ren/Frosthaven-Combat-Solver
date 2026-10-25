namespace ScenarioRuleLibrary;

public class CImpair_MessageData : CMessageData
{
	public CActor m_ActorToImpair;

	public bool m_ConditionAlreadyApplied;

	public CImpair_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Impair, actorSpawningMessage, animOverload)
	{
	}
}

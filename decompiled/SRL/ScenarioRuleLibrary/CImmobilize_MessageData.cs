namespace ScenarioRuleLibrary;

public class CImmobilize_MessageData : CMessageData
{
	public CActor m_ActorToImmobilize;

	public bool m_ConditionAlreadyApplied;

	public CImmobilize_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Immobilize, actorSpawningMessage, animOverload)
	{
	}
}

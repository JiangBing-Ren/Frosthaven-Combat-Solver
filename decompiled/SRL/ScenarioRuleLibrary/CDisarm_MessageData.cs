namespace ScenarioRuleLibrary;

public class CDisarm_MessageData : CMessageData
{
	public CActor m_ActorToDisarm;

	public bool m_ConditionAlreadyApplied;

	public CDisarm_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Disarm, actorSpawningMessage, animOverload)
	{
	}
}

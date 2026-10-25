namespace ScenarioRuleLibrary;

public class CMutate_MessageData : CMessageData
{
	public CActor m_Actor;

	public bool m_ConditionAlreadyApplied;

	public CMutate_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Mutate, actorSpawningMessage, animOverload)
	{
	}
}

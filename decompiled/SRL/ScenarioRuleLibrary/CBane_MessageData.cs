namespace ScenarioRuleLibrary;

public class CBane_MessageData : CMessageData
{
	public CActor m_ActorToBane;

	public bool m_ConditionAlreadyApplied;

	public CBane_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Bane, actorSpawningMessage, animOverload)
	{
	}
}

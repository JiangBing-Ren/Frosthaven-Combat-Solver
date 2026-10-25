namespace ScenarioRuleLibrary;

public class CInvisible_MessageData : CMessageData
{
	public CActor m_ActorToMakeInvisible;

	public bool m_ConditionAlreadyApplied;

	public CInvisible_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Invisible, actorSpawningMessage, animOverload)
	{
	}
}

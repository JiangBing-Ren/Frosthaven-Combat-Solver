namespace ScenarioRuleLibrary;

public class CBless_MessageData : CMessageData
{
	public CActor m_ActorToBless;

	public bool m_DifficultyModBless;

	public bool m_ConditionAlreadyApplied;

	public CBless_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Bless, actorSpawningMessage, animOverload)
	{
	}
}

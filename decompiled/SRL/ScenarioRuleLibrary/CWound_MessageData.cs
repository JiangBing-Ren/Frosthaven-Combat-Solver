namespace ScenarioRuleLibrary;

public class CWound_MessageData : CMessageData
{
	public CActor m_ActorToWound;

	public bool m_ConditionAlreadyApplied;

	public CWound_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Wound, actorSpawningMessage, animOverload)
	{
	}
}

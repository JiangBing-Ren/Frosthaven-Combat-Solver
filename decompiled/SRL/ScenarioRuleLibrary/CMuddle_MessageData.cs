namespace ScenarioRuleLibrary;

public class CMuddle_MessageData : CMessageData
{
	public CActor m_ActorToMuddle;

	public bool m_ConditionAlreadyApplied;

	public CMuddle_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Muddle, actorSpawningMessage, animOverload)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CStun_MessageData : CMessageData
{
	public CActor m_ActorToStun;

	public bool m_ConditionAlreadyApplied;

	public CStun_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Stun, actorSpawningMessage, animOverload)
	{
	}
}

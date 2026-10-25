namespace ScenarioRuleLibrary;

public class CStrengthen_MessageData : CMessageData
{
	public CActor m_ActorToStrengthen;

	public bool m_ConditionAlreadyApplied;

	public CStrengthen_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Strengthen, actorSpawningMessage, animOverload)
	{
	}
}

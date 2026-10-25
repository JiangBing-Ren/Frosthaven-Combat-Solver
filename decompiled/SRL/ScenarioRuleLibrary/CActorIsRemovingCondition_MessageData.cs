namespace ScenarioRuleLibrary;

public class CActorIsRemovingCondition_MessageData : CMessageData
{
	public CActor m_ActorAppliedTo;

	public CAbility m_Ability;

	public CCondition.ENegativeCondition m_NegativeCondition;

	public CActorIsRemovingCondition_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsRemovingCondition, actorSpawningMessage, animOverload)
	{
	}
}

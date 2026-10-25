namespace ScenarioRuleLibrary;

public class CTargetAttackBuff_MessageData : CMessageData
{
	public CActor m_ActorAttacking;

	public CActor m_ActorBeingAttacked;

	public CTargetAttackBuff_MessageData(CActor actorSpawningMessage)
		: base(MessageType.TargetAttackBuff, actorSpawningMessage)
	{
	}
}

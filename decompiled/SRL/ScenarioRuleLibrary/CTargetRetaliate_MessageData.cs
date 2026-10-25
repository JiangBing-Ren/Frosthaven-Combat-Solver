namespace ScenarioRuleLibrary;

public class CTargetRetaliate_MessageData : CMessageData
{
	public CActor m_ActorAttacking;

	public CActor m_ActorBeingAttacked;

	public int m_retaliateBuff;

	public CTargetRetaliate_MessageData(CActor actorSpawningMessage)
		: base(MessageType.TargetRetaliate, actorSpawningMessage)
	{
	}
}

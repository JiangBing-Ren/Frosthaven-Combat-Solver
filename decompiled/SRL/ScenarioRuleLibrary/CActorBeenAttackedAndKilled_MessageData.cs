namespace ScenarioRuleLibrary;

public class CActorBeenAttackedAndKilled_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CActor m_ActorBeingAttacked;

	public CAreaEffect AreaEffect;

	public int m_ActorOriginalHealth;

	public CAbilityAttack m_AttackAbility;

	public int m_AttackIndex;

	public CActorBeenAttackedAndKilled_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorBeenAttackedAndKilled, actorSpawningMessage)
	{
	}
}

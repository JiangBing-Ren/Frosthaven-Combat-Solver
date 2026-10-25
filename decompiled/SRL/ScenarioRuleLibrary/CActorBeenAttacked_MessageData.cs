namespace ScenarioRuleLibrary;

public class CActorBeenAttacked_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CActor m_ActorBeingAttacked;

	public CAreaEffect AreaEffect;

	public int m_ActorOriginalHealth;

	public CAbilityAttack m_AttackAbility;

	public int m_AttackIndex;

	public bool m_ActorWasAsleep;

	public bool m_LogDamageCalculationOnly;

	public CActorBeenAttacked_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorBeenAttacked, actorSpawningMessage)
	{
	}
}

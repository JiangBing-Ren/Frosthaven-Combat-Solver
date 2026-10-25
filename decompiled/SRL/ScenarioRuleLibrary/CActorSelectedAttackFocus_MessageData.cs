namespace ScenarioRuleLibrary;

public class CActorSelectedAttackFocus_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbility m_Ability;

	public CActor m_AttackFocus;

	public bool m_Adding;

	public CActorSelectedAttackFocus_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorSelectedAttackFocus, actorSpawningMessage)
	{
	}
}

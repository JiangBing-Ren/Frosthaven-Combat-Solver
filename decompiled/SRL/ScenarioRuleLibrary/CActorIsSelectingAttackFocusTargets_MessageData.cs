namespace ScenarioRuleLibrary;

public class CActorIsSelectingAttackFocusTargets_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_AttackAbility;

	public CAttackSummary m_AttackSummary;

	public CActorIsSelectingAttackFocusTargets_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingAttackFocusTargets, actorSpawningMessage)
	{
	}
}

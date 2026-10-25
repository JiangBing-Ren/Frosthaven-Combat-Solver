using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsAttacking_MessageData : CMessageData
{
	public CAbilityAttack m_AttackAbility;

	public CActor m_AttackingActor;

	public CActor m_ActorBeingAttacked;

	public List<CActor> m_ActorsAttackingAll;

	public CAttackSummary.TargetSummary m_TargetSummary;

	public CActorIsAttacking_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsAttacking, actorSpawningMessage)
	{
	}
}

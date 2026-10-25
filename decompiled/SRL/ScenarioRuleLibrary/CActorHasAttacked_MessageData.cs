using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CActorHasAttacked_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_AttackAbility;

	public List<CActor> m_AttackedActors;

	public AbilityData.EActiveBonusFlag m_ActiveBonusesFlags;

	public CActorHasAttacked_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorHasAttacked, actorSpawningMessage, animOverload)
	{
	}
}

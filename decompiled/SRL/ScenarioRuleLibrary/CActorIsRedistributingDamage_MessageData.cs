using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsRedistributingDamage_MessageData : CMessageData
{
	public List<CActor> m_ActorsRedistributingDamageTo;

	public CAbilityRedistributeDamage m_RedistributeDamageAbility;

	public CActorIsRedistributingDamage_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsRedistributingDamage, actorSpawningMessage)
	{
	}
}

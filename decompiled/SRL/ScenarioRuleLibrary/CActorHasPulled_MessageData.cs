using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorHasPulled_MessageData : CMessageData
{
	public List<CTile> m_Waypoints;

	public CAbilityPull m_PullAbility;

	public CActorHasPulled_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorHasPulled, actorSpawningMessage)
	{
	}
}

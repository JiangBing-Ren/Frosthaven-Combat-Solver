using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsPulling_MessageData : CMessageData
{
	public List<CTile> m_Waypoints;

	public CAbilityPull m_PullAbility;

	public CActorIsPulling_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsPulling, actorSpawningMessage)
	{
	}
}

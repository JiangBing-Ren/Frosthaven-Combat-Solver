using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsPushing_MessageData : CMessageData
{
	public List<CTile> m_Waypoints;

	public CAbilityPush m_PushAbility;

	public bool m_SkipPushAnim;

	public CActorIsPushing_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsPushing, actorSpawningMessage)
	{
	}
}

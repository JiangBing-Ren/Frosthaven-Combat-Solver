using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsTeleporting2_MessageData : CMessageData
{
	public List<CTile> m_Waypoints;

	public CAbilityTeleport2 m_Teleport2Ability;

	public CActor m_ActorToTeleport;

	public CActorIsTeleporting2_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsTeleporting2, actorSpawningMessage)
	{
	}
}

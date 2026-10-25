using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorHasTeleported2_MessageData : CMessageData
{
	public List<CTile> m_Waypoints;

	public CAbilityTeleport2 m_Teleport2Ability;

	public CActor m_ActorToTeleport;

	public CTile m_DestinationTile;

	public CActorHasTeleported2_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorHasTeleported2, actorSpawningMessage)
	{
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorHasMoved_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorHasMoved, actorSpawningMessage)
{
	public CAbility m_Ability;

	public CActor m_MovingActor;

	public List<CActor> m_ActorsToCarry;

	public bool m_Jump;

	public List<CTile> m_Waypoints;

	public bool m_ForceMoveViaLoco;

	public bool m_ShouldWait = true;

	public bool m_DontTeleport;

	public bool m_Slide;

	public float m_SlideSpeed;

	public float m_ForcedSpeed;

	public bool m_UseActorRunSpeedAsSlideSpeed;
}

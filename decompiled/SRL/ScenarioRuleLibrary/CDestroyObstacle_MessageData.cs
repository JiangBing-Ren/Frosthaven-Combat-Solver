using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDestroyObstacle_MessageData : CMessageData
{
	public CActor m_ActorDestroyingObstacle;

	public List<CTile> m_Tiles;

	public List<CObjectProp> m_DestroyedProps;

	public float m_DestroyDelay;

	public CAbility m_DestroyObstacleAbility;

	public bool m_FaceObstacle;

	public bool m_OverrideSetLastSelectedTile;

	public CDestroyObstacle_MessageData(string animOverload, CActor actorSpawningMessage, bool faceObstacle = true)
		: base(MessageType.DestroyObstacle, actorSpawningMessage, animOverload)
	{
		m_FaceObstacle = faceObstacle;
	}
}

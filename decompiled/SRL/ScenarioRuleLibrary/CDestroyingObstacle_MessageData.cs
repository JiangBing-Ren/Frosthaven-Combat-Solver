using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDestroyingObstacle_MessageData : CMessageData
{
	public CActor m_ActorDestroyingObstacle;

	public List<CTile> m_Tiles;

	public List<CObjectProp> m_DestroyedProps;

	public CAbility m_DestroyObstacleAbility;

	public bool m_FaceObstacle;

	public bool m_OverrideSetLastSelectedTile;

	public CDestroyingObstacle_MessageData(string animOverload, CActor actorSpawningMessage, bool faceObstacle = true)
		: base(MessageType.DestroyingObstacle, actorSpawningMessage, animOverload)
	{
		m_FaceObstacle = faceObstacle;
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CCreatingTerrain_MessageData : CMessageData
{
	public CActor m_ActorCreatingTerrain;

	public List<CTile> m_Tiles;

	public CAbility m_Ability;

	public CCreatingTerrain_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.CreatingTerrain, actorSpawningMessage, animOverload)
	{
	}
}

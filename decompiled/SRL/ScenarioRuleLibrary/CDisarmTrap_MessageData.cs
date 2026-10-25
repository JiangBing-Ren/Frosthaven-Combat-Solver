using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDisarmTrap_MessageData : CMessageData
{
	public CActor m_ActorDisarmingTrap;

	public List<CTile> m_Tiles;

	public List<CObjectTrap> m_DisarmedTraps;

	public CAbility m_DisarmTrapAbility;

	public CDisarmTrap_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.DisarmTrap, actorSpawningMessage, animOverload)
	{
	}
}

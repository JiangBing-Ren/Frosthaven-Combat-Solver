using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlacingTrap_MessageData : CMessageData
{
	public CActor m_ActorPlacingTrap;

	public List<CTile> m_Tiles;

	public CAbility m_TrapAbility;

	public CObjectTrap m_TrapObject;

	public CPlacingTrap_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PlacingTrap, actorSpawningMessage, animOverload)
	{
	}
}

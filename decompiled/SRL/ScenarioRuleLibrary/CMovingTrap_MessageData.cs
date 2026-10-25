using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CMovingTrap_MessageData : CMessageData
{
	public CAbility m_Ability;

	public List<CTile> m_From;

	public List<CTile> m_To;

	public CMovingTrap_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.MovingTrap, actorSpawningMessage, animOverload)
	{
	}
}

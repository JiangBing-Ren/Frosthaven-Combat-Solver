using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CRevive_MessageData : CMessageData
{
	public CActor m_ActorReviving;

	public CAbilityRevive m_ReviveAbility;

	public List<CTile> m_ReviveTiles;

	public CRevive_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Revive, actorSpawningMessage, animOverload)
	{
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CMoveActiveBonusTrackers : CMessageData
{
	public CActor m_Actor;

	public List<CAbilityCard> m_Cards;

	public CAbilityMoveActiveBonusTracker m_Ability;

	public CMoveActiveBonusTrackers(CActor actorSpawningMessage)
		: base(MessageType.MoveActiveBonusTrackers, actorSpawningMessage)
	{
	}
}

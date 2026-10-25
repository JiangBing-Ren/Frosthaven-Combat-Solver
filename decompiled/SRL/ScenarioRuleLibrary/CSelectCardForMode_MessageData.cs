using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSelectCardForMode_MessageData : CMessageData
{
	public CActor m_ActorReplayingCard;

	public CAbility m_Ability;

	public List<int> m_ValidCards;

	public CBaseCard.ECardPile m_PileToChooseFrom;

	public CSelectCardForMode_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectCardForMode, actorSpawningMessage, animOverload)
	{
	}
}

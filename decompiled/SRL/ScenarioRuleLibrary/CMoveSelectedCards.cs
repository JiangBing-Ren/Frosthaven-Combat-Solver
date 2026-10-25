using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CMoveSelectedCards : CMessageData
{
	public CActor m_ActorRecoveringCards;

	public CBaseCard.ECardPile m_MoveFromPile;

	public CBaseCard.ECardPile m_MoveToPile;

	public int m_quantity;

	public List<CAbilityCard> m_Cards;

	public CMoveSelectedCards(CActor actorSpawningMessage)
		: base(MessageType.MoveSelectedCards, actorSpawningMessage)
	{
	}
}

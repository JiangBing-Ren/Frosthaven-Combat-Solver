using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSelectAndMoveCard_MessageData : CMessageData
{
	public CActor m_ActorReplayingCard;

	public CAbilitySelectAndMoveCard m_Ability;

	public List<int> m_ValidCards;

	public List<CBaseCard.ECardPile> m_PilesToChooseFrom;

	public CSelectAndMoveCard_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectAndMoveCard, actorSpawningMessage, animOverload)
	{
	}
}

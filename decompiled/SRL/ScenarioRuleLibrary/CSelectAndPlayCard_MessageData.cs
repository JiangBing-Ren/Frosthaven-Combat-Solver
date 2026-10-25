using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSelectAndPlayCard_MessageData : CMessageData
{
	public CActor m_ActorReplayingCard;

	public CAbilitySelectAndPlayCard m_Ability;

	public List<int> m_ValidCards;

	public List<CBaseCard.ECardPile> m_PilesToChooseFrom;

	public CSelectAndPlayCard_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectAndPlayCard, actorSpawningMessage, animOverload)
	{
	}
}

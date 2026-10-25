using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSelectCard_MessageData : CMessageData
{
	public CActor m_ActorReplayingCard;

	public CAbilitySelectCard m_Ability;

	public List<int> m_ValidCards;

	public List<CBaseCard.ECardPile> m_PilesToChooseFrom;

	public CSelectCard_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectCard, actorSpawningMessage, animOverload)
	{
	}
}

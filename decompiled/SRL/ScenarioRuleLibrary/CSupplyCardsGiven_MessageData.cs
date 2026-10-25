using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSupplyCardsGiven_MessageData : CMessageData
{
	public CPlayerActor m_ActorGivenCards;

	public List<CAbilityCard> m_SupplyCardsGiven;

	public CSupplyCardsGiven_MessageData(CActor actorSpawningMessage)
		: base(MessageType.SupplyCardsGiven, actorSpawningMessage)
	{
	}
}

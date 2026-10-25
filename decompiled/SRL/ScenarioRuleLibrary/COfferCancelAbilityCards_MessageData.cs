namespace ScenarioRuleLibrary;

public class COfferCancelAbilityCards_MessageData : CMessageData
{
	public int m_SuggestedCancelCount;

	public COfferCancelAbilityCards_MessageData(CActor actorSpawningMessage)
		: base(MessageType.OfferCancelAbilityCards, actorSpawningMessage)
	{
	}
}

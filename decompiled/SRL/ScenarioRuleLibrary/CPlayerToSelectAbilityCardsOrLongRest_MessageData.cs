namespace ScenarioRuleLibrary;

public class CPlayerToSelectAbilityCardsOrLongRest_MessageData : CMessageData
{
	public bool m_SyncMultiplayer;

	public CPlayerToSelectAbilityCardsOrLongRest_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerToSelectAbilityCardsOrLongRest, actorSpawningMessage)
	{
	}
}

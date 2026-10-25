namespace ScenarioRuleLibrary;

public class CPlayerFinishedMovingAbilityCard : CMessageData
{
	public bool m_SendNetworkSelectedRoundCards;

	public CPlayerFinishedMovingAbilityCard(CActor actorSpawningMessage)
		: base(MessageType.FinishedMovingAbilityCard, actorSpawningMessage)
	{
	}
}

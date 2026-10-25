namespace ScenarioRuleLibrary;

public class CReturnToSummoner_MessageData : CMessageData
{
	public CActor m_Summoner;

	public CReturnToSummoner_MessageData(CActor actorSpawningMessage, CActor summoner)
		: base(MessageType.ReturnToSummoner, actorSpawningMessage)
	{
		m_Summoner = summoner;
	}
}

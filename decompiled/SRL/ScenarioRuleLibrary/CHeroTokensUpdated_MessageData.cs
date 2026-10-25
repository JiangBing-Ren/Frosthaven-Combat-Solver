namespace ScenarioRuleLibrary;

public class CHeroTokensUpdated_MessageData : CMessageData
{
	public int m_TimeTokens;

	public int m_ResonanceTokens;

	public int m_TrophyTokens;

	public CHeroTokensUpdated_MessageData(CActor targetActor)
		: base(MessageType.HeroTokensUpdated, targetActor)
	{
	}
}

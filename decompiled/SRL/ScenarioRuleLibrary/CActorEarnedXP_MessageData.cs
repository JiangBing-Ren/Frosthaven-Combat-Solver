namespace ScenarioRuleLibrary;

public class CActorEarnedXP_MessageData : CMessageData
{
	public int m_xpAmount;

	public int m_scenarioXP;

	public CActorEarnedXP_MessageData(CActor actorEarningXp)
		: base(MessageType.ActorEarnedXP, actorEarningXp)
	{
	}
}

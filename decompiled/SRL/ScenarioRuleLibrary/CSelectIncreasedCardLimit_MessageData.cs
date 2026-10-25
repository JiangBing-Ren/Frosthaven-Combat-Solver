namespace ScenarioRuleLibrary;

public class CSelectIncreasedCardLimit_MessageData : CMessageData
{
	public CActor m_ActorIncreasingCardLimit;

	public CAbility m_Ability;

	public CSelectIncreasedCardLimit_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectIncreasedCardLimit, actorSpawningMessage, animOverload)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CLoseGoalChestRewardChoice_MessageData : CMessageData
{
	public CAbilityLoseGoalChestReward m_Ability;

	public CLoseGoalChestRewardChoice_MessageData()
		: base(MessageType.LoseGoalChestRewardChoice, null)
	{
	}
}

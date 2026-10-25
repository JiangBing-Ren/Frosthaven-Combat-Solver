using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public abstract class ScenarioRewardManager : Singleton<ScenarioRewardManager>
{
	public class RewardInfo
	{
		public CActor actor;

		public List<Reward> rewards;

		public string sourceType;
	}

	public List<RewardInfo> rewardInfoStack = new List<RewardInfo>();

	public RewardInfo currentRewardInfo;

	public abstract bool IsShown { get; }

	public bool ReadyToShow => rewardInfoStack.Count > 0;

	public abstract void Show(Action onFinish);

	public abstract void StackRewardsToShow(CActor actor, List<Reward> rewards, string sourceType);
}

using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class CampaignScenarioRewardManager : ScenarioRewardManager
{
	[SerializeField]
	private CampaignRewardsManager manager;

	private bool isShown;

	private Action onFinish;

	private ActionPhaseType previousPhase;

	public override bool IsShown => isShown;

	public override void Show(Action onFinish)
	{
		this.onFinish = onFinish;
		IEnumerable<RewardInfo> source = from x in rewardInfoStack
			group x by new { x.sourceType, x.actor } into x
			select new RewardInfo
			{
				rewards = x.SelectMany((RewardInfo y) => y.rewards).ToList(),
				sourceType = x.First().sourceType,
				actor = x.First().actor
			};
		rewardInfoStack = source.ToList();
		foreach (RewardInfo item in rewardInfoStack)
		{
			List<Reward> list = item.rewards.Where((Reward x) => x.Type == ETreasureType.Resource).ToList();
			IEnumerable<IGrouping<LootCard.EType, Reward>> enumerable = from x in list
				group x by x.ResourceType;
			foreach (Reward item2 in list)
			{
				item.rewards.Remove(item2);
			}
			foreach (IGrouping<LootCard.EType, Reward> item3 in enumerable)
			{
				Reward reward = item3.First();
				int num = item3.Sum((Reward x) => x.Amount);
				if (num != 0)
				{
					item.rewards.Add(new Reward(reward.ResourceType, num, ETreasureDistributionType.Combined, item.actor.Class.ID));
				}
			}
			List<Reward> list2 = item.rewards.Where((Reward x) => x.Type == ETreasureType.Gold).ToList();
			foreach (Reward item4 in list2)
			{
				item.rewards.Remove(item4);
			}
			int num2 = list2.Sum((Reward x) => x.Amount);
			if (num2 > 0)
			{
				item.rewards.Add(new Reward(ETreasureType.Gold, num2, ETreasureDistributionType.Combined, item.actor.Class.ID));
			}
		}
		ShowCombatLog();
		foreach (RewardInfo item5 in rewardInfoStack)
		{
			item5.rewards.RemoveAll((Reward x) => x.Type == ETreasureType.EmpyRandomItemDeck || x.Type == ETreasureType.EmptyLootDeck || x.Type == ETreasureType.EmptyBlueprintDeck || x.Type == ETreasureType.EmptyScenarioDeck);
		}
		rewardInfoStack.RemoveAll((RewardInfo x) => x.rewards.Count == 0);
		if (rewardInfoStack.Count == 0)
		{
			onFinish?.Invoke();
			return;
		}
		previousPhase = ActionProcessor.CurrentPhase;
		isShown = true;
		ShowRewardsFromCurrentSource();
		if (FFSNetwork.IsOnline)
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioReward);
		}
	}

	public void ShowCombatLog()
	{
		foreach (RewardInfo item in rewardInfoStack)
		{
			string arg = LocalizationManager.GetTranslation(item.actor.ActorLocKey()) + Choreographer.GetActorIDForCombatLogIfNeeded(item.actor);
			foreach (Reward reward in item.rewards)
			{
				if (reward.Type == ETreasureType.Resource)
				{
					LootCard.EType resourceType = reward.ResourceType;
					string translation = LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_" + resourceType.ToString().ToUpper());
					int amount = reward.Amount;
					string text = string.Format(translation, arg, amount);
					Singleton<CombatLogHandler>.Instance.AddLog(text);
				}
				if (reward.Type == ETreasureType.Gold)
				{
					string translation2 = LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_MONEY");
					int amount2 = reward.Amount;
					string text2 = string.Format(translation2, arg, amount2);
					Singleton<CombatLogHandler>.Instance.AddLog(text2);
				}
				else if (reward.Item != null)
				{
					string translation3 = LocalizationManager.GetTranslation(reward.Item.Name);
					if (reward.Type == ETreasureType.Item)
					{
						string text3 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_RANDOM_ITEM"), arg, translation3);
						Singleton<CombatLogHandler>.Instance.AddLog(text3);
					}
					else if (reward.Type == ETreasureType.ItemBlueprint)
					{
						string text4 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_RANDOMITEMBLUEPRINT"), arg, translation3);
						Singleton<CombatLogHandler>.Instance.AddLog(text4);
					}
					else if (reward.Type == ETreasureType.SoldItem)
					{
						string text5 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_SOLD_RANDOM_ITEM"), arg, translation3);
						Singleton<CombatLogHandler>.Instance.AddLog(text5);
					}
				}
				else if (reward.Type == ETreasureType.EmptyLootDeck)
				{
					string text6 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_EMPTY_DECK"), arg, 1);
					Singleton<CombatLogHandler>.Instance.AddLog(text6);
				}
				else if (reward.Type == ETreasureType.EmpyRandomItemDeck)
				{
					string text7 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_NO_RANDOM_ITEM"), arg, 1, reward.Amount);
					Singleton<CombatLogHandler>.Instance.AddLog(text7);
				}
				else if (reward.Type == ETreasureType.EmptyScenarioDeck)
				{
					string text8 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_EMPTY_SCENARIO_INSPIRATION"), arg);
					Singleton<CombatLogHandler>.Instance.AddLog(text8);
				}
				else if (reward.Type == ETreasureType.EmptyBlueprintDeck)
				{
					string text9 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_EMPTY_BLUEPRINT_INSPIRATION"), arg);
					Singleton<CombatLogHandler>.Instance.AddLog(text9);
				}
				else if (reward.Type == ETreasureType.UnlockLocation)
				{
					string unlockName = reward.UnlockName;
					string term = "FS_" + unlockName + "_NAME";
					string text10 = string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_LOOT_RANDOM_SCENARIO"), arg, LocalizationManager.GetTranslation(term));
					Singleton<CombatLogHandler>.Instance.AddLog(text10);
				}
			}
		}
	}

	private void ShowRewardsFromCurrentSource()
	{
		RewardInfo rewardInfo = currentRewardInfo;
		currentRewardInfo = rewardInfoStack[0];
		rewardInfoStack.RemoveAt(0);
		string title = "";
		if (rewardInfo == null || currentRewardInfo.sourceType != rewardInfo.sourceType)
		{
			switch (currentRewardInfo.sourceType)
			{
			case "GoalChest":
			case "Chest":
				title = LocalizationManager.GetTranslation("GUI_REWARD_TREASURE");
				break;
			case "MoneyToken":
				title = LocalizationManager.GetTranslation("GUI_REWARD_MONEYTOKEN");
				break;
			case "Custom":
				if (currentRewardInfo.rewards.Any((Reward x) => x.Item != null))
				{
					title = LocalizationManager.GetTranslation("GUI_REWARD_ITEM");
					break;
				}
				title = "UNKNOWN LOOT SOURCE TYPE";
				Debug.LogError("Trying to display loot coming from " + currentRewardInfo.sourceType + " that is not supported, for actor " + currentRewardInfo.actor.GetPrefabName() + ".");
				break;
			default:
				title = "UNKNOWN LOOT SOURCE TYPE";
				Debug.LogError("Trying to display loot coming from " + currentRewardInfo.sourceType + " that is not supported, for actor " + currentRewardInfo.actor.GetPrefabName() + ".");
				break;
			}
		}
		if (currentRewardInfo.rewards.All((Reward it) => it.IsNegative()))
		{
			title = "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_REWARD_TRAP") + "</color>";
		}
		manager.ShowRewards(currentRewardInfo.rewards, title, "GUI_CONTINUE", () =>
		{
			if (FFSNetwork.IsOnline && currentRewardInfo.actor.IsUnderMyControl())
			{
				Synchronizer.SendGameAction(GameActionType.ConfirmReward, ActionPhaseType.ScenarioReward);
			}
			if (rewardInfoStack.Count > 0)
			{
				ShowRewardsFromCurrentSource();
			}
			else
			{
				isShown = false;
				onFinish?.Invoke();
				if (FFSNetwork.IsOnline)
				{
					ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, previousPhase);
				}
			}
		}, showAppliedEffects: false, FFSNetwork.IsOnline ? ((Func<bool>)(() => !FFSNetwork.IsOnline || currentRewardInfo.actor.IsUnderMyControl())) : null);
	}

	public void ProxyConfirm(GameAction gameAction)
	{
		Log.GUIInfo("ProxyConfirmRewards " + IsShown);
		if (isShown)
		{
			manager.Confirm();
		}
		else
		{
			Log.ErrorWithReport("Trying to click confirm button during reward window, but reward window hasn't been shown yet");
		}
	}

	public override void StackRewardsToShow(CActor actor, List<Reward> rewards, string sourceType)
	{
		rewardInfoStack.Add(new RewardInfo
		{
			actor = actor,
			rewards = rewards.ToList(),
			sourceType = sourceType
		});
	}
}

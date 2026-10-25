using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemRewardLosePicker : Singleton<ItemRewardLosePicker>
{
	[SerializeField]
	private ItemCardPicker picker;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private TMP_Text confirmButtonText;

	[SerializeField]
	private GameObject intertactableEffect;

	private Color defaultColorConfirmText;

	private List<RewardGroup> rewardGroupsToCheck;

	private List<Reward> rewardsWithItems;

	private List<CItem> itemsToChooseFrom;

	protected override void AwakeInit()
	{
		defaultColorConfirmText = confirmButtonText.color;
		confirmButton.onClick.AddListener(ConfirmSelectedRewardItems);
		enabled = false;
	}

	public void Show(int rewardsToLose = 1)
	{
		rewardGroupsToCheck = new List<RewardGroup>();
		rewardsWithItems = new List<Reward>();
		foreach (Tuple<string, RewardGroup> goalChestReward in ScenarioManager.CurrentScenarioState.GoalChestRewards)
		{
			rewardGroupsToCheck.Add(goalChestReward.Item2);
			foreach (Reward reward in goalChestReward.Item2.Rewards)
			{
				if (reward.Item != null)
				{
					rewardsWithItems.Add(reward);
				}
			}
		}
		itemsToChooseFrom = rewardsWithItems.Select((Reward x) => x.Item).ToList();
		SetConfirmButtonInteractable(interact: false);
		picker.Show(itemsToChooseFrom, rewardsToLose, (List<CItem> items) =>
		{
			SetConfirmButtonInteractable(!FFSNetwork.IsOnline || FFSNetwork.IsHost);
		}, () =>
		{
			SetConfirmButtonInteractable(interact: false);
		});
	}

	private void SetConfirmButtonInteractable(bool interact)
	{
		enabled = interact;
		confirmButton.interactable = interact;
		intertactableEffect.SetActive(interact);
		if (!Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			confirmButtonText.color = (interact ? defaultColorConfirmText : UIInfoTools.DefInstance.greyedOutTextColor);
		}
	}

	private void ConfirmSelectedRewardItems()
	{
		SetConfirmButtonInteractable(interact: false);
		picker.Hide();
		List<int> list = new List<int>();
		foreach (CItem currentSelectedItem in picker.GetCurrentSelectedItems())
		{
			int num = itemsToChooseFrom.IndexOf(currentSelectedItem);
			list.Add(num);
			Reward item = rewardsWithItems[num];
			foreach (RewardGroup item2 in rewardGroupsToCheck)
			{
				item2.Rewards.Remove(item);
			}
		}
		if (FFSNetwork.IsOnline)
		{
			Synchronizer.SendGameAction(GameActionType.LoseItemReward, ActionPhaseType.LosingItemRewards, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new IndexToken(list));
		}
		Finish();
	}

	private void Finish()
	{
		if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForLoseGoalChestRewardSelection)
		{
			Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
			ScenarioRuleClient.StepComplete();
		}
	}

	public void ProxyItemRewardLose(GameAction action)
	{
		SetConfirmButtonInteractable(interact: false);
		picker.Hide();
		Choreographer.s_Choreographer.FindPlayerActor(action.ActorID);
		int[] indexes = ((IndexToken)action.SupplementaryDataToken).Indexes;
		foreach (int index in indexes)
		{
			CItem item = itemsToChooseFrom[index];
			int index2 = itemsToChooseFrom.IndexOf(item);
			Reward item2 = rewardsWithItems[index2];
			foreach (RewardGroup item3 in rewardGroupsToCheck)
			{
				item3.Rewards.Remove(item2);
			}
		}
		Finish();
	}
}

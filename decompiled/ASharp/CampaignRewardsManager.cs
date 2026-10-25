using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class CampaignRewardsManager : Singleton<CampaignRewardsManager>
{
	private class CampaignRewardWindowBuilder
	{
		private Action onFinishedReveal;

		private string buttonKey;

		private Action onClicked;

		private UICampaignRewardWindow window;

		private GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Vertical;

		private Func<Reward, bool> highlightChecker;

		private bool isInteractable;

		public CampaignRewardWindowBuilder(UICampaignRewardWindow window)
		{
			this.window = window;
		}

		public CampaignRewardWindowBuilder SetOnFinishedRevealAnimationCallback(Action onFinishedReveal)
		{
			this.onFinishedReveal = onFinishedReveal;
			return this;
		}

		protected CampaignRewardWindowBuilder SetRewardsStartAxis(GridLayoutGroup.Axis startAxis)
		{
			this.startAxis = startAxis;
			return this;
		}

		public CampaignRewardWindowBuilder EnableButton(string buttonKey, Action onClicked, bool isInteractable)
		{
			this.buttonKey = buttonKey;
			this.onClicked = onClicked;
			this.isInteractable = isInteractable;
			return this;
		}

		public CampaignRewardWindowBuilder SetRewardHighlighChecker(Func<Reward, bool> highlightChecker)
		{
			this.highlightChecker = highlightChecker;
			return this;
		}

		public virtual UICampaignRewardWindow Build()
		{
			window.SetOnFinishedRevealAnimation(onFinishedReveal);
			window.SetRewardsStartAxis(startAxis);
			window.SetRewardHighlightChecker(highlightChecker);
			if (onClicked != null)
			{
				window.EnableContinueButton(buttonKey, onClicked, isInteractable);
			}
			else
			{
				window.DisableContinueButton();
			}
			return window;
		}
	}

	private class ProsperityRewardsWindowBuilder : CampaignRewardWindowBuilder
	{
		public ProsperityRewardsWindowBuilder(UICampaignRewardWindow window)
			: base(window)
		{
			SetRewardsStartAxis(GridLayoutGroup.Axis.Horizontal);
		}
	}

	[SerializeField]
	private UICampaignRewardWindow rewardsWindow;

	[SerializeField]
	private List<CampaignExtraRewardsProcess> extraProcess;

	[SerializeField]
	private string wealthLevelUpAudioItem = "PlaySound_UIProsperityLevelUp";

	private Action onClosed;

	private Action onConfirmed;

	private Coroutine interactionCheckCoroutine;

	public void ShowRewards(List<Reward> rewards, string title, string closeKey, Action onClosed = null, bool showAppliedEffects = true, Func<bool> interactionChecker = null)
	{
		StopCheckInteraction();
		this.onClosed = onClosed;
		onConfirmed = null;
		if (rewards.Count == 0)
		{
			StartCoroutine(FinishCoroutine(hideWindow: false));
			return;
		}
		onConfirmed = () =>
		{
			onConfirmed = null;
			StopCheckInteraction();
			if (showAppliedEffects)
			{
				ProcessEffectRewards(rewards);
			}
			else
			{
				Finish();
			}
		};
		new CampaignRewardWindowBuilder(rewardsWindow).EnableButton(closeKey, Confirm, interactionChecker?.Invoke() ?? true).SetRewardHighlighChecker(IsHighlighted).Build()
			.Show(title, rewards);
		if (interactionChecker != null)
		{
			interactionCheckCoroutine = StartCoroutine(CheckInteraction(interactionChecker, rewardsWindow));
		}
	}

	private IEnumerator CheckInteraction(Func<bool> interactionChecker, UICampaignRewardWindow rewardWindow)
	{
		bool interactable = interactionChecker();
		while (true)
		{
			yield return new WaitUntil(() => interactable != interactionChecker());
			interactable = !interactable;
			rewardsWindow.SetContinueButtonInteractable(interactable);
		}
	}

	private void StopCheckInteraction()
	{
		if (interactionCheckCoroutine != null)
		{
			StopCoroutine(interactionCheckCoroutine);
			interactionCheckCoroutine = null;
		}
	}

	public void Confirm()
	{
		onConfirmed?.Invoke();
	}

	private bool IsHighlighted(Reward reward)
	{
		if (reward.Type != ETreasureType.Prosperity || reward.Amount <= 0)
		{
			return false;
		}
		int num = CMapParty.CalculateProsperityLevel(AdventureState.MapState.MapParty.ProsperityXP + reward.Amount);
		return CMapParty.CalculateProsperityLevel(AdventureState.MapState.MapParty.ProsperityXP) < num;
	}

	private void Finish()
	{
		StopCheckInteraction();
		StartCoroutine(FinishCoroutine(hideWindow: true));
	}

	private IEnumerator FinishCoroutine(bool hideWindow)
	{
		yield return null;
		if (hideWindow)
		{
			rewardsWindow.Hide();
		}
		StopCheckInteraction();
		onConfirmed = null;
		onClosed?.Invoke();
	}

	public void ShowAppliedEffects(List<Reward> rewards, Action onClosed)
	{
		if (rewards.Count == 0)
		{
			onClosed?.Invoke();
			return;
		}
		this.onClosed = onClosed;
		ProcessEffectRewards(rewards);
	}

	private void ProcessEffectRewards(List<Reward> rewards)
	{
		ICallbackPromise callbackPromise = ProcessProsperityRewards(rewards).Then(() =>
		{
			rewardsWindow.Hide();
		});
		foreach (CampaignExtraRewardsProcess process in extraProcess)
		{
			callbackPromise = callbackPromise.Then(() => process.Process(rewards));
		}
		callbackPromise.Done(Finish);
	}

	private ICallbackPromise ProcessProsperityRewards(List<Reward> rewards)
	{
		List<Reward> list = rewards.FindAll((Reward it) => it.Type == ETreasureType.Prosperity);
		if (list.Sum((Reward it) => it.Amount) == 0)
		{
			return CallbackPromise.Resolved();
		}
		List<Reward> list2 = new List<Reward>();
		if (list2.Count > 0 && !list2.Exists((Reward it) => it.LevelUp))
		{
			return CallbackPromise.Resolved();
		}
		List<Reward> source = list.Except(list2).ToList();
		int num = CMapParty.CalculateProsperityLevel(AdventureState.MapState.MapParty.ProsperityXP + source.Sum((Reward it) => it.Amount));
		if (CMapParty.CalculateProsperityLevel(AdventureState.MapState.MapParty.ProsperityXP - list2.Sum((Reward it) => it.Amount)) >= num && !list2.Exists((Reward it) => it.LevelUp))
		{
			return CallbackPromise.Resolved();
		}
		List<Reward> list3 = AdventureState.MapState.MapParty.CalculateRewardsUnlockedByProsperityLevel(num);
		if (list3.IsNullOrEmpty())
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise promise = new CallbackPromise();
		new ProsperityRewardsWindowBuilder(rewardsWindow).SetOnFinishedRevealAnimationCallback(() =>
		{
			ShowWealthLevelupMessage(promise.Resolve);
		}).Build().Show(string.Format("{0} {1}!", LocalizationManager.GetTranslation("GUI_PartyProsperity"), num), list3);
		AudioControllerUtils.PlaySound(wealthLevelUpAudioItem);
		return promise;
	}

	private void ShowWealthLevelupMessage(Action onFinish)
	{
		Debug.LogError("Prosperity Reward DELETED");
		onFinish?.Invoke();
	}
}

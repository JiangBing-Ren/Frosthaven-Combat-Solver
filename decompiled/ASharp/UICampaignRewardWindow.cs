using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignRewardWindow : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private HeaderHighlightText header;

	[SerializeField]
	private ExtendedButton continueButton;

	[SerializeField]
	private GridLayoutGroup rewardsSection;

	[SerializeField]
	private UICampaignRewardWindowSlot rewardPrefab;

	[SerializeField]
	private int initialPool = 5;

	[SerializeField]
	private float delayBetweenRewards = 0.2f;

	[SerializeField]
	private float delayRevealRewards = 0.2f;

	private List<UICampaignRewardWindowSlot> rewardsUI = new List<UICampaignRewardWindowSlot>();

	private Action onFinishedRevealAnimation;

	private Action continueAction;

	private Func<Reward, bool> highlightChecker;

	private bool isRevealing;

	private List<Reward> rewards;

	protected void Awake()
	{
		continueButton.onClick.AddListener(() =>
		{
			continueAction?.Invoke();
		});
		window.onTransitionComplete.AddListener((UIWindow w, UIWindow.VisualState stat) =>
		{
			switch (stat)
			{
			case UIWindow.VisualState.Hidden:
				OnHidden();
				break;
			case UIWindow.VisualState.Shown:
				OnShown();
				break;
			}
		});
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection.transform, initialPool, (UICampaignRewardWindowSlot obj) =>
		{
			obj.gameObject.SetActive(value: false);
		});
	}

	public void Show(string titleKey, List<Reward> rewards)
	{
		this.rewards = rewards;
		header.SetHighlightKey(titleKey);
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection.transform, rewards.Count);
		for (int i = 0; i < rewards.Count; i++)
		{
			rewardsUI[i].Hide();
		}
		isRevealing = true;
		if (window.IsOpen)
		{
			OnShown();
			return;
		}
		window.Show();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowOpened, null, "CampaignRewardWindow"));
	}

	public void SetRewardsStartAxis(GridLayoutGroup.Axis axis)
	{
		rewardsSection.startAxis = axis;
	}

	public void SetOnFinishedRevealAnimation(Action onFinishedRevealAnimation)
	{
		this.onFinishedRevealAnimation = onFinishedRevealAnimation;
	}

	public void SetRewardHighlightChecker(Func<Reward, bool> highlightChecker)
	{
		this.highlightChecker = highlightChecker;
	}

	public void EnableContinueButton(string closeKey, Action continueAction, bool isInteractable = true)
	{
		this.continueAction = continueAction;
		continueButton.gameObject.SetActive(value: false);
		continueButton.TextLanguageKey = closeKey;
		SetContinueButtonInteractable(isInteractable);
	}

	public void SetContinueButtonInteractable(bool isInteractable)
	{
		continueButton.interactable = isInteractable;
		continueButton.GetComponentInChildren<TMP_Text>()?.CrossFadeColor(isInteractable ? UIInfoTools.Instance.White : UIInfoTools.DefInstance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
	}

	public void DisableContinueButton()
	{
		continueAction = null;
		continueButton.gameObject.SetActive(value: false);
	}

	private void OnShown()
	{
		header.Show(() =>
		{
			StartCoroutine(ShowRewards(rewards));
		});
	}

	public void Hide()
	{
		window.Hide();
	}

	private void OnDisable()
	{
		StopAnimations();
	}

	private void StopAnimations()
	{
		for (int i = 0; i < rewardsUI.Count && rewardsUI[i].gameObject.activeSelf; i++)
		{
			rewardsUI[i].StopAnimations();
		}
		header.Hide();
	}

	private void OnHidden()
	{
		StopAnimations();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowClosed, null, "CampaignRewardWindow"));
	}

	private IEnumerator ShowRewards(List<Reward> rewards)
	{
		isRevealing = true;
		if (rewardsSection.startAxis == GridLayoutGroup.Axis.Vertical)
		{
			for (int i = 0; i < rewards.Count; i++)
			{
				Reward reward = rewards[i];
				rewardsUI[i].ShowReward(reward, highlightChecker != null && highlightChecker(reward));
				yield return Timekeeper.instance.WaitForSeconds(delayBetweenRewards);
			}
		}
		else
		{
			Vector2 cellsPerAxis = rewardsSection.GetCellCountPerAxis(rewards.Count);
			for (int i = 0; (float)i < cellsPerAxis.x; i++)
			{
				for (int j = 0; (float)j < cellsPerAxis.y; j++)
				{
					int num = i + j * (int)cellsPerAxis.x;
					if (num >= rewards.Count)
					{
						break;
					}
					rewardsUI[num].ShowReward(rewards[num], highlightChecker != null && highlightChecker(rewards[num]));
					yield return Timekeeper.instance.WaitForSeconds(delayBetweenRewards);
				}
			}
		}
		for (int k = 0; k < rewards.Count; k++)
		{
			rewardsUI[k].Reveal();
		}
		yield return Timekeeper.instance.WaitForSeconds(delayRevealRewards);
		isRevealing = false;
		if (continueAction != null)
		{
			continueButton.gameObject.SetActive(value: true);
		}
		onFinishedRevealAnimation?.Invoke();
	}
}

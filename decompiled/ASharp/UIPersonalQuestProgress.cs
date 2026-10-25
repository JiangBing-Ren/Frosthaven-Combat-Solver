using System;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.PersonalQuests;
using UnityEngine;

public class UIPersonalQuestProgress : MonoBehaviour
{
	[SerializeField]
	protected UIPersonalQuest personalQuestInfo;

	[SerializeField]
	private UIProgressBar progressBar;

	private Action onEnabled;

	private Action onFinishedProgress;

	private void Awake()
	{
		if ((bool)progressBar)
		{
			progressBar.OnFinishedProgress.AddListener(() =>
			{
				onFinishedProgress?.Invoke();
			});
		}
	}

	public virtual void SetPersonalQuest(CPersonalQuestState personalQuest)
	{
		onEnabled = null;
		personalQuestInfo.SetPersonalQuest(personalQuest.CurrentPersonalQuestStepData);
		if (progressBar != null)
		{
			progressBar.SetAmount(personalQuest.PersonalQuestConditionState.TotalConditionsAndTargets, (personalQuest.State == EPersonalQuestState.Completed) ? personalQuest.PersonalQuestConditionState.TotalConditionsAndTargets : personalQuest.PersonalQuestConditionState.CurrentProgress);
		}
	}

	public void SetAmount(int amount)
	{
		if (progressBar != null)
		{
			progressBar.SetAmount(amount);
		}
	}

	public void SetPersonalQuest(CPersonalQuestState personalQuest, int playProgressFrom, Action onFinishedProgress = null)
	{
		this.onFinishedProgress = onFinishedProgress;
		SetPersonalQuest(personalQuest);
		if (!(progressBar != null))
		{
			return;
		}
		if (gameObject.activeInHierarchy)
		{
			progressBar.PlayFromProgress(playProgressFrom);
			return;
		}
		onEnabled = () =>
		{
			progressBar.PlayFromProgress(playProgressFrom);
		};
	}

	public void SetPersonalQuest(PersonalQuestYMLData personalQuest, int playProgressFrom, int progressTo, int maxProgress, Action onFinishedProgress = null)
	{
		this.onFinishedProgress = onFinishedProgress;
		onEnabled = null;
		personalQuestInfo.SetPersonalQuest(personalQuest);
		if (progressBar != null)
		{
			progressBar.SetAmount(progressTo, maxProgress);
		}
		if (!(progressBar != null))
		{
			return;
		}
		if (gameObject.activeInHierarchy)
		{
			progressBar.PlayFromProgress(playProgressFrom);
			return;
		}
		onEnabled = () =>
		{
			progressBar.PlayFromProgress(playProgressFrom);
		};
	}

	private void OnEnable()
	{
		onEnabled?.Invoke();
		onEnabled = null;
	}
}

using System.Collections.Generic;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIScenarioBattleGoalProgress : UIBattleGoalProgress
{
	public enum EState
	{
		Incompleted,
		Completed,
		Failed
	}

	[SerializeField]
	private List<GameObject> progressObjects;

	[Header("Incompleted")]
	[SerializeField]
	private GameObject incompletedCheck;

	[SerializeField]
	private Color incompletedColor;

	[Header("Completed")]
	[SerializeField]
	private GameObject completedCheck;

	[SerializeField]
	private GameObject failedCheck;

	[SerializeField]
	private Color completedColor;

	public EState state;

	private void Awake()
	{
		progressBar.OnCompletedProgress.AddListener(() =>
		{
			if (!m_BattleGoal.BattleGoalConditionState.NegativeCondition)
			{
				RefreshState(EState.Completed);
				ShowProgressBar(show: false);
			}
		});
	}

	public override void SetBattleGoal(CBattleGoalState battleGoal)
	{
		state = EState.Incompleted;
		base.SetBattleGoal(battleGoal);
		if (title?.gameObject != null)
		{
			title.gameObject.SetActive(!string.IsNullOrEmpty(battleGoal.BattleGoal.LocalisedName));
		}
	}

	private void ShowProgressBar(bool show)
	{
		for (int i = 0; i < progressObjects.Count; i++)
		{
			progressObjects[i].SetActive(show);
		}
	}

	private void RefreshState(EState state)
	{
		if (this.state != state)
		{
			Log.SimpleInfo("Battle Goal " + m_BattleGoal.ID + " progress updated from " + this.state.ToString() + " to " + state.ToString() + ". Target=" + m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets + ", progress=" + m_BattleGoal.BattleGoalConditionState.CurrentProgress);
		}
		this.state = state;
		incompletedCheck.SetActive(state == EState.Incompleted);
		completedCheck.SetActive(state == EState.Completed);
		failedCheck.SetActive(state == EState.Failed);
		description.Text.color = ((state == EState.Incompleted) ? incompletedColor : completedColor);
	}

	public override void UpdateProgress(bool playAnimation = true)
	{
		if (m_BattleGoal.BattleGoalConditionState.Failed)
		{
			ShowProgressBar(show: false);
			RefreshState(EState.Failed);
			return;
		}
		int totalConditionsAndTargets = m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets;
		int currentProgress = m_BattleGoal.BattleGoalConditionState.CurrentProgress;
		if (m_BattleGoal.BattleGoal.CheckAtEndOfScenario)
		{
			int totalFailConditionsAndTargets = m_BattleGoal.BattleGoalConditionState.TotalFailConditionsAndTargets;
			int currentFailProgress = m_BattleGoal.BattleGoalConditionState.CurrentFailProgress;
			if (totalConditionsAndTargets == 0 && totalFailConditionsAndTargets > 0 && !m_BattleGoal.BattleGoal.HideCounter)
			{
				ShowProgressBar(show: true);
				RefreshState(EState.Incompleted);
				UpdateProgress(totalFailConditionsAndTargets, currentFailProgress, playAnimation);
			}
			else
			{
				ShowProgressBar(show: false);
				RefreshState(EState.Incompleted);
			}
		}
		else if (totalConditionsAndTargets <= 1 || m_BattleGoal.BattleGoal.HideCounter)
		{
			ShowProgressBar(show: false);
			if (totalConditionsAndTargets != currentProgress && state != EState.Incompleted)
			{
				Log.SimpleInfo("Error: Battle Goal " + m_BattleGoal.ID + " changing from " + state.ToString() + " to " + EState.Incompleted.ToString() + ". Target=" + m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets + ", progress=" + m_BattleGoal.BattleGoalConditionState.CurrentProgress);
			}
			RefreshState((totalConditionsAndTargets == currentProgress) ? EState.Completed : EState.Incompleted);
		}
		else if (totalConditionsAndTargets != currentProgress || state != EState.Completed)
		{
			ShowProgressBar(show: true);
			RefreshState(EState.Incompleted);
			UpdateProgress(totalConditionsAndTargets, currentProgress, playAnimation);
		}
	}
}

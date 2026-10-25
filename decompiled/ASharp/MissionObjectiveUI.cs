using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class MissionObjectiveUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI objectiveText;

	[SerializeField]
	private ImageProgressBar progressBar;

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
	private Color completedColor;

	[Header("Failed")]
	[SerializeField]
	private GameObject failedCheck;

	[SerializeField]
	private Color failedColor;

	[HideInInspector]
	public CObjective m_Objective;

	private bool isCompleted;

	private void Awake()
	{
		progressBar.OnCompletedProgress.AddListener(() =>
		{
			ShowCompleted(completed: true);
			ShowProgressBar(show: false);
		});
	}

	public void Init(CObjective objective)
	{
		isCompleted = false;
		m_Objective = objective;
		UpdateMissionProgress(playAnimation: false);
		UpdateMissionText(initialState: true);
	}

	private void ShowProgressBar(bool show)
	{
		for (int i = 0; i < progressObjects.Count; i++)
		{
			progressObjects[i].SetActive(show);
		}
	}

	private void ShowCompleted(bool completed)
	{
		bool flag = (completed && m_Objective.Result == EObjectiveResult.Lose) || (m_Objective.Result == EObjectiveResult.Win && m_Objective.UnableToComplete);
		isCompleted = completed;
		incompletedCheck.SetActive(!completed);
		completedCheck.SetActive(completed && !flag);
		failedCheck.SetActive(flag);
		TextMeshProUGUI textMeshProUGUI = objectiveText;
		Color color;
		if (completed)
		{
			color = (flag ? failedColor : completedColor);
		}
		else
		{
			color = incompletedColor;
		}
		textMeshProUGUI.color = color;
	}

	public void UpdateMissionProgress(bool playAnimation = true)
	{
		if (m_Objective == null)
		{
			return;
		}
		if (m_Objective.IsComplete)
		{
			ShowProgressBar(show: false);
			ShowCompleted(completed: true);
			return;
		}
		m_Objective.GetObjectiveProgress(ScenarioManager.CurrentScenarioState.Players.Count, out var total, out var current);
		if (total <= 1 || m_Objective.HideCounter)
		{
			ShowProgressBar(show: false);
			ShowCompleted(completed: false);
			return;
		}
		ShowProgressBar(show: true);
		ShowCompleted(completed: false);
		progressBar.ShowProgressText(m_Objective.ObjectiveType != EObjectiveType.KillAllEnemies && m_Objective.ObjectiveType != EObjectiveType.KillAllBosses && !m_Objective.EventIdentifier.Contains("KillAllEnemies"));
		if (playAnimation)
		{
			progressBar.PlayProgressTo(current, total);
		}
		else
		{
			progressBar.SetAmount(current, total);
		}
	}

	public void UpdateMissionText(bool initialState = false)
	{
		if (m_Objective.LocKey != string.Empty && m_Objective.IsActive)
		{
			string text = m_Objective.LocalizeText(initialState);
			objectiveText.text = text;
		}
	}
}

using System;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditorObjectiveListItem : MonoBehaviour
{
	public Image Background;

	[HideInInspector]
	public int objectiveIndex;

	public EObjectiveResult CurrentObjectiveResult;

	public TextMeshProUGUI ObjectiveNameText;

	public UnityAction<LevelEditorObjectiveListItem> ButtonPressedAction;

	private Color m_BackgroundOriginalColor;

	public CObjective CurrentObjective
	{
		get
		{
			if (CurrentObjectiveResult != EObjectiveResult.Win)
			{
				return ScenarioManager.CurrentScenarioState.LoseObjectives[objectiveIndex];
			}
			return ScenarioManager.CurrentScenarioState.WinObjectives[objectiveIndex];
		}
	}

	public string ObjectiveEditorDisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(CurrentObjective.EventIdentifier) && !Guid.TryParse(CurrentObjective.EventIdentifier, out var _))
			{
				return CurrentObjective.EventIdentifier;
			}
			if (!string.IsNullOrWhiteSpace(CurrentObjective.CustomLocKey))
			{
				return CurrentObjective.CustomLocKey.Replace("FH/", "").Replace("FH_Scenario_Campaign_", "").Replace("FH_Scenario_Campaign_", "")
					.Replace("_SCENARIO_", "_");
			}
			return CurrentObjective.ObjectiveType.ToString();
		}
	}

	public void InitForObjective(CObjective objectiveToInitFor, int indexToUse, EObjectiveResult objectiveResult)
	{
		objectiveIndex = indexToUse;
		CurrentObjectiveResult = objectiveResult;
		RefreshDisplayName();
	}

	public void RefreshDisplayName()
	{
		ObjectiveNameText.text = ObjectiveEditorDisplayName;
		if (string.IsNullOrWhiteSpace(ObjectiveNameText.text))
		{
			ObjectiveNameText.text = "Objective #" + objectiveIndex;
		}
	}

	public void SetSelectedState(bool selected)
	{
		Background.color = (selected ? new Color(0.85f, 0.745f, 0.541f) : m_BackgroundOriginalColor);
	}

	public void OnButtonPressItemSelected()
	{
		ButtonPressedAction?.Invoke(this);
	}

	private void Awake()
	{
		m_BackgroundOriginalColor = Background.color;
	}
}

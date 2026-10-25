using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class ScenarioModifierContainer : MonoBehaviour
{
	[SerializeField]
	private GameObject ScenarioModifierPrefab;

	private string ScenarioID;

	private List<ScenarioModifierUI> ScenarioModifierInstances = new List<ScenarioModifierUI>();

	public void Init(string scenarioID, List<CScenarioModifier> scenarioModifiers)
	{
		ScenarioID = scenarioID;
		foreach (ScenarioModifierUI scenarioModifierInstance in ScenarioModifierInstances)
		{
			Object.Destroy(scenarioModifierInstance.gameObject);
		}
		ScenarioModifierInstances.Clear();
		foreach (CScenarioModifier scenarioModifier in scenarioModifiers)
		{
			AddModifier(scenarioModifier);
		}
		ScenarioScript current2 = ScenarioScript.current;
		if (current2 != null && current2.ShouldSortModifierTextsInOrderOfModifiers())
		{
			SortModifierTextsInOrderOfModifiers();
		}
	}

	public void ModifyUpdatedHiddenOrDeactivatedState(CScenarioModifier modifierUpdated)
	{
		if (modifierUpdated.IsHidden || modifierUpdated.Deactivated)
		{
			RemoveModifier(modifierUpdated);
			return;
		}
		AddModifier(modifierUpdated);
		ScenarioScript current = ScenarioScript.current;
		if (current != null && current.ShouldSortModifierTextsInOrderOfModifiers())
		{
			SortModifierTextsInOrderOfModifiers();
		}
	}

	private void AddModifier(CScenarioModifier scenarioModifier)
	{
		if (scenarioModifier != null && ScenarioModifierInstances.Find((ScenarioModifierUI o) => o.m_ScenarioModifier == scenarioModifier) == null)
		{
			InitialiseScenarioModifier(scenarioModifier);
		}
	}

	private void RemoveModifier(CScenarioModifier scenarioModifier)
	{
		if (scenarioModifier != null)
		{
			ScenarioModifierUI scenarioModifierUI = ScenarioModifierInstances.Find((ScenarioModifierUI o) => o.m_ScenarioModifier == scenarioModifier);
			if (scenarioModifierUI != null)
			{
				ScenarioModifierInstances.Remove(scenarioModifierUI);
				Object.Destroy(scenarioModifierUI.gameObject);
			}
		}
	}

	public void UpdateDescriptions()
	{
		ScenarioScript current = ScenarioScript.current;
		if (current != null && current.ShouldSortModifierTextsInOrderOfModifiers())
		{
			SortModifierTextsInOrderOfModifiers();
		}
		foreach (ScenarioModifierUI scenarioModifierInstance in ScenarioModifierInstances)
		{
			UpdateDescription(scenarioModifierInstance);
		}
	}

	private void UpdateDescription(ScenarioModifierUI scenarioModifierInstance)
	{
		if (scenarioModifierInstance != null)
		{
			UpdateScenarioModifierText(scenarioModifierInstance);
		}
	}

	private void InitialiseScenarioModifier(CScenarioModifier scenarioModifier)
	{
		if (!scenarioModifier.IsHidden && !scenarioModifier.Deactivated)
		{
			string text = scenarioModifier.LocalizeText(ScenarioID);
			if (!string.IsNullOrEmpty(text))
			{
				ScenarioModifierUI component = Object.Instantiate(ScenarioModifierPrefab, transform).GetComponent<ScenarioModifierUI>();
				component.transform.SetAsFirstSibling();
				component.Init(scenarioModifier, text);
				ScenarioModifierInstances.Add(component);
			}
		}
	}

	private void UpdateScenarioModifierText(ScenarioModifierUI scenarioModifierUI)
	{
		string text = scenarioModifierUI.m_ScenarioModifier.LocalizeText(ScenarioID);
		if (text != string.Empty && text != "")
		{
			scenarioModifierUI.SetText(text);
		}
	}

	private static int CompareScenarioModifierTextsByScenarioOrder(ScenarioModifierUI a, ScenarioModifierUI b)
	{
		List<CScenarioModifier> scenarioModifiers = ScenarioManager.CurrentScenarioState.ScenarioModifiers;
		int num = scenarioModifiers.IndexOf(a.m_ScenarioModifier);
		int value = scenarioModifiers.IndexOf(b.m_ScenarioModifier);
		return num.CompareTo(value);
	}

	private void SortModifierTextsInOrderOfModifiers()
	{
		ScenarioModifierInstances.Sort(CompareScenarioModifierTextsByScenarioOrder);
		foreach (ScenarioModifierUI scenarioModifierInstance in ScenarioModifierInstances)
		{
			scenarioModifierInstance.transform.SetAsLastSibling();
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class LevelEditorObjectiveDependencyPanel : MonoBehaviour
{
	public LevelEditorGenericListPanel ListPanel;

	public Dictionary<string, bool> DependenciesBeingShown;

	private List<string> m_OrderedKeys;

	private List<string> m_LabelStrings;

	private const string c_ObjectiveDependancyLabelFormat = "{0} | <b>{1}</b>";

	public void SetupForObjective(CObjective objective)
	{
		ListPanel.SetupItemsAvailableToAdd((from o in ScenarioManager.CurrentScenarioState.AllObjectives
			where o != objective && !string.IsNullOrEmpty(o.EventIdentifier)
			select o.EventIdentifier).ToList());
		DependenciesBeingShown = objective?.RequiredObjectiveStates?.ToDictionary((KeyValuePair<string, bool> kv) => kv.Key, (KeyValuePair<string, bool> kv) => kv.Value) ?? new Dictionary<string, bool>();
		RefreshList();
		ListPanel.SetupDelegateActions(OnAddCompletedDependancyPressed, OnDeleteDependancyPressed, OnDependancyPressed, OnAddIncompleteDependancyPressed);
	}

	private void RefreshList()
	{
		m_OrderedKeys = DependenciesBeingShown.Keys.ToList();
		m_LabelStrings = m_OrderedKeys.Select((string s) => string.Format("{0} | <b>{1}</b>", s, DependenciesBeingShown[s] ? "Complete" : "InComplete")).ToList();
		ListPanel.RefreshUIWithItems(m_LabelStrings);
	}

	public void OnAddCompletedDependancyPressed(string objectiveEventId)
	{
		if (!DependenciesBeingShown.ContainsKey(objectiveEventId))
		{
			DependenciesBeingShown.Add(objectiveEventId, value: true);
		}
		else
		{
			DependenciesBeingShown[objectiveEventId] = true;
		}
		RefreshList();
	}

	public void OnAddIncompleteDependancyPressed(string objectiveEventId)
	{
		if (!DependenciesBeingShown.ContainsKey(objectiveEventId))
		{
			DependenciesBeingShown.Add(objectiveEventId, value: false);
		}
		else
		{
			DependenciesBeingShown[objectiveEventId] = false;
		}
		RefreshList();
	}

	public void OnDeleteDependancyPressed(string objectiveEventId, int itemIndex)
	{
		if (DependenciesBeingShown.ContainsKey(objectiveEventId))
		{
			DependenciesBeingShown.Remove(objectiveEventId);
		}
		RefreshList();
	}

	public void OnDependancyPressed(string objectiveEventId, int itemIndex)
	{
	}
}

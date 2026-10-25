using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class MissionObjectiveContainer : MonoBehaviour
{
	[SerializeField]
	private GameObject MissionObjectivePrefab;

	[SerializeField]
	private Transform objectiveContainer;

	public List<MissionObjectiveUI> ObjectiveInstances { get; private set; } = new List<MissionObjectiveUI>();

	private int ObjectivePriority(CObjective objective)
	{
		if (objective.IsOptional)
		{
			return 2;
		}
		if (ScenarioManager.CurrentScenarioState.LoseObjectives.Contains(objective))
		{
			return 1;
		}
		return 0;
	}

	private void SortObjectives()
	{
		ObjectiveInstances.Sort((MissionObjectiveUI x, MissionObjectiveUI y) =>
		{
			int num = ObjectivePriority(y.m_Objective).CompareTo(ObjectivePriority(x.m_Objective));
			if (num != 0)
			{
				return num;
			}
			int num2 = ScenarioManager.CurrentScenarioState.WinObjectives.IndexOf(x.m_Objective);
			if (num2 == -1)
			{
				num2 = ScenarioManager.CurrentScenarioState.LoseObjectives.IndexOf(x.m_Objective);
			}
			int num3 = ScenarioManager.CurrentScenarioState.WinObjectives.IndexOf(y.m_Objective);
			if (num3 == -1)
			{
				num3 = ScenarioManager.CurrentScenarioState.LoseObjectives.IndexOf(y.m_Objective);
			}
			return num3.CompareTo(num2);
		});
		foreach (MissionObjectiveUI objectiveInstance in ObjectiveInstances)
		{
			objectiveInstance.transform.SetAsFirstSibling();
		}
	}

	public void Init(List<CObjective> winObjectives, List<CObjective> loseObjectives)
	{
		foreach (MissionObjectiveUI objectiveInstance in ObjectiveInstances)
		{
			Object.Destroy(objectiveInstance.gameObject);
		}
		ObjectiveInstances.Clear();
		foreach (CObjective winObjective in winObjectives)
		{
			InitialiseObjective(winObjective);
		}
		foreach (CObjective loseObjective in loseObjectives)
		{
			InitialiseObjective(loseObjective);
		}
		SortObjectives();
	}

	public void AddObjective(CObjective objective)
	{
		if (objective != null && ObjectiveInstances.Find((MissionObjectiveUI o) => o.m_Objective == objective) == null)
		{
			InitialiseObjective(objective);
			SortObjectives();
		}
	}

	public void CheckToRemoveObjectives()
	{
		for (int num = ObjectiveInstances.Count - 1; num >= 0; num--)
		{
			MissionObjectiveUI missionObjectiveUI = ObjectiveInstances[num];
			if (missionObjectiveUI.m_Objective.IsComplete && missionObjectiveUI.m_Objective.RemovesFromUIOnComplete)
			{
				RemoveObjective(missionObjectiveUI.m_Objective);
			}
		}
	}

	public void RemoveObjective(CObjective objective)
	{
		if (objective != null)
		{
			MissionObjectiveUI missionObjectiveUI = ObjectiveInstances.Find((MissionObjectiveUI o) => o.m_Objective == objective);
			if (missionObjectiveUI != null)
			{
				ObjectiveInstances.Remove(missionObjectiveUI);
				Object.Destroy(missionObjectiveUI.gameObject);
			}
		}
	}

	public void UpdateProgress()
	{
		foreach (MissionObjectiveUI objectiveInstance in ObjectiveInstances)
		{
			objectiveInstance.UpdateMissionProgress();
			objectiveInstance.UpdateMissionText();
		}
	}

	private void InitialiseObjective(CObjective objective)
	{
		if (objective.LocKey != string.Empty && objective.IsActive && !objective.IsHidden)
		{
			MissionObjectiveUI component = Object.Instantiate(MissionObjectivePrefab, objectiveContainer).GetComponent<MissionObjectiveUI>();
			component.transform.SetAsFirstSibling();
			component.Init(objective);
			ObjectiveInstances.Add(component);
		}
	}
}

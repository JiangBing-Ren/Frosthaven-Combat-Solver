using System.Collections.Generic;
using System.Linq;
using FHRL;
using UnityEngine;

public class FHScenarioObject : MonoBehaviour
{
	[field: SerializeField]
	public uint ScenarioID { get; private set; }

	[Tooltip("One or more Scenario IDs (this is checked as OR operation)")]
	[field: SerializeField]
	public uint[] Scenarios { get; private set; }

	[field: SerializeField]
	public bool ShownIfScenarioUnlocked { get; private set; }

	public void Init(IReadOnlyList<ScenarioID> scenarios)
	{
		if (ShownIfScenarioUnlocked)
		{
			if (Scenarios.All((uint scenarioID) => scenarios.Any((ScenarioID s) =>
			{
				uint iD = s.ID;
				return iD.Equals(scenarioID);
			})))
			{
				gameObject.SetActive(value: true);
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
		else if (Scenarios.Any((uint scenario) => scenarios.Any((ScenarioID s) =>
		{
			uint iD = s.ID;
			return iD.Equals(scenario);
		})))
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			gameObject.SetActive(value: true);
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

namespace FH.ApparanceBaking;

[CreateAssetMenu(fileName = "ScenarioDataLocksDef", menuName = "Defs/ScenarioDataLocksDef")]
public class ScenarioDataLocksDef : ScriptableObject
{
	[SerializeField]
	private List<string> _locks = new List<string>();

	public bool IsLocked(CCustomLevelData levelData)
	{
		return IsLocked(levelData.ScenarioState.BakingID);
	}

	public bool IsLocked(string scenarioName)
	{
		return _locks.Contains(scenarioName);
	}

	public bool Lock(CCustomLevelData levelData)
	{
		return Lock(levelData.ScenarioState.BakingID);
	}

	public bool Lock(string scenarioName)
	{
		if (IsLocked(scenarioName))
		{
			return false;
		}
		_locks.Add(scenarioName);
		return true;
	}

	public static ScenarioDataLocksDef Load()
	{
		return Resources.Load<ScenarioDataLocksDef>("ScenarioDataLocksDef");
	}

	public void OrderByName()
	{
		_locks = _locks.OrderBy((string i) => i).ToList();
	}
}

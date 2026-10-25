using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.ProcBackgroundsPrototype;

[CreateAssetMenu(menuName = "Defs/Proc Background Resources")]
public class ProcBackgroundResourcesDef : ScriptableObject
{
	[Serializable]
	internal class BGResource
	{
		public BackgroundElementType Type;

		public ScenarioPossibleRoom.EBiome Biome;

		public List<GameObject> Prefabs;
	}

	[SerializeField]
	private List<BGResource> _resources = new List<BGResource>();

	[SerializeField]
	private GameObject _errorCube;

	public IEnumerable<GameObject> GetResources(BackgroundElementType type, ScenarioPossibleRoom.EBiome biome)
	{
		return _resources.FirstOrDefault((BGResource a) => a.Type == type && a.Biome == biome)?.Prefabs ?? new List<GameObject> { _errorCube };
	}
}

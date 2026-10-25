using System;
using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.LevelComponents;

[Serializable]
public class BackgroundMistClutter
{
	[Header("Give this element a name to identify it in the list")]
	public string Name;

	public int Seed;

	public List<GameObject> Prefabs;

	[Min(0f)]
	public int Quota = 6;

	[Min(0f)]
	public float MinWallWidth = 3f;

	public float DepthOffset = 5f;

	public bool RandomizeDepthOffset;

	[Min(0f)]
	public float DepthOffsetMax = 10f;

	public ScenarioPossibleRoom.ETone ETone;

	public string MaptileBinding;

	[Min(0f)]
	public float AutoFitRadius;
}

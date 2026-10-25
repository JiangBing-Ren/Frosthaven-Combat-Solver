using System;
using System.Linq;
using FHRL;
using UnityEngine;

[Serializable]
public class QuestChainConfigUI
{
	public Sprite Icon;

	public Color Color;

	public ScenarioChainType[] Chains;

	public bool IsApplicableTo(ScenarioChainType chain)
	{
		return Chains.Any((ScenarioChainType c) => c == chain);
	}
}

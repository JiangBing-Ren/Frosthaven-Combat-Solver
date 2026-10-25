using System;
using ScenarioRuleLibrary.YML;
using UnityEngine;

[Serializable]
public class RewardConfigUI
{
	public ETreasureType type;

	public ETreasureDistributionType distribution;

	public Sprite icon;

	[Tooltip("Color of  icon")]
	public Color color = Color.white;

	public string extraId;
}

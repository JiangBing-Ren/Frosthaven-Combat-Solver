using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.SeasonTech;

public abstract class SeasonPreset : ScriptableObject
{
	public ScenarioPossibleRoom.ESubBiome SubBiome;

	public Texture2D SnowAlbedoRoughness;

	public Texture2D SnowNormal;
}

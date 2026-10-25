using System;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.SeasonTech;

[Serializable]
public class WorldSeasonState
{
	public const string PROP_TEMPERATURE = "_Season_Temperature";

	public const string PROP_SNOW_ALBEDO = "_Season_Snow_Albedo";

	public const string PROP_SNOW_NORMAL = "_Season_Snow_Normal";

	public const float TEMPERATURE_COLD = 0f;

	public const float TEMPERATURE_WARM = 1f;

	public ScenarioPossibleRoom.EBiomeState BiomeStateOverride;

	public float Temperature;

	public bool Rain;

	[NonSerialized]
	public SeasonPreset SeasonPreset;

	private static WorldSeasonState _lastState = TemplateDefault;

	public static readonly WorldSeasonState TemplateDefault = new WorldSeasonState
	{
		Temperature = 1f,
		Rain = false,
		BiomeStateOverride = ScenarioPossibleRoom.EBiomeState.Inherit,
		SeasonPreset = null
	};

	public static WorldSeasonState LastSeasonState
	{
		get
		{
			return _lastState ?? TemplateDefault;
		}
		private set
		{
			_lastState = value;
		}
	}

	public static void Set(WorldSeasonState state)
	{
		LastSeasonState = state;
		Shader.SetGlobalFloat("_Season_Temperature", state.Temperature);
		if (state.SeasonPreset != null)
		{
			Shader.SetGlobalTexture("_Season_Snow_Albedo", state.SeasonPreset.SnowAlbedoRoughness);
			Shader.SetGlobalTexture("_Season_Snow_Normal", state.SeasonPreset.SnowNormal);
		}
		else
		{
			Debug.LogWarning("WorldSeasonState has no SeasonPreset assigned. Please make sure that one gets assigned before calling Set().");
		}
		BiomeStateBasedToggle[] array = UnityEngine.Object.FindObjectsOfType<BiomeStateBasedToggle>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Set(state);
		}
		EnvironmentLoader.Instance.EnvironmentRoot.GetComponentInChildren<ProceduralScenario>().RefreshAmbience();
	}
}

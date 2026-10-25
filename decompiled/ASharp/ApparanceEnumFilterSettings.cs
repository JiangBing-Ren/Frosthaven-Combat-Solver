using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class ApparanceEnumFilterSettings : ScriptableObject
{
	[Serializable]
	public class ApparanceEnumFilterConfig
	{
		[ReadOnlyField]
		public string EnumName;

		public bool IsReadyForRelease;

		public ApparanceEnumFilterConfig(string EName)
		{
			EnumName = EName;
			IsReadyForRelease = true;
		}
	}

	[Header("Controls")]
	public bool PressToUpdateListFromAvailable;

	[Header("Configs")]
	public List<ApparanceEnumFilterConfig> BiomeConfigs = new List<ApparanceEnumFilterConfig>();

	public List<ApparanceEnumFilterConfig> SubBiomeConfigs = new List<ApparanceEnumFilterConfig>();

	public List<ApparanceEnumFilterConfig> ThemeConfigs = new List<ApparanceEnumFilterConfig>();

	public List<ApparanceEnumFilterConfig> SubThemeConfigs = new List<ApparanceEnumFilterConfig>();

	public List<ApparanceEnumFilterConfig> ToneConfigs = new List<ApparanceEnumFilterConfig>();

	private void OnValidate()
	{
		if (PressToUpdateListFromAvailable)
		{
			PressToUpdateListFromAvailable = false;
			UpdateConfigsFromAvailable();
		}
	}

	public void UpdateConfigsFromAvailable()
	{
		ScenarioPossibleRoom.EBiome[] biomes = ScenarioPossibleRoom.Biomes;
		for (int i = 0; i < biomes.Length; i++)
		{
			ScenarioPossibleRoom.EBiome biome = biomes[i];
			if (!BiomeConfigs.Any((ApparanceEnumFilterConfig b) => biome.ToString() == b.EnumName))
			{
				BiomeConfigs.Add(new ApparanceEnumFilterConfig(biome.ToString()));
			}
		}
		ScenarioPossibleRoom.ESubBiome[] subBiomes = ScenarioPossibleRoom.SubBiomes;
		for (int i = 0; i < subBiomes.Length; i++)
		{
			ScenarioPossibleRoom.ESubBiome subbiome = subBiomes[i];
			if (!SubBiomeConfigs.Any((ApparanceEnumFilterConfig b) => subbiome.ToString() == b.EnumName))
			{
				SubBiomeConfigs.Add(new ApparanceEnumFilterConfig(subbiome.ToString()));
			}
		}
		ScenarioPossibleRoom.ETheme[] themes = ScenarioPossibleRoom.Themes;
		for (int i = 0; i < themes.Length; i++)
		{
			ScenarioPossibleRoom.ETheme theme = themes[i];
			if (!ThemeConfigs.Any((ApparanceEnumFilterConfig b) => theme.ToString() == b.EnumName))
			{
				ThemeConfigs.Add(new ApparanceEnumFilterConfig(theme.ToString()));
			}
		}
		ScenarioPossibleRoom.ESubTheme[] subThemes = ScenarioPossibleRoom.SubThemes;
		for (int i = 0; i < subThemes.Length; i++)
		{
			ScenarioPossibleRoom.ESubTheme subTheme = subThemes[i];
			if (!SubThemeConfigs.Any((ApparanceEnumFilterConfig b) => subTheme.ToString() == b.EnumName))
			{
				SubThemeConfigs.Add(new ApparanceEnumFilterConfig(subTheme.ToString()));
			}
		}
		ScenarioPossibleRoom.ETone[] tones = ScenarioPossibleRoom.Tones;
		for (int i = 0; i < tones.Length; i++)
		{
			ScenarioPossibleRoom.ETone tone = tones[i];
			if (!ToneConfigs.Any((ApparanceEnumFilterConfig b) => tone.ToString() == b.EnumName))
			{
				ToneConfigs.Add(new ApparanceEnumFilterConfig(tone.ToString()));
			}
		}
	}

	public bool IsBiomeReadyForRelease(ScenarioPossibleRoom.EBiome biome)
	{
		return BiomeConfigs.FirstOrDefault((ApparanceEnumFilterConfig b) => b.EnumName == biome.ToString())?.IsReadyForRelease ?? false;
	}

	public bool IsSubBiomeReadyForRelease(ScenarioPossibleRoom.ESubBiome subBiome)
	{
		return SubBiomeConfigs.FirstOrDefault((ApparanceEnumFilterConfig b) => b.EnumName == subBiome.ToString())?.IsReadyForRelease ?? false;
	}

	public bool IsThemeReadyForRelease(ScenarioPossibleRoom.ETheme theme)
	{
		return ThemeConfigs.FirstOrDefault((ApparanceEnumFilterConfig b) => b.EnumName == theme.ToString())?.IsReadyForRelease ?? false;
	}

	public bool IsSubThemeReadyForRelease(ScenarioPossibleRoom.ESubTheme subTheme)
	{
		return SubThemeConfigs.FirstOrDefault((ApparanceEnumFilterConfig b) => b.EnumName == subTheme.ToString())?.IsReadyForRelease ?? false;
	}

	public bool IsToneReadyForRelease(ScenarioPossibleRoom.ETone tone)
	{
		return ToneConfigs.FirstOrDefault((ApparanceEnumFilterConfig b) => b.EnumName == tone.ToString())?.IsReadyForRelease ?? false;
	}

	public ScenarioPossibleRoom.EBiome[] AvailableBiomes()
	{
		return ScenarioPossibleRoom.Biomes.Where((ScenarioPossibleRoom.EBiome b) => IsBiomeReadyForRelease(b)).ToArray();
	}

	public ScenarioPossibleRoom.ESubBiome[] AvailableSubBiomes()
	{
		return ScenarioPossibleRoom.SubBiomes.Where((ScenarioPossibleRoom.ESubBiome b) => IsSubBiomeReadyForRelease(b)).ToArray();
	}

	public ScenarioPossibleRoom.ETheme[] AvailableThemes()
	{
		return ScenarioPossibleRoom.Themes.Where((ScenarioPossibleRoom.ETheme b) => IsThemeReadyForRelease(b)).ToArray();
	}

	public ScenarioPossibleRoom.ESubTheme[] AvailableSubThemes()
	{
		return ScenarioPossibleRoom.SubThemes.Where((ScenarioPossibleRoom.ESubTheme b) => IsSubThemeReadyForRelease(b)).ToArray();
	}

	public ScenarioPossibleRoom.ETone[] AvailableTones()
	{
		return ScenarioPossibleRoom.Tones.Where((ScenarioPossibleRoom.ETone b) => IsToneReadyForRelease(b)).ToArray();
	}
}

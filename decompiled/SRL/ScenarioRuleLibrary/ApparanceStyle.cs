using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class ApparanceStyle : ISerializable
{
	public int Seed { get; set; }

	public ScenarioPossibleRoom.EBiome Biome { get; private set; }

	public ScenarioPossibleRoom.ESubBiome SubBiome { get; private set; }

	public ScenarioPossibleRoom.ETheme Theme { get; private set; }

	public ScenarioPossibleRoom.ESubTheme SubTheme { get; private set; }

	public ScenarioPossibleRoom.ETone Tone { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Seed", Seed);
		info.AddValue("Biome", Biome);
		info.AddValue("SubBiome", SubBiome);
		info.AddValue("Theme", Theme);
		info.AddValue("SubTheme", SubTheme);
		info.AddValue("Tone", Tone);
	}

	public ApparanceStyle(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Seed":
					Seed = info.GetInt32("Seed");
					break;
				case "Biome":
					Biome = (ScenarioPossibleRoom.EBiome)info.GetValue("Biome", typeof(ScenarioPossibleRoom.EBiome));
					break;
				case "SubBiome":
					SubBiome = (ScenarioPossibleRoom.ESubBiome)info.GetValue("SubBiome", typeof(ScenarioPossibleRoom.ESubBiome));
					break;
				case "Theme":
					Theme = (ScenarioPossibleRoom.ETheme)info.GetValue("Theme", typeof(ScenarioPossibleRoom.ETheme));
					break;
				case "SubTheme":
					SubTheme = (ScenarioPossibleRoom.ESubTheme)info.GetValue("SubTheme", typeof(ScenarioPossibleRoom.ESubTheme));
					break;
				case "Tone":
					Tone = (ScenarioPossibleRoom.ETone)info.GetValue("Tone", typeof(ScenarioPossibleRoom.ETone));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize ApparanceStyle entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public ApparanceStyle(ScenarioPossibleRoom.EBiome biome, ScenarioPossibleRoom.ESubBiome subBiome = ScenarioPossibleRoom.ESubBiome.Default, ScenarioPossibleRoom.ETheme theme = ScenarioPossibleRoom.ETheme.Default, ScenarioPossibleRoom.ESubTheme subTheme = ScenarioPossibleRoom.ESubTheme.Default, ScenarioPossibleRoom.ETone tone = ScenarioPossibleRoom.ETone.Default)
	{
		Biome = biome;
		SubBiome = subBiome;
		Theme = theme;
		SubTheme = subTheme;
		Tone = tone;
	}

	public static List<Tuple<int, string>> Compare(ApparanceStyle style1, ApparanceStyle style2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		if (style1.Seed != style2.Seed)
		{
			ScenarioState.LogMismatch(list, isMPCompare, 801, "Apparance Style Seed does not match.", new List<string[]> { new string[3]
			{
				"Seed",
				style1.Seed.ToString(),
				style2.Seed.ToString()
			} });
		}
		if (style1.Biome != style2.Biome)
		{
			ScenarioState.LogMismatch(list, isMPCompare, 802, "Apparance Style Biome does not match.", new List<string[]> { new string[3]
			{
				"Biome",
				style1.Biome.ToString(),
				style2.Biome.ToString()
			} });
		}
		if (style1.SubBiome != style2.SubBiome)
		{
			ScenarioState.LogMismatch(list, isMPCompare, 803, "Apparance Style SubBiome does not match.", new List<string[]> { new string[3]
			{
				"SubBiome",
				style1.SubBiome.ToString(),
				style2.SubBiome.ToString()
			} });
		}
		if (style1.Theme != style2.Theme)
		{
			ScenarioState.LogMismatch(list, isMPCompare, 804, "Apparance Style Theme does not match.", new List<string[]> { new string[3]
			{
				"Theme",
				style1.Theme.ToString(),
				style2.Theme.ToString()
			} });
		}
		if (style1.SubTheme != style2.SubTheme)
		{
			ScenarioState.LogMismatch(list, isMPCompare, 805, "Apparance Style SubTheme does not match.", new List<string[]> { new string[3]
			{
				"SubTheme",
				style1.SubTheme.ToString(),
				style2.SubTheme.ToString()
			} });
		}
		if (style1.Tone != style2.Tone)
		{
			ScenarioState.LogMismatch(list, isMPCompare, 806, "Apparance Style Tone does not match.", new List<string[]> { new string[3]
			{
				"Tone",
				style1.Tone.ToString(),
				style2.Tone.ToString()
			} });
		}
		return list;
	}
}

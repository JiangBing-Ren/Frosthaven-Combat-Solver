using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using Libs.GameConsole;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.SeasonTech;

public class SeasonTechRuntime : MonoBehaviour
{
	[SerializeField]
	public WinterPreset[] WinterPresets;

	[SerializeField]
	public SummerPreset[] SummerPresets;

	public static SeasonTechRuntime Instance()
	{
		return Object.FindObjectOfType<SeasonTechRuntime>(includeInactive: true);
	}

	public SeasonPreset GetPreset(float temperature, ScenarioPossibleRoom.ESubBiome subBiome)
	{
		if (!(temperature > 0f))
		{
			return FindPresetInArray<WinterPreset>(WinterPresets);
		}
		return FindPresetInArray<SummerPreset>(SummerPresets);
		SeasonPreset FindPresetInArray<TPreset>(TPreset[] array) where TPreset : SeasonPreset
		{
			TPreset val = array.FirstOrDefault((TPreset x) => x.SubBiome == subBiome);
			if (val == null)
			{
				val = array.FirstOrDefault((TPreset x) => x.SubBiome == ScenarioPossibleRoom.ESubBiome.Default);
			}
			return val;
		}
	}

	private static void ConsoleSetSeason(IConsole console, float temperature)
	{
		SeasonTechRuntime seasonTechRuntime = Instance();
		if (seasonTechRuntime == null)
		{
			Debug.LogWarning("In order to apply a season, there needs to be a SeasonTechRuntime component anywhere in the scene. Make sure that you're playing a scenario, which is where it is expected to have one.");
			return;
		}
		ScenarioPossibleRoom.ESubBiome subBiome = ScenarioPossibleRoom.ESubBiome.Default;
		if (EnvironmentLoader.IsInstanceValid && EnvironmentLoader.Instance.EnvironmentRoot != null)
		{
			subBiome = EnvironmentLoader.Instance.EnvironmentRoot.GetComponent<ProceduralStyle>().SubBiome;
		}
		WorldSeasonState.Set(new WorldSeasonState
		{
			Temperature = temperature,
			SeasonPreset = seasonTechRuntime.GetPreset(1f, subBiome)
		});
	}

	[ConsoleCommand(Command = "Season_Set_Summer")]
	public static void ConsoleSetSummer(IConsole console)
	{
		ConsoleSetSeason(console, 1f);
	}

	[ConsoleCommand(Command = "Season_Set_Winter")]
	public static void ConsoleSetWinter(IConsole console)
	{
		ConsoleSetSeason(console, 0f);
	}
}

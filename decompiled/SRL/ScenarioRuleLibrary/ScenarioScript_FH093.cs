using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH093 : ScenarioScript
{
	public static string allyName
	{
		get
		{
			if (IsAlternative)
			{
				return "Yoglang";
			}
			return "Scabwit";
		}
	}

	public static string enemyName
	{
		get
		{
			if (IsAlternative)
			{
				return "Scabwit";
			}
			return "Yoglang";
		}
	}

	public static bool IsAlternative
	{
		get
		{
			if (!ScenarioManager.CurrentScenarioState.ScenarioSetup.IsAlternativeLevel)
			{
				return ScenarioManager.CurrentScenarioState.ID.Equals("FS_093_Midwinter_Brawl_B", StringComparison.InvariantCultureIgnoreCase);
			}
			return true;
		}
	}

	protected override bool Initialize()
	{
		GetScenarioModifier("PreventDamage").CustomLocKeyArgs = new List<object>
		{
			string.Empty,
			allyName
		};
		GetScenarioModifier("Hint - Ally Die").CustomLocKeyArgs = new List<object> { allyName };
		CObjective objective = GetObjective("Scabwit Dies");
		if (objective != null)
		{
			objective.CustomLocKeyArgs = new List<object> { allyName };
		}
		CObjective objective2 = GetObjective("Yoglang Dies");
		if (objective2 != null)
		{
			objective2.CustomLocKeyArgs = new List<object> { allyName };
		}
		return true;
	}

	protected override int GetConclusionIndexInternal()
	{
		if (IsAlternative)
		{
			return 1;
		}
		return 0;
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		return actor.GetPrefabName() switch
		{
			"AlgoxGuard" => "AlgoxGuardSnowspeaker", 
			"AlgoxGuard Elite" => "AlgoxGuardSnowspeaker Elite", 
			"AlgoxArcher" => "AlgoxArcherSnowspeaker", 
			"AlgoxArcher Elite" => "AlgoxArcherSnowspeaker Elite", 
			"AlgoxScout" => "AlgoxScoutSnowspeaker", 
			"AlgoxScout Elite" => "AlgoxScoutSnowspeaker Elite", 
			"AlgoxSnowspeaker" => "AlgoxSnowspeakerOrange", 
			"AlgoxSnowspeaker Elite" => "AlgoxSnowspeakerOrange Elite", 
			_ => null, 
		};
	}
}

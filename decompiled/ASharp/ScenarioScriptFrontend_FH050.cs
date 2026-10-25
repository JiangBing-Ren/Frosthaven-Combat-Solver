using System.Collections.Generic;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH050 : ScenarioScriptFrontend
{
	private readonly Dictionary<char, int[]> dict = new Dictionary<char, int[]>
	{
		{
			'a',
			new int[2] { 3, 3 }
		},
		{
			'b',
			new int[2] { 4, 2 }
		},
		{
			'c',
			new int[2] { 8, 2 }
		},
		{
			'd',
			new int[2] { 9, 4 }
		},
		{
			'e',
			new int[2] { 2, 6 }
		},
		{
			'f',
			new int[2] { 7, 8 }
		},
		{
			'g',
			new int[2] { 2, 8 }
		}
	};

	[ConsoleVariable(Alias = "ss050_show_spawner_symbols")]
	private static bool ShowSpawners_cvar;

	private ScenarioScript_FH050 _script050 => (ScenarioScript_FH050)_script;

	protected override void TickInternal()
	{
		if (!ShowSpawners_cvar)
		{
			return;
		}
		foreach (KeyValuePair<char, int[]> item in dict)
		{
			Vector3 cTPos = ScenarioScriptFrontend.GetCTPos(item.Value[0], item.Value[1]);
			QGL.LatePrintWorld(item.Key, cTPos, null, 2f);
		}
	}
}

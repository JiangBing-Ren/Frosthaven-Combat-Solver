using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH059 : ScenarioScriptFrontend
{
	[ConsoleVariable(Alias = "ss059_show_blockers")]
	private static bool ShowBlockers_cvar;

	private ScenarioScript_FH059 _script059 => (ScenarioScript_FH059)_script;

	protected override void TickInternal()
	{
		for (int i = 0; i < _script059.consoles.Count; i++)
		{
			CObjectObstacle cObjectObstacle = _script059.consoles[i];
			if (!ShowBlockers_cvar)
			{
				continue;
			}
			foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
			{
				Vector3 tileClientPos = GetTileClientPos(pathingBlocker);
				QGL.LatePrintWorld("block", tileClientPos, Color.red);
			}
		}
	}
}

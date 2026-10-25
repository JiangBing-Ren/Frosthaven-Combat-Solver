using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH046 : ScenarioScriptFrontend
{
	private ScenarioScript_FH046 _script46 => (ScenarioScript_FH046)_script;

	protected override void TickInternal()
	{
		if (ScenarioScript_FH046.DebugSnows_cvar == 0)
		{
			return;
		}
		List<Point> list = _script46.TilesSnows.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (ScenarioScriptFrontend.GetCT(list[i], out var ct) && ct.m_Tile.m_HexMap.Revealed)
			{
				Vector3 cTPos = ScenarioScriptFrontend.GetCTPos(ct);
				QGL.LatePrintWorld(_script46.TilesSnows[list[i]].ToString(), cTPos, Color.magenta, 2f);
			}
		}
	}
}

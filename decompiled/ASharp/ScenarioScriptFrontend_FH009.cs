using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH009 : ScenarioScriptFrontend
{
	private ScenarioScript_FH009 _script9 => (ScenarioScript_FH009)_script;

	protected override void TickInternal()
	{
		if (ScenarioScript_FH009.DebugLeverState_cvar == 0)
		{
			return;
		}
		for (int i = 0; i < _script9.levers.Length; i++)
		{
			if (ScenarioScriptFrontend.GetCT(_script9.levers[i].prop.ArrayIndex, out var ct) && ct.m_Tile.m_HexMap.Revealed)
			{
				Vector3 cTPos = ScenarioScriptFrontend.GetCTPos(ct);
				CActor actor = _script9.levers[i].actor;
				if (actor != null && actor.Health > 0)
				{
					QGL.LatePrintWorld("Lever is turned off.", cTPos, Color.magenta, 2f);
				}
				else if (actor != null && actor.Health <= 0)
				{
					QGL.LatePrintWorld("Lever is turned on.", cTPos, Color.green, 2f);
				}
			}
		}
	}
}

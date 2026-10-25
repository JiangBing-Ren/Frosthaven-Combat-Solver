using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH123 : ScenarioScriptFrontend
{
	private ScenarioScript_FH123 _script123 => (ScenarioScript_FH123)_script;

	protected override void TickInternal()
	{
		if (ScenarioScript_FH123.DebugPositions_cvar == 0)
		{
			return;
		}
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && ScenarioScriptFrontend.GetCT(cTile.m_ArrayIndex, out var ct))
			{
				Vector3 cTPos = ScenarioScriptFrontend.GetCTPos(ct);
				QGL.LatePrintWorld($"isStable: {ScenarioScript_FH123.IsStablePosition(cTile.m_ArrayIndex)}\nisSafe: {ScenarioScript_FH123.IsSafePosition(cTile.m_ArrayIndex)}\niWalkable: {ScenarioScript_FH123.IsWalkable(cTile)} \nisBlocked: {ScenarioScript_FH123.IsBlocked(cTile)}", cTPos, Color.magenta);
			}
		}
	}
}

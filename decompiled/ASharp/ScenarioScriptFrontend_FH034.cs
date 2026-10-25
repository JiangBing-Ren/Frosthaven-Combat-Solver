using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH034 : ScenarioScriptFrontend
{
	private ScenarioScript_FH034 _script034 => (ScenarioScript_FH034)_script;

	protected override void TickInternal()
	{
		if (!ScenarioScript_FH034.ShowChaoticBeams_cvar)
		{
			return;
		}
		if (_script034.GetChaoticBeams(out var beams))
		{
			int num = 0;
			{
				foreach (TileIndex item in beams)
				{
					CTile cTile = ScenarioManager.Tiles[item.X, item.Y];
					QGL.LatePrintWorld($"beam {num++}\n{cTile.m_HexMap.RoomName}", GetTileClientPos(item), Color.red);
				}
				return;
			}
		}
		_script.Error("No chaotic beams on map.", null, 21);
	}
}

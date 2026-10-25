using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH024 : ScenarioScriptFrontend
{
	private ScenarioScript_FH024 _script024 => (ScenarioScript_FH024)_script;

	protected override void TickInternal()
	{
		if (ScenarioScript_FH024.ShowChaoticBeams_cvar)
		{
			List<TileIndex> chaoticBeams = _script024.chaoticBeams;
			if (chaoticBeams != null)
			{
				int num = 0;
				foreach (TileIndex item in chaoticBeams)
				{
					CTile cTile = ScenarioManager.Tiles[item.X, item.Y];
					QGL.LatePrintWorld($"beam {num++}\n{item.X},{item.Y}\n{cTile.m_HexMap.RoomName}", GetTileClientPos(item.X, item.Y), Color.red);
				}
			}
			else
			{
				_script.Error("No chaotic beams on map.", null, 22);
			}
		}
		int num2 = Mathf.Clamp(ScenarioScript_FH024.ShowFxDeck_cvar, 0, 10);
		if (num2 > 0)
		{
			string fxDeckString = _script024.GetFxDeckString();
			if (!string.IsNullOrEmpty(fxDeckString))
			{
				WrapBox.DisableCanvasScale();
				WrapBox wrapBox = new WrapBox
				{
					w = Screen.width,
					h = Screen.height
				};
				Vector2 vector = QGL.MeasureString(fxDeckString, num2);
				wrapBox = wrapBox.TopRight(vector.x + 20f, vector.y + 20f);
				QGL.LateBlit(wrapBox.x, wrapBox.y, wrapBox.w, wrapBox.h, float.MaxValue, new Color(0f, 0f, 0f, 0.75f));
				wrapBox = wrapBox.Center(vector.x, vector.y);
				float x = wrapBox.x;
				float y = wrapBox.y;
				float scale = num2;
				QGL.LatePrint_tl(fxDeckString, x, y, Color.white, scale);
				WrapBox.EnableCanvasScale();
			}
		}
	}
}

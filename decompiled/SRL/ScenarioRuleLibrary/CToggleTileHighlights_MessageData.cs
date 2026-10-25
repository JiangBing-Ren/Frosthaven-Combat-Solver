using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CToggleTileHighlights_MessageData : CMessageData
{
	public IEnumerable<Point> TileIndices;

	public bool Enabled;

	public CVector4 Color;

	public bool StandingHighlights;

	public CToggleTileHighlights_MessageData(IEnumerable<Point> tileIndices, bool enabled, CVector4 color = null)
		: base(MessageType.ToggleTileHighlights, null)
	{
		TileIndices = tileIndices;
		Enabled = enabled;
		Color = color;
	}
}

using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CToggleTileLinks_MessageData : CMessageData
{
	public IEnumerable<(Point, Point)> Links;

	public bool Enabled;

	public CToggleTileLinks_MessageData(IEnumerable<(Point, Point)> links, bool enabled)
		: base(MessageType.ToggleTileLinks, null)
	{
		Links = links;
		Enabled = enabled;
	}
}

using AStar;

namespace ScenarioRuleLibrary;

public class CFocusCamera_MessageData : CMessageData
{
	public Point m_Point;

	public float? m_Zoom;

	public float m_delay;

	public CFocusCamera_MessageData(CActor actorSpawningMessage)
		: base(MessageType.FocusCamera, actorSpawningMessage)
	{
	}
}

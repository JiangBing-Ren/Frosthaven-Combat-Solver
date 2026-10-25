using AStar;

namespace ScenarioRuleLibrary;

public class CLerpProp_MessageData : CMessageData
{
	public CActor Actor;

	public bool UseActorSpeed;

	public float Speed;

	public CObjectProp Prop;

	public Point TargetLocation;

	public CVector3 TargetRotation;

	public CLerpProp_MessageData()
		: base(MessageType.LerpProp, null)
	{
	}
}

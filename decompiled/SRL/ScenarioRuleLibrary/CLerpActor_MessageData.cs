using AStar;

namespace ScenarioRuleLibrary;

public class CLerpActor_MessageData : CMessageData
{
	public CActor Actor;

	public CActor DriverActor;

	public bool UseDriverActorSpeed;

	public float Speed;

	public Point TargetLocation;

	public CVector3 TargetRotation;

	public CLerpActor_MessageData()
		: base(MessageType.LerpActor, null)
	{
	}
}

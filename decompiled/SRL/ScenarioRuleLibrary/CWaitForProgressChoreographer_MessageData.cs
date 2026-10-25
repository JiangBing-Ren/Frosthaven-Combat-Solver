namespace ScenarioRuleLibrary;

public class CWaitForProgressChoreographer_MessageData : CMessageData
{
	public int WaitTickFrame;

	public CActor WaitActor;

	public bool ClearEvents;

	public CWaitForProgressChoreographer_MessageData(CActor actorSpawningMessage)
		: base(MessageType.WaitForProgressChoreographer, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CForcedWaitForProgressChoreographer_MessageData : CMessageData
{
	public int WaitTickFrame;

	public CForcedWaitForProgressChoreographer_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ForcedWaitForProgressChoreographer, actorSpawningMessage)
	{
	}
}

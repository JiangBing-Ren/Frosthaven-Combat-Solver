namespace ScenarioRuleLibrary;

public class CUpdateCurrentActor_MessageData : CMessageData
{
	public CUpdateCurrentActor_MessageData(CActor actorSpawningMessage)
		: base(MessageType.UpdateCurrentActor, actorSpawningMessage)
	{
	}
}

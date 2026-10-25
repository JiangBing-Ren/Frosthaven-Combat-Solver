namespace ScenarioRuleLibrary;

public class CProcessAutosaves_MessageData : CMessageData
{
	public CProcessAutosaves_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ProcessAutosaves, actorSpawningMessage)
	{
	}
}

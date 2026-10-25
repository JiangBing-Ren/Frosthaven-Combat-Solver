namespace ScenarioRuleLibrary;

public class CFinishedDrawingModifier_MessageData : CMessageData
{
	public CFinishedDrawingModifier_MessageData(CActor actorSpawningMessage)
		: base(MessageType.FinishedDrawingModifier, actorSpawningMessage)
	{
	}
}

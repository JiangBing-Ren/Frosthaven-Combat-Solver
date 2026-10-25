namespace ScenarioRuleLibrary;

public class CStartedDrawingModifier_MessageData : CMessageData
{
	public CStartedDrawingModifier_MessageData(CActor actorSpawningMessage)
		: base(MessageType.StartedDrawingModifier, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CEndInitiativeAdjustments_MessageData : CMessageData
{
	public CEndInitiativeAdjustments_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndInitiativeAdjustments, actorSpawningMessage)
	{
	}
}

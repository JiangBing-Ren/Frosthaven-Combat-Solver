namespace ScenarioRuleLibrary;

public class CCheckForInitiativeAdjustments_MessageData : CMessageData
{
	public CCheckForInitiativeAdjustments_MessageData(CActor actorSpawningMessage)
		: base(MessageType.CheckForInitiativeAdjustments, actorSpawningMessage)
	{
	}
}

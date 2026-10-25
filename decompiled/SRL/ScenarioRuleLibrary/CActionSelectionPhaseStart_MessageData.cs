namespace ScenarioRuleLibrary;

public class CActionSelectionPhaseStart_MessageData : CMessageData
{
	public CActionSelectionPhaseStart_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActionSelectionPhaseStart, actorSpawningMessage)
	{
	}
}

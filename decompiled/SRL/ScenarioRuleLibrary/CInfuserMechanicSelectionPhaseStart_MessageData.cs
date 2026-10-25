namespace ScenarioRuleLibrary;

public class CInfuserMechanicSelectionPhaseStart_MessageData : CMessageData
{
	public CInfuserMechanicSelectionPhaseStart_MessageData(CActor actorSpawningMessage)
		: base(MessageType.InfuserMechanicSelectionPhaseStart, actorSpawningMessage)
	{
	}
}

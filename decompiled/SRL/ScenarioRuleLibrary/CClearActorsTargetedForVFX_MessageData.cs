namespace ScenarioRuleLibrary;

public class CClearActorsTargetedForVFX_MessageData : CMessageData
{
	public CClearActorsTargetedForVFX_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ClearActorsTargetedForVFX, actorSpawningMessage)
	{
	}
}

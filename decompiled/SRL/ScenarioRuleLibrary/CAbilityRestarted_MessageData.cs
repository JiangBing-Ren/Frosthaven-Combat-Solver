namespace ScenarioRuleLibrary;

public class CAbilityRestarted_MessageData : CMessageData
{
	public CAbilityRestarted_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AbilityRestarted, actorSpawningMessage)
	{
	}
}

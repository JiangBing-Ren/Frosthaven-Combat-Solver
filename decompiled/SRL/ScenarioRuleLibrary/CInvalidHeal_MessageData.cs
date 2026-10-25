namespace ScenarioRuleLibrary;

public class CInvalidHeal_MessageData : CMessageData
{
	public CInvalidHeal_MessageData(CActor actorSpawningMessage)
		: base(MessageType.InvalidHeal, actorSpawningMessage)
	{
	}
}

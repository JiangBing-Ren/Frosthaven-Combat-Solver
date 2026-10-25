namespace ScenarioRuleLibrary;

public class CStartSecondMergedAbility_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CStartSecondMergedAbility_MessageData(CActor actorSpawningMessage)
		: base(MessageType.StartSecondMergedAbility, actorSpawningMessage)
	{
	}
}

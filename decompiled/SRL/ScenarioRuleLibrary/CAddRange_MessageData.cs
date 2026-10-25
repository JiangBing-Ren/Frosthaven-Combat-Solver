namespace ScenarioRuleLibrary;

public class CAddRange_MessageData : CMessageData
{
	public CAbilityAddRange m_AddRangeAbility;

	public CActor m_ActorAppliedTo;

	public CAddRange_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AddRange, actorSpawningMessage, animOverload)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CRetaliate_MessageData : CMessageData
{
	public CAbilityRetaliate m_RetaliateAbility;

	public CActor m_ActorAppliedTo;

	public bool m_ConditionAlreadyApplied;

	public CRetaliate_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Retaliate, actorSpawningMessage, animOverload)
	{
	}
}

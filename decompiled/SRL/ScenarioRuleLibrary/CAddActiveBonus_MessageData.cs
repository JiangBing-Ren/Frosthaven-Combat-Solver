namespace ScenarioRuleLibrary;

public class CAddActiveBonus_MessageData : CMessageData
{
	public CAbilityAddActiveBonus m_AddActiveBonus;

	public CActor m_ActorAppliedTo;

	public CAddActiveBonus_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AddActiveBonus, actorSpawningMessage, animOverload)
	{
	}
}

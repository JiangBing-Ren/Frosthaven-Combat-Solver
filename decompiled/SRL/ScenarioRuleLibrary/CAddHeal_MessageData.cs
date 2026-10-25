namespace ScenarioRuleLibrary;

public class CAddHeal_MessageData : CMessageData
{
	public CAbilityAddHeal m_AddHealAbility;

	public CActor m_ActorAppliedTo;

	public CAddHeal_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AddHeal, actorSpawningMessage, animOverload)
	{
	}
}

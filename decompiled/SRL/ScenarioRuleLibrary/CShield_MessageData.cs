namespace ScenarioRuleLibrary;

public class CShield_MessageData : CMessageData
{
	public CAbilityShield m_ShieldAbility;

	public CActor m_ActorAppliedTo;

	public CShield_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Shield, actorSpawningMessage, animOverload)
	{
	}
}

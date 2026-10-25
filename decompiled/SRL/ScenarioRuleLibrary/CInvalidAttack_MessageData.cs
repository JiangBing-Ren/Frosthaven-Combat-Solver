namespace ScenarioRuleLibrary;

public class CInvalidAttack_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_Ability;

	public CInvalidAttack_MessageData(CActor actorSpawningMessage)
		: base(MessageType.InvalidAttack, actorSpawningMessage)
	{
	}
}

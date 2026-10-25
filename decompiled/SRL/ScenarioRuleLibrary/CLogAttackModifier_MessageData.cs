namespace ScenarioRuleLibrary;

public class CLogAttackModifier_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CActor m_ActorBeingAttacked;

	public CAbility m_Ability;

	public SEventAbilityAttack m_AttackEvent;

	public SEventAttackModifier m_ModifierEvent;

	public CLogAttackModifier_MessageData(CActor actorSpawningMessage)
		: base(MessageType.LogAttackModifier, actorSpawningMessage)
	{
	}
}

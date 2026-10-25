namespace ScenarioRuleLibrary;

public class CLogDamageCalculation_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CActor m_ActorBeingAttacked;

	public CAbility m_Ability;

	public SEventAbilityDamage m_DamageEvent;

	public SEventAbilityAttack m_AttackEvent;

	public SEventAttackModifier m_ModifierEvent;

	public string m_OverrideLogKey;

	public CLogDamageCalculation_MessageData(CActor actorSpawningMessage)
		: base(MessageType.LogDamageCalculation, actorSpawningMessage)
	{
	}
}

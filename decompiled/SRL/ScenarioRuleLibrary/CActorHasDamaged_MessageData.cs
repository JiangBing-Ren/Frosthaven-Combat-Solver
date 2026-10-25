namespace ScenarioRuleLibrary;

public class CActorHasDamaged_MessageData : CMessageData
{
	public CAbilityDamage m_DamageAbility;

	public CActorHasDamaged_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorHasDamaged, actorSpawningMessage, animOverload)
	{
	}
}

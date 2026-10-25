namespace ScenarioRuleLibrary;

public class CActorIsSelectingDamageFocus_MessageData : CMessageData
{
	public CAbilityDamage m_DamageAbility;

	public bool m_DamageSelf;

	public CActorIsSelectingDamageFocus_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingDamageFocus, actorSpawningMessage, animOverload)
	{
	}
}

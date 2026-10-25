namespace ScenarioRuleLibrary;

public class CDamageBuff_MessageData : CMessageData
{
	public CAbilityDamage m_DamageAbility;

	public CDamageBuff_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.DamageBuff, actorSpawningMessage, animOverload)
	{
	}
}

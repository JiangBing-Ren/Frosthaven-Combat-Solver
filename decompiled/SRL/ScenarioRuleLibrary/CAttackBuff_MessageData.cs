namespace ScenarioRuleLibrary;

public class CAttackBuff_MessageData : CMessageData
{
	public CAbilityAttack m_AttackAbility;

	public CAttackBuff_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AttackBuff, actorSpawningMessage, animOverload)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CAttackersGainDisadvantage_MessageData : CMessageData
{
	public CAbilityAttackersGainDisadvantage m_AttackersGainDisadvantageAbility;

	public CActor m_ActorAppliedTo;

	public CAttackersGainDisadvantage_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AttackersGainDisadvantage, actorSpawningMessage, animOverload)
	{
	}
}

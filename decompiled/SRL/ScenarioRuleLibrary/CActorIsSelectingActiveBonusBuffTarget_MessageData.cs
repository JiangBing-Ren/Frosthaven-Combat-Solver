namespace ScenarioRuleLibrary;

public class CActorIsSelectingActiveBonusBuffTarget_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_AttackAbility;

	public CAttackSummary m_AttackSummary;

	public CActiveBonus m_ActiveBonus;

	public CActorIsSelectingActiveBonusBuffTarget_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingActiveBonusBuffTarget, actorSpawningMessage)
	{
	}
}

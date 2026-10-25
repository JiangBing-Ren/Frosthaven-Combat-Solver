namespace ScenarioRuleLibrary;

public class CUpdateAttackFocusAfterAttackEffectInlineSubAbility : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_AttackAbility;

	public CAttackSummary m_AttackSummary;

	public CUpdateAttackFocusAfterAttackEffectInlineSubAbility(CActor actorSpawningMessage)
		: base(MessageType.UpdateAttackFocusAfterAttackEffectInlineSubAbility, actorSpawningMessage)
	{
	}
}

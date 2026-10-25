namespace ScenarioRuleLibrary;

public class CApplyToActorOverrideAugmentAttackType_MessageData : CMessageData
{
	public CAbilityOverrideAugmentAttackType m_OverrideAugmentAttackTypeAbility;

	public CActor m_Target;

	public CApplyToActorOverrideAugmentAttackType_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ApplyToActorOverrideAugmentAttackType, actorSpawningMessage)
	{
	}
}

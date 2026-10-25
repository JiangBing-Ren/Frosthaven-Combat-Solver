namespace ScenarioRuleLibrary;

public class CActorIsApplyingOverrideAugmentAttackType_MessageData : CMessageData
{
	public CAbilityOverrideAugmentAttackType m_OverrideAugmentAttackTypeAbility;

	public CActorIsApplyingOverrideAugmentAttackType_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsApplyingOverrideAugmentAttackType, actorSpawningMessage, animOverload)
	{
	}
}

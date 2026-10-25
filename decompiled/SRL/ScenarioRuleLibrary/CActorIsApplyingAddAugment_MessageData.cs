namespace ScenarioRuleLibrary;

public class CActorIsApplyingAddAugment_MessageData : CMessageData
{
	public CAbilityAddAugment m_AddAugmentAbility;

	public CActorIsApplyingAddAugment_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsApplyingAddAugment, actorSpawningMessage, animOverload)
	{
	}
}

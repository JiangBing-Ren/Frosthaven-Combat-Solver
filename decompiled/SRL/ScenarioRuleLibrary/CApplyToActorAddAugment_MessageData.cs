namespace ScenarioRuleLibrary;

public class CApplyToActorAddAugment_MessageData : CMessageData
{
	public CAbilityAddAugment m_AddAugmentAbility;

	public CActor m_Target;

	public CApplyToActorAddAugment_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ApplyToActorAddAugment, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CUpdateAdditionalPushDamagePreview_MessageData : CMessageData
{
	public CAttackSummary m_AdditionalPushDamageSummary;

	public CAbilityPush m_PushAbility;

	public CUpdateAdditionalPushDamagePreview_MessageData(CActor actorSpawningMessage)
		: base(MessageType.UpdateAdditionalPushDamagePreview, actorSpawningMessage)
	{
	}
}

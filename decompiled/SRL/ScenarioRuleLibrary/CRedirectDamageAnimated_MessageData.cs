namespace ScenarioRuleLibrary;

public class CRedirectDamageAnimated_MessageData : CMessageData
{
	public CActor m_RedirectingActor;

	public CActor m_TargetedActor;

	public CRedirectDamageAnimated_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.RedirectDamageAnimated, actorSpawningMessage, animOverload)
	{
	}
}

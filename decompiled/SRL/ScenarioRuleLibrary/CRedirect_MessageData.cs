namespace ScenarioRuleLibrary;

public class CRedirect_MessageData : CMessageData
{
	public CAbilityRedirect m_RedirectAbility;

	public CActor m_ActorAppliedTo;

	public CRedirect_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Redirect, actorSpawningMessage, animOverload)
	{
	}
}

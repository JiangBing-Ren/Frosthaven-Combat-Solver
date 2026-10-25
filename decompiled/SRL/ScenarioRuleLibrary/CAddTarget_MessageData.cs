namespace ScenarioRuleLibrary;

public class CAddTarget_MessageData : CMessageData
{
	public CAbilityAddTarget m_AddTargetAbility;

	public CActor m_ActorAppliedTo;

	public CAddTarget_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AddTarget, actorSpawningMessage, animOverload)
	{
	}
}

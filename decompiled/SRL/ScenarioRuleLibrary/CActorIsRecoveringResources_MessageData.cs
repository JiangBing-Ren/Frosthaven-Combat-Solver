namespace ScenarioRuleLibrary;

public class CActorIsRecoveringResources_MessageData : CMessageData
{
	public CActor m_ActorRecovering;

	public CAbility m_RecoverAbility;

	public CActorIsRecoveringResources_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsRecoveringResources, actorSpawningMessage)
	{
	}
}

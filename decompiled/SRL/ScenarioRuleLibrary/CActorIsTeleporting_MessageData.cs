namespace ScenarioRuleLibrary;

public class CActorIsTeleporting_MessageData : CMessageData
{
	public CActor m_ActorTeleporting;

	public CAbility m_TeleportAbility;

	public CActorIsTeleporting_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsTeleporting, actorSpawningMessage)
	{
	}
}

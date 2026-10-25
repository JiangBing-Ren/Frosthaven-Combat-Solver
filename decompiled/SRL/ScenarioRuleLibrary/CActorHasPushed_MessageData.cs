namespace ScenarioRuleLibrary;

public class CActorHasPushed_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorHasPushed, actorSpawningMessage)
{
	public CActor m_ActorBeingPushed;

	public CAbilityPush m_PushAbility;

	public bool m_ChoreographerWait = true;
}

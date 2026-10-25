namespace ScenarioRuleLibrary;

public class CActorIsSelectingTargetingFocus_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.ActorIsSelectingTargetingFocus, actorSpawningMessage)
{
	public CAbility m_TargetingAbility;

	public bool m_IsPositive;

	public bool m_ObjectiveRelated;

	public CActor m_CameraFocusActor;

	public bool m_CanUndo = true;
}

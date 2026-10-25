using AStar;

namespace ScenarioRuleLibrary;

public class CActorHasTeleported_MessageData : CMessageData
{
	public Point m_StartLocation;

	public Point m_EndLocation;

	public CActor m_ActorTeleported;

	public CAbility m_TeleportAbility;

	public bool m_UseFade;

	public bool m_UseAnimation;

	public CActorHasTeleported_MessageData(CActor actorSpawningMessage, bool useAnim = true, bool useFade = false)
		: base(MessageType.ActorHasTeleported, actorSpawningMessage)
	{
		m_UseAnimation = useAnim;
		m_UseFade = useFade;
	}
}

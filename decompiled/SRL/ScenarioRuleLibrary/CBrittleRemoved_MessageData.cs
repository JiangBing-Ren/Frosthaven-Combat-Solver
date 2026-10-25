namespace ScenarioRuleLibrary;

public class CBrittleRemoved_MessageData : CMessageData
{
	public CActor m_Actor;

	public int m_ActorOriginalHealth;

	public bool m_skipHitAnimation;

	public CBrittleRemoved_MessageData(CActor actorSpawningMessage, int actorOriginalHealth)
		: base(MessageType.BrittleRemoved, actorSpawningMessage)
	{
		m_Actor = actorSpawningMessage;
		m_ActorOriginalHealth = actorOriginalHealth;
	}
}

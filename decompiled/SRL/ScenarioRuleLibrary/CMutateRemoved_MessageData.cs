namespace ScenarioRuleLibrary;

public class CMutateRemoved_MessageData : CMessageData
{
	public CActor m_Actor;

	public int m_ActorOriginalHealth;

	public CMutateRemoved_MessageData(CActor actorSpawningMessage, int actorOriginalHealth)
		: base(MessageType.MutateRemoved, actorSpawningMessage)
	{
		m_Actor = actorSpawningMessage;
		m_ActorOriginalHealth = actorOriginalHealth;
	}
}

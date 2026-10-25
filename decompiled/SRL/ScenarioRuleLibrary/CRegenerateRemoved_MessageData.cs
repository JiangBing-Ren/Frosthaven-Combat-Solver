namespace ScenarioRuleLibrary;

public class CRegenerateRemoved_MessageData : CMessageData
{
	public CActor m_ActorDamaged;

	public CRegenerateRemoved_MessageData(CActor actorSpawningMessage)
		: base(MessageType.RegenerateRemoved, actorSpawningMessage)
	{
		m_ActorDamaged = actorSpawningMessage;
	}
}

namespace ScenarioRuleLibrary;

public class CPoisonTriggered_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CActor m_PoisonedActor;

	public CPoisonTriggered_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PoisonTriggered, actorSpawningMessage)
	{
	}
}

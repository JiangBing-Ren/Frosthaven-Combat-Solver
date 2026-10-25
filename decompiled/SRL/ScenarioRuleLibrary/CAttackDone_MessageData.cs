namespace ScenarioRuleLibrary;

public class CAttackDone_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAttackDone_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AttackDone, actorSpawningMessage)
	{
	}
}

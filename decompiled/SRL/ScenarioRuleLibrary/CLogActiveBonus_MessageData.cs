namespace ScenarioRuleLibrary;

public class CLogActiveBonus_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public CActor m_ActorAppliedTo;

	public CLogActiveBonus_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.LogActiveBonus, actorSpawningMessage, animOverload)
	{
	}
}

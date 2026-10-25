namespace ScenarioRuleLibrary;

public class CTargetShield_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public CActor m_ActorAttacking;

	public bool m_PlaySound;

	public CTargetShield_MessageData(CActor actorSpawningMessage)
		: base(MessageType.TargetShield, actorSpawningMessage)
	{
	}
}

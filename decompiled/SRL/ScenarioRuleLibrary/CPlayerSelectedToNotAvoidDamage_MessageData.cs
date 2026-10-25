namespace ScenarioRuleLibrary;

public class CPlayerSelectedToNotAvoidDamage_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public int m_ActorOriginalHealth;

	public CPlayerSelectedToNotAvoidDamage_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectedToNotAvoidDamage, actorSpawningMessage)
	{
	}
}

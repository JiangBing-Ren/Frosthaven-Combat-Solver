namespace ScenarioRuleLibrary;

public class CSupplyCardUsed_MessageData : CMessageData
{
	public CPlayerActor m_ActorUsedCard;

	public CAbilityCard m_SupplyCardUsed;

	public CSupplyCardUsed_MessageData(CActor actorSpawningMessage)
		: base(MessageType.SupplyCardUsed, actorSpawningMessage)
	{
	}
}

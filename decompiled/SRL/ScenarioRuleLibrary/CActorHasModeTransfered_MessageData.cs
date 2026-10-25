namespace ScenarioRuleLibrary;

public class CActorHasModeTransfered_MessageData : CMessageData
{
	public CAbilityModeTransfer m_ModeTransferAbility;

	public CTile m_DestinationTile;

	public CActorHasModeTransfered_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorHasModeTransfered, actorSpawningMessage)
	{
	}
}

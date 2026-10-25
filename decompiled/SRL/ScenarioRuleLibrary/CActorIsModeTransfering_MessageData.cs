namespace ScenarioRuleLibrary;

public class CActorIsModeTransfering_MessageData : CMessageData
{
	public CAbilityModeTransfer m_ModeTransferAbility;

	public CTile m_TeleportToTile;

	public CActorIsModeTransfering_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsModeTransfering, actorSpawningMessage)
	{
	}
}

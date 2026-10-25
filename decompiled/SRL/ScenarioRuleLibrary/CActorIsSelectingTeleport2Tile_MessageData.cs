namespace ScenarioRuleLibrary;

public class CActorIsSelectingTeleport2Tile_MessageData : CMessageData
{
	public CAbilityTeleport2 m_Teleport2Ability;

	public CActorIsSelectingTeleport2Tile_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingTeleport2Tile, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CActorIsSelectingPushTile_MessageData : CMessageData
{
	public CAbilityPush m_PushAbility;

	public CActorIsSelectingPushTile_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingPushTile, actorSpawningMessage)
	{
	}
}

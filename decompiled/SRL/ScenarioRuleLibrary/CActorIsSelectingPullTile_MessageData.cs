namespace ScenarioRuleLibrary;

public class CActorIsSelectingPullTile_MessageData : CMessageData
{
	public CAbilityPull m_PullAbility;

	public CActorIsSelectingPullTile_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingPullTile, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CActorIsSelectingMoveTile_MessageData : CMessageData
{
	public CAbilityMove m_MoveAbility;

	public CActorIsSelectingMoveTile_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingMoveTile, actorSpawningMessage)
	{
	}
}

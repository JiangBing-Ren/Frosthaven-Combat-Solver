namespace ScenarioRuleLibrary;

public class CActorSelectedTileToPullTowards : CMessageData
{
	public CAbilityPull m_PullAbility;

	public CTile m_TileToPullTowards;

	public CActorSelectedTileToPullTowards(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorSelectedTileToPullTowards, actorSpawningMessage, animOverload)
	{
	}
}

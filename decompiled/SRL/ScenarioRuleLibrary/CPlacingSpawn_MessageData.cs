namespace ScenarioRuleLibrary;

public class CPlacingSpawn_MessageData : CMessageData
{
	public CActor m_ActorPlacingSpawn;

	public CAbility m_Ability;

	public CPlacingSpawn_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PlacingSpawn, actorSpawningMessage, animOverload)
	{
	}
}

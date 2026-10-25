namespace ScenarioRuleLibrary;

public class CActorSelectedNullHexes_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CActorSelectedNullHexes_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorSelectedNullHexes, actorSpawningMessage, animOverload)
	{
	}
}

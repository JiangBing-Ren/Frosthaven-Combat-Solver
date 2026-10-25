namespace ScenarioRuleLibrary;

public class CActorBeenDoomed_MessageData : CMessageData
{
	public CActor m_ActorBeingDoomed;

	public CAbilityAddDoom m_DoomAbility;

	public CActorBeenDoomed_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorBeenDoomed, actorSpawningMessage)
	{
	}
}

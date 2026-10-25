namespace ScenarioRuleLibrary;

public class CActorIsRepeatingAbilities : CMessageData
{
	public CAbilityRepeatAbility m_RepeatAbility;

	public CActorIsRepeatingAbilities(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsRepeatingAbilities, actorSpawningMessage, animOverload)
	{
	}
}

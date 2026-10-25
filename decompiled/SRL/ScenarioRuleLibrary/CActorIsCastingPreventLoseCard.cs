namespace ScenarioRuleLibrary;

public class CActorIsCastingPreventLoseCard : CMessageData
{
	public CAbilityPreventLoseCard m_PreventLoseCardAbility;

	public CActorIsCastingPreventLoseCard(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsCastingPreventLoseCard, actorSpawningMessage, animOverload)
	{
	}
}

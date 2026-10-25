namespace ScenarioRuleLibrary;

public class CActorIsControlled_MessageData : CMessageData
{
	public CActor m_ControlledActor;

	public CAbilityControlActor m_ControlActorAbility;

	public CActorIsControlled_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsControlled, actorSpawningMessage)
	{
	}
}

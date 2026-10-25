namespace ScenarioRuleLibrary;

public class CActorIsCastingSwap_MessageData : CMessageData
{
	public CActor m_ActorCasting;

	public CActor m_FirstTarget;

	public CActor m_SecondTarget;

	public CAbility m_SwapAbility;

	public CActorIsCastingSwap_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsCastingSwap, actorSpawningMessage)
	{
	}
}

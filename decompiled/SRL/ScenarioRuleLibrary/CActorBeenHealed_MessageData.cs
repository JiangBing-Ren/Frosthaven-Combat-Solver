namespace ScenarioRuleLibrary;

public class CActorBeenHealed_MessageData : CMessageData
{
	public CActor m_ActorBeingHealed;

	public int m_HealAmount;

	public int m_ActorOriginalHealth;

	public bool m_PoisonTokenRemoved;

	public bool m_WoundTokenRemoved;

	public bool m_BaneTokenRemoved;

	public bool m_BrittleTokenRemoved;

	public CActorBeenHealed_MessageData(CActor actorSpawningMessage, CActor targetActor, int healAmount, int actorOriginalHealth, bool poisonTokenRemoved, bool woundTokenRemoved, bool baneTokenRemoved, bool brittleTokenRemoved)
		: base(MessageType.ActorBeenHealed, actorSpawningMessage)
	{
		m_ActorBeingHealed = targetActor;
		m_HealAmount = healAmount;
		m_ActorOriginalHealth = actorOriginalHealth;
		m_PoisonTokenRemoved = poisonTokenRemoved;
		m_WoundTokenRemoved = woundTokenRemoved;
		m_BaneTokenRemoved = baneTokenRemoved;
		m_BrittleTokenRemoved = brittleTokenRemoved;
	}
}

namespace ScenarioRuleLibrary;

public class CActorDead_MessageData : CMessageData
{
	public CActor m_Actor;

	public bool m_OnDeathAbility;

	public bool m_ActorWasAsleep;

	public bool m_InstantlyDeletePrefab;

	public bool m_DontLog;

	public CActorDead_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorDead, actorSpawningMessage)
	{
	}
}

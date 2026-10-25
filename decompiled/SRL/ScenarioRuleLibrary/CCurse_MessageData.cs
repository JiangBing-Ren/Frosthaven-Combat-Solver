namespace ScenarioRuleLibrary;

public class CCurse_MessageData : CMessageData
{
	public CActor m_ActorToCurse;

	public bool m_DifficultyModCurse;

	public bool m_ConditionAlreadyApplied;

	public CCurse_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Curse, actorSpawningMessage, animOverload)
	{
	}
}

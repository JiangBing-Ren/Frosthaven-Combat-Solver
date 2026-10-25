namespace ScenarioRuleLibrary;

public class CRegenerate_MessageData : CMessageData
{
	public CActor m_ActorToRegen;

	public bool m_ConditionAlreadyApplied;

	public CRegenerate_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Regenerate, actorSpawningMessage, animOverload)
	{
	}
}

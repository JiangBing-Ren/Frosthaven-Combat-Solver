namespace ScenarioRuleLibrary;

public class CEndedExtraTurn : CMessageData
{
	public CActor m_ActorTurnEnded;

	public CEndedExtraTurn(CActor actorSpawningMessage)
		: base(MessageType.EndedExtraTurn, actorSpawningMessage)
	{
	}
}

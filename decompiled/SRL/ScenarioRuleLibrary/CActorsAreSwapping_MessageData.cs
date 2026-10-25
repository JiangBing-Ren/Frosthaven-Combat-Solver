namespace ScenarioRuleLibrary;

public class CActorsAreSwapping_MessageData : CMessageData
{
	public CActor m_FirstTarget;

	public CActor m_SecondTarget;

	public CAbility m_SwapAbility;

	public CActorsAreSwapping_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorsAreSwapping, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CPhaseIn_MessageData : CMessageData
{
	public CTile m_Tile;

	public CActor m_Actor;

	public CPhaseIn_MessageData()
		: base(MessageType.PhaseIn, null)
	{
	}
}

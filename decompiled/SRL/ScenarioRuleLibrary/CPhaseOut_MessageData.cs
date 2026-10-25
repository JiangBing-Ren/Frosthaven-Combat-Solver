namespace ScenarioRuleLibrary;

public class CPhaseOut_MessageData : CMessageData
{
	public CTile m_Tile;

	public CActor m_Actor;

	public bool m_PhaseOutInstantly;

	public CPhaseOut_MessageData()
		: base(MessageType.PhaseOut, null)
	{
	}
}

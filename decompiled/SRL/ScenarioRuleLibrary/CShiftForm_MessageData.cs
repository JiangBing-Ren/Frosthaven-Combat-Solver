namespace ScenarioRuleLibrary;

public class CShiftForm_MessageData : CMessageData
{
	public CTile m_Tile;

	public CEnemyActor m_Actor;

	public CShiftForm_MessageData()
		: base(MessageType.ShiftForm, null)
	{
	}
}

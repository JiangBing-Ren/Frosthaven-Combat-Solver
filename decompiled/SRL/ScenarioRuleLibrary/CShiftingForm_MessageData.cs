namespace ScenarioRuleLibrary;

public class CShiftingForm_MessageData : CMessageData
{
	public CTile m_Tile;

	public CEnemyActor m_Actor;

	public CShiftingForm_MessageData()
		: base(MessageType.ShiftingForm, null)
	{
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CFinishedProcessingTileDeselected_MessageData : CMessageData
{
	public CTile m_SelectedTile;

	public List<CTile> m_OptionalTileList;

	public CAbility m_Ability;

	public bool m_CanUndo;

	public bool m_AreaEffectLocked;

	public CFinishedProcessingTileDeselected_MessageData()
		: base(MessageType.FinishedProcessingTileDeselected, null)
	{
	}
}

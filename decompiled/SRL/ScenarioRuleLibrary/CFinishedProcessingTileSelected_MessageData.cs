using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CFinishedProcessingTileSelected_MessageData : CMessageData
{
	public CTile m_SelectedTile;

	public List<CTile> m_OptionalTileList;

	public CAbility m_Ability;

	public bool m_AreaEffectLocked;

	public CFinishedProcessingTileSelected_MessageData()
		: base(MessageType.FinishedProcessingTileSelected, null)
	{
	}
}

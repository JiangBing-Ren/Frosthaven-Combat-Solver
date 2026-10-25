using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlayerSelectingObjectPosition_MessageData : CMessageData
{
	public CAbility m_Ability;

	public ScenarioManager.ObjectImportType m_SpawnType;

	public List<CAbilityFilter.EFilterTile> m_TileFilter;

	public bool m_ShowConsumeBar;

	public bool m_ShowActiveBonusBar = true;

	public bool m_IsPositive;

	public CPlayerSelectingObjectPosition_MessageData(CAbility ability, CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectingObjectPosition, actorSpawningMessage)
	{
		m_Ability = ability;
		if (m_Ability != null)
		{
			m_SpawnType = m_Ability.SpawnTypePreview;
		}
		else
		{
			m_SpawnType = ScenarioManager.ObjectImportType.None;
		}
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class COverrideItemUsedUpdateCombatLog_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.OverrideItemUsedUpdateCombatLog, actorSpawningMessage)
{
	public List<CItem> m_ItemsToUpdate = new List<CItem>();
}

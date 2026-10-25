using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CTriggeredObjectivesEventIdList_MessageData : CMessageData
{
	public List<string> m_TriggeredObjectiveEventIdList;

	public CTriggeredObjectivesEventIdList_MessageData()
		: base(MessageType.TriggeredObjectivesEventIdList, null)
	{
	}
}

using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CCombatLogMessage_MessageData : CMessageData
{
	public string m_MessageKey;

	public List<string> m_Params;

	public List<CActor> m_Actors;

	public CCombatLogMessage_MessageData()
		: base(MessageType.CombatLogMessage, null)
	{
	}
}

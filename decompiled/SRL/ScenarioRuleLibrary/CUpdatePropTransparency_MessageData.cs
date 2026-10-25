using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary;

[Obsolete]
public class CUpdatePropTransparency_MessageData : CMessageData
{
	public List<CObjectObstacle> m_PropList;

	public CUpdatePropTransparency_MessageData()
		: base(MessageType.UpdatePropTransparency, null)
	{
	}
}

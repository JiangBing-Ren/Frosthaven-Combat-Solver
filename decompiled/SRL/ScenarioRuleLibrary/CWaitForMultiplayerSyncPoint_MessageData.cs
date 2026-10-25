using System;

namespace ScenarioRuleLibrary;

public class CWaitForMultiplayerSyncPoint_MessageData : CMessageData
{
	public Action m_OnFinish;

	public CWaitForMultiplayerSyncPoint_MessageData()
		: base(MessageType.WaitForMultiplayerSyncPoint, null)
	{
	}
}

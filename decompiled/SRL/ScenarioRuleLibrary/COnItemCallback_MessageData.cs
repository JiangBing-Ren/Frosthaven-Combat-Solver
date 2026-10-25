using System;

namespace ScenarioRuleLibrary;

public class COnItemCallback_MessageData : CMessageData
{
	public EventHandler m_Callback;

	public CItem m_Item;

	public ESESubTypeItem m_CallbackType;

	public bool m_HideLog;

	public COnItemCallback_MessageData(CActor actorSpawningMessage)
		: base(MessageType.OnItemCallback, actorSpawningMessage)
	{
	}
}

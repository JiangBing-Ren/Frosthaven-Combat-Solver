using System;

namespace ScenarioRuleLibrary;

public class CWaitForActorToBeAtTile_MessageData : CMessageData
{
	public CTile m_TargetTile;

	public CObjectProp m_WaitForProp;

	public Action m_OnFinish;

	public CWaitForActorToBeAtTile_MessageData(CActor actorSpawningMessage, CTile tile, Action onFinish = null)
		: base(MessageType.WaitForActorToBeAtTile, actorSpawningMessage)
	{
		m_TargetTile = tile;
		m_OnFinish = onFinish;
	}

	public CWaitForActorToBeAtTile_MessageData(CObjectProp WaitForProp, CTile tile, Action onFinish = null)
		: base(MessageType.WaitForActorToBeAtTile, null)
	{
		m_TargetTile = tile;
		m_OnFinish = onFinish;
		m_WaitForProp = WaitForProp;
	}
}

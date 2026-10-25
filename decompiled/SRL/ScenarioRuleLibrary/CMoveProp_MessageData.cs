using System;

namespace ScenarioRuleLibrary;

public class CMoveProp_MessageData : CMessageData
{
	public CObjectProp m_MoveProp;

	public CTile m_MoveToTile;

	public float m_MoveSpeed;

	public CTile m_MoveFromTile;

	public CVector3 m_MoveFromRotation;

	public CVector3 m_MoveToRotation;

	public Action m_OnFinish;

	public bool m_MoveLogicPos = true;

	public CMoveProp_MessageData()
		: base(MessageType.MoveProp, null)
	{
	}
}

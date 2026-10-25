using AStar;

namespace ScenarioRuleLibrary;

public class CTryPlayVfxOnTile_MessageData : CMessageData
{
	public string m_VFXPropType;

	public Point m_ArrayIndex;

	public Point m_faceTowards;

	public CVector3 m_Rotation;

	public CTryPlayVfxOnTile_MessageData(CActor actorSpawningMessage)
		: base(MessageType.TryPlayVfxOnTile, actorSpawningMessage)
	{
	}
}

namespace ScenarioRuleLibrary;

public class CSpawn_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public float m_SpawnDelay;

	public CSpawn_MessageData(CActor actorSpawningMessage)
		: base(MessageType.Spawn, actorSpawningMessage)
	{
	}
}

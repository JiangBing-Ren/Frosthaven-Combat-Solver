namespace ScenarioRuleLibrary;

public class CSpawnObject_MessageData : CMessageData
{
	public CTile m_Tile;

	public string m_ObjectID;

	public CActor.EType m_enemyType;

	public int m_Level;

	public float m_SpawnDelay;

	public CObjectProp m_PropToAlignTo;

	public CSpawnObject_MessageData(CActor actor, CTile tile, string objectID, int level, float spawnDelay, CActor.EType enemyType, CObjectProp propToAlignTo = null)
		: base(MessageType.SpawnObject, actor)
	{
		m_Tile = tile;
		m_ObjectID = objectID;
		m_enemyType = enemyType;
		m_Level = level;
		m_SpawnDelay = spawnDelay;
		m_PropToAlignTo = propToAlignTo;
	}
}

namespace ScenarioRuleLibrary;

public class CSpawnerSpawningUnit_MessageData : CMessageData
{
	public CTile m_SpawnTile;

	public CEnemyActor m_SpawnEnemy;

	public int m_SpawnDelay;

	public bool m_TreatAsSummon;

	public CActor m_SpawnSource;

	public CSpawnerSpawningUnit_MessageData()
		: base(MessageType.SpawnerSpawningUnit, null)
	{
	}
}

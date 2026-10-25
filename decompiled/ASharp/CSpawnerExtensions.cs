using System.Collections.Generic;
using ScenarioRuleLibrary;

public static class CSpawnerExtensions
{
	public const string DontSpawnID = "DON'T SPAWN";

	public static CSpawner CreateDefaultSpawner(TileIndex tileIndex, string mapGuid, string spawnerGuid)
	{
		SpawnRoundEntry item = new SpawnRoundEntry(new List<string> { "DON'T SPAWN", "DON'T SPAWN", "DON'T SPAWN", "DON'T SPAWN" });
		List<SpawnRoundEntry> value = new List<SpawnRoundEntry> { item };
		Dictionary<string, List<SpawnRoundEntry>> dictionary = new Dictionary<string, List<SpawnRoundEntry>>();
		dictionary.Add("Default", value);
		List<int> spawnRoundInterval = new List<int> { 1, 1, 1, 1 };
		return new CSpawner(new SpawnerData(SpawnerData.ESpawnerTriggerType.StartRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 2, loopSpawnPattern: true, shouldCountTowardsKillAllEnemies: false, spawnRoundInterval, dictionary, ""), tileIndex, mapGuid, spawnerGuid);
	}

	public static CInteractableSpawner CreateDefaultInteractableSpawner(string spawnerPropType, TileIndex tileIndex, string mapGuid, string spawnerGuid)
	{
		SpawnRoundEntry item = new SpawnRoundEntry(new List<string> { "DON'T SPAWN", "DON'T SPAWN", "DON'T SPAWN", "DON'T SPAWN" });
		List<SpawnRoundEntry> value = new List<SpawnRoundEntry> { item };
		Dictionary<string, List<SpawnRoundEntry>> dictionary = new Dictionary<string, List<SpawnRoundEntry>>();
		dictionary.Add("Default", value);
		List<int> spawnRoundInterval = new List<int> { 1, 1, 1, 1 };
		SpawnerData spawnerData = new SpawnerData(SpawnerData.ESpawnerTriggerType.StartRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 2, loopSpawnPattern: true, shouldCountTowardsKillAllEnemies: false, spawnRoundInterval, dictionary, "");
		return new CInteractableSpawner(spawnerPropType, spawnerData, tileIndex, mapGuid, spawnerGuid);
	}
}

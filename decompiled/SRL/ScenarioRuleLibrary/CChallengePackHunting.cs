using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CChallengePackHunting : CChallenge
{
	public override uint ID => 1523u;

	public override string Name => "pack_hunting";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnStartScenario;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		List<CTile> list = ScenarioManager.StartingTiles.ToList();
		for (int i = 0; i < ScenarioScript.PartySize; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			CTile cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
			list.Remove(cTile);
			CMap hexMap = cTile.m_HexMap;
			List<string> spawnClass = new List<string> { "HoundID", "HoundID", "HoundID", "HoundID" };
			Dictionary<string, List<SpawnRoundEntry>> dictionary = new Dictionary<string, List<SpawnRoundEntry>>();
			dictionary["Default"] = new List<SpawnRoundEntry>
			{
				new SpawnRoundEntry(spawnClass)
			};
			CSpawner cSpawner = new CSpawner(new SpawnerData(SpawnerData.ESpawnerTriggerType.StartRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 3, loopSpawnPattern: false, shouldCountTowardsKillAllEnemies: false, new List<int> { 1, 1, 1, 1 }, dictionary, null), new TileIndex(cTile.m_ArrayIndex), hexMap.MapGuid);
			cTile.SpawnSpawner(cSpawner);
			cSpawner.SetActive(active: true);
		}
	}
}

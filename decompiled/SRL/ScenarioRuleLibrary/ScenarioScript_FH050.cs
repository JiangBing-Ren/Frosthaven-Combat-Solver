using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH050 : ScenarioScript
{
	protected override bool Initialize()
	{
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			if (spawner.SpawnerData.LoopSpawnPattern)
			{
				Error("loops: " + spawner.SpawnerGuid, null, 13);
				spawner.SpawnerData.LoopSpawnPattern = false;
			}
		}
		PutSnorkelsOn();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UnderwaterStart();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			PutSnorkelsOn(_tickActor);
			break;
		}
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		Trace("Spawner spawned: ", null, 33);
		foreach (SpawnRoundEntry spawnRoundEntry3 in spawner.SpawnRoundEntries)
		{
			string text = "";
			for (int i = 0; i < Math.Min(4, spawnRoundEntry3.SpawnClass.Count); i++)
			{
				string text2 = spawnRoundEntry3.SpawnClass[i];
				text = text + text2 + ", ";
			}
			Trace(text, null, 40);
		}
		Trace("---", null, 42);
		spawner.SetActive(active: false, forceDontSpawn: true);
		List<SpawnRoundEntry> spawnRoundEntries = spawner.SpawnRoundEntries;
		for (int j = 1; j < spawnRoundEntries.Count; j++)
		{
			SpawnRoundEntry spawnRoundEntry2 = spawnRoundEntries[j];
			TrySpawnActorAtNearestTile(spawner.ArrayIndex.X, spawner.ArrayIndex.Y, spawnRoundEntry2.SpawnClass[ScenarioScript.PartySize - 1], out var _);
		}
	}

	public override bool IsUnderwater(CTile tile = null)
	{
		return true;
	}
}

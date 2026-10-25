using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH036 : ScenarioScript
{
	[Serializable]
	public class Storage36
	{
		public int lastSpawnerIndicatorsDoors;

		public int lastHandleSpawnersRound;
	}

	private static int SPAWNING_START_ROUND = 2;

	private CSpawner[][] _spawners;

	private Point[][] _spawnersPos;

	private CObjectProp[] _doors;

	private Storage36 serializedProperties;

	private int lastSpawnerIndicatorsDoors
	{
		get
		{
			return serializedProperties.lastSpawnerIndicatorsDoors;
		}
		set
		{
			serializedProperties.lastSpawnerIndicatorsDoors = value;
		}
	}

	private int lastHandleSpawnersRound
	{
		get
		{
			return serializedProperties.lastHandleSpawnersRound;
		}
		set
		{
			serializedProperties.lastHandleSpawnersRound = value;
		}
	}

	private int NextRoundNumber => ScenarioManager.CurrentScenarioState.RoundNumber + 1;

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage36();
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_doors = new CObjectProp[4]
		{
			GetProp(2, 8, "MaintenanceShaft"),
			GetProp(8, 8, "MaintenanceShaft"),
			GetProp(2, 0, "MaintenanceShaft"),
			GetProp(8, 0, "MaintenanceShaft")
		};
		_spawners = new CSpawner[3][]
		{
			new CSpawner[3]
			{
				spawners[0],
				spawners[1],
				spawners[2]
			},
			new CSpawner[3]
			{
				spawners[3],
				spawners[4],
				spawners[5]
			},
			new CSpawner[3]
			{
				spawners[6],
				spawners[7],
				spawners[8]
			}
		};
		_spawnersPos = new Point[4][]
		{
			new Point[3]
			{
				new Point(1, 7),
				new Point(2, 7),
				new Point(2, 6)
			},
			new Point[3]
			{
				new Point(8, 7),
				new Point(7, 7),
				new Point(8, 6)
			},
			new Point[3]
			{
				new Point(1, 1),
				new Point(2, 1),
				new Point(2, 2)
			},
			new Point[3]
			{
				new Point(8, 1),
				new Point(7, 1),
				new Point(8, 2)
			}
		};
		lastHandleSpawnersRound = 0;
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		foreach (CObjectProp item in _doors.Where((CObjectProp x) => x.RuntimeAttachedActor != null && x.RuntimeAttachedActor.IsDead))
		{
			CNode node = ScenarioManager.PathFinder.GetNode(item.RuntimeAttachedActor.ArrayIndex);
			node.Walkable = false;
			node.Blocked = true;
		}
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (Storage36)JsonConvert.DeserializeObject(storage, typeof(Storage36));
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 102);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseStartRound()
	{
		if (ScenarioScript.RoundNumber == lastHandleSpawnersRound)
		{
			return;
		}
		HideAllSpawnersIndicators();
		if (NextRoundNumber >= SPAWNING_START_ROUND)
		{
			HandleSpawnersForRound(NextRoundNumber, (CSpawner s, CObjectProp d) =>
			{
				s.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
				lastSpawnerIndicatorsDoors = Array.IndexOf(_doors, d);
			});
		}
		if (ScenarioScript.RoundNumber >= SPAWNING_START_ROUND)
		{
			HandleSpawnersForRound(ScenarioScript.RoundNumber, (CSpawner s, CObjectProp _) =>
			{
				s.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
			});
		}
		lastHandleSpawnersRound = ScenarioScript.RoundNumber;
	}

	protected override void TickAbilityEndedInternal()
	{
		if (lastHandleSpawnersRound != 0 && _doors[lastSpawnerIndicatorsDoors].PropHealthDetails.CurrentHealth <= 0)
		{
			HideAllSpawnersIndicators();
		}
	}

	private void HandleSpawnersForRound(int roundNumber, Action<CSpawner, CObjectProp> spawnerAction)
	{
		int num = (roundNumber - SPAWNING_START_ROUND) % _doors.Length;
		CObjectProp cObjectProp = _doors[num];
		if (cObjectProp.PropHealthDetails.CurrentHealth > 0)
		{
			int num2 = (roundNumber - SPAWNING_START_ROUND) % _spawners.Length;
			CSpawner[] array = _spawners[num2];
			Point[] array2 = _spawnersPos[num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetLocation(new TileIndex(array2[i]));
				spawnerAction(array[i], cObjectProp);
			}
		}
	}

	private void HideAllSpawnersIndicators()
	{
		CSpawner[][] spawners = _spawners;
		foreach (CSpawner[] array in spawners)
		{
			for (int j = 0; j < array.Length; j++)
			{
				array[j].HideSpawnerProp();
			}
		}
	}

	protected override void TickDeathInternal()
	{
		CObjectProp cObjectProp = _doors.SingleOrDefault((CObjectProp x) => x.RuntimeAttachedActor == _tickActor);
		if (cObjectProp != null)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(cObjectProp.RuntimeAttachedActor.ArrayIndex);
			node.Walkable = false;
			node.Blocked = true;
		}
	}
}

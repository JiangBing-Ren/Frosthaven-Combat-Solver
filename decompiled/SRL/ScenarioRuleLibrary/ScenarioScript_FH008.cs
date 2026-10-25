using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH008 : ScenarioScript
{
	private CSpawner _spawner;

	private CObjectDoor _door1;

	private CScenarioModifier _houndSpawnModifier;

	protected override bool Initialize()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_spawner = spawners[0];
		_door1 = GetProp(10, 2, "ThinDoor") as CObjectDoor;
		_houndSpawnModifier = GetScenarioModifier("HoundSpawnModifier");
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (_spawner.SpawnerData.SpawnerTriggerType != SpawnerData.ESpawnerTriggerType.StartRound)
		{
			_spawner.SpawnerData.SpawnerTriggerType = SpawnerData.ESpawnerTriggerType.StartRound;
			_spawner.SpawnerData.SpawnerActivationType = SpawnerData.ESpawnerActivationType.ActivatedExternally;
			UpdateSpawner(updatePosition: false);
		}
	}

	protected override void TickRepositionInternal()
	{
		if (GetFirstDoor(out var firstDoor) && firstDoor.DoorIsOpen)
		{
			CScenarioModifier houndSpawnModifier = _houndSpawnModifier;
			if (houndSpawnModifier != null && houndSpawnModifier.IsHidden)
			{
				houndSpawnModifier.SetHiddenState(shouldSetHidden: false);
				Trace("Reveal the last special rule.", null, 43);
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 63);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		base.OnSpawnerHasSpawned(spawner, actor);
		UpdateSpawner();
	}

	protected override bool AllowEscapeZoneInternal(CActor actor)
	{
		int num = 0;
		foreach (EnemyState monster in ScenarioManager.CurrentScenarioState.Monsters)
		{
			if (monster.ClassID.Contains("PolarBear") && IsAlive(monster))
			{
				num++;
			}
		}
		return num == 0;
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	private bool GetFirstDoor(out CObjectDoor firstDoor)
	{
		firstDoor = null;
		string roomName = ScenarioManager.StartingTiles[0].m_HexMap.RoomName;
		foreach (CObjectDoor item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>().ToList())
		{
			if (item.PropTile.m_HexMap.RoomName == roomName || item.PropTile.m_Hex2Map.RoomName == roomName)
			{
				firstDoor = item;
				Trace("Found Door 1", null, 102);
				return true;
			}
		}
		if (firstDoor == null)
		{
			Error("Can't find Door 1", null, 108);
			return false;
		}
		return true;
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (!_spawner.IsActive)
		{
			UpdateSpawner();
		}
	}

	private void UpdateSpawner(bool updatePosition = true)
	{
		if (_door1.DoorIsClosed)
		{
			_spawner.SetActive(active: false);
			return;
		}
		if (updatePosition)
		{
			CTile cTile = null;
			List<CTile> startingTiles = ScenarioManager.StartingTiles;
			int num = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(startingTiles.Count);
			for (int i = 0; i < startingTiles.Count; i++)
			{
				CTile cTile2 = startingTiles[(i + num) % startingTiles.Count];
				if (ScenarioManager.Scenario.FindActorsAt(cTile2.m_ArrayIndex).Count == 0)
				{
					cTile = cTile2;
					break;
				}
			}
			if (cTile == null)
			{
				Error("Can't find free starting tile to spawn Hound on.", null, 141);
				return;
			}
			_spawner.SetLocation(cTile.m_Hex.ArrayIndex);
		}
		_spawner.SetActive(active: true, forceDontSpawn: true);
		_spawner.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
	}
}

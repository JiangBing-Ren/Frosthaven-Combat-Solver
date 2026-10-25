using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH025 : ScenarioScript
{
	private CObjectProp _debrisB;

	private CObjectProp _debrisD;

	private CSpawner _spawnerA1;

	private CSpawner _spawnerC1;

	private CSpawner _spawnerC2;

	private CObjectDoor _door1;

	private CObjectDoor _door2;

	private CScenarioModifier _specialRule1;

	private CScenarioModifier _specialRule2;

	private CScenarioModifier _specialRule3;

	private CScenarioModifier _specialRule4;

	public static readonly Tuple<int, int>[] Room1WallTiles = new Tuple<int, int>[4]
	{
		new Tuple<int, int>(3, 3),
		new Tuple<int, int>(4, 2),
		new Tuple<int, int>(4, 3),
		new Tuple<int, int>(5, 3)
	};

	public static readonly Tuple<int, int>[] Room2WallTiles = new Tuple<int, int>[4]
	{
		new Tuple<int, int>(15, 11),
		new Tuple<int, int>(16, 10),
		new Tuple<int, int>(15, 10),
		new Tuple<int, int>(14, 9)
	};

	public static readonly Tuple<int, int, string> Room1RuinedVFX = new Tuple<int, int, string>(6, 2, "DustCloudS25Room1");

	public static readonly Tuple<int, int, string> Room2RuinedVFX = new Tuple<int, int, string>(15, 9, "DustCloudS25Room2");

	public static readonly Tuple<int, int>[] Room1RuinedTiles = new Tuple<int, int>[13]
	{
		new Tuple<int, int>(4, 1),
		new Tuple<int, int>(5, 2),
		new Tuple<int, int>(5, 1),
		new Tuple<int, int>(6, 2),
		new Tuple<int, int>(6, 3),
		new Tuple<int, int>(6, 1),
		new Tuple<int, int>(7, 2),
		new Tuple<int, int>(7, 3),
		new Tuple<int, int>(7, 1),
		new Tuple<int, int>(8, 2),
		new Tuple<int, int>(8, 3),
		new Tuple<int, int>(8, 1),
		new Tuple<int, int>(9, 2)
	};

	public static readonly Tuple<int, int>[] Room2RuinedTiles = new Tuple<int, int>[9]
	{
		new Tuple<int, int>(15, 9),
		new Tuple<int, int>(16, 9),
		new Tuple<int, int>(15, 8),
		new Tuple<int, int>(15, 7),
		new Tuple<int, int>(16, 8),
		new Tuple<int, int>(17, 8),
		new Tuple<int, int>(17, 7),
		new Tuple<int, int>(16, 7),
		new Tuple<int, int>(16, 6)
	};

	private bool _room1Collapsed;

	private bool _room2Collapsed;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private bool IsDestroyed(CObjectProp prop)
	{
		return prop?.IsDestroyed ?? false;
	}

	protected override bool Initialize()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_debrisB = GetProp(3, 3, "TwoHexObstacle", checkDestroyed: true);
		_debrisD = GetProp(15, 11, "TwoHexObstacle", checkDestroyed: true);
		_room1Collapsed = IsDestroyed(_debrisB);
		_room2Collapsed = IsDestroyed(_debrisD);
		_spawnerA1 = spawners[0];
		_spawnerC1 = spawners[1];
		_spawnerC2 = spawners[2];
		SetPatchingBlockers(_debrisB as CObjectObstacle);
		SetPatchingBlockers(_debrisD as CObjectObstacle);
		_specialRule1 = GetScenarioModifier("specialRule1");
		_specialRule2 = GetScenarioModifier("specialRule2");
		_specialRule3 = GetScenarioModifier("specialRule3");
		_specialRule4 = GetScenarioModifier("specialRule4");
		string roomName = ScenarioManager.StartingTiles[0].m_HexMap.RoomName;
		string roomName2 = ScenarioManager.Tiles[_spawnerA1.ArrayIndex.X, _spawnerA1.ArrayIndex.Y].m_HexMap.RoomName;
		string roomName3 = ScenarioManager.Tiles[_spawnerC1.ArrayIndex.X, _spawnerC1.ArrayIndex.Y].m_HexMap.RoomName;
		foreach (CObjectDoor item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>().ToList())
		{
			if (item.PropTile.m_HexMap.RoomName == roomName && item.PropTile.m_Hex2Map.RoomName == roomName2)
			{
				Trace("Found the door 1.", null, 62);
				_door1 = item;
			}
			else if (item.PropTile.m_HexMap.RoomName == roomName && item.PropTile.m_Hex2Map.RoomName == roomName3)
			{
				Trace("Found the door 2.", null, 67);
				_door2 = item;
			}
		}
		if (_door1 == null)
		{
			Error("Place a door between start room and room 1!", null, 73);
			return false;
		}
		if (_door2 == null)
		{
			Error("Place a door between start room and room 2!", null, 78);
			return false;
		}
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateSpawners();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartScenario && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartTurn && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndTurn && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndRound)
		{
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 104);
		}
	}

	protected override void TickRepositionInternal()
	{
		UpdateScenarioModifiersVisibility();
		UpdateSpawners();
	}

	protected override void TickAbilityEndedInternal()
	{
		bool flag = false;
		if (_door1.DoorIsOpen && !_room1Collapsed && _debrisB.PropHealthDetails.CurrentHealth <= 0)
		{
			CollapseRoom(Room1WallTiles, Room1RuinedTiles, Room1RuinedVFX);
			_room1Collapsed = true;
			flag = true;
		}
		if (_door2.DoorIsOpen && !_room2Collapsed && _debrisD.PropHealthDetails.CurrentHealth <= 0)
		{
			CollapseRoom(Room2WallTiles, Room2RuinedTiles, Room2RuinedVFX);
			_room2Collapsed = true;
			flag = true;
		}
		if (flag)
		{
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
			{
				duration = 4f,
				magnitude = 0.1f,
				speed = 50f,
				distanceForce = 50f
			});
		}
		UpdateScenarioModifiersVisibility();
		UpdateSpawners();
	}

	private void UpdateSpawners()
	{
		UpdateSpawnersActive(_debrisB, _door1, _specialRule3, _spawnerA1);
		UpdateSpawnersActive(_debrisD, _door2, _specialRule4, _spawnerC1, _spawnerC2);
	}

	private void SpawnProp(CTile tile)
	{
		TileIndex tileIndex = new TileIndex(tile.m_ArrayIndex);
		List<TileIndex> list = new List<TileIndex>();
		list.Add(tileIndex);
		CObjectObstacle prop = new CObjectObstacle("RockSingle", ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, null, list, null, tile.m_HexMap.MapGuid, ignoresFlyAndJump: true, setPositionToCenterOfAllPathBlockers: false, blockLOS: true);
		tile.SpawnProp(prop, notifyClient: true, 2.5f);
	}

	private void UpdateSpawnersActive(CObjectProp debris, CObjectDoor door, CScenarioModifier roomModifier, CSpawner spawner1, CSpawner spawner2 = null)
	{
		if (door.DoorIsOpen)
		{
			roomModifier.SetHiddenState(IsDestroyed(debris));
			if (IsDestroyed(debris) && spawner1.IsActive)
			{
				spawner1.SetActive(active: false);
				spawner2?.SetActive(active: false);
			}
		}
	}

	private void UpdateScenarioModifiersVisibility()
	{
		if (_door1.DoorIsOpen || _door2.DoorIsOpen)
		{
			_specialRule1.Show();
			_specialRule2.Show();
		}
		_specialRule3.SetHiddenState(!_door1.DoorIsOpen || IsDestroyed(_debrisB));
		_specialRule4.SetHiddenState(!_door2.DoorIsOpen || IsDestroyed(_debrisD));
	}

	private void CollapseRoom(Tuple<int, int>[] WallTiles, Tuple<int, int>[] RuinedTiles, Tuple<int, int, string> VFX)
	{
		List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		list.AddRange(WallTiles);
		list.AddRange(RuinedTiles);
		bool flag = false;
		foreach (Tuple<int, int> item in list)
		{
			CTile tile = ScenarioManager.Tiles[item.Item1, item.Item2];
			List<CObjectProp> list2 = ScenarioManager.CurrentScenarioState.Props.FindAll((CObjectProp p) => p.ArrayIndex.X == tile.m_ArrayIndex.X && p.ArrayIndex.Y == tile.m_ArrayIndex.Y);
			foreach (CActor item2 in ScenarioManager.Scenario.AllActors.FindAll((CActor a) => a.ArrayIndex.X == tile.m_ArrayIndex.X && a.ArrayIndex.Y == tile.m_ArrayIndex.Y))
			{
				item2.NoGoldDrop = true;
				KillActor(item2);
			}
			foreach (CObjectProp item3 in list2)
			{
				item3.DestroyProp();
			}
			if (!flag)
			{
				flag = true;
				PlayRoomCollapseVFX(VFX);
			}
			SpawnProp(tile);
		}
	}

	private void PlayRoomCollapseVFX(Tuple<int, int, string> VFX)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = VFX.Item3,
			m_ArrayIndex = new Point(VFX.Item1, VFX.Item2)
		});
	}

	public void KillActor(CActor actor)
	{
		actor.Health = 0;
		GameState.KillActor(actor, actor, CActor.ECauseOfDeath.Damage, out var _);
		if (actor is CPlayerActor cPlayerActor)
		{
			cPlayerActor.CharacterClass.CheckForFinishedActiveBonuses(actor);
		}
		else if (actor is CHeroSummonActor cHeroSummonActor)
		{
			cHeroSummonActor.Summoner.CharacterClass.CheckForFinishedActiveBonuses(cHeroSummonActor.Summoner);
		}
	}

	private void SetPatchingBlockers(CObjectObstacle obstacle)
	{
		if (obstacle == null || obstacle.PathingBlockers == null)
		{
			return;
		}
		foreach (TileIndex pathingBlocker in obstacle.PathingBlockers)
		{
			CTile tile = ScenarioManager.GetTile(pathingBlocker);
			if (tile != null)
			{
				ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Blocked = true;
			}
		}
	}
}

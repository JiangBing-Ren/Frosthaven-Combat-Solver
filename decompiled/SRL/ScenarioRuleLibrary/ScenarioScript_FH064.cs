using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH064 : ScenarioScript
{
	private class SideRoomInfo
	{
		public string Name;

		public string CaptainClassID;

		public List<Point> MilitiaSpawnPositions;

		public List<Point> EnemySpawnPositions;

		public Point NearestPointInCentralRoom;

		public List<SideRoomInfo> AdjacentRooms = new List<SideRoomInfo>();

		public CMap Map;

		public CActor Captain;

		public List<CSpawner> MilitiaSpawners = new List<CSpawner>();

		public List<CSpawner>[] EnemySpawners = new List<CSpawner>[11];

		public bool IsDestroyed
		{
			get
			{
				if (Map != null)
				{
					return Map.Destroyed;
				}
				return true;
			}
		}

		public bool Contains(Point point)
		{
			return ScenarioManager.GetTile(point).m_HexMap == Map;
		}

		public override string ToString()
		{
			return Name ?? "";
		}
	}

	[ConsoleVariable(Alias = "ss64_defend_rounds", Persistent = false)]
	public static int DefendRounds = 10;

	public const string BossClassID = "VestigeOfTheImprisonedGodID";

	private readonly Point TreePosition = new Point(9, 11);

	private readonly List<Point> A_points = new List<Point>
	{
		new Point(3, 11),
		new Point(6, 16)
	};

	private readonly List<Point> B_points = new List<Point>
	{
		new Point(10, 17),
		new Point(15, 14)
	};

	private readonly List<Point> C_points = new List<Point>
	{
		new Point(15, 10),
		new Point(12, 5)
	};

	private readonly List<Point> D_points = new List<Point>
	{
		new Point(9, 4),
		new Point(4, 7)
	};

	private readonly Point E_point = new Point(8, 14);

	private readonly Point F_point = new Point(11, 14);

	private readonly Point G_point = new Point(11, 8);

	private readonly Point H_point = new Point(8, 8);

	private readonly List<Point> ESP_A = new List<Point>
	{
		new Point(1, 11),
		new Point(2, 12),
		new Point(2, 13),
		new Point(3, 14),
		new Point(3, 15),
		new Point(4, 16),
		new Point(4, 17),
		new Point(5, 18)
	};

	private readonly List<Point> ESP_B = new List<Point>
	{
		new Point(9, 19),
		new Point(10, 19),
		new Point(11, 19),
		new Point(12, 19),
		new Point(13, 19),
		new Point(15, 18),
		new Point(15, 17),
		new Point(16, 16),
		new Point(16, 15),
		new Point(17, 14)
	};

	private readonly List<Point> ESP_C = new List<Point>
	{
		new Point(17, 9),
		new Point(17, 8),
		new Point(16, 7),
		new Point(16, 6),
		new Point(15, 5),
		new Point(15, 4),
		new Point(14, 3)
	};

	private readonly List<Point> ESP_D = new List<Point>
	{
		new Point(7, 0),
		new Point(6, 1),
		new Point(5, 1),
		new Point(5, 2),
		new Point(4, 2),
		new Point(3, 3),
		new Point(2, 3)
	};

	public bool SecondPhase;

	private List<SideRoomInfo> SideRooms = new List<SideRoomInfo>();

	private SideRoomInfo RoomA;

	private SideRoomInfo RoomB;

	private SideRoomInfo RoomC;

	private SideRoomInfo RoomD;

	private CSpawner[] GSpawners = new CSpawner[2];

	private CActor tree;

	private CActor boss;

	private List<Point> bossMultihexArrayIndexes = new List<Point>
	{
		new Point(9, 11),
		new Point(9, 12),
		new Point(10, 12)
	};

	private CObjective_Scripted DefendTreeObjective;

	private CObjective KillBossObjective;

	[StateDump(Skip = true)]
	private int lastStartedRound;

	[StateDump(Skip = true)]
	private int lastEndedRound;

	protected override bool Initialize()
	{
		CSpawner.ConvertMultiEntrySpawnersToSeparateSpawners();
		tree = ScenarioManager.Scenario.FindObjectActorAt(TreePosition);
		boss = ScenarioManager.Scenario.FindEnemyAt(TreePosition);
		if (boss != null)
		{
			boss.MultiHexArrayIndexes = bossMultihexArrayIndexes;
			SecondPhase = true;
		}
		InitSideRooms();
		InitGSpawners();
		DefendTreeObjective = GetObjective("DefendTree") as CObjective_Scripted;
		DefendTreeObjective.ScriptedIsComplete = false;
		KillBossObjective = GetObjective("KillBoss");
		UpdateObjectives();
		return true;
	}

	protected override string SerializeInternal()
	{
		return base.SerializeInternal();
	}

	protected override void DeserializeInternal(string s)
	{
		UpdateTeleportTileHighlights();
		CObjectProp cObjectProp = ScenarioScript.FindPropWithHealth(TreePosition.X, TreePosition.Y);
		if (boss != null || tree.IsDead)
		{
			if (cObjectProp.RuntimeAttachedActor != null)
			{
				cObjectProp.RuntimeAttachedActor.ClearAttachedProp();
			}
			cObjectProp.ClearAttachedRuntimeActor();
		}
	}

	private void InitSideRooms()
	{
		RoomA = new SideRoomInfo
		{
			Name = "A (07-E)",
			CaptainClassID = "MoonshardS64ID",
			MilitiaSpawnPositions = A_points,
			EnemySpawnPositions = ESP_A,
			NearestPointInCentralRoom = E_point
		};
		SideRooms.Add(RoomA);
		RoomB = new SideRoomInfo
		{
			Name = "B (11-E)",
			CaptainClassID = "CaptainOlmsteadS64ID",
			MilitiaSpawnPositions = B_points,
			EnemySpawnPositions = ESP_B,
			NearestPointInCentralRoom = F_point
		};
		SideRooms.Add(RoomB);
		RoomC = new SideRoomInfo
		{
			Name = "C (2-A and 2-C)",
			CaptainClassID = "SathaS64ID",
			MilitiaSpawnPositions = C_points,
			EnemySpawnPositions = ESP_C,
			NearestPointInCentralRoom = G_point
		};
		SideRooms.Add(RoomC);
		RoomD = new SideRoomInfo
		{
			Name = "D (13-E)",
			CaptainClassID = "CrainS64ID",
			MilitiaSpawnPositions = D_points,
			EnemySpawnPositions = ESP_D,
			NearestPointInCentralRoom = H_point
		};
		SideRooms.Add(RoomD);
		RoomA.AdjacentRooms.Add(RoomB);
		RoomA.AdjacentRooms.Add(RoomD);
		RoomB.AdjacentRooms.Add(RoomA);
		RoomB.AdjacentRooms.Add(RoomC);
		RoomC.AdjacentRooms.Add(RoomB);
		RoomC.AdjacentRooms.Add(RoomD);
		RoomD.AdjacentRooms.Add(RoomA);
		RoomD.AdjacentRooms.Add(RoomC);
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			InitRoom(sideRoom);
		}
	}

	private void InitRoom(SideRoomInfo room)
	{
		room.Map = ScenarioManager.GetTile(room.MilitiaSpawnPositions[0]).m_HexMap;
		room.Captain = ScenarioManager.Scenario.AllAllyMonsters.FirstOrDefault((CEnemyActor x) => x.MonsterClass.ID == room.CaptainClassID);
		if (room.Captain == null)
		{
			Error($"No Captain {room.CaptainClassID} found for room {room}", null, 221);
		}
		if (room.Captain != null && !room.Captain.IsDead && room.Captain.Level > 5)
		{
			(room.Captain.Class as CMonsterClass)?.SetMonsterStatLevel(5);
		}
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			if (spawner.StartingMapGuid != room.Map.MapGuid)
			{
				continue;
			}
			Point point = new Point(spawner.ArrayIndex);
			if (spawner.ActorTypeToSpawn == CActor.EType.Ally)
			{
				if (!room.MilitiaSpawnPositions.Contains(point))
				{
					Error($"Militia Spawner at invalid position {point} in room {room}", null, 236);
				}
				room.MilitiaSpawners.Add(spawner);
			}
			else
			{
				if (spawner.ActorTypeToSpawn != CActor.EType.Enemy)
				{
					continue;
				}
				if (point != room.NearestPointInCentralRoom && !room.EnemySpawnPositions.Contains(point))
				{
					Error($"Enemy Spawner at invalid position {point} in room {room}", null, 242);
				}
				int spawnStartRound = spawner.SpawnerData.SpawnStartRound;
				if (spawnStartRound < 1 || spawnStartRound > 10)
				{
					Error($"Enemy Spawner for invalid round {spawnStartRound} in room {room}", null, 246);
					continue;
				}
				List<CSpawner> list = room.EnemySpawners[spawnStartRound];
				if (list == null)
				{
					list = (room.EnemySpawners[spawnStartRound] = new List<CSpawner>());
				}
				list.Add(spawner);
			}
		}
	}

	private void InitGSpawners()
	{
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			if (!(new Point(spawner.ArrayIndex) != G_point) && spawner.CurrentMap == spawner.StartingMap)
			{
				int spawnStartRound = spawner.SpawnerData.SpawnStartRound;
				int num;
				switch (spawnStartRound)
				{
				case 1:
					num = 0;
					break;
				case 2:
					num = 1;
					break;
				default:
					Error($"Spawner at point G with start round {spawnStartRound} (must be 1 or 2)", null, 274);
					continue;
				}
				if (GSpawners[num] != null)
				{
					Error($"Multiple spawners at point G for round {spawnStartRound}", null, 278);
				}
				GSpawners[num] = spawner;
			}
		}
		for (int i = 0; i < 2; i++)
		{
			if (GSpawners[i] == null)
			{
				Error($"Missing spawner {i} at point G", null, 284);
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 305);
			break;
		}
	}

	private void TickStartTurn()
	{
	}

	private void TickEndTurn()
	{
	}

	private void TickStartRound()
	{
		if (lastStartedRound == ScenarioManager.CurrentScenarioState.RoundNumber)
		{
			return;
		}
		lastStartedRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		if (lastStartedRound == 1)
		{
			SpawnMilitia(ScenarioManager.CurrentScenarioState.Players.Count * 2);
		}
		else
		{
			if (lastEndedRound == DefendRounds)
			{
				EnterSecondPhase();
			}
			if (!SecondPhase)
			{
				DestroyRoomsWithDeadCaptains();
				CheckForPlayerTeleports();
				SpawnMilitia(2);
			}
		}
		if (!SecondPhase)
		{
			RandomizeEnemySpawnPositions();
			UpdateObjectives();
		}
		else
		{
			SpawnAtG();
			UpdateGSpawnerIndicators();
		}
	}

	private void TickEndRound()
	{
		if (lastEndedRound != ScenarioManager.CurrentScenarioState.RoundNumber)
		{
			lastEndedRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		}
	}

	protected override void TickDeathInternal()
	{
		if (!SecondPhase && _tickActor == tree)
		{
			DefendTreeObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	public override void ValidateAbilityTargeting(CAbility ability)
	{
		if (!(ability is CAbilityTeleport2 { ActorToTeleport: not null } cAbilityTeleport))
		{
			return;
		}
		CTile tile = ScenarioManager.GetTile(cAbilityTeleport.ActorToTeleport.ArrayIndex);
		for (int num = ability.TilesInRange.Count - 1; num >= 0; num--)
		{
			if (ability.TilesInRange[num].m_HexMap != tile.m_HexMap)
			{
				ability.TilesInRange.RemoveAt(num);
			}
		}
	}

	private void RandomizeEnemySpawnPositions()
	{
		int num = ScenarioManager.CurrentScenarioState.RoundNumber + 1;
		if (num > DefendRounds)
		{
			return;
		}
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			if (sideRoom.IsDestroyed)
			{
				continue;
			}
			List<CSpawner> list = sideRoom.EnemySpawners[num];
			if (list == null)
			{
				continue;
			}
			List<Point> list2 = new List<Point>();
			foreach (Point enemySpawnPosition in sideRoom.EnemySpawnPositions)
			{
				if (IsFreeTile(enemySpawnPosition.X, enemySpawnPosition.Y))
				{
					list2.Add(enemySpawnPosition);
				}
			}
			foreach (CSpawner item in list)
			{
				if (list2.Count == 0)
				{
					list2.AddRange(sideRoom.EnemySpawnPositions);
				}
				int index = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list2.Count);
				Point point = list2[index];
				item.SetLocation(new TileIndex(point));
				list2.Remove(point);
			}
		}
	}

	private void SpawnAtG()
	{
		CSpawner spawner = GSpawners[ScenarioManager.CurrentScenarioState.RoundNumber % 2];
		ForceSpawn(spawner);
	}

	private void UpdateGSpawnerIndicators()
	{
		for (int i = 0; i < 2; i++)
		{
			CSpawner cSpawner = GSpawners[i];
			if (SecondPhase && ScenarioManager.CurrentScenarioState.RoundNumber % 2 != i)
			{
				cSpawner.CreateSpawnerProp(forceKeepItVisible: true);
			}
			else
			{
				cSpawner.HideSpawnerProp(forced: true);
			}
		}
	}

	private void CheckForPlayerTeleports()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			List<CTile> possibleTeleportTargets = GetPossibleTeleportTargets(playerActor.ArrayIndex);
			if (possibleTeleportTargets != null && possibleTeleportTargets.Count != 0)
			{
				ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_064_Select_Hex_1$", playerActor, null, TeleportPlayerCallback, 1, animateActor: false, (CAbilityNullHex a) => GetPossibleTeleportTargets(playerActor.ArrayIndex));
			}
		}
	}

	private List<CTile> PointsToEmptyTiles(List<Point> points)
	{
		List<CTile> list = new List<CTile>();
		foreach (Point point in points)
		{
			if (IsFreeTile(point.X, point.Y))
			{
				list.Add(ScenarioManager.GetTile(point));
			}
		}
		return list;
	}

	private List<CTile> GetPossibleTeleportTargets(Point fromPoint)
	{
		if (fromPoint == E_point)
		{
			if (RoomA.IsDestroyed)
			{
				return null;
			}
			return PointsToEmptyTiles(RoomA.MilitiaSpawnPositions);
		}
		if (fromPoint == F_point)
		{
			if (RoomB.IsDestroyed)
			{
				return null;
			}
			return PointsToEmptyTiles(RoomB.MilitiaSpawnPositions);
		}
		if (fromPoint == G_point)
		{
			if (RoomC.IsDestroyed)
			{
				return null;
			}
			return PointsToEmptyTiles(RoomC.MilitiaSpawnPositions);
		}
		if (fromPoint == H_point)
		{
			if (RoomD.IsDestroyed)
			{
				return null;
			}
			return PointsToEmptyTiles(RoomD.MilitiaSpawnPositions);
		}
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			if (sideRoom.IsDestroyed)
			{
				continue;
			}
			foreach (Point militiaSpawnPosition in sideRoom.MilitiaSpawnPositions)
			{
				if (militiaSpawnPosition != fromPoint)
				{
					continue;
				}
				List<CTile> list = new List<CTile>();
				if (IsFreeTile(sideRoom.NearestPointInCentralRoom.X, sideRoom.NearestPointInCentralRoom.Y))
				{
					list.Add(ScenarioManager.GetTile(sideRoom.NearestPointInCentralRoom));
				}
				foreach (SideRoomInfo adjacentRoom in sideRoom.AdjacentRooms)
				{
					if (!adjacentRoom.IsDestroyed)
					{
						List<CTile> collection = PointsToEmptyTiles(adjacentRoom.MilitiaSpawnPositions);
						list.AddRange(collection);
					}
				}
				return list;
			}
		}
		return null;
	}

	private void TeleportPlayerCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles.Count != 0)
		{
			CTile t = selectedTiles[0];
			TeleportActor(ability.TargetingActor, t);
		}
	}

	private void SpawnMilitia(int count)
	{
		int num = ScenarioManager.Scenario.AllyMonsters.Count((CEnemyActor a) => a.MonsterClass.ID == "MilitiaID");
		if (num + count > 12)
		{
			count = 12 - num;
		}
		if (count > 0)
		{
			ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_064_Select_Hex_2$", null, null, SpawnMilitiaCallback, count, animateActor: false, (CAbilityNullHex a) => CalcValidMilitiaSpawnTiles(), canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: false, ignoreDoorLocks: false, stackAsSeparateAbilities: false, autoTrigger: false, new List<(string, TValue)> { ("SpawnActorPreviewID", new TValue("MilitiaID")) });
		}
	}

	private List<CTile> CalcValidMilitiaSpawnTiles()
	{
		List<CTile> list = new List<CTile>();
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			if (sideRoom.IsDestroyed)
			{
				continue;
			}
			foreach (Point militiaSpawnPosition in sideRoom.MilitiaSpawnPositions)
			{
				if (IsFreeTile(militiaSpawnPosition.X, militiaSpawnPosition.Y))
				{
					list.Add(ScenarioManager.GetTile(militiaSpawnPosition));
				}
			}
		}
		return list;
	}

	private void SpawnMilitiaCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		foreach (CTile selectedTile in selectedTiles)
		{
			SpawnMilitia(selectedTile.m_ArrayIndex);
		}
	}

	private void SpawnMilitia(Point spawnPoint)
	{
		CSpawner cSpawner = null;
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			foreach (CSpawner militiaSpawner in sideRoom.MilitiaSpawners)
			{
				if (militiaSpawner.ArrayIndex.X == spawnPoint.X && militiaSpawner.ArrayIndex.Y == spawnPoint.Y)
				{
					cSpawner = militiaSpawner;
					break;
				}
				if (cSpawner != null)
				{
					break;
				}
			}
		}
		if (cSpawner != null)
		{
			ForceSpawn(cSpawner);
		}
	}

	private void ForceSpawn(CSpawner spawner)
	{
		spawner.SpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count, treatAsSummon: false, initial: false, startRound: true);
	}

	private void DestroyRoomsWithDeadCaptains()
	{
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			if (!sideRoom.IsDestroyed && sideRoom.Captain.IsDead)
			{
				DestroyRoom(sideRoom);
			}
		}
	}

	private bool ShouldMoveOnRoomDestroyed(SideRoomInfo room, CActor actor)
	{
		if (actor is CPlayerActor)
		{
			return true;
		}
		if (!SecondPhase)
		{
			return false;
		}
		if (actor is CHeroSummonActor)
		{
			return true;
		}
		if (actor == room.Captain)
		{
			return true;
		}
		return false;
	}

	private void DestroyRoom(SideRoomInfo room)
	{
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (room.Contains(allAliveActor.ArrayIndex))
			{
				if (!ShouldMoveOnRoomDestroyed(room, allAliveActor))
				{
					GameState.KillActor(allAliveActor, allAliveActor, CActor.ECauseOfDeath.Suicide, out var _);
					continue;
				}
				Point point = FindNearestFreePointInPointRoom(room.NearestPointInCentralRoom);
				TeleportActor(allAliveActor, ScenarioManager.GetTile(point));
			}
		}
		for (int i = 0; i < room.EnemySpawners.Length; i++)
		{
			List<CSpawner> list = room.EnemySpawners[i];
			if (list == null)
			{
				continue;
			}
			foreach (CSpawner item in list)
			{
				item.SetLocation(new TileIndex(room.NearestPointInCentralRoom));
			}
		}
		ScenarioManager.ToggleTileHighlights(room.MilitiaSpawnPositions, enabled: false);
		ScenarioManager.ToggleTileHighlights(new List<Point> { room.NearestPointInCentralRoom }, enabled: false);
		room.Map.Destroyed = true;
		CDestroyRoom_MessageData cDestroyRoom_MessageData = new CDestroyRoom_MessageData();
		cDestroyRoom_MessageData.m_MapToDestroy = room.Map;
		ScenarioRuleClient.MessageHandler(cDestroyRoom_MessageData);
	}

	public void EnterSecondPhase()
	{
		GameState.TriggerCampaignSection("FS_064_The_Frosthaven_Seal_End_Round_10_Part1");
		SecondPhase = true;
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			DestroyRoom(sideRoom);
			for (int i = 0; i < sideRoom.EnemySpawners.Length; i++)
			{
				List<CSpawner> list = sideRoom.EnemySpawners[i];
				if (list == null)
				{
					continue;
				}
				foreach (CSpawner item in list)
				{
					item.SetActive(active: false);
				}
			}
		}
		CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
		{
			m_Point = new Point(10, 12),
			m_Zoom = 1f
		};
		ScenarioRuleClient.MessageHandler(message);
		GameState.TriggerCampaignSection("FS_064_The_Frosthaven_Seal_End_Round_10_Part2");
		if (tree != null)
		{
			if (tree is CObjectActor { AttachedProp: not null, AttachedProp: var attachedProp } cObjectActor)
			{
				cObjectActor.ClearAttachedProp();
				attachedProp.ClearAttachedRuntimeActor();
				attachedProp.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			}
			GameState.KillActor(tree, tree, CActor.ECauseOfDeath.Suicide, out var _);
			tree = null;
		}
		CSpawner.SpawnUnitAtTile(ScenarioManager.CurrentScenarioState.Players.Count, CActor.EType.Enemy, "VestigeOfTheImprisonedGodID", TreePosition.X, TreePosition.Y, out boss);
		boss.MultiHexArrayIndexes = bossMultihexArrayIndexes;
		UpdateGSpawnerIndicators();
		UpdateObjectives();
		UpdateTeleportTileHighlights();
		GameState.TriggerCampaignSection("FS_064_The_Frosthaven_Seal_End_Round_10_Part3");
	}

	public static List<object> PlayerTiles()
	{
		List<object> list = new List<object>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			CTile tile = ScenarioManager.GetTile(playerActor.ArrayIndex);
			list.Add(tile);
		}
		return list;
	}

	public static void ModifyTerrainUnderPlayers()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			ModifyTerrainUnderPlayer(ScenarioManager.GetTile(playerActor.ArrayIndex));
		}
	}

	private static void ModifyTerrainUnderPlayer(CTile tile)
	{
		ScenarioManager.ObjectImportType terrainType = ScenarioManager.ObjectImportType.TerrainRubble;
		string overridePropName = "VestigeDifficultTerrain";
		foreach (CObjectProp prop in tile.m_Props)
		{
			if (prop is CObjectHazardousTerrain)
			{
				return;
			}
			if (prop is CObjectDifficultTerrain)
			{
				prop.DestroyProp();
				terrainType = ScenarioManager.ObjectImportType.TerrainThorns;
				overridePropName = "VestigeHazardTerrain";
				break;
			}
		}
		CAbilityCreateTerrain.SpawnTerrain(terrainType, tile, null, null, 0f, overridePropName);
	}

	public static List<CActor> GetPlayersWhoDidNotAttackBossThisTurn(CActor boss)
	{
		List<CActor> list = new List<CActor>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (!playerActor.ActorsAttackedThisRound.Contains(boss))
			{
				list.Add(playerActor);
			}
		}
		return list;
	}

	private void UpdateObjectives()
	{
		if (!SecondPhase)
		{
			DefendTreeObjective.SetActivation(active: true);
			KillBossObjective.SetActivation(active: false);
			DefendTreeObjective.Current = ScenarioManager.CurrentScenarioState.RoundNumber - 1;
			DefendTreeObjective.Total = DefendRounds;
		}
		else
		{
			DefendTreeObjective.SetActivation(active: false);
			KillBossObjective.SetActivation(active: true);
		}
		ScenarioManager.CurrentScenarioState.ScenarioModifiers[1].SetHiddenState(!SecondPhase);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		_ = ev.Type;
		_ = 2;
		return false;
	}

	private void UpdateTeleportTileHighlights()
	{
		List<Point> list = new List<Point>();
		List<Point> list2 = new List<Point>();
		foreach (SideRoomInfo sideRoom in SideRooms)
		{
			if (sideRoom.IsDestroyed)
			{
				list2.AddRange(sideRoom.MilitiaSpawnPositions);
				list2.Add(sideRoom.NearestPointInCentralRoom);
			}
			else
			{
				list.AddRange(sideRoom.MilitiaSpawnPositions);
				list.Add(sideRoom.NearestPointInCentralRoom);
			}
		}
		ScenarioManager.ToggleTileHighlights(list, enabled: true);
		ScenarioManager.ToggleTileHighlights(list2, enabled: false);
	}

	public override bool IsTileEleveated(CTile tile)
	{
		if (!(tile.m_ArrayIndex == new Point(9, 11)) && !(tile.m_ArrayIndex == new Point(9, 12)))
		{
			return tile.m_ArrayIndex == new Point(10, 12);
		}
		return true;
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("VestigeOfTheImprisonedGodID", 0, null, CActor.EType.Enemy);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("militia_count", (ScenarioManager.CurrentScenarioState.Players.Count * 2).ToString());
	}
}

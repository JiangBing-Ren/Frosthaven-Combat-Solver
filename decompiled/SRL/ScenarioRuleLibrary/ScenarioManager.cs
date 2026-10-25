using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FH;
using FH.Tools;
using Libs.Utils.Profiler;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioManager
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EDLLMode
	{
		None,
		Campaign,
		[Obsolete]
		Guildmaster,
		CustomScenario
	}

	[Serializable]
	[KronosEnumPolice(1)]
	public enum ObjectImportType
	{
		None = 255,
		Hero = 0,
		Monster = 1,
		Tile = 2,
		EdgeTile = 3,
		Door = 4,
		GenericProp = 5,
		Chest = 6,
		GoalChest = 7,
		MoneyToken = 8,
		Trap = 9,
		Obstacle = 10,
		Coverage = 11,
		HeroSummons = 12,
		Spawner = 13,
		PressurePlate = 14,
		TerrainHotCoals = 15,
		TerrainWater = 16,
		TerrainRubble = 17,
		TerrainThorns = 18,
		Portal = 19,
		CarryableQuestItem = 20,
		TerrainVisualEffect = 21,
		Resource = 22,
		MonsterGrave = 23,
		SnowRock = 24,
		TerrainIce = 25,
		Snowdrift = 26,
		VFXProp = 27,
		Shadow = 28,
		TerrainLampVisualEffect = 29
	}

	public enum EAdjacentPosition
	{
		ECenter,
		ELeft,
		ETopLeft,
		ETopRight,
		ERight,
		EBottomLeft,
		EBottomRight
	}

	[Serializable]
	public enum EPerPartySizeConfig
	{
		Normal,
		Hidden,
		ToElite
	}

	public static readonly EDLLMode[] DLLModes = (EDLLMode[])Enum.GetValues(typeof(EDLLMode));

	public static readonly ObjectImportType[] ObjectImportTypes = (ObjectImportType[])Enum.GetValues(typeof(ObjectImportType));

	public static readonly EAdjacentPosition[] AdjacentPositionTypes = (EAdjacentPosition[])Enum.GetValues(typeof(EAdjacentPosition));

	private static int s_Width;

	private static int s_Height;

	private static CPathFinder s_PathFinder;

	private static CTile[,] s_TileArray;

	private static CScenario s_Scenario;

	[StateDump(Skip = true)]
	private static bool s_IsFirstLoad;

	[StateDump(Skip = true)]
	private static List<CTile> s_StartingTiles;

	public static Action CheckAchievementsCallback;

	public static CTile[,] Tiles => s_TileArray;

	public static List<CTile> StartingTiles => s_StartingTiles;

	public static int Width => s_Width;

	public static int Height => s_Height;

	public static CPathFinder PathFinder => s_PathFinder;

	public static CScenario Scenario => s_Scenario;

	public static bool IsFirstLoad => s_IsFirstLoad;

	public static EDLLMode Mode { get; set; }

	public static StateShared.EHouseRulesFlag HouseRulesSettings { get; set; }

	[StateDump(Skip = true)]
	public static ScenarioState CurrentScenarioState { get; set; }

	[StateDump(Skip = true)]
	public static bool IsInTutorial => CurrentScenarioState?.ScenarioSetup?.IsInTutorial == true;

	[StateDump(Skip = true)]
	public static ScenarioState OldFastReloadState { get; set; }

	[StateDump(Skip = true)]
	public static ScenarioState NewFastReloadState { get; set; }

	public static bool IsFastReloading => OldFastReloadState != null;

	public static bool IsLoading
	{
		get
		{
			ScenarioState currentScenarioState = CurrentScenarioState;
			if (currentScenarioState == null || !currentScenarioState.LoadingScenarioState)
			{
				return IsFastReloading;
			}
			return true;
		}
	}

	public static bool IsLoadingNotFirsLoad
	{
		get
		{
			if (IsLoading)
			{
				ScenarioState currentScenarioState = CurrentScenarioState;
				if (currentScenarioState == null)
				{
					return false;
				}
				return !currentScenarioState.IsFirstLoad;
			}
			return false;
		}
	}

	public static uint GoldConversion => CurrentScenarioState?.ScenarioSetup?.GoldConversion ?? 1;

	public static uint GoldConversionNextLevel => CurrentScenarioState?.ScenarioSetup?.GoldConversionNextLevel ?? 1;

	public static int TrapDamage => CurrentScenarioState?.ScenarioSetup?.TrapDamage ?? 2;

	public static int HazardousTerrainDamage => CurrentScenarioState?.ScenarioSetup?.HazardousTerrainDamage ?? 1;

	public static bool IsInTactical => CurrentScenarioState != null;

	public static int GetDirAngle(EAdjacentPosition pos)
	{
		return pos switch
		{
			EAdjacentPosition.ECenter => 270, 
			EAdjacentPosition.ELeft => 180, 
			EAdjacentPosition.ETopLeft => 240, 
			EAdjacentPosition.ETopRight => 300, 
			EAdjacentPosition.ERight => 0, 
			EAdjacentPosition.EBottomLeft => 150, 
			EAdjacentPosition.EBottomRight => 60, 
			_ => 0, 
		};
	}

	public static bool DisableCameraRotation()
	{
		if (CurrentScenarioState != null)
		{
			foreach (ScenarioScript scenarioScript in CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.DisableCameraRotation)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void Load(ScenarioState scenarioState, bool firstLoad)
	{
		s_IsFirstLoad = firstLoad;
		CurrentScenarioState?.ScriptsDone();
		CurrentScenarioState = scenarioState;
		foreach (CMap map in scenarioState.Maps)
		{
			map.Reset();
		}
		if (firstLoad && (s_Scenario?.AllActors?.Count).GetValueOrDefault() != 0)
		{
			throw new Exception("Attempt to load new CScenario on top of already initialized one.");
		}
		s_Scenario = new CScenario(scenarioState.ID, scenarioState.Name, scenarioState.PositiveSpaceOffset);
		Log.SimpleInfo("[Scenario Level]: (Loading Scenario State) Current Scenario level is " + scenarioState.Level);
		s_Width = scenarioState.Width + 2;
		s_Height = scenarioState.Height + 2;
		s_TileArray = new CTile[s_Width, s_Height];
		s_PathFinder = new CPathFinder(s_Width, s_Height, oddEvenCheck: true);
		foreach (CMap map2 in scenarioState.Maps)
		{
			s_Scenario.AddMap(map2);
			foreach (CMapTile mapTile in map2.MapTiles)
			{
				if (s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y] == null)
				{
					CTile cTile = new CTile
					{
						m_HexMap = map2,
						m_Hex = mapTile,
						m_ArrayIndex = new Point(mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y)
					};
					s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y] = cTile;
				}
				else
				{
					s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y].m_Hex2Map = map2;
					s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y].m_Hex2 = mapTile;
				}
				s_PathFinder.Nodes[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y].IslandID = map2.IslandID;
			}
		}
		for (int i = 0; i < s_Height; i++)
		{
			for (int j = 0; j < s_Width; j++)
			{
				CTile cTile2 = s_TileArray[j, i];
				if (cTile2 != null && !cTile2.m_Hex.FlagsSet(EFlags.Blocked | EFlags.Edge))
				{
					s_PathFinder.Nodes[j, i].Walkable = true;
					s_PathFinder.Nodes[j, i].Blocked = false;
				}
			}
		}
	}

	public static void Unload()
	{
		CurrentScenarioState?.ScriptsDone();
		CurrentScenarioState = null;
		s_Scenario = null;
		s_TileArray = null;
		s_StartingTiles = null;
		s_PathFinder = null;
	}

	public static void ResetCustomIDs(ScenarioState currentScenarioState)
	{
		foreach (CMap map in currentScenarioState.Maps)
		{
			foreach (EnemyState monster in map.Monsters)
			{
				CMonsterClass cMonsterClass = MonsterClassManager.Find(monster.ClassID);
				if (cMonsterClass != null)
				{
					cMonsterClass.RecycleID(monster.ID);
					monster.ID = 0;
				}
			}
			foreach (EnemyState allyMonster in map.AllyMonsters)
			{
				CMonsterClass cMonsterClass2 = MonsterClassManager.Find(allyMonster.ClassID);
				if (cMonsterClass2 != null)
				{
					cMonsterClass2.RecycleID(allyMonster.ID);
					allyMonster.ID = 0;
				}
			}
			foreach (EnemyState enemy2Monster in map.Enemy2Monsters)
			{
				CMonsterClass cMonsterClass3 = MonsterClassManager.Find(enemy2Monster.ClassID);
				if (cMonsterClass3 != null)
				{
					cMonsterClass3.RecycleID(enemy2Monster.ID);
					enemy2Monster.ID = 0;
				}
			}
			foreach (EnemyState neutralMonster in map.NeutralMonsters)
			{
				CMonsterClass cMonsterClass4 = MonsterClassManager.Find(neutralMonster.ClassID);
				if (cMonsterClass4 != null)
				{
					cMonsterClass4.RecycleID(neutralMonster.ID);
					neutralMonster.ID = 0;
				}
			}
		}
		if (currentScenarioState.EnemyClassManager == null)
		{
			return;
		}
		foreach (EnemyClassState enemyClass in currentScenarioState.EnemyClassManager.EnemyClasses)
		{
			enemyClass.ResetIDs();
		}
	}

	public static void SetHouseRules(StateShared.EHouseRulesFlag houseRulesSettings)
	{
		HouseRulesSettings = houseRulesSettings;
		Log.SimpleInfo("[HOUSE RULES] - changed house rules settings in ScenarioManager to " + houseRulesSettings);
	}

	public static void Reset()
	{
		ElementInfusionBoardManager.Reset();
		CharacterClassManager.Reset();
		MonsterClassManager.Reset();
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			attackModifier.Card.Reset();
		}
		CurrentScenarioState?.ScriptsDone();
	}

	public static void InitScenario(bool levelEditing)
	{
		using (new MiniProfilerScope("InitScenario"))
		{
			GameState.IsInLevelEditor = levelEditing;
			ScenarioRuleClient.Reset();
			GameState.ProtectLastDummyCardID = true;
			CActiveBonus.ProtectIDCounter = true;
			if (!levelEditing && CurrentScenarioState.IsFirstLoad)
			{
				CurrentScenarioState.IsSelectingPositions = true;
				if (GameState.RandomiseOnLoad)
				{
					if (CurrentScenarioState != null)
					{
						CurrentScenarioState.RandomiseRNGOnLoad();
					}
					ResetCustomIDs(CurrentScenarioState);
				}
				foreach (EnemyState allEnemyState in CurrentScenarioState.AllEnemyStates)
				{
					allEnemyState.IsRevealed = false;
				}
			}
			List<CObjectProp> list = new List<CObjectProp>();
			foreach (CObjectProp item7 in CurrentScenarioState.Props.ToList())
			{
				bool flag = false;
				if (levelEditing || item7.GetConfigForPartySize(CurrentScenarioState.Players.Count) != EPerPartySizeConfig.Hidden)
				{
					if (s_TileArray[item7.ArrayIndex.X, item7.ArrayIndex.Y] != null)
					{
						s_TileArray[item7.ArrayIndex.X, item7.ArrayIndex.Y].SpawnProp(item7);
						flag = true;
					}
				}
				else
				{
					CurrentScenarioState.Props.Remove(item7);
					CurrentScenarioState.DestroyedProps.Add(item7);
				}
				if (!flag)
				{
					list.Add(item7);
				}
			}
			foreach (CObjectProp item8 in list)
			{
				Watchman.Instance.Complete(item8.InstanceName);
			}
			foreach (CSpawner spawner in CurrentScenarioState.Spawners)
			{
				if (levelEditing || spawner.GetConfigForPartySize(CurrentScenarioState.Players.Count) != EPerPartySizeConfig.Hidden)
				{
					s_TileArray[spawner.ArrayIndex.X, spawner.ArrayIndex.Y].SpawnSpawner(spawner);
					spawner.OnScenarioStart(CurrentScenarioState.IsFirstLoad);
					if (spawner.StartingMap.Revealed && (spawner.SpawnerData.SpawnerTriggerType != SpawnerData.ESpawnerTriggerType.TriggeredExternally || spawner.WasActiveBeforeSave))
					{
						spawner.UpdateWillSpawnNextRoundIndicator(CurrentScenarioState.Players.Count);
					}
				}
			}
			if (GameState.LastDummyCardID != 0)
			{
				Log.Error($"GameState.LastDuummyCardId is {GameState.LastDummyCardID} during InitScenario");
			}
			GameState.ProtectLastDummyCardID = false;
			CurrentScenarioState.ApplyClassStates();
			if (levelEditing)
			{
				foreach (CMap map in CurrentScenarioState.Maps)
				{
					map.Prepare();
					bool revealed = map.Revealed;
					map.Reveal(initial: true, noIDRegen: true, forLevelEditor: true);
					map.Revealed = revealed;
				}
			}
			else
			{
				foreach (CMap map2 in CurrentScenarioState.Maps)
				{
					map2.Prepare();
					if (map2.Revealed || map2.IsAdditionalDungeonEntranceRoom || map2.Players.Any((PlayerState p) => p.IsRevealed && !p.HiddenAtStart) || map2.Monsters.Any((EnemyState m) => m.IsRevealed) || map2.HeroSummons.Any((HeroSummonState h) => h.IsRevealed))
					{
						map2.Reveal(initial: true, noIDRegen: true);
						if (map2.Destroyed)
						{
							CDestroyRoom_MessageData cDestroyRoom_MessageData = new CDestroyRoom_MessageData();
							cDestroyRoom_MessageData.m_MapToDestroy = map2;
							cDestroyRoom_MessageData.m_RevealMonsters = false;
							ScenarioRuleClient.MessageHandler(cDestroyRoom_MessageData);
						}
					}
				}
			}
			foreach (CObjectDoor item9 in CurrentScenarioState.Props.OfType<CObjectDoor>())
			{
				item9.InitDoor();
				if (!s_IsFirstLoad && item9.Activated)
				{
					if (PathFinder != null && PathFinder.Nodes != null)
					{
						PathFinder.Nodes[item9.ArrayIndex.X, item9.ArrayIndex.Y].IsBridgeOpen = true;
					}
					CActivateProp_MessageData message = new CActivateProp_MessageData(null)
					{
						m_Prop = item9,
						m_InitialLoad = true
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			foreach (CObjectDifficultTerrain item10 in CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>())
			{
				item10.InitDifficultTerrain();
			}
			foreach (CObjectIceTerrain item11 in CurrentScenarioState.Props.OfType<CObjectIceTerrain>())
			{
				item11.SetIceTerrain(isIceTerrain: true);
			}
			foreach (CObjectObstacle item12 in CurrentScenarioState.Props.OfType<CObjectObstacle>())
			{
				item12.SetIsBlocker(isBlocker: true);
			}
			Scenario.PlayerActors.Clear();
			foreach (CPlayerActor initialPlayer in Scenario.InitialPlayers)
			{
				CPlayerActor item = initialPlayer.Clone();
				Scenario.PlayerActors.Add(item);
			}
			Scenario.HeroSummons.Clear();
			foreach (CHeroSummonActor initialHeroSummon in Scenario.InitialHeroSummons)
			{
				CHeroSummonActor item2 = initialHeroSummon.Clone();
				Scenario.HeroSummons.Add(item2);
			}
			Scenario.Enemies.Clear();
			foreach (CEnemyActor initialEnemy in Scenario.InitialEnemies)
			{
				CEnemyActor item3 = initialEnemy.Clone();
				Scenario.Enemies.Add(item3);
			}
			Scenario.AllyMonsters.Clear();
			foreach (CEnemyActor initialAllyMonster in Scenario.InitialAllyMonsters)
			{
				CEnemyActor item4 = initialAllyMonster.Clone();
				Scenario.AllyMonsters.Add(item4);
			}
			Scenario.Enemy2Monsters.Clear();
			foreach (CEnemyActor initialEnemy2Monster in Scenario.InitialEnemy2Monsters)
			{
				CEnemyActor item5 = initialEnemy2Monster.Clone();
				Scenario.Enemy2Monsters.Add(item5);
			}
			Scenario.NeutralMonsters.Clear();
			foreach (CEnemyActor initialNeutralMonster in Scenario.InitialNeutralMonsters)
			{
				CEnemyActor item6 = initialNeutralMonster.Clone();
				Scenario.NeutralMonsters.Add(item6);
			}
			Scenario.Objects.Clear();
			foreach (CObjectActor initialObject in Scenario.InitialObjects)
			{
				CObjectActor cObjectActor = initialObject.Clone();
				Scenario.Objects.Add(cObjectActor);
				if (cObjectActor.AttachedProp != null)
				{
					cObjectActor.AttachedProp.SetActorAttachedAtRuntime(cObjectActor);
				}
			}
			Scenario.SortLists();
			if (CActiveBonus.IDCounter != 0)
			{
				Log.Error($"CActiveBonus.IDCounter is {CActiveBonus.IDCounter} during InitScenario");
			}
			CActiveBonus.ProtectIDCounter = false;
			CurrentScenarioState.Apply(levelEditing);
			if (!levelEditing && CurrentScenarioState.IsFirstLoad)
			{
				foreach (CActor allAliveActor in Scenario.AllAliveActors)
				{
					CurrentScenarioState.PerformOnSpawnedModifiers(allAliveActor, CurrentScenarioState.Players.Count, levelEditing);
				}
			}
			if (GameState.RandomiseOnLoad && CurrentScenarioState.IsFirstLoad && CurrentScenarioState != null)
			{
				CurrentScenarioState.RandomiseDecksOnLoad();
			}
		}
	}

	public static void SetStartingTiles(List<CTile> startingTiles)
	{
		s_StartingTiles = startingTiles;
	}

	public static void ToggleTileHighlights(IEnumerable<Point> tileIndices, bool enabled, CVector4 color = null, bool standingHighlights = false)
	{
		CToggleTileHighlights_MessageData message = new CToggleTileHighlights_MessageData(tileIndices.ToList(), enabled, color)
		{
			StandingHighlights = standingHighlights
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void ToggleTileLinks(IEnumerable<(Point, Point)> links, bool enabled)
	{
		CToggleTileLinks_MessageData message = new CToggleTileLinks_MessageData(links.ToList(), enabled);
		ScenarioRuleClient.MessageHandler(message);
	}

	public static EAdjacentPosition GetOppositeDirection(EAdjacentPosition currentDirection)
	{
		return currentDirection switch
		{
			EAdjacentPosition.ELeft => EAdjacentPosition.ERight, 
			EAdjacentPosition.ERight => EAdjacentPosition.ELeft, 
			EAdjacentPosition.ETopLeft => EAdjacentPosition.EBottomRight, 
			EAdjacentPosition.ETopRight => EAdjacentPosition.EBottomLeft, 
			EAdjacentPosition.EBottomLeft => EAdjacentPosition.ETopRight, 
			EAdjacentPosition.EBottomRight => EAdjacentPosition.ETopLeft, 
			_ => EAdjacentPosition.ECenter, 
		};
	}

	public static CTile GetTile(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
		{
			return null;
		}
		return Tiles[x, y];
	}

	public static CTile GetTile(Point point)
	{
		return GetTile(point.X, point.Y);
	}

	public static CTile GetTile(TileIndex tileIndex)
	{
		return GetTile(tileIndex.X, tileIndex.Y);
	}

	public static CTile GetNextTileInDirection(CTile from, CTile to)
	{
		return (from x in GetAllAdjacentTiles(@from)
			orderby Point.HexDist(x.m_ArrayIndex, to.m_ArrayIndex)
			select x).FirstOrDefault();
	}

	public static CTile GetAdjacentTile(int x, int y, EAdjacentPosition eposition)
	{
		int num = x;
		int num2 = y;
		switch (eposition)
		{
		case EAdjacentPosition.ELeft:
			num--;
			break;
		case EAdjacentPosition.ERight:
			num++;
			break;
		case EAdjacentPosition.ETopLeft:
			num2++;
			break;
		case EAdjacentPosition.ETopRight:
			num++;
			num2++;
			break;
		case EAdjacentPosition.EBottomLeft:
			num2--;
			break;
		case EAdjacentPosition.EBottomRight:
			num++;
			num2--;
			break;
		}
		int num3 = (((num2 & 1) != 0) ? (Width - 2) : (Width - 1));
		if (num2 != y)
		{
			num = (((num2 & 1) != 0) ? (num - 1) : num);
		}
		if (num >= 0 && num2 >= 0 && num <= num3 && num2 <= Height - 1)
		{
			return Tiles[num, num2];
		}
		return null;
	}

	public static List<CTile> GetAllAdjacentTiles(int x, int y)
	{
		List<CTile> list = new List<CTile>();
		CTile adjacentTile = GetAdjacentTile(x, y, EAdjacentPosition.EBottomLeft);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(x, y, EAdjacentPosition.EBottomRight);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(x, y, EAdjacentPosition.ELeft);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(x, y, EAdjacentPosition.ERight);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(x, y, EAdjacentPosition.ETopLeft);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(x, y, EAdjacentPosition.ETopRight);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		return list;
	}

	public static List<CTile> GetAllAdjacentTiles(CTile tile)
	{
		return GetAllAdjacentTiles(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y);
	}

	public static List<CTile> GetAllUnblockedTilesFromOrigin(CTile originTile, int maxDepth = 100, List<CTile> tempTiles = null)
	{
		List<CTile> unblockedTiles = new List<CTile>();
		unblockedTiles.Add(originTile);
		return GetAllUnblockedTilesRecursive(ref unblockedTiles, ref originTile, 1, maxDepth, tempTiles);
	}

	public static List<CTile> GetAllUnblockedTilesFromOriginNonRecursive(CTile originTile, List<CTile> tempTiles = null)
	{
		List<CTile> list = new List<CTile>();
		Queue<CTile> queue = new Queue<CTile>();
		list.Add(originTile);
		queue.Enqueue(originTile);
		while (queue.Count > 0)
		{
			CTile tile = queue.Dequeue();
			foreach (CTile allAdjacentTile in GetAllAdjacentTiles(tile))
			{
				if (list.Contains(allAdjacentTile) || !allAdjacentTile.IsMapShared(tile))
				{
					continue;
				}
				if (allAdjacentTile.FindProp(ObjectImportType.Door) is CObjectDoor { DoorIsOpen: false })
				{
					list.Add(allAdjacentTile);
					continue;
				}
				CNode cNode = PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y];
				if ((!cNode.Blocked || (tempTiles != null && tempTiles.Contains(allAdjacentTile))) && !cNode.TransientBlocked && cNode.Walkable)
				{
					list.Add(allAdjacentTile);
					queue.Enqueue(allAdjacentTile);
				}
			}
		}
		return list;
	}

	public static List<CTile> GetAllUnblockedTilesRecursive(ref List<CTile> unblockedTiles, ref CTile currentTile, int currentDepth, int maxDepth, List<CTile> tempTiles)
	{
		foreach (CTile allAdjacentTile in GetAllAdjacentTiles(currentTile))
		{
			CTile currentTile2 = allAdjacentTile;
			CObjectDoor cObjectDoor = allAdjacentTile.FindProp(ObjectImportType.Door) as CObjectDoor;
			if (unblockedTiles.Contains(allAdjacentTile) || !allAdjacentTile.IsMapShared(currentTile))
			{
				continue;
			}
			if (cObjectDoor != null && !cObjectDoor.DoorIsOpen)
			{
				unblockedTiles.Add(allAdjacentTile);
			}
			else if ((!PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || (tempTiles != null && tempTiles.Contains(allAdjacentTile))) && !PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].TransientBlocked && PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Walkable)
			{
				unblockedTiles.Add(allAdjacentTile);
				if (currentDepth < maxDepth)
				{
					unblockedTiles = GetAllUnblockedTilesRecursive(ref unblockedTiles, ref currentTile2, currentDepth + 1, maxDepth, tempTiles);
				}
				else
				{
					Log.Warning("GetAllUnblockedTilesRecursive Max depth hit");
				}
			}
		}
		return unblockedTiles;
	}

	public static List<CTile> GetAllUnblockedTilesFromOriginNoDoors(CTile originTile, int maxDepth = 100, List<CTile> tempTiles = null)
	{
		List<CTile> unblockedTiles = new List<CTile>();
		unblockedTiles.Add(originTile);
		return GetAllUnblockedTilesRecursiveNoDoors(ref unblockedTiles, ref originTile, 1, maxDepth, tempTiles);
	}

	public static List<CTile> GetAllUnblockedTilesRecursiveNoDoors(ref List<CTile> unblockedTiles, ref CTile currentTile, int currentDepth, int maxDepth, List<CTile> tempTiles)
	{
		foreach (CTile allAdjacentTile in GetAllAdjacentTiles(currentTile))
		{
			CTile currentTile2 = allAdjacentTile;
			if (!unblockedTiles.Contains(allAdjacentTile) && allAdjacentTile.IsMapShared(currentTile) && !(allAdjacentTile.FindProp(ObjectImportType.Door) is CObjectDoor))
			{
				if ((!PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || (tempTiles != null && tempTiles.Contains(allAdjacentTile))) && !PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].TransientBlocked && PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Walkable)
				{
					unblockedTiles.Add(allAdjacentTile);
				}
				if (currentDepth < maxDepth)
				{
					unblockedTiles = GetAllUnblockedTilesRecursiveNoDoors(ref unblockedTiles, ref currentTile2, currentDepth + 1, maxDepth, tempTiles);
				}
				else
				{
					Log.Warning("GetAllUnblockedTilesRecursive Max depth hit");
				}
			}
		}
		return unblockedTiles;
	}

	public static List<CActor> GetAllAdjacentActors(CActor actor, bool useRealArrayIndex = true)
	{
		List<CActor> list = new List<CActor>();
		List<CTile> list2 = new List<CTile>();
		if (useRealArrayIndex)
		{
			list2.Add(GetTile(actor.RealArrayIndex));
		}
		else
		{
			list2.Add(GetTile(actor.ArrayIndex));
		}
		if (actor.MultiHexTiles != null)
		{
			list2.AddRange(actor.MultiHexTiles);
		}
		if (actor is CObjectActor { AttachedProp: not null } cObjectActor)
		{
			list2.AddRange(from x in cObjectActor.AttachedProp.GetTiles()
				select GetTile(x));
		}
		foreach (CTile item in CustomAbility.GetTilesInRange(list2, 1, includeOrigin: false))
		{
			CActor livingActor = item.LivingActor;
			if (livingActor != null)
			{
				list.Add(livingActor);
			}
		}
		return list;
	}

	private static CActor GetAdjacentActor(CActor actor, EAdjacentPosition position, bool useRealArrayIndex = true)
	{
		Point point = actor.RealArrayIndex;
		if (!useRealArrayIndex)
		{
			point = actor.ArrayIndex;
		}
		CTile adjacentTile = GetAdjacentTile(point.X, point.Y, position);
		CTile cTile = Tiles[point.X, point.Y];
		CNode cNode = ((cTile != null) ? PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y] : null);
		CNode cNode2 = ((adjacentTile != null) ? PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y] : null);
		if (adjacentTile != null && (cTile.m_HexMap == adjacentTile.m_HexMap || (cTile.m_HexMap != adjacentTile.m_HexMap && (cNode.IsBridge || cNode2.IsBridge))))
		{
			return Scenario.FindActorAt(adjacentTile.m_ArrayIndex);
		}
		return null;
	}

	public static EAdjacentPosition GetAdjacentPosition(int x, int y, int newx, int newy)
	{
		if (newy != y)
		{
			newx = (((newy & 1) != 0) ? (newx + 1) : newx);
		}
		if (newx + 1 == x && newy == y)
		{
			return EAdjacentPosition.ELeft;
		}
		if (newx - 1 == x && newy == y)
		{
			return EAdjacentPosition.ERight;
		}
		if (newx == x && newy - 1 == y)
		{
			return EAdjacentPosition.ETopLeft;
		}
		if (newx - 1 == x && newy - 1 == y)
		{
			return EAdjacentPosition.ETopRight;
		}
		if (newx == x && newy + 1 == y)
		{
			return EAdjacentPosition.EBottomLeft;
		}
		if (newx - 1 == x && newy + 1 == y)
		{
			return EAdjacentPosition.EBottomRight;
		}
		return EAdjacentPosition.ECenter;
	}

	public static bool IsTileAdjacent(int sourceX, int sourceY, int targetX, int targetY, bool ignoreWalls = false)
	{
		for (EAdjacentPosition eAdjacentPosition = EAdjacentPosition.ELeft; eAdjacentPosition <= EAdjacentPosition.EBottomRight; eAdjacentPosition++)
		{
			CTile adjacentTile = GetAdjacentTile(sourceX, sourceY, eAdjacentPosition);
			if (adjacentTile != null && targetX == adjacentTile.m_ArrayIndex.X && targetY == adjacentTile.m_ArrayIndex.Y)
			{
				CTile cTile = Tiles[sourceX, sourceY];
				CNode cNode = PathFinder.Nodes[sourceX, sourceY];
				CNode cNode2 = PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if (!ignoreWalls && cTile.m_HexMap != adjacentTile.m_HexMap && !cNode.IsBridge && !cNode2.IsBridge)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public static bool IsTileAdjacent(TileIndex source, TileIndex target, bool ignoreWalls = false)
	{
		return IsTileAdjacent(source.X, source.Y, target.X, target.Y, ignoreWalls = false);
	}

	public static bool IsTileAdjacent(CTile source, CTile target, bool ignoreWalls = false)
	{
		return IsTileAdjacent(source.m_ArrayIndex.X, source.m_ArrayIndex.Y, target.m_ArrayIndex.X, target.m_ArrayIndex.Y, ignoreWalls = false);
	}

	public static int GetTileDistance(int x1, int y1, int x2, int y2)
	{
		int num = x2 - x1;
		int value = y2 - y1;
		int num2 = Math.Abs(num);
		int num3 = Math.Abs(value);
		num2 = ((!((num < 0) ^ ((y1 & 1) == 1))) ? Math.Max(0, num2 - num3 / 2) : Math.Max(0, num2 - (num3 + 1) / 2));
		return num2 + num3;
	}

	public static int GetTileDistance(CTile tile1, CTile tile2)
	{
		return GetTileDistance(tile1.m_ArrayIndex.X, tile1.m_ArrayIndex.Y, tile2.m_ArrayIndex.X, tile2.m_ArrayIndex.Y);
	}

	public static int GetTileDistance(TileIndex tile1, TileIndex tile2)
	{
		return GetTileDistance(tile1.X, tile1.Y, tile2.X, tile2.Y);
	}

	public static int GetTileDistance(Point tile1, Point tile2)
	{
		return GetTileDistance(tile1.X, tile1.Y, tile2.X, tile2.Y);
	}

	public static bool RoomsBetweenTilesRevealed(CTile originTile, CTile destinationTile, bool checkTeleport = false)
	{
		if (originTile.IsMapShared(destinationTile))
		{
			return true;
		}
		List<CMap> checkedMaps = new List<CMap> { originTile.m_HexMap };
		List<CMap> checkedMapsForTeleport = new List<CMap> { originTile.m_HexMap };
		return RoomsBetweenTilesRevealedRecursive(foundRoomPath: false, originTile.m_HexMap, destinationTile, ref checkedMaps, ref checkedMapsForTeleport, checkTeleport);
	}

	private static bool RoomsBetweenTilesRevealedRecursive(bool foundRoomPath, CMap currentMap, CTile destinationTile, ref List<CMap> checkedMaps, ref List<CMap> checkedMapsForTeleport, bool checkTeleport)
	{
		foreach (CObjectDoor doorProp in CurrentScenarioState.DoorProps)
		{
			if ((doorProp.HexMap != currentMap && doorProp.Hex2Map != currentMap) || !doorProp.DoorIsOpen)
			{
				continue;
			}
			CTile cTile = Tiles[doorProp.ArrayIndex.X, doorProp.ArrayIndex.Y];
			CMap cMap = ((cTile.m_HexMap == currentMap) ? cTile.m_Hex2Map : cTile.m_HexMap);
			if (cMap == null)
			{
				continue;
			}
			if (cMap == destinationTile.m_HexMap)
			{
				foundRoomPath = true;
				break;
			}
			if (!checkedMaps.Contains(cMap))
			{
				checkedMaps.Add(cMap);
				foundRoomPath = RoomsBetweenTilesRevealedRecursive(foundRoomPath, cMap, destinationTile, ref checkedMaps, ref checkedMapsForTeleport, checkTeleport);
				if (foundRoomPath)
				{
					break;
				}
			}
		}
		if (checkTeleport)
		{
			foreach (CMap map in CurrentScenarioState.Maps)
			{
				if (map == currentMap)
				{
					continue;
				}
				foreach (CMapTile mapTile in map.MapTiles)
				{
					foreach (CNavLink navLink in PathFinder.GetNode(mapTile.ArrayIndex).NavLinks)
					{
						_ = navLink;
						CMap hexMap = destinationTile.m_HexMap;
						if (hexMap == currentMap)
						{
							foundRoomPath = true;
							break;
						}
						if (!checkedMapsForTeleport.Contains(hexMap))
						{
							checkedMapsForTeleport.Add(hexMap);
							foundRoomPath = RoomsBetweenTilesRevealedRecursive(foundRoomPath, hexMap, destinationTile, ref checkedMaps, ref checkedMapsForTeleport, checkTeleport);
							if (foundRoomPath)
							{
								break;
							}
						}
					}
					if (foundRoomPath)
					{
						break;
					}
				}
				if (foundRoomPath)
				{
					break;
				}
			}
		}
		return foundRoomPath;
	}

	public static void RevealAllMaps()
	{
		foreach (CObjectDoor item in from w in CurrentScenarioState.Props.OfType<CObjectDoor>()
			where !w.Activated && !w.IsDungeonEntrance
			select w)
		{
			item.ForceActivate(null);
		}
	}

	public static CActor FindActor(string actorGuid)
	{
		if (Scenario != null)
		{
			return Scenario.AllActors.SingleOrDefault((CActor s) => s.ActorGuid == actorGuid);
		}
		Log.ErrorWithReport("Scenario is null, unable to find actor by actor guid.");
		return null;
	}

	public static CActor FindActorWithAbilityCard(CAbilityCard card)
	{
		return Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.CharacterClass.AbilityCardsPool.Exists((CAbilityCard e) => e.ID == card.ID));
	}

	public static CActor FindActorWithActivatedItemFromID(int itemID, CCharacterClass characterClass)
	{
		List<CPlayerActor> list = Scenario.AllPlayers.Where((CPlayerActor w) => w.Inventory.AllItemsAndPets.Any((CItem e) => e.ID == itemID && e.SlotState == CItem.EItemSlotState.Active)).ToList();
		if (list.Count == 1)
		{
			return list[0];
		}
		CPlayerActor cPlayerActor = list.SingleOrDefault((CPlayerActor s) => s.CharacterClass.ID == characterClass.ID);
		if (cPlayerActor != null)
		{
			return cPlayerActor;
		}
		return null;
	}

	public static List<CTile> GetTilesInLine(int x, int y, int range, EAdjacentPosition eposition)
	{
		List<CTile> list = new List<CTile>();
		int x2 = x;
		int y2 = y;
		for (int i = 0; i < range; i++)
		{
			if (i != 0)
			{
				x2 = list.Last().m_ArrayIndex.X;
				y2 = list.Last().m_ArrayIndex.Y;
			}
			CTile adjacentTile = GetAdjacentTile(x2, y2, eposition);
			if (adjacentTile == null)
			{
				break;
			}
			list.Add(adjacentTile);
		}
		return list;
	}

	public static void CheckAchievements()
	{
		CheckAchievementsCallback?.Invoke();
	}
}

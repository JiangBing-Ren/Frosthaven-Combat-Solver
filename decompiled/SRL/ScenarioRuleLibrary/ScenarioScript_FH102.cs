using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH102 : ScenarioScript
{
	[Serializable]
	public class Storage102
	{
		public List<Point> WalkableTiles = new List<Point>();

		public int TileCountProcessed;

		public List<Point> NumberedTokens = new List<Point>();
	}

	[StateDump(Skip = true)]
	public Action OnTilesChanged;

	private Storage102 serializedProperties;

	private CObjective_Scripted KillSavvasObjective;

	private CObjective_Scripted PlaceXTilesObjective;

	public List<Point> WalkableTiles
	{
		get
		{
			return serializedProperties.WalkableTiles;
		}
		private set
		{
			serializedProperties.WalkableTiles = value;
		}
	}

	public int TileCountProcessed
	{
		get
		{
			return serializedProperties.TileCountProcessed;
		}
		private set
		{
			serializedProperties.TileCountProcessed = value;
		}
	}

	public List<Point> NumberedTokens
	{
		get
		{
			return serializedProperties.NumberedTokens;
		}
		private set
		{
			serializedProperties.NumberedTokens = value;
		}
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			return;
		}
		serializedProperties = (Storage102)JsonConvert.DeserializeObject(storage, typeof(Storage102));
		CTile[,] tiles = ScenarioManager.Tiles;
		int upperBound = tiles.GetUpperBound(0);
		int upperBound2 = tiles.GetUpperBound(1);
		for (int i = tiles.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = tiles.GetLowerBound(1); j <= upperBound2; j++)
			{
				ToggleTile(tiles[i, j], active: false, fromSerialization: true);
			}
		}
		foreach (Point walkableTile in WalkableTiles)
		{
			CTile tile = ScenarioManager.GetTile(walkableTile);
			if (tile != null)
			{
				ToggleTile(tile, active: true, fromSerialization: true);
			}
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage102();
		CPathFinder.s_CanStepCallback = (CPathFinderContext context, Point ptFrom, Point ptTo) => context.IsCalculatingDistance ? CPathFinder.ECanStepCallbackResult.ForceCanStep : CPathFinder.ECanStepCallbackResult.Default;
		KillSavvasObjective = GetObjective<CObjective_Scripted>("KillSavvas");
		PlaceXTilesObjective = GetObjective<CObjective_Scripted>("PlaceXTiles");
		return base.Initialize();
	}

	protected override void Done()
	{
		CPathFinder.s_CanStepCallback = null;
		base.Done();
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartScenario)
		{
			return;
		}
		int count = ScenarioManager.Scenario.AllPlayers.Count;
		if (count <= 2)
		{
			PlaceXTilesObjective.Total = 35;
		}
		else if (count <= 3)
		{
			PlaceXTilesObjective.Total = 40;
		}
		else if (count <= 4)
		{
			PlaceXTilesObjective.Total = 55;
		}
		KillSavvasObjective.Total = 1;
		CTile[,] tiles = ScenarioManager.Tiles;
		int upperBound = tiles.GetUpperBound(0);
		int upperBound2 = tiles.GetUpperBound(1);
		for (int i = tiles.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = tiles.GetLowerBound(1); j <= upperBound2; j++)
			{
				ToggleTile(tiles[i, j], active: false);
			}
		}
		foreach (CTile startingTile in ScenarioManager.StartingTiles)
		{
			ToggleTile(startingTile, active: true);
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor.Class.ID == "SavvasIcestormEliteID")
		{
			KillSavvasObjective.Current = 1;
			KillSavvasObjective.ScriptedIsComplete = KillSavvasObjective.Current >= KillSavvasObjective.Total;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	private void UpdatePlaceXObjective()
	{
		int num = Math.Max(0, WalkableTiles.Count - ScenarioManager.StartingTiles.Count);
		if (PlaceXTilesObjective.Current != num)
		{
			PlaceXTilesObjective.Current = num;
			if (PlaceXTilesObjective.Current >= PlaceXTilesObjective.Total)
			{
				PlaceXTilesObjective.ScriptedIsComplete = true;
			}
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	public static void ToggleTile(CTile tile, bool active, bool fromSerialization = false)
	{
		if (tile == null)
		{
			return;
		}
		CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
		node.Walkable = active;
		node.Blocked = !active;
		if (!(ScenarioScript.current is ScenarioScript_FH102 { OnTilesChanged: var onTilesChanged } scenarioScript_FH))
		{
			return;
		}
		onTilesChanged?.Invoke();
		if (fromSerialization)
		{
			return;
		}
		int count = ScenarioManager.Scenario.InitialPlayers.Count;
		if (!active)
		{
			scenarioScript_FH.WalkableTiles.Remove(tile.m_ArrayIndex);
			return;
		}
		scenarioScript_FH.WalkableTiles.Add(tile.m_ArrayIndex);
		scenarioScript_FH.UpdatePlaceXObjective();
		int num = scenarioScript_FH.PlaceXTilesObjective.Current;
		if (num < 5)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 5)
		{
			scenarioScript_FH.TileCountProcessed = 5;
			AddNumberedToken(tile);
		}
		if (num < 10)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 10)
		{
			scenarioScript_FH.TileCountProcessed = 10;
			if (count <= 3)
			{
				SpawnAtTile(tile, "NightDemonID");
				SpawnForEachNumberedToken("ChaosDemonEliteID");
			}
			else if (count == 4)
			{
				SpawnAtTile(tile, "NightDemonID");
			}
		}
		if (num < 15)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 15)
		{
			scenarioScript_FH.TileCountProcessed = 15;
			if (count <= 2)
			{
				AddNumberedToken(tile);
				SpawnForEachNumberedToken("NightDemonID");
			}
			else if (count <= 3)
			{
				SpawnAtTile(tile, "LivingSpiritEliteID");
				SpawnForEachNumberedToken("LivingSpiritID");
			}
			else if (count == 4)
			{
				SpawnAtTile(tile, "NightDemonID");
				SpawnForEachNumberedToken("ChaosDemonEliteID");
			}
		}
		if (num < 20)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 20)
		{
			scenarioScript_FH.TileCountProcessed = 20;
			if (count <= 2)
			{
				SpawnAtTile(tile, "LivingSpiritID");
				Point pos = PickRandomNumberedToken();
				SpawnChest(CObjectChest.EType.Gain10Gold_85, pos);
			}
			else if (count <= 3)
			{
				AddNumberedToken(tile);
				SpawnAtRandomNumberedToken("NightDemonEliteID");
			}
			else if (count == 4)
			{
				SpawnAtTile(tile, "LivingSpiritEliteID");
				SpawnForEachNumberedToken("LivingSpiritID");
			}
		}
		if (num < 25)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 25)
		{
			scenarioScript_FH.TileCountProcessed = 25;
			if (count <= 2)
			{
				SpawnAtTile(tile, "SavvasIcestormEliteID");
				SpawnAtRandomNumberedToken("ChaosDemonID");
			}
			else if (count <= 3)
			{
				SpawnAtTile(tile, "LivingSpiritID");
				CActor cActor = SpawnAtRandomNumberedToken("NightDemonID");
				Point pos2 = ((cActor != null) ? PickRandomNumberedToken(cActor.ArrayIndex) : PickRandomNumberedToken());
				SpawnChest(CObjectChest.EType.Gain10Gold_85, pos2);
			}
			else if (count == 4)
			{
				AddNumberedToken(tile);
				SpawnAtRandomNumberedToken("NightDemonID");
			}
		}
		if (num < 30)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 30)
		{
			scenarioScript_FH.TileCountProcessed = 30;
			if (count <= 2)
			{
				SpawnForEachNumberedToken("NightDemonID");
			}
			else if (count <= 3)
			{
				SpawnAtTile(tile, "SavvasIcestormEliteID");
			}
			else if (count == 4)
			{
				SpawnAtTile(tile, "ChaosDemonID");
				SpawnForEachNumberedToken("NightDemonID");
			}
		}
		if (num < 35)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 35)
		{
			scenarioScript_FH.TileCountProcessed = 35;
			if (count <= 2)
			{
				SpawnChest(CObjectChest.EType.UnlockScenario103_57, tile.m_ArrayIndex);
			}
			else if (count <= 3)
			{
				List<Point> list = new List<Point>();
				CActor cActor2 = SpawnAtRandomStartingPosition("ChaosDemonID");
				if (cActor2 != null)
				{
					list.Add(cActor2.ArrayIndex);
				}
				CActor cActor3 = SpawnAtRandomStartingPosition("LivingSpiritEliteID", list.ToArray());
				if (cActor3 != null)
				{
					list.Add(cActor3.ArrayIndex);
				}
				SpawnAtRandomStartingPosition("NightDemonEliteID", list.ToArray());
			}
			else if (count == 4)
			{
				SpawnAtTile(tile, "LivingSpiritID");
				Point pos3 = PickRandomNumberedToken();
				SpawnChest(CObjectChest.EType.Gain10Gold_85, pos3);
			}
		}
		if (num < 40)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 40)
		{
			scenarioScript_FH.TileCountProcessed = 40;
			switch (count)
			{
			case 3:
				SpawnChest(CObjectChest.EType.UnlockScenario103_57, tile.m_ArrayIndex);
				break;
			case 4:
				SpawnAtTile(tile, "SavvasIcestormEliteID");
				SpawnAtRandomNumberedToken("ChaosDemonID");
				break;
			}
		}
		if (num < 45)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 45)
		{
			scenarioScript_FH.TileCountProcessed = 45;
			if (count == 2)
			{
				SpawnStatue();
			}
			if (count == 4)
			{
				AddNumberedToken(tile);
				List<Point> list2 = new List<Point>();
				CActor cActor4 = SpawnAtRandomStartingPosition("ChaosDemonID");
				if (cActor4 != null)
				{
					list2.Add(cActor4.ArrayIndex);
				}
				CActor cActor5 = SpawnAtRandomStartingPosition("LivingSpiritEliteID", list2.ToArray());
				if (cActor5 != null)
				{
					list2.Add(cActor5.ArrayIndex);
				}
				SpawnAtRandomStartingPosition("NightDemonEliteID", list2.ToArray());
			}
		}
		if (num < 50)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 50)
		{
			scenarioScript_FH.TileCountProcessed = 50;
			if (count == 3)
			{
				SpawnStatue();
			}
			if (count == 4)
			{
				SpawnForEachNumberedToken("NightDemonID");
			}
		}
		if (num < 55)
		{
			return;
		}
		if (scenarioScript_FH.TileCountProcessed < 55)
		{
			scenarioScript_FH.TileCountProcessed = 55;
			if (count == 4)
			{
				SpawnChest(CObjectChest.EType.UnlockScenario103_57, tile.m_ArrayIndex);
			}
		}
		if (num >= 65 && scenarioScript_FH.TileCountProcessed < 65)
		{
			scenarioScript_FH.TileCountProcessed = 65;
			if (count == 4)
			{
				SpawnStatue();
			}
		}
	}

	public static CActor SpawnAtPos(Point pos, string classID)
	{
		ScenarioScript_FH102 obj = ScenarioScript.current as ScenarioScript_FH102;
		CTile cTile = GameState.FindClosestEmptyTile(ScenarioManager.GetTile(pos));
		obj.SpawnActorAtTile(cTile.m_ArrayIndex, classID, out var actor, addIntoInitiativeOrder: true);
		return actor;
	}

	public static void SpawnForEachNumberedToken(string classID)
	{
		foreach (Point numberedToken in (ScenarioScript.current as ScenarioScript_FH102).NumberedTokens)
		{
			SpawnAtPos(numberedToken, classID);
		}
	}

	public static void SpawnAtTile(CTile tile, string classID)
	{
		_ = ScenarioScript.current;
		SpawnAtPos(tile.m_ArrayIndex, classID);
	}

	public static CActor SpawnAtRandomNumberedToken(string classID)
	{
		_ = ScenarioScript.current;
		return SpawnAtPos(PickRandomNumberedToken(), classID);
	}

	public static CActor SpawnAtRandomStartingPosition(string classID, params Point[] exclude)
	{
		_ = ScenarioScript.current;
		return SpawnAtPos(PickRandomStartingPosition(exclude), classID);
	}

	public static void AddNumberedToken(CTile tile)
	{
		(ScenarioScript.current as ScenarioScript_FH102).NumberedTokens.Add(tile.m_ArrayIndex);
	}

	public static Point PickRandomNumberedToken(params Point[] exclude)
	{
		ScenarioScript_FH102 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH102;
		if (exclude != null && exclude.Length != 0)
		{
			List<Point> list = scenarioScript_FH.NumberedTokens.Except(exclude).ToList();
			return list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
		}
		return scenarioScript_FH.NumberedTokens[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(scenarioScript_FH.NumberedTokens.Count)];
	}

	public static Point PickRandomStartingPosition(params Point[] exclude)
	{
		_ = ScenarioScript.current;
		List<CTile> startingTiles = ScenarioManager.StartingTiles;
		if (exclude != null && exclude.Length != 0)
		{
			List<Point> list = startingTiles.Select((CTile x) => x.m_ArrayIndex).Except(exclude).ToList();
			return list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
		}
		return startingTiles[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(startingTiles.Count)].m_ArrayIndex;
	}

	public static void SpawnChest(CObjectChest.EType type, Point pos)
	{
		if (!ScenarioManager.CurrentScenarioState.ScenarioSetup.AlreadyLootedChests.Contains(type.ToString()))
		{
			CTile tile = ScenarioManager.GetTile(pos);
			CObjectChest cObjectChest = new CObjectChest(ScenarioManager.ObjectImportType.Chest.ToString(), ScenarioManager.ObjectImportType.Chest, new TileIndex(pos), null, null, null, tile.m_HexMap.MapGuid);
			cObjectChest.Type = type;
			cObjectChest.DamageValue = 0;
			cObjectChest.Conditions?.Clear();
			cObjectChest.SetLocation(new TileIndex(pos), null, null);
			tile.SpawnProp(cObjectChest);
		}
	}

	public static void SpawnStatue()
	{
		CTile closestEmptyTile = ScenarioScript.GetClosestEmptyTile((ScenarioScript.current as ScenarioScript_FH102).WalkableTiles[14]);
		SpawnObstacle(ESpecificPropType.StatueOfTheBuilder.ToString(), closestEmptyTile);
		CAbility cAbility = CAbilityHeal.CreateDefaultHeal(2, 1, 1, isMonster: false);
		cAbility.AddPositiveCondition(CCondition.EPositiveCondition.Bless);
		cAbility.AddPositiveCondition(CCondition.EPositiveCondition.Bless);
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			cAbility.Start(playerActor, playerActor);
			cAbility.ApplyToActor(playerActor);
			playerActor.StartLooting();
			playerActor.GainLootMoney(5);
			playerActor.ShowLastLootReward("", "MoneyToken", null, focusCamera: false, outOfPhase: true);
		}
	}

	public static void SpawnObstacle(string prefab, CTile tile)
	{
		TileIndex tileIndex = new TileIndex(tile.m_ArrayIndex);
		List<TileIndex> pathingBlockers = new List<TileIndex> { tileIndex };
		CObjectObstacle cObjectObstacle = new CObjectObstacle(prefab, ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, null, pathingBlockers, null, tile.m_HexMap.MapGuid, ignoresFlyAndJump: false);
		cObjectObstacle.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
		cObjectObstacle.PropCustomLocKey = "FH_102_Hand_of_the_Builder_T";
		cObjectObstacle.PropCustomDescriptionLocKey = "FH_102_Hand_of_the_Builder_D";
		tile.SpawnProp(cObjectObstacle);
	}

	public static void ToggleTiles(List<CTile> tiles, bool active)
	{
		foreach (CTile tile in tiles)
		{
			ToggleTile(tile, active);
		}
	}

	public static bool IsTileActive(CTile tile)
	{
		return ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Walkable;
	}

	private static bool IsTileOnWallEdge(CTile tile)
	{
		if (tile == null)
		{
			return false;
		}
		if (tile.m_ArrayIndex.X <= 1)
		{
			return true;
		}
		if (tile.m_ArrayIndex.X == 17 && tile.m_ArrayIndex.Y % 2 == 0)
		{
			return true;
		}
		if (tile.m_ArrayIndex.X == 16 && tile.m_ArrayIndex.Y % 2 != 0)
		{
			return true;
		}
		if (tile.m_ArrayIndex.Y == 0 || tile.m_ArrayIndex.Y == 16)
		{
			return true;
		}
		return false;
	}

	public static List<CTile> GetValidTilesToToggle(CActor actor)
	{
		_ = ScenarioScript.current;
		List<CTile> tilesInRange = GameState.GetTilesInRange(actor.ArrayIndex, 3);
		for (int num = tilesInRange.Count - 1; num >= 0; num--)
		{
			CTile tile = tilesInRange[num];
			bool flag = !IsTileActive(tile) && !IsTileOnWallEdge(tile);
			if (flag)
			{
				flag = GameState.GetTilesInRange(tile.m_ArrayIndex, 1).Any((CTile x) => x != tile && IsTileActive(x));
			}
			if (!flag)
			{
				tilesInRange.RemoveAt(num);
			}
		}
		return tilesInRange;
	}

	public override bool OverrideHasLOS(CTile sourceTile, CTile targetTile)
	{
		return true;
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		if (!IsTileActive(tile))
		{
			return true;
		}
		return base.TreatTileAsNonFeatureless(tile);
	}
}

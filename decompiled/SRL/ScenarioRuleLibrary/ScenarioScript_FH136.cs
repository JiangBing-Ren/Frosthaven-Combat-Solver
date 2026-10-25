using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH136 : ScenarioScript
{
	[Serializable]
	public class Storage136
	{
		public int MovingPlatformGoalIndex;

		public bool movedBoatA;

		public bool movedBoatB;
	}

	private const int boatMoveCount = 2;

	private CObjectProp boatA;

	private CObjectProp boatB;

	private CTile[] boatsGoalTiles;

	public List<CTile> nonWaterTiles = new List<CTile>();

	private Storage136 serializedProperties;

	private int MovingPlatformGoalIndex
	{
		get
		{
			return serializedProperties.MovingPlatformGoalIndex;
		}
		set
		{
			serializedProperties.MovingPlatformGoalIndex = value;
		}
	}

	private bool MovedBoatA
	{
		get
		{
			return serializedProperties.movedBoatA;
		}
		set
		{
			serializedProperties.movedBoatA = value;
		}
	}

	private bool MovedBoatB
	{
		get
		{
			return serializedProperties.movedBoatB;
		}
		set
		{
			serializedProperties.movedBoatB = value;
		}
	}

	private CTile MovingPlatformGoal => boatsGoalTiles[MovingPlatformGoalIndex];

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			UpdateGoalHighlights();
			return;
		}
		serializedProperties = (Storage136)JsonConvert.DeserializeObject(storage, typeof(Storage136));
		foreach (CTile item in from x in boatA.GetTiles()
			select ScenarioManager.GetTile(x))
		{
			item.AddProp(boatA);
		}
		foreach (CTile item2 in from x in boatB.GetTiles()
			select ScenarioManager.GetTile(x))
		{
			item2.AddProp(boatB);
		}
		UpdatePathFinderNodes(null);
		UpdateGoalHighlights();
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage136();
		boatA = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp x) => x.SpecificPropType == ESpecificPropType.MovingBoat136);
		boatB = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp x) => x.SpecificPropType == ESpecificPropType.MovingBoat136B);
		if (boatA?.PropTile != null)
		{
			boatA.PropCustomLocKey = "FH_136_River_Boat_T";
			boatA.PropCustomDescriptionLocKey = "FH_136_River_Boat_D";
			boatA.PropTile.AddProp(boatA, recursiveTiles: true);
		}
		if (boatB?.PropTile != null)
		{
			boatB.PropCustomLocKey = "FH_136_River_Boat_T";
			boatB.PropCustomDescriptionLocKey = "FH_136_River_Boat_D";
			boatB.PropTile.AddProp(boatB, recursiveTiles: true);
		}
		boatsGoalTiles = new CTile[2]
		{
			ScenarioManager.GetTile(9, 5),
			ScenarioManager.GetTile(16, 10)
		};
		nonWaterTiles.AddRange(ScenarioManager.StartingTiles);
		nonWaterTiles.Add(ScenarioManager.Tiles[17, 8]);
		nonWaterTiles.Add(ScenarioManager.Tiles[16, 7]);
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			if (OncePerRound())
			{
				if (MovedBoatA)
				{
					AutoMoveBoat(boatA, 2);
				}
				if (MovedBoatB)
				{
					AutoMoveBoat(boatB, 2);
				}
			}
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	private CObjectProp GetBoat(Point point)
	{
		if (boatA.GetTiles().Any((Point t) => t == point))
		{
			return boatA;
		}
		if (boatB.GetTiles().Any((Point t) => t == point))
		{
			return boatB;
		}
		return null;
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		UpdatePathFinderNodes(_tickActor);
	}

	protected override void TickAbilityEndedInternal()
	{
		CheckObjectivesComplete();
	}

	private void UpdatePathFinderNodes(CActor actor)
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && cTile.m_HexMap.Revealed)
			{
				CNode node = ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex);
				bool flag = (node.IsWaterTerrain = IsWaterTile(cTile));
				node.IsDifficultTerrain = flag || cTile.FindPropOfTypes(EPropType.TerrainRubble) != null;
				cTile.TreatAsWater = flag;
			}
		}
	}

	public bool IsWaterTile(CTile tile)
	{
		if (boatA.GetTiles().Contains(tile.m_ArrayIndex))
		{
			return false;
		}
		if (boatB.GetTiles().Contains(tile.m_ArrayIndex))
		{
			return false;
		}
		if (nonWaterTiles.Contains(tile))
		{
			return false;
		}
		if (tile.m_HexMap.RoomName == "Room_23")
		{
			return false;
		}
		return true;
	}

	public override bool IsUnderwater(CTile tile = null)
	{
		if (tile == null)
		{
			return false;
		}
		return ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).IsWaterTerrain;
	}

	private CTile GetAutomaticBoatValidMoveTile(CObjectProp boat)
	{
		_ = boat.PropTile;
		CTile goal = MovingPlatformGoal;
		List<Point> tiles = boat.GetTiles();
		CObjectProp cObjectProp = ((boat == boatA) ? boatB : boatA);
		if (tiles.Contains(goal.m_ArrayIndex))
		{
			return null;
		}
		List<CTile> list = new List<CTile>();
		List<Point> list2 = new List<Point>();
		list2.AddRange(cObjectProp.GetTiles());
		list2.AddRange(nonWaterTiles.Select((CTile t) => t.m_ArrayIndex));
		foreach (Point item in tiles)
		{
			List<Point> list3 = list2.ToList();
			CTile tile = ScenarioManager.GetTile(item);
			if (tile.LivingActor != null)
			{
				foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
				{
					if (!tiles.Contains(allAliveActor.ArrayIndex))
					{
						list3.Add(allAliveActor.ArrayIndex);
					}
				}
			}
			List<Point> list4 = ScenarioManager.PathFinder.FindPath(null, item, goal.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath, ignoreDoorLocks: true, out var context, list3);
			if (!foundPath)
			{
				for (int num = 0; num < list4.Count; num++)
				{
					Point pt = list4[num];
					CPathFinderContext.TNode node = context.GetNode(pt);
					if (!node.HasFlag(CPathFinderContext.TNode.Flags.Reachable))
					{
						list4.RemoveRange(num, list4.Count - num);
						break;
					}
				}
			}
			if (list4.Count != 0)
			{
				CTile cTile = ScenarioManager.Tiles[list4[0].X, list4[0].Y];
				if (!list.Contains(cTile) && !tiles.Contains(cTile.m_ArrayIndex) && CanMoveBoatOnTile(tile, cTile) && cTile.LivingActor == null)
				{
					list.Add(cTile);
				}
			}
		}
		CTile cTile2 = list.OrderBy((CTile x) => Point.HexDist(x.m_ArrayIndex, goal.m_ArrayIndex)).FirstOrDefault();
		if (cTile2 == null)
		{
			return null;
		}
		return cTile2;
	}

	private void AutoMoveBoat(CObjectProp boat, int remainingMoveCount)
	{
		if (remainingMoveCount <= 0 || ScenarioManager.Scenario.PlayerActors.FirstOrDefault() == null)
		{
			return;
		}
		CTile automaticBoatValidMoveTile = GetAutomaticBoatValidMoveTile(boat);
		if (automaticBoatValidMoveTile != null)
		{
			List<Point> tiles = boat.GetTiles();
			CTile cTile = ScenarioManager.Tiles[tiles[0].X, tiles[0].Y];
			CTile cTile2 = ScenarioManager.Tiles[tiles[1].X, tiles[1].Y];
			int distanceTo = CActor.GetDistanceTo(cTile.m_ArrayIndex, automaticBoatValidMoveTile.m_ArrayIndex, viaPathfinding: true);
			CTile cTile3 = ((CActor.GetDistanceTo(cTile2.m_ArrayIndex, automaticBoatValidMoveTile.m_ArrayIndex, viaPathfinding: true) >= distanceTo) ? cTile : cTile2);
			if (cTile3 != automaticBoatValidMoveTile)
			{
				MovePlatformCallback(boat, cTile3, automaticBoatValidMoveTile);
				AutoMoveBoat(boat, remainingMoveCount - 1);
			}
		}
	}

	public static void MoveBoatOnPath(CAbility ability)
	{
		if (ability is CAbilityNullHex { AbilityHasHappened: not false } cAbilityNullHex)
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			ScenarioScript_FH136 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH136;
			CObjectProp boat = scenarioScript_FH.GetBoat(ability.TargetingActor.ArrayIndex);
			List<CTile> tilesSelected = cAbilityNullHex.TilesSelected;
			for (int i = 0; i < tilesSelected.Count; i++)
			{
				scenarioScript_FH.MovePlatformCallback(boat, ability.TargetingActor, tilesSelected[i]);
			}
			if (ability.NumberTargetsRemaining > 0)
			{
				CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
				cAbility.StatIsBasedOnXEntries.Clear();
				cAbility.NumberTargets = ability.NumberTargetsRemaining;
				PhaseManager.StartAbilities(cAbility, ability.TargetingActor, stackInsteadOfInject: true);
			}
		}
	}

	public static bool IsOnBoat(CActor actor)
	{
		return (ScenarioScript.current as ScenarioScript_FH136).GetBoat(actor.ArrayIndex) != null;
	}

	public static bool CanMoveBoatOnTile(CTile tile)
	{
		CActor cActor = (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability)?.TargetingActor;
		ScenarioScript_FH136 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH136;
		CTile tile2 = cActor.Tile;
		if (tile.LivingActor != null && scenarioScript_FH.GetBoat(tile.m_ArrayIndex) != scenarioScript_FH.GetBoat(tile2.m_ArrayIndex) && !CActor.AreActorsAllied(cActor.Type, tile.LivingActor.Type, cActor, tile.LivingActor))
		{
			return false;
		}
		return CanMoveBoatOnTile(tile2, tile);
	}

	public static bool CanMoveBoatOnTile(CTile from, CTile tile)
	{
		if (tile.IsBlocked)
		{
			return false;
		}
		if (!tile.IsWalkable)
		{
			return false;
		}
		ScenarioScript_FH136 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH136;
		if (((scenarioScript_FH.GetBoat(from.m_ArrayIndex) == scenarioScript_FH.boatA) ? scenarioScript_FH.boatB : scenarioScript_FH.boatA).GetTiles().Contains(tile.m_ArrayIndex))
		{
			return false;
		}
		return true;
	}

	public static bool CanConfirmBoatMovement(CAbility ability)
	{
		if (ability.TilesSelected.Count == 0)
		{
			return true;
		}
		foreach (CTile item in ability.TilesSelected.TakeLast(2))
		{
			CActor livingActor = item.LivingActor;
			ScenarioScript_FH136 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH136;
			if (livingActor != null && livingActor != ability.TargetingActor && scenarioScript_FH.GetBoat(livingActor.ArrayIndex) != scenarioScript_FH.GetBoat(ability.TargetingActor.ArrayIndex))
			{
				return false;
			}
		}
		return true;
	}

	protected override void MovePlatformCallback(CObjectProp boat, CTile tileToMoveFrom, CTile tileToMoveTo, CActor movingActor = null)
	{
		base.MovePlatformCallback(boat, tileToMoveFrom, tileToMoveTo, movingActor);
		if (boat.GetTiles().Contains(MovingPlatformGoal.m_ArrayIndex) && MovingPlatformGoalIndex < boatsGoalTiles.Length - 1)
		{
			MovingPlatformGoalIndex++;
		}
		if (boat == boatA)
		{
			MovedBoatA = true;
		}
		if (boat == boatB)
		{
			MovedBoatB = true;
		}
		UpdatePathFinderNodes(_tickActor);
		UpdateGoalHighlights();
	}

	private void UpdateGoalHighlights()
	{
		CObjectDoor cObjectDoor = ScenarioScript.FindDoor(9, 5);
		ScenarioManager.ToggleTileHighlights(new Point[1] { cObjectDoor.ArrayIndex.ToPoint() }, !cObjectDoor.DoorIsOpen, new CVector4(1f, 1f, 0f, 1f), standingHighlights: true);
		CMap cMap = ScenarioScript.FindRoom("Room_23");
		for (int i = 0; i < boatsGoalTiles.Length; i++)
		{
			CTile cTile = boatsGoalTiles[i];
			ScenarioManager.ToggleTileHighlights(new Point[1] { cTile.m_ArrayIndex }, cTile.m_HexMap.Revealed && MovingPlatformGoalIndex == i && !cMap.Revealed, new CVector4(1f, 1f, 0f, 1f), standingHighlights: true);
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		UpdateGoalHighlights();
		UpdatePathFinderNodes(null);
	}
}

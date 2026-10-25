using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH134 : ScenarioScript
{
	private List<CActor> actorsMovedEndRound = new List<CActor>();

	private CMap oddRoom1;

	private CMap oddRoom2;

	private CMap evenRoom1;

	private CMap evenRoom2;

	private List<CObjectPressurePlate> evenRoundPlates = new List<CObjectPressurePlate>();

	private List<CObjectPressurePlate> oddRoundPlates = new List<CObjectPressurePlate>();

	private int curEvenRoundActivatedPressurePlates;

	private int curOddRoundActivatedPressurePlates;

	private const string SteelAutomaton = "SteelAutomaton";

	private const string ShrikeFiend = "ShrikeFiend";

	private List<Point> barrelTiles = new List<Point>
	{
		new Point(5, 9),
		new Point(10, 8),
		new Point(5, 13),
		new Point(6, 15),
		new Point(8, 15),
		new Point(10, 15),
		new Point(8, 7)
	};

	private List<Point> nestTiles = new List<Point>
	{
		new Point(3, 20),
		new Point(9, 3),
		new Point(4, 2),
		new Point(10, 19)
	};

	private List<(Point, Point)> tileLinks = new List<(Point, Point)>
	{
		(new Point(6, 20), new Point(10, 20)),
		(new Point(5, 19), new Point(9, 19)),
		(new Point(5, 3), new Point(9, 3)),
		(new Point(5, 2), new Point(9, 2))
	};

	private bool tempSpawning;

	private CSpawner AutomatonSpawner => ScenarioManager.CurrentScenarioState.Spawners.SingleOrDefault((CSpawner x) => x.ArrayIndex.X == 7 && x.ArrayIndex.Y == 11);

	private IEnumerable<CSpawner> ShrikeFiendSpawners => ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner x) => x != AutomatonSpawner);

	protected override bool Initialize()
	{
		oddRoom1 = ScenarioScript.FindRoom("Room_3");
		oddRoom2 = ScenarioScript.FindRoom("Room_4");
		evenRoom1 = ScenarioScript.FindRoom("Room_5");
		evenRoom2 = ScenarioScript.FindRoom("Room_6");
		evenRoundPlates.Add(ScenarioManager.GetTile(2, 20).FindPropOfType<CObjectPressurePlate>());
		evenRoundPlates.Add(ScenarioManager.GetTile(13, 20).FindPropOfType<CObjectPressurePlate>());
		oddRoundPlates.Add(ScenarioManager.GetTile(2, 2).FindPropOfType<CObjectPressurePlate>());
		oddRoundPlates.Add(ScenarioManager.GetTile(13, 2).FindPropOfType<CObjectPressurePlate>());
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		ToggleBarrelPathfinding(enabled: false);
		ToggleNestPathfinding(enabled: false);
		UpdateTeleportLinks();
		CheckPressurePlates(spawn: false);
		CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllAliveMonsters.SingleOrDefault((CEnemyActor x) => x.Class.ID.Contains("SteelAutomaton"));
		if (cEnemyActor != null)
		{
			cEnemyActor.NoGoldDrop = true;
		}
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			actorsMovedEndRound.Clear();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		}
	}

	private void UpdateTeleportLinks()
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		foreach (var tileLink in tileLinks)
		{
			var (point, point2) = tileLink;
			if (ScenarioManager.GetTile(point).m_HexMap.Revealed && ScenarioManager.GetTile(point2).m_HexMap.Revealed)
			{
				CNode nodeA = ScenarioManager.PathFinder.GetNode(point);
				CNode nodeB = ScenarioManager.PathFinder.GetNode(point2);
				if (!nodeA.NavLinks.Any((CNavLink x) => x.DestinationNode == nodeB))
				{
					CNavLink item = new CNavLink(2, nodeB, blocked: true);
					nodeA.NavLinks.Add(item);
				}
				if (!nodeB.NavLinks.Any((CNavLink x) => x.DestinationNode == nodeA))
				{
					CNavLink item2 = new CNavLink(2, nodeA, blocked: true);
					nodeB.NavLinks.Add(item2);
				}
				list.Add(tileLink);
			}
		}
		ScenarioManager.ToggleTileHighlights(list.Select(((Point, Point) x) => x.Item1).Concat(list.Select(((Point, Point) x) => x.Item2)), enabled: true);
		ScenarioManager.ToggleTileLinks(list, enabled: true);
	}

	private void CheckPressurePlates(bool spawn = true)
	{
		int num = evenRoundPlates.Count((CObjectPressurePlate x) => x.Activated);
		int num2 = oddRoundPlates.Count((CObjectPressurePlate x) => x.Activated);
		CSpawner cSpawner = null;
		if (spawn)
		{
			if (num != curEvenRoundActivatedPressurePlates)
			{
				cSpawner = ShrikeFiendSpawners.OrderBy((CSpawner x) => _tickActor.GetDistanceTo(x.ArrayIndex.ToPoint())).First();
			}
			if (num2 != curOddRoundActivatedPressurePlates)
			{
				cSpawner = ShrikeFiendSpawners.OrderBy((CSpawner x) => _tickActor.GetDistanceTo(x.ArrayIndex.ToPoint())).First();
			}
		}
		curEvenRoundActivatedPressurePlates = num;
		curOddRoundActivatedPressurePlates = num2;
		if (cSpawner == null)
		{
			return;
		}
		tempSpawning = true;
		ToggleNestPathfinding(enabled: true);
		cSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		ToggleNestPathfinding(enabled: false);
		tempSpawning = false;
		CActor cActor = ScenarioManager.GetTile(cSpawner.ArrayIndex)?.LivingActor;
		if (cActor != null && cActor.Class.ID.Contains("SteelAutomaton"))
		{
			cActor.NoGoldDrop = true;
		}
		if (num != 2 || num2 != 2)
		{
			return;
		}
		GameState.TriggerCampaignSection("FS_134_Tower_Of_Knowledge_Special_1_1");
		cSpawner = AutomatonSpawner;
		ScenarioManager.GetTile(7, 11).FindObstacleProp()?.DestroyProp();
		if (cSpawner != null)
		{
			tempSpawning = true;
			cSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			tempSpawning = false;
			cActor = ScenarioManager.GetTile(cSpawner.ArrayIndex)?.LivingActor;
			if (cActor != null && cActor.Class.ID.Contains("SteelAutomaton"))
			{
				cActor.NoGoldDrop = true;
			}
		}
	}

	public override bool TreatTileAsFeatureless(CTile tile)
	{
		if (tempSpawning && nestTiles.Contains(tile.m_ArrayIndex) && tile.FindObstacleProp() != null)
		{
			return true;
		}
		return base.TreatTileAsFeatureless(tile);
	}

	public override bool AllowPathfindingThroughTileForEnemy(CTile tile, CEnemyActor actor)
	{
		if (actor != null && actor.Class.ID.Contains("ShrikeFiend") && nestTiles.Contains(tile.m_ArrayIndex))
		{
			return true;
		}
		return base.AllowPathfindingThroughTileForEnemy(tile, actor);
	}

	private void ToggleNestPathfinding(bool enabled)
	{
		foreach (Point nestTile in nestTiles)
		{
			if (ScenarioManager.GetTile(nestTile).FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
			{
				ScenarioManager.PathFinder.GetNode(nestTile).Blocked = !enabled;
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		CheckPressurePlates();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		UpdateTeleportLinks();
		if (_tickRoomRevealed == oddRoom1 && !oddRoom2.Revealed)
		{
			oddRoom2.Reveal(initial: true);
		}
		if (_tickRoomRevealed == oddRoom2 && !oddRoom1.Revealed)
		{
			oddRoom1.Reveal(initial: true);
		}
		if (_tickRoomRevealed == evenRoom1 && !evenRoom2.Revealed)
		{
			evenRoom2.Reveal(initial: true);
		}
		if (_tickRoomRevealed == evenRoom2 && !evenRoom1.Revealed)
		{
			evenRoom1.Reveal(initial: true);
		}
	}

	private void TickEndRound()
	{
		ToggleBarrelPathfinding(enabled: true);
		TryPushEndRoundActor(_tickActor, actorsMovedEndRound);
		ToggleBarrelPathfinding(enabled: false);
		CheckBarrelDeath();
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		ToggleBarrelPathfinding(_tickAbility is CAbilityPull || _tickAbility is CAbilityPush || (_tickAbility is CAbilityMove && _tickAbility.ControllingActor != null));
		ToggleNestPathfinding(_tickAbility is CAbilityMove && _tickAbility.TargetingActor.Class.ID.Contains("ShrikeFiend") && _tickAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		CAbility tickAbility = _tickAbility;
		if (tickAbility is CAbilityPull || tickAbility is CAbilityPush || (tickAbility is CAbilityMove && _tickAbility.ControllingActor != null))
		{
			CheckBarrelDeath();
		}
		if (tickAbility is CAbilityMove { Jump: not false })
		{
			CheckPressurePlates();
		}
		ToggleBarrelPathfinding(enabled: false);
		ToggleNestPathfinding(enabled: false);
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		ToggleBarrelPathfinding(enabled: false);
		ToggleNestPathfinding(enabled: false);
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor.Class.ID.Contains("SteelAutomaton"))
		{
			CTile tile = _tickActor.Tile;
			CObjectChest cObjectChest = new CObjectChest(ScenarioManager.ObjectImportType.GoalChest.ToString(), ScenarioManager.ObjectImportType.GoalChest, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
			cObjectChest.PropCustomLocKey = "FH/FH_Scenario_Campaign_134_SCENARIO_OBJECT_1";
			tile.SpawnProp(cObjectChest);
		}
	}

	private void CheckBarrelDeath()
	{
		foreach (Point barrelTile in barrelTiles)
		{
			CTile tile = ScenarioManager.GetTile(barrelTile);
			CObjectProp cObjectProp = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle);
			if (cObjectProp != null && tile.LivingActors.Count() > 1)
			{
				GameState.KillActor(cObjectProp.RuntimeAttachedActor, cObjectProp.RuntimeAttachedActor, CActor.ECauseOfDeath.Undetermined, out var _);
			}
		}
	}

	private void ToggleBarrelPathfinding(bool enabled)
	{
		foreach (Point barrelTile in barrelTiles)
		{
			CObjectProp cObjectProp = ScenarioManager.GetTile(barrelTile).FindProp(ScenarioManager.ObjectImportType.Obstacle);
			if (cObjectProp != null)
			{
				if (cObjectProp.RuntimeAttachedActor != null)
				{
					cObjectProp.RuntimeAttachedActor.SetIgnoreActorCollision(enabled);
				}
				ScenarioManager.PathFinder.GetNode(barrelTile).Blocked = !enabled;
			}
		}
	}

	private bool TryPushEndRoundActor(CActor actor, List<CActor> actorsMoved)
	{
		if (actorsMoved.Contains(actor))
		{
			return false;
		}
		if (!(actor is CPlayerActor) && !(actor is CHeroSummonActor))
		{
			return false;
		}
		CTile cTile = null;
		CNode cNode = null;
		if (ScenarioScript.RoundNumber % 2 == 0 && curEvenRoundActivatedPressurePlates < 2 && actor.Tile.m_HexMap != evenRoom1 && actor.Tile.m_HexMap != evenRoom2)
		{
			cTile = ScenarioManager.GetAdjacentTile(actor.Tile.m_ArrayIndex.X, actor.Tile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.EBottomLeft);
			cNode = ((cTile == null) ? null : ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex));
			if (cTile == null || cNode.Blocked || !cNode.Walkable || (cTile.LivingActor != null && !(cTile.LivingActor is CObjectActor)))
			{
				cTile = ScenarioManager.GetAdjacentTile(actor.Tile.m_ArrayIndex.X, actor.Tile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.EBottomRight);
			}
		}
		else if (ScenarioScript.RoundNumber % 2 != 0 && curOddRoundActivatedPressurePlates < 2 && actor.Tile.m_HexMap != oddRoom1 && actor.Tile.m_HexMap != oddRoom2)
		{
			cTile = ScenarioManager.GetAdjacentTile(actor.Tile.m_ArrayIndex.X, actor.Tile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ETopRight);
			cNode = ((cTile == null) ? null : ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex));
			if (cTile == null || cNode.Blocked || !cNode.Walkable || (cTile.LivingActor != null && !(cTile.LivingActor is CObjectActor)))
			{
				cTile = ScenarioManager.GetAdjacentTile(actor.Tile.m_ArrayIndex.X, actor.Tile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ETopLeft);
			}
		}
		cNode = ((cTile == null) ? null : ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex));
		if (cTile == null || cNode.Blocked || !cNode.Walkable)
		{
			return false;
		}
		if (cTile.LivingActor != null && !(cTile.LivingActor is CObjectActor) && !TryPushEndRoundActor(cTile.LivingActor, actorsMoved))
		{
			return false;
		}
		actorsMoved.Add(actor);
		CustomAbility.MoveActorInDirection(actor, cTile);
		return true;
	}
}

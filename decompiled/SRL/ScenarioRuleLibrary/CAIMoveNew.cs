using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAIMoveNew
{
	private class CEnemyEval
	{
		public Point position;

		public CActor actor;

		public int hexDist;

		public bool isFocusOverride;

		public List<CAttackTileEval> attackTiles;

		public CAttackTileEval lowestCostAttackTile;

		public CAttackTileEval bestAttackTile;

		public List<TPossibleAoEPlacement> possibleAoEPlacements;

		public int lowestMoveCostToAttack
		{
			get
			{
				if (lowestCostAttackTile != null)
				{
					return lowestCostAttackTile.moveCost.movePoints;
				}
				return int.MaxValue;
			}
		}

		public bool isAttackableThisTurn
		{
			get
			{
				if (lowestCostAttackTile != null)
				{
					return lowestCostAttackTile.reachable;
				}
				return false;
			}
		}

		public override string ToString()
		{
			return $"{TValue.ToString(actor)} at {position}";
		}
	}

	private class CAttackTileEval
	{
		public CEnemyEval enemyEval;

		public Point position;

		public CPathFinderContext.TNode.TCost moveCost;

		public bool reachable;

		public int distToTarget;

		public bool evaluated;

		public TAttackEval attackEval;

		public TPossibleAoEPlacement AoEPlacement;

		public override string ToString()
		{
			string text = $"{position}: move cost: {moveCost}";
			if (evaluated)
			{
				text += $", {attackEval}";
			}
			return text;
		}
	}

	private struct TAttackEval
	{
		public int numTargets;

		public int numDisadvantages;

		public List<TAdditionalTargetEval> additionalTargets;

		public override string ToString()
		{
			return $"targets: {numTargets}, disadvantages: {numDisadvantages}";
		}
	}

	private struct TAdditionalTargetEval
	{
		public CActor target;

		public int priority;

		public bool disadvantage;

		public override string ToString()
		{
			return $"{target}: priority {priority}, disadvantage: {disadvantage}";
		}
	}

	private struct TPossibleAoEPlacement
	{
		public Point targetPoint;

		public float angle;

		public bool flipped;

		public List<Point> hexes;

		public int distToFocus;

		public int distToAnyTarget;

		public override string ToString()
		{
			string text = ((hexes == null) ? "null" : hexes.Count.ToString());
			return $"{targetPoint}: angle: {angle}, flip: {flipped}, hexes: {text}";
		}
	}

	public class CActorPath
	{
		public CActor m_Actor;

		public CTile m_Tile;

		public List<Point> m_ArrayIndices;

		public List<CTile> m_Waypoints;
	}

	private struct TReachableTileEval
	{
		public Point position;

		public CPathFinderContext.TNode.TCost cost;

		public int distToFocus;

		public List<Point> path;

		public override string ToString()
		{
			return $"{position}: {cost}";
		}
	}

	private CActor actor;

	private int maxMoveCount;

	private bool jump;

	private bool fly;

	private bool jumpingTraps;

	private bool jumpingHazards;

	private bool ignoreDifficultTerrain;

	private bool ignoreHazardousTerrain;

	private int waterTilesThatCostZero;

	private bool isWaterMonster;

	private bool isPlayer;

	private int range;

	private bool allowMove;

	private CAbilityAttack attack;

	private bool moveTest;

	private bool ignoreDoorLocks;

	private CAbilityAttack thisAttack;

	private bool? canTargetInvisible;

	private int AoERadius;

	private int addRangeAoE;

	private int numTargets;

	private CActor focusOverrideActor;

	private Point focusOverridePoint;

	private bool shouldPathThroughDoors;

	private bool disallowDoorways;

	private bool focusBenign;

	private bool useFurthestFocus;

	private bool useSummonerToMeasureFrom;

	private bool? targetATile;

	private int overrideMoveRange = -1;

	private CAIFocusOverrideDetails.EOverrideType focusOverrideType;

	private CPathFinderContext pfContext;

	public List<Point> ReachablePoints = new List<Point>();

	private List<CEnemyEval> enemyEvals = new List<CEnemyEval>();

	private CEnemyEval finalFocusEval;

	[ConsoleVariable(Alias = "debug_ai_move", Persistent = false)]
	public static bool DebugAIMove = false;

	private static List<Point> tmp_points = new List<Point>(128);

	private static List<Point> tmp_points2 = new List<Point>(128);

	private static List<Point> tmp_points3 = new List<Point>(128);

	private List<TReachableTileEval> reachableTiles;

	public IEnumerable<CActor> ActorsCanAttackWithThisCalc()
	{
		foreach (CEnemyEval enemyEval in enemyEvals)
		{
			if (enemyEval.actor != null && enemyEval.isAttackableThisTurn)
			{
				yield return enemyEval.actor;
			}
		}
	}

	public CAIMoveNew(CActor actor, int maxMoveCount, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, bool isWaterMonster, bool isPlayer, int range, bool allowMove, CAbilityAttack attack, bool moveTest)
	{
		this.actor = actor;
		this.maxMoveCount = maxMoveCount;
		this.jump = jump;
		this.fly = fly;
		jumpingTraps = jump && !fly;
		jumpingHazards = jump && !fly;
		this.ignoreDifficultTerrain = ignoreDifficultTerrain;
		this.ignoreHazardousTerrain = ignoreHazardousTerrain;
		this.waterTilesThatCostZero = waterTilesThatCostZero;
		this.isWaterMonster = isWaterMonster;
		this.isPlayer = isPlayer;
		this.range = range;
		this.allowMove = allowMove;
		this.attack = attack;
		this.moveTest = moveTest;
	}

	public void Process()
	{
		if (DebugAIMove)
		{
			CExpression.Globals.breakpoint();
		}
		ResolveFocusOverride();
		ResolveAttackAbility();
		InitPathfinderContext();
		AddActorsToPathFinderAndEnemyEvals();
		AddLevelObjectsToPathFindingContext(pfContext, actor, overrideMoveRange, focusOverridePoint, fly, disallowDoorways, shouldPathThroughDoors, isWaterMonster, jump);
		pfContext.FloodFill();
		if (!AddFocusOverrideToEnemyEvals())
		{
			return;
		}
		CalculateValidAttackTilesForAllEnemies();
		SortEnemyEvals();
		if (enemyEvals.Count != 0)
		{
			IEnumerable<CEnemyEval> source = enemyEvals.Where((CEnemyEval x) => x.lowestCostAttackTile != null);
			finalFocusEval = source.FirstOrDefault();
			if (finalFocusEval?.lowestCostAttackTile != null)
			{
				FindBestAttackPositionForTheFocus();
				FindPathToFocus();
				CalcWaypoints();
				CalcMoveFocusActors();
				SetAoEAttackAbilityParameters();
			}
		}
	}

	public List<Point> GetFinalFocusBestAttackTiles(bool reachable = true)
	{
		return (from tile in finalFocusEval.attackTiles
			where tile.moveCost.CompareTo(finalFocusEval.bestAttackTile.moveCost) == 0 && tile.reachable == reachable
			select tile.position).ToList();
	}

	private void ResolveFocusOverride()
	{
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
		if (actorState != null && actorState.AIFocusOverride != null && actorState.AIFocusOverride.OverrideType != CAIFocusOverrideDetails.EOverrideType.None)
		{
			focusOverrideType = actorState.AIFocusOverride.ResolveOverrideTargetForCurrentScenario(actor, out focusOverrideActor, out focusOverridePoint, out shouldPathThroughDoors, out disallowDoorways, out focusBenign, out useFurthestFocus, out targetATile, out overrideMoveRange, out useSummonerToMeasureFrom, out ignoreHazardousTerrain);
			if (actorState.AIFocusOverride.OverrideTargetType == CAIFocusOverrideDetails.EOverrideTargetType.Prop && actorState.AIFocusOverride.TargetPropType != EPropType.None && focusOverridePoint == default(Point))
			{
				focusOverrideType = CAIFocusOverrideDetails.EOverrideType.None;
			}
			if (overrideMoveRange >= 0)
			{
				range = overrideMoveRange;
			}
		}
	}

	public static void GetPossibleEmptyTiles(CActor actor, List<CTile> tilesInRange, out List<CTile> failTile, out List<string> failReason, out List<CTile> possibleTiles, MonsterYMLData SelectedMonsterYMLData = null, bool IsMonsterSummon = false)
	{
		failTile = new List<CTile>();
		failReason = new List<string>();
		possibleTiles = new List<CTile>();
		GetPossibleEmptyTiles(actor, tilesInRange, failTile, failReason, possibleTiles, SelectedMonsterYMLData, IsMonsterSummon);
	}

	public static void GetPossibleEmptyTiles(CActor actor, List<CTile> tilesInRange, List<CTile> failTile, List<string> failReason, List<CTile> possibleTiles, MonsterYMLData SelectedMonsterYMLData = null, bool IsMonsterSummon = false)
	{
		GetPossibleEmptyTiles(actor, actor.Tile, tilesInRange, failTile, failReason, possibleTiles, SelectedMonsterYMLData, IsMonsterSummon);
	}

	public static void GetPossibleEmptyTiles(CActor actor, CTile actorTile, List<CTile> tilesInRange, List<CTile> failTile, List<string> failReason, List<CTile> possibleTiles, MonsterYMLData SelectedMonsterYMLData = null, bool IsMonsterSummon = false)
	{
		CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour(actor);
		actorTile.FindProp(ScenarioManager.ObjectImportType.Door);
		CMonsterClass cMonsterClass = ((SelectedMonsterYMLData == null) ? null : MonsterClassManager.Classes.Find((CMonsterClass c) => c.ID == SelectedMonsterYMLData.ID));
		bool flag = IsMonsterSummon && (cMonsterClass?.IsWaterMonster ?? false);
		foreach (CTile item in tilesInRange)
		{
			if (item != null)
			{
				CTile cTile = mirrorAbilityBehaviour?.FindMirrorTile(item);
				bool num = ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex) != null || cTile?.LivingActor != null;
				CNode cNode = ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				CNode cNode2 = ((cTile == null) ? null : ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y]);
				bool flag2 = item.IsWater || ((ScenarioScript.current != null && ScenarioScript.current.IsUnderwater(item)) & flag);
				bool flag3 = cNode.Walkable && !cNode.Blocked && (cNode2 == null || (cNode2.Walkable && !cNode2.Blocked));
				bool flag4 = num;
				bool revealed = item.m_HexMap.Revealed;
				bool flag5 = item.FindDoor()?.DoorIsClosed ?? false;
				bool flag6 = CAbilityFilter.IsTileFeatureless(item, includeInitial: false, cMonsterClass?.ID) && (cTile == null || CAbilityFilter.IsTileFeatureless(cTile, includeInitial: false, cMonsterClass?.ID));
				if (item.m_HexMap == actorTile.m_HexMap || item.m_Hex2Map == actorTile.m_HexMap)
				{
					_ = 1;
				}
				else
					_ = item.m_HexMap == actorTile.m_Hex2Map;
				if (((SelectedMonsterYMLData != null) & flag) && !flag2)
				{
					failTile.Add(item);
					failReason.Add("Trying to spawn water monster on non water tile");
				}
				else if (flag5)
				{
					failTile.Add(item);
					failReason.Add("Closed Door");
				}
				else if (!flag6)
				{
					failTile.Add(item);
					failReason.Add("Tile contains a Prop");
				}
				else if (!revealed)
				{
					failTile.Add(item);
					failReason.Add("Tile not revealed");
				}
				else if (flag4)
				{
					failTile.Add(item);
					failReason.Add("Actor on tile");
				}
				else if (!flag3)
				{
					failTile.Add(item);
					failReason.Add("Tile is a wall/blocked");
				}
				else
				{
					possibleTiles.Add(item);
				}
			}
			else
			{
				failTile.Add(actorTile);
				failReason.Add("No adjacent tile");
			}
		}
		if (possibleTiles.Count == 0 && failTile.Count == 0)
		{
			failTile.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
			failReason.Add("Actor target has no adjacent tiles");
		}
	}

	public static void SelectTilesBasedOnTargetingFocus(CActor TargetingActor, int Range, List<CTile> possibleTiles, CAbility.EAbilityTargeting Targeting, List<CTile> TilesSelected, int m_NumberTargetsRemaining, bool AllTargets)
	{
		if (TargetingActor.AIMoveFocusActors.Count == 0)
		{
			TargetingActor.Move(1, jump: false, fly: false, Range, allowMove: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, null, firstMove: true, moveTest: false, carryOtherActors: false);
		}
		TargetingActor.AIMoveFocusActors.RemoveAll((CActor x) => x == null);
		if (TargetingActor.AIMoveFocusActors.Count != 0)
		{
			List<CActorPath> list = new List<CActorPath>();
			List<CActorPath> list2 = new List<CActorPath>();
			foreach (CTile possibleTile in possibleTiles)
			{
				CActorPath cActorPath = new CActorPath();
				cActorPath.m_Tile = possibleTile;
				cActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(TargetingActor, possibleTile.m_ArrayIndex, ScenarioManager.Tiles[TargetingActor.AIMoveFocusActors[0].ArrayIndex.X, TargetingActor.AIMoveFocusActors[0].ArrayIndex.Y], out cActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestination: true, ignoreMoveCost: true, out var foundPath, shouldPathThroughDoors: false);
				if (foundPath)
				{
					list.Add(cActorPath);
				}
				else if (cActorPath.m_Waypoints.Count > 0)
				{
					list2.Add(cActorPath);
				}
			}
			list.Sort((CActorPath x, CActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
			list2.Sort((CActorPath x, CActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
			if (Targeting == CAbility.EAbilityTargeting.Custom && TilesSelected.Count > 0)
			{
				TilesSelected.RemoveAll((CTile x) => !possibleTiles.Contains(x));
				return;
			}
			if (AllTargets)
			{
				TilesSelected.AddRange(list.Select((CActorPath x) => x.m_Tile));
				return;
			}
			for (int num = 0; num < m_NumberTargetsRemaining; num++)
			{
				if (list.Count > 0 && list.Count > num)
				{
					TilesSelected.Add(list[num].m_Tile);
					continue;
				}
				int num2 = num - list.Count;
				if (list2.Count > 0 && list2.Count > num2)
				{
					TilesSelected.Add(list2[num2].m_Tile);
				}
				else if (possibleTiles.Count > num)
				{
					TilesSelected.Add(possibleTiles[num]);
					possibleTiles.RemoveAt(num);
				}
			}
			return;
		}
		if (Targeting == CAbility.EAbilityTargeting.Custom && TilesSelected.Count > 0)
		{
			TilesSelected.RemoveAll((CTile x) => !possibleTiles.Contains(x));
			return;
		}
		if (AllTargets)
		{
			TilesSelected.AddRange(possibleTiles);
			return;
		}
		for (int num3 = 0; num3 < m_NumberTargetsRemaining; num3++)
		{
			if (possibleTiles.Count > num3)
			{
				TilesSelected.Add(possibleTiles[num3]);
			}
		}
	}

	private void ResolveAttackAbility()
	{
		thisAttack = attack;
		int num = range;
		CActorStatic.ResolveAttackAbility(actor, ref num, ref canTargetInvisible, ref thisAttack);
		if (overrideMoveRange < 0)
		{
			range = num;
		}
		canTargetInvisible = GameState.CanTargetInvisible(actor, canTargetInvisible);
		if (!canTargetInvisible.HasValue)
		{
			canTargetInvisible = false;
		}
		CAbilityAttack cAbilityAttack = thisAttack;
		if (cAbilityAttack != null && cAbilityAttack.MiscAbilityData?.AttackFurthestTarget == true)
		{
			useFurthestFocus = true;
		}
		if (focusBenign || (thisAttack != null && thisAttack.StoppedByDisarm(actor)))
		{
			thisAttack = null;
			if (overrideMoveRange < 0)
			{
				range = 1;
			}
		}
		else
		{
			AoERadius = CalcAoERadius(thisAttack);
			addRangeAoE = AoERadius;
			if (thisAttack?.AreaEffect != null)
			{
				if (thisAttack.AreaEffect.Melee)
				{
					range = 0;
				}
				else
				{
					addRangeAoE += AoERadius;
				}
			}
		}
		if (thisAttack != null)
		{
			numTargets = thisAttack.NumberTargets;
			AbilityData.MiscAbilityData miscAbilityData = thisAttack.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.TargetOneEnemyWithAllAttacks == true)
			{
				numTargets = 1;
			}
			else if (numTargets < 0)
			{
				numTargets = int.MaxValue;
			}
			else if (numTargets == 0)
			{
				numTargets = 1;
			}
		}
		else
		{
			numTargets = 1;
		}
	}

	private void InitPathfinderContext()
	{
		ReachablePoints.Clear();
		pfContext = ScenarioManager.PathFinder.InitQuery(actor, actor.ArrayIndex, actor.ArrayIndex, hasTarget: false, maxMoveCount, jump || fly, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, null, null, ReachablePoints, ignoreDoorLocks, null, isCalculatingDistance: false);
		pfContext.fillEntireLevel = true;
		if ((jump || fly) && CObjectTrap.IgnoreFlyAndJump(actor))
		{
			pfContext.ignoreTraps = false;
			jumpingTraps = false;
		}
	}

	private void AddActorsToPathFinderAndEnemyEvals()
	{
		foreach (CActor otherActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (otherActor == actor || otherActor.Deactivated || otherActor.PhasedOut)
			{
				continue;
			}
			ref CPathFinderContext.TNode node = ref pfContext.GetNode(otherActor.ArrayIndex);
			if (!CanStopOnTopOf(actor, otherActor))
			{
				node.flags |= CPathFinderContext.TNode.Flags.CannotStop;
			}
			if (otherActor.IsDowned)
			{
				node.flags |= CPathFinderContext.TNode.Flags.Blocked;
			}
			if (otherActor.MultiHexArrayIndexes != null && otherActor.MultiHexArrayIndexes.Count > 0)
			{
				foreach (Point multiHexArrayIndex in otherActor.MultiHexArrayIndexes)
				{
					ref CPathFinderContext.TNode node2 = ref pfContext.GetNode(multiHexArrayIndex);
					if (!CanStopOnTopOf(actor, otherActor))
					{
						node2.flags |= CPathFinderContext.TNode.Flags.CannotStop;
					}
					if (otherActor.IsDowned)
					{
						node2.flags |= CPathFinderContext.TNode.Flags.Blocked;
					}
				}
			}
			if (!CActor.AreActorsEnemies(actor.Type, otherActor.Type, actor, otherActor))
			{
				continue;
			}
			bool flag = otherActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible);
			if (!CanMoveThroughEnemy(actor, otherActor))
			{
				node.flags |= CPathFinderContext.TNode.Flags.Blocked;
			}
			if (otherActor.MultiHexArrayIndexes != null && otherActor.MultiHexArrayIndexes.Count > 0)
			{
				foreach (Point multiHexArrayIndex2 in otherActor.MultiHexArrayIndexes)
				{
					ref CPathFinderContext.TNode node3 = ref pfContext.GetNode(multiHexArrayIndex2);
					if (!CanMoveThroughEnemy(actor, otherActor))
					{
						node3.flags |= CPathFinderContext.TNode.Flags.Blocked;
					}
				}
			}
			if (otherActor.Untargetable || (flag && canTargetInvisible != true) || otherActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep) || (thisAttack != null && CAbility.ImmuneToAbility(otherActor, thisAttack, targeting: true)))
			{
				continue;
			}
			CActor summoner = actor;
			CHeroSummonActor cHeroSummonActor = actor as CHeroSummonActor;
			if (useSummonerToMeasureFrom && cHeroSummonActor != null && cHeroSummonActor.Summoner != null)
			{
				summoner = cHeroSummonActor.Summoner;
			}
			int hexDist = ScenarioManager.PathFinder.FindPath(summoner, otherActor.ArrayIndex, summoner.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false).Count;
			if (!foundPath)
			{
				hexDist = Point.HexDist(summoner.ArrayIndex, otherActor.ArrayIndex);
			}
			if (ScenarioManager.CurrentScenarioState.ScenarioScripts.Any((ScenarioScript x) => x.IgnoreAIFocus(actor, otherActor)))
			{
				continue;
			}
			if (otherActor is CObjectActor { AttachedProp: var attachedProp } cObjectActor)
			{
				if ((attachedProp?.PropHealthDetails != null && cObjectActor.AttachedProp.PropHealthDetails.IgnoredByAIFocus) || (cObjectActor.AttachedProp?.PropHealthDetails != null && cObjectActor.AttachedProp.PropHealthDetails.IgnoredByHeroSummonFocus && (cHeroSummonActor != null || (actor.Type == CActor.EType.Ally && actor is CEnemyActor { PlayerSummoner: not null }))))
				{
					continue;
				}
				if (cObjectActor.AttachedProp is CObjectObstacle cObjectObstacle)
				{
					foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
					{
						Point point = new Point(pathingBlocker);
						int hexDist2 = ScenarioManager.PathFinder.FindPath(summoner, point, summoner.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath2, ignoreDoorLocks: false).Count;
						if (!foundPath2)
						{
							hexDist2 = Point.HexDist(point, otherActor.ArrayIndex);
						}
						CEnemyEval item = new CEnemyEval
						{
							position = point,
							actor = otherActor,
							hexDist = hexDist2
						};
						enemyEvals.Add(item);
					}
					continue;
				}
			}
			if (otherActor.MultiHexArrayIndexes != null && otherActor.MultiHexArrayIndexes.Count > 0)
			{
				foreach (Point multiHexArrayIndex3 in otherActor.MultiHexArrayIndexes)
				{
					int hexDist3 = ScenarioManager.PathFinder.FindPath(summoner, multiHexArrayIndex3, summoner.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath3, ignoreDoorLocks: false).Count;
					if (!foundPath3)
					{
						hexDist3 = Point.HexDist(multiHexArrayIndex3, otherActor.ArrayIndex);
					}
					CEnemyEval item2 = new CEnemyEval
					{
						position = multiHexArrayIndex3,
						actor = otherActor,
						hexDist = hexDist3
					};
					enemyEvals.Add(item2);
				}
			}
			else
			{
				CEnemyEval item3 = new CEnemyEval
				{
					position = otherActor.ArrayIndex,
					actor = otherActor,
					hexDist = hexDist
				};
				enemyEvals.Add(item3);
			}
		}
	}

	public static bool CanSummonStopOnTopOf(CActor summon, CActor otherActor)
	{
		HeroSummonYMLData heroSummonYMLData = (summon as CHeroSummonActor)?.SummonData;
		if (heroSummonYMLData == null || heroSummonYMLData.CanMoveOnActorClassID == null)
		{
			return false;
		}
		if (otherActor.Class?.ID != heroSummonYMLData.CanMoveOnActorClassID)
		{
			return false;
		}
		return true;
	}

	public static bool CanEnemyStopOnTopOf(CActor enemy, CActor otherActor)
	{
		CMonsterClass cMonsterClass = (enemy as CEnemyActor)?.MonsterClass;
		if (otherActor == null || otherActor.Class?.ID == null)
		{
			return false;
		}
		if (cMonsterClass?.MonsterYML?.CanMoveOnActorClassID == null)
		{
			return false;
		}
		if (!cMonsterClass.MonsterYML.CanMoveOnActorClassID.Contains(otherActor.Class.ID))
		{
			return false;
		}
		return true;
	}

	public static bool CanStopOnTopOf(CActor movingActor, CActor otherActor)
	{
		if (otherActor.IsSuperTransientBlocker)
		{
			return false;
		}
		if (movingActor.IgnoreActorCollision || CanSummonStopOnTopOf(movingActor, otherActor) || CanEnemyStopOnTopOf(movingActor, otherActor))
		{
			return true;
		}
		if (otherActor.IgnoreActorCollision)
		{
			return true;
		}
		return false;
	}

	public static bool CanMoveThroughEnemy(CActor movingActor, CActor otherActor)
	{
		if (!otherActor.IsTransientBlocker)
		{
			return true;
		}
		if (movingActor.IgnoreActorCollision)
		{
			return true;
		}
		if (!otherActor.VisibleBlocksPathing)
		{
			return true;
		}
		if (otherActor.DoesNotBlock)
		{
			return true;
		}
		return false;
	}

	private bool CheckJumpOnTrapOrHazard(CPathFinderContext.TNode node)
	{
		if (jumpingTraps && node.HasFlag(CPathFinderContext.TNode.Flags.Trap))
		{
			return true;
		}
		if (jumpingHazards && node.HasFlag(CPathFinderContext.TNode.Flags.HazardousTerrain))
		{
			return true;
		}
		return false;
	}

	public static void AddDoorsToPathFindingContext(CPathFinderContext pfContext, bool disallowDoorways, bool shouldPathThroughDoors)
	{
		foreach (CObjectDoor item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>())
		{
			ref CPathFinderContext.TNode node = ref pfContext.GetNode(new Point(item.ArrayIndex));
			if (disallowDoorways || (!shouldPathThroughDoors && !item.DoorIsOpen))
			{
				node.flags |= CPathFinderContext.TNode.Flags.SuperBlocked | CPathFinderContext.TNode.Flags.CannotStop;
			}
		}
	}

	public static void AddLevelObjectsToPathFindingContext(CPathFinderContext pfContext, CActor actor, int overrideMoveRange = 0, Point focusOverridePoint = default(Point), bool fly = false, bool disallowDoorways = false, bool shouldPathThroughDoors = false, bool isWaterMonster = false, bool jump = false)
	{
		CPathFinderContext.TNode.Flags flags = CPathFinderContext.TNode.Flags.Blocked;
		if (!fly)
		{
			flags |= CPathFinderContext.TNode.Flags.CannotStop;
		}
		CTile[,] tiles;
		if (actor.HasMirrorActor())
		{
			CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
			if (mirrorAbilityBehaviour != null && (actor == mirrorAbilityBehaviour.ActiveBonus.Actor || actor == mirrorAbilityBehaviour.MirrorTarget))
			{
				tiles = ScenarioManager.Tiles;
				foreach (CTile cTile in tiles)
				{
					if (cTile != null)
					{
						if (!mirrorAbilityBehaviour.IsMoveTileValidForAbilityMirrored(cTile.m_ArrayIndex, actor))
						{
							pfContext.GetNode(cTile.m_ArrayIndex).flags |= flags;
						}
						if (!mirrorAbilityBehaviour.IsStopTileValidForAbilityMirrored(cTile.m_ArrayIndex, actor))
						{
							pfContext.GetNode(cTile.m_ArrayIndex).flags |= CPathFinderContext.TNode.Flags.CannotStop;
						}
					}
				}
			}
		}
		lock (ScenarioManager.Scenario.Objects)
		{
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				if ((!@object.IsAttachedToProp || !(@object.AttachedProp is CObjectObstacle)) && !@object.IgnoreActorCollision && !@object.DoesNotBlock)
				{
					pfContext.GetNode(@object.ArrayIndex).flags |= flags;
				}
			}
		}
		lock (ScenarioManager.Scenario.HeroSummons)
		{
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				if (heroSummon.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					pfContext.GetNode(heroSummon.ArrayIndex).flags |= CPathFinderContext.TNode.Flags.Trap;
				}
			}
		}
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
			{
				Point point = new Point(prop.ArrayIndex);
				ref CPathFinderContext.TNode node = ref pfContext.GetNode(point);
				if (prop.NegativeHex && (overrideMoveRange != 0 || !(point == focusOverridePoint)))
				{
					node.flags |= CPathFinderContext.TNode.Flags.HazardousTerrain;
				}
				if (prop.PropTile.IsBlocked && !fly && !CAbilityFilter.IsTileFeatureless(prop.PropTile))
				{
					node.flags |= CPathFinderContext.TNode.Flags.CannotStop;
				}
				if (prop is CObjectObstacle cObjectObstacle)
				{
					foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
					{
						bool flag = false;
						foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
						{
							if (scenarioScript.AllowPathfindingThroughTileForEnemy(ScenarioManager.GetTile(pathingBlocker.ToPoint()), actor as CEnemyActor))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							continue;
						}
						ref CPathFinderContext.TNode node2 = ref pfContext.GetNode(new Point(pathingBlocker));
						if (cObjectObstacle.RuntimeAttachedActor != null)
						{
							if (!cObjectObstacle.RuntimeAttachedActor.BlocksPathing)
							{
								continue;
							}
							if (cObjectObstacle.RuntimeAttachedActor.IsTransientBlocker)
							{
								node2.flags |= CPathFinderContext.TNode.Flags.CannotStop;
							}
						}
						node2.flags |= flags;
						if (cObjectObstacle.IgnoresFlyAndJump)
						{
							node2.flags |= CPathFinderContext.TNode.Flags.SuperBlocked;
						}
					}
				}
				else if (prop is CObjectResource && node.HasFlag(CPathFinderContext.TNode.Flags.Blocked))
				{
					node.flags |= flags;
				}
			}
			foreach (CObjectTrap item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>())
			{
				if (!item.Activated)
				{
					pfContext.GetNode(new Point(item.ArrayIndex)).flags |= CPathFinderContext.TNode.Flags.Trap;
				}
			}
			foreach (CObjectDifficultTerrain item2 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>())
			{
				if (item2.TreatAsTrap && item2.TreatAsTrapFilter.IsValidTarget(actorState))
				{
					pfContext.GetNode(new Point(item2.ArrayIndex)).flags |= CPathFinderContext.TNode.Flags.Trap;
				}
			}
			foreach (CObjectHazardousTerrain item3 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>())
			{
				ref CPathFinderContext.TNode node3 = ref pfContext.GetNode(new Point(item3.ArrayIndex));
				if (item3.WillActivate(actor, actorIsJumping: false, actorIsFlying: false))
				{
					node3.flags |= CPathFinderContext.TNode.Flags.HazardousTerrain;
				}
			}
			AddDoorsToPathFindingContext(pfContext, disallowDoorways, shouldPathThroughDoors);
		}
		foreach (Point negativeHex in ScenarioManager.CurrentScenarioState.GetNegativeHexes(actor))
		{
			pfContext.GetNode(negativeHex).flags |= CPathFinderContext.TNode.Flags.HazardousTerrain;
		}
		foreach (Point superNegativeHex in ScenarioManager.CurrentScenarioState.SuperNegativeHexes)
		{
			pfContext.GetNode(superNegativeHex).flags |= CPathFinderContext.TNode.Flags.SuperNegative;
		}
		if (!isWaterMonster)
		{
			return;
		}
		tiles = ScenarioManager.Tiles;
		foreach (CTile cTile2 in tiles)
		{
			if (cTile2 != null && !cTile2.IsWater && (ScenarioScript.current == null || !ScenarioScript.current.IsUnderwater(cTile2)))
			{
				pfContext.GetNode(cTile2.m_ArrayIndex).flags |= CPathFinderContext.TNode.Flags.Blocked | CPathFinderContext.TNode.Flags.CannotStop;
			}
		}
	}

	private bool AddFocusOverrideToEnemyEvals()
	{
		if (focusOverrideType == CAIFocusOverrideDetails.EOverrideType.None)
		{
			return true;
		}
		CEnemyEval cEnemyEval = null;
		bool flag = false;
		if (focusOverrideActor != null)
		{
			for (int i = 0; i < enemyEvals.Count; i++)
			{
				CEnemyEval cEnemyEval2 = enemyEvals[i];
				if (cEnemyEval2.actor == focusOverrideActor)
				{
					cEnemyEval = cEnemyEval2;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			CActor summoner = actor;
			if (useSummonerToMeasureFrom && actor is CHeroSummonActor { Summoner: not null } cHeroSummonActor)
			{
				summoner = cHeroSummonActor.Summoner;
			}
			cEnemyEval = new CEnemyEval
			{
				position = focusOverridePoint,
				actor = focusOverrideActor,
				hexDist = HexDistToActor(summoner.ArrayIndex, focusOverrideActor, focusOverridePoint)
			};
		}
		CalculateValidAttackTiles(cEnemyEval);
		if (focusOverrideType == CAIFocusOverrideDetails.EOverrideType.OverrideFocusIfCanAttack && (cEnemyEval.lowestCostAttackTile == null || !cEnemyEval.lowestCostAttackTile.reachable))
		{
			return true;
		}
		if (cEnemyEval.lowestCostAttackTile == null)
		{
			return false;
		}
		cEnemyEval.isFocusOverride = true;
		if (!flag)
		{
			enemyEvals.Insert(0, cEnemyEval);
		}
		return true;
	}

	private void CalculateValidAttackTilesForAllEnemies()
	{
		foreach (CEnemyEval enemyEval in enemyEvals)
		{
			CalculateValidAttackTiles(enemyEval);
		}
	}

	private void CalculateValidAttackTiles(CEnemyEval enemyEval)
	{
		if (enemyEval.attackTiles != null)
		{
			return;
		}
		enemyEval.attackTiles = new List<CAttackTileEval>();
		if (enemyEval.actor != null && enemyEval.actor.CanBeTargetedFromAnywhere() && actor.CanTargetFromAnywhere(range + addRangeAoE, hasOtherTargets: false))
		{
			enemyEval.lowestCostAttackTile = new CAttackTileEval
			{
				enemyEval = enemyEval,
				position = actor.ArrayIndex,
				moveCost = pfContext.GetNode(actor.ArrayIndex, init: false).cost,
				distToTarget = 0,
				reachable = true
			};
			return;
		}
		tmp_points.Clear();
		CPathFinderContext cPathFinderContext = new CPathFinderContext(ScenarioManager.PathFinder, actor, enemyEval.position, enemyEval.position, hasTarget: false, range + addRangeAoE, ignoreBlocked: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, tmp_points);
		cPathFinderContext.ignoreNavLinks = true;
		cPathFinderContext.CopyNodes(pfContext);
		cPathFinderContext.FloodFill();
		foreach (Point tmp_point in tmp_points)
		{
			ref CPathFinderContext.TNode node = ref pfContext.GetNode(tmp_point, init: false);
			if (!node.HasAllFlags(CPathFinderContext.TNode.Flags.Visited | CPathFinderContext.TNode.Flags.Walkable) || node.HasFlag(CPathFinderContext.TNode.Flags.CannotStop) || !CheckLoS(tmp_point, enemyEval.position))
			{
				continue;
			}
			CPathFinderContext.TNode.TCost cost = node.cost;
			if (CheckJumpOnTrapOrHazard(node))
			{
				cost.movePoints += 10000;
			}
			CAttackTileEval cAttackTileEval = new CAttackTileEval
			{
				enemyEval = enemyEval,
				position = tmp_point,
				moveCost = cost,
				distToTarget = HexDistToActor(tmp_point, enemyEval.actor, enemyEval.position),
				reachable = node.HasFlag(CPathFinderContext.TNode.Flags.Reachable)
			};
			if (thisAttack?.AreaEffect != null)
			{
				EvalAttackTile(cAttackTileEval);
				if (cAttackTileEval.attackEval.numTargets == 0)
				{
					continue;
				}
			}
			enemyEval.attackTiles.Add(cAttackTileEval);
			if (enemyEval.actor == null && tmp_point == enemyEval.position)
			{
				enemyEval.lowestCostAttackTile = cAttackTileEval;
				break;
			}
			if (enemyEval.lowestCostAttackTile == null || cAttackTileEval.moveCost.CompareTo(enemyEval.lowestCostAttackTile.moveCost) < 0)
			{
				enemyEval.lowestCostAttackTile = cAttackTileEval;
			}
		}
	}

	public static int HexDistToActor(Point fromPoint, CActor toActor, Point toPoint)
	{
		if (toActor is CObjectActor { AttachedProp: CObjectObstacle attachedProp })
		{
			int num = int.MaxValue;
			{
				foreach (TileIndex pathingBlocker in attachedProp.PathingBlockers)
				{
					Point pt = new Point(pathingBlocker);
					int num2 = Point.HexDist(fromPoint, pt);
					if (num2 < num)
					{
						num = num2;
					}
				}
				return num;
			}
		}
		return Point.HexDist(fromPoint, toPoint);
	}

	public static bool CheckLoS(Point fromPoint, Point toPoint)
	{
		CTile tile = ScenarioManager.GetTile(fromPoint);
		if (tile == null)
		{
			return false;
		}
		CTile tile2 = ScenarioManager.GetTile(toPoint);
		if (tile2 == null)
		{
			return false;
		}
		if (CActor.HaveLOS(tile, tile2))
		{
			return true;
		}
		return false;
	}

	private static int CompareEnemies(CEnemyEval a, CEnemyEval b)
	{
		int num = b.isFocusOverride.CompareTo(a.isFocusOverride);
		if (num != 0)
		{
			return num;
		}
		int num2 = a.lowestMoveCostToAttack.CompareTo(b.lowestMoveCostToAttack);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = a.hexDist.CompareTo(b.hexDist);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = GameState.s_ActorAdjustedInitiativeComparer.Compare(a.actor, b.actor);
		if (num4 != 0)
		{
			return num4;
		}
		return 0;
	}

	private static int CompareEnemiesUsingFurthesFocus(CEnemyEval a, CEnemyEval b)
	{
		int num = b.hexDist.CompareTo(a.hexDist);
		if (num != 0)
		{
			return num;
		}
		return CompareEnemies(a, b);
	}

	private void SortEnemyEvals()
	{
		if (useFurthestFocus)
		{
			enemyEvals.Sort(CompareEnemiesUsingFurthesFocus);
		}
		else
		{
			enemyEvals.Sort(CompareEnemies);
		}
	}

	private void FindBestAttackPositionForTheFocus()
	{
		if (finalFocusEval.bestAttackTile != null)
		{
			return;
		}
		finalFocusEval.bestAttackTile = finalFocusEval.lowestCostAttackTile;
		if (finalFocusEval.bestAttackTile == null || !finalFocusEval.bestAttackTile.reachable || thisAttack == null)
		{
			return;
		}
		if (thisAttack.TargetingActor == null)
		{
			thisAttack.TargetingActor = actor;
		}
		if (thisAttack.AreaEffect == null && numTargets == 1 && (thisAttack.IsMeleeAttack || finalFocusEval.bestAttackTile.distToTarget > 1))
		{
			return;
		}
		EvalAttackTile(finalFocusEval.bestAttackTile);
		int num = CPathFinderContext.NumTraps(finalFocusEval.bestAttackTile.moveCost.movePoints);
		foreach (CAttackTileEval attackTile in finalFocusEval.attackTiles)
		{
			if (attackTile != finalFocusEval.bestAttackTile && attackTile.reachable && CPathFinderContext.NumTraps(attackTile.moveCost.movePoints) <= num)
			{
				EvalAttackTile(attackTile);
				if (CompareAttackTiles(attackTile, finalFocusEval.bestAttackTile, useMoveCostTieBreaker: true) < 0)
				{
					finalFocusEval.bestAttackTile = attackTile;
				}
			}
		}
	}

	private void EvalAttackTile(CAttackTileEval tileEval)
	{
		if (tileEval.evaluated)
		{
			return;
		}
		tileEval.evaluated = true;
		if (thisAttack.AreaEffect != null)
		{
			EvalAoEAttackTile(tileEval);
			return;
		}
		tileEval.attackEval.numTargets = 1;
		if (IsDisadvantageAttack(actor, thisAttack, tileEval.enemyEval.actor, tileEval.distToTarget))
		{
			tileEval.attackEval.numDisadvantages = 1;
		}
		if (numTargets > 1)
		{
			for (int i = 0; i < enemyEvals.Count; i++)
			{
				CEnemyEval cEnemyEval = enemyEvals[i];
				if (cEnemyEval.actor == null || cEnemyEval.actor == tileEval.enemyEval.actor || ContainsAdditionalTarget(tileEval.attackEval.additionalTargets, cEnemyEval.actor))
				{
					continue;
				}
				int num = HexDistToActor(tileEval.position, cEnemyEval.actor, cEnemyEval.position);
				if (num <= range && CheckLoS(tileEval.position, cEnemyEval.position))
				{
					TAdditionalTargetEval item = new TAdditionalTargetEval
					{
						target = cEnemyEval.actor,
						priority = i,
						disadvantage = IsDisadvantageAttack(actor, thisAttack, cEnemyEval.actor, num)
					};
					if (tileEval.attackEval.additionalTargets == null)
					{
						tileEval.attackEval.additionalTargets = new List<TAdditionalTargetEval>();
					}
					tileEval.attackEval.additionalTargets.Add(item);
				}
			}
		}
		if (tileEval.attackEval.additionalTargets == null)
		{
			return;
		}
		if (tileEval.attackEval.additionalTargets.Count > numTargets - 1)
		{
			tileEval.attackEval.additionalTargets.Sort(CompareAdditionalTargetsByDisadvantageAndPriority);
			tileEval.attackEval.additionalTargets.RemoveRange(numTargets - 1, tileEval.attackEval.additionalTargets.Count - (numTargets - 1));
			tileEval.attackEval.additionalTargets.Sort(CompareAdditionalTargetsByPriority);
		}
		tileEval.attackEval.numTargets += tileEval.attackEval.additionalTargets.Count;
		foreach (TAdditionalTargetEval additionalTarget in tileEval.attackEval.additionalTargets)
		{
			if (additionalTarget.disadvantage)
			{
				tileEval.attackEval.numDisadvantages++;
			}
		}
	}

	private bool ContainsAdditionalTarget(List<TAdditionalTargetEval> additionalTargets, CActor target)
	{
		if (additionalTargets == null)
		{
			return false;
		}
		foreach (TAdditionalTargetEval additionalTarget in additionalTargets)
		{
			if (additionalTarget.target == target)
			{
				return true;
			}
		}
		return false;
	}

	private static int CompareAdditionalTargetsByDisadvantageAndPriority(TAdditionalTargetEval a, TAdditionalTargetEval b)
	{
		int num = a.disadvantage.CompareTo(b.disadvantage);
		if (num != 0)
		{
			return num;
		}
		return a.priority.CompareTo(b.priority);
	}

	private static int CompareAdditionalTargetsByPriority(TAdditionalTargetEval a, TAdditionalTargetEval b)
	{
		return a.priority.CompareTo(b.priority);
	}

	public static int CalcAoERadius(CAbilityAttack thisAttack)
	{
		if (thisAttack?.AreaEffect == null)
		{
			return 0;
		}
		int num = 0;
		Point pt = new Point(0, 0);
		foreach (CAreaEffect.CAreaEffectHex enabledHex in thisAttack.AreaEffect.EnabledHexes)
		{
			int num2 = Point.HexDist(enabledHex.ArrayIndex, pt);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private void EvalAoEAttackTile(CAttackTileEval tileEval)
	{
		List<TPossibleAoEPlacement> list = null;
		if (thisAttack.AreaEffect.Melee)
		{
			list = CalcPossibleAoEPlacements(thisAttack, tileEval.position, tileEval.enemyEval.position);
		}
		else
		{
			bool flag = tileEval.enemyEval.actor?.CanBeTargetedFromAnywhere() ?? false;
			flag &= actor.CanTargetFromAnywhere(range, hasOtherTargets: false);
			CalcPossibleAoEPlacementsAroundEnemy(tileEval.enemyEval);
			list = new List<TPossibleAoEPlacement>();
			foreach (TPossibleAoEPlacement possibleAoEPlacement in tileEval.enemyEval.possibleAoEPlacements)
			{
				if ((!flag && !IsAoEPlacementInRange(tileEval.position, possibleAoEPlacement, range)) || (!flag && !CheckLoS(tileEval.position, possibleAoEPlacement.targetPoint)))
				{
					continue;
				}
				bool flag2 = true;
				foreach (Point hex in possibleAoEPlacement.hexes)
				{
					if (ScenarioManager.GetTile(hex) == null || !ScenarioManager.GetTile(hex).IsWalkable)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					list.Add(possibleAoEPlacement);
				}
			}
		}
		if (list == null || list.Count == 0)
		{
			return;
		}
		tileEval.AoEPlacement = list[0];
		tileEval.attackEval = EvalAoEAttack(tileEval, tileEval.AoEPlacement.hexes);
		for (int i = 1; i < list.Count; i++)
		{
			TPossibleAoEPlacement aoEPlacement = list[i];
			TAttackEval tAttackEval = EvalAoEAttack(tileEval, aoEPlacement.hexes);
			int num = CompareAttacks(tAttackEval, tileEval.attackEval);
			if (num == 0)
			{
				num = CompareAdditionalTargets(tAttackEval.additionalTargets, tileEval.attackEval.additionalTargets);
			}
			if (num == 0)
			{
				num = aoEPlacement.distToFocus.CompareTo(tileEval.AoEPlacement.distToFocus);
			}
			if (num == 0)
			{
				num = aoEPlacement.distToAnyTarget.CompareTo(tileEval.AoEPlacement.distToAnyTarget);
			}
			if (num < 0)
			{
				tileEval.AoEPlacement = aoEPlacement;
				tileEval.attackEval = tAttackEval;
			}
		}
	}

	private static bool IsAoEPlacementInRange(Point casterPosition, TPossibleAoEPlacement placement, int range)
	{
		foreach (Point hex in placement.hexes)
		{
			if (Point.HexDist(casterPosition, hex) <= range)
			{
				return true;
			}
		}
		return false;
	}

	public static void CalcAoEHexes(CAbilityAttack thisAttack, Point AoETarget, float rotation, bool flipped, List<Point> hexes)
	{
		CAreaEffect.GetTransformedEnemyHexesPositions(thisAttack.AreaEffect, AoETarget, rotation, flipped, hexes);
	}

	private List<TPossibleAoEPlacement> CalcPossibleAoEPlacements(CAbilityAttack thisAttack, Point AoETarget, Point focusPoint)
	{
		List<TPossibleAoEPlacement> list = new List<TPossibleAoEPlacement>();
		int num = Point.HexDist(AoETarget, focusPoint);
		int num2 = num;
		foreach (CEnemyEval enemyEval in enemyEvals)
		{
			int num3 = Point.HexDist(AoETarget, enemyEval.position);
			if (num3 < num2)
			{
				num2 = num3;
			}
		}
		AbilityData.MiscAbilityData miscAbilityData = thisAttack.MiscAbilityData;
		int num4;
		int num5;
		if (miscAbilityData == null)
		{
			num4 = 0;
		}
		else
		{
			num4 = ((miscAbilityData.AreaEffectSymmetrical == true) ? 1 : 0);
			if (num4 != 0)
			{
				num5 = 0;
				goto IL_0089;
			}
		}
		num5 = 1;
		goto IL_0089;
		IL_0089:
		int num6 = num5;
		float num7 = ((num4 != 0) ? 60f : 360f);
		for (int i = 0; i <= num6; i++)
		{
			bool flipped = i != 0;
			for (float num8 = 0f; num8 < num7; num8 += 60f)
			{
				CalcAoEHexes(thisAttack, AoETarget, num8, flipped, tmp_points2);
				if (tmp_points2.Contains(focusPoint))
				{
					TPossibleAoEPlacement item = new TPossibleAoEPlacement
					{
						targetPoint = AoETarget,
						angle = num8,
						flipped = flipped,
						hexes = tmp_points2.ToList(),
						distToFocus = num,
						distToAnyTarget = num2
					};
					list.Add(item);
				}
			}
		}
		return list;
	}

	private void CalcPossibleAoEPlacementsAroundEnemy(CEnemyEval enemyEval)
	{
		if (enemyEval.possibleAoEPlacements != null)
		{
			return;
		}
		enemyEval.possibleAoEPlacements = new List<TPossibleAoEPlacement>();
		GameState.GetTilesInRange(enemyEval.position, AoERadius, tmp_points3);
		foreach (Point item in tmp_points3)
		{
			List<TPossibleAoEPlacement> collection = CalcPossibleAoEPlacements(thisAttack, item, enemyEval.position);
			enemyEval.possibleAoEPlacements.AddRange(collection);
		}
	}

	private TAttackEval EvalAoEAttack(CAttackTileEval tileEval, List<Point> AoEHexes)
	{
		TAttackEval result = new TAttackEval
		{
			numTargets = 1
		};
		if (IsDisadvantageAttack(actor, thisAttack, tileEval.enemyEval.actor, tileEval.distToTarget))
		{
			result.numDisadvantages = 1;
		}
		for (int i = 0; i < enemyEvals.Count; i++)
		{
			CEnemyEval cEnemyEval = enemyEvals[i];
			if (cEnemyEval != tileEval.enemyEval && (cEnemyEval.actor == null || cEnemyEval.actor != tileEval.enemyEval.actor) && AoEHexes.Contains(cEnemyEval.position) && !ContainsAdditionalTarget(result.additionalTargets, cEnemyEval.actor) && CheckLoS(tileEval.position, cEnemyEval.position))
			{
				int distToTarget = HexDistToActor(tileEval.position, cEnemyEval.actor, cEnemyEval.position);
				bool flag = IsDisadvantageAttack(actor, thisAttack, cEnemyEval.actor, distToTarget);
				TAdditionalTargetEval item = new TAdditionalTargetEval
				{
					target = cEnemyEval.actor,
					priority = i,
					disadvantage = flag
				};
				if (result.additionalTargets == null)
				{
					result.additionalTargets = new List<TAdditionalTargetEval>();
				}
				result.additionalTargets.Add(item);
				result.numTargets++;
				if (flag)
				{
					result.numDisadvantages++;
				}
			}
		}
		return result;
	}

	private void SetAoEAttackAbilityParameters()
	{
		if (thisAttack?.AreaEffect == null || finalFocusEval?.bestAttackTile == null)
		{
			return;
		}
		CAbilityAttack cAbilityAttack = PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.LastOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability.ID == thisAttack.ID)?.m_Ability as CAbilityAttack;
		if (cAbilityAttack == null)
		{
			cAbilityAttack = PhaseManager.CurrentPhaseAction.RemainingPhaseAbilities.FirstOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability.ID == thisAttack.ID)?.m_Ability as CAbilityAttack;
			if (cAbilityAttack == null)
			{
				return;
			}
		}
		cAbilityAttack.AreaRangeTileX = finalFocusEval.bestAttackTile.AoEPlacement.targetPoint.X;
		cAbilityAttack.AreaRangeTileY = finalFocusEval.bestAttackTile.AoEPlacement.targetPoint.Y;
		cAbilityAttack.AreaEffectAngle = finalFocusEval.bestAttackTile.AoEPlacement.angle;
		cAbilityAttack.AreaEffectFlipped = finalFocusEval.bestAttackTile.AoEPlacement.flipped;
	}

	public static bool IsDisadvantageAttack(CActor actor, CAbilityAttack thisAttack, CActor target, int distToTarget)
	{
		if (target == null)
		{
			return false;
		}
		if (actor.Tokens.HasKey(CCondition.ENegativeCondition.Muddle))
		{
			return true;
		}
		if (target.IsOriginalMonsterType && (target as CEnemyActor).MonsterClass.AttackersGainDisadv)
		{
			return true;
		}
		if (target.Type == CActor.EType.Player)
		{
			thisAttack.ProcessSongOverridesAndAbilities(actor, CSong.ESongActivationType.AbilityStart, temporaryOverrides: true);
			if (CActiveBonus.FindApplicableActiveBonuses(target, CAbility.EAbilityType.AttackersGainDisadvantage).Any((CActiveBonus x) => x is CAttackersGainDisadvantageActiveBonus cAttackersGainDisadvantageActiveBonus && !cAttackersGainDisadvantageActiveBonus.Ability.IsItemAbility))
			{
				return true;
			}
			if (CActiveBonus.FindApplicableActiveBonuses(target, CAbility.EAbilityType.Shield).Any((CActiveBonus x) => x is CShieldActiveBonus && x.HasBehaviour<CShieldActiveBonus_ShieldAndDisadvantage>()))
			{
				return true;
			}
		}
		if (thisAttack.IsMeleeAttack || (thisAttack.AreaEffect != null && thisAttack.AreaEffect.Melee))
		{
			return false;
		}
		if (distToTarget == 1)
		{
			return true;
		}
		return false;
	}

	private static int CompareAttacks(TAttackEval a, TAttackEval b)
	{
		int num = b.numTargets.CompareTo(a.numTargets);
		if (num != 0)
		{
			return num;
		}
		int num2 = a.numDisadvantages.CompareTo(b.numDisadvantages);
		if (num2 != 0)
		{
			return num2;
		}
		return 0;
	}

	private static int CompareAdditionalTargets(List<TAdditionalTargetEval> a, List<TAdditionalTargetEval> b)
	{
		if (a == null)
		{
			if (b == null)
			{
				return 0;
			}
			return 1;
		}
		if (b == null)
		{
			return -1;
		}
		int num = b.Count.CompareTo(a.Count);
		if (num != 0)
		{
			return num;
		}
		for (int i = 0; i < a.Count; i++)
		{
			TAdditionalTargetEval tAdditionalTargetEval = a[i];
			TAdditionalTargetEval tAdditionalTargetEval2 = b[i];
			int num2 = tAdditionalTargetEval.priority.CompareTo(tAdditionalTargetEval2.priority);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return 0;
	}

	private static int CompareAttackTiles(CAttackTileEval a, CAttackTileEval b, bool useMoveCostTieBreaker)
	{
		int num = CPathFinderContext.NumTraps(a.moveCost.movePoints).CompareTo(CPathFinderContext.NumTraps(b.moveCost.movePoints));
		if (num != 0)
		{
			return num;
		}
		int num2 = b.reachable.CompareTo(a.reachable);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = CompareAttacks(a.attackEval, b.attackEval);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = a.moveCost.movePoints.CompareTo(b.moveCost.movePoints);
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = CompareAdditionalTargets(a.attackEval.additionalTargets, b.attackEval.additionalTargets);
		if (num5 != 0)
		{
			return num5;
		}
		if (useMoveCostTieBreaker)
		{
			int num6 = a.moveCost.CompareTo(b.moveCost);
			if (num6 != 0)
			{
				return num6;
			}
		}
		return 0;
	}

	private static int CompareAttackTilesUsingMoveCostTieBreaker(CAttackTileEval a, CAttackTileEval b)
	{
		return CompareAttackTiles(a, b, useMoveCostTieBreaker: true);
	}

	private void FindPathToFocus()
	{
		if (actor.AIMoveFocusPath == null)
		{
			actor.AIMoveFocusPath = new List<Point>();
		}
		if (finalFocusEval.bestAttackTile == null || maxMoveCount == 0)
		{
			actor.AIMoveFocusPath.Clear();
			return;
		}
		pfContext.GetPathTo(finalFocusEval.bestAttackTile.position, actor.AIMoveFocusPath, maxMoveCount);
		if (IsCurrentPathPerfect())
		{
			return;
		}
		if (actor.AIPreferMoveFocusPath && maxMoveCount > 1)
		{
			pfContext.GetPathTo(finalFocusEval.bestAttackTile.position, actor.AIMoveFocusPath, maxMoveCount - 1);
			if (IsCurrentPathPerfect(checkIfAllMovementIsUsed: false))
			{
				return;
			}
		}
		int num = SortAttackTilesAndFindNumberOfBests();
		InitReachableTiles();
		actor.AIMoveFocusPath.Clear();
		_ = actor.ArrayIndex;
		CPathFinderContext.TNode.TCost tCost = finalFocusEval.bestAttackTile.moveCost;
		CPathFinderContext.TNode.TCost cost = tCost;
		foreach (TReachableTileEval reachableTile in reachableTiles)
		{
			CPathFinderContext.TNode.TCost cost2 = reachableTile.cost;
			int num2 = cost2.CompareTo(cost);
			if (num2 > 0)
			{
				continue;
			}
			for (int i = 0; i < num; i++)
			{
				CAttackTileEval attackTile = finalFocusEval.attackTiles[i];
				CPathFinderContext.TNode.TCost tCost2 = CalcPathCost(reachableTile, attackTile);
				cost2 = reachableTile.cost;
				CPathFinderContext.TNode.TCost tCost3 = cost2.Add(tCost2);
				num2 = CPathFinderContext.NumTraps(tCost3.movePoints).CompareTo(CPathFinderContext.NumTraps(cost.movePoints));
				if (num2 == 0)
				{
					num2 = tCost2.CompareTo(tCost);
				}
				if (num2 == 0)
				{
					num2 = tCost3.CompareTo(cost);
				}
				if (num2 < 0)
				{
					tCost = tCost2;
					cost = tCost3;
					actor.AIMoveFocusPath.Clear();
					actor.AIMoveFocusPath.AddRange(reachableTile.path);
				}
			}
		}
	}

	private bool IsCurrentPathPerfect(bool checkIfAllMovementIsUsed = true)
	{
		if (actor.AIMoveFocusPath.Count == 0)
		{
			return true;
		}
		Point point = actor.AIMoveFocusPath[actor.AIMoveFocusPath.Count - 1];
		ref CPathFinderContext.TNode node = ref pfContext.GetNode(point);
		if (node.HasFlag(CPathFinderContext.TNode.Flags.CannotStop))
		{
			return false;
		}
		if (CheckJumpOnTrapOrHazard(node))
		{
			return false;
		}
		if (point == finalFocusEval.bestAttackTile.position)
		{
			return true;
		}
		if (checkIfAllMovementIsUsed && CPathFinderContext.CostWithoutTraps(node.cost.movePoints) < maxMoveCount)
		{
			return false;
		}
		return true;
	}

	private int SortAttackTilesAndFindNumberOfBests()
	{
		finalFocusEval.attackTiles.Sort(CompareAttackTilesUsingMoveCostTieBreaker);
		int i;
		for (i = 1; i < finalFocusEval.attackTiles.Count && CompareAttackTiles(finalFocusEval.attackTiles[i], finalFocusEval.bestAttackTile, useMoveCostTieBreaker: false) <= 0; i++)
		{
		}
		return i;
	}

	private void InitReachableTiles()
	{
		reachableTiles = new List<TReachableTileEval>(pfContext.outReachablePoints.Count - 1);
		foreach (Point outReachablePoint in pfContext.outReachablePoints)
		{
			if (!(outReachablePoint == actor.ArrayIndex))
			{
				ref CPathFinderContext.TNode node = ref pfContext.GetNode(outReachablePoint);
				CPathFinderContext.TNode.TCost cost = node.cost;
				if (CheckJumpOnTrapOrHazard(node))
				{
					cost.movePoints += 10000;
				}
				TReachableTileEval item = new TReachableTileEval
				{
					position = outReachablePoint,
					cost = cost,
					distToFocus = Point.HexDist(outReachablePoint, finalFocusEval.position),
					path = new List<Point>()
				};
				pfContext.GetPathTo(outReachablePoint, item.path);
				reachableTiles.Add(item);
			}
		}
		reachableTiles.Sort(CompareReachableTiles);
	}

	private static int CompareReachableTiles(TReachableTileEval a, TReachableTileEval b)
	{
		return a.distToFocus.CompareTo(b.distToFocus);
	}

	private CPathFinderContext.TNode.TCost CalcPathCost(TReachableTileEval reachableTile, CAttackTileEval attackTile)
	{
		if (reachableTile.position == attackTile.position)
		{
			return default;
		}
		pfContext.Reset(reachableTile.position, attackTile.position);
		pfContext.maxMovementPoints = int.MaxValue;
		if (!pfContext.FloodFill())
		{
			return new CPathFinderContext.TNode.TCost
			{
				movePoints = int.MaxValue
			};
		}
		ref CPathFinderContext.TNode node = ref pfContext.GetNode(attackTile.position);
		CPathFinderContext.TNode.TCost cost = node.cost;
		if (CheckJumpOnTrapOrHazard(node))
		{
			cost.movePoints += 10000;
		}
		return cost;
	}

	private void CalcWaypoints()
	{
		if (actor.AIMoveFocusWaypoints == null)
		{
			actor.AIMoveFocusWaypoints = new List<CTile>();
		}
		else
		{
			actor.AIMoveFocusWaypoints.Clear();
		}
		if (actor.AIMoveFocusPath == null || actor.AIMoveFocusPath.Count == 0)
		{
			return;
		}
		Point point = actor.AIMoveFocusPath[0];
		Point point2 = new Point(point.X - actor.ArrayIndex.X, point.Y - actor.PrevArrayIndex.Y);
		for (int i = 1; i < actor.AIMoveFocusPath.Count; i++)
		{
			Point point3 = actor.AIMoveFocusPath[i];
			Point point4 = new Point(point3.X - point.X, point3.Y - point.Y);
			if (point4 != point2)
			{
				actor.AIMoveFocusWaypoints.Add(ScenarioManager.GetTile(point));
			}
			point = point3;
			point2 = point4;
		}
		actor.AIMoveFocusWaypoints.Add(ScenarioManager.GetTile(point));
		CActorStatic.s_LastCalculatedPathAI = new CActorStatic.CTargetPath
		{
			m_ArrayIndices = actor.AIMoveFocusPath.ToList(),
			m_ArrayIndicesBeforeCull = actor.AIMoveFocusPath.ToList(),
			m_Target = finalFocusEval.actor,
			m_TargetTile = finalFocusEval.position,
			m_Waypoints = actor.AIMoveFocusWaypoints.ToList()
		};
		CActorStatic.s_LastCalculatedPathActorAI = actor;
	}

	private void CalcMoveFocusActors()
	{
		bool flag = false;
		for (int num = enemyEvals.Count - 1; num >= 0; num--)
		{
			CEnemyEval cEnemyEval = enemyEvals[num];
			if (actor.AIMoveFocusActors.Contains(cEnemyEval.actor))
			{
				actor.AIMoveFocusActors.Remove(cEnemyEval.actor);
				actor.AIMoveFocusTiles.Remove(cEnemyEval.position);
			}
			actor.AIMoveFocusActors.Insert(0, cEnemyEval.actor);
			actor.AIMoveFocusTiles.Insert(0, cEnemyEval.position);
			flag = flag || cEnemyEval.isAttackableThisTurn;
		}
		if (thisAttack == null || !actor.CanTargetFromAnywhere(thisAttack.Range, flag) || ScenarioScript.current == null || !ScenarioScript.current.EnemiesAttackWithMinimalMovement())
		{
			return;
		}
		foreach (CActor aIMoveFocusActor in actor.AIMoveFocusActors)
		{
			if (aIMoveFocusActor.CanBeTargetedFromAnywhere())
			{
				actor.AIMoveFocusPath.Clear();
				break;
			}
		}
	}

	public static void AnalyzeMonsterAbilities()
	{
		foreach (MonsterYMLData monster in ScenarioRuleClient.SRLYML.Monsters)
		{
			if (monster.AbilityCards == null)
			{
				continue;
			}
			foreach (MonsterCardYMLData abilityCard in monster.AbilityCards)
			{
				if (abilityCard.CardAction?.Abilities == null)
				{
					continue;
				}
				CAbilityMove cAbilityMove = null;
				CAbilityAttack cAbilityAttack = null;
				foreach (CAbility ability in abilityCard.CardAction.Abilities)
				{
					if (ability is CAbilityMove cAbilityMove2)
					{
						if (cAbilityMove != null)
						{
							error($"{abilityCard.FileName}: Monster card with multiple move abilities: {monster.ID}: {abilityCard.ID}: {cAbilityMove.ID} and {cAbilityMove2.ID}");
						}
						else
						{
							cAbilityMove = cAbilityMove2;
						}
					}
					else if (ability is CAbilityTeleport cAbilityTeleport)
					{
						error($"{abilityCard.FileName}: Monster with teleport ability: {monster.ID}: {abilityCard.ID}: {cAbilityTeleport.ID}");
					}
					else if (ability is CAbilityTeleport2 cAbilityTeleport2)
					{
						error($"{abilityCard.FileName}: Monster with teleport 2 ability: {monster.ID}: {abilityCard.ID}: {cAbilityTeleport2.ID}");
					}
					else
					{
						if (!(ability is CAbilityAttack cAbilityAttack2))
						{
							continue;
						}
						if (cAbilityAttack2.Targeting != CAbility.EAbilityTargeting.Range)
						{
							error($"{abilityCard.FileName}: Monster attack ability with targeting = {cAbilityAttack2.Targeting}: {monster.ID}: {abilityCard.ID}: {cAbilityAttack2.ID}");
						}
						int num = cAbilityAttack2.NumberTargets;
						if (!cAbilityAttack2.TargetIsBase)
						{
							num += MaxTargets(monster);
						}
						if (num != 1 && cAbilityAttack2.AreaEffect != null)
						{
							error($"{abilityCard.FileName}: Monster attack ability with multiple targets and AoE: {monster.ID}: {abilityCard.ID}: {cAbilityAttack2.ID}: Target: {num}");
						}
						if (cAbilityAttack2.AreaEffect != null)
						{
							_ = cAbilityAttack2.AreaEffect.Melee;
							foreach (CAreaEffect.CAreaEffectHex hex in cAbilityAttack2.AreaEffect.Hexes)
							{
								if (hex.Flags == CAreaEffect.EHexFlag.None && (hex.ArrayIndex.X != 0 || hex.ArrayIndex.Y != 0))
								{
									error($"{abilityCard.FileName}: Monster AoE attack with G not at 0,0: {monster.ID}: {abilityCard.ID}: {cAbilityAttack2.ID}");
								}
								if (hex.Flags.HasFlag(CAreaEffect.EHexFlag.Ally))
								{
									error($"{abilityCard.FileName}: Monster AoE attack with blue hex: {monster.ID}: {abilityCard.ID}: {cAbilityAttack2.ID}");
								}
							}
						}
						if (cAbilityAttack == null)
						{
							cAbilityAttack = cAbilityAttack2;
						}
					}
				}
			}
		}
		static int MaxTargets(MonsterYMLData monster)
		{
			if (monster.MonsterBaseStats == null)
			{
				return 1;
			}
			int num2 = 1;
			foreach (BaseStats monsterBaseStat in monster.MonsterBaseStats)
			{
				if (monsterBaseStat.Target > num2)
				{
					num2 = monsterBaseStat.Target;
				}
			}
			return num2;
		}
		static void error(string msg)
		{
			Log.Error(msg);
		}
	}
}

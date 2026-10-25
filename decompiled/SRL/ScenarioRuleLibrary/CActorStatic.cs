using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Libs.Utils.Profiler;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CActorStatic
{
	public class CTargetPath
	{
		public CActor m_Target;

		public Point m_TargetTile;

		public Point m_PathEndTile;

		public List<Point> m_ArrayIndices;

		public List<Point> m_ArrayIndicesBeforeCull;

		public List<CTile> m_Waypoints;

		public bool m_AllyBlockingCulledPathEnd;

		public bool m_AllyBlockingFullPathEnd;

		public int m_HypotheticalMoveNodesLeftToTargetToGetInRange;

		public int m_ActualMoveNodesLeftToTargetToGetInRange;

		public int m_EndOfPathRangeToTarget;

		public float m_CrowFlyDistance;

		public List<Point> m_TrapsInPath;

		public List<Point> m_AlliesInPath;

		public int m_TargetPathID;

		public bool m_IsDisadvantage;

		public CTargetPath()
		{
			m_ArrayIndices = new List<Point>();
			m_ArrayIndicesBeforeCull = new List<Point>();
			m_TrapsInPath = new List<Point>();
			m_AlliesInPath = new List<Point>();
			m_Waypoints = new List<CTile>();
		}

		public static CTargetPath Copy(CTargetPath path)
		{
			return new CTargetPath
			{
				m_Target = path.m_Target,
				m_TargetTile = path.m_TargetTile,
				m_PathEndTile = path.m_PathEndTile,
				m_ArrayIndices = path.m_ArrayIndices.ToList(),
				m_ArrayIndicesBeforeCull = path.m_ArrayIndicesBeforeCull.ToList(),
				m_Waypoints = path.m_Waypoints.ToList(),
				m_AllyBlockingCulledPathEnd = path.m_AllyBlockingCulledPathEnd,
				m_AllyBlockingFullPathEnd = path.m_AllyBlockingFullPathEnd,
				m_HypotheticalMoveNodesLeftToTargetToGetInRange = path.m_HypotheticalMoveNodesLeftToTargetToGetInRange,
				m_ActualMoveNodesLeftToTargetToGetInRange = path.m_ActualMoveNodesLeftToTargetToGetInRange,
				m_EndOfPathRangeToTarget = path.m_EndOfPathRangeToTarget,
				m_CrowFlyDistance = path.m_CrowFlyDistance,
				m_TrapsInPath = path.m_TrapsInPath.ToList(),
				m_AlliesInPath = path.m_AlliesInPath.ToList(),
				m_TargetPathID = path.m_TargetPathID,
				m_IsDisadvantage = path.m_IsDisadvantage
			};
		}
	}

	public class COptimalPath
	{
		public CTargetPath m_TargetPath;

		public int m_NumberTargets;

		public List<CActor> m_AdjacentTargets;

		public List<CActor> m_validActorsInRange;

		public float m_AreaEffectAngle;

		public int m_TileX;

		public int m_TileY;

		public int Priority;

		public int SubPriority;
	}

	public class CAOEMarker
	{
		public int m_TileX;

		public int m_TileY;
	}

	[ConsoleVariable(Alias = "use_new_ai_move", Persistent = false)]
	public static bool UseNewAIMove = true;

	public static CActor s_LastCalculatedPathActorAI;

	public static CTargetPath s_LastCalculatedPathAI;

	private static List<CTargetPath> s_LastCalculatedPaths;

	private static List<COptimalPath> s_OptimalPaths;

	public static bool AOETargetCheck(CAbilityAttack ability, CTile fromTile, CTile targetTile, int range)
	{
		if (ability?.AreaEffect != null)
		{
			if (ability.AreaEffect.Melee)
			{
				List<CTile> tilesInRange = GameState.GetTilesInRange(fromTile.m_ArrayIndex, range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				for (float num = ((ability.MiscAbilityData.AreaEffectSymmetrical == true) ? 300 : 0); num < 360f; num += 60f)
				{
					List<CTile> validTilesIncludingBlocked = null;
					Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = null;
					if (CAreaEffect.GetValidTiles(fromTile, fromTile, ability.AreaEffect, num, flipped: false, ability.AreaEffectFlipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks).Contains(targetTile))
					{
						return true;
					}
				}
				return false;
			}
			foreach (CTile item in GameState.GetTilesInRange(fromTile.m_ArrayIndex, range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true))
			{
				List<CTile> tilesInRange2 = GameState.GetTilesInRange(item.m_ArrayIndex, 1, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				tilesInRange2.Add(ScenarioManager.Tiles[item.m_ArrayIndex.X, item.m_ArrayIndex.Y]);
				for (float num2 = ((ability.MiscAbilityData.AreaEffectSymmetrical == true) ? 300 : 0); num2 < 360f; num2 += 60f)
				{
					List<CTile> validTilesIncludingBlocked2 = null;
					Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles2 = null;
					if (CAreaEffect.GetValidTiles(fromTile, item, ability.AreaEffect, num2, flipped: false, ability.AreaEffectFlipAxis, tilesInRange2, ref validTilesIncludingBlocked2, ref flagsOfValidTiles2, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks).Contains(targetTile))
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	public static List<CTile> GetAOETiles(CAbilityAttack ability, CTile fromTile, int range)
	{
		List<CTile> list = new List<CTile>();
		if (ability?.AreaEffect != null)
		{
			if (ability.AreaEffect.Melee)
			{
				List<CTile> tilesInRange = GameState.GetTilesInRange(fromTile.m_ArrayIndex, range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				for (float num = ((ability.MiscAbilityData.AreaEffectSymmetrical == true) ? 300 : 0); num < 360f; num += 60f)
				{
					List<CTile> validTilesIncludingBlocked = null;
					Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = null;
					List<CTile> validTiles = CAreaEffect.GetValidTiles(fromTile, fromTile, ability.AreaEffect, num, flipped: false, ability.AreaEffectFlipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks);
					list.AddRange(validTiles);
				}
			}
			else
			{
				foreach (CTile item in GameState.GetTilesInRange(fromTile.m_ArrayIndex, range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true))
				{
					List<CTile> tilesInRange2 = GameState.GetTilesInRange(item.m_ArrayIndex, 1, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
					tilesInRange2.Add(ScenarioManager.Tiles[item.m_ArrayIndex.X, item.m_ArrayIndex.Y]);
					for (float num2 = ((ability.MiscAbilityData.AreaEffectSymmetrical == true) ? 300 : 0); num2 < 360f; num2 += 60f)
					{
						List<CTile> validTilesIncludingBlocked2 = null;
						Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles2 = null;
						List<CTile> validTiles2 = CAreaEffect.GetValidTiles(fromTile, item, ability.AreaEffect, num2, flipped: false, ability.AreaEffectFlipAxis, tilesInRange2, ref validTilesIncludingBlocked2, ref flagsOfValidTiles2, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks);
						list.AddRange(validTiles2);
					}
				}
			}
		}
		return list.Distinct().ToList();
	}

	private static bool IsBlockingObstacle(CTile tile, out CObjectObstacle obstacle)
	{
		obstacle = null;
		if (tile == null)
		{
			return false;
		}
		obstacle = (CObjectObstacle)tile.FindProp(ScenarioManager.ObjectImportType.Obstacle);
		if (obstacle == null)
		{
			return false;
		}
		if (obstacle.PathingBlockers.Count == 0)
		{
			return false;
		}
		foreach (TileIndex pathingBlocker in obstacle.PathingBlockers)
		{
			if (pathingBlocker.X == tile.m_ArrayIndex.X && pathingBlocker.Y == tile.m_ArrayIndex.Y)
			{
				return true;
			}
		}
		return false;
	}

	public static bool InternalAIMove(CActor actor, CActor targetActor, Point targetArrayIndex, List<CActor> targetActors, List<CActor> blockingActors, List<CTile> tilesInRange, int maxMoveCount, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool isWaterMonster, bool isPlayer, int range, ref List<CTargetPath> targetPaths, bool trapsBlock, List<Point> trapsToBlock, bool excludeTargetInPath, bool alliesBlock, List<Point> alliesToBlock, bool obstaclesBlock, bool hazardTerrainBlock, CAbilityAttack attack = null, bool? canTargetInvisible = false, bool shouldPathThroughDoors = false, bool noMovementNeeded = false, bool openDoorwaysBlock = false, bool ignoreDoorLocks = false)
	{
		int num = 0;
		CTile item = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
		if (targetActor == null || !targetActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) || canTargetInvisible == true)
		{
			if (tilesInRange.Contains(item))
			{
				bool foundPath = false;
				Point sampleHexArrayIndex = (blockingActors.Any((CActor a) => a.ArrayIndex == targetArrayIndex) ? actor.ArrayIndex : targetArrayIndex);
				CTargetPath item2 = CActor.CalculateTargetActorPath(actor, actor.ArrayIndex, targetActors, blockingActors, targetActor, targetArrayIndex, sampleHexArrayIndex, trapsBlock, trapsToBlock, alliesBlock, alliesToBlock, isWaterMonster, isPlayer, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, excludeDestinationInPath: false, targetActorShouldBlock: true, maxMoveCount, range, ref foundPath, obstaclesBlock, hazardTerrainBlock, shouldPathThroughDoors, openDoorwaysBlock);
				if (foundPath)
				{
					targetPaths.Add(item2);
					return targetPaths.Count > 0;
				}
			}
			List<CTargetPath> list = new List<CTargetPath>();
			List<CTile> list2 = null;
			List<Point> list3 = new List<Point>();
			ScenarioManager.PathFinder.FloodFill_ToRange(actor, actor.ArrayIndex, maxMoveCount, (fly | jump) || !trapsBlock, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, null, null, list3, null, ignoreDoorLocks);
			list2 = new List<CTile>(list3.Count);
			foreach (Point item4 in list3)
			{
				list2.Add(ScenarioManager.Tiles[item4.X, item4.Y]);
			}
			list2.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
			HeroSummonYMLData summonData = (actor as CHeroSummonActor)?.SummonData;
			CMonsterClass enemyClass = (actor as CEnemyActor)?.MonsterClass;
			bool flag = false;
			foreach (CTile item5 in list2)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(item5.m_ArrayIndex);
				bool flag2 = false;
				if (cActor is CHeroSummonActor cHeroSummonActor && !CActor.AreActorsAllied(actor.Type, cActor.Type, actor, cActor) && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					flag2 = true;
				}
				if ((!ScenarioManager.Scenario.FindActorsAt(item5.m_ArrayIndex).All((CActor x) => x.IgnoreActorCollision || (summonData?.CanMoveOnActorClassID != null && x.Class?.ID == summonData.CanMoveOnActorClassID) || (enemyClass?.MonsterYML?.CanMoveOnActorClassID != null && x.Class.ID != null && enemyClass.MonsterYML.CanMoveOnActorClassID.Contains(x.Class.ID))) && cActor != actor) || (!fly && ScenarioManager.PathFinder.Nodes[item5.m_ArrayIndex.X, item5.m_ArrayIndex.Y].Blocked) || (IsBlockingObstacle(item5, out var obstacle) && obstacle.IgnoresFlyAndJump) || !ScenarioManager.PathFinder.Nodes[item5.m_ArrayIndex.X, item5.m_ArrayIndex.Y].Walkable || (!fly && (item5.FindProp(ScenarioManager.ObjectImportType.Trap) != null || flag2) && (item5.m_ArrayIndex.X != actor.ArrayIndex.X || item5.m_ArrayIndex.Y != actor.ArrayIndex.Y) && (item5.FindProp(ScenarioManager.ObjectImportType.Trap) == null || (!item5.FindProp(ScenarioManager.ObjectImportType.Trap).Activated && (trapsBlock || trapsToBlock != null) && (trapsToBlock == null || trapsToBlock.Contains(item5.m_ArrayIndex)))) && (!flag2 || ((trapsBlock || trapsToBlock != null) && (trapsToBlock == null || trapsToBlock.Contains(item5.m_ArrayIndex))))) || (!(fly | ignoreHazardousTerrain) && (item5.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null || item5.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null) && (item5.m_ArrayIndex.X != actor.ArrayIndex.X || item5.m_ArrayIndex.Y != actor.ArrayIndex.Y) && ((item5.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) == null && item5.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) == null) || ((hazardTerrainBlock || trapsToBlock != null) && (trapsToBlock == null || trapsToBlock.Contains(item5.m_ArrayIndex))))) || (item5.FindProp(ScenarioManager.ObjectImportType.Door) != null && (item5.FindProp(ScenarioManager.ObjectImportType.Door) == null || openDoorwaysBlock || !(ScenarioManager.PathFinder.Nodes[item5.m_ArrayIndex.X, item5.m_ArrayIndex.Y].IsBridgeOpen | shouldPathThroughDoors))))
				{
					continue;
				}
				flag = false;
				num++;
				CTargetPath item3 = CActor.CalculateTargetActorPath(actor, actor.ArrayIndex, targetActors, blockingActors, targetActor, targetArrayIndex, item5.m_ArrayIndex, trapsBlock, trapsToBlock, alliesBlock, alliesToBlock, isWaterMonster, isPlayer, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, excludeDestinationInPath: false, targetActorShouldBlock: true, maxMoveCount, range, ref flag, obstaclesBlock, hazardTerrainBlock, shouldPathThroughDoors, openDoorwaysBlock);
				if (flag)
				{
					list.Add(item3);
					if (noMovementNeeded)
					{
						targetPaths.Add(item3);
						return targetPaths.Count > 0;
					}
				}
			}
			foreach (CTile item6 in tilesInRange)
			{
				CActor cActor2 = ScenarioManager.Scenario.FindActorAt(item6.m_ArrayIndex);
				bool flag3 = false;
				if (cActor2 is CHeroSummonActor cHeroSummonActor2 && !CActor.AreActorsAllied(actor.Type, cActor2.Type, actor, cActor2) && cHeroSummonActor2.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					flag3 = true;
				}
				if ((!ScenarioManager.Scenario.FindActorsAt(item6.m_ArrayIndex).All((CActor x) => x.IgnoreActorCollision || (summonData?.CanMoveOnActorClassID != null && x.Class?.ID == summonData.CanMoveOnActorClassID) || (enemyClass?.MonsterYML?.CanMoveOnActorClassID != null && x.Class.ID != null && enemyClass.MonsterYML.CanMoveOnActorClassID.Contains(x.Class.ID))) && ScenarioManager.Scenario.FindActorAt(item6.m_ArrayIndex) != actor) || (!fly && ScenarioManager.PathFinder.Nodes[item6.m_ArrayIndex.X, item6.m_ArrayIndex.Y].Blocked) || (IsBlockingObstacle(item6, out var obstacle2) && obstacle2.IgnoresFlyAndJump) || !ScenarioManager.PathFinder.Nodes[item6.m_ArrayIndex.X, item6.m_ArrayIndex.Y].Walkable || (!fly && (item6.FindProp(ScenarioManager.ObjectImportType.Trap) != null || flag3) && (item6.m_ArrayIndex.X != actor.ArrayIndex.X || item6.m_ArrayIndex.Y != actor.ArrayIndex.Y) && (item6.FindProp(ScenarioManager.ObjectImportType.Trap) == null || (!item6.FindProp(ScenarioManager.ObjectImportType.Trap).Activated && (trapsBlock || trapsToBlock != null) && (trapsToBlock == null || trapsToBlock.Contains(item6.m_ArrayIndex)))) && (!flag3 || ((trapsBlock || trapsToBlock != null) && (trapsToBlock == null || trapsToBlock.Contains(item6.m_ArrayIndex))))) || (!(fly | ignoreHazardousTerrain) && (item6.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null || item6.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null) && (item6.m_ArrayIndex.X != actor.ArrayIndex.X || item6.m_ArrayIndex.Y != actor.ArrayIndex.Y) && ((item6.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) == null && item6.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) == null) || ((hazardTerrainBlock || trapsToBlock != null) && (trapsToBlock == null || trapsToBlock.Contains(item6.m_ArrayIndex))))) || (item6.FindProp(ScenarioManager.ObjectImportType.Door) != null && (item6.FindProp(ScenarioManager.ObjectImportType.Door) == null || openDoorwaysBlock || !(ScenarioManager.PathFinder.Nodes[item6.m_ArrayIndex.X, item6.m_ArrayIndex.Y].IsBridgeOpen | shouldPathThroughDoors))))
				{
					continue;
				}
				num++;
				bool flag4 = false;
				CTargetPath cTargetPath = new CTargetPath();
				if (list.Count <= 0)
				{
					continue;
				}
				CTargetPath cTargetPath2 = null;
				int num2 = int.MaxValue;
				int num3 = int.MaxValue;
				float num4 = float.MaxValue;
				foreach (CTargetPath item7 in list)
				{
					Point point = ((item7.m_ArrayIndices.Count >= 1) ? item7.m_ArrayIndices[Math.Max(item7.m_ArrayIndices.Count - 1, 0)] : actor.ArrayIndex);
					num++;
					bool foundPath2 = false;
					CTargetPath cTargetPath3 = CActor.CalculateTargetActorPath(actor, point, targetActors, blockingActors, targetActor, targetArrayIndex, item6.m_ArrayIndex, trapsBlock, trapsToBlock, alliesBlock, alliesToBlock, isWaterMonster, isPlayer, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, excludeDestinationInPath: false, targetActorShouldBlock: true, maxMoveCount, range, ref foundPath2, obstaclesBlock, hazardTerrainBlock, shouldPathThroughDoors, openDoorwaysBlock);
					List<Point> arrayIndicesBeforeCull = cTargetPath3.m_ArrayIndicesBeforeCull;
					if (!foundPath2)
					{
						continue;
					}
					int num5 = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, arrayIndicesBeforeCull, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, null);
					int num6 = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, item7.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
					item7.m_ActualMoveNodesLeftToTargetToGetInRange = num5;
					item7.m_TrapsInPath.AddRange(cTargetPath3.m_TrapsInPath);
					item7.m_TrapsInPath = item7.m_TrapsInPath.Distinct().ToList();
					float traversalCost = CNode.GetTraversalCost(point, item6.m_ArrayIndex, fly, jump, ignoreMoveCost: true, ignoreDifficultTerrain);
					if (trapsToBlock != null && (trapsToBlock == null || item7.m_TrapsInPath.Count <= 0))
					{
						continue;
					}
					if (item7.m_TrapsInPath.Count < num3)
					{
						num3 = item7.m_TrapsInPath.Count;
						num2 = num5;
						num4 = traversalCost;
						cTargetPath2 = item7;
					}
					else
					{
						if (item7.m_TrapsInPath.Count != num3)
						{
							continue;
						}
						if (num5 < num2)
						{
							num2 = num5;
							num4 = traversalCost;
							cTargetPath2 = item7;
						}
						else if (num5 == num2)
						{
							if (num6 < cTargetPath2.m_ArrayIndices.Count)
							{
								num4 = traversalCost;
								cTargetPath2 = item7;
							}
							else if (num6 == cTargetPath2.m_ArrayIndices.Count && traversalCost < num4)
							{
								num4 = traversalCost;
								cTargetPath2 = item7;
							}
						}
					}
				}
				if (flag4 && cTargetPath.m_ActualMoveNodesLeftToTargetToGetInRange == cTargetPath.m_HypotheticalMoveNodesLeftToTargetToGetInRange && cTargetPath2 != null && cTargetPath.m_ActualMoveNodesLeftToTargetToGetInRange <= cTargetPath2.m_ActualMoveNodesLeftToTargetToGetInRange && (!trapsBlock || cTargetPath.m_TrapsInPath.Count <= cTargetPath2.m_TrapsInPath.Count))
				{
					targetPaths.Add(cTargetPath);
				}
				else if (cTargetPath2 != null)
				{
					CTargetPath cTargetPath4 = CTargetPath.Copy(cTargetPath2);
					cTargetPath4.m_CrowFlyDistance = CalculateCrowFlyDistance(actor.ArrayIndex, cTargetPath4.m_TargetTile);
					if (flag4)
					{
						cTargetPath4.m_HypotheticalMoveNodesLeftToTargetToGetInRange = cTargetPath.m_HypotheticalMoveNodesLeftToTargetToGetInRange;
					}
					cTargetPath4.m_AlliesInPath = null;
					targetPaths.Add(cTargetPath4);
				}
			}
		}
		return targetPaths.Count > 0;
	}

	public static int GetAOESize(CAbilityAttack attack)
	{
		int result = 0;
		if (attack?.AreaEffect != null)
		{
			int x = attack.AreaEffect.EnabledHexes.OrderByDescending((CAreaEffect.CAreaEffectHex cAreaEffectHex) => cAreaEffectHex.ArrayIndex.X).First().ArrayIndex.X;
			int x2 = attack.AreaEffect.EnabledHexes.OrderBy((CAreaEffect.CAreaEffectHex cAreaEffectHex) => cAreaEffectHex.ArrayIndex.X).First().ArrayIndex.X;
			int y = attack.AreaEffect.EnabledHexes.OrderByDescending((CAreaEffect.CAreaEffectHex cAreaEffectHex) => cAreaEffectHex.ArrayIndex.Y).First().ArrayIndex.Y;
			int y2 = attack.AreaEffect.EnabledHexes.OrderBy((CAreaEffect.CAreaEffectHex cAreaEffectHex) => cAreaEffectHex.ArrayIndex.Y).First().ArrayIndex.Y;
			result = Math.Max(0, Math.Max(1 + x - x2, 1 + y - y2) - (attack.AreaEffect.Melee ? 1 : 0));
		}
		return result;
	}

	public static bool FindAllPathsToTarget(CActor actor, CActor targetActor, Point targetArrayIndex, List<CActor> targetActors, List<CActor> blockingActors, ref List<CTargetPath> targetPaths, bool? canTargetInvisible, bool tryTraps, int maxMoveCount, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool isWaterMonster, bool isPlayer, int range, bool allowMove, CAbilityAttack attack, bool shouldPathThroughDoors, bool openDoorwaysBlock, ref bool targetFound, bool noMovementNeeded, bool focusBenign = false, bool moveTest = false, bool? targetATile = null, bool ignoreDoorLocks = false)
	{
		List<CTargetPath> targetPaths2 = new List<CTargetPath>();
		List<CTile> list = new List<CTile>();
		bool result = false;
		bool foundPath = false;
		ScenarioManager.PathFinder.FindPath(actor, actor.ArrayIndex, targetArrayIndex, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks);
		bool flag = range > 1 && !CActor.HaveLOS(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y], ScenarioManager.Tiles[targetArrayIndex.X, targetArrayIndex.Y]);
		bool flag2 = targetATile != false && targetActor == null;
		bool flag3 = false;
		int num = 0;
		if (!focusBenign)
		{
			num = GetAOESize(attack);
		}
		if (maxMoveCount == 0 && !moveTest)
		{
			List<CTile> list2 = new List<CTile>();
			list2.AddRange(GameState.GetTilesInRange(actor.ArrayIndex, range + num, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, shouldPathThroughDoors, flag2));
			if (!list2.Contains(ScenarioManager.Tiles[targetArrayIndex.X, targetArrayIndex.Y]))
			{
				flag3 = true;
			}
		}
		if (!flag3)
		{
			Point a = Point.OddRToAxial(actor.ArrayIndex);
			Point b = Point.OddRToAxial(targetArrayIndex);
			int num2 = Point.AxialDistance(a, b);
			if ((!foundPath || num2 > range) | flag | flag2)
			{
				list.AddRange(GameState.GetTilesInRange(targetArrayIndex, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, shouldPathThroughDoors, flag2));
				if (attack?.AreaEffect != null && !focusBenign)
				{
					list.AddRange(GetAOETiles(attack, ScenarioManager.Tiles[targetArrayIndex.X, targetArrayIndex.Y], range));
				}
				list = list.Distinct().ToList();
			}
			else if (targetActor == null || !targetActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) || canTargetInvisible == true)
			{
				list.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
			}
			if (list.Contains(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]))
			{
				noMovementNeeded = true;
			}
			bool flag4 = false;
			if (!tryTraps)
			{
				flag4 = InternalAIMove(actor, targetActor, targetArrayIndex, targetActors, blockingActors, list, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, ref targetPaths2, !fly, null, excludeTargetInPath: true, alliesBlock: false, null, !fly, !fly, attack, canTargetInvisible, shouldPathThroughDoors, noMovementNeeded, openDoorwaysBlock);
			}
			if (flag4)
			{
				targetPaths.AddRange(targetPaths2);
				result = true;
				targetFound = true;
			}
			else if (tryTraps)
			{
				List<CTargetPath> trapPaths = new List<CTargetPath>();
				if (InternalAIMove(actor, targetActor, targetArrayIndex, targetActors, blockingActors, list, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, ref trapPaths, trapsBlock: false, null, excludeTargetInPath: true, alliesBlock: false, null, !fly, hazardTerrainBlock: false, attack, canTargetInvisible, shouldPathThroughDoors, noMovementNeeded: false, openDoorwaysBlock))
				{
					if (tryTraps && !moveTest)
					{
						List<Point> list3 = new List<Point>();
						foreach (CTargetPath item in trapPaths)
						{
							foreach (Point item2 in item.m_TrapsInPath)
							{
								if (!list3.Contains(item2))
								{
									list3.Add(item2);
								}
							}
						}
						trapPaths.Sort((CTargetPath x, CTargetPath y) => x.m_TrapsInPath.Count.CompareTo(y.m_TrapsInPath.Count));
						trapPaths.RemoveAll((CTargetPath x) => x.m_TrapsInPath.Count > trapPaths[0].m_TrapsInPath.Count);
						int num3 = Math.Min(3, trapPaths[0].m_TrapsInPath.Count);
						int i;
						for (i = 1; i <= num3; i++)
						{
							List<List<Point>> list4 = ItemCombinations(list3, (i != 1) ? i : 0, i);
							List<CTargetPath> targetPaths3 = new List<CTargetPath>();
							foreach (List<Point> item3 in list4)
							{
								InternalAIMove(actor, targetActor, targetArrayIndex, targetActors, blockingActors, list, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, ref targetPaths3, trapsBlock: false, item3, excludeTargetInPath: true, alliesBlock: false, null, obstaclesBlock: true, hazardTerrainBlock: false, attack, canTargetInvisible, shouldPathThroughDoors, noMovementNeeded: false, openDoorwaysBlock);
							}
							if (targetPaths3.Count > 0 && targetPaths3.Exists((CTargetPath x) => x.m_TrapsInPath.Count <= i))
							{
								targetPaths.AddRange(targetPaths3);
								flag4 = true;
								return true;
							}
						}
						targetPaths.AddRange(trapPaths);
						flag4 = true;
						result = true;
					}
					if (moveTest)
					{
						targetPaths.AddRange(trapPaths);
						flag4 = true;
						result = true;
					}
				}
			}
		}
		return result;
	}

	public static void AIMove(CActor actor, int maxMoveCount, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool isWaterMonster, bool isPlayer, int range, bool allowMove, CAbilityAttack attack, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		if ((actor.AIMoveFocusPath != null && actor.AIMoveFocusPath.Count == 0) | firstMove)
		{
			using (new MiniProfilerScope("CActorStatic.AIMove"))
			{
				CActor.EnableLOSCache(enable: true);
				if (UseNewAIMove)
				{
					new CAIMoveNew(actor, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, isWaterMonster, isPlayer, range, allowMove, attack, moveTest).Process();
				}
				else
				{
					CPathFinder.EnablePathfinderCache(enable: true);
					CalcAIMoveFocusPath(actor, maxMoveCount, jump, fly, ignoreDifficultTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, allowMove, attack, moveTest);
					CPathFinder.EnablePathfinderCache(enable: false);
				}
				CActor.EnableLOSCache(enable: false);
			}
		}
		if (allowMove && actor.AIMoveFocusPath != null && actor.AIMoveFocusPath.Count > 0 && CAbilityMove.CalculateMoveUsed(actor, actor.ArrayIndex, actor.AIMoveFocusPath, 0, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, 0, freeTileAwayFromCaster, CAbilityMove.s_AllArrayIndicesAlongPath.Count > 0) <= maxMoveCount)
		{
			actor.ArrayIndex = actor.AIMoveFocusPath[0];
			actor.AIMoveFocusPath.Remove(actor.AIMoveFocusPath[0]);
			if (actor.AIMoveFocusPath.Count == 0)
			{
				actor.AIMoveFocusPath = null;
			}
		}
	}

	private static void CalcAIMoveFocusPath(CActor actor, int maxMoveCount, bool jump, bool fly, bool ignoreDifficultTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool isWaterMonster, bool isPlayer, int range, bool allowMove, CAbilityAttack attack, bool moveTest)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		bool? canTargetInvisible = false;
		CAbilityAttack thisAttack = attack;
		CActor cActor = null;
		ResolveAttackAbility(actor, ref range, ref canTargetInvisible, ref thisAttack);
		List<CTargetPath> targetPaths = (s_LastCalculatedPaths = new List<CTargetPath>());
		CActor overrideActor = null;
		Point overridePoint = default;
		bool needToOpenDoor = false;
		bool disallowDoorways = false;
		bool focusBenign = false;
		bool useFurthestFocus = false;
		bool useSummonerToMeasureFrom = false;
		bool flag = true;
		bool? targetATile = null;
		int overrideMoveRange = -1;
		CAIFocusOverrideDetails.EOverrideType eOverrideType = CAIFocusOverrideDetails.EOverrideType.None;
		List<CTargetPath> targetPaths2 = new List<CTargetPath>();
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
		if (actorState != null && actorState.AIFocusOverride != null && actorState.AIFocusOverride.OverrideType != CAIFocusOverrideDetails.EOverrideType.None)
		{
			eOverrideType = actorState.AIFocusOverride.ResolveOverrideTargetForCurrentScenario(actor, out overrideActor, out overridePoint, out needToOpenDoor, out disallowDoorways, out focusBenign, out useFurthestFocus, out targetATile, out overrideMoveRange, out useSummonerToMeasureFrom, out ignoreDifficultTerrain);
			if (actorState.AIFocusOverride.OverrideTargetType == CAIFocusOverrideDetails.EOverrideTargetType.Prop && actorState.AIFocusOverride.TargetPropType != EPropType.None && overridePoint == default(Point))
			{
				flag = false;
			}
		}
		if (focusBenign)
		{
			range = 1;
		}
		List<CActor> list = new List<CActor>();
		List<CActor> blockingActors = null;
		CalcTargetAndBlockingActors(actor, isPlayer, thisAttack, list, ref blockingActors);
		if (!canTargetInvisible.HasValue)
		{
			canTargetInvisible = false;
		}
		bool flag2 = false;
		bool flag3 = false;
		if ((eOverrideType == CAIFocusOverrideDetails.EOverrideType.OverrideFocus || eOverrideType == CAIFocusOverrideDetails.EOverrideType.OverrideFocusIfCanAttack) & flag)
		{
			bool flag4 = false;
			bool targetFound = false;
			bool noMovementNeeded = false;
			List<CActor> list2 = new List<CActor>();
			if (!FindAllPathsToTarget(actor, overrideActor, overridePoint, list, blockingActors, ref targetPaths2, canTargetInvisible, moveTest, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain: false, 0, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, allowMove, thisAttack, needToOpenDoor, disallowDoorways, ref targetFound, noMovementNeeded, focusBenign, moveTest, targetATile))
			{
				list2.Add(overrideActor);
			}
			else
			{
				flag4 = true;
			}
			if (!flag4)
			{
				FindAllPathsToTarget(actor, overrideActor, overridePoint, list, blockingActors, ref targetPaths2, canTargetInvisible, !actor.TreatTrapsAsObstacles, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain: false, 0, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, allowMove, thisAttack, needToOpenDoor, disallowDoorways, ref targetFound, noMovementNeeded, focusBenign, moveTest, targetATile);
			}
			if (eOverrideType == CAIFocusOverrideDetails.EOverrideType.OverrideFocus || (eOverrideType == CAIFocusOverrideDetails.EOverrideType.OverrideFocusIfCanAttack && targetPaths2.Any((CTargetPath p) => p.m_EndOfPathRangeToTarget <= range)))
			{
				targetPaths = new List<CTargetPath>(targetPaths2);
				flag2 = (overrideActor == null) | focusBenign;
				flag3 = true;
			}
		}
		if (!flag3)
		{
			List<CActor> list3 = new List<CActor>();
			bool flag5 = false;
			bool targetFound2 = false;
			bool noMovementNeeded2 = false;
			if (eOverrideType == CAIFocusOverrideDetails.EOverrideType.None || eOverrideType == CAIFocusOverrideDetails.EOverrideType.OverrideFocusIfCanAttack)
			{
				foreach (CActor item in list)
				{
					item.CrowFlyDistance = CalculateCrowFlyDistance(actor.ArrayIndex, item.ArrayIndex);
				}
				int num = int.MaxValue;
				if (attack != null && attack.MiscAbilityData?.AttackFurthestTarget == true)
				{
					num = 0;
					list.Sort((CActor a1, CActor a2) => a2.CrowFlyDistance.CompareTo(a1.CrowFlyDistance));
				}
				else
				{
					list.Sort((CActor a1, CActor a2) => a1.CrowFlyDistance.CompareTo(a2.CrowFlyDistance));
				}
				float num2 = 0f;
				bool flag6 = !MultipleTargets(actor);
				cActor = null;
				foreach (CActor item2 in list)
				{
					if (attack != null && attack.MiscAbilityData?.AttackFurthestTarget == true)
					{
						if (flag6 && item2.CrowFlyDistance < num2 && item2.CrowFlyDistance < (float)num)
						{
							continue;
						}
						cActor = item2;
					}
					else
					{
						if (flag6 && item2.CrowFlyDistance > num2 + 1f && item2.CrowFlyDistance > (float)num)
						{
							continue;
						}
						cActor = item2;
					}
					_ = list.Count;
					_ = 8;
					if (!FindAllPathsToTarget(actor, item2, item2.ArrayIndex, list, blockingActors, ref targetPaths, canTargetInvisible, moveTest, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain: false, 0, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, allowMove, thisAttack, shouldPathThroughDoors: false, disallowDoorways, ref targetFound2, noMovementNeeded2, focusBenign, moveTest))
					{
						list3.Add(item2);
						continue;
					}
					foreach (CTargetPath item3 in targetPaths)
					{
						if (attack != null && attack.MiscAbilityData?.AttackFurthestTarget == true)
						{
							if (item3.m_ArrayIndices.Count > num)
							{
								num = item3.m_ArrayIndices.Count;
							}
						}
						else if (item3.m_ArrayIndices.Count < num)
						{
							num = item3.m_ArrayIndices.Count;
						}
					}
					num2 = item2.CrowFlyDistance;
					flag5 = true;
				}
				if (!flag5)
				{
					num = int.MaxValue;
					num2 = 0f;
					cActor = null;
					flag6 = !MultipleTargets(actor);
					foreach (CActor item4 in list3)
					{
						if (attack != null && attack.MiscAbilityData?.AttackFurthestTarget == true)
						{
							if (flag6 && item4.CrowFlyDistance < num2 && item4.CrowFlyDistance < (float)num)
							{
								continue;
							}
							cActor = item4;
						}
						else
						{
							if (flag6 && item4.CrowFlyDistance > num2 + 1f && item4.CrowFlyDistance > (float)num)
							{
								continue;
							}
							cActor = item4;
						}
						if (!FindAllPathsToTarget(actor, item4, item4.ArrayIndex, list, blockingActors, ref targetPaths, canTargetInvisible, !actor.TreatTrapsAsObstacles, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain: false, 0, freeTileAwayFromCaster, isWaterMonster, isPlayer, range, allowMove, thisAttack, shouldPathThroughDoors: false, disallowDoorways, ref targetFound2, noMovementNeeded2, focusBenign, moveTest))
						{
							continue;
						}
						foreach (CTargetPath item5 in targetPaths)
						{
							if (attack != null && attack.MiscAbilityData?.AttackFurthestTarget == true)
							{
								if (item5.m_ArrayIndices.Count > num)
								{
									num = item5.m_ArrayIndices.Count;
								}
							}
							else if (item5.m_ArrayIndices.Count < num)
							{
								num = item5.m_ArrayIndices.Count;
							}
						}
						num2 = item4.CrowFlyDistance;
					}
				}
			}
		}
		List<CTargetPath> list4 = new List<CTargetPath>(targetPaths);
		list4.Sort((CTargetPath x, CTargetPath y) => GameState.s_ActorAdjustedInitiativeComparer.Compare(x.m_Target, y.m_Target));
		list4.Sort((CTargetPath x, CTargetPath y) => x.m_CrowFlyDistance.CompareTo(y.m_CrowFlyDistance));
		FilterPathsUsingDefaultLogic(actor, ref targetPaths, range, isPlayer, fly, jump, ignoreDifficultTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, eOverrideType, thisAttack != null && thisAttack.MiscAbilityData?.AttackFurthestTarget == true);
		s_LastCalculatedPaths = targetPaths;
		if (targetPaths.Count <= 0)
		{
			return;
		}
		CTargetPath targetPath = targetPaths[0];
		if (!flag2 && targetPath.m_ArrayIndices.Count == 0)
		{
			CheckForDisadvantage(actor, maxMoveCount, range, fly, isWaterMonster, needToOpenDoor, disallowDoorways, ref targetPath);
		}
		if (!flag2)
		{
			CheckForOptimalAttackPosition(actor, maxMoveCount, range, list, blockingActors, ref targetPath, fly, jump, ignoreDifficultTerrain, ignoreHazardousTerrain: false, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster, isPlayer, needToOpenDoor, disallowDoorways, list4);
		}
		actor.AIMoveFocusPath = targetPath.m_ArrayIndices;
		actor.AIMoveFocusWaypoints = targetPath.m_Waypoints;
		foreach (CTargetPath path in targetPaths)
		{
			if (actor.AIMoveFocusActors.Find((CActor x) => x == path.m_Target) == null)
			{
				actor.AIMoveFocusActors.Add(path.m_Target);
				Log.SimpleInfo("Added actor to AIMoveFocusActors List: " + path.m_Target?.ActorLocKey());
				actor.AIMoveFocusTiles.Add(path.m_TargetTile);
			}
		}
		List<CActor> list5 = new List<CActor>();
		foreach (CTargetPath path2 in list4)
		{
			if (actor.AIMoveFocusActors.Find((CActor x) => x == path2.m_Target) == null && list5.Find((CActor x) => x == path2.m_Target) == null)
			{
				list5.Add(path2.m_Target);
			}
		}
		Point startLocation = actor.ArrayIndex;
		if (targetPath != null && targetPath.m_ArrayIndices != null && targetPath.m_ArrayIndices.Count > 0)
		{
			startLocation = targetPath.m_ArrayIndices.Last();
		}
		List<GameState.CRangeSortedActor> list6 = new List<GameState.CRangeSortedActor>();
		foreach (CActor item6 in list5)
		{
			if (item6 != null)
			{
				List<Point> list7 = ScenarioManager.PathFinder.FindPath(actor, actor.ArrayIndex, item6.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var _, ignoreDoorLocks: false);
				ScenarioManager.PathFinder.FindPath(actor, startLocation, item6.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath2, ignoreDoorLocks: false);
				if (foundPath2 && CActor.HaveLOS(ScenarioManager.Tiles[startLocation.X, startLocation.Y], ScenarioManager.Tiles[item6.ArrayIndex.X, item6.ArrayIndex.Y]))
				{
					list6.Add(new GameState.CRangeSortedActor(item6, list7.Count));
				}
			}
		}
		foreach (CActor item7 in GameState.ActorListOnly(list6, sortForAttack: true))
		{
			actor.AIMoveFocusActors.Add(item7);
			Log.SimpleInfo("Added actor to AIMoveFocusActors List from extra actors check: " + item7.ActorLocKey());
		}
		s_LastCalculatedPathAI = new CTargetPath
		{
			m_AllyBlockingCulledPathEnd = targetPath.m_AllyBlockingCulledPathEnd,
			m_ArrayIndices = new List<Point>(targetPath.m_ArrayIndices),
			m_ArrayIndicesBeforeCull = new List<Point>(targetPath.m_ArrayIndicesBeforeCull),
			m_HypotheticalMoveNodesLeftToTargetToGetInRange = targetPath.m_HypotheticalMoveNodesLeftToTargetToGetInRange,
			m_Target = targetPath.m_Target,
			m_TargetTile = targetPath.m_TargetTile,
			m_Waypoints = new List<CTile>(targetPath.m_Waypoints)
		};
		s_LastCalculatedPathActorAI = actor;
		stopwatch.Stop();
		Console.WriteLine("Time taken : {0}", stopwatch.Elapsed);
		Log.SimpleInfo((moveTest ? "Test Focus " : "") + "Movement for " + actor.ActorLocKey() + ", move=" + maxMoveCount + ", range=" + range);
		string text = "Possible targets=";
		foreach (CActor item8 in list)
		{
			text = text + "[" + item8.ActorLocKey() + " at X=" + item8.ArrayIndex.X + ",Y=" + item8.ArrayIndex.Y + ",I=" + GameState.UpdatedInitiativeWithSummons(item8, item8.Initiative()) + "] ";
		}
		Log.SimpleInfo(text);
		if (cActor != null && list.Count > 1)
		{
			Log.SimpleInfo("Multi target optimization stopped at [" + cActor.ActorLocKey() + " at X=" + cActor.ArrayIndex.X + ",Y=" + cActor.ArrayIndex.Y + ",I=" + GameState.UpdatedInitiativeWithSummons(cActor, cActor.Initiative()) + "] ");
		}
		Log.SimpleInfo(actor.ActorLocKey() + " elapsed movement time=" + stopwatch.ElapsedMilliseconds + "ms");
		if (!moveTest)
		{
			Log.SimpleInfo(actor.ActorLocKey() + " moves from X=" + actor.ArrayIndex.X + " Y=" + actor.ArrayIndex.Y + ", to X=" + ((targetPath.m_ArrayIndices.Count > 0) ? (targetPath.m_ArrayIndices.Last().X + " Y=" + targetPath.m_ArrayIndices.Last().Y) : (actor.ArrayIndex.X + " Y=" + actor.ArrayIndex.Y)) + ", hexes=" + targetPath.m_ArrayIndices.Count + ", length=" + CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, targetPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, freeTileAwayFromCaster) + ", traps=" + targetPath.m_TrapsInPath.Distinct().ToList().Count + ", length remaining to target=" + targetPath.m_ActualMoveNodesLeftToTargetToGetInRange + ", focus=" + ((targetPath.m_Target != null) ? (targetPath.m_Target.ActorLocKey() + " at X=" + targetPath.m_Target.ArrayIndex.X + " Y=" + targetPath.m_Target.ArrayIndex.Y) : "") + ", crowfly distance=" + targetPath.m_CrowFlyDistance);
		}
	}

	private static void CalcTargetAndBlockingActors(CActor actor, bool isPlayer, CAbilityAttack thisAttack, List<CActor> targetActors, ref List<CActor> blockingActors)
	{
		if (isPlayer)
		{
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				if (enemy != actor && (thisAttack == null || !CAbility.ImmuneToAbility(enemy, thisAttack)))
				{
					targetActors.Add(enemy);
				}
			}
			foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
			{
				if (enemy2Monster != actor && (thisAttack == null || !CAbility.ImmuneToAbility(enemy2Monster, thisAttack)))
				{
					targetActors.Add(enemy2Monster);
				}
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				if ((@object.AttachedProp == null || @object.AttachedProp.PropHealthDetails == null || !@object.AttachedProp.PropHealthDetails.IgnoredByAIFocus) && (thisAttack.TargetingActor.Type != CActor.EType.HeroSummon || @object.AttachedProp == null || @object.AttachedProp.PropHealthDetails == null || !@object.AttachedProp.PropHealthDetails.IgnoredByHeroSummonFocus) && (thisAttack.TargetingActor.Type != CActor.EType.Ally || !(thisAttack.TargetingActor is CEnemyActor { PlayerSummoner: not null }) || @object.AttachedProp == null || @object.AttachedProp.PropHealthDetails == null || !@object.AttachedProp.PropHealthDetails.IgnoredByHeroSummonFocus) && @object != actor && !CActor.AreActorsAllied(actor.Type, @object.Type, actor, @object) && (thisAttack == null || !CAbility.ImmuneToAbility(@object, thisAttack)))
				{
					targetActors.Add(@object);
				}
			}
		}
		else
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (thisAttack == null || !CAbility.ImmuneToAbility(playerActor, thisAttack))
				{
					targetActors.Add(playerActor);
				}
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				if (thisAttack == null || !CAbility.ImmuneToAbility(heroSummon, thisAttack))
				{
					targetActors.Add(heroSummon);
				}
			}
			foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
			{
				if (thisAttack == null || !CAbility.ImmuneToAbility(allyMonster, thisAttack))
				{
					targetActors.Add(allyMonster);
				}
			}
			foreach (CObjectActor object2 in ScenarioManager.Scenario.Objects)
			{
				if ((object2.AttachedProp == null || object2.AttachedProp.PropHealthDetails == null || !object2.AttachedProp.PropHealthDetails.IgnoredByAIFocus) && (thisAttack.TargetingActor.Type != CActor.EType.HeroSummon || object2.AttachedProp == null || object2.AttachedProp.PropHealthDetails == null || !object2.AttachedProp.PropHealthDetails.IgnoredByHeroSummonFocus) && (thisAttack.TargetingActor.Type != CActor.EType.Ally || !(thisAttack.TargetingActor is CEnemyActor { PlayerSummoner: not null }) || object2.AttachedProp == null || object2.AttachedProp.PropHealthDetails == null || !object2.AttachedProp.PropHealthDetails.IgnoredByHeroSummonFocus) && object2 != actor && !CActor.AreActorsAllied(actor.Type, object2.Type, actor, object2) && (thisAttack == null || !CAbility.ImmuneToAbility(object2, thisAttack)))
				{
					targetActors.Add(object2);
				}
			}
			if (actor.Type == CActor.EType.Enemy)
			{
				foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
				{
					if (neutralMonster != actor && (thisAttack == null || !CAbility.ImmuneToAbility(neutralMonster, thisAttack)))
					{
						targetActors.Add(neutralMonster);
					}
				}
				foreach (CEnemyActor enemy2Monster2 in ScenarioManager.Scenario.Enemy2Monsters)
				{
					if (enemy2Monster2 != actor && (thisAttack == null || !CAbility.ImmuneToAbility(enemy2Monster2, thisAttack)))
					{
						targetActors.Add(enemy2Monster2);
					}
				}
			}
			else if (actor.Type == CActor.EType.Enemy2)
			{
				foreach (CEnemyActor neutralMonster2 in ScenarioManager.Scenario.NeutralMonsters)
				{
					if (neutralMonster2 != actor && (thisAttack == null || !CAbility.ImmuneToAbility(neutralMonster2, thisAttack)))
					{
						targetActors.Add(neutralMonster2);
					}
				}
				foreach (CEnemyActor enemy2 in ScenarioManager.Scenario.Enemies)
				{
					if (enemy2 != actor && (thisAttack == null || !CAbility.ImmuneToAbility(enemy2, thisAttack)))
					{
						targetActors.Add(enemy2);
					}
				}
			}
			else if (actor.Type == CActor.EType.Neutral)
			{
				foreach (CEnemyActor enemy3 in ScenarioManager.Scenario.Enemies)
				{
					if (enemy3 != actor && (thisAttack == null || !CAbility.ImmuneToAbility(enemy3, thisAttack)))
					{
						targetActors.Add(enemy3);
					}
				}
				foreach (CEnemyActor enemy2Monster3 in ScenarioManager.Scenario.Enemy2Monsters)
				{
					if (enemy2Monster3 != actor && (thisAttack == null || !CAbility.ImmuneToAbility(enemy2Monster3, thisAttack)))
					{
						targetActors.Add(enemy2Monster3);
					}
				}
			}
		}
		targetActors.RemoveAll((CActor x) => x.Deactivated);
		targetActors.RemoveAll((CActor x) => x.PhasedOut);
		targetActors.RemoveAll((CActor x) => x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
		blockingActors = targetActors.ToList();
		blockingActors.RemoveAll((CActor x) => x.DoesNotBlock);
		targetActors.RemoveAll((CActor x) => x.Untargetable);
	}

	public static void ResolveAttackAbility(CActor actor, ref int range, ref bool? canTargetInvisible, ref CAbilityAttack thisAttack)
	{
		CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
		if (currentPhaseAction != null && thisAttack == null)
		{
			thisAttack = currentPhaseAction.CurrentPhaseAbilities.LastOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability is CAbilityAttack cAbilityAttack2 && cAbilityAttack2.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)?.m_Ability as CAbilityAttack;
			if (thisAttack == null)
			{
				thisAttack = currentPhaseAction.RemainingPhaseAbilities.FirstOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability is CAbilityAttack cAbilityAttack2 && cAbilityAttack2.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)?.m_Ability as CAbilityAttack;
			}
		}
		if (thisAttack != null && !thisAttack.StoppedByDisarm(actor))
		{
			if (!thisAttack.HasStarted)
			{
				CAbilityAttack cAbilityAttack = (cAbilityAttack = (CAbilityAttack)CAbility.CopyAbility(thisAttack, generateNewID: false));
				cAbilityAttack.AbilityStartListenersInvoked = true;
				cAbilityAttack.Start(actor, actor);
				range = cAbilityAttack.Range;
				thisAttack = cAbilityAttack;
			}
			else
			{
				range = thisAttack.Range;
			}
			range = Math.Max(1, range);
			canTargetInvisible = thisAttack?.MiscAbilityData?.CanTargetInvisible;
		}
		else
		{
			range = 1;
		}
	}

	private static void FilterPathsUsingDefaultLogic(CActor actor, ref List<CTargetPath> currentPaths, int range, bool isPlayer, bool fly, bool jump, bool ignoreDifficultTerain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, CAIFocusOverrideDetails.EOverrideType focusOverrideType = CAIFocusOverrideDetails.EOverrideType.None, bool furthestPath = false)
	{
		List<CTargetPath> pathsToReduce = new List<CTargetPath>(currentPaths);
		s_LastCalculatedPaths = pathsToReduce;
		pathsToReduce.Sort((CTargetPath x, CTargetPath y) => x.m_TrapsInPath.Count.CompareTo(y.m_TrapsInPath.Count));
		pathsToReduce.RemoveAll((CTargetPath x) => x.m_TrapsInPath.Count > pathsToReduce[0].m_TrapsInPath.Count);
		s_LastCalculatedPaths = pathsToReduce;
		foreach (CTargetPath uniqueTarget in (from targetPath in pathsToReduce
			group targetPath by targetPath.m_Target into @group
			select @group.First()).ToList())
		{
			if (uniqueTarget.m_Target == null && focusOverrideType != CAIFocusOverrideDetails.EOverrideType.None)
			{
				if (pathsToReduce.Find((CTargetPath x) => x.m_Target == null && x.m_PathEndTile == x.m_TargetTile) != null)
				{
					pathsToReduce.RemoveAll((CTargetPath x) => x.m_Target == null && x.m_PathEndTile != x.m_TargetTile);
				}
				pathsToReduce.Sort((CTargetPath x, CTargetPath y) => x.m_EndOfPathRangeToTarget.CompareTo(y.m_EndOfPathRangeToTarget));
				pathsToReduce.RemoveAll((CTargetPath x) => x.m_EndOfPathRangeToTarget > pathsToReduce[0].m_EndOfPathRangeToTarget);
			}
			if (pathsToReduce.Find((CTargetPath x) => !x.m_IsDisadvantage && x.m_Target == uniqueTarget.m_Target && x.m_ActualMoveNodesLeftToTargetToGetInRange <= 0) != null)
			{
				pathsToReduce.RemoveAll((CTargetPath x) => x.m_IsDisadvantage && x.m_ArrayIndices.Count != 0 && x.m_ActualMoveNodesLeftToTargetToGetInRange <= 0 && x.m_Target == uniqueTarget.m_Target);
			}
			if (pathsToReduce.Find((CTargetPath x) => !x.m_AllyBlockingFullPathEnd && x.m_Target == uniqueTarget.m_Target) != null)
			{
				pathsToReduce.RemoveAll((CTargetPath x) => x.m_AllyBlockingFullPathEnd && x.m_Target == uniqueTarget.m_Target);
			}
			if (pathsToReduce.Find((CTargetPath x) => x.m_HypotheticalMoveNodesLeftToTargetToGetInRange >= 0 && x.m_Target == uniqueTarget.m_Target) != null)
			{
				pathsToReduce.RemoveAll((CTargetPath x) => x.m_HypotheticalMoveNodesLeftToTargetToGetInRange < 0 && x.m_Target == uniqueTarget.m_Target);
				continue;
			}
			List<CTargetPath> tooCloseActorPaths = pathsToReduce.FindAll((CTargetPath x) => x.m_HypotheticalMoveNodesLeftToTargetToGetInRange < 0 && x.m_Target == uniqueTarget.m_Target);
			tooCloseActorPaths.Sort((CTargetPath x, CTargetPath y) => y.m_HypotheticalMoveNodesLeftToTargetToGetInRange.CompareTo(x.m_HypotheticalMoveNodesLeftToTargetToGetInRange));
			pathsToReduce.RemoveAll((CTargetPath x) => x.m_HypotheticalMoveNodesLeftToTargetToGetInRange < tooCloseActorPaths[0].m_HypotheticalMoveNodesLeftToTargetToGetInRange && x.m_Target == uniqueTarget.m_Target);
		}
		s_LastCalculatedPaths = pathsToReduce;
		pathsToReduce.Sort((CTargetPath x, CTargetPath y) => x.m_HypotheticalMoveNodesLeftToTargetToGetInRange.CompareTo(y.m_HypotheticalMoveNodesLeftToTargetToGetInRange));
		s_LastCalculatedPaths = pathsToReduce;
		if (furthestPath)
		{
			pathsToReduce.Sort((CTargetPath x, CTargetPath y) => y.m_CrowFlyDistance.CompareTo(x.m_CrowFlyDistance));
			pathsToReduce.RemoveAll((CTargetPath x) => x.m_CrowFlyDistance < pathsToReduce[0].m_CrowFlyDistance);
		}
		else
		{
			pathsToReduce.Sort((CTargetPath x, CTargetPath y) => x.m_CrowFlyDistance.CompareTo(y.m_CrowFlyDistance));
			pathsToReduce.RemoveAll((CTargetPath x) => x.m_CrowFlyDistance > pathsToReduce[0].m_CrowFlyDistance);
		}
		s_LastCalculatedPaths = pathsToReduce;
		pathsToReduce.Sort((CTargetPath x, CTargetPath y) => x.m_ActualMoveNodesLeftToTargetToGetInRange.CompareTo(y.m_ActualMoveNodesLeftToTargetToGetInRange));
		s_LastCalculatedPaths = pathsToReduce;
		foreach (CTargetPath uniqueTarget2 in (from targetPath in pathsToReduce
			group targetPath by targetPath.m_Target into @group
			select @group.First()).ToList())
		{
			if (pathsToReduce.Find((CTargetPath x) => !x.m_AllyBlockingCulledPathEnd && x.m_Target == uniqueTarget2.m_Target) != null)
			{
				pathsToReduce.RemoveAll((CTargetPath x) => x.m_AllyBlockingCulledPathEnd && x.m_Target == uniqueTarget2.m_Target);
			}
		}
		s_LastCalculatedPaths = pathsToReduce;
		pathsToReduce.Sort((CTargetPath x, CTargetPath y) => (CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) + x.m_ActualMoveNodesLeftToTargetToGetInRange).CompareTo(CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, y.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) + y.m_ActualMoveNodesLeftToTargetToGetInRange));
		pathsToReduce.RemoveAll((CTargetPath x) => CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) + x.m_ActualMoveNodesLeftToTargetToGetInRange > CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, pathsToReduce[0].m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) + pathsToReduce[0].m_ActualMoveNodesLeftToTargetToGetInRange);
		s_LastCalculatedPaths = pathsToReduce;
		pathsToReduce.Sort((CTargetPath x, CTargetPath y) => GameState.s_ActorAdjustedInitiativeComparer.Compare(x.m_Target, y.m_Target));
		pathsToReduce.RemoveAll((CTargetPath x) => ((x.m_Target == null) ? 99 : GameState.UpdatedInitiativeWithSummons(x.m_Target, x.m_Target.Initiative())) > ((pathsToReduce[0].m_Target == null) ? 99 : GameState.UpdatedInitiativeWithSummons(pathsToReduce[0].m_Target, pathsToReduce[0].m_Target.Initiative())));
		if (!isPlayer && pathsToReduce.Count > 0 && pathsToReduce[0].m_Target != null && pathsToReduce[0].m_Target.Type == CActor.EType.HeroSummon)
		{
			pathsToReduce.RemoveAll((CTargetPath p) => p.m_Target != null && p.m_Target.Type != CActor.EType.HeroSummon);
		}
		currentPaths = pathsToReduce;
	}

	private static bool IsIgnoreDisadvantage(CActor target, CActor actor, CAbilityAttack abilityAttack)
	{
		abilityAttack.ProcessSongOverridesAndAbilities(actor, CSong.ESongActivationType.AbilityStart, temporaryOverrides: true);
		bool flag = actor.Tokens.HasKey(CCondition.ENegativeCondition.Muddle);
		bool flag2 = !abilityAttack.StoppedByDisarm(actor);
		bool flag3 = target != null && ((target.IsOriginalMonsterType && (target as CEnemyActor).MonsterClass.AttackersGainDisadv) || (target.Type == CActor.EType.Player && (CActiveBonus.FindApplicableActiveBonuses(target, CAbility.EAbilityType.AttackersGainDisadvantage).FindAll((CActiveBonus x) => x is CAttackersGainDisadvantageActiveBonus cAttackersGainDisadvantageActiveBonus && !cAttackersGainDisadvantageActiveBonus.Ability.IsItemAbility).Count > 0 || CActiveBonus.FindApplicableActiveBonuses(target, CAbility.EAbilityType.Shield).FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.HasBehaviour<CShieldActiveBonus_ShieldAndDisadvantage>()).Count > 0)));
		if (!(((flag | flag2) || abilityAttack.IsMeleeAttack) | flag3))
		{
			AbilityData.MiscAbilityData miscAbilityData = abilityAttack.MiscAbilityData;
			if (miscAbilityData == null)
			{
				return false;
			}
			return miscAbilityData.AttackHasDisadvantage == true;
		}
		return true;
	}

	private static void CheckForDisadvantage(CActor actor, int maxMoveCount, int range, bool fly, bool isWaterMonster, bool shouldPathThroughDoors, bool openDoorwaysBlock, ref CTargetPath targetPath)
	{
		CPhaseAction.CPhaseAbility cPhaseAbility = ((PhaseManager.PhaseType == CPhase.PhaseType.Action) ? ((CPhaseAction)PhaseManager.Phase).RemainingPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Attack) : null);
		if (cPhaseAbility == null)
		{
			cPhaseAbility = ((((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Attack) ? ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility : null);
		}
		if (cPhaseAbility == null)
		{
			return;
		}
		CAbilityAttack abilityAttack = cPhaseAbility.m_Ability as CAbilityAttack;
		if (IsIgnoreDisadvantage(targetPath.m_Target, actor, abilityAttack) || !ScenarioManager.IsTileAdjacent(actor.ArrayIndex.X, actor.ArrayIndex.Y, targetPath.m_TargetTile.X, targetPath.m_TargetTile.Y))
		{
			return;
		}
		List<CTile> tilesInRange = GameState.GetTilesInRange(actor, maxMoveCount, CAbility.EAbilityTargeting.Range, !fly, fly, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: false, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, shouldPathThroughDoors);
		List<CActor> list = new List<CActor>();
		List<CActor> list2 = new List<CActor>();
		if (actor.Type == CActor.EType.Enemy)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				list.Add(playerActor);
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				list.Add(heroSummon);
			}
			foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
			{
				list.Add(allyMonster);
			}
			foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
			{
				list.Add(neutralMonster);
			}
		}
		else if (actor.Type == CActor.EType.Neutral)
		{
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				list.Add(playerActor2);
			}
			foreach (CHeroSummonActor heroSummon2 in ScenarioManager.Scenario.HeroSummons)
			{
				list.Add(heroSummon2);
			}
			foreach (CEnemyActor allyMonster2 in ScenarioManager.Scenario.AllyMonsters)
			{
				list.Add(allyMonster2);
			}
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				list.Add(enemy);
			}
		}
		else
		{
			foreach (CEnemyActor enemy2 in ScenarioManager.Scenario.Enemies)
			{
				list.Add(enemy2);
			}
			foreach (CEnemyActor neutralMonster2 in ScenarioManager.Scenario.NeutralMonsters)
			{
				list.Add(neutralMonster2);
			}
		}
		list2 = list.ToList();
		_ = ScenarioManager.Tiles[targetPath.m_TargetTile.X, targetPath.m_TargetTile.Y];
		List<CTargetPath> list3 = new List<CTargetPath>();
		foreach (CTile item2 in tilesInRange)
		{
			bool foundPath = false;
			CTargetPath item = CActor.CalculateTargetActorPath(actor, actor.ArrayIndex, list, list2, targetPath.m_Target, targetPath.m_TargetTile, item2.m_ArrayIndex, !fly, null, alliesBlock: false, null, isWaterMonster, isPlayer: false, jump: false, fly, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestinationInPath: false, targetActorShouldBlock: true, maxMoveCount, 0, ref foundPath, !fly, !fly, shouldPathThroughDoors, openDoorwaysBlock);
			if (foundPath)
			{
				list3.Add(item);
			}
		}
		list3.RemoveAll((CTargetPath x) => x.m_EndOfPathRangeToTarget > range);
		list3.RemoveAll((CTargetPath x) => x.m_EndOfPathRangeToTarget <= 1);
		if (list3.Count > 0)
		{
			list3.Sort((CTargetPath x, CTargetPath y) => CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_ArrayIndices, fly, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, null).CompareTo(CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, y.m_ArrayIndices, fly, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, null)));
			int shortest = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, list3[0].m_ArrayIndices, fly, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, null);
			list3.RemoveAll((CTargetPath x) => CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_ArrayIndices, fly, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, null) > shortest);
			list3.Sort((CTargetPath x, CTargetPath y) => CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_ArrayIndices, fly, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, null).CompareTo(CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, y.m_ArrayIndices, fly, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreBlockedTileMoveCost: false, 0, null)));
		}
		if (list3.Count > 0)
		{
			targetPath.m_ArrayIndices = list3[0].m_ArrayIndices;
			targetPath.m_ArrayIndicesBeforeCull = list3[0].m_ArrayIndicesBeforeCull;
		}
	}

	private static bool MultipleTargets(CActor actor)
	{
		CPhaseAction.CPhaseAbility cPhaseAbility = ((PhaseManager.PhaseType == CPhase.PhaseType.Action) ? ((CPhaseAction)PhaseManager.Phase).RemainingPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Attack) : null);
		if (cPhaseAbility == null)
		{
			cPhaseAbility = ((((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Attack) ? ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility : null);
		}
		if (cPhaseAbility == null)
		{
			cPhaseAbility = ((PhaseManager.PhaseType == CPhase.PhaseType.Action) ? ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Attack) : null);
		}
		if (cPhaseAbility != null && !cPhaseAbility.m_Ability.StoppedByDisarm(actor))
		{
			CAbility ability = cPhaseAbility.m_Ability;
			if (ability.NumberTargets > 1 || ability.NumberTargets == -1 || ability.AreaEffect != null)
			{
				if (ability.MiscAbilityData != null && ability.MiscAbilityData.TargetOneEnemyWithAllAttacks.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
					if (miscAbilityData == null || miscAbilityData.TargetOneEnemyWithAllAttacks.Value)
					{
						goto IL_011d;
					}
				}
				return true;
			}
		}
		goto IL_011d;
		IL_011d:
		return false;
	}

	private static void CheckForOptimalAttackPosition(CActor actor, int maxMoveCount, int range, List<CActor> targetActors, List<CActor> blockingActors, ref CTargetPath targetPath, bool fly, bool jump, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool isWaterMonster, bool isPlayer, bool shouldPathThroughDoors, bool openDoorwaysBlock, List<CTargetPath> targetPathsBeforeCull)
	{
		if (actor.Type == CActor.EType.Player)
		{
			return;
		}
		CPhaseAction.CPhaseAbility cPhaseAbility = ((PhaseManager.PhaseType == CPhase.PhaseType.Action) ? ((CPhaseAction)PhaseManager.Phase).RemainingPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Attack) : null);
		if (cPhaseAbility == null)
		{
			cPhaseAbility = ((((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Attack) ? ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility : null);
		}
		if (cPhaseAbility == null)
		{
			cPhaseAbility = ((PhaseManager.PhaseType == CPhase.PhaseType.Action) ? ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability.AbilityType == CAbility.EAbilityType.Attack) : null);
		}
		if (cPhaseAbility == null || cPhaseAbility.m_Ability.StoppedByDisarm(actor))
		{
			return;
		}
		CAbility ability = cPhaseAbility.m_Ability;
		int num = (from w in CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.AddRange)
			where w.Ability.Augment == null
			select w).Sum((CActiveBonus x) => x.ReferenceStrength(ability, null));
		int val = ability.Range + num;
		val = Math.Max(1, val);
		AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.AreaEffectSymmetrical == true)
		{
			val++;
		}
		if (ability.NumberTargets <= 1 && ability.NumberTargets != -1 && ability.AreaEffect == null)
		{
			return;
		}
		if (ability.MiscAbilityData != null && ability.MiscAbilityData.TargetOneEnemyWithAllAttacks.HasValue)
		{
			AbilityData.MiscAbilityData miscAbilityData2 = ability.MiscAbilityData;
			if (miscAbilityData2 == null || miscAbilityData2.TargetOneEnemyWithAllAttacks.Value)
			{
				return;
			}
		}
		s_OptimalPaths = new List<COptimalPath>();
		List<CTile> tilesInRange = GameState.GetTilesInRange(actor, maxMoveCount, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, fly, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, shouldPathThroughDoors);
		tilesInRange.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
		List<CAOEMarker> list = new List<CAOEMarker>();
		foreach (CTile item in tilesInRange)
		{
			bool flag = item == ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			CActor cActor = ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex);
			bool flag2 = false;
			if (cActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
			{
				flag2 = true;
			}
			if (!flag && (ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex) != null || !(!ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Blocked | fly) || (IsBlockingObstacle(item, out var obstacle) && obstacle.IgnoresFlyAndJump) || !ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable || (!(fly | ignoreDifficultTerrain) && (item.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null || item.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null)) || (!fly && (item.FindProp(ScenarioManager.ObjectImportType.Trap) != null || flag2) && (item.FindProp(ScenarioManager.ObjectImportType.Trap) == null || !item.FindProp(ScenarioManager.ObjectImportType.Trap).Activated)) || (item.FindProp(ScenarioManager.ObjectImportType.Door) != null && !ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].IsBridgeOpen)))
			{
				continue;
			}
			bool foundPath = false;
			CTargetPath cTargetPath = CActor.CalculateTargetActorPath(actor, actor.ArrayIndex, targetActors, blockingActors, targetPath.m_Target, targetPath.m_TargetTile, item.m_ArrayIndex, trapsBlock: true, null, alliesBlock: false, null, isWaterMonster, isPlayer, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, excludeDestinationInPath: false, targetActorShouldBlock: true, maxMoveCount, range, ref foundPath, obstaclesBlock: true, hazardTerrainBlock: true, shouldPathThroughDoors, openDoorwaysBlock);
			if (!(foundPath | flag) || cTargetPath.m_ActualMoveNodesLeftToTargetToGetInRange != 0)
			{
				continue;
			}
			if (ability.AreaEffect != null)
			{
				if (ability.AreaEffect.Melee)
				{
					List<CTile> tilesInRange2 = GameState.GetTilesInRange(item.m_ArrayIndex, val, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
					for (float num2 = 0f; num2 < 360f; num2 += 60f)
					{
						List<CTile> validTilesIncludingBlocked = null;
						Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = null;
						List<CTile> validTiles = CAreaEffect.GetValidTiles(item, item, ability.AreaEffect, num2, flipped: false, ability.AreaEffectFlipAxis, tilesInRange2, ref validTilesIncludingBlocked, ref flagsOfValidTiles, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks);
						List<CActor> actorsInRange = GameState.GetActorsInRange(item.m_ArrayIndex, actor, val, null, ability.AbilityFilter, ability.AreaEffect, validTiles, ability.IsTargetedAbility, null, ability.MiscAbilityData?.CanTargetInvisible);
						TryAddOptimalPath(actor, actorsInRange, item, targetPath, cTargetPath, num2, null, ability.MiscAbilityData?.CanTargetInvisible);
					}
					continue;
				}
				List<CTile> tilesInRange3 = GameState.GetTilesInRange(item.m_ArrayIndex, val, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				tilesInRange3.Add(ScenarioManager.Tiles[item.m_ArrayIndex.X, item.m_ArrayIndex.Y]);
				foreach (CTile attackTile in tilesInRange3)
				{
					CAOEMarker AOEMarker = new CAOEMarker();
					AOEMarker.m_TileX = attackTile.m_ArrayIndex.X;
					AOEMarker.m_TileY = attackTile.m_ArrayIndex.Y;
					if (list.FirstOrDefault((CAOEMarker x) => x.m_TileX == AOEMarker.m_TileX && x.m_TileY == AOEMarker.m_TileY) == null)
					{
						list.Add(AOEMarker);
						GameState.GetActorOnTile(attackTile, actor, ability.AbilityFilter, null, ability.IsTargetedAbility, ability.MiscAbilityData?.CanTargetInvisible);
						List<CTile> tilesInRange4 = GameState.GetTilesInRange(attackTile.m_ArrayIndex, 1, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true);
						tilesInRange4.Add(ScenarioManager.Tiles[attackTile.m_ArrayIndex.X, attackTile.m_ArrayIndex.Y]);
						for (float num3 = ((ability.MiscAbilityData.AreaEffectSymmetrical == true) ? 300 : 0); num3 < 360f; num3 += 60f)
						{
							List<CTile> validTilesIncludingBlocked2 = null;
							Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles2 = null;
							List<CTile> validTiles2 = CAreaEffect.GetValidTiles(item, attackTile, ability.AreaEffect, num3, flipped: false, ability.AreaEffectFlipAxis, tilesInRange4, ref validTilesIncludingBlocked2, ref flagsOfValidTiles2, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks);
							List<CActor> actorsInRange2 = GameState.GetActorsInRange(item.m_ArrayIndex, actor, val + 5, null, ability.AbilityFilter, ability.AreaEffect, validTiles2, ability.IsTargetedAbility, null, ability.MiscAbilityData?.CanTargetInvisible);
							TryAddOptimalPath(actor, actorsInRange2, item, targetPath, cTargetPath, num3, attackTile, ability.MiscAbilityData?.CanTargetInvisible);
						}
						continue;
					}
					float sourceAngle;
					for (sourceAngle = ((ability.MiscAbilityData.AreaEffectSymmetrical == true) ? 300 : 0); sourceAngle < 360f; sourceAngle += 60f)
					{
						COptimalPath cOptimalPath = s_OptimalPaths.FirstOrDefault((COptimalPath x) => x.m_TileX == attackTile.m_ArrayIndex.X && x.m_TileY == attackTile.m_ArrayIndex.Y && x.m_AreaEffectAngle == sourceAngle);
						if (cOptimalPath != null)
						{
							TryAddOptimalPath(actor, cOptimalPath.m_validActorsInRange, item, targetPath, cTargetPath, sourceAngle, attackTile, ability.MiscAbilityData?.CanTargetInvisible);
						}
					}
				}
			}
			else
			{
				List<CActor> actorsInRange3 = GameState.GetActorsInRange(item.m_ArrayIndex, actor, range, null, ability.AbilityFilter, null, null, isTargetedAbility: false, null, ability.MiscAbilityData?.CanTargetInvisible);
				TryAddOptimalPath(actor, actorsInRange3, item, targetPath, cTargetPath, 0f, null, ability.MiscAbilityData?.CanTargetInvisible);
			}
		}
		if (s_OptimalPaths.Count <= 0)
		{
			return;
		}
		bool flag3 = true;
		CActor focusTarget = targetPath.m_Target;
		if (ability is CAbilityAttack abilityAttack)
		{
			flag3 = IsIgnoreDisadvantage(focusTarget, actor, abilityAttack);
		}
		if (!flag3 && (!targetPath.m_IsDisadvantage || s_OptimalPaths.Any((COptimalPath op) => !op.m_AdjacentTargets.Contains(focusTarget))))
		{
			s_OptimalPaths.RemoveAll((COptimalPath op) => op.m_AdjacentTargets.Contains(focusTarget));
		}
		if (s_OptimalPaths.Count <= 0)
		{
			return;
		}
		s_OptimalPaths.Sort((COptimalPath x, COptimalPath y) => y.m_NumberTargets.CompareTo(x.m_NumberTargets));
		int maxTargets = s_OptimalPaths[0].m_NumberTargets;
		if (ability.NumberTargets > 1 && ability.AreaEffect == null)
		{
			maxTargets = ((s_OptimalPaths[0].m_NumberTargets > ability.NumberTargets) ? ability.NumberTargets : s_OptimalPaths[0].m_NumberTargets);
		}
		s_OptimalPaths.RemoveAll((COptimalPath x) => x.m_NumberTargets < maxTargets);
		if (s_OptimalPaths.Count <= 0)
		{
			return;
		}
		if (!flag3)
		{
			s_OptimalPaths.Sort((COptimalPath x, COptimalPath y) => x.m_AdjacentTargets.Count.CompareTo(y.m_AdjacentTargets.Count));
			int disLimit = ((s_OptimalPaths[0].m_AdjacentTargets.Count > maxTargets - 1) ? s_OptimalPaths[0].m_AdjacentTargets.Count : (maxTargets - 1));
			s_OptimalPaths.RemoveAll((COptimalPath x) => x.m_AdjacentTargets.Count > disLimit);
			if (s_OptimalPaths.Count <= 0)
			{
				return;
			}
		}
		s_OptimalPaths.Sort((COptimalPath x, COptimalPath y) => CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_TargetPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster).CompareTo(CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, y.m_TargetPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster)));
		int shortestmove = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, s_OptimalPaths[0].m_TargetPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
		s_OptimalPaths.RemoveAll((COptimalPath x) => CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, x.m_TargetPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) > shortestmove);
		if (s_OptimalPaths.Count <= 0)
		{
			return;
		}
		foreach (COptimalPath s_OptimalPath in s_OptimalPaths)
		{
			List<GameState.CRangeSortedActor> list2 = new List<GameState.CRangeSortedActor>();
			if (s_OptimalPath.m_TargetPath.m_ArrayIndices != null && s_OptimalPath.m_TargetPath.m_ArrayIndices.Count > 0)
			{
				s_OptimalPath.Priority = int.MaxValue;
				Point startLocation = s_OptimalPath.m_TargetPath.m_ArrayIndices.Last();
				foreach (CActor item2 in s_OptimalPath.m_validActorsInRange)
				{
					if (item2 == focusTarget)
					{
						continue;
					}
					List<Point> list3 = ScenarioManager.PathFinder.FindPath(actor, actor.ArrayIndex, item2.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var _, ignoreDoorLocks: false);
					ScenarioManager.PathFinder.FindPath(actor, startLocation, item2.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath3, ignoreDoorLocks: false);
					if (foundPath3 && CActor.HaveLOS(ScenarioManager.Tiles[startLocation.X, startLocation.Y], ScenarioManager.Tiles[item2.ArrayIndex.X, item2.ArrayIndex.Y]))
					{
						list2.Add(new GameState.CRangeSortedActor(item2, list3.Count));
						if (list3.Count < s_OptimalPath.Priority)
						{
							s_OptimalPath.Priority = list3.Count;
						}
					}
				}
			}
			s_OptimalPath.m_validActorsInRange = GameState.ActorListOnly(list2, sortForAttack: true, shortestOnly: true);
		}
		s_OptimalPaths.Sort((COptimalPath x, COptimalPath y) => x.Priority.CompareTo(y.Priority));
		s_OptimalPaths.RemoveAll((COptimalPath x) => x.Priority > s_OptimalPaths[0].Priority);
		if (s_OptimalPaths.Count > 0)
		{
			targetPath.m_ArrayIndices = s_OptimalPaths[0].m_TargetPath.m_ArrayIndices;
			targetPath.m_ArrayIndicesBeforeCull = new List<Point>(targetPath.m_ArrayIndices);
			if (ability is CAbilityAttack cAbilityAttack)
			{
				cAbilityAttack.AreaEffectAngle = s_OptimalPaths[0].m_AreaEffectAngle;
				cAbilityAttack.AreaRangeTileX = s_OptimalPaths[0].m_TileX;
				cAbilityAttack.AreaRangeTileY = s_OptimalPaths[0].m_TileY;
			}
		}
	}

	private static void TryAddOptimalPath(CActor actor, List<CActor> validActorsInRange, CTile tile, CTargetPath targetPath, CTargetPath optimalTargetPath, float areaEffectAngle, CTile attackTile = null, bool? canTargetInvisible = false)
	{
		if (GameState.CanTargetInvisible(actor, canTargetInvisible) != true)
		{
			validActorsInRange.RemoveAll((CActor x) => x.Tokens.HasKey(CCondition.EPositiveCondition.Invisible));
		}
		if (validActorsInRange.Count <= 0)
		{
			return;
		}
		List<CActor> list = new List<CActor>();
		foreach (CActor item in validActorsInRange)
		{
			if (ScenarioManager.IsTileAdjacent(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, item.ArrayIndex.X, item.ArrayIndex.Y))
			{
				list.Add(item);
			}
		}
		CActor focusTarget = targetPath.m_Target;
		if (validActorsInRange.Find((CActor x) => x == focusTarget) != null)
		{
			COptimalPath cOptimalPath = new COptimalPath();
			cOptimalPath.m_TargetPath = optimalTargetPath;
			cOptimalPath.m_AdjacentTargets = list;
			cOptimalPath.m_NumberTargets = validActorsInRange.Count;
			cOptimalPath.m_validActorsInRange = validActorsInRange;
			cOptimalPath.m_AreaEffectAngle = areaEffectAngle;
			if (attackTile != null)
			{
				cOptimalPath.m_TileX = attackTile.m_ArrayIndex.X;
				cOptimalPath.m_TileY = attackTile.m_ArrayIndex.Y;
			}
			s_OptimalPaths.Add(cOptimalPath);
		}
	}

	public static bool IsIceTerrainSlideInterrupted(CActor movingActor, List<Point> path, List<Point> waypoints = null)
	{
		if (movingActor.IgnoreIceSlideEffect || path == null)
		{
			return true;
		}
		Point lastPoint = path[path.Count - 1];
		CNode cNode = ScenarioManager.PathFinder.Nodes[lastPoint.X, lastPoint.Y];
		if (!cNode.IsIceTerrain)
		{
			return false;
		}
		Point point;
		if (path.Count <= 1)
		{
			point = ((waypoints != null && waypoints.Count > 1) ? waypoints[waypoints.Count - 2] : movingActor.ArrayIndex);
		}
		else
		{
			point = path[path.Count - 2];
		}
		CNode cNode2 = ScenarioManager.PathFinder.Nodes[point.X, point.Y];
		if (cNode2.HasShadow && cNode.HasShadow && cNode2.NavLinks.Any((CNavLink x) => x.DestinationNode.Location == lastPoint))
		{
			return true;
		}
		ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(point.X, point.Y, lastPoint.X, lastPoint.Y);
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(lastPoint.X, lastPoint.Y, adjacentPosition);
		if (adjacentTile != null)
		{
			if (ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex, movingActor) != null)
			{
				return true;
			}
			CObjectDoor cObjectDoor = adjacentTile.FindDoor();
			if (cObjectDoor != null && cObjectDoor.DoorIsLocked && !cObjectDoor.DoorIsOpen)
			{
				return true;
			}
			CNode node = ScenarioManager.PathFinder.GetNode(adjacentTile.m_ArrayIndex);
			if (node.Blocked)
			{
				return true;
			}
			if (!node.Walkable)
			{
				return true;
			}
			if (node.IslandID != cNode2.IslandID && !node.IsBridgeOpen && !cNode2.IsBridgeOpen && !node.IsBridge && !cNode2.IsBridge)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static void PostProcessTargetPath(CTargetPath targetActorPath, CActor actor, bool isPlayer, int maxMoveCount, bool fly, bool jump, bool ignoreDifficultTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, int range)
	{
		int num = -1;
		targetActorPath.m_AllyBlockingCulledPathEnd = false;
		HeroSummonYMLData summonData = (actor as CHeroSummonActor)?.SummonData;
		CMonsterClass enemyClass = (actor as CEnemyActor)?.MonsterClass;
		for (int num2 = targetActorPath.m_ArrayIndices.Count - 1; num2 >= 0; num2--)
		{
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1].X, targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1].Y, ScenarioManager.EAdjacentPosition.ECenter);
			bool flag = false;
			bool flag2 = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, targetActorPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) > maxMoveCount;
			bool flag3 = ScenarioManager.Scenario.FindActorsAt(targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1]).Any((CActor x) => !x.IgnoreActorCollision && (summonData?.CanMoveOnActorClassID == null || x.Class?.ID != summonData.CanMoveOnActorClassID) && (enemyClass?.MonsterYML?.CanMoveOnActorClassID == null || x.Class?.ID == null || enemyClass.MonsterYML.CanMoveOnActorClassID.Contains(x.Class.ID)));
			bool flag4 = (fly | jump) || !ScenarioManager.PathFinder.Nodes[targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1].X, targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1].Y].Blocked;
			if (flag4 && IsBlockingObstacle(adjacentTile, out var obstacle))
			{
				flag4 = (fly | jump) && obstacle.IgnoresFlyAndJump;
			}
			bool flag5 = !ScenarioManager.PathFinder.Nodes[targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1].X, targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1].Y].Walkable;
			if (((flag2 | flag3) || !flag4) | flag5)
			{
				targetActorPath.m_ArrayIndices.Remove(targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1]);
				flag = true;
			}
			if (!flag)
			{
				break;
			}
		}
		if (targetActorPath.m_ArrayIndices.Count > 0)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(targetActorPath.m_ArrayIndices[targetActorPath.m_ArrayIndices.Count - 1]);
			if (cActor != null)
			{
				targetActorPath.m_AllyBlockingCulledPathEnd = CActor.AreActorsAllied(actor.Type, cActor.Type, actor, cActor);
				num = targetActorPath.m_ArrayIndices.Count - 1;
			}
		}
		if (targetActorPath.m_AllyBlockingCulledPathEnd && targetActorPath.m_ArrayIndices.Count > 0 && num > -1 && num != targetActorPath.m_ArrayIndices.Count && num != maxMoveCount - 1)
		{
			targetActorPath.m_AllyBlockingCulledPathEnd = false;
		}
		if (targetActorPath.m_ArrayIndicesBeforeCull.Count > 0)
		{
			CActor cActor2 = ScenarioManager.Scenario.FindActorAt(targetActorPath.m_ArrayIndicesBeforeCull.Last());
			targetActorPath.m_AllyBlockingFullPathEnd = cActor2 != null && CActor.AreActorsAllied(cActor2.Type, actor.Type, cActor2, actor);
		}
		targetActorPath.m_EndOfPathRangeToTarget = int.MaxValue;
		Point startLocation = ((targetActorPath.m_ArrayIndices.Count > 0) ? targetActorPath.m_ArrayIndices[Math.Max(targetActorPath.m_ArrayIndices.Count - 1, 0)] : actor.ArrayIndex);
		bool foundPath = false;
		List<Point> path = ScenarioManager.PathFinder.FindPath(null, startLocation, targetActorPath.m_TargetTile, ignoreBlocked: true, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false);
		if (foundPath)
		{
			targetActorPath.m_EndOfPathRangeToTarget = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, path, fly | jump, ignoreMoveCost: true, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
			targetActorPath.m_IsDisadvantage = !actor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm) && targetActorPath.m_EndOfPathRangeToTarget == 1 && range > 1;
		}
		targetActorPath.m_CrowFlyDistance = CalculateCrowFlyDistance(actor.ArrayIndex, targetActorPath.m_TargetTile);
		targetActorPath.m_HypotheticalMoveNodesLeftToTargetToGetInRange = Math.Max(CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, targetActorPath.m_ArrayIndicesBeforeCull, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) - maxMoveCount, 0);
		targetActorPath.m_ActualMoveNodesLeftToTargetToGetInRange = CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, targetActorPath.m_ArrayIndicesBeforeCull, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster) - CAbilityMove.CalculateMoveCost(actor, actor.ArrayIndex, targetActorPath.m_ArrayIndices, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
	}

	public static float CalculateCrowFlyDistance(Point start, Point end)
	{
		bool foundPath = false;
		List<Point> list = ScenarioManager.PathFinder.FindPath(null, start, end, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false);
		if (foundPath)
		{
			return list.Count;
		}
		return float.MaxValue;
	}

	public static List<List<T>> GetAllCombos<T>(List<T> list)
	{
		int num = (int)Math.Pow(2.0, list.Count) - 1;
		List<List<T>> list2 = new List<List<T>>();
		for (int i = 1; i < num + 1; i++)
		{
			list2.Add(new List<T>());
			for (int j = 0; j < list.Count; j++)
			{
				if ((i >> j) % 2 != 0)
				{
					list2.Last().Add(list[j]);
				}
			}
		}
		return list2;
	}

	public static List<List<T>> ItemCombinations<T>(List<T> inputList, int minimumItems = 1, int maximumItems = int.MaxValue)
	{
		int num = (int)Math.Pow(2.0, inputList.Count) - 1;
		List<List<T>> list = new List<List<T>>(num + 1);
		if (minimumItems == 0)
		{
			list.Add(new List<T>());
		}
		if (minimumItems <= 1 && maximumItems >= inputList.Count)
		{
			for (int i = 1; i <= num; i++)
			{
				list.Add(GenerateCombination(inputList, i));
			}
		}
		else
		{
			for (int j = 1; j <= num; j++)
			{
				int num2 = CountBits(j);
				if (num2 >= minimumItems && num2 <= maximumItems)
				{
					list.Add(GenerateCombination(inputList, j));
				}
			}
		}
		return list;
	}

	private static List<T> GenerateCombination<T>(List<T> inputList, int bitPattern)
	{
		List<T> list = new List<T>(inputList.Count);
		for (int i = 0; i < inputList.Count; i++)
		{
			if (((bitPattern >> i) & 1) == 1)
			{
				list.Add(inputList[i]);
			}
		}
		return list;
	}

	private static int CountBits(int bitPattern)
	{
		int num = 0;
		while (bitPattern != 0)
		{
			num++;
			bitPattern &= bitPattern - 1;
		}
		return num;
	}

	public static void Reset()
	{
		s_LastCalculatedPathActorAI = null;
		s_LastCalculatedPathAI = null;
		s_LastCalculatedPaths = null;
		s_OptimalPaths = null;
	}
}

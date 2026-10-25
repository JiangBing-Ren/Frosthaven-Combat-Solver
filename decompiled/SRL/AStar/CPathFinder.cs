using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Libs.Utils.Profiler;
using ScenarioRuleLibrary;

namespace AStar;

public class CPathFinder
{
	public enum EAdjacentPosition
	{
		None,
		ELeft,
		ETopLeft,
		ETopRight,
		ERight,
		EBottomLeft,
		EBottomRight
	}

	public enum ECanStepCallbackResult
	{
		Default,
		ForceCanStep,
		ForceCannotStep
	}

	public delegate ECanStepCallbackResult CanStepCallback(CPathFinderContext context, Point ptFrom, Point ptTo);

	public delegate void PrePathfindingAction(CPathFinderContext context);

	public class FindPathParams : IEquatable<FindPathParams>
	{
		public Point startLocation;

		public Point endLocation;

		public bool ignoreBlocked;

		public bool ignoreMoveCost;

		public bool ignoreDifficultTerrain;

		public bool ignoreHazardousTerrain;

		public int waterTilesThatCostZero;

		public bool Equals(FindPathParams other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (startLocation.Equals(other.startLocation) && endLocation.Equals(other.endLocation) && ignoreBlocked == other.ignoreBlocked && ignoreMoveCost == other.ignoreMoveCost && ignoreDifficultTerrain == other.ignoreDifficultTerrain && ignoreHazardousTerrain == other.ignoreHazardousTerrain)
			{
				return waterTilesThatCostZero == other.waterTilesThatCostZero;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((FindPathParams)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(startLocation, endLocation, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero);
		}
	}

	private class FindPathResult
	{
		public bool foundPath;

		public List<Point> path;
	}

	private static object s_LockObject = new object();

	private static Thread s_LockingThread;

	public static CanStepCallback s_CanStepCallback;

	private int m_Width;

	private int m_Height;

	private CNode[,] m_Nodes;

	private CNode m_StartNode;

	private CNode m_EndNode;

	private CNode m_PreferredAdjacentStartNode;

	public bool HasIceTerrain;

	private static List<Point> _adjacentLocations = new List<Point>(6);

	private static List<CNode> _adjacentNodes = new List<CNode>(6);

	[ThreadStatic]
	private static Dictionary<FindPathParams, FindPathResult> s_FindPathCache;

	[ThreadStatic]
	private static bool s_CachePathfinderResults;

	public static bool OddEvenCheck => s_OddEvenCheck;

	public static bool Locked => s_LockingThread != null;

	public CNode[,] Nodes => m_Nodes;

	public int Width => m_Width;

	public int Height => m_Height;

	public static bool s_OddEvenCheck { get; private set; }

	public CPathFinder(int width, int height, bool oddEvenCheck)
	{
		s_OddEvenCheck = oddEvenCheck;
		CreateNodes(width, height);
		_adjacentLocations = new List<Point>();
		_adjacentNodes = new List<CNode>(8);
	}

	public CNode GetNode(int x, int y)
	{
		return Nodes[x, y];
	}

	public CNode GetNode(Point arrayIndex)
	{
		return Nodes[arrayIndex.X, arrayIndex.Y];
	}

	public CNode GetNode(TileIndex index)
	{
		return Nodes[index.X, index.Y];
	}

	public CPathFinderContext InitQuery(CActor actor, Point startPoint, Point targetPoint, bool hasTarget, int maxMovementPoints, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, List<Point> blockedList, List<Point> superBlockedList, List<Point> outReachablePoints, bool ignoreDoorLocks, List<Point> transientBlockedList, bool isCalculatingDistance, bool ignoreCannotStop = false, CAbility.EAbilityType moveAbility = CAbility.EAbilityType.None)
	{
		CPathFinderContext cPathFinderContext = new CPathFinderContext(this, actor, startPoint, targetPoint, hasTarget, maxMovementPoints, ignoreBlocked, ignoreMoveCost | ignoreBlocked | ignoreDifficultTerrain, ignoreMoveCost | ignoreBlocked | ignoreHazardousTerrain, waterTilesThatCostZero, outReachablePoints, moveAbility);
		cPathFinderContext.IsCalculatingDistance = isCalculatingDistance;
		cPathFinderContext.ignoreDoorLocks = ignoreDoorLocks;
		cPathFinderContext.ignoreCannotStop = ignoreCannotStop;
		if (blockedList != null)
		{
			foreach (Point blocked in blockedList)
			{
				cPathFinderContext.GetNode(blocked).flags |= CPathFinderContext.TNode.Flags.Blocked;
			}
		}
		if (superBlockedList != null)
		{
			foreach (Point superBlocked in superBlockedList)
			{
				cPathFinderContext.GetNode(superBlocked).flags |= CPathFinderContext.TNode.Flags.SuperBlocked;
			}
		}
		if (transientBlockedList != null)
		{
			foreach (Point transientBlocked in transientBlockedList)
			{
				cPathFinderContext.GetNode(transientBlocked).flags |= CPathFinderContext.TNode.Flags.CannotStop;
			}
		}
		foreach (CTile item in ScenarioManager.CurrentScenarioState.ScenarioScripts.SelectMany((ScenarioScript x) => x.TilesCannotStopOnTopOf()))
		{
			cPathFinderContext.GetNode(item.m_ArrayIndex).flags |= CPathFinderContext.TNode.Flags.CannotStop;
		}
		foreach (CTile item2 in ScenarioManager.CurrentScenarioState.ScenarioScripts.SelectMany((ScenarioScript x) => x.TilesFullyBlockedForMovement()))
		{
			cPathFinderContext.GetNode(item2.m_ArrayIndex).flags |= CPathFinderContext.TNode.Flags.Blocked | CPathFinderContext.TNode.Flags.CannotStop;
		}
		return cPathFinderContext;
	}

	public List<Point> CalcBlockedForWaypointFloodFill(CActor movingActor, CAbility movingAbility)
	{
		if (movingActor == null)
		{
			return null;
		}
		List<Point> list = new List<Point>();
		if (movingActor.OriginalType != CActor.EType.Player && !movingAbility.GetMiscVar("CanOpenDoors").GetBool())
		{
			IEnumerable<Point> collection = from x in ScenarioManager.CurrentScenarioState.DoorProps
				where (x as CObjectDoor).DoorIsClosed
				select x.ArrayIndex.ToPoint();
			list.AddRange(collection);
		}
		return list;
	}

	public List<Point> FindAllBestDestinations(CActor actor, Point target, List<Point> transientBlocked = null, bool addWithFloodedIfEmpty = false)
	{
		List<Point> list = new List<Point>();
		FloodFill_ToRange(actor, actor.ArrayIndex, 1, ignoreBlocked: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, null, list, transientBlocked, ignoreDoorLocks: false, (CPathFinderContext context) =>
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				if (!CActor.AreActorsAllied(actor.OriginalType, allAliveActor.OriginalType, actor, allAliveActor))
				{
					context.GetNode(allAliveActor.ArrayIndex).flags |= CPathFinderContext.TNode.Flags.Blocked;
				}
			}
		});
		list.Remove(actor.ArrayIndex);
		if (list.Count == 0)
		{
			return new List<Point>();
		}
		List<Point> list2 = new List<Point>();
		int num = int.MaxValue;
		foreach (Point item in list)
		{
			if (item == target)
			{
				list2.Clear();
				list2.Add(item);
				break;
			}
			List<Point> outFloodedPoints = new List<Point>();
			CPathFinderContext cPathFinderContext = FloodFill_ToLocation(actor, item, target, ignoreBlocked: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: true, 0, null, null, outFloodedPoints, out var foundPath, null, ignoreDoorLocks: false, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: false, (CPathFinderContext context) =>
			{
				foreach (CActor allAliveActor2 in ScenarioManager.Scenario.AllAliveActors)
				{
					if (!CActor.AreActorsAllied(actor.OriginalType, allAliveActor2.OriginalType, actor, allAliveActor2))
					{
						context.GetNode(allAliveActor2.ArrayIndex).flags |= CPathFinderContext.TNode.Flags.Blocked;
					}
				}
			});
			if (!foundPath)
			{
				continue;
			}
			List<Point> list3 = new List<Point>();
			if (cPathFinderContext.GetPathTo(target, list3))
			{
				int cost = cPathFinderContext.GetCost(list3.Last());
				if (cost < num)
				{
					num = cost;
					list2.Clear();
				}
				if (cost <= num)
				{
					list2.Add(item);
				}
			}
		}
		if (addWithFloodedIfEmpty && list2.Count == 0)
		{
			list2.AddRange(list);
		}
		return list2;
	}

	public CPathFinderContext FloodFill_ToRange(CActor actor, Point startLocation, int movementRange, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, List<Point> blockedList, List<Point> superBlockedList, List<Point> outFloodedPoints, List<Point> transientBlockedList, bool ignoreDoorLocks, PrePathfindingAction PreFloodActions = null, bool isCalculatingDistance = false, bool ignoreCannotStop = false, CAbility.EAbilityType moveAbility = CAbility.EAbilityType.None)
	{
		CPathFinderContext cPathFinderContext = InitQuery(actor, startLocation, startLocation, hasTarget: false, movementRange, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, blockedList, superBlockedList, outFloodedPoints, ignoreDoorLocks, transientBlockedList, isCalculatingDistance, ignoreCannotStop, moveAbility);
		PreFloodActions?.Invoke(cPathFinderContext);
		cPathFinderContext.FloodFill();
		return cPathFinderContext;
	}

	public CPathFinderContext FloodFill_ToLocation(CActor actor, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, List<Point> blockedList, List<Point> superBlockedList, List<Point> outFloodedPoints, out bool foundPath, List<Point> transientBlockedList, bool ignoreDoorLocks, bool assignLevelObjects = false, int maxMovementPoints = int.MaxValue, bool jump = false, bool isCalculatingDistance = false, PrePathfindingAction PreFloodActions = null, bool ignoreCannotStop = false, bool fly = false)
	{
		CPathFinderContext cPathFinderContext = InitQuery(actor, startLocation, endLocation, hasTarget: true, maxMovementPoints, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, blockedList, superBlockedList, outFloodedPoints, ignoreDoorLocks, transientBlockedList, isCalculatingDistance, ignoreCannotStop);
		PreFloodActions?.Invoke(cPathFinderContext);
		if (assignLevelObjects)
		{
			bool shouldPathThroughDoors = actor is CPlayerActor;
			bool jump2 = jump;
			bool fly2 = fly;
			CAIMoveNew.AddLevelObjectsToPathFindingContext(cPathFinderContext, actor, 0, default, fly2, disallowDoorways: false, shouldPathThroughDoors, isWaterMonster: false, jump2);
		}
		foundPath = cPathFinderContext.FloodFill();
		return cPathFinderContext;
	}

	public static void EnablePathfinderCache(bool enable)
	{
		s_CachePathfinderResults = enable;
		if (!enable)
		{
			s_FindPathCache?.Clear();
		}
		else if (s_FindPathCache == null)
		{
			s_FindPathCache = new Dictionary<FindPathParams, FindPathResult>();
		}
	}

	public List<Point> FindPath(CActor actor, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, out bool foundPath, bool ignoreDoorLocks, List<Point> blockedList = null, List<Point> superBlockedList = null, List<Point> transientBlockedList = null, bool assignLevelObjects = false, int maxMovementPoints = int.MaxValue, bool jump = false, bool isCalculatingDistance = false, bool ignoreCannotStop = false, bool fly = false)
	{
		CPathFinderContext context;
		return FindPath(actor, startLocation, endLocation, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks, out context, blockedList, superBlockedList, transientBlockedList, assignLevelObjects, maxMovementPoints, jump, isCalculatingDistance, ignoreCannotStop, fly);
	}

	public List<Point> FindPath(CActor actor, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, out bool foundPath, bool ignoreDoorLocks, out CPathFinderContext context, List<Point> blockedList = null, List<Point> superBlockedList = null, List<Point> transientBlockedList = null, bool assignLevelObjects = false, int maxMovementPoints = int.MaxValue, bool jump = false, bool isCalculatingDistance = false, bool ignoreCannotStop = false, bool fly = false)
	{
		context = null;
		if (startLocation == endLocation)
		{
			foundPath = false;
			return new List<Point>();
		}
		CNode node = GetNode(startLocation);
		CNode node2 = GetNode(endLocation);
		if (s_CanStepCallback == null && IsAdjacent(startLocation, endLocation))
		{
			if (!ignoreBlocked && blockedList != null && blockedList.Contains(endLocation))
			{
				foundPath = false;
				return new List<Point>();
			}
			if (superBlockedList != null && superBlockedList.Contains(endLocation))
			{
				foundPath = false;
				return new List<Point>();
			}
			if (transientBlockedList != null && transientBlockedList.Contains(endLocation))
			{
				foundPath = false;
				return new List<Point>();
			}
			if (node.NavTo(node2, ignoreBlocked) && ScenarioScript.current?.TilesCannotStopOnTopOf()?.Contains(ScenarioManager.GetTile(node2.Location)) != true)
			{
				foundPath = true;
				return new List<Point> { endLocation };
			}
		}
		FindPathParams key = null;
		FindPathResult value = null;
		bool flag = s_CachePathfinderResults;
		if (flag)
		{
			key = new FindPathParams
			{
				startLocation = startLocation,
				endLocation = endLocation,
				ignoreBlocked = ignoreBlocked,
				ignoreMoveCost = ignoreMoveCost,
				ignoreDifficultTerrain = ignoreDifficultTerrain,
				ignoreHazardousTerrain = ignoreHazardousTerrain,
				waterTilesThatCostZero = waterTilesThatCostZero
			};
			if (s_FindPathCache.TryGetValue(key, out value))
			{
				foundPath = value.foundPath;
				return value.path.ToList();
			}
			value = new FindPathResult();
		}
		using (new MiniProfilerScope("CPathFinder.FindPath"))
		{
			context = FloodFill_ToLocation(actor, startLocation, endLocation, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, blockedList, superBlockedList, null, out foundPath, transientBlockedList, ignoreDoorLocks, assignLevelObjects, maxMovementPoints, jump, isCalculatingDistance, null, ignoreCannotStop, fly);
			List<Point> list = new List<Point>();
			context.GetPathTo(context.targetPoint, list);
			if (flag)
			{
				value.path = list.ToList();
				value.foundPath = foundPath;
				s_FindPathCache[key] = value;
			}
			return list;
		}
	}

	private void CreateNodes(int width, int height)
	{
		m_Width = width;
		m_Height = height;
		m_Nodes = new CNode[m_Width, m_Height];
		for (int i = 0; i < m_Height; i++)
		{
			for (int j = 0; j < m_Width; j++)
			{
				m_Nodes[j, i] = new CNode(j, i);
			}
		}
	}

	public void Lock(List<Point> transientBlockedList, List<Point> transientSuperBlockedList)
	{
		try
		{
			if (s_LockingThread == Thread.CurrentThread)
			{
				Log.ErrorWithReport("[ASTAR] AStar state already locked by this thread!");
				return;
			}
			Monitor.Enter(s_LockObject);
			s_LockingThread = Thread.CurrentThread;
			for (int i = 0; i < m_Height; i++)
			{
				for (int j = 0; j < m_Width; j++)
				{
					m_Nodes[j, i].SuperBlocked = false;
					m_Nodes[j, i].TransientBlocked = false;
				}
			}
			if (transientBlockedList != null)
			{
				foreach (Point transientBlocked in transientBlockedList)
				{
					if (Nodes[transientBlocked.X, transientBlocked.Y] != null)
					{
						Nodes[transientBlocked.X, transientBlocked.Y].TransientBlocked = true;
						continue;
					}
					string[] array = new string[5] { "[FindPath-Lock] - Blocked Node at x:", null, null, null, null };
					int x = transientBlocked.X;
					array[1] = x.ToString();
					array[2] = ",y:";
					x = transientBlocked.Y;
					array[3] = x.ToString();
					array[4] = " is null, skipping";
					Log.SimpleInfo(string.Concat(array));
				}
			}
			if (transientSuperBlockedList == null)
			{
				return;
			}
			foreach (Point transientSuperBlocked in transientSuperBlockedList)
			{
				if (Nodes[transientSuperBlocked.X, transientSuperBlocked.Y] != null)
				{
					Nodes[transientSuperBlocked.X, transientSuperBlocked.Y].SuperBlocked = true;
					continue;
				}
				string[] array2 = new string[5] { "[FindPath-Lock] - SuperBlocked Node at x:", null, null, null, null };
				int x = transientSuperBlocked.X;
				array2[1] = x.ToString();
				array2[2] = ",y:";
				x = transientSuperBlocked.Y;
				array2[3] = x.ToString();
				array2[4] = " is null, skipping";
				Log.SimpleInfo(string.Concat(array2));
			}
		}
		catch (Exception ex)
		{
			Log.SimpleInfo("[FindPath-Lock] - Caught exception during FindPath-Lock, logging it and ignoring it: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void Unlock()
	{
		if (s_LockingThread == null)
		{
			Log.ErrorWithReport("[ASTAR] AStar state not locked!");
			return;
		}
		for (int i = 0; i < m_Height; i++)
		{
			for (int j = 0; j < m_Width; j++)
			{
				m_Nodes[j, i].TransientBlocked = false;
			}
		}
		if (s_LockingThread != null)
		{
			s_LockingThread = null;
			Monitor.Exit(s_LockObject);
		}
	}

	public static List<Point> GetAdjacentLocations(Point fromPoint, List<Point> results)
	{
		results.Clear();
		if (s_OddEvenCheck)
		{
			if ((fromPoint.Y & 1) == 0)
			{
				results.Add(new Point(fromPoint.X - 1, fromPoint.Y - 1));
				results.Add(new Point(fromPoint.X - 1, fromPoint.Y));
				results.Add(new Point(fromPoint.X - 1, fromPoint.Y + 1));
				results.Add(new Point(fromPoint.X, fromPoint.Y + 1));
				results.Add(new Point(fromPoint.X + 1, fromPoint.Y));
				results.Add(new Point(fromPoint.X, fromPoint.Y - 1));
				return results;
			}
			results.Add(new Point(fromPoint.X - 1, fromPoint.Y));
			results.Add(new Point(fromPoint.X, fromPoint.Y + 1));
			results.Add(new Point(fromPoint.X + 1, fromPoint.Y + 1));
			results.Add(new Point(fromPoint.X + 1, fromPoint.Y));
			results.Add(new Point(fromPoint.X + 1, fromPoint.Y - 1));
			results.Add(new Point(fromPoint.X, fromPoint.Y - 1));
			return results;
		}
		results.Add(new Point(fromPoint.X - 1, fromPoint.Y - 1));
		results.Add(new Point(fromPoint.X - 1, fromPoint.Y));
		results.Add(new Point(fromPoint.X - 1, fromPoint.Y + 1));
		results.Add(new Point(fromPoint.X, fromPoint.Y + 1));
		results.Add(new Point(fromPoint.X + 1, fromPoint.Y + 1));
		results.Add(new Point(fromPoint.X + 1, fromPoint.Y));
		results.Add(new Point(fromPoint.X + 1, fromPoint.Y - 1));
		results.Add(new Point(fromPoint.X, fromPoint.Y - 1));
		return results;
	}

	public static EAdjacentPosition Get_EAdjacentPosition(Point from, Point to, bool noThrow = false)
	{
		if (from == to)
		{
			return EAdjacentPosition.None;
		}
		if (to.Y != from.Y)
		{
			to.X = (((to.Y & 1) != 0) ? (to.X + 1) : to.X);
		}
		if (to.X + 1 == from.X && to.Y == from.Y)
		{
			return EAdjacentPosition.ELeft;
		}
		if (to.X - 1 == from.X && to.Y == from.Y)
		{
			return EAdjacentPosition.ERight;
		}
		if (to.X == from.X && to.Y - 1 == from.Y)
		{
			return EAdjacentPosition.ETopLeft;
		}
		if (to.X - 1 == from.X && to.Y - 1 == from.Y)
		{
			return EAdjacentPosition.ETopRight;
		}
		if (to.X == from.X && to.Y + 1 == from.Y)
		{
			return EAdjacentPosition.EBottomLeft;
		}
		if (to.X - 1 == from.X && to.Y + 1 == from.Y)
		{
			return EAdjacentPosition.EBottomRight;
		}
		if (noThrow)
		{
			return EAdjacentPosition.None;
		}
		throw new ArgumentException("Invalid coordinates");
	}

	public static bool IsAdjacent(Point from, Point to)
	{
		return Get_EAdjacentPosition(from, to, noThrow: true) != EAdjacentPosition.None;
	}

	public static Point GetAdjacentPosition(Point pt, EAdjacentPosition eposition)
	{
		int num = pt.X;
		int num2 = pt.Y;
		switch (eposition)
		{
		case EAdjacentPosition.None:
			return pt;
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
		if (num2 != pt.Y)
		{
			num = (((num2 & 1) != 0) ? (num - 1) : num);
		}
		return new Point(num, num2);
	}

	private static EAdjacentPosition GetOpposite_EAdjacentPosition(EAdjacentPosition adjPos)
	{
		return adjPos switch
		{
			EAdjacentPosition.None => EAdjacentPosition.None, 
			EAdjacentPosition.ELeft => EAdjacentPosition.ERight, 
			EAdjacentPosition.ERight => EAdjacentPosition.ELeft, 
			EAdjacentPosition.ETopLeft => EAdjacentPosition.EBottomRight, 
			EAdjacentPosition.ETopRight => EAdjacentPosition.EBottomLeft, 
			EAdjacentPosition.EBottomLeft => EAdjacentPosition.ETopRight, 
			EAdjacentPosition.EBottomRight => EAdjacentPosition.ETopLeft, 
			_ => throw new ArgumentException("Invalid adjacent position, can't return the opposite"), 
		};
	}
}

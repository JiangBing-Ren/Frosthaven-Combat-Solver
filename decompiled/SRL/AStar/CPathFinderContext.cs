using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;

namespace AStar;

public class CPathFinderContext
{
	public struct TNode
	{
		[Flags]
		public enum Flags
		{
			Initted = 1,
			Visited = 2,
			Reachable = 4,
			Walkable = 8,
			Blocked = 0x10,
			SuperBlocked = 0x20,
			DifficultTerrain = 0x40,
			IceTerrain = 0x80,
			HazardousTerrain = 0x100,
			WaterTerrain = 0x200,
			Trap = 0x400,
			Bridge = 0x800,
			CannotStop = 0x1000,
			BridgeOpen = 0x2000,
			SuperNegative = 0x4000
		}

		public struct TCost
		{
			public int movePoints;

			public int numHexes;

			public int numSlides;

			public int numDifficultTerrains;

			public int numWaterTerrains;

			public int CompareTo(TCost cost)
			{
				int num = movePoints.CompareTo(cost.movePoints);
				if (num != 0)
				{
					return num;
				}
				int num2 = (numSlides + numDifficultTerrains + numWaterTerrains).CompareTo(cost.numSlides + cost.numDifficultTerrains + cost.numWaterTerrains);
				if (num2 != 0)
				{
					return num2;
				}
				int num3 = numHexes.CompareTo(cost.numHexes);
				if (num3 != 0)
				{
					return num3;
				}
				return 0;
			}

			public TCost Add(TCost cost)
			{
				return new TCost
				{
					movePoints = movePoints + cost.movePoints,
					numHexes = numHexes + cost.numHexes,
					numSlides = numSlides + cost.numSlides,
					numDifficultTerrains = numDifficultTerrains + cost.numDifficultTerrains,
					numWaterTerrains = numWaterTerrains + cost.numWaterTerrains
				};
			}

			public override string ToString()
			{
				return $"MP: {movePoints}, hexes: {numHexes}, slides: {numSlides}, difficult: {numDifficultTerrains}, water: {numWaterTerrains}";
			}
		}

		public Flags flags;

		public TCost cost;

		public Point prev;

		public CPathFinder.EAdjacentPosition slideDirection;

		public int estimatedCostToTarget;

		public int islandID;

		public List<CNavLink> navLinks;

		public bool HasFlag(Flags flag)
		{
			return (flags & flag) != 0;
		}

		public bool HasAllFlags(Flags flag)
		{
			return (flags & flag) == flag;
		}
	}

	public struct FrontierHeapNode
	{
		public Point pt;

		public TNode.TCost eval;
	}

	public const int TRAP_COST = 10000;

	public CPathFinder pathFinder;

	public CActor actor;

	public Point startPoint;

	public Point targetPoint;

	public bool hasTarget;

	public int maxMovementPoints;

	public int waterTilesThatCostZero;

	public bool ignoreBlocked;

	public bool ignoreTraps;

	public bool ignoreIceSlideEffect;

	public bool ignoreIceSlideWeight;

	public bool ignoreDifficultTerrain;

	public bool ignoreHazardousTerrain;

	public bool ignoreNavLinks;

	public bool fillEntireLevel;

	public bool useAStar;

	public bool ignoreDoorLocks;

	public bool ignoreCannotStop;

	public Point? freeHexesAwayFromThisTile;

	public bool IsCalculatingDistance;

	private TNode[,] nodes;

	private Queue<Point> frontierQueue;

	private MinHeap<FrontierHeapNode> frontierHeap;

	public List<Point> outReachablePoints;

	private List<Point> _adjacentLocations = new List<Point>(6);

	public CPathFinderContext(CPathFinder pathFinder, CActor actor, Point startPoint, Point targetPoint, bool hasTarget, int maxMovementPoints, bool ignoreBlocked, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, List<Point> outReachablePoints, CAbility.EAbilityType moveAbility = CAbility.EAbilityType.None)
	{
		this.pathFinder = pathFinder;
		this.actor = actor;
		this.startPoint = startPoint;
		this.targetPoint = targetPoint;
		this.hasTarget = hasTarget;
		this.maxMovementPoints = maxMovementPoints;
		this.ignoreBlocked = ignoreBlocked;
		ignoreTraps = ignoreHazardousTerrain;
		this.ignoreDifficultTerrain = ignoreDifficultTerrain;
		this.ignoreHazardousTerrain = ignoreHazardousTerrain;
		ignoreNavLinks = false;
		this.waterTilesThatCostZero = waterTilesThatCostZero;
		this.outReachablePoints = outReachablePoints;
		if (actor != null)
		{
			if (moveAbility == CAbility.EAbilityType.None || moveAbility == CAbility.EAbilityType.Move)
			{
				ignoreIceSlideEffect = actor.IgnoreIceSlideEffect;
				ignoreIceSlideWeight = actor.IgnoreIceSlideWeight;
			}
			actor.HasFreeTileAwayFromCaster(out freeHexesAwayFromThisTile);
		}
		useAStar = hasTarget;
		nodes = new TNode[pathFinder.Width, pathFinder.Height];
		if (hasTarget)
		{
			frontierHeap = new MinHeap<FrontierHeapNode>(FrontierHeapCompare);
		}
		else
		{
			frontierQueue = new Queue<Point>(128);
		}
		outReachablePoints?.Add(startPoint);
		ref TNode node = ref GetNode(startPoint);
		node.flags |= TNode.Flags.Visited | TNode.Flags.Reachable;
		node.prev = startPoint;
		outReachablePoints?.Add(startPoint);
		if (!hasTarget || targetPoint != startPoint)
		{
			PushToFrontier(startPoint);
		}
	}

	public void CopyNodes(CPathFinderContext context)
	{
		for (int i = 0; i < pathFinder.Height; i++)
		{
			for (int j = 0; j < pathFinder.Width; j++)
			{
				Point pt = new Point(j, i);
				ref TNode node = ref context.GetNode(pt, init: false);
				if (node.HasFlag(TNode.Flags.Initted))
				{
					GetNode(pt).flags = node.flags & ~(TNode.Flags.Visited | TNode.Flags.Reachable);
				}
			}
		}
	}

	public int GetCost(Point point)
	{
		return GetNode(point).cost.movePoints;
	}

	public void Reset(Point startPoint, Point targetPoint, bool hasTarget = true, List<Point> outReachablePoints = null)
	{
		this.startPoint = startPoint;
		this.targetPoint = targetPoint;
		this.hasTarget = hasTarget;
		this.outReachablePoints = outReachablePoints;
		useAStar = hasTarget;
		for (int i = 0; i < pathFinder.Height; i++)
		{
			for (int j = 0; j < pathFinder.Width; j++)
			{
				Point point = new Point(j, i);
				ref TNode node = ref GetNode(point, init: false);
				node.flags &= ~(TNode.Flags.Visited | TNode.Flags.Reachable);
				node.cost = default;
				node.prev = default;
				node.slideDirection = CPathFinder.EAdjacentPosition.None;
				if (useAStar && node.HasFlag(TNode.Flags.Initted))
				{
					node.estimatedCostToTarget = Point.HexDist(point, targetPoint);
				}
				else
				{
					node.estimatedCostToTarget = 0;
				}
			}
		}
		if (hasTarget)
		{
			if (frontierHeap == null)
			{
				frontierHeap = new MinHeap<FrontierHeapNode>(FrontierHeapCompare);
			}
		}
		else if (frontierQueue == null)
		{
			frontierQueue = new Queue<Point>(128);
		}
		frontierQueue?.Clear();
		frontierHeap?.Clear();
		ref TNode node2 = ref GetNode(startPoint);
		node2.flags |= TNode.Flags.Visited | TNode.Flags.Reachable;
		node2.prev = startPoint;
		outReachablePoints?.Add(startPoint);
		if (!hasTarget || targetPoint != startPoint)
		{
			PushToFrontier(startPoint);
		}
	}

	public bool Contains(Point pt)
	{
		if (pt.X < 0 || pt.Y < 0 || pt.X >= pathFinder.Width || pt.Y >= pathFinder.Height)
		{
			return false;
		}
		return true;
	}

	public ref TNode GetNode(Point pt, bool init = true)
	{
		ref TNode reference = ref nodes[pt.X, pt.Y];
		if (!reference.HasFlag(TNode.Flags.Initted))
		{
			reference.flags |= TNode.Flags.Initted;
			CNode node = pathFinder.GetNode(pt);
			if (node.Walkable)
			{
				reference.flags |= TNode.Flags.Walkable;
			}
			if (node.Blocked)
			{
				reference.flags |= TNode.Flags.Blocked;
			}
			if (node.IsDifficultTerrain)
			{
				reference.flags |= TNode.Flags.DifficultTerrain;
			}
			if (node.IsWaterTerrain)
			{
				reference.flags |= TNode.Flags.WaterTerrain;
			}
			if (node.IsIceTerrain)
			{
				reference.flags |= TNode.Flags.IceTerrain;
			}
			if (node.IsBridge)
			{
				reference.flags |= TNode.Flags.Bridge;
			}
			if (node.IsBridgeOpen)
			{
				reference.flags |= TNode.Flags.BridgeOpen;
			}
			reference.islandID = node.IslandID;
			if (!ignoreNavLinks && node.NavLinks != null && node.NavLinks.Count > 0)
			{
				reference.navLinks = node.NavLinks;
			}
			if (useAStar)
			{
				reference.estimatedCostToTarget = Point.HexDist(pt, targetPoint);
			}
		}
		return ref reference;
	}

	public static int FrontierHeapCompare(FrontierHeapNode lhs, FrontierHeapNode rhs)
	{
		return lhs.eval.CompareTo(rhs.eval);
	}

	public void PushToFrontier(Point pt)
	{
		if (hasTarget)
		{
			ref TNode node = ref GetNode(pt);
			TNode.TCost cost = node.cost;
			if (useAStar)
			{
				cost.movePoints += node.estimatedCostToTarget;
			}
			frontierHeap.Add(new FrontierHeapNode
			{
				pt = pt,
				eval = cost
			});
		}
		else
		{
			frontierQueue.Enqueue(pt);
		}
	}

	public bool PopFromFrontier(out Point pt)
	{
		if (hasTarget)
		{
			if (frontierHeap.Count == 0)
			{
				pt = default;
				return false;
			}
			pt = frontierHeap.Remove().pt;
			return true;
		}
		if (frontierQueue.Count == 0)
		{
			pt = default;
			return false;
		}
		pt = frontierQueue.Dequeue();
		return true;
	}

	public bool FloodFill()
	{
		Point pt;
		while (PopFromFrontier(out pt))
		{
			ref TNode node = ref GetNode(pt);
			if (node.cost.movePoints >= maxMovementPoints && !fillEntireLevel)
			{
				continue;
			}
			CPathFinder.GetAdjacentLocations(pt, _adjacentLocations);
			if (!ignoreNavLinks && node.navLinks != null)
			{
				foreach (CNavLink navLink in node.navLinks)
				{
					if (ignoreBlocked || !navLink.Blocked)
					{
						Point ptNext = navLink.DestinationNode.Location;
						CPathFinder.EAdjacentPosition eAdjacentPosition = SlideOnIce(ref ptNext, pt, ref node, out var numSlides);
						ref TNode node2 = ref GetNode(ptNext);
						int num = ((eAdjacentPosition != CPathFinder.EAdjacentPosition.None) ? 1 : CalcWeight(ptNext, ref node2, pt, ref node, navLink.Weight));
						if ((!ignoreTraps && node2.HasFlag(TNode.Flags.Trap)) || (!ignoreHazardousTerrain && node2.HasFlag(TNode.Flags.HazardousTerrain)))
						{
							num += 10000;
						}
						else if (node2.HasFlag(TNode.Flags.SuperNegative))
						{
							num += 10000;
						}
						Visit(ptNext, ref node2, num, eAdjacentPosition, numSlides, pt, ref node);
						if (hasTarget && ptNext == targetPoint && node2.HasFlag(TNode.Flags.Visited))
						{
							return node2.HasFlag(TNode.Flags.Reachable);
						}
						_adjacentLocations.Remove(ptNext);
					}
				}
			}
			foreach (Point adjacentLocation in _adjacentLocations)
			{
				if (Contains(adjacentLocation))
				{
					Point ptNext2 = adjacentLocation;
					CPathFinder.EAdjacentPosition eAdjacentPosition2 = SlideOnIce(ref ptNext2, pt, ref node, out var numSlides2);
					ref TNode node3 = ref GetNode(ptNext2);
					int num2 = ((eAdjacentPosition2 != CPathFinder.EAdjacentPosition.None) ? 1 : CalcWeight(ptNext2, ref node3, pt, ref node));
					if ((!ignoreTraps && node3.HasFlag(TNode.Flags.Trap)) || (!ignoreHazardousTerrain && node3.HasFlag(TNode.Flags.HazardousTerrain)))
					{
						num2 += 10000;
					}
					else if (node3.HasFlag(TNode.Flags.SuperNegative))
					{
						num2 += 10000;
					}
					Visit(ptNext2, ref node3, num2, eAdjacentPosition2, numSlides2, pt, ref node);
					if (hasTarget && ptNext2 == targetPoint && node3.HasFlag(TNode.Flags.Visited))
					{
						return node3.HasFlag(TNode.Flags.Reachable);
					}
				}
			}
		}
		return false;
	}

	public static int CostWithoutTraps(int costWithTraps)
	{
		return costWithTraps % 10000;
	}

	public static int NumTraps(int costWithTraps)
	{
		return costWithTraps / 10000;
	}

	public bool GetPathTo(Point targetPoint, List<Point> outPath, int maxMoveCost = int.MaxValue)
	{
		outPath.Clear();
		if (!Contains(targetPoint))
		{
			return false;
		}
		if (!GetNode(targetPoint).HasFlag(TNode.Flags.Visited))
		{
			return false;
		}
		Point point = targetPoint;
		while (point != startPoint)
		{
			ref TNode node = ref GetNode(point);
			if (maxMoveCost != int.MaxValue && CostWithoutTraps(node.cost.movePoints) > maxMoveCost)
			{
				point = node.prev;
				continue;
			}
			if (outPath.Count > 1000)
			{
				Log.ErrorWithReport("Infinite loop in CPathFinder.GetPathTo");
				break;
			}
			outPath.Add(point);
			if (node.slideDirection != CPathFinder.EAdjacentPosition.None)
			{
				AddSlidePoints(point, ref node, outPath);
			}
			point = node.prev;
		}
		outPath.Reverse();
		return true;
	}

	private void AddSlidePoints(Point pt, ref TNode node, List<Point> outPath)
	{
		int count = outPath.Count;
		Point pt2 = node.prev;
		while (true)
		{
			Point adjacentPosition = CPathFinder.GetAdjacentPosition(pt2, node.slideDirection);
			if (!(adjacentPosition == pt))
			{
				outPath.Insert(count, adjacentPosition);
				pt2 = adjacentPosition;
				continue;
			}
			break;
		}
	}

	public CPathFinder.EAdjacentPosition SlideOnIce(ref Point ptNext, Point ptPrev, ref TNode nodePrev, out int numSlides)
	{
		numSlides = 0;
		if (ignoreBlocked || ignoreIceSlideEffect)
		{
			return CPathFinder.EAdjacentPosition.None;
		}
		ref TNode node = ref GetNode(ptNext);
		if (!node.HasFlag(TNode.Flags.IceTerrain))
		{
			return CPathFinder.EAdjacentPosition.None;
		}
		if (!CanStep(ptNext, ref node, ptPrev, ref nodePrev))
		{
			return CPathFinder.EAdjacentPosition.None;
		}
		CPathFinder.EAdjacentPosition eAdjacentPosition = CPathFinder.Get_EAdjacentPosition(ptPrev, ptNext, noThrow: true);
		if (eAdjacentPosition == CPathFinder.EAdjacentPosition.None)
		{
			return CPathFinder.EAdjacentPosition.None;
		}
		ref TNode node2;
		do
		{
			Point adjacentPosition = CPathFinder.GetAdjacentPosition(ptNext, eAdjacentPosition);
			if (!Contains(adjacentPosition))
			{
				break;
			}
			node2 = ref GetNode(adjacentPosition);
			Point point = ptNext;
			if (!CanStep(adjacentPosition, ref node2, point, ref GetNode(point)) || IsSlideBlockedByFigure(adjacentPosition))
			{
				break;
			}
			ptNext = adjacentPosition;
			numSlides++;
		}
		while (node2.HasFlag(TNode.Flags.IceTerrain));
		return eAdjacentPosition;
	}

	private bool IsSlideBlockedByFigure(Point pt)
	{
		if (ScenarioManager.Scenario.FindActorAt(pt, actor) != null)
		{
			return true;
		}
		return false;
	}

	private bool IsDoorLocked(Point pt)
	{
		CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.Tiles[pt.X, pt.Y].FindProp(ScenarioManager.ObjectImportType.Door);
		if (cObjectDoor != null && cObjectDoor.DoorIsLocked)
		{
			return true;
		}
		return false;
	}

	private bool CanStep(Point ptNext, ref TNode nodeNext, Point ptPrev, ref TNode nodePrev, bool __skipCheckForBlockedAndImpassable = false, bool skipIslandCheck = false)
	{
		if (CPathFinder.s_CanStepCallback != null)
		{
			switch (CPathFinder.s_CanStepCallback(this, ptPrev, ptNext))
			{
			case CPathFinder.ECanStepCallbackResult.ForceCanStep:
				return true;
			case CPathFinder.ECanStepCallbackResult.ForceCannotStep:
				return false;
			}
		}
		if (!__skipCheckForBlockedAndImpassable)
		{
			if (!nodeNext.HasFlag(TNode.Flags.Walkable))
			{
				return false;
			}
			if (nodeNext.HasFlag(TNode.Flags.SuperBlocked))
			{
				return false;
			}
			if (nodeNext.HasFlag(TNode.Flags.Bridge) && !nodeNext.HasAllFlags(TNode.Flags.BridgeOpen) && IsDoorLocked(ptNext))
			{
				if (ignoreDoorLocks || (actor != null && actor.CanUnlockDoors))
				{
					return true;
				}
				return false;
			}
			if (!ignoreBlocked && nodeNext.HasFlag(TNode.Flags.Blocked))
			{
				return false;
			}
		}
		if (!ignoreNavLinks && nodePrev.navLinks != null)
		{
			foreach (CNavLink navLink in nodePrev.navLinks)
			{
				if ((ignoreBlocked || !navLink.Blocked) && navLink.DestinationNode.Location == ptNext)
				{
					return true;
				}
			}
		}
		if (!skipIslandCheck && nodeNext.islandID != nodePrev.islandID && !nodeNext.HasAllFlags(TNode.Flags.BridgeOpen) && !nodePrev.HasAllFlags(TNode.Flags.BridgeOpen) && !nodeNext.HasFlag(TNode.Flags.Bridge) && !nodePrev.HasFlag(TNode.Flags.Bridge))
		{
			return false;
		}
		return true;
	}

	private int CalcWeight(Point ptNext, ref TNode nodeNext, Point ptPrev, ref TNode nodePrev, int val = 1)
	{
		int num = 0;
		if (freeHexesAwayFromThisTile.HasValue && maxMovementPoints - nodePrev.cost.movePoints > 0 && Point.HexDist(freeHexesAwayFromThisTile.Value, ptPrev) == 1 && Point.HexDist(freeHexesAwayFromThisTile.Value, ptNext) > 1)
		{
			num = 1;
		}
		if (nodeNext.HasFlag(TNode.Flags.IceTerrain))
		{
			if (ignoreIceSlideWeight)
			{
				return 0;
			}
			return val - num;
		}
		int num2 = nodePrev.cost.numWaterTerrains + (nodeNext.HasFlag(TNode.Flags.WaterTerrain) ? 1 : 0);
		if (waterTilesThatCostZero != 0 && num2 <= waterTilesThatCostZero && nodeNext.HasFlag(TNode.Flags.WaterTerrain))
		{
			return 0;
		}
		if (!ignoreDifficultTerrain && nodeNext.HasFlag(TNode.Flags.DifficultTerrain))
		{
			return val + 1 - num;
		}
		return val - num;
	}

	private void Visit(Point ptNext, ref TNode nodeNext, int weight, CPathFinder.EAdjacentPosition slideDir, int numSlides, Point ptPrev, ref TNode nodePrev)
	{
		if (!CanStep(ptNext, ref nodeNext, ptPrev, ref nodePrev, __skipCheckForBlockedAndImpassable: true, slideDir != CPathFinder.EAdjacentPosition.None))
		{
			return;
		}
		TNode.TCost cost = nodePrev.cost;
		cost.movePoints += weight;
		int num = CostWithoutTraps(cost.movePoints);
		if (num > maxMovementPoints && !fillEntireLevel)
		{
			return;
		}
		cost.numHexes += Point.HexDist(ptPrev, ptNext);
		cost.numSlides += numSlides;
		if (waterTilesThatCostZero != 0 && nodeNext.HasFlag(TNode.Flags.WaterTerrain))
		{
			cost.numWaterTerrains++;
		}
		if (!ignoreDifficultTerrain && nodeNext.HasFlag(TNode.Flags.DifficultTerrain))
		{
			cost.numDifficultTerrains++;
		}
		if (nodeNext.HasFlag(TNode.Flags.Visited))
		{
			if (nodeNext.cost.CompareTo(cost) <= 0)
			{
				return;
			}
		}
		else
		{
			nodeNext.flags |= TNode.Flags.Visited;
		}
		nodeNext.cost = cost;
		nodeNext.prev = ptPrev;
		nodeNext.slideDirection = slideDir;
		if (!CanStep(ptNext, ref nodeNext, ptPrev, ref nodePrev, __skipCheckForBlockedAndImpassable: false, slideDir != CPathFinder.EAdjacentPosition.None))
		{
			return;
		}
		if (num <= maxMovementPoints && !nodeNext.HasFlag(TNode.Flags.Reachable))
		{
			bool flag = !nodeNext.HasFlag(TNode.Flags.CannotStop) || ignoreCannotStop;
			if (flag && !ignoreBlocked && nodeNext.HasFlag(TNode.Flags.Blocked))
			{
				flag = false;
			}
			if (flag)
			{
				nodeNext.flags |= TNode.Flags.Reachable;
				if (outReachablePoints != null)
				{
					outReachablePoints.Add(ptNext);
				}
			}
		}
		else if (num > maxMovementPoints && nodeNext.HasFlag(TNode.Flags.Reachable))
		{
			nodeNext.flags &= ~TNode.Flags.Reachable;
			if (outReachablePoints != null)
			{
				outReachablePoints.Remove(ptNext);
			}
		}
		PushToFrontier(ptNext);
	}
}

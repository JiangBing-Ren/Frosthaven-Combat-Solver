using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AStar;

namespace ScenarioRuleLibrary;

public class MF
{
	public static string RefStr(object obj)
	{
		if (obj == null)
		{
			return "null";
		}
		if (StateDump.IsDumping)
		{
			return "";
		}
		return RuntimeHelpers.GetHashCode(obj).ToString("X8");
	}

	public static List<CTile> FindMapDoors(CTile originTile)
	{
		List<CTile> list = new List<CTile>();
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].IsBridge && (cTile.m_HexMap == originTile.m_HexMap || cTile.m_Hex2Map == originTile.m_HexMap))
			{
				list.Add(cTile);
			}
		}
		return list;
	}

	public static bool IsBlockingSpawnPosition(CTile spawnTile, List<CTile> selectedTiles, CActor actorSpawningBlocker, ref List<CTile> extraUnblockedTiles, List<CTile> unblockedTiles = null)
	{
		List<Point> list = new List<Point>();
		if (selectedTiles != null)
		{
			foreach (CTile selectedTile in selectedTiles)
			{
				list.Add(selectedTile.m_ArrayIndex);
			}
		}
		if (unblockedTiles != null)
		{
			foreach (CTile unblockedTile in unblockedTiles)
			{
				list.RemoveAll((Point x) => x.Equals(unblockedTile.m_ArrayIndex));
			}
		}
		if (spawnTile != null)
		{
			list.Add(spawnTile.m_ArrayIndex);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			list.Add(@object.ArrayIndex);
		}
		ScenarioManager.PathFinder.Lock(list, null);
		CTile originTile = ScenarioManager.Tiles[actorSpawningBlocker.ArrayIndex.X, actorSpawningBlocker.ArrayIndex.Y];
		bool flag = true;
		if (spawnTile != null)
		{
			flag = spawnTile.FindProp(ScenarioManager.ObjectImportType.Door) != null;
			if (!flag)
			{
				foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(spawnTile))
				{
					CObjectDoor cObjectDoor = allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) as CObjectDoor;
					if (((ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].TransientBlocked) && ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Walkable) || (cObjectDoor != null && !cObjectDoor.DoorIsOpen))
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			List<CTile> allUnblockedTilesFromOriginNonRecursive = ScenarioManager.GetAllUnblockedTilesFromOriginNonRecursive(originTile, unblockedTiles);
			List<CTile> list2 = new List<CTile>();
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile in tiles)
			{
				if (cTile != null && (cTile.m_HexMap == null || cTile.m_HexMap.Revealed || cTile.m_Hex2Map == null || cTile.m_Hex2Map.Revealed) && ScenarioManager.RoomsBetweenTilesRevealed(originTile, cTile) && (!ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].Blocked || (unblockedTiles != null && unblockedTiles.Contains(cTile))) && !ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].TransientBlocked && ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].Walkable)
				{
					list2.Add(cTile);
				}
			}
			ScenarioManager.PathFinder.Unlock();
			extraUnblockedTiles = list2.Except(extraUnblockedTiles).Except(allUnblockedTilesFromOriginNonRecursive).ToList();
			return extraUnblockedTiles.Any();
		}
		ScenarioManager.PathFinder.Unlock();
		return false;
	}

	public static bool IsZero(double d)
	{
		return Math.Abs(d) < 1E-10;
	}

	public static float SnapFloat(float snapsize, float val)
	{
		return snapsize * (float)(int)(Math.Abs(val) / snapsize + 0.5f) * (float)Math.Sign(val);
	}

	public static void ArrayIndexToCartesianCoord(Point arrayIndex, float xScalar, float yScalar, out float x, out float y)
	{
		x = ((float)arrayIndex.X + (((arrayIndex.Y & 1) == 1) ? 0.5f : 0f)) * xScalar;
		y = (float)arrayIndex.Y * yScalar;
	}
}

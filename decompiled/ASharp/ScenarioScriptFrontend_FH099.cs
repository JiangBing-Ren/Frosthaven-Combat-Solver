using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH099 : ScenarioScriptFrontend
{
	private const int PlatformLeftLowerPlaneID = 0;

	private const int PlatformLeftHigherPlaneID = 1;

	private const int PlatformRightLowerPlaneID = 2;

	private const int PlatformRightHigherPlaneID = 3;

	private const int PlatformLowerWallPlaneID = 5;

	private const int PlatformHigherWallPlaneID = 6;

	private const int groundPlaneID = 4;

	private static Plane platformLeftLowerPlane;

	private static Plane platformLeftHigherPlane;

	private static Plane platformRightLowerPlane;

	private static Plane platformRightHigherPlane;

	private static Plane platformLowerWallPlane;

	private static Plane platformHigherWallPlane;

	private static Plane groundPlane;

	private ScenarioScript_FH099 _script099 => (ScenarioScript_FH099)_script;

	public override bool _ManualActorHeightControl => true;

	protected override bool GetPlatformID(CTile tile, object logicObject, out int platformID)
	{
		if (platformLeftLowerTiles().Contains(tile))
		{
			platformID = 0;
			return true;
		}
		if (platformLeftHigherTiles().Contains(tile))
		{
			platformID = 1;
			return true;
		}
		if (platformRightLowerTiles().Contains(tile))
		{
			platformID = 2;
			return true;
		}
		if (platformRightHigherTiles().Contains(tile))
		{
			platformID = 3;
			return true;
		}
		platformID = 4;
		return false;
	}

	protected override bool GetPlane(int platformID, out Plane plane)
	{
		switch (platformID)
		{
		case 0:
			plane = platformLeftLowerPlane;
			return true;
		case 1:
			plane = platformLeftHigherPlane;
			return true;
		case 2:
			plane = platformRightLowerPlane;
			return true;
		case 3:
			plane = platformRightHigherPlane;
			return true;
		case 5:
			plane = platformLowerWallPlane;
			return true;
		case 6:
			plane = platformHigherWallPlane;
			return true;
		case 4:
			plane = groundPlane;
			return true;
		default:
			plane = default;
			return false;
		}
	}

	protected override bool GetPlaneHeights(int platformID, out float minHeight, out float maxHeight)
	{
		switch (platformID)
		{
		case 0:
			minHeight = 0f;
			maxHeight = 1f;
			return true;
		case 1:
			minHeight = 1f;
			maxHeight = 2f;
			return true;
		case 2:
			minHeight = 0f;
			maxHeight = 1f;
			return true;
		case 3:
			minHeight = 1f;
			maxHeight = 2f;
			return true;
		case 4:
			minHeight = 0f;
			maxHeight = 0f;
			return true;
		default:
			minHeight = 0f;
			maxHeight = 0f;
			return false;
		}
	}

	private IEnumerable<string> platformLowerWalls()
	{
		yield return "Wall 16";
		yield return "Wall 15";
		yield return "Wall 14";
		yield return "Wall 20";
		yield return "Wall 21";
		yield return "Wall 22";
	}

	private IEnumerable<string> paltformHigherWalls()
	{
		yield return "Wall 5";
		yield return "Wall 6";
		yield return "Wall 7";
		yield return "Wall 31";
		yield return "Wall 30";
		yield return "Wall 29";
	}

	private IEnumerable<CTile> platformLeftLowerTiles()
	{
		for (int x = 2; x <= 7; x++)
		{
			for (int y = 15; y <= 17; y++)
			{
				CTile tile = ScenarioManager.GetTile(x, y);
				if (tile != null)
				{
					yield return tile;
				}
			}
		}
	}

	private IEnumerable<string> platformLeftLowerWalls()
	{
		yield return "Wall 3";
		yield return "Wall 4";
		yield return "Wall 12";
		yield return "Wall 13";
	}

	private IEnumerable<Bounds> platformLeftLowerBounds()
	{
		yield return new Bounds(new Vector3(0.3519907f, 1.703421f, 12.597588f), new Vector3(14.820295f, 6.95446f, 4.40876f));
	}

	private IEnumerable<CTile> platformLeftHigherTiles()
	{
		for (int x = 1; x <= 6; x++)
		{
			for (int y = 18; y <= 21; y++)
			{
				CTile tile = ScenarioManager.GetTile(x, y);
				if (tile != null)
				{
					yield return tile;
				}
			}
		}
	}

	private IEnumerable<string> platformLeftHigherWalls()
	{
		yield return "Wall 8";
		yield return "Wall 9";
		yield return "Wall 10";
		yield return "Wall 11";
	}

	private IEnumerable<Bounds> platformLeftHigherBounds()
	{
		yield return new Bounds(new Vector3(1.672599f, 1.7034211f, 20.134163f), new Vector3(10.904008f, 6.95446f, 10.283188f));
	}

	private IEnumerable<CTile> platformRightLowerTiles()
	{
		for (int x = 2; x <= 7; x++)
		{
			for (int y = 5; y <= 7; y++)
			{
				CTile tile = ScenarioManager.GetTile(x, y);
				if (tile != null)
				{
					yield return tile;
				}
			}
		}
	}

	private IEnumerable<string> platformRightLowerWalls()
	{
		yield return "Wall 23";
		yield return "Wall 24";
		yield return "Wall 32";
		yield return "Wall 33";
		yield return "Wall 34";
	}

	private IEnumerable<Bounds> platformRightLowerBounds()
	{
		yield return new Bounds(new Vector3(0.3519907f, 1.7034211f, -3.5457096f), new Vector3(14.820295f, 6.95446f, 4.4543f));
	}

	private IEnumerable<CTile> platformRightHigherTiles()
	{
		for (int x = 1; x <= 6; x++)
		{
			for (int y = 1; y <= 4; y++)
			{
				CTile tile = ScenarioManager.GetTile(x, y);
				if (tile != null)
				{
					yield return tile;
				}
			}
		}
	}

	private IEnumerable<string> platformRightHigherWalls()
	{
		yield return "Wall 25";
		yield return "Wall 26";
		yield return "Wall 27";
		yield return "Wall 28";
	}

	private IEnumerable<Bounds> platformRightHigherBounds()
	{
		yield return new Bounds(new Vector3(1.8092132f, 1.7034211f, -11.082283f), new Vector3(10.995086f, 6.95446f, 10.693034f));
	}

	protected override void CacheRendHeights()
	{
		base.CacheRendHeights();
		CMap cMap = ScenarioScript.FindRoom("Room_1");
		CacheWallsForPlatform(5, cMap.MapGuid, visible: true, platformLowerWalls().ToList());
		CacheWallsForPlatform(6, cMap.MapGuid, visible: true, paltformHigherWalls().ToList());
		CacheTilesForPlatform(3, platformRightHigherTiles().ToList(), visible: true);
		CacheWallsForPlatform(3, cMap.MapGuid, visible: true, platformRightHigherWalls().ToList());
		CacheTilesForPlatform(2, platformRightLowerTiles().ToList(), visible: true);
		CacheWallsForPlatform(2, cMap.MapGuid, visible: true, platformRightLowerWalls().ToList());
		CacheTilesForPlatform(1, platformLeftHigherTiles().ToList(), visible: true);
		CacheWallsForPlatform(1, cMap.MapGuid, visible: true, platformLeftHigherWalls().ToList());
		CacheTilesForPlatform(0, platformLeftLowerTiles().ToList(), visible: true);
		CacheWallsForPlatform(0, cMap.MapGuid, visible: true, platformLeftLowerWalls().ToList());
		List<string> objectNames = new List<string> { "Clutter", "Scatter", "Lightshafts" };
		CacheObjectsInRoomForPlatform(0, cMap.MapGuid, objectNames, visible: true, platformLeftLowerBounds().ToList());
		CacheObjectsInRoomForPlatform(1, cMap.MapGuid, objectNames, visible: true, platformLeftHigherBounds().ToList());
		CacheObjectsInRoomForPlatform(2, cMap.MapGuid, objectNames, visible: true, platformRightLowerBounds().ToList());
		CacheObjectsInRoomForPlatform(3, cMap.MapGuid, objectNames, visible: true, platformRightHigherBounds().ToList());
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		platformLeftLowerPlane = new Plane(Quaternion.Euler(-30f, 0f, 0f) * Vector3.up, new Vector3(0f, 0.5f, 10.42418f));
		platformLeftHigherPlane = new Plane(Quaternion.Euler(-30f, 0f, 0f) * Vector3.up, new Vector3(0f, 1.5f, 14.89169f));
		platformRightLowerPlane = new Plane(Quaternion.Euler(30f, 0f, 0f) * Vector3.up, new Vector3(0f, 0.5f, -1.489169f));
		platformRightHigherPlane = new Plane(Quaternion.Euler(30f, 0f, 0f) * Vector3.up, new Vector3(0f, 1.5f, -5.956676f));
		platformLowerWallPlane = new Plane(Vector3.up, new Vector3(0f, 1f, 0f));
		platformHigherWallPlane = new Plane(Vector3.up, new Vector3(0f, 2f, 0f));
		groundPlane = new Plane(Vector3.up, Vector3.zero);
	}

	[MapTileGenBeforeLights]
	public static void CheckRoomWallsForAdjustmentsBeforeLights(ProceduralMapTile mapTile)
	{
		CMap cMap = ScenarioScript.FindRoom("Room_1");
		_ = mapTile.name == cMap.MapInstanceName;
	}
}

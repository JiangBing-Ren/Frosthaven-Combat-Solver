using System;
using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.EnvironmentFX;
using FH.LevelComponents.ProcGen;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH015 : ScenarioScriptFrontend
{
	private const int Platform0PlaneID = 0;

	private const int Platform1PlaneID = 1;

	private static Plane platform0Plane;

	private static Plane platform1Plane;

	private ScenarioScript_FH015 _script015 => (ScenarioScript_FH015)_script;

	public override bool _ManualActorHeightControl => true;

	protected override bool UpdateHeights()
	{
		return base.UpdateHeights();
	}

	protected override bool GetPlatformID(CTile tile, object logicObject, out int platformID)
	{
		if (tile.m_HexMap.RoomName == "Room_1" || tile.m_HexMap.RoomName == "Room_3")
		{
			platformID = 0;
			return true;
		}
		platformID = 1;
		return false;
	}

	protected override bool GetPlane(int platformID, out Plane plane)
	{
		switch (platformID)
		{
		case 0:
			plane = platform0Plane;
			return true;
		case 1:
			plane = platform1Plane;
			return true;
		default:
			plane = default;
			return false;
		}
	}

	protected override void CacheRendHeights()
	{
		base.CacheRendHeights();
		CMap cMap = ScenarioScript.FindRoom("Room_1");
		CMap cMap2 = ScenarioScript.FindRoom("Room_3");
		CMap cMap3 = ScenarioScript.FindRoom("Room_2");
		CacheTilesForPlatform(0, cMap.MapTiles.Select((CMapTile mapTile) => ScenarioManager.GetTile(mapTile.ArrayIndex)).ToList(), visible: true);
		CacheTilesForPlatform(0, cMap2.MapTiles.Select((CMapTile mapTile) => ScenarioManager.GetTile(mapTile.ArrayIndex)).ToList(), visible: true);
		CacheWallsForPlatform(0, cMap.MapGuid, visible: true);
		CacheWallsForPlatform(0, cMap2.MapGuid, visible: true);
		CacheReflectionProbesInRoomForPlatform(0, cMap.MapGuid, visible: true);
		CacheReflectionProbesInRoomForPlatform(0, cMap2.MapGuid, visible: true);
		List<string> objectNames = new List<string> { "Clutter", "LightShafts" };
		CacheObjectsInRoomForPlatform(0, cMap.MapGuid, objectNames, visible: true);
		CacheObjectsInRoomForPlatform(0, cMap2.MapGuid, objectNames, visible: true);
		CacheRendHeightsForPlatform(1, null, cMap3.MapInstanceName, visible: true);
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		platform0Plane = new Plane(Vector3.up, new Vector3(12.35f, 3.37f, 12.87f));
		platform1Plane = new Plane(Vector3.up, Vector3.zero);
	}

	[MapTileGenBeforeLights]
	public static void CheckRoomWallsForAdjustmentsBeforeLights(ProceduralMapTile mapTile)
	{
		ScenarioScript.FindRoom("Room_1");
		CMap cMap = ScenarioScript.FindRoom("Room_2");
		if (mapTile.name == cMap.MapInstanceName)
		{
			foreach (Transform item in mapTile.transform.Find("Walls"))
			{
				if (!item.name.Contains("_NoSlice_NoFade") && (item.name == "Wall 1" || item.name == "Wall 2" || item.name == "Wall 3" || item.name == "Wall 4"))
				{
					if (item.name != "Wall 1")
					{
						item.GetOrAdd<EnableCameraFade>();
					}
					item.name += "_NoSlice_NoFade";
				}
			}
			return;
		}
		foreach (Transform item2 in mapTile.transform.Find("Walls"))
		{
			if (item2.name.Contains("_NoSlice_NoFade"))
			{
				continue;
			}
			item2.name += "_NoSlice_NoFade";
			Transform transform3 = item2.Find("Generated Content");
			if (transform3 != null)
			{
				foreach (Transform item3 in transform3)
				{
					if (!item3.name.Contains("Floor", StringComparison.InvariantCultureIgnoreCase) && !item2.name.Contains("Wall 0") && !item2.name.Contains("Wall 5"))
					{
						item3.GetOrAdd<EnableCameraFade>();
					}
				}
			}
			else
			{
				item2.GetOrAdd<EnableCameraFade>();
			}
		}
	}
}

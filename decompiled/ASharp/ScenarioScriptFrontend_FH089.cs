using System;
using System.Collections.Generic;
using FH.ApparanceBaking;
using FH.LevelComponents.EnvironmentFX;
using FH.LevelComponents.ProcGen;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH089 : ScenarioScriptFrontend
{
	public static float RidgeHeight = 1f;

	public static float RidgeAngle = 30f;

	private Plane ridgeLeftPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

	private Plane ridgeRightPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

	private const int ridgeLeftID = 0;

	private const int ridgeRightID = 1;

	private ScenarioScript_FH089 _script089 => (ScenarioScript_FH089)_script;

	public override bool _ManualActorHeightControl => true;

	protected override bool GetPlatformID(CTile tile, object logicObject, out int platformID)
	{
		if (ScenarioScript_FH089.CheckHeightLeft(tile.m_ArrayIndex))
		{
			platformID = 0;
			return true;
		}
		if (ScenarioScript_FH089.CheckHeightRight(tile.m_ArrayIndex))
		{
			platformID = 1;
			return true;
		}
		platformID = 2;
		return false;
	}

	protected override bool GetPlaneHeights(int platformID, out float minHeight, out float maxHeight)
	{
		if (platformID == 0 || platformID == 1)
		{
			minHeight = 0f;
			maxHeight = RidgeHeight;
			return true;
		}
		return base.GetPlaneHeights(platformID, out minHeight, out maxHeight);
	}

	protected override bool GetPlane(int platformID, out Plane plane)
	{
		switch (platformID)
		{
		case 0:
			plane = ridgeLeftPlane;
			return true;
		case 1:
			plane = ridgeRightPlane;
			return true;
		default:
			plane = default;
			return false;
		}
	}

	protected override void InitializeInternal()
	{
		ridgeLeftPlane = new Plane(Quaternion.Euler(0f, -30f, RidgeAngle) * Vector3.up, new Vector3(10.74f, 0f, -5.23f));
		ridgeRightPlane = new Plane(Quaternion.Euler(0f, -30f, 0f - RidgeAngle) * Vector3.up, new Vector3(4.3f, 0f, -8.95f));
		base.InitializeInternal();
	}

	protected override void CacheRendHeights()
	{
		base.CacheRendHeights();
		CMap cMap = ScenarioScript.FindRoom("Room_2");
		if (cMap == null)
		{
			return;
		}
		GameObject gameObject = EnvironmentLoader.Instance?.GetMap(cMap.MapGuid);
		if (gameObject != null)
		{
			Transform transform = gameObject.transform.Find("Walls");
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in transform)
			{
				if (item.name.Contains("Wall 5", StringComparison.InvariantCultureIgnoreCase) || item.name.Contains("Wall 6", StringComparison.InvariantCultureIgnoreCase) || item.name.Contains("Wall 7", StringComparison.InvariantCultureIgnoreCase) || item.name.Contains("Wall 8", StringComparison.InvariantCultureIgnoreCase) || item.name.Contains("Wall 9", StringComparison.InvariantCultureIgnoreCase))
				{
					list.Add(item.gameObject);
				}
			}
			List<CTile> tiles = new List<CTile>
			{
				ScenarioManager.GetTile(13, 16),
				ScenarioManager.GetTile(12, 16),
				ScenarioManager.GetTile(11, 15),
				ScenarioManager.GetTile(13, 17),
				ScenarioManager.GetTile(12, 17),
				ScenarioManager.GetTile(11, 15),
				ScenarioManager.GetTile(11, 16),
				ScenarioManager.GetTile(13, 18),
				ScenarioManager.GetTile(12, 18),
				ScenarioManager.GetTile(11, 17),
				ScenarioManager.GetTile(11, 18),
				ScenarioManager.GetTile(10, 17),
				ScenarioManager.GetTile(12, 19),
				ScenarioManager.GetTile(11, 19),
				ScenarioManager.GetTile(10, 19),
				ScenarioManager.GetTile(10, 18),
				ScenarioManager.GetTile(12, 20),
				ScenarioManager.GetTile(11, 20),
				ScenarioManager.GetTile(10, 20)
			};
			CacheRendHeightsForPlatform(0, list, visible: true);
			CacheTilesForPlatform(0, tiles, visible: true);
			List<GameObject> list2 = new List<GameObject>();
			foreach (Transform item2 in transform)
			{
				if (item2.name.Contains("Wall 11", StringComparison.InvariantCultureIgnoreCase) || item2.name.Contains("Wall 12", StringComparison.InvariantCultureIgnoreCase) || item2.name.Contains("Wall 13", StringComparison.InvariantCultureIgnoreCase) || item2.name.Contains("Wall 14", StringComparison.InvariantCultureIgnoreCase) || item2.name.Contains("Wall 15", StringComparison.InvariantCultureIgnoreCase))
				{
					list2.Add(item2.gameObject);
				}
			}
			List<CTile> tiles2 = new List<CTile>
			{
				ScenarioManager.GetTile(7, 12),
				ScenarioManager.GetTile(6, 11),
				ScenarioManager.GetTile(5, 11),
				ScenarioManager.GetTile(4, 11),
				ScenarioManager.GetTile(6, 13),
				ScenarioManager.GetTile(6, 12),
				ScenarioManager.GetTile(5, 12),
				ScenarioManager.GetTile(4, 12),
				ScenarioManager.GetTile(6, 14),
				ScenarioManager.GetTile(5, 13),
				ScenarioManager.GetTile(4, 13),
				ScenarioManager.GetTile(3, 13),
				ScenarioManager.GetTile(5, 15),
				ScenarioManager.GetTile(5, 14),
				ScenarioManager.GetTile(4, 14),
				ScenarioManager.GetTile(3, 14),
				ScenarioManager.GetTile(4, 15),
				ScenarioManager.GetTile(3, 15),
				ScenarioManager.GetTile(4, 16)
			};
			CacheRendHeightsForPlatform(1, list2, visible: true);
			CacheTilesForPlatform(1, tiles2, visible: true);
		}
		else
		{
			ScenarioScriptFrontend.RendHeightsInvalidated = true;
		}
	}

	[MapTileGenBeforeLights]
	public static void CheckRoomWallsForAdjustmentsBeforeLights(ProceduralMapTile mapTile)
	{
		CMap cMap = ScenarioScript.FindRoom("Room_2");
		if (!mapTile.name.Equals(cMap.MapInstanceName, StringComparison.InvariantCultureIgnoreCase))
		{
			return;
		}
		foreach (Transform item in mapTile.transform.Find("Walls"))
		{
			if (item.name.Equals("Wall 5", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 6", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 7", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 8", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 9", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 11", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 12", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 13", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 14", StringComparison.InvariantCultureIgnoreCase) || item.name.Equals("Wall 15", StringComparison.InvariantCultureIgnoreCase))
			{
				item.name += "_NoSlice_NoFade";
				item.GetOrAdd<EnableCameraFade>();
			}
		}
	}
}

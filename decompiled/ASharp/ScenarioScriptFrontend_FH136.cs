using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH136 : ScenarioScriptFrontend
{
	private const int GroundPlaneID = 0;

	private const int WaterPlaneID = 1;

	private const int OnBoatPlaneID = 2;

	private static Plane platform0Plane;

	private static Plane platform1Plane;

	private static Plane platform2Plane;

	public static string BoatPrefabNameA = ESpecificPropType.MovingBoat136.ToString();

	public static string BoatPrefabNameB = ESpecificPropType.MovingBoat136B.ToString();

	private ScenarioScript_FH136 _script136 => (ScenarioScript_FH136)_script;

	public override bool _ManualActorHeightControl => true;

	protected override bool GetPlatformID(CTile tile, object logicObject, out int platformID)
	{
		CObjectProp cObjectProp = logicObject as CObjectProp;
		bool flag = cObjectProp?.PrefabName.Contains("Terrain") ?? false;
		if (logicObject == null)
		{
			platformID = 1;
			return true;
		}
		if (tile.m_HexMap.RoomName == "Room_23")
		{
			platformID = 1;
			return true;
		}
		if (logicObject == null)
		{
			platformID = 1;
			return true;
		}
		if (cObjectProp != null)
		{
			if (cObjectProp.PrefabName.Contains("Water"))
			{
				platformID = 0;
				return true;
			}
			if (flag)
			{
				platformID = 1;
				return true;
			}
			if (cObjectProp is CObjectDoor || cObjectProp.PrefabName == BoatPrefabNameA || cObjectProp.PrefabName == BoatPrefabNameB || cObjectProp is CObjectTrap)
			{
				platformID = 1;
				return true;
			}
		}
		foreach (CObjectProp prop in tile.m_Props)
		{
			if (prop.PrefabName == BoatPrefabNameA || prop.PrefabName == BoatPrefabNameB)
			{
				platformID = 2;
				return true;
			}
		}
		platformID = 0;
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
		case 2:
			plane = platform2Plane;
			return true;
		default:
			plane = default;
			return false;
		}
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		platform0Plane = new Plane(Vector3.up, new Vector3(0f, -0.22f, 0f));
		platform1Plane = new Plane(Vector3.up, Vector3.zero);
		platform2Plane = new Plane(Vector3.up, new Vector3(0f, 0.022f, 0f));
	}
}

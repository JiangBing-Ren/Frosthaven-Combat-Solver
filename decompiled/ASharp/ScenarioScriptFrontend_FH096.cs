using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH096 : ScenarioScriptFrontend
{
	private const int GroundPlaneID = 0;

	private const int PlatformPlaneID = 1;

	private static Plane platform0Plane;

	private static Plane platform1Plane;

	public static string MovingPlatformPrefabName = ESpecificPropType.MovingPlatform.ToString();

	private ScenarioScript_FH096 _script096 => (ScenarioScript_FH096)_script;

	public override bool _ManualActorHeightControl => true;

	protected override bool GetPlatformID(CTile tile, object logicObject, out int platformID)
	{
		if (logicObject is CObjectProp cObjectProp && (cObjectProp is CObjectDoor || cObjectProp.PrefabName == MovingPlatformPrefabName || cObjectProp.PrefabName.Contains("Terrain")))
		{
			platformID = 0;
			return true;
		}
		foreach (CObjectProp prop in tile.m_Props)
		{
			if (prop.PrefabName == MovingPlatformPrefabName)
			{
				platformID = 1;
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
		default:
			plane = default;
			return false;
		}
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		platform0Plane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
		platform1Plane = new Plane(Vector3.up, new Vector3(0f, 0.539f, 0f));
	}
}

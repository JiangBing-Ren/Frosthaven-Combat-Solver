using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using Libs.GameConsole;
using Libs.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

public class WallsOcclusionDetection : OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>
{
	private List<CMap> _mapTiles;

	private List<Vector2> _pointsOfInterest;

	private Vector2 _cameraPosition;

	private Vector2 _focusPoint;

	private ShapeDrawer _shapeDrawer;

	private bool _showDebug;

	private readonly string[] _excludedPOINames = new string[5] { "SkipVisuals", "Light", "VisualEffect", "NoInterest", "Dummy" };

	protected override List<IWallOccluder> FetchOccluders()
	{
		return GetComponentsInChildren<IWallOccluder>().ToList();
	}

	protected override void InitializeReferences()
	{
		_pointsOfInterest = new List<Vector2>();
		ProceduralMapTile[] componentsInChildren = GetComponentsInChildren<ProceduralMapTile>(includeInactive: true);
		if (_mapTiles == null)
		{
			_mapTiles = new List<CMap>();
		}
		ProceduralMapTile[] array = componentsInChildren;
		foreach (ProceduralMapTile proceduralMapTile in array)
		{
			if (proceduralMapTile.TryGetComponent<RoomVisibilityTracker>(out var component))
			{
				if (component.CentralTile?.m_ClientTile != null)
				{
					_mapTiles.Add(component.CentralTile.m_ClientTile.m_Tile.m_HexMap);
				}
				else
				{
					Debug.LogError(string.Format("{0} is missing a Central Tile. Attach the central tile to the room prefab {1}", "RoomVisibilityTracker", proceduralMapTile));
				}
			}
		}
	}

	protected override WallOcclusionCheckJob CreateJob(int startIndex, int count)
	{
		return new WallOcclusionCheckJob(startIndex, count, _cameraPosition, _focusPoint, _pointsOfInterest, _occluders);
	}

	protected override void DisposeJob(WallOcclusionCheckJob job)
	{
		job.Dispose();
	}

	private void Update()
	{
		_cameraPosition = OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector2(CameraController.s_CameraController.m_Camera.transform.position);
		_focusPoint = OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector2(CameraController.s_CameraController.m_TargetFocalPoint);
		_pointsOfInterest.Clear();
		foreach (CMap mapTile in _mapTiles)
		{
			if (!mapTile.Revealed)
			{
				continue;
			}
			foreach (CObjectProp prop in mapTile.Props)
			{
				if (prop?.Position != null && prop.ObjectType != ScenarioManager.ObjectImportType.Door && !_excludedPOINames.Any((string x) => prop.InstanceName.Contains(x)))
				{
					_pointsOfInterest.Add(new Vector2(prop.Position.X, prop.Position.Z));
				}
			}
			foreach (CSpawner spawner in mapTile.Spawners)
			{
				CObjectProp prop2 = spawner.Prop;
				if (prop2?.Position != null && !_excludedPOINames.Any((string x) => prop2.InstanceName.Contains(x)))
				{
					_pointsOfInterest.Add(new Vector2(prop2.Position.X, prop2.Position.Z));
				}
			}
		}
		List<GameObject> allActorsInScene = Choreographer.s_Choreographer.AllActorsInScene;
		for (int num = 0; num < allActorsInScene.Count; num++)
		{
			GameObject gameObject = allActorsInScene[num];
			if (gameObject.activeSelf)
			{
				string actorName = gameObject.name;
				if (gameObject.TryGetComponent<CharacterManager>(out var component) && component.CharacterChildPrefabInstance != null)
				{
					actorName = component.CharacterChildPrefabInstance.name;
				}
				if (!_excludedPOINames.Any((string x) => actorName.Contains(x)))
				{
					_pointsOfInterest.Add(OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector2(gameObject.transform.position));
				}
			}
		}
		Tick();
		if (_showDebug)
		{
			DrawDebug();
		}
	}

	private void DrawDebug()
	{
		int num = 0;
		for (int i = 0; i < _occluders.Count; i++)
		{
			IWallOccluder wallOccluder = _occluders[i];
			bool flag = _lastResults[i] == WallOcclusionCheckResult.HitCenter;
			bool flag2 = _lastResults[i] == WallOcclusionCheckResult.HitLeftEdge;
			bool flag3 = _lastResults[i] == WallOcclusionCheckResult.HitRightEdge;
			Color green = Color.green;
			Color color = ((flag | flag2 | flag3) ? (green * 0.5f) : Color.red);
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(wallOccluder.LeftCorner), OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(wallOccluder.RightCorner), 0.1f, flag ? green : color);
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(wallOccluder.LeftCorner + wallOccluder.Forward * 0.6f), OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(wallOccluder.LeftCorner - wallOccluder.Forward * 0.6f), 0.1f, flag2 ? green : color);
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(wallOccluder.RightCorner + wallOccluder.Forward * 0.6f), OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(wallOccluder.RightCorner - wallOccluder.Forward * 0.6f), 0.1f, flag3 ? green : color);
		}
		for (int j = 0; j < _pointsOfInterest.Count; j++)
		{
			Color color2 = ((Vector2.SqrMagnitude(_pointsOfInterest[j] - _focusPoint) < 100f) ? Color.cyan : (Color.gray * 0.5f));
			_shapeDrawer.DrawSegment(num++, OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(_cameraPosition), OcclusionDetection<IWallOccluder, WallOcclusionCheckJob, WallOcclusionCheckResult>.ToVector3(_pointsOfInterest[j]), 0.05f, color2);
			_shapeDrawer.DrawCircleXZ(num++, _pointsOfInterest[j], 0.2f, 20, 0.05f, color2);
		}
		_shapeDrawer.DrawCircleXZ(num++, _focusPoint, Mathf.Sqrt(100f), 30, 0.05f, Color.white);
		_shapeDrawer.DrawCircleXZ(num++, _cameraPosition, 0.3f, 25, 0.05f, Color.magenta);
	}

	[ConsoleCommand(Command = "wall_fade_debug")]
	public static void Cmd_ToggleDrawDebug(IConsole console)
	{
		Object.FindObjectsOfType<WallsOcclusionDetection>().ForEach((WallsOcclusionDetection x) =>
		{
			x.ToggleDebug();
		});
	}

	public void ToggleDebug()
	{
		_showDebug = !_showDebug;
		if (_shapeDrawer == null)
		{
			GameObject gameObject = new GameObject("GetType_DebugDrawer");
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			_shapeDrawer = gameObject.AddComponent<ShapeDrawer>();
			gameObject.SetActive(_showDebug);
		}
		_shapeDrawer.gameObject.SetActive(_showDebug);
	}
}

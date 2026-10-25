using System.Collections.Generic;
using System.Linq;
using AStar;
using FH.LevelComponents.ProcGen;
using IKPn.Extras;
using Libs.GameConsole;
using Libs.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH016 : ScenarioScriptFrontend
{
	private const float minBoundHeight = 0f;

	private const float maxBoundHeight = 4f;

	[ConsoleVariable(Alias = "ss016_show_tile_labels")]
	private static bool ShowTileLabels_cvar;

	[ConsoleVariable(Alias = "ss016_platform_always_hide")]
	private static int PlatformAlwaysHide_cvar;

	[ConsoleVariable(Alias = "ss016_show_platform_bounds")]
	private static bool ShowPlatformBounds_cvar;

	[ConsoleVariable(Alias = "ss016_show_entrance_bounds")]
	private static bool ShowEntranceBounds_cvar;

	[ConsoleVariable(Alias = "ss016_show_hidden_bounds")]
	private static bool ShowHiddenBounds_cvar;

	[ConsoleVariable(Alias = "ss016_showGa_platform_tiles")]
	private static bool ShowPlatformTiles_cvar;

	[ConsoleVariable(Alias = "ss016_show_elevator_tiles")]
	private static bool ShowElevatorTiles_cvar;

	[ConsoleVariable(Alias = "ss016_show_teleport_tiles")]
	private static string ShowTeleportTiles_cvar;

	[ConsoleVariable(Alias = "ss016_show_all_marked_tiles")]
	private static bool ShowAllMarkedTiles_cvar;

	private static GameObject _innerWallPlatform0;

	private static GameObject _innerWallPlatform1;

	private static GameObject _wall1;

	private static GameObject _wall5;

	private static Dictionary<SimpleRotate, Vector3> rotaters = new Dictionary<SimpleRotate, Vector3>();

	private const int Platform0PlaneID = 0;

	private const int Platform1PlaneID = 1;

	private const int Entrance0PlaneID = 2;

	private const int Entrance1PlaneID = 3;

	private const int CentralRoomPlaneID = 4;

	private static bool platform0Hidden;

	private static bool platform1Hidden;

	private static Plane platform0Plane;

	private static Plane platform1Plane;

	private static Plane centerRoomPlane;

	private static Vector3 platform0Pos;

	private static Vector3 platform1Pos;

	private static Vector3 centerRoomPos;

	private ScenarioScript_FH016 _script016 => (ScenarioScript_FH016)_script;

	private static GameObject innerWallPlatform0
	{
		get
		{
			if (_innerWallPlatform0 == null || _innerWallPlatform1 == null)
			{
				List<GameObject> list = (from obj in Object.FindObjectsOfType<GameObject>(includeInactive: true)
					where obj.name.Contains("RoomWall 1")
					select obj).ToList();
				if (list.Count > 0)
				{
					_innerWallPlatform0 = list[0];
				}
				if (list.Count > 1)
				{
					_innerWallPlatform1 = list[1];
				}
			}
			return _innerWallPlatform0;
		}
	}

	private static GameObject innerWallPlatform1
	{
		get
		{
			if (_innerWallPlatform0 == null || _innerWallPlatform1 == null)
			{
				List<GameObject> list = (from obj in Object.FindObjectsOfType<GameObject>(includeInactive: true)
					where obj.name.Contains("RoomWall 1")
					select obj).ToList();
				if (list.Count > 0)
				{
					_innerWallPlatform0 = list[0];
				}
				if (list.Count > 1)
				{
					_innerWallPlatform1 = list[1];
				}
			}
			return _innerWallPlatform1;
		}
	}

	private static GameObject wall1
	{
		get
		{
			if (_wall1 == null || _wall1 == null)
			{
				List<GameObject> list = (from obj in Object.FindObjectsOfType<GameObject>(includeInactive: true)
					where obj.name.Contains("Elevator Wall 1")
					select obj).ToList();
				if (list.Count > 0)
				{
					_wall1 = list[0];
				}
				if (list.Count > 1)
				{
					_wall1 = list[1];
				}
			}
			return _wall1;
		}
	}

	private static GameObject wall5
	{
		get
		{
			if (_wall5 == null || _wall5 == null)
			{
				List<GameObject> list = (from obj in Object.FindObjectsOfType<GameObject>(includeInactive: true)
					where obj.name.Contains("Elevator Wall 5")
					select obj).ToList();
				if (list.Count > 0)
				{
					_wall5 = list[0];
				}
				if (list.Count > 1)
				{
					_wall5 = list[1];
				}
			}
			return _wall5;
		}
	}

	public override bool _ManualActorHeightControl => true;

	private bool Platform0Bounds(out Bounds bounds)
	{
		bounds = default;
		if (GetBoundsFromClientTiles(8, 14, 3, 13, out bounds))
		{
			bounds.min -= new Vector3(2f, 1f, 0.7f);
			bounds.max += new Vector3(2f, 0f, 20f);
			return true;
		}
		return false;
	}

	private bool Entrance0Bounds(out Bounds bounds)
	{
		bounds = default;
		if (GetBoundsFromClientTiles(4, 12, 6, 12, out bounds))
		{
			bounds.min -= new Vector3(0f, -0.5f, 1f);
			bounds.max += new Vector3(0.5f, 0f, 1f);
			return true;
		}
		return false;
	}

	private void UnhidePlatform0()
	{
		ToggleVisibiliy(0, visible: true);
		innerWallPlatform0.SetActive(value: true);
	}

	private void HidePlatform0()
	{
		ToggleVisibiliy(0, visible: false);
		innerWallPlatform0.SetActive(value: false);
	}

	private void UnhideWall0()
	{
		ToggleVisibiliy(2, visible: true);
		PlayAudioIfSceneReady("ElevatorDoorOpen");
	}

	private void HideWall0()
	{
		ToggleVisibiliy(2, visible: false);
		PlayAudioIfSceneReady("ElevatorDoorClose");
	}

	private bool Platform1Bounds(out Bounds bounds)
	{
		bounds = default;
		if (GetBoundsFromClientTiles(6, 2, 1, 3, out bounds))
		{
			bounds.min -= new Vector3(2f, 1f, 20f);
			bounds.max += new Vector3(2f, 0f, 0.7f);
			return true;
		}
		return false;
	}

	private bool Entrance1Bounds(out Bounds bounds)
	{
		bounds = default;
		if (GetBoundsFromClientTiles(4, 4, 6, 4, out bounds))
		{
			bounds.min -= new Vector3(0.35f, -0.5f, 1f);
			bounds.max += new Vector3(0.35f, 0f, 1f);
			return true;
		}
		return false;
	}

	private void UnhidePlatform1()
	{
		ToggleVisibiliy(1, visible: true);
		innerWallPlatform1.SetActive(value: true);
	}

	private void HidePlatform1()
	{
		ToggleVisibiliy(1, visible: false);
		innerWallPlatform1.SetActive(value: false);
	}

	private void UnhideWall1()
	{
		ToggleVisibiliy(3, visible: true);
		PlayAudioIfSceneReady("ElevatorDoorOpen");
	}

	private void HideWall1()
	{
		ToggleVisibiliy(3, visible: false);
		PlayAudioIfSceneReady("ElevatorDoorClose");
	}

	private void PlayAudioIfSceneReady(string audioId)
	{
		if ((!(Choreographer.s_Choreographer != null) || Choreographer.s_Choreographer.LoadingComplete) && (!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone))
		{
			AudioController.Play(audioId);
		}
	}

	protected override void TickInternal()
	{
		platformLabel(6, 2, 5);
		if (!platformLabel(8, 14, 2))
		{
			platformLabel(8, 14, 8);
		}
		tileLabel(8, 8, "This room is the Elevator\ndescending a floor each round.");
		CalcPlatformVisibility();
		List<CObjectActor> list = ScenarioManager.Scenario.Objects.ToList();
		Choreographer s_Choreographer = Choreographer.s_Choreographer;
		foreach (CObjectActor item in list)
		{
			if (!s_Choreographer.FindClientObjectActor(item))
			{
				CClientTile cClientTile = ScenarioScriptFrontend._cta[item.StartArrayIndex.X, item.StartArrayIndex.Y];
				if (cClientTile != null)
				{
					_script.Trace("Created actor {objectActor.Class.ID}", null, 283);
					s_Choreographer.m_ClientObjects.Add(s_Choreographer.CreateCharacterActor(cClientTile, item));
				}
			}
		}
		if (ShowPlatformBounds_cvar)
		{
			if (Platform0Bounds(out var bounds))
			{
				DrawBox(bounds);
				QGL.LatePrintWorld("Platform 0", bounds.center);
			}
			if (Platform1Bounds(out bounds))
			{
				DrawBox(bounds);
				QGL.LatePrintWorld("Platform 1", bounds.center);
			}
		}
		if (ShowEntranceBounds_cvar)
		{
			if (Entrance0Bounds(out var bounds2))
			{
				DrawBox(bounds2);
			}
			if (Entrance1Bounds(out bounds2))
			{
				DrawBox(bounds2);
			}
		}
		if (ShowPlatformTiles_cvar)
		{
			foreach (CTile item2 in _script016.tilesP0)
			{
				CClientTile cClientTile2 = ScenarioScriptFrontend._cta[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y];
				if ((bool)cClientTile2.m_GameObject)
				{
					Vector3 position = cClientTile2.m_GameObject.transform.position;
					QGL.LatePrintWorld(item2.m_ArrayIndex.ToString(), position, Color.cyan);
				}
			}
			foreach (CTile item3 in _script016.tilesP1)
			{
				CClientTile cClientTile3 = ScenarioScriptFrontend._cta[item3.m_ArrayIndex.X, item3.m_ArrayIndex.Y];
				if ((bool)cClientTile3.m_GameObject)
				{
					Vector3 position2 = cClientTile3.m_GameObject.transform.position;
					QGL.LatePrintWorld(item3.m_ArrayIndex.ToString(), position2, Color.cyan);
				}
			}
		}
		if (ShowElevatorTiles_cvar)
		{
			HashSet<Point> hashSet = new HashSet<Point>();
			CClientTile[,] cta = ScenarioScriptFrontend._cta;
			foreach (CClientTile cClientTile4 in cta)
			{
				if (cClientTile4 != null && cClientTile4.m_Tile != null)
				{
					hashSet.Add(cClientTile4.m_Tile.m_ArrayIndex);
				}
			}
			foreach (CTile item4 in _script016.tilesP0)
			{
				hashSet.Remove(item4.m_ArrayIndex);
			}
			foreach (CTile item5 in _script016.tilesP1)
			{
				hashSet.Remove(item5.m_ArrayIndex);
			}
			foreach (Point item6 in hashSet)
			{
				CClientTile cClientTile5 = ScenarioScriptFrontend._cta[item6.X, item6.Y];
				if ((bool)cClientTile5.m_GameObject)
				{
					Vector3 position3 = cClientTile5.m_GameObject.transform.position;
					QGL.LatePrintWorld($"{item6.X}:{item6.Y}", position3, Color.yellow);
				}
			}
		}
		if (!string.IsNullOrEmpty(ShowTeleportTiles_cvar) && ScenarioScriptFrontend.GetFirstMatchingActor(ShowTeleportTiles_cvar, out var result))
		{
			int x = result.ArrayIndex.X;
			int y = result.ArrayIndex.Y;
			if (_script016.PickElevatorTileToTeleport(x, y, out var result2))
			{
				Vector3 position4 = ScenarioScriptFrontend._cta[result2.m_ArrayIndex.X, result2.m_ArrayIndex.Y].m_GameObject.transform.position;
				QGL.LatePrintWorld("will teleport here\n" + result2.m_ArrayIndex.ToString(), position4, Color.green);
			}
		}
		if (ShowAllMarkedTiles_cvar)
		{
			for (int k = 0; k < ScenarioScript_FH016.MarkedTiles.Length; k++)
			{
				char c = (char)(97 + k);
				Point[] array = ScenarioScript_FH016.MarkedTiles[k];
				for (int l = 0; l < array.Length; l++)
				{
					Point p = array[l];
					Vector3 tileClientPos = GetTileClientPos(p);
					QGL.LatePrintWorld($"({c}) {p.X}:{p.Y}", tileClientPos, Color.white);
				}
			}
		}
		if (Cellophane.VarChanged("DebugPebbles_cvar", _script016.GetType()))
		{
			BackendCmd("ss016_update_pebbles");
		}
		if (Cellophane.VarChanged("DebugDescent_cvar", _script016.GetType()))
		{
			BackendCmd("ss016_update_entrance_blockers");
		}
		base.TickInternal();
		bool platformLabel(int x2, int y2, int floor)
		{
			bool flag = false;
			int num = floor - _script016.elevatorFloor;
			if (num == 0)
			{
				tileLabel(x2, y2, "Elevator reached this Platform");
				return true;
			}
			if (num == -3)
			{
				tileLabel(x2, y2, "This Platform is removed", Color.red);
				return true;
			}
			if (num < -3)
			{
				return false;
			}
			string arg = ((num > 0) ? "below" : "above");
			tileLabel(x2, y2, $"This Platform is {Mathf.Abs(num)} floor(s)\n{arg} the Elevator");
			return true;
		}
		void tileLabel(int num, int num2, string text, Color? color = null)
		{
			if (ShowTileLabels_cvar)
			{
				num = Mathf.Clamp(num, 0, ScenarioScriptFrontend._cta.GetLength(0) - 1);
				num2 = Mathf.Clamp(num2, 0, ScenarioScriptFrontend._cta.GetLength(1) - 1);
				color = color ?? new Color?(Color.white);
				GameObject gameObject = ScenarioScriptFrontend._cta[num, num2]?.m_GameObject;
				if (!gameObject)
				{
					_script.Trace($"Tile Label: Not a valid tile {num} {num2}", null, 227);
				}
				else
				{
					Vector3 position5 = gameObject.transform.position;
					Vector3 vector = position5 + new Vector3(0f, 3f, 0f);
					Vector2 vector2 = QGL.MeasureString(text);
					Color value = Color.white * 0.4f;
					value.a = 1f;
					QGL.LateDrawLineWorld(position5, vector, value);
					QGL.LateBlitWorld(position5, 5f, 5f, Color.white);
					QGL.LateBlitWorld(vector, vector2.x + 10f, vector2.y + 10f, value);
					QGL.LatePrintWorld(text, vector, color);
				}
			}
		}
	}

	private void UpdateRotatorSpeed(bool moved)
	{
		foreach (KeyValuePair<SimpleRotate, Vector3> rotater in rotaters)
		{
			if (!(rotater.Key == null))
			{
				if (moved)
				{
					rotater.Key.vector = Vector3.Lerp(rotater.Key.vector, rotater.Value * 100f, Time.deltaTime * 4f);
				}
				else
				{
					rotater.Key.vector = Vector3.Lerp(rotater.Key.vector, rotater.Value, Time.deltaTime * 4f);
				}
			}
		}
	}

	protected override bool UpdatePlanes(bool instant = false)
	{
		float num = (float)(_script016.elevatorFloor - 2) / 2f;
		float num2 = (float)(_script016.elevatorFloor - 5) / 2f;
		float num3 = (float)(_script016.elevatorFloor - 8) / 2f;
		bool result = false;
		List<int> list = new List<int>();
		bool flag = false;
		bool flag2;
		if (platform0Hidden)
		{
			flag2 = UpdateHeightOfPlane(ref platform0Pos, ref platform0Plane, 0.5f, 4f, 0f, instant);
		}
		else
		{
			flag2 = ((!(num <= 1f)) ? UpdateHeightOfPlane(ref platform0Pos, ref platform0Plane, 1f - Mathf.Clamp((num3 + 1f) / 2f, 0f, 1f), 4f, 0f, instant) : UpdateHeightOfPlane(ref platform0Pos, ref platform0Plane, 1f - Mathf.Clamp((num + 1f) / 2f, 0f, 1f), 4f, 0f, instant));
		}
		if (flag2)
		{
			result = true;
			list.Add(0);
			list.Add(2);
		}
		bool flag3 = false;
		if ((!platform1Hidden) ? UpdateHeightOfPlane(ref platform1Pos, ref platform1Plane, 1f - Mathf.Clamp((num2 + 1f) / 2f, 0f, 1f), 4f, 0f, instant) : UpdateHeightOfPlane(ref platform1Pos, ref platform1Plane, 0.5f, 4f, 0f, instant))
		{
			result = true;
			list.Add(1);
			list.Add(3);
		}
		if (UpdateHeightOfPlane(ref centerRoomPos, ref centerRoomPlane, 0.5f, 4f, 0f, instant: true))
		{
			result = true;
			list.Add(4);
		}
		if (list.Count > 0)
		{
			SetPlatformHeightsDirty(list.ToArray());
		}
		return result;
	}

	protected override bool UpdateHeights()
	{
		bool flag = base.UpdateHeights();
		UpdateRotatorSpeed(flag);
		return flag;
	}

	private void CalcPlatformVisibility(bool reset = false)
	{
		bool flag = Mathf.Abs(5 - _script016.elevatorFloor) > 2 || PlatformAlwaysHide_cvar == 2;
		if ((flag != platform1Hidden) | reset)
		{
			platform1Hidden = flag;
			CacheRendHeights();
			if (!flag)
			{
				UnhidePlatform1();
				HideWall1();
				UpdateHeightOfPlane(ref platform1Pos, ref platform1Plane, 1.5f, 4f, 0f, instant: true);
			}
			else
			{
				HidePlatform1();
				UnhideWall1();
			}
		}
		bool flag2 = (Mathf.Abs(2 - _script016.elevatorFloor) > 2 && Mathf.Abs(8 - _script016.elevatorFloor) > 2) || PlatformAlwaysHide_cvar == 3;
		if ((flag2 != platform0Hidden) | reset)
		{
			CacheRendHeights();
			platform0Hidden = flag2;
			if (!flag2)
			{
				UnhidePlatform0();
				HideWall0();
				UpdateHeightOfPlane(ref platform0Pos, ref platform0Plane, 1.5f, 4f, 0f, instant: true);
			}
			else
			{
				HidePlatform0();
				UnhideWall0();
			}
		}
	}

	protected override bool GetPlatformID(CTile tile, object logicObject, out int platformID)
	{
		if (_script016.tilesP0.Contains(tile))
		{
			platformID = 0;
			return true;
		}
		if (_script016.tilesP1.Contains(tile))
		{
			platformID = 1;
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
			plane = platform0Plane;
			return true;
		case 1:
			plane = platform1Plane;
			return true;
		case 2:
		case 3:
		case 4:
			plane = centerRoomPlane;
			return true;
		default:
			plane = default;
			return false;
		}
	}

	protected override void CacheRendHeights()
	{
		base.CacheRendHeights();
		Bounds item = default;
		List<string> objectNames = new List<string> { "LightShafts" };
		string mapGuid = ScenarioManager.CurrentScenarioState.Maps.FirstOrDefault().MapGuid;
		if (Entrance0Bounds(out var bounds))
		{
			List<Bounds> bounds2 = new List<Bounds> { bounds };
			if (wall1 != null)
			{
				GameObject item2 = wall1.FindInChildren("MR_FloorElevator_A_3D")?.gameObject;
				GameObject item3 = wall1.FindInChildren("MR_FloorElevator_A_3D (1)")?.gameObject;
				CacheRendHeightsForPlatform(0, new List<GameObject> { item2 }, !platform0Hidden);
				CacheRendHeightsForPlatform(4, new List<GameObject> { item3 }, visible: true);
			}
			CacheObjectsInRoomForPlatform(2, mapGuid, objectNames, platform0Hidden, bounds2);
			CacheRendHeightsForPlatform(2, bounds2, null, platform0Hidden);
			item.Encapsulate(bounds);
		}
		else
		{
			ScenarioScriptFrontend.RendHeightsInvalidated = true;
		}
		if (Entrance1Bounds(out var bounds3))
		{
			List<Bounds> bounds4 = new List<Bounds> { bounds3 };
			if (wall5 != null)
			{
				GameObject item4 = wall5.FindInChildren("MR_FloorElevator_A_3D")?.gameObject;
				GameObject item5 = wall5.FindInChildren("MR_FloorElevator_A_3D (1)")?.gameObject;
				CacheRendHeightsForPlatform(1, new List<GameObject> { item4 }, !platform1Hidden);
				CacheRendHeightsForPlatform(4, new List<GameObject> { item5 }, visible: true);
			}
			CacheObjectsInRoomForPlatform(3, mapGuid, objectNames, platform1Hidden, bounds4);
			CacheRendHeightsForPlatform(3, bounds4, null, platform1Hidden);
			item.Encapsulate(bounds3);
		}
		else
		{
			ScenarioScriptFrontend.RendHeightsInvalidated = true;
		}
		if (Platform0Bounds(out var bounds5))
		{
			List<Bounds> bounds6 = new List<Bounds> { bounds5 };
			CacheObjectsInRoomForPlatform(0, mapGuid, objectNames, !platform0Hidden, bounds6);
			CacheRendHeightsForPlatform(0, bounds6, null, !platform0Hidden);
			item.Encapsulate(bounds5);
		}
		else
		{
			ScenarioScriptFrontend.RendHeightsInvalidated = true;
		}
		if (Platform1Bounds(out var bounds7))
		{
			List<Bounds> bounds8 = new List<Bounds> { bounds7 };
			CacheObjectsInRoomForPlatform(1, mapGuid, objectNames, !platform1Hidden, bounds8);
			CacheRendHeightsForPlatform(1, bounds8, null, !platform1Hidden);
			item.Encapsulate(bounds7);
		}
		else
		{
			ScenarioScriptFrontend.RendHeightsInvalidated = true;
		}
		if (ScenarioScriptFrontend.RendHeightsInvalidated)
		{
			return;
		}
		item.min -= Vector3.one * 20f;
		item.max += Vector3.one * 20f;
		List<Bounds> bounds9 = new List<Bounds> { item };
		CacheObjectsInRoomForPlatform(4, mapGuid, objectNames, !platform1Hidden, bounds9);
		CacheRendHeightsForPlatform(4, bounds9, null, visible: true);
		CacheReflectionProbesInRoomForPlatform(4, mapGuid, visible: true);
		List<SimpleRotate> componentsInRoots = GetComponentsInRoots<SimpleRotate>(includeInactive: true);
		if (componentsInRoots == null)
		{
			return;
		}
		foreach (SimpleRotate item6 in componentsInRoots)
		{
			if (!(item6 == null))
			{
				rotaters[item6] = item6.vector;
			}
		}
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		CalcPlatformVisibility(reset: true);
		UpdatePlanes(instant: true);
	}

	protected override void DeinitializeInternal()
	{
		base.DeinitializeInternal();
		_innerWallPlatform0 = null;
		_innerWallPlatform1 = null;
		rotaters.Clear();
		platform0Hidden = false;
		platform1Hidden = false;
		platform0Plane = default;
		platform1Plane = default;
		centerRoomPlane = default;
		platform0Pos = default;
		platform1Pos = default;
		centerRoomPos = default;
	}

	[MapTileGenBeforeLights]
	public static void CheckRoomWallsForAdjustmentsBeforeLights(ProceduralMapTile mapTile)
	{
		GameObject gameObject = mapTile.gameObject.FindInChildren("Elevator Wall 1", includeInactive: true, partialNameCheck: true);
		if (gameObject != null)
		{
			gameObject.FindAllInChildren("PCG_MR_Floor_Sshape_A", includeInactive: true, partialNameCheck: true).ForEach((GameObject x) =>
			{
				DisableRenderers(x);
			});
		}
		(from x in mapTile.gameObject.FindAllInChildren("PCG_MR_Floor_Fshape_A", includeInactive: true, partialNameCheck: true)
			where CalcHorDist(x.transform.localPosition, new Vector3(-1.719572f, 0f, 5.956469f)) <= 0.01f || CalcHorDist(x.transform.localPosition, new Vector3(0.0001878738f, 0f, 5.956756f)) <= 0.01f || CalcHorDist(x.transform.localPosition, new Vector3(1.719516f, 0f, 5.956469f)) <= 0.01f || CalcHorDist(x.transform.localPosition, new Vector3(-1.719356f, 0f, -5.956595f)) <= 0.01f || CalcHorDist(x.transform.localPosition, new Vector3(0.0001626015f, 0f, -5.956801f)) <= 0.01f || CalcHorDist(x.transform.localPosition, new Vector3(1.719375f, 0f, -5.956553f)) <= 0.01f
			select x).ToList().ForEach((GameObject x) =>
		{
			DisableRenderers(x);
		});
		GameObject gameObject2 = mapTile.gameObject.FindInChildren("2ndRoomWall 1", includeInactive: true, partialNameCheck: true);
		if (gameObject2 != null)
		{
			gameObject2.FindAllInChildren("EN_MR_FloorBeam_A", includeInactive: true, partialNameCheck: true).ForEach((GameObject x) =>
			{
				DisableRenderers(x);
			});
		}
		GameObject gameObject3 = mapTile.gameObject.FindInChildren("Elevator Wall 5", includeInactive: true, partialNameCheck: true);
		if (gameObject3 != null)
		{
			gameObject3.FindAllInChildren("PCG_MR_Floor_Sshape_A", includeInactive: true, partialNameCheck: true).ForEach((GameObject x) =>
			{
				DisableRenderers(x);
			});
		}
		static float CalcHorDist(Vector3 a, Vector3 b)
		{
			a.y = 0f;
			b.y = 0f;
			return (a - b).magnitude;
		}
		static void DisableRenderers(GameObject x)
		{
			x.GetComponentsInChildren<Renderer>().ForEach((Renderer renderer) =>
			{
				renderer.enabled = false;
			});
		}
	}
}

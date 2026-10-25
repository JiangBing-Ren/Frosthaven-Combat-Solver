using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using AStar;
using Chronos;
using DevLocker.GFrame.Input;
using FH;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FH.LevelEditor;
using FH.Misc;
using GLOOM;
using GLOOM.MainMenu;
using JetBrains.Annotations;
using Libs.GameConsole;
using Libs.Utils;
using Libs.Utils.Profiler;
using SETUtil;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelEditorController : MonoBehaviour
{
	public delegate IEnumerator PlaceNewMapDelegate(GameObject root);

	public enum ECaptureTileFunction
	{
		None = 0,
		SpawnUnit = 1,
		MoveUnit = 2,
		RecordTile = 4,
		SpawnInMultipleTiles = 5,
		LinkProps = 6,
		UnlinkProps = 7
	}

	public enum ELevelEditorState
	{
		Idle,
		PreviewingLoadOwnParty,
		PreviewingFixedPartyLevel,
		Editing
	}

	public enum ELevelEditingState
	{
		None,
		RoomPlacement,
		DoorPlacement,
		PropActorPlacement
	}

	public enum ERoomPlacementState
	{
		None,
		PlacingRoom,
		PlacingDoor
	}

	public static LevelEditorController s_Instance;

	public GameObject LevelEditorUIPrefab;

	private ELevelEditingState m_CurrentLevelEditingState = ELevelEditingState.RoomPlacement;

	public ScenarioManager.EDLLMode LastLoadedRuleset;

	[HideInInspector]
	public LevelEditorUI m_LevelEditorUIInstance;

	[HideInInspector]
	public bool AutoTestNeedsSaving;

	private List<string> m_EnemyStateGuidsToSetHidden;

	private int m_CurrentId = 1;

	private const string c_RoomNamePrefix = "Room_";

	private SharedLibrary.Random m_Random;

	public GameObject LocationIndicatorPrefab;

	private GameObject m_CurrentLocationIndicator;

	private IEnumerator m_LocationIndicatorRoutine;

	private GameObject m_CurrentMovingObject;

	private GameObject m_Plane;

	private GameObject m_DirectionalLight;

	public Dictionary<string, bool> WallVisibilityOverrides = new Dictionary<string, bool>();

	public bool BulkLevelProcessingInProgress;

	private bool m_EndBulkRun;

	private IEnumerator m_BulkLevelProcessingRoutine;

	public List<string> LoggedErrors = new List<string>();

	public List<string> LoggedWarnings = new List<string>();

	private readonly Queue<IEnumerator> _pendingTasks = new Queue<IEnumerator>();

	private string m_ToPlaceObjectIdentifier;

	private string m_ToMoveObjectGuid;

	private ECaptureTileFunction m_CaptureTileFunction;

	private Action<CClientTile> m_TileSelectedAction;

	public HashSet<GameObject> AllMaps { get; private set; }

	public static GameObject MapSceneRoot => Choreographer.s_Choreographer.m_MapSceneRoot;

	public ELevelEditorState CurrentState { get; private set; }

	public ELevelEditingState CurrentLevelEditingState
	{
		get
		{
			return m_CurrentLevelEditingState;
		}
		set
		{
			if (m_CurrentLevelEditingState != value)
			{
				m_CurrentLevelEditingState = value;
				if (m_LevelEditorUIInstance != null && m_LevelEditorUIInstance.TabTogglesController != null)
				{
					m_LevelEditorUIInstance.TabTogglesController.RefreshAvailableToggles(m_CurrentLevelEditingState);
				}
			}
		}
	}

	public ERoomPlacementState CurrentPlacementState { get; private set; }

	public bool IsEditing => CurrentState >= ELevelEditorState.Editing;

	public bool IsPreviewingLevel
	{
		get
		{
			if (CurrentState != ELevelEditorState.PreviewingFixedPartyLevel)
			{
				return CurrentState == ELevelEditorState.PreviewingLoadOwnParty;
			}
			return true;
		}
	}

	public CCustomLevelData LoadedLevel { get; private set; }

	public string LoadedLevelFileName { get; private set; }

	public string LoadedAutoTestFileName { get; private set; }

	public bool BulkProcessingErrorOccurred { get; set; }

	public CAreaEffect CurrentAreaEffectSelection { get; set; }

	private bool CaptureTile { get; set; }

	private LevelEditorUnitsPanel.UnitType CaptureTileUnitType { get; set; }

	private int m_CaptureTileExpectedCount { get; set; }

	private List<CClientTile> m_CurrentlyCapturedTiles { get; set; }

	private void Awake()
	{
		OnAwakeInit();
	}

	public void OnAwakeInit()
	{
		s_Instance = this;
		WallVisibilityOverrides = new Dictionary<string, bool>();
	}

	private void Update()
	{
		if (CurrentState != ELevelEditorState.Editing || (CurrentPlacementState != ERoomPlacementState.PlacingRoom && CurrentPlacementState != ERoomPlacementState.PlacingDoor) || !(Camera.main != null))
		{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(InputFHManager.CursorPosition);
		int mask = LayerMask.GetMask("LevelEditorPlane");
		if (Physics.Raycast(ray, out var hitInfo, 50000f, mask))
		{
			m_CurrentMovingObject.transform.position = hitInfo.point;
			SnapGameObjectToHexes(m_CurrentMovingObject);
		}
		if (InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight.WasPressedThisFrame())
		{
			m_CurrentMovingObject.transform.Rotate(0f, 60f, 0f, Space.Self);
		}
		if (InputFHManager.Controls.UI.RightClick.WasPressedThisFrame())
		{
			if (CurrentPlacementState == ERoomPlacementState.PlacingRoom)
			{
				CMap item = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == m_CurrentMovingObject.name);
				ScenarioManager.CurrentScenarioState.Maps.Remove(item);
				AllMaps.Remove(m_CurrentMovingObject);
				m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
			}
			UnityEngine.Object.Destroy(m_CurrentMovingObject);
			CurrentPlacementState = ERoomPlacementState.None;
			m_CurrentMovingObject = null;
			s_Instance.m_LevelEditorUIInstance.FinishedAction();
		}
		if (InputFHManager.Controls.UI.Click.WasPressedThisFrame())
		{
			bool flag = false;
			switch (CurrentPlacementState)
			{
			case ERoomPlacementState.PlacingRoom:
				flag = LockRoomPlacement();
				break;
			case ERoomPlacementState.PlacingDoor:
				flag = LockDoorPlacement();
				break;
			}
			if (flag)
			{
				m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
				m_LevelEditorUIInstance.RoomPanel.RefreshDoorUIWithCurrentState();
				CurrentPlacementState = ERoomPlacementState.None;
				m_CurrentMovingObject = null;
				s_Instance.m_LevelEditorUIInstance.FinishedAction();
			}
		}
	}

	public void StartLevelEditor()
	{
		AllMaps = new HashSet<GameObject>();
		if (m_Plane == null)
		{
			m_Plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			m_Plane.transform.SetPositionAndRotation(new Vector3(m_Plane.transform.position.x, 0f, m_Plane.transform.position.z), m_Plane.transform.rotation);
			m_Plane.transform.localScale = new Vector3(10f, 1f, 10f);
			m_Plane.GetComponent<Renderer>().enabled = false;
			m_Plane.layer = 31;
		}
		if (m_DirectionalLight == null)
		{
			m_DirectionalLight = new GameObject("Light");
			m_DirectionalLight.AddComponent<Light>().type = LightType.Directional;
		}
		SetLevelEditorState(ELevelEditorState.Editing);
		if (SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState != null)
		{
			Log.Info("Load scenario \"" + SaveData.Instance.Global.CurrentEditorLevelData.Name + "\"");
			UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState);
		}
		else
		{
			Log.Error("You are loading a file that doesn't have a valid ScenarioState. It probably means that you are loading a deprecated file. Please, verify.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00016", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
		}
	}

	public void QuitLevelEditor()
	{
		InputFHManager.Controls.DisableAll(this);
		SetLevelEditorState(ELevelEditorState.Idle);
		if (!GloomUtility.IsGUIVisible())
		{
			GloomUtility.ToggleGUI();
		}
		SaveData.Instance.Global.CurrentEditorAutoTestData = null;
		SaveData.Instance.Global.CurrentEditorLevelData = null;
		if (m_LevelEditorUIInstance != null)
		{
			UnityEngine.Object.Destroy(m_LevelEditorUIInstance.gameObject);
		}
		if (m_DirectionalLight != null)
		{
			UnityEngine.Object.Destroy(m_DirectionalLight);
		}
		if (m_Plane != null)
		{
			UnityEngine.Object.Destroy(m_Plane);
		}
	}

	public void SetLevelEditorState(ELevelEditorState stateToSet)
	{
		if (CurrentState != stateToSet)
		{
			if (stateToSet != ELevelEditorState.Editing)
			{
				GameState.IsInLevelEditor = false;
			}
			if (stateToSet == ELevelEditorState.Editing)
			{
				InputFHManager.Controls.Enable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight);
			}
			else if (CurrentState == ELevelEditorState.Editing)
			{
				InputFHManager.Controls.Disable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight);
			}
			WallVisibilityOverrides.Clear();
			CurrentState = stateToSet;
		}
	}

	public IEnumerator LoadGameMaps(ScenarioState state, bool isFirstLoad, Action<ByRef<bool>> spawnPropsAndCharacters, [CanBeNull] KronosProfilingScaffolding profiling)
	{
		RespectFrameTime respect = new RespectFrameTime("LevelEditorController.LoadGameMaps");
		bool roomPlacement;
		try
		{
			AllMaps = new HashSet<GameObject>();
			m_EnemyStateGuidsToSetHidden = (from a in state.Monsters
				where !a.IsRevealed
				select a.ActorGuid).ToList();
			Log.Info("Level editor generating level from state with map count: " + state.Maps.Count);
			m_Random = new SharedLibrary.Random(state.Seed);
			roomPlacement = state.Maps.Count == 0;
			if (roomPlacement)
			{
				CurrentLevelEditingState = ELevelEditingState.RoomPlacement;
			}
			if (Application.isPlaying)
			{
				yield return EnvironmentLoader.Instance.Load(state, PlaceNewMaps, (IMapping mapping, ByRef<bool> chainedProcedureCompletion) =>
				{
					foreach (GameObject map2 in mapping.GetMaps())
					{
						s_Instance.AllMaps.Add(map2);
					}
					Choreographer.s_Choreographer.m_MapSceneRoot = mapping.EnvironmentRoot;
					ClientScenarioManager.Create(state, isFirstLoad);
					if (spawnPropsAndCharacters != null)
					{
						spawnPropsAndCharacters(chainedProcedureCompletion);
					}
					else
					{
						chainedProcedureCompletion.Value = true;
					}
				}, (IMapping mapping) =>
				{
					Choreographer.s_Choreographer.m_MapSceneRoot = mapping.EnvironmentRoot;
				}, profiling);
				yield break;
			}
			GameObject root = (Choreographer.s_Choreographer.m_MapSceneRoot = EnvironmentLoader.Instance.ApparanceMapsRoot);
			yield return PlaceNewMaps(root);
			EnvironmentLoader.ApparanceMapping preliminaryMapping = new EnvironmentLoader.ApparanceMapping(root, state, partial: true);
			foreach (GameObject map3 in preliminaryMapping.GetMaps())
			{
				s_Instance.AllMaps.Add(map3);
			}
			Choreographer.EnsureBackgroundSpawned(preliminaryMapping.EnvironmentRoot.transform);
			yield return EnvironmentLoader.InitializeProceduralContent(preliminaryMapping, state).WaitForCompletion();
			foreach (IProceduralContent item in preliminaryMapping.GetProps().SelectMany((GameObject a) => a.GetComponentsInChildren<IProceduralContent>()).Distinct())
			{
				item.RebuildContent();
			}
			((IProceduralContent)preliminaryMapping.EnvironmentRoot.GetComponentInChildren<ProceduralBackground>(includeInactive: false)).RebuildContent();
			ClientScenarioManager.Create(state, isFirstLoad);
		}
		finally
		{
			if (respect != null)
			{
				((IDisposable)respect).Dispose();
			}
		}
		IEnumerator PlaceNewMaps(GameObject gameObject)
		{
			using (new MiniProfilerScope("LevelEditorController.LoadGameMaps.PlaceNewMaps"))
			{
				if (!roomPlacement)
				{
					s_Instance.CurrentLevelEditingState = ELevelEditingState.PropActorPlacement;
				}
				ByRef<GameObject> newMapRoot = new ByRef<GameObject>();
				foreach (CMap map in state.Maps)
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded();
					}
					yield return PlaceMap(map, state, isFirstLoad, preview: false, gameObject, newMapRoot);
				}
				yield return UnityGameEditorRuntime.InitialiseScenarioMap(state, gameObject);
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded();
				}
				if (state.Maps.Count > 0)
				{
					SetupDoorsFromFullLoad(state, isFirstLoad);
				}
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded();
				}
				profiling?.Stopwatch?.Time("Instantiated Map Rooms");
			}
		}
	}

	private static IEnumerator PlaceMap(CMap map, ScenarioState state, bool generateNewMap, bool preview, GameObject mapsRoot, ByRef<GameObject> newMapRoot)
	{
		using (RespectFrameTime respect = new RespectFrameTime("LevelEditorController.PlaceMap"))
		{
			string text = map.MapType.ToString();
			GameObject prefab = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_mapsleveleditor", "Map " + text, "mapsleveleditor");
			GameObject newMapRootGameObject = SceneUtil.Instantiate(prefab, Vector3.zero, Quaternion.identity, mapsRoot.transform);
			newMapRootGameObject.name = map.MapInstanceName;
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded();
			}
			ApparanceMap mapInfo = newMapRootGameObject.GetComponent<ApparanceMap>();
			mapInfo.MapGuid = map.MapGuid;
			mapInfo.RoomName = map.RoomName;
			mapInfo.Tiles = UnityGameEditorRuntime.FindUnityGameObjects(newMapRootGameObject, ScenarioManager.ObjectImportType.Tile);
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded();
			}
			newMapRootGameObject.transform.eulerAngles = Vector3.zero;
			int num = 0;
			Vector3 identityCenter = Vector3.zero;
			foreach (GameObject tile in mapInfo.Tiles)
			{
				identityCenter += tile.transform.position;
				num++;
			}
			identityCenter /= (float)num;
			map.Centre = new CVector3(identityCenter.x, identityCenter.y, identityCenter.z);
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded();
			}
			GameObject gameObject = null;
			float num2 = float.MaxValue;
			foreach (GameObject tile2 in mapInfo.Tiles)
			{
				if ((tile2.transform.position - identityCenter).magnitude < num2)
				{
					gameObject = tile2;
					num2 = (tile2.transform.position - identityCenter).magnitude;
				}
			}
			map.ClosestTileIdentityPosition = new CVector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded();
			}
			if (map.Position == null)
			{
				map.Position = GloomUtility.VToCV(newMapRootGameObject.transform.position);
				map.Rotation = GloomUtility.VToCV(newMapRootGameObject.transform.eulerAngles);
				map.Angle = newMapRootGameObject.transform.eulerAngles.y;
			}
			if (preview)
			{
				newMapRootGameObject.FindInChildren("Walls").SetActive(value: false);
				map.Angle = newMapRootGameObject.transform.eulerAngles.y;
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded();
				}
			}
			else if (!generateNewMap)
			{
				if (map.Position == null)
				{
					map.Position = new CVector3(0f, 0f, 0f);
				}
				Transform transform = newMapRootGameObject.transform;
				transform.position = new Vector3(map.Position.X, map.Position.Y, map.Position.Z);
				transform.eulerAngles = new Vector3(0f, map.Angle, 0f);
				foreach (CObjectDoor doorProp in state.DoorProps.Where((CObjectProp x) => x.StartingMapGuid == map.MapGuid))
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded();
					}
					GameObject gameObject2 = SpawnDoor(doorProp, newMapRootGameObject.transform);
					gameObject2.name = doorProp.InstanceName;
					gameObject2.transform.position = new Vector3(doorProp.Position.X, doorProp.Position.Y, doorProp.Position.Z);
					gameObject2.transform.localEulerAngles = new Vector3(0f, doorProp.Angle, 0f);
					UnityGameEditorDoorProp component = gameObject2.GetComponent<UnityGameEditorDoorProp>();
					if (component != null)
					{
						component.m_InitiallyVisable = true;
						component.m_IsDungeonEntrance = doorProp.IsDungeonEntrance;
						component.m_IsDungeonExit = doorProp.IsDungeonExit;
					}
				}
			}
			if (preview || !generateNewMap)
			{
				InitNewMapStyle(newMapRootGameObject, map);
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded();
				}
			}
			Choreographer.s_Choreographer.InitNewMap(newMapRootGameObject, map);
			newMapRoot.Value = newMapRootGameObject;
		}
		static void InitNewMapStyle(GameObject gameObject3, CMap cMap)
		{
			ProceduralStyle component2 = gameObject3.GetComponent<ProceduralStyle>();
			if (cMap.SelectedPossibleRoom != null)
			{
				component2.Biome = cMap.SelectedPossibleRoom.Biome;
				component2.SubBiome = cMap.SelectedPossibleRoom.SubBiome;
				component2.Theme = cMap.SelectedPossibleRoom.Theme;
				component2.SubTheme = cMap.SelectedPossibleRoom.SubTheme;
				component2.Tone = cMap.SelectedPossibleRoom.Tone;
				component2.BiomeState = cMap.SelectedPossibleRoom.BiomeState;
			}
			gameObject3.SetActive(value: false);
			gameObject3.SetActive(value: true);
			cMap.Position = GloomUtility.VToCV(gameObject3.transform.position);
			cMap.Rotation = GloomUtility.VToCV(gameObject3.transform.eulerAngles);
			cMap.Angle = gameObject3.transform.eulerAngles.y;
			s_Instance.AllMaps.Add(gameObject3);
		}
	}

	private void SetupDoorsFromFullLoad(ScenarioState state, bool firstLoad)
	{
		foreach (CObjectDoor item in state.Props.OfType<CObjectDoor>())
		{
			GameObject gameObject = GameObject.Find(item.InstanceName);
			if (gameObject != null)
			{
				ApparanceLayer.Instance.CreateDoorFrame(item, gameObject);
			}
		}
	}

	public void OnProcGenLoadCompleteCallback(Scene gameScene)
	{
		if (m_LevelEditorUIInstance == null)
		{
			m_LevelEditorUIInstance = UnityEngine.Object.Instantiate(LevelEditorUIPrefab).GetComponent<LevelEditorUI>();
			m_LevelEditorUIInstance.TabTogglesController.RefreshAvailableToggles(CurrentLevelEditingState);
			SceneManager.MoveGameObjectToScene(m_LevelEditorUIInstance.gameObject, gameScene);
		}
		foreach (string enemyGuid in m_EnemyStateGuidsToSetHidden)
		{
			EnemyState enemyState = ScenarioManager.CurrentScenarioState.Monsters.FirstOrDefault((EnemyState a) => a.ActorGuid == enemyGuid);
			if (enemyState != null)
			{
				enemyState.IsRevealed = false;
			}
		}
		m_EnemyStateGuidsToSetHidden.Clear();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			foreach (PlayerState item in map.Players.Where((PlayerState p) => !p.IsRevealed && !ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor pl) => pl.ActorGuid == p.ActorGuid)))
			{
				RevealHiddenCharacterInState(item, forLevelEditor: true);
			}
			foreach (EnemyState item2 in map.Monsters.Where((EnemyState e) => !e.IsRevealed))
			{
				item2.Load();
			}
		}
		EnsureApparanceOverrideExistsForGuidAndStyle("ScenarioApparanceOverrideGUID", MapSceneRoot.GetComponent<ProceduralStyle>());
		if (AllMaps.Count > 0)
		{
			CameraController.s_CameraController.m_TargetFocalPoint = AllMaps.First().transform.position;
		}
	}

	public void AddRoomOfType(EMapType mapType)
	{
		if (_pendingTasks.Count > 0)
		{
			_pendingTasks.Enqueue(AddRoomOfTypeCo(mapType));
		}
		else
		{
			StartCoroutine(AddRoomOfTypeCo(mapType));
		}
	}

	private IEnumerator AddRoomOfTypeCo(EMapType mapType)
	{
		string roomName = "Room_" + m_CurrentId;
		m_CurrentId++;
		CMap mapToCreate = new CMap(roomName, mapType, string.Empty, new ScenarioPossibleRoom("Room_Room_" + m_CurrentId, string.Empty, ScenarioPossibleRoom.EBiome.Inherit, ScenarioPossibleRoom.ESubBiome.Inherit, ScenarioPossibleRoom.ETheme.Inherit, ScenarioPossibleRoom.ESubTheme.Inherit, ScenarioPossibleRoom.ETone.Inherit, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null), null)
		{
			Revealed = true
		};
		ByRef<GameObject> createdMap = new ByRef<GameObject>();
		yield return PlaceMap(mapToCreate, null, generateNewMap: false, preview: true, MapSceneRoot, createdMap);
		if (createdMap.Value == null)
		{
			Log.Info("Failed to find valid position for new room at door");
			yield break;
		}
		ScenarioManager.CurrentScenarioState.Maps.Add(mapToCreate);
		CameraController.s_CameraController.m_TargetFocalPoint = createdMap.Value.transform.position;
		m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
		m_CurrentMovingObject = createdMap.Value;
		CurrentPlacementState = ERoomPlacementState.PlacingRoom;
		s_Instance.m_LevelEditorUIInstance.RoomBeingPlaced(mapType.ToString());
		if (_pendingTasks.TryDequeue(out var result))
		{
			StartCoroutine(result);
		}
	}

	public bool AddDoorOfType(CObjectDoor.EDoorType doorType)
	{
		if (GetDoorBodyPrefab(doorType) == null)
		{
			return false;
		}
		m_CurrentMovingObject = SpawnDoorLevelEditor(doorType, MapSceneRoot.transform);
		m_LevelEditorUIInstance.RoomPanel.RefreshDoorUIWithCurrentState();
		CurrentPlacementState = ERoomPlacementState.PlacingDoor;
		s_Instance.m_LevelEditorUIInstance.DoorBeingPlaced(doorType.ToString());
		return true;
	}

	public bool LockRoomPlacement()
	{
		foreach (Transform item in MapSceneRoot.transform)
		{
			if (item == m_CurrentMovingObject.transform)
			{
				continue;
			}
			foreach (Transform item2 in item.transform)
			{
				foreach (Transform item3 in m_CurrentMovingObject.transform)
				{
					if (Vector3.Distance(item2.position, item3.position) < 0.1f)
					{
						UnityGameEditorObject component = item2.gameObject.GetComponent<UnityGameEditorObject>();
						UnityGameEditorObject component2 = item3.gameObject.GetComponent<UnityGameEditorObject>();
						if (!(component == null) && !(component2 == null) && (!(component != null) || !(component2 != null) || component.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile || component2.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile))
						{
							return false;
						}
					}
				}
			}
		}
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == m_CurrentMovingObject.name);
		cMap.Position = GloomUtility.VToCV(m_CurrentMovingObject.transform.position);
		cMap.Rotation = GloomUtility.VToCV(m_CurrentMovingObject.transform.eulerAngles);
		cMap.Angle = m_CurrentMovingObject.transform.eulerAngles.y;
		return true;
	}

	public bool LockDoorPlacement()
	{
		BoxCollider component = m_CurrentMovingObject.GetComponent<BoxCollider>();
		Transform parent1 = null;
		Transform parent2 = null;
		foreach (Transform item in MapSceneRoot.transform)
		{
			bool flag = false;
			if (item == m_CurrentMovingObject.transform || !(component != null))
			{
				continue;
			}
			foreach (Transform item2 in item.transform)
			{
				UnityGameEditorObject component2 = item2.gameObject.GetComponent<UnityGameEditorObject>();
				if (!(component2 != null) || (component2.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile && component2.m_ObjectType != ScenarioManager.ObjectImportType.Tile))
				{
					continue;
				}
				Collider component3 = component2.GetComponent<Collider>();
				if (component3 != null && component.bounds.Intersects(component3.bounds))
				{
					if (parent1 == null)
					{
						parent1 = item;
						break;
					}
					if (parent2 == null)
					{
						parent2 = item;
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (parent1 != null)
		{
			CObjectDoor cObjectDoor = null;
			CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == parent1.name);
			CMap cMap2 = null;
			if (parent2 != null)
			{
				cMap2 = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == parent2.name);
			}
			if (cMap.EntranceDoor == null)
			{
				m_CurrentMovingObject.transform.SetParent(parent1);
				UnityGameEditorDoorProp component4 = m_CurrentMovingObject.GetComponent<UnityGameEditorDoorProp>();
				if (cMap2 == null)
				{
					component4.m_IsDungeonEntrance = true;
					if (!ApparanceLayer.IsThinDoor(component4.m_DoorType))
					{
						if (Vector3.Dot(m_CurrentMovingObject.transform.forward, (parent1.position - m_CurrentMovingObject.transform.position).normalized) < 0f)
						{
							m_CurrentMovingObject.transform.Rotate(0f, 180f, 0f, Space.Self);
						}
					}
					else if (Vector3.Dot(m_CurrentMovingObject.transform.right, (parent1.position - m_CurrentMovingObject.transform.position).normalized) < 0f)
					{
						m_CurrentMovingObject.transform.Rotate(0f, 180f, 0f, Space.Self);
					}
				}
				cObjectDoor = UnityGameEditorRuntime.MakeDoorLevelEditor(m_CurrentMovingObject, ScenarioManager.CurrentScenarioState);
				cMap.EntranceDoor = cObjectDoor.InstanceName;
				cMap2?.ExitDoors.Add(cObjectDoor.InstanceName);
			}
			else
			{
				bool flag2 = false;
				if (cMap2 != null && cMap2.EntranceDoor == null)
				{
					flag2 = true;
				}
				m_CurrentMovingObject.transform.SetParent(flag2 ? parent2 : parent1);
				cObjectDoor = UnityGameEditorRuntime.MakeDoorLevelEditor(m_CurrentMovingObject, ScenarioManager.CurrentScenarioState);
				cMap.ExitDoors.Add(cObjectDoor.InstanceName);
				if (cMap2 != null)
				{
					if (flag2)
					{
						cMap2.EntranceDoor = cObjectDoor.InstanceName;
					}
					else
					{
						cMap2.ExitDoors.Add(cObjectDoor.InstanceName);
					}
				}
			}
			if (parent2 == null)
			{
				cMap.DungeonEntranceDoor = cObjectDoor.InstanceName;
			}
			return true;
		}
		return false;
	}

	public void OnFinishedPlacingRooms()
	{
		CurrentLevelEditingState = ELevelEditingState.DoorPlacement;
	}

	public void OnBuildTileMap()
	{
		try
		{
			RefreshTileMapForCurrentState();
			RefreshPropPositionsForNewTileMap();
			ClientScenarioManager.Create(ScenarioManager.CurrentScenarioState, firstLoad: true);
			foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
			{
				GameObject gameObject = AllMaps.SingleOrDefault((GameObject x) => x.name == map.MapInstanceName);
				gameObject.FindInChildren("Walls", includeInactive: true).SetActive(value: true);
				SetupDoorsForNewRoom(ScenarioManager.CurrentScenarioState, map, gameObject);
			}
			try
			{
				RefreshApparance();
			}
			catch (Exception ex)
			{
				Log.Warning("Refreshing Apparance threw error <IGNORE ME> msg:\n" + ex.Message + "\n" + ex.StackTrace);
			}
			CurrentLevelEditingState = ELevelEditingState.PropActorPlacement;
			if (m_DirectionalLight != null)
			{
				UnityEngine.Object.Destroy(m_DirectionalLight);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception occurred in OnBuildTileMap");
		}
	}

	public void TryDeleteRoom(CMap mapToRemove)
	{
		try
		{
			GameObject gameObject = GameObject.Find(mapToRemove.MapInstanceName);
			if (ScenarioManager.CurrentScenarioState.Maps.Any((CMap map) => map.RoomName == mapToRemove.ParentName))
			{
				CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Single((CMap map) => map.RoomName == mapToRemove.ParentName);
				cMap.Children.Remove(mapToRemove.MapGuid);
				cMap.ExitDoors.Remove(mapToRemove.EntranceDoor);
			}
			ScenarioManager.CurrentScenarioState.Props.RemoveAll((CObjectProp prop) => mapToRemove.Props.Contains(prop));
			ScenarioManager.CurrentScenarioState.Maps.RemoveAll((CMap map) => map.MapGuid == mapToRemove.MapGuid);
			AllMaps.Remove(gameObject);
			UnityEngine.Object.DestroyImmediate(gameObject);
			RefreshTileMapForCurrentState();
			RefreshPropPositionsForNewTileMap();
			ClientScenarioManager.Create(ScenarioManager.CurrentScenarioState, firstLoad: true);
			m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
		}
		catch (Exception ex)
		{
			Log.Error("Error trying to delete map with Guid " + mapToRemove.MapGuid + "\n" + ex.Message + "\n" + ex.StackTrace);
			throw;
		}
	}

	public void TryDeleteDoor(CObjectDoor doorPropToRemove)
	{
		UnityEngine.Object.DestroyImmediate(GameObject.Find(doorPropToRemove.InstanceName));
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Single((CMap map) => map.MapGuid == doorPropToRemove.StartingMapGuid);
		if (cMap.EntranceDoor == doorPropToRemove.InstanceName)
		{
			cMap.EntranceDoor = null;
		}
		if (cMap.ExitDoors.Contains(doorPropToRemove.InstanceName))
		{
			cMap.ExitDoors.Remove(doorPropToRemove.InstanceName);
		}
		if (cMap.DungeonExitDoor == doorPropToRemove.InstanceName)
		{
			cMap.DungeonExitDoor = null;
		}
		if (cMap.DungeonEntranceDoor == doorPropToRemove.InstanceName)
		{
			cMap.DungeonEntranceDoor = null;
		}
		ScenarioManager.CurrentScenarioState.Props.Remove(doorPropToRemove);
		m_LevelEditorUIInstance.RoomPanel.RefreshDoorUIWithCurrentState();
	}

	public void TryRotateDoor(CObjectDoor doorPropToRotate)
	{
		GameObject gameObject = GameObject.Find(doorPropToRotate.InstanceName);
		gameObject.transform.Rotate(0f, 60f, 0f, Space.Self);
		Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
		doorPropToRotate.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), new CVector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), new CVector3(gameObject.transform.localEulerAngles.x, gameObject.transform.localEulerAngles.y, gameObject.transform.localEulerAngles.z));
	}

	public void TryRotateProp(CObjectProp propToRotate)
	{
		GameObject gameObject = GameObject.Find(propToRotate.InstanceName);
		gameObject.transform.Rotate(0f, 30f, 0f, Space.Self);
		Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
		propToRotate.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), new CVector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), new CVector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z));
	}

	public string CanRoomBeDeleted(CMap mapToTryDelete)
	{
		string text = "";
		if (ScenarioManager.CurrentScenarioState.Props.Any((CObjectProp prop) => prop.ObjectType != ScenarioManager.ObjectImportType.Door))
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "props in level";
		}
		if (ScenarioManager.Scenario.PlayerActors.Count > 0)
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "players in level";
		}
		if (ScenarioManager.Scenario.Enemies.Count > 0)
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "enemies in level";
		}
		if (mapToTryDelete.Children != null && mapToTryDelete.Children.Count > 0)
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "child room" + ((mapToTryDelete.Children.Count > 1) ? "s" : "");
		}
		return text;
	}

	public bool CanRoomBeAdded()
	{
		if (CurrentLevelEditingState != ELevelEditingState.RoomPlacement)
		{
			return false;
		}
		return true;
	}

	public bool CanDoorBeDeleted()
	{
		if (CurrentLevelEditingState != ELevelEditingState.DoorPlacement)
		{
			return false;
		}
		return true;
	}

	public bool CanDoorBeAdded()
	{
		if (CurrentLevelEditingState != ELevelEditingState.DoorPlacement)
		{
			return false;
		}
		return true;
	}

	public void SetBiomeForRoom(CMap mapToSet, ScenarioPossibleRoom.EBiome biome)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetBiome(biome);
		GameObject gameObject = GameObject.Find(mapToSet.MapInstanceName);
		if (!gameObject)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.Biome = biome;
				obj.ForceValidate();
			}
		}
	}

	public void SetSubBiomeForRoom(CMap mapToSet, ScenarioPossibleRoom.ESubBiome subBiome)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetSubBiome(subBiome);
		GameObject gameObject = GameObject.Find(mapToSet.MapInstanceName);
		if (!gameObject)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.SubBiome = subBiome;
				obj.ForceValidate();
			}
		}
	}

	public void SetThemeForRoom(CMap mapToSet, ScenarioPossibleRoom.ETheme theme)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetTheme(theme);
		GameObject gameObject = GameObject.Find(mapToSet.MapInstanceName);
		if (!gameObject)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.Theme = theme;
				obj.ForceValidate();
			}
		}
	}

	public void SetSubThemeForRoom(CMap mapToSet, ScenarioPossibleRoom.ESubTheme subTheme)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetSubTheme(subTheme);
		GameObject gameObject = GameObject.Find(mapToSet.MapInstanceName);
		if (!gameObject)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.SubTheme = subTheme;
				obj.ForceValidate();
			}
		}
	}

	public void SetToneForRoom(CMap mapToSet, ScenarioPossibleRoom.ETone tone)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetTone(tone);
		GameObject gameObject = GameObject.Find(mapToSet.MapInstanceName);
		if (!gameObject)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.Tone = tone;
				obj.ForceValidate();
			}
		}
	}

	private void SetupDoorsForNewRoom(ScenarioState state, CMap newMap, GameObject newMapObject)
	{
		foreach (CObjectDoor item in from door in state.Props.OfType<CObjectDoor>()
			where door.StartingMapGuid == newMap.MapGuid
			select door)
		{
			GameObject gameObject = GameObject.Find(item.InstanceName);
			if (gameObject != null)
			{
				ApparanceLayer.Instance.CreateDoorFrame(item, gameObject);
			}
		}
	}

	private void RefreshTileMapForCurrentState()
	{
		Vector3Int vector3Int = new Vector3Int(int.MaxValue, 0, int.MaxValue);
		Vector3Int vector3Int2 = new Vector3Int(int.MinValue, 0, int.MinValue);
		Vector3Int zero = Vector3Int.zero;
		Vector3Int zero2 = Vector3Int.zero;
		bool flag = false;
		foreach (Transform item in MapSceneRoot.transform)
		{
			foreach (Transform item2 in item.transform)
			{
				UnityGameEditorObject component = item2.gameObject.GetComponent<UnityGameEditorObject>();
				if (component != null && (component.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
				{
					Vector3Int tileIntegerSnapSpace = MF.GetTileIntegerSnapSpace(item2.position);
					if (vector3Int.z > tileIntegerSnapSpace.z)
					{
						flag = (Mathf.Abs(tileIntegerSnapSpace.z) & 1) == 1;
					}
					vector3Int.x = ((vector3Int.x > tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : vector3Int.x);
					vector3Int.z = ((vector3Int.z > tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : vector3Int.z);
					vector3Int2.x = ((vector3Int2.x < tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : vector3Int2.x);
					vector3Int2.z = ((vector3Int2.z < tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : vector3Int2.z);
				}
			}
		}
		zero2 = new Vector3Int(-vector3Int.x + 1, 0, -vector3Int.z);
		if (flag)
		{
			zero2.z++;
		}
		zero = new Vector3Int(vector3Int2.x - vector3Int.x + 1, 0, vector3Int2.z - vector3Int.z + 1);
		int num = 0;
		foreach (Transform item3 in MapSceneRoot.transform)
		{
			ApparanceMap mapInfo = item3.gameObject.GetComponent<ApparanceMap>();
			if (mapInfo == null)
			{
				continue;
			}
			CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
			item3.name = cMap.MapInstanceName;
			cMap.MapTiles.Clear();
			foreach (Transform item4 in item3.transform)
			{
				UnityGameEditorObject component2 = item4.gameObject.GetComponent<UnityGameEditorObject>();
				if (component2 != null && (component2.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component2.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
				{
					Vector3Int vector3Int3 = MF.GetTileIntegerSnapSpace(item4.position) + zero2;
					CMapTile cMapTile = new CMapTile(component2.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile, new TileIndex(vector3Int3.x, vector3Int3.z), GloomUtility.VToCV(item4.position), SharedClient.GlobalRNG);
					item4.name = cMapTile.TileGuid;
					cMap.MapTiles.Add(cMapTile);
				}
			}
			num++;
		}
		GameObject gameObject = GameObject.Find("ScenarioMetadata");
		if (gameObject != null)
		{
			gameObject.GetComponent<ScenarioMetadata>();
		}
		CVectorInt3 positiveSpaceOffset = new CVectorInt3(zero2.x, zero2.y, zero2.z);
		ScenarioManager.CurrentScenarioState.InitScenarioState(zero.x, zero.z, positiveSpaceOffset);
	}

	private void RefreshPropPositionsForNewTileMap()
	{
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			GameObject gameObject = GameObject.Find(prop.InstanceName);
			Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(gameObject.transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
			prop.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), new CVector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), new CVector3(gameObject.transform.localEulerAngles.x, gameObject.transform.localEulerAngles.y, gameObject.transform.localEulerAngles.z));
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			Vector3Int vector3Int2 = MF.GetTileIntegerSnapSpace(Choreographer.s_Choreographer.FindClientActorGameObject(playerActor).transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
			playerActor.ArrayIndex = new Point(vector3Int2.x, vector3Int2.z);
		}
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			Vector3Int vector3Int3 = MF.GetTileIntegerSnapSpace(Choreographer.s_Choreographer.FindClientActorGameObject(enemy).transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
			enemy.ArrayIndex = new Point(vector3Int3.x, vector3Int3.z);
		}
	}

	[ContextMenu("RefreshApparance")]
	private void RefreshApparance()
	{
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (!gameObject.activeInHierarchy)
			{
				continue;
			}
			UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
			if (component != null && component.m_InvisableAtRuntime)
			{
				MeshRenderer component2 = gameObject.GetComponent<MeshRenderer>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
			}
		}
		MapSceneRoot.GetComponent<ProceduralScenario>().AnalyseWalls();
	}

	public void ToggleRoomVisibility(CMap map)
	{
		if (!WallVisibilityOverrides.ContainsKey(map.MapGuid))
		{
			WallVisibilityOverrides.Add(map.MapGuid, value: true);
		}
		WallVisibilityOverrides[map.MapGuid] = !WallVisibilityOverrides[map.MapGuid];
		GameObject.Find(map.MapInstanceName);
	}

	private static GameObject GetDoorBodyPrefab(CObjectDoor.EDoorType doorType)
	{
		return ApparanceLayer.Instance.GetDoorPrefab(doorType, isEntrance: false, isExit: false, isDoorBody: true);
	}

	private static GameObject SpawnDoor(CObjectDoor door, Transform parent)
	{
		GameObject doorBodyPrefab = GetDoorBodyPrefab(door.DoorType);
		door.PrefabName = doorBodyPrefab.name;
		GameObject gameObject = UnityEngine.Object.Instantiate(doorBodyPrefab, parent);
		UnityGameEditorScenarioEditorGizmo componentInChildren = gameObject.GetComponentInChildren<UnityGameEditorScenarioEditorGizmo>();
		if ((bool)componentInChildren)
		{
			if (s_Instance.IsEditing)
			{
				componentInChildren.Show();
			}
			else
			{
				componentInChildren.Hide();
			}
		}
		Watchman instance = Watchman.Instance;
		if (instance != null)
		{
			instance.Complete(door.InstanceName);
			return gameObject;
		}
		return gameObject;
	}

	private static GameObject SpawnDoorLevelEditor(CObjectDoor.EDoorType doorType, Transform parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(GetDoorBodyPrefab(doorType), parent);
		UnityGameEditorScenarioEditorGizmo componentInChildren = gameObject.GetComponentInChildren<UnityGameEditorScenarioEditorGizmo>();
		if ((bool)componentInChildren)
		{
			if (s_Instance.IsEditing)
			{
				componentInChildren.Show();
				return gameObject;
			}
			componentInChildren.Hide();
		}
		return gameObject;
	}

	public bool GetRoomVisibilityOverride(CMap forMap)
	{
		if (forMap == null)
		{
			return true;
		}
		bool result = true;
		if (WallVisibilityOverrides.ContainsKey(forMap.MapGuid))
		{
			result = WallVisibilityOverrides[forMap.MapGuid];
		}
		return result;
	}

	public void SetRoomRevealed(CMap mapToToggle, bool revealed)
	{
		mapToToggle.Revealed = revealed;
		if (mapToToggle.Props.FirstOrDefault((CObjectProp s) => s.InstanceName == mapToToggle.EntranceDoor) is CObjectDoor { IsDungeonEntrance: false } cObjectDoor)
		{
			SetDoorOpen(revealed, cObjectDoor);
		}
		foreach (PlayerState player in mapToToggle.Players)
		{
			player.IsRevealed = revealed;
		}
		foreach (EnemyState monster in mapToToggle.Monsters)
		{
			monster.IsRevealed = revealed;
		}
		foreach (EnemyState allyMonster in mapToToggle.AllyMonsters)
		{
			allyMonster.IsRevealed = revealed;
		}
		foreach (EnemyState enemy2Monster in mapToToggle.Enemy2Monsters)
		{
			enemy2Monster.IsRevealed = revealed;
		}
		foreach (EnemyState neutralMonster in mapToToggle.NeutralMonsters)
		{
			neutralMonster.IsRevealed = revealed;
		}
		foreach (ObjectState @object in mapToToggle.Objects)
		{
			@object.IsRevealed = revealed;
		}
	}

	public void SetPlayerRevealed(PlayerState playerStateToReveal, bool revealed)
	{
		playerStateToReveal.IsRevealed = revealed;
		playerStateToReveal.HiddenAtStart = !playerStateToReveal.IsRevealed;
	}

	public void SetEnemyRevealed(EnemyState enemyStateToReveal, bool revealed)
	{
		enemyStateToReveal.IsRevealed = revealed;
	}

	public void SetDoorOpen(bool shouldOpen, CObjectDoor door)
	{
		if (!shouldOpen && door.Activated)
		{
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].IsBridgeOpen = false;
			MF.GameObjectAnimatorPlay(GameObject.Find(door.InstanceName), "Idle");
			door.SetActivatedFromLevelEditor(activate: false);
		}
		else if (shouldOpen && !door.Activated)
		{
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].IsBridgeOpen = true;
			MF.GameObjectAnimatorPlay(GameObject.Find(door.InstanceName), "Open");
			door.SetActivatedFromLevelEditor(activate: true);
		}
	}

	public void SetDoorType(CObjectDoor doorProp, CObjectDoor.EDoorType typeToSet)
	{
		CObjectDoor.EDoorType doorType = doorProp.DoorType;
		doorProp.SetDoorType(typeToSet);
		if (doorType != doorProp.DoorType)
		{
			GameObject gameObject = GameObject.Find(doorProp.HexMap.MapInstanceName);
			GameObject gameObject2 = GameObject.Find(doorProp.InstanceName);
			GameObject gameObject3 = SpawnDoor(doorProp, gameObject.transform);
			gameObject3.name = doorProp.InstanceName;
			gameObject3.transform.position = gameObject2.transform.position;
			gameObject3.transform.rotation = gameObject2.transform.rotation;
			gameObject3.transform.SetSiblingIndex(gameObject2.transform.GetSiblingIndex());
			UnityEngine.Object.DestroyImmediate(gameObject2);
			UnityGameEditorDoorProp component = gameObject3.GetComponent<UnityGameEditorDoorProp>();
			if (component != null)
			{
				component.m_InitiallyVisable = true;
				component.m_IsDungeonEntrance = doorProp.IsDungeonEntrance;
				component.m_IsDungeonExit = doorProp.IsDungeonExit;
			}
			ApparanceLayer.Instance.CreateDoorFrame(doorProp, gameObject3);
			if (WallVisibilityOverrides.ContainsKey(doorProp.HexMap.MapGuid) && !WallVisibilityOverrides[doorProp.HexMap.MapGuid])
			{
				ToggleRoomVisibility(doorProp.HexMap);
			}
		}
	}

	[ConsoleCommand(Command = "regenerate_apparance")]
	public static void Cmd_RegenerateApparance(IConsole console, params string[] args)
	{
		RegenerateApparance();
	}

	public static void RegenerateApparance()
	{
		GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
		int seed = SharedClient.GlobalRNG.Next();
		if (mapSceneRoot != null)
		{
			ProceduralStyle component = mapSceneRoot.GetComponent<ProceduralStyle>();
			if (component != null)
			{
				component.Seed = seed;
				component.ForceValidate();
			}
		}
	}

	public void EnsureApparanceOverrideExistsForGuidAndStyle(string guid, ProceduralStyle style, bool overrideExistingWithCurrent = false)
	{
		bool flag = false;
		CApparanceOverrideDetails cApparanceOverrideDetails = SaveData.Instance.Global.CurrentEditorLevelData.ApparanceOverrideList.FirstOrDefault((CApparanceOverrideDetails o) => o.GUID == guid);
		if (cApparanceOverrideDetails == null)
		{
			cApparanceOverrideDetails = new CApparanceOverrideDetails(guid);
			flag = true;
		}
		if ((flag | overrideExistingWithCurrent) && style != null)
		{
			cApparanceOverrideDetails.OverrideSeed = style.Seed;
			cApparanceOverrideDetails.OverrideBiome = style.Biome;
			cApparanceOverrideDetails.OverrideSubBiome = style.SubBiome;
			cApparanceOverrideDetails.OverrideTheme = style.Theme;
			cApparanceOverrideDetails.OverrideSubTheme = style.SubTheme;
			cApparanceOverrideDetails.OverrideTone = style.Tone;
			cApparanceOverrideDetails.OverrideBiomeState = style.BiomeState;
			style.FetchObjectHierarchyValues(out var objectIndex, out var _);
			cApparanceOverrideDetails.OverrideSiblingIndex = objectIndex;
		}
		AddOrReplaceApparanceOverride(cApparanceOverrideDetails);
	}

	public void AddOrReplaceApparanceOverride(CApparanceOverrideDetails overrideToAdd)
	{
		SaveData.Instance.Global.CurrentEditorLevelData.ApparanceOverrideList.RemoveAll((CApparanceOverrideDetails o) => o.GUID == overrideToAdd.GUID);
		SaveData.Instance.Global.CurrentEditorLevelData.ApparanceOverrideList.Add(overrideToAdd);
	}

	public static void SetBiome(ScenarioPossibleRoom.EBiome biome, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Log.Warning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetBiome(biome);
					}
				}
			}
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			if (!(mapSceneRoot != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = mapSceneRoot.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.Biome = biome;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetSubBiome(ScenarioPossibleRoom.ESubBiome subbiome, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Log.Warning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetSubBiome(subbiome);
					}
				}
			}
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			if (!(mapSceneRoot != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = mapSceneRoot.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.SubBiome = subbiome;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetTheme(ScenarioPossibleRoom.ETheme theme, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Log.Warning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetTheme(theme);
					}
				}
			}
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			if (!(mapSceneRoot != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = mapSceneRoot.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.Theme = theme;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetSubTheme(ScenarioPossibleRoom.ESubTheme subtheme, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Log.Warning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetSubTheme(subtheme);
					}
				}
			}
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			if (!(mapSceneRoot != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = mapSceneRoot.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.SubTheme = subtheme;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetTone(ScenarioPossibleRoom.ETone tone, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Log.Warning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetTone(tone);
					}
				}
			}
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			if (!(mapSceneRoot != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = mapSceneRoot.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.Tone = tone;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void BeginPlacingUnit(string unitID, LevelEditorUnitsPanel.UnitType unitType, CAreaEffect areaEffect = null)
	{
		s_Instance.m_ToPlaceObjectIdentifier = unitID;
		s_Instance.CaptureTile = true;
		s_Instance.CaptureTileUnitType = unitType;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.SpawnUnit;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.CurrentAreaEffectSelection = areaEffect;
		if (areaEffect != null)
		{
			WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.LevelEditorSpawning;
		}
		if (unitType == LevelEditorUnitsPanel.UnitType.Prop)
		{
			s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(unitID, s_Instance.m_CurrentlyCapturedTiles.Count + 1, s_Instance.m_CaptureTileExpectedCount);
		}
		else
		{
			s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(unitID);
		}
	}

	public static void BeginMovingUnit(string unitGuid, LevelEditorUnitsPanel.UnitType unitType)
	{
		s_Instance.m_ToMoveObjectGuid = unitGuid;
		s_Instance.CaptureTile = true;
		s_Instance.CaptureTileUnitType = unitType;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.MoveUnit;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(unitGuid);
	}

	public static void BeginLinkProp(string unitGuid, LevelEditorUnitsPanel.UnitType unitType)
	{
		s_Instance.m_ToMoveObjectGuid = unitGuid;
		s_Instance.CaptureTile = true;
		s_Instance.CaptureTileUnitType = unitType;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.LinkProps;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.m_LevelEditorUIInstance.ObjectBeingLinked(unitGuid);
	}

	public static void BeginUnlinkProp(string unitGuid, LevelEditorUnitsPanel.UnitType unitType)
	{
		s_Instance.m_ToMoveObjectGuid = unitGuid;
		s_Instance.CaptureTile = true;
		s_Instance.CaptureTileUnitType = unitType;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.UnlinkProps;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.m_LevelEditorUIInstance.ObjectBeingUnlinked(unitGuid);
	}

	public static void SelectTile(Action<CClientTile> tileSelectedAction)
	{
		s_Instance.CaptureTile = true;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.RecordTile;
		s_Instance.m_TileSelectedAction = tileSelectedAction;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.m_LevelEditorUIInstance.TileBeingSelected();
	}

	public static bool InterceptTileAction(CClientTile clientTile)
	{
		if (s_Instance != null && s_Instance.CurrentState == ELevelEditorState.Editing && s_Instance.CaptureTile)
		{
			s_Instance.m_CurrentlyCapturedTiles.Add(clientTile);
			if (s_Instance.m_CurrentlyCapturedTiles.Count >= s_Instance.m_CaptureTileExpectedCount)
			{
				if (s_Instance.CurrentAreaEffectSelection != null)
				{
					List<CTile> list = new List<CTile>();
					int length = ScenarioManager.Tiles.GetLength(1);
					int length2 = ScenarioManager.Tiles.GetLength(0);
					for (int i = 0; i < length2; i++)
					{
						for (int j = 0; j < length; j++)
						{
							list.Add(ScenarioManager.Tiles[i, j]);
						}
					}
					List<CTile> validTilesIncludingBlocked = null;
					Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = null;
					foreach (CTile validTile in CAreaEffect.GetValidTiles(clientTile.m_Tile, clientTile.m_Tile, s_Instance.CurrentAreaEffectSelection, WorldspaceStarHexDisplay.Instance.AreaEffectAngle, WorldspaceStarHexDisplay.Instance.AreaEffectFlipped, null, list, ref validTilesIncludingBlocked, ref flagsOfValidTiles, ignoreLos: false, ignoreDoorLocks: false))
					{
						CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[validTile.m_ArrayIndex.X, validTile.m_ArrayIndex.Y];
						if (!s_Instance.m_CurrentlyCapturedTiles.Contains(item))
						{
							s_Instance.m_CurrentlyCapturedTiles.Add(item);
						}
					}
				}
				switch (s_Instance.m_CaptureTileFunction)
				{
				case ECaptureTileFunction.SpawnUnit:
				{
					CNode cNode = ScenarioManager.PathFinder.Nodes[clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y];
					List<CObjectProp> list2 = clientTile.m_Tile.m_Props.ToList();
					if (s_Instance.CaptureTileUnitType == LevelEditorUnitsPanel.UnitType.Spawner)
					{
						list2.RemoveAll((CObjectProp x) => x.PropType == EPropType.TerrainWater);
					}
					if (s_Instance.CaptureTileUnitType == LevelEditorUnitsPanel.UnitType.Prop)
					{
						if (s_Instance.m_ToPlaceObjectIdentifier == "TerrainVisualEffect")
						{
							list2.Clear();
						}
						else
						{
							list2.RemoveAll((CObjectProp x) => x.ObjectType != GlobalSettings.GetObjectImportType(s_Instance.m_ToPlaceObjectIdentifier));
						}
					}
					if (list2.Count != 0 || !cNode.Walkable || cNode.Blocked)
					{
						s_Instance.m_CurrentlyCapturedTiles.Remove(clientTile);
						return false;
					}
					switch (s_Instance.CaptureTileUnitType)
					{
					case LevelEditorUnitsPanel.UnitType.Enemy:
						if (SpawnMonsterAtLocation(s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.m_ToPlaceObjectIdentifier) != null)
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Objects:
						if (SpawnObjectAtLocation(s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Player:
						if (SpawnCharacterAtLocation(s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Spawner:
					{
						string text = Guid.NewGuid().ToString();
						if (SpawnSpawnerAtLocation(s_Instance.m_CurrentlyCapturedTiles, s_Instance.m_ToPlaceObjectIdentifier, text))
						{
							s_Instance.m_ToPlaceObjectIdentifier = text;
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					}
					case LevelEditorUnitsPanel.UnitType.Prop:
						if (SpawnPropAtLocation(s_Instance.m_CurrentlyCapturedTiles, s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					}
					break;
				}
				case ECaptureTileFunction.MoveUnit:
					switch (s_Instance.CaptureTileUnitType)
					{
					case LevelEditorUnitsPanel.UnitType.Enemy:
						if (MoveMonsterToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Objects:
						if (MoveObjectToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Player:
						if (MoveCharacterToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Prop:
						if (MovePropToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Spawner:
						if (MoveSpawnerToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					default:
						Log.Error($"MoveUnit not implemented yet for unit:{s_Instance.CaptureTileUnitType}. Please, implement it.");
						break;
					}
					break;
				case ECaptureTileFunction.RecordTile:
					s_Instance.m_TileSelectedAction?.Invoke(clientTile);
					s_Instance.m_TileSelectedAction = null;
					s_Instance.m_LevelEditorUIInstance.FinishedAction();
					break;
				case ECaptureTileFunction.LinkProps:
				{
					CObjectProp cObjectProp2 = FindProp(s_Instance.m_ToMoveObjectGuid);
					if (clientTile.m_Tile.m_Props.Count > 0)
					{
						CObjectProp otherProp2 = clientTile.m_Tile.m_Props.First();
						cObjectProp2.LinkToOtherProp(otherProp2);
					}
					s_Instance.m_LevelEditorUIInstance.FinishedAction();
					break;
				}
				case ECaptureTileFunction.UnlinkProps:
				{
					CObjectProp cObjectProp = FindProp(s_Instance.m_ToMoveObjectGuid);
					if (clientTile.m_Tile.m_Props.Count > 0)
					{
						CObjectProp otherProp = clientTile.m_Tile.m_Props.First();
						cObjectProp.UnlinkProp(otherProp);
					}
					s_Instance.m_LevelEditorUIInstance.FinishedAction();
					break;
				}
				}
				s_Instance.CaptureTile = false;
				s_Instance.m_CaptureTileExpectedCount = 0;
				s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
			}
			else
			{
				s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(s_Instance.m_ToMoveObjectGuid, s_Instance.m_CurrentlyCapturedTiles.Count + 1, s_Instance.m_CaptureTileExpectedCount);
			}
			return true;
		}
		return false;
	}

	public void ShowLocationIndicator(Vector3 locationToShowAt)
	{
		if (m_LocationIndicatorRoutine != null)
		{
			StopCoroutine(m_LocationIndicatorRoutine);
			m_LocationIndicatorRoutine = null;
		}
		if (m_CurrentLocationIndicator != null)
		{
			UnityEngine.Object.Destroy(m_CurrentLocationIndicator);
		}
		m_LocationIndicatorRoutine = ShowLocationIndicatorForATime(locationToShowAt);
		StartCoroutine(m_LocationIndicatorRoutine);
	}

	private IEnumerator ShowLocationIndicatorForATime(Vector3 atlocation)
	{
		m_CurrentLocationIndicator = UnityEngine.Object.Instantiate(LocationIndicatorPrefab, atlocation, Quaternion.identity);
		yield return Timekeeper.instance.WaitForSeconds(2f);
		if (m_CurrentLocationIndicator != null)
		{
			UnityEngine.Object.Destroy(m_CurrentLocationIndicator);
		}
	}

	public static int AddScenarioModifierToScenario(CScenarioModifier scenarioModifierToAdd)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null)
		{
			currentScenarioState.ScenarioModifiers.Add(scenarioModifierToAdd);
			return currentScenarioState.ScenarioModifiers.Count - 1;
		}
		return -1;
	}

	public static void ReplaceScenarioModifierInScenarioAtIndex(int scenarioModifierIndex, CScenarioModifier scenarioModifierToReplaceWith)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && scenarioModifierIndex < currentScenarioState.ScenarioModifiers.Count)
		{
			currentScenarioState.ScenarioModifiers[scenarioModifierIndex] = scenarioModifierToReplaceWith;
		}
	}

	public static void DeleteScenarioModifierFromScenario(int scenarioModifierIndex)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && scenarioModifierIndex < currentScenarioState.ScenarioModifiers.Count)
		{
			currentScenarioState.ScenarioModifiers.RemoveAt(scenarioModifierIndex);
		}
	}

	public static int AddWinObjectiveToScenario(CObjective objectiveToAdd)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null)
		{
			currentScenarioState.WinObjectives.Add(objectiveToAdd);
			return currentScenarioState.WinObjectives.Count - 1;
		}
		return -1;
	}

	public static int AddLoseObjectiveToScenario(CObjective objectiveToAdd)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null)
		{
			currentScenarioState.LoseObjectives.Add(objectiveToAdd);
			return currentScenarioState.LoseObjectives.Count - 1;
		}
		return -1;
	}

	public static void ReplaceWinObjectiveInScenarioAtIndex(int objectiveIndex, CObjective objectiveToReplaceWith)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.WinObjectives.Count)
		{
			currentScenarioState.WinObjectives[objectiveIndex] = objectiveToReplaceWith;
		}
	}

	public static void ReplaceLoseObjectiveInScenarioAtIndex(int objectiveIndex, CObjective objectiveToReplaceWith)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.LoseObjectives.Count)
		{
			currentScenarioState.LoseObjectives[objectiveIndex] = objectiveToReplaceWith;
		}
	}

	public static void DeleteWinObjectiveFromScenario(int objectiveIndex)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.WinObjectives.Count)
		{
			currentScenarioState.WinObjectives.RemoveAt(objectiveIndex);
		}
	}

	public static void DeleteLoseObjectiveFromScenario(int objectiveIndex)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.LoseObjectives.Count)
		{
			currentScenarioState.LoseObjectives.RemoveAt(objectiveIndex);
		}
	}

	public void ExitToMainMenuThroughUI()
	{
		if (IsEditing)
		{
			MainMenuUIManager.SetLoadingCompleteCallback(OnExitedToMainMenuFromLevelEditorCallback);
		}
		QuitLevelEditor();
	}

	public void OnExitedToMainMenuFromLevelEditorCallback()
	{
		MainMenuUIManager.Instance.ModeSelectionScreen.ShowForLevelEditor(autoSelectRuleset: true, LastLoadedRuleset);
	}

	public static bool SpawnSpawnerAtLocation(List<CClientTile> tiles, string spawnerType, string newSpawnerGuid)
	{
		if (tiles == null || tiles.Count < 1)
		{
			Log.Info("Failed to spawn prop - Incorrect tile count");
			return false;
		}
		foreach (CClientTile tile in tiles)
		{
			if ((!CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.None) && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.MoneyToken) == null) || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) != null)
			{
				Log.Error("Failed to spawn prop - One of the tiles chosen was invalid");
				return false;
			}
			foreach (CClientTile item2 in tiles.Where((CClientTile t) => t != tile).ToList())
			{
				if (!ScenarioManager.IsTileAdjacent(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y, item2.m_Tile.m_ArrayIndex.X, item2.m_Tile.m_ArrayIndex.Y))
				{
					Log.Error("Failed to spawn prop - Tiles chosen were not adjacent");
					return false;
				}
			}
			Log.Info("Currently Captured tile| X:" + tile.m_Tile.m_ArrayIndex.X + " Y:" + tile.m_Tile.m_ArrayIndex.Y);
		}
		if (!ScenarioManager.CurrentScenarioState.Spawners.Any((CSpawner p) => p.SpawnerGuid == newSpawnerGuid))
		{
			switch (spawnerType)
			{
			case "Spawner":
			{
				CSpawner item = CSpawnerExtensions.CreateDefaultSpawner(new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(item);
				tiles[0].m_Tile.m_Spawners.Add(item);
				Log.Info($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			case "SewerPipeSpawner":
			{
				CInteractableSpawner cInteractableSpawner3 = CSpawnerExtensions.CreateDefaultInteractableSpawner(spawnerType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(cInteractableSpawner3);
				tiles[0].m_Tile.m_Spawners.Add(cInteractableSpawner3);
				cInteractableSpawner3.CreateSpawnerProp();
				Log.Info($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			case "GraveSingleSpawner":
			{
				CInteractableSpawner cInteractableSpawner2 = CSpawnerExtensions.CreateDefaultInteractableSpawner(spawnerType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(cInteractableSpawner2);
				tiles[0].m_Tile.m_Spawners.Add(cInteractableSpawner2);
				cInteractableSpawner2.CreateSpawnerProp();
				Log.Info($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			case "GraveDoubleSpawner":
			{
				List<TileIndex> pathingBlockers = tiles.Select((CClientTile t) => new TileIndex(t.m_Tile.m_ArrayIndex)).ToList();
				Quaternion quaternion = Quaternion.identity;
				int num = 0;
				if (tiles.Count == 2)
				{
					quaternion = Quaternion.LookRotation(tiles[0].m_GameObject.transform.position - tiles[1].m_GameObject.transform.position, Vector3.up);
					num = 90;
				}
				Vector3 eulerAngles = quaternion.eulerAngles;
				eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + (float)num, eulerAngles.z);
				CVector3 rotation = GloomUtility.VToCV(eulerAngles);
				CInteractableSpawner cInteractableSpawner = CSpawnerExtensions.CreateDefaultInteractableSpawner(spawnerType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(cInteractableSpawner);
				tiles[0].m_Tile.m_Spawners.Add(cInteractableSpawner);
				cInteractableSpawner.CreateSpawnerProp(pathingBlockers, rotation);
				Log.Info($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			}
		}
		else
		{
			Log.Error("Data for Spawner:" + newSpawnerGuid + " already exists in the party, so a new one was not added");
		}
		return false;
	}

	public static CActor SpawnMonsterAtLocation(CClientTile tile, string monsterID, int level = 1, CActor.EType type = CActor.EType.Enemy, bool playSummonedAnim = false)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		if ((cActor == null || cActor.IsDead) && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == monsterID);
			cMonsterClass.SetMonsterStatLevel(level);
			if (cMonsterClass != null)
			{
				int num = -1;
				if (s_Instance.IsEditing)
				{
					int checkID = 1;
					while (num == -1)
					{
						if (!ScenarioManager.Scenario.AllMonsters.Any((CEnemyActor x) => x.MonsterClass.ID == monsterID && x.StandeeID == checkID))
						{
							num = checkID;
							cMonsterClass.RemoveAvailableID(num);
							break;
						}
						checkID++;
					}
				}
				else
				{
					num = cMonsterClass.GetNextID();
				}
				if (num == -1)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return null;
				}
				int num2;
				for (num2 = num; num2 > cMonsterClass.Models.Count - 1; num2 -= cMonsterClass.Models.Count)
				{
				}
				EnemyState enemyState = new EnemyState(cMonsterClass.ID, num2, null, tile.m_Tile.m_HexMap.MapGuid, new TileIndex(tile.m_Tile.m_ArrayIndex), cMonsterClass.Health(), cMonsterClass.Health(), level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, type);
				enemyState.ID = num;
				CActor cActor2 = null;
				switch (type)
				{
				case CActor.EType.Ally:
					cActor2 = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false);
					if (cActor2 == null)
					{
						Log.Error("Maximum enemies of this type are already added to the scenario");
						return null;
					}
					ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientAllyMonsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.AllyMonsters.Last()));
					break;
				case CActor.EType.Enemy2:
					cActor2 = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial: false);
					if (cActor2 == null)
					{
						Log.Error("Maximum enemies of this type are already added to the scenario");
						return null;
					}
					ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.Enemy2Monsters.Last()));
					break;
				case CActor.EType.Neutral:
					cActor2 = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial: false);
					if (cActor2 == null)
					{
						Log.Error("Maximum enemies of this type are already added to the scenario");
						return null;
					}
					ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientNeutralMonsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.NeutralMonsters.Last()));
					break;
				default:
					cActor2 = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false);
					if (cActor2 == null)
					{
						Log.Error("Maximum enemies of this type are already added to the scenario");
						return null;
					}
					ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientEnemies.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.Enemies.Last(), playSummonedAnim));
					break;
				}
				enemyState.IsRevealed = tile.m_Tile.m_HexMap.Revealed;
				Choreographer.s_Choreographer.SetCharacterPositions();
				Log.Info("Spawned " + LocalizationManager.GetTranslation(cMonsterClass.LocKey) + " on tile (" + tile.m_Tile.m_ArrayIndex.X + ", " + tile.m_Tile.m_ArrayIndex.Y + ")");
				ScenarioManager.CurrentScenarioState.Update();
				return cActor2;
			}
			Log.Error("Unable to find monster " + monsterID);
		}
		else
		{
			Log.Error("Invalid location selected for monster to spawn.");
		}
		return null;
	}

	public static CActor SpawnMonsterAtLocation(EnemyState enemyState)
	{
		string monsterID = enemyState.ClassID;
		int level = enemyState.Level;
		CActor.EType type = enemyState.Type;
		CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.GetClientTile(enemyState.Location.ToPoint());
		CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == monsterID);
		cMonsterClass.SetMonsterStatLevel(level);
		if (cMonsterClass != null)
		{
			int num = -1;
			if (s_Instance.IsEditing)
			{
				int checkID = 1;
				while (num == -1)
				{
					if (!ScenarioManager.Scenario.AllMonsters.Any((CEnemyActor x) => x.MonsterClass.ID == monsterID && x.StandeeID == checkID))
					{
						num = checkID;
						cMonsterClass.RemoveAvailableID(num);
						break;
					}
					checkID++;
				}
			}
			else
			{
				num = cMonsterClass.GetNextID();
			}
			if (num == -1)
			{
				Log.Error("Maximum enemies of this type are already added to the scenario");
				return null;
			}
			for (int num2 = num; num2 > cMonsterClass.Models.Count - 1; num2 -= cMonsterClass.Models.Count)
			{
			}
			enemyState.ID = num;
			CActor cActor = null;
			switch (type)
			{
			case CActor.EType.Ally:
				cActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false);
				if (cActor == null)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return null;
				}
				ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
				Choreographer.s_Choreographer.m_ClientAllyMonsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(clientTile, ScenarioManager.Scenario.AllyMonsters.Last()));
				break;
			case CActor.EType.Enemy2:
				cActor = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial: false);
				if (cActor == null)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return null;
				}
				ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
				Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(clientTile, ScenarioManager.Scenario.Enemy2Monsters.Last()));
				break;
			case CActor.EType.Neutral:
				cActor = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial: false);
				if (cActor == null)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return null;
				}
				ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
				Choreographer.s_Choreographer.m_ClientNeutralMonsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(clientTile, ScenarioManager.Scenario.NeutralMonsters.Last()));
				break;
			default:
				cActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false);
				if (cActor == null)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return null;
				}
				ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
				Choreographer.s_Choreographer.m_ClientEnemies.Add(Choreographer.s_Choreographer.CreateCharacterActor(clientTile, ScenarioManager.Scenario.Enemies.Last()));
				break;
			}
			enemyState.IsRevealed = clientTile.m_Tile.m_HexMap.Revealed;
			Choreographer.s_Choreographer.SetCharacterPositions();
			Log.Info("Spawned " + LocalizationManager.GetTranslation(cMonsterClass.LocKey) + " on tile (" + clientTile.m_Tile.m_ArrayIndex.X + ", " + clientTile.m_Tile.m_ArrayIndex.Y + ")");
			ScenarioManager.CurrentScenarioState.Update();
			return cActor;
		}
		Log.Error("Unable to find monster " + monsterID);
		return null;
	}

	public static bool SpawnObjectAtLocation(CClientTile tile, string objectID, int level = 1)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CObjectClass cObjectClass = MonsterClassManager.ObjectClasses.SingleOrDefault((CObjectClass x) => x.ID == objectID);
			cObjectClass.SetMonsterStatLevel(level);
			if (cObjectClass != null)
			{
				int nextID = cObjectClass.GetNextID();
				if (nextID == -1)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return false;
				}
				int num;
				for (num = nextID; num > cObjectClass.Models.Count - 1; num -= cObjectClass.Models.Count)
				{
				}
				ObjectState objectState = new ObjectState(cObjectClass.ID, num, null, tile.m_Tile.m_HexMap.MapGuid, new TileIndex(tile.m_Tile.m_ArrayIndex), cObjectClass.Health(), cObjectClass.Health(), level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, CActor.EType.Enemy);
				objectState.ID = nextID;
				if (ScenarioManager.Scenario.AddObject(objectState, initial: false) == null)
				{
					Log.Error("Maximum enemies of this type are already added to the scenario");
					return false;
				}
				ScenarioManager.CurrentScenarioState.Objects.Add(objectState);
				objectState.IsRevealed = tile.m_Tile.m_HexMap.Revealed;
				Choreographer.s_Choreographer.m_ClientObjects.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.Objects.Last()));
				Choreographer.s_Choreographer.SetCharacterPositions();
				Log.Info("Spawned " + LocalizationManager.GetTranslation(cObjectClass.LocKey) + " on tile (" + tile.m_Tile.m_ArrayIndex.X + ", " + tile.m_Tile.m_ArrayIndex.Y + ")");
				ScenarioManager.CurrentScenarioState.Update();
				return true;
			}
			Log.Error("Unable to find monster " + objectID);
		}
		else
		{
			Log.Error("Invalid location selected for monster to spawn.");
		}
		return false;
	}

	public static bool SpawnCharacterAtLocation(CClientTile tile, string characterID)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null)
		{
			CCharacterClass characterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == characterID);
			if (characterClass != null)
			{
				if (!ScenarioManager.CurrentScenarioState.Players.Any((PlayerState p) => p.ClassID == characterID))
				{
					List<CAbilityCard> list = new List<CAbilityCard>();
					list = CharacterClassManager.AllAbilityCards.Where((CAbilityCard x) => x.ClassID == characterClass.ID).ToList();
					List<Tuple<int, int>> selectedCardIDs = (from c in (from c in list.GetRange(0, list.Count)
							select new { c.ID, c.CardInstanceID }).AsEnumerable()
						select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList();
					PlayerState playerState = new PlayerState(characterID, 0, null, tile.m_Tile.m_HexMap.MapGuid, new TileIndex(tile.m_Tile.m_ArrayIndex), characterClass.Health(), characterClass.Health(), 1, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: false, CActor.ECauseOfDeath.StillAlive, 1, null, 0, 0, null, null, isLongResting: false, isShortResting: false, new AbilityDeckState(selectedCardIDs, list.Select((CAbilityCard c) => c.ID)), new AttackModifierDeckState(characterClass), new List<CItem>(), characterClass.TimeTokens, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EPressure.None, 1, 2, characterClass.TrophyTokens, 0, characterClass.ResonanceTokens)
					{
						IsRevealed = true
					};
					ScenarioManager.CurrentScenarioState.Players.Add(playerState);
					ScenarioManager.Scenario.AddPlayer(playerState, initial: false);
					playerState.IsRevealed = tile.m_Tile.m_HexMap.Revealed;
					playerState.HiddenAtStart = !playerState.IsRevealed;
					Choreographer.s_Choreographer.m_ClientPlayers.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.PlayerActors.Last()));
					Choreographer.s_Choreographer.SetCharacterPositions();
					Log.Info("Spawned " + LocalizationManager.GetTranslation(characterClass.LocKey) + " on tile (" + tile.m_Tile.m_ArrayIndex.X + ", " + tile.m_Tile.m_ArrayIndex.Y + ")");
					return true;
				}
				Log.Error("Data for Character " + characterID + " already exists in the party, so a new one was not added");
			}
			else
			{
				Log.Error("Unable to find Character " + characterID);
			}
		}
		else
		{
			Log.Error("Invalid location selected for character to spawn.");
		}
		return false;
	}

	private static void CalcPathingBlockersAndRotation(string propName, List<CClientTile> tiles, out List<TileIndex> pathingBlockers, out CVector3 rotationCEulers)
	{
		pathingBlockers = tiles.Select((CClientTile t) => new TileIndex(t.m_Tile.m_ArrayIndex)).ToList();
		if (propName == ESpecificPropType.Sarcophagus.ToString() || propName == ESpecificPropType.MovingPlatform.ToString() || propName == ESpecificPropType.MovingBoat136.ToString() || propName == ESpecificPropType.MovingBoat136B.ToString() || propName == ESpecificPropType.FlotsamTwoHex.ToString())
		{
			int num = 360 - WorldspaceStarHexDisplay.Instance.AreaEffectAngle;
			rotationCEulers = new CVector3(0f, num, 0f);
			return;
		}
		Quaternion quaternion = Quaternion.identity;
		int num2 = 0;
		if (tiles.Count == 1)
		{
			quaternion = Quaternion.identity * Quaternion.AngleAxis(WorldspaceStarHexDisplay.Instance.AreaEffectAngle, Vector3.up);
			num2 = 90;
		}
		else if (tiles.Count == 2)
		{
			quaternion = Quaternion.LookRotation(tiles[1].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position, Vector3.up);
			num2 = 90;
		}
		else if (tiles.Count == 3)
		{
			if (propName == "ThreeHexCurvedObstacle")
			{
				Vector3 vector = tiles[0].m_GameObject.transform.position - tiles[1].m_GameObject.transform.position;
				Vector3 vector2 = tiles[0].m_GameObject.transform.position - tiles[2].m_GameObject.transform.position;
				quaternion = Quaternion.LookRotation(vector - vector2, Vector3.up);
				num2 = 60;
			}
			else if (propName == "ThreeHexStraightObstacle")
			{
				quaternion = Quaternion.LookRotation(tiles[2].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position, Vector3.up);
				num2 = 90;
			}
			else
			{
				Vector3 vector3 = tiles[1].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position;
				Vector3 vector4 = tiles[2].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position;
				quaternion = Quaternion.LookRotation((Vector3.Cross(vector3, vector4).y < 0f) ? vector3 : vector4, Vector3.up);
				num2 = 150;
			}
		}
		Vector3 eulerAngles = quaternion.eulerAngles;
		eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + (float)num2, eulerAngles.z);
		rotationCEulers = GloomUtility.VToCV(eulerAngles);
	}

	public static bool SpawnPropAtLocation(List<CClientTile> tiles, string propName)
	{
		if (tiles == null || tiles.Count < 1)
		{
			Log.Info("Failed to spawn prop - Incorrect tile count");
			return false;
		}
		ScenarioManager.ObjectImportType objectImportType = GlobalSettings.GetObjectImportType(propName);
		foreach (CClientTile tile in tiles)
		{
			if (objectImportType == ScenarioManager.ObjectImportType.TerrainVisualEffect)
			{
				continue;
			}
			if (!CObjectProp.IsLootableObjectImportType(objectImportType))
			{
				switch (objectImportType)
				{
				case ScenarioManager.ObjectImportType.Tile:
				case ScenarioManager.ObjectImportType.TerrainHotCoals:
				case ScenarioManager.ObjectImportType.TerrainWater:
				case ScenarioManager.ObjectImportType.TerrainRubble:
				case ScenarioManager.ObjectImportType.TerrainThorns:
				case ScenarioManager.ObjectImportType.TerrainVisualEffect:
				{
					List<ScenarioManager.ObjectImportType> typesToCheckFor = new List<ScenarioManager.ObjectImportType>
					{
						ScenarioManager.ObjectImportType.TerrainHotCoals,
						ScenarioManager.ObjectImportType.TerrainRubble,
						ScenarioManager.ObjectImportType.TerrainThorns,
						ScenarioManager.ObjectImportType.TerrainWater,
						ScenarioManager.ObjectImportType.TerrainVisualEffect,
						ScenarioManager.ObjectImportType.Obstacle,
						ScenarioManager.ObjectImportType.PressurePlate
					};
					if (tile.m_Tile.CheckForPropTypes(typesToCheckFor))
					{
						return false;
					}
					break;
				}
				default:
					if ((!CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.EmptyHex) && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.MoneyToken) == null) || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) != null)
					{
						Log.Error("Failed to spawn prop - One of the tiles chosen was invalid");
						return false;
					}
					break;
				case ScenarioManager.ObjectImportType.Shadow:
					break;
				}
			}
			Log.Info("Currently Captured tile| X:" + tile.m_Tile.m_ArrayIndex.X + " Y:" + tile.m_Tile.m_ArrayIndex.Y);
		}
		switch (objectImportType)
		{
		case ScenarioManager.ObjectImportType.GenericProp:
		case ScenarioManager.ObjectImportType.VFXProp:
		{
			CObjectProp prop11 = new CObjectProp(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop11);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.MoneyToken:
		{
			CObjectGoldPile prop12 = new CObjectGoldPile(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop12);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Chest:
		{
			CObjectChest prop9 = new CObjectChest(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop9);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Obstacle:
		{
			CalcPathingBlockersAndRotation(propName, tiles, out var pathingBlockers3, out var rotationCEulers);
			bool flag = propName == "WallObstacle";
			string text = propName;
			TileIndex arrayIndex = new TileIndex(tiles[0].m_Tile.m_ArrayIndex);
			CVector3 rotation3 = rotationCEulers;
			List<TileIndex> pathingBlockers4 = pathingBlockers3;
			string mapGuid = tiles[0].m_Tile.m_HexMap.MapGuid;
			bool ignoresFlyAndJump = flag;
			bool blockLOS = flag;
			CObjectObstacle prop10 = new CObjectObstacle(text, objectImportType, arrayIndex, null, rotation3, pathingBlockers4, null, mapGuid, ignoresFlyAndJump, tiles.Count > 1, blockLOS);
			tiles[0].m_Tile.SpawnProp(prop10);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Trap:
		{
			CObjectTrap prop5 = new CObjectTrap(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, 0, 0, 0, new List<CCondition.ENegativeCondition>(), null, 0);
			tiles[0].m_Tile.SpawnProp(prop5);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.GoalChest:
		{
			CObjectChest prop6 = new CObjectChest(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop6);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.PressurePlate:
		{
			CObjectPressurePlate prop3 = new CObjectPressurePlate(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop3);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainHotCoals:
		case ScenarioManager.ObjectImportType.TerrainThorns:
		{
			CObjectHazardousTerrain prop4 = new CObjectHazardousTerrain(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop4);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainWater:
		case ScenarioManager.ObjectImportType.TerrainRubble:
		{
			CalcPathingBlockersAndRotation(propName, tiles, out var _, out var rotationCEulers3);
			CObjectDifficultTerrain prop19 = new CObjectDifficultTerrain(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, rotationCEulers3, null, tiles[0].m_Tile.m_HexMap.MapGuid, treatAsTrap: false, new CObjectiveFilter());
			tiles[0].m_Tile.SpawnProp(prop19);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Portal:
		{
			CObjectPortal prop18 = new CObjectPortal(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid, new CObjectiveFilter());
			tiles[0].m_Tile.SpawnProp(prop18);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.CarryableQuestItem:
		{
			CObjectQuestItem prop17 = new CObjectQuestItem(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop17);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainVisualEffect:
		{
			CalcPathingBlockersAndRotation(propName, tiles, out var _, out var rotationCEulers2);
			CObjectTerrainVisual prop16 = new CObjectTerrainVisual(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, rotationCEulers2, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop16);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Resource:
		{
			CObjectResource prop15 = new CObjectResource(ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == propName), propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop15);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.MonsterGrave:
		{
			CObjectMonsterGrave prop14 = new CObjectMonsterGrave(isEliteGrave: false, propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop14);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Shadow:
		{
			CObjectShadow prop13 = new CObjectShadow(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop13);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.SnowRock:
		{
			List<TileIndex> pathingBlockers2 = tiles.Select((CClientTile t) => new TileIndex(t.m_Tile.m_ArrayIndex)).ToList();
			Quaternion quaternion2 = Quaternion.identity;
			int num2 = 0;
			if (tiles.Count == 2)
			{
				quaternion2 = Quaternion.LookRotation(tiles[1].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position, Vector3.up);
				num2 = 90;
			}
			Vector3 eulerAngles2 = quaternion2.eulerAngles;
			eulerAngles2 = new Vector3(eulerAngles2.x, eulerAngles2.y + (float)num2, eulerAngles2.z);
			CVector3 rotation2 = GloomUtility.VToCV(eulerAngles2);
			CObjectSnowRock prop8 = new CObjectSnowRock(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_Hex.Position, rotation2, pathingBlockers2, null, tiles[0].m_Tile.m_HexMap.MapGuid, ignoresFlyAndJump: false, tiles.Count > 1);
			tiles[0].m_Tile.SpawnProp(prop8);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainIce:
		{
			CObjectIceTerrain prop7 = new CObjectIceTerrain(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop7);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Snowdrift:
		{
			List<TileIndex> pathingBlockers = tiles.Select((CClientTile t) => new TileIndex(t.m_Tile.m_ArrayIndex)).ToList();
			Quaternion quaternion = Quaternion.identity;
			int num = 0;
			if (tiles.Count == 2)
			{
				quaternion = Quaternion.LookRotation(tiles[1].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position, Vector3.up);
				num = 90;
			}
			Vector3 eulerAngles = quaternion.eulerAngles;
			eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + (float)num, eulerAngles.z);
			CVector3 rotation = GloomUtility.VToCV(eulerAngles);
			CObjectObstacle prop2 = new CObjectObstacle(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, rotation, pathingBlockers, null, tiles[0].m_Tile.m_HexMap.MapGuid, ignoresFlyAndJump: false, tiles.Count > 1);
			tiles[0].m_Tile.SpawnProp(prop2);
			Log.Info("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainLampVisualEffect:
		{
			CObjectProp prop = new CObjectProp(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop);
			Log.Info("Spawned prop - lamp " + propName);
			break;
		}
		default:
			Log.Error("Unsupported prop type!");
			break;
		}
		return true;
	}

	public static bool SpawnSpawnerAtLocation(List<CClientTile> tiles)
	{
		if (tiles == null || tiles.Count < 1)
		{
			Log.Info("Failed to spawn spawner - Incorrect tile count");
			return false;
		}
		SpawnRoundEntry item = new SpawnRoundEntry(new List<string> { "", "Bandit Archer", "Bandit Guard", "Bandit Guard" });
		Dictionary<string, List<SpawnRoundEntry>> dictionary = new Dictionary<string, List<SpawnRoundEntry>>();
		dictionary.Add("Default", new List<SpawnRoundEntry> { item });
		List<int> spawnRoundInterval = new List<int> { 1, 1, 1, 1 };
		SpawnerData spawnerData = new SpawnerData(SpawnerData.ESpawnerTriggerType.StartRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 2, loopSpawnPattern: true, shouldCountTowardsKillAllEnemies: false, spawnRoundInterval, dictionary, "");
		List<CMapTile> list = new List<CMapTile>();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			foreach (CClientTile tile in tiles)
			{
				if (tile.m_Tile.m_HexMap == map || tile.m_Tile.m_Hex2Map == map)
				{
					list.Add(map.MapTiles.SingleOrDefault((CMapTile m) => m.ArrayIndex.X == tile.m_Tile.m_ArrayIndex.X && m.ArrayIndex.Y == tile.m_Tile.m_ArrayIndex.Y));
				}
			}
		}
		CSpawner spawner = new CSpawner(spawnerData, list[0].ArrayIndex, tiles[0].m_Tile.m_HexMap.MapGuid);
		tiles[0].m_Tile.SpawnSpawner(spawner);
		return true;
	}

	public static bool RevealHiddenCharacterInState(PlayerState stateToReveal, bool forLevelEditor = false)
	{
		try
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.AddPlayer(stateToReveal, initial: false);
			stateToReveal.Load(forceLoad: true);
			stateToReveal.LoadAbiltyDeck(forceLoad: true);
			if (!forLevelEditor)
			{
				stateToReveal.IsRevealed = true;
			}
			Choreographer.s_Choreographer.m_ClientPlayers.Add(Choreographer.s_Choreographer.CreateCharacterActor(CAutoTileClick.TileIndexToClientTile(new TileIndex(cPlayerActor.ArrayIndex)), cPlayerActor));
			Choreographer.s_Choreographer.SetCharacterPositions();
			Log.Info("Revealed " + LocalizationManager.GetTranslation(cPlayerActor.ActorLocKey()));
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Failed to reveal hidden character\nException:" + ex.Message + "\nStack:" + ex.StackTrace);
			return false;
		}
	}

	public static bool RemoveMonster(CEnemyActor enemyToRemove)
	{
		try
		{
			enemyToRemove.MonsterClass.RecycleID(enemyToRemove.ID);
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(enemyToRemove);
			ScenarioManager.Scenario.RemoveEnemy(enemyToRemove);
			ScenarioManager.Scenario.DeadEnemies.Remove(enemyToRemove);
			ScenarioManager.Scenario.RemoveAllyMonsters(enemyToRemove);
			ScenarioManager.Scenario.DeadAllyMonsters.Remove(enemyToRemove);
			ScenarioManager.Scenario.RemoveEnemy2Monster(enemyToRemove);
			ScenarioManager.Scenario.DeadEnemy2Monsters.Remove(enemyToRemove);
			ScenarioManager.Scenario.RemoveNeutralMonster(enemyToRemove);
			ScenarioManager.Scenario.DeadNeutralMonsters.Remove(enemyToRemove);
			ScenarioManager.CurrentScenarioState.Monsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.AllyMonsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.Enemy2Monsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.NeutralMonsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.ResetEnemyClassManager();
			ScenarioManager.CurrentScenarioState.Update();
			Choreographer.s_Choreographer.m_ClientEnemies.Remove(gameObject);
			Choreographer.s_Choreographer.m_ClientAllyMonsters.Remove(gameObject);
			Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Remove(gameObject);
			Choreographer.s_Choreographer.m_ClientNeutralMonsters.Remove(gameObject);
			Choreographer.s_Choreographer.AllActorsInScene.Remove(gameObject);
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI?.Destroy();
			UnityEngine.Object.Destroy(gameObject);
			return true;
		}
		catch
		{
			Log.Error("Failed to remove Enemy: " + LocalizationManager.GetTranslation(enemyToRemove.ActorLocKey()));
			return false;
		}
	}

	public static bool RemoveObject(CObjectActor objectToRemove)
	{
		try
		{
			objectToRemove.MonsterClass.RecycleID(objectToRemove.ID);
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(objectToRemove);
			ScenarioManager.Scenario.RemoveObjectActor(objectToRemove);
			ScenarioManager.Scenario.DeadObjects.Remove(objectToRemove);
			ScenarioManager.CurrentScenarioState.Objects.RemoveAll((ObjectState ec) => ec.ActorGuid == objectToRemove.ActorGuid);
			Choreographer.s_Choreographer.m_ClientObjects.Remove(gameObject);
			Choreographer.s_Choreographer.AllActorsInScene.Remove(gameObject);
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI?.Destroy();
			UnityEngine.Object.Destroy(gameObject);
			return true;
		}
		catch
		{
			Log.Error("Failed to remove Obect: " + LocalizationManager.GetTranslation(objectToRemove.ActorLocKey()));
			return false;
		}
	}

	public static bool RemovePlayer(CPlayerActor playerToRemove)
	{
		try
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(playerToRemove);
			ScenarioManager.Scenario.RemovePlayer(playerToRemove);
			ScenarioManager.Scenario.ExhaustedPlayers.Remove(playerToRemove);
			ScenarioManager.CurrentScenarioState.Players.RemoveAll((PlayerState pc) => pc.ActorGuid == playerToRemove.ActorGuid);
			Choreographer.s_Choreographer.m_ClientPlayers.Remove(gameObject);
			Choreographer.s_Choreographer.AllActorsInScene.Remove(gameObject);
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI?.Destroy();
			UnityEngine.Object.Destroy(gameObject);
			return true;
		}
		catch
		{
			Log.Error("Failed to remove Player Character: " + LocalizationManager.GetTranslation(playerToRemove.ActorLocKey()));
			return false;
		}
	}

	public static bool RemoveProp(CObjectProp propToRemove)
	{
		try
		{
			CObjectProp.FindPropTile(propToRemove.InstanceName)?.DelProp(propToRemove.PropGuid);
			ScenarioManager.CurrentScenarioState.Props.RemoveAll((CObjectProp p) => p.PropGuid == propToRemove.PropGuid);
			GameObject obj = GameObject.Find(propToRemove.InstanceName);
			if (propToRemove.ObjectType == ScenarioManager.ObjectImportType.Tile || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect)
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
			else
			{
				UnityEngine.Object.Destroy(obj, 0f);
			}
			Log.Info("Successfully removed prop" + propToRemove.PrefabName);
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to remove to remove prop: " + propToRemove.PrefabName + "\nException: " + ex.Message + "\nStack: " + ex.StackTrace);
			return false;
		}
	}

	public static bool RemoveSpawner(CSpawner spawnerToRemove)
	{
		try
		{
			ScenarioManager.CurrentScenarioState.Spawners.Remove(spawnerToRemove);
			if (spawnerToRemove is CInteractableSpawner { Prop: not null } cInteractableSpawner)
			{
				RemoveProp(cInteractableSpawner.Prop);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Exception trying to remove to remove spawner: " + spawnerToRemove.SpawnerGuid + "\nException: " + ex.Message + "\nStack: " + ex.StackTrace);
			return false;
		}
	}

	public static bool MoveMonsterToLocation(CClientTile tile, string monsterGuid)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.First((CEnemyActor enemyActor) => enemyActor.ActorGuid == monsterGuid);
			if (cEnemyActor != null)
			{
				ScenarioManager.CurrentScenarioState.Monsters.Single((EnemyState p) => p.ActorGuid == monsterGuid).Location = new TileIndex(tile.m_Tile.m_ArrayIndex);
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor);
				cEnemyActor.ArrayIndex = tile.m_Tile.m_ArrayIndex;
				ActorBehaviour.GetActorBehaviour(gameObject).ForceSetLocoIntermediateTarget(tile.m_GameObject.transform.position);
				return true;
			}
		}
		return false;
	}

	public static bool MoveObjectToLocation(CClientTile tile, string monsterGuid)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CObjectActor cObjectActor = ScenarioManager.Scenario.Objects.First((CObjectActor objectActor) => objectActor.ActorGuid == monsterGuid);
			if (cObjectActor != null)
			{
				ScenarioManager.CurrentScenarioState.Objects.Single((ObjectState p) => p.ActorGuid == monsterGuid).Location = new TileIndex(tile.m_Tile.m_ArrayIndex);
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cObjectActor);
				cObjectActor.ArrayIndex = tile.m_Tile.m_ArrayIndex;
				ActorBehaviour.GetActorBehaviour(gameObject).ForceSetLocoIntermediateTarget(tile.m_GameObject.transform.position);
				return true;
			}
		}
		return false;
	}

	public static bool MoveCharacterToLocation(CClientTile tile, string characterGuid)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.First((CPlayerActor playerActor) => playerActor.ActorGuid == characterGuid);
			if (cPlayerActor != null)
			{
				ScenarioManager.CurrentScenarioState.Players.Single((PlayerState p) => p.ActorGuid == characterGuid).Location = new TileIndex(tile.m_Tile.m_ArrayIndex);
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cPlayerActor);
				cPlayerActor.ArrayIndex = tile.m_Tile.m_ArrayIndex;
				ActorBehaviour.GetActorBehaviour(gameObject).ForceSetLocoIntermediateTarget(tile.m_GameObject.transform.position);
				return true;
			}
		}
		return false;
	}

	public static bool MovePropToLocation(CClientTile tile, string propGuid)
	{
		CObjectProp cObjectProp = FindProp(propGuid);
		if (cObjectProp != null)
		{
			if (cObjectProp is CObjectObstacle cObjectObstacle && cObjectObstacle.PathingBlockers.Count > 1)
			{
				return false;
			}
			CTile cTile = CObjectProp.FindPropTile(cObjectProp.InstanceName);
			cTile.DelProp(propGuid);
			cTile.m_Hex2Map?.Props.RemoveAll((CObjectProp prop) => prop.PropGuid == propGuid);
			cTile.m_HexMap?.Props.RemoveAll((CObjectProp prop) => prop.PropGuid == propGuid);
			cObjectProp.SetLocation(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y), GloomUtility.VToCV(tile.m_GameObject.transform.position), cObjectProp.Rotation);
			tile.m_Tile.AddProp(cObjectProp);
			cObjectProp.SetNewStartingMapGuid(tile.m_Tile.m_HexMap.MapGuid);
			GameObject.Find(cObjectProp.InstanceName).transform.position = tile.m_GameObject.transform.position;
			tile.m_Tile.m_HexMap?.Props.Add(cObjectProp);
			tile.m_Tile.m_Hex2Map?.Props.Add(cObjectProp);
			if (cObjectProp is CObjectObstacle cObjectObstacle2)
			{
				cObjectObstacle2.PathingBlockers.Clear();
				cObjectObstacle2.PathingBlockers.Add(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y));
			}
			return true;
		}
		return false;
	}

	public static CObjectProp FindProp(string propGuid)
	{
		foreach (CMap map in ScenarioManager.Scenario.Maps)
		{
			CObjectProp cObjectProp = map.Props.SingleOrDefault((CObjectProp prop) => prop.PropGuid == propGuid);
			if (cObjectProp != null)
			{
				return cObjectProp;
			}
		}
		return null;
	}

	public static bool MoveSpawnerToLocation(CClientTile tile, string spawnerGuid)
	{
		if (CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.None))
		{
			CSpawner cSpawner = null;
			foreach (CMap map in ScenarioManager.Scenario.Maps)
			{
				if (map.Spawners.Any((CSpawner prop) => prop.SpawnerGuid == spawnerGuid))
				{
					cSpawner = map.Spawners.First((CSpawner prop) => prop.SpawnerGuid == spawnerGuid);
					break;
				}
			}
			if (cSpawner != null)
			{
				cSpawner.SetLocation(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y));
				if (cSpawner.Prop != null)
				{
					cSpawner.Prop.SetLocation(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y), GloomUtility.VToCV(tile.m_GameObject.transform.position), cSpawner.Prop.Rotation);
					cSpawner.Prop.SetNewStartingMapGuid(tile.m_Tile.m_HexMap.MapGuid);
					GameObject.Find(cSpawner.Prop.InstanceName).transform.position = tile.m_GameObject.transform.position;
				}
				return true;
			}
		}
		return false;
	}

	private void SnapGameObjectToHexes(GameObject gameObject)
	{
		gameObject.GetComponent<UnityGameEditorObject>();
		Vector3 position = gameObject.transform.position;
		position.z = MF.SnapFloat(UnityGameEditorRuntime.s_TileSize.z, position.z);
		if ((Mathf.Abs(MF.GetTileIntegerSnapSpace(position).z) & 1) == 1)
		{
			position.x = MF.SnapFloat(UnityGameEditorRuntime.s_TileSize.x, position.x - UnityGameEditorRuntime.s_TileSize.x * 0.5f) + UnityGameEditorRuntime.s_TileSize.x * 0.5f;
		}
		else
		{
			position.x = MF.SnapFloat(UnityGameEditorRuntime.s_TileSize.x, position.x);
		}
		position.y = 0f;
		gameObject.transform.position = position;
	}

	public async void IterateOverAllAvailableLevels(float perLevelPostLoadDelay, Func<UnityAction<string>, CCustomLevelData, IEnumerator> perLevelOperation, UnityAction<string> onCompleteOperation, bool loadIntoLevelEditor = true)
	{
		if (m_BulkLevelProcessingRoutine != null)
		{
			StopCoroutine(m_BulkLevelProcessingRoutine);
			m_BulkLevelProcessingRoutine = null;
		}
		List<FileInfo> availableFiles = SaveData.Instance.LevelEditorDataManager.AvailableFiles;
		List<CCustomLevelData> availableLevels = new List<CCustomLevelData>(availableFiles.Count);
		foreach (FileInfo item in availableFiles)
		{
			availableLevels.Add(await SaveData.Instance.LevelEditorDataManager.GetLevelDataForFile(item));
		}
		m_BulkLevelProcessingRoutine = LoopThoughLevelsAndProcess(availableLevels, perLevelPostLoadDelay, perLevelOperation, onCompleteOperation, loadIntoLevelEditor);
		StartCoroutine(m_BulkLevelProcessingRoutine);
	}

	private IEnumerator LoopThoughLevelsAndProcess(List<CCustomLevelData> levels, float perLevelPostLoadDelay, Func<UnityAction<string>, CCustomLevelData, IEnumerator> perLevelOperation, UnityAction<string> onCompleteOperation, bool loadIntoLevelEditor = true)
	{
		SceneController.Instance.DisableLoadingScreen();
		string reportString = "Bulk Process of [" + levels.Count + "] Levels" + ((levels.Count == 1) ? string.Empty : "s") + " Start " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n==================================================";
		new List<string>();
		int numberOfLevelsProcessed = 0;
		BulkLevelProcessingInProgress = true;
		Application.logMessageReceived += LogCallback;
		while (numberOfLevelsProcessed < levels.Count && !m_EndBulkRun)
		{
			if (loadIntoLevelEditor)
			{
				ELevelEditorState currentState = CurrentState;
				if (currentState != ELevelEditorState.Idle)
				{
					if (currentState == ELevelEditorState.Editing)
					{
						while (TransitionManager.s_Instance == null || !TransitionManager.s_Instance.TransitionDone || SceneController.Instance.ScenarioIsLoading || SceneController.Instance.IsLoading)
						{
							yield return null;
						}
						yield return new WaitForSeconds(perLevelPostLoadDelay);
						yield return perLevelOperation(ProcessResultAction, SaveData.Instance.Global.CurrentEditorLevelData);
						if (LoggedErrors.Count > 0)
						{
							reportString = reportString + ": \n\t\tThe following Exceptions occurred during this level process:\n\t\t\t" + LoggedErrors.Select((string s) => SecurityElement.Escape(s));
							LoggedErrors.Clear();
						}
						if (LoggedWarnings.Count > 0)
						{
							reportString = reportString + ": \n\t\tThe following Warnings occurred during this level process:\n\t\t\t" + LoggedWarnings.Select((string s) => SecurityElement.Escape(s));
							LoggedWarnings.Clear();
						}
						numberOfLevelsProcessed++;
						s_Instance?.ExitToMainMenuThroughUI();
						UIManager.LoadMainMenu();
					}
				}
				else if (SaveData.Instance.Global.GameMode == EGameMode.MainMenu && !SceneController.Instance.IsLoading)
				{
					BulkProcessingErrorOccurred = false;
					reportString += $"\n\t{levels[numberOfLevelsProcessed].Name.ToUpper()}:\n\t=============";
					if (levels[numberOfLevelsProcessed].ScenarioState != null)
					{
						Task task = SaveData.Instance.LoadCustomLevelFromData(levels[numberOfLevelsProcessed], ELevelEditorState.Editing);
						yield return new WaitForTask(task);
						SceneController.Instance.LevelEditorStart();
					}
					else
					{
						numberOfLevelsProcessed++;
					}
				}
			}
			else
			{
				BulkProcessingErrorOccurred = false;
				reportString += $"\n\t{levels[numberOfLevelsProcessed].Name.ToUpper()}:\n\t=============";
				if (levels[numberOfLevelsProcessed].ScenarioState != null)
				{
					yield return new WaitForSeconds(perLevelPostLoadDelay);
					Task<CCustomLevelData> deepCopyTask = levels[numberOfLevelsProcessed].DeepCopySerializableObject<CCustomLevelData>();
					yield return new WaitForTask(deepCopyTask);
					yield return perLevelOperation(ProcessResultAction, deepCopyTask.Result);
					if (LoggedErrors.Count > 0)
					{
						reportString = reportString + ": \n\t\tThe following Exceptions occurred during this level process:\n\t\t\t" + LoggedErrors.Select((string s) => SecurityElement.Escape(s));
						LoggedErrors.Clear();
					}
					if (LoggedWarnings.Count > 0)
					{
						reportString = reportString + ": \n\t\tThe following Warnings occurred during this level process:\n\t\t\t" + LoggedWarnings.Select((string s) => SecurityElement.Escape(s));
						LoggedWarnings.Clear();
					}
					numberOfLevelsProcessed++;
				}
				else
				{
					numberOfLevelsProcessed++;
				}
			}
			yield return null;
		}
		m_EndBulkRun = false;
		Application.logMessageReceived -= LogCallback;
		try
		{
			List<string> list = new List<string>();
			while (reportString.Length > 16000)
			{
				int num = reportString.IndexOf('\n', 16000);
				if (num > 0 && num + 1 < reportString.Length)
				{
					list.Add(reportString.Substring(0, num));
					reportString = reportString.Substring(num + 1);
					continue;
				}
				list.Add(reportString);
				break;
			}
			if (reportString.Length > 0)
			{
				list.Add(reportString);
			}
			foreach (string item in list)
			{
				Log.Info(item);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception while trying to split report for debug log.");
		}
		BulkLevelProcessingInProgress = false;
		onCompleteOperation?.Invoke(reportString);
		void ProcessResultAction(string processResult)
		{
			reportString = reportString + "\n\t" + processResult;
		}
	}

	private void LogCallback(string condition, string stacktrace, LogType type)
	{
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			if (type != LogType.Error || !condition.Contains("AmplitudeHttp"))
			{
				LoggedErrors.Add(condition + "\n" + stacktrace);
			}
			break;
		case LogType.Warning:
			if (!condition.Contains("SendMessage"))
			{
				LoggedWarnings.Add(condition);
			}
			break;
		}
	}

	public void August12BulkLevelProcessingLoop()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(10f, LevelOperation, onCompleteOperation);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData _irrelevantLevelData)
		{
			s_Instance.m_LevelEditorUIInstance.ApparancePanel.EnsureAllStylesHaveAnOverride();
			s_Instance.m_LevelEditorUIInstance.LevelDataPanel.OnSaveDataPressed();
			int num = 0;
			foreach (CObjectProp prop in SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState.Props)
			{
				if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth)
				{
					num++;
				}
			}
			string text = $"{SaveData.Instance.Global.CurrentEditorLevelData.Name} Apparance overrides updated. It has {num} Props with health";
			Log.Info(text);
			processString(text);
			yield break;
		}
	}

	public void September8thRemovePlayerStatesFromCampaignScenariosBulkLevelProcessingLoop()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(10f, LevelOperation, onCompleteOperation);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData _irrelevantLevelData)
		{
			for (int num = ScenarioManager.Scenario.PlayerActors.Count - 1; num >= 0; num--)
			{
				RemovePlayer(ScenarioManager.Scenario.PlayerActors[num]);
			}
			ScenarioManager.Scenario.ExhaustedPlayers.Clear();
			ScenarioManager.CurrentScenarioState.Players.Clear();
			string text = ((ScenarioManager.CurrentScenarioState.RoundNumber > 1) ? $"{SaveData.Instance.Global.CurrentEditorLevelData.Name} Scenario State round number was INCORRECT" : $"{SaveData.Instance.Global.CurrentEditorLevelData.Name} Scenario State round number was CORRECT");
			ScenarioManager.CurrentScenarioState.RoundNumber = 1;
			foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				actorState.ResetCauseOfDeath();
			}
			s_Instance.m_LevelEditorUIInstance.LevelDataPanel.OnSaveDataPressed();
			Log.Info(text);
			processString(text);
			yield break;
		}
	}

	public void Sept15thBulkLevelProcessingLoop()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, PerLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator PerLevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			int numberOfPropsWithHealthNeedingUpdating = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && prop.PropActorHasBeenAssigned)
				{
					numberOfPropsWithHealthNeedingUpdating++;
					prop.ClearAttachedRuntimeActor();
				}
			}
			List<string> objectGuidsToRemove = new List<string>();
			foreach (ObjectState @object in levelData.ScenarioState.Objects)
			{
				if (@object.IsAttachedToProp)
				{
					objectGuidsToRemove.Add(@object.ActorGuid);
				}
			}
			levelData.ScenarioState.Objects.RemoveAll((ObjectState o) => objectGuidsToRemove.Contains(o.ActorGuid));
			string text = "Didn't need saving";
			if (numberOfPropsWithHealthNeedingUpdating > 0 || objectGuidsToRemove.Count > 0)
			{
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				text = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
			}
			string text2 = $"Level {levelData.Name} Processed. It had {numberOfPropsWithHealthNeedingUpdating} Props with health needing updates, {objectGuidsToRemove.Count} Objects were removed from the state, {text}";
			Log.Info(text2);
			processString(text2);
		}
	}

	public async void BulkTransplantApparanceOverridesFromVerySpecificFolder()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo("C:\\Users\\seanw\\Desktop\\UnBorkage-Working\\Latest before applying fix");
		List<CCustomLevelData> levelsInFile = new List<CCustomLevelData>();
		if (directoryInfo.Exists)
		{
			FileInfo[] files = directoryInfo.GetFiles("*.lvldat", SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo file in array)
			{
				CCustomLevelData cCustomLevelData = await SaveData.Instance.LevelEditorDataManager.GetLevelDataForFile(file);
				if (cCustomLevelData != null)
				{
					levelsInFile.Add(cCustomLevelData);
				}
			}
		}
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			bool flag = false;
			bool flag2 = false;
			string text = "Didn't need saving";
			CCustomLevelData cCustomLevelData2 = levelsInFile.FirstOrDefault((CCustomLevelData l) => l.Name == levelData.Name);
			if (cCustomLevelData2 != null)
			{
				List<CApparanceOverrideDetails> list = cCustomLevelData2.ApparanceOverrideList.Except(levelData.ApparanceOverrideList).ToList();
				List<CApparanceOverrideDetails> list2 = levelData.ApparanceOverrideList.Except(cCustomLevelData2.ApparanceOverrideList).ToList();
				if (list.Count > 0 || list2.Count > 0)
				{
					levelData.ApparanceOverrideList = cCustomLevelData2.ApparanceOverrideList.ToList();
					Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
					yield return new WaitForTask(saveTask);
					text = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
					flag = true;
				}
				flag2 = true;
			}
			string text2 = string.Format("Level {0} Processed. {1}, {2}, {3}", levelData.Name, flag2 ? "Successfully found a level to transplant from" : "Failed to find a level to transplant from", flag ? "Level needed a transplant" : "Level didn't need a transplant", text);
			Log.Info(text2);
			processString(text2);
		}
	}

	public void BulkCheckForIncorrectPathingBlockersInObstacles()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			List<CObjectObstacle> list = new List<CObjectObstacle>();
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				CObjectObstacle obstacleProp = prop as CObjectObstacle;
				if (obstacleProp != null && !obstacleProp.PathingBlockers.Any((TileIndex x) => x.Equals(obstacleProp.ArrayIndex)))
				{
					list.Add(obstacleProp);
				}
			}
			string text = $"Level {levelData.Name} Processed. It had {list.Count} obstacle props needing replacing as they have incorrect PathingBlocker indexes.";
			Log.Info(text);
			foreach (CObjectObstacle item in list)
			{
				Log.Info($"Obstacle with GUID: {item.PropGuid} in Level {levelData.Name} has incorrect pathing blockers");
			}
			processString(text);
			yield break;
		}
	}

	public void BulkCheckForScenarioModifierLevelEvents()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			int num = 0;
			foreach (CLevelEvent levelEvent in levelData.LevelEvents)
			{
				if (levelEvent.EventType == CLevelEvent.ELevelEventType.CloseDoor || levelEvent.EventType == CLevelEvent.ELevelEventType.UnlockDoor)
				{
					num++;
				}
			}
			string text = $"Level {levelData.Name} Processed. It had {num} level events that need replacing as closing/opening/unlocking doors from the main thread is not safe.";
			Log.Info(text);
			processString(text);
			yield break;
		}
	}

	public void BulkFixTrapPropPrefabName()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			int adjustPrefabNameCount = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				if (prop.ObjectType == ScenarioManager.ObjectImportType.Trap && prop.PrefabName == "BearTrap")
				{
					prop.PrefabName = "Trap";
					adjustPrefabNameCount++;
				}
			}
			string arg = "Didn't need saving";
			if (adjustPrefabNameCount > 0)
			{
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				arg = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
			}
			string text = $"Level {levelData.Name} Processed. It had {adjustPrefabNameCount} traps that needed their prefab name replacing, {arg}.";
			Log.Info(text);
			processString(text);
		}
	}

	public void BulkFixIncorrectBakedPropsDuringLevelEditor()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			int adjustPrefabCount = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				if (prop.IsBaked)
				{
					prop.IsBaked = false;
					adjustPrefabCount++;
				}
			}
			string arg = "Didn't need saving";
			if (adjustPrefabCount > 0)
			{
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				arg = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
			}
			string text = $"Level {levelData.Name} Processed. It had {adjustPrefabCount} props that needed to be set as not baked, {arg}.";
			Log.Info(text);
			processString(text);
		}
	}

	public static void ReplaceEels(ScenarioState scenarioState, out int adjustedMonsterCount)
	{
		adjustedMonsterCount = 0;
		if (scenarioState == null)
		{
			return;
		}
		foreach (EnemyState item in scenarioState.Monsters.Concat(scenarioState.NeutralMonsters).Concat(scenarioState.AllyMonsters).Concat(scenarioState.Enemy2Monsters))
		{
			if (item.ClassID == "LightningEelSeaID")
			{
				item.ClassID = "LightningEelID";
				adjustedMonsterCount++;
			}
			if (item.ClassID == "LightningEelSeaEliteID")
			{
				item.ClassID = "LightningEelEliteID";
				adjustedMonsterCount++;
			}
		}
		foreach (CSpawner spawner in scenarioState.Spawners)
		{
			foreach (KeyValuePair<string, List<SpawnRoundEntry>> item2 in spawner.SpawnerData.SpawnRoundEntryDictionary)
			{
				foreach (SpawnRoundEntry item3 in item2.Value)
				{
					for (int i = 0; i < item3.SpawnClass.Count; i++)
					{
						string text = item3.SpawnClass[i];
						if (text == "LightningEelSeaID")
						{
							item3.SpawnClass[i] = "LightningEelID";
							adjustedMonsterCount++;
						}
						if (text == "LightningEelSeaEliteID")
						{
							item3.SpawnClass[i] = "LightningEelEliteID";
							adjustedMonsterCount++;
						}
					}
				}
			}
		}
		foreach (EnemyClassState enemyClass in scenarioState.EnemyClassManager.EnemyClasses)
		{
			string classID = enemyClass.ClassID;
			if (classID == "LightningEelSeaID")
			{
				enemyClass.ClassID = "LightningEelID";
				adjustedMonsterCount++;
			}
			if (classID == "LightningEelSeaEliteID")
			{
				enemyClass.ClassID = "LightningEelEliteID";
				adjustedMonsterCount++;
			}
		}
	}

	public void BulkReplaceAllWaterVersionsOfLightningEelWithNormaolOne()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			ReplaceEels(levelData.ScenarioState, out var adjustedMonsterCount);
			string arg = "Didn't need saving";
			if (adjustedMonsterCount > 0)
			{
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				arg = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
			}
			string text = $"Level {levelData.Name} Processed. It had {adjustedMonsterCount} sea lightning eels that needed to be changed, {arg}.";
			Log.Info(text);
			processString(text);
		}
	}

	public void BulkSaveOutPropApparanceSettings()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(10f, LevelOperation, onCompleteOperation);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			int adjustedPropsCount = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				CApparanceOverrideDetails apparanceOverrideDetailsForGUID = levelData.GetApparanceOverrideDetailsForGUID(prop.PropGuid);
				if (apparanceOverrideDetailsForGUID != null && apparanceOverrideDetailsForGUID.OverrideSiblingIndex == -1)
				{
					GameObject.Find(prop.InstanceName).GetComponent<ProceduralStyle>().FetchObjectHierarchyValues(out var objectIndex, out var objectCount);
					apparanceOverrideDetailsForGUID.OverrideSiblingIndex = objectIndex;
					objectCount = adjustedPropsCount++;
				}
			}
			string arg = "Didn't need saving";
			if (adjustedPropsCount > 0)
			{
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				arg = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
			}
			string text = $"Level {levelData.Name} Processed. It had {adjustedPropsCount} props that needed their Apparance Override Sibling Index fixing, {arg}.";
			Log.Info(text);
			processString(text);
		}
	}

	public void BulkCheckForSpecificScenarioModifiersAndLog()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			List<EScenarioModifierType> typesToCheckFor = new List<EScenarioModifierType>
			{
				EScenarioModifierType.ApplyConditionToActor,
				EScenarioModifierType.MoveActorsInDirection,
				EScenarioModifierType.ApplyActiveBonusToActor,
				EScenarioModifierType.MovePropsInSequence,
				EScenarioModifierType.MovePropsToNearestPlayer,
				EScenarioModifierType.TriggerAbility
			};
			string text = string.Empty;
			if (levelData.ScenarioState.ScenarioModifiers.Any((CScenarioModifier x) => typesToCheckFor.Contains(x.ScenarioModifierType)))
			{
				text = $"Level {levelData.Name} Processed. Found one of the scenario modifiers we're searching for in it.";
				Log.Info(text);
			}
			processString(text);
			yield break;
		}
	}

	public void BulkFixMaxHealthLevelData()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			string retVal = string.Empty;
			if (!levelData.EnemyMaxHealthBasedOnPartyLevel)
			{
				levelData.EnemyMaxHealthBasedOnPartyLevel = true;
				retVal = $"Level {levelData.Name} Processed. Updating EnemyMaxHealthBasedOnPartyLevel value to be true.";
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				_ = saveTask.Result.success;
				Log.Info(retVal);
			}
			processString(retVal);
		}
	}

	public void Bulk_FH_7521_MigrateMonsterCardIDs()
	{
		Dictionary<int, int> replaceIDs = (from line in File.ReadLines("__ReplaceIDs.csv")
			select line.Split(',')).ToDictionary((string[] values) => int.Parse(values[0]), (string[] values) => int.Parse(values[1]));
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			bool flag = false;
			StringBuilder retVal = new StringBuilder();
			foreach (EnemyClassState enemyClass in levelData.ScenarioState.EnemyClassManager.EnemyClasses)
			{
				if (MigrateDeckTuple(enemyClass, "SelectedAbilityCardIDsAndInstanceIDs", enemyClass.AbilityDeck.SelectedAbilityCardIDsAndInstanceIDs, retVal, levelData.Name))
				{
					flag = true;
				}
				if (MigrateDeckTuple(enemyClass, "HandAbilityCardIDsAndInstanceIDs", enemyClass.AbilityDeck.HandAbilityCardIDsAndInstanceIDs, retVal, levelData.Name))
				{
					flag = true;
				}
				if (MigrateDeckTuple(enemyClass, "RoundAbilityCardIDsAndInstanceID", enemyClass.AbilityDeck.RoundAbilityCardIDsAndInstanceID, retVal, levelData.Name))
				{
					flag = true;
				}
				if (MigrateDeckTuple(enemyClass, "RoundCardsSelectedInCardSelectionIDsAndInstanceID", enemyClass.AbilityDeck.RoundCardsSelectedInCardSelectionIDsAndInstanceID, retVal, levelData.Name))
				{
					flag = true;
				}
				if (MigrateDeck(enemyClass, "DiscardedAbilityCardIDs", enemyClass.AbilityDeck.DiscardedAbilityCardIDs, retVal, levelData.Name))
				{
					flag = true;
				}
				if (MigrateDeck(enemyClass, "LostAbilityCardIDs", enemyClass.AbilityDeck.LostAbilityCardIDs, retVal, levelData.Name))
				{
					flag = true;
				}
				if (MigrateDeck(enemyClass, "PermaLostAbilityCardIDs", enemyClass.AbilityDeck.PermaLostAbilityCardIDs, retVal, levelData.Name))
				{
					flag = true;
				}
			}
			if (flag)
			{
				Task<(bool success, string path)> saveTask = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(saveTask);
				string text = (saveTask.Result.success ? "Saved successfully" : "Failed to save");
				retVal.AppendLine("Level " + levelData.Name + " Processed. " + text);
			}
			processString(retVal.ToString());
		}
		bool MigrateDeck(EnemyClassState enemyClassState, string deckType, List<int> deck, StringBuilder retVal, string levelName)
		{
			bool result = false;
			if (deck != null && deck.Count > 0)
			{
				for (int i = 0; i < deck.Count; i++)
				{
					int num = deck[i];
					if (replaceIDs.TryGetValue(num, out var value))
					{
						retVal.AppendLine($"Replacing {num} with {value} for \"{deckType}\" deck of \"{enemyClassState.ClassID}\" class in \"{levelName}\"");
						num = value;
						result = true;
					}
					if (num > 10000)
					{
						retVal.AppendLine($"10k ID left: {num} for \"{deckType}\" deck of  \"{enemyClassState.ClassID}\" class in \"{levelName}\"");
					}
					deck[i] = num;
				}
			}
			return result;
		}
		bool MigrateDeckTuple(EnemyClassState enemyClassState, string deckType, List<Tuple<int, int>> deck, StringBuilder retVal, string levelName)
		{
			bool result = false;
			if (deck != null && deck.Count > 0)
			{
				for (int i = 0; i < deck.Count; i++)
				{
					Tuple<int, int> tuple = deck[i];
					if (replaceIDs.TryGetValue(tuple.Item1, out var value) && tuple.Item1 != value)
					{
						retVal.AppendLine($"Replacing {tuple.Item1} with {value} for \"{deckType}\" deck of \"{enemyClassState.ClassID}\" class in \"{levelName}\"");
						tuple = Tuple.Create(value, tuple.Item2);
						result = true;
					}
					if (replaceIDs.TryGetValue(tuple.Item2, out var value2) && tuple.Item2 != value2)
					{
						retVal.AppendLine($"Replacing {tuple.Item2} with {value2} for \"{deckType}\" deck of \"{enemyClassState.ClassID}\" class in \"{levelName}\"");
						tuple = Tuple.Create(tuple.Item1, value2);
						result = true;
					}
					if (tuple.Item1 > 10000 || tuple.Item2 > 10000)
					{
						retVal.AppendLine($"10k ID left: {tuple.Item1}, {tuple.Item2} for \"{deckType}\" deck of  \"{enemyClassState.ClassID}\" class in \"{levelName}\"");
					}
					deck[i] = tuple;
				}
			}
			return result;
		}
	}

	public void BulkLogAllIdsInLevels()
	{
		List<KeyValuePair<string, List<string>>> entriesPerLevel = new List<KeyValuePair<string, List<string>>>();
		IterateOverAllAvailableLevels(0.5f, LevelOperation, ProcessCompleteOperation, loadIntoLevelEditor: false);
		IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			List<string> entries = new List<string>();
			entries.AddRange(from s in levelData.ScenarioState.Players.Select((PlayerState s) => s.ClassID).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in levelData.ScenarioState.HeroSummons.Select((HeroSummonState s) => s.ClassID).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in levelData.ScenarioState.AllEnemyStates.Select((EnemyState s) => s.ClassID).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in (from s in levelData.ScenarioState.EnemyClassManager.EnemyClasses
					select s.ClassID into id
					where !entries.Contains(id)
					select id).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in (from s in levelData.ScenarioState.HeroSummonClassManager.HeroSummonClasses
					select s.ClassID into id
					where !entries.Contains(id)
					select id).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in levelData.ScenarioState.Props.Select((CObjectProp s) => s.PrefabName).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in levelData.ScenarioState.ActivatedProps.Select((CObjectProp s) => s.PrefabName).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in levelData.ScenarioState.TransparentProps.Select((CObjectObstacle s) => s.PrefabName).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in levelData.ScenarioState.DestroyedProps.Select((CObjectProp s) => s.PrefabName).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in (from s in levelData.ScenarioState.Spawners
					where s.Prop != null
					select s.Prop.PrefabName).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in (from id in levelData.ScenarioState.Spawners.SelectMany((CSpawner s) => s.SpawnerData.SpawnRoundEntryDictionary.Values).SelectMany((List<SpawnRoundEntry> list) => list).SelectMany((SpawnRoundEntry sre) => sre.SpawnClass)
					where id != "NONE" && id != "DON'T SPAWN"
					where !entries.Contains(id)
					select id).Distinct()
				orderby s
				select s);
			entries.AddRange(from s in (from s in levelData.ScenarioState.ScenarioModifiers
					select s.ScenarioAbilityID into id
					where id != "NONE"
					select id).Distinct()
				orderby s
				select s);
			entries.RemoveAll(string.IsNullOrWhiteSpace);
			entriesPerLevel.Add(KeyValuePair.Create(levelData.Name, entries));
			processString("Processed " + levelData.Name);
			yield break;
		}
		void ProcessCompleteOperation(string completionReport)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Per Level Entries:");
			foreach (KeyValuePair<string, List<string>> item in entriesPerLevel)
			{
				stringBuilder.AppendLine("- " + item.Key + " - " + string.Join(", ", item.Value));
			}
			List<string> values = (from s in entriesPerLevel.SelectMany((KeyValuePair<string, List<string>> pair) => pair.Value).Distinct()
				orderby s
				select s).ToList();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("All Entry Types:");
			stringBuilder.AppendLine(string.Join("\n", values));
			Log.Info(stringBuilder.ToString());
			File.WriteAllText("BulkLogAllIdsInLevels.txt", stringBuilder.ToString());
		}
	}

	public void BulkLogAllStorySpeakers()
	{
		List<KeyValuePair<string, List<CLevelMessagePage>>> pagesPerLevel = new List<KeyValuePair<string, List<CLevelMessagePage>>>();
		IterateOverAllAvailableLevels(0.5f, LevelOperation, ProcessCompleteOperation, loadIntoLevelEditor: false);
		IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			List<CLevelMessagePage> list = new List<CLevelMessagePage>();
			list.AddRange(levelData.LevelMessages.SelectMany((CLevelMessage m) => m.Pages));
			if (list.Count > 0)
			{
				pagesPerLevel.Add(KeyValuePair.Create(levelData.Name, list));
			}
			processString("Processed " + levelData.Name);
			yield break;
		}
		void ProcessCompleteOperation(string completionReport)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Per Level Story Speakers:");
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, List<CLevelMessagePage>> item in pagesPerLevel)
			{
				stringBuilder.AppendLine("\t" + item.Key + ":");
				foreach (CLevelMessagePage item2 in item.Value)
				{
					list.Clear();
					list.Add(KeyValuePair.Create("Speaker", item2.SpeakingCharacterGUID));
					list.Add(KeyValuePair.Create("Key", item2.PageTextKey));
					list.Add(KeyValuePair.Create("BG", item2.SpeakingCharacterBackgroundId));
					list.Add(KeyValuePair.Create("ImagePath", item2.ImagePath));
					list.Add(KeyValuePair.Create("Audio", item2.SpeakingCharacterAudioId));
					list.Add(KeyValuePair.Create("Expression", item2.SpeakingCharacterExpression.ToString()));
					stringBuilder.Append("\t\t");
					bool flag = true;
					foreach (KeyValuePair<string, string> item3 in list)
					{
						if (!string.IsNullOrWhiteSpace(item3.Value))
						{
							if (!flag)
							{
								stringBuilder.Append(" | ");
							}
							flag = false;
							stringBuilder.Append(item3.Key + ": " + item3.Value);
						}
					}
					stringBuilder.AppendLine();
				}
			}
			List<string> values = (from s in (from p in pagesPerLevel.SelectMany((KeyValuePair<string, List<CLevelMessagePage>> pair) => pair.Value)
					select p.SpeakingCharacterGUID into s
					where !string.IsNullOrWhiteSpace(s)
					select s).Distinct()
				orderby s
				select s).ToList();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("All Speaker IDs:");
			stringBuilder.AppendLine(string.Join("\n", values));
			Log.Info(stringBuilder.ToString());
			File.WriteAllText("BulkLogAllStorySpeakers.txt", stringBuilder.ToString());
		}
	}

	public void BulkRemoveBanditsFromSpawners()
	{
		UnityAction<string> onCompleteOperation = (string completionReport) =>
		{
		};
		IterateOverAllAvailableLevels(0.5f, LevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
		static IEnumerator LevelOperation(UnityAction<string> processString, CCustomLevelData levelData)
		{
			int changes = 0;
			foreach (SpawnRoundEntry item in levelData.ScenarioState.Spawners.SelectMany((CSpawner s) => s.SpawnerData.SpawnRoundEntryDictionary.Values).SelectMany((List<SpawnRoundEntry> list) => list))
			{
				for (int num = 0; num < item.SpawnClass.Count; num++)
				{
					if (item.SpawnClass[num].Contains("Bandit", StringComparison.OrdinalIgnoreCase) && (item.SpawnClass[num].Contains("Guard", StringComparison.OrdinalIgnoreCase) || item.SpawnClass[num].Contains("Archer", StringComparison.OrdinalIgnoreCase)))
					{
						item.SpawnClass[num] = "DON'T SPAWN";
						changes++;
					}
				}
			}
			if (changes > 0)
			{
				Task<(bool, string)> task = SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name);
				yield return new WaitForTask(task);
				processString($"Level {levelData.Name} - bandits killed: {changes}");
			}
			else
			{
				processString("");
			}
		}
	}
}

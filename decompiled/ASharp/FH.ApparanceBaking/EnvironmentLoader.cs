using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Apparance.Net;
using FH.LevelComponents.ProcGen;
using FH.Misc;
using Libs.GameConsole;
using Libs.Utils;
using Libs.Utils.Profiler;
using SETUtil;
using SETUtil.Common.Extend;
using SETUtil.Extend;
using SETUtil.Materials;
using SETUtil.SceneUI;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.ApparanceBaking;

public class EnvironmentLoader : MonoBehaviour, IMapping, ILogging
{
	[Serializable]
	public class Mapping
	{
		[Serializable]
		public class Tuple<K, V>
		{
			public K Key;

			public V Value;

			public Tuple(K k, V v)
			{
				Key = k;
				Value = v;
			}
		}

		[Serializable]
		public class TileData
		{
			public GameObject LogicObject;

			public GameObject RenderObject;

			public TileIndex ArrayIndex;

			public UnityEngine.Vector3 WorldPos;

			public List<GameObject> Terrain = new List<GameObject>();

			public TileData(GameObject logicObject, GameObject renderObject, TileIndex arrayIndex, UnityEngine.Vector3 worldPos)
			{
				LogicObject = logicObject;
				RenderObject = renderObject;
				ArrayIndex = arrayIndex;
				WorldPos = worldPos;
			}

			public void LinkTerrainVisuals(GameObject terrain)
			{
				Terrain.Add(terrain);
			}
		}

		public List<Tuple<string, GameObject>> Maps = new List<Tuple<string, GameObject>>();

		public List<Tuple<string, TileData>> Tiles = new List<Tuple<string, TileData>>();

		public List<Tuple<string, GameObject>> Props = new List<Tuple<string, GameObject>>();

		[Header("Missing tiles are ones that exist in the level data but not in the loaded maptile prefab.")]
		public List<string> MissingTiles = new List<string>();
	}

	public class ApparanceMapping : IMapping
	{
		public GameObject EnvironmentRoot { get; internal set; }

		public Mapping Mapping { get; private set; }

		public ApparanceMapping(GameObject environmentRoot, ScenarioState state, bool partial)
		{
			EnvironmentRoot = environmentRoot;
			Mapping = (partial ? ScenarioMaptileEditHelpers.GeneratePartialMappings(environmentRoot, state) : ScenarioMaptileEditHelpers.GenerateMappings(environmentRoot, state));
		}

		public IEnumerable<GameObject> GetMaps()
		{
			return Mapping.Maps.Select((Mapping.Tuple<string, GameObject> a) => a.Value);
		}

		public GameObject GetMap(string guid)
		{
			return Mapping.Maps.FirstOrDefault((Mapping.Tuple<string, GameObject> a) => a.Key == guid)?.Value;
		}

		public GameObject GetProp(string guid)
		{
			return Mapping.Props.FirstOrDefault((Mapping.Tuple<string, GameObject> a) => a.Key == guid)?.Value;
		}

		public GameObject GetTile(string guid)
		{
			return Mapping.Tiles.FirstOrDefault((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid)?.Value.RenderObject;
		}

		public GameObject GetTileHex(string guid)
		{
			return Mapping.Tiles.FirstOrDefault((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid)?.Value.LogicObject;
		}

		public void AddMap(GameObject map, string guid)
		{
			if (!Mapping.Maps.Any((Mapping.Tuple<string, GameObject> a) => a.Key == guid))
			{
				Mapping.Maps.Add(new Mapping.Tuple<string, GameObject>(guid, map));
			}
		}

		public void AddProp(GameObject prop, string guid)
		{
			if (!Mapping.Props.Any((Mapping.Tuple<string, GameObject> a) => a.Key == guid))
			{
				Mapping.Props.Add(new Mapping.Tuple<string, GameObject>(guid, prop));
			}
		}

		public void AddTile(Mapping.TileData tile, string guid)
		{
			if (!Mapping.Tiles.Any((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid))
			{
				Mapping.Tiles.Add(new Mapping.Tuple<string, Mapping.TileData>(guid, tile));
			}
		}

		public void RemoveTile(string guid)
		{
			Mapping.Tiles.RemoveAll((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid);
		}

		public IEnumerable<GameObject> GetProps()
		{
			return Mapping.Props.Select((Mapping.Tuple<string, GameObject> a) => a.Value);
		}

		public IEnumerable<GameObject> GetTiles()
		{
			return Mapping.Tiles.Select((Mapping.Tuple<string, Mapping.TileData> a) => a.Value.RenderObject);
		}
	}

	public class BakedMapping : IMapping
	{
		public GameObject EnvironmentRoot { get; set; }

		public MappingRecord MappingRecord { get; set; }

		public IEnumerable<GameObject> GetMaps()
		{
			return MappingRecord.Mapping.Maps.Select((Mapping.Tuple<string, GameObject> a) => a.Value);
		}

		public GameObject GetMap(string guid)
		{
			return MappingRecord.Mapping.Maps.FirstOrDefault((Mapping.Tuple<string, GameObject> a) => a.Key == guid)?.Value;
		}

		public GameObject GetProp(string guid)
		{
			return MappingRecord.Mapping.Props.FirstOrDefault((Mapping.Tuple<string, GameObject> a) => a.Key == guid)?.Value;
		}

		public GameObject GetTile(string guid)
		{
			return MappingRecord.Mapping.Tiles.FirstOrDefault((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid)?.Value.RenderObject;
		}

		public GameObject GetTileHex(string guid)
		{
			return MappingRecord.Mapping.Tiles.FirstOrDefault((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid)?.Value.LogicObject;
		}

		public void AddMap(GameObject map, string guid)
		{
			if (!MappingRecord.Mapping.Maps.Any((Mapping.Tuple<string, GameObject> a) => a.Key == guid))
			{
				MappingRecord.Mapping.Maps.Add(new Mapping.Tuple<string, GameObject>(guid, map));
			}
		}

		public void AddProp(GameObject prop, string guid)
		{
			if (!MappingRecord.Mapping.Props.Any((Mapping.Tuple<string, GameObject> a) => a.Key == guid))
			{
				MappingRecord.Mapping.Props.Add(new Mapping.Tuple<string, GameObject>(guid, prop));
			}
		}

		public void AddTile(Mapping.TileData tile, string guid)
		{
			if (!MappingRecord.Mapping.Tiles.Any((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid))
			{
				MappingRecord.Mapping.Tiles.Add(new Mapping.Tuple<string, Mapping.TileData>(guid, tile));
			}
		}

		public void RemoveTile(string guid)
		{
			MappingRecord.Mapping.Tiles.RemoveAll((Mapping.Tuple<string, Mapping.TileData> a) => a.Key == guid);
		}

		public IEnumerable<GameObject> GetProps()
		{
			return MappingRecord.Mapping.Props.Select((Mapping.Tuple<string, GameObject> a) => a.Value);
		}

		public IEnumerable<GameObject> GetTiles()
		{
			return MappingRecord.Mapping.Tiles.Select((Mapping.Tuple<string, Mapping.TileData> a) => a.Value.RenderObject);
		}
	}

	public class GymMapping : IMapping
	{
		public GameObject EnvironmentRoot { get; private set; }

		public GymMapping(GameObject environmentRoot)
		{
			EnvironmentRoot = environmentRoot;
		}

		public IEnumerable<GameObject> GetMaps()
		{
			return from a in EnvironmentRoot.GetComponentsInChildren<ProceduralMapTile>(includeInactive: true)
				select a.gameObject;
		}

		public GameObject GetMap(string guid)
		{
			return EnvironmentRoot.GetComponentsInChildren<ProceduralMapTile>(includeInactive: true).FirstOrDefault((ProceduralMapTile a) => a.name.Contains(guid))?.gameObject;
		}

		public GameObject GetProp(string guid)
		{
			return EnvironmentRoot.GetComponentsInChildren<ProceduralProp>(includeInactive: true).FirstOrDefault((ProceduralProp a) => a.name.Contains(guid))?.gameObject;
		}

		public GameObject GetTile(string guid)
		{
			return EnvironmentRoot.GetComponentsInChildren<ProceduralTile>(includeInactive: true).FirstOrDefault((ProceduralTile a) => a.name.Contains(guid))?.gameObject;
		}

		public GameObject GetTileHex(string guid)
		{
			return GetTile(guid);
		}

		public IEnumerable<GameObject> GetProps()
		{
			return from a in EnvironmentRoot.GetComponentsInChildren<ProceduralProp>(includeInactive: true)
				select a.gameObject;
		}

		public IEnumerable<GameObject> GetTiles()
		{
			return from a in EnvironmentRoot.GetComponentsInChildren<ProceduralTile>(includeInactive: true)
				select a.gameObject;
		}

		public void AddMap(GameObject map, string guid)
		{
			throw new NotImplementedException();
		}

		public void AddProp(GameObject prop, string guid)
		{
			throw new NotImplementedException();
		}

		public void AddTile(Mapping.TileData tile, string guid)
		{
			throw new NotImplementedException();
		}

		public void RemoveTile(string guid)
		{
		}
	}

	public class ApparanceGenerationTracking
	{
		[NonSerialized]
		private HashSet<string> contentExpected = new HashSet<string>();

		[NonSerialized]
		private HashSet<string> contentComplete = new HashSet<string>();

		[NonSerialized]
		private HashSet<string> debugWaitingOn = new HashSet<string>();

		private Action _onComplete;

		private GameObject _contentRoot;

		private bool _complete;

		public bool GenerationCompletedOnce => _complete;

		public void Start(GameObject contentRoot, Action onComplete2)
		{
			_complete = false;
			_contentRoot = contentRoot;
			contentExpected.Clear();
			contentExpected.AddRange(from a in contentRoot.GetComponentsInChildren<Transform>(includeInactive: true)
				where a.GetComponent<IContentUpdateMonitor>() != null && a.GetComponentsInChildren<ApparanceEntity>(includeInactive: true).Length != 0
				select a.name);
			contentComplete.Clear();
			_onComplete = onComplete2;
			CheckContentComplete();
		}

		public void OnContentGenerated(UnityEngine.Object content, string name)
		{
			contentComplete.Add(name);
			ApparanceEntity component = ((Component)content).GetComponent<ApparanceEntity>();
			if ((bool)component)
			{
				component.Frozen = true;
			}
			Transform transform;
			if ((bool)component.GetComponent<ProceduralMapTile>() && (bool)(transform = component.transform.Find("Generated Content")))
			{
				_ = (bool)transform.Find("Full");
			}
			if (!ProceduralMapTile.IsInLevelEditorMode && Application.isPlaying)
			{
				string.Join(",", from a in UnityEngine.Object.FindObjectsOfType<ProceduralMapTile>()
					select a.name);
				if (content is ProceduralProp proceduralProp)
				{
					UnityGameEditorObject component2 = proceduralProp.gameObject.GetComponent<UnityGameEditorObject>();
					if (component2 != null)
					{
						component2.AddOutlinesToLinkedProps();
					}
				}
			}
			CheckContentComplete();
			if (_complete || contentComplete.Count == 0)
			{
				UnityEngine.Debug.Log($"Apparance content generated: {contentComplete.Count}/{contentExpected.Count}");
			}
		}

		private IEnumerator Repopulate(Transform child)
		{
			ApparanceEntity[] ape = child.GetComponentsInChildren<ApparanceEntity>();
			ApparanceEntity[] array = ape;
			foreach (ApparanceEntity obj in array)
			{
				obj.IsPopulated = false;
				obj.gameObject.SetActive(value: false);
			}
			yield return null;
			array = ape;
			foreach (ApparanceEntity obj2 in array)
			{
				obj2.IsPopulated = true;
				obj2.gameObject.SetActive(value: true);
			}
			yield return null;
			IProceduralContent[] componentsInChildren = child.GetComponentsInChildren<IProceduralContent>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RebuildContent();
			}
		}

		public void CheckContentComplete()
		{
			if (_complete)
			{
				return;
			}
			if (!_contentRoot)
			{
				_complete = true;
				return;
			}
			contentExpected.RemoveWhere((string a) => !GameObject.Find(a));
			debugWaitingOn.Clear();
			if (!contentExpected.IsSubsetOf(contentComplete))
			{
				return;
			}
			bool flag = true;
			Transform[] componentsInChildren = _contentRoot.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == "Generated Content" && (bool)transform.parent.GetComponent<ApparanceEntity>())
				{
					if (transform.childCount == 0 && !ContentBelongsToSpecialCase_Skip(transform))
					{
						flag = false;
						debugWaitingOn.Add(transform.parent.name);
						Instance.StartSmartCoroutine(Repopulate(transform));
						break;
					}
					if (transform.childCount > 0 && ContentBelongsToSpecialCase_Wait(transform))
					{
						flag = false;
						debugWaitingOn.Add(transform.parent.name);
					}
				}
			}
			ProceduralMapTile[] componentsInChildren2 = _contentRoot.GetComponentsInChildren<ProceduralMapTile>();
			foreach (ProceduralMapTile proceduralMapTile in componentsInChildren2)
			{
				if (!proceduralMapTile.transform.Find("Generated Content") || !proceduralMapTile.transform.Find("Generated Content").Find("Full"))
				{
					flag = false;
				}
			}
			if (flag)
			{
				_complete = true;
				_onComplete?.Invoke();
			}
		}

		private bool ContentBelongsToSpecialCase_Wait(Transform child)
		{
			if (child.parent.name == "N:f157a492-aa5e-806a-8aa3-845f46b000c0" && (child.GetChild(0) == null || !child.Find("Full") || child.Find("Full").childCount < 2))
			{
				return true;
			}
			return false;
		}

		public string DebugPrint()
		{
			string text = $"Apparance content generated: {contentComplete.Count}/{contentExpected.Count}.\nLeft:\n" + string.Join(",\n", contentExpected.Where((string a) => !contentComplete.Contains(a)).Concat(debugWaitingOn));
			UnityEngine.Debug.Log(text);
			return text;
		}

		private static bool ContentBelongsToSpecialCase_Skip(Transform child)
		{
			if (child.parent.name == "Wall 1" && (bool)child.parent.parent && (bool)child.parent.parent.parent && child.parent.parent.parent.name == "FH_11_12:3f63b54a-6198-4aee-87ac-65470a2ed2b3")
			{
				return true;
			}
			if (child.parent.name.StartsWith("EmptyDoor"))
			{
				return true;
			}
			if (child.parent.name == "Wall 3 (1)" && (bool)child.parent.parent && (bool)child.parent.parent.parent && child.parent.parent.parent.name == "FH_04_SC78:28c0357c-35fe-4b02-92b0-3b7977976053")
			{
				return true;
			}
			if ((child.parent.name == "Wall 2 (2)" || child.parent.name == "Wall 2 (6)") && (bool)child.parent.parent && (bool)child.parent.parent.parent && child.parent.parent.parent.name == "N_FS111:44804585-dd90-4a3e-9246-3fe23974088f")
			{
				return true;
			}
			if ((bool)child.parent.parent && (bool)child.parent.parent.parent && (child.parent.parent.parent.name == "L_FS_114_1:157d0d4a-9570-4a06-b441-f786871a9d2a" || child.parent.parent.parent.name == "L_FS_114_2:191fdf41-65bc-4a9c-a2e9-3440fcce4a49"))
			{
				return true;
			}
			return false;
		}
	}

	private class GYMApparanceGenerationTracking
	{
		private DateTime _lastSpawn = DateTime.Now;

		private object _currentCoroutine;

		private GUILabel _label = new GUILabel("Rebuilding Procedural Lighting...", new Rect(10f, 400f, 200f, 30f), Color.red);

		public void Spawned()
		{
		}

		private IEnumerator WaitForSpawn()
		{
			while (DateTime.Now - _lastSpawn < TimeSpan.FromSeconds(4.0))
			{
				EditorUtil.DrawSceneElement(_label);
				yield return null;
			}
			try
			{
				ScenarioMaptileEditHelpers.MaptileRefreshPostProcessing();
			}
			finally
			{
				_currentCoroutine = null;
			}
		}
	}

	private enum Preference
	{
		None,
		PreferApparance,
		ApparanceExclusively,
		PreferBaked,
		BakedOnly,
		SimpleFast
	}

	public class GeneratedContentCache
	{
		public GameObject go;

		public ScenarioPossibleRoom.EBiomeState biomeState;
	}

	private const bool ALLOW_MERGE_BAKED_CONTENT = false;

	public static bool ReducedPostProcessing = false;

	private GameObject apparanceGameObject;

	private string serializedApparanceState;

	[SerializeField]
	private ApparanceEngine apparanceEngine;

	[SerializeField]
	private GameObject apparanceMapsRoot;

	[SerializeField]
	private GameObject bakedMapsRoot;

	[SerializeField]
	private GameObject simpleTileFallback;

	private AsyncOperation _loadOp;

	private List<Scene> _loadedScenes = new List<Scene>();

	private IMapping _environmentMapping;

	private static Dictionary<string, GeneratedContentCache> DestroyedGeneratedContent = new Dictionary<string, GeneratedContentCache>();

	private bool _isLoaded;

	private static Preference _mode = Preference.None;

	private static EnvironmentLoader _instance = null;

	private readonly List<SmartCoroutine> _runningCoroutines = new List<SmartCoroutine>();

	public GameObject ApparanceMapsRoot => apparanceMapsRoot;

	public bool IsLoadingComplete => _isLoaded;

	public ApparanceGenerationTracking ApparanceGenerationTracker { get; private set; } = new ApparanceGenerationTracking();

	private static GYMApparanceGenerationTracking _GYMApparanceGenerationTracker { get; set; } = new GYMApparanceGenerationTracking();

	public static EnvironmentLoader Instance
	{
		get
		{
			if (!_instance)
			{
				return _instance = UnityEngine.Object.FindObjectOfType<EnvironmentLoader>();
			}
			return _instance;
		}
	}

	public static bool IsInstanceValid => _instance != null;

	public GameObject EnvironmentRoot => _environmentMapping?.EnvironmentRoot ?? ApparanceMapsRoot;

	public SmartCoroutine StartSmartCoroutine(IEnumerator crt, bool track = true)
	{
		RemoveCompletedSmartCoroutines();
		SmartCoroutine smartCoroutine = SmartCoroutine.Start(this, crt);
		if (track)
		{
			_runningCoroutines.Add(smartCoroutine);
		}
		return smartCoroutine;
	}

	private void RemoveCompletedSmartCoroutines()
	{
		_runningCoroutines.RemoveAll((SmartCoroutine co) => co.IsCompleted);
	}

	public IEnumerator Wait()
	{
		using (new MiniProfilerScope("EnvironmentLoader.Wait"))
		{
			UnityEngine.Debug.Log("Waiting for EnvironmentLoader to complete");
			int timeout = 40;
			float start = Time.timeSinceLevelLoad;
			while (!IsLoadingComplete)
			{
				if (Time.timeSinceLevelLoad - start > (float)timeout)
				{
					string text = ApparanceGenerationTracker?.DebugPrint() ?? "None";
					UnityEngine.Debug.LogError("EnvironmentLoader is taking too long to complete. This is likely a bug. Please report this to the tech art team. Left: " + ((text.Length > 200) ? (text.Substring(0, 200) + "\n...") : text));
				}
				yield return null;
			}
			UnityEngine.Debug.Log("EnvironmentLoader completed");
		}
	}

	public IEnumerator Load(ScenarioState state, LevelEditorController.PlaceNewMapDelegate newMapPlacement, Action<IMapping, ByRef<bool>> mapInit, Action<IMapping> onComplete = null, KronosProfilingScaffolding profiling = null)
	{
		bool isInLevelEditor;
		bool isNewLevelEditor;
		ByRef<bool> chainedProcedureCompletion;
		using (new MiniProfilerScope("EnvironmentLoader.Load"))
		{
			if (PlayerPrefs.HasKey("EnvLoader_DebugMode"))
			{
				_mode = (Preference)PlayerPrefs.GetInt("EnvLoader_DebugMode");
			}
			if (DebugMenu.DbgSkipProceduralScenario_kvar)
			{
				_mode = Preference.SimpleFast;
			}
			apparanceEngine.enabled = false;
			_isLoaded = false;
			string bakingId = state.BakingID;
			bool flag = HasBaking(bakingId);
			if (_mode == Preference.BakedOnly && !flag)
			{
				throw new Exception("No baked environment available.");
			}
			bool flag2 = true;
			bool flag3 = ScenarioDataLocksDef.Load().IsLocked(state.BakingID);
			bool flag4 = true;
			isInLevelEditor = ProceduralMapTile.IsInLevelEditorMode;
			isNewLevelEditor = state.Maps.Count == 0;
			chainedProcedureCompletion = new ByRef<bool>(value: false);
			if (!isInLevelEditor)
			{
				bool flag5 = flag3 && _mode != Preference.ApparanceExclusively && _mode != Preference.PreferApparance;
				bool flag6 = (flag2 & flag4) && _mode == Preference.None;
				bool flag7 = _mode == Preference.PreferBaked || _mode == Preference.BakedOnly;
				if (flag && (flag5 | flag6 | flag7))
				{
					DisableApparance();
					profiling?.Stopwatch?.Time("Start Baked Load");
					using (new MiniProfilerScope("EnvironmentLoader.Load.LoadSceneAsync"))
					{
						_loadOp = SceneManager.LoadSceneAsync(bakingId, LoadSceneMode.Additive);
						yield return _loadOp;
					}
					profiling?.Stopwatch?.Time("Baked scene loaded");
					Scene sceneByName = SceneManager.GetSceneByName(bakingId);
					_loadedScenes.Add(sceneByName);
					GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
					GameObject gameObject = rootGameObjects.First((GameObject a) => a.name == "BakedEnvironment");
					_environmentMapping = new BakedMapping
					{
						EnvironmentRoot = bakedMapsRoot,
						MappingRecord = gameObject.GetComponent<MappingRecord>()
					};
					foreach (Transform item in bakedMapsRoot.transform)
					{
						UnityEngine.Object.Destroy(item.gameObject);
					}
					GameObject[] array = rootGameObjects;
					for (int num = 0; num < array.Length; num++)
					{
						array[num].transform.SetParent(bakedMapsRoot.transform);
					}
					SceneManager.UnloadSceneAsync(sceneByName);
					_loadedScenes.Remove(sceneByName);
					UnityEngine.Object.Destroy(apparanceMapsRoot);
					mapInit(_environmentMapping, chainedProcedureCompletion);
					foreach (GameObject prop in _environmentMapping.GetProps())
					{
						Watchman.Instance?.Complete(prop.name);
					}
					StartSmartCoroutine(WhenChainedProcedureCompletes());
				}
				else if (_mode == Preference.SimpleFast)
				{
					UnityEngine.Object.Destroy(bakedMapsRoot);
					DisableApparance();
					yield return newMapPlacement(apparanceMapsRoot);
					_environmentMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: true);
					mapInit(_environmentMapping, chainedProcedureCompletion);
					yield return WaitUntilChainedProcedureCompletes(chainedProcedureCompletion);
					Mapping mapping = new Mapping();
					ProceduralMapTile[] componentsInChildren = apparanceMapsRoot.GetComponentsInChildren<ProceduralMapTile>(includeInactive: true);
					foreach (CMap map in state.Maps)
					{
						GameObject gameObject2 = componentsInChildren.Single((ProceduralMapTile a) => a.name.Contains(map.MapGuid)).gameObject;
						mapping.Maps.Add(new Mapping.Tuple<string, GameObject>(map.MapGuid, gameObject2));
						GameObject gameObject3 = gameObject2.FindInImmediateChildren("Generated Content");
						if (gameObject3 == null)
						{
							throw new Exception("No Generated Content found in " + map.RoomName + " at " + state.ScenarioFileName);
						}
						GameObject gameObject4 = gameObject3.FindInImmediateChildren("Full");
						if (gameObject4 == null)
						{
							gameObject4 = new GameObject("Full");
							gameObject4.transform.SetParent(gameObject3.transform);
						}
						foreach (CMapTile mapTile in map.MapTiles)
						{
							GameObject gameObject5 = gameObject2.FindInChildren(mapTile.TileGuid, includeInactive: true);
							if (gameObject5 != null)
							{
								UnityEngine.Vector3 position = new UnityEngine.Vector3(mapTile.Position.X, mapTile.Position.Y, mapTile.Position.Z);
								GameObject gameObject6 = new GameObject("_FShape_" + mapTile.TileGuid);
								gameObject6.transform.SetParent(gameObject4.transform);
								gameObject6.transform.position = position;
								gameObject6.transform.rotation = gameObject5.transform.rotation;
								gameObject6.AddComponent<MeshFilter>().sharedMesh = gameObject5.GetComponent<MeshFilter>().sharedMesh;
								gameObject6.AddComponent<MeshRenderer>().sharedMaterial = gameObject5.GetComponent<MeshRenderer>().sharedMaterial;
								gameObject5.GetComponent<Renderer>().enabled = false;
							}
						}
					}
					_environmentMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: false);
					foreach (GameObject prop2 in _environmentMapping.GetProps())
					{
						UnityGameEditorObject component = prop2.GetComponent<UnityGameEditorObject>();
						if (!prop2.GetComponent<ProceduralDoorway>() || (bool)component)
						{
							ShouldShowBakedVariant(prop2.gameObject, component.PropObject.PropTile, component.PropObject);
						}
					}
					yield return OnLoadComplete(_environmentMapping, profiling);
					DoWhenLoadingCompletes(_environmentMapping, state);
				}
				else
				{
					yield return TriggerApparanceMode_Internal(state, newMapPlacement, mapInit, (IMapping m) =>
					{
						DoWhenLoadingCompletes(m, state);
					}, profiling, bakingId, flag, isInLevelEditor, isNewLevelEditor, chainedProcedureCompletion);
				}
			}
			else
			{
				DisableApparance();
				yield return null;
				if (0 == 0)
				{
					UnityEngine.Object.Destroy(bakedMapsRoot);
				}
				yield return newMapPlacement(apparanceMapsRoot);
				_environmentMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: true);
				mapInit(_environmentMapping, chainedProcedureCompletion);
				StartSmartCoroutine(WhenChainedProcedureCompletes2()).WaitForCompletion();
			}
			yield return Wait();
		}
		void DoWhenLoadingCompletes(IMapping m, ScenarioState s)
		{
			RemoveCompletedSmartCoroutines();
			foreach (CObjectProp prop3 in s.Props)
			{
				if (prop3 != null)
				{
					GameObject gameObject7 = GetProp(prop3.PropGuid);
					if (!gameObject7)
					{
						gameObject7 = Choreographer.FindPropGameObject(prop3);
					}
					if (!(gameObject7 == null))
					{
						if (prop3.IsBaked)
						{
							OnRuntimePropSpawn(gameObject7, prop3, ClientScenarioManager.s_ClientScenarioManager.GetTile(prop3.ArrayIndex.ToPoint()));
						}
						else if (!ScenarioMaptileEditHelpers.IsTerrainVisualEffectProp(prop3.PropType) && (bool)gameObject7.GetComponent<ProceduralTile>())
						{
							CutTerrainHole(prop3.PropTile?.m_Hex?.TileGuid ?? prop3.PropTile?.m_Hex2?.TileGuid);
							UnityGameEditorObject component2 = gameObject7.GetComponent<UnityGameEditorObject>();
							if (component2 != null)
							{
								component2.AddOutlinesToLinkedProps();
							}
						}
					}
				}
			}
			onComplete?.Invoke(m);
		}
		IEnumerator Repopulate()
		{
			_ = apparanceEngine;
			ApparanceEntity[] ape = _environmentMapping.EnvironmentRoot.GetComponentsInChildren<ApparanceEntity>(includeInactive: false);
			ApparanceEntity[] array2 = ape;
			foreach (ApparanceEntity obj in array2)
			{
				obj.IsPopulated = false;
				obj.gameObject.SetActive(value: false);
			}
			yield return null;
			array2 = ape;
			foreach (ApparanceEntity obj2 in array2)
			{
				obj2.IsPopulated = true;
				obj2.gameObject.SetActive(value: true);
			}
			yield return null;
			IProceduralContent[] componentsInChildren2 = _environmentMapping.EnvironmentRoot.GetComponentsInChildren<IProceduralContent>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].RebuildContent();
			}
		}
		IEnumerator WhenChainedProcedureCompletes()
		{
			using (new MiniProfilerScope("EnvironmentLoader.Load.WhenChainedProcedureCompletes"))
			{
				yield return WaitUntilChainedProcedureCompletes(chainedProcedureCompletion);
				yield return InitializeProceduralContent(_environmentMapping, state).WaitForCompletion();
				yield return OnLoadComplete(_environmentMapping, profiling);
				DoWhenLoadingCompletes(_environmentMapping, state);
			}
		}
		IEnumerator WhenChainedProcedureCompletes2()
		{
			yield return WaitUntilChainedProcedureCompletes(chainedProcedureCompletion);
			if (isInLevelEditor & isNewLevelEditor)
			{
				_environmentMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: true);
				yield return OnLoadComplete(_environmentMapping, profiling);
				DoWhenLoadingCompletes(_environmentMapping, state);
			}
			else
			{
				yield return InitializeProceduralContent(_environmentMapping, state).WaitForCompletion();
				EnableApparance();
				RemoveCompletedSmartCoroutines();
				StartSmartCoroutine(Repopulate());
				_environmentMapping.EnvironmentRoot.GetComponent<ProceduralScenario>().Update();
				yield return OnLoadComplete(_environmentMapping, profiling);
				DoWhenLoadingCompletes(_environmentMapping, state);
			}
		}
	}

	public static bool HasBaking(string scnearioName)
	{
		string scenePath = "Assets/Scenes/Environments/" + scnearioName + ".unity";
		if (!string.IsNullOrEmpty(scnearioName))
		{
			return SceneUtility.GetBuildIndexByScenePath(scenePath) >= 0;
		}
		return false;
	}

	private IEnumerator TriggerApparanceMode_Internal(ScenarioState state, LevelEditorController.PlaceNewMapDelegate newMapPlacement, Action<IMapping, ByRef<bool>> mapInit, Action<IMapping> onComplete, KronosProfilingScaffolding profiling, string bakedScene, bool hasBaking, bool isInLevelEditor, bool isNewLevelEditor, ByRef<bool> chainedProcedureCompletion)
	{
		DisableApparance();
		yield return null;
		bool mergeBakedContent = false;
		if (!mergeBakedContent)
		{
			UnityEngine.Object.Destroy(bakedMapsRoot);
		}
		yield return newMapPlacement(apparanceMapsRoot);
		ApparanceMapping preliminaryMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: true);
		mapInit(preliminaryMapping, chainedProcedureCompletion);
		yield return WaitUntilChainedProcedureCompletes(chainedProcedureCompletion);
		if (isInLevelEditor & isNewLevelEditor)
		{
			_environmentMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: true);
			DisableApparance();
			yield return OnLoadComplete(_environmentMapping, profiling);
			onComplete(_environmentMapping);
		}
		if (!isInLevelEditor)
		{
			Choreographer.EnsureBackgroundSpawned(preliminaryMapping.EnvironmentRoot.transform);
		}
		yield return InitializeProceduralContent(preliminaryMapping, state).WaitForCompletion();
		profiling?.Stopwatch?.Time("Start Apparance Load");
		ApparanceGenerationTracker.Start(apparanceMapsRoot, () =>
		{
			StartSmartCoroutine(OnApparanceGenerationComplete());
		});
		EnableApparance();
		RemoveCompletedSmartCoroutines();
		StartSmartCoroutine(Repopulate());
		ProceduralBackground componentInChildren = preliminaryMapping.EnvironmentRoot.GetComponentInChildren<ProceduralBackground>(includeInactive: false);
		if ((bool)componentInChildren)
		{
			((IProceduralContent)componentInChildren).RebuildContent();
		}
		preliminaryMapping.EnvironmentRoot.GetComponent<ProceduralScenario>().Update();
		IEnumerator OnApparanceGenerationComplete()
		{
			profiling?.Stopwatch?.Time("Apparance content generated");
			_environmentMapping = new ApparanceMapping(apparanceMapsRoot, state, partial: false);
			FreezeApparance(apparanceMapsRoot);
			if (mergeBakedContent)
			{
				_loadOp = SceneManager.LoadSceneAsync(bakedScene, LoadSceneMode.Additive);
				yield return _loadOp;
				Scene sceneByName = SceneManager.GetSceneByName(bakedScene);
				_loadedScenes.Add(sceneByName);
				GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
				GameObject gameObject = rootGameObjects.First((GameObject a) => a.name == "BakedEnvironment");
				gameObject.GetComponents<MonoBehaviour>().ForEach((MonoBehaviour a) =>
				{
					UnityEngine.Object.Destroy(a);
				});
				foreach (Transform item in bakedMapsRoot.transform)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
				GameObject[] array = rootGameObjects;
				foreach (GameObject gameObject2 in array)
				{
					if (gameObject2 != gameObject)
					{
						gameObject2.transform.SetParent(bakedMapsRoot.transform);
					}
				}
				SceneManager.UnloadSceneAsync(sceneByName);
				_loadedScenes.Remove(sceneByName);
			}
			yield return OnLoadComplete(_environmentMapping, profiling);
			onComplete(_environmentMapping);
		}
		IEnumerator Repopulate()
		{
			_ = apparanceEngine;
			ApparanceEntity[] ape = preliminaryMapping.EnvironmentRoot.GetComponentsInChildren<ApparanceEntity>(includeInactive: false);
			ApparanceEntity[] array = ape;
			foreach (ApparanceEntity obj in array)
			{
				obj.IsPopulated = false;
				obj.gameObject.SetActive(value: false);
			}
			yield return null;
			array = ape;
			foreach (ApparanceEntity obj2 in array)
			{
				obj2.IsPopulated = true;
				obj2.gameObject.SetActive(value: true);
			}
			yield return null;
			IProceduralContent[] componentsInChildren = preliminaryMapping.EnvironmentRoot.GetComponentsInChildren<IProceduralContent>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RebuildContent();
			}
		}
	}

	private IEnumerator WaitUntilChainedProcedureCompletes(ByRef<bool> chainedProcedureCompletion)
	{
		using (new MiniProfilerScope("EnvironmentLoader.WaitUntilChainedProcedureCompletes"))
		{
			int timeout = 30;
			float start = Time.timeSinceLevelLoad;
			while (!chainedProcedureCompletion.Value)
			{
				if (Time.timeSinceLevelLoad - start > (float)timeout)
				{
					UnityEngine.Debug.LogError($"[ERROR] Chained procedure did not complete in time ({timeout}s).");
					break;
				}
				yield return null;
			}
		}
	}

	private void FixCharacterLayers()
	{
		foreach (CharacterManager activeCharacterManager in CharacterManager.GetActiveCharacterManagers())
		{
			activeCharacterManager.UpdateLayers();
		}
	}

	private IEnumerator OnLoadComplete(IMapping mapping, KronosProfilingScaffolding profiling = null)
	{
		using (new MiniProfilerScope("EnvironmentLoader.OnLoadComplete"))
		{
			if (mapping == null)
			{
				throw new ArgumentNullException("mapping");
			}
			if (mapping.EnvironmentRoot == null)
			{
				throw new NullReferenceException("EnvironmentRoot");
			}
			yield return mapping.EnvironmentRoot.GetComponentInChildren<ProceduralScenario>().OnMaptileGenerationCompleteCrt(mapping);
			_isLoaded = true;
			FixCharacterLayers();
			ApparanceEngine.SetCustomLogging(null);
			profiling?.Stopwatch?.Time("Environment loaded and initialized");
		}
	}

	public void DisableApparance()
	{
		if ((bool)apparanceEngine)
		{
			serializedApparanceState = JsonUtility.ToJson(apparanceEngine);
			apparanceEngine.enabled = false;
			apparanceGameObject = apparanceEngine.gameObject;
			SceneUtil.SmartDestroy(apparanceEngine);
			UnityEngine.Debug.Log("Replacing Apparance Logging with Default handler.");
		}
	}

	public void EnableApparance(int throttle = int.MaxValue)
	{
		ApparanceEngine orAdd = apparanceGameObject.GetOrAdd<ApparanceEngine>();
		JsonUtility.FromJsonOverwrite(serializedApparanceState, orAdd);
		apparanceEngine = orAdd;
		apparanceEngine.enabled = true;
		apparanceEngine.PrefabInstancing = true;
		apparanceEngine.ShowGenerated = true;
		apparanceEngine.PlayerLiveEditing = true;
		apparanceEngine.EditorLiveEditing = true;
		int processorCount = SystemInfo.processorCount;
		apparanceEngine.PlayerSynthesisThreads = (apparanceEngine.EditorSynthesisThreads = Mathf.Min(processorCount, throttle));
		UnityEngine.Debug.Log("Replacing Apparance Logging with Custom handler.");
		ApparanceEngine.SetCustomLogging(this, replace: true);
	}

	public SmartCoroutine RegenerateProcGen(Action onComplete = null, bool freeze = true)
	{
		return RegenerateProcGen(EnvironmentRoot, onComplete, freeze);
	}

	public SmartCoroutine RegenerateProcGen(GameObject root, Action onComplete = null, bool freeze = true)
	{
		RemoveCompletedSmartCoroutines();
		return SmartCoroutine.Start(this, RegenerateProcGenCrt());
		IEnumerator OnApparanceGenerationComplete()
		{
			ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
			if (currentScenarioState != null)
			{
				_environmentMapping = new ApparanceMapping(apparanceMapsRoot, currentScenarioState, partial: false);
			}
			if (freeze)
			{
				FreezeApparance(apparanceMapsRoot);
			}
			onComplete?.Invoke();
			yield return OnLoadComplete(_environmentMapping);
		}
		IEnumerator RegenerateProcGenCrt()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (root == null)
			{
				yield return null;
			}
			stopwatch.Restart();
			if (!ProceduralMapTile.IsInLevelEditorMode)
			{
				while (_runningCoroutines.Count > 0)
				{
					yield return null;
				}
			}
			ApparanceGenerationTracker.Start(root, () =>
			{
				StartSmartCoroutine(OnApparanceGenerationComplete());
			});
			RegenerateApparance(root);
		}
	}

	public bool IsApparanceRunning()
	{
		if (!apparanceEngine)
		{
			return false;
		}
		return apparanceEngine.enabled;
	}

	public static void FreezeApparance(GameObject newRoot)
	{
		ApparanceEntity[] componentsInChildren = newRoot.GetComponentsInChildren<ApparanceEntity>();
		foreach (ApparanceEntity obj in componentsInChildren)
		{
			obj.Frozen = true;
			obj.enabled = false;
		}
		Instance.DisableApparance();
	}

	private static void RegenerateApparance(GameObject newRoot)
	{
		Instance.StartSmartCoroutine(Crt());
		IEnumerator Crt()
		{
			Instance.EnableApparance();
			yield return null;
			ApparanceEntity[] componentsInChildren = newRoot.GetComponentsInChildren<ApparanceEntity>(includeInactive: true);
			ApparanceEntity[] array = componentsInChildren;
			foreach (ApparanceEntity apparanceEntity in array)
			{
				apparanceEntity.Frozen = false;
				apparanceEntity.enabled = true;
				yield return null;
				apparanceEntity.GetComponent<IProceduralContent>()?.RebuildContent();
			}
			yield return null;
			newRoot.SetActive(value: false);
			yield return null;
			newRoot.SetActive(value: true);
		}
	}

	public void OnRuntimePropSpawn(GameObject goProp, CObjectProp cProp, CClientTile tile)
	{
		if (!_isLoaded || !Application.isPlaying)
		{
			return;
		}
		TryRestoreGeneratedContent(goProp, cProp);
		AddProp(goProp, cProp.PropGuid);
		ShouldShowBakedVariant(goProp, tile.m_Tile, cProp);
		string text = tile?.m_Tile?.m_HexMap?.MapGuid;
		if (!string.IsNullOrEmpty(text))
		{
			GameObject map = GetMap(text);
			if ((bool)map)
			{
				ProceduralMapTile component = map.GetComponent<ProceduralMapTile>();
				if ((bool)component && (bool)component.Ambience)
				{
					Renderer[] componentsInChildren = goProp.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].SetLayer(component.Ambience.currentRenderingLayer);
					}
				}
			}
		}
		AddProp(goProp, cProp.PropGuid);
		UnityGameEditorObject component2 = goProp.GetComponent<UnityGameEditorObject>();
		if (component2 != null)
		{
			component2.AddOutlinesToLinkedProps();
		}
	}

	private void ShouldShowBakedVariant(GameObject goProp, CTile tile, CObjectProp prop)
	{
		if (goProp == null)
		{
			return;
		}
		GameObject gameObject;
		bool flag = (gameObject = goProp.FindInChildren("Generated Content")) == null || gameObject.transform.childCount == 0 || prop.IsBaked;
		if (ProceduralMapTile.IsInLevelEditorMode || GameState.IsInLevelEditor)
		{
			flag = false;
		}
		if (flag)
		{
			BakedProp componentInChildren = goProp.GetComponentInChildren<BakedProp>(includeInactive: true);
			if ((bool)componentInChildren)
			{
				componentInChildren.Show();
			}
			if (prop != null && !ProceduralMapTile.IsInLevelEditorMode && !GameState.IsInLevelEditor)
			{
				prop.IsBaked = true;
			}
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: false);
			}
		}
		ApparanceEntity componentInChildren2 = goProp.GetComponentInChildren<ApparanceEntity>(includeInactive: true);
		if ((bool)componentInChildren2)
		{
			componentInChildren2.Frozen = flag;
			componentInChildren2.enabled = !flag;
		}
		IProceduralTile componentInChildren3 = goProp.GetComponentInChildren<IProceduralTile>(includeInactive: true);
		bool flag2 = componentInChildren3 != null;
		if ((flag || (flag2 && prop.PropType == EPropType.PressurePlate)) && componentInChildren3 != null && componentInChildren3.IsActive)
		{
			string text = tile.m_HexMap?.MapTiles.Concat(tile.m_HexMap?.MapTiles ?? new List<CMapTile>()).First((CMapTile a) => TileIndex.Compare(a.ArrayIndex, tile.m_ArrayIndex)).TileGuid;
			if (!string.IsNullOrEmpty(text))
			{
				_ = goProp.transform.position;
				CutTerrainHole(text);
			}
		}
	}

	private void OnDestroy()
	{
		_instance = null;
		Unload();
	}

	internal void Unload()
	{
		string[] array = new string[SceneManager.sceneCount];
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			array[i] = SceneManager.GetSceneAt(i).name;
		}
		_ = ProceduralMapTile.IsInLevelEditorMode;
		foreach (Scene loadedScene in _loadedScenes)
		{
			SceneManager.UnloadSceneAsync(loadedScene);
		}
	}

	public static void ProceduralContentCreated(UnityEngine.Object content)
	{
		if ((bool)Instance)
		{
			Instance.ApparanceGenerationTracker.OnContentGenerated(content, content.name);
		}
		else if (!Application.isPlaying)
		{
			_GYMApparanceGenerationTracker.Spawned();
		}
	}

	public GameObject GetProp(string guid)
	{
		GameObject gameObject = _environmentMapping?.GetProp(guid);
		if (gameObject != null)
		{
			return gameObject;
		}
		if (!IsLoadingComplete)
		{
			throw new Exception("Forbidden before EnvironmentMapping is initialized");
		}
		return null;
	}

	public GameObject GetTile(string guid)
	{
		return _environmentMapping?.GetTile(guid);
	}

	public GameObject GetTileHex(string guid)
	{
		return _environmentMapping?.GetTileHex(guid);
	}

	public void RemoveTile(string guid)
	{
		_environmentMapping?.RemoveTile(guid);
	}

	public GameObject GetMap(string guid)
	{
		GameObject gameObject = _environmentMapping?.GetMap(guid);
		if (gameObject != null)
		{
			return gameObject;
		}
		if (!IsLoadingComplete)
		{
			throw new Exception("Forbidden before EnvironmentMapping is initialized");
		}
		return null;
	}

	public IEnumerable<GameObject> GetMaps()
	{
		return _environmentMapping?.GetMaps() ?? throw new Exception("Forbidden before EnvironmentMapping is initialized");
	}

	public IEnumerable<GameObject> GetProps()
	{
		return _environmentMapping?.GetProps() ?? throw new Exception("Forbidden before EnvironmentMapping is initialized");
	}

	public IEnumerable<GameObject> GetTiles()
	{
		return _environmentMapping?.GetTiles() ?? throw new Exception("Forbidden before EnvironmentMapping is initialized");
	}

	public void AddMap(GameObject map, string guid)
	{
		_environmentMapping?.AddMap(map, guid);
	}

	public void AddProp(GameObject prop, string guid)
	{
		if (!(_environmentMapping?.GetProp(guid)))
		{
			_environmentMapping?.AddProp(prop, guid);
		}
	}

	public void AddTile(Mapping.TileData tile, string guid)
	{
		_environmentMapping?.AddTile(tile, guid);
	}

	public GameObject PlugTerrainHole(UnityEngine.Vector3 pos, string tileGuid, string roomGuid, GameObject destroyedBy = null)
	{
		GameObject map = GetMap(roomGuid);
		GameObject gameObject = map.transform.Find("Generated Content")?.gameObject;
		if (!gameObject)
		{
			gameObject = new GameObject("Generated Content");
			gameObject.transform.SetParent(map.transform);
		}
		GameObject gameObject2 = gameObject.FindInChildren("Full");
		if (!gameObject2)
		{
			gameObject2 = new GameObject("Full");
			gameObject2.transform.SetParent(gameObject.transform);
		}
		GameObject tile = GetTile(tileGuid);
		if (tile != null && (destroyedBy == null || destroyedBy != tile))
		{
			tile.SetActive(value: true);
			return tile;
		}
		Transform transform = null;
		List<Transform> list = GetTiles()?.Where((GameObject a) => a != null && (a.name.Contains("Hex", StringComparison.InvariantCultureIgnoreCase) || a.name.Contains("Floor", StringComparison.InvariantCultureIgnoreCase)) && a.name.Contains("Shape", StringComparison.InvariantCultureIgnoreCase))?.Select((GameObject a) => a.transform)?.ToList();
		if (list != null && list.Count > 0)
		{
			transform = list.OrderBy((Transform x) => UnityEngine.Vector3.SqrMagnitude(x.position - pos)).FirstOrDefault();
		}
		GameObject gameObject3 = ((!(transform != null)) ? UnityEngine.Object.Instantiate(simpleTileFallback, gameObject2.transform) : UnityEngine.Object.Instantiate(transform.gameObject, transform.parent));
		gameObject3.SetActive(value: true);
		gameObject3.transform.position = pos;
		Mapping.TileData tile2 = new Mapping.TileData(null, gameObject3, null, pos);
		AddTile(tile2, tileGuid);
		return gameObject3;
	}

	public void CutTerrainHole(string tileGuid, Vector4? circleArea = null)
	{
		GameObject tile = _environmentMapping.GetTile(tileGuid);
		if (!tile)
		{
			return;
		}
		if (circleArea.HasValue)
		{
			Vector4 value = circleArea.Value;
			UnityEngine.Vector3 vector = new UnityEngine.Vector3(value.x, value.y, value.z);
			float num = value.w;
			if ((double)num <= 0.001)
			{
				num = 1f;
			}
			float num2 = num * num;
			if ((tile.transform.position - vector).sqrMagnitude <= num2)
			{
				tile.SetActive(value: false);
			}
		}
		else
		{
			tile.SetActive(value: false);
		}
	}

	public static SmartCoroutine InitializeProceduralContent(IMapping mapping, ScenarioState state)
	{
		return _instance.StartSmartCoroutine(InitializeProceduralContentCo(mapping, state));
	}

	private static IEnumerator InitializeProceduralContentCo(IMapping mapping, ScenarioState state)
	{
		using RespectFrameTime respect = new RespectFrameTime("EnvironmentLoader.InitializeProceduralContentCo");
		if (respect.ShouldWait())
		{
			yield return respect.WaitUntilNextFrameIfNeeded("Beginning of Initialize Procedural Content");
		}
		int seed = state.Seed;
		UnityGameEditorObject[] array = UnityGameEditorObject.s_all.ToArray();
		foreach (UnityGameEditorObject unityGameEditorObject in array)
		{
			if (unityGameEditorObject.gameObject.activeInHierarchy && unityGameEditorObject.m_InvisableAtRuntime)
			{
				MeshRenderer component = unityGameEditorObject.GetComponent<MeshRenderer>();
				if (component != null)
				{
					component.enabled = UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime;
				}
			}
		}
		ProceduralScenario procScenario = mapping.EnvironmentRoot.GetComponent<ProceduralScenario>();
		procScenario.ScenarioSeed = seed;
		int doorCount = -1;
		CObjectDoor[] doors = state.Props.OfType<CObjectDoor>().ToArray();
		CObjectDoor[] array2 = doors;
		foreach (CObjectDoor door in array2)
		{
			if (respect.ShouldWait())
			{
				int i = doorCount + 1;
				doorCount = i;
				yield return respect.WaitUntilNextFrameIfNeeded($"Set Door Lock Types {i}");
			}
			GameObject gameObject = mapping.GetProp(door.PropGuid);
			if (gameObject == null)
			{
				gameObject = Choreographer.FindPropGameObject(door);
			}
			UnityGameEditorDoorProp component2 = gameObject.GetComponent<UnityGameEditorDoorProp>();
			if ((bool)component2 && (door.LockType == CObjectDoor.ELockType.None || door.DoorIsLocked))
			{
				component2.m_LockType = door.LockType;
			}
		}
		if (respect.ShouldWait())
		{
			yield return respect.WaitUntilNextFrameIfNeeded("After Set Door Lock Types");
		}
		procScenario.AnalyseWalls();
		if (respect.ShouldWait())
		{
			yield return respect.WaitUntilNextFrameIfNeeded("After Analyse Walls");
		}
		if (!LevelEditorController.s_Instance.IsEditing && !LevelEditorController.s_Instance.IsPreviewingLevel && !SaveData.Instance.Global.CurrentlyPlayingCustomLevel)
		{
			yield break;
		}
		CCustomLevelData customData = SaveData.Instance.Global.CurrentCustomLevelData ?? SaveData.Instance.Global.CurrentEditorLevelData;
		if (!customData.HasApparanceOverrides)
		{
			yield break;
		}
		CApparanceOverrideDetails apparanceOverrideDetailsForGUID = customData.GetApparanceOverrideDetailsForGUID("ScenarioApparanceOverrideGUID");
		if (apparanceOverrideDetailsForGUID != null)
		{
			mapping.EnvironmentRoot.GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID);
		}
		int j = -1;
		CMap[] array3 = state.Maps.ToArray();
		CMap[] array4 = array3;
		foreach (CMap map in array4)
		{
			if (respect.ShouldWait())
			{
				int i = j + 1;
				j = i;
				yield return respect.WaitUntilNextFrameIfNeeded($"Check map custom overrides {i}");
			}
			GameObject map2 = mapping.GetMap(map.MapGuid);
			if (!(map2 != null))
			{
				continue;
			}
			CApparanceOverrideDetails apparanceOverrideDetailsForGUID2 = customData.GetApparanceOverrideDetailsForGUID(map.MapGuid);
			if (apparanceOverrideDetailsForGUID2 != null)
			{
				map2.GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID2);
			}
			ProceduralWall[] componentsInChildren = map2.GetComponentsInChildren<ProceduralWall>();
			for (int l = 0; l < componentsInChildren.Length; l++)
			{
				CApparanceOverrideDetails apparanceOverrideDetailsForGUID3 = customData.GetApparanceOverrideDetailsForGUID(CApparanceOverrideDetails.GetWallGUID(map, l));
				if (apparanceOverrideDetailsForGUID3 != null)
				{
					componentsInChildren[l].GetComponent<ProceduralStyle>()?.InitialiseWithOverride(apparanceOverrideDetailsForGUID3);
				}
			}
		}
		int doorIndex = -1;
		array2 = doors;
		foreach (CObjectDoor door in array2)
		{
			if (respect.ShouldWait())
			{
				int i = doorIndex + 1;
				doorIndex = i;
				yield return respect.WaitUntilNextFrameIfNeeded($"Check door apparance overrides {i}");
			}
			CApparanceOverrideDetails doorOverrideData = customData.GetApparanceOverrideDetailsForGUID(door.PropGuid);
			if (doorOverrideData == null)
			{
				continue;
			}
			GameObject prop = mapping.GetProp(door.PropGuid);
			ProceduralStyle[] doorstyles = prop.GetComponentsInChildren<ProceduralStyle>();
			for (int m = 0; m < doorstyles.Length; m++)
			{
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded($"Door Styles {m}");
				}
				doorstyles[m]?.InitialiseWithOverride(doorOverrideData);
			}
			for (int m = 0; m < ApparanceLayer.Instance.DoorInstances.Count; m++)
			{
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded($"Check list of doors {m}");
				}
				if (!(ApparanceLayer.Instance.DoorInstances[m].name == door.InstanceName))
				{
					continue;
				}
				doorstyles = ApparanceLayer.Instance.DoorInstances[m].GetComponentsInChildren<ProceduralStyle>();
				for (int n = 0; n < doorstyles.Length; n++)
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded();
					}
					doorstyles[m]?.InitialiseWithOverride(doorOverrideData);
				}
			}
		}
	}

	public void BeforePropDestroyed(GameObject goProp, CObjectProp cProp)
	{
		GameObject gameObject = goProp.FindInImmediateChildren("Generated Content");
		if (!(gameObject == null))
		{
			if (DestroyedGeneratedContent.TryGetValue(cProp.PropGuid, out var value))
			{
				UnityEngine.Object.Destroy(value.go);
			}
			BreakableObject componentInChildren = gameObject.GetComponentInChildren<BreakableObject>(includeInactive: true);
			if (componentInChildren == null)
			{
				componentInChildren = goProp.GetComponentInChildren<BreakableObject>(includeInactive: true);
			}
			if (componentInChildren != null)
			{
				componentInChildren.Restore();
			}
			MaterialModification[] componentsInChildren = gameObject.GetComponentsInChildren<MaterialModification>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Restore();
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, ObjectPool.instance.transform);
			gameObject2.name = gameObject.name;
			gameObject2.SetActive(value: false);
			GeneratedContentCache generatedContentCache = new GeneratedContentCache
			{
				go = gameObject2,
				biomeState = ScenarioPossibleRoom.EBiomeState.Inherit
			};
			ProceduralStyle component = goProp.GetComponent<ProceduralStyle>();
			if (component != null)
			{
				generatedContentCache.biomeState = component.BiomeState;
			}
			DestroyedGeneratedContent[cProp.PropGuid] = generatedContentCache;
		}
	}

	private void TryRestoreGeneratedContent(GameObject goProp, CObjectProp cProp)
	{
		GameObject gameObject = goProp.FindInImmediateChildren("Generated Content");
		if (!(gameObject == null) && gameObject.transform.childCount == 0 && DestroyedGeneratedContent.TryGetValue(cProp.PropGuid, out var value))
		{
			ProceduralStyle component = goProp.GetComponent<ProceduralStyle>();
			if (component != null)
			{
				component.BiomeState = value.biomeState;
			}
			GameObject go = value.go;
			go.transform.SetParent(gameObject.transform.parent);
			go.transform.localPosition = gameObject.transform.localPosition;
			go.transform.localRotation = gameObject.transform.localRotation;
			go.SetActive(value: true);
			gameObject.transform.SetParent(null);
			UnityEngine.Object.Destroy(gameObject);
			if ((bool)goProp.GetComponent<ProceduralTile>())
			{
				CutTerrainHole(cProp.PropTile?.m_Hex?.TileGuid ?? cProp.PropTile?.m_Hex2?.TileGuid);
			}
			DestroyedGeneratedContent.Remove(cProp.PropGuid);
		}
	}

	public void ClearDestroytedContent()
	{
		foreach (KeyValuePair<string, GeneratedContentCache> item in DestroyedGeneratedContent)
		{
			UnityEngine.Object.DestroyImmediate(item.Value.go);
		}
		DestroyedGeneratedContent.Clear();
	}

	[ConsoleCommand(Command = "EnvLoader_ApparancePrint", Description = "Prints the current state of the Apparance tracker.")]
	public static void CommandApparancePrint(IConsole console)
	{
		console.WriteLine(Instance.ApparanceGenerationTracker.DebugPrint());
	}

	[ConsoleCommand(Command = "EnvLoader_Default", Description = "Default, contextual loading. Load baked if available in release builds, prefer proc gen. in all other cases")]
	public static void CommandDefault(IConsole console)
	{
		_mode = Preference.None;
		PlayerPrefs.DeleteKey("EnvLoader_DebugMode");
		console.WriteLine("Environment loader set to default mode.");
	}

	[ConsoleCommand(Command = "EnvLoader_PreferApparance", Description = "Use apparance content and carry over baked custom changes from environments")]
	public static void CommandPreferApparance(IConsole console)
	{
		_mode = Preference.PreferApparance;
		PlayerPrefs.DeleteKey("EnvLoader_DebugMode");
		console.WriteLine("Environment loader set to prefer apparance content.");
	}

	[ConsoleCommand(Command = "EnvLoader_ApparanceOnly", Description = "Set environment loader to use only apparance content and ignore available baked environments")]
	public static void CommandApparanceOnly(IConsole console)
	{
		_mode = Preference.ApparanceExclusively;
		PlayerPrefs.DeleteKey("EnvLoader_DebugMode");
		console.WriteLine("Environment loader set to use only apparance content.");
	}

	[ConsoleCommand(Command = "EnvLoader_PreferBaked", Description = "Set environment loader to use baked environments when available.")]
	public static void CommandPreferBaked(IConsole console)
	{
		_mode = Preference.PreferBaked;
		PlayerPrefs.DeleteKey("EnvLoader_DebugMode");
		console.WriteLine("Environment loader set to prefer baked environments.");
	}

	[ConsoleCommand(Command = "EnvLoader_BakedOnly", Description = "Set environment loader to use baked environments exclusively.")]
	public static void CommandBakedOnly(IConsole console)
	{
		_mode = Preference.BakedOnly;
		PlayerPrefs.DeleteKey("EnvLoader_DebugMode");
		console.WriteLine("Environment loader set to use baked environments exclusively.");
	}

	[ConsoleCommand(Command = "EnvLoader_SimpleFast", Description = "Set environment loader to use fast graphics that load quickly.")]
	public static void CommandSimpleFast(IConsole console)
	{
		_mode = Preference.SimpleFast;
		PlayerPrefs.DeleteKey("EnvLoader_DebugMode");
		console.WriteLine("Environment loader set to use simple and fast graphics.");
	}

	[ConsoleCommand(Command = "EnvLoader_SavePrefs", Description = "Save environment loader preferences. Next mode change will delete stored prefs.")]
	public static void CommandSavePrefs(IConsole console)
	{
		PlayerPrefs.SetInt("EnvLoader_DebugMode", (int)_mode);
		console.WriteLine("Environment loader preferences saved.");
	}

	[ConsoleCommand(Command = "EnvLoader_Mode", Description = "Prints out the current map loading mode")]
	public static void CommandPreference(IConsole console)
	{
		console.WriteLine($"Environment loader is set to: {_mode}");
		if ((bool)Instance)
		{
			console.WriteLine($"Environment root: {Instance.EnvironmentRoot}");
		}
	}

	public void SetTempMapping(IMapping currentMapping)
	{
		_environmentMapping = currentMapping;
		RemoveCompletedSmartCoroutines();
	}

	public static void EnableApparanceInGYMs()
	{
		ApparanceEngine apparanceEngine = UnityEngine.Object.FindObjectOfType<ApparanceEngine>();
		apparanceEngine.enabled = true;
		apparanceEngine.PrefabInstancing = true;
		apparanceEngine.ShowGenerated = true;
		apparanceEngine.PlayerLiveEditing = true;
		apparanceEngine.EditorLiveEditing = true;
	}

	public void LogMessage(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	public void LogWarning(string message)
	{
		UnityEngine.Debug.LogWarning(message);
	}

	public void LogError(string message)
	{
		if (!message.Contains("unknown"))
		{
			UnityEngine.Debug.LogError("[ERROR] " + message);
		}
		else
		{
			UnityEngine.Debug.Log("Safely caught: " + message);
		}
	}
}

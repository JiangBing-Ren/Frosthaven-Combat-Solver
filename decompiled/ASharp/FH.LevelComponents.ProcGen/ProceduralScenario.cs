using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Chronos;
using ClockStone;
using FH.ApparanceBaking;
using FH.LevelComponents.TopDownProjection;
using FH.Misc;
using FH.PostProcessing;
using FH.SeasonTech;
using FH.Tools;
using Libs.Utils;
using Libs.Utils.Maths;
using Libs.Utils.Profiler;
using MeshSlicing;
using SETUtil.Extend;
using SETUtil.SceneUI;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[ExecuteAlways]
[RequireComponent(typeof(ProceduralStyle))]
public class ProceduralScenario : MonoBehaviour
{
	public Dictionary<Mesh, ProceduralMapTile.StreamReconstructionData> ReadableMeshCached = new Dictionary<Mesh, ProceduralMapTile.StreamReconstructionData>();

	private Vector3 lastFocalPoint;

	public KronosFogOfWar KronosFogOfWar;

	[NonSerialized]
	public VerticalMeshSlicer VerticalMeshSlicer;

	[NonSerialized]
	public SliceFillerGenerationSettingsDef SliceFillerSettingsDef;

	[NonSerialized]
	public OcclusionFadingSettingsDef OcclusionFadingSettingsDef;

	private Dictionary<string, AudioObject> _ambienceAudio = new Dictionary<string, AudioObject>();

	private ProceduralMapTile[] _mapTiles;

	private string[] _mapAmbientDescriptors;

	[Header("Visual Debugging : Hide Content")]
	[Tooltip("Temporarily disable all the generated content in this scenario")]
	public bool ShowGeneratedContent = true;

	[Tooltip("Temporarily disable all the base tiles in this scenario")]
	public bool ShowTemplateTiles = true;

	[Tooltip("Temporarily disable any custom tile based objects, e.g. doors, chests, etc manually in test scenes")]
	public bool ShowCustomTiles = true;

	[Header("Visual Debugging : Wall Analysis")]
	[Tooltip("Temporarily show wall bounds (original, and adjusted if available)")]
	public bool ShowWallBounds;

	[Tooltip("Temporarily perform scenario-wide wall calculation and adjustment")]
	public bool PerformWallAnalysis;

	[Tooltip("Draw all lines with endpoint jitter, helps see overlapping segments")]
	public bool JitterLines;

	public int onlyDrawThisWall = -1;

	[Header("Visual Debugging : Style")]
	[Tooltip("Temporarily assign random visual style parameters to all the rooms")]
	public bool RandomiseRooms;

	public int RandomRoomSeed;

	private ApparanceStepDebugScaffolding apparanceDebug = new ApparanceStepDebugScaffolding();

	private ProceduralWallAnalyser wallAnalyser;

	private bool m_InitialEditorWallSetup;

	private List<GameObject> toActivate = new List<GameObject>();

	private List<GameObject> toDeactivate = new List<GameObject>();

	private List<GameObject> toRefreshTracking = new List<GameObject>();

	private List<ProceduralWallAnalyser.WallInfo> originalWalls = new List<ProceduralWallAnalyser.WallInfo>();

	private List<ProceduralWallAnalyser.WallInfo> currentWalls = new List<ProceduralWallAnalyser.WallInfo>();

	[NonSerialized]
	private GloomUtility.ValueChangeTracker m_ValueTracking;

	[NonSerialized]
	private Ambience selectedAmbience;

	private SmartCoroutine currentAmbienceTransitionCrt;

	public const float ambienceTransitionTime = 0.5f;

	private static GUILabel currentAmbienceSceneViewLabel = new GUILabel("Ambience");

	private Vector3 ambienceGizmoTarget = Vector3.zero;

	public AmbienceDiscriminator AmbienceDiscriminator = new AmbienceDiscriminator();

	public const int SHADOWCASTER_LIMIT = 10;

	private static readonly float _camSpaceBoundMin = 0.2f;

	private static readonly float _camSpaceBoundMax = 0.8f;

	public int ScenarioSeed
	{
		get
		{
			ProceduralStyle component = GetComponent<ProceduralStyle>();
			if (component != null)
			{
				return component.Seed;
			}
			return 0;
		}
		set
		{
			ProceduralStyle component = GetComponent<ProceduralStyle>();
			if (component != null)
			{
				component.Seed = value;
			}
		}
	}

	private bool IsEditor => !Application.isPlaying;

	private GloomUtility.ValueChangeTracker ValueTracker
	{
		get
		{
			if (m_ValueTracking == null)
			{
				m_ValueTracking = new GloomUtility.ValueChangeTracker();
			}
			return m_ValueTracking;
		}
	}

	public bool IsVertexPaintingComplete => MapTiles.All((ProceduralMapTile a) => a.IsVertexPaintingComplete);

	private IEnumerable<GameObject> PrefabWalls
	{
		get
		{
			foreach (ProceduralMapTile mapTile in MapTiles)
			{
				_ = mapTile;
				GameObject gameObject = null;
				if (!(gameObject != null))
				{
					continue;
				}
				GameObject gameObject2 = gameObject.FindInImmediateChildren("Walls", includeInactive: true);
				if (!(gameObject2 != null))
				{
					continue;
				}
				foreach (GameObject item in ChildrenOf(gameObject2))
				{
					yield return item;
				}
			}
		}
	}

	private IEnumerable<GameObject> Walls
	{
		get
		{
			foreach (ProceduralMapTile mapTile in MapTiles)
			{
				GameObject gameObject = mapTile.gameObject.FindInImmediateChildren("Walls", includeInactive: true);
				if (!(gameObject != null))
				{
					continue;
				}
				foreach (GameObject item in ChildrenOf(gameObject))
				{
					yield return item;
				}
			}
		}
	}

	private IEnumerable<GameObject> CustomTiles => from o in ObjectsOfTypeRecursive(gameObject, typeof(ProceduralTile), continue_inside_match: false)
		where o.GetComponent<ApparanceEntity>() == null
		select o;

	private IEnumerable<GameObject> GeneratedContent
	{
		get
		{
			foreach (GameObject entity in Entities)
			{
				GameObject gameObject = entity.FindInImmediateChildren("Generated Content", includeInactive: true);
				if (gameObject != null)
				{
					yield return gameObject;
				}
			}
		}
	}

	private IEnumerable<GameObject> Entities => ObjectsOfTypeRecursive(gameObject, typeof(ApparanceEntity));

	private IEnumerable<GameObject> Templates
	{
		get
		{
			foreach (ProceduralMapTile mapTile in MapTiles)
			{
				IEnumerable<GameObject> enumerable = ChildrenOf(mapTile.gameObject).Where((GameObject o) =>
				{
					if (o.name == "Generated Content" && o.name == "Walls")
					{
						return false;
					}
					UnityGameEditorObject component = o.GetComponent<UnityGameEditorObject>();
					return (component != null && (component.m_ObjectType == ScenarioManager.ObjectImportType.Coverage || component.m_ObjectType == ScenarioManager.ObjectImportType.Tile || component.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile)) ? true : false;
				});
				foreach (GameObject item in enumerable)
				{
					yield return item;
				}
			}
			foreach (GameObject prop in Props)
			{
				IEnumerable<GameObject> enumerable2 = ChildrenOf(prop).Where((GameObject o) =>
				{
					UnityGameEditorObject component = o.GetComponent<UnityGameEditorObject>();
					return component != null && component.m_ObjectType == ScenarioManager.ObjectImportType.Coverage;
				});
				foreach (GameObject item2 in enumerable2)
				{
					yield return item2;
				}
			}
		}
	}

	internal IEnumerable<ProceduralMapTile> MapTiles => GetComponentsInChildren<ProceduralMapTile>(includeInactive: true);

	private GameObject PropsContainer
	{
		get
		{
			if (!IsEditor)
			{
				GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					if (gameObject.name == "Props")
					{
						return gameObject;
					}
				}
				return null;
			}
			return base.gameObject;
		}
	}

	private IEnumerable<GameObject> Props
	{
		get
		{
			if (!IsEditor)
			{
				foreach (GameObject item in ChildrenOf(PropsContainer))
				{
					if (item.GetComponent<UnityGameEditorObject>() != null)
					{
						yield return item;
					}
				}
				yield break;
			}
			foreach (GameObject item2 in ChildrenOf(PropsContainer))
			{
				if (item2.GetComponent<ProceduralProp>() != null)
				{
					yield return item2;
				}
			}
		}
	}

	internal IEnumerable<GameObject> Doorways
	{
		get
		{
			if (!IsEditor)
			{
				foreach (ProceduralMapTile mapTile in MapTiles)
				{
					ProceduralDoorway[] componentsInChildren = mapTile.GetComponentsInChildren<ProceduralDoorway>();
					foreach (ProceduralDoorway proceduralDoorway in componentsInChildren)
					{
						yield return proceduralDoorway.gameObject;
					}
				}
				yield break;
			}
			foreach (ProceduralMapTile mapTile2 in MapTiles)
			{
				ProceduralDoorway[] componentsInChildren = mapTile2.GetComponentsInChildren<ProceduralDoorway>();
				foreach (ProceduralDoorway proceduralDoorway2 in componentsInChildren)
				{
					yield return proceduralDoorway2.gameObject;
				}
			}
		}
	}

	private IEnumerable<GameObject> MyChildren => ChildrenOf(gameObject);

	internal void NotifySeedChange()
	{
		DistributeSeeds();
	}

	protected virtual void OnDestroy()
	{
		AmbienceDiscriminator.Clear();
		apparanceDebug.Dispose();
		ReadableMeshCached.Clear();
		KronosFogOfWar?.DiscardCel();
		VerticalMeshSlicer?.Dispose();
	}

	public void AnalyseWalls()
	{
		wallAnalyser = new ProceduralWallAnalyser();
		DistributeSeeds();
		using (new BakedWallBounds.AnalysisAssist(GetComponentsInChildren<ProceduralWall>()))
		{
			CacheWallBounds(IsEditor);
			wallAnalyser.SetSeed(ScenarioSeed);
			wallAnalyser.SetWalls(IsEditor ? originalWalls : currentWalls);
			if (wallAnalyser.Run())
			{
				wallAnalyser.ApplyWalls(currentWalls);
			}
			wallAnalyser = null;
		}
	}

	private void DistributeSeeds()
	{
		System.Random random = new System.Random(ScenarioSeed);
		foreach (ProceduralMapTile mapTile in MapTiles)
		{
			int seed = random.Next();
			ProceduralStyle component = mapTile.GetComponent<ProceduralStyle>();
			if (component != null && !component.HasBeenManuallyInitialised)
			{
				component.Seed = seed;
			}
			System.Random random2 = new System.Random(seed);
			foreach (ProceduralWall item in GloomUtility.FindAllDescendentComponents<ProceduralWall>(mapTile))
			{
				int seed2 = random2.Next();
				ProceduralStyle component2 = item.GetComponent<ProceduralStyle>();
				if (component2 != null && !component2.HasBeenManuallyInitialised)
				{
					component2.Seed = seed2;
				}
			}
		}
	}

	private void OnEnable()
	{
		if (IsEditor)
		{
			m_InitialEditorWallSetup = true;
		}
	}

	private void OnValidate()
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = false;
		if (ValueTracker.CheckValue("ShowGeneratedContent", ShowGeneratedContent) || ValueTracker.CheckValue("ShowTemplateTiles", ShowTemplateTiles) || ValueTracker.CheckValue("ShowCustomTiles", ShowCustomTiles) || ValueTracker.CheckValue("ShowWallBounds", ShowWallBounds))
		{
			flag = true;
		}
		if (flag)
		{
			ApplyVisualOptions();
		}
		bool flag2 = ValueTracker.CheckValue("RandomiseRooms", RandomiseRooms ? 1 : 0);
		bool flag3 = ValueTracker.CheckValue("RandomRoomSeed", RandomRoomSeed);
		if (!(flag2 | flag3))
		{
			return;
		}
		if (flag2)
		{
			if (RandomiseRooms)
			{
				SetRandomRoomStyles();
			}
			else
			{
				ClearRandomRoomStyles();
			}
		}
		else if (flag3 && RandomiseRooms)
		{
			SetRandomRoomStyles();
		}
	}

	private void ApplyVisualOptions()
	{
		if (IsEditor)
		{
			ApplyGeneratedContent();
			ApplyTileTemplates();
			ApplyCustomTiles();
		}
	}

	private void ApplyGeneratedContent()
	{
		if (!IsEditor)
		{
			return;
		}
		foreach (GameObject item in GeneratedContent)
		{
			DeferredSetActive(item, ShowGeneratedContent);
		}
	}

	private void ApplyTileTemplates()
	{
		if (!IsEditor)
		{
			return;
		}
		foreach (GameObject template in Templates)
		{
			DeferredSetActive(template, ShowTemplateTiles);
		}
	}

	private void ApplyCustomTiles()
	{
		if (!IsEditor)
		{
			return;
		}
		foreach (GameObject customTile in CustomTiles)
		{
			if (customTile.activeSelf != ShowCustomTiles)
			{
				DeferredSetActive(customTile, ShowCustomTiles);
				toRefreshTracking.Add(customTile);
			}
		}
	}

	private void CacheWallBounds(bool include_prefabs)
	{
		originalWalls.Clear();
		currentWalls.Clear();
		List<GameObject> list = null;
		if (include_prefabs)
		{
			list = new List<GameObject>();
			list.AddRange(PrefabWalls);
		}
		List<GameObject> list2 = new List<GameObject>();
		list2.AddRange(Walls);
		int num = list2.Count;
		if (include_prefabs)
		{
			num = Math.Min(list.Count, num);
		}
		for (int i = 0; i < num; i++)
		{
			Transform transform = list2[i].transform;
			if (include_prefabs)
			{
				BoxCollider component = list[i].GetComponent<BoxCollider>();
				if (component != null)
				{
					Transform transform2 = component.transform;
					Matrix4x4 localToWorld = transform.transform.parent.localToWorldMatrix * transform2.localToWorldMatrix;
					originalWalls.Add(new ProceduralWallAnalyser.WallInfo
					{
						wallObject = list2[i],
						boxBounds = component,
						boxParent = transform,
						localToWorld = localToWorld
					});
				}
			}
			BoxCollider component2 = list2[i].GetComponent<BoxCollider>();
			Matrix4x4 localToWorldMatrix = list2[i].transform.localToWorldMatrix;
			currentWalls.Add(new ProceduralWallAnalyser.WallInfo
			{
				wallObject = list2[i],
				boxBounds = component2,
				boxParent = transform,
				localToWorld = localToWorldMatrix
			});
		}
	}

	private void DeferredSetActive(GameObject o, bool active)
	{
		if (active)
		{
			toActivate.Add(o);
		}
		else
		{
			toDeactivate.Add(o);
		}
	}

	private void SetRandomRoomStyles()
	{
		ProceduralStyle component = GetComponent<ProceduralStyle>();
		if (!(component != null))
		{
			return;
		}
		System.Random random = new System.Random(RandomRoomSeed);
		foreach (ProceduralMapTile mapTile in MapTiles)
		{
			ProceduralStyle component2 = mapTile.GetComponent<ProceduralStyle>();
			if (component2 != null)
			{
				SetRandomEnum<ScenarioPossibleRoom.EBiome>(out component2.Biome, 2, random);
				SetRandomEnum<ScenarioPossibleRoom.ESubBiome>(out component2.SubBiome, 1, random);
				SetRandomEnum<ScenarioPossibleRoom.ETheme>(out component2.Theme, 1, random);
				SetRandomEnum<ScenarioPossibleRoom.ESubTheme>(out component2.SubTheme, 1, random);
				SetRandomEnum<ScenarioPossibleRoom.ETone>(out component2.Tone, 1, random);
				ScenarioSeed = random.Next();
				component.CheckChanges(invalidate_all: true);
			}
		}
	}

	private void ClearRandomRoomStyles()
	{
		ProceduralStyle component = GetComponent<ProceduralStyle>();
		if (!(component != null))
		{
			return;
		}
		new System.Random(RandomRoomSeed);
		foreach (ProceduralMapTile mapTile in MapTiles)
		{
			ProceduralStyle component2 = mapTile.GetComponent<ProceduralStyle>();
			if (component2 != null)
			{
				component2.Biome = ScenarioPossibleRoom.EBiome.Inherit;
				component2.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
				component2.Theme = ScenarioPossibleRoom.ETheme.Inherit;
				component2.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
				component2.Tone = ScenarioPossibleRoom.ETone.Inherit;
				ScenarioSeed = 0;
				component.CheckChanges(invalidate_all: true);
			}
		}
	}

	private static void SetRandomEnum<ET>(out ET enum_value, int min_index, System.Random rng)
	{
		List<ET> list = typeof(ET).GetEnumValues().Cast<ET>().ToList();
		int num = list.Count - 1;
		int index = min_index + rng.Next(num - min_index);
		ET val = list[index];
		enum_value = val;
	}

	public void Update()
	{
		UpdateAmbience();
		UpdateAmbientAudioPlayers();
		VerticalMeshSlicer verticalMeshSlicer = VerticalMeshSlicer;
		if (verticalMeshSlicer != null && !verticalMeshSlicer.Idle)
		{
			VerticalMeshSlicer.Tick();
		}
	}

	private void UpdateAmbience()
	{
		bool flag = selectedAmbience == null;
		if (HasCameraPositionChangedThisFrame() || flag)
		{
			Vector3 focalPoint = GetFocalPoint();
			ProceduralMapTile proceduralMapTile = FindClosestVisibleMaptile(focalPoint, _mapTiles);
			if (proceduralMapTile != null && (flag || !IsSelectedAmbienceSameAs(proceduralMapTile.Ambience)))
			{
				StartNewTransition(proceduralMapTile, _mapTiles);
			}
		}
	}

	private void StartNewTransition(ProceduralMapTile closest, IEnumerable<ProceduralMapTile> mapTiles)
	{
		currentAmbienceTransitionCrt?.Stop();
		if (enabled || !ProceduralMapTile.IsInLevelEditorMode)
		{
			currentAmbienceTransitionCrt = SmartCoroutine.Start(this, TransitionToAmbience(closest.Ambience, mapTiles));
		}
	}

	public Vector3 GetFocalPoint()
	{
		Vector3 result = Vector3.zero;
		if (CameraController.s_CameraController != null)
		{
			result = CameraController.s_CameraController.m_TargetFocalPoint;
		}
		return result;
	}

	private bool HasCameraPositionChangedThisFrame()
	{
		Vector3 focalPoint = GetFocalPoint();
		if ((double)(lastFocalPoint - focalPoint).sqrMagnitude > 0.02)
		{
			lastFocalPoint = focalPoint;
			return true;
		}
		return false;
	}

	private IEnumerator TransitionToAmbience(Ambience ambience, IEnumerable<ProceduralMapTile> mapTiles)
	{
		selectedAmbience = ambience;
		float transition = 0f;
		Dictionary<ProceduralMapTile, float> blendTargets = new Dictionary<ProceduralMapTile, float>(mapTiles.Count());
		bool isGamePlaying = Application.isPlaying;
		foreach (ProceduralMapTile item in mapTiles.Where((ProceduralMapTile a) => (bool)a && (bool)a.Ambience))
		{
			item.Ambience.OnBeginBlending();
			blendTargets.Add(item, (item.Ambience == ambience) ? 1 : 0);
		}
		if (mapTiles != null)
		{
			while (transition < 1f)
			{
				TransitionAll(transition, blendTargets);
				transition += AmbienceTransitionDelta(isGamePlaying);
				yield return null;
			}
		}
		TransitionAll(1f, blendTargets);
		currentAmbienceTransitionCrt = null;
		static void TransitionAll(float blend, Dictionary<ProceduralMapTile, float> dictionary)
		{
			foreach (KeyValuePair<ProceduralMapTile, float> item2 in dictionary)
			{
				ProceduralMapTile key = item2.Key;
				if ((bool)key && (bool)key.Ambience)
				{
					key.Ambience.ApplyBlend(dictionary[key], blend);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float AmbienceTransitionDelta(bool isGamePlaying)
	{
		return Timekeeper.SafeDeltaTime() / 0.5f;
	}

	private bool IsSelectedAmbienceSameAs(Ambience ambience)
	{
		if (!selectedAmbience || !ambience)
		{
			return false;
		}
		return Ambience.Same(selectedAmbience, ambience);
	}

	private ProceduralMapTile FindClosestVisibleMaptile(Vector3 target, IEnumerable<ProceduralMapTile> mapTiles)
	{
		if (mapTiles == null)
		{
			return null;
		}
		Vector2 point = new Vector2(target.x, target.z);
		ProceduralMapTile proceduralMapTile = null;
		(ProceduralMapTile, float) tuple = (null, float.MaxValue);
		foreach (ProceduralMapTile item in mapTiles.Where((ProceduralMapTile a) => a != null))
		{
			if (item.GetVisibility() == ProceduralMapTile.Visibility.Revealed)
			{
				if (Utl.IsPointInside2DShape(point, item.GetShape()))
				{
					proceduralMapTile = item;
					break;
				}
				BoxCollider component = item.GetComponent<BoxCollider>();
				Vector3 zero = Vector3.zero;
				zero = ((!component) ? item.transform.position : component.bounds.center);
				float num = Vector3.SqrMagnitude(zero - target);
				if (item != null && (!tuple.Item1 || tuple.Item2 > num))
				{
					tuple = (item, num);
				}
			}
		}
		if (!(proceduralMapTile != null))
		{
			return tuple.Item1;
		}
		return proceduralMapTile;
	}

	private static void OptimizeAndCapShadowcasters(ProceduralMapTile[] maps)
	{
		int perMapBudget = Mathf.CeilToInt(10f / (float)maps.Length);
		for (int i = 0; i < maps.Length; i++)
		{
			CapShadowcastersInMap(maps[i].gameObject, perMapBudget);
		}
		static void CapShadowcastersInMap(GameObject gameObject, int num3)
		{
			List<Light> list = (from a in gameObject.GetComponentsInChildren<Light>()
				where a.type != LightType.Directional && a.shadows != LightShadows.None
				select a).ToList();
			if (list.Count != 0)
			{
				list.Sort((Light a, Light b) => b.range.CompareTo(a.range) + b.shadowStrength.CompareTo(a.shadowStrength));
				int num = 0;
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					if (num2 > num3)
					{
						list[num2].shadows = LightShadows.None;
						num++;
					}
				}
				if (num > 0)
				{
					Debug.Log($"Disabled {num} shadows for lights in room {gameObject.name}");
				}
			}
		}
	}

	private void ProcessBackgroundMist(GameObject root, IEnumerable<ProceduralMapTile> maps)
	{
		ProceduralMistSurrounding proceduralMistSurrounding = root.GetComponentsInChildren<ProceduralMistSurrounding>(includeInactive: false).FirstOrDefault();
		if ((bool)proceduralMistSurrounding)
		{
			proceduralMistSurrounding.Generate(MultiplaneSystem.GetInstance(), maps, GetVisualsBounds(maps), ref VerticalMeshSlicer.OnMeshSlicingComplete);
			if (VerticalMeshSlicer.CompletedAtLeastOnce)
			{
				VerticalMeshSlicer.OnMeshSlicingComplete?.Invoke();
			}
		}
	}

	private IEnumerator ProcessMapTile(IMapping mapping, ProceduralMapTile proceduralMapTile)
	{
		yield return proceduralMapTile.PostProcessingCo(mapping);
		proceduralMapTile.InitLighting(AmbienceDiscriminator);
		proceduralMapTile.MarkVisibilityDirty();
	}

	public IEnumerator OnMaptileGenerationCompleteCrt(IMapping mapping)
	{
		using (new MiniProfilerScope("OnMaptileGenerationCompleteCrt"))
		{
			if (VerticalMeshSlicer == null)
			{
				SliceFillerSettingsDef = SliceFillerGenerationSettingsDef.LoadResource();
				VerticalMeshSlicer = new VerticalMeshSlicer(SliceFillerSettingsDef.CuttingPlaneHeight);
				Shader.SetGlobalFloat("_WallFadeBottomThreshold", SliceFillerSettingsDef.CuttingPlaneHeight);
			}
			if (OcclusionFadingSettingsDef == null)
			{
				OcclusionFadingSettingsDef = OcclusionFadingSettingsDef.LoadResource();
			}
			Debug.Log("Initial Apparance Generation Complete. Auxiliary post processing is now allowed.");
			ProceduralMapTile[] maps = (from a in mapping.GetMaps()
				select a.GetComponent<ProceduralMapTile>()).ToArray();
			if (ProceduralMapTile.IsInLevelEditorMode)
			{
				maps.ForEach((ProceduralMapTile m) =>
				{
					m.gameObject.SetActive(value: true);
				});
			}
			_mapTiles = maps;
			_mapAmbientDescriptors = new string[_mapTiles.Length];
			InitOrRefreshBiomeStates();
			AmbienceDiscriminator.Clear();
			ProceduralMapTile[] array = maps;
			foreach (ProceduralMapTile proceduralMapTile in array)
			{
				if (proceduralMapTile.GetVisibility() == ProceduralMapTile.Visibility.Revealed)
				{
					yield return ProcessMapTile(mapping, proceduralMapTile);
				}
			}
			array = maps;
			foreach (ProceduralMapTile proceduralMapTile2 in array)
			{
				if (proceduralMapTile2.GetVisibility() == ProceduralMapTile.Visibility.Unrevealed)
				{
					yield return ProcessMapTile(mapping, proceduralMapTile2);
				}
			}
			TileBehaviour[] centers = (from a in mapping.GetMaps()
				select a.GetComponent<RoomVisibilityTracker>().CentralTile).ToArray();
			ScenarioPossibleRoom.ESubBiome[] biomes = (from a in mapping.GetMaps()
				select a.GetComponent<ProceduralStyle>().SubBiome).ToArray();
			ScenarioPossibleRoom.ETheme[] themes = (from a in mapping.GetMaps()
				select a.GetComponent<ProceduralStyle>().Theme).ToArray();
			ScenarioPossibleRoom.ETone[] tones = (from a in mapping.GetMaps()
				select a.GetComponent<ProceduralStyle>().Tone).ToArray();
			if (ProceduralMapTile.IsApplicationPlaying)
			{
				InitAudio(mapping, maps, centers, biomes, themes, tones);
			}
			ProcessBackgroundMist(mapping.EnvironmentRoot, maps);
			OptimizeAndCapShadowcasters(maps);
			KronosFogOfWar?.OnMapUpdate();
			if (!ProceduralMapTile.IsInLevelEditorMode)
			{
				this.GetOrAdd<WallsOcclusionDetection>().Setup();
			}
			InitFoW(maps);
			int num2 = 0;
			foreach (ProceduralMapTile mapTile in MapTiles)
			{
				if (mapTile != null)
				{
					num2++;
					mapTile.WhosYourDaddy(this);
				}
			}
			_ = ProceduralMapTile.IsInLevelEditorMode;
			if (Application.isPlaying)
			{
				Update();
			}
		}
		void CleanupBiomeStates()
		{
			bool isApplicationPlaying = ProceduralMapTile.IsApplicationPlaying;
			BiomeStateBasedToggle[] componentsInChildren = mapping.EnvironmentRoot.GetComponentsInChildren<BiomeStateBasedToggle>();
			foreach (BiomeStateBasedToggle biomeStateBasedToggle in componentsInChildren)
			{
				if ((!ProceduralMapTile.IsInLevelEditorMode & isApplicationPlaying) && !biomeStateBasedToggle.IsCurrentlyActiveAndRelevant())
				{
					UnityEngine.Object.Destroy(biomeStateBasedToggle.gameObject);
				}
			}
		}
		void InitOrRefreshBiomeStates()
		{
			BiomeStateBasedToggle[] componentsInChildren = mapping.EnvironmentRoot.GetComponentsInChildren<BiomeStateBasedToggle>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Refresh();
			}
			CleanupBiomeStates();
		}
	}

	private void InitAudio(IMapping mapping, ProceduralMapTile[] maps, TileBehaviour[] centers, ScenarioPossibleRoom.ESubBiome[] biomes, ScenarioPossibleRoom.ETheme[] themes, ScenarioPossibleRoom.ETone[] tones)
	{
		ScenarioPossibleRoom.EBiomeState[] biomeStates = (from a in mapping.GetMaps()
			select a.GetComponent<ProceduralStyle>().BiomeState).ToArray();
		if ((bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			ProceduralStyle component = EnvironmentLoader.Instance.EnvironmentRoot.GetComponent<ProceduralStyle>();
			SetupAudioFX(maps, centers, biomes, themes, tones, biomeStates, component);
		}
	}

	private void SetupAudioFX(ProceduralMapTile[] maps, TileBehaviour[] centers, ScenarioPossibleRoom.ESubBiome[] biomes, ScenarioPossibleRoom.ETheme[] themes, ScenarioPossibleRoom.ETone[] tones, ScenarioPossibleRoom.EBiomeState[] biomeStates, ProceduralStyle rootStyle)
	{
		string[] allAudioKeys = SingletonMonoBehaviour<AudioController>.Instance.GetAllAudioKeys();
		for (int i = 0; i < centers.Length; i++)
		{
			if (biomes[i] == ScenarioPossibleRoom.ESubBiome.Inherit)
			{
				biomes[i] = rootStyle.SubBiome;
			}
			if (themes[i] == ScenarioPossibleRoom.ETheme.Inherit)
			{
				themes[i] = rootStyle.Theme;
			}
			if (tones[i] == ScenarioPossibleRoom.ETone.Inherit)
			{
				tones[i] = rootStyle.Tone;
			}
			if (biomeStates[i] == ScenarioPossibleRoom.EBiomeState.Inherit)
			{
				biomeStates[i] = rootStyle.BiomeState;
			}
			string value = Enum.GetName(typeof(ScenarioPossibleRoom.EBiomeState), biomeStates[i]);
			if (biomeStates[i] == ScenarioPossibleRoom.EBiomeState.Natural)
			{
				value = (((double)WorldSeasonState.LastSeasonState.Temperature > 0.5) ? "Summer" : "Winter");
			}
			else if (biomeStates[i] == ScenarioPossibleRoom.EBiomeState.EternalSummer)
			{
				value = "Summer";
			}
			else if (biomeStates[i] == ScenarioPossibleRoom.EBiomeState.EternalWinter)
			{
				value = "Winter";
			}
			string value2 = Enum.GetName(typeof(ScenarioPossibleRoom.ESubBiome), biomes[i]);
			string value3 = Enum.GetName(typeof(ScenarioPossibleRoom.ETheme), themes[i]);
			string value4 = Enum.GetName(typeof(ScenarioPossibleRoom.ETone), tones[i]);
			string text = null;
			string[] tieredMatch = new string[13];
			string[] array = allAudioKeys;
			foreach (string text2 in array)
			{
				bool flag = text2.Contains(value2);
				bool flag2 = text2.Contains(value3);
				bool flag3 = text2.Contains(value4);
				bool flag4 = text2.Contains(value);
				if (flag & flag2 & flag3 & flag4)
				{
					tieredMatch[0] = text2;
					break;
				}
				if (flag & flag2 & flag4)
				{
					SetTier(1, text2);
				}
				if (flag & flag2 & flag3)
				{
					SetTier(2, text2);
				}
				if (flag & flag3 & flag4)
				{
					SetTier(3, text2);
				}
				if (flag2 & flag3 & flag4)
				{
					SetTier(4, text2);
				}
				if (flag & flag2)
				{
					SetTier(5, text2);
				}
				if (flag & flag3)
				{
					SetTier(6, text2);
				}
				if (flag2 & flag3)
				{
					SetTier(7, text2);
				}
				if (flag & flag4)
				{
					SetTier(8, text2);
				}
				if (flag2 & flag4)
				{
					SetTier(9, text2);
				}
				if (flag3)
				{
					SetTier(10, text2);
				}
				if (flag2)
				{
					SetTier(11, text2);
				}
				if (flag)
				{
					SetTier(12, text2);
				}
			}
			for (int k = 0; k < tieredMatch.Length; k++)
			{
				if (!string.IsNullOrEmpty(tieredMatch[k]))
				{
					text = tieredMatch[k];
					break;
				}
			}
			if (text == null)
			{
				text = "DefaultAmbient";
			}
			_mapAmbientDescriptors[i] = text;
			if (!_ambienceAudio.ContainsKey(text))
			{
				AudioObject audioObject = AudioController.Play(text, centers[i].m_ClientTile.m_GameObject.transform);
				if ((bool)audioObject && maps[i].GetVisibility() == ProceduralMapTile.Visibility.Unrevealed)
				{
					audioObject.volume = 0f;
				}
				_ambienceAudio[_mapAmbientDescriptors[i]] = audioObject;
			}
			else if (_ambienceAudio[text] != null && maps[i].GetVisibility() == ProceduralMapTile.Visibility.Revealed)
			{
				_ambienceAudio[text].volume = 1f;
			}
			void SetTier(int tier, string text3)
			{
				if (string.IsNullOrEmpty(tieredMatch[tier]))
				{
					tieredMatch[tier] = text3;
				}
			}
		}
	}

	public void UpdateAmbientAudioPlayers()
	{
		if (_mapTiles == null)
		{
			return;
		}
		foreach (string key in _ambienceAudio.Keys)
		{
			if (_ambienceAudio[key] == null)
			{
				continue;
			}
			float num = 0f;
			for (int i = 0; i < _mapTiles.Length; i++)
			{
				if (!(_mapAmbientDescriptors[i] == key) || _mapTiles[i].GetVisibility() != ProceduralMapTile.Visibility.Revealed || !_mapTiles[i].IsShapeDefined())
				{
					continue;
				}
				Vector3[] array = (from v in _mapTiles[i].GetShape()
					select new Vector3(v.x, 0f, v.y)).ToArray();
				float num2 = array.Length;
				float num3 = 0f;
				Vector3 zero = Vector3.zero;
				Vector3[] array2 = array;
				foreach (Vector3 vector in array2)
				{
					zero += vector;
					Vector3 vector2 = CameraController.s_CameraController.m_Camera.WorldToViewportPoint(vector);
					if (vector2.x > _camSpaceBoundMin && vector2.x < _camSpaceBoundMax && vector2.y > _camSpaceBoundMin && vector2.y < _camSpaceBoundMax && vector2.z > 0f)
					{
						num3++;
					}
				}
				num += ((num3 > 0f) ? 0.5f : 0f);
				num += num3 / num2;
				zero /= num2;
				Vector3 vector3 = CameraController.s_CameraController.m_Camera.WorldToViewportPoint(zero);
				if (vector3.x > _camSpaceBoundMin && vector3.x < _camSpaceBoundMax && vector3.y > _camSpaceBoundMin && vector3.y < _camSpaceBoundMax)
				{
					num++;
				}
			}
			num = Mathf.Clamp01(num);
			float num5 = 0.7f * ((CameraController.s_CameraController.Zoom - CameraController.s_CameraController.TotalMinimumFOV) / (CameraController.s_CameraController.m_DefaultFOV - CameraController.s_CameraController.TotalMinimumFOV));
			num *= 1f - num5;
			num *= 0.01f * (float)SaveData.Instance.Global.AmbientVolume;
			_ambienceAudio[key].volume = Mathf.Lerp(_ambienceAudio[key].volume, num, Time.deltaTime * 0.5f);
		}
	}

	private void InitFoW(IEnumerable<ProceduralMapTile> maps)
	{
		if ((bool)MultiplaneSystem.GetInstance())
		{
			MultiplaneSystem.GetInstance().Init();
			Bounds visualsBounds = GetVisualsBounds(maps);
			if (KronosFogOfWar == null)
			{
				KronosFogOfWar = new KronosFogOfWar();
			}
			KronosFogOfWar.Init(MultiplaneSystem.GetInstance(), maps, visualsBounds, enabled: true);
		}
	}

	public static Bounds GetVisualsBounds(IEnumerable<ProceduralMapTile> maps)
	{
		Bounds result = default;
		MeshRenderer[] array = maps.SelectMany((ProceduralMapTile a) => a.GetComponentsInChildren<MeshRenderer>()).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			MeshRenderer meshRenderer = array[num];
			if (num == 0)
			{
				result = meshRenderer.bounds;
			}
			else
			{
				result.Encapsulate(meshRenderer.bounds);
			}
		}
		return result;
	}

	private static IEnumerable<GameObject> ChildrenOf(GameObject o)
	{
		if (o != null)
		{
			for (int i = 0; i < o.transform.childCount; i++)
			{
				yield return o.transform.GetChild(i).gameObject;
			}
		}
	}

	private static IEnumerable<GameObject> ObjectsOfTypeRecursive(GameObject o, Type component_type, bool continue_inside_match = true)
	{
		bool recurse = true;
		if (o.GetComponent(component_type) != null)
		{
			yield return o;
			if (!continue_inside_match)
			{
				recurse = false;
			}
		}
		if (!recurse)
		{
			yield break;
		}
		for (int i = 0; i < o.transform.childCount; i++)
		{
			GameObject o2 = o.transform.GetChild(i).gameObject;
			foreach (GameObject item in ObjectsOfTypeRecursive(o2, component_type, continue_inside_match))
			{
				yield return item;
			}
		}
	}

	internal void OnMapTileVisibilityChanged(ProceduralMapTile proceduralMapTile, ProceduralMapTile.Visibility currentVisibility)
	{
		foreach (ProceduralMapTile mapTile in MapTiles)
		{
			if (mapTile != proceduralMapTile)
			{
				mapTile.MarkVisibilityDirty();
			}
		}
	}

	internal void RefreshAmbience()
	{
		ProceduralMapTile[] mapTiles = _mapTiles;
		AmbienceDiscriminator discriminator = AmbienceDiscriminator;
		discriminator.Clear();
		ProceduralMapTile[] array = mapTiles;
		foreach (ProceduralMapTile proceduralMapTile in array)
		{
			if (proceduralMapTile.GetVisibility() == ProceduralMapTile.Visibility.Revealed)
			{
				RefreshMap(proceduralMapTile);
			}
		}
		array = mapTiles;
		foreach (ProceduralMapTile proceduralMapTile2 in array)
		{
			if (proceduralMapTile2.GetVisibility() == ProceduralMapTile.Visibility.Unrevealed)
			{
				RefreshMap(proceduralMapTile2);
			}
		}
		void RefreshMap(ProceduralMapTile map)
		{
			map.InitLighting(discriminator);
			map.MarkVisibilityDirty();
		}
	}
}

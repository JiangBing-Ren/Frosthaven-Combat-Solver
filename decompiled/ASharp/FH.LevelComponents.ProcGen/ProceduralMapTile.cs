using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Apparance.Net;
using FH.ApparanceBaking;
using FH.LevelComponents.EnvironmentFX;
using FH.Misc;
using FH.PostProcessing;
using FH.SeasonTech;
using FH.Tools;
using Libs.GameConsole;
using Libs.Utils;
using MeshSlicing;
using SETUtil;
using SETUtil.Common.Extend;
using SETUtil.Extend;
using SETUtil.Functional.Extend;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace FH.LevelComponents.ProcGen;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
[ExecuteAlways]
public class ProceduralMapTile : ProceduralTileObserver, IMapShapeBrush, IRoomVisibility
{
	public enum Visibility
	{
		Unrevealed,
		Revealed
	}

	public struct StreamReconstructionData
	{
		public Color[] Colors;

		public UnityEngine.Vector3[] Vertices;

		public bool Error;

		internal static StreamReconstructionData FromMesh(Mesh mesh)
		{
			int num = mesh.vertices.Length;
			UnityEngine.Vector3[] array = new UnityEngine.Vector3[num];
			Array.Copy(mesh.vertices, array, num);
			Color[] array2 = new Color[num];
			if (mesh.colors.Length != 0 && mesh.colors.Length == num)
			{
				Array.Copy(mesh.colors, array2, num);
			}
			return new StreamReconstructionData
			{
				Vertices = array,
				Colors = array2
			};
		}

		internal Mesh ToMeshStream()
		{
			return new Mesh
			{
				vertices = Vertices,
				colors = Colors
			};
		}
	}

	private class MeshModificationHandle<T>
	{
		public MeshFilter Filter;

		public MeshRenderer Renderer;

		public Task<T> Task;

		public bool IsComplete => Task?.IsCompleted ?? false;
	}

	public enum RendererTypeContext
	{
		Tiles,
		Walls,
		Props,
		Bays,
		Pillars,
		Character,
		Doors
	}

	private struct RendererContext
	{
		public RendererTypeContext RendererTypeContext;

		public (UnityEngine.Vector2 start, UnityEngine.Vector2 end)[] Edges;

		public VerticalMeshSlicer VerticalMeshSlicer;

		public SliceFillerGenerationSettingsDef SliceFillerSettingsDef;
	}

	public static HashSet<string> RevealedRooms = new HashSet<string>();

	[SerializeField]
	private List<UnityEngine.Vector2> shape2d = new List<UnityEngine.Vector2>(10);

	private Matrix4x4? localToWorldMatrixCached;

	private Matrix4x4? worldToLocalMatrixCached;

	[Tooltip("Test mode to show wall ends and successful corner joins")]
	public bool showWallConnections;

	public bool makeIntoPrefabs;

	public bool disableFogOfWarMask;

	private Dictionary<UnityEngine.Object, UnityEngine.Object> _runtimeResources = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private ProceduralWallAnalyser wallAnalyser;

	private ProceduralScenario _proceduralScenarioParent;

	public static bool ContentGenerationActive;

	public static bool DebugDisablePostProcessing = false;

	private const bool BlockPostProcessingLocal = false;

	private Visibility? lastVisibility;

	private HashSet<Ambience> _ambiences = new HashSet<Ambience>();

	private Dictionary<Renderer, MeshModificationHandle<StreamReconstructionData>> _vtxPaintHandles = new Dictionary<Renderer, MeshModificationHandle<StreamReconstructionData>>();

	private const float _vertexPaintedEdgesFalloff = 2f;

	private static bool _synchronousVertexPaint;

	private AmbienceDiscriminator.Differentiation _ambienceDifferentiation;

	private HashSet<Renderer> done = new HashSet<Renderer>();

	private bool _postVisibilityDirty;

	private const string GENERATED_CONTENT_NAME = "Generated Content";

	private SmartCoroutine _currentPostProcessingCo;

	public static bool IsInLevelEditorMode
	{
		get
		{
			if (IsApplicationPlaying && (bool)SaveData.Instance)
			{
				GlobalData global = SaveData.Instance.Global;
				if (global == null)
				{
					return false;
				}
				return global.GameMode == EGameMode.LevelEditor;
			}
			return false;
		}
	}

	public static bool IsApplicationPlaying => Application.isPlaying;

	public bool IsVertexPaintingComplete => _vtxPaintHandles.Count == 0;

	public bool ContentComplete { get; private set; }

	private IEnumerable<GameObject> AllWalls
	{
		get
		{
			GameObject gameObject = base.gameObject.FindInChildren("Walls", includeInactive: true);
			if (!(gameObject != null))
			{
				yield break;
			}
			foreach (ProceduralWall item in gameObject.FindInImmediateChildren<ProceduralWall>())
			{
				yield return item.gameObject;
			}
		}
	}

	private IEnumerable<ProceduralDoorway> AllDoors => gameObject.GetComponentsInChildren<ProceduralDoorway>();

	private bool _blockPostProcessing
	{
		get
		{
			if (!EnvironmentLoader.ReducedPostProcessing)
			{
				_ = IsApplicationPlaying;
				return DebugDisablePostProcessing;
			}
			return true;
		}
	}

	public bool HasSetInitialVisibility => lastVisibility.HasValue;

	public Ambience Ambience => _ambiences.FirstOrDefault((Ambience a) => a != null && a.isActiveAndEnabled);

	public IEnumerable<GameObject> ConnectedDoors
	{
		get
		{
			foreach (GameObject doorway in GetScenario().Doorways)
			{
				ProceduralDoorway component = doorway.GetComponent<ProceduralDoorway>();
				if (component.GetFrontMapTile() == this || component.GetBackMapTile() == this)
				{
					yield return component.gameObject;
				}
			}
		}
	}

	public IEnumerable<(ProceduralDoorway door, ProceduralMapTile room)> ConnectedRooms
	{
		get
		{
			ProceduralScenario scenario = GetScenario();
			if (scenario == null)
			{
				yield break;
			}
			foreach (GameObject doorway in scenario.Doorways)
			{
				ProceduralDoorway component = doorway.GetComponent<ProceduralDoorway>();
				if (component.GetFrontMapTile() == this)
				{
					yield return (door: component, room: component.GetBackMapTile());
				}
				else if (component.GetBackMapTile() == this)
				{
					yield return (door: component, room: component.GetFrontMapTile());
				}
			}
		}
	}

	private bool FadeIsAvailable => IsApplicationPlaying;

	private bool SliceIsAvailable
	{
		get
		{
			if (FadeIsAvailable)
			{
				return !IsInLevelEditorMode;
			}
			return false;
		}
	}

	public static event Action ContentGenerationStarted;

	public bool IsShapeDefined()
	{
		return shape2d.Count > 2;
	}

	public List<UnityEngine.Vector2> GetShape()
	{
		return shape2d.Select((UnityEngine.Vector2 a) => LocalToWorld(a)).ToList();
	}

	private UnityEngine.Vector2 LocalToWorld(UnityEngine.Vector2 p)
	{
		if (IsInLevelEditorMode)
		{
			localToWorldMatrixCached = transform.localToWorldMatrix;
		}
		Matrix4x4? matrix4x = localToWorldMatrixCached;
		Matrix4x4 matrix4x3;
		if (!matrix4x.HasValue)
		{
			Matrix4x4? matrix4x2 = (localToWorldMatrixCached = transform.localToWorldMatrix);
			matrix4x3 = matrix4x2.Value;
		}
		else
		{
			matrix4x3 = matrix4x.GetValueOrDefault();
		}
		Matrix4x4 matrix4x4 = matrix4x3;
		UnityEngine.Vector3 vector = matrix4x4.MultiplyPoint3x4(new UnityEngine.Vector3(p.x, 0f, p.y));
		return new UnityEngine.Vector2(vector.x, vector.z);
	}

	private UnityEngine.Vector2 WorldToLocal(UnityEngine.Vector2 p)
	{
		if (IsInLevelEditorMode)
		{
			worldToLocalMatrixCached = transform.worldToLocalMatrix;
		}
		Matrix4x4? matrix4x = worldToLocalMatrixCached;
		Matrix4x4 matrix4x3;
		if (!matrix4x.HasValue)
		{
			Matrix4x4? matrix4x2 = (worldToLocalMatrixCached = transform.worldToLocalMatrix);
			matrix4x3 = matrix4x2.Value;
		}
		else
		{
			matrix4x3 = matrix4x.GetValueOrDefault();
		}
		Matrix4x4 matrix4x4 = matrix4x3;
		UnityEngine.Vector3 vector = matrix4x4.MultiplyPoint3x4(new UnityEngine.Vector3(p.x, 0f, p.y));
		return new UnityEngine.Vector2(vector.x, vector.z);
	}

	[ContextMenu("Update Shape From Walls Random")]
	public void UpdateShapeFromWallsRandom()
	{
		List<ProceduralWallAnalyser.WallInfo> list = Tools_GatherAnalysisWallInfo();
		if (list.Count > 0)
		{
			ProceduralWallAnalyser.WallInfo item = list.SelectRandom();
			list.Remove(item);
			list.Insert(0, item);
			UpdateShapeFromWallsInternal(list);
		}
	}

	[ContextMenu("Update Shape From Walls")]
	public void UpdateShapeFromWalls()
	{
		List<ProceduralWallAnalyser.WallInfo> walls = Tools_GatherAnalysisWallInfo();
		UpdateShapeFromWallsInternal(walls);
	}

	private void UpdateShapeFromWallsInternal(List<ProceduralWallAnalyser.WallInfo> walls)
	{
		shape2d.Clear();
		HashSet<ProceduralWallAnalyser.WallInfo> hashSet = new HashSet<ProceduralWallAnalyser.WallInfo>();
		UnityEngine.Vector3 vector = UnityEngine.Vector2.zero;
		foreach (ProceduralWallAnalyser.WallInfo wall in walls)
		{
			var (vector2, vector3) = GetLeftRightCorners(wall);
			if (hashSet.Count == 0)
			{
				shape2d.Add(WorldToLocal(new UnityEngine.Vector2(vector2.x, vector2.z)));
				shape2d.Add(WorldToLocal(new UnityEngine.Vector2(vector3.x, vector3.z)));
				vector = vector3;
				hashSet.Add(wall);
			}
			foreach (ProceduralWallAnalyser.WallInfo wall2 in walls)
			{
				if (!hashSet.Contains(wall2))
				{
					var (vector4, vector5) = GetLeftRightCorners(wall2);
					if ((vector4 - vector).sqrMagnitude < 1f)
					{
						shape2d.Add(WorldToLocal(new UnityEngine.Vector2(vector5.x, vector5.z)));
						vector = vector5;
						hashSet.Add(wall2);
					}
					else if ((vector5 - vector).sqrMagnitude < 1f)
					{
						shape2d.Add(WorldToLocal(new UnityEngine.Vector2(vector4.x, vector4.z)));
						vector = vector4;
						hashSet.Add(wall2);
					}
				}
			}
		}
		static (UnityEngine.Vector3 left, UnityEngine.Vector3 right) GetLeftRightCorners(ProceduralWallAnalyser.WallInfo w)
		{
			Matrix4x4 localToWorldMatrix = w.boxBounds.transform.localToWorldMatrix;
			UnityEngine.Vector3 vector6 = w.boxBounds.size / 2f;
			UnityEngine.Vector3 item = localToWorldMatrix.MultiplyPoint3x4(new UnityEngine.Vector3(vector6.x, vector6.y, 0f - vector6.z));
			UnityEngine.Vector3 item2 = localToWorldMatrix.MultiplyPoint3x4(new UnityEngine.Vector3(0f - vector6.x, vector6.y, 0f - vector6.z));
			return (left: item, right: item2);
		}
	}

	public Visibility GetVisibility()
	{
		if (IsInLevelEditorMode)
		{
			return Visibility.Revealed;
		}
		if (RoomVisibilityTracker.RunningAutomationTests)
		{
			return Visibility.Revealed;
		}
		TileBehaviour roomVisibilityCentralTile = GetRoomVisibilityCentralTile();
		if (roomVisibilityCentralTile.m_ClientTile?.m_Tile?.m_HexMap != null)
		{
			if (!roomVisibilityCentralTile.m_ClientTile.m_Tile.m_HexMap.RevealedAndNotDestroyed)
			{
				return Visibility.Unrevealed;
			}
			return Visibility.Revealed;
		}
		return Visibility.Unrevealed;
	}

	private TileBehaviour GetRoomVisibilityCentralTile()
	{
		TileBehaviour tileBehaviour = GetComponent<RoomVisibilityTracker>().CentralTile;
		if (!tileBehaviour)
		{
			tileBehaviour = GetComponentInChildren<TileBehaviour>(includeInactive: true);
		}
		return tileBehaviour;
	}

	public void WhosYourDaddy(ProceduralScenario parent)
	{
		_proceduralScenarioParent = parent;
	}

	public static void RepopulateRevealedRooms(ScenarioState state)
	{
		RevealedRooms = new HashSet<string>();
		foreach (CMap map in state.Maps)
		{
			if (map.Revealed)
			{
				RevealedRooms.Add(map.MapGuid);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if ((bool)_proceduralScenarioParent)
		{
			Visibility visibility = GetVisibility();
			if (lastVisibility != visibility || _postVisibilityDirty)
			{
				if (lastVisibility != visibility)
				{
					_proceduralScenarioParent.OnMapTileVisibilityChanged(this, visibility);
				}
				ApplyVisibility(_proceduralScenarioParent.KronosFogOfWar);
				lastVisibility = visibility;
				_postVisibilityDirty = false;
				if (visibility == Visibility.Revealed)
				{
					TileBehaviour roomVisibilityCentralTile = GetRoomVisibilityCentralTile();
					string item = null;
					if ((bool)roomVisibilityCentralTile)
					{
						item = roomVisibilityCentralTile.m_ClientTile?.m_Tile?.m_HexMap.MapGuid;
					}
					if ((bool)roomVisibilityCentralTile && !RevealedRooms.Contains(item))
					{
						UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.RoomRevealed, null, (roomVisibilityCentralTile.m_ClientTile?.m_Tile?.m_HexMap == null) ? string.Empty : roomVisibilityCentralTile.m_ClientTile.m_Tile.m_HexMap.MapGuid));
						RevealedRooms.Add(item);
					}
				}
			}
		}
		if (showWallConnections != (wallAnalyser != null))
		{
			if (showWallConnections)
			{
				wallAnalyser = new ProceduralWallAnalyser();
			}
			else
			{
				wallAnalyser = null;
			}
		}
		UpdateVertexPaintHandles();
	}

	public void MarkVisibilityDirty()
	{
		_postVisibilityDirty = true;
	}

	private void UpdateVertexPaintHandles()
	{
		if (_vtxPaintHandles.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<Renderer, MeshModificationHandle<StreamReconstructionData>> item in _vtxPaintHandles.Where((KeyValuePair<Renderer, MeshModificationHandle<StreamReconstructionData>> a) => (bool)a.Key && a.Value.IsComplete))
		{
			MeshModificationHandle<StreamReconstructionData> value = item.Value;
			StreamReconstructionData result = value.Task.Result;
			if (result.Error)
			{
				Debug.LogError($"Vertex calculation error was detected while reconstructing vertex stream of {value.Renderer}");
				continue;
			}
			Mesh mesh = result.ToMeshStream();
			value.Renderer.additionalVertexStreams = mesh;
			TrackDynamicResource(value.Renderer, mesh);
		}
		if (_vtxPaintHandles.All((KeyValuePair<Renderer, MeshModificationHandle<StreamReconstructionData>> a) => !a.Key || a.Value.IsComplete))
		{
			_vtxPaintHandles.Clear();
		}
	}

	private bool ApplyVisibility(KronosFogOfWar fow)
	{
		bool flag = GetVisibility() == Visibility.Revealed;
		ShowProps(flag);
		EnableLightsAndEffects(gameObject, flag);
		if (_ambienceDifferentiation != null && Ambience != null)
		{
			ApplyAmbienceDiscrimination(_ambienceDifferentiation);
			foreach (ProceduralDoorway allDoor in AllDoors)
			{
				ProceduralMapTile frontMapTile = allDoor.GetFrontMapTile();
				if ((bool)frontMapTile && frontMapTile._ambienceDifferentiation == null)
				{
					return false;
				}
				RenderingLayer renderingLayer = (((bool)frontMapTile && frontMapTile.GetVisibility() == Visibility.Revealed) ? frontMapTile._ambienceDifferentiation.Layer : AmbienceDiscriminator.UnrevealedLayer);
				ProceduralMapTile backMapTile = allDoor.GetBackMapTile();
				if ((bool)backMapTile && backMapTile._ambienceDifferentiation == null)
				{
					return false;
				}
				RenderingLayer layer;
				if ((bool)backMapTile)
				{
					layer = ((backMapTile.GetVisibility() == Visibility.Revealed) ? backMapTile._ambienceDifferentiation.Layer : AmbienceDiscriminator.UnrevealedLayer);
				}
				else
				{
					layer = renderingLayer;
				}
				RenderingLayer visibility = renderingLayer.Merge(layer);
				allDoor.SetVisibility(visibility);
			}
			try
			{
				if (IsApplicationPlaying)
				{
					gameObject.GetComponentsInChildren<PropSpecialCaseAdjuster2000>(includeInactive: true).ForEach((PropSpecialCaseAdjuster2000 a) =>
					{
						a.AdjustLighting();
					});
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			GameObject.Find("Light").And((GameObject x) => x.transform.parent == null).Then((GameObject x) => x.GetComponent<Light>())
				.And((Light x) => x.type == LightType.Directional)
				.Then((Light x) =>
				{
					x.enabled = false;
				})
				.Then((Light x) =>
				{
					x.gameObject.SetActive(value: false);
				});
			fow?.OnMapUpdate();
			UpdateReflectionProbes(2);
			return true;
		}
		return false;
	}

	private void ShowProps(bool show)
	{
		CMap cMap = GetRoomVisibilityCentralTile().m_ClientTile?.m_Tile?.m_HexMap;
		if (cMap == null)
		{
			return;
		}
		foreach (CObjectProp prop in cMap.Props)
		{
			if (prop.SkipVisuals)
			{
				GameObject gameObject = EnvironmentLoader.Instance.GetProp(prop.PropGuid);
				if (!gameObject)
				{
					gameObject = base.gameObject.FindInChildren(prop.InstanceName, includeInactive: true);
				}
				if ((bool)gameObject)
				{
					Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = false;
					}
				}
				continue;
			}
			GameObject gameObject2 = base.gameObject.FindInChildren(prop.InstanceName);
			if (gameObject2 == null || gameObject2.name.Contains("_TurnedOff", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			OutlineWrapper componentInChildren = gameObject2.GetComponentInChildren<OutlineWrapper>();
			if (prop.ObjectType.GetName().Contains("Door"))
			{
				if (componentInChildren != null)
				{
					ProceduralDoorway componentInChildren2 = gameObject2.GetComponentInChildren<ProceduralDoorway>();
					ProceduralMapTile frontMapTile = componentInChildren2.GetFrontMapTile();
					bool flag = (object)frontMapTile != null && frontMapTile.GetVisibility() == Visibility.Revealed;
					ProceduralMapTile backMapTile = componentInChildren2.GetBackMapTile();
					bool flag2 = (object)backMapTile != null && backMapTile.GetVisibility() == Visibility.Revealed;
					componentInChildren.SetRevealState(flag | flag2);
				}
				continue;
			}
			componentInChildren?.SetRevealState(show);
			if (!gameObject2.TryGetComponent<PropParent>(out var component))
			{
				component = gameObject2.AddComponent<PropParent>();
				component.Init(placedBeforehand: false);
			}
			component.SetPropHoverable(show);
			if (ApparanceUtils.PropNeedsPlaceholder(prop, gameObject2, out var placeholderPositions) && component.PlaceholderCount < placeholderPositions.Count)
			{
				foreach (UnityEngine.Vector3 item in placeholderPositions)
				{
					if (TryFindClosestTile(item, out var closestTile))
					{
						component.AddPlaceholder(closestTile, item, component.transform.rotation);
					}
				}
			}
			component.SetPropVisibility(show, Ambience);
		}
	}

	private bool TryFindClosestTile(UnityEngine.Vector3 position, out GameObject closestTile)
	{
		bool result = false;
		closestTile = null;
		float num = float.MaxValue;
		foreach (GameObject tile in EnvironmentLoader.Instance.GetTiles())
		{
			if (!(tile == null) && IsValidHexTile(tile))
			{
				float sqrMagnitude = (tile.transform.position - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					closestTile = tile;
					num = sqrMagnitude;
					result = true;
				}
			}
		}
		return result;
		static bool IsValidHexTile(GameObject tile)
		{
			if (!tile.name.Contains("VisualEffect"))
			{
				return !tile.name.Contains("Portal");
			}
			return false;
		}
	}

	public static void EnableLightsAndEffects(GameObject o, bool enable)
	{
		if (!(o != null))
		{
			return;
		}
		Light[] componentsInChildren = o.GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren)
		{
			if (!light.name.Contains("LightingTransitionLight"))
			{
				light.enabled = enable;
			}
		}
		ParticleSystem[] componentsInChildren2 = o.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enableEmission = enable;
		}
	}

	[ContextMenu("Notify Content Placement Complete")]
	public override void NotifyContentPlacementComplete()
	{
		GameObject gameObject = base.gameObject.FindInChildren("Generated Content", includeInactive: true);
		if (gameObject != null)
		{
			gameObject.layer = 10;
			foreach (Transform item in gameObject.transform)
			{
				item.gameObject.layer = 10;
			}
		}
		if (!ContentGenerationActive)
		{
			Log.Info("[Apparance] Content generation active.");
			ContentGenerationActive = true;
			if (ContentGenerationStarted != null)
			{
				ContentGenerationStarted();
			}
		}
		base.NotifyContentPlacementComplete();
		ContentComplete = true;
	}

	[ContextMenu("Replace With Prefabs")]
	private void ReplaceWithPrefabs()
	{
	}

	[ContextMenu("Remove FoW Mask")]
	private void RemoveFowMask()
	{
		Transform[] array = SceneUtil.CollectAllChildren(base.transform);
		foreach (Transform transform in array)
		{
			if (transform.name == "FoWMask")
			{
				transform.gameObject.SetActive(value: false);
			}
		}
	}

	public override void WriteExtraParameters(ParameterCollection parameters, int next_id)
	{
		ProceduralStyle proceduralStyle = GloomUtility.EnsureComponent<ProceduralStyle>(gameObject);
		ParameterCollection style_struct = parameters.WriteListBegin(next_id++);
		proceduralStyle.WriteStyleParameters(style_struct, GetScenarioSeed(), proceduralStyle.Seed, proceduralStyle.Seed);
		parameters.WriteListEnd();
	}

	public void ResetBeforeLoad()
	{
		lastVisibility = null;
	}

	public void AddAmbience(IEnumerable<Ambience> ambience)
	{
		_ambiences.AddRange(ambience);
	}

	private List<ProceduralWallAnalyser.WallInfo> Tools_GatherAnalysisWallInfo()
	{
		List<ProceduralWallAnalyser.WallInfo> list = new List<ProceduralWallAnalyser.WallInfo>();
		foreach (GameObject allWall in AllWalls)
		{
			Transform transform = allWall.transform;
			BoxCollider component = allWall.GetComponent<BoxCollider>();
			if (component != null)
			{
				Transform transform2 = component.transform;
				Matrix4x4 localToWorld = transform.transform.parent.localToWorldMatrix * transform2.localToWorldMatrix;
				list.Add(new ProceduralWallAnalyser.WallInfo
				{
					wallObject = allWall,
					boxBounds = component,
					boxParent = transform,
					localToWorld = localToWorld
				});
			}
		}
		return list;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		foreach (KeyValuePair<UnityEngine.Object, UnityEngine.Object> runtimeResource in _runtimeResources)
		{
			SceneUtil.SmartDestroy(runtimeResource.Value);
		}
		_runtimeResources.Clear();
	}

	public IEnumerator PostProcessingCo(IMapping mapping)
	{
		if (_blockPostProcessing)
		{
			yield break;
		}
		using RespectFrameTime respect = new RespectFrameTime("ProceduralMapTile.PostProcessingCo");
		if (!Application.isPlaying)
		{
			done.Clear();
		}
		ProceduralScenario component = mapping.EnvironmentRoot.GetComponent<ProceduralScenario>();
		if (IsApplicationPlaying)
		{
			PropSpecialCaseAdjuster2000.OnStartPostProcessMapTile(this);
		}
		if (makeIntoPrefabs)
		{
			ReplaceWithPrefabs();
		}
		if (disableFogOfWarMask)
		{
			RemoveFowMask();
		}
		Dictionary<RendererTypeContext, List<Renderer>> renderersByContext = new Dictionary<RendererTypeContext, List<Renderer>>();
		foreach (object value in Enum.GetValues(typeof(RendererTypeContext)))
		{
			renderersByContext.Add((RendererTypeContext)value, new List<Renderer>());
		}
		List<BoxCollider> wallsInfo = new List<BoxCollider>();
		List<Renderer> sliceableRenderers = new List<Renderer>();
		Transform t = transform;
		VerticalMeshSlicer verticalMeshSlicer = component.VerticalMeshSlicer;
		SliceFillerGenerationSettingsDef sliceFillerSettingsDef = component.SliceFillerSettingsDef;
		OcclusionFadingSettingsDef occlusionFadingSettingsDef = component.OcclusionFadingSettingsDef;
		CleanupEmptyBiomeStates(t);
		SetTilePropPlaceholders(out var newPlaceholders);
		if (newPlaceholders.Count > 0)
		{
			renderersByContext[RendererTypeContext.Tiles].AddRange(newPlaceholders);
			Global.AddRange(done, newPlaceholders);
		}
		if (respect.ShouldWait())
		{
			yield return respect.WaitUntilNextFrameIfNeeded("After setting tile placeholders");
		}
		for (int i = 0; i < t.childCount; i++)
		{
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded($"Processing child {i}");
			}
			Transform childTransform = t.GetChild(i);
			if (childTransform.name.Equals("Generated Content"))
			{
				Renderer[] componentsInChildren = childTransform.GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer item in componentsInChildren)
				{
					if (!done.Contains(item))
					{
						renderersByContext[RendererTypeContext.Tiles].Add(item);
						done.Add(item);
					}
				}
				if (respect.ShouldWait())
				{
					yield return respect.WaitUntilNextFrameIfNeeded("After tiles");
				}
				continue;
			}
			if (childTransform.name.Equals("Walls"))
			{
				for (int wallIndex = 0; wallIndex < childTransform.childCount; wallIndex++)
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded($"Wall {wallIndex}");
					}
					Transform wall = childTransform.GetChild(wallIndex);
					List<Renderer> fadeableWallPieces = new List<Renderer>();
					bool wallCanFade = !occlusionFadingSettingsDef.ShouldExclude(wall.gameObject, RendererTypeContext.Walls);
					bool wallCanSlice = !sliceFillerSettingsDef.ShouldExclude(wall.gameObject);
					wallsInfo.Add(wall.GetComponent<BoxCollider>());
					for (int wallChildIndex = 0; wallChildIndex < wall.childCount; wallChildIndex++)
					{
						if (respect.ShouldWait())
						{
							yield return respect.WaitUntilNextFrameIfNeeded($"Wall Child {wallChildIndex}");
						}
						Transform wallChild = wall.GetChild(wallChildIndex);
						if (!wallChild.name.Equals("Generated Content"))
						{
							continue;
						}
						for (int wallPieceIndex = 0; wallPieceIndex < wallChild.childCount; wallPieceIndex++)
						{
							if (respect.ShouldWait())
							{
								yield return respect.WaitUntilNextFrameIfNeeded($"Wall Piece {wallPieceIndex}");
							}
							Transform child = wallChild.GetChild(wallPieceIndex);
							RendererTypeContext rendererTypeContext = CategorizeObject(child, RendererTypeContext.Walls);
							Renderer[] componentsInChildren2 = child.GetComponentsInChildren<Renderer>(includeInactive: true);
							renderersByContext[rendererTypeContext].AddRange(componentsInChildren2);
							Global.AddRange(done, componentsInChildren2);
							if (rendererTypeContext == RendererTypeContext.Bays && child.TryGetComponent<BoxCollider>(out var component2))
							{
								wallsInfo.Add(component2);
							}
							if (!FadeIsAvailable)
							{
								continue;
							}
							foreach (Renderer renderer in componentsInChildren2)
							{
								if (wallCanFade && !occlusionFadingSettingsDef.ShouldExclude(renderer, rendererTypeContext))
								{
									fadeableWallPieces.Add(renderer);
									if (wallCanSlice && SliceIsAvailable && !sliceFillerSettingsDef.ShouldExclude(renderer))
									{
										sliceableRenderers.Add(renderer);
									}
								}
							}
						}
					}
					if (FadeIsAvailable)
					{
						OccludingRendererGroup orAdd = wall.GetOrAdd<OccludingRendererGroup>();
						orAdd.Setup(-new UnityEngine.Vector2(wall.forward.x, wall.forward.z).normalized, fadeableWallPieces, includeChildRenderers: false);
						orAdd.SetOcclusionActive(active: true);
					}
				}
				continue;
			}
			RendererTypeContext rendererTypeContext2 = CategorizeObject(childTransform, RendererTypeContext.Props);
			Renderer[] componentsInChildren3 = childTransform.GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer2 in componentsInChildren3)
			{
				if (done.Contains(renderer2))
				{
					continue;
				}
				if (rendererTypeContext2 == RendererTypeContext.Doors)
				{
					RendererTypeContext rendererTypeContext3 = CategorizeObject(renderer2.transform, rendererTypeContext2);
					if (rendererTypeContext3 == RendererTypeContext.Tiles)
					{
						renderersByContext[rendererTypeContext3].Add(renderer2);
						done.Add(renderer2);
						continue;
					}
				}
				renderersByContext[rendererTypeContext2].Add(renderer2);
				done.Add(renderer2);
			}
		}
		(UnityEngine.Vector2 start, UnityEngine.Vector2 end)[] edges = FindEdges(wallsInfo);
		foreach (KeyValuePair<RendererTypeContext, List<Renderer>> context in renderersByContext)
		{
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded($"Post Process Renderer Group {context.Key}");
			}
			if (context.Key != RendererTypeContext.Character)
			{
				yield return PostProcessRendererGroup(context.Value, new RendererContext
				{
					RendererTypeContext = context.Key,
					Edges = edges,
					VerticalMeshSlicer = verticalMeshSlicer,
					SliceFillerSettingsDef = sliceFillerSettingsDef
				});
			}
		}
		UnityEngine.Object.FindObjectsOfType<NaMitkoPlo4kite>().ForEach((NaMitkoPlo4kite a) =>
		{
			a.Init();
		});
		SliceRenderers(sliceableRenderers, new RendererContext
		{
			RendererTypeContext = RendererTypeContext.Walls,
			Edges = edges,
			VerticalMeshSlicer = verticalMeshSlicer,
			SliceFillerSettingsDef = sliceFillerSettingsDef
		});
		if (IsApplicationPlaying)
		{
			gameObject.GetComponentsInChildren<PropSpecialCaseAdjuster2000>(includeInactive: true).ForEach((PropSpecialCaseAdjuster2000 a) =>
			{
				a.AdjustProp();
			});
		}
		if (respect.ShouldWait())
		{
			yield return respect.WaitUntilNextFrameIfNeeded("After Adjusting Special Cases 2000");
		}
		if ((bool)EnvironmentLoader.Instance)
		{
			yield return UpdateReflectionProbes(3).WaitForCompletion();
		}
		else if (IsApplicationPlaying)
		{
			throw new Exception("EnvironmentLoader.Instance is null");
		}
		MarkVisibilityDirty();
	}

	private RendererTypeContext CategorizeObject(Transform asset, RendererTypeContext fallback)
	{
		string lower = asset.name.ToLower();
		if (AssetIs("_floor_") || AssetIs("_hex_") || AssetIs("_tile_") || AssetIs("full"))
		{
			return RendererTypeContext.Tiles;
		}
		if (AssetIs("_wall_"))
		{
			return RendererTypeContext.Walls;
		}
		if (AssetIs("_bay_"))
		{
			return RendererTypeContext.Bays;
		}
		if (AssetIs("_pillar_"))
		{
			return RendererTypeContext.Pillars;
		}
		if (AssetIs("corpse") || AssetIs("so_") || AssetIs("mo_") || AssetIs("he_") || AssetIs("bo_") || AssetIs("npc_"))
		{
			return RendererTypeContext.Character;
		}
		if (AssetIs("door"))
		{
			return RendererTypeContext.Doors;
		}
		return fallback;
		bool AssetIs(string name)
		{
			return lower.Contains(name);
		}
	}

	private (UnityEngine.Vector2 start, UnityEngine.Vector2 end)[] FindEdges(List<BoxCollider> wallsInfo)
	{
		List<GameObject> list = ConnectedDoors.ToList();
		(UnityEngine.Vector2, UnityEngine.Vector2)[] array = new (UnityEngine.Vector2, UnityEngine.Vector2)[wallsInfo.Count + list.Count * 2];
		int i;
		for (i = 0; i < wallsInfo.Count; i++)
		{
			BoxCollider boxCollider = wallsInfo[i];
			Transform transform = boxCollider.transform;
			UnityEngine.Vector3 position = transform.position;
			UnityEngine.Vector3 right = transform.right;
			UnityEngine.Vector3 vector = -transform.forward * boxCollider.size.z / 2f;
			UnityEngine.Vector3 vector2 = right * (boxCollider.size.x / 2f);
			UnityEngine.Vector3 vector3 = position - vector2 + vector;
			UnityEngine.Vector3 vector4 = position + vector2 + vector;
			array[i] = (new UnityEngine.Vector2(vector3.x, vector3.z), new UnityEngine.Vector2(vector4.x, vector4.z));
		}
		for (int j = 0; j < list.Count; j++)
		{
			UnityGameEditorDoorProp componentInParent = list[j].GetComponentInParent<UnityGameEditorDoorProp>(includeInactive: true);
			BoxCollider component = componentInParent.GetComponent<BoxCollider>();
			bool flag = ApparanceLayer.IsThinDoor(componentInParent.m_DoorType);
			UnityEngine.Vector3 position2 = component.transform.position;
			UnityEngine.Vector3 vector5 = componentInParent.RightVector();
			UnityEngine.Vector3 vector6 = (flag ? 0.4f : 1f) * -componentInParent.ForwardVector() * component.size.z / 2f;
			UnityEngine.Vector3 vector7 = vector5 * component.size.x;
			UnityEngine.Vector3 vector8 = position2 - vector7;
			UnityEngine.Vector3 vector9 = position2 + vector7;
			UnityEngine.Vector3 vector10 = vector8 + vector6;
			UnityEngine.Vector3 vector11 = vector9 + vector6;
			array[i] = (new UnityEngine.Vector2(vector10.x, vector10.z), new UnityEngine.Vector2(vector11.x, vector11.z));
			i++;
			vector10 = vector8 - vector6;
			vector11 = vector9 - vector6;
			array[i] = (new UnityEngine.Vector2(vector10.x, vector10.z), new UnityEngine.Vector2(vector11.x, vector11.z));
			i++;
		}
		return array;
	}

	private void SetTilePropPlaceholders(out List<Renderer> newPlaceholders)
	{
		newPlaceholders = new List<Renderer>();
		CMap cMap = GetRoomVisibilityCentralTile().m_ClientTile?.m_Tile?.m_HexMap;
		if (cMap == null)
		{
			return;
		}
		foreach (CObjectProp prop in cMap.Props)
		{
			if (prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) == ScenarioManager.EPerPartySizeConfig.Hidden || prop.SkipVisuals)
			{
				continue;
			}
			GameObject gameObject = base.gameObject.FindInChildren(prop.InstanceName);
			if (!gameObject)
			{
				if (IsInLevelEditorMode)
				{
					continue;
				}
				if (!prop.IsBaked)
				{
					ScenarioState scenarioState = SaveData.Instance?.Global?.CurrentCustomLevelData?.ScenarioState;
					if (scenarioState != null && !scenarioState.Props.Any((CObjectProp x) => x.InstanceName == prop.InstanceName))
					{
						continue;
					}
				}
				if (prop.GetTiles().Count > 1 || prop is CObjectDoor)
				{
					gameObject = Choreographer.FindPropGameObject(prop);
					if (gameObject != null)
					{
						continue;
					}
				}
				Log.Error("propGO missing " + prop.InstanceName, base.gameObject);
			}
			else
			{
				if (!ApparanceUtils.PropNeedsPlaceholder(prop, gameObject, out var placeholderPositions))
				{
					continue;
				}
				if (!gameObject.TryGetComponent<PropParent>(out var component))
				{
					component = gameObject.AddComponent<PropParent>();
					component.Init(placedBeforehand: false);
				}
				if (component.PlaceholderCount >= placeholderPositions.Count)
				{
					continue;
				}
				foreach (UnityEngine.Vector3 item in placeholderPositions)
				{
					if (TryFindClosestTile(item, out var closestTile))
					{
						component.AddPlaceholder(closestTile, item, component.transform.rotation);
						IEnumerable<Renderer> placeholderRenderers = component.GetPlaceholderRenderers();
						if (placeholderRenderers != null)
						{
							newPlaceholders.AddRange(placeholderRenderers);
						}
						else
						{
							Log.Error($"GetPlaceholderRenderers() returned null for {component}");
						}
					}
				}
			}
		}
	}

	private static void CleanupEmptyBiomeStates(Transform t)
	{
		if (IsInLevelEditorMode || !Application.isPlaying)
		{
			return;
		}
		BiomeStateBasedToggle[] componentsInChildren = t.GetComponentsInChildren<BiomeStateBasedToggle>(includeInactive: true);
		foreach (BiomeStateBasedToggle biomeStateBasedToggle in componentsInChildren)
		{
			if (biomeStateBasedToggle.transform.childCount > 0)
			{
				continue;
			}
			BiomeStateBasedToggle[] references = biomeStateBasedToggle.GetReferences();
			SceneUtil.SmartDestroy(biomeStateBasedToggle.gameObject);
			BiomeStateBasedToggle[] array = references;
			foreach (BiomeStateBasedToggle biomeStateBasedToggle2 in array)
			{
				if (biomeStateBasedToggle2 != null)
				{
					biomeStateBasedToggle2.CleanUpReferences();
				}
			}
		}
	}

	public void InitLighting(AmbienceDiscriminator _discriminator)
	{
		Transform transform = base.transform.Find("Generated Content").Find("Full");
		if (transform != null)
		{
			foreach (Transform item in transform)
			{
				AddAmbience(item.GetComponentsInChildren<Ambience>(includeInactive: true));
			}
		}
		Visibility visibility = GetVisibility();
		EnableLightsAndEffects(gameObject, visibility == Visibility.Revealed);
		Ambience ambience = Ambience;
		if ((bool)ambience)
		{
			_ambienceDifferentiation = _discriminator.GetLayerDifferentiationFor(ambience);
			ApplyAmbienceDiscrimination(_ambienceDifferentiation);
		}
		if (IsApplicationPlaying)
		{
			TerrainLamp[] componentsInChildren = GetComponentsInChildren<TerrainLamp>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Init();
			}
		}
	}

	private void ApplyAmbienceDiscrimination(AmbienceDiscriminator.Differentiation ambienceDifferentiation)
	{
		Visibility visibility = GetVisibility();
		HashSet<Light> hashSet = new HashSet<Light>();
		HashSet<Light> hashSet2 = new HashSet<Light>();
		ProceduralWall.ResetWallCache();
		foreach (var connectedRoom in ConnectedRooms)
		{
			if (connectedRoom.door == null || connectedRoom.room == null)
			{
				Debug.LogWarning("Skipped configuring lighting transition for door, because it was missing essential components.");
				continue;
			}
			UnityGameEditorDoorProp componentInParent = connectedRoom.door.GetComponentInParent<UnityGameEditorDoorProp>();
			if (componentInParent == null)
			{
				Debug.LogWarning("Skipped configuring lighting transition for door, because it was missing the UnityGameEditorDoorProp component.");
				continue;
			}
			ProceduralMapTile frontMapTile = connectedRoom.door.GetFrontMapTile();
			bool flag = frontMapTile == this;
			bool flag2 = frontMapTile.GetVisibility() == Visibility.Revealed;
			bool flag3 = connectedRoom.door.GetBackMapTile().GetVisibility() == Visibility.Revealed;
			Light[] componentsInChildren = componentInParent.GetComponentsInChildren<Light>(includeInactive: true);
			foreach (Light light in componentsInChildren)
			{
				UnityEngine.Vector3 lhs = light.transform.position - componentInParent.transform.position;
				lhs.y = 0f;
				bool flag4 = UnityEngine.Vector3.Dot(lhs, componentInParent.ForwardVector()) >= 0f;
				if ((!flag4 & flag) || (flag4 && !flag))
				{
					hashSet.Add(light);
				}
				else
				{
					hashSet2.Add(light);
				}
			}
			DoorLightingTransition orAdd = componentInParent.GetOrAdd<DoorLightingTransition>();
			bool invertDirection = flag2;
			if (orAdd.ConfigureLight(Ambience, invertDirection))
			{
				orAdd.SetLightActive(connectedRoom.room.GetVisibility() != visibility);
			}
			if (flag2 | flag3)
			{
				componentInParent.GetOrAdd<EnableCameraFade>();
			}
		}
		HashSet<Renderer> renderers = new HashSet<Renderer>();
		HashSet<Renderer> renderers2 = new HashSet<Renderer>();
		HashSet<Light> hashSet3 = new HashSet<Light>(GetComponentsInChildren<Light>(includeInactive: true));
		foreach (Light item in hashSet)
		{
			hashSet3.Remove(item);
		}
		foreach (Light item2 in hashSet2)
		{
			hashSet3.Add(item2);
		}
		CollectRecursive(Visibility.Revealed, transform, renderers);
		CollectRecursive(Visibility.Unrevealed, transform, renderers2);
		Ambience.Init(ambienceDifferentiation, this);
		SetRenderingLayersENV(ambienceDifferentiation.Layer, renderers, hashSet3);
		SetRenderingLayersENV(AmbienceDiscriminator.UnrevealedLayer, renderers2, new List<Light>());
		static void CollectRecursive(Visibility visibility2, Transform node, HashSet<Renderer> hashSet4)
		{
			IRoomVisibility component = node.GetComponent<IRoomVisibility>();
			if (component != null && component.GetVisibility() != visibility2)
			{
				return;
			}
			MeshRenderer component2 = node.GetComponent<MeshRenderer>();
			if ((bool)component2 && component2.enabled)
			{
				hashSet4.Add(component2);
			}
			foreach (Transform item3 in node)
			{
				CollectRecursive(visibility2, item3, hashSet4);
			}
		}
	}

	private IEnumerator PostProcessRendererGroup(IEnumerable<Renderer> renderers, RendererContext context)
	{
		using RespectFrameTime respect = new RespectFrameTime("ProceduralMapTile_PostProcessRendererGroup");
		List<Renderer> rendererList = renderers.Where((Renderer x) => x != null).ToList();
		List<ProceduralStyle> list = new List<ProceduralStyle>();
		GeneralHelpersRuntime.GetProceduralStylesInParentsRecursive(transform, null, list);
		ScenarioPossibleRoom.EBiomeState? overrideState = ((WorldSeasonState.LastSeasonState.BiomeStateOverride != ScenarioPossibleRoom.EBiomeState.Inherit) ? new ScenarioPossibleRoom.EBiomeState?(WorldSeasonState.LastSeasonState.BiomeStateOverride) : ((ScenarioPossibleRoom.EBiomeState?)null));
		IEnumerable<(ScenarioPossibleRoom.EBiome Biome, ScenarioPossibleRoom.ESubBiome SubBiome, ScenarioPossibleRoom.ETheme Theme, ScenarioPossibleRoom.ESubTheme SubTheme, ScenarioPossibleRoom.ETone Tone, ScenarioPossibleRoom.EBiomeState)> parentStyleElements = list.Select((ProceduralStyle a) => (Biome: a.Biome, SubBiome: a.SubBiome, Theme: a.Theme, SubTheme: a.SubTheme, Tone: a.Tone, overrideState ?? a.BiomeState));
		GetVisibility();
		for (int i = 0; i < rendererList.Count; i++)
		{
			if (respect.ShouldWait())
			{
				yield return respect.WaitUntilNextFrameIfNeeded($"Start post processing renderer {i}");
			}
			Renderer renderer = rendererList[i];
			if (renderer == null)
			{
				continue;
			}
			OptimizeShadows(context, renderer);
			if (!(renderer is MeshRenderer renderer2))
			{
				continue;
			}
			MeshFilter component = renderer.GetComponent<MeshFilter>();
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh.isReadable)
			{
				StreamReconstructionData data = StreamReconstructionData.FromMesh(sharedMesh);
				MeshModificationHandle<StreamReconstructionData> meshModificationHandle = new MeshModificationHandle<StreamReconstructionData>();
				Matrix4x4 mtx = renderer.transform.localToWorldMatrix;
				meshModificationHandle.Renderer = renderer2;
				meshModificationHandle.Filter = component;
				meshModificationHandle.Task = Task.Run(() => PureCalculateVertexColors(context, parentStyleElements, data, mtx));
				if (_synchronousVertexPaint)
				{
					meshModificationHandle.Task.Wait();
				}
				_vtxPaintHandles[renderer] = meshModificationHandle;
			}
		}
	}

	private void SliceRenderers(IEnumerable<Renderer> renderers, RendererContext context)
	{
		foreach (Renderer item in renderers.Where((Renderer x) => x != null).ToList())
		{
			if (item is MeshRenderer)
			{
				MeshFilter component = item.GetComponent<MeshFilter>();
				context.VerticalMeshSlicer.Push(new (MeshRenderer, MeshFilter)[1] { ((MeshRenderer)item, component) }, context.SliceFillerSettingsDef.FillerMaterial);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void OptimizeShadows(RendererContext context, Renderer renderer)
	{
		if (context.RendererTypeContext == RendererTypeContext.Tiles)
		{
			renderer.shadowCastingMode = ShadowCastingMode.Off;
		}
		else
		{
			renderer.staticShadowCaster = true;
		}
	}

	private void TrackDynamicResource(Renderer renderer, Mesh vertexStream)
	{
		if (_runtimeResources.ContainsKey(renderer) && _runtimeResources[renderer] != null)
		{
			SceneUtil.SmartDestroy(_runtimeResources[renderer]);
		}
		_runtimeResources[renderer] = vertexStream;
	}

	private void SetRenderingLayersENV(RenderingLayer layerMask, IEnumerable<Renderer> renderers, IEnumerable<Light> lights)
	{
		_ = IsApplicationPlaying;
		foreach (Renderer renderer in renderers)
		{
			renderer.SetLayer(layerMask);
		}
		foreach (Light light in lights)
		{
			if (!light.name.Contains("LightingTransitionLight", StringComparison.InvariantCulture))
			{
				light.SetLayer(layerMask);
			}
		}
	}

	private StreamReconstructionData PureCalculateVertexColors(RendererContext context, IEnumerable<(ScenarioPossibleRoom.EBiome Biome, ScenarioPossibleRoom.ESubBiome SubBiome, ScenarioPossibleRoom.ETheme Theme, ScenarioPossibleRoom.ESubTheme SubTheme, ScenarioPossibleRoom.ETone Tone, ScenarioPossibleRoom.EBiomeState BiomeState)> parentStyleElements, StreamReconstructionData data, Matrix4x4 mtx)
	{
		Color[] array = new Color[data.Vertices.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (data.Colors.Length > i)
			{
				array[i] = data.Colors[i];
			}
			array[i].b = PureSelectStyleVertexColor(parentStyleElements);
			if (context.RendererTypeContext == RendererTypeContext.Tiles)
			{
				array[i] = SelectEdgeTileVertexColors(array[i], mtx.MultiplyPoint3x4(data.Vertices[i]), context.Edges);
			}
		}
		return new StreamReconstructionData
		{
			Colors = array,
			Vertices = data.Vertices,
			Error = (array.Length != data.Vertices.Length)
		};
	}

	private Color SelectEdgeTileVertexColors(Color current, UnityEngine.Vector3 vertexWorldPos, (UnityEngine.Vector2 start, UnityEngine.Vector2 end)[] edges)
	{
		int num = -1;
		UnityEngine.Vector2 vector = new UnityEngine.Vector2(vertexWorldPos.x, vertexWorldPos.z);
		UnityEngine.Vector2 b = new UnityEngine.Vector2(float.MaxValue, float.MaxValue);
		float num2 = float.MaxValue;
		for (int i = 0; i < edges.Length; i++)
		{
			UnityEngine.Vector2 normalized = (edges[i].end - edges[i].start).normalized;
			float num3 = UnityEngine.Vector2.Dot(vector - edges[i].start, normalized);
			if (!(num3 < 0f))
			{
				UnityEngine.Vector2 vector2 = edges[i].start + num3 * normalized;
				float sqrMagnitude = (vector2 - vector).sqrMagnitude;
				if (num < 0 || (!(sqrMagnitude > num2) && !((vector2 - edges[i].start).sqrMagnitude > (edges[i].end - edges[i].start).sqrMagnitude)))
				{
					num = i;
					b = vector2;
					num2 = sqrMagnitude;
				}
			}
		}
		if (num >= 0)
		{
			current.r = Math.Clamp(1f - UnityEngine.Vector2.Distance(vector, b) / 2f, 0f, 1f);
		}
		return current;
	}

	private static float PureSelectStyleVertexColor(IEnumerable<(ScenarioPossibleRoom.EBiome Biome, ScenarioPossibleRoom.ESubBiome SubBiome, ScenarioPossibleRoom.ETheme Theme, ScenarioPossibleRoom.ESubTheme SubTheme, ScenarioPossibleRoom.ETone Tone, ScenarioPossibleRoom.EBiomeState BiomeState)> style)
	{
		int num = ((style.FirstOrDefault(((ScenarioPossibleRoom.EBiome Biome, ScenarioPossibleRoom.ESubBiome SubBiome, ScenarioPossibleRoom.ETheme Theme, ScenarioPossibleRoom.ESubTheme SubTheme, ScenarioPossibleRoom.ETone Tone, ScenarioPossibleRoom.EBiomeState BiomeState) a) => a.BiomeState != ScenarioPossibleRoom.EBiomeState.Inherit).BiomeState == ScenarioPossibleRoom.EBiomeState.Natural) ? 1 : 0);
		return num;
	}

	private SmartCoroutine UpdateReflectionProbes(int fullResProbeDelay = 0)
	{
		if (!enabled || !gameObject.activeSelf)
		{
			return null;
		}
		return SmartCoroutine.Start(this, Internal_StartProbeRendering());
		static IEnumerator Internal_DeferredRenderProbeInRes(ReflectionProbe probe, int resolution, int delay = 1)
		{
			yield return new WaitForSeconds(delay);
			if (!(probe == null))
			{
				probe.resolution = resolution;
				probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
				probe.RenderProbe();
			}
		}
		IEnumerator Internal_StartProbeRendering()
		{
			Ambience ambience = Ambience;
			ReflectionProbe[] componentsInChildren = transform.GetComponentsInChildren<ReflectionProbe>();
			foreach (ReflectionProbe reflectionProbe in componentsInChildren)
			{
				if ((bool)ambience && (bool)ambience.reflectionCubemapOverride)
				{
					reflectionProbe.mode = ReflectionProbeMode.Custom;
					reflectionProbe.customBakedTexture = ambience.reflectionCubemapOverride;
				}
				else
				{
					reflectionProbe.cullingMask = reflectionProbe.cullingMask & ~(1 << LayerMask.NameToLayer("Monster")) & ~(1 << LayerMask.NameToLayer("Hero"));
					reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
					bool flag = reflectionProbe.resolution == 16;
					if (fullResProbeDelay > 0 && !flag)
					{
						int resolution = reflectionProbe.resolution;
						reflectionProbe.resolution = 16;
						SmartCoroutine.Start(this, Internal_DeferredRenderProbeInRes(reflectionProbe, resolution, fullResProbeDelay));
					}
					reflectionProbe.RenderProbe();
					yield return null;
				}
			}
		}
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugToggleMaptilePostprocess", Description = "Dont do vertex paint and optimizations etc")]
	public static void DebugToggleMaptilePostprocess(IConsole console)
	{
		DebugDisablePostProcessing = !DebugDisablePostProcessing;
		console.WriteLine((!DebugDisablePostProcessing).ToString());
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugEnableMaptilePostprocess", Description = "Dont do vertex paint and optimizations etc")]
	public static void DebugToggleMaptilePostprocess(IConsole console, bool value)
	{
		DebugDisablePostProcessing = value;
		console.WriteLine((!DebugDisablePostProcessing).ToString());
	}

	[ContextMenu("Clear Renderers")]
	private void ClearRenderers()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].additionalVertexStreams = null;
		}
	}

	[ConsoleCommand(Command = "ToggleSynchrnousVertexPaint")]
	public static void ToggleSynchrnousVertexPaint(IConsole console)
	{
		_synchronousVertexPaint = !_synchronousVertexPaint;
		console.WriteLine(_synchronousVertexPaint.ToString());
	}
}

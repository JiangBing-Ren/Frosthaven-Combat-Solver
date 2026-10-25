using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using Libs.Utils;
using Libs.Utils.Profiler;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Rendering;

public class ClientScenarioManager : MonoBehaviour
{
	public GameObject m_TilePrefab;

	public GameObject m_Board;

	public Material m_PathRendererMaterial;

	public Material m_DarkPathRendererMaterial;

	public static ClientScenarioManager s_ClientScenarioManager;

	private const float k_MovementScalar = 26f;

	private bool m_AStarDisplay;

	private bool m_AIPathDisplay;

	private bool m_LevelFlowDisplay;

	private bool m_LOSDisplay;

	private CClientTile m_LOSSourceTile;

	private CClientTile m_LOSTargetTile;

	private CClientTile[,] m_ClientTileArray;

	private List<GameObject> m_AStarLineRendererGameObjects = new List<GameObject>();

	private List<GameObject> m_AIPathGameObjects = new List<GameObject>();

	private List<GameObject> m_LevelFlowGameObjects = new List<GameObject>();

	private List<GameObject> m_LOSGameObjects = new List<GameObject>();

	private GameObject m_DungeonEntrance;

	private GameObject m_DungeonExit;

	private List<List<CClientTile>> m_PossibleStartingTiles = new List<List<CClientTile>>();

	private List<CClientTile> m_DungeonExitTiles = new List<CClientTile>();

	private List<CClientTile> m_ReachHexTiles = new List<CClientTile>();

	private List<Tuple<CClientTile, Color?>> m_GenericHighlightedTiles = new List<Tuple<CClientTile, Color?>>();

	private List<(CClientTile, CClientTile)> m_GenericHighlightedLinks = new List<(CClientTile, CClientTile)>();

	private Vector3 m_LOSDebugShift1;

	private Vector3 m_LOSDebugShift2;

	private CActor m_CurrentAIPathActor;

	public CClientTile[,] ClientTileArray => m_ClientTileArray;

	public GameObject DungeonEntrance => m_DungeonEntrance;

	public GameObject DungeonExit => m_DungeonExit;

	public List<List<CClientTile>> PossibleStartingTiles => m_PossibleStartingTiles;

	public List<CClientTile> DungeonExitTiles => m_DungeonExitTiles;

	public List<CClientTile> ReachHexTiles => m_ReachHexTiles;

	public List<Tuple<CClientTile, Color?>> GenericHighlightedTiles => m_GenericHighlightedTiles;

	public List<(CClientTile, CClientTile)> GenericHighlightedLinks => m_GenericHighlightedLinks;

	public ScenarioState CurrentState { get; private set; }

	public bool LOSDisplay => m_LOSDisplay;

	public List<CClientTile> AllPossibleStartingTiles
	{
		get
		{
			List<CClientTile> list = new List<CClientTile>();
			foreach (List<CClientTile> possibleStartingTile in m_PossibleStartingTiles)
			{
				list.AddRange(possibleStartingTile.Where((CClientTile cTile) => !cTile.m_Tile.IsBlocked));
			}
			return list;
		}
	}

	private void Awake()
	{
		OnAwakeInit();
	}

	public void OnAwakeInit()
	{
		s_ClientScenarioManager = this;
		enabled = false;
	}

	private void OnDestroy()
	{
		s_ClientScenarioManager = null;
	}

	public static void Create(ScenarioState scenarioState, bool firstLoad)
	{
		using (new MiniProfilerScope("ClientScenarioManager.Create"))
		{
			Transform envRoot = (EnvironmentLoader.Instance.EnvironmentRoot ? EnvironmentLoader.Instance.EnvironmentRoot.transform : EnvironmentLoader.Instance.ApparanceMapsRoot.transform);
			ScenarioManager.Load(scenarioState, firstLoad);
			s_ClientScenarioManager.CurrentState = scenarioState;
			s_ClientScenarioManager.m_ClientTileArray = new CClientTile[ScenarioManager.Width, ScenarioManager.Height];
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CTile cTile = ScenarioManager.Tiles[j, i];
					if (cTile == null)
					{
						continue;
					}
					GameObject gameObject = FindThatActuallyWorks(cTile.m_Hex.TileGuid);
					GameObject gameObject2 = ((gameObject != null) ? gameObject.gameObject : null);
					if (gameObject2 != null)
					{
						CClientTile cClientTile = new CClientTile();
						s_ClientScenarioManager.m_ClientTileArray[j, i] = cClientTile;
						cClientTile.m_GameObject = gameObject2;
						cClientTile.m_TileBehaviour = gameObject2.GetComponent<TileBehaviour>();
						cClientTile.m_Tile = cTile;
						cClientTile.m_TileBehaviour.m_ClientTile = cClientTile;
						if (cTile.m_Hex2 != null)
						{
							cClientTile.m_TileBehaviour1 = (cClientTile.m_GameObject1 = FindThatActuallyWorks(cTile.m_Hex2.TileGuid)).GetComponent<TileBehaviour>();
							cClientTile.m_TileBehaviour1.m_ClientTile = cClientTile;
						}
						if (Application.isPlaying)
						{
							cClientTile.m_TileBehaviour.CreateWorldspaceTileBehaviourUI();
						}
					}
				}
			}
			bool flag = false;
			bool flag2 = false;
			s_ClientScenarioManager.m_PossibleStartingTiles.Clear();
			s_ClientScenarioManager.m_DungeonExitTiles.Clear();
			s_ClientScenarioManager.m_ReachHexTiles.Clear();
			s_ClientScenarioManager.GenericHighlightedTiles.Clear();
			s_ClientScenarioManager.GenericHighlightedLinks.Clear();
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject item in Choreographer.s_Choreographer.AllMaps.Where((GameObject a) => a != null))
			{
				list.AddRange(UnityGameEditorRuntime.FindUnityGameObjects(item, ScenarioManager.ObjectImportType.Tile));
			}
			list = list.Distinct().ToList();
			UnityGameEditorDoorProp[] componentsInChildren = Choreographer.s_Choreographer.m_MapSceneRoot.GetComponentsInChildren<UnityGameEditorDoorProp>();
			if (SaveData.Instance.Global.CurrentCustomLevelData != null && SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes.Count > 0)
			{
				flag = RefreshStartingPositions(scenarioState);
				flag2 = RefreshExitPositions(scenarioState);
			}
			bool num = componentsInChildren.Any((UnityGameEditorDoorProp d) => d.m_IsDungeonEntrance) | flag;
			bool flag3 = ProceduralMapTile.IsInLevelEditorMode && !Choreographer.s_Choreographer.AllMaps.Any();
			UnityGameEditorDoorProp[] array;
			if (!num && !flag3 && Choreographer.s_Choreographer.AllMaps.Count > 0)
			{
				List<GameObject> list2 = new List<GameObject>();
				list2.AddRange(UnityGameEditorRuntime.FindUnityGameObjects(Choreographer.s_Choreographer.AllMaps.First(), ScenarioManager.ObjectImportType.Tile));
				int num2 = 0;
				List<CClientTile> list3 = new List<CClientTile>();
				foreach (GameObject item2 in list2)
				{
					CClientTile clientTile = item2.GetComponent<TileBehaviour>().m_ClientTile;
					CActor cActor = ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex);
					if (cActor == null || cActor.Type == CActor.EType.Player)
					{
						CNode cNode = ScenarioManager.PathFinder.Nodes[clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y];
						if (clientTile.m_Tile.m_Props.Count == 0 && cNode.Walkable && !cNode.Blocked)
						{
							list3.Add(clientTile);
							num2++;
						}
					}
					if (num2 >= 5)
					{
						s_ClientScenarioManager.m_PossibleStartingTiles.Add(list3);
						break;
					}
				}
			}
			else
			{
				array = componentsInChildren;
				foreach (UnityGameEditorDoorProp unityGameEditorDoorProp in array)
				{
					int num4 = 0;
					List<CClientTile> list4 = new List<CClientTile>();
					if (!unityGameEditorDoorProp.m_IsDungeonEntrance)
					{
						continue;
					}
					s_ClientScenarioManager.m_DungeonEntrance = unityGameEditorDoorProp.gameObject;
					if (flag)
					{
						continue;
					}
					List<GameObject> list5 = new List<GameObject>();
					list5.AddRange(UnityGameEditorRuntime.FindUnityGameObjects(unityGameEditorDoorProp.transform.parent.gameObject, ScenarioManager.ObjectImportType.Tile));
					foreach (GameObject item3 in Choreographer.FindValidStartingPositions(list5, s_ClientScenarioManager.m_DungeonEntrance, null))
					{
						CClientTile clientTile2 = item3.GetComponent<TileBehaviour>().m_ClientTile;
						CActor cActor2 = ScenarioManager.Scenario.FindActorAt(clientTile2.m_Tile.m_ArrayIndex);
						if (cActor2 == null || cActor2.Type == CActor.EType.Player)
						{
							CNode cNode2 = ScenarioManager.PathFinder.Nodes[clientTile2.m_Tile.m_ArrayIndex.X, clientTile2.m_Tile.m_ArrayIndex.Y];
							if (clientTile2.m_Tile.m_Props.Count == 0 && cNode2.Walkable && !cNode2.Blocked)
							{
								list4.Add(clientTile2);
								num4++;
							}
						}
						if (num4 >= 5)
						{
							s_ClientScenarioManager.m_PossibleStartingTiles.Add(list4);
							break;
						}
					}
				}
			}
			ScenarioManager.SetStartingTiles(s_ClientScenarioManager.AllPossibleStartingTiles.Select((CClientTile x) => x.m_Tile).ToList());
			array = componentsInChildren;
			foreach (UnityGameEditorDoorProp unityGameEditorDoorProp2 in array)
			{
				if (!unityGameEditorDoorProp2.m_IsDungeonExit)
				{
					continue;
				}
				s_ClientScenarioManager.m_DungeonExit = unityGameEditorDoorProp2.gameObject;
				GameObject gameObject3 = null;
				foreach (GameObject allMap in Choreographer.s_Choreographer.AllMaps)
				{
					if (UnityGameEditorRuntime.FindUnityGameObjects(allMap, ScenarioManager.ObjectImportType.Door).Contains(unityGameEditorDoorProp2.gameObject))
					{
						gameObject3 = allMap;
						break;
					}
				}
				if (flag2 || !(gameObject3 != null))
				{
					continue;
				}
				foreach (GameObject item4 in Choreographer.FindValidStartingPositions(UnityGameEditorRuntime.FindUnityGameObjects(gameObject3, ScenarioManager.ObjectImportType.Tile), s_ClientScenarioManager.m_DungeonExit, null))
				{
					CClientTile clientTile3 = item4.GetComponent<TileBehaviour>().m_ClientTile;
					if (ScenarioManager.Scenario.FindActorAt(clientTile3.m_Tile.m_ArrayIndex) == null)
					{
						CNode cNode3 = ScenarioManager.PathFinder.Nodes[clientTile3.m_Tile.m_ArrayIndex.X, clientTile3.m_Tile.m_ArrayIndex.Y];
						if (clientTile3.m_Tile.m_Props.Count == 0 && cNode3.Walkable && !cNode3.Blocked)
						{
							s_ClientScenarioManager.m_DungeonExitTiles.Add(clientTile3);
						}
					}
					if (s_ClientScenarioManager.m_DungeonExitTiles.Count >= 5)
					{
						break;
					}
				}
			}
			if (firstLoad && !flag2 && s_ClientScenarioManager.m_DungeonExitTiles.Count > 0)
			{
				List<TileIndex> list6 = new List<TileIndex>();
				foreach (CClientTile dungeonExitTile in s_ClientScenarioManager.m_DungeonExitTiles)
				{
					list6.Add(new TileIndex(dungeonExitTile.m_Tile.m_ArrayIndex));
				}
				foreach (CObjective_ActorReachPosition item5 in scenarioState.WinObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorReachPosition))
				{
					item5.ActorTargetPositions.AddRange(list6);
				}
				foreach (CObjective_ActorReachPosition item6 in scenarioState.LoseObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorReachPosition))
				{
					item6.ActorTargetPositions.AddRange(list6);
				}
			}
			s_ClientScenarioManager.enabled = true;
			GameObject FindThatActuallyWorks(string name)
			{
				Transform[] componentsInChildren2 = envRoot.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform in componentsInChildren2)
				{
					if (transform.name == name)
					{
						return transform.gameObject;
					}
				}
				return null;
			}
		}
	}

	public CClientTile GetTile(Point tileIndex)
	{
		return ClientTileArray[tileIndex.X, tileIndex.Y];
	}

	public static bool RefreshStartingPositions(ScenarioState scenarioState)
	{
		s_ClientScenarioManager.m_PossibleStartingTiles.Clear();
		bool result = false;
		Dictionary<CMap, List<CClientTile>> dictionary = new Dictionary<CMap, List<CClientTile>>();
		if (SaveData.Instance.Global.CurrentCustomLevelData.ExplicitlyDontSplitStartingTileIndexesByIslands)
		{
			List<CClientTile> list = new List<CClientTile>();
			foreach (TileIndex startingTileIndex in SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes)
			{
				CTile cTile = ScenarioManager.Tiles[startingTileIndex.X, startingTileIndex.Y];
				if (cTile != null && cTile.m_HexMap.Revealed && !cTile.m_HexMap.Destroyed)
				{
					CClientTile cClientTile = s_ClientScenarioManager.m_ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y];
					if (cClientTile == null)
					{
						Log.Error($"Client starting tile is null on scenario {scenarioState.BakingID} at pos {cTile.m_ArrayIndex}");
					}
					else
					{
						list.Add(cClientTile);
					}
				}
			}
			list.RemoveAll((CClientTile t) => scenarioState.Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle && p.GetConfigForPartySize(scenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && p.ArrayIndex.X == t.m_Tile.m_ArrayIndex.X && p.ArrayIndex.Y == t.m_Tile.m_ArrayIndex.Y) || scenarioState.ActivatedProps.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle && p.GetConfigForPartySize(scenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && p.ArrayIndex.X == t.m_Tile.m_ArrayIndex.X && p.ArrayIndex.Y == t.m_Tile.m_ArrayIndex.Y));
			if (list.Count > 0)
			{
				s_ClientScenarioManager.m_PossibleStartingTiles.Add(list);
				result = true;
			}
			result = true;
		}
		else if (SaveData.Instance.Global.CurrentCustomLevelData.SplitStartingTileIndexesByIslands)
		{
			List<Point> list2 = SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes.Select((TileIndex x) => x.ToPoint()).ToList();
			List<Point> list3 = list2.ToList();
			while (list3.Count > 0)
			{
				Point item = list3[0];
				new List<Point>();
				List<Point> list4 = new List<Point>();
				List<Point> list5 = new List<Point>();
				list4.Add(item);
				while (list4.Count > 0)
				{
					Point point = list4[0];
					list4.RemoveAt(0);
					list5.Add(point);
					list3.Remove(point);
					List<Point> list6 = new List<Point>();
					CPathFinder.GetAdjacentLocations(point, list6);
					foreach (Point item2 in list6)
					{
						if (list2.Contains(item2) && !list5.Contains(item2))
						{
							list4.Add(item2);
						}
					}
				}
				List<CClientTile> list7 = new List<CClientTile>();
				foreach (Point n in list5)
				{
					CClientTile cClientTile2 = s_ClientScenarioManager.m_ClientTileArray[n.X, n.Y];
					if (cClientTile2 != null && !list7.Any((CClientTile x) => x.m_Tile.m_ArrayIndex == n))
					{
						list7.Add(cClientTile2);
					}
				}
				s_ClientScenarioManager.m_PossibleStartingTiles.Add(list7);
				result = true;
			}
		}
		else
		{
			foreach (TileIndex startingTileIndex2 in SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes)
			{
				CTile cTile2 = ScenarioManager.Tiles[startingTileIndex2.X, startingTileIndex2.Y];
				if (cTile2 != null && cTile2.m_HexMap.Revealed && !cTile2.m_HexMap.Destroyed)
				{
					CClientTile cClientTile3 = s_ClientScenarioManager.m_ClientTileArray[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y];
					if (cClientTile3 == null)
					{
						Log.Error($"Client starting tile is null on scenario {scenarioState.BakingID} at pos {cTile2.m_ArrayIndex}");
						continue;
					}
					if (dictionary.TryGetValue(cTile2.m_HexMap, out var value))
					{
						value.Add(cClientTile3);
						continue;
					}
					dictionary.Add(cTile2.m_HexMap, new List<CClientTile> { cClientTile3 });
				}
			}
			foreach (List<CClientTile> value2 in dictionary.Values)
			{
				List<CClientTile> list8 = value2.ToList();
				list8.RemoveAll((CClientTile t) => scenarioState.Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle && p.GetConfigForPartySize(scenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && p.ArrayIndex.X == t.m_Tile.m_ArrayIndex.X && p.ArrayIndex.Y == t.m_Tile.m_ArrayIndex.Y) || scenarioState.ActivatedProps.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle && p.GetConfigForPartySize(scenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && p.ArrayIndex.X == t.m_Tile.m_ArrayIndex.X && p.ArrayIndex.Y == t.m_Tile.m_ArrayIndex.Y));
				if (list8.Count > 0)
				{
					s_ClientScenarioManager.m_PossibleStartingTiles.Add(list8);
					result = true;
				}
			}
		}
		return result;
	}

	public static bool RefreshExitPositions(ScenarioState scenarioState)
	{
		bool result = false;
		s_ClientScenarioManager.m_DungeonExitTiles.Clear();
		s_ClientScenarioManager.m_ReachHexTiles.Clear();
		foreach (CObjective_ActorReachPosition item in scenarioState.WinObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorReachPosition))
		{
			foreach (TileIndex actorTargetPosition in item.ActorTargetPositions)
			{
				CClientTile cClientTile = s_ClientScenarioManager.m_ClientTileArray[actorTargetPosition.X, actorTargetPosition.Y];
				CNode cNode = ScenarioManager.PathFinder.Nodes[actorTargetPosition.X, actorTargetPosition.Y];
				if (cClientTile.m_Tile.Obstacle == null && cNode.Walkable && !cNode.Blocked)
				{
					s_ClientScenarioManager.m_ReachHexTiles.AddNotContained(cClientTile);
					result = true;
				}
			}
		}
		foreach (CObjective_ActorsEscaped item2 in scenarioState.WinObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorsEscaped))
		{
			foreach (TileIndex escapePosition in item2.EscapePositions)
			{
				CClientTile cClientTile2 = s_ClientScenarioManager.m_ClientTileArray[escapePosition.X, escapePosition.Y];
				CNode cNode2 = ScenarioManager.PathFinder.Nodes[escapePosition.X, escapePosition.Y];
				if (cClientTile2.m_Tile.Obstacle == null && cNode2.Walkable && !cNode2.Blocked)
				{
					s_ClientScenarioManager.m_DungeonExitTiles.AddNotContained(cClientTile2);
					result = true;
				}
			}
		}
		return result;
	}

	public void ToggleLevelFlowDisplay()
	{
		m_LevelFlowDisplay = !m_LevelFlowDisplay;
		if (m_LevelFlowDisplay)
		{
			if (m_LevelFlowGameObjects.Count != 0)
			{
				return;
			}
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			{
				foreach (CMap map in CurrentState.Maps)
				{
					ApparanceMap component = mapSceneRoot.FindInChildren(map.MapInstanceName).GetComponent<ApparanceMap>();
					GameObject gameObject = new GameObject("LevelFlowRenderer");
					m_LevelFlowGameObjects.Add(gameObject);
					LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
					lineRenderer.sharedMaterial = m_PathRendererMaterial;
					lineRenderer.widthMultiplier = 0.025f;
					lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
					lineRenderer.receiveShadows = false;
					lineRenderer.allowOcclusionWhenDynamic = false;
					lineRenderer.positionCount = 0;
					List<Vector3> list = new List<Vector3>();
					Vector3 vector = ((component.EntranceDoor != null) ? component.EntranceDoor.transform.position : new Vector3(map.Centre.X, map.Centre.Y, map.Centre.Z));
					Vector3 vector2 = new Vector3(map.DefaultLevelFlowNormal.X, map.DefaultLevelFlowNormal.Y, map.DefaultLevelFlowNormal.Z);
					list.Add(vector + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 3f + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 2f + Vector3.Cross(Vector3.up, vector2) + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 3f + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 2f - Vector3.Cross(Vector3.up, vector2) + new Vector3(0f, 0.05f, 0f));
					lineRenderer.enabled = m_LevelFlowDisplay;
					lineRenderer.positionCount = list.Count;
					lineRenderer.SetPositions(list.ToArray());
					GameObject gameObject2 = new GameObject("LevelCenterRenderer");
					m_LevelFlowGameObjects.Add(gameObject2);
					LineRenderer lineRenderer2 = gameObject2.AddComponent<LineRenderer>();
					lineRenderer2.sharedMaterial = m_PathRendererMaterial;
					lineRenderer2.widthMultiplier = 1f;
					lineRenderer2.shadowCastingMode = ShadowCastingMode.Off;
					lineRenderer2.receiveShadows = false;
					lineRenderer2.allowOcclusionWhenDynamic = false;
					lineRenderer2.positionCount = 0;
					list = new List<Vector3>();
					vector = new Vector3(map.Centre.X, map.Centre.Y, map.Centre.Z);
					list.Add(vector + new Vector3(0f, 0.05f, 0f) + new Vector3(0.5f, 0f, 0f));
					list.Add(vector + new Vector3(0f, 0.05f, 0f) + new Vector3(-1f, 0f, 0f));
					lineRenderer2.enabled = m_LevelFlowDisplay;
					lineRenderer2.positionCount = list.Count;
					lineRenderer2.SetPositions(list.ToArray());
				}
				return;
			}
		}
		foreach (GameObject levelFlowGameObject in m_LevelFlowGameObjects)
		{
			UnityEngine.Object.Destroy(levelFlowGameObject);
		}
		m_LevelFlowGameObjects.Clear();
	}

	private void LOSDebugPrimitive(Vector3 vector1, Vector3 vector2, Material material, string primitive)
	{
		GameObject gameObject = new GameObject("LOSRenderer");
		m_LOSGameObjects.Add(gameObject);
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.sharedMaterial = material;
		lineRenderer.widthMultiplier = 0.025f;
		lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
		lineRenderer.receiveShadows = false;
		lineRenderer.allowOcclusionWhenDynamic = false;
		lineRenderer.positionCount = 0;
		lineRenderer.loop = true;
		List<Vector3> list = new List<Vector3>();
		if (primitive == "square")
		{
			list.Add(vector1 + new Vector3(0.1f, 0f, 0f) + new Vector3(0f, 0.05f, 0f));
			list.Add(vector1 + new Vector3(0f, 0f, 0.1f) + new Vector3(0f, 0.05f, 0f));
			list.Add(vector1 + new Vector3(-0.1f, 0f, 0f) + new Vector3(0f, 0.05f, 0f));
			list.Add(vector1 + new Vector3(0f, 0f, -0.1f) + new Vector3(0f, 0.05f, 0f));
		}
		else
		{
			list.Add(vector1 + new Vector3(0f, 0.05f, 0f));
			list.Add(vector2 + new Vector3(0f, 0.05f, 0f));
		}
		lineRenderer.enabled = true;
		lineRenderer.positionCount = list.Count;
		lineRenderer.SetPositions(list.ToArray());
	}

	private void DebugLOSDisplay(Vector sourcePosition, Vector targetPosition, bool rayBlocked, string primitive = "line")
	{
		LOSDebugPrimitive(new Vector3(sourcePosition.X, 0f, sourcePosition.Y) - m_LOSDebugShift1, new Vector3(targetPosition.X, 0f, targetPosition.Y) - m_LOSDebugShift2, rayBlocked ? m_PathRendererMaterial : m_DarkPathRendererMaterial, primitive);
	}

	public void ClearAIPathVisuals(CActor actor)
	{
		if (m_CurrentAIPathActor == actor)
		{
			return;
		}
		foreach (GameObject aIPathGameObject in m_AIPathGameObjects)
		{
			UnityEngine.Object.Destroy(aIPathGameObject);
		}
		m_AIPathGameObjects.Clear();
		m_CurrentAIPathActor = null;
		CActorStatic.s_LastCalculatedPathActorAI = null;
	}

	public void Update()
	{
		if (m_LOSGameObjects.Count > 0)
		{
			foreach (GameObject lOSGameObject in m_LOSGameObjects)
			{
				UnityEngine.Object.Destroy(lOSGameObject);
			}
			m_LOSGameObjects.Clear();
		}
		if (m_LOSDisplay || m_LOSSourceTile != null)
		{
			CClientTile cClientTile;
			if (m_LOSDisplay)
			{
				cClientTile = ((Choreographer.s_Choreographer.m_CurrentActor == null) ? null : s_ClientScenarioManager.m_ClientTileArray[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y]);
			}
			else
			{
				cClientTile = m_LOSSourceTile;
			}
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Singleton<WorldspacePicker>.Instance.m_HexSelectionRaycastLayer);
			CClientTile cClientTile2 = null;
			if (m_LOSTargetTile != null)
			{
				cClientTile2 = m_LOSTargetTile;
			}
			else if (cInteractable != null && cInteractable.GetComponent<TileBehaviour>() != null)
			{
				cClientTile2 = cInteractable.GetComponent<TileBehaviour>().m_ClientTile;
			}
			CActor.SetLOSTileScalar(UnityGameEditorRuntime.s_TileSize.x, UnityGameEditorRuntime.s_TileSize.z);
			if (cClientTile != null && cClientTile2 != null)
			{
				m_LOSDebugShift1 = Vector3.zero;
				m_LOSDebugShift2 = Vector3.zero;
				ScenarioRuleLibrary.MF.ArrayIndexToCartesianCoord(cClientTile.m_Tile.m_ArrayIndex, UnityGameEditorRuntime.s_TileSize.x, UnityGameEditorRuntime.s_TileSize.z, out m_LOSDebugShift1.x, out m_LOSDebugShift1.z);
				m_LOSDebugShift1 -= cClientTile.m_GameObject.transform.position;
				ScenarioRuleLibrary.MF.ArrayIndexToCartesianCoord(cClientTile2.m_Tile.m_ArrayIndex, UnityGameEditorRuntime.s_TileSize.x, UnityGameEditorRuntime.s_TileSize.z, out m_LOSDebugShift2.x, out m_LOSDebugShift2.z);
				m_LOSDebugShift2 -= cClientTile2.m_GameObject.transform.position;
				CActor.HaveLOS(cClientTile.m_Tile, cClientTile2.m_Tile, DebugLOSDisplay);
			}
		}
		if (m_AIPathDisplay)
		{
			if (CActorStatic.s_LastCalculatedPathActorAI == null || CActorStatic.s_LastCalculatedPathActorAI == m_CurrentAIPathActor || CActorStatic.s_LastCalculatedPathAI == null || CActorStatic.s_LastCalculatedPathAI.m_ArrayIndices.Count <= 0)
			{
				return;
			}
			m_CurrentAIPathActor = CActorStatic.s_LastCalculatedPathActorAI;
			foreach (GameObject aIPathGameObject in m_AIPathGameObjects)
			{
				UnityEngine.Object.Destroy(aIPathGameObject);
			}
			m_AIPathGameObjects.Clear();
			if (CActorStatic.s_LastCalculatedPathAI.m_ArrayIndicesBeforeCull != null)
			{
				GameObject gameObject = new GameObject("AIPathRendererFull");
				m_AIPathGameObjects.Add(gameObject);
				LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.sharedMaterial = m_PathRendererMaterial;
				lineRenderer.widthMultiplier = 0.1f;
				lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
				lineRenderer.receiveShadows = false;
				lineRenderer.allowOcclusionWhenDynamic = false;
				lineRenderer.positionCount = 0;
				lineRenderer.numCapVertices = 8;
				lineRenderer.numCornerVertices = 8;
				List<Vector3> list = new List<Vector3>();
				list.Add(Choreographer.s_Choreographer.FindClientActorGameObject(m_CurrentAIPathActor).transform.position + new Vector3(0f, 0.25f, 0f));
				foreach (Point item in CActorStatic.s_LastCalculatedPathAI.m_ArrayIndicesBeforeCull)
				{
					list.Add(m_ClientTileArray[item.X, item.Y].m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
				}
				lineRenderer.enabled = m_AIPathDisplay;
				lineRenderer.positionCount = list.Count;
				lineRenderer.SetPositions(list.ToArray());
			}
			GameObject gameObject2 = new GameObject("AIPathRenderer");
			m_AIPathGameObjects.Add(gameObject2);
			LineRenderer lineRenderer2 = gameObject2.AddComponent<LineRenderer>();
			lineRenderer2.sharedMaterial = m_DarkPathRendererMaterial;
			lineRenderer2.widthMultiplier = 0.1f;
			lineRenderer2.shadowCastingMode = ShadowCastingMode.Off;
			lineRenderer2.receiveShadows = false;
			lineRenderer2.allowOcclusionWhenDynamic = false;
			lineRenderer2.positionCount = 0;
			lineRenderer2.numCapVertices = 8;
			lineRenderer2.numCornerVertices = 8;
			List<Vector3> list2 = new List<Vector3>();
			list2.Add(Choreographer.s_Choreographer.FindClientActorGameObject(m_CurrentAIPathActor).transform.position + new Vector3(0f, 0.25f, 0f));
			foreach (Point arrayIndex in CActorStatic.s_LastCalculatedPathAI.m_ArrayIndices)
			{
				list2.Add(m_ClientTileArray[arrayIndex.X, arrayIndex.Y].m_GameObject.transform.position + new Vector3(0f, 0.25f, 0f));
			}
			lineRenderer2.enabled = m_AIPathDisplay;
			lineRenderer2.positionCount = list2.Count;
			lineRenderer2.SetPositions(list2.ToArray());
			return;
		}
		foreach (GameObject aIPathGameObject2 in m_AIPathGameObjects)
		{
			UnityEngine.Object.Destroy(aIPathGameObject2);
		}
		m_AIPathGameObjects.Clear();
	}

	public void ToggleLOSDisplay(bool? newState = null)
	{
		m_LOSDisplay = (newState.HasValue ? newState.Value : (!m_LOSDisplay));
	}

	public void EnableUserLOSDisplay(bool isOn)
	{
		if (!isOn)
		{
			m_LOSSourceTile = null;
		}
		else if (Singleton<WorldspacePicker>.InstanceFast != null)
		{
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Singleton<WorldspacePicker>.Instance.m_HexSelectionRaycastLayer);
			m_LOSSourceTile = ((cInteractable == null || cInteractable.GetComponent<TileBehaviour>() == null) ? null : cInteractable.GetComponent<TileBehaviour>().m_ClientTile);
		}
	}

	public void SetSpecificSourceAndTargetTile(CClientTile sourceTile, CClientTile targetTile)
	{
		m_LOSSourceTile = sourceTile;
		m_LOSTargetTile = targetTile;
	}

	public void ToggleAStarDisplay()
	{
		m_AStarDisplay = !m_AStarDisplay;
		if (m_AStarDisplay)
		{
			if (m_AStarLineRendererGameObjects.Count != 0)
			{
				return;
			}
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				GameObject gameObject = new GameObject("PathRenderer");
				m_AStarLineRendererGameObjects.Add(gameObject);
				LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.sharedMaterial = m_PathRendererMaterial;
				lineRenderer.widthMultiplier = 0.025f;
				lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
				lineRenderer.receiveShadows = false;
				lineRenderer.allowOcclusionWhenDynamic = false;
				lineRenderer.positionCount = 0;
				List<Vector3> list = new List<Vector3>();
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CClientTile cClientTile = s_ClientScenarioManager.m_ClientTileArray[j, i];
					if (cClientTile == null || ScenarioManager.PathFinder.Nodes[j, i] == null || !ScenarioManager.PathFinder.Nodes[j, i].Walkable || ScenarioManager.PathFinder.Nodes[j, i].Blocked)
					{
						continue;
					}
					for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition >= ScenarioManager.EAdjacentPosition.ERight; eAdjacentPosition--)
					{
						CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, eAdjacentPosition);
						if (adjacentTile != null && ScenarioManager.PathFinder.Nodes[j, i].NavTo(ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y], ignoreBlocked: false))
						{
							list.Add(cClientTile.m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
							list.Add(m_ClientTileArray[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
							list.Add(cClientTile.m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
						}
						else if (eAdjacentPosition == ScenarioManager.EAdjacentPosition.ERight)
						{
							lineRenderer.enabled = m_AStarDisplay;
							lineRenderer.positionCount = list.Count;
							lineRenderer.SetPositions(list.ToArray());
							gameObject = new GameObject("PathRenderer");
							m_AStarLineRendererGameObjects.Add(gameObject);
							lineRenderer = gameObject.AddComponent<LineRenderer>();
							lineRenderer.sharedMaterial = m_PathRendererMaterial;
							lineRenderer.widthMultiplier = 0.025f;
							lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
							lineRenderer.receiveShadows = false;
							lineRenderer.allowOcclusionWhenDynamic = false;
							lineRenderer.positionCount = 0;
							list = new List<Vector3>();
						}
					}
				}
				lineRenderer.enabled = m_AStarDisplay;
				lineRenderer.positionCount = list.Count;
				lineRenderer.SetPositions(list.ToArray());
			}
			return;
		}
		foreach (GameObject aStarLineRendererGameObject in m_AStarLineRendererGameObjects)
		{
			UnityEngine.Object.Destroy(aStarLineRendererGameObject);
		}
		m_AStarLineRendererGameObjects.Clear();
	}

	public void ToggleAIPathDisplay()
	{
		m_AIPathDisplay = !m_AIPathDisplay;
	}

	public CClientTile GetClientTile(Point pos)
	{
		if (pos.X < 0 || pos.X >= ScenarioManager.Width)
		{
			return null;
		}
		if (pos.Y < 0 || pos.Y >= ScenarioManager.Height)
		{
			return null;
		}
		return s_ClientScenarioManager.ClientTileArray[pos.X, pos.Y];
	}
}

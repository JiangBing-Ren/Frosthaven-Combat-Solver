using System;
using System.Collections.Generic;
using AStar;
using EPOOutline;
using FH;
using FH.LevelComponents.ProcGen;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;

[ExecuteInEditMode]
public class UnityGameEditorObject : MonoBehaviour, IGeneralInfoProvider
{
	[NonSerialized]
	public bool m_ForPreview;

	public bool m_ShouldSnapToHexSpacing;

	public bool m_InvisableAtRuntime;

	public ScenarioManager.ObjectImportType m_ObjectType;

	public string m_PrefabName;

	public bool m_ShouldSnapRotation;

	public bool m_ExcludeFromExport;

	[HideInInspector]
	public List<ScenarioManager.ObjectImportType> m_ProcGenGameObjectsOnTile = new List<ScenarioManager.ObjectImportType>();

	private List<Point> m_PathingBlockers = new List<Point>();

	private List<Outlinable> m_OutlinableList;

	private CObjectProp m_PropObject;

	public static List<UnityGameEditorObject> s_all = new List<UnityGameEditorObject>();

	private bool m_started;

	public CObjectProp PropObject
	{
		get
		{
			return m_PropObject;
		}
		set
		{
			m_PropObject = value;
		}
	}

	private void OnEnable()
	{
		s_all.Add(this);
		Watchman.Instance?.Complete(gameObject.name);
		if (m_InvisableAtRuntime && Application.isPlaying && !ProceduralMapTile.IsInLevelEditorMode)
		{
			MeshRenderer component = GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.enabled = UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime;
			}
		}
	}

	private void OnDisable()
	{
		s_all.Remove(this);
	}

	public void Start()
	{
		if (m_started)
		{
			return;
		}
		m_started = true;
		if (!Application.isPlaying)
		{
			return;
		}
		if (m_ObjectType != ScenarioManager.ObjectImportType.Tile && m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile && m_ObjectType != ScenarioManager.ObjectImportType.Coverage)
		{
			gameObject.layer = LayerMask.NameToLayer("Hovering");
		}
		if (!(GetComponent<Coverage>() == null) || ScenarioManager.Scenario == null || ScenarioManager.Scenario.PositiveSpaceOffset == null)
		{
			return;
		}
		Vector3Int vector3Int = new Vector3Int(ScenarioManager.Scenario.PositiveSpaceOffset.X, ScenarioManager.Scenario.PositiveSpaceOffset.Y, ScenarioManager.Scenario.PositiveSpaceOffset.Z);
		Coverage[] componentsInChildren = gameObject.GetComponentsInChildren<Coverage>();
		foreach (Coverage obj in componentsInChildren)
		{
			Vector3Int vector3Int2 = MF.GetTileIntegerSnapSpace(obj.gameObject.transform.position) + vector3Int;
			m_PathingBlockers.Add(new Point(vector3Int2.x, vector3Int2.z));
			UnityEngine.Object.Destroy(obj.gameObject);
		}
		if (m_ObjectType == ScenarioManager.ObjectImportType.MoneyToken)
		{
			Transform transform = base.transform.Find("GoldPile");
			Transform transform2 = base.transform.Find("LootBag");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: true);
			}
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(value: false);
			}
		}
	}

	public void ReleasePathing()
	{
		if (m_ForPreview || ScenarioManager.PathFinder?.Nodes == null)
		{
			return;
		}
		if (PropObject is CObjectObstacle { PathingBlockers: not null } cObjectObstacle)
		{
			if (!cObjectObstacle.ReleasePFAfterDestroy)
			{
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					ScenarioManager.PathFinder.Nodes[pathingBlocker.X, pathingBlocker.Y].Blocked = false;
				}
			}
		}
		else
		{
			foreach (Point pathingBlocker2 in m_PathingBlockers)
			{
				ScenarioManager.PathFinder.Nodes[pathingBlocker2.X, pathingBlocker2.Y].Blocked = false;
			}
		}
		if (PropObject is CObjectIceTerrain { ReleasePFAfterDestroy: false } cObjectIceTerrain)
		{
			ScenarioManager.PathFinder.Nodes[cObjectIceTerrain.ArrayIndex.X, cObjectIceTerrain.ArrayIndex.Y].IsIceTerrain = false;
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			ReleasePathing();
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		if (PropObject == null)
		{
			return null;
		}
		if (PropObject is CObjectTerrainVisual { PropCustomLocKey: null })
		{
			return null;
		}
		return UIGeneralInfoUtils.GetPropData(this);
	}

	public void AddOutlinesToLinkedProps()
	{
		CObjectProp propObject = PropObject;
		if (propObject != null && propObject.HasLinkedProps && (!string.IsNullOrEmpty(propObject.PropCustomLocKey) || propObject.SpecificPropType == ESpecificPropType.LurkerWavethrowerWater || propObject.SpecificPropType == ESpecificPropType.CrashingTideWater || propObject is CObjectDifficultTerrain) && GetComponent<Outlinable>() == null)
		{
			Outlinable outlinable = gameObject.gameObject.AddComponent<Outlinable>();
			outlinable.AddAllChildRenderersToRenderingList(RenderersAddingMode.MeshRenderer | RenderersAddingMode.SkinnedMeshRenderer);
			OutlineWrapper outlineWrapper = outlinable.gameObject.AddComponent<OutlineWrapper>();
			outlineWrapper.ObjectImportType = propObject.ObjectType;
			outlineWrapper.Refresh();
		}
	}
}

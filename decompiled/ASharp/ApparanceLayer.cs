using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

[ExecuteAlways]
public class ApparanceLayer : MonoBehaviour
{
	[Header("0 = Door Hex | 1 = Door Body (level door, level editor door)")]
	public GameObject[] ThinDoor;

	public GameObject[] ThickDoor;

	public GameObject[] ThinNarrowDoor;

	public GameObject[] ThickNarrowDoor;

	public GameObject[] EmptyDoor;

	public GameObject[] EmptyDoorThin;

	[Header("Entrance doors")]
	public GameObject EntranceThinDoor;

	public GameObject EntranceThickDoor;

	[Header("Exit doors")]
	public GameObject ExitThinDoor;

	public GameObject ExitThickDoor;

	private static ApparanceLayer _instance;

	public List<GameObject> DoorInstances = new List<GameObject>();

	public static ApparanceLayer Instance
	{
		get
		{
			return _instance ?? (_instance = UnityEngine.Object.FindObjectOfType<ApparanceLayer>());
		}
		private set
		{
			_instance = value;
		}
	}

	private void Start()
	{
		Instance = this;
		DoorInstances = new List<GameObject>();
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public void Create(CObjectDoor door, GameObject procDoor)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(GetDoorFramePrefab(door), transform);
		DoorInstances.Add(gameObject);
		gameObject.transform.localPosition = GloomUtility.CVToV(door.Position);
		gameObject.transform.localEulerAngles = new Vector3(0f, door.Angle, 0f);
		UnityGameEditorDoorProp component = procDoor.GetComponent<UnityGameEditorDoorProp>();
		if (!(component != null))
		{
			return;
		}
		if (component.m_IsDungeonEntrance || component.m_IsDungeonExit)
		{
			gameObject.transform.SetParent(procDoor.transform.parent);
			gameObject.name = procDoor.name;
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].Blocked = true;
			UnityEngine.Object.Destroy(procDoor);
			return;
		}
		UnityGameEditorObject component2 = procDoor.GetComponent<UnityGameEditorObject>();
		if (component2 != null)
		{
			component2.PropObject = door;
		}
		gameObject.transform.SetParent(procDoor.transform);
		component.CreateHoverCapsule(gameObject);
		if (0 == 0 && !component.m_InitiallyVisable)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	internal static List<string> GetRealDoorTypeNames()
	{
		List<string> list = Enum.GetNames(typeof(CObjectDoor.EDoorType)).ToList();
		list.Remove(CObjectDoor.EDoorType.HexOpeningDoor.ToString());
		return list;
	}

	public void CreateDoorFrame(CObjectDoor door, GameObject procDoor)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(GetDoorFramePrefab(door), transform);
		DoorInstances.Add(gameObject);
		gameObject.transform.localPosition = GloomUtility.CVToV(door.Position);
		gameObject.transform.SetParent(procDoor.transform);
		gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		UnityGameEditorDoorProp component = procDoor.GetComponent<UnityGameEditorDoorProp>();
		if (!(component != null))
		{
			return;
		}
		if (component.m_IsDungeonEntrance || component.m_IsDungeonExit)
		{
			throw new Exception("Dungeon entrance/exit doors should not be created in this way. (" + procDoor.name + ")");
		}
		UnityGameEditorObject component2 = procDoor.GetComponent<UnityGameEditorObject>();
		if (component2 != null)
		{
			component2.PropObject = door;
		}
		component.CreateHoverCapsule(gameObject);
		if (0 == 0 && !component.m_InitiallyVisable)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	public void DestroyDoors()
	{
		foreach (GameObject doorInstance in DoorInstances)
		{
			UnityEngine.Object.Destroy(doorInstance);
		}
	}

	public GameObject GetDoorFramePrefab(CObjectDoor doorObject)
	{
		return GetDoorPrefab(doorObject.DoorType, doorObject.IsDungeonEntrance, doorObject.IsDungeonExit);
	}

	public GameObject GetDoorPrefab(CObjectDoor.EDoorType door, bool isEntrance = false, bool isExit = false, bool isDoorBody = false)
	{
		int num = (isDoorBody ? 1 : 0);
		switch (door)
		{
		case CObjectDoor.EDoorType.ThickDoor:
			if (isEntrance)
			{
				return EntranceThickDoor;
			}
			if (isExit)
			{
				return ExitThickDoor;
			}
			return ThickDoor[num];
		case CObjectDoor.EDoorType.ThinDoor:
			if (isEntrance)
			{
				return EntranceThinDoor;
			}
			if (isExit)
			{
				return ExitThinDoor;
			}
			return ThinDoor[num];
		case CObjectDoor.EDoorType.EmptyDoor:
			return EmptyDoor[num];
		case CObjectDoor.EDoorType.EmptyDoorThin:
			return EmptyDoorThin[num];
		case CObjectDoor.EDoorType.ThickNarrowDoor:
			return ThickNarrowDoor[num];
		case CObjectDoor.EDoorType.ThinNarrowDoor:
			return ThinNarrowDoor[num];
		default:
			Log.Error($"Invalid door type {door}");
			return null;
		}
	}

	public static bool IsThinDoor(CObjectDoor.EDoorType doorType)
	{
		switch (doorType)
		{
		case CObjectDoor.EDoorType.ThinDoor:
		case CObjectDoor.EDoorType.ThinNarrowDoor:
		case CObjectDoor.EDoorType.EmptyDoor:
		case CObjectDoor.EDoorType.EmptyDoorThin:
			return true;
		case CObjectDoor.EDoorType.ThickDoor:
		case CObjectDoor.EDoorType.ThickNarrowDoor:
			return false;
		default:
			Log.Error("Invalid door type " + doorType);
			return false;
		}
	}
}

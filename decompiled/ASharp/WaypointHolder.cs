using System;
using System.Collections.Generic;
using UnityEngine;

public class WaypointHolder : MonoBehaviour
{
	public enum EWaypointType
	{
		Start,
		Node,
		Small,
		End
	}

	[Serializable]
	public class WaypointPrefab
	{
		public EWaypointType Type;

		public GameObject Prefab;
	}

	public static readonly Vector3 YOffset = new Vector3(0f, 0.1f, 0f);

	public List<WaypointPrefab> m_Prefabs;

	public void EnableType(EWaypointType type)
	{
		foreach (WaypointPrefab prefab in m_Prefabs)
		{
			if (prefab.Type == type)
			{
				prefab.Prefab.SetActive(value: true);
				break;
			}
		}
	}

	private void OnDisable()
	{
		foreach (WaypointPrefab prefab in m_Prefabs)
		{
			prefab.Prefab.SetActive(value: false);
		}
	}
}

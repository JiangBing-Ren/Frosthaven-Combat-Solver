using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using UnityEngine;

public class DoorFrameVisibility : MonoBehaviour
{
	public List<GameObject> FrontObjects = new List<GameObject>();

	public List<GameObject> BackObjects = new List<GameObject>();

	public List<GameObject> BothObjects = new List<GameObject>();

	public List<GameObject> AnyObjects = new List<GameObject>();

	[Header("Editor Only:")]
	[Tooltip("This is only used for the 'Fetch Objects Automatically' tool")]
	public Vector3 FrontDirection = Vector3.forward;

	public void UpdateVisibility(bool front, bool back)
	{
		foreach (GameObject frontObject in FrontObjects)
		{
			ProceduralMapTile.EnableLightsAndEffects(frontObject, front);
		}
		foreach (GameObject backObject in BackObjects)
		{
			ProceduralMapTile.EnableLightsAndEffects(backObject, back);
		}
		foreach (GameObject bothObject in BothObjects)
		{
			ProceduralMapTile.EnableLightsAndEffects(bothObject, front & back);
		}
		foreach (GameObject anyObject in AnyObjects)
		{
			ProceduralMapTile.EnableLightsAndEffects(anyObject, front | back);
		}
	}

	[ContextMenu("Fetch Objects Automatically")]
	public void FetchObjectsAutomatically()
	{
		ClearObjects();
		List<GameObject> list = new List<GameObject>();
		list.AddRange(from x in GetComponentsInChildren<Light>(includeInactive: true)
			select x.gameObject);
		list.AddRange(from x in GetComponentsInChildren<ParticleSystem>(includeInactive: true)
			select x.gameObject);
		foreach (GameObject item in list)
		{
			float num = Vector3.Dot(item.transform.position - transform.position, FrontDirection);
			bool flag = num > 0f;
			bool flag2 = num < 0f;
			if (flag)
			{
				FrontObjects.Add(item);
			}
			else if (flag2)
			{
				BackObjects.Add(item);
			}
			else
			{
				AnyObjects.Add(item);
			}
		}
		Debug.Log($"Automatically fetched objects for: {gameObject.name}. Found:\n{FrontObjects.Count} in the " + $"front, {BackObjects.Count} in the back, and {AnyObjects.Count} for any side.");
	}

	[ContextMenu("Clear Objects")]
	public void ClearObjects()
	{
		FrontObjects.Clear();
		BackObjects.Clear();
		BothObjects.Clear();
	}
}

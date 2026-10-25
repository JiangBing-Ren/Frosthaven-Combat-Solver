using System.Collections.Generic;
using SETUtil;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[ExecuteAlways]
public class BayAreaFloorGenerator : MonoBehaviour
{
	private const float TILE_SIZE_X = 1.723f;

	private const float TILE_SIZE_Y = 1.4921619f;

	private const float PLACEMENT_HEIGHT = 0f;

	public GameObject TilePrefab;

	private Vector3 lastScale = Vector3.zero;

	private const bool updateEveryFrame = false;

	private List<GameObject> spawned = new List<GameObject>();

	private Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);

	private Matrix4x4 worldToLocal;

	private const float tileCoverageRadius = 0.7491305f;

	private void Update()
	{
		if (!Mathf.Approximately(Vector3.SqrMagnitude(lastScale - transform.localScale), 0f))
		{
			Populate();
		}
	}

	private void OnDestroy()
	{
		Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(Vector3.one / 2f, Vector3.one);
	}

	private void InitializeLocal()
	{
		worldToLocal = transform.worldToLocalMatrix;
		localBounds = new Bounds(Vector3.one / 2f, Vector3.one);
	}

	private void Clear()
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == null) && !(transform == base.transform))
			{
				SceneUtil.SmartDestroy(transform.gameObject);
			}
		}
		foreach (GameObject item in spawned)
		{
			if ((bool)item)
			{
				SceneUtil.SmartDestroy(item);
			}
		}
		spawned.Clear();
	}

	[ContextMenu("Populate")]
	private void Populate()
	{
		Clear();
		InitializeLocal();
		if (!TilePrefab || string.IsNullOrEmpty(base.gameObject.scene.name))
		{
			return;
		}
		Transform transform = base.transform;
		Vector3 localScale = transform.localScale;
		_ = base.transform.worldToLocalMatrix;
		Vector3 vector;
		_ = (vector = SnapToHexGrid(transform.position)) + localScale;
		bool flag = Mathf.Abs(Vector3.Angle(transform.forward, Vector3.forward) % 60f - 30f) < 5f;
		Vector3 vector2 = ((!flag) ? transform.right : (-transform.forward));
		Vector3 vector3 = ((!flag) ? transform.forward : (-transform.right));
		int num = Mathf.CeilToInt((flag ? Mathf.Abs(localScale.z) : Mathf.Abs(localScale.x)) / 1.723f);
		int num2 = Mathf.CeilToInt(((flag ? Mathf.Abs(localScale.x) : Mathf.Abs(localScale.z)) + 1.4921619f) / 1.4921619f);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float num3 = ((j % 2 == 1) ? 0.8615f : 0f);
				Vector3 vector4 = new Vector3(vector.x, 0f, vector.z) + vector2 * ((float)i * 1.723f + num3) * Mathf.Sign(transform.localScale.x) + vector3 * ((float)j * 1.4921619f) * Mathf.Sign(transform.localScale.y);
				if (IsInside(vector4))
				{
					GameObject gameObject = SceneUtil.Instantiate(TilePrefab);
					gameObject.hideFlags = HideFlags.DontSave;
					gameObject.transform.SetParent(transform.parent);
					gameObject.transform.localScale = Vector3.one;
					gameObject.transform.position = vector4;
					gameObject.transform.rotation = Quaternion.identity;
					spawned.Add(gameObject);
				}
			}
		}
		lastScale = transform.localScale;
	}

	private Vector3 SnapToHexGrid(Vector3 position)
	{
		int num = Mathf.RoundToInt(position.z / 1.4921619f);
		float num2 = num;
		float x = ((Mathf.Abs(num) % 2 != 1) ? ((float)Mathf.RoundToInt(position.x / 1.723f) * 1.723f) : (((float)Mathf.RoundToInt(position.x / 1.723f) - 0.5f) * 1.723f));
		return new Vector3(x, position.y, num2 * 1.4921619f);
	}

	private bool IsInside(Vector3 point)
	{
		return localBounds.Contains(worldToLocal.MultiplyPoint3x4(point));
	}
}

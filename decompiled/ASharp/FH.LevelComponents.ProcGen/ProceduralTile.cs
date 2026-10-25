using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.LevelComponents.ProcGen;

[ExecuteAlways]
public class ProceduralTile : MonoBehaviour, IProceduralTile
{
	public float ContentHeight;

	public float ContentRadius;

	public int TileFlags;

	[Description("Should this be included in the tile list (to be avoided)?")]
	public bool Include = true;

	[Description("Should have it's position snapped to the hex grid?")]
	public bool Snap = true;

	private Vector3 previousPosition = offGrid;

	private Vector3 previousSnapPosition = offGrid;

	private float previousContentRadius;

	private float previousContentHeight;

	private bool previousActive;

	private bool initialInit;

	private static Vector3 offGrid = new Vector3(100000f, 100000f, 0f);

	bool IProceduralTile.IsActive
	{
		get
		{
			if (Include)
			{
				return gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	float IProceduralTile.ContentHeight => ContentHeight;

	float IProceduralTile.ContentRadius => ContentRadius;

	int IProceduralTile.Flags => TileFlags;

	float IProceduralTile.Orientation => transform.rotation.eulerAngles.y;

	Vector3 IProceduralTile.Position => transform.position;

	private void Start()
	{
		initialInit = true;
	}

	private void OnEnable()
	{
		initialInit = true;
		ProceduralTileTracker.RegisterTile(this);
	}

	private void OnDisable()
	{
		ProceduralTileTracker.NotifyTileMove(this, previousSnapPosition, previousSnapPosition);
		previousPosition = offGrid;
		previousSnapPosition = offGrid;
		previousContentRadius = 0f;
		previousContentHeight = 0f;
		previousActive = false;
		initialInit = false;
		ProceduralTileTracker.UnregisterTile(this);
	}

	private void Update()
	{
		Scene scene = gameObject.scene;
		if (scene.name == null || scene.rootCount == 0)
		{
			return;
		}
		Vector3 position = transform.position;
		if (position != previousPosition || initialInit)
		{
			initialInit = false;
			if (Snap && ProceduralTileTracker.SnapTilePosition(ref position))
			{
				transform.position = position;
			}
			previousPosition = position;
			if (position != previousSnapPosition)
			{
				ProceduralTileTracker.NotifyTileMove(this, previousSnapPosition, position);
				previousSnapPosition = position;
			}
		}
		bool activeInHierarchy = gameObject.activeInHierarchy;
		if (ContentRadius != previousContentRadius || ContentHeight != previousContentHeight || previousActive != activeInHierarchy)
		{
			ProceduralTileTracker.NotifyTileMove(this, position, position);
			previousContentRadius = ContentRadius;
			previousContentHeight = ContentHeight;
			previousActive = activeInHierarchy;
		}
	}

	internal ProceduralMapTile FindMapTileByPosition(Vector3 position, out Vector3 wall_size)
	{
		Vector2 p = new Vector2(position.x, position.z);
		float num = 1000f;
		ProceduralWall proceduralWall = null;
		foreach (ProceduralWall item in ProceduralWall.FindWallsNear(position, 2.2f))
		{
			Vector3 vector = (item.LeftCorner.Position + item.RightCorner.Position) * 0.5f;
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			Vector2 vector3 = new Vector2(0f - item.transform.forward.x, 0f - item.transform.forward.z);
			float num2 = ProceduralWallAnalyser.ProjectPointOntoSection(vector2, vector2 + vector3, p);
			if (num2 > 0.1f && num2 < num)
			{
				num = num2;
				proceduralWall = item;
			}
		}
		if (proceduralWall != null)
		{
			wall_size = proceduralWall.cachedSize;
			return proceduralWall.GetComponentInParent<ProceduralMapTile>(includeInactive: true);
		}
		wall_size = Vector3.zero;
		return null;
	}
}

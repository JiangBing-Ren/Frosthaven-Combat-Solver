using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[ExecuteInEditMode]
public class ProceduralTileTracker : MonoBehaviour
{
	public GameObject ReferenceTile;

	private static HashSet<ProceduralTileObserver> _activeObservers = new HashSet<ProceduralTileObserver>();

	private static HashSet<ProceduralTile> _activeTiles = new HashSet<ProceduralTile>();

	[NonSerialized]
	public const float MajorSpacing = 1.7195444f;

	[NonSerialized]
	public const float MinorSpacing = 1.4891691f;

	public const bool IsReady = true;

	public static int TileAddressFromPosition(Vector3 position)
	{
		int num = (int)Math.Floor(((double)position.z + 0.7445845603942871) / 1.4891691207885742);
		float num2 = (((num & 1) != 0) ? 0f : 0.5f) * 1.7195444f;
		return TileAddressFromXY((int)Math.Floor((position.x + num2) / 1.7195444f), num);
	}

	public static Vector3 TilePositionFromAddress(int a)
	{
		int num = TileXFromAddress(a);
		int num2 = TileYFromAddress(a);
		float num3 = (((num2 & 1) != 0) ? 0.5f : 0f);
		float x = ((float)num + num3) * 1.7195444f;
		float z = (float)num2 * 1.4891691f;
		return new Vector3(x, 0f, z);
	}

	public static int TileAddressFromXY(int x, int y)
	{
		return (x & 0xFFFF) | ((y & 0xFFFF) << 16);
	}

	public static int TileXFromAddress(int a)
	{
		return (short)(a & 0xFFFF);
	}

	public static int TileYFromAddress(int a)
	{
		return (short)((a >> 16) & 0xFFFF);
	}

	public static bool SnapTilePosition(ref Vector3 position)
	{
		Vector3 vector = position;
		Vector3 vector2 = TilePositionFromAddress(TileAddressFromPosition(vector));
		if (vector2 != vector)
		{
			position = vector2;
			position.y = vector.y;
			return true;
		}
		return false;
	}

	public static float Snap(float value, float step_size)
	{
		return (float)Math.Round(value / step_size) * step_size;
	}

	public static float Snap(float value, float step_size, out int index)
	{
		float num = value / step_size;
		num = (float)Math.Round(num);
		index = (int)num;
		return num * step_size;
	}

	public static float SnapHexOrientation(float hex_orientation_degrees)
	{
		return Snap(hex_orientation_degrees, 60f);
	}

	public static float SnapWallOrientation(float wall_orientation_degrees)
	{
		return Snap(wall_orientation_degrees, 30f);
	}

	public static float SnapWallOrientation(float wall_orientation_degrees, out int wall_segment_index)
	{
		return Snap(wall_orientation_degrees, 30f, out wall_segment_index);
	}

	public static float GetWallSpacingSnapOffset(Vector2 position, Vector2 normal, int wall_segment_index)
	{
		float step_size = (((wall_segment_index & 1) != 0) ? 0.8597722f : 0.49638972f);
		float num = Vector2.Dot(position, normal);
		return Snap(num, step_size) - num;
	}

	public static Vector2 SnapWallVertex(Vector2 position)
	{
		Vector3 position2 = new Vector3(position.x, 0f, position.y);
		SnapTilePosition(ref position2);
		Vector2 vector = new Vector2(position2.x, position2.z);
		Vector2 vector2 = position - vector;
		float wall_orientation_degrees = (float)Math.Atan2(vector2.y, vector2.x) * 180f / MathF.PI;
		wall_orientation_degrees = SnapWallOrientation(wall_orientation_degrees, out var wall_segment_index);
		Vector2 vector3 = RotateVector(Vector2.right, wall_orientation_degrees);
		float num = 0.99277943f;
		float num2 = 0.8597722f;
		float num3 = num * 0.5773503f;
		Vector2 vector4;
		Vector2 vector5;
		if ((wall_segment_index & 1) == 0)
		{
			vector4 = vector3 * num2;
			vector5 = vector3 * num3;
		}
		else
		{
			vector4 = vector3 * num;
			vector5 = vector3 * num * 0.5f;
		}
		Vector2 vector6 = vector;
		vector4 += vector;
		vector5 += vector;
		float sqrMagnitude = (position - vector6).sqrMagnitude;
		float sqrMagnitude2 = (position - vector4).sqrMagnitude;
		float sqrMagnitude3 = (position - vector5).sqrMagnitude;
		if (sqrMagnitude < sqrMagnitude2)
		{
			if (sqrMagnitude < sqrMagnitude3)
			{
				return vector6;
			}
			return vector5;
		}
		if (sqrMagnitude2 < sqrMagnitude3)
		{
			return vector4;
		}
		return vector5;
	}

	private static Vector2 RotateVector(Vector2 v, float angle_degrees)
	{
		float num = MathF.PI * angle_degrees / -180f;
		float num2 = (float)Math.Cos(num);
		float num3 = (float)Math.Sin(num);
		float x = v.x * num2 + v.y * num3;
		float y = (0f - v.x) * num3 + v.y * num2;
		return new Vector2(x, y);
	}

	private static bool AccumulateBounds(GameObject o, ref Bounds b)
	{
		bool result = false;
		Renderer component = o.GetComponent<Renderer>();
		if (component != null)
		{
			if (b.size.magnitude == 0f)
			{
				b = component.bounds;
			}
			else
			{
				b.Encapsulate(component.bounds);
			}
			result = true;
		}
		for (int i = 0; i < o.transform.childCount; i++)
		{
			if (AccumulateBounds(o.transform.GetChild(i).gameObject, ref b))
			{
				result = true;
			}
		}
		return result;
	}

	private void Start()
	{
		Init();
	}

	private void OnValidate()
	{
		Init();
	}

	private void Init()
	{
	}

	public static void RegisterObserver(ProceduralTileObserver observer)
	{
		_activeObservers.Add(observer);
	}

	public static void UnregisterObserver(ProceduralTileObserver observer)
	{
		_activeObservers.Remove(observer);
	}

	public static void RegisterTile(ProceduralTile tile)
	{
		_activeTiles.Add(tile);
	}

	public static void UnregisterTile(ProceduralTile tile)
	{
		_activeTiles.Remove(tile);
	}

	public static void NotifyTileMove(ProceduralTile tile, Vector3 prev_pos, Vector3 new_pos)
	{
		if (!(tile != null))
		{
			return;
		}
		foreach (ProceduralTileObserver activeObserver in _activeObservers)
		{
			if (activeObserver != null && activeObserver.gameObject != null)
			{
				Bounds bounds = default;
				CollectBounds(activeObserver.gameObject, ref bounds);
				bounds.Expand(3.4390888f);
				bool was_in = bounds.Contains(prev_pos);
				bool now_in = bounds.Contains(new_pos) && ((IProceduralTile)tile).IsActive;
				bool physical_move = new_pos != prev_pos;
				activeObserver.NotifyTile(tile.gameObject, was_in, now_in, physical_move);
			}
		}
	}

	public static void NotifyTileState(ProceduralTile tile)
	{
		NotifyTileMove(tile, tile.transform.position, tile.transform.position);
	}

	public static void CollectBounds(GameObject obj, ref Bounds bounds)
	{
		if (!(obj != null))
		{
			return;
		}
		BoxCollider component = obj.GetComponent<BoxCollider>();
		if (component != null)
		{
			if (bounds.size.x < 0.001f && bounds.size.y < 0.001f && bounds.size.z < 0.001f)
			{
				bounds = component.bounds;
			}
			else
			{
				bounds.Encapsulate(component.bounds);
			}
		}
		else
		{
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				CollectBounds(obj.transform.GetChild(i).gameObject, ref bounds);
			}
		}
	}

	public static void NotifyObserverMove(GameObject observer_object, Bounds previous_bounds, Bounds new_bounds)
	{
		ProceduralTileObserver component = observer_object.GetComponent<ProceduralTileObserver>();
		if (!(component != null))
		{
			return;
		}
		previous_bounds.Expand(3.4390888f);
		new_bounds.Expand(3.4390888f);
		foreach (ProceduralTile activeTile in _activeTiles)
		{
			if (activeTile != null && activeTile.transform != null)
			{
				Vector3 position = activeTile.transform.position;
				bool was_in = previous_bounds.Contains(position);
				bool now_in = new_bounds.Contains(position) && ((IProceduralTile)activeTile).IsActive;
				component.NotifyTile(activeTile.gameObject, was_in, now_in, physical_move: false);
			}
		}
	}

	private void OnDestroy()
	{
		_activeObservers.Clear();
	}
}

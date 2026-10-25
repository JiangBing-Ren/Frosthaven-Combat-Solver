using System;
using System.Collections.Generic;
using Libs.Utils;
using SETUtil;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ShapeDrawer : MonoBehaviour
{
	private readonly Dictionary<int, LineRenderer> _shapes = new Dictionary<int, LineRenderer>();

	private readonly Queue<LineRenderer> _inactive = new Queue<LineRenderer>();

	private Material _defaultMaterial;

	public void ClearShape(int id)
	{
		if (_shapes.TryGetValue(id, out var value))
		{
			value.gameObject.SetActive(value: false);
			_shapes.Remove(id);
			_inactive.Enqueue(value);
		}
	}

	public void ClearAll()
	{
		_shapes.Values.ForEach((LineRenderer shape) =>
		{
			shape.gameObject.SetActive(value: false);
			_inactive.Enqueue(shape);
		});
		_shapes.Clear();
		if ((bool)_defaultMaterial)
		{
			SceneUtil.SmartDestroy(_defaultMaterial);
		}
	}

	public void DrawSegment(int id, Vector3 start, Vector3 end, float thickness, Color color)
	{
		LineRenderer orAddShape = GetOrAddShape(id);
		orAddShape.positionCount = 2;
		orAddShape.SetPositions(new Vector3[2] { start, end });
		orAddShape.ResetBounds();
		orAddShape.widthMultiplier = thickness;
		orAddShape.startColor = color;
		orAddShape.endColor = color;
		orAddShape.loop = false;
	}

	public void DrawPath(int id, Vector3[] path, float thickness, Color color, bool loop = false)
	{
		LineRenderer orAddShape = GetOrAddShape(id);
		orAddShape.positionCount = path.Length;
		orAddShape.SetPositions(path);
		orAddShape.ResetBounds();
		orAddShape.widthMultiplier = thickness;
		orAddShape.startColor = color;
		orAddShape.endColor = color;
		orAddShape.loop = loop;
	}

	public void DrawPath(int id, NativeArray<Vector3> path, float thickness, Color color, bool loop = false)
	{
		LineRenderer orAddShape = GetOrAddShape(id);
		orAddShape.positionCount = path.Length;
		orAddShape.SetPositions(path);
		orAddShape.ResetBounds();
		orAddShape.widthMultiplier = thickness;
		orAddShape.startColor = color;
		orAddShape.endColor = color;
		orAddShape.loop = loop;
	}

	public void DrawCircleXZ(int id, Vector2 position, float radius, int precision, float thickness, Color color)
	{
		precision = Mathf.Max(3, precision);
		float num = MathF.PI * 2f / (float)precision;
		Vector3[] array = new Vector3[precision];
		for (int i = 0; i < precision; i++)
		{
			Vector3 vector = ToVector3(position);
			vector.x += Mathf.Sin(num * (float)i) * radius;
			vector.z += Mathf.Cos(num * (float)i) * radius;
			array[i] = vector;
		}
		DrawPath(id, array, thickness, color, loop: true);
	}

	private LineRenderer GetOrAddShape(int id)
	{
		if (_shapes.TryGetValue(id, out var value))
		{
			return value;
		}
		return AddShape(id);
	}

	private LineRenderer AddShape(int id)
	{
		if (_inactive.TryDequeue(out var result))
		{
			result.gameObject.name = $"Shape_{id}";
			result.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = new GameObject($"Shape_{id}");
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			result = gameObject.AddComponent<LineRenderer>();
		}
		SetLineRendererDefaults(result);
		_shapes.Add(id, result);
		return result;
	}

	private void SetLineRendererDefaults(LineRenderer shape)
	{
		if (_defaultMaterial == null)
		{
			_defaultMaterial = new Material(Shader.Find("ShapeOverlay"));
		}
		shape.sharedMaterial = _defaultMaterial;
		shape.generateLightingData = false;
		shape.shadowCastingMode = ShadowCastingMode.Off;
		shape.alignment = LineAlignment.Local;
		shape.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		shape.loop = false;
		shape.SetPositions(Array.Empty<Vector3>());
		shape.positionCount = 0;
	}

	private static Vector2 ToVector2(Vector3 vector)
	{
		return new Vector2(vector.x, vector.z);
	}

	private static Vector3 ToVector3(Vector2 vector)
	{
		return new Vector3(vector.x, 0f, vector.y);
	}

	private void OnDisable()
	{
		ClearAll();
	}
}

using Libs.Utils;
using UnityEngine;

[ExecuteAlways]
public class CurveBlendingTest : MonoBehaviour
{
	[Header("Source")]
	public AnimationCurve A;

	public AnimationCurve B;

	[Range(0f, 1f)]
	public float Factor;

	[Header("Result")]
	public AnimationCurve Blend;

	[Space]
	public float DrawerLineThickness = 0.1f;

	public float DrawerScale = 10f;

	public int DrawerCurveResolution = 200;

	[Range(0f, 0.4f)]
	public float DrawKeyframes;

	private ShapeDrawer _drawer;

	private int _shapeCount;

	private bool _shouldDrawKeyframes;

	private void OnEnable()
	{
		transform.DestroyChildren(!Application.isPlaying);
		_drawer = null;
		if (A == null)
		{
			A = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		}
		if (B == null)
		{
			B = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
		if (Blend == null)
		{
			Blend = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
	}

	private void Update()
	{
		if (!_drawer)
		{
			GameObject gameObject = new GameObject("ShapeDrawer");
			gameObject.transform.parent = transform;
			_drawer = gameObject.AddComponent<ShapeDrawer>();
		}
		bool flag = !Mathf.Approximately(DrawKeyframes, 0f);
		if (flag != _shouldDrawKeyframes)
		{
			_drawer.ClearAll();
		}
		_shouldDrawKeyframes = flag;
		CurveBlending.BlendCurvesGeometrically(A, B, Blend, Factor, 0f);
		DrawCurve(_drawer, 0, A, Color.green);
		DrawCurve(_drawer, 1, B, Color.green);
		DrawCurve(_drawer, 2, Blend, Color.blue);
	}

	private void DrawCurve(ShapeDrawer drawer, int curveIndex, AnimationCurve curve, Color color)
	{
		Vector3 position = transform.position;
		Vector3 vector = Vector3.forward * DrawerScale;
		Vector3 vector2 = Vector3.right * DrawerScale;
		int num = curveIndex * 1000;
		drawer.DrawSegment(num, position, position + vector, DrawerLineThickness, Color.gray);
		drawer.DrawSegment(num + 1, position + vector2, position + vector + vector2, DrawerLineThickness, Color.gray);
		drawer.DrawSegment(num + 2, position, position + vector2, DrawerLineThickness, Color.gray);
		drawer.DrawSegment(num + 3, position + vector, position + vector2 + vector, DrawerLineThickness, Color.gray);
		Vector3[] array = new Vector3[DrawerCurveResolution];
		float num2 = 1f / Mathf.Max(DrawerCurveResolution, 1f);
		for (int i = 0; i < DrawerCurveResolution; i++)
		{
			float num3 = (float)i * num2;
			array[i] = position + vector * curve.Evaluate(num3) + vector2 * num3;
		}
		drawer.DrawPath(num + 4, array, DrawerLineThickness, color);
		if (_shouldDrawKeyframes)
		{
			for (int j = 0; j < curve.keys.Length; j++)
			{
				Keyframe keyframe = curve.keys[j];
				DrawPoint(drawer, num + 5 + j, new Vector2(keyframe.time, keyframe.value), DrawKeyframes, color);
			}
		}
	}

	private void DrawPoint(ShapeDrawer drawer, int shapeIndex, Vector2 position, float size, Color color)
	{
		Vector3 vector = Vector3.forward * DrawerScale;
		Vector3 vector2 = Vector3.right * DrawerScale;
		Vector3 vector3 = transform.position + vector2 * position.x + vector * position.y;
		drawer.DrawCircleXZ(shapeIndex, new Vector2(vector3.x, vector3.z), size, 10, DrawerLineThickness, color);
	}
}

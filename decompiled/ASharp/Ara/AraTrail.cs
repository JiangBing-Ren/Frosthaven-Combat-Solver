using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ara;

[ExecuteInEditMode]
public class AraTrail : MonoBehaviour
{
	public enum TrailAlignment
	{
		View,
		Velocity,
		Local
	}

	public enum Timescale
	{
		Normal,
		Unscaled
	}

	public enum TextureMode
	{
		Stretch,
		Tile
	}

	public struct CurveFrame(Vector3 position, Vector3 normal, Vector3 bitangent, Vector3 tangent)
	{
		public Vector3 position = position;

		public Vector3 normal = normal;

		public Vector3 bitangent = bitangent;

		public Vector3 tangent = tangent;

		public Vector3 Transport(Vector3 newTangent, Vector3 newPosition)
		{
			Vector3 vector = newPosition - position;
			float num = Vector3.Dot(vector, vector);
			Vector3 vector2 = normal - 2f / (num + 1E-05f) * Vector3.Dot(vector, normal) * vector;
			Vector3 vector3 = tangent - 2f / (num + 1E-05f) * Vector3.Dot(vector, tangent) * vector;
			Vector3 vector4 = newTangent - vector3;
			float num2 = Vector3.Dot(vector4, vector4);
			Vector3 rhs = vector2 - 2f / (num2 + 1E-05f) * Vector3.Dot(vector4, vector2) * vector4;
			Vector3 vector5 = Vector3.Cross(newTangent, rhs);
			normal = rhs;
			bitangent = vector5;
			tangent = newTangent;
			position = newPosition;
			return normal;
		}
	}

	public struct Point(Vector3 position, Vector3 velocity, Vector3 tangent, Vector3 normal, Color color, float thickness, float lifetime)
	{
		public Vector3 position = position;

		public Vector3 velocity = velocity;

		public Vector3 tangent = tangent;

		public Vector3 normal = normal;

		public Color color = color;

		public float thickness = thickness;

		public float life = lifetime;

		public bool discontinuous = false;

		private static float CatmullRom(float p0, float p1, float p2, float p3, float t)
		{
			float num = t * t;
			return 0.5f * (2f * p1 + (0f - p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * num + (0f - p0 + 3f * p1 - 3f * p2 + p3) * num * t);
		}

		private static Color CatmullRomColor(Color p0, Color p1, Color p2, Color p3, float t)
		{
			return new Color(CatmullRom(p0[0], p1[0], p2[0], p3[0], t), CatmullRom(p0[1], p1[1], p2[1], p3[1], t), CatmullRom(p0[2], p1[2], p2[2], p3[2], t), CatmullRom(p0[3], p1[3], p2[3], p3[3], t));
		}

		private static Vector3 CatmullRom3D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			return new Vector3(CatmullRom(p0[0], p1[0], p2[0], p3[0], t), CatmullRom(p0[1], p1[1], p2[1], p3[1], t), CatmullRom(p0[2], p1[2], p2[2], p3[2], t));
		}

		public static Point Interpolate(Point a, Point b, Point c, Point d, float t)
		{
			return new Point(CatmullRom3D(a.position, b.position, c.position, d.position, t), CatmullRom3D(a.velocity, b.velocity, c.velocity, d.velocity, t), CatmullRom3D(a.tangent, b.tangent, c.tangent, d.tangent, t), CatmullRom3D(a.normal, b.normal, c.normal, d.normal, t), CatmullRomColor(a.color, b.color, c.color, d.color, t), CatmullRom(a.thickness, b.thickness, c.thickness, d.thickness, t), CatmullRom(a.life, b.life, c.life, d.life, t));
		}

		public static Point operator +(Point p1, Point p2)
		{
			return new Point(p1.position + p2.position, p1.velocity + p2.velocity, p1.tangent + p2.tangent, p1.normal + p2.normal, p1.color + p2.color, p1.thickness + p2.thickness, p1.life + p2.life);
		}

		public static Point operator -(Point p1, Point p2)
		{
			return new Point(p1.position - p2.position, p1.velocity - p2.velocity, p1.tangent - p2.tangent, p1.normal - p2.normal, p1.color - p2.color, p1.thickness - p2.thickness, p1.life - p2.life);
		}
	}

	public const float epsilon = 1E-05f;

	[Header("Overall")]
	[Tooltip("Whether to use world or local space to generate and simulate the trail.")]
	public Space space;

	[Tooltip("Whether to use regular time.")]
	public Timescale timescale;

	[Tooltip("How to align the trail geometry: facing the camera (view) of using the transform's rotation (local).")]
	public TrailAlignment alignment;

	[Tooltip("Thickness multiplier, in meters.")]
	public float thickness = 0.1f;

	[Tooltip("Amount of smoothing iterations applied to the trail shape.")]
	[Range(1f, 8f)]
	public int smoothness = 1;

	[Tooltip("Calculate accurate thickness at sharp corners.")]
	public bool highQualityCorners;

	[Range(0f, 12f)]
	public int cornerRoundness = 5;

	[Header("Lenght")]
	[Tooltip("How should the thickness of the curve evolve over its lenght. The horizontal axis is normalized lenght (in the [0,1] range) and the vertical axis is a thickness multiplier.")]
	public AnimationCurve thicknessOverLenght = AnimationCurve.Linear(0f, 1f, 0f, 1f);

	[Tooltip("How should vertex color evolve over the trail's length.")]
	public Gradient colorOverLenght = new Gradient();

	[Header("Time")]
	[Tooltip("How should the thickness of the curve evolve with its lifetime. The horizontal axis is normalized lifetime (in the [0,1] range) and the vertical axis is a thickness multiplier.")]
	public AnimationCurve thicknessOverTime = AnimationCurve.Linear(0f, 1f, 0f, 1f);

	[Tooltip("How should vertex color evolve over the trail's lifetime.")]
	public Gradient colorOverTime = new Gradient();

	[Header("Emission")]
	public bool emit = true;

	[Tooltip("Initial thickness of trail points when they are first spawned.")]
	public float initialThickness = 1f;

	[Tooltip("Initial color of trail points when they are first spawned.")]
	public Color initialColor = Color.white;

	[Tooltip("Initial velocity of trail points when they are first spawned.")]
	public Vector3 initialVelocity = Vector3.zero;

	[Tooltip("Minimum amount of time (in seconds) that must pass before spawning a new point.")]
	public float timeInterval = 0.025f;

	[Tooltip("Minimum distance (in meters) that must be left between consecutive points in the trail.")]
	public float minDistance = 0.025f;

	[Tooltip("Duration of the trail (in seconds).")]
	public float time = 2f;

	[Header("Physics")]
	[Tooltip("Toggles trail physics.")]
	public bool enablePhysics;

	[Tooltip("Amount of seconds pre-simulated before the trail appears. Useful when you want a trail to be already simulating when the game starts.")]
	public float warmup;

	[Tooltip("Gravity affecting the trail.")]
	public Vector3 gravity = Vector3.zero;

	[Tooltip("Amount of speed transferred from the transform to the trail. 0 means no velocity is transferred, 1 means 100% of the velocity is transferred.")]
	[Range(0f, 1f)]
	public float inertia;

	[Tooltip("Amount of temporal smoothing applied to the velocity transferred from the transform to the trail.")]
	[Range(0f, 1f)]
	public float velocitySmoothing = 0.75f;

	[Tooltip("Amount of damping applied to the trail's velocity. Larger values will slow down the trail more as time passes.")]
	[Range(0f, 1f)]
	public float damping = 0.75f;

	[Header("Rendering")]
	public Material[] materials = new Material[1];

	public ShadowCastingMode castShadows = ShadowCastingMode.On;

	public bool receiveShadows = true;

	public bool useLightProbes = true;

	[Header("Texture")]
	[Tooltip("How to apply the texture over the trail: stretch it all over its lenght, or tile it.")]
	public TextureMode textureMode;

	[Tooltip("When the texture mode is set to 'Tile', defines the width of each tile.")]
	public float uvFactor = 1f;

	[Tooltip("When the texture mode is set to 'Tile', defines where to begin tiling from: 0 means the start of the trail, 1 means the end.")]
	[Range(0f, 1f)]
	public float tileAnchor = 1f;

	[HideInInspector]
	public List<Point> points = new List<Point>();

	private List<Point> renderablePoints = new List<Point>();

	private List<int> discontinuities = new List<int>();

	private Mesh mesh_;

	private Vector3 velocity = Vector3.zero;

	private Vector3 prevPosition;

	private float speed;

	private float accumTime;

	private List<Vector3> vertices = new List<Vector3>();

	private List<Vector3> normals = new List<Vector3>();

	private List<Vector4> tangents = new List<Vector4>();

	private List<Vector2> uvs = new List<Vector2>();

	private List<Color> vertColors = new List<Color>();

	private List<int> tris = new List<int>();

	private float DeltaTime
	{
		get
		{
			if (timescale != Timescale.Unscaled)
			{
				return Time.deltaTime;
			}
			return Time.unscaledDeltaTime;
		}
	}

	private float FixedDeltaTime
	{
		get
		{
			if (timescale != Timescale.Unscaled)
			{
				return Time.fixedDeltaTime;
			}
			return Time.fixedUnscaledDeltaTime;
		}
	}

	public Mesh mesh => mesh_;

	public event Action onUpdatePoints;

	public void OnValidate()
	{
		time = Mathf.Max(time, 1E-05f);
		warmup = Mathf.Max(0f, warmup);
	}

	public void Awake()
	{
		Warmup();
	}

	private void OnEnable()
	{
		prevPosition = transform.position;
		velocity = Vector3.zero;
		mesh_ = new Mesh();
		mesh_.name = "ara_trail_mesh";
		mesh_.MarkDynamic();
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(UpdateTrailMesh));
	}

	private void OnDisable()
	{
		UnityEngine.Object.DestroyImmediate(mesh_);
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(UpdateTrailMesh));
	}

	public void Clear()
	{
		points.Clear();
	}

	private void UpdateVelocity()
	{
		if (DeltaTime > 0f)
		{
			velocity = Vector3.Lerp((transform.position - prevPosition) / DeltaTime, velocity, velocitySmoothing);
			speed = velocity.magnitude;
		}
		prevPosition = transform.position;
	}

	private void LateUpdate()
	{
		UpdateVelocity();
		EmissionStep(DeltaTime);
		SnapLastPointToTransform();
		UpdatePointsLifecycle();
		if (onUpdatePoints != null)
		{
			onUpdatePoints();
		}
	}

	private void EmissionStep(float time)
	{
		accumTime += time;
		if (accumTime >= timeInterval && emit)
		{
			Vector3 vector = ((space == Space.Self) ? transform.localPosition : transform.position);
			if (points.Count <= 1 || Vector3.Distance(vector, points[points.Count - 2].position) >= minDistance)
			{
				EmitPoint(vector);
				accumTime = 0f;
			}
		}
	}

	private void Warmup()
	{
		if (!Application.isPlaying || !enablePhysics)
		{
			return;
		}
		for (float num = warmup; num > FixedDeltaTime; num -= FixedDeltaTime)
		{
			PhysicsStep(FixedDeltaTime);
			EmissionStep(FixedDeltaTime);
			SnapLastPointToTransform();
			UpdatePointsLifecycle();
			if (onUpdatePoints != null)
			{
				onUpdatePoints();
			}
		}
	}

	private void PhysicsStep(float timestep)
	{
		float num = Mathf.Pow(1f - Mathf.Clamp01(damping), timestep);
		for (int i = 0; i < points.Count; i++)
		{
			Point value = points[i];
			value.velocity += gravity * timestep;
			value.velocity *= num;
			value.position += value.velocity * timestep;
			points[i] = value;
		}
	}

	private void FixedUpdate()
	{
		if (enablePhysics)
		{
			PhysicsStep(FixedDeltaTime);
		}
	}

	public void EmitPoint(Vector3 position)
	{
		points.Add(new Point(position, initialVelocity + velocity * inertia, transform.right, transform.forward, initialColor, initialThickness, time));
	}

	private void SnapLastPointToTransform()
	{
		if (points.Count > 0)
		{
			Point value = points[points.Count - 1];
			if (!emit)
			{
				value.discontinuous = true;
			}
			if (!value.discontinuous)
			{
				value.position = ((space == Space.Self) ? transform.localPosition : transform.position);
				value.normal = transform.forward;
				value.tangent = transform.right;
			}
			points[points.Count - 1] = value;
		}
	}

	private void UpdatePointsLifecycle()
	{
		for (int num = points.Count - 1; num >= 0; num--)
		{
			Point value = points[num];
			value.life -= DeltaTime;
			points[num] = value;
			if (value.life <= 0f)
			{
				if (smoothness <= 1)
				{
					points.RemoveAt(num);
				}
				else if (points[Mathf.Min(num + 1, points.Count - 1)].life <= 0f && points[Mathf.Min(num + 2, points.Count - 1)].life <= 0f)
				{
					points.RemoveAt(num);
				}
			}
		}
	}

	private void ClearMeshData()
	{
		mesh_.Clear();
		vertices.Clear();
		normals.Clear();
		tangents.Clear();
		uvs.Clear();
		vertColors.Clear();
		tris.Clear();
	}

	private void CommitMeshData()
	{
		mesh_.SetVertices(vertices);
		mesh_.SetNormals(normals);
		mesh_.SetTangents(tangents);
		mesh_.SetColors(vertColors);
		mesh_.SetUVs(0, uvs);
		mesh_.SetTriangles(tris, 0, calculateBounds: true);
	}

	private void RenderMesh(Camera cam)
	{
		for (int i = 0; i < materials.Length; i++)
		{
			Graphics.DrawMesh(mesh_, (space == Space.Self && transform.parent != null) ? transform.parent.localToWorldMatrix : Matrix4x4.identity, materials[i], gameObject.layer, cam, 0, null, castShadows, receiveShadows, null, useLightProbes);
		}
	}

	public float GetLenght(List<Point> input)
	{
		float num = 0f;
		for (int i = 0; i < input.Count - 1; i++)
		{
			num += Vector3.Distance(input[i].position, input[i + 1].position);
		}
		return num;
	}

	private List<Point> GetRenderablePoints(List<Point> input, int start, int end)
	{
		renderablePoints.Clear();
		if (smoothness <= 1)
		{
			for (int i = start; i <= end; i++)
			{
				renderablePoints.Add(points[i]);
			}
			return renderablePoints;
		}
		float num = 1f / (float)smoothness;
		for (int j = start; j < end; j++)
		{
			Point a = ((j == start) ? (points[start] + (points[start] - points[j + 1])) : points[j - 1]);
			Point d = ((j == end - 1) ? (points[end] + (points[end] - points[end - 1])) : points[j + 2]);
			for (int k = 0; k < smoothness; k++)
			{
				float t = (float)k * num;
				Point item = Point.Interpolate(a, points[j], points[j + 1], d, t);
				if (item.life > 0f)
				{
					renderablePoints.Add(item);
				}
			}
		}
		if (points[end].life > 0f)
		{
			renderablePoints.Add(points[end]);
		}
		return renderablePoints;
	}

	private CurveFrame InitializeCurveFrame(Vector3 point, Vector3 nextPoint)
	{
		Vector3 tangent = nextPoint - point;
		if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(tangent.normalized, transform.forward)), 1f))
		{
			tangent += transform.right * 0.01f;
		}
		return new CurveFrame(point, transform.forward, transform.up, tangent);
	}

	private void UpdateTrailMesh(Camera cam)
	{
		ClearMeshData();
		if (points.Count <= 1)
		{
			return;
		}
		Vector3 localCamPosition = ((space == Space.Self && transform.parent != null) ? transform.parent.InverseTransformPoint(cam.transform.position) : cam.transform.position);
		discontinuities.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].discontinuous || i == points.Count - 1)
			{
				discontinuities.Add(i);
			}
		}
		int start = 0;
		for (int j = 0; j < discontinuities.Count; j++)
		{
			UpdateSegmentMesh(points, start, discontinuities[j], localCamPosition);
			start = discontinuities[j] + 1;
		}
		CommitMeshData();
		RenderMesh(cam);
	}

	private void UpdateSegmentMesh(List<Point> input, int start, int end, Vector3 localCamPosition)
	{
		List<Point> list = GetRenderablePoints(input, start, end);
		if (list.Count <= 1)
		{
			return;
		}
		float num = Mathf.Max(GetLenght(list), 1E-05f);
		float num2 = 0f;
		float num3 = ((textureMode == TextureMode.Stretch) ? 0f : ((0f - uvFactor) * num * tileAnchor));
		Vector4 zero = Vector4.zero;
		Vector2 zero2 = Vector2.zero;
		bool flag = highQualityCorners && alignment != TrailAlignment.Local;
		CurveFrame curveFrame = InitializeCurveFrame(list[list.Count - 1].position, list[list.Count - 2].position);
		int item = 1;
		int item2 = 0;
		for (int num4 = list.Count - 1; num4 >= 0; num4--)
		{
			int index = Mathf.Max(num4 - 1, 0);
			int index2 = Mathf.Min(num4 + 1, list.Count - 1);
			Vector3 vector = list[index].position - list[num4].position;
			Vector3 vector2 = list[num4].position - list[index2].position;
			float magnitude = vector.magnitude;
			vector.Normalize();
			vector2.Normalize();
			Vector3 vector3 = ((alignment == TrailAlignment.Local) ? list[num4].tangent : (vector + vector2));
			vector3.Normalize();
			Vector3 vector4 = list[num4].normal;
			if (alignment != TrailAlignment.Local)
			{
				vector4 = ((alignment == TrailAlignment.View) ? (localCamPosition - list[num4].position) : curveFrame.Transport(vector3, list[num4].position));
			}
			vector4.Normalize();
			Vector3 vector5 = ((alignment == TrailAlignment.Velocity) ? curveFrame.bitangent : Vector3.Cross(vector3, vector4));
			vector5.Normalize();
			float num5 = num2 / num;
			float num6 = Mathf.Clamp01(1f - list[num4].life / time);
			num2 += magnitude;
			Color item3 = list[num4].color * colorOverTime.Evaluate(num6) * colorOverLenght.Evaluate(num5);
			num3 += uvFactor * ((textureMode == TextureMode.Stretch) ? (magnitude / num) : magnitude);
			float num7 = thickness * list[num4].thickness * thicknessOverTime.Evaluate(num6) * thicknessOverLenght.Evaluate(num5);
			Quaternion quaternion = Quaternion.identity;
			Vector3 vector6 = Vector3.zero;
			float num8 = 0f;
			float num9 = num7;
			Vector3 vector7 = vector5;
			if (flag)
			{
				Vector3 vector8 = ((num4 == 0) ? vector5 : Vector3.Cross(vector, Vector3.Cross(vector5, vector3)).normalized);
				if (cornerRoundness > 0)
				{
					vector7 = ((num4 == list.Count - 1) ? (-vector5) : Vector3.Cross(vector2, Vector3.Cross(vector5, vector3)).normalized);
					num8 = ((num4 == 0 || num4 == list.Count - 1) ? 1f : Mathf.Sign(Vector3.Dot(vector, -vector7)));
					float num10 = ((num4 == 0 || num4 == list.Count - 1) ? MathF.PI : Mathf.Acos(Mathf.Clamp(Vector3.Dot(vector8, vector7), -1f, 1f)));
					quaternion = Quaternion.AngleAxis(57.29578f * num10 / (float)cornerRoundness, vector4 * num8);
					vector6 = vector7 * num7 * num8;
				}
				if (vector8.sqrMagnitude > 0.1f)
				{
					num9 = num7 / Mathf.Max(Vector3.Dot(vector5, vector8), 0.15f);
				}
			}
			if (flag && cornerRoundness > 0)
			{
				if (num8 > 0f)
				{
					vertices.Add(list[num4].position + vector7 * num7);
					vertices.Add(list[num4].position - vector5 * num9);
				}
				else
				{
					vertices.Add(list[num4].position + vector5 * num9);
					vertices.Add(list[num4].position - vector7 * num7);
				}
			}
			else
			{
				vertices.Add(list[num4].position + vector5 * num9);
				vertices.Add(list[num4].position - vector5 * num9);
			}
			normals.Add(-vector4);
			normals.Add(-vector4);
			zero = -vector5;
			zero.w = 1f;
			tangents.Add(zero);
			tangents.Add(zero);
			vertColors.Add(item3);
			vertColors.Add(item3);
			zero2.Set(num3, 0f);
			uvs.Add(zero2);
			zero2.Set(num3, 1f);
			uvs.Add(zero2);
			if (num4 < list.Count - 1)
			{
				int num11 = vertices.Count - 1;
				tris.Add(num11);
				tris.Add(item);
				tris.Add(item2);
				tris.Add(item2);
				tris.Add(num11 - 1);
				tris.Add(num11);
			}
			item = vertices.Count - 1;
			item2 = vertices.Count - 2;
			if (flag && cornerRoundness > 0)
			{
				for (int i = 0; i <= cornerRoundness; i++)
				{
					vertices.Add(list[num4].position + vector6);
					normals.Add(-vector4);
					tangents.Add(zero);
					vertColors.Add(item3);
					zero2.Set(num3, (!(num8 > 0f)) ? 1 : 0);
					uvs.Add(zero2);
					int num12 = vertices.Count - 1;
					tris.Add(num12);
					tris.Add(item);
					tris.Add(item2);
					if (num8 > 0f)
					{
						item2 = num12;
					}
					else
					{
						item = num12;
					}
					vector6 = quaternion * vector6;
				}
			}
		}
	}
}

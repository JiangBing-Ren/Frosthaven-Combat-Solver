using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SETUtil;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace MeshSlicing;

public sealed class VerticalMeshSlicer : IDisposable
{
	public delegate void OnMeshSlicingCompletion();

	public struct MeshData
	{
		public List<Vector3> Vertices;

		public List<int> Triangles;

		public List<Vector3> Normals;

		internal static MeshData FromMesh(Mesh mesh)
		{
			return new MeshData
			{
				Vertices = new List<Vector3>(mesh.vertices),
				Triangles = new List<int>(mesh.triangles)
			};
		}

		internal Mesh ToMesh()
		{
			Mesh mesh = new Mesh
			{
				vertices = Vertices.ToArray(),
				normals = Normals.ToArray(),
				triangles = Triangles.ToArray()
			};
			if (mesh.bounds.size.sqrMagnitude > 0f)
			{
				mesh.RecalculateBounds();
			}
			return mesh;
		}
	}

	private class Target
	{
		public MeshRenderer Renderer;

		public MeshFilter MeshFilter;

		public Material FillerMaterial;

		public Target(MeshRenderer renderer, MeshFilter meshFilter, Material fillerMaterial)
		{
			Renderer = renderer;
			MeshFilter = meshFilter;
			FillerMaterial = fillerMaterial;
		}
	}

	private HashSet<UnityEngine.Object> _dynamicResources = new HashSet<UnityEngine.Object>();

	private Queue<(Target target, MeshData meshData, Matrix4x4 localToWorld, float plane)> _queue = new Queue<(Target, MeshData, Matrix4x4, float)>();

	private Queue<Target> _cachedQueue = new Queue<Target>();

	private List<(Target target, Task<IEnumerable<MeshData>> job)> _runningJobs = new List<(Target, Task<IEnumerable<MeshData>>)>();

	private Queue<(Target target, IEnumerable<MeshData> slices)> _results = new Queue<(Target, IEnumerable<MeshData>)>();

	private int _debugStopAt;

	private float _plane;

	private int _jobsBudget;

	private int _instantiationBudget;

	private readonly bool synchronous;

	private Dictionary<int, Mesh> _cache = new Dictionary<int, Mesh>();

	private CustomSampler samplerAlloc;

	private CustomSampler samplerSlice;

	private CustomSampler samplerTraceIntersection;

	private CustomSampler samplerMergeByProximity;

	private CustomSampler samplerBuildTrianglesArray;

	private const float Z_FIX_DELTA = 0.002f;

	private const float VERTEX_PROXIMITY_DELTA = 0.0001f;

	private const int WHILE_SAFETY = 32000;

	private const int CONVEX_SHAPE_BUFFER_SIZE = 8000;

	public const string PROC_MESH_NAME = "_ProceduralMeshSlice";

	private int _complete;

	private int _expected;

	public OnMeshSlicingCompletion OnMeshSlicingComplete;

	public bool Idle
	{
		get
		{
			if (_queue.Count <= 0 && _results.Count <= 0 && _runningJobs.Count <= 0)
			{
				return _cachedQueue.Count <= 0;
			}
			return false;
		}
	}

	public bool CompletedAtLeastOnce { get; private set; }

	public VerticalMeshSlicer(float plane, int jobsBudget = 2, int instantiationBudget = 7, bool synchronous = false, int debugStopAt = -1)
	{
		_debugStopAt = debugStopAt;
		_plane = plane;
		_jobsBudget = Mathf.Min(jobsBudget, Environment.ProcessorCount);
		_instantiationBudget = instantiationBudget;
		this.synchronous = synchronous;
		samplerAlloc = CustomSampler.Create("Allocate");
		samplerSlice = CustomSampler.Create("Slice");
		samplerTraceIntersection = CustomSampler.Create("Trace Intersection");
		samplerMergeByProximity = CustomSampler.Create("Merge by Proximity");
		samplerBuildTrianglesArray = CustomSampler.Create("Build Triangles Array");
	}

	public void Dispose()
	{
		ClearDynamicResources();
		_cache.Clear();
		_queue.Clear();
		_cachedQueue.Clear();
		_runningJobs.Clear();
		_results.Clear();
	}

	public void Tick()
	{
		for (int i = 0; i < _runningJobs.Count; i++)
		{
			(Target, Task<IEnumerable<MeshData>>) item = _runningJobs[i];
			if (item.Item2.IsCompleted)
			{
				_results.Enqueue((item.Item1, item.Item2.Result));
				item.Item2.Dispose();
				_runningJobs.Remove(item);
				i--;
			}
		}
		while (_queue.Count > 0 && _runningJobs.Count < _jobsBudget)
		{
			(Target target, MeshData meshData, Matrix4x4 localToWorld, float plane) q = _queue.Dequeue();
			if (!q.target.MeshFilter)
			{
				continue;
			}
			Mesh sharedMesh = q.target.MeshFilter.sharedMesh;
			if (!_cache.ContainsKey(HashOf(sharedMesh, q.target.Renderer.transform)))
			{
				if (!synchronous)
				{
					_runningJobs.Add((q.target, Task.Run(() => AsyncSlice(q.meshData, q.localToWorld, q.plane))));
				}
				else
				{
					_results.Enqueue((q.target, AsyncSlice(q.meshData, q.localToWorld, q.plane)));
				}
			}
			else
			{
				_cachedQueue.Enqueue(q.target);
			}
		}
		for (int num = 0; num < _instantiationBudget; num++)
		{
			if (_cachedQueue.Count <= 0)
			{
				break;
			}
			Target target = _cachedQueue.Dequeue();
			if ((bool)target.MeshFilter)
			{
				Mesh sharedMesh2 = target.MeshFilter.sharedMesh;
				int num2 = HashOf(target.MeshFilter.sharedMesh, target.Renderer.transform);
				if (_cache.ContainsKey(num2))
				{
					SpawnSliceRenderer(target.FillerMaterial, _cache[num2], target.Renderer);
				}
				else
				{
					Debug.LogWarning($"Cache miss {num2} for {sharedMesh2.name} at {target.Renderer.name}. Did you move it?");
				}
			}
			_complete++;
		}
		for (int num3 = 0; num3 < _instantiationBudget; num3++)
		{
			if (_results.Count <= 0)
			{
				break;
			}
			if (!_results.TryDequeue(out (Target, IEnumerable<MeshData>) result))
			{
				break;
			}
			if ((bool)result.Item1.Renderer)
			{
				int key = HashOf(result.Item1.MeshFilter.sharedMesh, result.Item1.Renderer.transform);
				foreach (MeshData item2 in result.Item2)
				{
					Mesh mesh = item2.ToMesh();
					SpawnSliceRenderer(result.Item1.FillerMaterial, mesh, result.Item1.Renderer);
					AddToCache(key, mesh);
				}
			}
			_complete++;
		}
		if (_expected > 0 && _complete == _expected && Idle)
		{
			Debug.Log($"Slice generation complete. Generated {_complete} jobs at {Time.timeSinceLevelLoad} after level loaded. From cache {_cache.Count}. Now idle.");
			OnMeshSlicingComplete?.Invoke();
			CompletedAtLeastOnce = true;
			_expected = 0;
			_complete = 0;
		}
	}

	private List<(int startVtx, int endVtx, int[] triangleIndices)> GetEdges(MeshData mesh)
	{
		Dictionary<(int, int), HashSet<int>> dictionary = new Dictionary<(int, int), HashSet<int>>();
		int num = mesh.Triangles.Count / 3;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 3;
			int num3 = mesh.Triangles[num2];
			int num4 = mesh.Triangles[num2 + 1];
			int num5 = mesh.Triangles[num2 + 2];
			(int, int) key = (Math.Min(num3, num4), Math.Max(num3, num4));
			(int, int) key2 = (Math.Min(num4, num5), Math.Max(num4, num5));
			(int, int) key3 = (Math.Min(num5, num3), Math.Max(num5, num3));
			if (dictionary.ContainsKey(key))
			{
				dictionary[key].Add(i);
			}
			else
			{
				dictionary.Add(key, new HashSet<int> { i });
			}
			if (dictionary.ContainsKey(key2))
			{
				dictionary[key2].Add(i);
			}
			else
			{
				dictionary.Add(key2, new HashSet<int> { i });
			}
			if (dictionary.ContainsKey(key3))
			{
				dictionary[key3].Add(i);
				continue;
			}
			dictionary.Add(key3, new HashSet<int> { i });
		}
		List<(int, int, int[])> list = new List<(int, int, int[])>();
		foreach (KeyValuePair<(int, int), HashSet<int>> item in dictionary)
		{
			int[] array = new int[item.Value.Count];
			item.Value.CopyTo(array);
			list.Add((item.Key.Item1, item.Key.Item2, array));
		}
		return list;
	}

	private static bool ArePointsCollinear2Dxz(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return Mathf.Approximately((p2.y - p1.z) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.z - p2.z), 0f);
	}

	private IEnumerable<MeshData> AsyncSlice(MeshData meshData, Matrix4x4 localToWorld, float plane)
	{
		_ = new int[8000];
		_ = new bool[8000];
		List<(int, int, int[])> edges = GetEdges(meshData);
		List<(int, Vector3)> list = new List<(int, Vector3)>();
		for (int i = 0; i < edges.Count; i++)
		{
			(int, int, int[]) tuple = edges[i];
			Vector3 lineStart = localToWorld.MultiplyPoint3x4(meshData.Vertices[tuple.Item1]);
			Vector3 lineEnd = localToWorld.MultiplyPoint3x4(meshData.Vertices[tuple.Item2]);
			Vector3? vector = IntersectFlatPlane(plane, lineStart, lineEnd);
			if (vector.HasValue)
			{
				list.Add((i, vector.Value));
			}
		}
		List<(int, Vector3, int, Vector3)> list2 = new List<(int, Vector3, int, Vector3)>();
		for (int j = 0; j < list.Count; j++)
		{
			(int, Vector3) tuple2 = list[j];
			for (int k = 0; k < list.Count; k++)
			{
				(int, Vector3) tuple3 = list[k];
				if (tuple2.Item1 == tuple3.Item1)
				{
					continue;
				}
				bool flag = false;
				for (int l = 0; l < list2.Count; l++)
				{
					(int, Vector3, int, Vector3) tuple4 = list2[l];
					if ((tuple4.Item1 == tuple2.Item1 && tuple4.Item3 == tuple3.Item1) || (tuple4.Item1 == tuple3.Item1 && tuple4.Item3 == tuple2.Item1))
					{
						flag = true;
						break;
					}
				}
				if (!flag && HaveSharedTriangle(edges[tuple2.Item1].Item3, edges[tuple3.Item1].Item3))
				{
					list2.Add((tuple2.Item1, tuple2.Item2, tuple3.Item1, tuple3.Item2));
				}
			}
		}
		List<List<Vector3>> list3 = new List<List<Vector3>>
		{
			new List<Vector3>()
		};
		bool[] closed = new bool[list2.Count];
		for (int m = 0; m < list2.Count; m++)
		{
			closed[m] = false;
		}
		if (closed.Length != 0)
		{
			closed[0] = true;
			list3[0].Add(list2[0].Item2);
			list3[0].Add(list2[0].Item4);
			Vector3 vector2 = list2[0].Item4;
			_ = list2[0];
			int num = 0;
			while (AnyOpenLeft() && num < 32000)
			{
				if (num == 31999)
				{
					throw new Exception("Too many loops.");
				}
				bool flag2 = false;
				List<Vector3> list4 = list3[list3.Count - 1];
				for (int n = 0; n < closed.Length; n++)
				{
					if (closed[n])
					{
						continue;
					}
					if (Vector3.SqrMagnitude(vector2 - list2[n].Item2) < 0.0001f)
					{
						flag2 = true;
						closed[n] = true;
						list4.Add(list2[n].Item4);
						vector2 = list2[n].Item4;
					}
					else if (Vector3.SqrMagnitude(vector2 - list2[n].Item4) < 0.0001f)
					{
						flag2 = true;
						closed[n] = true;
						list4.Add(list2[n].Item2);
						vector2 = list2[n].Item2;
					}
					else if (Vector3.SqrMagnitude(list4[0] - list2[n].Item2) < 0.0001f)
					{
						flag2 = true;
						closed[n] = true;
						if (Vector3.SqrMagnitude(list4[0] - list2[n].Item4) > 0.0001f)
						{
							list4.Insert(0, list2[n].Item4);
						}
					}
					else if (Vector3.SqrMagnitude(list4[0] - list2[n].Item4) < 0.0001f)
					{
						flag2 = true;
						closed[n] = true;
						if (Vector3.SqrMagnitude(list4[0] - list2[n].Item2) > 0.0001f)
						{
							list4.Insert(0, list2[n].Item2);
						}
					}
				}
				if (!flag2)
				{
					int num2 = closed.FindIndex((bool a) => !a);
					(int, Vector3, int, Vector3) tuple5 = list2[num2];
					list3.Add(new List<Vector3> { tuple5.Item2, tuple5.Item4 });
					closed[num2] = true;
					vector2 = tuple5.Item2;
				}
				num++;
			}
		}
		if (list3.Count == 0)
		{
			return new MeshData[0];
		}
		int num3 = 0;
		for (int num4 = 0; num4 < list3.Count; num4++)
		{
			int count = list3[num4].Count;
			if (count >= 3)
			{
				num3 += count;
			}
		}
		if (num3 < 3)
		{
			return new MeshData[0];
		}
		int capacity = (num3 - 2) * 3;
		new List<int>(capacity);
		MeshData meshData2 = new MeshData
		{
			Vertices = new List<Vector3>(num3),
			Triangles = new List<int>(capacity),
			Normals = new List<Vector3>(num3)
		};
		System.Random random = new System.Random();
		for (int num5 = 0; num5 < list3.Count; num5++)
		{
			List<Vector3> list5 = list3[num5];
			if (list5.Count < 3)
			{
				continue;
			}
			List<Vector3> list6 = new List<Vector3>(list5.Count);
			int num6 = list5.Count - 1;
			list6.Add(list5[0]);
			for (int num7 = 1; num7 < list5.Count; num7++)
			{
				if (!ArePointsCollinear2Dxz(list5[num7 - 1], list5[num7], list5[(num7 + 1) % list5.Count]) && !(Vector3.SqrMagnitude(list5[num7 - 1] - list5[num7]) < 0.0001f) && (num7 != num6 || !(Vector3.SqrMagnitude(list5[num6] - list5[0]) < 0.0001f)))
				{
					list6.Add(list5[num7]);
				}
			}
			float num8 = SignedArea2Dx2(list6);
			Vector2[] array = new Vector2[list6.Count];
			for (int num9 = 0; num9 < list6.Count; num9++)
			{
				Vector3 vector3 = list6[(num8 < 0f) ? (list6.Count - num9 - 1) : num9];
				array[num9] = new Vector2(vector3.x, vector3.z);
			}
			(IList<float2>, int[]) tuple6 = new Triangulator(new SinglePolygonData(array)).Execute(_debugStopAt);
			float y = list6[0].y - (float)random.NextDouble() * 0.002f;
			list6 = tuple6.Item1.Select((float2 a) => localToWorld.inverse.MultiplyPoint3x4(new Vector3(a.x, y, a.y))).ToList();
			int count2 = meshData2.Vertices.Count;
			for (int num10 = 0; num10 < list6.Count; num10++)
			{
				meshData2.Vertices.Add(list6[num10]);
				meshData2.Normals.Add(Vector3.up);
			}
			for (int num11 = 0; num11 < tuple6.Item2.Length; num11++)
			{
				meshData2.Triangles.Add(count2 + tuple6.Item2[num11]);
			}
		}
		return new MeshData[1] { meshData2 };
		bool AnyOpenLeft()
		{
			for (int num12 = 0; num12 < closed.Length; num12++)
			{
				if (!closed[num12])
				{
					return true;
				}
			}
			return false;
		}
	}

	private float SignedArea2Dx2(List<Vector3> verts2D)
	{
		float num = 0f;
		for (int i = 0; i < verts2D.Count; i++)
		{
			float x = verts2D[i].x;
			float z = verts2D[i].z;
			int index = (i + 1) % verts2D.Count;
			float x2 = verts2D[index].x;
			float z2 = verts2D[index].z;
			num += x * z2 - x2 * z;
		}
		return num;
	}

	public void Push((MeshRenderer renderer, MeshFilter filter)[] renderers, Material material, bool noCache = false)
	{
		if (noCache)
		{
			_cache.Clear();
		}
		_expected += renderers.Length;
		for (int i = 0; i < renderers.Length; i++)
		{
			(MeshRenderer, MeshFilter) tuple = renderers[i];
			Transform transform = tuple.Item2.gameObject.transform.Find("_ProceduralMeshSlice");
			if (noCache && (bool)transform)
			{
				SceneUtil.SmartDestroy(transform.gameObject);
				transform = null;
			}
			if ((bool)transform || !BoundsIntersectPlane(tuple.Item1, _plane))
			{
				_expected--;
				continue;
			}
			Material fillerMaterial = material;
			if (tuple.Item1.TryGetComponent<MeshSliceMaterialOverride>(out var component))
			{
				fillerMaterial = component.Material;
			}
			Mesh sharedMesh = tuple.Item2.sharedMesh;
			if (_cache.TryGetValue(HashOf(sharedMesh, tuple.Item1.transform), out var value))
			{
				if ((bool)value)
				{
					_cachedQueue.Enqueue(new Target(tuple.Item1, tuple.Item2, fillerMaterial));
					continue;
				}
				_cache.Remove(HashOf(sharedMesh, tuple.Item1.transform));
			}
			MeshData item = MeshData.FromMesh(sharedMesh);
			Matrix4x4 localToWorldMatrix = tuple.Item1.transform.localToWorldMatrix;
			_queue.Enqueue((new Target(tuple.Item1, tuple.Item2, fillerMaterial), item, localToWorldMatrix, _plane));
		}
	}

	private int HashOf(Mesh sharedMesh, Transform transform)
	{
		return ((sharedMesh.GetHashCode() * 23 + (int)(transform.position.y * 100f)) * 23 + (int)(Vector3.Angle(transform.up, Vector3.up) * 100f)) * 23 + (int)(transform.lossyScale.y * 100f);
	}

	public void ClearDynamicResources()
	{
		foreach (UnityEngine.Object dynamicResource in _dynamicResources)
		{
			if ((bool)dynamicResource)
			{
				SceneUtil.SmartDestroy(dynamicResource);
			}
		}
		_dynamicResources.Clear();
	}

	private void AddToCache(int key, Mesh mesh)
	{
		_cache[key] = mesh;
	}

	private void SpawnSliceRenderer(Material material, Mesh mesh, MeshRenderer parentRenderer)
	{
		GameObject gameObject = new GameObject("_ProceduralMeshSlice");
		gameObject.hideFlags = HideFlags.DontSave;
		gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = material;
		meshRenderer.renderingLayerMask = parentRenderer.renderingLayerMask;
		meshRenderer.gameObject.layer = parentRenderer.gameObject.layer;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		Transform transform = gameObject.transform;
		Transform transform2 = parentRenderer.transform;
		transform.SetParent(transform2, worldPositionStays: false);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		_dynamicResources.Add(gameObject);
		_dynamicResources.Add(mesh);
	}

	private static bool BoundsIntersectPlane(MeshRenderer renderer, float plane)
	{
		Bounds bounds = renderer.bounds;
		if (bounds.extents.y + bounds.center.y > plane)
		{
			return bounds.center.y - bounds.extents.y < plane;
		}
		return false;
	}

	private static bool HaveSharedTriangle(int[] triangleIndices1, int[] triangleIndices2)
	{
		foreach (int num in triangleIndices1)
		{
			foreach (int num2 in triangleIndices2)
			{
				if (num == num2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static Vector3? IntersectFlatPlane(float plane, Vector3 lineStart, Vector3 lineEnd)
	{
		if ((lineStart.y > plane && lineEnd.y > plane) || (lineStart.y < plane && lineEnd.y < plane))
		{
			return null;
		}
		return Vector3.Lerp(lineStart, lineEnd, Mathf.Abs(plane - lineStart.y) / Mathf.Abs(lineEnd.y - lineStart.y));
	}
}

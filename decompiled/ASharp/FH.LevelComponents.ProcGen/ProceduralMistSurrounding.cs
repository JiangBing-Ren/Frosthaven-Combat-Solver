using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FH.ApparanceBaking;
using FH.LevelComponents.TopDownProjection;
using FH.PostProcessing;
using FH.SeasonTech;
using IKPn;
using Libs.Utils.Maths;
using MeshSlicing;
using SETUtil;
using SETUtil.Common;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace FH.LevelComponents.ProcGen;

[ExecuteAlways]
public class ProceduralMistSurrounding : MonoBehaviour
{
	private class PartQuota
	{
		public Vector2[] Area;

		public int Quota;

		public PartQuota(Vector2[] area, int quota)
		{
			Area = area;
			Quota = quota;
		}
	}

	private class Handle
	{
		public Task Task;

		public List<CMeshData> Meshes = new List<CMeshData>();

		public bool Complete => Task?.IsCompleted ?? false;
	}

	public struct CMeshData
	{
		public Vector3[] Vertices;

		public Color[] Colors;

		public List<int> Triangles;

		public int MaterialID;
	}

	private const int MATERIAL_ID_TRANSPARENT = 0;

	private const int MATERIAL_ID_SOLID = 1;

	private const float DBG_DRAW_LIFETIME = 70f;

	private List<Handle> _handles = new List<Handle>();

	private List<UnityEngine.Object> dynamicResources = new List<UnityEngine.Object>();

	private ISurroundingOverride _override;

	private Queue<UnityEngine.Object> _disposeQueue = new Queue<UnityEngine.Object>();

	[Range(0f, 1f)]
	public float Decay = 0.5f;

	public float HeightMax = 5f;

	public float HeightMin;

	public int Size = 200;

	[FormerlySerializedAs("Scale")]
	public float Density = 1f;

	public float BaseHeight = -1f;

	public float BaseSize = 500f;

	public float ClipHeight = 0.5f;

	[SerializeField]
	private Material SolidMaterial;

	[SerializeField]
	private Material TransparentMaterial;

	[SerializeField]
	private Material _clippingCelOverrideMat;

	[SerializeField]
	private List<Material> _clippingCelPostProcess;

	[SerializeField]
	private Transform _overrideReflectionProbe;

	[SerializeField]
	private BackgroundMistClutter[] _backgroundClutter;

	private bool _backgroundSpawnedAlready;

	private Cel clippingCel;

	private const int TARGET_RES = 256;

	private static int _BgClipTexProperty = Shader.PropertyToID("_Global_BgMist_ClippingCel");

	private static int[,] _marchingSquaresLookup = new int[16, 12]
	{
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			0, 1, 7, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			1, 2, 3, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			0, 2, 3, 0, 3, 7, -1, -1, -1, -1,
			-1, -1
		},
		{
			3, 4, 5, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			0, 1, 7, 1, 3, 7, 7, 3, 5, 3,
			4, 5
		},
		{
			1, 2, 4, 1, 4, 5, -1, -1, -1, -1,
			-1, -1
		},
		{
			0, 4, 7, 7, 4, 5, 0, 2, 4, -1,
			-1, -1
		},
		{
			7, 5, 6, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			0, 1, 5, 0, 5, 6, -1, -1, -1, -1,
			-1, -1
		},
		{
			1, 2, 3, 1, 3, 7, 7, 3, 5, 7,
			5, 6
		},
		{
			0, 2, 6, 2, 3, 6, 3, 5, 6, -1,
			-1, -1
		},
		{
			7, 3, 4, 7, 4, 6, -1, -1, -1, -1,
			-1, -1
		},
		{
			0, 4, 6, 0, 1, 3, 0, 3, 4, -1,
			-1, -1
		},
		{
			1, 2, 7, 2, 6, 7, 2, 4, 6, -1,
			-1, -1
		},
		{
			0, 2, 4, 0, 4, 6, -1, -1, -1, -1,
			-1, -1
		}
	};

	private static int[,] _marchingVertexColors = new int[16, 8]
	{
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 1, 0, 0, 0, 0, 0 },
		{ 1, 1, 1, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 1, 0, 0, 0 },
		{ 1, 0, 0, 0, 1, 0, 0, 0 },
		{ 0, 0, 1, 1, 1, 0, 0, 0 },
		{ 1, 1, 1, 1, 1, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 1, 0 },
		{ 1, 0, 0, 0, 0, 0, 1, 1 },
		{ 0, 0, 1, 0, 0, 0, 1, 0 },
		{ 1, 1, 1, 0, 0, 0, 1, 1 },
		{ 0, 0, 0, 0, 1, 1, 1, 0 },
		{ 1, 0, 0, 0, 1, 1, 1, 1 },
		{ 0, 0, 1, 1, 1, 1, 1, 0 },
		{ 1, 1, 1, 1, 1, 1, 1, 1 }
	};

	private static (int x, int y)[] _saddleVertexRelations = new (int, int)[8]
	{
		(0, 0),
		(1, 0),
		(2, 0),
		(2, 1),
		(2, 2),
		(1, 2),
		(0, 2),
		(0, 1)
	};

	private static (int a, int b)[] _interpolationLookup = new (int, int)[8]
	{
		(0, 0),
		(0, 1),
		(1, 1),
		(1, 2),
		(2, 2),
		(2, 3),
		(3, 3),
		(0, 3)
	};

	[SerializeField]
	[Range(0f, 15f)]
	private int _dbgMarchingSegmentDraw;

	public bool Done
	{
		get
		{
			if (_handles.Count == 0)
			{
				return dynamicResources.Count > 0;
			}
			return false;
		}
	}

	private void OnDestroy()
	{
		Clear();
		if (clippingCel != null)
		{
			clippingCel.Dispose();
			clippingCel = null;
			Shader.SetGlobalTexture(_BgClipTexProperty, null);
		}
		FlushDisposeQueue();
	}

	private void FlushDisposeQueue()
	{
		while (_disposeQueue.Count > 0)
		{
			UnityEngine.Object obj = _disposeQueue.Dequeue();
			if (obj != null)
			{
				SceneUtil.SmartDestroy(obj);
			}
		}
	}

	private void SmartDispose(UnityEngine.Object obj)
	{
		if (!(obj == null))
		{
			if (Application.isPlaying)
			{
				SceneUtil.SmartDestroy(obj);
				return;
			}
			obj.hideFlags = HideFlags.DontSave;
			_disposeQueue.Enqueue(obj);
		}
	}

	public void Generate(MultiplaneSystem multiplaneSystem, IEnumerable<ProceduralMapTile> maps, Bounds levelBounds, ref VerticalMeshSlicer.OnMeshSlicingCompletion onMeshSlicingComplete)
	{
		if (_override == null)
		{
			_override = GetComponent<ISurroundingOverride>();
		}
		Handle handle = new Handle();
		List<Vector2>[] allShapes = (from a in maps.SelectMany((ProceduralMapTile a) => a.GetComponentsInChildren<IMapShapeBrush>(includeInactive: true))
			select a.GetShape()).ToArray();
		List<Vector2>[] shapes = (from a in maps.SelectMany((ProceduralMapTile a) => a.GetComponentsInChildren<ProceduralMapTile>(includeInactive: true))
			select a.GetShape()).ToArray();
		if (allShapes.Length == 0)
		{
			return;
		}
		if (clippingCel == null && (bool)multiplaneSystem)
		{
			int num = 10;
			clippingCel = multiplaneSystem.NewCel(new CelParams
			{
				LevelBounds = new Bounds(new Vector3(levelBounds.center.x, num / 2, levelBounds.center.z), new Vector3(levelBounds.size.x, num, levelBounds.size.z)),
				Resolution = Vector2Int.one * 256,
				TextureFormat = RenderTextureFormat.R8,
				DepthBufferPrecision = 16,
				ReplacementMaterial = _clippingCelOverrideMat,
				PostProcessing = _clippingCelPostProcess,
				Content = new KronosScenarioCelContent(maps, null),
				MinClipHeight = -2f,
				CullingMask = 0
			});
			Shader.SetGlobalTexture(_BgClipTexProperty, clippingCel.RenderTextureTarget);
		}
		float ovHeightMax = _override?.GetHeightMax(HeightMax) ?? HeightMax;
		float ovHeightMin = _override?.GetHeightMin(HeightMin) ?? HeightMin;
		handle.Task = TaskUtil.RunWithExceptions(() =>
		{
			int num2 = Size / 2;
			Vector3[] array = new Vector3[Size * Size];
			Color[] array2 = new Color[Size * Size];
			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					Vector2 v2pt = new Vector2(i - num2, j - num2);
					int num3 = i + Size * j;
					bool flag = allShapes.Any((List<Vector2> polygon) => Utl.IsPointInside2DShape(v2pt / Density, polygon));
					array[num3] = new Vector3(v2pt.x / Density, flag ? ovHeightMax : ovHeightMin, v2pt.y / Density);
					array2[num3] = (flag ? Color.black : Color.white);
				}
			}
			for (int num4 = Size / 2; num4 > 1; num4--)
			{
				for (int num5 = Size / 2; num5 > 1; num5--)
				{
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, num4, num5, Decay);
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, Size - num4, num5, Decay);
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, num4, Size - num5, Decay);
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, Size - num4, Size - num5, Decay);
				}
			}
			for (int num6 = 1; num6 < Size - 1; num6++)
			{
				for (int num7 = 1; num7 < Size - 1; num7++)
				{
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, num6, num7, Decay);
				}
			}
			for (int num8 = Size - 2; num8 > 0; num8--)
			{
				for (int num9 = Size - 2; num9 > 0; num9--)
				{
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, num8, num9, Decay);
				}
			}
			for (int num10 = 1; num10 < Size - 1; num10++)
			{
				for (int num11 = 1; num11 < Size - 1; num11++)
				{
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, num11, num10, Decay);
				}
			}
			for (int num12 = Size - 2; num12 > 0; num12--)
			{
				for (int num13 = Size - 2; num13 > 0; num13--)
				{
					BlurVertex(array, Size, ovHeightMin, ovHeightMax, num13, num12, Decay);
				}
			}
			int num14 = Size - 1;
			int num15 = num14 * num14;
			int num16 = num14 * 2;
			List<int> list = new List<int>(num15 * 12);
			List<Vector3> list2 = new List<Vector3>(3 * num15);
			List<Color> list3 = new List<Color>(list2.Count);
			int[] array3 = new int[num16 * num16];
			for (int num17 = 0; num17 < array3.Length; num17++)
			{
				array3[num17] = -1;
			}
			float num18 = ovHeightMin + 0.01f;
			for (int num19 = 0; num19 < Size - 1; num19++)
			{
				for (int num20 = 0; num20 < Size - 1; num20++)
				{
					int num21 = Size * num19;
					int num22 = Size * (num19 + 1);
					int num23 = num20 + num21;
					int num24 = num20 + num22;
					int num25 = num20 + 1 + num22;
					int num26 = num20 + 1 + num21;
					int[] array4 = new int[4] { num23, num26, num25, num24 };
					Mathf.Min(array[num23].y, array[num24].y, array[num25].y, array[num26].y);
					if (Mathf.Max(array[num23].y, array[num24].y, array[num25].y, array[num26].y) > num18)
					{
						int num27 = 0;
						for (int num28 = 0; num28 < 4; num28++)
						{
							if (array[array4[num28]].y < ovHeightMax)
							{
								num27 |= 1 << num28;
							}
						}
						if (num27 != 0)
						{
							for (int num29 = 0; num29 < 4; num29++)
							{
								int num30 = num29 * 3;
								if (_marchingSquaresLookup[num27, num30] == -1)
								{
									break;
								}
								for (int num31 = 2; num31 >= 0; num31--)
								{
									int num32 = _marchingSquaresLookup[num27, num30 + num31];
									(int, int) tuple = _saddleVertexRelations[num32];
									int num33 = num20 * 2 + tuple.Item1 + num16 * (num19 * 2 + tuple.Item2);
									int num34 = array3[num33];
									if (num34 == -1)
									{
										(int, int) tuple2 = _interpolationLookup[num32];
										Vector3 vector = array[array4[tuple2.Item1]];
										Vector3 vector2 = array[array4[tuple2.Item2]];
										Vector3 item = (vector + vector2) / 2f;
										int count = list2.Count;
										list2.Add(item);
										array3[num33] = count;
										num34 = count;
										list3.Add((_marchingVertexColors[num27, num32] == 0) ? Color.black : Color.white);
									}
									list.Add(num34);
								}
							}
						}
					}
				}
			}
			handle.Meshes.Add(new CMeshData
			{
				Vertices = list2.ToArray(),
				Colors = list3.ToArray(),
				Triangles = list,
				MaterialID = 0
			});
			handle.Meshes.Add(new CMeshData
			{
				Vertices = new Vector3[4]
				{
					new Vector3(BaseSize, BaseHeight, 0f - BaseSize),
					new Vector3(0f - BaseSize, BaseHeight, 0f - BaseSize),
					new Vector3(0f - BaseSize, BaseHeight, BaseSize),
					new Vector3(BaseSize, BaseHeight, BaseSize)
				},
				Colors = new Color[4]
				{
					Color.white,
					Color.white,
					Color.white,
					Color.white
				},
				Triangles = new List<int> { 0, 1, 3, 3, 1, 2 },
				MaterialID = 1
			});
		});
		_handles.Add(handle);
		SpawnClutter(maps, shapes);
		onMeshSlicingComplete = (VerticalMeshSlicer.OnMeshSlicingCompletion)Delegate.Remove(onMeshSlicingComplete, new VerticalMeshSlicer.OnMeshSlicingCompletion(RefreshClippingCel));
		onMeshSlicingComplete = (VerticalMeshSlicer.OnMeshSlicingCompletion)Delegate.Combine(onMeshSlicingComplete, new VerticalMeshSlicer.OnMeshSlicingCompletion(RefreshClippingCel));
	}

	private void RefreshClippingCel()
	{
		Debug.Log("Clipping cel late refresh.");
		if (clippingCel != null)
		{
			clippingCel.RequestRefresh();
		}
	}

	private void SpawnClutter(IEnumerable<ProceduralMapTile> maps, List<Vector2>[] shapes, bool preview = false)
	{
		if ((_backgroundSpawnedAlready && !preview) || _backgroundClutter.Length == 0)
		{
			return;
		}
		float num = (preview ? 1f : 70f);
		ProceduralStyle parentStyle = maps.First().GetParentStyle();
		BackgroundMistClutter backgroundMistClutter = _backgroundClutter.FirstOrDefault((BackgroundMistClutter a) => !string.IsNullOrEmpty(a.MaptileBinding) && maps.Any((ProceduralMapTile b) => b.name.Contains(a.MaptileBinding)));
		if (backgroundMistClutter != null)
		{
			GameObject gameObject = SceneUtil.Instantiate(backgroundMistClutter.Prefabs[0], Vector3.zero, Quaternion.identity, transform);
			dynamicResources.Add(gameObject);
			_backgroundSpawnedAlready = true;
			if (!Application.isPlaying)
			{
				gameObject.hideFlags = HideFlags.DontSave;
			}
			if ((bool)_overrideReflectionProbe)
			{
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer obj in componentsInChildren)
				{
					obj.reflectionProbeUsage = ReflectionProbeUsage.Simple;
					obj.probeAnchor = _overrideReflectionProbe;
					obj.shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			return;
		}
		backgroundMistClutter = _backgroundClutter.FirstOrDefault((BackgroundMistClutter a) => a.ETone == parentStyle.Tone);
		if (backgroundMistClutter == null)
		{
			backgroundMistClutter = _backgroundClutter.FirstOrDefault((BackgroundMistClutter a) => a.ETone == ScenarioPossibleRoom.ETone.Inherit);
		}
		if (backgroundMistClutter != null)
		{
			System.Random random = new System.Random(backgroundMistClutter.Seed);
			System.Random rand = new System.Random(backgroundMistClutter.Seed);
			List<(Vector3, Vector3, int)> list = new List<(Vector3, Vector3, int)>();
			foreach (List<Vector2> list2 in shapes)
			{
				for (int num3 = 0; num3 < list2.Count; num3++)
				{
					Vector2 vector = list2[num3];
					Vector2 vector2 = list2[(num3 + 1) % list2.Count];
					Vector2 vector3 = vector2 - vector;
					float magnitude = vector3.magnitude;
					if (magnitude < backgroundMistClutter.MinWallWidth)
					{
						continue;
					}
					Vector2 vector4 = vector3 / magnitude;
					Vector3 vector5 = Vector3.Cross(new Vector3(vector4.x, 0f, vector4.y), Vector3.up);
					Vector2 vector6 = new Vector2(vector5.x, vector5.z);
					Vector2 wCenter2f = (vector + vector2) / 2f;
					Vector2 vector7 = vector6 * (backgroundMistClutter.RandomizeDepthOffset ? MathUtil.SeededRandomRange(rand, backgroundMistClutter.DepthOffset, backgroundMistClutter.DepthOffsetMax) : backgroundMistClutter.DepthOffset);
					Vector2[] array = new Vector2[2]
					{
						wCenter2f + vector7,
						wCenter2f - vector7
					};
					for (int num4 = 0; num4 < array.Length; num4++)
					{
						Vector2 slot2f = array[num4];
						Vector2 errorDelta2f = (slot2f - wCenter2f) * 0.02f;
						bool flag = shapes.Any((List<Vector2> polygon) =>
						{
							Vector2 start = slot2f;
							return Utl.IsIntersectingShape(wCenter2f + errorDelta2f, start, polygon);
						});
						Vector2[] hexagon = null;
						if (!flag && backgroundMistClutter.AutoFitRadius > 0f)
						{
							hexagon = Hexagon(backgroundMistClutter.AutoFitRadius, slot2f);
							int h;
							for (h = 0; h < 6; h++)
							{
								if (shapes.Any((List<Vector2> polygon) =>
								{
									Vector2 start = hexagon[h];
									return Utl.IsIntersectingShape(hexagon[(h + 1) % 6], start, polygon);
								}))
								{
									flag = true;
									break;
								}
							}
						}
						Vector3 vector8 = new Vector3(slot2f.x, 0f, slot2f.y);
						Vector3 vector9 = new Vector3(wCenter2f.x, 0f, wCenter2f.y);
						if (flag)
						{
							_ = vector4 * MathUtil.SeededRandomRange(rand, 0.1f, 0.3f);
							IKPEditorUtils.DrawDebugCube(vector8, Color.red, num);
							Debug.DrawLine(vector8, vector9, Color.red, num);
							continue;
						}
						list.Add((vector8, vector9, random.Next()));
						IKPEditorUtils.DrawDebugCube(vector8, Color.green, num);
						Debug.DrawLine(vector8, vector9, Color.green, num);
						if (hexagon != null)
						{
							for (int num5 = 0; num5 < 6; num5++)
							{
								Vector2 vector10 = hexagon[num5];
								Vector2 vector11 = hexagon[(num5 + 1) % 6];
								Debug.DrawLine(new Vector3(vector10.x, 0f, vector10.y), new Vector3(vector11.x, 0f, vector11.y), Color.green, num);
							}
						}
					}
				}
			}
			list.Sort(((Vector3 point, Vector3 wallCenter, int rand) a, (Vector3 point, Vector3 wallCenter, int rand) b) => a.rand.CompareTo(b.rand));
			for (int num6 = 0; num6 < backgroundMistClutter.Quota && num6 < list.Count; num6++)
			{
				GameObject gameObject2 = SceneUtil.Instantiate(backgroundMistClutter.Prefabs[random.Next() % backgroundMistClutter.Prefabs.Count], list[num6].Item1, Quaternion.identity, transform);
				Renderer[] componentsInChildren = gameObject2.GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer renderer in componentsInChildren)
				{
					if ((bool)_overrideReflectionProbe)
					{
						renderer.reflectionProbeUsage = ReflectionProbeUsage.Simple;
						renderer.probeAnchor = _overrideReflectionProbe;
					}
					renderer.shadowCastingMode = ShadowCastingMode.Off;
				}
				if (!Application.isPlaying)
				{
					gameObject2.hideFlags = HideFlags.DontSave;
				}
				gameObject2.transform.LookAt(list[num6].Item2);
				RandomisePrefab componentInChildren = gameObject2.GetComponentInChildren<RandomisePrefab>();
				if (componentInChildren != null)
				{
					componentInChildren.Seed = random.Next();
				}
				if (Application.isPlaying)
				{
					MonoBehaviour[] componentsInChildren2 = gameObject2.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
					foreach (MonoBehaviour monoBehaviour in componentsInChildren2)
					{
						if (!(monoBehaviour is RandomisePrefab) && !(monoBehaviour is BiomeStateBasedToggle))
						{
							SceneUtil.SmartDestroy(monoBehaviour);
						}
					}
				}
				dynamicResources.Add(gameObject2);
			}
		}
		_backgroundSpawnedAlready = true;
	}

	public static Vector2[] Hexagon(float rad, Vector2 slot2f)
	{
		return new Vector2[6]
		{
			slot2f + new Vector2(rad, 0f),
			slot2f + new Vector2(rad * 0.5f, (0f - rad) * 0.71f),
			slot2f + new Vector2((0f - rad) * 0.5f, (0f - rad) * 0.71f),
			slot2f + new Vector2(0f - rad, 0f),
			slot2f + new Vector2((0f - rad) * 0.5f, rad * 0.71f),
			slot2f + new Vector2(rad * 0.5f, rad * 0.71f)
		};
	}

	private static void BlurVertex(Vector3[] vertices, int size, float heightMin, float heightMax, int x, int y, float decay)
	{
		int num = x + size * y;
		if (vertices[num].y < heightMax)
		{
			int num2 = size * y;
			int num3 = size * (y - 1);
			int num4 = size * (y + 1);
			int[] array = new int[8]
			{
				x - 1 + num3,
				x + num3,
				x + 1 + num3,
				x - 1 + num2,
				x + 1 + num2,
				x - 1 + num4,
				x + num4,
				x + 1 + num4
			};
			float a = heightMin;
			int[] array2 = array;
			foreach (int num5 in array2)
			{
				a = Mathf.Max(a, vertices[num5].y);
			}
			vertices[num] = new Vector3(vertices[num].x, Mathf.Lerp(a, heightMin, decay), vertices[num].z);
		}
	}

	public void Update()
	{
		Handle handle = null;
		while ((handle = _handles.FirstOrDefault((Handle a) => a?.Complete ?? false)) != null)
		{
			ShowMeshes(handle.Meshes);
			_handles.Remove(handle);
		}
		_handles.RemoveAll((Handle a) => a == null);
		FlushDisposeQueue();
	}

	private void OnValidate()
	{
		if (!Application.isPlaying && transform.parent != null && transform.gameObject.GetComponentInParent<ProceduralStyle>() != null && (bool)EnvironmentLoader.Instance)
		{
			Debug.Log("Regenerating Clutter Visuals");
			Clear((UnityEngine.Object a) => !a.name.Contains("_ProceduralMesh") && !string.IsNullOrEmpty(a.name) && !(a is Mesh));
			ProceduralMapTile[] array = (from a in EnvironmentLoader.Instance.EnvironmentRoot.GetComponentsInChildren<ProceduralMapTile>()
				where a.gameObject.activeSelf
				select a).ToArray();
			List<Vector2>[] array2 = array.Select((ProceduralMapTile a) => a.GetShape()).ToArray();
			if (array2.Length != 0)
			{
				SpawnClutter(array, array2, preview: true);
			}
		}
	}

	private void ShowMeshes(IList<CMeshData> meshes)
	{
		List<(MeshRenderer, Mesh)> list = (from a in base.gameObject.GetComponentsInChildren<MeshRenderer>()
			where a.name == "_ProceduralMesh"
			select (renderer: a, mesh: a.GetComponent<MeshFilter>().sharedMesh)).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			if (list[num].Item2 == null)
			{
				SceneUtil.SmartDestroy(list[num].Item1);
				list.RemoveAt(num);
				num--;
			}
		}
		int count = meshes.Count;
		while (count < list.Count)
		{
			(MeshRenderer, Mesh) item = list.Last();
			list.Remove(item);
			SceneUtil.SmartDestroy(item.Item2);
			SceneUtil.SmartDestroy(item.Item1.gameObject);
		}
		int count2 = list.Count;
		for (int num2 = 0; num2 < count - count2; num2++)
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			dynamicResources.Add(mesh);
			GameObject gameObject = new GameObject("_ProceduralMesh");
			gameObject.layer = base.gameObject.layer;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.lightProbeUsage = (_overrideReflectionProbe ? LightProbeUsage.CustomProvided : LightProbeUsage.Off);
			meshRenderer.probeAnchor = _overrideReflectionProbe;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.SetLayer(RenderingLayer.EVERYTHING);
			gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
			gameObject.hideFlags = HideFlags.DontSave;
			dynamicResources.Add(gameObject);
			gameObject.transform.SetParent(transform, worldPositionStays: true);
			list.Add((meshRenderer, mesh));
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			(MeshRenderer, Mesh) tuple = list[num3];
			CMeshData data = meshes[num3];
			_override?.ModifyMeshData(ref data);
			tuple.Item2.Clear();
			tuple.Item2.vertices = data.Vertices;
			tuple.Item2.colors = data.Colors;
			tuple.Item2.triangles = data.Triangles.ToArray();
			tuple.Item2.RecalculateNormals();
			tuple.Item2.RecalculateBounds();
			tuple.Item1.sharedMaterial = ResolveMaterialID(data.MaterialID);
		}
		if (clippingCel != null)
		{
			clippingCel.RequestRefresh();
		}
	}

	private void Clear(Func<UnityEngine.Object, bool> predicate = null)
	{
		if (dynamicResources == null)
		{
			return;
		}
		Action<UnityEngine.Object> action = (UnityEngine.Object a) =>
		{
			if (predicate(a))
			{
				SmartDispose(a);
			}
		};
		dynamicResources.ForEach((predicate != null) ? action : new Action<UnityEngine.Object>(SmartDispose));
		dynamicResources.Clear();
	}

	private Material ResolveMaterialID(int id)
	{
		switch (id)
		{
		case 1:
			if (_override == null)
			{
				return SolidMaterial;
			}
			return _override.GetSolidMaterial(SolidMaterial);
		case 0:
			if (_override == null)
			{
				return TransparentMaterial;
			}
			return _override.GetTransparentMaterial(TransparentMaterial);
		default:
			throw new ArgumentOutOfRangeException($"Undefined material binding for id {id}");
		}
	}

	[ContextMenu("Regenerate")]
	private void DebugRegenerate()
	{
		Clear();
		ProceduralMapTile[] maps = UnityEngine.Object.FindObjectsOfType<ProceduralMapTile>(includeInactive: true);
		VerticalMeshSlicer.OnMeshSlicingCompletion onMeshSlicingComplete = () =>
		{
		};
		Generate(MultiplaneSystem.GetInstance(), maps, ProceduralScenario.GetVisualsBounds(maps), ref onMeshSlicingComplete);
	}

	[ContextMenu("Dbg/Marching Segment Draw")]
	private void DebugMarchingSegmentDraw()
	{
		GameObject gameObject = GameObject.Find("_MarchingSegment_DEBUG");
		if (!gameObject)
		{
			gameObject = new GameObject("_MarchingSegment_DEBUG");
			gameObject.AddComponent<MeshFilter>();
			gameObject.AddComponent<MeshRenderer>();
			gameObject.hideFlags = HideFlags.DontSave;
		}
		List<Vector3> list = new List<Vector3>
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0.5f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 0.5f),
			new Vector3(1f, 0f, 1f),
			new Vector3(0.5f, 0f, 1f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 0f, 0.5f)
		};
		List<int> list2 = new List<int>();
		int dbgMarchingSegmentDraw = _dbgMarchingSegmentDraw;
		for (int i = 0; i < 4; i++)
		{
			int num = i * 3;
			if (_marchingSquaresLookup[dbgMarchingSegmentDraw, num] == -1)
			{
				break;
			}
			for (int num2 = 2; num2 >= 0; num2--)
			{
				int item = _marchingSquaresLookup[dbgMarchingSegmentDraw, num + num2];
				list2.Add(item);
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.hideFlags = HideFlags.DontSave;
		if ((bool)gameObject.GetComponent<MeshFilter>().sharedMesh)
		{
			SceneUtil.SmartDestroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
		}
		gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
		Debug.DrawLine(list[0], list[1], Color.blue, 70f);
		Debug.DrawLine(list[1], list[2], Color.blue, 70f);
		Debug.DrawLine(list[2], list[3], Color.blue, 70f);
		Debug.DrawLine(list[3], list[4], Color.red, 70f);
		Debug.DrawLine(list[4], list[5], Color.red, 70f);
		Debug.DrawLine(list[5], list[6], Color.red, 70f);
		Debug.DrawLine(list[6], list[7], Color.red, 70f);
		Debug.DrawLine(list[7], list[0], Color.red, 70f);
	}
}

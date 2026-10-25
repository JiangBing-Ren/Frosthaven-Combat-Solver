using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FH.LevelComponents.ProcGen;
using Libs.Build;
using Libs.GameConsole;
using SETUtil;
using SETUtil.Common.Extend;
using SETUtil.MeshExporter;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace FH.Tools;

[Preserve]
public static class GeneralHelpersRuntime
{
	[Preserve]
	public static StringBuilder LogSceneGraphicsInfo(bool extended = false)
	{
		Dictionary<Type, int> rendererTypes = new Dictionary<Type, int>
		{
			{
				typeof(MeshRenderer),
				0
			},
			{
				typeof(SkinnedMeshRenderer),
				1
			},
			{
				typeof(LineRenderer),
				2
			},
			{
				typeof(ParticleSystemRenderer),
				3
			}
		};
		StringBuilder stringBuilder = new StringBuilder("[Scene Graphics Info]\n");
		if (extended)
		{
			stringBuilder.AppendLine($"Screen: {Screen.width}x{Screen.height}");
		}
		SkinnedMeshRenderer[] array = new SkinnedMeshRenderer[0];
		Light[] array2 = new Light[0];
		HashSet<Material> hashSet = new HashSet<Material>();
		MeshFilter[] array3 = new MeshFilter[0];
		uint num = 0u;
		GameObject gameObject = FindCharactersBoard();
		string text = "None";
		uint num2 = 0u;
		SkinnedMeshRenderer[] array4 = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>(includeInactive: false);
		SkinnedMeshRenderer[] array5 = array4;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array5)
		{
			int num3 = skinnedMeshRenderer.bones.Length;
			num += (uint)num3;
			if (num2 < num3)
			{
				num2 = (uint)num3;
				text = skinnedMeshRenderer.name;
			}
		}
		if ((bool)gameObject)
		{
			array = gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			array3 = gameObject.transform.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			array2 = gameObject.transform.GetComponentsInChildren<Light>(includeInactive: false);
			uint num4 = 0u;
			ulong num5 = 0uL;
			uint num6 = 0u;
			uint childCount = (uint)gameObject.transform.childCount;
			array5 = array;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in array5)
			{
				num5 += (ulong)skinnedMeshRenderer2.sharedMesh.vertexCount;
				if (num4 < skinnedMeshRenderer2.sharedMesh.vertexCount)
				{
					num4 = (uint)skinnedMeshRenderer2.sharedMesh.vertexCount;
				}
				if (num6 < skinnedMeshRenderer2.sharedMaterials.Length)
				{
					num6 = (uint)skinnedMeshRenderer2.sharedMaterials.Length;
				}
				hashSet.AddRange(skinnedMeshRenderer2.sharedMaterials);
			}
			uint num7 = 0u;
			uint num8 = 0u;
			foreach (Transform item in gameObject.transform)
			{
				if (item.gameObject.activeSelf)
				{
					num8++;
				}
				uint num9 = 0u;
				array5 = item.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
				foreach (SkinnedMeshRenderer skinnedMeshRenderer3 in array5)
				{
					num9 += (uint)skinnedMeshRenderer3.bones.Length;
				}
				num7 = Math.Max(num7, num9);
			}
			stringBuilder.AppendLine($"Characters (+disabled): {childCount}");
			stringBuilder.AppendLine($"Characters Enabled: {num8}");
			stringBuilder.AppendLine($"Character MeshFilters: {array3.Length}");
			stringBuilder.AppendLine($"Character Skin Meshes: {array.Length}");
			stringBuilder.AppendLine($"Characters Total Vert: {num5}");
			stringBuilder.AppendLine($"Character Max Vert: {num4}");
			stringBuilder.AppendLine($"Character All Material: {hashSet.Count}");
			stringBuilder.AppendLine($"Character Max Material: {num6}");
			stringBuilder.AppendLine($"Character All Lights: {array2.Length}");
			stringBuilder.AppendLine($"Character Avg Lights: {((childCount != 0) ? ((float)array2.Length / (float)childCount) : (-1f))}");
			stringBuilder.AppendLine($"Character Avg Bones: {(ulong)num / (ulong)childCount}");
			stringBuilder.AppendLine($"Character Max Bones: {num7}");
		}
		else if (extended)
		{
			stringBuilder.AppendLine("No characters board");
		}
		stringBuilder.AppendLine($"Avg Bones Per Skin: {((array4.Length != 0) ? (num / array4.Length) : 0)}");
		stringBuilder.AppendLine($"Most Bones Per Skin: {num2}");
		if (extended)
		{
			stringBuilder.AppendLine(" ^ " + text);
		}
		Light[] source;
		int num10 = (source = UnityEngine.Object.FindObjectsOfType<Light>(includeInactive: false).ToArray()).Length;
		stringBuilder.AppendLine($"Lights All: {num10}");
		stringBuilder.AppendLine($"Lights Env: {num10 - array2.Length}");
		int num11 = source.Where((Light a) => a.type == LightType.Point).Count();
		stringBuilder.AppendLine($"Point Lights All: {num11}");
		stringBuilder.AppendLine($"Point Lights Env: {num11 - array2.Where((Light a) => a.type == LightType.Point).Count()}");
		stringBuilder.AppendLine($"Spot Lights All: {source.Where((Light a) => a.type == LightType.Spot).Count()}");
		stringBuilder.AppendLine($"Dir Lights: {source.Where((Light a) => a.type == LightType.Directional).Count()}");
		stringBuilder.AppendLine($"Shadow Casting Lights: {source.Where((Light a) => a.shadows != LightShadows.None).Count()}");
		stringBuilder.AppendLine($"Total GameObjects: {UnityEngine.Object.FindObjectsOfType<GameObject>(includeInactive: true).Length}");
		stringBuilder.AppendLine($"Total Active GameObjects: {UnityEngine.Object.FindObjectsOfType<GameObject>(includeInactive: false).Length}");
		ParticleSystem[] array6 = UnityEngine.Object.FindObjectsOfType<ParticleSystem>(includeInactive: false);
		stringBuilder.AppendLine($"Particles: {array6.Length}");
		if (extended)
		{
			stringBuilder.AppendLine("\n\t" + string.Join("\n\t", array6.Select((ParticleSystem a) => (!a) ? "NULL" : a.name)));
		}
		Renderer[] array7;
		stringBuilder.AppendLine($"Renderers All: {(array7 = (from a in UnityEngine.Object.FindObjectsOfType<Renderer>(includeInactive: false)
			where a.enabled
			select a).ToArray()).Length}");
		MeshRenderer[] array8;
		stringBuilder.AppendLine($"Mesh Renderers All: {(array8 = (from a in UnityEngine.Object.FindObjectsOfType<MeshRenderer>(includeInactive: false)
			where a.enabled
			select a).ToArray()).Length}");
		MeshFilter[] array9;
		stringBuilder.AppendLine($"Mesh Filters All: {(array9 = UnityEngine.Object.FindObjectsOfType<MeshFilter>(includeInactive: false)).Length}");
		stringBuilder.AppendLine($"Skinned Meshes: {array4.Where((SkinnedMeshRenderer a) => a.enabled).ToArray().Length}");
		int num12 = array7.Where((Renderer a) => a.shadowCastingMode != ShadowCastingMode.Off).Count();
		stringBuilder.AppendLine($"Shadow Casters: {num12}");
		Dictionary<Material, int> dictionary = new Dictionary<Material, int>();
		HashSet<Shader> hashSet2 = new HashSet<Shader>();
		uint num13 = 0u;
		Renderer[] array10 = array7;
		foreach (Renderer renderer in array10)
		{
			if (renderer.sharedMaterials == null)
			{
				continue;
			}
			int num14 = renderer.sharedMaterials.Length;
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if ((bool)material)
				{
					if (!dictionary.ContainsKey(material))
					{
						dictionary.Add(material, 0);
					}
					else
					{
						dictionary[material] |= 64;
					}
					Type type = renderer.GetType();
					dictionary[material] |= (rendererTypes.ContainsKey(type) ? rendererTypes[type] : 16);
				}
			}
			if (num13 < num14)
			{
				num13 = (uint)num14;
			}
			hashSet2.AddRange(from a in renderer.sharedMaterials
				where (bool)a && (bool)a.shader
				select a.shader);
		}
		stringBuilder.AppendLine($"Materials ENV + PART: {dictionary.Count - hashSet.Count}");
		stringBuilder.AppendLine($"Total Materials All: {dictionary.Count}");
		stringBuilder.AppendLine($"Total Shaders All: {hashSet2.Count}");
		if (extended)
		{
			stringBuilder.AppendLine("Shaders: \n\t" + string.Join("\n\t", hashSet2.Select((Shader a) => (!a) ? "NULL" : a.name)));
		}
		IEnumerable<KeyValuePair<Material, int>> enumerable = dictionary.Where((KeyValuePair<Material, int> a) => a.Value >= 64 && !a.Value.ContainsFlag(1 << rendererTypes[typeof(SkinnedMeshRenderer)]));
		stringBuilder.AppendLine($"Unique ENV materials: {enumerable.Count()}");
		if (extended)
		{
			foreach (KeyValuePair<Material, int> item2 in enumerable)
			{
				if (!(item2.Key == null))
				{
					stringBuilder.AppendLine(" ^ " + item2.Key.name);
				}
			}
		}
		ulong num16 = 0uL;
		uint num17 = 0u;
		MeshFilter[] array11 = array9;
		foreach (MeshFilter meshFilter in array11)
		{
			if (!array3.Contains(meshFilter))
			{
				num17++;
				num16 += (ulong)(meshFilter.sharedMesh?.vertexCount ?? 0);
			}
		}
		stringBuilder.AppendLine($"Total Verts ENV: {num16}");
		stringBuilder.AppendLine($"Avg Verts ENV: {((num17 != 0) ? (num16 / num17) : 0)}");
		ulong num18 = 0uL;
		array11 = array9;
		foreach (MeshFilter meshFilter2 in array11)
		{
			num18 += (ulong)((meshFilter2.sharedMesh?.triangles.Length ?? 0) / 3);
		}
		stringBuilder.AppendLine($"Total Triangles ENV (/3): {num18}");
		return stringBuilder;
	}

	[Preserve]
	[ConsoleCommand(Command = "gc", Description = "Invoke Garbage Collect")]
	public static void ForceGC(IConsole console = null)
	{
		Debug.Log("Forcing CG");
		GC.Collect();
		GC.WaitForPendingFinalizers();
		Resources.UnloadUnusedAssets();
	}

	public static void Dirtify(UnityEngine.Object o)
	{
		_ = Application.isPlaying;
	}

	[Preserve]
	public static void DebugPrintToDedicatedUIText(string message)
	{
		GameObject gameObject = GameObject.Find("DEDICATED_DEBUG_SCREEN_TEXT");
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Text>().text = message;
			Dirtify(gameObject);
		}
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugSimplifyAllMeshes", Description = "Trun all meshes into simple boxes")]
	public static void SimplifyAllMeshesInScene(IConsole console = null)
	{
		SimplifyAllMeshesInScene((float n) =>
		{
		});
	}

	[Preserve]
	public static void SimplifyAllMeshesInScene(Action<float> updateProgress)
	{
		Func<Mesh, Mesh> func = (Mesh input) =>
		{
			Vector3 vector = Vector3.one * 0.5f;
			Vector3[] vertices = new Vector3[8]
			{
				new Vector3(0f, 0f, 0f) - vector,
				new Vector3(1f, 0f, 0f) - vector,
				new Vector3(1f, 1f, 0f) - vector,
				new Vector3(0f, 1f, 0f) - vector,
				new Vector3(0f, 1f, 1f) - vector,
				new Vector3(1f, 1f, 1f) - vector,
				new Vector3(1f, 0f, 1f) - vector,
				new Vector3(0f, 0f, 1f) - vector
			};
			int[] triangles = new int[36]
			{
				0, 2, 1, 0, 3, 2, 2, 3, 4, 2,
				4, 5, 1, 2, 5, 1, 5, 6, 0, 7,
				4, 0, 4, 3, 5, 4, 7, 5, 7, 6,
				0, 6, 7, 0, 1, 6
			};
			Mesh mesh = new Mesh();
			mesh.Clear();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.Optimize();
			mesh.RecalculateNormals();
			return mesh;
		};
		HashSet<Mesh> hashSet = new HashSet<Mesh>();
		updateProgress(0f);
		MeshFilter[] array = (from a in UnityEngine.Object.FindObjectsOfType<MeshFilter>()
			where a.sharedMesh != null
			select a).ToArray();
		MeshFilter[] array2 = array;
		foreach (MeshFilter meshFilter in array2)
		{
			hashSet.Add(meshFilter.sharedMesh);
		}
		Dictionary<Mesh, Mesh> dictionary = new Dictionary<Mesh, Mesh>(hashSet.Count);
		int num2 = 0;
		foreach (Mesh item in hashSet)
		{
			updateProgress((float)num2++ / (float)hashSet.Count * 0.5f);
			dictionary.Add(item, func(item));
		}
		num2 = 0;
		array2 = array;
		foreach (MeshFilter meshFilter2 in array2)
		{
			updateProgress(0.5f + (float)num2++ / (float)hashSet.Count * 0.5f);
			meshFilter2.sharedMesh = dictionary[meshFilter2.sharedMesh];
			Dirtify(meshFilter2);
		}
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugMeshCombineAll", Description = "Batch together environment meshes by material")]
	public static void MeshCombineAll(IConsole console = null)
	{
		MeshFilter[] array = UnityEngine.Object.FindObjectsOfType<MeshFilter>(includeInactive: false);
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Dictionary<string, List<Tuple<MeshFilter, List<CombineInstance>, int>>> dictionary = new Dictionary<string, List<Tuple<MeshFilter, List<CombineInstance>, int>>>();
		MeshFilter[] array2 = array;
		foreach (MeshFilter meshFilter in array2)
		{
			Mesh sharedMesh = meshFilter.sharedMesh;
			MeshRenderer component = meshFilter.GetComponent<MeshRenderer>();
			if (!component)
			{
				continue;
			}
			if (!sharedMesh)
			{
				hashSet.Add(component.gameObject);
				continue;
			}
			Material[] sharedMaterials = component.sharedMaterials;
			if (sharedMaterials.Length != 0)
			{
				string key = string.Join("", sharedMaterials.Select((Material a) => (!a) ? "-" : a.name));
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, new List<Tuple<MeshFilter, List<CombineInstance>, int>>());
				}
				if (dictionary[key].Count == 0 || dictionary[key].Last().Item3 + sharedMesh.vertexCount > 65500)
				{
					dictionary[key].Add(new Tuple<MeshFilter, List<CombineInstance>, int>(meshFilter, new List<CombineInstance>(), 0));
				}
				Tuple<MeshFilter, List<CombineInstance>, int> tuple = dictionary[key][dictionary[key].Count - 1];
				tuple.Item2.Add(new CombineInstance
				{
					mesh = sharedMesh,
					transform = meshFilter.transform.localToWorldMatrix
				});
				dictionary[key][dictionary[key].Count - 1] = new Tuple<MeshFilter, List<CombineInstance>, int>(tuple.Item1, tuple.Item2, tuple.Item3 + sharedMesh.vertexCount);
				hashSet.Add(meshFilter.gameObject);
			}
		}
		Func<MeshFilter, MeshFilter> func = (MeshFilter filter) =>
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(filter.gameObject);
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.name = "_CombinedInstance_" + filter.sharedMesh.name;
			return gameObject.GetComponent<MeshFilter>();
		};
		foreach (KeyValuePair<string, List<Tuple<MeshFilter, List<CombineInstance>, int>>> item in dictionary)
		{
			foreach (Tuple<MeshFilter, List<CombineInstance>, int> item2 in item.Value)
			{
				MeshFilter meshFilter2 = func(item2.Item1);
				Mesh mesh = new Mesh();
				mesh.CombineMeshes(item2.Item2.ToArray(), mergeSubMeshes: true, useMatrices: true);
				meshFilter2.sharedMesh = mesh;
				Dirtify(meshFilter2);
			}
		}
		foreach (GameObject item3 in hashSet)
		{
			try
			{
				SceneUtil.SmartDestroy(item3);
			}
			catch
			{
				item3.SetActive(value: false);
				Dirtify(item3);
			}
		}
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugHalveCharactersCount", Description = "Remove half the skin meshes")]
	public static void DebugHalveCharactersCount(IConsole console = null)
	{
		GameObject gameObject = FindCharactersBoard();
		if (!gameObject)
		{
			console?.WriteLine("Could not find characters board");
			Debug.LogError("Could not find characters board");
			return;
		}
		int num = 0;
		foreach (Transform item in gameObject.transform)
		{
			if (item.gameObject.activeSelf)
			{
				item.gameObject.SetActive(num++ % 2 == 0);
			}
		}
		DumpSceneGraphicsInfoSummary(console);
	}

	public static GameObject FindCharactersBoard()
	{
		GameObject gameObject = null;
		ClientScenarioManager clientScenarioManager = UnityEngine.Object.FindObjectOfType<ClientScenarioManager>();
		if ((bool)clientScenarioManager)
		{
			gameObject = clientScenarioManager.m_Board;
		}
		if (!gameObject)
		{
			gameObject = GameObject.Find("Board");
		}
		if (!gameObject)
		{
			gameObject = GameObject.Find("BAKED_CHARACTERS");
		}
		return gameObject;
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugHalveMeshes", Description = "Remove half the solid meshes (MeshRenderers)")]
	public static void DebugHalveMeshes(IConsole console = null)
	{
		uint num = 0u;
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.enabled)
			{
				meshRenderer.enabled = num++ % 2 == 0;
				Dirtify(meshRenderer);
			}
		}
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugHalveLights", Description = "Remove half the lights")]
	public static void DebugHalveLights(IConsole console = null)
	{
		uint num = 0u;
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>(includeInactive: true);
		foreach (Light light in array)
		{
			if (light.enabled && light.type != LightType.Directional)
			{
				light.enabled = num++ % 2 == 0;
				Dirtify(light);
			}
		}
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "SensibleOptimize", Description = "Batch together meshes, disable shadow casters, merge lights")]
	public static void SensibleOptimize(IConsole console = null)
	{
		MeshCombineAll(console);
		DisableEnvironmentShadowcast(console);
		RemoveCloseTogetherLights(console);
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugRemoveCloseTogetherLights", Description = "Detect lights that are too close and deletes one")]
	public static void RemoveCloseTogetherLights(IConsole console)
	{
		int num = 0;
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>(includeInactive: true);
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			if (!light.enabled || light.type != LightType.Point)
			{
				continue;
			}
			Light[] array3 = array;
			foreach (Light light2 in array3)
			{
				if (light2.enabled && !(light == light2) && light2.type == LightType.Point && Vector3.SqrMagnitude(light.transform.position - light2.transform.position) < 1f && light.color == light2.color && !light.GetComponent<Animator>())
				{
					light.gameObject.SetActive(value: false);
					Dirtify(light);
					light2.transform.position = (light.transform.position + light2.transform.position) / 2f;
					Dirtify(light2.transform);
					num++;
				}
			}
		}
		Debug.Log($"Optimized {num} lights from a total of {array.Length} present.");
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugDisableEnvironmentShadowcast", Description = "")]
	public static void DisableEnvironmentShadowcast(IConsole console)
	{
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer obj in array)
		{
			obj.shadowCastingMode = ShadowCastingMode.Off;
			Dirtify(obj);
		}
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugTurnOffGlobalVolume", Description = "Turn off post processing volume named BAKED_GlobalVolume")]
	public static void DebugTurnOffGlobalVolume(IConsole console)
	{
		Volume[] array = UnityEngine.Object.FindObjectsOfType<Volume>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		DumpSceneGraphicsInfoSummary(console);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugClearVertexPaint", Description = "")]
	public static void DebugClearVertexPaint(IConsole console)
	{
		Scene[] allScenes = SceneManager.GetAllScenes();
		foreach (Scene scene in allScenes)
		{
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				MeshRenderer[] componentsInChildren = rootGameObjects[j].GetComponentsInChildren<MeshRenderer>(includeInactive: true);
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					componentsInChildren[k].additionalVertexStreams = null;
				}
			}
		}
	}

	[Preserve]
	[ConsoleCommand(Command = "DumpSceneGraphicsInfoSummary", Description = "Prints out information about the scene to the console")]
	public static void DumpSceneGraphicsInfoSummary(IConsole console)
	{
		string text = LogSceneGraphicsInfo().ToString();
		console?.WriteLine(text);
		Debug.Log(text);
		DebugPrintToDedicatedUIText(text);
	}

	[Preserve]
	[ConsoleCommand(Command = "DumpSceneGraphicsInfoExtended", Description = "Prints out information about the scene to the console")]
	public static void DumpSceneGraphicsInfoExtended(IConsole console)
	{
		string text = LogSceneGraphicsInfo(extended: true).ToString();
		console?.WriteLine(text);
		Debug.Log(text);
		DebugPrintToDedicatedUIText(text);
	}

	[Preserve]
	[ConsoleCommand(Command = "LoadSceneAt", Description = "Load scene at given index")]
	public static void LoadSceneAt(IConsole console, int index)
	{
		SceneManager.LoadScene(index);
	}

	[Preserve]
	[ConsoleCommand(Command = "DebugRemoveDisabledObjects", Description = "Delete objects that are disabled")]
	public static void DebugRemoveDisabledObjects(IConsole console = null)
	{
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!monoBehaviour.enabled)
			{
				SceneUtil.SmartDestroy(monoBehaviour);
			}
		}
		GameObject[] array2 = UnityEngine.Object.FindObjectsOfType<GameObject>(includeInactive: true);
		foreach (GameObject gameObject in array2)
		{
			if (!gameObject.activeInHierarchy)
			{
				SceneUtil.SmartDestroy(gameObject);
			}
		}
	}

	[Preserve]
	[ConsoleCommand(Command = "ForceQuit", Description = "Calls Application.Quit()")]
	public static void ForceQuit(IConsole console)
	{
		Application.Quit();
	}

	[Preserve]
	[ConsoleCommand(Command = "MonitorMemory", Description = "Monitor memory for x seconds")]
	public static void MonitorMemory(IConsole console, int forSeconds)
	{
		GameObject gameObject = new GameObject("_Mem_Alloc_Debug_Host");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		gameObject.AddComponent<MemAllocMonitorCrtHost>().Run(forSeconds, (string s) =>
		{
			Debug.Log(s);
			console.WriteLine(s);
		});
	}

	public static void Replace(GameObject gameObject, GameObject prefab)
	{
		GameObject gameObject2 = SceneUtil.Instantiate(prefab, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent);
		gameObject2.transform.localScale = gameObject.transform.localScale;
		gameObject2.SetActive(gameObject.activeSelf);
		try
		{
			SceneUtil.SmartDestroy(gameObject);
		}
		catch (Exception arg)
		{
			Debug.LogError($"could not delete {gameObject}. Reason: {arg}");
			gameObject.SetActive(value: false);
			gameObject.name = "UNABLE_TO_DELETE";
		}
	}

	public static void GetProceduralStylesInParentsRecursive(Transform currentNode, ProceduralStyle lastProceduralStyle, List<ProceduralStyle> outResult)
	{
		ProceduralStyle componentInParent = currentNode.GetComponentInParent<ProceduralStyle>(includeInactive: true);
		if ((bool)componentInParent && componentInParent != lastProceduralStyle)
		{
			outResult.Add(componentInParent);
			Transform transform = componentInParent.transform;
			if ((bool)transform.parent)
			{
				GetProceduralStylesInParentsRecursive(transform.parent, componentInParent, outResult);
			}
		}
	}

	[ConsoleCommand(Command = "Revision")]
	public static void Revision(IConsole console)
	{
		console.WriteLine($"Revision: {RuntimeBuildInfo.BuildRevisionNumber}");
		console.WriteLine("Build type: RELEASE");
	}

	[ConsoleCommand(Command = "ExportSceneAsObj", Description = "Exports the scene as an OBJ files in the specified dir")]
	public static void ExportSceneAsObj(IConsole console, string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		if (!directoryInfo.Exists)
		{
			console.WriteLine("Directory " + path + " does not exist. Creating.");
			directoryInfo.Create();
		}
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		int num = 0;
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if (meshRenderer.GetComponent<MeshFilter>().sharedMesh == null)
			{
				console.WriteLine("Skipping " + meshRenderer.name + " as it has no mesh");
			}
			else
			{
				OBJExporter.ExportObject(Path.Combine(path, $"{num}_{meshRenderer.name}.obj"), meshRenderer.gameObject);
			}
		}
		console.WriteLine($"Exported {num} objects to {path}");
	}
}

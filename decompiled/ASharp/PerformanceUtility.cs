using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

public static class PerformanceUtility
{
	public static bool CanApplyChaos = true;

	public static void TurnOffPointLightShadows()
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
		foreach (Light light in array)
		{
			if (light.type == LightType.Point)
			{
				light.shadows = LightShadows.None;
				Debug.Log("Turned off light:" + light.name);
			}
		}
	}

	public static void Combine()
	{
		GameObject gameObject = UnityEngine.Object.FindObjectsOfType<GameObject>().FirstOrDefault((GameObject go) => go.name == "Maps");
		if ((bool)gameObject)
		{
			StaticBatchingUtility.Combine(gameObject);
			Debug.Log("Combined " + gameObject.name);
		}
	}

	public static void ListMeshWithSubMesh()
	{
		InternalListMeshWithSubMesh(null);
	}

	public static void ListMeshWithSubMeshAndHide()
	{
		InternalListMeshWithSubMesh((MeshRenderer m) =>
		{
			m.gameObject.SetActive(value: false);
		});
	}

	public static void InternalListMeshWithSubMesh(Action<MeshRenderer> callback)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>();
		Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		Regex regex = new Regex("(.*?)\\s\\(\\d\\)");
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if ((bool)meshRenderer && meshRenderer.enabled && meshRenderer.sharedMaterials.Length > 1)
			{
				Transform transform = meshRenderer.transform;
				while (transform.name.StartsWith("LOD"))
				{
					transform = transform.parent;
				}
				string name = transform.name;
				Match match = regex.Match(name);
				string text = (match.Success ? match.Groups[1].Value : meshRenderer.gameObject.name);
				dictionary2[text] = meshRenderer.gameObject;
				dictionary[text] = (dictionary3.ContainsKey(text) ? (dictionary[text] + " \n\n Obj:" + text + " Mesh:" + meshRenderer.name + " has subMesh") : ("Obj:" + text + " Mesh:" + meshRenderer.name + " has subMesh"));
				dictionary3[text] = ((!dictionary3.ContainsKey(text)) ? 1 : (dictionary3[text] + 1));
				callback?.Invoke(meshRenderer);
			}
		}
		List<KeyValuePair<string, int>> list = dictionary3.ToList();
		list.Sort((KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2) => pair2.Value.CompareTo(pair1.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			Debug.Log($"Count:{item.Value} key:{item.Key} value:{dictionary[item.Key]}", dictionary2[item.Key]);
		}
	}

	public static void ListAllNotInstancedMeshes()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>();
		Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		Regex regex = new Regex("(.*?)\\s\\(\\d\\)");
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if (!meshRenderer || !meshRenderer.enabled)
			{
				continue;
			}
			Material[] sharedMaterials = meshRenderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (!material)
				{
					Debug.LogWarning("Strange - Obj:" + meshRenderer.gameObject.name + " Mesh:" + meshRenderer.name + " with null material!", meshRenderer.gameObject);
				}
				else if ((bool)material && !material.enableInstancing)
				{
					Transform transform = meshRenderer.transform;
					while (transform.name.StartsWith("LOD"))
					{
						transform = transform.parent;
					}
					string name = transform.name;
					Match match = regex.Match(name);
					string text = (match.Success ? match.Groups[1].Value : meshRenderer.gameObject.name);
					dictionary2[text] = meshRenderer.gameObject;
					dictionary[text] = (dictionary3.ContainsKey(text) ? (dictionary[text] + " \n\n Obj:" + text + " Mesh:" + meshRenderer.name + " Material:" + material.name + " NOT GPU Instanced") : ("Obj:" + text + " Mesh:" + meshRenderer.name + " Material:" + material.name + " NOT GPU Instanced"));
					dictionary3[text] = ((!dictionary3.ContainsKey(text)) ? 1 : (dictionary3[text] + 1));
				}
			}
		}
		List<KeyValuePair<string, int>> list = dictionary3.ToList();
		list.Sort((KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2) => pair2.Value.CompareTo(pair1.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			Debug.Log($"Count:{item.Value} key:{item.Key} value:{dictionary[item.Key]}", dictionary2[item.Key]);
		}
	}

	public static void ListAllShadowCasters()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>();
		Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		Regex regex = new Regex("(.*?)\\s\\(\\d\\)");
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if ((bool)meshRenderer && meshRenderer.enabled && meshRenderer.shadowCastingMode != ShadowCastingMode.Off)
			{
				Transform transform = meshRenderer.transform;
				while (transform.name.StartsWith("LOD"))
				{
					transform = transform.parent;
				}
				string name = transform.name;
				Match match = regex.Match(name);
				string text = (match.Success ? match.Groups[1].Value : meshRenderer.gameObject.name);
				dictionary2[text] = meshRenderer.gameObject;
				dictionary[text] = (dictionary3.ContainsKey(text) ? (dictionary[text] + " \n\n Obj:" + text + " Mesh:" + meshRenderer.name + " Casting Shadow") : ("Obj:" + text + " Mesh:" + meshRenderer.name + " Casting Shadow"));
				dictionary3[text] = ((!dictionary3.ContainsKey(text)) ? 1 : (dictionary3[text] + 1));
			}
		}
		List<KeyValuePair<string, int>> list = dictionary3.ToList();
		list.Sort((KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2) => pair2.Value.CompareTo(pair1.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			Debug.Log($"Count:{item.Value} key:{item.Key} value:{dictionary[item.Key]}", dictionary2[item.Key]);
		}
	}

	public static void SetShadowsOff()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>();
		Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		Regex regex = new Regex("(.*?)\\s\\(\\d\\)");
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if ((bool)meshRenderer && meshRenderer.enabled && meshRenderer.shadowCastingMode != ShadowCastingMode.Off)
			{
				Match match = regex.Match(meshRenderer.gameObject.name);
				string text = (match.Success ? match.Groups[1].Value : meshRenderer.gameObject.name);
				dictionary2[text] = meshRenderer.gameObject;
				dictionary[text] = (dictionary3.ContainsKey(text) ? (dictionary[text] + " \n\n Obj:" + text + " Mesh:" + meshRenderer.name + " Casting Shadow") : ("Obj:" + text + " Mesh:" + meshRenderer.name + " Casting Shadow"));
				dictionary3[text] = ((!dictionary3.ContainsKey(text)) ? 1 : (dictionary3[text] + 1));
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
		List<KeyValuePair<string, int>> list = dictionary3.ToList();
		list.Sort((KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2) => pair2.Value.CompareTo(pair1.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			Debug.Log($"Count:{item.Value} key:{item.Key} value:{dictionary[item.Key]}", dictionary2[item.Key]);
		}
	}
}

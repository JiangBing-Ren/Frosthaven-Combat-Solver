using System.Collections.Generic;
using System.Linq;
using SETUtil.Common.Extend;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.ProcBackgroundsPrototype;

public class ProceduralBackground : MonoBehaviour, IBackgroundPostProcessor
{
	public const float FEATURE_RADIUS = 100f;

	public const float ISLAND_RADIUS = 20f;

	public ProcBackgroundResourcesDef procBackgroundResources;

	public void Process(Scene scene, GameBackgrounds.WorldContext context)
	{
		HashSet<UnityGameEditorObject> hashSet = new HashSet<UnityGameEditorObject>();
		foreach (UnityGameEditorObject item2 in from a in context.MapsRoot.GetComponentsInChildren<UnityGameEditorObject>(includeInactive: true)
			where a.m_ObjectType == ScenarioManager.ObjectImportType.Tile
			select a)
		{
			hashSet.Add(item2);
		}
		HashSet<BackgroundMidgroundElement> hashSet2 = new HashSet<BackgroundMidgroundElement>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int num = 0; num < rootGameObjects.Length; num++)
		{
			BackgroundMidgroundElement[] componentsInChildren = rootGameObjects[num].GetComponentsInChildren<BackgroundMidgroundElement>(includeInactive: true);
			foreach (BackgroundMidgroundElement item in componentsInChildren)
			{
				hashSet2.Add(item);
			}
		}
		foreach (BackgroundMidgroundElement element in hashSet2)
		{
			List<GameObject> list = procBackgroundResources.GetResources(element.Type, context.Biome).ToList();
			if (list.Count == 0)
			{
				continue;
			}
			GameObject gameObject = list.SelectRandom();
			if (!gameObject)
			{
				continue;
			}
			bool flag = false;
			switch (element.Type)
			{
			case BackgroundElementType.Feature:
				if (!hashSet.Any((UnityGameEditorObject a) => Vector3.SqrMagnitude(a.transform.position - element.transform.position) < 100f))
				{
					flag = true;
				}
				break;
			case BackgroundElementType.Island:
				if (hashSet.Any((UnityGameEditorObject a) => Vector3.SqrMagnitude(a.transform.position - element.transform.position) < 20f))
				{
					flag = true;
				}
				break;
			case BackgroundElementType.Farground:
				flag = true;
				break;
			}
			if (flag)
			{
				SceneManager.MoveGameObjectToScene(Object.Instantiate(gameObject, element.transform.position, element.transform.rotation), scene);
			}
		}
	}
}

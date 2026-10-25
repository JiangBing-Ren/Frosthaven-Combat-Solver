using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.ProcBackgroundsPrototype;

public sealed class GameBackgrounds : MonoBehaviour
{
	public class WorldContext
	{
		public GameObject MapsRoot { get; private set; }

		public ScenarioPossibleRoom.EBiome Biome { get; private set; }

		private WorldContext()
		{
		}

		public static WorldContext Get(GameObject m_MapSceneRoot)
		{
			return new WorldContext
			{
				MapsRoot = m_MapSceneRoot,
				Biome = m_MapSceneRoot.GetComponent<ProceduralStyle>().Biome
			};
		}
	}

	public const string SCENE_PREFIX = "SBackground_";

	public const string NONE_SELECT = "None";

	private List<SceneInstanceHandle> _sceneInstances = new List<SceneInstanceHandle>();

	public static List<string> AllScenes => GetFromBuildSettings();

	private static List<string> GetFromBuildSettings()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
			if (fileNameWithoutExtension.StartsWith("SBackground_"))
			{
				list.Add(fileNameWithoutExtension);
			}
		}
		return list;
	}

	public static GameBackgrounds FindOrCreateInstanceSlow()
	{
		GameBackgrounds gameBackgrounds = Object.FindObjectsOfType<GameBackgrounds>().FirstOrDefault();
		if (!gameBackgrounds)
		{
			gameBackgrounds = new GameObject().AddComponent<GameBackgrounds>();
		}
		return gameBackgrounds;
	}

	private void BeginLoadInAsync(string sceneName, WorldContext context)
	{
	}

	private void LoadingCompleted(AsyncOperation handle)
	{
		SceneInstanceHandle sceneInstanceHandle = _sceneInstances.Single((SceneInstanceHandle a) => a.handle == handle);
		Scene sceneByName = SceneManager.GetSceneByName(sceneInstanceHandle.sceneName);
		GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
		for (int num = 0; num < rootGameObjects.Length; num++)
		{
			IBackgroundPostProcessor[] componentsInChildren = rootGameObjects[num].GetComponentsInChildren<IBackgroundPostProcessor>();
			for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
			{
				componentsInChildren[num2].Process(sceneByName, sceneInstanceHandle.context);
			}
		}
	}

	public void Select(string current, WorldContext context)
	{
		StartCoroutine(SelectCrt(current, context));
	}

	private IEnumerator SelectCrt(string current, WorldContext context)
	{
		yield return UnloadAllScenesCrt();
		if (current != "None")
		{
			BeginLoadInAsync(current, context);
		}
	}

	public IEnumerator UnloadAllScenesCrt()
	{
		while (_sceneInstances.Any((SceneInstanceHandle a) =>
		{
			AsyncOperation handle = a.handle;
			return handle != null && !handle.isDone;
		}))
		{
			yield return null;
		}
		foreach (SceneInstanceHandle sceneInstance in _sceneInstances)
		{
			if (SceneManager.GetSceneByName(sceneInstance.sceneName).IsValid())
			{
				yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneInstance.sceneName), UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
			}
		}
		_sceneInstances.Clear();
	}
}

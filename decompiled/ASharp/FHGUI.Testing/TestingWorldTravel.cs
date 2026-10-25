using System.Collections;
using System.Collections.Generic;
using FH.YML;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHGUI.Modules;
using FHRL;
using Libs.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FHGUI.Testing;

public class TestingWorldTravel : MonoBehaviour
{
	[Header("Components")]
	public NarrationModalModule Module;

	public NarrativeDataSO DataSO;

	public TMP_Dropdown DropdownScenarios;

	private FHWorldMapScene _mapScene;

	private const float SCENE_LOAD_PERCENT = 0.9f;

	private const string PARTY_VIEW_SCENE_NAME = "FHWorldMap";

	private static readonly Vector3 SPAWN_POSITION = new Vector3(0f, 0f, 0f);

	private const float _transitionSeconds = 2f;

	private void Start()
	{
		StartCoroutine(SetupScene());
	}

	private IEnumerator SetupScene()
	{
		yield return LoadWorldMapSceneCrt(gameObject.scene);
		ShowAll();
	}

	private void ShowAll()
	{
		_mapScene.Show(BuildScenarios());
	}

	private List<ScenarioState> BuildScenarios()
	{
		List<ScenarioState> list = new List<ScenarioState>();
		foreach (KeyValuePair<ScenarioID, ScenarioDef> scenario in YMLDefRepository.Repo.DefRepository.Scenarios)
		{
			ScenarioState item = new ScenarioState(scenario.Value, YMLDefRepository.Repo.DefRepository);
			list.Add(item);
		}
		return list;
	}

	private IEnumerator LoadWorldMapSceneCrt(Scene mainScene)
	{
		AsyncOperation loadOp = SceneManager.LoadSceneAsync("FHWorldMap", LoadSceneMode.Additive);
		loadOp.allowSceneActivation = false;
		while (!loadOp.isDone)
		{
			if (loadOp.progress >= 0.9f)
			{
				loadOp.allowSceneActivation = true;
			}
			yield return null;
		}
		Scene sceneByName = SceneManager.GetSceneByName("FHWorldMap");
		FHWorldMapScene component = sceneByName.GetComponent<FHWorldMapScene>(inChildren: true);
		if (component != null)
		{
			SceneManager.MoveGameObjectToScene(component.Root.gameObject, mainScene);
			component.Root.position = SPAWN_POSITION;
			_mapScene = component;
		}
		else
		{
			Log.Error("Cannot find ISceneRoot root in scene FHWorldMap");
		}
		if (sceneByName.isLoaded)
		{
			AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("FHWorldMap");
			while (!unloadOp.isDone)
			{
				yield return null;
			}
		}
	}
}

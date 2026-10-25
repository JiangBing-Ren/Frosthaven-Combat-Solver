using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FHGUI;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Services;
using FHRL;
using FHRL.Buildings;
using Libs.GameConsole;
using Libs.Utils;
using ScenarioRuleLibrary;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class OutpostSceneController : MonoBehaviour
{
	private enum SceneContext
	{
		None,
		Outpost,
		WorldMap,
		Character,
		Party,
		OutpostCamp
	}

	[Serializable]
	public struct OutpostSceneSettings
	{
		[SerializeReference]
		public ISceneRoot ScenePlaceholder;

		public Transform Root;
	}

	private struct SceneRootWrapper : ISceneRoot
	{
		public Transform Root { get; }

		public SceneRootWrapper(Transform root)
		{
			Root = root;
		}

		public void Show(object data = null)
		{
			Root.gameObject.SetActive(value: true);
		}

		public void Hide()
		{
			Root.gameObject.SetActive(value: false);
		}

		public bool ShouldRefresh(object data = null)
		{
			return false;
		}
	}

	[SerializeField]
	private OutpostSceneSettings _outpostScene;

	[SerializeField]
	private OutpostSceneSettings _outpostCampScene;

	[SerializeField]
	private OutpostSceneSettings _worldMapScene;

	[SerializeField]
	private OutpostSceneSettings _characterScene;

	[SerializeField]
	private OutpostSceneSettings _partyScene;

	[SerializeField]
	private Transform _defaultScene;

	private SceneContext _currentContext;

	private Dictionary<SceneContext, ISceneRoot> _scenes;

	private const string OUTPOST_SCENE = "FHOutpostMap";

	private const string OUTPOST_CAMP_SCENE = "FHOutpostCamp";

	private const string WORLD_MAP_SCENE = "FHWorldMap";

	private const string CHARACTER_SELECTION_SCENE = "FHCharacterSelection";

	private const string PARTY_SELECTION_SCENE = "FHPartyView";

	private const float SCENE_LOAD_PERCENT = 0.9f;

	[ConsoleVariable(Alias = "debug_move_scenes", Persistent = true)]
	private static bool MOVE_AND_UNLOAD_SCENES;

	private Dictionary<string, ProfilerMarker> _markers = new Dictionary<string, ProfilerMarker>(5);

	public OutpostMapScene OutpostMapScene => (OutpostMapScene)GetScene(GetOutpostContext());

	public FHWorldMapScene WorldMapScene => (FHWorldMapScene)GetScene(SceneContext.WorldMap);

	public FHCharacterSelectionScene CharacterSelectionScene => (FHCharacterSelectionScene)GetScene(SceneContext.Character);

	public FHPartyViewScene PartyScene => (FHPartyViewScene)GetScene(SceneContext.Party);

	public IEnumerator Init(CampaignState campaignState)
	{
		SceneContext[] ctx = ((campaignState.GameMode.Name == "Tutorial") ? new SceneContext[1] : ((SceneContext[])Enum.GetValues(typeof(SceneContext))));
		yield return LoadScenesCrt(ctx);
		SceneContext[] array;
		if (MOVE_AND_UNLOAD_SCENES)
		{
			array = ctx;
			foreach (SceneContext context in array)
			{
				MoveScene(context);
			}
			yield return UnloadScenesCrt(ctx);
		}
		_scenes = new Dictionary<SceneContext, ISceneRoot>(ctx.Length);
		array = ctx;
		foreach (SceneContext sceneContext in array)
		{
			_scenes.Add(sceneContext, InitScene(sceneContext));
		}
		foreach (KeyValuePair<SceneContext, ISceneRoot> scene in _scenes)
		{
			scene.Deconstruct(out var _, out var value);
			value?.Hide();
		}
		ChangeContext(SceneContext.None, null, force: true);
	}

	public void Unload()
	{
		UnloadScenes(GetLoadedScenes());
	}

	public IEnumerator UnloadCrt()
	{
		yield return UnloadScenesCrt(GetLoadedScenes());
	}

	private IEnumerator LoadScenesCrt(IEnumerable<SceneContext> scenesToLoad)
	{
		List<AsyncOperation> loading = new List<AsyncOperation>();
		foreach (SceneContext item in scenesToLoad)
		{
			if (item != SceneContext.None)
			{
				if (Singleton<FHMapChoreographer>.InstanceFast.UsePlaceholderMaps && InstantiateScenePlaceholder(item) != null)
				{
					InitScene(item);
				}
				else
				{
					loading.Add(LoadScene(item));
				}
			}
		}
		loading.RemoveAll((AsyncOperation op) => op == null);
		while (loading.Any())
		{
			for (int num = loading.Count - 1; num >= 0; num--)
			{
				AsyncOperation asyncOperation = loading[num];
				if (asyncOperation.isDone)
				{
					loading.RemoveAt(num);
				}
				else if (asyncOperation.progress >= 0.9f)
				{
					asyncOperation.allowSceneActivation = true;
				}
			}
			yield return null;
		}
	}

	private IEnumerator UnloadScenesCrt(IEnumerable<SceneContext> scenesToUnload)
	{
		List<AsyncOperation> loading = new List<AsyncOperation>();
		foreach (SceneContext item in scenesToUnload)
		{
			if (item != SceneContext.None)
			{
				loading.Add(UnloadScene(item));
			}
		}
		loading.RemoveAll((AsyncOperation op) => op == null);
		while (loading.Any())
		{
			for (int num = loading.Count - 1; num >= 0; num--)
			{
				if (loading[num].isDone)
				{
					loading.RemoveAt(num);
				}
			}
			yield return null;
		}
	}

	private void UnloadScenes(IEnumerable<SceneContext> scenesToUnload)
	{
		new List<AsyncOperation>();
		foreach (SceneContext item in scenesToUnload)
		{
			if (item != SceneContext.None)
			{
				SceneManager.UnloadSceneAsync(GetSceneName(item));
			}
		}
	}

	private SceneContext GetOutpostContext()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast.View.GameState.GameMode.OutpostMarkerLocation == "CampOutpost")
		{
			return SceneContext.OutpostCamp;
		}
		return SceneContext.Outpost;
	}

	public void OpenOutpostMap(bool force = false)
	{
		FHMapChoreographer instanceFast = Singleton<FHMapChoreographer>.InstanceFast;
		FHMapView view = instanceFast.View;
		ChangeContext(GetOutpostContext(), new OutpostMapScene.OutpostVillageSceneData
		{
			Buildings = view.GetService<BuildingViewService>().GetBuildings().ToList(),
			AvailableToConstruct = from def in view.GetService<BuildingViewService>().GetAvailableToConstruct()
				select def.ID,
			Stickers = instanceFast.GameState.Stickers,
			Season = instanceFast.GameState.Outpost.Calendar.Season
		}, force);
	}

	public void OpenWorldMap(bool force = false)
	{
		List<FHRL.ScenarioState> item = Singleton<FHMapChoreographer>.InstanceFast?.View?.GetService<ScenarioViewService>().GetGetVisibleScenarios();
		uint valueOrDefault = (Singleton<FHMapChoreographer>.InstanceFast?.View?.GameState.Outpost.Buildings.FirstOrDefault((BuildingState state) => state.Def is Walls_BuildingTypeDef)?.Level).GetValueOrDefault();
		ChangeContext(SceneContext.WorldMap, (item, valueOrDefault), force);
	}

	public void OpenDefault()
	{
		ChangeContext(SceneContext.None, null, force: true);
	}

	public void OpenCharacterScreen(ECharacter character, bool force = false)
	{
		ChangeContext(SceneContext.Character, character, force);
	}

	public void OpenCharacterScreen(CharacterDataDTO character)
	{
		OpenCharacterScreen(CrossReference.ClassDefToECharacterModel(character.Class));
	}

	public void OpenCharacterScreen(CharacterState character)
	{
		OpenCharacterScreen(CrossReference.ClassDefToECharacterModel(character.ClassDef));
	}

	public void OpenPartyScreen(FHPartyViewScene.PartySceneParams partyParams, bool force = false)
	{
		ChangeContext(SceneContext.Party, partyParams, force);
	}

	public void Reset()
	{
		ChangeContext(SceneContext.None);
	}

	private void ChangeContext(SceneContext context, object data = null, bool force = false)
	{
		_scenes.TryGetValue(_currentContext, out var value);
		if (force || _currentContext != context || (value != null && value.ShouldRefresh(data)))
		{
			value?.Hide();
			_currentContext = context;
			_scenes.TryGetValue(_currentContext, out value);
			value?.Show(data);
		}
	}

	private ISceneRoot InitScene(SceneContext context)
	{
		return context switch
		{
			SceneContext.None => (ISceneRoot)new SceneRootWrapper(_defaultScene), 
			SceneContext.Outpost => InitOutpostScene(), 
			SceneContext.OutpostCamp => InitOutpostCampScene(), 
			SceneContext.WorldMap => InitWorldMapScene(), 
			SceneContext.Character => InitCharacterScene(), 
			SceneContext.Party => InitPartyScene(), 
			_ => throw new ArgumentOutOfRangeException("context", context, null), 
		};
	}

	private OutpostMapScene InitOutpostScene()
	{
		OutpostMapScene outpostMapScene = null;
		try
		{
			outpostMapScene = FindSceneRoot<OutpostMapScene>(SceneContext.Outpost);
			LinkUICamera(outpostMapScene.Camera);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			if (SceneController.Instance != null)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("error loading the outpost map", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		finally
		{
		}
		return outpostMapScene;
	}

	private OutpostMapScene InitOutpostCampScene()
	{
		OutpostMapScene outpostMapScene = null;
		try
		{
			outpostMapScene = FindSceneRoot<OutpostMapScene>(SceneContext.OutpostCamp);
			LinkUICamera(outpostMapScene.Camera);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			if (SceneController.Instance != null)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("error loading the outpost camp map", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		finally
		{
		}
		return outpostMapScene;
	}

	private FHWorldMapScene InitWorldMapScene()
	{
		FHWorldMapScene fHWorldMapScene = null;
		try
		{
			fHWorldMapScene = FindSceneRoot<FHWorldMapScene>(SceneContext.WorldMap);
			LinkUICamera(fHWorldMapScene.CameraRig.Camera);
			fHWorldMapScene.Init();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			if (SceneController.Instance != null)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("error loading the world map", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		finally
		{
		}
		return fHWorldMapScene;
	}

	private FHCharacterSelectionScene InitCharacterScene()
	{
		FHCharacterSelectionScene fHCharacterSelectionScene = null;
		try
		{
			fHCharacterSelectionScene = FindSceneRoot<FHCharacterSelectionScene>(SceneContext.Character);
			LinkUICamera(fHCharacterSelectionScene.Camera);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			if (SceneController.Instance != null)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("error loading the character selection scene", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		finally
		{
		}
		return fHCharacterSelectionScene;
	}

	private FHPartyViewScene InitPartyScene()
	{
		FHPartyViewScene fHPartyViewScene = null;
		try
		{
			fHPartyViewScene = FindSceneRoot<FHPartyViewScene>(SceneContext.Party);
			LinkUICamera(fHPartyViewScene.Camera);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			if (SceneController.Instance != null)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("error loading the party scene", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		finally
		{
		}
		return fHPartyViewScene;
	}

	private AsyncOperation LoadScene(SceneContext context)
	{
		using (AutoProfilerMarker($"Loading scene {context}"))
		{
			string sceneName = GetSceneName(context);
			if (sceneName.IsNullOrEmpty())
			{
				Log.Error($"Outpost scene for context {context} is missing!");
				return null;
			}
			try
			{
				AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				asyncOperation.allowSceneActivation = false;
				asyncOperation.completed += (AsyncOperation operation) =>
				{
				};
				return asyncOperation;
			}
			catch (Exception)
			{
				Log.Error("Failed to load scene with name " + sceneName);
			}
		}
		return null;
	}

	private AsyncOperation UnloadScene(SceneContext context)
	{
		using (AutoProfilerMarker($"Unloading scene {context}"))
		{
			string sceneName = GetSceneName(context);
			if (!SceneManager.GetSceneByName(sceneName).isLoaded)
			{
				return null;
			}
			return SceneManager.UnloadSceneAsync(sceneName);
		}
	}

	private ISceneRoot GetScene(SceneContext context)
	{
		if (_scenes == null)
		{
			Log.Warning("Outpost Scenes are not initialized");
			return null;
		}
		_scenes.TryGetValue(context, out var value);
		if (value == null)
		{
			Log.Warning($"Scene with context {context} has not been loaded properly!");
		}
		return value;
	}

	private IEnumerable<SceneContext> GetLoadedScenes()
	{
		if (MOVE_AND_UNLOAD_SCENES)
		{
			yield break;
		}
		foreach (KeyValuePair<SceneContext, ISceneRoot> scene in _scenes)
		{
			SceneContext key = scene.Key;
			if (SceneManager.GetSceneByName(GetSceneName(key)).isLoaded)
			{
				yield return key;
			}
		}
	}

	private string GetSceneName(SceneContext context)
	{
		return context switch
		{
			SceneContext.None => string.Empty, 
			SceneContext.Outpost => "FHOutpostMap", 
			SceneContext.WorldMap => "FHWorldMap", 
			SceneContext.Character => "FHCharacterSelection", 
			SceneContext.Party => "FHPartyView", 
			SceneContext.OutpostCamp => "FHOutpostCamp", 
			_ => throw new ArgumentOutOfRangeException("context", context, null), 
		};
	}

	private Transform GetSceneParent(SceneContext context)
	{
		return GetSettings(context)?.Root;
	}

	private ISceneRoot GetScenePlaceholder(SceneContext context)
	{
		ISceneRoot sceneRoot = GetSettings(context)?.ScenePlaceholder;
		if (sceneRoot != null)
		{
			return sceneRoot;
		}
		return null;
	}

	private OutpostSceneSettings? GetSettings(SceneContext context)
	{
		return context switch
		{
			SceneContext.None => (OutpostSceneSettings?)null, 
			SceneContext.Outpost => _outpostScene, 
			SceneContext.OutpostCamp => _outpostCampScene, 
			SceneContext.WorldMap => _worldMapScene, 
			SceneContext.Character => _characterScene, 
			SceneContext.Party => _partyScene, 
			_ => throw new ArgumentOutOfRangeException("context", context, null), 
		};
	}

	private ISceneRoot InstantiateScenePlaceholder(SceneContext context)
	{
		ISceneRoot scenePlaceholder = GetScenePlaceholder(context);
		if (scenePlaceholder == null)
		{
			Log.Warning($"Placeholder outpost scene not linked for context {context}");
			return scenePlaceholder;
		}
		Transform sceneParent = GetSceneParent(context);
		return UnityEngine.Object.Instantiate(scenePlaceholder.Root.gameObject, sceneParent).GetComponent<ISceneRoot>();
	}

	private bool MoveScene(SceneContext context)
	{
		string sceneName = GetSceneName(context);
		Scene sceneByName = SceneManager.GetSceneByName(sceneName);
		if (!sceneByName.isLoaded)
		{
			return false;
		}
		Transform sceneParent = GetSceneParent(context);
		ISceneRoot component = sceneByName.GetComponent<ISceneRoot>(inChildren: true);
		if (component == null)
		{
			Log.Error("Cannot find ISceneRoot root in scene " + sceneName);
			return false;
		}
		SceneManager.MoveGameObjectToScene(component.Root.gameObject, gameObject.scene);
		component.Root.SetParent(sceneParent);
		return true;
	}

	private T FindSceneRootComponent<T>(SceneContext context) where T : MonoBehaviour, ISceneRoot
	{
		string sceneName = GetSceneName(context);
		if (sceneName.IsNullOrEmpty())
		{
			return null;
		}
		T component = SceneManager.GetSceneByName(sceneName).GetComponent<T>(inChildren: true);
		if (component == null)
		{
			Log.Error("Cannot find FHWorldMapScene root in world world map");
			return null;
		}
		return component;
	}

	private T FindSceneRoot<T>(SceneContext context) where T : MonoBehaviour, ISceneRoot
	{
		if (GetScenePlaceholder(context) == null && !MOVE_AND_UNLOAD_SCENES)
		{
			return FindSceneRootComponent<T>(context);
		}
		return GetSceneParent(context).GetComponentInChildren<T>(includeInactive: true);
	}

	private bool LinkUICamera(Camera camera)
	{
		UniversalAdditionalCameraData component = camera.GetComponent<UniversalAdditionalCameraData>();
		if (component == null)
		{
			return false;
		}
		component.cameraStack.Clear();
		component.cameraStack.Add(UIManager.Instance.UICamera);
		return true;
	}

	[Conditional("PROFILE_OUTPOST_SCENES")]
	private void BeginProfile(string msg)
	{
		if (_markers.TryGetValue(msg, out var value))
		{
			Log.Warning("Profiler marker with name " + msg + " exists");
			return;
		}
		value = new ProfilerMarker(msg);
		_markers.Add(msg, value);
	}

	[Conditional("PROFILE_OUTPOST_SCENES")]
	private void EndProfile(string msg)
	{
		if (_markers.TryGetValue(msg, out var _))
		{
			_markers.Remove(msg);
		}
		else
		{
			Log.Warning("Profiler marker with name " + msg + " doe not exist");
		}
	}

	private ProfilerMarker.AutoScope? AutoProfilerMarker(string msg)
	{
		return null;
	}
}

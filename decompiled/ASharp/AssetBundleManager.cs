using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Libs.Utils;
using Libs.Utils.Profiler;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Events;

public class AssetBundleManager : MonoBehaviour
{
	private static AssetBundleManager m_Instance;

	public BundleLoadSettings BundleLoadConfigs;

	private List<AssetBundle> m_CurrentlyLoadedBundles;

	private List<Tuple<string, AssetBundleCreateRequest>> m_AllOnDemandLoadRequests;

	private Dictionary<ECharacter, List<string>> m_CharacterOnDemandLoadRequests;

	private Dictionary<ECharacter, List<UnityAction>> m_PostLoadActionsPerCharacter;

	private Dictionary<CClass.ENPCModel, List<string>> m_NPCOnDemandLoadRequests;

	private Dictionary<CClass.ENPCModel, List<UnityAction>> m_PostLoadActionsPerNPC;

	[HideInInspector]
	public float InitialLoadProgress;

	[HideInInspector]
	public bool InitialLoadComplete;

	private IEnumerator m_InitialLoadRoutine;

	private IEnumerator m_OnDemandLoadRoutine;

	private bool m_OnDemandRoutineLoading;

	private IEnumerator m_ScenarioLoadRoutine;

	[HideInInspector]
	public bool ScenarioBundlesLoading;

	private static Dictionary<string, Sprite> _laodedSprites = new Dictionary<string, Sprite>();

	public static AssetBundleManager Instance
	{
		get
		{
			if (!(m_Instance != null))
			{
				return m_Instance = UnityEngine.Object.FindObjectOfType<AssetBundleManager>();
			}
			return m_Instance;
		}
		set
		{
			m_Instance = value;
		}
	}

	private void Awake()
	{
		OnAwakeInit();
	}

	public void OnAwakeInit()
	{
		Instance = this;
		m_CurrentlyLoadedBundles = new List<AssetBundle>();
		m_AllOnDemandLoadRequests = new List<Tuple<string, AssetBundleCreateRequest>>();
		m_CharacterOnDemandLoadRequests = new Dictionary<ECharacter, List<string>>();
		m_PostLoadActionsPerCharacter = new Dictionary<ECharacter, List<UnityAction>>();
		m_NPCOnDemandLoadRequests = new Dictionary<CClass.ENPCModel, List<string>>();
		m_PostLoadActionsPerNPC = new Dictionary<CClass.ENPCModel, List<UnityAction>>();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void LoadAllInitiallyRequiredBundles()
	{
		InitialLoadComplete = false;
		InitialLoadProgress = 0f;
		if (m_InitialLoadRoutine != null)
		{
			StopCoroutine(m_InitialLoadRoutine);
			m_InitialLoadRoutine = null;
		}
		m_InitialLoadRoutine = LoadAllInitiallyRequiredBundlesSequentially();
		StartCoroutine(m_InitialLoadRoutine);
	}

	private IEnumerator LoadAllInitiallyRequiredBundlesSequentially()
	{
		Log.InfoFormat("[ASSET BUNDLE MANAGER] LoadAllInitiallyRequiredBundlesSequentially Started");
		float timeAtStart = Time.realtimeSinceStartup;
		List<BundleLoadSettings.BundleLoadConfig> bundlesToLoad = new List<BundleLoadSettings.BundleLoadConfig>();
		bundlesToLoad.AddRange(BundleLoadConfigs.BundleConfigs.Where((BundleLoadSettings.BundleLoadConfig b) => b.AlwaysLoaded));
		bundlesToLoad.AddRange(BundleLoadConfigs.DLCBundleConfigs.Where((BundleLoadSettings.BundleLoadConfig b) => b.AlwaysLoaded));
		float progressPerBundle = 1f / (float)bundlesToLoad.Count;
		for (int i = 0; i < bundlesToLoad.Count; i++)
		{
			if (File.Exists(bundlesToLoad[i].AssetsBundleLoadPath))
			{
				AssetBundleCreateRequest loadRequest = AssetBundle.LoadFromFileAsync(bundlesToLoad[i].AssetsBundleLoadPath);
				while (!loadRequest.isDone)
				{
					yield return null;
					InitialLoadProgress = (float)i * progressPerBundle + loadRequest.progress * progressPerBundle;
				}
				InitialLoadProgress = (float)(i + 1) * progressPerBundle;
			}
			else
			{
				Log.WarningFormat("[ASSET BUNDLE MANAGER] - Unable to find expected bundle at location {0}", bundlesToLoad[i].AssetsBundleLoadPath);
			}
		}
		BundleLoadSettings.BundleLoadConfig bundleLoadConfig = new BundleLoadSettings.BundleLoadConfig("UnityBuiltIn.bundle");
		if (File.Exists(bundleLoadConfig.AssetsBundleLoadPath))
		{
			yield return AssetBundle.LoadFromFileAsync(bundleLoadConfig.AssetsBundleLoadPath);
		}
		else
		{
			UnityEngine.Debug.LogError("Failed to load built-in shaders bundle.");
		}
		Log.InfoFormat("[ASSET BUNDLE MANAGER] - Initial game load: Finished loading {0} bundle(s) [{1}] in {2:0.00}s", bundlesToLoad.Count, string.Join(", ", bundlesToLoad.Select((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName)), Time.realtimeSinceStartup - timeAtStart);
		UpdateCurrentBundlesList();
		InitialLoadProgress = 1f;
		InitialLoadComplete = true;
	}

	public void EnsureActorPrefabBundlesLoadedBeforeAction(string actorPrefabName, UnityAction actionToPerform, bool async = true)
	{
		string flattenedPrefabName = actorPrefabName.ToLowerInvariant().RemoveSpaces();
		ECharacter eCharacter = CCharacterClass.CharacterNames.FirstOrDefault((ECharacter c) => c.ToString().ToLowerInvariant() == flattenedPrefabName);
		if (eCharacter != ECharacter.None)
		{
			EnsureCharacterLoadedBeforeAction(eCharacter, actionToPerform, async);
			return;
		}
		CClass.ENPCModel eNPCModel = CClass.NPCModels.FirstOrDefault((CClass.ENPCModel n) => n.ToString().ToLowerInvariant() == flattenedPrefabName);
		if (eNPCModel != CClass.ENPCModel.None)
		{
			EnsureNPCLoadedBeforeAction(eNPCModel, actionToPerform, async);
		}
	}

	public void EnsureCharacterLoadedBeforeAction(ECharacter characterToBeLoaded, UnityAction actionToPeform, bool async = true)
	{
		List<BundleLoadSettings.BundleLoadConfig> bundlesNeededForRequirements = BundleLoadConfigs.GetBundlesNeededForRequirements(new List<ECharacter> { characterToBeLoaded }, null);
		bundlesNeededForRequirements.RemoveAll((BundleLoadSettings.BundleLoadConfig s) => m_CurrentlyLoadedBundles.Any((AssetBundle b) => b.name == s.AssetBundleName));
		if (bundlesNeededForRequirements.Count > 0)
		{
			if (async)
			{
				if (!m_PostLoadActionsPerCharacter.ContainsKey(characterToBeLoaded))
				{
					m_PostLoadActionsPerCharacter.Add(characterToBeLoaded, new List<UnityAction>());
				}
				m_PostLoadActionsPerCharacter[characterToBeLoaded].Add(actionToPeform);
				{
					foreach (BundleLoadSettings.BundleLoadConfig bundleConfigToLoad in bundlesNeededForRequirements)
					{
						if (!m_AllOnDemandLoadRequests.Any((Tuple<string, AssetBundleCreateRequest> t) => t.Item1 == bundleConfigToLoad.AssetBundleName))
						{
							CreateOnDemandBundleLoadRequest(bundleConfigToLoad);
							if (!m_CharacterOnDemandLoadRequests.ContainsKey(characterToBeLoaded))
							{
								m_CharacterOnDemandLoadRequests.Add(characterToBeLoaded, new List<string>());
							}
							m_CharacterOnDemandLoadRequests[characterToBeLoaded].Add(bundleConfigToLoad.AssetBundleName);
						}
					}
					return;
				}
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (BundleLoadSettings.BundleLoadConfig item in bundlesNeededForRequirements)
			{
				AssetBundle.LoadFromFile(item.AssetsBundleLoadPath);
			}
			Log.InfoFormat("[ASSET BUNDLE MANAGER] - Lazy-loaded the following bundle(s) syncronously: [{0}] in {1:0.00}s", string.Join(", ", bundlesNeededForRequirements.Select((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName).ToArray()), Time.realtimeSinceStartup - realtimeSinceStartup);
			UpdateCurrentBundlesList();
			actionToPeform?.Invoke();
		}
		else
		{
			UpdateCurrentBundlesList();
			actionToPeform?.Invoke();
		}
	}

	public void EnsureNPCLoadedBeforeAction(CClass.ENPCModel npcToBeLoaded, UnityAction actionToPeform, bool async = true)
	{
		List<BundleLoadSettings.BundleLoadConfig> bundlesNeededForRequirements = BundleLoadConfigs.GetBundlesNeededForRequirements(null, new List<CClass.ENPCModel> { npcToBeLoaded });
		bundlesNeededForRequirements.RemoveAll((BundleLoadSettings.BundleLoadConfig s) => m_CurrentlyLoadedBundles.Any((AssetBundle b) => b.name == s.AssetBundleName));
		if (bundlesNeededForRequirements.Count > 0)
		{
			if (async)
			{
				if (!m_PostLoadActionsPerNPC.ContainsKey(npcToBeLoaded))
				{
					m_PostLoadActionsPerNPC.Add(npcToBeLoaded, new List<UnityAction>());
				}
				m_PostLoadActionsPerNPC[npcToBeLoaded].Add(actionToPeform);
				{
					foreach (BundleLoadSettings.BundleLoadConfig bundleConfigToLoad in bundlesNeededForRequirements)
					{
						if (!m_AllOnDemandLoadRequests.Any((Tuple<string, AssetBundleCreateRequest> t) => t.Item1 == bundleConfigToLoad.AssetBundleName))
						{
							CreateOnDemandBundleLoadRequest(bundleConfigToLoad);
							if (!m_NPCOnDemandLoadRequests.ContainsKey(npcToBeLoaded))
							{
								m_NPCOnDemandLoadRequests.Add(npcToBeLoaded, new List<string>());
							}
							m_NPCOnDemandLoadRequests[npcToBeLoaded].Add(bundleConfigToLoad.AssetBundleName);
						}
					}
					return;
				}
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (BundleLoadSettings.BundleLoadConfig item in bundlesNeededForRequirements)
			{
				AssetBundle.LoadFromFile(item.AssetsBundleLoadPath);
			}
			Log.InfoFormat("[ASSET BUNDLE MANAGER] - Lazy-loaded the following bundle(s) syncronously: [{0}] in {1:0.00}s", string.Join(", ", bundlesNeededForRequirements.Select((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName).ToArray()), Time.realtimeSinceStartup - realtimeSinceStartup);
			UpdateCurrentBundlesList();
			actionToPeform?.Invoke();
		}
		else
		{
			UpdateCurrentBundlesList();
			actionToPeform?.Invoke();
		}
	}

	private void CreateOnDemandBundleLoadRequest(BundleLoadSettings.BundleLoadConfig bundleConfig)
	{
		m_AllOnDemandLoadRequests.Add(new Tuple<string, AssetBundleCreateRequest>(bundleConfig.AssetBundleName, AssetBundle.LoadFromFileAsync(bundleConfig.AssetsBundleLoadPath)));
		if (!m_OnDemandRoutineLoading)
		{
			if (m_OnDemandLoadRoutine != null)
			{
				StopCoroutine(m_OnDemandLoadRoutine);
				m_OnDemandLoadRoutine = null;
			}
			m_OnDemandLoadRoutine = ContinuouslyEvaluateCurrentOnDemandLoadRequests();
			StartCoroutine(m_OnDemandLoadRoutine);
		}
	}

	private IEnumerator ContinuouslyEvaluateCurrentOnDemandLoadRequests()
	{
		while (m_AllOnDemandLoadRequests.Count > 0)
		{
			m_OnDemandRoutineLoading = true;
			yield return null;
			List<Tuple<string, AssetBundleCreateRequest>> list = new List<Tuple<string, AssetBundleCreateRequest>>();
			foreach (Tuple<string, AssetBundleCreateRequest> allOnDemandLoadRequest in m_AllOnDemandLoadRequests)
			{
				if (allOnDemandLoadRequest.Item2.isDone)
				{
					list.Add(allOnDemandLoadRequest);
				}
			}
			if (list.Count > 0)
			{
				UpdateCurrentBundlesList();
			}
			foreach (Tuple<string, AssetBundleCreateRequest> item in list)
			{
				m_AllOnDemandLoadRequests.Remove(item);
				List<ECharacter> list2 = new List<ECharacter>();
				foreach (ECharacter key in m_CharacterOnDemandLoadRequests.Keys)
				{
					if (m_CharacterOnDemandLoadRequests[key].Contains(item.Item1))
					{
						list2.Add(key);
					}
				}
				foreach (ECharacter item2 in list2)
				{
					m_CharacterOnDemandLoadRequests[item2].Remove(item.Item1);
					if (m_CharacterOnDemandLoadRequests[item2].Count != 0)
					{
						continue;
					}
					foreach (UnityAction item3 in m_PostLoadActionsPerCharacter[item2])
					{
						item3?.Invoke();
					}
					m_CharacterOnDemandLoadRequests.Remove(item2);
					m_PostLoadActionsPerCharacter.Remove(item2);
				}
				List<CClass.ENPCModel> list3 = new List<CClass.ENPCModel>();
				foreach (CClass.ENPCModel key2 in m_NPCOnDemandLoadRequests.Keys)
				{
					if (m_NPCOnDemandLoadRequests[key2].Contains(item.Item1))
					{
						list3.Add(key2);
					}
				}
				foreach (CClass.ENPCModel item4 in list3)
				{
					m_NPCOnDemandLoadRequests[item4].Remove(item.Item1);
					if (m_NPCOnDemandLoadRequests[item4].Count != 0)
					{
						continue;
					}
					foreach (UnityAction item5 in m_PostLoadActionsPerNPC[item4])
					{
						item5?.Invoke();
					}
					m_NPCOnDemandLoadRequests.Remove(item4);
					m_PostLoadActionsPerNPC.Remove(item4);
				}
			}
		}
		m_OnDemandRoutineLoading = false;
	}

	public bool LoadBundleManually(string assetBundleName)
	{
		if (IsAssetBundleLoaded(assetBundleName))
		{
			return true;
		}
		BundleLoadSettings.BundleLoadConfig bundleLoadConfig = BundleLoadConfigs.BundleConfigs.SingleOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName.Equals(assetBundleName));
		if (bundleLoadConfig == null)
		{
			return false;
		}
		AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleLoadConfig.AssetsBundleLoadPath);
		UpdateCurrentBundlesList();
		return assetBundle != null;
	}

	public void UnloadBundleManually(string assetBundleName)
	{
		AssetBundle assetBundle = m_CurrentlyLoadedBundles?.SingleOrDefault((AssetBundle x) => x.name == assetBundleName);
		if (!(assetBundle == null))
		{
			assetBundle.Unload(unloadAllLoadedObjects: true);
			UpdateCurrentBundlesList();
		}
	}

	public void LoadAllScenarioAssetsBeforeAction(ScenarioState scenarioStateBeingLoaded, CMapState mapStateBeingLoaded = null, UnityAction postLoadAction = null)
	{
		ScenarioBundlesLoading = true;
		List<ECharacter> charactersNeeded = new List<ECharacter>();
		List<CClass.ENPCModel> modelsNeeded = new List<CClass.ENPCModel>();
		if (scenarioStateBeingLoaded != null)
		{
			ScrapeScenarioStateForRequiredChars(scenarioStateBeingLoaded, scenarioStateBeingLoaded.Players.Count, out charactersNeeded, out modelsNeeded);
		}
		List<ECharacter> charactersNeeded2 = new List<ECharacter>();
		List<CClass.ENPCModel> modelsNeeded2 = new List<CClass.ENPCModel>();
		if (mapStateBeingLoaded != null && mapStateBeingLoaded.CurrentMapScenarioState != null && mapStateBeingLoaded.CurrentMapScenarioState.CurrentState != null)
		{
			ScrapeScenarioStateForRequiredChars(mapStateBeingLoaded.CurrentMapScenarioState.CurrentState, mapStateBeingLoaded.MapParty.CheckCharacters.Count, out charactersNeeded2, out modelsNeeded2);
		}
		List<ECharacter> list = new List<ECharacter>();
		list.AddRange(charactersNeeded);
		list.AddRange(charactersNeeded2);
		list = list.Distinct().ToList();
		List<CClass.ENPCModel> list2 = new List<CClass.ENPCModel>();
		list2.AddRange(modelsNeeded);
		list2.AddRange(modelsNeeded2);
		list2 = list2.Distinct().ToList();
		if (m_ScenarioLoadRoutine != null)
		{
			StopCoroutine(m_ScenarioLoadRoutine);
			m_ScenarioLoadRoutine = null;
		}
		m_ScenarioLoadRoutine = LoadRequiredBundles(BundleLoadConfigs.GetBundlesNeededForRequirements(list, list2), postLoadAction);
		StartCoroutine(m_ScenarioLoadRoutine);
	}

	private IEnumerator LoadRequiredBundles(List<BundleLoadSettings.BundleLoadConfig> bundlesToLoad, UnityAction postLoadAction)
	{
		float timeAtStart = Time.realtimeSinceStartup;
		List<AssetBundleCreateRequest> loadRequests = new List<AssetBundleCreateRequest>();
		foreach (BundleLoadSettings.BundleLoadConfig bundleToLoad in bundlesToLoad)
		{
			if (!m_CurrentlyLoadedBundles.Any((AssetBundle b) => b.name == bundleToLoad.AssetBundleName))
			{
				AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(bundleToLoad.AssetsBundleLoadPath);
				loadRequests.Add(assetBundleCreateRequest);
				yield return assetBundleCreateRequest;
			}
		}
		if (loadRequests.Count > 0)
		{
			Log.InfoFormat("[ASSET BUNDLE MANAGER] - Scenario preparation: Finished loading {0} bundle(s) [{1}] in {2:0.00}s", loadRequests.Count, string.Join(", ", loadRequests.Select((AssetBundleCreateRequest l) => l.assetBundle.name)), Time.realtimeSinceStartup - timeAtStart);
		}
		UpdateCurrentBundlesList();
		ScenarioBundlesLoading = false;
		postLoadAction?.Invoke();
	}

	public void UnloadNotRequiredBundles()
	{
		ScenarioBundlesLoading = true;
		UpdateCurrentBundlesList();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		List<AssetBundle> list = new List<AssetBundle>();
		foreach (AssetBundle loadedBundle in m_CurrentlyLoadedBundles)
		{
			if (loadedBundle.name == "UnityBuiltIn.bundle")
			{
				continue;
			}
			BundleLoadSettings.BundleLoadConfig bundleLoadConfig = BundleLoadConfigs.BundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName == loadedBundle.name);
			if (bundleLoadConfig == null)
			{
				bundleLoadConfig = BundleLoadConfigs.DLCBundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName == loadedBundle.name);
			}
			if (bundleLoadConfig != null && !bundleLoadConfig.AlwaysLoaded)
			{
				list.Add(loadedBundle);
			}
		}
		List<string> list2 = list.Select((AssetBundle b) => b.name).ToList();
		if (list.Count > 0)
		{
			foreach (AssetBundle item in list)
			{
				item.Unload(unloadAllLoadedObjects: true);
			}
			Log.InfoFormat("[ASSET BUNDLE MANAGER] - Finished unloading {0} bundle(s) [{1}] in {2:0.00}s", list2.Count, string.Join(", ", list2), Time.realtimeSinceStartup - realtimeSinceStartup);
		}
		UpdateCurrentBundlesList();
		ScenarioBundlesLoading = false;
	}

	public void DebugRequiredBundles(ScenarioState stateToCheck, int partySize)
	{
		List<ECharacter> charactersNeeded = new List<ECharacter>();
		List<CClass.ENPCModel> modelsNeeded = new List<CClass.ENPCModel>();
		ScrapeScenarioStateForRequiredChars(stateToCheck, partySize, out charactersNeeded, out modelsNeeded);
		string text = string.Empty;
		string text2 = string.Empty;
		foreach (ECharacter item in charactersNeeded)
		{
			text += string.Format("{0}{1}", string.IsNullOrEmpty(text) ? "" : ", ", item.ToString());
		}
		foreach (CClass.ENPCModel item2 in modelsNeeded)
		{
			text2 += string.Format("{0}{1}", string.IsNullOrEmpty(text2) ? "" : ", ", item2.ToString());
		}
		Log.InfoFormat("[ASSET BUNDLE MANAGER] - Scenario requires {0} ECharacters and {1} ENPCModels. ECharacters = {2} | ENPCModels = {3}", charactersNeeded.Count, modelsNeeded.Count, text, text2);
	}

	[ContextMenu("Debug print all loaded bundles")]
	public void DebugLoadedBundles()
	{
		List<string> list = (from b in AssetBundle.GetAllLoadedAssetBundles()
			select b.name).ToList();
		Log.InfoFormat("[ASSET BUNDLE MANAGER] - There are currently {0} bundle(s) loaded: [{1}]", list.Count, string.Join(", ", list));
	}

	public T1 LoadAssetFromBundle<T1>(string assetBundleName, string fileName, string assetDatabaseFolderName, string fileType = "prefab", string explicitPath = null, bool suppressError = false) where T1 : UnityEngine.Object
	{
		string assetPath = (string.IsNullOrEmpty(explicitPath) ? Path.Combine("Assets", "_AssetBundles", assetDatabaseFolderName, fileName + "." + fileType).Replace("\\", "/") : explicitPath);
		if (0 == 0)
		{
			return Instance.LoadAssetFromBundleInternal<T1>(assetBundleName, assetPath, suppressError);
		}
		return null;
	}

	public IEnumerator LoadAssetFromBundleAsync<T1>(string assetBundleName, string fileName, string assetDatabaseFolderName, ByRef<T1> outAsset, string fileType = "prefab", string explicitPath = null, bool suppressError = false) where T1 : UnityEngine.Object
	{
		outAsset.Value = null;
		string assetPath = (string.IsNullOrEmpty(explicitPath) ? Path.Combine("Assets", "_AssetBundles", assetDatabaseFolderName, fileName + "." + fileType).Replace("\\", "/") : explicitPath);
		if (0 == 0)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			AssetBundleRequest op = Instance.LoadAssetFromBundleInternalAsync<T1>(assetBundleName, assetPath, suppressError);
			if (op != null)
			{
				yield return op;
				outAsset.Value = (T1)op.asset;
			}
			sw.Stop();
			if (sw.Elapsed.TotalSeconds > 0.10000000149011612)
			{
				Log.Warning($"[ASSET BUNDLE MANAGER] - Loading async asset {assetPath} from bundle {assetBundleName} took {sw.Elapsed.TotalSeconds:F3} seconds!");
			}
		}
	}

	private AssetBundleRequest LoadAssetFromBundleInternalAsync<T1>(string assetBundleName, string assetPath, bool supressError = false) where T1 : UnityEngine.Object
	{
		AssetBundle assetBundle = m_CurrentlyLoadedBundles.SingleOrDefault((AssetBundle x) => x.name == assetBundleName);
		if (assetBundle != null)
		{
			AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<T1>(assetPath);
			if (assetBundleRequest.isDone && assetBundleRequest.asset == null && !supressError)
			{
				Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Unable to load asset with path: {0} in asset bundle: {1}", assetPath, assetBundleName);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00026", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
			}
			return assetBundleRequest;
		}
		Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Unable to find asset bundle for name: {0}", assetBundleName);
		SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00025", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
		return null;
	}

	private T1 LoadAssetFromBundleInternal<T1>(string assetBundleName, string assetPath, bool supressError = false) where T1 : UnityEngine.Object
	{
		T1 val = null;
		AssetBundle assetBundle = m_CurrentlyLoadedBundles.SingleOrDefault((AssetBundle x) => x.name == assetBundleName);
		if (assetBundle != null)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			val = assetBundle.LoadAsset<T1>(assetPath);
			if (val == null)
			{
				string value = assetBundle.GetAllAssetNames().FirstOrDefault((string p) => p.Equals(assetPath, StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrEmpty(value))
				{
					val = assetBundle.LoadAsset<T1>(value);
				}
			}
			stopwatch.Stop();
			if (val != null)
			{
				if (stopwatch.Elapsed.TotalSeconds > 0.10000000149011612)
				{
					Log.Warning($"[ASSET BUNDLE MANAGER] - Loading on main thread asset {assetPath} from bundle {assetBundleName} took {stopwatch.Elapsed.TotalSeconds:F3} seconds!");
				}
				return val;
			}
			if (!supressError)
			{
				Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Unable to load asset with path: {0} in asset bundle: {1}", assetPath, assetBundleName);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00026", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
			}
			else
			{
				Log.WarningFormat("[ASSET BUNDLE MANAGER] - Unable to load asset with path: {0} in asset bundle: {1}", assetPath, assetBundleName);
			}
			return null;
		}
		Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Unable to find asset bundle for name: {0}", assetBundleName);
		SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00025", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
		return null;
	}

	public async Task<GameObject> GetCharacterPrefabFromBundleAsync(CActor.EType type, string prefabName, string skinString = null, bool lazyLoadIfMissing = true)
	{
		using (new MiniProfilerScope("AssetBundleManager.GetCharacterPrefabFromBundleAsync"))
		{
			BundleLoadSettings.BundleLoadConfig bundleConfig = BundleConfigForPrefab(type, prefabName);
			_ = string.Empty;
			string text = ((bundleConfig.BundleDLC != DLCRegistry.EDLCKey.None) ? Path.Combine("Assets", "_AssetBundles", GloomUtility.GetEnumCategory(bundleConfig.BundleDLC), "Content", "CharacterPrefabs", string.Format("{0}{1}{2}.prefab", prefabName, string.IsNullOrEmpty(skinString) ? string.Empty : "_", skinString)).Replace("\\", "/") : Path.Combine("Assets", "_AssetBundles", (type == CActor.EType.Player) ? "heroes" : "npcs", string.Format("{0}{1}{2}.prefab", prefabName, string.IsNullOrEmpty(skinString) ? string.Empty : "_", skinString)).Replace("\\", "/"));
			AssetBundle assetBundle = m_CurrentlyLoadedBundles.FirstOrDefault((AssetBundle b) => b.name == bundleConfig.AssetBundleName);
			if ((assetBundle == null) & lazyLoadIfMissing)
			{
				bool wait = true;
				EnsureActorPrefabBundlesLoadedBeforeAction(prefabName, () =>
				{
					wait = false;
				});
				while (wait)
				{
					using (new MiniProfilerScope("yield for EnsureActorPrefabBundlesLoadedBeforeAction"))
					{
						await Task.Yield();
					}
				}
				return await GetCharacterPrefabFromBundleAsync(type, prefabName, skinString, lazyLoadIfMissing: false);
			}
			if (assetBundle == null)
			{
				Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Asset bundle for character prefab \"{0}\" not yet loaded, make sure to call EnsureCharacterLoadedBeforeAction first, or call this method with the \"lazyLoadIfMissing\" set to true.", prefabName);
				return null;
			}
			GameObject obj = assetBundle.LoadAsset<GameObject>(text);
			await MF.YieldIfOverBudget("await in YieldIfOverBudget after LoadAsset");
			return obj;
		}
	}

	public GameObject GetCharacterPrefabFromBundle(CActor.EType type, string prefabName, string skinString = null, bool lazyLoadIfMissing = true)
	{
		BundleLoadSettings.BundleLoadConfig bundleConfig = BundleConfigForPrefab(type, prefabName);
		string empty = string.Empty;
		empty = ((bundleConfig.BundleDLC != DLCRegistry.EDLCKey.None) ? Path.Combine("Assets", "_AssetBundles", GloomUtility.GetEnumCategory(bundleConfig.BundleDLC), "Content", "CharacterPrefabs", string.Format("{0}{1}{2}.prefab", prefabName, string.IsNullOrEmpty(skinString) ? string.Empty : "_", skinString)).Replace("\\", "/") : Path.Combine("Assets", "_AssetBundles", (type == CActor.EType.Player) ? "heroes" : "npcs", string.Format("{0}{1}{2}.prefab", prefabName, string.IsNullOrEmpty(skinString) ? string.Empty : "_", skinString)).Replace("\\", "/"));
		AssetBundle assetBundle = m_CurrentlyLoadedBundles.FirstOrDefault((AssetBundle b) => b.name == bundleConfig.AssetBundleName);
		if ((assetBundle == null) & lazyLoadIfMissing)
		{
			EnsureActorPrefabBundlesLoadedBeforeAction(prefabName, null, async: false);
			return GetCharacterPrefabFromBundle(type, prefabName, skinString, lazyLoadIfMissing: false);
		}
		if (assetBundle == null)
		{
			Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Asset bundle for character prefab \"{0}\" not yet loaded, make sure to call EnsureCharacterLoadedBeforeAction first, or call this method with the \"lazyLoadIfMissing\" set to true.", prefabName);
			return null;
		}
		return assetBundle.LoadAsset<GameObject>(empty);
	}

	public BundleLoadSettings.BundleLoadConfig BundleConfigForPrefab(CActor.EType type, string prefabName)
	{
		string prefabBundleName = string.Empty;
		BundleLoadSettings.BundleLoadConfig.EBundleConfigType bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.None;
		if (type == CActor.EType.Player)
		{
			prefabBundleName = string.Format("{0}_{1}", "hero", prefabName.ToLowerInvariant().RemoveSpaces());
			bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.Hero;
		}
		else
		{
			prefabBundleName = string.Format("{0}_{1}", "npc", prefabName.ToLowerInvariant().RemoveSpaces());
			bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.NPC;
		}
		BundleLoadSettings.BundleLoadConfig bundleLoadConfig = BundleLoadConfigs.BundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.BundleConfigType == bundleTypeToFind && b.AssetBundleName == prefabBundleName);
		if (bundleLoadConfig == null)
		{
			foreach (BundleLoadSettings.BundleLoadConfig dLCBundleConfig in BundleLoadConfigs.DLCBundleConfigs)
			{
				string[] array = dLCBundleConfig.AssetBundleName.Split('_');
				if (array.Length == 4)
				{
					string text = $"{array[2]}_{array[3]}";
					if (prefabBundleName == text)
					{
						bundleLoadConfig = dLCBundleConfig;
						break;
					}
				}
				else
				{
					Log.WarningFormat("[ASSET BUNDLE MANAGER] - Unsupported DLC bundle config name {0}", dLCBundleConfig.AssetBundleName);
				}
			}
			if (bundleLoadConfig == null)
			{
				prefabBundleName = string.Format("{0}_{1}", "npc", prefabName.ToLowerInvariant().RemoveSpaces());
				bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.NPC;
				bundleLoadConfig = BundleLoadConfigs.BundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.BundleConfigType == bundleTypeToFind && b.AssetBundleName == prefabBundleName);
				if (bundleLoadConfig == null)
				{
					Log.ErrorFormat("[ASSET BUNDLE MANAGER] - Failed to find bundle config for character prefab: " + prefabName + " (" + prefabBundleName + ")");
					return null;
				}
			}
		}
		return bundleLoadConfig;
	}

	private void UpdateCurrentBundlesList()
	{
		m_CurrentlyLoadedBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
	}

	public bool AssetBundleExists(string assetBundleName)
	{
		return BundleLoadConfigs.BundleConfigs.SingleOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.AssetBundleName.Equals(assetBundleName)) != null;
	}

	public bool IsAssetBundleLoaded(string assetBundleName)
	{
		return m_CurrentlyLoadedBundles?.SingleOrDefault((AssetBundle x) => x.name == assetBundleName) != null;
	}

	public static void ScrapeScenarioStateForRequiredChars(ScenarioState stateToScrape, int partySize, out List<ECharacter> charactersNeeded, out List<CClass.ENPCModel> modelsNeeded)
	{
		charactersNeeded = new List<ECharacter>();
		modelsNeeded = new List<CClass.ENPCModel>();
		foreach (PlayerState player in stateToScrape.Players)
		{
			ECharacter eCharacterFromPlayerState = GetECharacterFromPlayerState(player);
			if (!charactersNeeded.Contains(eCharacterFromPlayerState))
			{
				charactersNeeded.Add(eCharacterFromPlayerState);
			}
		}
		foreach (EnemyState allEnemyState in stateToScrape.AllEnemyStates)
		{
			CClass.ENPCModel nPCModelFromEnemyState = GetNPCModelFromEnemyState(allEnemyState, partySize);
			if (nPCModelFromEnemyState != CClass.ENPCModel.None && !modelsNeeded.Contains(nPCModelFromEnemyState))
			{
				modelsNeeded.Add(nPCModelFromEnemyState);
			}
		}
		foreach (CObjectProp prop in stateToScrape.Props)
		{
			if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && !modelsNeeded.Contains(CClass.ENPCModel.PropDummyObject))
			{
				modelsNeeded.Add(CClass.ENPCModel.PropDummyObject);
				break;
			}
		}
		foreach (CSpawner spawner in stateToScrape.Spawners)
		{
			List<CClass.ENPCModel> nPCModelsFromSpawner = GetNPCModelsFromSpawner(spawner, partySize);
			if (nPCModelsFromSpawner == null || nPCModelsFromSpawner.Count <= 0)
			{
				continue;
			}
			foreach (CClass.ENPCModel item in nPCModelsFromSpawner)
			{
				if (!modelsNeeded.Contains(item))
				{
					modelsNeeded.Add(item);
				}
			}
		}
	}

	private static ECharacter GetECharacterFromPlayerState(PlayerState playerState)
	{
		return CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.ID == playerState.ClassID)?.CharacterModel ?? ECharacter.None;
	}

	private static CClass.ENPCModel GetNPCModelFromEnemyState(EnemyState enemyState, int partySize)
	{
		CMonsterClass monsterClass = MonsterClassManager.MonsterAndObjectClassesWithInitiative.FirstOrDefault((CMonsterClass c) => c.ID == enemyState.ClassID);
		if (monsterClass != null)
		{
			ScenarioManager.EPerPartySizeConfig configForPartySize = enemyState.GetConfigForPartySize(partySize);
			CClass.ENPCModel npcModelType = MonstersYML.GetNPCModelEnumFromSpaceName(monsterClass.Models[enemyState.ChosenModelIndex]);
			switch (configForPartySize)
			{
			case ScenarioManager.EPerPartySizeConfig.Normal:
				return CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s == npcModelType);
			case ScenarioManager.EPerPartySizeConfig.Hidden:
				return CClass.ENPCModel.None;
			case ScenarioManager.EPerPartySizeConfig.ToElite:
				if (monsterClass.NonEliteVariant != null)
				{
					return CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s == npcModelType);
				}
				if (MonsterClassManager.MonsterAndObjectClassesWithInitiative.FirstOrDefault((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == monsterClass.ID) == null)
				{
					Log.WarningFormat("[ASSET BUNDLE MANAGER] - Unable to find Elite version of monster class \"{0}\"", monsterClass.ID);
					return CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s == npcModelType);
				}
				return CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s == npcModelType);
			}
		}
		return CClass.ENPCModel.None;
	}

	private static List<CClass.ENPCModel> GetNPCModelsFromSpawner(CSpawner spawner, int partySize)
	{
		List<CClass.ENPCModel> list = new List<CClass.ENPCModel>();
		foreach (string monsterString in spawner.SpawnerData.SpawnRoundEntryDictionary.Values.SelectMany((List<SpawnRoundEntry> l) => l.SelectMany((SpawnRoundEntry e) => e.SpawnClass)).ToList())
		{
			CMonsterClass cMonsterClass = MonsterClassManager.MonsterAndObjectClassesWithInitiative.FirstOrDefault((CMonsterClass c) => c.ID == monsterString);
			if (cMonsterClass != null)
			{
				CClass.ENPCModel npcModelType = MonstersYML.GetNPCModelEnumFromSpaceName(cMonsterClass.DefaultModel);
				CClass.ENPCModel eNPCModel = CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s == npcModelType);
				if (eNPCModel != CClass.ENPCModel.None)
				{
					list.Add(eNPCModel);
				}
			}
		}
		return list;
	}

	public static void ClearImages()
	{
		_laodedSprites.Clear();
	}

	public static Sprite LoadImage(string path)
	{
		path = path.Replace("\\", "/");
		if (_laodedSprites.TryGetValue(path, out var value))
		{
			return value;
		}
		byte[] array = null;
		string path2 = Path.Combine(Application.streamingAssetsPath, path + ".png");
		if (File.Exists(path2))
		{
			array = File.ReadAllBytes(path2);
		}
		else
		{
			Log.Error("File not found while trying to load image. Path: '" + path + "'");
		}
		if (array == null)
		{
			Log.Error("Unable to load image from path " + path);
			return null;
		}
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.LoadImage(array);
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		_laodedSprites[path] = sprite;
		return sprite;
	}
}

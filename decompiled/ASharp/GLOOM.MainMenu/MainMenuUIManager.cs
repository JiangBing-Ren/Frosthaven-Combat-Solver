using System;
using System.Collections;
using System.Collections.Generic;
using FH.GraphicalUserInterface.MainMenu;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class MainMenuUIManager : MonoBehaviour
{
	public static MainMenuUIManager Instance;

	[SerializeField]
	private CanvasGroup canvasGroupInteraction;

	private HashSet<object> requestDisableInteraction = new HashSet<object>();

	[Header("Screen Components")]
	public UIMainOptionsMenu mainMenu;

	public CustomPartySetup CustomPartySetupScreen;

	public LevelEditorMainMenu LevelEditorMainMenuScreen;

	public UIMainMenuModeSelection ModeSelectionScreen;

	public UIModuleWelcomeWindow WelcomeScreen;

	public UIWindow IntroCinematicScreen;

	private static Action m_OnLoadingCompleteCallback;

	public static bool JoinSessionCommandLineProcessed;

	private bool m_Initialised;

	public bool Initialised => m_Initialised;

	public bool IntroCinematicIsPlaying => IntroCinematicScreen?.IsOpen ?? false;

	private void Awake()
	{
		m_Initialised = false;
		Instance = this;
	}

	private void Start()
	{
		Log.GUITrace("BRYNN: MainMenuUIManager Start() Start" + DateTime.Now);
		StartCoroutine(InitializeWhenFullyloaded());
		Log.GUITrace("BRYNN: MainMenuUIManager Start() End" + DateTime.Now);
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void OnEnable()
	{
		Log.GUITrace("BRYNN: MainMenuUIManager OnEnable Start " + DateTime.Now);
		StatsDataTracker.Instance.m_IsCampaignModeRunning = false;
		StatsDataTracker.Instance.m_IsNewAdventureModeRunning = false;
		Log.GUITrace("BRYNN: MainMenuUIManager OnEnable End " + DateTime.Now);
		if (MainMenuUIAutomation.LevelEditorAutoSelectMode)
		{
			MainMenuUIAutomation.LevelEditorAutoSelectMode = false;
			ModeSelectionScreen.ShowForLevelEditor(autoSelectRuleset: true, ScenarioManager.EDLLMode.Campaign);
			MainMenuUIAutomation.LevelEditorAutoSelectMode = true;
		}
	}

	public void RequestDisableInteraction(bool disable, object request)
	{
		if (disable)
		{
			requestDisableInteraction.Add(request);
		}
		else
		{
			requestDisableInteraction.Remove(request);
		}
		canvasGroupInteraction.blocksRaycasts = requestDisableInteraction.Count == 0;
	}

	public static void SetLoadingCompleteCallback(Action callbackToSet)
	{
		m_OnLoadingCompleteCallback = callbackToSet;
	}

	private IEnumerator InitializeWhenFullyloaded()
	{
		Log.GUITrace("BRYNN: MainMenuUIManager InitializeWhenFullyloaded Start " + DateTime.Now);
		while (!PersistentData.s_Instance.IsDataLoaded)
		{
			yield return null;
		}
		Log.GUITrace("BRYNN: MainMenuUIManager InitializeWhenFullyloaded DataIsLoaded " + DateTime.Now);
		yield return null;
		InitializeUI();
		m_OnLoadingCompleteCallback?.Invoke();
		m_OnLoadingCompleteCallback = null;
		Log.GUITrace("BRYNN: MainMenuUIManager InitializeWhenFullyloaded End " + DateTime.Now);
	}

	private void InitializeUI()
	{
		SceneController.Instance.MainSceneCamera.SetActive(value: false);
		mainMenu.Show();
		m_Initialised = true;
	}

	public void ShowIntroCinematic()
	{
		IntroCinematicScreen?.Show();
	}
}

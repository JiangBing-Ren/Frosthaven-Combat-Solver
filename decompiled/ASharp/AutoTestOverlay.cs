using System;
using System.Linq;
using System.Threading.Tasks;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoTestOverlay : MonoBehaviour
{
	public GameObject[] UIForRecording;

	public GameObject[] UIForNotRecording;

	public GameObject[] UIForPlayingFromMenu;

	public GameObject[] UIForPlayingAsAutoTest;

	public GameObject UIInteractionBlocker;

	[Header("Top Panel")]
	public TextMeshProUGUI TitleLabel;

	[Header("Left Panel")]
	public TextMeshProUGUI EvaluationLogText;

	[Header("Right Panel")]
	public Transform RightPanel;

	public Button PlaybackRecordedActionsButton;

	public TextMeshProUGUI RecordedActionsText;

	public TextMeshProUGUI StepCountText;

	public TextMeshProUGUI SteppingStatusText;

	public Button StartRecordingButton;

	public Button StopAndSaveButton;

	public Button ManuallyExecuteButton;

	public Button UpdateState;

	public Button ExitToMainMenu;

	private void Awake()
	{
		foreach (Transform child in RightPanel)
		{
			if (AutoTestController.s_Instance.CurrentState == EAutoTestControllerState.EditingTargetState || AutoTestController.s_Instance.CurrentState == EAutoTestControllerState.PlayFromLevelEditor)
			{
				child.gameObject.SetActive(UIForNotRecording.Any((GameObject go) => go == child.gameObject));
			}
			else if (AutoTestController.s_Instance.CurrentState == EAutoTestControllerState.PlayFromMenu)
			{
				child.gameObject.SetActive(UIForPlayingFromMenu.Any((GameObject go) => go == child.gameObject));
			}
			else if (AutoTestController.s_Instance.CurrentState == EAutoTestControllerState.PlayAsAutotest)
			{
				child.gameObject.SetActive(UIForPlayingAsAutoTest.Any((GameObject go) => go == child.gameObject));
			}
		}
		UpdateUIForTestReadyState();
		TitleLabel.text = "AUTOTEST [" + AutoTestController.s_Instance.CurrentAutoTestData.Name + "] RUNNING IN " + ((AutoTestController.s_Instance.CurrentState == EAutoTestControllerState.EditingTargetState) ? "EDIT" : "PLAYBACK") + " MODE";
		PlaybackRecordedActionsButton.interactable = AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions != null;
		if (AutoTestController.s_ChoreographerPaused)
		{
			OnChoreographerPaused();
		}
		else
		{
			OnChoreographerResumed();
		}
		AutoTestController.s_Instance.OnChoreographerPaused.AddListener(OnChoreographerPaused);
		AutoTestController.s_Instance.OnChoreographerResumed.AddListener(OnChoreographerResumed);
		AutoTestController.s_Instance.OnProcessedMessageCountChanged.AddListener(ChoreographerMessageCountChanged);
		AutoTestController.s_Instance.OnUIActionRecorded.AddListener(OnUIActionRecorded);
		AutoTestController.s_Instance.OnAutoTestReadyStateChanged.AddListener(UpdateUIForTestReadyState);
		Singleton<ESCMenu>.Instance.OnShown.AddListener(OnEscMenuShown);
		Singleton<ESCMenu>.Instance.OnHidden.AddListener(OnEscMenuHidden);
	}

	private void OnDestroy()
	{
		if (AutoTestController.s_Instance != null)
		{
			AutoTestController.s_Instance.OnChoreographerPaused.RemoveListener(OnChoreographerPaused);
			AutoTestController.s_Instance.OnChoreographerResumed.RemoveListener(OnChoreographerResumed);
			AutoTestController.s_Instance.OnProcessedMessageCountChanged.RemoveListener(ChoreographerMessageCountChanged);
			AutoTestController.s_Instance.OnUIActionRecorded.RemoveListener(OnUIActionRecorded);
			AutoTestController.s_Instance.OnAutoTestReadyStateChanged.RemoveListener(UpdateUIForTestReadyState);
		}
		if (Singleton<ESCMenu>.Instance != null)
		{
			Singleton<ESCMenu>.Instance.OnShown.RemoveListener(OnEscMenuShown);
			Singleton<ESCMenu>.Instance.OnHidden.RemoveListener(OnEscMenuHidden);
		}
		if (DebugMenu.Instance != null)
		{
			DebugMenu.Instance.OnDebugMenuShownEvent.RemoveListener(OnEscMenuShown);
			DebugMenu.Instance.OnDebugMenuHiddenEvent.RemoveListener(OnEscMenuHidden);
		}
	}

	private async Task SaveDataToAutotest()
	{
		try
		{
			AutoTestData currentEditorAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
			ScenarioManager.CurrentScenarioState.Update();
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.Clear();
			AutoTestData autoTestData = currentEditorAutoTestData;
			autoTestData.ExpectedResultingScenarioState = await ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>();
			AutoTestController.s_Instance.AutoTestExpectedStateSaved();
			LevelEditorController.s_Instance.AutoTestNeedsSaving = true;
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save Expected state to Autotest: " + ex.Message);
		}
	}

	private async Task QuitBackToLevelEditor()
	{
		CCustomLevelData currentEditorLevelData = SaveData.Instance.Global.CurrentEditorLevelData;
		currentEditorLevelData.ScenarioState = await SaveData.Instance.Global.CurrentAutoTestDataCopy.ScenarioState.DeepCopySerializableObject<ScenarioState>();
		AutoTestData currentEditorAutoTestData = SaveData.Instance.Global.CurrentEditorAutoTestData;
		currentEditorAutoTestData.ScenarioState = await SaveData.Instance.Global.CurrentAutoTestDataCopy.ScenarioState.DeepCopySerializableObject<ScenarioState>();
		ScenarioRuleClient.Stop();
		SaveData.Instance.Global.CloseCustomLevel();
		AutoTestController.s_Instance.QuitFromAutotest(andThenLoadIntoEditor: true);
	}

	public async void OnQuitToLevelEditorPressed()
	{
		await QuitBackToLevelEditor();
	}

	public void OnStartRecordingPressed()
	{
		AutoTestController.s_Instance.ShouldRecordUIActions = true;
		foreach (Transform child in RightPanel)
		{
			child.gameObject.SetActive(UIForRecording.Any((GameObject go) => go == child.gameObject));
		}
		if (AutoTestController.s_ChoreographerPaused)
		{
			OnChoreographerPaused();
		}
		else
		{
			OnChoreographerResumed();
		}
		DebugMenu.Instance.OnDebugMenuShownEvent.AddListener(OnEscMenuShown);
		DebugMenu.Instance.OnDebugMenuHiddenEvent.AddListener(OnEscMenuHidden);
	}

	public async void OnStopRecordingSaveAndExitPressed()
	{
		if (Choreographer.s_Choreographer.m_WaitState.m_State >= Choreographer.ChoreographerStateType.Play)
		{
			AutoTestController.s_Instance.ShouldRecordUIActions = false;
			await SaveDataToAutotest();
			QuitBackToLevelEditor();
		}
		else
		{
			EvaluationLogText.text = "<color=red>NEED TO BE WAITING FOR USER INTERACTION BEFORE SAVING";
		}
	}

	public void OnPlaybackAutoLogPressed()
	{
		AutoTestController.s_Instance.PlaybackRecordedUIActions();
		ManuallyExecuteButton.gameObject.SetActive(value: false);
	}

	public void ForceEvaluationPressed()
	{
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.AutoTestPlaybackCompleted();
		}
	}

	public async void OnUpdateState()
	{
		if (SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile != null)
		{
			ScenarioManager.CurrentScenarioState.Update();
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.Clear();
			AutoTestData data = SaveData.Instance.Global.LastLoadedAutoTestData;
			AutoTestData autoTestData = data;
			autoTestData.ExpectedResultingScenarioState = await ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>();
			await SaveData.Instance.AutoTestDataManager.SaveAutoTestData(data, SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile);
			SteppingStatusText.text = "State Updated";
		}
	}

	public void OnExitToMainMenu()
	{
		AutoTestController.s_Instance.CurrentAutoLogPlayback = null;
		Singleton<ESCMenu>.Instance.LoadMainMenu();
	}

	public void OnChoreographerPaused()
	{
		if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			UIInteractionBlocker.SetActive(value: true);
		}
	}

	public void OnChoreographerResumed()
	{
		SteppingStatusText.text = "<color=green>WAITING FOR SRL/USER";
		UIInteractionBlocker.SetActive(value: false);
	}

	public void ChoreographerMessageCountChanged()
	{
		StepCountText.text = "[" + AutoTestController.s_Instance.ChoreographerMessagesProcessed + "]<size=10>CHOREOGRAPHER STEPS PROCESSED";
	}

	public void OnUIActionRecorded(string recordLog)
	{
		RecordedActionsText.text = recordLog;
	}

	public void UpdateUIForTestReadyState()
	{
		StartRecordingButton.interactable = AutoTestController.s_Instance.AutoTestCanBegin;
		StopAndSaveButton.interactable = AutoTestController.s_Instance.AutoTestCanBegin;
		PlaybackRecordedActionsButton.interactable = AutoTestController.s_Instance.AutoTestCanBegin;
	}

	public void OnEscMenuShown()
	{
		if (AutoTestController.s_ChoreographerPaused)
		{
			UIInteractionBlocker.SetActive(value: false);
		}
	}

	public void OnEscMenuHidden()
	{
		if (AutoTestController.s_ShouldRecordUIActionsForAutoTest && AutoTestController.s_ChoreographerPaused)
		{
			UIInteractionBlocker.SetActive(value: true);
		}
	}
}

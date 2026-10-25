using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AStar;
using Libs.GameConsole;
using ReportIssueGui;
using SETUtil;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AutoTestController : MonoBehaviour
{
	public const string ENV_IGNORE_ERROR = "ENV_IGNORE_ERROR";

	public static AutoTestController s_Instance;

	public GameObject AutoTestOverlayPrefab;

	public static List<string> s_LoggedErrors = new List<string>();

	public static List<string> s_LoggedWarnings = new List<string>();

	public static bool s_BulkAutoTestEvaluationInProgress;

	private EAutoTestControllerState m_CurrentState;

	private bool m_EndBulkRun;

	[NonSerialized]
	public AutoTestOverlay CurrentOverlay;

	[NonSerialized]
	public AutoTestData CurrentAutoTestData;

	[NonSerialized]
	public AutoLogPlayback CurrentAutoLogPlayback;

	[NonSerialized]
	public bool ChoreographerNeedsStepping;

	[NonSerialized]
	public bool ShouldRecordUIActions;

	private int m_ChoreographerMessagesProcessed;

	private int m_NumTestsProcessed;

	private bool m_AutoTestCanBegin;

	[NonSerialized]
	public UnityEvent OnChoreographerPaused = new UnityEvent();

	[NonSerialized]
	public UnityEvent OnChoreographerResumed = new UnityEvent();

	[NonSerialized]
	public UnityEvent OnProcessedMessageCountChanged = new UnityEvent();

	[NonSerialized]
	public UnityEventWithString OnUIActionRecorded = new UnityEventWithString();

	[NonSerialized]
	public UnityEvent OnAutoTestReadyStateChanged = new UnityEvent();

	private bool m_ChoreographerPaused;

	public const float cDefaultAutoTestTimeout = 300f;

	private float m_PlaybackTimeElapsed;

	private IEnumerator m_AutoTestTimeOutRoutine;

	private bool m_EvaluationResultsReady;

	public bool m_AutotestResult;

	private string m_CurrentEvaluationResults;

	private IEnumerator m_BulkAutotestEvaluationRoutine;

	public static bool GameBootedForAutoTests { get; set; }

	public bool TestErrorOccurred { get; set; }

	public static bool s_AutoTestCurrentlyLoaded
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.CurrentAutoTestData != null;
			}
			return false;
		}
	}

	public static bool s_ChoreographerPaused
	{
		get
		{
			if (s_AutoTestCurrentlyLoaded)
			{
				return s_Instance.m_ChoreographerPaused;
			}
			return false;
		}
	}

	public static bool s_AutoLogPlaybackInProgress
	{
		get
		{
			if (s_AutoTestCurrentlyLoaded)
			{
				return s_Instance.CurrentAutoLogPlayback != null;
			}
			return false;
		}
	}

	public static bool s_ShouldRecordUIActionsForAutoTest
	{
		get
		{
			if (s_AutoTestCurrentlyLoaded)
			{
				return s_Instance.ShouldRecordUIActions;
			}
			return false;
		}
	}

	public EAutoTestControllerState CurrentState => m_CurrentState;

	public bool AutotestStarted { get; set; }

	public int ChoreographerMessagesProcessed
	{
		get
		{
			return m_ChoreographerMessagesProcessed;
		}
		set
		{
			if (value != m_ChoreographerMessagesProcessed)
			{
				m_ChoreographerMessagesProcessed = value;
				OnProcessedMessageCountChanged.Invoke();
			}
		}
	}

	public bool AutoTestCanBegin
	{
		get
		{
			return m_AutoTestCanBegin;
		}
		set
		{
			m_AutoTestCanBegin = value;
			OnAutoTestReadyStateChanged.Invoke();
		}
	}

	public bool IsPausedForSpecificMessageType { get; private set; }

	public CMessageData.MessageType MessageTypePausedFor { get; private set; }

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
	}

	private void Update()
	{
		if (GloomhavenShared.CurrentAutoTestState == GloomhavenShared.AutoTestState.WaitingForStart)
		{
			StartAutoTests(null);
		}
	}

	[ConsoleCommand(Command = "AutoTest", Description = "Starts the autotest process")]
	public static void RunAllAutoTests(IConsole console)
	{
		if (GloomhavenShared.CurrentAutoTestState == GloomhavenShared.AutoTestState.NotStarted)
		{
			GloomhavenShared.CurrentAutoTestState = GloomhavenShared.AutoTestState.WaitingForStart;
			return;
		}
		console.WriteLine("AutoTest already running");
		Debug.Log("AutoTest already running");
	}

	public void StartAutoTests(UnityAction<List<GloomhavenShared.AutotestResult>> onCompletionReport)
	{
		GloomhavenShared.CurrentAutoTestState = GloomhavenShared.AutoTestState.Running;
		StartCoroutine(AutoTestsWithLoadIntoLevelEditor(onCompletionReport ?? new UnityAction<List<GloomhavenShared.AutotestResult>>(DefaultHandleAutoTestResults)));
	}

	private void DefaultHandleAutoTestResults(List<GloomhavenShared.AutotestResult> arg0)
	{
		foreach (GloomhavenShared.AutotestResult item in arg0)
		{
			Debug.Log((item.Result ? "[Test Pass]" : "[Test Fail]") + " " + item.TestName + ". \n" + item.TestLog);
		}
	}

	private IEnumerator AutoTestsWithLoadIntoLevelEditor(UnityAction<List<GloomhavenShared.AutotestResult>> onCompletionReport)
	{
		float safetyTimeout = 120f;
		float safetyTimeoutStart = Time.realtimeSinceStartup;
		while (!SaveData.Instance || SaveData.Instance.AutoTestDataManager == null)
		{
			if (Time.realtimeSinceStartup - safetyTimeoutStart > safetyTimeout)
			{
				throw new Exception("AutoTestsWithLoadIntoLevelEditor timed out waiting for SaveData.Instance.AutoTestDataManager");
			}
			yield return null;
		}
		EvaluateAllAvailableTestsAndReportString((List<GloomhavenShared.AutotestResult> report) =>
		{
			onCompletionReport?.Invoke(report);
			GloomhavenShared.CurrentAutoTestState = GloomhavenShared.AutoTestState.Finished;
		});
	}

	public static void OnAutoTestCompleteAndQuit(List<GloomhavenShared.AutotestResult> completionReport)
	{
		ReportIssueWindow.GetLogFileLocation();
		Action<string> action = null;
		Action action2 = null;
		StringBuilder stringBuilder = new StringBuilder();
		action = (string s) =>
		{
			stringBuilder.AppendLine(s);
		};
		action2 = () =>
		{
			FileUtil.WriteTextToFile(SaveData.Instance.AutoTestResultsLog, stringBuilder.ToString());
		};
		action("--- Auto Test Results ---");
		foreach (GloomhavenShared.AutotestResult item in completionReport)
		{
			string text = (item.Result ? "[Test Pass]" : "[Test Fail]");
			string testName = item.TestName;
			string testLog = item.TestLog;
			action("[AutoTests] " + text + " " + testName);
			action(testLog);
			action("\n---\n");
		}
		action2();
		Application.Quit(completionReport.Any((GloomhavenShared.AutotestResult a) => !a.Result) ? 42 : 0);
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
	}

	private void SetState(EAutoTestControllerState newState)
	{
		if (newState != m_CurrentState)
		{
			m_CurrentState = newState;
		}
	}

	public static void SetChoreographerNeedsStepping(bool shouldNeedStepping)
	{
		if (s_Instance != null)
		{
			s_Instance.ChoreographerNeedsStepping = shouldNeedStepping;
			if (shouldNeedStepping)
			{
				SetChoreographerPauseFlag(shouldPause: true);
			}
		}
	}

	public static void SetChoreographerPauseFlag(bool shouldPause, int messageTypeInt = int.MaxValue)
	{
		if (!(s_Instance != null) || s_Instance.m_ChoreographerPaused == shouldPause)
		{
			return;
		}
		s_Instance.m_ChoreographerPaused = shouldPause;
		if (shouldPause)
		{
			if (messageTypeInt != int.MaxValue)
			{
				s_Instance.IsPausedForSpecificMessageType = true;
				s_Instance.MessageTypePausedFor = (CMessageData.MessageType)messageTypeInt;
			}
			else
			{
				s_Instance.IsPausedForSpecificMessageType = false;
			}
			s_Instance.OnChoreographerPaused.Invoke();
		}
		else
		{
			s_Instance.OnChoreographerResumed.Invoke();
		}
	}

	public static void ChoreographerMessageProcessed(CMessageData.MessageType messageTypeProcessed)
	{
		if (messageTypeProcessed == CMessageData.MessageType.FinishedProcessingTileSelected)
		{
			Log.Info("FinishedProcessingTileSelected");
		}
		if (s_ShouldRecordUIActionsForAutoTest && messageTypeProcessed != CMessageData.MessageType.SRLQueueDebugLog)
		{
			s_Instance.LogChoreographerStep(messageTypeProcessed);
			s_Instance.ChoreographerMessagesProcessed++;
		}
		if (s_AutoLogPlaybackInProgress)
		{
			if (!s_Instance.CurrentAutoLogPlayback.CurrentLogPlayedBack.ContainsTypedChoreographerSteps || messageTypeProcessed != CMessageData.MessageType.SRLQueueDebugLog)
			{
				SetChoreographerPauseFlag(shouldPause: true, (int)messageTypeProcessed);
				s_Instance.ChoreographerMessagesProcessed++;
			}
		}
		else if (s_Instance.ChoreographerNeedsStepping)
		{
			SetChoreographerPauseFlag(shouldPause: true);
		}
	}

	public static List<Tuple<int, string>> AutoTestPlaybackCompleted()
	{
		List<Tuple<int, string>> result = s_Instance.PerformAutoTestEvaluation();
		SetChoreographerPauseFlag(shouldPause: true);
		return result;
	}

	public static bool CheckResults(List<Tuple<int, string>> results)
	{
		List<int> ignoreResults = new List<int>
		{
			1007, 1029, 2914, 1028, 2104, 109, 2911, 3100, 1201, 1203,
			1206, 120, 1028, 1617
		};
		try
		{
			string environmentVariable = Environment.GetEnvironmentVariable("ENV_IGNORE_ERROR");
			if (environmentVariable != null && environmentVariable.Length > 0)
			{
				string[] array = environmentVariable.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (int.TryParse(array[i].Trim(), out var result) && !ignoreResults.Contains(result))
					{
						ignoreResults.Add(result);
					}
				}
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception while parsing ENV string for autotests\n");
		}
		return !results.Any((Tuple<int, string> a) => !ignoreResults.Contains(a.Item1));
	}

	public void LoadIntoAutoTest(AutoTestData dataToPlay, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false)
	{
		if (withoutAutotestFunctionality)
		{
			CurrentAutoTestData = dataToPlay;
			Log.Info("Load scenario \"" + CurrentAutoTestData.Name + "\"");
			UnityGameEditorRuntime.LoadScenario((autoTestState == EAutoTestControllerState.EditingTargetState) ? dataToPlay.ExpectedResultingScenarioState : dataToPlay.ScenarioState);
			return;
		}
		AutoTestCanBegin = false;
		CurrentAutoTestData = dataToPlay;
		SetChoreographerNeedsStepping(shouldNeedStepping: false);
		SetState(autoTestState);
		ChoreographerMessagesProcessed = ((autoTestState == EAutoTestControllerState.EditingTargetState) ? dataToPlay.ChoreographerStepCountUntilResult : 0);
		CurrentOverlay = UnityEngine.Object.Instantiate(AutoTestOverlayPrefab, transform.root.parent).GetComponent<AutoTestOverlay>();
		Log.Info("Load scenario \"" + CurrentAutoTestData.Name + "\"");
		UnityGameEditorRuntime.LoadScenario((autoTestState == EAutoTestControllerState.EditingTargetState) ? dataToPlay.ExpectedResultingScenarioState : dataToPlay.ScenarioState);
		StartCoroutine(WaitUntilReadyBeforeAllowingAutotest());
	}

	public void AutoTestExpectedStateSaved()
	{
		CurrentAutoTestData.ChoreographerStepCountUntilResult = ChoreographerMessagesProcessed;
		CurrentAutoTestData.ExpectedStateTimeStamp = DateTime.Now;
	}

	public void PlaybackRecordedUIActions()
	{
		SetChoreographerNeedsStepping(shouldNeedStepping: true);
		SetChoreographerPauseFlag(shouldPause: false);
		CurrentAutoLogPlayback = new AutoLogPlayback();
		Log.Info("Starting Autotest: " + CurrentAutoTestData.Name);
		CurrentAutoLogPlayback.PlaybackAutoLog(CurrentAutoTestData.RecordedUIActions, CurrentAutoTestData.UseRealtime);
		if (m_AutoTestTimeOutRoutine != null)
		{
			StopCoroutine(m_AutoTestTimeOutRoutine);
			m_AutoTestTimeOutRoutine = null;
		}
		m_AutoTestTimeOutRoutine = CheckForTimeout();
		StartCoroutine(m_AutoTestTimeOutRoutine);
	}

	public void QuitFromAutotest(bool andThenLoadIntoEditor = false)
	{
		Choreographer.s_Choreographer.ClearMessageQueue();
		SaveData.Instance.Global.CloseCustomLevel();
		if (andThenLoadIntoEditor)
		{
			SaveData.Instance.Global.CurrentEditorLevelData = SaveData.Instance.Global.CurrentAutoTestDataCopy;
			SceneController.Instance.LevelEditorStart();
		}
		else
		{
			SaveData.Instance.Global.CurrentAutoTestDataCopy = null;
		}
		SetState(EAutoTestControllerState.None);
		AutotestStarted = false;
		CurrentAutoTestData = null;
		CurrentAutoLogPlayback = null;
		m_ChoreographerPaused = false;
		ShouldRecordUIActions = false;
		ChoreographerNeedsStepping = false;
		m_ChoreographerMessagesProcessed = 0;
		AutoTestCanBegin = false;
		m_CurrentEvaluationResults = string.Empty;
		m_EvaluationResultsReady = false;
		if (!GloomUtility.IsGUIVisible())
		{
			GloomUtility.ToggleGUI();
		}
		if (CurrentOverlay != null)
		{
			UnityEngine.Object.Destroy(CurrentOverlay.gameObject);
		}
	}

	public void OnProcGenLoadCompleteCallback(Scene gameScene)
	{
		if (CurrentOverlay != null)
		{
			SceneManager.MoveGameObjectToScene(CurrentOverlay.gameObject, gameScene);
		}
	}

	private List<Tuple<int, string>> PerformAutoTestEvaluation(bool fromTimeout = false, bool fromError = false, string errorMessage = null)
	{
		if (fromTimeout)
		{
			Log.Error("Autotest timed out!");
		}
		if (fromError)
		{
			Log.Error("Autotest failed due to an error during the test");
		}
		string empty = string.Empty;
		List<Tuple<int, string>> list = null;
		bool flag;
		try
		{
			ScenarioManager.CurrentScenarioState.Update();
			ScenarioState expectedResultingScenarioState = CurrentAutoTestData.ExpectedResultingScenarioState;
			ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
			empty = empty + "\nAutoTest [" + CurrentAutoTestData.Name + "] took (" + m_PlaybackTimeElapsed + "s), ended at " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + ". Evaluation:";
			list = ScenarioState.CompareStates(currentScenarioState, expectedResultingScenarioState, isMPCompare: false);
			if (CheckResults(list))
			{
				flag = true;
				empty += "\n\t [PASS] State passed compare check";
			}
			else
			{
				flag = false;
				empty += "\n\t [FAIL] The following failures occurred during the state compare check";
				foreach (Tuple<int, string> item in list)
				{
					empty = empty + "\n\t\t[" + item.Item1 + "] " + item.Item2;
				}
			}
		}
		catch (Exception ex)
		{
			empty = "\n\n AutoTest [" + CurrentAutoTestData.Name + "] took (" + m_PlaybackTimeElapsed + "s). Evaluation failed after (" + m_PlaybackTimeElapsed + "s) with Evaluation Exception:" + ex.Message + "\n" + ex.StackTrace;
			flag = false;
		}
		if (fromError)
		{
			flag = false;
			empty = empty + "\n\t-\n\tTEST ENDED BECAUSE OF ERROR(" + errorMessage + "s)\n\t-";
		}
		if (fromTimeout)
		{
			flag = false;
			empty += "\n\t-\n\tTEST ENDED BECAUSE TIMEOUT REACHED\n\t-";
		}
		empty = empty + "\nRESULT:" + (flag ? "PASS" : "FAIL");
		if (!flag && !string.IsNullOrEmpty(CurrentAutoLogPlayback.AutotestPlaybackReport))
		{
			empty = empty + "\n\tAUTOTEST PLAYBACK REPORT:\n" + CurrentAutoLogPlayback.AutotestPlaybackReport;
		}
		if (CurrentOverlay != null)
		{
			CurrentOverlay.EvaluationLogText.text = empty;
		}
		m_AutotestResult = flag;
		m_CurrentEvaluationResults = empty;
		m_EvaluationResultsReady = true;
		CurrentAutoLogPlayback.TestEnded = true;
		Log.Info(empty);
		return list;
	}

	public void EndAutotestsWithoutEval()
	{
		CurrentAutoLogPlayback.StopPlayback();
		CurrentAutoLogPlayback = null;
		if (s_BulkAutoTestEvaluationInProgress)
		{
			m_EndBulkRun = true;
		}
	}

	public void EndTestPlaybackFromErrorMessage(string errorMessage)
	{
		PerformAutoTestEvaluation(fromTimeout: false, fromError: true, errorMessage);
	}

	public void EndTestPlaybackFromТimeout(string errorMessage)
	{
		PerformAutoTestEvaluation(fromTimeout: true, fromError: false, errorMessage);
	}

	private IEnumerator WaitUntilReadyBeforeAllowingAutotest()
	{
		while ((Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForCardSelection && (TransitionManager.s_Instance == null || !TransitionManager.s_Instance.TransitionDone)) || SceneController.Instance.ScenarioIsLoading || SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		s_Instance.ChoreographerMessagesProcessed = 0;
		yield return new WaitForSeconds(2f);
		AutoTestCanBegin = true;
	}

	private IEnumerator CheckForTimeout()
	{
		float timeAtStart = Time.realtimeSinceStartup;
		float timeOutToUse = CurrentAutoTestData.CustomTimeout;
		m_PlaybackTimeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (s_AutoLogPlaybackInProgress && !s_Instance.CurrentAutoLogPlayback.FinalRecordedChoreographerEventOccured)
		{
			m_PlaybackTimeElapsed = Time.realtimeSinceStartup - timeAtStart;
			if (m_PlaybackTimeElapsed >= timeOutToUse)
			{
				Log.Info("AUTOTEST TIMED OUT");
				PerformAutoTestEvaluation(fromTimeout: true);
				break;
			}
			yield return null;
		}
	}

	public static void LogButtonClick(GameObject buttonGO, object sender, [CallerMemberName] string additionalInfo = null)
	{
		ReproStepsLogger.LogButtonClick(buttonGO, sender, additionalInfo);
		if (s_ShouldRecordUIActionsForAutoTest)
		{
			s_Instance.OnUIActionRecorded?.Invoke("Recorded UI Press - " + buttonGO.name);
			s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Add(new CAutoButtonClick(s_Instance.CurrentAutoTestData.RecordedUIActions.NextEventID, AutoLogUtility.GetGameObjectHierarchy(buttonGO)));
		}
	}

	public static void LogTileClick(CClientTile selectedTile, List<CTile> optionalTileList, List<Point> waypoints, CClientTile placementTile, CInteractable.EInputType inputType)
	{
		ReproStepsLogger.LogTileClick(selectedTile, optionalTileList, waypoints, placementTile, inputType);
		if (s_ShouldRecordUIActionsForAutoTest)
		{
			s_Instance.OnUIActionRecorded?.Invoke("Recorded Tile Press - X:" + selectedTile.m_Tile.m_ArrayIndex.X + " Y:" + selectedTile.m_Tile.m_ArrayIndex.Y);
			s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Add(new CAutoTileClick(s_Instance.CurrentAutoTestData.RecordedUIActions.NextEventID, new TileIndex(selectedTile.m_Tile.m_ArrayIndex.X, selectedTile.m_Tile.m_ArrayIndex.Y), CAutoTileClick.CTileToTileIndexList(optionalTileList), CAutoTileClick.AStarToTileIndexList(waypoints), CAutoTileClick.ClientTileToTileIndex(placementTile), inputType));
		}
	}

	public static void LogAOETileHover(CClientTile placementTile)
	{
		if (s_ShouldRecordUIActionsForAutoTest && placementTile != null)
		{
			s_Instance.OnUIActionRecorded?.Invoke("Recorded AOE Tile Hover - X:" + placementTile.m_Tile.m_ArrayIndex.X + " Y:" + placementTile.m_Tile.m_ArrayIndex.Y);
			s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Add(new CAutoAOETileHover(s_Instance.CurrentAutoTestData.RecordedUIActions.NextEventID, CAutoTileClick.ClientTileToTileIndex(placementTile)));
		}
	}

	public static void LogAOERotate(bool clockwise)
	{
		string text = (clockwise ? "CW" : "CCW");
		ReproStepsLogger.LogButtonClick(null, null, "Rotate AOE (" + text + ")");
		if (s_ShouldRecordUIActionsForAutoTest)
		{
			s_Instance.OnUIActionRecorded?.Invoke("Recorded AOE rotate " + (clockwise ? "Clockwise" : "AntiClockwise"));
			s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Add(new CAutoAOERotate(s_Instance.CurrentAutoTestData.RecordedUIActions.NextEventID, clockwise));
		}
	}

	public static void LogButtonHover(GameObject buttonGO, object sender, string additionalInfo = null)
	{
		if (s_ShouldRecordUIActionsForAutoTest)
		{
			s_Instance.OnUIActionRecorded?.Invoke("Recorded UI Pointer Hover - " + buttonGO.name);
			s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Add(new CAutoButtonHover(s_Instance.CurrentAutoTestData.RecordedUIActions.NextEventID, AutoLogUtility.GetGameObjectHierarchy(buttonGO)));
		}
	}

	public void LogChoreographerStep(CMessageData.MessageType messageTypeProcessed)
	{
		OnUIActionRecorded?.Invoke("Recorded ChoreographerStep");
		CurrentAutoTestData.RecordedUIActions.Events.Add(new CAutoStepChoreographer(CurrentAutoTestData.RecordedUIActions.NextEventID, messageTypeProcessed));
	}

	public static void LogNextRewardClicked()
	{
		if (s_ShouldRecordUIActionsForAutoTest)
		{
			s_Instance.OnUIActionRecorded?.Invoke("Recorded Next Reward Click");
			s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Add(new CAuto(CAuto.EAutoType.NextRewardClicked, s_Instance.CurrentAutoTestData.RecordedUIActions.NextEventID));
		}
	}

	public static bool IsBulkEvaluating()
	{
		if (s_Instance == null)
		{
			return false;
		}
		if (SaveData.Instance.AutoTestDataManager.LoadedAutoTests != null && SaveData.Instance.AutoTestDataManager.LoadedAutoTests.Count > 0 && s_Instance.m_NumTestsProcessed < SaveData.Instance.AutoTestDataManager.LoadedAutoTests.Count)
		{
			return !s_Instance.m_EndBulkRun;
		}
		return false;
	}

	private void EvaluateAllAvailableTestsAndReportString(UnityAction<List<GloomhavenShared.AutotestResult>> onCompletionReport)
	{
		if (m_BulkAutotestEvaluationRoutine != null)
		{
			StopCoroutine(m_BulkAutotestEvaluationRoutine);
			m_BulkAutotestEvaluationRoutine = null;
		}
		SceneController.Instance.YML.LoadSingleScenarios(ScenarioManager.EDLLMode.CustomScenario);
		m_BulkAutotestEvaluationRoutine = LoopThroughAutoTestsAndReport(onCompletionReport);
		StartCoroutine(m_BulkAutotestEvaluationRoutine);
	}

	private IEnumerator LoopThroughAutoTestsAndReport(UnityAction<List<GloomhavenShared.AutotestResult>> onCompletionReport)
	{
		yield return PersistentData.s_Instance.InitMonsterCards();
		if (PersistentData.s_Instance.FailedLoading)
		{
			throw new Exception("Unable to load Monster Card YML\n");
		}
		Task task = SaveData.Instance.AutoTestDataManager.LoadAllAutoTestDataFromFile();
		yield return new WaitForTask(task);
		SceneController.Instance.DisableLoadingScreen();
		Func<List<AutoTestData>> tests = () => SaveData.Instance.AutoTestDataManager.LoadedAutoTests;
		string reportString = "Bulk Evaluation of [" + tests().Count + "] AutoTest" + ((tests().Count == 1) ? string.Empty : "s") + " Start " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n==================================================";
		List<GloomhavenShared.AutotestResult> results = new List<GloomhavenShared.AutotestResult>();
		m_NumTestsProcessed = 0;
		s_BulkAutoTestEvaluationInProgress = true;
		Application.logMessageReceived += LogCallback;
		while (m_NumTestsProcessed < tests().Count && !m_EndBulkRun)
		{
			switch (CurrentState)
			{
			case EAutoTestControllerState.None:
				if (SaveData.Instance.Global.GameMode == EGameMode.MainMenu && !SceneController.Instance.IsLoading)
				{
					TestErrorOccurred = false;
					SceneController.Instance.ShowLoadingScreen();
					Thread loadYML = new Thread(() =>
					{
						SceneController.Instance.YML.LoadCampaign();
					});
					loadYML.Name = "GH-AutoTestController => YML.LoadCampaign()";
					loadYML.Start();
					while (loadYML.IsAlive)
					{
						yield return null;
					}
					if (!SceneController.Instance.YML.LastLoadResult)
					{
						Log.Error("Unable to load Guildmaster YML");
						m_EndBulkRun = true;
						continue;
					}
					yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
					if (PersistentData.s_Instance.FailedLoading)
					{
						Log.Error("Unable to load Monster Card YML\n");
						m_EndBulkRun = true;
						continue;
					}
					Task task2 = SaveData.Instance.LoadAutoTestFromData(tests()[m_NumTestsProcessed], SaveData.Instance.AutoTestDataManager.AutoTestFiles[m_NumTestsProcessed]);
					yield return new WaitForTask(task2);
					yield return SceneController.Instance.BulkRunAutoTestStart(SaveData.Instance.Global.CurrentAutoTestDataCopy, EAutoTestControllerState.PlayAsAutotest);
				}
				break;
			case EAutoTestControllerState.PlayAsAutotest:
				if (m_EvaluationResultsReady)
				{
					if (s_LoggedErrors.Count > 0)
					{
						m_CurrentEvaluationResults = m_CurrentEvaluationResults.Remove(m_CurrentEvaluationResults.Length - 5);
						m_CurrentEvaluationResults = m_CurrentEvaluationResults + ": FAIL\nThe following Exceptions occurred during this test:\n" + string.Join("\n\n", s_LoggedErrors.Select((string s) => SecurityElement.Escape(s)));
						m_AutotestResult = false;
						s_LoggedErrors.Clear();
					}
					if (s_LoggedWarnings.Count > 0)
					{
						m_CurrentEvaluationResults = m_CurrentEvaluationResults + "\nThe following Warnings occurred during this test:\n\t" + string.Join("\n\n", s_LoggedWarnings.Select((string s) => SecurityElement.Escape(s)));
						s_LoggedWarnings.Clear();
					}
					results.Add(new GloomhavenShared.AutotestResult(tests()[m_NumTestsProcessed].Name, m_AutotestResult, m_CurrentEvaluationResults));
					reportString += m_CurrentEvaluationResults;
					m_NumTestsProcessed++;
					Choreographer.s_Choreographer.ClearMessageQueue();
					yield return null;
					UIManager.LoadMainMenu();
				}
				else if (!s_AutoLogPlaybackInProgress && AutoTestCanBegin)
				{
					PlaybackRecordedUIActions();
				}
				else
				{
					if (!TestErrorOccurred)
					{
						break;
					}
					m_CurrentEvaluationResults = "Fail: An error occurred while trying to run this test\nThe following Exceptions occurred during this test:\n" + string.Join("\n\n", s_LoggedErrors.Select((string s) => SecurityElement.Escape(s)));
					m_AutotestResult = false;
					s_LoggedErrors.Clear();
					if (s_LoggedWarnings.Count > 0)
					{
						m_CurrentEvaluationResults = m_CurrentEvaluationResults + "\nThe following Warnings occurred during this test:\n" + string.Join("\n\n", s_LoggedWarnings.Select((string s) => SecurityElement.Escape(s)));
						s_LoggedWarnings.Clear();
					}
					results.Add(new GloomhavenShared.AutotestResult(tests()[m_NumTestsProcessed].Name, m_AutotestResult, m_CurrentEvaluationResults));
					reportString += m_CurrentEvaluationResults;
					m_NumTestsProcessed++;
					UIManager.LoadMainMenu();
				}
				break;
			}
			yield return null;
		}
		m_EndBulkRun = false;
		Application.logMessageReceived -= LogCallback;
		try
		{
			List<string> list = new List<string>();
			while (reportString.Length > 16000)
			{
				int num = reportString.IndexOf('\n', 16000);
				if (num > 0 && num + 1 < reportString.Length)
				{
					list.Add(reportString.Substring(0, num));
					reportString = reportString.Substring(num + 1);
					continue;
				}
				list.Add(reportString);
				break;
			}
			if (reportString.Length > 0)
			{
				list.Add(reportString);
			}
			foreach (string item in list)
			{
				Log.Info(item);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception while trying to split report for debug log.\n");
		}
		s_BulkAutoTestEvaluationInProgress = false;
		onCompletionReport?.Invoke(results);
	}

	private void LogCallback(string condition, string stacktrace, LogType type)
	{
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			if (type != LogType.Error || !condition.Contains("AmplitudeHttp"))
			{
				s_LoggedErrors.Add("[Log Error] " + condition + "\n" + stacktrace);
			}
			break;
		case LogType.Warning:
			if (!condition.Contains("SendMessage"))
			{
				s_LoggedWarnings.Add(condition);
			}
			break;
		}
	}
}

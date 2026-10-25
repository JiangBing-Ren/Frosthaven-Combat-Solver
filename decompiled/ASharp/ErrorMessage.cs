using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DevLocker.GFrame.Input.UIScope;
using FFSNet;
using GLOOM;
using Libs.GameConsole;
using ReportIssueGui;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour
{
	public class LabelAction
	{
		public string ButtonLabelKey;

		public ErrorDelegate ButtonAction;

		public InputAction InputAction;

		public LabelAction(string buttonLabelKey, ErrorDelegate buttonAction, InputAction inputAction)
		{
			ButtonLabelKey = buttonLabelKey;
			ButtonAction = buttonAction;
			InputAction = inputAction;
		}
	}

	public delegate void ErrorDelegate();

	public static bool Unattended;

	public UIScope Scope;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI MessageText;

	public TextMeshProUGUI InputBoxPlaceholder;

	public TextMeshProUGUI InputBoxText;

	public TextMeshProUGUI SendingErrorReportText;

	public GameObject ErrorMessageGO;

	public GameObject TitleGO;

	public GameObject MessageGO;

	public GameObject MessageButtonsGO;

	public GameObject CreateErrorReportButtonGO;

	public GameObject ErrorReportDialogGO;

	public GameObject SendErrorReportButtonsGO;

	public GameObject SendingErrorReportTextGO;

	public GameObject ButtonsLayoutGroup;

	public GameObject ButtonPrefab;

	[Header("Progress Bar")]
	[SerializeField]
	private Slider m_Slider;

	public const float c_SliderMaxValue = 100f;

	[ConsoleVariable(Alias = "ignore_all_errors", Persistent = false)]
	public static bool IgnoreAllErrors;

	private ErrorDelegate m_ButtonAction;

	private Dictionary<Button, ErrorDelegate> m_ActiveButtonActions = new Dictionary<Button, ErrorDelegate>();

	private string m_ErrorTitleKey;

	private string m_ErrorMessageKey;

	private string m_ErrorStack;

	private string m_ExceptionMessage;

	private bool m_GameObjectState_Threaded;

	private bool m_SuppressShowingErrorMessages;

	private bool m_ErrorReportSent;

	private bool m_DebugErrorReport;

	private UnityAction<bool> m_OnClosed;

	internal static bool __DisableDebugMessageBoxesHotkeyPressed;

	public bool ShowingMessage => m_GameObjectState_Threaded;

	public string Currentkey => m_ErrorTitleKey;

	public bool CheckShowingMessageFromThread => m_GameObjectState_Threaded;

	public bool SuppressShowingErrorMessages
	{
		get
		{
			if (!m_SuppressShowingErrorMessages)
			{
				return IgnoreAllErrors;
			}
			return true;
		}
		set
		{
			m_SuppressShowingErrorMessages = value;
		}
	}

	private void OnEnable()
	{
		InputBoxPlaceholder.text = LocalizationManager.GetTranslation("GUI_ERROR_REPORT_PLACEHOLDER");
		SendingErrorReportText.text = LocalizationManager.GetTranslation("GUI_SENDING_ERROR_REPORT");
	}

	private void UpdateProgressBar(float amount)
	{
		amount = Math.Min(amount, 100f);
		m_Slider.value = amount;
	}

	private void ToggleErrorReportDialog(bool active)
	{
		bool wasActive = gameObject.activeInHierarchy;
		if (!active)
		{
			m_OnClosed?.Invoke(m_ErrorReportSent);
		}
		if (!active)
		{
			return;
		}
		Hide();
		UnityEngine.Object.FindObjectOfType<ReportIssueInput>().ShowAndWait((bool _) =>
		{
			if (wasActive)
			{
				gameObject.SetActive(value: true);
			}
		});
	}

	public void ShowMessageDefaultTitle(string errorMessageKey, string buttonLabelKey, string errorStack, ErrorDelegate action, string exceptionMessage = null, bool showErrorReportButton = true, bool trackDebug = true, bool allowMessageBox = true)
	{
		ShowMessage(errorMessageKey, buttonLabelKey, errorStack, "GUI_ERROR_TITLE", action, exceptionMessage, showErrorReportButton, trackDebug, allowMessageBox);
	}

	public void ShowMessage(string errorMessageKey, string buttonLabelKey, string errorStack, string errorTitleKey, ErrorDelegate action, string exceptionMessage = null, bool showErrorReportButton = true, bool trackDebug = true, bool allowMessageBox = true)
	{
		FastReload.StopAutoUndoRedo();
		if (ShowingMessage || SuppressShowingErrorMessages || Unattended)
		{
			return;
		}
		string translation = LocalizationManager.GetTranslation(errorTitleKey);
		string translation2 = LocalizationManager.GetTranslation(errorMessageKey);
		if (allowMessageBox && !ShowDebugMessageBox(translation2, translation, errorStack, exceptionMessage))
		{
			return;
		}
		m_OnClosed = null;
		m_ErrorTitleKey = errorTitleKey;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = exceptionMessage;
		m_ErrorStack = errorStack;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: false);
		CreateErrorReportButtonGO.SetActive(showErrorReportButton);
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance != null && s_Instance.BulkLevelProcessingInProgress)
		{
			Log.GUIError(translation2);
			LevelEditorController.s_Instance.BulkProcessingErrorOccurred = true;
			return;
		}
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.s_Instance.EndTestPlaybackFromErrorMessage(translation2);
		}
		else
		{
			if (AutoTestController.GameBootedForAutoTests)
			{
				Log.GUIError(translation2);
				AutoTestController.s_Instance.TestErrorOccurred = true;
				return;
			}
			if (!ShowingMessage)
			{
				TitleText.text = translation;
				MessageText.text = translation2;
				GameObject gameObject = UnityEngine.Object.Instantiate(ButtonPrefab, ButtonsLayoutGroup.transform);
				gameObject.SetActive(value: true);
				Button newButton = gameObject.GetComponentInChildren<Button>();
				if ((bool)newButton)
				{
					newButton.SetTextKey(buttonLabelKey);
					newButton.onClick.AddListener(() =>
					{
						OnButtonClick(newButton);
					});
					HotkeyClickableScopeElement component = newButton.GetComponent<HotkeyClickableScopeElement>();
					component.SetInputAction(InputActionReference.Create(InputFHManager.Controls.Generic.Confirm));
					component.ApplyInputActionToChildren();
					Scope.TryAppendOwnedScopeElements(component.GetComponentsInChildren<IScopeElement>());
					m_ActiveButtonActions.Add(newButton, action);
				}
				base.gameObject.SetActive(value: true);
				m_GameObjectState_Threaded = true;
			}
		}
		if (ShowingMessage)
		{
			CoroutineHelper.RunCoroutine(GloomUtility.TakeErrorScreenshot(ErrorMessageGO));
		}
	}

	public void ShowMultiChoiceMessageDefaultTitle(string errorMessageKey, string errorStack, List<LabelAction> buttons, string exceptionMessage = null, bool allowMessageBox = true)
	{
		ShowMultiChoiceMessage(errorMessageKey, buttons, "GUI_ERROR_TITLE", errorStack, exceptionMessage, showErrorReportButton: true, trackDebug: true, allowMessageBox);
	}

	public void ShowMultiChoiceMessage(string errorMessageKey, List<LabelAction> buttons, string errorTitleKey, string errorStack, string exceptionMessage = null, bool showErrorReportButton = true, bool trackDebug = true, bool allowMessageBox = true)
	{
		FastReload.StopAutoUndoRedo();
		if (ShowingMessage || SuppressShowingErrorMessages || Unattended)
		{
			return;
		}
		string translation = LocalizationManager.GetTranslation(errorTitleKey);
		string translation2 = LocalizationManager.GetTranslation(errorMessageKey);
		if (allowMessageBox && !ShowDebugMessageBox(translation2, translation, errorStack, exceptionMessage))
		{
			return;
		}
		m_OnClosed = null;
		m_ErrorTitleKey = errorTitleKey;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = exceptionMessage;
		m_ErrorStack = errorStack;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: false);
		CreateErrorReportButtonGO.SetActive(showErrorReportButton);
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance != null && s_Instance.BulkLevelProcessingInProgress)
		{
			Log.GUIError(translation2);
			LevelEditorController.s_Instance.BulkProcessingErrorOccurred = true;
			return;
		}
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.s_Instance.EndTestPlaybackFromErrorMessage(translation2);
		}
		else
		{
			if (AutoTestController.GameBootedForAutoTests)
			{
				Log.GUIError(translation2);
				AutoTestController.s_Instance.TestErrorOccurred = true;
				return;
			}
			if (!ShowingMessage)
			{
				TitleText.text = translation;
				MessageText.text = translation2;
				foreach (LabelAction button in buttons)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(ButtonPrefab, ButtonsLayoutGroup.transform);
					gameObject.SetActive(value: true);
					Button newButton = gameObject.GetComponentInChildren<Button>();
					if ((bool)newButton)
					{
						newButton.SetTextKey(button.ButtonLabelKey);
						newButton.onClick.AddListener(() =>
						{
							OnButtonClick(newButton);
						});
						if (button.InputAction != null)
						{
							HotkeyClickableScopeElement component = newButton.GetComponent<HotkeyClickableScopeElement>();
							component.SetInputAction(InputActionReference.Create(button.InputAction));
							component.ApplyInputActionToChildren();
							Scope.TryAppendOwnedScopeElements(component.GetComponentsInChildren<IScopeElement>());
						}
						m_ActiveButtonActions.Add(newButton, button.ButtonAction);
					}
				}
				base.gameObject.SetActive(value: true);
				m_GameObjectState_Threaded = true;
			}
		}
		if (ShowingMessage)
		{
			CoroutineHelper.RunCoroutine(GloomUtility.TakeErrorScreenshot(ErrorMessageGO));
		}
	}

	public void ShowGenericMessage(string titleKey, string errorMessageKey, string errorStack, List<LabelAction> buttons, string exceptionMessage = null, bool allowMessageBox = false)
	{
		FastReload.StopAutoUndoRedo();
		if (ShowingMessage || SuppressShowingErrorMessages || Unattended)
		{
			return;
		}
		string translation = LocalizationManager.GetTranslation(titleKey);
		string translation2 = LocalizationManager.GetTranslation(errorMessageKey);
		if (allowMessageBox && !ShowDebugMessageBox(translation2, translation, errorStack, exceptionMessage))
		{
			return;
		}
		m_OnClosed = null;
		m_ErrorTitleKey = titleKey;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = exceptionMessage;
		m_ErrorStack = errorStack;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: false);
		CreateErrorReportButtonGO.SetActive(value: false);
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance != null && s_Instance.BulkLevelProcessingInProgress)
		{
			Log.GUIError(translation2);
			LevelEditorController.s_Instance.BulkProcessingErrorOccurred = true;
		}
		else if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			base.gameObject.SetActive(value: false);
			m_GameObjectState_Threaded = false;
		}
		else if (AutoTestController.GameBootedForAutoTests)
		{
			Log.GUIError(translation2);
			AutoTestController.s_Instance.TestErrorOccurred = true;
		}
		else
		{
			if (ShowingMessage)
			{
				return;
			}
			TitleText.text = translation;
			MessageText.text = translation2;
			foreach (LabelAction button in buttons)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(ButtonPrefab, ButtonsLayoutGroup.transform);
				gameObject.SetActive(value: true);
				Button newButton = gameObject.GetComponentInChildren<Button>();
				newButton.SetTextKey(button.ButtonLabelKey);
				newButton.onClick.AddListener(() =>
				{
					OnButtonClick(newButton);
				});
				if (button.InputAction != null)
				{
					HotkeyClickableScopeElement component = newButton.GetComponent<HotkeyClickableScopeElement>();
					component.SetInputAction(InputActionReference.Create(button.InputAction));
					component.ApplyInputActionToChildren();
					Scope.TryAppendOwnedScopeElements(component.GetComponentsInChildren<IScopeElement>());
				}
				m_ActiveButtonActions.Add(newButton, button.ButtonAction);
			}
			base.gameObject.SetActive(value: true);
			m_GameObjectState_Threaded = true;
		}
	}

	public void ShowGenericDebugMessage(string title, string messageText, List<LabelAction> buttons, bool allowMessageBox = true)
	{
		FastReload.StopAutoUndoRedo();
		if (ShowingMessage || SuppressShowingErrorMessages || Unattended || (allowMessageBox && !ShowDebugMessageBox(messageText, title)))
		{
			return;
		}
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: false);
		CreateErrorReportButtonGO.SetActive(value: false);
		if (ShowingMessage)
		{
			return;
		}
		TitleText.text = title;
		MessageText.text = messageText;
		foreach (LabelAction button in buttons)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ButtonPrefab, ButtonsLayoutGroup.transform);
			gameObject.SetActive(value: true);
			Button newButton = gameObject.GetComponentInChildren<Button>();
			if (!(newButton == null))
			{
				newButton.SetTextKey(button.ButtonLabelKey);
				newButton.onClick.AddListener(() =>
				{
					OnButtonClick(newButton);
				});
				if (button.InputAction != null)
				{
					HotkeyClickableScopeElement component = newButton.GetComponent<HotkeyClickableScopeElement>();
					component.SetInputAction(InputActionReference.Create(button.InputAction));
					component.ApplyInputActionToChildren();
					Scope.TryAppendOwnedScopeElements(component.GetComponentsInChildren<IScopeElement>());
				}
				m_ActiveButtonActions.Add(newButton, button.ButtonAction);
			}
		}
		base.gameObject.SetActive(value: true);
		m_GameObjectState_Threaded = true;
	}

	public void OnButtonClick(Button buttonClicked)
	{
		if (!m_ActiveButtonActions.TryGetValue(buttonClicked, out m_ButtonAction) || m_ButtonAction == null)
		{
			return;
		}
		foreach (Button key in m_ActiveButtonActions.Keys)
		{
			UnityEngine.Object.Destroy(key.gameObject);
		}
		m_ActiveButtonActions.Clear();
		gameObject.SetActive(value: false);
		m_GameObjectState_Threaded = false;
		m_ButtonAction();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		m_GameObjectState_Threaded = false;
	}

	public void CloseErrorMessage()
	{
		foreach (Button key in m_ActiveButtonActions.Keys)
		{
			UnityEngine.Object.Destroy(key.gameObject);
		}
		m_ActiveButtonActions.Clear();
		gameObject.SetActive(value: false);
		m_GameObjectState_Threaded = false;
	}

	public void ShowSendErrorReport(string errorMessageKey, UnityAction<bool> onFinished = null, bool takeScreenshot = true)
	{
		m_OnClosed = onFinished;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = null;
		m_ErrorStack = Environment.StackTrace;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: true);
		m_DebugErrorReport = true;
		if (takeScreenshot)
		{
			CoroutineHelper.RunCoroutine(GloomUtility.TakeErrorScreenshot(ErrorMessageGO));
		}
	}

	public void OnSendErrorReportClick()
	{
		SendErrorReport();
	}

	private async Task GetPlayerLogFromHost()
	{
		string logPath = await RootSaveData.PlayerLogPathFromHost;
		if (File.Exists(logPath))
		{
			File.Delete(logPath);
		}
		GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingPlayerLog;
		Synchronizer.RequestCustomData(DataActionType.SendPlayerLogFromHost);
		await FHTaskExtensions.WaitUntil(() => File.Exists(logPath), 100, 10000);
	}

	[Obsolete]
	public async void SendErrorReport()
	{
		throw new Exception("This is obsolete");
	}

	public static bool ShowDebugMessageBox(string message, string title = null, string callStack = null, string exceptionMessage = null)
	{
		return true;
	}
}

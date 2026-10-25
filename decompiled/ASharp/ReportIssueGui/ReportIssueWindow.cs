using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Base.Utils.Web;
using FFSNet;
using Libs.Build;
using Libs.GameConsole;
using Libs.Time;
using Libs.Utils;
using SETUtil.Functional.Extend;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReportIssueGui;

public class ReportIssueWindow : MonoBehaviour
{
	internal class SaveGameRecordsFilter
	{
		public bool FilterRecords = true;

		public HashSet<string> SelectedRecords = new HashSet<string>();

		public static SaveGameRecordsFilter None = new SaveGameRecordsFilter
		{
			FilterRecords = false
		};

		public IEnumerable<string> FilterThrough(string[] records)
		{
			return null;
		}
	}

	[ConsoleVariable(Alias = "extended_issue_tool", Description = "Sets if the issue tool is shown in its extended version or not")]
	public static bool ForceExtendedVersion;

	public string DefaultInfoMessage = "Click the screenshot to draw on it";

	public int MaxWindowHeight = 610;

	public int MinWindowHeight = 480;

	[Bind]
	private InputField _nameInput;

	[Bind]
	private InputField _titleInput;

	[Bind]
	private InputField _detailsInput;

	[Bind]
	private InputField _assigneeSearchField;

	[Bind]
	private GameObject _assigneeSearchPanel;

	[Bind]
	private RectTransform _screenshotSpace;

	[Bind]
	private RawImage _screenshotPreviewRawImage;

	[Bind]
	private GameObject _reportIssueWindow;

	[Bind]
	private Text _infoText;

	[Bind]
	private Button _sendAllButton;

	[Bind]
	private Button _sendLiteButton;

	[Bind]
	private Button _saveLocalButton;

	[Bind]
	private Button _cancelButton;

	[Bind(Name = "ReportIssueWindow")]
	private CanvasGroup _windowCanvasGroup;

	[Bind]
	private EditScreenshotWindow _editScreenshotWindow;

	[Bind]
	private Dropdown _versionDropdown;

	[Bind]
	private Dropdown _componentDropdown;

	[Bind]
	private Dropdown _reporterDropdown;

	[Bind]
	private Dropdown _assigneeDropdown;

	[Bind]
	private Dropdown _priorityDropdown;

	[Bind]
	private Dropdown _severityDropdown;

	[Bind]
	private Dropdown _categoryDropdown;

	[Bind]
	private ReportIssueWindowSavesField _savesFilterDropdown;

	[Bind]
	private Toggle _openInBrowserToggle;

	[Bind]
	private Toggle _campaignSaves;

	[Bind]
	private GameObject _versionAndComponent;

	[Bind]
	private GameObject _reporterAndAssignee;

	[Bind]
	private GameObject _priorityAndSeverity;

	[Bind]
	private GameObject _savesAndOpenIssue;

	private static ReportIssueWindow _instance;

	private bool _takingScreenshot;

	private Texture2D _screenshotTexture;

	private Texture2D _originalScreenshotTexture;

	private IssueUploader _uploader;

	private bool _reportSentSuccessfuly;

	private Coroutine _hideMessageCoroutine;

	private readonly Queue<Action> _unityThreadActions = new Queue<Action>();

	private byte[] _reportIssueDataBytes = new byte[0];

	private string[] _assigneeNames = new string[0];

	private GameObject _prevSelectedObject;

	private string _desyncTitle;

	private string _desyncDescription;

	private byte[] _desyncStateDump1;

	private byte[] _desyncStateDump2;

	private byte[] _desyncStateDump3;

	private byte[] _desyncStateDump4;

	private byte[] _desyncHostPlayerLog;

	private Dictionary<int, byte[]> _peerLogs = new Dictionary<int, byte[]>();

	private int _peerLogsPending;

	public bool Visible
	{
		get
		{
			return _reportIssueWindow.activeSelf;
		}
		private set
		{
			_reportIssueWindow.SetActive(value);
		}
	}

	public bool ReportSentSuccessfuly => _reportSentSuccessfuly;

	private bool IsBusy => _uploader != null;

	private bool ExtendedVersion
	{
		get
		{
			if (!ForceExtendedVersion)
			{
				return RuntimeBuildInfo.IsRunningFromSnapshotInternalNetwork(async: false);
			}
			return true;
		}
	}

	internal static ReportIssueWindow Create()
	{
		if (_instance == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Interface/ReportIssue/ReportIssueCanvas"));
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_instance = gameObject.GetComponentInChildren<ReportIssueWindow>();
			_instance.Visible = false;
			_instance._editScreenshotWindow.Visible = false;
		}
		return _instance;
	}

	internal static ReportIssueWindow Get()
	{
		if (_instance == null)
		{
			return null;
		}
		return _instance;
	}

	public void Awake()
	{
		Binder.BindFields(this);
		_sendLiteButton.onClick.AddListener(() =>
		{
			OnSendReport(sendLiteReport: true);
		});
		_sendAllButton.onClick.AddListener(() =>
		{
			OnSendReport();
		});
		_saveLocalButton.onClick.AddListener(OnSaveLocalReport);
		_cancelButton.onClick.AddListener(OnCancel);
		_screenshotPreviewRawImage.gameObject.AddComponent<EventTrigger>().AddCallback(EventTriggerType.PointerClick, OnScreenshotClick);
		_nameInput.onValueChanged.AddListener(OnFieldValueChanged);
		_titleInput.onValueChanged.AddListener(OnFieldValueChanged);
		_detailsInput.onValueChanged.AddListener(OnFieldValueChanged);
	}

	private void OnScreenshotClick(BaseEventData ev)
	{
		if (!IsBusy)
		{
			_editScreenshotWindow.Visible = true;
			_editScreenshotWindow.ScreenshotTexture = _screenshotTexture;
			_editScreenshotWindow.OriginalScreenshotTexture = _originalScreenshotTexture;
		}
	}

	private void OnCancel()
	{
		ToggleVisibility();
	}

	public static void Hide()
	{
		if (!(_instance == null) && _instance.Visible)
		{
			_instance.ToggleVisibility();
		}
	}

	private bool IsDropdownDefaultValue(string val)
	{
		if (val.StartsWith("<"))
		{
			return val.EndsWith(">");
		}
		return false;
	}

	private void ProcessDropdownValue(Dropdown dropdown, string settingsKey, out string val)
	{
		string text = "";
		if (dropdown.value > -1 && dropdown.value < dropdown.options.Count)
		{
			text = dropdown.options[dropdown.value].text;
		}
		if (IsDropdownDefaultValue(text))
		{
			text = "";
		}
		StoreSetting(settingsKey, text);
		val = text;
	}

	private void OnSendReport(bool sendLiteReport = false)
	{
		IDictionary<string, object> dictionary = new Dictionary<string, object>();
		string jiraUser;
		string jiraKey;
		string project;
		string issueType;
		if (ExtendedVersion)
		{
			jiraUser = JiraConstants.JiraUserStudio;
			jiraKey = JiraConstants.JiraKeyStudio;
			project = JiraConstants.ProjectIDStudio;
			issueType = JiraConstants.IssueTypeStudio;
			string val = _reporterDropdown.options[_reporterDropdown.value].text.Trim();
			if (IsDropdownDefaultValue(val))
			{
				ShowErrorMessage("Reporter should be set");
				return;
			}
			ProcessDropdownValue(_versionDropdown, "ReportVersion", out var val2);
			if (val2 != "")
			{
				dictionary.Add("fixVersions", new List<object>
				{
					new Dictionary<string, object> { { "name", val2 } }
				});
			}
			ProcessDropdownValue(_componentDropdown, "ReportComponent", out val2);
			if (val2 != "")
			{
				dictionary.Add("components", new List<object>
				{
					new Dictionary<string, object> { { "name", val2 } }
				});
			}
			ProcessDropdownValue(_reporterDropdown, "ReportReporter", out val2);
			if (val2 != "")
			{
				dictionary.Add("reporter", new Dictionary<string, object> { 
				{
					"accountId",
					JiraConstants.Users[val2]
				} });
				dictionary.Add("Reporter Name", val2);
			}
			ProcessDropdownValue(_assigneeDropdown, "ReportAssignee", out val2);
			if (val2 != "")
			{
				dictionary.Add("assignee", new Dictionary<string, object> { 
				{
					"accountId",
					JiraConstants.Users[val2]
				} });
			}
			ProcessDropdownValue(_priorityDropdown, "ReportPriority", out val2);
			if (val2 != "")
			{
				dictionary.Add("priority", new Dictionary<string, object> { { "name", val2 } });
			}
			ProcessDropdownValue(_severityDropdown, "ReportSeverity", out val2);
			if (val2 != "")
			{
				dictionary.Add("Severity", new Dictionary<string, object> { { "value", val2 } });
			}
		}
		else
		{
			jiraUser = JiraConstants.JiraUserCustomer;
			jiraKey = JiraConstants.JiraKeyCustomer;
			project = JiraConstants.ProjectIDCustomer;
			issueType = JiraConstants.IssueTypeCustomer;
			string text = _nameInput.text.Trim();
			if (text.Length == 0)
			{
				ShowErrorMessage("Name field is empty");
				return;
			}
			StoreSetting("ReportIssueName", text);
			dictionary.Add("Reporter Name", _nameInput.text.Trim());
			string item = JiraConstants.Categories[_categoryDropdown.value].Item2;
			dictionary.Add("Bug Category", new Dictionary<string, object> { { "value", item } });
		}
		if (_titleInput.text.Trim().Length == 0)
		{
			ShowErrorMessage("Title field is empty");
			return;
		}
		if (!ExtendedVersion && _detailsInput.text.Trim().Length == 0)
		{
			ShowErrorMessage("Description field is empty");
			return;
		}
		_windowCanvasGroup.interactable = false;
		_reportSentSuccessfuly = false;
		_uploader = new IssueUploader();
		_uploader.OnUploadStatusChanged += (string message, Exception exception, bool finished, object context) =>
		{
			AddUnityThreadAction(() =>
			{
				_infoText.color = ((exception != null) ? Color.red : Color.blue);
				_infoText.text = message;
				if (exception != null)
				{
					Debug.LogException(exception);
				}
				if (finished)
				{
					if (exception == null)
					{
						if (ExtendedVersion && _openInBrowserToggle.isOn)
						{
							Application.OpenURL(JiraWebApi.GetIssueUrl((string)context));
						}
						StartCoroutine(RunAfter(5f, () =>
						{
							_windowCanvasGroup.interactable = true;
							_uploader = null;
							_reportSentSuccessfuly = true;
							ToggleVisibility();
						}));
					}
					else
					{
						_windowCanvasGroup.interactable = true;
						_uploader = null;
					}
				}
			});
		};
		StartCoroutine(CollectAndUploadIssueData(jiraUser, jiraKey, project, issueType, dictionary, sendLiteReport));
	}

	private IEnumerator CollectPeerLogsCrt()
	{
		if (FFSNetwork.IsClient)
		{
			switch (GHClientCallbacks.CurrentStreamMode)
			{
			case GHClientCallbacks.EStreamMode.ReceivingPlayerLogForReport:
				if (GHClientCallbacks.IsStreamInProgress)
				{
					yield break;
				}
				break;
			default:
				yield break;
			case GHClientCallbacks.EStreamMode.NotSet:
				break;
			}
			_peerLogsPending = 1;
			_infoText.color = Color.grey;
			_infoText.text = "Collecting host log...";
			bool received = false;
			byte[] hostLog = null;
			GHClientCallbacks.OnHostPlayerLogForReportReceived = (byte[] bytes) =>
			{
				hostLog = bytes;
				received = true;
			};
			GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingPlayerLogForReport;
			Synchronizer.RequestCustomData(DataActionType.SendPlayerLogFromHost);
			float deadline = Time.unscaledTime + 60f;
			float hardDeadline = Time.unscaledTime + 120f;
			yield return new WaitUntil(() => received || Time.unscaledTime >= hardDeadline || (Time.unscaledTime >= deadline && !GHClientCallbacks.IsStreamInProgress));
			if (!received)
			{
				GHClientCallbacks.OnHostPlayerLogForReportReceived = null;
				hostLog = Encoding.UTF8.GetBytes("<<< host unresponsive — log collection timed out >>>");
			}
			_peerLogs[0] = hostLog;
			_peerLogsPending = 0;
			ResetInfoMessage();
		}
		else
		{
			if (!FFSNetwork.IsHost)
			{
				yield break;
			}
			List<NetworkPlayer> list = PlayerRegistry.AllPlayers.Where((NetworkPlayer p) => p != PlayerRegistry.MyPlayer && p.IsClient).ToList();
			if (list.Count == 0)
			{
				yield break;
			}
			_peerLogsPending = list.Count;
			_infoText.color = Color.grey;
			_infoText.text = $"Collecting logs from {list.Count} client(s)...";
			bool done = false;
			GHHostCallbacks.RequestClientLogs((Dictionary<int, byte[]> logs) =>
			{
				foreach (KeyValuePair<int, byte[]> log in logs)
				{
					_peerLogs[log.Key] = log.Value;
				}
				_peerLogsPending = 0;
				done = true;
			});
			yield return new WaitUntil(() => done);
			ResetInfoMessage();
		}
	}

	private void OnSaveLocalReport()
	{
		if (!IsBusy)
		{
			_windowCanvasGroup.interactable = false;
			StartCoroutine(SaveLocalReportCoroutine());
		}
	}

	private IEnumerator SaveLocalReportCoroutine()
	{
		yield return new WaitUntil(() => _peerLogsPending == 0);
		IDictionary<string, object> fields = new Dictionary<string, object>();
		ByRef<IList<IssueUploader.UploadFile>> filesToUpload = new ByRef<IList<IssueUploader.UploadFile>>();
		yield return StartCoroutine(CollectIssueData(fields, filesToUpload, sendLiteReport: false));
		string text = Path.Combine(Application.persistentDataPath, "LocalReports", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		Directory.CreateDirectory(text);
		foreach (IssueUploader.UploadFile item in filesToUpload.Value)
		{
			if (item.Data != null && item.Data.Length != 0)
			{
				File.WriteAllBytes(Path.Combine(text, item.Name), item.Data);
			}
		}
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsClient && _peerLogs.TryGetValue(0, out var value))
			{
				File.WriteAllBytes(Path.Combine(text, "HostPlayer.log"), value);
			}
			else if (FFSNetwork.IsHost)
			{
				foreach (KeyValuePair<int, byte[]> kv in _peerLogs)
				{
					NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID.ID == kv.Key);
					string text2 = ((networkPlayer != null) ? ("Client_" + networkPlayer.Username) : $"Client_{kv.Key}");
					string path = text2 + ".log";
					int num = 1;
					while (File.Exists(Path.Combine(text, path)))
					{
						path = $"{text2}_{num++}.log";
					}
					File.WriteAllBytes(Path.Combine(text, path), kv.Value);
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, object> item2 in fields)
		{
			stringBuilder.AppendLine($"{item2.Key}: {item2.Value}");
		}
		File.WriteAllText(Path.Combine(text, "ReportInfo.txt"), stringBuilder.ToString());
		_windowCanvasGroup.interactable = true;
		_infoText.color = Color.blue;
		_infoText.text = "Report saved to:\n" + text;
		Application.OpenURL("file://" + text.Replace('\\', '/'));
	}

	private IEnumerator CollectAndUploadIssueData(string jiraUser, string jiraKey, string project, string issueType, IDictionary<string, object> issueFields, bool sendLiteReport)
	{
		yield return new WaitUntil(() => _peerLogsPending == 0);
		ByRef<IList<IssueUploader.UploadFile>> filesToUpload = new ByRef<IList<IssueUploader.UploadFile>>();
		yield return StartCoroutine(CollectIssueData(issueFields, filesToUpload, sendLiteReport));
		if (FFSNetwork.IsOnline && !sendLiteReport)
		{
			if (FFSNetwork.IsClient && _peerLogs.TryGetValue(0, out var value))
			{
				filesToUpload.Value.Add(new IssueUploader.UploadFile
				{
					Name = "HostPlayer.log",
					Data = value
				});
			}
			else if (FFSNetwork.IsHost)
			{
				foreach (KeyValuePair<int, byte[]> kv in _peerLogs)
				{
					NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID.ID == kv.Key);
					string text = ((networkPlayer != null) ? ("Client_" + networkPlayer.Username + ".log") : $"Client_{kv.Key}.log");
					filesToUpload.Value.Add(new IssueUploader.UploadFile
					{
						Name = text,
						Data = kv.Value
					});
				}
			}
		}
		_uploader.CreateIssueAsync(jiraUser, jiraKey, project, issueType, issueFields, Application.temporaryCachePath, filesToUpload.Value);
	}

	private void OnFieldValueChanged(string val)
	{
		ResetInfoMessage();
	}

	internal void ToggleVisibility()
	{
		if (IsBusy)
		{
			return;
		}
		_prevSelectedObject = null;
		_desyncTitle = null;
		_desyncDescription = null;
		_desyncStateDump1 = null;
		_desyncStateDump2 = null;
		_desyncStateDump3 = null;
		_desyncStateDump4 = null;
		_desyncHostPlayerLog = null;
		if (!Visible)
		{
			_savesFilterDropdown.Interactable = false;
			_savesFilterDropdown.UpdateLabel("<All Saves>");
			_savesFilterDropdown.LateUpdateState();
			if (!_takingScreenshot)
			{
				_takingScreenshot = true;
				StartCoroutine(TakeScreenshotCrt());
			}
		}
		else
		{
			Visible = false;
			UnityEngine.Object.Destroy(_screenshotTexture);
			UnityEngine.Object.Destroy(_originalScreenshotTexture);
			DesyncDetector.OnReportIssueUIHidden();
		}
		_editScreenshotWindow.Visible = false;
	}

	public static void ShowDesync(string title, string description, byte[] stateDump1, byte[] stateDump2, byte[] stateDump3, byte[] stateDump4, byte[] hostPlayerLog)
	{
		if (!(_instance == null) && !_instance.Visible)
		{
			_instance.ToggleVisibility();
			_instance._desyncTitle = title;
			_instance._desyncDescription = description;
			_instance._desyncStateDump1 = stateDump1;
			_instance._desyncStateDump2 = stateDump2;
			_instance._desyncStateDump3 = stateDump3;
			_instance._desyncStateDump4 = stateDump4;
			_instance._desyncHostPlayerLog = hostPlayerLog;
			FastReload.StopAutoUndoRedo();
		}
	}

	public static void UpdateDesyncInfo(string title, string description, byte[] stateDump1, byte[] stateDump2, byte[] stateDump3, byte[] stateDump4, byte[] hostPlayerLog)
	{
		if (!(_instance == null) && (_instance.Visible || _instance._takingScreenshot))
		{
			_instance._desyncTitle = title;
			_instance._desyncDescription = description;
			_instance._desyncStateDump1 = stateDump1;
			_instance._desyncStateDump2 = stateDump2;
			_instance._desyncStateDump3 = stateDump3;
			_instance._desyncStateDump4 = stateDump4;
			_instance._desyncHostPlayerLog = hostPlayerLog;
			_instance.FillInForDesync();
		}
	}

	private void FillInForDesync()
	{
		if (_desyncTitle != null)
		{
			_instance._titleInput.text = _desyncTitle;
			_instance._detailsInput.text = _desyncDescription;
		}
	}

	private IEnumerator<YieldInstruction> TakeScreenshotCrt()
	{
		yield return new WaitForEndOfFrame();
		int width = Screen.width;
		int height = Screen.height;
		_screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);
		_screenshotTexture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		_screenshotTexture.Apply();
		_originalScreenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);
		Graphics.CopyTexture(_screenshotTexture, _originalScreenshotTexture);
		_takingScreenshot = false;
		Visible = true;
		yield return new WaitForEndOfFrame();
		InitGui();
	}

	private IEnumerator DoNextUpdate(Action action)
	{
		yield return null;
		action?.Invoke();
	}

	private static void InitDropdown(Dropdown dropdown, IEnumerable<string> values, string defaultValue, string settingsKey)
	{
		dropdown.ClearOptions();
		dropdown.AddOptions(new List<string> { defaultValue });
		dropdown.AddOptions(values.ToList());
		string item = RestoreSetting(settingsKey, "");
		int num = dropdown.options.Select((Dropdown.OptionData opt) => opt.text).ToList().IndexOf(item);
		if (num != -1)
		{
			dropdown.value = num;
		}
	}

	private void InitInternalVersionData()
	{
		InitDropdown(_versionDropdown, JiraConstants.Versions, "<No Version>", "ReportVersion");
		InitDropdown(_componentDropdown, JiraConstants.Components, "<No Component>", "ReportComponent");
		InitDropdown(_reporterDropdown, JiraConstants.Users.Keys.OrderBy((string nm) => nm), "<Default Reporter>", "ReportReporter");
		InitDropdown(_assigneeDropdown, JiraConstants.Users.Keys.OrderBy((string nm) => nm), "<Default Assignee>", "ReportAssignee");
		InitDropdown(_priorityDropdown, JiraConstants.Priorities, "<Default Priority>", "ReportPriority");
		InitDropdown(_severityDropdown, JiraConstants.Severity, "<Default Severity>", "ReportSeverity");
		_assigneeNames = new List<string>(JiraConstants.Users.Keys).ToArray();
	}

	private void InitReleaseVersionData()
	{
		_categoryDropdown.ClearOptions();
		List<string> options = JiraConstants.Categories.Select(((string, string) v) => v.Item1).ToList();
		_categoryDropdown.AddOptions(options);
		_categoryDropdown.value = 0;
	}

	private void InitGui()
	{
		bool extendedVersion = ExtendedVersion;
		_versionAndComponent.SetActive(extendedVersion);
		_reporterAndAssignee.SetActive(extendedVersion);
		_priorityAndSeverity.SetActive(extendedVersion);
		_savesAndOpenIssue.SetActive(extendedVersion);
		_nameInput.gameObject.SetActive(!extendedVersion);
		_assigneeSearchPanel.SetActive(extendedVersion);
		_sendLiteButton.gameObject.SetActive(extendedVersion);
		_categoryDropdown.gameObject.SetActive(!extendedVersion);
		if (extendedVersion)
		{
			InitInternalVersionData();
		}
		else
		{
			InitReleaseVersionData();
		}
		int num = Mathf.Clamp(Screen.height, MinWindowHeight, MaxWindowHeight);
		RectTransform component = _reportIssueWindow.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, num);
		float x = _screenshotSpace.sizeDelta.x;
		float y = _screenshotSpace.sizeDelta.y;
		float num2 = _screenshotTexture.width;
		float num3 = _screenshotTexture.height;
		float num4 = x / y;
		float num5 = num2 / num3;
		if (num4 > num5)
		{
			Vector2 sizeDelta = new Vector2(y / num3 * num2, y);
			_screenshotPreviewRawImage.rectTransform.sizeDelta = sizeDelta;
		}
		else
		{
			Vector2 sizeDelta2 = new Vector2(x, x / num2 * num3);
			_screenshotPreviewRawImage.rectTransform.sizeDelta = sizeDelta2;
		}
		_screenshotPreviewRawImage.texture = _screenshotTexture;
		_nameInput.text = RestoreSetting("ReportIssueName", "");
		_titleInput.text = string.Empty;
		_detailsInput.text = string.Empty;
		if (Application.platform == RuntimePlatform.XboxOne)
		{
			_openInBrowserToggle.isOn = false;
			_openInBrowserToggle.gameObject.SetActive(value: false);
		}
		_campaignSaves.isOn = true;
		ResetInfoMessage();
		if (_desyncTitle != null)
		{
			FillInForDesync();
		}
		else
		{
			DesyncDetector.FillInErrorReport();
		}
		_peerLogs.Clear();
		_peerLogsPending = 0;
		if (FFSNetwork.IsOnline && _desyncHostPlayerLog == null)
		{
			StartCoroutine(CollectPeerLogsCrt());
		}
		StartCoroutine(DoNextUpdate(() =>
		{
			if (extendedVersion)
			{
				_titleInput.Select();
			}
			else
			{
				_nameInput.Select();
			}
			_prevSelectedObject = _titleInput.gameObject;
		}));
	}

	private void ResetInfoMessage()
	{
		_infoText.text = DefaultInfoMessage;
		_infoText.color = Color.black;
	}

	private void ShowErrorMessage(string message)
	{
		_infoText.color = Color.red;
		_infoText.text = message;
		if (_hideMessageCoroutine != null)
		{
			StopCoroutine(_hideMessageCoroutine);
		}
		_hideMessageCoroutine = StartCoroutine(HideInfoTextAfter(2f));
	}

	private IEnumerator<YieldInstruction> HideInfoTextAfter(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		ResetInfoMessage();
		_hideMessageCoroutine = null;
	}

	private IEnumerator<YieldInstruction> RunAfter(float time, Action action)
	{
		yield return new WaitForSeconds(time);
		action();
	}

	private IEnumerator<NextUpdate> AddSavegameToFiles(Stream stream, string fileName, IList<IssueUploader.UploadFile> filesToUpload)
	{
		IssueUploader.UploadFile item = new IssueUploader.UploadFile
		{
			Name = fileName,
			ContentType = "application/octet-stream",
			SuffixIssueId = true,
			Data = new byte[stream.Length],
			GroupName = "Savegames",
			UploadPriority = 4
		};
		stream.Read(item.Data, 0, (int)stream.Length);
		filesToUpload.Add(item);
		yield break;
	}

	private static IEnumerator SaveFastReloadHistoryCoro()
	{
		yield return new WaitForTask(FastReload.SaveAllHistory());
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, string[] excludePattern, string includePattern, ByRef<string> currentDirLog)
	{
		sourceDirName = sourceDirName.Replace("\\", "/");
		destDirName = destDirName.Replace("\\", "/");
		includePattern = includePattern.Replace("\\", "/");
		for (int i = 0; i < excludePattern.Length; i++)
		{
			excludePattern[i] = excludePattern[i].Replace("\\", "/");
		}
		currentDirLog.Value = sourceDirName;
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		if (Ignored(sourceDirName))
		{
			return;
		}
		Directory.CreateDirectory(destDirName);
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, overwrite: false);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				DirectoryCopy(directoryInfo2.FullName, destDirName2, copySubDirs, excludePattern, includePattern, currentDirLog);
			}
		}
		bool Ignored(string d)
		{
			if (IsSubdirectoryOf(d, includePattern))
			{
				return false;
			}
			return excludePattern.Any((string a) => d.Contains(a));
		}
		static bool IsSubdirectoryOf(string parent, string child)
		{
			parent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			child = Path.GetFullPath(child).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return child.StartsWith(parent, StringComparison.OrdinalIgnoreCase);
		}
	}

	private IEnumerator CollectSaveGames(IList<IssueUploader.UploadFile> filesToUpload, SaveGameRecordsFilter filter)
	{
		yield return StartCoroutine(SaveFastReloadHistoryCoro());
		if (_campaignSaves.isOn)
		{
			Task<string> task = SaveData.Instance.Then((SaveData saveData) => saveData.Global?.CurrentAdventureData).Then((PartyAdventureData partyAdventureData) => partyAdventureData.PartySaveDir);
			if (task != null)
			{
				DateTime timeout = DateTime.Now + TimeSpan.FromMinutes(2.0);
				while (!task.IsCompleted && DateTime.Now < timeout)
				{
					yield return null;
				}
				if (task.IsCompleted)
				{
					string result = task.Result;
					if (Directory.Exists(result))
					{
						ByRef<string> tempDir = new ByRef<string>();
						yield return GetTemp(result, tempDir);
						bool ok = false;
						tempDir.Value.And((string temp) => !string.IsNullOrEmpty(temp)).Then((string temp) =>
						{
							IssueUploader.UploadFile item2 = new IssueUploader.UploadFile
							{
								Name = "SaveGames",
								ContentType = "application/zip",
								SuffixIssueId = true,
								Directory = temp,
								UploadPriority = 4,
								DeleteWhenDone = true
							};
							filesToUpload.Add(item2);
							ok = true;
						});
						if (ok)
						{
							yield break;
						}
					}
				}
				if (task.IsFaulted)
				{
					Debug.LogError($"Error in async task: {task.Exception}");
				}
			}
		}
		IssueUploader.UploadFile item = new IssueUploader.UploadFile
		{
			Name = "SaveGames",
			ContentType = "application/zip",
			SuffixIssueId = true,
			Directory = Application.persistentDataPath,
			UploadPriority = 4
		};
		filesToUpload.Add(item);
		IEnumerator GetTemp(string campaignSaveDir, ByRef<string> outResult)
		{
			string tempDir2 = null;
			ByRef<string> currentDirLog = new ByRef<string>();
			string tempCachePath = Application.temporaryCachePath;
			string persistentDataPath = Application.persistentDataPath;
			Task task2 = Task.Run(() =>
			{
				try
				{
					tempDir2 = Path.Combine(tempCachePath, "FilteredSaves_" + Guid.NewGuid().ToString("N"));
					DirectoryCopy(persistentDataPath, tempDir2, copySubDirs: true, new string[4]
					{
						Directory.GetParent(campaignSaveDir).FullName,
						"/Steam/",
						"/EGS/",
						"/Local/"
					}, campaignSaveDir, currentDirLog);
					if (string.IsNullOrEmpty(tempDir2) || !Directory.Exists(tempDir2))
					{
						throw new Exception("tempDir empty");
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to filter campaign saves: " + ex.Message);
					if (tempDir2 != null && Directory.Exists(tempDir2))
					{
						try
						{
							Directory.Delete(tempDir2, recursive: true);
						}
						catch
						{
						}
					}
					tempDir2 = null;
				}
			});
			DateTime timestart = DateTime.Now;
			while (!task2.IsCompleted && DateTime.Now - timestart < TimeSpan.FromMinutes(2.0))
			{
				_infoText.text = "Copying: " + currentDirLog?.Value;
				yield return null;
			}
			outResult.Value = tempDir2;
		}
	}

	private IEnumerator<YieldInstruction> CollectIssueData(IDictionary<string, object> fields, ByRef<IList<IssueUploader.UploadFile>> filesToUpload, bool sendLiteReport)
	{
		IssueUploader.UploadFile item = new IssueUploader.UploadFile
		{
			Name = "Screenshot.png",
			ContentType = "image/png",
			SuffixIssueId = true,
			Data = _screenshotTexture.EncodeToPNG(),
			UploadPriority = 1
		};
		filesToUpload.Value = new List<IssueUploader.UploadFile> { item };
		bool flag = _desyncHostPlayerLog == null && _desyncStateDump3 == null && _desyncStateDump4 == null;
		if (_desyncStateDump1 != null)
		{
			IssueUploader.UploadFile item2 = new IssueUploader.UploadFile
			{
				Name = (flag ? "SavedState.yml" : "HostState.yml"),
				ContentType = "application/yaml",
				SuffixIssueId = true,
				Data = _desyncStateDump1,
				UploadPriority = 3
			};
			filesToUpload.Value.Add(item2);
		}
		if (_desyncStateDump2 != null)
		{
			IssueUploader.UploadFile item3 = new IssueUploader.UploadFile
			{
				Name = (flag ? "LoadedState.yml" : "ClientState.yml"),
				ContentType = "application/yaml",
				SuffixIssueId = true,
				Data = _desyncStateDump2,
				UploadPriority = 3
			};
			filesToUpload.Value.Add(item3);
		}
		if (_desyncStateDump3 != null)
		{
			IssueUploader.UploadFile item4 = new IssueUploader.UploadFile
			{
				Name = (flag ? "SavedState-Prev.yml" : "HostState-Prev.yml"),
				ContentType = "application/yaml",
				SuffixIssueId = true,
				Data = _desyncStateDump3,
				UploadPriority = 3
			};
			filesToUpload.Value.Add(item4);
		}
		if (_desyncStateDump4 != null)
		{
			IssueUploader.UploadFile item5 = new IssueUploader.UploadFile
			{
				Name = (flag ? "LoadedState-Prev.yml" : "ClientState-Prev.yml"),
				ContentType = "application/yaml",
				SuffixIssueId = true,
				Data = _desyncStateDump4,
				UploadPriority = 3
			};
			filesToUpload.Value.Add(item5);
		}
		if (_desyncHostPlayerLog != null)
		{
			IssueUploader.UploadFile item6 = new IssueUploader.UploadFile
			{
				Name = "HostPlayer.log",
				ContentType = "text/plain",
				SuffixIssueId = true,
				Data = _desyncHostPlayerLog,
				UploadPriority = 3
			};
			filesToUpload.Value.Add(item6);
		}
		if (!sendLiteReport)
		{
			try
			{
				string logFileLocation = GetLogFileLocation();
				if (!string.IsNullOrEmpty(logFileLocation) && File.Exists(logFileLocation))
				{
					using FileStream fileStream = new FileStream(logFileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					IssueUploader.UploadFile item7 = new IssueUploader.UploadFile
					{
						Name = Path.GetFileName(logFileLocation),
						ContentType = "text/plain",
						SuffixIssueId = true,
						Data = new byte[fileStream.Length],
						UploadPriority = 2
					};
					fileStream.Read(item7.Data, 0, (int)fileStream.Length);
					filesToUpload.Value.Add(item7);
				}
				string text;
				if (!string.IsNullOrEmpty(logFileLocation))
				{
					string[] array = new string[5]
					{
						Path.GetDirectoryName(logFileLocation),
						null,
						null,
						null,
						null
					};
					char directorySeparatorChar = Path.DirectorySeparatorChar;
					array[1] = directorySeparatorChar.ToString();
					array[2] = Path.GetFileNameWithoutExtension(logFileLocation);
					array[3] = "-prev";
					array[4] = Path.GetExtension(logFileLocation);
					text = string.Concat(array);
				}
				else
				{
					text = string.Empty;
				}
				string text2 = text;
				if (!string.IsNullOrEmpty(text2) && File.Exists(text2))
				{
					using FileStream fileStream2 = new FileStream(text2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					byte[] array2;
					if (fileStream2.Length > 204800)
					{
						array2 = new byte[204800];
						fileStream2.Seek(fileStream2.Length - 204800, SeekOrigin.Begin);
						fileStream2.Read(array2, 0, 204800);
					}
					else
					{
						array2 = new byte[fileStream2.Length];
						fileStream2.Read(array2, 0, (int)fileStream2.Length);
					}
					IssueUploader.UploadFile item8 = new IssueUploader.UploadFile
					{
						Name = Path.GetFileName(text2),
						ContentType = "text/plain",
						SuffixIssueId = true,
						Data = array2,
						UploadPriority = 2
					};
					filesToUpload.Value.Add(item8);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			yield return StartCoroutine(CollectSaveGames(filesToUpload.Value, SaveGameRecordsFilter.None));
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PropertyInfo item9 in from fi in typeof(SystemInfo).GetProperties(BindingFlags.Static | BindingFlags.Public)
			orderby fi.Name
			select fi)
		{
			stringBuilder.Append(item9.Name).Append(": ").Append(item9.GetValue(null))
				.AppendLine();
		}
		stringBuilder.Append("screenResolution: ").Append(Screen.currentResolution).AppendLine();
		stringBuilder.Append("screenDpi: ").Append(Screen.dpi).AppendLine();
		stringBuilder.Append("fullScreen: ").Append(Screen.fullScreen).AppendLine();
		stringBuilder.Append("Joysticks: ").Append(string.Join(", ", Input.GetJoystickNames()));
		string text3 = _titleInput.text.Trim();
		if (!ExtendedVersion && !GameConsoleWindow.DisableConsoleAccess)
		{
			text3 = "[Cheats] " + text3;
		}
		fields.AddRange(new Dictionary<string, object>
		{
			{ "Summary", text3 },
			{
				"Description",
				_detailsInput.text.Trim()
			},
			{
				"Reporter ID",
				SystemInfo.deviceUniqueIdentifier
			},
			{
				"Build Revision Number",
				RuntimeBuildInfo.BuildRevisionNumber
			},
			{
				"Build VCS Path",
				RuntimeBuildInfo.BuildRepoPath
			},
			{
				"Environment",
				stringBuilder.ToString()
			},
			{
				"Operating System",
				new Dictionary<string, object> { 
				{
					"value",
					RuntimeBuildInfo.OperatingSystem
				} }
			}
		});
	}

	private void AddUnityThreadAction(Action action)
	{
		_unityThreadActions.Enqueue(action);
	}

	private void Update()
	{
		while (_unityThreadActions.Count > 0)
		{
			_unityThreadActions.Dequeue()();
		}
		if (Visible && _prevSelectedObject != null)
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (currentSelectedGameObject == null || !currentSelectedGameObject.transform.IsChildOf(transform))
			{
				EventSystem.current.SetSelectedGameObject(_prevSelectedObject);
			}
			_prevSelectedObject = EventSystem.current.currentSelectedGameObject;
		}
	}

	public static string GetLogFileLocation()
	{
		string instanceId = RuntimeBuildInfo.InstanceId;
		if (instanceId != null)
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "Player-" + instanceId + ".log");
		}
		return Application.consoleLogPath;
	}

	public void AssigneeNameInputUpdate()
	{
		List<string> list = new List<string>(_assigneeNames.Length);
		string text = _assigneeSearchField.text.Trim();
		if (text != string.Empty)
		{
			string[] assigneeNames = _assigneeNames;
			foreach (string text2 in assigneeNames)
			{
				if (text2.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1)
				{
					list.Add(text2);
				}
			}
		}
		else
		{
			list.AddRange(_assigneeNames);
		}
		UpdateAssigneeDropdown(list);
	}

	private void UpdateAssigneeDropdown(List<string> suggestions)
	{
		_assigneeDropdown.ClearOptions();
		_assigneeDropdown.AddOptions(suggestions);
		_assigneeDropdown.value = 0;
	}

	private void StoreSetting(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
		PlayerPrefs.Save();
	}

	private static string RestoreSetting(string key, string defaultVal)
	{
		return PlayerPrefs.GetString(key, defaultVal);
	}
}

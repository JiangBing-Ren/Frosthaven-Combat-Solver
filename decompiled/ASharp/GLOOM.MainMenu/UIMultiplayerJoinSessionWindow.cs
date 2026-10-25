using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIMultiplayerJoinSessionWindow : UISubmenuGOWindow
{
	[SerializeField]
	private UIKeyboard controllerKeyboard;

	[Header("Invite code")]
	[SerializeField]
	public TMP_InputField inviteCodeInput;

	[SerializeField]
	private int minSizeSessionId = 1;

	[Header("Connection status")]
	[SerializeField]
	private GameObject statusPanel;

	[SerializeField]
	private TextLocalizedListener statusTitle;

	[SerializeField]
	private TextLocalizedListener statusText;

	[SerializeField]
	private LoopAnimator connectingAnimation;

	[Header("Users")]
	[SerializeField]
	private GameObject partyPanel;

	[SerializeField]
	private TextMeshProUGUI partyName;

	[SerializeField]
	private MultiplayerUserState[] partyUsers;

	[Header("Confirm")]
	[SerializeField]
	private FHButton continueButton;

	[SerializeField]
	private GameObject m_HighlightConfirmButton;

	[Header("Confirm")]
	[SerializeField]
	private Color ColorStatuStextNormal;

	[SerializeField]
	private Color ColorStatuStextError;

	[SerializeField]
	private FHButton closeButton;

	public static UIMultiplayerJoinSessionWindow s_This;

	private ConnectionState connectionState;

	public UnityEvent OnStartJoin;

	public UnityEvent OnFailedJoin;

	public UnityEvent OnStartCancel;

	public UnityEvent OnFinishCancel;

	private UnityAction continueAction;

	public ConnectionState CurrentConnectionState => connectionState;

	protected override void AwakeInit()
	{
		base.AwakeInit();
		continueButton.onClick.AddListener(HandleOnContinue);
		closeButton.onClick.AddListener(Hide);
		inviteCodeInput.onValueChanged.AddListener(ValidateCode);
		inviteCodeInput.onSubmit.AddListener((string s) =>
		{
			HandleOnContinue();
		});
		showAnimator.OnAnimationFinished.AddListener(() =>
		{
			if (inviteCodeInput.text.IsNullOrEmpty())
			{
				inviteCodeInput.ActivateInputField();
				inviteCodeInput.Select();
			}
		});
	}

	protected void OnEnable()
	{
		s_This = this;
		Clear(clearInput: true);
	}

	private void ValidateCode(string code)
	{
		bool interactable = code.Length >= minSizeSessionId;
		EnableConfirmationButton(interactable);
	}

	private void Clear(bool clearInput)
	{
		Log.GUIInfo("GUI: Join session screen reset.");
		connectionState = ConnectionState.None;
		continueAction = JoinSessionNULL;
		continueButton.SetTextKey("GUI_CONFIRM");
		inviteCodeInput.interactable = true;
		statusPanel.SetActive(value: false);
		partyPanel.SetActive(value: false);
		connectingAnimation.StopLoop(resetToInitial: true);
		if (clearInput)
		{
			inviteCodeInput.text = string.Empty;
			inviteCodeInput.caretPosition = 0;
		}
		EnableConfirmationButton(inviteCodeInput.text.Length >= minSizeSessionId);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		s_This = null;
	}

	private void EnableConfirmationButton(bool interactable)
	{
		continueButton.interactable = interactable;
		if ((bool)m_HighlightConfirmButton)
		{
			m_HighlightConfirmButton.SetActive(interactable);
		}
	}

	private void HandleOnContinue()
	{
		if (continueButton.interactable)
		{
			continueAction?.Invoke();
		}
	}

	public void Cancel()
	{
		EnableConfirmationButton(interactable: false);
		OnStartCancel.Invoke();
		FFSNetwork.CancelJoiningSession(() =>
		{
			Clear(clearInput: false);
			OnFinishCancel.Invoke();
		});
	}

	public void Retry()
	{
		Clear(clearInput: true);
	}

	private void JoinSessionNULL()
	{
		JoinSession();
	}

	public bool JoinSession(string sessionID = null)
	{
		if (!sessionID.IsNOTNullOrEmpty() && inviteCodeInput.text.Length <= 0)
		{
			return false;
		}
		continueAction = Cancel;
		continueButton.SetTextKey("GUI_CANCEL");
		continueButton.interactable = true;
		inviteCodeInput.interactable = false;
		controllerKeyboard.Hide();
		statusTitle.SetTextKey("GUI_MULTIPLAYER_CONNECTING");
		statusTitle.Text.color = UIInfoTools.Instance.playerConnectingColor;
		connectingAnimation.StartLoop(resetToInitial: true);
		statusPanel.SetActive(value: true);
		statusText.SetTextKey("GUI_MULTIPLAYER_CONNECTING_SearchingForSession");
		statusText.Text.color = ColorStatuStextNormal;
		sessionID = (sessionID.IsNOTNullOrEmpty() ? sessionID : inviteCodeInput.text.ToUpper());
		OnStartJoin.Invoke();
		FFSNetwork.JoinSession(sessionID, OnConnectionEstablished, OnConnectionStateUpdated, OnConnectionFailed, OnDisconnected);
		return true;
	}

	private void OnConnectionEstablished(CustomDataToken gameData)
	{
		Log.GUIInfo("GUI: OnConnectionEstablished");
		connectionState = ConnectionState.None;
		connectingAnimation.StopLoop(resetToInitial: true);
		continueAction = null;
		statusTitle.SetTextKey("GUI_MULTIPLAYER_INVITE_ACCEPTED");
		statusTitle.Text.color = UIInfoTools.Instance.playerOnlineColor;
		statusText.SetTextKey("GUI_MULTIPLAYER_JOINING_SESSION");
		statusText.Text.color = ColorStatuStextNormal;
		partyName.text = ((GameToken)gameData).SaveName;
	}

	private void OnConnectionStateUpdated(ConnectionState connectionState)
	{
		Log.GUIInfo("GUI: OnConnectionStateUpdated (" + connectionState.ToString() + ") " + PlayerRegistry.AllPlayers.IsNullOrEmpty());
		this.connectionState = connectionState;
		statusText.SetTextKey($"GUI_MULTIPLAYER_CONNECTING_{connectionState}");
		statusText.Text.color = ColorStatuStextNormal;
		if (connectionState == ConnectionState.SessionFound)
		{
			ShowPlayers();
		}
	}

	private void ShowPlayers()
	{
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			partyUsers[i].Show(PlayerRegistry.AllPlayers[i], !PlayerRegistry.IsJoining(PlayerRegistry.AllPlayers[i]));
		}
		for (int j = PlayerRegistry.AllPlayers.Count; j < partyUsers.Length; j++)
		{
			partyUsers[j].Hide();
		}
		partyPanel.SetActive(value: true);
	}

	private void OnConnectionFailed(ConnectionErrorCode errorCode)
	{
		Log.GUIInfo("GUI: OnConnectionFailed");
		ShowConnectionError(errorCode.ToString());
		OnFailedJoin.Invoke();
	}

	private void ShowConnectionError(string errorCode)
	{
		continueAction = Retry;
		continueButton.SetTextKey("GUI_RETRY");
		statusTitle.SetTextKey("GUI_MULTIPLAYER_CONNECTION_FAILED");
		statusTitle.Text.color = UIInfoTools.Instance.warningColor;
		statusText.SetTextKey("GUI_MULTIPLAYER_CONNECTION_FAILED_" + errorCode);
		statusText.Text.color = ColorStatuStextError;
		connectingAnimation.StopLoop(resetToInitial: true);
	}

	private void OnDisconnected(DisconnectionErrorCode errorCode)
	{
		if (connectionState.In(ConnectionState.WaitUntilSavePoint, ConnectionState.SavePointReached, ConnectionState.DownloadingNewSave, ConnectionState.Connecting))
		{
			ShowConnectionError(errorCode.ToString());
		}
		else if (connectionState != ConnectionState.None && errorCode == DisconnectionErrorCode.HostEndedSession)
		{
			Clear(clearInput: true);
			if (PlayerRegistry.HostPlayer != null)
			{
				string explanation = string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOST_DISCONNECTED_CONFIRMATION"), PlayerRegistry.HostPlayer.Username);
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOST_DISCONNECTED_CONFIRMATION_TITLE"), explanation, UIInfoTools.Instance.warningColor, "GUI_CONTINUE");
			}
		}
		OnFailedJoin.Invoke();
	}
}

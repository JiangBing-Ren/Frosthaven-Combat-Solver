using System;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FFSNet;
using GLOOM;
using MEC;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle), typeof(UIWindow))]
public class UIReadyToggle : Singleton<UIReadyToggle>
{
	public enum EAutoEndSelection
	{
		NA,
		Manual,
		Auto
	}

	public enum EReadyUpType
	{
		Participant,
		Player
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextLocalizedListener buttonText;

	[SerializeField]
	private Graphic textGraphic;

	[SerializeField]
	private ImageProgressBar progressBar;

	private ExtendedToggle toggle;

	private UnityAction onReady;

	private UnityAction onUnready;

	private UnityAction onAllPlayersReady;

	private UnityAction<NetworkPlayer, bool> onReadiedPlayersChanged;

	private Func<bool> canReadyUp;

	private UnityAction<bool> onStartProgress;

	private UnityAction<bool> onEndProgress;

	private bool validateReadyUpOnPlayerLeft;

	private string readyTextLoc;

	private string unreadyTextLoc;

	private string readyAudioItem;

	private List<NetworkPlayer> playersAwaited = new List<NetworkPlayer>();

	private bool waitingForReadyCheckResponses;

	private CoroutineHandle stateSyncCheckRoutine;

	private EReadyUpType readyUpType;

	private EReadyUpToggleStates readyUpToggleState;

	private static readonly float stateSyncCheckInterval = 0.2f;

	private static readonly float clientStateSyncTimeoutDuration = 20f;

	private static readonly float globalStateSyncTimeoutDuration = 30f;

	public List<NetworkPlayer> PlayersReady { get; private set; } = new List<NetworkPlayer>();

	public bool IsInteractable => toggle.interactable;

	public bool ToggledOn => toggle.isOn;

	public bool IsVisible => window.IsOpen;

	public EReadyUpType ReadyUpType => readyUpType;

	public bool IsProgressingBar => progressBar.gameObject.activeInHierarchy;

	public Dictionary<NetworkPlayer, EReadyUpToggleStates> PlayerReadiedState { get; private set; } = new Dictionary<NetworkPlayer, EReadyUpToggleStates>();

	public EAutoEndSelection AutoEndSelection { get; private set; }

	protected override void AwakeInit()
	{
		toggle = GetComponent<ExtendedToggle>();
		toggle.onValueChanged.AddListener(InputToggle);
	}

	protected override void AwakenedDestroy()
	{
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ResetPlayerACKs();
	}

	private void InputToggle(bool isOn)
	{
		if (ScenarioRuleClient.IsHandStateMessagePending)
		{
			Log.Info("Ignoring ready up toggle click because a player is about to send his hand/short rest state");
			toggle.SetIsOnWithoutNotify(PlayersReady.Contains(PlayerRegistry.MyPlayer));
		}
		else if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
		{
			UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			toggle.SetIsOnWithoutNotify(PlayersReady.Contains(PlayerRegistry.MyPlayer));
		}
		else if (isOn != PlayersReady.Contains(PlayerRegistry.MyPlayer))
		{
			Log.GUIInfoFormat("Start ready {0} progression", isOn);
			progressBar.SetAmount(0f, 1f);
			progressBar.gameObject.SetActive(value: true);
			onStartProgress?.Invoke(isOn);
			progressBar.PlayProgressTo(1f, LocalizationManager.GetTranslation(isOn ? "GUI_MULTIPLAYER_READYING" : "GUI_MULTIPLAYER_UNREADYING"), () =>
			{
				Log.GUIInfoFormat("Completed ready {0} progression", isOn);
				onEndProgress?.Invoke(isOn);
				ReadyUp(isOn);
			});
		}
		else
		{
			Log.GUIInfo("Cancel ready progression");
			progressBar.gameObject.SetActive(value: false);
			toggle.SetIsOnWithoutNotify(isOn);
			onEndProgress?.Invoke(isOn);
		}
	}

	public void Initialize(bool show = true, UnityAction onReady = null, UnityAction onUnready = null, UnityAction onAllPlayersReady = null, UnityAction<NetworkPlayer, bool> onReadiedPlayersChanged = null, Func<bool> canReadyUp = null, string readyTextLoc = "GUI_READY", string unreadyTextLoc = "GUI_UNREADY", string readyAudioItem = "PlaySound_UIMultiPlayerReady", bool bringToFront = false, UnityAction<bool> onStartProgress = null, UnityAction<bool> onEndProgress = null, bool validateReadyUpOnPlayerLeft = false, EReadyUpType readyUpType = EReadyUpType.Participant, EReadyUpToggleStates readyUpToggleState = EReadyUpToggleStates.NotSet)
	{
		this.onReady = onReady;
		this.onUnready = onUnready;
		this.onAllPlayersReady = onAllPlayersReady;
		this.onReadiedPlayersChanged = onReadiedPlayersChanged;
		this.canReadyUp = canReadyUp;
		this.readyTextLoc = readyTextLoc;
		this.unreadyTextLoc = unreadyTextLoc;
		this.readyAudioItem = readyAudioItem;
		this.onStartProgress = onStartProgress;
		this.onEndProgress = onEndProgress;
		this.validateReadyUpOnPlayerLeft = validateReadyUpOnPlayerLeft;
		this.readyUpType = readyUpType;
		this.readyUpToggleState = readyUpToggleState;
		toggle.mouseDownAudioItem = readyAudioItem;
		SetInteractable(interactable: false);
		toggle.SetIsOnWithoutNotify(value: false);
		buttonText.SetTextKey(readyTextLoc);
		progressBar.gameObject.SetActive(value: false);
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		foreach (NetworkPlayer item in PlayersReady.Except(PlayerRegistry.AllPlayers).ToList())
		{
			OnPlayerLeft(item);
		}
		if (bringToFront)
		{
			BringToFront();
		}
		if (show)
		{
			ToggleVisibility(visible: true);
		}
	}

	public void BringToFront()
	{
		UIUtility.BringToFront(window.gameObject);
	}

	public void Reset()
	{
		Log.GUIInfo("Reset players ready");
		onReady = null;
		onUnready = null;
		onAllPlayersReady = null;
		onReadiedPlayersChanged = null;
		toggle.mouseDownAudioItem = readyAudioItem;
		validateReadyUpOnPlayerLeft = false;
		AutoEndSelection = EAutoEndSelection.NA;
		progressBar.gameObject.SetActive(value: false);
		SetInteractable(interactable: false);
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ResetPlayerACKs();
		PlayersReady.Clear();
		PlayerReadiedState.Clear();
		playersAwaited.Clear();
		window.Hide();
	}

	public void ResetPlayerACKs()
	{
		if (FFSNetwork.IsHost)
		{
			Timing.KillCoroutines(stateSyncCheckRoutine);
			waitingForReadyCheckResponses = false;
			PlayerRegistry.AllPlayers.ForEach((NetworkPlayer x) =>
			{
				x.PlayersACKedMyLatestControllableState.Clear();
			});
		}
		else
		{
			PlayerRegistry.AllPlayers.ForEach((NetworkPlayer x) =>
			{
				Timing.KillCoroutines(x.PlayerID.ToString());
			});
		}
	}

	public void ToggleVisibility(bool visible, bool bringToFront = false)
	{
		bool isVisible = IsVisible;
		progressBar.gameObject.SetActive(value: false);
		if (visible)
		{
			if (bringToFront)
			{
				UIUtility.BringToFront(window.gameObject);
			}
			window.Show();
		}
		else
		{
			CancelProgress();
			window.Hide();
		}
		Log.NetworkInfo("MP Ready Toggle set to " + (visible ? " VISIBLE." : "INVISIBLE."));
		UpdateAutoEndSelection(isVisible, IsInteractable);
	}

	public void SetInteractable(bool interactable)
	{
		if (toggle.interactable != interactable)
		{
			toggle.interactable = interactable;
			textGraphic.CrossFadeColor(interactable ? UIInfoTools.Instance.White : UIInfoTools.DefInstance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
			Log.NetworkInfo("MP Ready Toggle set to " + (interactable ? " INTERACTABLE." : " UNINTERACTABLE."));
			UpdateAutoEndSelection(IsVisible, !interactable);
		}
	}

	public void CancelProgress()
	{
		if (progressBar.gameObject.activeSelf)
		{
			progressBar.gameObject.SetActive(value: false);
			bool isOn = toggle.isOn;
			toggle.SetIsOnWithoutNotify(!isOn);
			onEndProgress?.Invoke(isOn);
			Log.GUIInfo("Cancel ready progression");
		}
	}

	public void WaitForPlayerBeforeProceeding(NetworkPlayer player)
	{
		if (!playersAwaited.Contains(player))
		{
			playersAwaited.Add(player);
			Log.NetworkInfo(player.Username + " added to the list of awaited players.");
		}
	}

	public void ReadyUp(bool toggledOn)
	{
		ReadyUp(toggledOn, autoValidateUnreadying: false);
	}

	public void ReadyUp(bool toggledOn, bool autoValidateUnreadying)
	{
		AutoEndSelection = EAutoEndSelection.NA;
		progressBar.gameObject.SetActive(value: false);
		if (FFSNetwork.IsOnline && (PlayersReady.Count < ((readyUpType == EReadyUpType.Participant) ? PlayerRegistry.Participants : PlayerRegistry.AllPlayers).Count || (!toggledOn & autoValidateUnreadying)))
		{
			if (toggledOn)
			{
				if (canReadyUp != null && !canReadyUp())
				{
					toggle.mouseDownAudioItem = readyAudioItem;
					toggle.SetIsOnWithoutNotify(value: false);
					return;
				}
				ReadyUpPlayer(PlayerRegistry.MyPlayer, readyUpToggleState);
			}
			else
			{
				UnreadyPlayer(PlayerRegistry.MyPlayer, isValidatedAction: false, autoValidateUnreadying);
			}
		}
		toggle.mouseDownAudioItem = (toggledOn ? null : readyAudioItem);
	}

	private bool ReadyUpPlayer(NetworkPlayer player, EReadyUpToggleStates state, bool force = false)
	{
		if (ScenarioRuleClient.IsHandStateMessagePending && !force)
		{
			Log.Info("Ignoring ready up final callback (after progress bar) because a player is about to send his hand/short rest state");
			toggle.SetIsOnWithoutNotify(PlayersReady.Contains(PlayerRegistry.MyPlayer));
			return false;
		}
		if (PlayersReady.Exists((NetworkPlayer e) => e.PlayerID == player.PlayerID))
		{
			return false;
		}
		if (playersAwaited.Contains(player))
		{
			playersAwaited.Remove(player);
		}
		PlayersReady.Add(player);
		PlayerReadiedState[player] = state;
		onReadiedPlayersChanged?.Invoke(player, arg1: true);
		Log.NetworkInfo("Readied " + player.Username + ". Players Ready Count: " + PlayersReady.Count);
		if (player.Equals(PlayerRegistry.MyPlayer))
		{
			buttonText.SetTextKey(unreadyTextLoc);
			toggle.SetIsOnWithoutNotify(value: true);
			onReady?.Invoke();
			if (FFSNetwork.IsHost)
			{
				Synchronizer.SendGameAction(GameActionType.ReadyUpPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new ControllableStateRevisionToken(PlayerRegistry.MyPlayer), new ReadyUpToken(readyUpToggleState.ToString()));
			}
			else
			{
				Synchronizer.SendGameAction(GameActionType.ReadyUpPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, null, new ReadyUpToken(readyUpToggleState.ToString()));
			}
		}
		if (FFSNetwork.IsHost)
		{
			if (readyUpType == EReadyUpType.Participant && PlayerRegistry.MyPlayer.IsParticipant && PlayersReady.Count >= PlayerRegistry.Participants.Count && playersAwaited.Count == 0 && PlayersReady.Contains(PlayerRegistry.MyPlayer))
			{
				stateSyncCheckRoutine = Timing.RunCoroutine(WaitForStateSyncBeforeProceeding());
			}
			else if (readyUpType == EReadyUpType.Player && PlayersReady.Count >= PlayerRegistry.AllPlayers.Count && playersAwaited.Count == 0)
			{
				stateSyncCheckRoutine = Timing.RunCoroutine(WaitForStateSyncBeforeProceeding());
			}
		}
		return true;
	}

	private IEnumerator<float> WaitForStateSyncBeforeProceeding()
	{
		float timeOutTime = Timekeeper.instance.m_GlobalClock.time + globalStateSyncTimeoutDuration;
		ResetPlayerACKs();
		waitingForReadyCheckResponses = true;
		Log.GUIInfo("WaitForStateSyncBeforeProceeding " + readyUpType.ToString() + " (connecting:" + ((PlayerRegistry.ConnectingUsers != null) ? PlayerRegistry.ConnectingUsers.Count : 0) + ")");
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.PlayersACKedMyLatestControllableState.Add(PlayerRegistry.MyPlayer);
		}
		Synchronizer.SendGameAction(GameActionType.AllPlayersReadyNew, ActionProcessor.CurrentPhase);
		bool reportedSoftlock = false;
		while (PlayersReady.Exists((NetworkPlayer x) => x.IsActive && ((readyUpType == EReadyUpType.Player) ? PlayerRegistry.AllPlayers : PlayerRegistry.Participants).Where((NetworkPlayer w) => !w.HasDesynched).ToList().Exists((NetworkPlayer y) => y != x && !x.PlayersACKedMyLatestControllableState.Contains(y))))
		{
			if (Timekeeper.instance.m_GlobalClock.time > timeOutTime && !reportedSoftlock)
			{
				string text = "Player ACKs Missing:\n";
				foreach (NetworkPlayer player in PlayersReady.Where((NetworkPlayer w) => w.IsActive))
				{
					IEnumerable<string> enumerable = from s in (readyUpType == EReadyUpType.Player) ? PlayerRegistry.AllPlayers : PlayerRegistry.Participants
						where s != player && !player.PlayersACKedMyLatestControllableState.Contains(s)
						select s.Username;
					if (enumerable.Count() > 0)
					{
						text = text + player.Username + " is missing ACK from: " + string.Join(", ", enumerable);
					}
				}
				reportedSoftlock = true;
				UIMultiplayerNotifications.ShowPossibleTimeout();
			}
			yield return Timing.WaitForSeconds(stateSyncCheckInterval);
			if (!PlayerRegistry.ConnectingUsers.IsNullOrEmpty())
			{
				Log.GUIInfo("User entered room while WaitForStateSyncBeforeProceeding");
			}
		}
		UIMultiplayerNotifications.HidePossibleTimeout();
		while (ActionProcessor.CurrentPhase == ActionPhaseType.MapLoadoutScreen && ActionProcessor.ActionQueue.Any((GameAction it) => it.TargetPhaseID == 3))
		{
			Log.GUIError("Waiting all actions end");
			yield return float.NegativeInfinity;
		}
		Synchronizer.SendGameAction(GameActionType.ReadyProceed, ActionProcessor.CurrentPhase);
		waitingForReadyCheckResponses = false;
		Proceed();
	}

	private void Proceed()
	{
		if (onAllPlayersReady != null)
		{
			Log.GUIInfo("AllPlayersReady (connecting: " + ((PlayerRegistry.ConnectingUsers != null) ? PlayerRegistry.ConnectingUsers.Count : 0) + ")");
			UnityAction unityAction = onAllPlayersReady;
			Reset();
			unityAction();
		}
		else
		{
			Log.NetworkWarning("Error proceeding. No callback method found.");
		}
	}

	private bool UnreadyPlayer(NetworkPlayer player, bool isValidatedAction = false, bool autoValidateAction = false, bool force = false)
	{
		if (ScenarioRuleClient.IsHandStateMessagePending && !force)
		{
			Log.Info("Ignoring unready final callback (after progress bar) because a player is about to send his hand/short rest state");
			toggle.SetIsOnWithoutNotify(PlayersReady.Contains(PlayerRegistry.MyPlayer));
			return false;
		}
		if (FFSNetwork.IsHost && waitingForReadyCheckResponses)
		{
			return false;
		}
		if (!PlayersReady.Exists((NetworkPlayer e) => e.PlayerID == player.PlayerID))
		{
			return false;
		}
		Log.GUIInfo("Unready player " + player.Username);
		if (playersAwaited.Contains(player))
		{
			playersAwaited.Remove(player);
		}
		if (player.Equals(PlayerRegistry.MyPlayer))
		{
			if (!isValidatedAction)
			{
				if (FFSNetwork.IsClient)
				{
					SetInteractable(interactable: false);
				}
				Synchronizer.SendGameAction(GameActionType.UnreadyPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: true, disableAutoReplication: false, 0, 0, 0, 0, autoValidateAction);
			}
		}
		else if (FFSNetwork.IsHost && !autoValidateAction && ((readyUpType == EReadyUpType.Participant && PlayerRegistry.MyPlayer.IsParticipant && PlayersReady.Count >= PlayerRegistry.Participants.Count) || (readyUpType == EReadyUpType.Player && PlayersReady.Count >= PlayerRegistry.AllPlayers.Count)))
		{
			return false;
		}
		if (FFSNetwork.IsHost | isValidatedAction)
		{
			if (PlayersReady.Contains(player))
			{
				Timing.KillCoroutines(player.PlayerID.ToString());
				if (FFSNetwork.IsHost)
				{
					Log.NetworkInfo("Cleared Player ACKed players for Player with ID: " + player.PlayerID.ToString());
					player.PlayersACKedMyLatestControllableState.Clear();
					Timing.KillCoroutines(stateSyncCheckRoutine);
					waitingForReadyCheckResponses = false;
				}
				PlayersReady.Remove(player);
				PlayerReadiedState.Remove(player);
				onReadiedPlayersChanged?.Invoke(player, arg1: false);
				Log.NetworkInfo("Players Ready Count: " + PlayersReady.Count + ": " + string.Join(",", PlayersReady.Select((NetworkPlayer it) => it.Username)));
			}
			if (player.Equals(PlayerRegistry.MyPlayer))
			{
				SetInteractable(interactable: true);
				toggle.SetIsOnWithoutNotify(value: false);
				buttonText.SetTextKey(readyTextLoc);
				onUnready?.Invoke();
			}
		}
		return true;
	}

	public void UpdateAutoEndSelection()
	{
		UpdateAutoEndSelection(IsVisible, IsInteractable);
	}

	private void UpdateAutoEndSelection(bool wasVisible, bool wasInteractable)
	{
		if (!GameState.s_EnableAutoEndSelectionAfterShortRest)
		{
			AutoEndSelection = EAutoEndSelection.NA;
			ShowRequestChangeCardsButton(show: false);
			return;
		}
		if (AutoEndSelection == EAutoEndSelection.NA)
		{
			if (CanRequestChangeCards())
			{
				AutoEndSelection = EAutoEndSelection.Auto;
				ShowRequestChangeCardsButton(show: true);
			}
			else
			{
				AutoEndSelection = EAutoEndSelection.NA;
			}
		}
		ShowRequestChangeCardsButton(AutoEndSelection != EAutoEndSelection.NA);
		if (!IsVisible || !IsInteractable || (wasVisible & wasInteractable))
		{
			return;
		}
		if (AutoEndSelection != EAutoEndSelection.Auto)
		{
			AutoEndSelection = EAutoEndSelection.NA;
			return;
		}
		AutoEndSelection = EAutoEndSelection.NA;
		if (!ToggledOn)
		{
			ReadyUp(toggledOn: true);
		}
	}

	private void ShowRequestChangeCardsButton(bool show)
	{
	}

	public bool CanRequestChangeCards()
	{
		if (PhaseManager.PhaseType != CPhase.PhaseType.ShortRest)
		{
			return false;
		}
		if (ScenarioManager.Scenario?.AllPlayers == null)
		{
			return false;
		}
		bool result = false;
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			if (allPlayer.IsUnderMyControl())
			{
				result = true;
				if (allPlayer.CharacterClass.RoundAbilityCards.Count < allPlayer.MaxRoundCardsCount && !allPlayer.CharacterClass.LongRest)
				{
					return false;
				}
			}
		}
		return result;
	}

	public void RequestChangeCards(bool changeCards)
	{
		if (AutoEndSelection != EAutoEndSelection.NA)
		{
			AutoEndSelection = (changeCards ? EAutoEndSelection.Manual : EAutoEndSelection.Auto);
		}
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		bool flag = PlayersReady.Remove(player);
		PlayerReadiedState.Remove(player);
		if (FFSNetwork.IsHost)
		{
			PlayersReady.ForEach((NetworkPlayer x) =>
			{
				x.PlayersACKedMyLatestControllableState.Remove(player);
			});
		}
		if (playersAwaited.Contains(player))
		{
			playersAwaited.Remove(player);
		}
		if (!validateReadyUpOnPlayerLeft)
		{
			return;
		}
		if (flag)
		{
			onReadiedPlayersChanged?.Invoke(player, arg1: false);
			if (player.Equals(PlayerRegistry.MyPlayer))
			{
				onUnready?.Invoke();
			}
		}
		if (FFSNetwork.IsHost && ((readyUpType == EReadyUpType.Participant && PlayerRegistry.MyPlayer.IsParticipant && PlayersReady.Count >= PlayerRegistry.Participants.Count && PlayersReady.Contains(PlayerRegistry.MyPlayer)) || (readyUpType == EReadyUpType.Player && PlayersReady.Count >= PlayerRegistry.AllPlayers.Count)) && playersAwaited.Count == 0)
		{
			stateSyncCheckRoutine = Timing.RunCoroutine(WaitForStateSyncBeforeProceeding());
		}
	}

	public void ClientProceed()
	{
		Log.NetworkInfo("Proceeding to the next phase.");
		Proceed();
	}

	public void ProxySetReadyState(GameAction action, ref bool forwardAction)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			if (action.ActionTypeID == 20)
			{
				ReadyUpToken readyUpToken = (ReadyUpToken)action.SupplementaryDataToken2;
				forwardAction = ReadyUpPlayer(player, (EReadyUpToggleStates)Enum.Parse(typeof(EReadyUpToggleStates), readyUpToken.ToggleState), force: true);
				if (FFSNetwork.IsHost & forwardAction)
				{
					action.SupplementaryDataToken = new ControllableStateRevisionToken(player);
				}
			}
			else
			{
				forwardAction = UnreadyPlayer(player, isValidatedAction: true, action.SupplementaryDataBoolean, force: true);
			}
			return;
		}
		throw new Exception("Error readying up a proxy playerActor. The player does not exist.");
	}

	public void ClientCheckPlayerControllableStateRevisionsMatch(GameAction action)
	{
		if (FFSNetwork.IsClient)
		{
			NetworkPlayer player = PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax);
			Synchronizer.SendGameAction(GameActionType.StatesSynchronized, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, 0, 0, 0, player.PlayerID.ID);
		}
	}

	private IEnumerator<float> WaitUntilRevisionsMatch(NetworkPlayer targetPlayer, ControllableStateRevisionToken latestRevision)
	{
		float timeOutTime = Timekeeper.instance.m_GlobalClock.time + clientStateSyncTimeoutDuration;
		while (!latestRevision.IsSameRevision(targetPlayer) || ActionProcessor.IsProcessing)
		{
			if (Timekeeper.instance.m_GlobalClock.time > timeOutTime)
			{
				UIMultiplayerNotifications.ShowPossibleTimeout();
			}
			yield return Timing.WaitForSeconds(stateSyncCheckInterval);
		}
		UIMultiplayerNotifications.HidePossibleTimeout();
		Synchronizer.SendGameAction(GameActionType.StatesSynchronized, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, 0, 0, 0, targetPlayer.PlayerID.ID);
	}

	public void ServerACKSyncedStateRevision(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax);
		if (player != null)
		{
			NetworkPlayer player2 = PlayerRegistry.GetPlayer(action.PlayerID);
			if (player2 != null)
			{
				player.PlayersACKedMyLatestControllableState.Add(player2);
				Log.NetworkInfo("Player " + player2.Username + "(PlayerID: " + player2.PlayerID.ToString() + ") ACKed having just acquired the latest states for " + player.Username + "'s controllables (PlayerID: " + player.PlayerID.ToString() + ").");
			}
			else
			{
				Log.NetworkWarning("Error ACKing synced state revision. ACKed player does not exist (PlayerID: " + action.PlayerID + ").");
			}
		}
		else
		{
			Log.NetworkWarning("Error ACKing synced state revision. Controlling player does not exist (PlayerID: " + action.SupplementaryDataIDMax + ").");
		}
	}

	public void ClientCheckPlayerControllableStateRevisionsMatchNew(GameAction action)
	{
		if (FFSNetwork.IsClient)
		{
			Synchronizer.SendGameAction(GameActionType.StatesSynchronizedNew, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, 0, 0, 0, PlayerRegistry.MyPlayer.PlayerID.ID);
		}
	}

	public void ServerACKSyncedStateRevisionNew(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax);
		if (player != null)
		{
			foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
			{
				allPlayer.PlayersACKedMyLatestControllableState.Add(player);
			}
			return;
		}
		Log.NetworkWarning("Error ACKing synced state revision. Controlling player does not exist (PlayerID: " + action.SupplementaryDataIDMax + ").");
	}
}

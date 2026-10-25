using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MEC;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FFSNet;

public static class ActionProcessor
{
	public struct ProcessorState
	{
		public ActionProcessorStateType StateType { get; private set; }

		public ActionPhaseType PhaseType { get; private set; }

		public ProcessorState(ActionProcessorStateType stateType, ActionPhaseType phaseType)
		{
			StateType = stateType;
			PhaseType = phaseType;
		}

		public override string ToString()
		{
			return $"{StateType}, {PhaseType}";
		}
	}

	private static Queue<GameAction> actionQueue = new Queue<GameAction>();

	private static ProcessorState currentState;

	private static ProcessorState previousState;

	private static ProcessorState savedState;

	private static bool readyToProcessNextAction;

	private static bool processingAction;

	private static CoroutineHandle actionProcessingRoutine;

	private static bool processingRoutineRunning;

	private static int incorrectActionsDetectedCounter;

	public static int MaxConsecutiveIncorrectActionsAllowed { get; set; }

	public static bool IsProcessing => processingAction;

	public static ActionPhaseType CurrentPhase => currentState.PhaseType;

	public static bool HasSavedState => !savedState.Equals(default(ProcessorState));

	public static bool HasSavedStateInSamePhase
	{
		get
		{
			if (!savedState.Equals(default(ProcessorState)))
			{
				return savedState.PhaseType == CurrentPhase;
			}
			return false;
		}
	}

	public static GameAction IsWaitingForTileSelectionFinishedMessage { get; set; }

	public static float QueueDelay { get; set; }

	public static ProcessorState CurrentState => currentState;

	public static ProcessorState PreviousState => previousState;

	public static ProcessorState SavedState => savedState;

	public static bool ReadyToProcessNextAction
	{
		get
		{
			if (readyToProcessNextAction)
			{
				if (SceneController.Instance.IsLoading)
				{
					return global::Singleton<FHMapChoreographer>.InstanceFast != null;
				}
				return true;
			}
			return false;
		}
	}

	public static Queue<GameAction> ActionQueue => actionQueue;

	public static void SetState(ActionProcessorStateType stateType, ActionPhaseType phaseType = ActionPhaseType.NONE, bool savePreviousState = false, bool processQueueImmediately = false)
	{
		if (stateType == currentState.StateType && phaseType == currentState.PhaseType)
		{
			return;
		}
		Log.NetworkInfo($"Switching Action Processor state from {currentState} to {stateType}, {phaseType}, savePreviousState: {savePreviousState}{Log.AddStackTraceForLog()}");
		previousState = currentState;
		currentState = ((stateType == ActionProcessorStateType.SwitchBackToSavedState) ? savedState : new ProcessorState(stateType, phaseType));
		if (stateType == ActionProcessorStateType.SwitchBackToSavedState)
		{
			savedState = default;
		}
		else if (savePreviousState && stateType != ActionProcessorStateType.SwitchBackToSavedState)
		{
			savedState = previousState;
		}
		if (currentState.StateType != ActionProcessorStateType.ProcessFreely && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !ScenarioRuleClient.ScenarioRuleClientStopped)
		{
			Log.Error($"Attempting to switch Action Processor state from {previousState} to {currentState} during SelectAbilityCardsOrLongRest!");
			currentState = new ProcessorState(ActionProcessorStateType.ProcessFreely, currentState.PhaseType);
		}
		Log.NetworkInfo($"[Core] Action Processor STATE: {currentState.StateType} @ {currentState.PhaseType}");
		if (currentState.StateType.In(ActionProcessorStateType.ProcessFreely, ActionProcessorStateType.ProcessOneAndHalt, ActionProcessorStateType.ProcessOneAndSwitchBack))
		{
			readyToProcessNextAction = true;
			if (FFSNetwork.IsOnline)
			{
				StartUpActionProcessing(processQueueImmediately);
			}
		}
		else if (currentState.StateType == ActionProcessorStateType.Halted)
		{
			readyToProcessNextAction = false;
		}
		else
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00014", "Cannot process state: " + currentState.StateType);
		}
	}

	public static void SetStateOnControllerChanged(GameAction action)
	{
		if (action.ActionTypeID == 78 && action.DataInt != 0)
		{
			if (CurrentState.StateType != ActionProcessorStateType.NONE)
			{
				Log.NetworkInfo("[TEST] Sending SendBackActionProcessorState.  MyPlayer = " + PlayerRegistry.MyPlayer.PlayerID.ToString() + " Other Player = " + action.PlayerID + " State = " + CurrentState.StateType);
				Synchronizer.SendSideAction(GameActionType.SendBackActionProcessorState, null, canBeUnreliable: false, sendToHostOnly: false, action.PlayerID, (int)CurrentState.StateType);
			}
			SetState(action);
		}
		else
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00014", "Incorrect Values sent to SetStateOnControllerChanged.  Action Type '" + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + "' State = '" + (ActionProcessorStateType)action.DataInt/*cast due to constrained. prefix*/);
		}
	}

	public static void SetState(GameAction action)
	{
		if ((action.ActionTypeID == 78 || action.ActionTypeID == 79) && action.DataInt != 0)
		{
			currentState = new ProcessorState((ActionProcessorStateType)action.DataInt, currentState.PhaseType);
			Log.NetworkInfo($"[Core] STATE: {currentState.StateType} @ {currentState.PhaseType}");
			if (currentState.StateType.In(ActionProcessorStateType.ProcessFreely, ActionProcessorStateType.ProcessOneAndHalt, ActionProcessorStateType.ProcessOneAndSwitchBack))
			{
				readyToProcessNextAction = true;
				if (FFSNetwork.IsOnline)
				{
					StartUpActionProcessing(processQueueImmediately: false);
				}
			}
			else if (currentState.StateType == ActionProcessorStateType.Halted)
			{
				readyToProcessNextAction = false;
			}
			else
			{
				Log.NetworkError("ERROR_MULTIPLAYER_00014", "Cannot process state: " + currentState.StateType);
			}
		}
		else
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00014", "Incorrect Values sent to SetStateOnControllerChanged.  Action Type '" + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + "' State = '" + (ActionProcessorStateType)action.DataInt/*cast due to constrained. prefix*/);
		}
	}

	public static void SaveCurrentState()
	{
		savedState = currentState;
	}

	public static void ClearSavedState()
	{
		savedState = default;
	}

	public static void ProcessSideAction(GameAction action)
	{
		GameActionType actionTypeID = (GameActionType)action.ActionTypeID;
		if (PlayerRegistry.MyPlayer == null || (FFSNetwork.IsClient && !PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment))
		{
			if (actionTypeID != GameActionType.EndOfRoundAllPlayersReady && actionTypeID != GameActionType.EndOfTurnAllPlayersReady && actionTypeID != GameActionType.EndOfAbilityAllPlayersReady && actionTypeID != GameActionType.StateHash && actionTypeID != GameActionType.NotifyLoadingFinished && actionTypeID != GameActionType.GameLoadedAndHostReady && actionTypeID != GameActionType.LogReproStep && actionTypeID != GameActionType.ConsoleCommand)
			{
				ActionProcessorCoroutineHelper.RunCoroutine(ProcessSideActionCoroutine(action));
			}
			if (actionTypeID != GameActionType.NotifyLoadingFinished && actionTypeID != GameActionType.GameLoadedAndHostReady && actionTypeID != GameActionType.LogReproStep && actionTypeID != GameActionType.ConsoleCommand)
			{
				return;
			}
		}
		if (actionTypeID == GameActionType.FastUndoToCheckpoint && (!FastReload.HasLastCheckpoint(action.SupplementaryDataString) || !FastReload.CanUndo()))
		{
			ActionProcessorCoroutineHelper.RunCoroutine(ProcessFastUndoActionCoroutine(action));
			return;
		}
		try
		{
			if (action.TargetPlayerID == 0 || action.TargetPlayerID == PlayerRegistry.MyPlayer.PlayerID.ID)
			{
				Log.NetworkInfo("Processing SideAction (" + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + "), DataTOKEN: " + ((action.SupplementaryDataToken != null) ? "YES" : "NO"));
				action.Execute();
				return;
			}
			Log.NetworkInfo("Ignoring SideAction (" + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + ") as TargetPlayerID does not match. TargetPlayerID: " + action.TargetPlayerID + " My PlayerID: " + PlayerRegistry.MyPlayer.PlayerID.ToString());
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
			throw;
		}
	}

	private static IEnumerator ProcessSideActionCoroutine(GameAction action)
	{
		while (FFSNetwork.IsOnline && (PlayerRegistry.MyPlayer == null || (FFSNetwork.IsClient && !PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment)))
		{
			yield return null;
		}
		if (FFSNetwork.IsOnline)
		{
			ProcessSideAction(action);
		}
	}

	private static IEnumerator ProcessFastUndoActionCoroutine(GameAction action)
	{
		int incorrectCnt = 0;
		do
		{
			if (Choreographer.s_Choreographer.m_MessageQueue.Count > 0 || ScenarioRuleClient.IsProcessingOrMessagesQueued)
			{
				yield return new WaitForEndOfFrame();
			}
			if (FastReload.IndexInHistory <= 0)
			{
				yield break;
			}
			incorrectCnt++;
			if (incorrectCnt >= MaxConsecutiveIncorrectActionsAllowed)
			{
				NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
				NetworkPlayer myPlayer = PlayerRegistry.MyPlayer;
				string text = ((!FFSNetwork.Manager.DisplayControllableNameForIncorrectActionErrors) ? string.Empty : ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject?.GetName());
				Log.ErrorWithReport("Error processing action. Timed out after " + FFSNetwork.Manager.IncorrectActionDetectedTimeOutDuration + " seconds waiting for the expected checkpoint to undo to." + PlayerRegistry.Participants.Count + "-player game. Acting Player: " + ((player != null) ? (player.Username + " (ID: " + player.PlayerID.ToString() + ", " + (player.IsClient ? "CLIENT" : "HOST") + ((!text.IsNullOrEmpty()) ? (", " + text) : "") + ")") : "NULL") + ", Expected checkpoint: " + action.SupplementaryDataString + ". My Player: " + ((myPlayer != null) ? (myPlayer.Username + " (ID: " + myPlayer.PlayerID.ToString() + ", " + (myPlayer.IsClient ? "CLIENT" : "HOST") + ")") : "NULL") + ", Last checkpoint: " + FastReload.History?.LastOrDefault()?.name + ".");
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
		while (!FastReload.HasLastCheckpoint(action.SupplementaryDataString) || !FastReload.CanUndo());
		ProcessSideAction(action);
	}

	public static void QueueUpAction(GameAction action, bool processQueueImmediately = false)
	{
		if (QueueDelay > 0f)
		{
			ActionProcessorCoroutineHelper.RunCoroutine(QueueUpActionAfterDelay(action, processQueueImmediately));
			return;
		}
		actionQueue.Enqueue(action);
		Log.NetworkInfo("New action queued. Queue size: " + actionQueue.Count);
		StartUpActionProcessing(processQueueImmediately);
	}

	private static IEnumerator QueueUpActionAfterDelay(GameAction action, bool processQueueImmediately = false)
	{
		Log.NetworkInfo("Waiting to add new action queued. Queue size: " + actionQueue.Count);
		yield return new WaitForSeconds(QueueDelay);
		actionQueue.Enqueue(action);
		Log.NetworkInfo("New action queued. Queue size: " + actionQueue.Count);
		StartUpActionProcessing(processQueueImmediately);
	}

	private static void StartUpActionProcessing(bool processQueueImmediately)
	{
		if ((!processQueueImmediately || !TryProcessNextAction()) && !processingRoutineRunning)
		{
			actionProcessingRoutine = Timing.RunCoroutine(TryProcessActions());
		}
	}

	private static IEnumerator<float> TryProcessActions()
	{
		yield return float.NegativeInfinity;
		if (FFSNetwork.IsOnline)
		{
			Log.NetworkInfo("Starting a new action processing coroutine.");
		}
		processingRoutineRunning = true;
		while ((ReadyToProcessNextAction || actionQueue.Count > 0) && !FFSNetwork.HasDesynchronized)
		{
			if (TryProcessNextAction() && actionQueue.Count > 0)
			{
				yield return Timing.WaitForSeconds(1f);
			}
			else
			{
				yield return Timing.WaitForSeconds(FFSNetwork.Manager.ActionQueueProcessingInterval);
			}
		}
		processingRoutineRunning = false;
	}

	private static bool TryProcessNextAction()
	{
		try
		{
			if (ReadyToProcessNextAction && actionQueue.Count > 0 && !processingAction)
			{
				GameAction action = actionQueue.Peek();
				if (action == null)
				{
					throw new Exception("Error processing action. Action returns null.");
				}
				if (PlayerRegistry.GetPlayer(action.PlayerID) == null)
				{
					Log.NetworkInfo("[Core] Attempting to process message from NULL player (PlayerID " + action.PlayerID + ").  Skipping this action");
					actionQueue.Dequeue();
					return true;
				}
				GameActionType actionTypeID = (GameActionType)action.ActionTypeID;
				ActionPhaseType actionPhaseType = (ActionPhaseType)action.TargetPhaseID;
				if (actionTypeID == GameActionType.PlaceCharacter && FFSNetwork.IsHost)
				{
					bool flag = Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForCardSelection && global::Singleton<CardsHandManager>.Instance.IsActive() && WorldspaceStarHexDisplay.Instance.CurrentDisplayState == WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.CharacterPlacement;
					if (CurrentPhase != ActionPhaseType.StartOfRound || !flag)
					{
						actionQueue.Dequeue();
						return true;
					}
				}
				if (actionTypeID == GameActionType.TakeDamage)
				{
					if (!global::Singleton<TakeDamagePanel>.Instance.IsValid())
					{
						incorrectActionsDetectedCounter++;
						Log.NetworkWarning($"Incorrect action detected (check first occurrence). Counter: {incorrectActionsDetectedCounter}/{MaxConsecutiveIncorrectActionsAllowed}");
						if (incorrectActionsDetectedCounter >= MaxConsecutiveIncorrectActionsAllowed)
						{
							NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
							NetworkPlayer myPlayer = PlayerRegistry.MyPlayer;
							string text = ((!FFSNetwork.Manager.DisplayControllableNameForIncorrectActionErrors) ? string.Empty : ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject?.GetName());
							throw new Exception("Error processing action. Timed out after " + FFSNetwork.Manager.IncorrectActionDetectedTimeOutDuration + " seconds waiting for take damage UI to appear in a " + PlayerRegistry.Participants.Count + "-player game. Acting Player: " + ((player != null) ? (player.Username + " (ID: " + player.PlayerID.ToString() + ", " + (player.IsClient ? "CLIENT" : "HOST") + ((!text.IsNullOrEmpty()) ? (", " + text) : "") + ")") : "NULL") + ", Action: " + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + ", TargetPhase: " + ((ActionPhaseType)action.TargetPhaseID/*cast due to constrained. prefix*/).ToString() + ". My Player: " + ((myPlayer != null) ? (myPlayer.Username + " (ID: " + myPlayer.PlayerID.ToString() + ", " + (myPlayer.IsClient ? "CLIENT" : "HOST") + ")") : "NULL") + ", Phase: " + currentState.PhaseType.ToString() + ".");
						}
						return false;
					}
					incorrectActionsDetectedCounter = 0;
				}
				if (actionTypeID == GameActionType.ChooseAbility)
				{
					if (!UIUseAbilitiesBar.ReadyToProxyChooseAbility(action))
					{
						incorrectActionsDetectedCounter++;
						Log.NetworkWarning($"Incorrect action detected (check first occurrence). Counter: {incorrectActionsDetectedCounter}/{MaxConsecutiveIncorrectActionsAllowed}");
						if (incorrectActionsDetectedCounter >= MaxConsecutiveIncorrectActionsAllowed)
						{
							NetworkPlayer player2 = PlayerRegistry.GetPlayer(action.PlayerID);
							NetworkPlayer myPlayer2 = PlayerRegistry.MyPlayer;
							string text2 = ((!FFSNetwork.Manager.DisplayControllableNameForIncorrectActionErrors) ? string.Empty : ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject?.GetName());
							throw new Exception("Error processing action. Timed out after " + FFSNetwork.Manager.IncorrectActionDetectedTimeOutDuration + " seconds waiting for choice UI to appear in a " + PlayerRegistry.Participants.Count + "-player game. Acting Player: " + ((player2 != null) ? (player2.Username + " (ID: " + player2.PlayerID.ToString() + ", " + (player2.IsClient ? "CLIENT" : "HOST") + ((!text2.IsNullOrEmpty()) ? (", " + text2) : "") + ")") : "NULL") + ", Action: " + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + ", TargetPhase: " + ((ActionPhaseType)action.TargetPhaseID/*cast due to constrained. prefix*/).ToString() + ". My Player: " + ((myPlayer2 != null) ? (myPlayer2.Username + " (ID: " + myPlayer2.PlayerID.ToString() + ", " + (myPlayer2.IsClient ? "CLIENT" : "HOST") + ")") : "NULL") + ", Phase: " + currentState.PhaseType.ToString() + ".");
						}
						return false;
					}
					incorrectActionsDetectedCounter = 0;
				}
				if (currentState.PhaseType == ActionPhaseType.StartOfRound || actionPhaseType == ActionPhaseType.StartOfRound)
				{
					actionPhaseType = ActionPhaseType.NONE;
					action.TargetPhaseID = (int)actionPhaseType;
				}
				if (actionPhaseType == ActionPhaseType.ScenarioEnded && (currentState.PhaseType == ActionPhaseType.MapHQ || currentState.PhaseType == ActionPhaseType.MapAtLinkedScenario))
				{
					actionQueue.Dequeue();
					return true;
				}
				if ((actionTypeID == GameActionType.SelectTarget || actionTypeID == GameActionType.DeselectTarget) && actionPhaseType == ActionPhaseType.SecondAction && currentState.PhaseType == ActionPhaseType.TargetSelection)
				{
					actionPhaseType = ActionPhaseType.TargetSelection;
					action.TargetPhaseID = (int)actionPhaseType;
				}
				if ((actionTypeID == GameActionType.SelectTarget || actionTypeID == GameActionType.DeselectTarget) && actionPhaseType == ActionPhaseType.ThirdAction && currentState.PhaseType == ActionPhaseType.TargetSelection)
				{
					actionPhaseType = ActionPhaseType.TargetSelection;
					action.TargetPhaseID = (int)actionPhaseType;
				}
				if (actionPhaseType == ActionPhaseType.MapHQ && currentState.PhaseType == ActionPhaseType.MapAtLinkedScenario)
				{
					actionPhaseType = ActionPhaseType.MapAtLinkedScenario;
					action.TargetPhaseID = (int)actionPhaseType;
				}
				if (actionPhaseType == ActionPhaseType.MapAtLinkedScenario && currentState.PhaseType == ActionPhaseType.MapHQ)
				{
					actionPhaseType = ActionPhaseType.MapHQ;
					action.TargetPhaseID = (int)actionPhaseType;
				}
				if ((actionTypeID == GameActionType.CancelAbilityAugment || actionTypeID == GameActionType.AugmentCardAbility || actionTypeID == GameActionType.SelectTarget || actionTypeID == GameActionType.DeselectTarget || actionTypeID == GameActionType.UseItem || actionTypeID == GameActionType.CreateWaypoint) && actionPhaseType != currentState.PhaseType)
				{
					if (Choreographer.s_Choreographer.m_MessageQueue.Count > 0 || ScenarioRuleClient.IsProcessingOrMessagesQueued)
					{
						return false;
					}
					actionQueue.Dequeue();
					return false;
				}
				if (currentState.PhaseType == ActionPhaseType.ScenarioEnded && actionPhaseType != ActionPhaseType.ScenarioEnded)
				{
					actionQueue.Dequeue();
					return true;
				}
				if (currentState.PhaseType == ActionPhaseType.StartOfRound && actionPhaseType == ActionPhaseType.MapLoadoutScreen && (actionTypeID == GameActionType.StatesSynchronized || actionTypeID == GameActionType.ReadyUpPlayer || actionTypeID == GameActionType.UnreadyPlayer))
				{
					actionQueue.Dequeue();
					return true;
				}
				if (currentState.PhaseType != ActionPhaseType.StartOfRound && actionPhaseType == ActionPhaseType.StartOfRound && (actionTypeID == GameActionType.ReadyUpPlayer || actionTypeID == GameActionType.UnreadyPlayer))
				{
					actionQueue.Dequeue();
					return true;
				}
				if (actionTypeID == GameActionType.StatesSynchronized && actionPhaseType != CurrentPhase && actionPhaseType != ActionPhaseType.NONE)
				{
					NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID.ID == action.TargetPlayerID);
					if (networkPlayer == null || !networkPlayer.IsActive)
					{
						actionQueue.Dequeue();
						return true;
					}
				}
				if (currentState.PhaseType == ActionPhaseType.MapLoadoutScreen && actionPhaseType == ActionPhaseType.MapHQ && (actionTypeID == GameActionType.StatesSynchronized || actionTypeID == GameActionType.ReadyUpPlayer || actionTypeID == GameActionType.UnreadyPlayer))
				{
					actionQueue.Dequeue();
					return true;
				}
				if ((actionTypeID == GameActionType.ReadyUpPlayer || actionTypeID == GameActionType.UnreadyPlayer) && FFSNetwork.IsClient && action.TargetPhaseID != (int)currentState.PhaseType && action.TargetPhaseID != 0)
				{
					actionQueue.Dequeue();
					return true;
				}
				if (actionTypeID == GameActionType.ConfirmAction && ((ScenarioRuleClient.IsProcessingOrMessagesQueued && !GameState.WaitingForPlayerToSelectDamageResponse && !GameState.WaitingForPlayerActorToAvoidDamageResponse && !GameState.WaitingForPlayerActorToContinueAbility) || Choreographer.s_Choreographer.m_MessageQueue.Count > 0))
				{
					Log.NetworkWarning("Unable to process ConfirmAction until SRL and choreographer are done processing their messages");
					return false;
				}
				if (actionTypeID == GameActionType.StatesSynchronized && action.TargetPhaseID != (int)currentState.PhaseType)
				{
					action.TargetPhaseID = 0;
				}
				if (action.TargetPhaseID != (int)currentState.PhaseType && action.TargetPhaseID != 0)
				{
					incorrectActionsDetectedCounter++;
					if (incorrectActionsDetectedCounter == 1)
					{
						Log.NetworkWarning($"Incorrect action detected (Action: {(GameActionType)action.ActionTypeID} | TargetPhase: {(ActionPhaseType)action.TargetPhaseID} | CurrentPhase: {currentState.PhaseType}). Desync countdown started.");
					}
					else
					{
						Log.NetworkWarning($"Incorrect action detected (check first occurrence). Counter: {incorrectActionsDetectedCounter}/{MaxConsecutiveIncorrectActionsAllowed}");
					}
					if (incorrectActionsDetectedCounter >= MaxConsecutiveIncorrectActionsAllowed)
					{
						NetworkPlayer player3 = PlayerRegistry.GetPlayer(action.PlayerID);
						NetworkPlayer myPlayer3 = PlayerRegistry.MyPlayer;
						string text3 = ((!FFSNetwork.Manager.DisplayControllableNameForIncorrectActionErrors) ? string.Empty : ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject?.GetName());
						throw new Exception("Error processing action. Timed out after " + FFSNetwork.Manager.IncorrectActionDetectedTimeOutDuration + " seconds waiting for a suitable phase to play the action in a " + PlayerRegistry.Participants.Count + "-player game. Acting Player: " + ((player3 != null) ? (player3.Username + " (ID: " + player3.PlayerID.ToString() + ", " + (player3.IsClient ? "CLIENT" : "HOST") + ((!text3.IsNullOrEmpty()) ? (", " + text3) : "") + ")") : "NULL") + ", Action: " + ((GameActionType)action.ActionTypeID/*cast due to constrained. prefix*/).ToString() + ", TargetPhase: " + ((ActionPhaseType)action.TargetPhaseID/*cast due to constrained. prefix*/).ToString() + ". My Player: " + ((myPlayer3 != null) ? (myPlayer3.Username + " (ID: " + myPlayer3.PlayerID.ToString() + ", " + (myPlayer3.IsClient ? "CLIENT" : "HOST") + ")") : "NULL") + ", Phase: " + currentState.PhaseType.ToString() + ".");
					}
					return false;
				}
				incorrectActionsDetectedCounter = 0;
				ProcessAction(action);
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
			return false;
		}
	}

	private static bool ProcessAction(GameAction action)
	{
		Log.NetworkInfo("Processing action STARTED.");
		processingAction = true;
		actionQueue.Dequeue();
		if (currentState.StateType == ActionProcessorStateType.ProcessOneAndHalt)
		{
			SetState(ActionProcessorStateType.Halted);
		}
		Log.NetworkInfo($"[Core] Processing GameAction {action}");
		if (action.Execute())
		{
			FinishProcessingAction(action);
			return true;
		}
		return false;
	}

	public static void FinishProcessingAction(GameAction action)
	{
		if (!FFSNetwork.IsOnline)
		{
			return;
		}
		if (FFSNetwork.IsHost && action.IsValid)
		{
			if (!action.DoNotForwardAction)
			{
				Synchronizer.ForwardGameActionToClients(action);
			}
			DesyncDetector.RequestDelayedCheckOnHost(aggressive: true);
		}
		processingAction = false;
		Log.NetworkInfo($"[Core] Finished processing GameAction {action}. Remaining in queue: {actionQueue.Count}");
		if (currentState.StateType == ActionProcessorStateType.ProcessOneAndSwitchBack)
		{
			Log.NetworkInfo("Switching back to previous state.");
			SetState(previousState.StateType, previousState.PhaseType);
		}
	}

	public static void OnRestartRound()
	{
		actionQueue.Clear();
		currentState = new ProcessorState(ActionProcessorStateType.Halted, ActionPhaseType.StartOfRound);
		previousState = default;
		savedState = default;
		Timing.KillCoroutines(actionProcessingRoutine);
		ActionProcessorCoroutineHelper.StopAllCoroutinesOnHelper();
		processingRoutineRunning = false;
		readyToProcessNextAction = false;
	}

	public static void Shutdown()
	{
		currentState = default;
		previousState = default;
		savedState = default;
		actionQueue.Clear();
		Timing.KillCoroutines(actionProcessingRoutine);
		ActionProcessorCoroutineHelper.StopAllCoroutinesOnHelper();
		processingRoutineRunning = false;
		readyToProcessNextAction = false;
		processingAction = false;
		incorrectActionsDetectedCounter = 0;
	}
}

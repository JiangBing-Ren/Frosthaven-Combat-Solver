using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Scenario;
using FHGUI.Scenario.Services;
using UnityEngine;

namespace FFSNet;

public class GameAction
{
	private static readonly Dictionary<GameActionType, GameActionDelegate> actions = new Dictionary<GameActionType, GameActionDelegate>
	{
		{
			GameActionType.NONE,
			delegate
			{
				ShowUninitializedActionErrorMessage();
			}
		},
		{
			GameActionType.PickDifficulty,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				UIDifficultySelector.ClientSetDifficulty(a);
			}
		},
		{
			GameActionType.GameLoadedAndClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIMultiplayerSelectPlayerScreen>.Instance.ServerACKClientLoadedInAndReady(a);
			}
		},
		{
			GameActionType.SwitchCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.MPSwitchCharacter(a, ref v, ref f);
			}
		},
		{
			GameActionType.RemoveCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.MPRemoveCharacter(a, ref v, ref f);
			}
		},
		{
			GameActionType.LevelUp,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ProxyLevelUp(a);
			}
		},
		{
			GameActionType.ModifyPerks,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ProxyModifyPerks(a);
			}
		},
		{
			GameActionType.ModifyCardInventory,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ProxyModifyCardInventory(a);
			}
		},
		{
			GameActionType.EquipItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerEquipItem(a);
			}
		},
		{
			GameActionType.UnequipItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerUnequipItem(a);
			}
		},
		{
			GameActionType.BindAndEquipItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerBindAndEquipItem(a);
			}
		},
		{
			GameActionType.UnbindItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerUnbindItem(a);
			}
		},
		{
			GameActionType.ReadyUpPlayer,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ProxySetReadyState(a, ref v);
			}
		},
		{
			GameActionType.UnreadyPlayer,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ProxySetReadyState(a, ref v);
			}
		},
		{
			GameActionType.AllPlayersReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ClientCheckPlayerControllableStateRevisionsMatch(a);
			}
		},
		{
			GameActionType.StatesSynchronized,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ServerACKSyncedStateRevision(a);
			}
		},
		{
			GameActionType.ReadyProceed,
			delegate
			{
				global::Singleton<UIReadyToggle>.Instance.ClientProceed();
			}
		},
		{
			GameActionType.DebugWinScenario,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ClientDebugWinScenario(a);
			}
		},
		{
			GameActionType.DebugLoseScenario,
			delegate
			{
				Choreographer.s_Choreographer?.ClientDebugLoseScenario();
			}
		},
		{
			GameActionType.AbandonScenario,
			delegate
			{
				Choreographer.s_Choreographer?.ClientAbandonScenario();
			}
		},
		{
			GameActionType.SelectRoundCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxySelectRoundCards(a);
			}
		},
		{
			GameActionType.SelectCardAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxySelectCardAbility(a);
			}
		},
		{
			GameActionType.AugmentCardAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyToggleAugment(a);
			}
		},
		{
			GameActionType.CancelAbilityAugment,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyToggleAugment(a);
			}
		},
		{
			GameActionType.GroupAugmentCardAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugmentationGroup(a);
			}
		},
		{
			GameActionType.CancelAbilityAugmentGroup,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugmentationGroup(a);
			}
		},
		{
			GameActionType.SelectTarget,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxySelectAbilityTarget(a, deselectInstead: false, ref f);
			}
		},
		{
			GameActionType.DeselectTarget,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxySelectAbilityTarget(a, deselectInstead: true, ref f);
			}
		},
		{
			GameActionType.CreateWaypoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Waypoint.ProxySelectTileToCreateMovementPath(a);
			}
		},
		{
			GameActionType.DeleteLastWaypoint,
			delegate
			{
				Waypoint.ProxyDeleteLastWaypoint();
			}
		},
		{
			GameActionType.PressWaypoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Waypoint.ProxyPressWaypoint(a);
			}
		},
		{
			GameActionType.PlaceCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ServerTryAndPlaceCharacterAtRoundStart(a, ref v, ref f);
			}
		},
		{
			GameActionType.UndoAction,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxyUndoAction(a, ref f);
			}
		},
		{
			GameActionType.ClearTargets,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxyUndoAction(a, ref f);
			}
		},
		{
			GameActionType.ConfirmAction,
			delegate
			{
				Choreographer.s_Choreographer?.ProxyConfirmAction();
			}
		},
		{
			GameActionType.SkipAction,
			delegate
			{
				Choreographer.s_Choreographer?.ProxySkipAction();
			}
		},
		{
			GameActionType.TakeDamage,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyTakeDamage(a);
			}
		},
		{
			GameActionType.ConfirmShields,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyConfirmShields(a);
			}
		},
		{
			GameActionType.BurnAvailableCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyBurnAvailableCard(a);
			}
		},
		{
			GameActionType.BurnDiscardedCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyBurnDiscardedCards(a);
			}
		},
		{
			GameActionType.AbilityDiscardCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyAbilityDiscardCard(a);
			}
		},
		{
			GameActionType.AbilityLoseCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyAbilityLoseCard(a);
			}
		},
		{
			GameActionType.AbilityMoveActiveBonusTracker,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyAbilityMoveActiveBonusTracker(a);
			}
		},
		{
			GameActionType.ShortRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyShortRest(a);
			}
		},
		{
			GameActionType.LongRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyLongRest(a);
			}
		},
		{
			GameActionType.DebugAddItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				DebugMenu.Instance.ProxyDebugAddItem(a);
			}
		},
		{
			GameActionType.UseItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseItemsBar>.Instance.ProxyUseItemBonus(a);
			}
		},
		{
			GameActionType.ClickItemBonusSlot,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseItemsBar>.Instance.ProxyUseItemBonus(a);
			}
		},
		{
			GameActionType.ClickActiveBonusSlot,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIActiveBonusBar>.Instance.ProxyUseActiveBonus(a);
			}
		},
		{
			GameActionType.CancelActiveBonus,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyCancelActiveBonuses(a);
			}
		},
		{
			GameActionType.CancelImprovedShortRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyCancelImprovedShortRest(a);
			}
		},
		{
			GameActionType.PickElement,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAbilitiesBar>.Instance.ProxyInfuseAbility(a);
			}
		},
		{
			GameActionType.RecoverCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyRecoverCards(a);
			}
		},
		{
			GameActionType.RefreshItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<ItemCardRefreshPicker>.Instance.ProxyRefreshItems(a);
			}
		},
		{
			GameActionType.ApplySingleTargetEffect,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ApplySingleTargetEffect(a);
			}
		},
		{
			GameActionType.SelectExtraTurnCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxySelectExtraTurnCards(a);
			}
		},
		{
			GameActionType.PingHex,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<PingManager>.Instance?.ProxyPingHex(a);
			}
		},
		{
			GameActionType.ToggleLongRestAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyToggleSelectLongRestAbility(a);
			}
		},
		{
			GameActionType.RedistributeHealthAssign,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyDistributeAssignPopup(a);
			}
		},
		{
			GameActionType.SelectAbilityCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIAbilityCardPicker>.Instance.ProxySelectAbilityCard(a, select: true);
			}
		},
		{
			GameActionType.DeselectAbilityCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIAbilityCardPicker>.Instance.ProxySelectAbilityCard(a, select: false);
			}
		},
		{
			GameActionType.SelectDiscardedCardToPlay,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxySelectDiscardedCardToPlay(a);
			}
		},
		{
			GameActionType.EndOfRoundCompareClientFinished,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ServerACKClientFinishingEndOfRoundComparison(a);
			}
		},
		{
			GameActionType.EndOfRoundCompareComplete,
			delegate
			{
				Choreographer.s_Choreographer?.StateCompareFinished();
			}
		},
		{
			GameActionType.EndOfAbilityClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ServerACKClientReachingEndOfAbility(a);
				}
			}
		},
		{
			GameActionType.EndOfAbilityAllPlayersReady,
			delegate
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ClientProceedToActionSelection();
				}
			}
		},
		{
			GameActionType.EndOfTurnClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ServerACKClientReachingEndOfTurn(a);
				}
			}
		},
		{
			GameActionType.EndOfTurnAllPlayersReady,
			delegate
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ClientProceedToNextTurn();
				}
			}
		},
		{
			GameActionType.EndOfRoundClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ServerACKClientReachingEndOfRound(a);
				}
			}
		},
		{
			GameActionType.EndOfRoundAllPlayersReady,
			delegate
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ClientProceedToNextRound();
				}
			}
		},
		{
			GameActionType.RestartRoundMessage,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxyRestartRoundMessage((SceneController.RestartRoundMode)a.DataInt);
			}
		},
		{
			GameActionType.RestartRound,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.RestartRound((SceneController.RestartRoundMode)a.DataInt, fromNetwork: true);
			}
		},
		{
			GameActionType.FastUndoToCheckpoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxyUndoToCheckpoint(a);
			}
		},
		{
			GameActionType.SendActionProcessorState,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				ActionProcessor.SetStateOnControllerChanged(a);
			}
		},
		{
			GameActionType.SendBackActionProcessorState,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				ActionProcessor.SetState(a);
			}
		},
		{
			GameActionType.ReadyForAssignment,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				SceneController.Instance.PlayerReadyForAssignment(a);
			}
		},
		{
			GameActionType.NotifyClientsPlayersAreJoining,
			delegate
			{
				PlayerRegistry.ToggleOtherClientsAreJoining(value: true);
			}
		},
		{
			GameActionType.NotifyClientsPlayersAreFinishedJoining,
			delegate
			{
				PlayerRegistry.ToggleOtherClientsAreJoining(value: false);
			}
		},
		{
			GameActionType.NotifyLoadingFinished,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				PlayerRegistry.ProxyNotifyLoadingFinished(a);
			}
		},
		{
			GameActionType.AutoSaveOnClientJoined,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				FastReload.ResetOnClientJoined(a.DataInt);
			}
		},
		{
			GameActionType.ChooseAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAbilitiesBar>.Instance.ProxyChooseAbility(a);
			}
		},
		{
			GameActionType.ConsumeItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<ItemCardRefreshPicker>.Instance.ProxyConsumeItems(a);
			}
		},
		{
			GameActionType.AssignSlot,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Service.AssignSlotToPlayer(a);
			}
		},
		{
			GameActionType.DistributeUISelect,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyDistributeSelectPopup(a);
			}
		},
		{
			GameActionType.LoseItemReward,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<ItemRewardLosePicker>.Instance.ProxyItemRewardLose(a);
			}
		},
		{
			GameActionType.DeleteCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				(global::Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI).CampaignCharacterSelector.ProxyDeleteCharacter(a);
			}
		},
		{
			GameActionType.ForceUnreadyUp,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (PlayerRegistry.MyPlayer.PlayerID.ID == a.SupplementaryDataIDMax)
				{
					ReadyUpToken readyUpToken = (ReadyUpToken)a.SupplementaryDataToken;
					EReadyUpToggleStates eReadyUpToggleStates = (EReadyUpToggleStates)Enum.Parse(typeof(EReadyUpToggleStates), readyUpToken.ToggleState);
					EReadyUpToggleStates eReadyUpToggleStates2 = (global::Singleton<UIReadyToggle>.Instance.PlayerReadiedState.ContainsKey(PlayerRegistry.MyPlayer) ? global::Singleton<UIReadyToggle>.Instance.PlayerReadiedState[PlayerRegistry.MyPlayer] : EReadyUpToggleStates.NotSet);
					Log.GUIInfo("Received request to unready state " + eReadyUpToggleStates.ToString() + " (Player state: " + eReadyUpToggleStates2.ToString() + ")");
					if (eReadyUpToggleStates != eReadyUpToggleStates2)
					{
						global::Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
					}
				}
			}
		},
		{
			GameActionType.ClientDesync,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				PlayerRegistry.ProxyClientDesync(a);
			}
		},
		{
			GameActionType.ConfirmReward,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				((CampaignScenarioRewardManager)global::Singleton<ScenarioRewardManager>.Instance).ProxyConfirm(a);
			}
		},
		{
			GameActionType.ChangeHouseRuleSettings,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				HouseRulesSettings.ProxySetHouseRuleSettings(a);
			}
		},
		{
			GameActionType.GameLoadedAndHostReady,
			delegate
			{
				SceneController.Instance.GameLoadedAndHostReady = true;
			}
		},
		{
			GameActionType.RetryScenario,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				UINewAdventureResultsManager.Implementation.ProxyRetryScenario(a);
			}
		},
		{
			GameActionType.ExitScenario,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				UINewAdventureResultsManager.Implementation.ProxyExitScenario(a);
			}
		},
		{
			GameActionType.SendLocalSaveFileQuestCompletion,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
				{
					if (a.SupplementaryDataIDMin != 0)
					{
						PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer it) => it.PlayerID.ID == a.SupplementaryDataIDMin);
					}
					else
					{
						PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer it) => it.PlayerID.ID == a.PlayerID);
					}
					Debug.LogError("Unable to find player with ID for SendLocalSaveFileQuestCompletion: " + ((a.SupplementaryDataIDMin != 0) ? a.SupplementaryDataIDMin : a.PlayerID));
				}
			}
		},
		{
			GameActionType.ShowQuestCompletionImport,
			delegate
			{
			}
		},
		{
			GameActionType.FinishedSendingLocalSaveFileQuestCompletion,
			delegate
			{
				_ = SaveData.Instance.Global.CurrentGameState;
				_ = 1;
			}
		},
		{
			GameActionType.FHMultiplayerCommand,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				FHMapChoreographer.OutOfSceneCommand(a);
			}
		},
		{
			GameActionType.ConsoleCommand,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				DebugMenu.ProxyExecCommand(a);
			}
		},
		{
			GameActionType.StateHash,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				DesyncDetector.OnStateHashReceived(a);
			}
		},
		{
			GameActionType.SelectOutOfPhaseCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxySelectOutOfPhaseCards(a);
			}
		},
		{
			GameActionType.LogReproStep,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				ReproStepsLogger.LogRemoteEvent(a);
			}
		},
		{
			GameActionType.ChangeAgility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyChangeAgility(a);
			}
		},
		{
			GameActionType.ChangePendingForm,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<CardsHandManager>.Instance.ProxyChangePendingForm(a);
			}
		},
		{
			GameActionType.ConfirmationBoxAnswer,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIConfirmationBoxManager>.Instance.ProxyAnswer(a);
			}
		},
		{
			GameActionType.ClientReportingDesyncState,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				DesyncDetector.OnClientReportingDesyncState(a);
			}
		},
		{
			GameActionType.ContinueCrashingTideLongRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxyContinueCrashingTideLongRest(a);
			}
		},
		{
			GameActionType.RequestSyncCheck,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				DesyncDetector.ProxyRequestSyncCheck(a);
			}
		},
		{
			GameActionType.AugmentCardAbilityNoButton,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugmentationNoButton(a);
			}
		},
		{
			GameActionType.CancelAbilityAugmentNoButton,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugmentationNoButton(a);
			}
		},
		{
			GameActionType.CardForModeSelected,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer?.ProxyCardForModeSelected(a);
			}
		},
		{
			GameActionType.SetRegularSpeedMultiplier,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				SaveData.Instance.Global.MultiplayerRegularSpeedMultiplier = (float)a.DataInt / 100f;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
			}
		},
		{
			GameActionType.AllPlayersReadyNew,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ClientCheckPlayerControllableStateRevisionsMatchNew(a);
			}
		},
		{
			GameActionType.StatesSynchronizedNew,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ServerACKSyncedStateRevisionNew(a);
			}
		},
		{
			GameActionType.SetChooseAdvantageModifiersValue,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.SetChooseAdvantageModifiers(a.SupplementaryDataBoolean, force: true);
			}
		},
		{
			GameActionType.SetConfirmAttackAbilityBeforeFinishing,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.SetConfirmAttackAbilityFinished(a.SupplementaryDataBoolean, force: true);
			}
		},
		{
			GameActionType.MoveTrackerSelectedCardsUpdated,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<FHScenarioViewController>.Instance.GetService<ReactionViewService>().Proxy_TrackerOfActiveBonus_UpdateSelectedCards(a);
			}
		},
		{
			GameActionType.FastUndoClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				FastReload.ClientReadyForFastUndo(a);
			}
		},
		{
			GameActionType.FastUndoCheckClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				FastReload.ClientTryConfirmReadyForFastUndo(a);
			}
		},
		{
			GameActionType.HostConfirmsAllClientsReachedSyncPoint,
			delegate
			{
				Choreographer.HostConfirmAllClientsReachedSyncPoint();
			}
		},
		{
			GameActionType.ClientReachedSyncPoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.ClientReachedSyncPoint(a);
			}
		},
		{
			GameActionType.ChangeSelectionPlayerToBurnCardsForAlly,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyChangeSelectionPlayerToBurnCardsForAlly(a);
			}
		},
		{
			GameActionType.ChooseCardsToBurnForAllyMonster,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyChooseCardsToBurnForActor(a);
			}
		},
		{
			GameActionType.ChoosePlayerToBurnCardsForAllyMonster,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyChoosePlayerToBurnCardsForAlly(a, global::Singleton<UIScenarioMultiplayerController>.Instance.damageSummaryToBurnCardForAlly);
			}
		},
		{
			GameActionType.ReceiveDamageInsteadOfNegatingForAllyMonster,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyReceiveDamageInsteadOfNegating(a);
			}
		},
		{
			GameActionType.ChooseIfNegateOrReceiveDamageForAllyMonster,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyShowSelectNegateDamage(a);
			}
		},
		{
			GameActionType.CardToRecoverSelectedCardsUpdated,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<FHScenarioViewController>.Instance.GetService<ReactionViewService>().Proxy_OnSelectCardToRecover_UpdateSelectedCards(a);
			}
		}
	};

	public static GameActionDelegate ExecuteActionHandler;

	private bool isValid = true;

	public GameActionType ActionType => (GameActionType)ActionTypeID;

	public ActionPhaseType TargetPhase => (ActionPhaseType)TargetPhaseID;

	public int ActionID { get; set; }

	public int ActionTypeID { get; private set; }

	public int PlayerID { get; private set; }

	public int ActorID { get; private set; }

	public int TargetPhaseID { get; set; }

	public int TargetPlayerID { get; private set; }

	public int DataInt { get; private set; }

	public int DataInt2 { get; private set; }

	public int SupplementaryDataIDMin { get; set; }

	public int SupplementaryDataIDMed { get; set; }

	public int SupplementaryDataIDMax { get; set; }

	public bool SupplementaryDataBoolean { get; set; }

	public string SupplementaryDataString { get; set; }

	public IProtocolToken SupplementaryDataToken { get; set; }

	public IProtocolToken SupplementaryDataToken2 { get; set; }

	public IProtocolToken SupplementaryDataToken3 { get; set; }

	public IProtocolToken SupplementaryDataToken4 { get; set; }

	public bool ValidateAction { get; private set; }

	public bool DoNotForwardAction { get; private set; }

	public string ClassID { get; private set; }

	public bool ExecuteLocallyFirst { get; private set; }

	public bool IsValid => isValid;

	public static GameAction FHCreate(GameActionType actionType, IProtocolToken token)
	{
		return new GameAction
		{
			ActionTypeID = (int)actionType,
			SupplementaryDataToken = token
		};
	}

	private GameAction()
	{
	}

	public GameAction(GameActionType actionType, ActionPhaseType targetPhaseType = ActionPhaseType.NONE, bool executeLocallyFirst = false, bool validateOnServerBeforeExecuting = false, bool doNotForward = false, int actorID = 0, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, string supplementaryDataString = null, IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null, string classID = null)
	{
		if (PlayerRegistry.MyPlayer != null)
		{
			ActionID = ++PlayerRegistry.MyPlayer.LastActionID;
			DesyncDetector.DelaySyncChecksOnHost();
			DesyncDetector.RequestDelayedCheckOnHost(aggressive: true);
			ActionTypeID = (int)actionType;
			PlayerID = PlayerRegistry.MyPlayer.PlayerID.ID;
			ActorID = actorID;
			TargetPhaseID = (int)targetPhaseType;
			SupplementaryDataIDMin = supplementaryDataIDMin;
			SupplementaryDataIDMed = supplementaryDataIDMed;
			SupplementaryDataIDMax = supplementaryDataIDMax;
			SupplementaryDataBoolean = supplementaryDataBoolean;
			SupplementaryDataString = supplementaryDataString;
			SupplementaryDataToken = supplementaryDataToken;
			SupplementaryDataToken2 = supplementaryDataToken2;
			SupplementaryDataToken3 = supplementaryDataToken3;
			SupplementaryDataToken4 = supplementaryDataToken4;
			ValidateAction = validateOnServerBeforeExecuting;
			DoNotForwardAction = doNotForward;
			ExecuteLocallyFirst = executeLocallyFirst;
			ClassID = classID;
		}
	}

	public GameAction(GameActionEventToken evnt)
	{
		ActionID = evnt.ActionID;
		ActionTypeID = evnt.ActionTypeID;
		PlayerID = evnt.PlayerID;
		ActorID = evnt.ActorID;
		TargetPhaseID = evnt.TargetPhaseID;
		SupplementaryDataIDMin = evnt.SupplementaryDataIDMin;
		SupplementaryDataIDMed = evnt.SupplementaryDataIDMed;
		SupplementaryDataIDMax = evnt.SupplementaryDataIDMax;
		SupplementaryDataBoolean = evnt.SupplementaryDataBoolean;
		SupplementaryDataString = evnt.SupplementaryDataString;
		SupplementaryDataToken = evnt.SupplementaryDataToken;
		SupplementaryDataToken2 = evnt.SupplementaryDataToken2;
		SupplementaryDataToken3 = evnt.SupplementaryDataToken3;
		SupplementaryDataToken4 = evnt.SupplementaryDataToken4;
		ValidateAction = evnt.ValidateAction;
		DoNotForwardAction = evnt.DoNotForwardAction;
	}

	public GameAction(NetworkActionEventToken actionEvent)
	{
		NetworkAction networkAction = (NetworkAction)actionEvent.Token;
		ActionTypeID = networkAction.ActionTypeID;
		PlayerID = networkAction.PlayerID;
		SupplementaryDataToken = networkAction.DataToken;
		TargetPlayerID = networkAction.TargetPlayerID;
		DataInt = networkAction.DataInt;
		DataInt2 = networkAction.DataInt2;
		SupplementaryDataBoolean = networkAction.DataBoolean;
		SupplementaryDataString = networkAction.DataString;
	}

	public GameAction(NetworkAction networkAction)
	{
		ActionTypeID = networkAction.ActionTypeID;
		PlayerID = networkAction.PlayerID;
		SupplementaryDataToken = networkAction.DataToken;
		TargetPlayerID = networkAction.TargetPlayerID;
		DataInt = networkAction.DataInt;
		DataInt2 = networkAction.DataInt2;
		SupplementaryDataBoolean = networkAction.DataBoolean;
		SupplementaryDataString = networkAction.DataString;
	}

	public GameAction(GameActionEventClassIDToken evnt)
	{
		ActionID = evnt.ActionID;
		ActionTypeID = evnt.ActionTypeID;
		PlayerID = evnt.PlayerID;
		ActorID = evnt.ActorID;
		TargetPhaseID = evnt.TargetPhaseID;
		SupplementaryDataIDMin = evnt.SupplementaryDataIDMin;
		SupplementaryDataBoolean = evnt.SupplementaryDataBoolean;
		ClassID = evnt.ClassID;
	}

	public bool Execute()
	{
		try
		{
			FFSNetwork.IsExecutingGameAction = true;
			if (ExecuteActionHandler != null)
			{
				bool executionFinished = true;
				ExecuteActionHandler(this, ref isValid, ref executionFinished);
				return executionFinished;
			}
			if (actions.TryGetValue((GameActionType)ActionTypeID, out var value))
			{
				bool executionFinished2 = true;
				value(this, ref isValid, ref executionFinished2);
				return executionFinished2;
			}
		}
		finally
		{
			FFSNetwork.IsExecutingGameAction = false;
		}
		throw new Exception($"Error executing action. Matching delegate method not found for action ({ActionType}).");
	}

	public static void DefaultActionHandle(GameAction action, ref bool isValid, ref bool executionFinished)
	{
		try
		{
			FFSNetwork.IsExecutingGameAction = true;
			if (actions.TryGetValue((GameActionType)action.ActionTypeID, out var value))
			{
				executionFinished = true;
				value(action, ref isValid, ref executionFinished);
				return;
			}
		}
		finally
		{
			FFSNetwork.IsExecutingGameAction = false;
		}
		throw new Exception($"Error executing action. Matching delegate method not found for action ({action.ActionType}) .");
	}

	private static void ShowUninitializedActionErrorMessage()
	{
		throw new Exception("Error executing action. Uninitialized action detected.");
	}

	public override string ToString()
	{
		return $"[{PlayerID}:{ActionID}] {GetType().Name}([{ActionTypeID}] {ActionType} @ {TargetPhase})";
	}
}

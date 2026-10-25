using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Libs.GameConsole;
using Libs.Threading;
using SharedLibrary;

namespace ScenarioRuleLibrary;

public class ScenarioRuleClient
{
	public enum EMessageType
	{
		STEP_COMPLETE,
		NEXT_PHASE,
		PASS,
		END_ABILITY_SYNCHRONISE,
		UNDO,
		STOP,
		ACTIVATE_TRAP,
		END_TURN_SYNCHRONISE,
		PASS_STEP,
		TILE_SELECTED,
		ACTIVATE_HAZARDOUS_TERRAIN,
		CLEAR_ROUND_ABILITY_CARDS,
		MOVE_ABILITY_CARD,
		TILE_DESELECTED,
		APPLY_SINGLE_TARGET,
		ACTIVATE_CHEST,
		TOGGLE_ACTIONAUGMENTATION,
		END_ROUND_SYNCHRONISE,
		TOGGLE_ACTIVE_BONUS,
		CLEAR_TARGETS,
		LOCK_ACTIVE_BONUS,
		UPDATE_ATTACK_PATH,
		QUEUE_SPAWNER_TRIGGER,
		UPDATE_MOVE_CARRY,
		TOGGLE_SCENARIO_MODIFIER_ACTIVATION,
		TOGGLE_DOOR,
		TOGGLE_ITEM,
		SORT_INITIATIVE,
		SET_START_ROUND_DECK_STATE,
		GET_START_ROUND_DECK_STATE,
		SHORT_REST,
		PRE_SHORT_REST,
		TRIGGER_PENDING_SHORT_REST_EFFECTS,
		TRIGGER_PENDING_LONG_REST_EFFECTS,
		PRE_LONG_REST,
		TRIGGER_SCENARIO_MODIFIER,
		HANDLE_LONG_REST,
		APPLY_SCENARIO_REWARDS,
		FINISH_SELECT_AND_MOVE_CARD_ABILITY,
		CHECK_FINISHED_BONUSES,
		ARBITRARY_COMMAND
	}

	public class CSRLMessage
	{
		public EMessageType m_MessageType;

		public uint m_MessageID;

		public string m_CallerStack;

		public virtual bool LogCallStack => false;

		public CSRLMessage(EMessageType eMessageType)
		{
			m_MessageType = eMessageType;
		}

		public override string ToString()
		{
			return $"{GetType().Name}({m_MessageType})";
		}
	}

	public class ArbitraryCommandMessage : CSRLMessage
	{
		public Action action;

		public string command;

		public object context;

		public string description;

		public ArbitraryCommandMessage(string cmd, object ctx, Action action, string description = null)
			: base(EMessageType.ARBITRARY_COMMAND)
		{
			command = cmd;
			context = ctx;
			this.action = action;
			this.description = description;
		}

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(description))
			{
				return GetType().Name + "(" + description + ")";
			}
			return base.ToString();
		}
	}

	private class CSRLMoveAbilityCardMessage : CSRLMessage
	{
		public CCharacterClass m_CharacterClass;

		public CAbilityCard m_AbilityCard;

		public List<CAbilityCard> m_FromAbilityCardList;

		public List<CAbilityCard> m_ToAbilityCardList;

		public string m_FromAbilityCardListName;

		public string m_ToAbilityCardListName;

		public bool m_SendNetworkSelectedCardsWhenCompleted;

		public override bool LogCallStack => true;

		public CSRLMoveAbilityCardMessage(CCharacterClass characterClass, CAbilityCard abilityCard, List<CAbilityCard> fromAbilityCardList, List<CAbilityCard> toAbilityCardList, string fromAbilityCardListName, string toAbilityCardListName, bool sendNetworkSelectedCardsWhenCompleted)
			: base(EMessageType.MOVE_ABILITY_CARD)
		{
			m_CharacterClass = characterClass;
			m_AbilityCard = abilityCard;
			m_FromAbilityCardList = fromAbilityCardList;
			m_ToAbilityCardList = toAbilityCardList;
			m_FromAbilityCardListName = fromAbilityCardListName;
			m_ToAbilityCardListName = toAbilityCardListName;
			m_SendNetworkSelectedCardsWhenCompleted = sendNetworkSelectedCardsWhenCompleted;
		}
	}

	private class CSRLClearRoundAbilityCardsMessage : CSRLMessage
	{
		public CCharacterClass m_CharacterClass;

		public CSRLClearRoundAbilityCardsMessage(CCharacterClass characterClass)
			: base(EMessageType.CLEAR_ROUND_ABILITY_CARDS)
		{
			m_CharacterClass = characterClass;
		}
	}

	private class CSRLApplyScenarioRewardsMessage : CSRLMessage
	{
		public bool Won;

		public CSRLApplyScenarioRewardsMessage(bool won)
			: base(EMessageType.APPLY_SCENARIO_REWARDS)
		{
			Won = won;
		}
	}

	private class CSRLTileSelectedMessage : CSRLMessage
	{
		public CTile m_Tile;

		public List<CTile> m_OptionalTileList;

		public CSRLTileSelectedMessage(CTile tile = null, List<CTile> optionalTileList = null)
			: base(EMessageType.TILE_SELECTED)
		{
			m_Tile = tile;
			m_OptionalTileList = optionalTileList;
		}
	}

	private class CSRLTileDeselectedMessage : CSRLMessage
	{
		public CTile m_Tile;

		public List<CTile> m_OptionalTileList;

		public CSRLTileDeselectedMessage(CTile tile = null, List<CTile> optionalTileList = null)
			: base(EMessageType.TILE_DESELECTED)
		{
			m_Tile = tile;
			m_OptionalTileList = optionalTileList;
		}
	}

	private class CSRLApplySingleTargetMessage : CSRLMessage
	{
		public CActor m_Actor;

		public CSRLApplySingleTargetMessage(CActor actor = null)
			: base(EMessageType.APPLY_SINGLE_TARGET)
		{
			m_Actor = actor;
		}
	}

	private class CSRLToggleActionAugmentationMessage : CSRLMessage
	{
		public CActionAugmentation m_ActionAugmentation;

		public CSRLToggleActionAugmentationMessage(CActionAugmentation actionAugmentation = null)
			: base(EMessageType.TOGGLE_ACTIONAUGMENTATION)
		{
			m_ActionAugmentation = actionAugmentation;
		}
	}

	private class CSRLToggleActiveBonusMessage : CSRLMessage
	{
		public CActiveBonus m_ActiveBonus;

		public ElementInfusionBoardManager.EElement? m_Element;

		public CActor m_Actor;

		public object m_ExtraOption;

		public int m_PhaseWhenClickedInt;

		public CSRLToggleActiveBonusMessage(CActiveBonus activeBonus, ElementInfusionBoardManager.EElement? element, CActor actor, int phaseWhenClickedInt, object extraOption = null)
			: base(EMessageType.TOGGLE_ACTIVE_BONUS)
		{
			m_ActiveBonus = activeBonus;
			m_Element = element;
			m_Actor = actor;
			m_ExtraOption = extraOption;
			m_PhaseWhenClickedInt = phaseWhenClickedInt;
		}
	}

	private class CSRLLockActiveBonusMessage : CSRLMessage
	{
		public CActiveBonus m_ActiveBonus;

		public CSRLLockActiveBonusMessage(CActiveBonus activeBonus)
			: base(EMessageType.LOCK_ACTIVE_BONUS)
		{
			m_ActiveBonus = activeBonus;
		}
	}

	private class CSRLUpdateAttackPathMessage : CSRLMessage
	{
		public List<CTile> m_OptionalTileList;

		public CSRLUpdateAttackPathMessage(List<CTile> optionalTileList = null)
			: base(EMessageType.UPDATE_ATTACK_PATH)
		{
			m_OptionalTileList = optionalTileList;
		}
	}

	private class CSRLUpdateMoveCarryMessage : CSRLMessage
	{
		public List<CTile> m_OptionalTileList;

		public bool m_RemoveWaypoint;

		public CSRLUpdateMoveCarryMessage(List<CTile> optionalTileList = null, bool removeWaypoint = false)
			: base(EMessageType.UPDATE_MOVE_CARRY)
		{
			m_OptionalTileList = optionalTileList;
			m_RemoveWaypoint = removeWaypoint;
		}
	}

	private class CSRLToggleScenarioModifierActivation : CSRLMessage
	{
		public string m_ScenarioModifierEventIdentifier;

		public bool m_Activate;

		public CSRLToggleScenarioModifierActivation(string eventIdentifier, bool activate)
			: base(EMessageType.TOGGLE_SCENARIO_MODIFIER_ACTIVATION)
		{
			m_ScenarioModifierEventIdentifier = eventIdentifier;
			m_Activate = activate;
		}
	}

	private class CSRLTriggerScenarioModifier : CSRLMessage
	{
		public string m_ScenarioModifierEventIdentifier;

		public CSRLTriggerScenarioModifier(string eventIdentifier)
			: base(EMessageType.TRIGGER_SCENARIO_MODIFIER)
		{
			m_ScenarioModifierEventIdentifier = eventIdentifier;
		}
	}

	private class CSRLToggleDoorMessage : CSRLMessage
	{
		public string m_DoorGuid;

		public bool m_OpenDoor;

		public bool m_UnlockOnly;

		public bool m_LockDoor;

		public CSRLToggleDoorMessage(string doorGuid, bool openDoor, bool unlockOnly, bool lockDoor)
			: base(EMessageType.TOGGLE_DOOR)
		{
			m_DoorGuid = doorGuid;
			m_OpenDoor = openDoor;
			m_UnlockOnly = unlockOnly;
			m_LockDoor = lockDoor;
		}
	}

	private class CSRLToggleItemMessage : CSRLMessage
	{
		public CItem m_Item;

		public CActor m_Actor;

		public CSRLToggleItemMessage(CItem item, CActor actor)
			: base(EMessageType.TOGGLE_ITEM)
		{
			m_Item = item;
			m_Actor = actor;
		}
	}

	private class CSRLUndoMessage : CSRLMessage
	{
		public List<CActiveBonus> m_ActiveBonusesToUntoggle;

		public CSRLUndoMessage(List<CActiveBonus> activeBonusesToUntoggle)
			: base(EMessageType.UNDO)
		{
			m_ActiveBonusesToUntoggle = activeBonusesToUntoggle;
		}
	}

	private class CSRLSetStartRoundDeckStateMessage : CSRLMessage
	{
		public CPlayerActor m_PlayerActor;

		public StartRoundCardState m_StartRoundCardState;

		public CSRLSetStartRoundDeckStateMessage(CPlayerActor playerActor, StartRoundCardState startRoundCardState)
			: base(EMessageType.SET_START_ROUND_DECK_STATE)
		{
			m_PlayerActor = playerActor;
			m_StartRoundCardState = startRoundCardState;
		}
	}

	private class CSRLGetStartRoundDeckStateMessage : CSRLMessage
	{
		public CPlayerActor m_PlayerActor;

		public int m_GameActionID;

		public string m_FastReloadID;

		public CSRLGetStartRoundDeckStateMessage(CPlayerActor playerActor, int gameActionID, string fastReloadID)
			: base(EMessageType.GET_START_ROUND_DECK_STATE)
		{
			m_PlayerActor = playerActor;
			m_GameActionID = gameActionID;
			m_FastReloadID = fastReloadID;
		}
	}

	private class CSRLHandleLongRestMessage : CSRLMessage
	{
		public CPlayerActor m_PlayerActor;

		public CAbilityCard m_CardLost;

		public bool m_ImprovedShortRest;

		public bool m_FromStateUpdate;

		public string m_FastReloadID;

		public CSRLHandleLongRestMessage(CPlayerActor playerActor, CAbilityCard cardLost, bool fromStateUpdate, string fastReloadID, bool improvedShortRest = false)
			: base(EMessageType.HANDLE_LONG_REST)
		{
			m_PlayerActor = playerActor;
			m_CardLost = cardLost;
			m_ImprovedShortRest = improvedShortRest;
			m_FromStateUpdate = fromStateUpdate;
			m_FastReloadID = fastReloadID;
		}
	}

	private class CSRLPreShortRestPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public bool IsOnline;

		public CSRLPreShortRestPlayerMessage(CPlayerActor playerActor, bool isOnline)
			: base(EMessageType.PRE_SHORT_REST)
		{
			PlayerActor = playerActor;
			IsOnline = isOnline;
		}
	}

	private class CSRLPreLongRestPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public CSRLPreLongRestPlayerMessage(CPlayerActor playerActor)
			: base(EMessageType.PRE_LONG_REST)
		{
			PlayerActor = playerActor;
		}
	}

	private class CSRLShortRestPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public CAbilityCard DiscardedAbilityCard;

		public bool LoseHealth;

		public bool FromStateUpdate;

		public bool UpdateScenarioRNG;

		public CSRLShortRestPlayerMessage(CPlayerActor playerActor, CAbilityCard discardedAbilityCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate)
			: base(EMessageType.SHORT_REST)
		{
			PlayerActor = playerActor;
			DiscardedAbilityCard = discardedAbilityCard;
			LoseHealth = loseHealth;
			FromStateUpdate = fromStateUpdate;
			UpdateScenarioRNG = updateScenarioRNG;
		}
	}

	private class CSRLTriggerShortRestEffectsPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public CSRLTriggerShortRestEffectsPlayerMessage(CPlayerActor playerActor)
			: base(EMessageType.TRIGGER_PENDING_SHORT_REST_EFFECTS)
		{
			PlayerActor = playerActor;
		}
	}

	private class CSRLTriggerLongRestEffectsPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public CSRLTriggerLongRestEffectsPlayerMessage(CPlayerActor playerActor)
			: base(EMessageType.TRIGGER_PENDING_LONG_REST_EFFECTS)
		{
			PlayerActor = playerActor;
		}
	}

	private class CSRLFinishSelectAndMoveCardAbilityPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public CAbilitySelectAndMoveCard Ability;

		public CSRLFinishSelectAndMoveCardAbilityPlayerMessage(CPlayerActor playerActor, CAbilitySelectAndMoveCard ability)
			: base(EMessageType.FINISH_SELECT_AND_MOVE_CARD_ABILITY)
		{
			PlayerActor = playerActor;
			Ability = ability;
		}
	}

	private class CSRLCheckFinishedBonusesMessage : CSRLMessage
	{
		public CActor Actor;

		public CSRLCheckFinishedBonusesMessage(CActor actor)
			: base(EMessageType.CHECK_FINISHED_BONUSES)
		{
			Actor = actor;
		}
	}

	public delegate void MessageHandlerCallback(CMessageData message);

	public static readonly object Synchronizer = new object();

	internal static CommandExecEngine CommandExecEngine;

	public static bool IsChoreographerProcessing;

	public const int c_NumberGloomhavenLevels = 8;

	public static volatile uint s_SRLLastProcessedMessageID;

	private static IThreadHandle s_WorkThread;

	private static volatile bool s_IsAlive;

	private static AutoResetEvent s_InitEvent = new AutoResetEvent(initialState: false);

	private static readonly ConcurrentQueue<CSRLMessage> s_MessageQueue = new ConcurrentQueue<CSRLMessage>();

	private static bool s_Stop;

	private static bool s_BlockMessageProcessing;

	private static uint s_SRLNextMessageID;

	private static MessageHandlerCallback s_MessageHandler;

	private static bool s_Initialised;

	private static CSRLYML s_SRLYML;

	public static MessageHandlerCallback MessageHandler => s_MessageHandler;

	public static int MessageQueueLength => s_MessageQueue.Count;

	public static List<CSRLMessage> MessageQueueCopy => s_MessageQueue.ToList();

	public static CSRLYML SRLYML => s_SRLYML;

	public static bool IsMessageProcessingBlocked => s_BlockMessageProcessing;

	public static bool ScenarioRuleClientStopped => !s_IsAlive;

	public static bool IsProcessing { get; private set; }

	public static bool IsProcessingOrMessagesQueued
	{
		get
		{
			if (s_MessageQueue.Count <= 0)
			{
				return IsProcessing;
			}
			return true;
		}
	}

	public static bool IsHandStateMessagePending { get; private set; }

	public static bool IsWorkerThread
	{
		get
		{
			if (s_WorkThread != null)
			{
				return s_WorkThread.IsCurrentThread;
			}
			return false;
		}
	}

	public static void Initialise(bool reinitialise = false)
	{
		Log.SimpleInfo("ScenarioRuleClient initialising");
		if (!s_Initialised | reinitialise)
		{
			s_SRLYML = new CSRLYML();
			GameState.OutputAdvantageInfo = false;
			s_Initialised = true;
		}
		SEventLogMessageHandler.Initialise();
	}

	public static void Uninitalize()
	{
		s_Initialised = false;
	}

	public static void LoadData()
	{
		CharacterClassManager.Load();
		MonsterClassManager.Load();
		ActionManager.Load();
	}

	public static void UnloadData()
	{
		CharacterClassManager.Unload();
		MonsterClassManager.Unload();
		ActionManager.Unload();
	}

	public static void ToggleMessageProcessing(bool process)
	{
		Log.SimpleInfo("ScenarioRuleClient toggling message processing to: " + process);
		s_BlockMessageProcessing = !process;
	}

	private static void ProcessMessage(CSRLMessage message)
	{
		try
		{
			CSRLQueueStatusLog_MessageData cSRLQueueStatusLog_MessageData = new CSRLQueueStatusLog_MessageData("", null);
			cSRLQueueStatusLog_MessageData.m_SRLMessage = message;
			MessageHandler(cSRLQueueStatusLog_MessageData);
			ArbitraryCommandMessage arbitraryCommandMessage = message as ArbitraryCommandMessage;
			string text = ((arbitraryCommandMessage == null || arbitraryCommandMessage.description == null) ? ("ScenarioRuleClient ProcessMessage: " + Log.ColorizeHex(message.m_MessageType.ToString(), "CA6F1E") + ".") : ("ScenarioRuleClient ProcessMessage: " + Log.ColorizeHex(arbitraryCommandMessage.description, "CA6F1E") + "."));
			if (message.LogCallStack)
			{
				text = text + "\nSender callstack:\n" + message.m_CallerStack + "\n-----";
			}
			Log.SimpleInfo(text);
			switch (message.m_MessageType)
			{
			case EMessageType.ARBITRARY_COMMAND:
				if (arbitraryCommandMessage.action != null)
				{
					arbitraryCommandMessage.action();
				}
				else if (arbitraryCommandMessage.command != null)
				{
					if (GameConsoleWindow.Instance.CommandRepo != null)
					{
						Log.Info("Executing on the SRL thread " + arbitraryCommandMessage.command);
						CommandExecEngine.ExecuteCommandLine(arbitraryCommandMessage.command);
					}
				}
				else
				{
					Log.Error("null SRL arb command");
				}
				break;
			case EMessageType.STEP_COMPLETE:
				PhaseManager.StepComplete();
				break;
			case EMessageType.NEXT_PHASE:
				GameState.NextPhase();
				break;
			case EMessageType.PASS:
				if (PhaseManager.CurrentPhase is CPhaseInfuserMechanicSelection cPhaseInfuserMechanicSelection)
				{
					cPhaseInfuserMechanicSelection.ManuallyEnd();
				}
				else if (PhaseManager.PhaseType == CPhase.PhaseType.ShortRest || PhaseManager.PhaseType == CPhase.PhaseType.Action || PhaseManager.PhaseType == CPhase.PhaseType.CheckForForgoActionActiveBonuses || GameState.CurrentActionInitiator == GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase)
				{
					PhaseManager.NextStep(passing: true);
				}
				else
				{
					GameState.NextPhase();
				}
				break;
			case EMessageType.PASS_STEP:
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					PhaseManager.StepComplete(passingStep: true);
				}
				else
				{
					PhaseManager.StepComplete();
				}
				break;
			case EMessageType.END_ABILITY_SYNCHRONISE:
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					((CPhaseAction)PhaseManager.Phase).EndAbilitySynchronise();
					break;
				}
				CSRLWrongPhaseException_MessageData message15 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message15);
				break;
			}
			case EMessageType.END_TURN_SYNCHRONISE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.EndTurn)
				{
					((CPhaseEndTurn)PhaseManager.Phase).EndTurnSynchronise();
				}
				else if (PhaseManager.PhaseType != CPhase.PhaseType.EndRound)
				{
					CSRLWrongPhaseException_MessageData message11 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message11);
				}
				break;
			case EMessageType.UNDO:
			{
				CSRLUndoMessage cSRLUndoMessage = (CSRLUndoMessage)message;
				if (PhaseManager.Phase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction)
				{
					CAbility ability2 = cPhaseAction.CurrentPhaseAbility.m_Ability;
					if (ability2.MiscAbilityData == null || !ability2.MiscAbilityData.IsCostAbility)
					{
						ability2.Undo();
					}
					bool flag3 = true;
					bool hideHelpBox = true;
					if (ability2.IsItemAbility)
					{
						cPhaseAction.UndoItemAbility();
					}
					else
					{
						CAbility parentAbility = ability2.ParentAbility;
						if (parentAbility != null && parentAbility.AbilityType == CAbility.EAbilityType.ChooseAbility)
						{
							cPhaseAction.UndoChooseAbility();
							flag3 = false;
						}
						else if (ability2.MiscAbilityData == null || !ability2.MiscAbilityData.IsCostAbility)
						{
							GameState.InternalCurrentActor.Inventory.Undo();
							cPhaseAction.ResetActionAugmentationsToConsume();
							if (GameState.PendingInfusionPhase != null)
							{
								PhaseManager.SetNextPhase(CPhase.PhaseType.InfuserMechanicSelection);
							}
							else
							{
								PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
							}
							hideHelpBox = false;
						}
					}
					if (flag3)
					{
						GameState.Undo();
					}
					foreach (CActiveBonus item2 in cSRLUndoMessage.m_ActiveBonusesToUntoggle)
					{
						item2.ToggleActiveBonus(null, null);
					}
					if (cPhaseAction.CurrentTriggeredActiveBonus != null && cPhaseAction.CurrentTriggeredActiveBonus.ToggledBonus)
					{
						cPhaseAction.CurrentTriggeredActiveBonus.ToggleActiveBonus(null, GameState.InternalCurrentActor);
					}
					if (ability2.MiscAbilityData == null || !ability2.MiscAbilityData.IsCostAbility)
					{
						CFinishedProcessingUndo_MessageData cFinishedProcessingUndo_MessageData = new CFinishedProcessingUndo_MessageData();
						cFinishedProcessingUndo_MessageData.m_Ability = ability2;
						cFinishedProcessingUndo_MessageData.m_HideHelpBox = hideHelpBox;
						MessageHandler(cFinishedProcessingUndo_MessageData);
					}
				}
				else
				{
					CSRLWrongPhaseException_MessageData message19 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message19);
				}
				break;
			}
			case EMessageType.STOP:
				GameState.Stop();
				break;
			case EMessageType.ACTIVATE_TRAP:
			{
				CSRLTrapMessage cSRLTrapMessage = (CSRLTrapMessage)message;
				cSRLTrapMessage.m_TrapProp.AutomaticActivate(cSRLTrapMessage.m_Actor);
				break;
			}
			case EMessageType.ACTIVATE_CHEST:
			{
				CSRLChestMessage cSRLChestMessage = (CSRLChestMessage)message;
				cSRLChestMessage.m_ChestProp.DelayedActivate(cSRLChestMessage.m_Actor);
				break;
			}
			case EMessageType.TILE_SELECTED:
			{
				CSRLTileSelectedMessage cSRLTileSelectedMessage = (CSRLTileSelectedMessage)message;
				PhaseManager.TileSelected(cSRLTileSelectedMessage.m_Tile, cSRLTileSelectedMessage.m_OptionalTileList);
				break;
			}
			case EMessageType.TILE_DESELECTED:
			{
				CSRLTileDeselectedMessage cSRLTileDeselectedMessage = (CSRLTileDeselectedMessage)message;
				PhaseManager.TileDeselected(cSRLTileDeselectedMessage.m_Tile, cSRLTileDeselectedMessage.m_OptionalTileList);
				break;
			}
			case EMessageType.ACTIVATE_HAZARDOUS_TERRAIN:
			{
				CSRLHazardousTerrainMessage cSRLHazardousTerrainMessage = (CSRLHazardousTerrainMessage)message;
				cSRLHazardousTerrainMessage.m_HazardousTerrainProp.AutomaticActivate(cSRLHazardousTerrainMessage.m_Actor);
				break;
			}
			case EMessageType.CLEAR_ROUND_ABILITY_CARDS:
				((CSRLClearRoundAbilityCardsMessage)message).m_CharacterClass.ClearRoundAbilityCards();
				break;
			case EMessageType.MOVE_ABILITY_CARD:
			{
				CSRLMoveAbilityCardMessage cSRLMoveAbilityCardMessage = (CSRLMoveAbilityCardMessage)message;
				cSRLMoveAbilityCardMessage.m_CharacterClass.MoveAbilityCard(cSRLMoveAbilityCardMessage.m_AbilityCard, cSRLMoveAbilityCardMessage.m_FromAbilityCardList, cSRLMoveAbilityCardMessage.m_ToAbilityCardList, cSRLMoveAbilityCardMessage.m_FromAbilityCardListName, cSRLMoveAbilityCardMessage.m_ToAbilityCardListName, cSRLMoveAbilityCardMessage.m_SendNetworkSelectedCardsWhenCompleted);
				break;
			}
			case EMessageType.APPLY_SINGLE_TARGET:
				PhaseManager.ApplySingleTarget(((CSRLApplySingleTargetMessage)message).m_Actor);
				break;
			case EMessageType.TOGGLE_ACTIONAUGMENTATION:
			{
				CSRLToggleActionAugmentationMessage cSRLToggleActionAugmentationMessage = (CSRLToggleActionAugmentationMessage)message;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					((CPhaseAction)PhaseManager.Phase).ToggleActionAugmentation(cSRLToggleActionAugmentationMessage.m_ActionAugmentation);
					break;
				}
				CSRLWrongPhaseException_MessageData message17 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message17);
				break;
			}
			case EMessageType.END_ROUND_SYNCHRONISE:
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.EndRound)
				{
					((CPhaseEndRound)PhaseManager.Phase).EndRoundSynchronise();
					break;
				}
				CSRLWrongPhaseException_MessageData message16 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message16);
				break;
			}
			case EMessageType.TOGGLE_ACTIVE_BONUS:
			{
				CSRLToggleActiveBonusMessage cSRLToggleActiveBonusMessage = (CSRLToggleActiveBonusMessage)message;
				cSRLToggleActiveBonusMessage.m_ActiveBonus.ToggleActiveBonus(cSRLToggleActiveBonusMessage.m_Element, cSRLToggleActiveBonusMessage.m_Actor, cSRLToggleActiveBonusMessage.m_ExtraOption);
				CFinishedProcessingActiveBonusToggle_MessageData cFinishedProcessingActiveBonusToggle_MessageData = new CFinishedProcessingActiveBonusToggle_MessageData();
				cFinishedProcessingActiveBonusToggle_MessageData.m_ActiveBonus = cSRLToggleActiveBonusMessage.m_ActiveBonus;
				cFinishedProcessingActiveBonusToggle_MessageData.m_PhaseWhenClickedInt = cSRLToggleActiveBonusMessage.m_PhaseWhenClickedInt;
				cFinishedProcessingActiveBonusToggle_MessageData.m_ActorSpawningMessage = cSRLToggleActiveBonusMessage.m_Actor;
				MessageHandler(cFinishedProcessingActiveBonusToggle_MessageData);
				break;
			}
			case EMessageType.TOGGLE_ITEM:
			{
				CSRLToggleItemMessage cSRLToggleItemMessage = (CSRLToggleItemMessage)message;
				CAbility ability = null;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					ability = ((CPhaseAction)PhaseManager.Phase)?.CurrentPhaseAbility?.m_Ability;
				}
				CItem item = cSRLToggleItemMessage.m_Item;
				cSRLToggleItemMessage.m_Actor.Inventory.ToggleItem(item);
				CFinishedProcessingItemToggle_MessageData cFinishedProcessingItemToggle_MessageData = new CFinishedProcessingItemToggle_MessageData();
				cFinishedProcessingItemToggle_MessageData.m_Ability = ability;
				MessageHandler(cFinishedProcessingItemToggle_MessageData);
				break;
			}
			case EMessageType.CLEAR_TARGETS:
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					CAbility ability3 = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability;
					if (ability3.CanClearTargets())
					{
						ability3.ClearTargets();
					}
					CFinishedProcessingClearTargets_MessageData cFinishedProcessingClearTargets_MessageData = new CFinishedProcessingClearTargets_MessageData();
					cFinishedProcessingClearTargets_MessageData.m_ActorSpawningMessage = ability3?.TargetingActor;
					cFinishedProcessingClearTargets_MessageData.m_Ability = ability3;
					MessageHandler(cFinishedProcessingClearTargets_MessageData);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message20 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message20);
				}
				break;
			case EMessageType.LOCK_ACTIVE_BONUS:
			{
				CSRLLockActiveBonusMessage cSRLLockActiveBonusMessage = (CSRLLockActiveBonusMessage)message;
				cSRLLockActiveBonusMessage.m_ActiveBonus.ToggleLocked = !cSRLLockActiveBonusMessage.m_ActiveBonus.ToggleLocked;
				break;
			}
			case EMessageType.UPDATE_ATTACK_PATH:
			{
				CSRLUpdateAttackPathMessage cSRLUpdateAttackPathMessage = (CSRLUpdateAttackPathMessage)message;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					bool flag2 = true;
					if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability is CAbilityAttack cAbilityAttack)
					{
						cAbilityAttack.UpdateAllTargetsOnAttackPath(cSRLUpdateAttackPathMessage.m_OptionalTileList);
						flag2 = false;
					}
					if (flag2 && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability is CAbilityAttack)?.m_Ability is CAbilityAttack cAbilityAttack2)
					{
						cAbilityAttack2.UpdateAllTargetsOnAttackPath(cSRLUpdateAttackPathMessage.m_OptionalTileList);
						flag2 = false;
					}
					if (flag2)
					{
						CSRLWrongPhaseException_MessageData message8 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
						MessageHandler(message8);
					}
				}
				else
				{
					CSRLWrongPhaseException_MessageData message9 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message9);
				}
				break;
			}
			case EMessageType.UPDATE_MOVE_CARRY:
			{
				CSRLUpdateMoveCarryMessage cSRLUpdateMoveCarryMessage = (CSRLUpdateMoveCarryMessage)message;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability is CAbilityMove cAbilityMove)
					{
						cAbilityMove.UpdateCarryActors(cSRLUpdateMoveCarryMessage.m_OptionalTileList, cSRLUpdateMoveCarryMessage.m_RemoveWaypoint);
						break;
					}
					CSRLWrongPhaseException_MessageData message4 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message4);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message5 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message5);
				}
				break;
			}
			case EMessageType.TOGGLE_SCENARIO_MODIFIER_ACTIVATION:
			{
				CSRLToggleScenarioModifierActivation toggleMessage2 = (CSRLToggleScenarioModifierActivation)message;
				ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.EventIdentifier == toggleMessage2.m_ScenarioModifierEventIdentifier)?.SetDeactivated(!toggleMessage2.m_Activate);
				break;
			}
			case EMessageType.TRIGGER_SCENARIO_MODIFIER:
			{
				CSRLTriggerScenarioModifier toggleMessage = (CSRLTriggerScenarioModifier)message;
				ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.EventIdentifier == toggleMessage.m_ScenarioModifierEventIdentifier)?.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber, forceActivate: true);
				break;
			}
			case EMessageType.TOGGLE_DOOR:
			{
				CSRLToggleDoorMessage toggleDoorMessage = (CSRLToggleDoorMessage)message;
				CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.FirstOrDefault((CObjectProp d) => d.PropGuid == toggleDoorMessage.m_DoorGuid);
				if (cObjectDoor == null)
				{
					break;
				}
				if (toggleDoorMessage.m_OpenDoor)
				{
					if (toggleDoorMessage.m_UnlockOnly)
					{
						cObjectDoor.UnlockLockedDoorWithoutOpening();
					}
					else
					{
						cObjectDoor.SetExtraLockState(lockedStateToSet: false);
					}
				}
				else
				{
					cObjectDoor.CloseOpenedDoor(toggleDoorMessage.m_LockDoor);
				}
				break;
			}
			case EMessageType.SORT_INITIATIVE:
				GameState.SortIntoInitiativeAndIDOrder();
				break;
			case EMessageType.SET_START_ROUND_DECK_STATE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
				{
					CSRLSetStartRoundDeckStateMessage cSRLSetStartRoundDeckStateMessage = (CSRLSetStartRoundDeckStateMessage)message;
					cSRLSetStartRoundDeckStateMessage.m_PlayerActor.CharacterClass.ProxySetStartRoundDeckState(cSRLSetStartRoundDeckStateMessage.m_StartRoundCardState);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message18 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message18);
				}
				break;
			case EMessageType.GET_START_ROUND_DECK_STATE:
			{
				CSRLGetStartRoundDeckStateMessage cSRLGetStartRoundDeckStateMessage = (CSRLGetStartRoundDeckStateMessage)message;
				CPlayerActor playerActor = cSRLGetStartRoundDeckStateMessage.m_PlayerActor;
				if (GameState.IsPerformingCardOutOfPhase || GameState.IsSelectingCardForMode())
				{
					ToggleIsHandStateMessagePending(val: false);
				}
				else if (PhaseManager.IsInCardsSelectionPhase)
				{
					CReplicateStartRoundCardState_MessageData message13 = new CReplicateStartRoundCardState_MessageData(playerActor, cSRLGetStartRoundDeckStateMessage.m_GameActionID, playerActor.CharacterClass.GetCurrentCardState(cSRLGetStartRoundDeckStateMessage.m_FastReloadID));
					MessageHandler(message13);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message14 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message14);
					ToggleIsHandStateMessagePending(val: false);
				}
				break;
			}
			case EMessageType.PRE_LONG_REST:
			{
				CSRLPreLongRestPlayerMessage longRestMessage = (CSRLPreLongRestPlayerMessage)message;
				TriggerLongRestEffects(longRestMessage.PlayerActor, processImmediately: true);
				if (PhaseManager.CurrentPhaseAction != null)
				{
					PhaseManager.CurrentPhaseAction.m_OnEndPhaseAction = () =>
					{
						CContinueLongRest_MessageData message22 = new CContinueLongRest_MessageData(longRestMessage.PlayerActor);
						MessageHandler(message22);
					};
					return;
				}
				CContinueLongRest_MessageData message12 = new CContinueLongRest_MessageData(longRestMessage.PlayerActor);
				MessageHandler(message12);
				break;
			}
			case EMessageType.HANDLE_LONG_REST:
			{
				CSRLHandleLongRestMessage cSRLHandleLongRestMessage = (CSRLHandleLongRestMessage)message;
				GameState.PlayerLongRested(cSRLHandleLongRestMessage.m_CardLost, cSRLHandleLongRestMessage.m_FastReloadID, cSRLHandleLongRestMessage.m_PlayerActor, cSRLHandleLongRestMessage.m_ImprovedShortRest, cSRLHandleLongRestMessage.m_FromStateUpdate);
				CPlayerActor cPlayerActor = cSRLHandleLongRestMessage.m_PlayerActor;
				if (cPlayerActor == null)
				{
					cPlayerActor = (CPlayerActor)GameState.CurrentActor;
				}
				CPlayerFinishedLongRest_MessageData message10 = new CPlayerFinishedLongRest_MessageData(cPlayerActor)
				{
					m_LostCard = cSRLHandleLongRestMessage.m_CardLost,
					m_ImprovedShortRestUsed = cSRLHandleLongRestMessage.m_ImprovedShortRest,
					m_FromStateUpdate = cSRLHandleLongRestMessage.m_FromStateUpdate
				};
				MessageHandler(message10);
				break;
			}
			case EMessageType.PRE_SHORT_REST:
			{
				CSRLPreShortRestPlayerMessage cSRLPreShortRestPlayerMessage = (CSRLPreShortRestPlayerMessage)message;
				bool flag = GameState.ShowItemSelection(cSRLPreShortRestPlayerMessage.PlayerActor, cSRLPreShortRestPlayerMessage.PlayerActor, null, CItem.EItemTrigger.BeforeShortRest, onlyInActionPhase: false);
				TriggerShortRestEffects(cSRLPreShortRestPlayerMessage.PlayerActor, processImmediately: true);
				GameState.HasShortRestWithPreRestAbilities = GameState.PendingOnRestAbilities.Count == 0;
				if (PhaseManager.PhaseType == CPhase.PhaseType.ShortRest && ((!cSRLPreShortRestPlayerMessage.IsOnline && !GameState.s_DelayedShortRestInSinglePlayer) || flag))
				{
					CPerformShortRest_MessageData message7 = new CPerformShortRest_MessageData(cSRLPreShortRestPlayerMessage.PlayerActor);
					MessageHandler(message7);
				}
				break;
			}
			case EMessageType.SHORT_REST:
				if (PhaseManager.PhaseType == CPhase.PhaseType.ShortRest)
				{
					CSRLShortRestPlayerMessage cSRLShortRestPlayerMessage = (CSRLShortRestPlayerMessage)message;
					GameState.PlayerShortRested(cSRLShortRestPlayerMessage.PlayerActor, cSRLShortRestPlayerMessage.DiscardedAbilityCard, cSRLShortRestPlayerMessage.LoseHealth, cSRLShortRestPlayerMessage.UpdateScenarioRNG, cSRLShortRestPlayerMessage.FromStateUpdate);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message6 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message6);
				}
				break;
			case EMessageType.TRIGGER_PENDING_SHORT_REST_EFFECTS:
			{
				CSRLTriggerShortRestEffectsPlayerMessage cSRLTriggerShortRestEffectsPlayerMessage = (CSRLTriggerShortRestEffectsPlayerMessage)message;
				if (!GameState.TriggerAnyOnShortRestAddActiveBonuses(cSRLTriggerShortRestEffectsPlayerMessage.PlayerActor))
				{
					CPlayerFinishedShortRestWithNoEffects_MessageData message3 = new CPlayerFinishedShortRestWithNoEffects_MessageData(cSRLTriggerShortRestEffectsPlayerMessage.PlayerActor);
					MessageHandler(message3);
				}
				break;
			}
			case EMessageType.CHECK_FINISHED_BONUSES:
			{
				CSRLCheckFinishedBonusesMessage cSRLCheckFinishedBonusesMessage = (CSRLCheckFinishedBonusesMessage)message;
				if (cSRLCheckFinishedBonusesMessage.Actor != null)
				{
					cSRLCheckFinishedBonusesMessage.Actor.CharacterClass?.CheckForFinishedActiveBonuses(cSRLCheckFinishedBonusesMessage.Actor);
				}
				else
				{
					GameState.CheckForFinishedActiveBonuses();
				}
				break;
			}
			case EMessageType.TRIGGER_PENDING_LONG_REST_EFFECTS:
			{
				CSRLTriggerLongRestEffectsPlayerMessage cSRLTriggerLongRestEffectsPlayerMessage = (CSRLTriggerLongRestEffectsPlayerMessage)message;
				if (!GameState.TriggerAnyOnLongRestAddActiveBonuses())
				{
					CPlayerFinishedLongRestWithNoEffects_MessageData message2 = new CPlayerFinishedLongRestWithNoEffects_MessageData(cSRLTriggerLongRestEffectsPlayerMessage.PlayerActor);
					MessageHandler(message2);
				}
				break;
			}
			case EMessageType.FINISH_SELECT_AND_MOVE_CARD_ABILITY:
				((CSRLFinishSelectAndMoveCardAbilityPlayerMessage)message).Ability.EndSwap(fromLogicThread: true);
				break;
			case EMessageType.APPLY_SCENARIO_REWARDS:
			{
				CSRLApplyScenarioRewardsMessage cSRLApplyScenarioRewardsMessage = message as CSRLApplyScenarioRewardsMessage;
				ScenarioManager.CurrentScenarioState.ApplyRewards(cSRLApplyScenarioRewardsMessage.Won);
				break;
			}
			}
			s_SRLLastProcessedMessageID = message.m_MessageID;
			if (arbitraryCommandMessage != null && arbitraryCommandMessage.description != null)
			{
				Log.SimpleInfo(Log.ColorizeHex("ScenarioRuleClient Finished ProcessMessage: " + arbitraryCommandMessage.description, "7F7F7F"));
			}
			else
			{
				Log.SimpleInfo(Log.ColorizeHex("ScenarioRuleClient Finished ProcessMessage: " + message.m_MessageType, "7F7F7F"));
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			CSRLException_MessageData message21 = new CSRLException_MessageData(ex.Message, ex.ToString());
			MessageHandler(message21);
		}
	}

	public static void SetMessageHandler(MessageHandlerCallback messageHandler)
	{
		Log.SimpleInfo($"ScenarioRuleClient setting message handler {messageHandler}");
		s_MessageHandler = messageHandler;
	}

	public static bool InAbility(CAbility.EAbilityType abilityType)
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
		{
			return ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == abilityType;
		}
		return false;
	}

	public static void Reset()
	{
		Log.SimpleInfo("ScenarioRuleClient is resetting. ");
		GameState.Reset();
		PhaseManager.Reset();
		CPhaseAction.Reset();
		CActiveBonus.IDCounter = 0;
		GameState.LastDummyCardID = 0;
		SEventLogMessageHandler.Reset();
		s_MessageQueue.Clear();
		IsProcessing = false;
		s_BlockMessageProcessing = false;
		s_SRLNextMessageID = 0u;
		s_SRLLastProcessedMessageID = 0u;
	}

	private static void Work()
	{
		s_IsAlive = true;
		GameState.Start();
		s_InitEvent.Set();
		CommandExecEngine = new CommandExecEngine(GameConsoleWindow.Instance.CommandRepo);
		Log.Info("Created SRL commands exec engine.");
		while (!s_Stop && !ThreadManager.IsCurrentThreadCancel())
		{
			SEventLogMessageHandler.ProcessQueue();
			if (IsMessageProcessingBlocked)
			{
				continue;
			}
			lock (Synchronizer)
			{
				if (!IsChoreographerProcessing)
				{
					IsProcessing = true;
				}
			}
			CSRLMessage result;
			while (!s_Stop && IsProcessing && s_MessageQueue.TryDequeue(out result))
			{
				ProcessMessage(result);
			}
			IsProcessing = false;
			Thread.Sleep(1);
		}
		s_IsAlive = false;
		Log.Info("ScenarioRuleClient Work thread stopped");
	}

	public static void Start(ThreadManager manager)
	{
		Log.SimpleInfo("ScenarioRuleClient is starting. ");
		if (s_WorkThread != null && s_WorkThread.IsAlive && s_IsAlive)
		{
			Log.ErrorWithReport("ScenarioRuleClient Unable to start work thread as it is already running");
			return;
		}
		s_Stop = false;
		IsProcessing = false;
		s_MessageQueue.Clear();
		s_IsAlive = true;
		s_WorkThread = manager.RunThread("GH-ScenarioRuleClient", Work);
	}

	public static void Stop()
	{
		if (s_WorkThread != null && s_WorkThread.IsAlive && !s_Stop)
		{
			Log.SimpleInfo("ScenarioRuleClient is stopping. ");
			s_Stop = true;
			s_WorkThread.CancelThread();
			s_WorkThread = null;
		}
	}

	public static void WaitForSRLThreadInit()
	{
		s_InitEvent.WaitOne(TimeSpan.FromSeconds(60.0));
	}

	public static void AddSRLQueueMessage(CSRLMessage srlMessage, bool processImmediately)
	{
		if (srlMessage is ArbitraryCommandMessage { description: not null } arbitraryCommandMessage)
		{
			Log.SimpleInfo("ScenarioRuleClient Add SRL queue message of type: " + arbitraryCommandMessage.description + " process immediately: " + processImmediately);
		}
		else
		{
			Log.SimpleInfo("ScenarioRuleClient Add SRL queue message of type: " + srlMessage.m_MessageType.ToString() + " process immediately: " + processImmediately);
		}
		if (!processImmediately && GameState.ThreadIsSleeping)
		{
			Log.Warning($"Attempting to enqueue SRL message {srlMessage} while the SRL thread is sleeping");
		}
		if (s_IsAlive)
		{
			srlMessage.m_MessageID = ++s_SRLNextMessageID;
			srlMessage.m_CallerStack = Environment.StackTrace;
			if (processImmediately)
			{
				ProcessMessage(srlMessage);
			}
			else
			{
				s_MessageQueue.Enqueue(srlMessage);
			}
		}
		else
		{
			Log.SimpleInfo("ScenarioRuleClient thread was not alive when trying to add SRL queue message");
		}
	}

	public static string GetFullStack()
	{
		string text = Environment.StackTrace;
		if (s_MessageQueue.TryPeek(out var result))
		{
			text = text + "\nLast message: " + result.m_MessageType;
			text = text + "\nCaller Stack:\n" + result.m_CallerStack;
		}
		return text;
	}

	public static void StepComplete(bool processImmediately = false, bool fromSRL = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.STEP_COMPLETE), processImmediately);
	}

	public static void NextPhase(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.NEXT_PHASE), processImmediately);
	}

	public static void TileSelected(CTile tile, List<CTile> optionalTileList, bool processImmediately = false)
	{
		string text = "Passing tiles to SRL for processing: \nTile: " + ((tile == null) ? "none" : tile.m_ArrayIndex.ToString()) + "\n";
		if (optionalTileList != null && optionalTileList.Count > 0)
		{
			text += "Optional Tile List: ";
			for (int i = 0; i < optionalTileList.Count; i++)
			{
				CTile cTile = optionalTileList[i];
				text = text + "\nOptional Tile: " + cTile.m_ArrayIndex.ToString();
			}
		}
		Log.SimpleInfo(text);
		AddSRLQueueMessage(new CSRLTileSelectedMessage(tile, optionalTileList), processImmediately);
	}

	public static void TileDeselected(CTile tile, List<CTile> optionalTileList, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLTileDeselectedMessage(tile, optionalTileList), processImmediately);
	}

	public static void ApplySingleTarget(CActor actor, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLApplySingleTargetMessage(actor), processImmediately);
	}

	public static void ToggleActionAugmentation(CActionAugmentation actionAugmentation, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLToggleActionAugmentationMessage(actionAugmentation), processImmediately);
	}

	public static void ToggleActiveBonus(CActiveBonus activeBonus, CActor actor, ElementInfusionBoardManager.EElement? element, int phaseWhenClickedInt, object extraOption = null, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLToggleActiveBonusMessage(activeBonus, element, actor, phaseWhenClickedInt, extraOption), processImmediately);
	}

	public static void LockActiveBonus(CActiveBonus activeBonus, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLLockActiveBonusMessage(activeBonus), processImmediately);
	}

	public static void ToggleScenarioModifierActivation(string modifierEventIdentifier, bool activate, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLToggleScenarioModifierActivation(modifierEventIdentifier, activate), processImmediately);
	}

	public static void TriggerScenarioModifier(string modifierEventIdentifier, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLTriggerScenarioModifier(modifierEventIdentifier), processImmediately);
	}

	public static void ToggleDoor(string doorGuid, bool openDoor, bool unlockOnly = false, bool lockDoor = false, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLToggleDoorMessage(doorGuid, openDoor, unlockOnly, lockDoor), processImmediately);
	}

	public static void ToggleItem(CItem item, CActor actor, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLToggleItemMessage(item, actor), processImmediately);
	}

	public static void ClearRoundAbilityCards(CCharacterClass characterClass)
	{
		AddSRLQueueMessage(new CSRLClearRoundAbilityCardsMessage(characterClass), processImmediately: false);
	}

	public static void MoveAbilityCard(CCharacterClass characterClass, CAbilityCard abilityCard, List<CAbilityCard> fromAbilityCardList, List<CAbilityCard> toAbilityCardList, string fromAbilityCardListName, string toAbilityCardListName, bool networkAction)
	{
		AddSRLQueueMessage(new CSRLMoveAbilityCardMessage(characterClass, abilityCard, fromAbilityCardList, toAbilityCardList, fromAbilityCardListName, toAbilityCardListName, networkAction), processImmediately: true);
	}

	public static void ExecuteSRLCommand(string command = null, object context = null, Action action = null, string description = null, bool processImmediately = false)
	{
		AddSRLQueueMessage(new ArbitraryCommandMessage(command, context, action, description), processImmediately);
	}

	[ConsoleCommand(Command = "backend_cmd")]
	public static void BackendCmd(IConsole console, string cmd)
	{
		console.WriteLine("Executing backend cmd " + cmd);
		ExecuteSRLCommand(cmd, null, null, "CHEAT_" + cmd);
	}

	public static void Pass(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.PASS), processImmediately);
	}

	public static void PassStep(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.PASS_STEP), processImmediately);
	}

	public static void EndAbilitySynchronise(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.END_ABILITY_SYNCHRONISE), processImmediately);
	}

	public static void EndTurnSynchronise(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.END_TURN_SYNCHRONISE), processImmediately);
	}

	public static void EndRoundSynchronise(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.END_ROUND_SYNCHRONISE), processImmediately);
	}

	public static void Undo(List<CActiveBonus> activeBonusesToUntoggle)
	{
		AddSRLQueueMessage(new CSRLUndoMessage(activeBonusesToUntoggle), processImmediately: false);
	}

	public static void ClearTargets(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.CLEAR_TARGETS), processImmediately);
	}

	public static void UpdateAttackpath(List<CTile> optionalTileList, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLUpdateAttackPathMessage(optionalTileList), processImmediately);
	}

	public static void UpdateMoveCarry(List<CTile> optionalTileList, bool removeWaypoint = false, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLUpdateMoveCarryMessage(optionalTileList, removeWaypoint), processImmediately);
	}

	public static void SortInitiative(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.SORT_INITIATIVE), processImmediately);
	}

	public static void PreShortRest(CPlayerActor playerActor, bool isOnline, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLPreShortRestPlayerMessage(playerActor, isOnline), processImmediately);
	}

	public static void PreLongRest(CPlayerActor playerActor, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLPreLongRestPlayerMessage(playerActor), processImmediately);
	}

	public static void TriggerShortRestEffects(CPlayerActor playerActor, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLTriggerShortRestEffectsPlayerMessage(playerActor), processImmediately);
	}

	public static void TriggerLongRestEffects(CPlayerActor playerActor, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLTriggerLongRestEffectsPlayerMessage(playerActor), processImmediately);
	}

	public static void FinishSelectAndMoveCardAbility(CPlayerActor playerActor, CAbilitySelectAndMoveCard ability, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLFinishSelectAndMoveCardAbilityPlayerMessage(playerActor, ability), processImmediately);
	}

	public static void ShortRestPlayer(CPlayerActor playerActor, CAbilityCard discardedCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate = false, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLShortRestPlayerMessage(playerActor, discardedCard, loseHealth, updateScenarioRNG, fromStateUpdate), processImmediately);
	}

	public static void ProxySetStartRoundDeckState(CPlayerActor playerActor, StartRoundCardState startRoundCardState, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLSetStartRoundDeckStateMessage(playerActor, startRoundCardState), processImmediately);
	}

	public static void GetAndReplicateStartRoundDeckState(CPlayerActor playerActor, int gameActionID, string fastReloadID, bool processImmediately = false)
	{
		ToggleIsHandStateMessagePending(val: true);
		AddSRLQueueMessage(new CSRLGetStartRoundDeckStateMessage(playerActor, gameActionID, fastReloadID), processImmediately);
	}

	public static void HandleLongRest(CPlayerActor playerActor, CAbilityCard cardToBurn, bool fromStateUpdate, string fastReloadID, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLHandleLongRestMessage(playerActor, cardToBurn, fromStateUpdate, fastReloadID), processImmediately);
	}

	public static void HandleImprovedShortRest(CPlayerActor playerActor, CAbilityCard cardToBurn, bool fromStateUpdate, string fastReloadID, bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLHandleLongRestMessage(playerActor, cardToBurn, fromStateUpdate, fastReloadID, improvedShortRest: true), processImmediately);
	}

	public static void ApplyScenarioRewards(bool won)
	{
		AddSRLQueueMessage(new CSRLApplyScenarioRewardsMessage(won), processImmediately: false);
	}

	public static bool CheckFinishedBonuses(CActor actor = null, bool important = false)
	{
		if (!important)
		{
			return false;
		}
		if (GameState.ThreadIsSleeping)
		{
			return false;
		}
		AddSRLQueueMessage(new CSRLCheckFinishedBonusesMessage(actor), processImmediately: false);
		return true;
	}

	public static List<CActor> GetActorsInRange(CActor targetActor, CActor filterActor, int range, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, CAreaEffect areaEffect, List<CTile> areaEffectTiles, bool isTargetedAbility, bool? canTargetInvisible = false)
	{
		return GameState.GetActorsInRange(targetActor, filterActor, range, actorsToIgnore, abilityFilter, areaEffect, areaEffectTiles, isTargetedAbility, null, canTargetInvisible);
	}

	public static void Stop(bool processImmediately = false)
	{
		AddSRLQueueMessage(new CSRLMessage(EMessageType.STOP), processImmediately);
	}

	public static double NextDouble(SharedLibrary.Random rnd, double min, double max)
	{
		return rnd.NextDouble() * (max - min) + min;
	}

	public static void FirstAbilityStarted()
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
		{
			((CPhaseAction)PhaseManager.Phase).OnFirstAbilityStarted();
		}
	}

	public static void ToggleIsHandStateMessagePending(bool val)
	{
		IsHandStateMessagePending = val;
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FHGUI.Elements;
using RuntimeInspectorNamespace;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI.Scenario.Services;

public class UndoActionViewService : FHScenarioViewService
{
	private delegate bool MessageHandleUndoButtonDelegate(CMessageData messageData, out bool interactablem, out UndoButton.EButtonState buttonState);

	private static Dictionary<CMessageData.MessageType, MessageHandleUndoButtonDelegate> _undoButtonHandlers;

	private static CAbility Ability => Choreographer.s_Choreographer.m_CurrentAbility;

	private static bool FirstAbility => Choreographer.s_Choreographer.FirstAbility;

	private static Choreographer.CWaitState m_WaitState => Choreographer.s_Choreographer.m_WaitState;

	private static CActor CurrentActor => Choreographer.s_Choreographer.CurrentActor;

	public UndoActionViewService(FHScenarioViewController view)
		: base(view)
	{
		if (_undoButtonHandlers == null)
		{
			PopulateHandlers();
		}
	}

	private static void PopulateHandlers()
	{
		_undoButtonHandlers = new Dictionary<CMessageData.MessageType, MessageHandleUndoButtonDelegate>();
		foreach (MethodInfo method in (from m in typeof(UndoActionViewService).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
			where m.HasAttribute<MessageHandlerAttribute>()
			select m).ToList())
		{
			MessageHandlerAttribute attribute = method.GetAttribute<MessageHandlerAttribute>();
			_undoButtonHandlers[attribute.MessageType] = delegate(CMessageData messageData, out bool interactable, out UndoButton.EButtonState buttonState)
			{
				object[] array = new object[3] { messageData, null, null };
				bool result = (bool)method.Invoke(null, array);
				interactable = (bool)array[1];
				buttonState = (UndoButton.EButtonState)array[2];
				return result;
			};
		}
	}

	public static bool TryGetUndoButtonParam(CActor actor, out ButtonsBarParam resultButtonParam)
	{
		if (_undoButtonHandlers == null)
		{
			PopulateHandlers();
		}
		FHScenarioViewController instance = Singleton<FHScenarioViewController>.Instance;
		bool flag = actor != null && actor.Type == CActor.EType.Player;
		bool interactablem = true;
		bool flag2 = true;
		UndoButton.EButtonState buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (!instance.IsTurnStarted || instance.CurrentTurnMessageData.Count == 0)
		{
			flag2 = false;
		}
		if (Ability == null || !flag)
		{
			flag2 = false;
		}
		if (TryGetUndowWaypointButtonParam(actor, out var resultButtonParam2))
		{
			resultButtonParam = resultButtonParam2;
			return true;
		}
		if (GameState.OverridingCurrentActor && Waypoint.s_Waypoints.Count <= 0 && (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == null || !(PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityChooseAbility { State: CAbilityTargeting.TargetingState.PreProcessingBeforeActorApplies })))
		{
			flag2 = false;
		}
		if (!flag2)
		{
			resultButtonParam = default;
			return false;
		}
		foreach (CMessageData currentTurnMessageDatum in instance.CurrentTurnMessageData)
		{
			if (_undoButtonHandlers.TryGetValue(currentTurnMessageDatum.m_Type, out var value))
			{
				flag2 = value(currentTurnMessageDatum, out interactablem, out buttonState);
			}
		}
		if (!flag2)
		{
			resultButtonParam = default;
			return false;
		}
		string labelKey = "GUI_UNDO";
		switch (buttonState)
		{
		case UndoButton.EButtonState.EUNDOBUTTONUNDO:
			labelKey = "GUI_UNDO";
			break;
		case UndoButton.EButtonState.EUNDOBUTTONUNDOWAYPOINT:
			labelKey = "GUI_UNDO_WAYPOINT";
			break;
		case UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS:
			labelKey = "GUI_CLEAR_TARGETS";
			break;
		}
		ButtonsBarParam buttonsBarParam = new ButtonsBarParam(labelKey, InputFHManager.Controls.Generic.Confirm, Choreographer.s_Choreographer.m_UndoButton.OnClick)
		{
			Interactable = interactablem
		};
		resultButtonParam = buttonsBarParam;
		return true;
	}

	[MessageHandler(CMessageData.MessageType.StartAbility)]
	private static bool TryGetUndo_StartAbility(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CStartActorAbility_MessageData cStartActorAbility_MessageData = (CStartActorAbility_MessageData)message;
		interactable = cStartActorAbility_MessageData.m_Ability.CanUndo && cStartActorAbility_MessageData.m_IsFirstAbility;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (message.m_ActorSpawningMessage.Type == CActor.EType.Player && cStartActorAbility_MessageData.m_Ability.CanUndo)
		{
			CAbility ability = cStartActorAbility_MessageData.m_Ability;
			if (ability != null)
			{
				AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.AutotriggerAbility.HasValue)
				{
					return !cStartActorAbility_MessageData.m_Ability.MiscAbilityData.AutotriggerAbility.Value;
				}
			}
			return true;
		}
		return false;
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingMoveTile)]
	private static bool TryGetUndo_ActorIsSelectingMoveTile(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		interactable = false;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		CActorIsSelectingMoveTile_MessageData cActorIsSelectingMoveTile_MessageData = (CActorIsSelectingMoveTile_MessageData)message;
		if (cActorIsSelectingMoveTile_MessageData.m_MoveAbility != null && cActorIsSelectingMoveTile_MessageData.m_MoveAbility.AbilityHasBeenCancelled)
		{
			return false;
		}
		bool result = message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		bool flag = message.m_ActorSpawningMessage is CPlayerActor;
		interactable = flag && cActorIsSelectingMoveTile_MessageData.m_MoveAbility.CanUndo && (FirstAbility || cActorIsSelectingMoveTile_MessageData.m_MoveAbility.IsItemAbility);
		return result;
	}

	[MessageHandler(CMessageData.MessageType.StartSecondMergedAbility)]
	private static bool TryGetUndo_StartSecondMergedAbility(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CStartSecondMergedAbility_MessageData cStartSecondMergedAbility_MessageData = (CStartSecondMergedAbility_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = cStartSecondMergedAbility_MessageData.m_Ability.CanUndo;
		if (message.m_ActorSpawningMessage.Type == CActor.EType.Player)
		{
			return FirstAbility;
		}
		return false;
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingAttackFocusTargets)]
	private static bool TryGetUndo_ActorIsSelectingAttackFocusTargets(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData = (CActorIsSelectingAttackFocusTargets_MessageData)message;
		bool flag = false;
		bool flag2 = cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor.Type == CActor.EType.Player;
		bool flag3 = flag2 && (!cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.ProcessIfDead || cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor.IsDead);
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (flag2 && cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.CanApplyActiveBonusTogglesTo() && cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.AbilityType != CAbility.EAbilityType.Attack)
		{
			flag = true;
		}
		interactable = true;
		if (!flag)
		{
			if (cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.AreaEffect != null)
			{
				if (!WorldspaceStarHexDisplay.Instance.IsAOELocked())
				{
					AbilityData.MiscAbilityData miscAbilityData = cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.MiscAbilityData;
					if ((miscAbilityData == null || miscAbilityData.ExactRange != true) && !cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.UseSubAbilityTargeting)
					{
						goto IL_0142;
					}
				}
				buttonState = UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS;
				return true;
			}
		}
		else if (!cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.ProcessIfDead)
		{
			if ((cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.ActorsToTarget.Count > 0) & flag3)
			{
				buttonState = ((!cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.AllTargets) ? UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS : UndoButton.EButtonState.EUNDOBUTTONUNDO);
				if (FirstAbility || !cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.AllTargets)
				{
					return cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.CanUndo;
				}
				return false;
			}
			if (cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.ChainAttack & flag2)
			{
				if (FirstAbility)
				{
					return cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility.CanUndo;
				}
				return false;
			}
		}
		goto IL_0142;
		IL_0142:
		return false;
	}

	[MessageHandler(CMessageData.MessageType.PlayerFinishedSelectingToAvoidDamageOrNot)]
	private static bool TryGetUndo_PlayerFinishedSelectingToAvoidDamageOrNot(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		_ = (CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		if (CurrentActor != null && CurrentActor.Type == CActor.EType.Player && PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction)
		{
			CAbility ability = cPhaseAction.CurrentPhaseAbility.m_Ability;
			if (ability.CanReceiveTileSelection())
			{
				interactable = ability.CanUndo && FirstAbility;
				return true;
			}
		}
		return false;
	}

	[MessageHandler(CMessageData.MessageType.ActorWantsAnActionConfirmation)]
	private static bool TryGetUndo_ActorWantsAnActionConfirmation(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorWantsAnActionConfirmation_MessageData cActorWantsAnActionConfirmation_MessageData = (CActorWantsAnActionConfirmation_MessageData)message;
		return TryGetUndo_Default(cActorWantsAnActionConfirmation_MessageData.m_Ability, cActorWantsAnActionConfirmation_MessageData.m_CanUndo, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorWantsAnActiveBonusConfirmation)]
	private static bool TryGetUndo_ActorWantsAnActiveBonusConfirmation(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorWantsAnActiveBonusConfirmation_MessageData cActorWantsAnActiveBonusConfirmation_MessageData = (CActorWantsAnActiveBonusConfirmation_MessageData)message;
		return TryGetUndo_Default(cActorWantsAnActiveBonusConfirmation_MessageData.m_Ability, cActorWantsAnActiveBonusConfirmation_MessageData.m_CanUndo, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorWantsAPerkConfirmation)]
	private static bool TryGetUndo_ActorWantsAPerkConfirmation(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorWantsAPerkConfirmation_MessageData cActorWantsAPerkConfirmation_MessageData = (CActorWantsAPerkConfirmation_MessageData)message;
		return TryGetUndo_Default(cActorWantsAPerkConfirmation_MessageData.m_Ability, cActorWantsAPerkConfirmation_MessageData.m_CanUndo, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorWantsAnAttackModifierConfirmation)]
	private static bool TryGetUndo_ActorWantsAnAttackModifierConfirmation(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorWantsAnAttackModifierConfirmation_MessageData cActorWantsAnAttackModifierConfirmation_MessageData = (CActorWantsAnAttackModifierConfirmation_MessageData)message;
		return TryGetUndo_Default(cActorWantsAnAttackModifierConfirmation_MessageData.m_Ability, cActorWantsAnAttackModifierConfirmation_MessageData.m_CanUndo, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ReturnToSummoner)]
	private static bool TryGetUndo_ReturnToSummoner(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		_ = (CReturnToSummoner_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = false;
		return true;
	}

	[MessageHandler(CMessageData.MessageType.SelectLoseCards)]
	private static bool TryGetUndo_SelectLoseCards(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Default2(((CSelectLoseCards_MessageData)message).m_Ability, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.SelectCardsWithActiveBonusTracker)]
	private static bool TryGetUndo_SelectCardsWithActiveBonusTracker(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Default2(((CSelectCardsWithActiveBonusTracker_MessageData)message).m_Ability, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.NoValidCardsForAbility)]
	private static bool TryGetUndo_NoValidCardsForAbility(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CNoValidCardsForAbility_MessageData cNoValidCardsForAbility_MessageData = (CNoValidCardsForAbility_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		return CPhaseAction.IsFirstAbility(cNoValidCardsForAbility_MessageData.m_Ability);
	}

	[MessageHandler(CMessageData.MessageType.GenericAbilityInvalidHelpText)]
	private static bool TryGetUndo_GenericAbilityInvalidHelpText(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CGenericAbilityInvalidHelpText_MessageData cGenericAbilityInvalidHelpText_MessageData = (CGenericAbilityInvalidHelpText_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		if (!CPhaseAction.IsFirstAbility(cGenericAbilityInvalidHelpText_MessageData.m_Ability))
		{
			return cGenericAbilityInvalidHelpText_MessageData.m_Ability.AllowUndoAfterFirst;
		}
		return true;
	}

	[MessageHandler(CMessageData.MessageType.PlayerSelectingObjectPosition)]
	private static bool TryGetUndo_PlayerSelectingObjectPosition(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CPlayerSelectingObjectPosition_MessageData cPlayerSelectingObjectPosition_MessageData = (CPlayerSelectingObjectPosition_MessageData)message;
		CPlayerActor cPlayerActor = GameState.GetControllingActor(cPlayerSelectingObjectPosition_MessageData.m_ActorSpawningMessage) as CPlayerActor;
		if (cPlayerActor == null)
		{
			cPlayerActor = cPlayerSelectingObjectPosition_MessageData.m_ActorSpawningMessage as CPlayerActor;
		}
		if (cPlayerActor == null)
		{
			cPlayerActor = GameState.HostCharacter;
		}
		bool flag = WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() > 0;
		buttonState = (flag ? UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS : UndoButton.EButtonState.EUNDOBUTTONUNDO);
		interactable = cPlayerSelectingObjectPosition_MessageData.m_Ability.CanUndo && ((flag && !cPlayerSelectingObjectPosition_MessageData.m_Ability.AllTargets) || FirstAbility || cPlayerSelectingObjectPosition_MessageData.m_Ability.AllowUndoAfterFirst);
		return cPlayerActor != null;
	}

	[MessageHandler(CMessageData.MessageType.ShowElementPicker)]
	private static bool TryGetUndo_ShowElementPicker(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CShowElementPicker_MessageData cShowElementPicker_MessageData = (CShowElementPicker_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		if (message.m_ActorSpawningMessage.Type == CActor.EType.Player && cShowElementPicker_MessageData.m_InfuseAbility.CanUndo)
		{
			return FirstAbility;
		}
		return false;
	}

	[MessageHandler(CMessageData.MessageType.ShowConsumes)]
	private static bool TryGetUndo_ShowConsumes(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CShowConsumes_MessageData cShowConsumes_MessageData = (CShowConsumes_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = cShowConsumes_MessageData.m_IsFirstAbility;
		return cShowConsumes_MessageData.m_Ability.CanUndo;
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingTeleport2Tile)]
	private static bool TryGetUndo_ActorIsSelectingTeleport2Tile(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorIsSelectingTeleport2Tile_MessageData cActorIsSelectingTeleport2Tile_MessageData = (CActorIsSelectingTeleport2Tile_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForMoveAnim)
		{
			interactable = cActorIsSelectingTeleport2Tile_MessageData.m_Teleport2Ability.CanUndo && FirstAbility;
			return message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		}
		List<CTile> teleportTiles = cActorIsSelectingTeleport2Tile_MessageData.m_Teleport2Ability.GetTeleportTiles(cActorIsSelectingTeleport2Tile_MessageData.m_Teleport2Ability.TargetingActor.ArrayIndex, cActorIsSelectingTeleport2Tile_MessageData.m_Teleport2Ability.Strength);
		if (cActorIsSelectingTeleport2Tile_MessageData.m_Teleport2Ability.GetVar("TargetShadows").GetBool() && teleportTiles.Count == 1)
		{
			interactable = FirstAbility;
			return true;
		}
		interactable = true;
		return true;
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingPullTile)]
	private static bool TryGetUndo_ActorIsSelectingPullTile(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorIsSelectingPullTile_MessageData cActorIsSelectingPullTile_MessageData = (CActorIsSelectingPullTile_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForMoveAnim)
		{
			bool flag = FirstAbility;
			if (cActorIsSelectingPullTile_MessageData.m_PullAbility.IsInlineSubAbility)
			{
				flag = PhaseManager.CurrentPhaseAction.FirstPhaseAbility?.m_Ability == cActorIsSelectingPullTile_MessageData.m_PullAbility;
			}
			interactable = cActorIsSelectingPullTile_MessageData.m_PullAbility.CanUndo & flag;
			return message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		}
		interactable = true;
		return true;
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingPushTile)]
	private static bool TryGetUndo_ActorIsSelectingPushTile(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		_ = (CActorIsSelectingPushTile_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForMoveAnim)
		{
			interactable = false;
			return message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		}
		interactable = true;
		return true;
	}

	[MessageHandler(CMessageData.MessageType.ShowChooseAbilityUI)]
	private static bool TryGetUndo_ShowChooseAbilityUI(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CShowChooseAbilityUI_MessageData cShowChooseAbilityUI_MessageData = (CShowChooseAbilityUI_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		if (cShowChooseAbilityUI_MessageData.m_ActorSpawningMessage is CPlayerActor)
		{
			interactable = cShowChooseAbilityUI_MessageData.m_ChooseAbility.CanUndo && FirstAbility;
			return message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		}
		return true;
	}

	[MessageHandler(CMessageData.MessageType.ShowEndOfActionToggleBonuses)]
	private static bool TryGetUndo_ShowEndOfActionToggleBonuses(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorSelectedNullHexes)]
	private static bool TryGetUndo_ActorSelectedNullHexes(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.FinishedProcessingClearTargets)]
	private static bool TryGetUndo_FinishedProcessingClearTargets(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CFinishedProcessingClearTargets_MessageData cFinishedProcessingClearTargets_MessageData = (CFinishedProcessingClearTargets_MessageData)message;
		CAbility ability = cFinishedProcessingClearTargets_MessageData.m_Ability;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		if ((FirstAbility || ability.IsItemAbility || cFinishedProcessingClearTargets_MessageData.m_Ability.AllowUndoAfterFirst) && ability.CanUndo)
		{
			return !ability.IsControlAbility;
		}
		return false;
	}

	[MessageHandler(CMessageData.MessageType.PlayerToSelectAbilityCardsOrLongRest)]
	private static bool TryGetUndo_PlayerToSelectAbilityCardsOrLongRest(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.PlayerToShortRest)]
	private static bool TryGetUndo_PlayerToShortRest(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActionSelectionPhaseStart)]
	private static bool TryGetUndo_ActionSelectionPhaseStart(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.PlayerSelectingToAvoidDamageOrNot)]
	private static bool TryGetUndo_PlayerSelectingToAvoidDamageOrNot(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorWantsALootCardConfirmation)]
	private static bool TryGetUndo_ActorWantsALootCardConfirmation(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingTargetingFocus)]
	private static bool TryGetUndo_ActorIsSelectingTargetingFocus(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = (CActorIsSelectingTargetingFocus_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		return cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility.CanUndo;
	}

	[MessageHandler(CMessageData.MessageType.ActorHasDamaged)]
	private static bool TryGetUndo_ActorHasDamaged(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.SelectRecoverCards)]
	private static bool TryGetUndo_SelectRecoverCards(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.SelectAndPlayCard)]
	private static bool TryGetUndo_SelectAndPlayCard(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.SelectAndMoveCard)]
	private static bool TryGetUndo_SelectAndMoveCard(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.SelectCard)]
	private static bool TryGetUndo_SelectCard(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.PlacingSpawn)]
	private static bool TryGetUndo_PlacingSpawn(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ExtraTurnCardsSelected)]
	private static bool TryGetUndo_ExtraTurnCardsSelected(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.EndAbility)]
	private static bool TryGetUndo_EndAbility(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Hidden(out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.SelectExtraTurnCards)]
	private static bool TryGetUndo_SelectExtraTurnCards(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		return TryGetUndo_Default2(((CSelectExtraTurnCards_MessageData)message).m_ExtraTurnAbility, out interactable, out buttonState);
	}

	[MessageHandler(CMessageData.MessageType.ActorIsSelectingDamageFocus)]
	private static bool TryGetUndo_ActorIsSelectingDamageFocus(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CActorIsSelectingDamageFocus_MessageData cActorIsSelectingDamageFocus_MessageData = (CActorIsSelectingDamageFocus_MessageData)message;
		bool flag = message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		interactable = true;
		if (flag && !cActorIsSelectingDamageFocus_MessageData.m_DamageAbility.AllTargets && cActorIsSelectingDamageFocus_MessageData.m_DamageAbility.ActorsToTarget.Count > 0)
		{
			buttonState = UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS;
			return true;
		}
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		if (FirstAbility && cActorIsSelectingDamageFocus_MessageData.m_DamageAbility.CanUndo)
		{
			return cActorIsSelectingDamageFocus_MessageData.m_DamageAbility.CanSkip;
		}
		return false;
	}

	[MessageHandler(CMessageData.MessageType.ShowSelectAbility)]
	private static bool TryGetUndo_ShowSelectAbility(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CShowSelectAbility_MessageData cShowSelectAbility_MessageData = (CShowSelectAbility_MessageData)message;
		CAbilityChooseAbility chooseAbility = cShowSelectAbility_MessageData.m_ChooseAbility;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		bool flag = chooseAbility.CanUndo && message.m_ActorSpawningMessage.Type == CActor.EType.Player;
		interactable = flag && cShowSelectAbility_MessageData.m_IsFirstAbility;
		return flag;
	}

	[MessageHandler(CMessageData.MessageType.FinishedProcessingTileSelected)]
	private static bool TryGetUndo_FinishedProcessingTileSelected(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CFinishedProcessingTileSelected_MessageData cFinishedProcessingTileSelected_MessageData = (CFinishedProcessingTileSelected_MessageData)message;
		if (PhaseManager.Phase.Type == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
		{
			interactable = cFinishedProcessingTileSelected_MessageData.m_Ability.CanUndo && (FirstAbility || cFinishedProcessingTileSelected_MessageData.m_Ability.AllowUndoAfterFirst);
			if ((cFinishedProcessingTileSelected_MessageData.m_Ability.AreaEffect != null || cFinishedProcessingTileSelected_MessageData.m_Ability.AreaEffectBackup != null) && !WorldspaceStarHexDisplay.Instance.IsAOELocked() && (cFinishedProcessingTileSelected_MessageData.m_Ability.ValidActorsInRange.Count > 0 || cFinishedProcessingTileSelected_MessageData.m_Ability.TilesSelected.Count > 0 || (cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget != null && cFinishedProcessingTileSelected_MessageData.m_Ability.ActorsToTarget.Count > 0)) && cFinishedProcessingTileSelected_MessageData.m_AreaEffectLocked && (!cFinishedProcessingTileSelected_MessageData.m_Ability.RequiresAllyTargeting() || cFinishedProcessingTileSelected_MessageData.m_Ability.ValidAlliesTargeted()))
			{
				buttonState = UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS;
				return true;
			}
		}
		else
		{
			_ = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cFinishedProcessingTileSelected_MessageData.m_SelectedTile.m_ArrayIndex.X, cFinishedProcessingTileSelected_MessageData.m_SelectedTile.m_ArrayIndex.Y];
			bool flag = WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() > 0;
			if ((!cFinishedProcessingTileSelected_MessageData.m_Ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) & flag) && (!cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets || FirstAbility) && ((!cFinishedProcessingTileSelected_MessageData.m_Ability.IsInlineSubAbility && !cFinishedProcessingTileSelected_MessageData.m_Ability.UseSubAbilityTargeting) || cFinishedProcessingTileSelected_MessageData.m_Ability.CanClearTargets()))
			{
				buttonState = ((!cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets && cFinishedProcessingTileSelected_MessageData.m_Ability.CanClearTargets()) ? UndoButton.EButtonState.EUNDOBUTTONCLEARTARGETS : UndoButton.EButtonState.EUNDOBUTTONUNDO);
				interactable = true;
				if (!((FirstAbility || !cFinishedProcessingTileSelected_MessageData.m_Ability.AllTargets) & flag) || !cFinishedProcessingTileSelected_MessageData.m_Ability.CanUndo)
				{
					if (!cFinishedProcessingTileSelected_MessageData.m_Ability.CanUndo)
					{
						return cFinishedProcessingTileSelected_MessageData.m_Ability.CanClearTargets();
					}
					return false;
				}
				return true;
			}
		}
		interactable = false;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		return false;
	}

	[MessageHandler(CMessageData.MessageType.FinishedProcessingTileDeselected)]
	private static bool TryGetUndo_FinishedProcessingTileDeselected(CMessageData message, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		CFinishedProcessingTileDeselected_MessageData cFinishedProcessingTileDeselected_MessageData = (CFinishedProcessingTileDeselected_MessageData)message;
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = false;
		if (PhaseManager.Phase.Type == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
		{
			if (cFinishedProcessingTileDeselected_MessageData.m_Ability.AreaEffect != null)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileCancelClickAudioItem);
				if (cFinishedProcessingTileDeselected_MessageData.m_Ability.MiscAbilityData != null && cFinishedProcessingTileDeselected_MessageData.m_Ability.MiscAbilityData.ExactRange.HasValue && cFinishedProcessingTileDeselected_MessageData.m_Ability.MiscAbilityData.ExactRange.Value)
				{
					interactable = cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility;
				}
				else if (WorldspaceStarHexDisplay.Instance.IsAOELocked())
				{
					interactable = cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility;
					return true;
				}
			}
			else
			{
				if (WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() <= 0 && cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility)
				{
					return true;
				}
				interactable = cFinishedProcessingTileDeselected_MessageData.m_Ability.CanUndo && FirstAbility;
			}
		}
		return false;
	}

	private static bool TryGetUndo_Default(CAbility ability, bool messageCanUndo, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		bool flag = ability != null;
		interactable = flag && ability.CanUndo && FirstAbility;
		return messageCanUndo;
	}

	private static bool TryGetUndo_Hidden(out bool interactable, out UndoButton.EButtonState buttonState)
	{
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = false;
		return false;
	}

	private static bool TryGetUndo_Default2(CAbility ability, out bool interactable, out UndoButton.EButtonState buttonState)
	{
		buttonState = UndoButton.EButtonState.EUNDOBUTTONUNDO;
		interactable = true;
		return ability.MiscAbilityData?.CanUndo == true;
	}

	private static bool TryGetUndowWaypointButtonParam(CActor actor, out ButtonsBarParam resultButtonParam)
	{
		if (Waypoint.s_Waypoints.Count > 0 && !Waypoint.s_LockWaypoints)
		{
			resultButtonParam = new ButtonsBarParam("GUI_UNDO_WAYPOINT", InputFHManager.Controls.Generic.Confirm, Choreographer.s_Choreographer.m_UndoButton.OnClick);
			return true;
		}
		resultButtonParam = default;
		return false;
	}
}

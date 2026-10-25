using System;
using System.Collections;
using System.Collections.Generic;
using DevLocker.GFrame.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

public class FHControls : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable, IInputContext
{
	public struct ScenarioActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction ClearTargetsOrUndo => m_Wrapper.m_Scenario_ClearTargetsOrUndo;

		public InputAction ConfirmAction => m_Wrapper.m_Scenario_ConfirmAction;

		public InputAction RetryScenario => m_Wrapper.m_Scenario_RetryScenario;

		public InputAction SkipAction => m_Wrapper.m_Scenario_SkipAction;

		public InputAction BurnOneCard => m_Wrapper.m_Scenario_BurnOneCard;

		public InputAction ReceiveDamage => m_Wrapper.m_Scenario_ReceiveDamage;

		public InputAction BurnTwoCards => m_Wrapper.m_Scenario_BurnTwoCards;

		public InputAction PrevActor => m_Wrapper.m_Scenario_PrevActor;

		public InputAction NextActor => m_Wrapper.m_Scenario_NextActor;

		public InputAction ClearActorHighlight => m_Wrapper.m_Scenario_ClearActorHighlight;

		public InputAction PrevUseItem => m_Wrapper.m_Scenario_PrevUseItem;

		public InputAction NextUseItem => m_Wrapper.m_Scenario_NextUseItem;

		public InputAction FocusUseItemBar => m_Wrapper.m_Scenario_FocusUseItemBar;

		public InputAction UnfocusUseItemBar => m_Wrapper.m_Scenario_UnfocusUseItemBar;

		public InputAction ToggleCharacterMode => m_Wrapper.m_Scenario_ToggleCharacterMode;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Scenario;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(ScenarioActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IScenarioActions instance)
		{
			if (instance != null && !m_Wrapper.m_ScenarioActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_ScenarioActionsCallbackInterfaces.Add(instance);
				ClearTargetsOrUndo.started += instance.OnClearTargetsOrUndo;
				ClearTargetsOrUndo.performed += instance.OnClearTargetsOrUndo;
				ClearTargetsOrUndo.canceled += instance.OnClearTargetsOrUndo;
				ConfirmAction.started += instance.OnConfirmAction;
				ConfirmAction.performed += instance.OnConfirmAction;
				ConfirmAction.canceled += instance.OnConfirmAction;
				RetryScenario.started += instance.OnRetryScenario;
				RetryScenario.performed += instance.OnRetryScenario;
				RetryScenario.canceled += instance.OnRetryScenario;
				SkipAction.started += instance.OnSkipAction;
				SkipAction.performed += instance.OnSkipAction;
				SkipAction.canceled += instance.OnSkipAction;
				BurnOneCard.started += instance.OnBurnOneCard;
				BurnOneCard.performed += instance.OnBurnOneCard;
				BurnOneCard.canceled += instance.OnBurnOneCard;
				ReceiveDamage.started += instance.OnReceiveDamage;
				ReceiveDamage.performed += instance.OnReceiveDamage;
				ReceiveDamage.canceled += instance.OnReceiveDamage;
				BurnTwoCards.started += instance.OnBurnTwoCards;
				BurnTwoCards.performed += instance.OnBurnTwoCards;
				BurnTwoCards.canceled += instance.OnBurnTwoCards;
				PrevActor.started += instance.OnPrevActor;
				PrevActor.performed += instance.OnPrevActor;
				PrevActor.canceled += instance.OnPrevActor;
				NextActor.started += instance.OnNextActor;
				NextActor.performed += instance.OnNextActor;
				NextActor.canceled += instance.OnNextActor;
				ClearActorHighlight.started += instance.OnClearActorHighlight;
				ClearActorHighlight.performed += instance.OnClearActorHighlight;
				ClearActorHighlight.canceled += instance.OnClearActorHighlight;
				PrevUseItem.started += instance.OnPrevUseItem;
				PrevUseItem.performed += instance.OnPrevUseItem;
				PrevUseItem.canceled += instance.OnPrevUseItem;
				NextUseItem.started += instance.OnNextUseItem;
				NextUseItem.performed += instance.OnNextUseItem;
				NextUseItem.canceled += instance.OnNextUseItem;
				FocusUseItemBar.started += instance.OnFocusUseItemBar;
				FocusUseItemBar.performed += instance.OnFocusUseItemBar;
				FocusUseItemBar.canceled += instance.OnFocusUseItemBar;
				UnfocusUseItemBar.started += instance.OnUnfocusUseItemBar;
				UnfocusUseItemBar.performed += instance.OnUnfocusUseItemBar;
				UnfocusUseItemBar.canceled += instance.OnUnfocusUseItemBar;
				ToggleCharacterMode.started += instance.OnToggleCharacterMode;
				ToggleCharacterMode.performed += instance.OnToggleCharacterMode;
				ToggleCharacterMode.canceled += instance.OnToggleCharacterMode;
			}
		}

		private void UnregisterCallbacks(IScenarioActions instance)
		{
			ClearTargetsOrUndo.started -= instance.OnClearTargetsOrUndo;
			ClearTargetsOrUndo.performed -= instance.OnClearTargetsOrUndo;
			ClearTargetsOrUndo.canceled -= instance.OnClearTargetsOrUndo;
			ConfirmAction.started -= instance.OnConfirmAction;
			ConfirmAction.performed -= instance.OnConfirmAction;
			ConfirmAction.canceled -= instance.OnConfirmAction;
			RetryScenario.started -= instance.OnRetryScenario;
			RetryScenario.performed -= instance.OnRetryScenario;
			RetryScenario.canceled -= instance.OnRetryScenario;
			SkipAction.started -= instance.OnSkipAction;
			SkipAction.performed -= instance.OnSkipAction;
			SkipAction.canceled -= instance.OnSkipAction;
			BurnOneCard.started -= instance.OnBurnOneCard;
			BurnOneCard.performed -= instance.OnBurnOneCard;
			BurnOneCard.canceled -= instance.OnBurnOneCard;
			ReceiveDamage.started -= instance.OnReceiveDamage;
			ReceiveDamage.performed -= instance.OnReceiveDamage;
			ReceiveDamage.canceled -= instance.OnReceiveDamage;
			BurnTwoCards.started -= instance.OnBurnTwoCards;
			BurnTwoCards.performed -= instance.OnBurnTwoCards;
			BurnTwoCards.canceled -= instance.OnBurnTwoCards;
			PrevActor.started -= instance.OnPrevActor;
			PrevActor.performed -= instance.OnPrevActor;
			PrevActor.canceled -= instance.OnPrevActor;
			NextActor.started -= instance.OnNextActor;
			NextActor.performed -= instance.OnNextActor;
			NextActor.canceled -= instance.OnNextActor;
			ClearActorHighlight.started -= instance.OnClearActorHighlight;
			ClearActorHighlight.performed -= instance.OnClearActorHighlight;
			ClearActorHighlight.canceled -= instance.OnClearActorHighlight;
			PrevUseItem.started -= instance.OnPrevUseItem;
			PrevUseItem.performed -= instance.OnPrevUseItem;
			PrevUseItem.canceled -= instance.OnPrevUseItem;
			NextUseItem.started -= instance.OnNextUseItem;
			NextUseItem.performed -= instance.OnNextUseItem;
			NextUseItem.canceled -= instance.OnNextUseItem;
			FocusUseItemBar.started -= instance.OnFocusUseItemBar;
			FocusUseItemBar.performed -= instance.OnFocusUseItemBar;
			FocusUseItemBar.canceled -= instance.OnFocusUseItemBar;
			UnfocusUseItemBar.started -= instance.OnUnfocusUseItemBar;
			UnfocusUseItemBar.performed -= instance.OnUnfocusUseItemBar;
			UnfocusUseItemBar.canceled -= instance.OnUnfocusUseItemBar;
			ToggleCharacterMode.started -= instance.OnToggleCharacterMode;
			ToggleCharacterMode.performed -= instance.OnToggleCharacterMode;
			ToggleCharacterMode.canceled -= instance.OnToggleCharacterMode;
		}

		public void RemoveCallbacks(IScenarioActions instance)
		{
			if (m_Wrapper.m_ScenarioActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IScenarioActions instance)
		{
			foreach (IScenarioActions scenarioActionsCallbackInterface in m_Wrapper.m_ScenarioActionsCallbackInterfaces)
			{
				UnregisterCallbacks(scenarioActionsCallbackInterface);
			}
			m_Wrapper.m_ScenarioActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct ScenarioWorldspaceActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction PickTarget => m_Wrapper.m_ScenarioWorldspace_PickTarget;

		public InputAction SwapTarget => m_Wrapper.m_ScenarioWorldspace_SwapTarget;

		public InputAction RotateTargetRight => m_Wrapper.m_ScenarioWorldspace_RotateTargetRight;

		public InputAction RotateTargetLeft => m_Wrapper.m_ScenarioWorldspace_RotateTargetLeft;

		public InputAction HighlightInteractablesOverlay => m_Wrapper.m_ScenarioWorldspace_HighlightInteractablesOverlay;

		public InputAction LineOfSightOverlay => m_Wrapper.m_ScenarioWorldspace_LineOfSightOverlay;

		public InputAction ToggleSpeedUp => m_Wrapper.m_ScenarioWorldspace_ToggleSpeedUp;

		public InputAction Ping => m_Wrapper.m_ScenarioWorldspace_Ping;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_ScenarioWorldspace;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(ScenarioWorldspaceActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IScenarioWorldspaceActions instance)
		{
			if (instance != null && !m_Wrapper.m_ScenarioWorldspaceActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_ScenarioWorldspaceActionsCallbackInterfaces.Add(instance);
				PickTarget.started += instance.OnPickTarget;
				PickTarget.performed += instance.OnPickTarget;
				PickTarget.canceled += instance.OnPickTarget;
				SwapTarget.started += instance.OnSwapTarget;
				SwapTarget.performed += instance.OnSwapTarget;
				SwapTarget.canceled += instance.OnSwapTarget;
				RotateTargetRight.started += instance.OnRotateTargetRight;
				RotateTargetRight.performed += instance.OnRotateTargetRight;
				RotateTargetRight.canceled += instance.OnRotateTargetRight;
				RotateTargetLeft.started += instance.OnRotateTargetLeft;
				RotateTargetLeft.performed += instance.OnRotateTargetLeft;
				RotateTargetLeft.canceled += instance.OnRotateTargetLeft;
				HighlightInteractablesOverlay.started += instance.OnHighlightInteractablesOverlay;
				HighlightInteractablesOverlay.performed += instance.OnHighlightInteractablesOverlay;
				HighlightInteractablesOverlay.canceled += instance.OnHighlightInteractablesOverlay;
				LineOfSightOverlay.started += instance.OnLineOfSightOverlay;
				LineOfSightOverlay.performed += instance.OnLineOfSightOverlay;
				LineOfSightOverlay.canceled += instance.OnLineOfSightOverlay;
				ToggleSpeedUp.started += instance.OnToggleSpeedUp;
				ToggleSpeedUp.performed += instance.OnToggleSpeedUp;
				ToggleSpeedUp.canceled += instance.OnToggleSpeedUp;
				Ping.started += instance.OnPing;
				Ping.performed += instance.OnPing;
				Ping.canceled += instance.OnPing;
			}
		}

		private void UnregisterCallbacks(IScenarioWorldspaceActions instance)
		{
			PickTarget.started -= instance.OnPickTarget;
			PickTarget.performed -= instance.OnPickTarget;
			PickTarget.canceled -= instance.OnPickTarget;
			SwapTarget.started -= instance.OnSwapTarget;
			SwapTarget.performed -= instance.OnSwapTarget;
			SwapTarget.canceled -= instance.OnSwapTarget;
			RotateTargetRight.started -= instance.OnRotateTargetRight;
			RotateTargetRight.performed -= instance.OnRotateTargetRight;
			RotateTargetRight.canceled -= instance.OnRotateTargetRight;
			RotateTargetLeft.started -= instance.OnRotateTargetLeft;
			RotateTargetLeft.performed -= instance.OnRotateTargetLeft;
			RotateTargetLeft.canceled -= instance.OnRotateTargetLeft;
			HighlightInteractablesOverlay.started -= instance.OnHighlightInteractablesOverlay;
			HighlightInteractablesOverlay.performed -= instance.OnHighlightInteractablesOverlay;
			HighlightInteractablesOverlay.canceled -= instance.OnHighlightInteractablesOverlay;
			LineOfSightOverlay.started -= instance.OnLineOfSightOverlay;
			LineOfSightOverlay.performed -= instance.OnLineOfSightOverlay;
			LineOfSightOverlay.canceled -= instance.OnLineOfSightOverlay;
			ToggleSpeedUp.started -= instance.OnToggleSpeedUp;
			ToggleSpeedUp.performed -= instance.OnToggleSpeedUp;
			ToggleSpeedUp.canceled -= instance.OnToggleSpeedUp;
			Ping.started -= instance.OnPing;
			Ping.performed -= instance.OnPing;
			Ping.canceled -= instance.OnPing;
		}

		public void RemoveCallbacks(IScenarioWorldspaceActions instance)
		{
			if (m_Wrapper.m_ScenarioWorldspaceActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IScenarioWorldspaceActions instance)
		{
			foreach (IScenarioWorldspaceActions scenarioWorldspaceActionsCallbackInterface in m_Wrapper.m_ScenarioWorldspaceActionsCallbackInterfaces)
			{
				UnregisterCallbacks(scenarioWorldspaceActionsCallbackInterface);
			}
			m_Wrapper.m_ScenarioWorldspaceActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct ScenarioDecksActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction OpenViewAllCardsWindow => m_Wrapper.m_ScenarioDecks_OpenViewAllCardsWindow;

		public InputAction ViewHeroDeck1 => m_Wrapper.m_ScenarioDecks_ViewHeroDeck1;

		public InputAction ViewHeroDeck2 => m_Wrapper.m_ScenarioDecks_ViewHeroDeck2;

		public InputAction ViewHeroDeck3 => m_Wrapper.m_ScenarioDecks_ViewHeroDeck3;

		public InputAction ViewHeroDeck4 => m_Wrapper.m_ScenarioDecks_ViewHeroDeck4;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_ScenarioDecks;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(ScenarioDecksActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IScenarioDecksActions instance)
		{
			if (instance != null && !m_Wrapper.m_ScenarioDecksActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_ScenarioDecksActionsCallbackInterfaces.Add(instance);
				OpenViewAllCardsWindow.started += instance.OnOpenViewAllCardsWindow;
				OpenViewAllCardsWindow.performed += instance.OnOpenViewAllCardsWindow;
				OpenViewAllCardsWindow.canceled += instance.OnOpenViewAllCardsWindow;
				ViewHeroDeck1.started += instance.OnViewHeroDeck1;
				ViewHeroDeck1.performed += instance.OnViewHeroDeck1;
				ViewHeroDeck1.canceled += instance.OnViewHeroDeck1;
				ViewHeroDeck2.started += instance.OnViewHeroDeck2;
				ViewHeroDeck2.performed += instance.OnViewHeroDeck2;
				ViewHeroDeck2.canceled += instance.OnViewHeroDeck2;
				ViewHeroDeck3.started += instance.OnViewHeroDeck3;
				ViewHeroDeck3.performed += instance.OnViewHeroDeck3;
				ViewHeroDeck3.canceled += instance.OnViewHeroDeck3;
				ViewHeroDeck4.started += instance.OnViewHeroDeck4;
				ViewHeroDeck4.performed += instance.OnViewHeroDeck4;
				ViewHeroDeck4.canceled += instance.OnViewHeroDeck4;
			}
		}

		private void UnregisterCallbacks(IScenarioDecksActions instance)
		{
			OpenViewAllCardsWindow.started -= instance.OnOpenViewAllCardsWindow;
			OpenViewAllCardsWindow.performed -= instance.OnOpenViewAllCardsWindow;
			OpenViewAllCardsWindow.canceled -= instance.OnOpenViewAllCardsWindow;
			ViewHeroDeck1.started -= instance.OnViewHeroDeck1;
			ViewHeroDeck1.performed -= instance.OnViewHeroDeck1;
			ViewHeroDeck1.canceled -= instance.OnViewHeroDeck1;
			ViewHeroDeck2.started -= instance.OnViewHeroDeck2;
			ViewHeroDeck2.performed -= instance.OnViewHeroDeck2;
			ViewHeroDeck2.canceled -= instance.OnViewHeroDeck2;
			ViewHeroDeck3.started -= instance.OnViewHeroDeck3;
			ViewHeroDeck3.performed -= instance.OnViewHeroDeck3;
			ViewHeroDeck3.canceled -= instance.OnViewHeroDeck3;
			ViewHeroDeck4.started -= instance.OnViewHeroDeck4;
			ViewHeroDeck4.performed -= instance.OnViewHeroDeck4;
			ViewHeroDeck4.canceled -= instance.OnViewHeroDeck4;
		}

		public void RemoveCallbacks(IScenarioDecksActions instance)
		{
			if (m_Wrapper.m_ScenarioDecksActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IScenarioDecksActions instance)
		{
			foreach (IScenarioDecksActions scenarioDecksActionsCallbackInterface in m_Wrapper.m_ScenarioDecksActionsCallbackInterfaces)
			{
				UnregisterCallbacks(scenarioDecksActionsCallbackInterface);
			}
			m_Wrapper.m_ScenarioDecksActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct CameraActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction Movement => m_Wrapper.m_Camera_Movement;

		public InputAction Zoom => m_Wrapper.m_Camera_Zoom;

		public InputAction Rotate => m_Wrapper.m_Camera_Rotate;

		public InputAction DragRotate => m_Wrapper.m_Camera_DragRotate;

		public InputAction DragMove => m_Wrapper.m_Camera_DragMove;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Camera;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(CameraActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(ICameraActions instance)
		{
			if (instance != null && !m_Wrapper.m_CameraActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_CameraActionsCallbackInterfaces.Add(instance);
				Movement.started += instance.OnMovement;
				Movement.performed += instance.OnMovement;
				Movement.canceled += instance.OnMovement;
				Zoom.started += instance.OnZoom;
				Zoom.performed += instance.OnZoom;
				Zoom.canceled += instance.OnZoom;
				Rotate.started += instance.OnRotate;
				Rotate.performed += instance.OnRotate;
				Rotate.canceled += instance.OnRotate;
				DragRotate.started += instance.OnDragRotate;
				DragRotate.performed += instance.OnDragRotate;
				DragRotate.canceled += instance.OnDragRotate;
				DragMove.started += instance.OnDragMove;
				DragMove.performed += instance.OnDragMove;
				DragMove.canceled += instance.OnDragMove;
			}
		}

		private void UnregisterCallbacks(ICameraActions instance)
		{
			Movement.started -= instance.OnMovement;
			Movement.performed -= instance.OnMovement;
			Movement.canceled -= instance.OnMovement;
			Zoom.started -= instance.OnZoom;
			Zoom.performed -= instance.OnZoom;
			Zoom.canceled -= instance.OnZoom;
			Rotate.started -= instance.OnRotate;
			Rotate.performed -= instance.OnRotate;
			Rotate.canceled -= instance.OnRotate;
			DragRotate.started -= instance.OnDragRotate;
			DragRotate.performed -= instance.OnDragRotate;
			DragRotate.canceled -= instance.OnDragRotate;
			DragMove.started -= instance.OnDragMove;
			DragMove.performed -= instance.OnDragMove;
			DragMove.canceled -= instance.OnDragMove;
		}

		public void RemoveCallbacks(ICameraActions instance)
		{
			if (m_Wrapper.m_CameraActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(ICameraActions instance)
		{
			foreach (ICameraActions cameraActionsCallbackInterface in m_Wrapper.m_CameraActionsCallbackInterfaces)
			{
				UnregisterCallbacks(cameraActionsCallbackInterface);
			}
			m_Wrapper.m_CameraActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct CameraFreeActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction FreeCamMovement => m_Wrapper.m_CameraFree_FreeCamMovement;

		public InputAction FreeCamRotate => m_Wrapper.m_CameraFree_FreeCamRotate;

		public InputAction FreeCamMoveVertical => m_Wrapper.m_CameraFree_FreeCamMoveVertical;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_CameraFree;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(CameraFreeActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(ICameraFreeActions instance)
		{
			if (instance != null && !m_Wrapper.m_CameraFreeActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_CameraFreeActionsCallbackInterfaces.Add(instance);
				FreeCamMovement.started += instance.OnFreeCamMovement;
				FreeCamMovement.performed += instance.OnFreeCamMovement;
				FreeCamMovement.canceled += instance.OnFreeCamMovement;
				FreeCamRotate.started += instance.OnFreeCamRotate;
				FreeCamRotate.performed += instance.OnFreeCamRotate;
				FreeCamRotate.canceled += instance.OnFreeCamRotate;
				FreeCamMoveVertical.started += instance.OnFreeCamMoveVertical;
				FreeCamMoveVertical.performed += instance.OnFreeCamMoveVertical;
				FreeCamMoveVertical.canceled += instance.OnFreeCamMoveVertical;
			}
		}

		private void UnregisterCallbacks(ICameraFreeActions instance)
		{
			FreeCamMovement.started -= instance.OnFreeCamMovement;
			FreeCamMovement.performed -= instance.OnFreeCamMovement;
			FreeCamMovement.canceled -= instance.OnFreeCamMovement;
			FreeCamRotate.started -= instance.OnFreeCamRotate;
			FreeCamRotate.performed -= instance.OnFreeCamRotate;
			FreeCamRotate.canceled -= instance.OnFreeCamRotate;
			FreeCamMoveVertical.started -= instance.OnFreeCamMoveVertical;
			FreeCamMoveVertical.performed -= instance.OnFreeCamMoveVertical;
			FreeCamMoveVertical.canceled -= instance.OnFreeCamMoveVertical;
		}

		public void RemoveCallbacks(ICameraFreeActions instance)
		{
			if (m_Wrapper.m_CameraFreeActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(ICameraFreeActions instance)
		{
			foreach (ICameraFreeActions cameraFreeActionsCallbackInterface in m_Wrapper.m_CameraFreeActionsCallbackInterfaces)
			{
				UnregisterCallbacks(cameraFreeActionsCallbackInterface);
			}
			m_Wrapper.m_CameraFreeActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct OutpostActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction Save => m_Wrapper.m_Outpost_Save;

		public InputAction NextPhase => m_Wrapper.m_Outpost_NextPhase;

		public InputAction NextNarration => m_Wrapper.m_Outpost_NextNarration;

		public InputAction ReplayNarration => m_Wrapper.m_Outpost_ReplayNarration;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Outpost;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(OutpostActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IOutpostActions instance)
		{
			if (instance != null && !m_Wrapper.m_OutpostActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_OutpostActionsCallbackInterfaces.Add(instance);
				Save.started += instance.OnSave;
				Save.performed += instance.OnSave;
				Save.canceled += instance.OnSave;
				NextPhase.started += instance.OnNextPhase;
				NextPhase.performed += instance.OnNextPhase;
				NextPhase.canceled += instance.OnNextPhase;
				NextNarration.started += instance.OnNextNarration;
				NextNarration.performed += instance.OnNextNarration;
				NextNarration.canceled += instance.OnNextNarration;
				ReplayNarration.started += instance.OnReplayNarration;
				ReplayNarration.performed += instance.OnReplayNarration;
				ReplayNarration.canceled += instance.OnReplayNarration;
			}
		}

		private void UnregisterCallbacks(IOutpostActions instance)
		{
			Save.started -= instance.OnSave;
			Save.performed -= instance.OnSave;
			Save.canceled -= instance.OnSave;
			NextPhase.started -= instance.OnNextPhase;
			NextPhase.performed -= instance.OnNextPhase;
			NextPhase.canceled -= instance.OnNextPhase;
			NextNarration.started -= instance.OnNextNarration;
			NextNarration.performed -= instance.OnNextNarration;
			NextNarration.canceled -= instance.OnNextNarration;
			ReplayNarration.started -= instance.OnReplayNarration;
			ReplayNarration.performed -= instance.OnReplayNarration;
			ReplayNarration.canceled -= instance.OnReplayNarration;
		}

		public void RemoveCallbacks(IOutpostActions instance)
		{
			if (m_Wrapper.m_OutpostActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IOutpostActions instance)
		{
			foreach (IOutpostActions outpostActionsCallbackInterface in m_Wrapper.m_OutpostActionsCallbackInterfaces)
			{
				UnregisterCallbacks(outpostActionsCallbackInterface);
			}
			m_Wrapper.m_OutpostActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct UIActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Click => m_Wrapper.m_UI_Click;

		public InputAction ScrollWheel => m_Wrapper.m_UI_ScrollWheel;

		public InputAction MiddleClick => m_Wrapper.m_UI_MiddleClick;

		public InputAction RightClick => m_Wrapper.m_UI_RightClick;

		public InputAction TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;

		public InputAction TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;

		public InputAction ShowInfo => m_Wrapper.m_UI_ShowInfo;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Click.started += instance.OnClick;
				Click.performed += instance.OnClick;
				Click.canceled += instance.OnClick;
				ScrollWheel.started += instance.OnScrollWheel;
				ScrollWheel.performed += instance.OnScrollWheel;
				ScrollWheel.canceled += instance.OnScrollWheel;
				MiddleClick.started += instance.OnMiddleClick;
				MiddleClick.performed += instance.OnMiddleClick;
				MiddleClick.canceled += instance.OnMiddleClick;
				RightClick.started += instance.OnRightClick;
				RightClick.performed += instance.OnRightClick;
				RightClick.canceled += instance.OnRightClick;
				TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
				ShowInfo.started += instance.OnShowInfo;
				ShowInfo.performed += instance.OnShowInfo;
				ShowInfo.canceled += instance.OnShowInfo;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
			Submit.started -= instance.OnSubmit;
			Submit.performed -= instance.OnSubmit;
			Submit.canceled -= instance.OnSubmit;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			Click.started -= instance.OnClick;
			Click.performed -= instance.OnClick;
			Click.canceled -= instance.OnClick;
			ScrollWheel.started -= instance.OnScrollWheel;
			ScrollWheel.performed -= instance.OnScrollWheel;
			ScrollWheel.canceled -= instance.OnScrollWheel;
			MiddleClick.started -= instance.OnMiddleClick;
			MiddleClick.performed -= instance.OnMiddleClick;
			MiddleClick.canceled -= instance.OnMiddleClick;
			RightClick.started -= instance.OnRightClick;
			RightClick.performed -= instance.OnRightClick;
			RightClick.canceled -= instance.OnRightClick;
			TrackedDevicePosition.started -= instance.OnTrackedDevicePosition;
			TrackedDevicePosition.performed -= instance.OnTrackedDevicePosition;
			TrackedDevicePosition.canceled -= instance.OnTrackedDevicePosition;
			TrackedDeviceOrientation.started -= instance.OnTrackedDeviceOrientation;
			TrackedDeviceOrientation.performed -= instance.OnTrackedDeviceOrientation;
			TrackedDeviceOrientation.canceled -= instance.OnTrackedDeviceOrientation;
			ShowInfo.started -= instance.OnShowInfo;
			ShowInfo.performed -= instance.OnShowInfo;
			ShowInfo.canceled -= instance.OnShowInfo;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct GenericActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction Back => m_Wrapper.m_Generic_Back;

		public InputAction Confirm => m_Wrapper.m_Generic_Confirm;

		public InputAction AlternativeChoice => m_Wrapper.m_Generic_AlternativeChoice;

		public InputAction NextSection => m_Wrapper.m_Generic_NextSection;

		public InputAction PrevSection => m_Wrapper.m_Generic_PrevSection;

		public InputAction NextSubsection => m_Wrapper.m_Generic_NextSubsection;

		public InputAction PrevSubsection => m_Wrapper.m_Generic_PrevSubsection;

		public InputAction NextListEntry => m_Wrapper.m_Generic_NextListEntry;

		public InputAction PrevListEntry => m_Wrapper.m_Generic_PrevListEntry;

		public InputAction HorizontalScroll => m_Wrapper.m_Generic_HorizontalScroll;

		public InputAction VerticalScroll => m_Wrapper.m_Generic_VerticalScroll;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Generic;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(GenericActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IGenericActions instance)
		{
			if (instance != null && !m_Wrapper.m_GenericActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_GenericActionsCallbackInterfaces.Add(instance);
				Back.started += instance.OnBack;
				Back.performed += instance.OnBack;
				Back.canceled += instance.OnBack;
				Confirm.started += instance.OnConfirm;
				Confirm.performed += instance.OnConfirm;
				Confirm.canceled += instance.OnConfirm;
				AlternativeChoice.started += instance.OnAlternativeChoice;
				AlternativeChoice.performed += instance.OnAlternativeChoice;
				AlternativeChoice.canceled += instance.OnAlternativeChoice;
				NextSection.started += instance.OnNextSection;
				NextSection.performed += instance.OnNextSection;
				NextSection.canceled += instance.OnNextSection;
				PrevSection.started += instance.OnPrevSection;
				PrevSection.performed += instance.OnPrevSection;
				PrevSection.canceled += instance.OnPrevSection;
				NextSubsection.started += instance.OnNextSubsection;
				NextSubsection.performed += instance.OnNextSubsection;
				NextSubsection.canceled += instance.OnNextSubsection;
				PrevSubsection.started += instance.OnPrevSubsection;
				PrevSubsection.performed += instance.OnPrevSubsection;
				PrevSubsection.canceled += instance.OnPrevSubsection;
				NextListEntry.started += instance.OnNextListEntry;
				NextListEntry.performed += instance.OnNextListEntry;
				NextListEntry.canceled += instance.OnNextListEntry;
				PrevListEntry.started += instance.OnPrevListEntry;
				PrevListEntry.performed += instance.OnPrevListEntry;
				PrevListEntry.canceled += instance.OnPrevListEntry;
				HorizontalScroll.started += instance.OnHorizontalScroll;
				HorizontalScroll.performed += instance.OnHorizontalScroll;
				HorizontalScroll.canceled += instance.OnHorizontalScroll;
				VerticalScroll.started += instance.OnVerticalScroll;
				VerticalScroll.performed += instance.OnVerticalScroll;
				VerticalScroll.canceled += instance.OnVerticalScroll;
			}
		}

		private void UnregisterCallbacks(IGenericActions instance)
		{
			Back.started -= instance.OnBack;
			Back.performed -= instance.OnBack;
			Back.canceled -= instance.OnBack;
			Confirm.started -= instance.OnConfirm;
			Confirm.performed -= instance.OnConfirm;
			Confirm.canceled -= instance.OnConfirm;
			AlternativeChoice.started -= instance.OnAlternativeChoice;
			AlternativeChoice.performed -= instance.OnAlternativeChoice;
			AlternativeChoice.canceled -= instance.OnAlternativeChoice;
			NextSection.started -= instance.OnNextSection;
			NextSection.performed -= instance.OnNextSection;
			NextSection.canceled -= instance.OnNextSection;
			PrevSection.started -= instance.OnPrevSection;
			PrevSection.performed -= instance.OnPrevSection;
			PrevSection.canceled -= instance.OnPrevSection;
			NextSubsection.started -= instance.OnNextSubsection;
			NextSubsection.performed -= instance.OnNextSubsection;
			NextSubsection.canceled -= instance.OnNextSubsection;
			PrevSubsection.started -= instance.OnPrevSubsection;
			PrevSubsection.performed -= instance.OnPrevSubsection;
			PrevSubsection.canceled -= instance.OnPrevSubsection;
			NextListEntry.started -= instance.OnNextListEntry;
			NextListEntry.performed -= instance.OnNextListEntry;
			NextListEntry.canceled -= instance.OnNextListEntry;
			PrevListEntry.started -= instance.OnPrevListEntry;
			PrevListEntry.performed -= instance.OnPrevListEntry;
			PrevListEntry.canceled -= instance.OnPrevListEntry;
			HorizontalScroll.started -= instance.OnHorizontalScroll;
			HorizontalScroll.performed -= instance.OnHorizontalScroll;
			HorizontalScroll.canceled -= instance.OnHorizontalScroll;
			VerticalScroll.started -= instance.OnVerticalScroll;
			VerticalScroll.performed -= instance.OnVerticalScroll;
			VerticalScroll.canceled -= instance.OnVerticalScroll;
		}

		public void RemoveCallbacks(IGenericActions instance)
		{
			if (m_Wrapper.m_GenericActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IGenericActions instance)
		{
			foreach (IGenericActions genericActionsCallbackInterface in m_Wrapper.m_GenericActionsCallbackInterfaces)
			{
				UnregisterCallbacks(genericActionsCallbackInterface);
			}
			m_Wrapper.m_GenericActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct SharedActions(FHControls wrapper)
	{
		private FHControls m_Wrapper = wrapper;

		public InputAction ShowPauseMenu => m_Wrapper.m_Shared_ShowPauseMenu;

		public InputAction HidePauseMenu => m_Wrapper.m_Shared_HidePauseMenu;

		public InputAction ShowSettings => m_Wrapper.m_Shared_ShowSettings;

		public InputAction HideSettings => m_Wrapper.m_Shared_HideSettings;

		public InputAction Close => m_Wrapper.m_Shared_Close;

		public InputAction QuickSave => m_Wrapper.m_Shared_QuickSave;

		public InputAction QuickLoad => m_Wrapper.m_Shared_QuickLoad;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Shared;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(SharedActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(ISharedActions instance)
		{
			if (instance != null && !m_Wrapper.m_SharedActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_SharedActionsCallbackInterfaces.Add(instance);
				ShowPauseMenu.started += instance.OnShowPauseMenu;
				ShowPauseMenu.performed += instance.OnShowPauseMenu;
				ShowPauseMenu.canceled += instance.OnShowPauseMenu;
				HidePauseMenu.started += instance.OnHidePauseMenu;
				HidePauseMenu.performed += instance.OnHidePauseMenu;
				HidePauseMenu.canceled += instance.OnHidePauseMenu;
				ShowSettings.started += instance.OnShowSettings;
				ShowSettings.performed += instance.OnShowSettings;
				ShowSettings.canceled += instance.OnShowSettings;
				HideSettings.started += instance.OnHideSettings;
				HideSettings.performed += instance.OnHideSettings;
				HideSettings.canceled += instance.OnHideSettings;
				Close.started += instance.OnClose;
				Close.performed += instance.OnClose;
				Close.canceled += instance.OnClose;
				QuickSave.started += instance.OnQuickSave;
				QuickSave.performed += instance.OnQuickSave;
				QuickSave.canceled += instance.OnQuickSave;
				QuickLoad.started += instance.OnQuickLoad;
				QuickLoad.performed += instance.OnQuickLoad;
				QuickLoad.canceled += instance.OnQuickLoad;
			}
		}

		private void UnregisterCallbacks(ISharedActions instance)
		{
			ShowPauseMenu.started -= instance.OnShowPauseMenu;
			ShowPauseMenu.performed -= instance.OnShowPauseMenu;
			ShowPauseMenu.canceled -= instance.OnShowPauseMenu;
			HidePauseMenu.started -= instance.OnHidePauseMenu;
			HidePauseMenu.performed -= instance.OnHidePauseMenu;
			HidePauseMenu.canceled -= instance.OnHidePauseMenu;
			ShowSettings.started -= instance.OnShowSettings;
			ShowSettings.performed -= instance.OnShowSettings;
			ShowSettings.canceled -= instance.OnShowSettings;
			HideSettings.started -= instance.OnHideSettings;
			HideSettings.performed -= instance.OnHideSettings;
			HideSettings.canceled -= instance.OnHideSettings;
			Close.started -= instance.OnClose;
			Close.performed -= instance.OnClose;
			Close.canceled -= instance.OnClose;
			QuickSave.started -= instance.OnQuickSave;
			QuickSave.performed -= instance.OnQuickSave;
			QuickSave.canceled -= instance.OnQuickSave;
			QuickLoad.started -= instance.OnQuickLoad;
			QuickLoad.performed -= instance.OnQuickLoad;
			QuickLoad.canceled -= instance.OnQuickLoad;
		}

		public void RemoveCallbacks(ISharedActions instance)
		{
			if (m_Wrapper.m_SharedActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(ISharedActions instance)
		{
			foreach (ISharedActions sharedActionsCallbackInterface in m_Wrapper.m_SharedActionsCallbackInterfaces)
			{
				UnregisterCallbacks(sharedActionsCallbackInterface);
			}
			m_Wrapper.m_SharedActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IScenarioActions
	{
		void OnClearTargetsOrUndo(InputAction.CallbackContext context);

		void OnConfirmAction(InputAction.CallbackContext context);

		void OnRetryScenario(InputAction.CallbackContext context);

		void OnSkipAction(InputAction.CallbackContext context);

		void OnBurnOneCard(InputAction.CallbackContext context);

		void OnReceiveDamage(InputAction.CallbackContext context);

		void OnBurnTwoCards(InputAction.CallbackContext context);

		void OnPrevActor(InputAction.CallbackContext context);

		void OnNextActor(InputAction.CallbackContext context);

		void OnClearActorHighlight(InputAction.CallbackContext context);

		void OnPrevUseItem(InputAction.CallbackContext context);

		void OnNextUseItem(InputAction.CallbackContext context);

		void OnFocusUseItemBar(InputAction.CallbackContext context);

		void OnUnfocusUseItemBar(InputAction.CallbackContext context);

		void OnToggleCharacterMode(InputAction.CallbackContext context);
	}

	public interface IScenarioWorldspaceActions
	{
		void OnPickTarget(InputAction.CallbackContext context);

		void OnSwapTarget(InputAction.CallbackContext context);

		void OnRotateTargetRight(InputAction.CallbackContext context);

		void OnRotateTargetLeft(InputAction.CallbackContext context);

		void OnHighlightInteractablesOverlay(InputAction.CallbackContext context);

		void OnLineOfSightOverlay(InputAction.CallbackContext context);

		void OnToggleSpeedUp(InputAction.CallbackContext context);

		void OnPing(InputAction.CallbackContext context);
	}

	public interface IScenarioDecksActions
	{
		void OnOpenViewAllCardsWindow(InputAction.CallbackContext context);

		void OnViewHeroDeck1(InputAction.CallbackContext context);

		void OnViewHeroDeck2(InputAction.CallbackContext context);

		void OnViewHeroDeck3(InputAction.CallbackContext context);

		void OnViewHeroDeck4(InputAction.CallbackContext context);
	}

	public interface ICameraActions
	{
		void OnMovement(InputAction.CallbackContext context);

		void OnZoom(InputAction.CallbackContext context);

		void OnRotate(InputAction.CallbackContext context);

		void OnDragRotate(InputAction.CallbackContext context);

		void OnDragMove(InputAction.CallbackContext context);
	}

	public interface ICameraFreeActions
	{
		void OnFreeCamMovement(InputAction.CallbackContext context);

		void OnFreeCamRotate(InputAction.CallbackContext context);

		void OnFreeCamMoveVertical(InputAction.CallbackContext context);
	}

	public interface IOutpostActions
	{
		void OnSave(InputAction.CallbackContext context);

		void OnNextPhase(InputAction.CallbackContext context);

		void OnNextNarration(InputAction.CallbackContext context);

		void OnReplayNarration(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnNavigate(InputAction.CallbackContext context);

		void OnSubmit(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnClick(InputAction.CallbackContext context);

		void OnScrollWheel(InputAction.CallbackContext context);

		void OnMiddleClick(InputAction.CallbackContext context);

		void OnRightClick(InputAction.CallbackContext context);

		void OnTrackedDevicePosition(InputAction.CallbackContext context);

		void OnTrackedDeviceOrientation(InputAction.CallbackContext context);

		void OnShowInfo(InputAction.CallbackContext context);
	}

	public interface IGenericActions
	{
		void OnBack(InputAction.CallbackContext context);

		void OnConfirm(InputAction.CallbackContext context);

		void OnAlternativeChoice(InputAction.CallbackContext context);

		void OnNextSection(InputAction.CallbackContext context);

		void OnPrevSection(InputAction.CallbackContext context);

		void OnNextSubsection(InputAction.CallbackContext context);

		void OnPrevSubsection(InputAction.CallbackContext context);

		void OnNextListEntry(InputAction.CallbackContext context);

		void OnPrevListEntry(InputAction.CallbackContext context);

		void OnHorizontalScroll(InputAction.CallbackContext context);

		void OnVerticalScroll(InputAction.CallbackContext context);
	}

	public interface ISharedActions
	{
		void OnShowPauseMenu(InputAction.CallbackContext context);

		void OnHidePauseMenu(InputAction.CallbackContext context);

		void OnShowSettings(InputAction.CallbackContext context);

		void OnHideSettings(InputAction.CallbackContext context);

		void OnClose(InputAction.CallbackContext context);

		void OnQuickSave(InputAction.CallbackContext context);

		void OnQuickLoad(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Scenario;

	private List<IScenarioActions> m_ScenarioActionsCallbackInterfaces = new List<IScenarioActions>();

	private readonly InputAction m_Scenario_ClearTargetsOrUndo;

	private readonly InputAction m_Scenario_ConfirmAction;

	private readonly InputAction m_Scenario_RetryScenario;

	private readonly InputAction m_Scenario_SkipAction;

	private readonly InputAction m_Scenario_BurnOneCard;

	private readonly InputAction m_Scenario_ReceiveDamage;

	private readonly InputAction m_Scenario_BurnTwoCards;

	private readonly InputAction m_Scenario_PrevActor;

	private readonly InputAction m_Scenario_NextActor;

	private readonly InputAction m_Scenario_ClearActorHighlight;

	private readonly InputAction m_Scenario_PrevUseItem;

	private readonly InputAction m_Scenario_NextUseItem;

	private readonly InputAction m_Scenario_FocusUseItemBar;

	private readonly InputAction m_Scenario_UnfocusUseItemBar;

	private readonly InputAction m_Scenario_ToggleCharacterMode;

	private readonly InputActionMap m_ScenarioWorldspace;

	private List<IScenarioWorldspaceActions> m_ScenarioWorldspaceActionsCallbackInterfaces = new List<IScenarioWorldspaceActions>();

	private readonly InputAction m_ScenarioWorldspace_PickTarget;

	private readonly InputAction m_ScenarioWorldspace_SwapTarget;

	private readonly InputAction m_ScenarioWorldspace_RotateTargetRight;

	private readonly InputAction m_ScenarioWorldspace_RotateTargetLeft;

	private readonly InputAction m_ScenarioWorldspace_HighlightInteractablesOverlay;

	private readonly InputAction m_ScenarioWorldspace_LineOfSightOverlay;

	private readonly InputAction m_ScenarioWorldspace_ToggleSpeedUp;

	private readonly InputAction m_ScenarioWorldspace_Ping;

	private readonly InputActionMap m_ScenarioDecks;

	private List<IScenarioDecksActions> m_ScenarioDecksActionsCallbackInterfaces = new List<IScenarioDecksActions>();

	private readonly InputAction m_ScenarioDecks_OpenViewAllCardsWindow;

	private readonly InputAction m_ScenarioDecks_ViewHeroDeck1;

	private readonly InputAction m_ScenarioDecks_ViewHeroDeck2;

	private readonly InputAction m_ScenarioDecks_ViewHeroDeck3;

	private readonly InputAction m_ScenarioDecks_ViewHeroDeck4;

	private readonly InputActionMap m_Camera;

	private List<ICameraActions> m_CameraActionsCallbackInterfaces = new List<ICameraActions>();

	private readonly InputAction m_Camera_Movement;

	private readonly InputAction m_Camera_Zoom;

	private readonly InputAction m_Camera_Rotate;

	private readonly InputAction m_Camera_DragRotate;

	private readonly InputAction m_Camera_DragMove;

	private readonly InputActionMap m_CameraFree;

	private List<ICameraFreeActions> m_CameraFreeActionsCallbackInterfaces = new List<ICameraFreeActions>();

	private readonly InputAction m_CameraFree_FreeCamMovement;

	private readonly InputAction m_CameraFree_FreeCamRotate;

	private readonly InputAction m_CameraFree_FreeCamMoveVertical;

	private readonly InputActionMap m_Outpost;

	private List<IOutpostActions> m_OutpostActionsCallbackInterfaces = new List<IOutpostActions>();

	private readonly InputAction m_Outpost_Save;

	private readonly InputAction m_Outpost_NextPhase;

	private readonly InputAction m_Outpost_NextNarration;

	private readonly InputAction m_Outpost_ReplayNarration;

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputAction m_UI_TrackedDevicePosition;

	private readonly InputAction m_UI_TrackedDeviceOrientation;

	private readonly InputAction m_UI_ShowInfo;

	private readonly InputActionMap m_Generic;

	private List<IGenericActions> m_GenericActionsCallbackInterfaces = new List<IGenericActions>();

	private readonly InputAction m_Generic_Back;

	private readonly InputAction m_Generic_Confirm;

	private readonly InputAction m_Generic_AlternativeChoice;

	private readonly InputAction m_Generic_NextSection;

	private readonly InputAction m_Generic_PrevSection;

	private readonly InputAction m_Generic_NextSubsection;

	private readonly InputAction m_Generic_PrevSubsection;

	private readonly InputAction m_Generic_NextListEntry;

	private readonly InputAction m_Generic_PrevListEntry;

	private readonly InputAction m_Generic_HorizontalScroll;

	private readonly InputAction m_Generic_VerticalScroll;

	private readonly InputActionMap m_Shared;

	private List<ISharedActions> m_SharedActionsCallbackInterfaces = new List<ISharedActions>();

	private readonly InputAction m_Shared_ShowPauseMenu;

	private readonly InputAction m_Shared_HidePauseMenu;

	private readonly InputAction m_Shared_ShowSettings;

	private readonly InputAction m_Shared_HideSettings;

	private readonly InputAction m_Shared_Close;

	private readonly InputAction m_Shared_QuickSave;

	private readonly InputAction m_Shared_QuickLoad;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	private int m_TouchSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public ScenarioActions Scenario => new ScenarioActions(this);

	public ScenarioWorldspaceActions ScenarioWorldspace => new ScenarioWorldspaceActions(this);

	public ScenarioDecksActions ScenarioDecks => new ScenarioDecksActions(this);

	public CameraActions Camera => new CameraActions(this);

	public CameraFreeActions CameraFree => new CameraFreeActions(this);

	public OutpostActions Outpost => new OutpostActions(this);

	public UIActions UI => new UIActions(this);

	public GenericActions Generic => new GenericActions(this);

	public SharedActions Shared => new SharedActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public InputControlScheme TouchScheme
	{
		get
		{
			if (m_TouchSchemeIndex == -1)
			{
				m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
			}
			return asset.controlSchemes[m_TouchSchemeIndex];
		}
	}

	public IInputContext InputContext { get; private set; }

	public IInputContext.InputBehaviours DefaultBehaviours => InputContext.DefaultBehaviours;

	public bool DeviceSupportsUINavigationSelection => InputContext.DeviceSupportsUINavigationSelection;

	public InputDevice ForcedDevice
	{
		get
		{
			return InputContext.ForcedDevice;
		}
		set
		{
			InputContext.ForcedDevice = value;
		}
	}

	public InputActionsMaskedStack InputActionsMaskedStack => InputContext.InputActionsMaskedStack;

	public InputUser User => InputContext.User;

	public ReadOnlyArray<InputDevice> PairedDevices => InputContext.PairedDevices;

	public event Action LastUsedDeviceChanged
	{
		add
		{
			InputContext.LastUsedDeviceChanged += value;
		}
		remove
		{
			InputContext.LastUsedDeviceChanged -= value;
		}
	}

	public event Action LastUsedInputControlSchemeChanged
	{
		add
		{
			InputContext.LastUsedInputControlSchemeChanged += value;
		}
		remove
		{
			InputContext.LastUsedInputControlSchemeChanged -= value;
		}
	}

	public FHControls()
	{
		asset = InputActionAsset.FromJson("{\n    \"version\": 1,\n    \"name\": \"FHControls\",\n    \"maps\": [\n        {\n            \"name\": \"Scenario\",\n            \"id\": \"a84b5132-8a57-4da6-84cb-cae7d2334c0e\",\n            \"actions\": [\n                {\n                    \"name\": \"ClearTargetsOrUndo\",\n                    \"type\": \"Button\",\n                    \"id\": \"b88a5ba3-39f2-4c70-9c68-9313d68987b2\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ConfirmAction\",\n                    \"type\": \"Button\",\n                    \"id\": \"9c64cd99-d62c-4cf6-b283-95381248fe4e\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RetryScenario\",\n                    \"type\": \"Button\",\n                    \"id\": \"09261e1c-7951-4cff-88bc-8fc019044c91\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SkipAction\",\n                    \"type\": \"Button\",\n                    \"id\": \"8dcf74dd-9534-49b9-98f7-5ffe22cc5945\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"BurnOneCard\",\n                    \"type\": \"Button\",\n                    \"id\": \"1cfbdc43-f73a-49c5-a9db-aa69d48c0497\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ReceiveDamage\",\n                    \"type\": \"Button\",\n                    \"id\": \"d2cdf753-6372-4cfa-91ca-d2062178363b\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"BurnTwoCards\",\n                    \"type\": \"Button\",\n                    \"id\": \"ce9d076d-6123-436c-ac90-0ab8a036e1a9\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrevActor\",\n                    \"type\": \"Button\",\n                    \"id\": \"978cced6-7a3d-40ee-a10b-73ac50a39cbf\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextActor\",\n                    \"type\": \"Button\",\n                    \"id\": \"912573e9-65e3-4a1e-9679-6a8ff46b7226\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ClearActorHighlight\",\n                    \"type\": \"Button\",\n                    \"id\": \"5f0aae4d-2d29-4997-b013-f0966335a3ef\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrevUseItem\",\n                    \"type\": \"Button\",\n                    \"id\": \"85a8b332-ef34-464c-985d-a38fe5b70c27\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextUseItem\",\n                    \"type\": \"Button\",\n                    \"id\": \"5a4fe592-e869-4c59-bd10-ebb49db9dab3\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"FocusUseItemBar\",\n                    \"type\": \"Button\",\n                    \"id\": \"d8ceeab3-e4e5-4bdf-8917-3709da77d94c\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"UnfocusUseItemBar\",\n                    \"type\": \"Button\",\n                    \"id\": \"55de8b11-cafd-4f3d-8eb7-c458544275f6\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleCharacterMode\",\n                    \"type\": \"Button\",\n                    \"id\": \"93954825-225b-44aa-af93-729d8b4c7fdf\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"b399377a-ec4e-45f9-a434-5bc1a84ae279\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ClearTargetsOrUndo\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"55622d89-d9b2-4481-bb06-fd7cb83fe272\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ClearTargetsOrUndo\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"aac1d84c-ed5e-4537-b938-70dc714e96ca\",\n                    \"path\": \"*/{Back}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad;Keyboard&Mouse\",\n                    \"action\": \"ClearTargetsOrUndo\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9450a457-a794-46cb-8a2d-d5ffcb8d49b3\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ConfirmAction\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"13c46352-b188-4ef4-aea2-a05e403e339d\",\n                    \"path\": \"<Keyboard>/enter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ConfirmAction\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f49c214e-818a-4743-97d7-4d93350ef3a6\",\n                    \"path\": \"<Keyboard>/numpadEnter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ConfirmAction\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"399c9606-1868-43b5-916e-3b41f9281be8\",\n                    \"path\": \"*/{Submit}\",\n                    \"interactions\": \"Hold\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad;Keyboard&Mouse\",\n                    \"action\": \"ConfirmAction\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e65362e8-c04d-4724-baf8-f0fe1a8d801e\",\n                    \"path\": \"<Keyboard>/b\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SkipAction\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5b4533b4-7d56-4c18-b604-901bda184c62\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"SkipAction\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6717b6dc-5b4a-480a-9d92-2023e43dbfca\",\n                    \"path\": \"<Keyboard>/x\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"BurnOneCard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a37e690c-ca57-4e4a-a088-bed137e08ae6\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"BurnOneCard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"da27fefc-40d5-48ef-9279-782420e17640\",\n                    \"path\": \"<Keyboard>/c\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ReceiveDamage\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e737f92e-8c16-4188-bd3c-00da1aa371cc\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"Hold\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ReceiveDamage\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8abae7e5-5fd5-4eb2-91f0-73fe6ead3536\",\n                    \"path\": \"<Keyboard>/v\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"BurnTwoCards\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0b223783-ee8d-4d62-bd39-ca84db5c90ff\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"BurnTwoCards\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e6d41eae-977b-4987-811b-627e89ef72ee\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PrevActor\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"982b191c-3948-44ab-afa2-6b38c0035340\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextActor\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5527e9a8-6421-4a4c-afda-cb2c201afe7e\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a0428d57-ab98-4083-9b1f-0b8ba44b20c8\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6775fbc2-712b-4e8f-ad0a-f46232923fa8\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"821114ed-7944-4927-9494-b0d23b883b4c\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1b89c62a-02dd-41d2-b114-73a0d680c69b\",\n                    \"path\": \"<Gamepad>/dpad/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"42022b77-2115-4dac-9d40-f3f40d8a7403\",\n                    \"path\": \"<Gamepad>/dpad/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"973d201a-cbc9-4c13-ad0c-00d7f48cef0f\",\n                    \"path\": \"<Gamepad>/dpad/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8c5b6d0a-edf3-4d85-8aba-6f75bfc80444\",\n                    \"path\": \"<Gamepad>/dpad/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"bf9535ae-741e-416f-ad11-2a6256011d64\",\n                    \"path\": \"<Gamepad>/leftStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"daffa3b0-984b-45e5-8c04-3091a72aae41\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5e742ffa-979c-48c7-a0b0-55a84dbecc4d\",\n                    \"path\": \"<Gamepad>/rightStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"daa7ef10-666b-4693-b863-936be74a659a\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4951d9d6-2e2c-4717-bac9-c47c067e82fc\",\n                    \"path\": \"<Gamepad>/select\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f961817a-cecf-43f5-ab93-3e502f3c9927\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ClearActorHighlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"47efd298-c255-4c00-8a0e-9ca6facd9f13\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PrevUseItem\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e7d62ad7-f78c-46a3-87ce-4dd33c2d8a49\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextUseItem\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"db59b450-7f49-4b3a-a035-9289aed86be4\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1c804cc2-4613-4954-b438-01a62c7d885f\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1d6479fd-5edf-49cc-8876-f1672ed28215\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cfbea515-1ac6-4482-925a-eb507104321e\",\n                    \"path\": \"<Gamepad>/leftStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c0ec4afa-7f90-4982-82d9-e4ac129df418\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7fae2de3-8556-4f7d-b051-b96942312114\",\n                    \"path\": \"<Gamepad>/rightStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"822bfab3-a6fd-4b28-9e30-cd026b7b80f0\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"83e9d572-69ed-421c-a37f-bc9fe142f243\",\n                    \"path\": \"<Gamepad>/select\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UnfocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7c21ad8e-83e6-48b4-9877-9063e58705cb\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"FocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1949d19b-fad8-4ebc-a18e-5ad831299759\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"FocusUseItemBar\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cabb9d2d-8001-4854-be13-b5c96e029a19\",\n                    \"path\": \"*/{Back}\",\n                    \"interactions\": \"Hold\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"RetryScenario\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c6bc571b-04aa-40da-996a-0be7864ea0ab\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ToggleCharacterMode\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"ScenarioWorldspace\",\n            \"id\": \"34d7edc0-d77a-42cb-a43d-950fd6198840\",\n            \"actions\": [\n                {\n                    \"name\": \"PickTarget\",\n                    \"type\": \"Button\",\n                    \"id\": \"3a92489f-62b1-4ed1-b1fe-e0758c717aec\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SwapTarget\",\n                    \"type\": \"Button\",\n                    \"id\": \"bbe24441-4eda-4bfc-9493-42dae0c33e95\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RotateTargetRight\",\n                    \"type\": \"Button\",\n                    \"id\": \"3553ba58-849a-45f2-9eb3-c11d14cc89be\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RotateTargetLeft\",\n                    \"type\": \"Button\",\n                    \"id\": \"92637e15-c938-45e5-af5c-4f806c202fd2\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HighlightInteractablesOverlay\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"fc1771a3-5189-4074-9a84-30f0547be43c\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"LineOfSightOverlay\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"de7f45ec-29f2-4cbb-93cb-9094a687ca7d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleSpeedUp\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"4324ab46-17fe-4ed5-928b-456565af15a2\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Ping\",\n                    \"type\": \"Button\",\n                    \"id\": \"f02f5722-963c-43d8-ae82-48224339363d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"e7d74eaa-8317-4944-b29a-cc49d3ed3177\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"Tap\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PickTarget\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0df6aceb-6983-4aa0-b246-fea5d2a5e522\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"PickTarget\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f04561c4-ced8-476a-99e4-14fe99d68a56\",\n                    \"path\": \"<Keyboard>/end\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ToggleSpeedUp\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"05cc0a08-285f-47e2-b838-6c6a490bfdda\",\n                    \"path\": \"<Gamepad>/leftStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Ping\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"CTRL+Click\",\n                    \"id\": \"fb1f88f4-4cbf-4436-b818-2137a9deb548\",\n                    \"path\": \"OneModifier\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Ping\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"binding\",\n                    \"id\": \"bce7bea9-6e63-4b73-b3f7-34f5f0f2b67c\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Ping\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"modifier\",\n                    \"id\": \"acf399b1-8223-4546-84a4-f4b810939a5e\",\n                    \"path\": \"<Keyboard>/ctrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Ping\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d657c6c7-3862-4b35-84de-cde733ee1cbe\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"RotateTargetLeft\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4548da10-22e6-4880-937b-6ba56b68fa0b\",\n                    \"path\": \"<Keyboard>/comma\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"RotateTargetLeft\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1648662a-b5a5-428f-8547-daf7beddde69\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"RotateTargetRight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cca114ea-6e35-483a-8f16-b7cdac57d9d2\",\n                    \"path\": \"<Keyboard>/r\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"RotateTargetRight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f1e7d9ca-db54-4dc1-baca-1c6b978a3688\",\n                    \"path\": \"<Keyboard>/period\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"RotateTargetRight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e83b9c26-3971-4c11-9b64-be4c23f71aed\",\n                    \"path\": \"<Gamepad>/rightStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"HighlightInteractablesOverlay\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c651e7b8-c4c6-4cae-ad4e-9b6be9e94927\",\n                    \"path\": \"<Keyboard>/tab\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"HighlightInteractablesOverlay\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0a666ee4-cf6b-4b91-9488-3f21990db7b4\",\n                    \"path\": \"<Gamepad>/rightStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"LineOfSightOverlay\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"3ef1927f-583f-478e-921c-d4465d313275\",\n                    \"path\": \"<Keyboard>/l\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"LineOfSightOverlay\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e019fc02-0a9d-4e70-a07c-c3759dd2450f\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapTarget\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"ScenarioDecks\",\n            \"id\": \"34f7d4e3-886b-4c08-8c41-e026f7024d64\",\n            \"actions\": [\n                {\n                    \"name\": \"OpenViewAllCardsWindow\",\n                    \"type\": \"Button\",\n                    \"id\": \"cc9949f6-7340-4b9c-b969-8cfe711d9613\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ViewHeroDeck1\",\n                    \"type\": \"Button\",\n                    \"id\": \"827bd4d5-bf88-482f-b331-5369fcf7453e\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ViewHeroDeck2\",\n                    \"type\": \"Button\",\n                    \"id\": \"087bf677-6929-424d-89c4-3ca8e76fb3b4\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ViewHeroDeck3\",\n                    \"type\": \"Button\",\n                    \"id\": \"c2ff7f82-6901-4080-b7a7-65a0768f0ecc\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ViewHeroDeck4\",\n                    \"type\": \"Button\",\n                    \"id\": \"f136c4fb-a0b8-4666-8abc-942086316fd9\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"cccd22d4-d43d-447f-a82d-962a71b994e5\",\n                    \"path\": \"<Keyboard>/alt\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"OpenViewAllCardsWindow\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"20ca1bf5-b52a-4827-ae6e-da836a01a393\",\n                    \"path\": \"<Keyboard>/1\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ViewHeroDeck1\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9deb13f6-2a3a-4c83-bdae-a2e1a037b3a2\",\n                    \"path\": \"<Keyboard>/2\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ViewHeroDeck2\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"19651b5b-dabe-4926-bb77-f92a6374685d\",\n                    \"path\": \"<Keyboard>/3\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ViewHeroDeck3\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ab083221-a009-4e7a-bc61-a38e7d7c4003\",\n                    \"path\": \"<Keyboard>/4\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ViewHeroDeck4\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Camera\",\n            \"id\": \"13eb8769-dc77-4aba-80f4-06619289673c\",\n            \"actions\": [\n                {\n                    \"name\": \"Movement\",\n                    \"type\": \"Value\",\n                    \"id\": \"72e57917-055a-469f-aa38-1bee88bcfcb9\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Zoom\",\n                    \"type\": \"Value\",\n                    \"id\": \"1ed04289-fd42-4935-8fa3-24b06841877e\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Rotate\",\n                    \"type\": \"Value\",\n                    \"id\": \"d9ac757d-487f-4f2f-ba6a-ff66945b47a2\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"DragRotate\",\n                    \"type\": \"Button\",\n                    \"id\": \"97f721fd-b494-403b-badb-ebeef7f4d819\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"DragMove\",\n                    \"type\": \"Button\",\n                    \"id\": \"7e76bb67-d616-46da-aa0e-9924c26d96ea\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"WASD\",\n                    \"id\": \"7a8ce22c-c87d-45fe-a764-803982125d54\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"312f65ca-81aa-4ceb-9da9-d6824700ed41\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"fc0ef8a7-e9ed-47c7-b952-d97bc81a441e\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"2fab64bd-cae6-4b00-b2a9-99430241dcb4\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"e78479c0-0288-4261-b832-5ae88fe621f6\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"AllArrows\",\n                    \"id\": \"70d667e4-13b6-42d7-9c62-72b967b1d79c\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"0a0f3933-5502-4dd7-b6b4-5d0315082190\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"41138a9e-cdf3-4581-a9ba-5e1d220c15b9\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"bd746ade-4a87-4711-99a2-4b4f7b7ad5cd\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"87b1289a-b50d-49ae-bf93-ac137ccda21b\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"55e1bec5-4723-4fc6-8467-0a90f48c9b04\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Movement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"ScrollWheel\",\n                    \"id\": \"0b5cd452-1ac1-4acd-b2b0-c88147ce7b55\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=0.05)\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"8643cabf-8d1a-47f1-add9-8f6b806b4e28\",\n                    \"path\": \"<Mouse>/scroll/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"c5fa20c3-ecc1-4d0c-a47b-a4ceab8134b6\",\n                    \"path\": \"<Mouse>/scroll/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"PageUpDown\",\n                    \"id\": \"24b2502a-d9dc-48d3-9f88-b1ca5d399a44\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"4cc40c99-f9d3-4e06-be91-daf90a808788\",\n                    \"path\": \"<Keyboard>/pageDown\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"b211dd11-609e-44c3-a21b-773ca2467298\",\n                    \"path\": \"<Keyboard>/pageUp\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"PlusMinus\",\n                    \"id\": \"8c5aa785-2670-43f7-8ca7-64fbaed39914\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"b5d6378d-46de-450f-88c6-63e3870b3441\",\n                    \"path\": \"<Keyboard>/numpadMinus\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"519b151a-9edf-4f25-927a-76a3fab6517c\",\n                    \"path\": \"<Keyboard>/numpadPlus\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Right Stick\",\n                    \"id\": \"1e2f62b1-43b9-4ec2-9bf6-25946620acfb\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"824b34ee-1cfc-4634-97b8-309ab92b3809\",\n                    \"path\": \"<Gamepad>/rightStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"36e4d8fc-f3fb-46bd-8ef6-d7efebaeb465\",\n                    \"path\": \"<Gamepad>/rightStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"QE\",\n                    \"id\": \"3c29dbd4-506e-471a-8c08-87ab898dc89b\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"29ff1379-1647-41db-a4c4-18cdf875e8c2\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"98e747ff-cc1a-4c6d-a964-ebc991424c8d\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Right Stick\",\n                    \"id\": \"cd0f9fd1-0994-4e26-9ffe-96fa36c76fac\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"528694bc-8f07-4c8b-b5be-27ac41aca9c3\",\n                    \"path\": \"<Gamepad>/rightStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"2ad63a78-47d0-48bb-ae42-d288d5de976f\",\n                    \"path\": \"<Gamepad>/rightStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5ab654e5-aceb-4320-b08e-32e5af8a9b43\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"DragRotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"One Modifier\",\n                    \"id\": \"7cad0504-836e-4324-ba62-e901ef77ab46\",\n                    \"path\": \"OneModifier\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"DragMove\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"modifier\",\n                    \"id\": \"8034209b-b670-4fef-bbac-51ac66137a0a\",\n                    \"path\": \"<Keyboard>/ctrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"DragMove\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"binding\",\n                    \"id\": \"5bf6d5d7-845d-47fd-a995-b350ae0292d5\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"DragMove\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"83e84089-18a2-445e-baf7-dc29210f5824\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"DragMove\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"CameraFree\",\n            \"id\": \"64a356e7-064b-4c22-8195-a132c1b79b6f\",\n            \"actions\": [\n                {\n                    \"name\": \"FreeCamMovement\",\n                    \"type\": \"Value\",\n                    \"id\": \"a6e0b5e9-b60b-4327-8217-680c71019154\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"FreeCamRotate\",\n                    \"type\": \"Value\",\n                    \"id\": \"0aaab2dd-ed68-4bf3-bb0b-fb9f2eb9cb31\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"FreeCamMoveVertical\",\n                    \"type\": \"Value\",\n                    \"id\": \"217cbc8e-155a-45b9-b599-1567abdbeadc\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"WASD\",\n                    \"id\": \"99516825-645b-4e6d-b6f1-43a82faf30ac\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"3b3d0b4a-4d9f-4c71-ac2f-cd47280465d1\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"55cc49e4-7741-4ab0-8189-c0edbb5f4414\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"15b3ef29-7331-4778-9b31-4251871c7bea\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"8aca7c5a-a03f-40b0-8c3e-20c8403d626f\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"AllArrows\",\n                    \"id\": \"14c67769-420d-4afc-8498-67014bd95b1c\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"99074900-5c30-4e6d-af3e-92311b84e22f\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"8b32071b-db27-40b9-b69d-1eefd07a642f\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"61be530c-a0d8-400e-a86f-e594507d6d68\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"dfca62bd-c68b-4e4a-8a42-5d837ae6ecdc\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0ef9f789-1e2f-40f7-9780-e5420566fc83\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"FreeCamMovement\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2ed97895-c2a4-4ae4-a742-0413e921b26e\",\n                    \"path\": \"<Mouse>/delta\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamRotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"33de4c5c-fb01-4874-887c-4fcf052c009e\",\n                    \"path\": \"<Gamepad>/rightStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"ScaleVector2(x=8,y=8)\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"FreeCamRotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"QE\",\n                    \"id\": \"c631c40b-5c1b-4aa2-b6c3-b53207db6239\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"FreeCamMoveVertical\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"324fb44c-e9bc-4b1e-84ef-84b2b416143f\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMoveVertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"ee85e172-419a-4252-b27d-446eedd617ee\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"FreeCamMoveVertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Triggers\",\n                    \"id\": \"141213ff-7a32-463b-83cf-334bb599c709\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"FreeCamMoveVertical\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"053ad671-4d1d-4e12-b7ad-58919de368ae\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"FreeCamMoveVertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"8e9c651a-ab2a-4845-8218-f020c99b10a9\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"FreeCamMoveVertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                }\n            ]\n        },\n        {\n            \"name\": \"Outpost\",\n            \"id\": \"022d814a-7425-469c-8778-51f6ad1689ba\",\n            \"actions\": [\n                {\n                    \"name\": \"Save\",\n                    \"type\": \"Button\",\n                    \"id\": \"7ca786d3-c50a-4dc6-b47d-31a536efa889\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextPhase\",\n                    \"type\": \"Button\",\n                    \"id\": \"0ed098ed-2110-412c-ba0e-b2a4d822bea1\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextNarration\",\n                    \"type\": \"Button\",\n                    \"id\": \"4f000544-c663-4392-8b28-6a18c2ced550\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ReplayNarration\",\n                    \"type\": \"Button\",\n                    \"id\": \"a01f6e76-81fc-447e-a57e-1f3a707b4166\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"35ae1654-dbbe-4594-b4f2-152fef460f81\",\n                    \"path\": \"<Gamepad>/select\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Save\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e7f59653-fe56-40f7-b0b3-e9e8071b7e08\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextPhase\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"bf628773-f004-421a-a58c-65e2a0566745\",\n                    \"path\": \"<DualShockGamepad>/touchpadButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Save\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a2e7049a-5a83-418b-a9d0-21b4ba63df43\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NextNarration\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8ced3874-5386-4c32-9375-a88006515c8f\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextNarration\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e8b747d0-fd22-49da-a0ca-545bb003c5da\",\n                    \"path\": \"<Keyboard>/R\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ReplayNarration\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb7a40ac-eb7e-40ab-844e-f40f7f35fca5\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ReplayNarration\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"UI\",\n            \"id\": \"36699a9d-0fc8-428a-941c-cd7c0d64be36\",\n            \"actions\": [\n                {\n                    \"name\": \"Navigate\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"54525046-29d2-4cda-aaec-a9b82a378ec7\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Submit\",\n                    \"type\": \"Button\",\n                    \"id\": \"279b44f2-1205-4c28-b887-362c4943c659\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Tap\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Cancel\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"dcea39f7-1e91-4a47-9e1d-a65ca4cd0e06\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Point\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"ad9e3e2f-9e9a-45e9-99ee-8b5cdc9c4f35\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Click\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"2be8c320-66bd-4c7d-928e-3b58efec8203\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ScrollWheel\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"b7ba00c9-cd6e-428c-b724-11b6ec118b42\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MiddleClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"98a7491d-0609-49ac-aa6d-b8ebb3518e89\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RightClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"9eaf26e4-8d08-406c-aba9-1318e475f693\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"TrackedDevicePosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"dc03e0d8-fd60-4eda-87e9-e73abdf8057b\",\n                    \"expectedControlType\": \"Vector3\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"TrackedDeviceOrientation\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"90d54a8c-9c4d-47ec-9510-e5322715eff2\",\n                    \"expectedControlType\": \"Quaternion\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ShowInfo\",\n                    \"type\": \"Button\",\n                    \"id\": \"e19b19d6-f7a1-46f8-918c-9aa5f87f989f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"Press\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\n                    \"path\": \"<Gamepad>/dpad\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Gamepad\",\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf\",\n                    \"path\": \"<Gamepad>/leftStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"2db08d65-c5fb-421b-983f-c71163608d67\",\n                    \"path\": \"<Gamepad>/leftStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"8ba04515-75aa-45de-966d-393d9bbd1c14\",\n                    \"path\": \"<Gamepad>/leftStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"fcd248ae-a788-4676-a12e-f4d81205600b\",\n                    \"path\": \"<Gamepad>/leftStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Joystick\",\n                    \"id\": \"e25d9774-381c-4a61-b47c-7b6b299ad9f9\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Keyboard\",\n                    \"id\": \"ff527021-f211-4c02-933e-5976594c46ed\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"eb480147-c587-4a33-85ed-eb0ab9942c43\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"85d264ad-e0a0-4565-b7ff-1a37edde51ac\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"cea9b045-a000-445b-95b8-0c171af70a3b\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"4cda81dc-9edd-4e03-9d7c-a71a14345d0b\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\n                    \"path\": \"*/{Submit}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad;Keyboard&Mouse;Touch\",\n                    \"action\": \"Submit\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\n                    \"path\": \"*/{Cancel}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Touch;Gamepad;Keyboard&Mouse\",\n                    \"action\": \"Cancel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\n                    \"path\": \"<Mouse>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e1394cbc-336e-44ce-9ea8-6007ed6193f7\",\n                    \"path\": \"<Pen>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5693e57a-238a-46ed-b5ae-e64e6e574302\",\n                    \"path\": \"<Touchscreen>/touch*/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Touch\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4faf7dc9-b979-4210-aa8c-e808e1ef89f5\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Click\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8d66d5ba-88d7-48e6-b1cd-198bbfef7ace\",\n                    \"path\": \"<Pen>/tip\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Click\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"47c2a644-3ebc-4dae-a106-589b7ca75b59\",\n                    \"path\": \"<Touchscreen>/touch*/press\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Touch\",\n                    \"action\": \"Click\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\n                    \"path\": \"<Mouse>/scroll\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"ScrollWheel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"MiddleClick\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"RightClick\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"156b494a-1b72-4637-abcc-673312f5b375\",\n                    \"path\": \"<Keyboard>/t\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ShowInfo\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"128b2fb1-af12-47cf-9064-0ae13fab7b34\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ShowInfo\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Generic\",\n            \"id\": \"c0dd4bb8-b434-48e5-bcd8-e3f6590ba5c9\",\n            \"actions\": [\n                {\n                    \"name\": \"Back\",\n                    \"type\": \"Button\",\n                    \"id\": \"3bbe5692-1874-443b-93f6-cb8a7eea718a\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Confirm\",\n                    \"type\": \"Button\",\n                    \"id\": \"dd40e659-2525-4a8c-a56d-9e5ce74051a1\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"AlternativeChoice\",\n                    \"type\": \"Button\",\n                    \"id\": \"c810e781-e8dd-4d49-b0de-60e948fbccca\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextSection\",\n                    \"type\": \"Button\",\n                    \"id\": \"3abb3d3f-b486-4f63-abe0-f795fb1342cd\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrevSection\",\n                    \"type\": \"Button\",\n                    \"id\": \"3ea85ef3-5f21-4ad3-a0c8-31b003bd8a53\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextSubsection\",\n                    \"type\": \"Button\",\n                    \"id\": \"ded2c18a-1908-41f7-8c29-5ecf39396abe\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrevSubsection\",\n                    \"type\": \"Button\",\n                    \"id\": \"14d44bbd-03d3-4e59-9a43-eff488ed26ac\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"NextListEntry\",\n                    \"type\": \"Button\",\n                    \"id\": \"febb67b0-2320-4791-ba0d-23e8be6a77f8\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrevListEntry\",\n                    \"type\": \"Button\",\n                    \"id\": \"f3135e94-409a-429d-982e-38916ffa8164\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HorizontalScroll\",\n                    \"type\": \"Value\",\n                    \"id\": \"5d4298a6-0ad3-4fb9-84b6-b5037a41c751\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"VerticalScroll\",\n                    \"type\": \"Value\",\n                    \"id\": \"b1b2f2f6-7286-43e1-93fa-236bf9bbb55b\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"11b0552b-44df-4cc7-acd5-bf6c669f5c22\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Back\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"46062f62-4509-4337-ab88-cafdae202cdf\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Back\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0e856c71-d343-443e-9761-dd29c3282071\",\n                    \"path\": \"*/{Back}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad;Keyboard&Mouse\",\n                    \"action\": \"Back\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c9264633-0d18-4c29-a1ad-34bda9ec34fb\",\n                    \"path\": \"<Keyboard>/enter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Confirm\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1744cc73-c376-483c-9a2d-832ae0cf6636\",\n                    \"path\": \"<Keyboard>/numpadEnter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Confirm\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"589d6359-788e-4ce2-bebd-99f58690a87b\",\n                    \"path\": \"*/{Submit}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad;Keyboard&Mouse\",\n                    \"action\": \"Confirm\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ba42f8fa-228c-48bc-acc5-94c9a59f7e5e\",\n                    \"path\": \"<Keyboard>/rightBracket\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NextSection\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"70a8a5fd-1eda-43af-a842-7bb55d332651\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextSection\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"84bc279e-720c-4858-acda-f6d9a1cc8b9d\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NextListEntry\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6ea3fef6-6e53-458d-8cda-f2233197531f\",\n                    \"path\": \"<Gamepad>/dpad/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextListEntry\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"62ae90ce-9021-4feb-b5d4-4c84912b8f00\",\n                    \"path\": \"<Keyboard>/leftBracket\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"PrevSection\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ef978be4-7a5e-4298-858a-d8131888dae3\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PrevSection\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6008d5f6-2339-4f5b-a698-efb05a3b0ad8\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"PrevListEntry\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e98eabfd-31e7-44d3-92c5-e4e0e5748b5a\",\n                    \"path\": \"<Gamepad>/dpad/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PrevListEntry\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"26786a29-3c92-4cc8-acff-83fb453d75b8\",\n                    \"path\": \"<Gamepad>/leftStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PrevListEntry\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"530388bf-ad14-4ce7-a71f-6981944c2f7c\",\n                    \"path\": \"<Gamepad>/leftStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextListEntry\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Right Stick\",\n                    \"id\": \"39c755b8-811d-4a6f-abac-6d575b0fe70b\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HorizontalScroll\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"fbfa6ec7-2ce6-4e53-a55b-d2b40d397f84\",\n                    \"path\": \"<Gamepad>/rightStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"HorizontalScroll\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"e9149de1-8776-41ea-b3db-d3c341186369\",\n                    \"path\": \"<Gamepad>/rightStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"HorizontalScroll\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Right Stick\",\n                    \"id\": \"74208213-82ac-485f-bc2b-9e59b93a4e99\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"VerticalScroll\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"73504116-21bd-432d-90ff-ae3d0edabe14\",\n                    \"path\": \"<Gamepad>/rightStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"VerticalScroll\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"7e6839cd-a7d1-43c0-b3a8-9d7375569a2b\",\n                    \"path\": \"<Gamepad>/rightStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"VerticalScroll\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5404fa45-4ea8-43ef-8cb0-a57b8be92e13\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"NextSubsection\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7e434203-095e-4129-ad58-d9969210bdaf\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"PrevSubsection\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e5b25330-9902-43c5-91ab-5956b14452f2\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"AlternativeChoice\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Shared\",\n            \"id\": \"a4a6ef25-dc1b-4dbb-89a2-d2b884ed64e1\",\n            \"actions\": [\n                {\n                    \"name\": \"ShowPauseMenu\",\n                    \"type\": \"Button\",\n                    \"id\": \"d33e8034-c74d-40c4-84fc-d0dae0e6d4f0\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HidePauseMenu\",\n                    \"type\": \"Button\",\n                    \"id\": \"80062e69-12bb-43cb-a93f-1e31a0775d3f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ShowSettings\",\n                    \"type\": \"Button\",\n                    \"id\": \"5c7498c8-c3d7-43f9-9425-9532656946cf\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HideSettings\",\n                    \"type\": \"Button\",\n                    \"id\": \"1406c655-e91e-430b-a35c-ba601df4ebd2\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Close\",\n                    \"type\": \"Button\",\n                    \"id\": \"f9780b55-e365-4d13-ae44-c61115519621\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"QuickSave\",\n                    \"type\": \"Button\",\n                    \"id\": \"9a775b68-e88d-47ee-ad84-c8085bd7fb6c\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"QuickLoad\",\n                    \"type\": \"Button\",\n                    \"id\": \"80c936d9-db22-42b9-a9e5-319b2423457a\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"40c5bbdc-c978-4ecc-96ae-8c3460f0a906\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"ShowPauseMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cac972b0-fffd-4c5e-8fb4-5fbb55283d94\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ShowPauseMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"eed39e94-57c0-4900-b603-e6f317641c80\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"HidePauseMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"3b1fe69e-695f-4ce4-abc3-52ad8f946c1f\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"HidePauseMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c8cfb2d6-e342-48c2-b565-c5b3bbdd7407\",\n                    \"path\": \"*/{Back}\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse;Gamepad\",\n                    \"action\": \"HidePauseMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ef8138d8-1ac4-49c7-9512-9d890ef13da6\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"ShowSettings\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8fd539da-6046-443e-a51c-27f52abaed1c\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"HideSettings\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ecba42c6-bd3d-42e6-a8b8-ee87d078c078\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Close\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d429e298-29dd-413b-b6f3-b017b68d23ac\",\n                    \"path\": \"<Keyboard>/f5\",\n                    \"interactions\": \"Press(behavior=1)\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"QuickSave\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"bd5bc4d6-102c-4f9a-9df0-c257070af2b1\",\n                    \"path\": \"<Keyboard>/f9\",\n                    \"interactions\": \"Press(behavior=1)\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"QuickLoad\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        }\n    ],\n    \"controlSchemes\": [\n        {\n            \"name\": \"Keyboard&Mouse\",\n            \"bindingGroup\": \"Keyboard&Mouse\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Keyboard>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                },\n                {\n                    \"devicePath\": \"<Mouse>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Gamepad\",\n            \"bindingGroup\": \"Gamepad\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Gamepad>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Touch\",\n            \"bindingGroup\": \"Touch\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Touchscreen>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        }\n    ]\n}");
		m_Scenario = asset.FindActionMap("Scenario", throwIfNotFound: true);
		m_Scenario_ClearTargetsOrUndo = m_Scenario.FindAction("ClearTargetsOrUndo", throwIfNotFound: true);
		m_Scenario_ConfirmAction = m_Scenario.FindAction("ConfirmAction", throwIfNotFound: true);
		m_Scenario_RetryScenario = m_Scenario.FindAction("RetryScenario", throwIfNotFound: true);
		m_Scenario_SkipAction = m_Scenario.FindAction("SkipAction", throwIfNotFound: true);
		m_Scenario_BurnOneCard = m_Scenario.FindAction("BurnOneCard", throwIfNotFound: true);
		m_Scenario_ReceiveDamage = m_Scenario.FindAction("ReceiveDamage", throwIfNotFound: true);
		m_Scenario_BurnTwoCards = m_Scenario.FindAction("BurnTwoCards", throwIfNotFound: true);
		m_Scenario_PrevActor = m_Scenario.FindAction("PrevActor", throwIfNotFound: true);
		m_Scenario_NextActor = m_Scenario.FindAction("NextActor", throwIfNotFound: true);
		m_Scenario_ClearActorHighlight = m_Scenario.FindAction("ClearActorHighlight", throwIfNotFound: true);
		m_Scenario_PrevUseItem = m_Scenario.FindAction("PrevUseItem", throwIfNotFound: true);
		m_Scenario_NextUseItem = m_Scenario.FindAction("NextUseItem", throwIfNotFound: true);
		m_Scenario_FocusUseItemBar = m_Scenario.FindAction("FocusUseItemBar", throwIfNotFound: true);
		m_Scenario_UnfocusUseItemBar = m_Scenario.FindAction("UnfocusUseItemBar", throwIfNotFound: true);
		m_Scenario_ToggleCharacterMode = m_Scenario.FindAction("ToggleCharacterMode", throwIfNotFound: true);
		m_ScenarioWorldspace = asset.FindActionMap("ScenarioWorldspace", throwIfNotFound: true);
		m_ScenarioWorldspace_PickTarget = m_ScenarioWorldspace.FindAction("PickTarget", throwIfNotFound: true);
		m_ScenarioWorldspace_SwapTarget = m_ScenarioWorldspace.FindAction("SwapTarget", throwIfNotFound: true);
		m_ScenarioWorldspace_RotateTargetRight = m_ScenarioWorldspace.FindAction("RotateTargetRight", throwIfNotFound: true);
		m_ScenarioWorldspace_RotateTargetLeft = m_ScenarioWorldspace.FindAction("RotateTargetLeft", throwIfNotFound: true);
		m_ScenarioWorldspace_HighlightInteractablesOverlay = m_ScenarioWorldspace.FindAction("HighlightInteractablesOverlay", throwIfNotFound: true);
		m_ScenarioWorldspace_LineOfSightOverlay = m_ScenarioWorldspace.FindAction("LineOfSightOverlay", throwIfNotFound: true);
		m_ScenarioWorldspace_ToggleSpeedUp = m_ScenarioWorldspace.FindAction("ToggleSpeedUp", throwIfNotFound: true);
		m_ScenarioWorldspace_Ping = m_ScenarioWorldspace.FindAction("Ping", throwIfNotFound: true);
		m_ScenarioDecks = asset.FindActionMap("ScenarioDecks", throwIfNotFound: true);
		m_ScenarioDecks_OpenViewAllCardsWindow = m_ScenarioDecks.FindAction("OpenViewAllCardsWindow", throwIfNotFound: true);
		m_ScenarioDecks_ViewHeroDeck1 = m_ScenarioDecks.FindAction("ViewHeroDeck1", throwIfNotFound: true);
		m_ScenarioDecks_ViewHeroDeck2 = m_ScenarioDecks.FindAction("ViewHeroDeck2", throwIfNotFound: true);
		m_ScenarioDecks_ViewHeroDeck3 = m_ScenarioDecks.FindAction("ViewHeroDeck3", throwIfNotFound: true);
		m_ScenarioDecks_ViewHeroDeck4 = m_ScenarioDecks.FindAction("ViewHeroDeck4", throwIfNotFound: true);
		m_Camera = asset.FindActionMap("Camera", throwIfNotFound: true);
		m_Camera_Movement = m_Camera.FindAction("Movement", throwIfNotFound: true);
		m_Camera_Zoom = m_Camera.FindAction("Zoom", throwIfNotFound: true);
		m_Camera_Rotate = m_Camera.FindAction("Rotate", throwIfNotFound: true);
		m_Camera_DragRotate = m_Camera.FindAction("DragRotate", throwIfNotFound: true);
		m_Camera_DragMove = m_Camera.FindAction("DragMove", throwIfNotFound: true);
		m_CameraFree = asset.FindActionMap("CameraFree", throwIfNotFound: true);
		m_CameraFree_FreeCamMovement = m_CameraFree.FindAction("FreeCamMovement", throwIfNotFound: true);
		m_CameraFree_FreeCamRotate = m_CameraFree.FindAction("FreeCamRotate", throwIfNotFound: true);
		m_CameraFree_FreeCamMoveVertical = m_CameraFree.FindAction("FreeCamMoveVertical", throwIfNotFound: true);
		m_Outpost = asset.FindActionMap("Outpost", throwIfNotFound: true);
		m_Outpost_Save = m_Outpost.FindAction("Save", throwIfNotFound: true);
		m_Outpost_NextPhase = m_Outpost.FindAction("NextPhase", throwIfNotFound: true);
		m_Outpost_NextNarration = m_Outpost.FindAction("NextNarration", throwIfNotFound: true);
		m_Outpost_ReplayNarration = m_Outpost.FindAction("ReplayNarration", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
		m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
		m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
		m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", throwIfNotFound: true);
		m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
		m_UI_ShowInfo = m_UI.FindAction("ShowInfo", throwIfNotFound: true);
		m_Generic = asset.FindActionMap("Generic", throwIfNotFound: true);
		m_Generic_Back = m_Generic.FindAction("Back", throwIfNotFound: true);
		m_Generic_Confirm = m_Generic.FindAction("Confirm", throwIfNotFound: true);
		m_Generic_AlternativeChoice = m_Generic.FindAction("AlternativeChoice", throwIfNotFound: true);
		m_Generic_NextSection = m_Generic.FindAction("NextSection", throwIfNotFound: true);
		m_Generic_PrevSection = m_Generic.FindAction("PrevSection", throwIfNotFound: true);
		m_Generic_NextSubsection = m_Generic.FindAction("NextSubsection", throwIfNotFound: true);
		m_Generic_PrevSubsection = m_Generic.FindAction("PrevSubsection", throwIfNotFound: true);
		m_Generic_NextListEntry = m_Generic.FindAction("NextListEntry", throwIfNotFound: true);
		m_Generic_PrevListEntry = m_Generic.FindAction("PrevListEntry", throwIfNotFound: true);
		m_Generic_HorizontalScroll = m_Generic.FindAction("HorizontalScroll", throwIfNotFound: true);
		m_Generic_VerticalScroll = m_Generic.FindAction("VerticalScroll", throwIfNotFound: true);
		m_Shared = asset.FindActionMap("Shared", throwIfNotFound: true);
		m_Shared_ShowPauseMenu = m_Shared.FindAction("ShowPauseMenu", throwIfNotFound: true);
		m_Shared_HidePauseMenu = m_Shared.FindAction("HidePauseMenu", throwIfNotFound: true);
		m_Shared_ShowSettings = m_Shared.FindAction("ShowSettings", throwIfNotFound: true);
		m_Shared_HideSettings = m_Shared.FindAction("HideSettings", throwIfNotFound: true);
		m_Shared_Close = m_Shared.FindAction("Close", throwIfNotFound: true);
		m_Shared_QuickSave = m_Shared.FindAction("QuickSave", throwIfNotFound: true);
		m_Shared_QuickLoad = m_Shared.FindAction("QuickLoad", throwIfNotFound: true);
	}

	~FHControls()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}

	public void SetInputContext(IInputContext context)
	{
		InputContext = context;
	}

	public void PerformPairingWithDevice(InputDevice device, InputUserPairingOptions options = InputUserPairingOptions.None)
	{
		InputContext.PerformPairingWithDevice(device, options);
	}

	public void PerformPairingWithEmptyDevice()
	{
		InputContext.PerformPairingWithEmptyDevice();
	}

	public void UnpairDevice(InputDevice device)
	{
		InputContext.UnpairDevice(device);
	}

	public void UnpairDevices()
	{
		InputContext.UnpairDevices();
	}

	public InputAction FindActionFor(string actionNameOrId, bool throwIfNotFound = false)
	{
		return InputContext.FindActionFor(actionNameOrId, throwIfNotFound);
	}

	public InputAction FindActionFor(Guid id, bool throwIfNotFound = false)
	{
		return InputContext.FindActionFor(id, throwIfNotFound);
	}

	public void EnableAction(object source, InputAction action)
	{
		InputContext.EnableAction(source, action);
	}

	public void DisableAction(object source, InputAction action)
	{
		InputContext.DisableAction(source, action);
	}

	public void DisableAll(object source)
	{
		InputContext.DisableAll(source);
	}

	public IEnumerable<InputAction> GetInputActionsEnabledBy(object source)
	{
		return InputContext.GetInputActionsEnabledBy(source);
	}

	public IEnumerable<object> GetEnablingSourcesFor(InputAction action)
	{
		return InputContext.GetEnablingSourcesFor(action);
	}

	public bool IsEnabledBy(object source, InputAction action)
	{
		return InputContext.IsEnabledBy(source, action);
	}

	public void PushOrSetActionsMask(object source, IEnumerable<InputAction> actionsMask, bool setBackToTop = false)
	{
		InputContext.PushOrSetActionsMask(source, actionsMask, setBackToTop);
	}

	public void PopActionsMask(object source)
	{
		InputContext.PopActionsMask(source);
	}

	public IEnumerable<InputAction> GetUIActions()
	{
		return InputContext.GetUIActions();
	}

	public IEnumerable<InputAction> GetAllActions()
	{
		return InputContext.GetAllActions();
	}

	public InputDevice GetLastUsedInputDevice()
	{
		return InputContext.GetLastUsedInputDevice();
	}

	public InputControlScheme GetLastUsedInputControlScheme()
	{
		return InputContext.GetLastUsedInputControlScheme();
	}

	public void TriggerLastUsedDeviceChanged()
	{
		InputContext.TriggerLastUsedDeviceChanged();
	}

	public void TriggerLastUsedInputControlSchemeChanged()
	{
		InputContext.TriggerLastUsedInputControlSchemeChanged();
	}

	public IEnumerable<InputControlScheme> GetAllInputControlSchemes()
	{
		return InputContext.GetAllInputControlSchemes();
	}

	public IReadOnlyList<IInputBindingDisplayDataProvider> GetAllDisplayDataProviders()
	{
		return InputContext.GetAllDisplayDataProviders();
	}

	public IInputBindingDisplayDataProvider GetCurrentDisplayDataProvider()
	{
		return InputContext.GetCurrentDisplayDataProvider();
	}
}

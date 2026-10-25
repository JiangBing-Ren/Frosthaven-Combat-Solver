using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.UIScope;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldspacePicker : Singleton<WorldspacePicker>, IScopeElement
{
	public LayerMask m_HexSelectionRaycastLayer;

	public bool ShouldSuppressSelectionThisFrame;

	private void OnEnable()
	{
		InputFHManager.Controls.Enable(this, InputFHManager.Controls.ScenarioWorldspace.PickTarget);
		InputFHManager.Controls.Enable(this, InputFHManager.Controls.ScenarioWorldspace.SwapTarget);
	}

	private void OnDisable()
	{
		InputFHManager.Controls?.DisableAll(this);
	}

	private void LateUpdate()
	{
		if (TimeManager.IsPaused || CameraController.s_Translated || CameraController.s_Rotated)
		{
			return;
		}
		if (ShouldSuppressSelectionThisFrame)
		{
			ShouldSuppressSelectionThisFrame = false;
		}
		else
		{
			if (EventSystem.current.IsPointerOverGameObject() || ((bool)DebugMenu.Instance && DebugMenu.Instance.IsMenuOpen && DebugMenu.Instance.CaptureTileFunction == DebugMenu.CaptureTileFunctions.None))
			{
				return;
			}
			CInteractable.EInputType eInputType = CInteractable.EInputType.None;
			if (InputFHManager.Controls.ScenarioWorldspace.PickTarget.WasPerformedThisFrame())
			{
				eInputType = CInteractable.EInputType.Primary;
			}
			else if (InputFHManager.Controls.ScenarioWorldspace.SwapTarget.WasPerformedThisFrame())
			{
				eInputType = CInteractable.EInputType.Alternative;
			}
			if (eInputType == CInteractable.EInputType.None)
			{
				return;
			}
			bool flag = false;
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, m_HexSelectionRaycastLayer);
			if ((bool)cInteractable)
			{
				TileBehaviour component = cInteractable.GetComponent<TileBehaviour>();
				ControllerInputPointer.CursorType = ((!(component == null) && component.m_ClientTile != null) ? ECursorType.Default : ECursorType.Invalid);
				if ((component == null || component.m_ClientTile == null || InteractabilityManager.ShouldAllowSelectionForTileIndex(component.m_ClientTile.m_Tile.m_ArrayIndex)) && (Choreographer.s_Choreographer.ThisPlayerHasTurnControl || DebugMenu.Instance.CaptureTile))
				{
					flag = true;
					cInteractable.ShowNormalInterface(disabled: false, eInputType);
					if (component != null && component.m_ClientTile != null)
					{
						UIEventManager.AddTileSelectEvent(component.m_ClientTile.m_Tile.m_ArrayIndex, eInputType);
					}
				}
			}
			else if (TileBehaviour.s_Callback != null && PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityAttack { State: CAbilityAttack.EAttackState.SelectAttackFocus, AreaEffect: not null, IsMeleeAttack: not false })
			{
				TileBehaviour.s_Callback(null, null, networkActionIfOnline: true, isUserClick: true, SaveData.Instance.Global.EnableSecondClickHexToConfirm, eInputType);
			}
			else
			{
				ControllerInputPointer.CursorType = ECursorType.Invalid;
			}
			if (!flag)
			{
				CameraController.s_CameraController.SetFocalPointGameObject(null);
			}
		}
	}
}

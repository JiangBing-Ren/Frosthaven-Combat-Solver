using AStar;
using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WorldspaceOverlayToolsScopeElement : MonoBehaviour, IScopeElement
{
	public InputActionReference HighlightAction;

	public InputActionReference LineOfSightAction;

	public InputActionReference SpeedUpAction;

	public InputActionReference PingAction;

	private InputAction _highlightAction;

	private InputAction _LOSAction;

	private InputAction _speedUpAction;

	private InputAction _pingAction;

	private void OnEnable()
	{
		_highlightAction = InputFHManager.Controls.Enable(this, HighlightAction);
		_highlightAction.canceled += HanldeOnCancelInputAction;
		_LOSAction = InputFHManager.Controls.Enable(this, LineOfSightAction);
		_LOSAction.canceled += HanldeOnCancelInputAction;
		_speedUpAction = InputFHManager.Controls.Enable(this, SpeedUpAction);
		_speedUpAction.canceled += HanldeOnCancelInputAction;
		_pingAction = InputFHManager.Controls.Enable(this, PingAction);
		_pingAction.canceled += HanldeOnCancelInputAction;
	}

	private void HanldeOnCancelInputAction(InputAction.CallbackContext context)
	{
		if (context.action == _highlightAction)
		{
			ShowHighlightForInteractables(show: false);
		}
	}

	private void OnDisable()
	{
		InputFHManager.Controls?.DisableAll(this);
		_highlightAction.canceled -= HanldeOnCancelInputAction;
	}

	private void Update()
	{
		if (InputFHManager.IsTextFieldFocused)
		{
			return;
		}
		if (_highlightAction.WasPressedThisFrame())
		{
			ShowHighlightForInteractables(show: true);
		}
		else if (_highlightAction.WasReleasedThisFrame())
		{
			ShowHighlightForInteractables(show: false);
		}
		if (_LOSAction.WasPressedThisFrame())
		{
			if (ClientScenarioManager.s_ClientScenarioManager != null)
			{
				ClientScenarioManager.s_ClientScenarioManager.EnableUserLOSDisplay(isOn: true);
			}
		}
		else if (_LOSAction.WasReleasedThisFrame() && ClientScenarioManager.s_ClientScenarioManager != null)
		{
			ClientScenarioManager.s_ClientScenarioManager.EnableUserLOSDisplay(isOn: false);
		}
		if (_speedUpAction.WasPressedThisFrame())
		{
			if (CanHoldRequestSpeedUp())
			{
				SaveData.Instance.Global.SetSpeedUpHoldRequest(active: true);
			}
		}
		else if (_speedUpAction.WasReleasedThisFrame() && CanHoldRequestSpeedUp())
		{
			SaveData.Instance.Global.SetSpeedUpHoldRequest(active: false);
		}
		if (_pingAction.WasPerformedThisFrame() && Singleton<WorldspacePicker>.InstanceFast != null)
		{
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Singleton<WorldspacePicker>.InstanceFast.m_HexSelectionRaycastLayer);
			bool flag = !EventSystem.current.IsPointerOverGameObject() && (DebugMenu.Instance == null || !DebugMenu.Instance.IsMenuOpen);
			if ((bool)cInteractable & flag)
			{
				Point arrayIndex = cInteractable.GetComponent<TileBehaviour>().m_ClientTile.m_Tile.m_ArrayIndex;
				CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[arrayIndex.X, arrayIndex.Y];
				Singleton<UIScenarioMultiplayerController>.Instance.PingTile(tile);
			}
		}
	}

	private void ShowHighlightForInteractables(bool show)
	{
		WorldspaceStarHexDisplay.Instance.HighlightAllHexes(show);
		InitiativeTrack.Instance.ShowActorDetails(show);
		WorldspaceUITools.Instance.ActivateAllOutlines(show);
		WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
		if (array != null)
		{
			WorldspacePanelUIController[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].InfoUI.ShowCharacterName(show);
			}
		}
	}

	private bool CanHoldRequestSpeedUp()
	{
		return !FFSNetwork.IsOnline;
	}
}

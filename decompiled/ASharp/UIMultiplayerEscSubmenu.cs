using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHRL;
using GLOOM;
using MapRuleLibrary.Adventure;
using SETUtil.Extend;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIMultiplayerEscSubmenu : Singleton<UIMultiplayerEscSubmenu>
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private ScrollRect contentScroll;

	[SerializeField]
	private List<Graphic> focusMasks;

	[SerializeField]
	private UIMultiplayerConfirmationBox confirmationBox;

	[Header("Session")]
	[SerializeField]
	private Button startSessionButton;

	[SerializeField]
	private Button endSessionButton;

	[SerializeField]
	private CanvasGroup onlineOptionsGroup;

	[Header("Invite")]
	[SerializeField]
	private Button copyCodeButton;

	[SerializeField]
	private Button generateCodeButton;

	[SerializeField]
	private TextMeshProUGUI inviteCodeText;

	[SerializeField]
	private UIMultiplayerInvitePlayersWindow invitePlayersWindow;

	[SerializeField]
	private UIMultiplayerSignInConfirmationBox signInConfirmationBox;

	[Header("Steam")]
	[SerializeField]
	private GameObject steamPanel;

	[SerializeField]
	private Button openSTEAMButton;

	[Header("Epic")]
	[SerializeField]
	private GameObject epicPanel;

	[SerializeField]
	private Button openEpicButton;

	[SerializeField]
	private Graphic epicMask;

	[SerializeField]
	private List<Graphic> focusEpicMasks;

	[Header("Assign characters")]
	[SerializeField]
	private UIMultiplayerMerchantRoster heroSlotsController;

	[SerializeField]
	private UIMultiplayerSelectPlayerScreen selectPlayerPopup;

	[SerializeField]
	private UIMultiplayerPartyAssignments partySlots;

	[SerializeField]
	private UIMultiplayerPlayerRoster playerSlots;

	[Header("Info")]
	[SerializeField]
	private TMP_Text warnings;

	private INetworkSessionService service;

	private INetworkHeroAssignService heroAssignService;

	private INetworkLoginService epicLoginService = new DummyNetworkLoginService(signedIn: false, 5f);

	private bool invalidate;

	public bool IsHosting
	{
		get
		{
			if (FFSNetwork.IsOnline)
			{
				return !FFSNetwork.IsStartingUp;
			}
			return false;
		}
	}

	public UnityEvent OnHide => window.onHidden;

	protected override void AwakeInit()
	{
		if ((bool)heroSlotsController)
		{
			heroSlotsController.OnSelectedSlot.AddListener(OnSelectedHeroToAssign);
			heroSlotsController.OnDeselectedSlot.AddListener(OnDeselectedHeroToAssign);
		}
		window.onShown.AddListener(OnShown);
		window.onHidden.AddListener(OnHidden);
		endSessionButton.onClick.AddListener(EndSession);
		if ((bool)startSessionButton)
		{
			startSessionButton.onClick.AddListener(StartSession);
		}
		copyCodeButton.onClick.AddListener(CopyCode);
		generateCodeButton.onClick.AddListener(GenerateCode);
		service = GetComponent<INetworkSessionService>();
		heroAssignService = GetComponent<INetworkHeroAssignService>();
		PlatformLayer.Instance.NewPlatformLayer.GetPlatformInvitesSupported((bool result) =>
		{
			steamPanel.SetActive(result);
		});
		openEpicButton.onClick.AddListener(OpenEpicInvite);
		partySlots.OnSelectedSlot.AddListener(OnSelectedPartSlotToAssign);
	}

	private void SelectFirstElement()
	{
		if (startSessionButton != null && startSessionButton.gameObject.activeInHierarchy)
		{
			startSessionButton.Select();
		}
		else if (heroSlotsController != null && heroSlotsController.CurrentSlot == null)
		{
			endSessionButton.Select();
		}
	}

	public void Show()
	{
		epicPanel.SetActive(SaveData.Instance.Global.EpicLogin);
		window.Show();
	}

	public void Hide()
	{
		window.Hide();
	}

	private void Focus(bool focus)
	{
		for (int i = 0; i < focusMasks.Count; i++)
		{
			focusMasks[i].enabled = !focus;
		}
		for (int j = 0; j < focusEpicMasks.Count; j++)
		{
			focusEpicMasks[j].enabled = !focus;
		}
		epicMask.SetAlpha(1f);
		if ((bool)heroSlotsController)
		{
			heroSlotsController.Focus(focus);
		}
	}

	private void OnClosedSelectPlayer()
	{
		if ((bool)heroSlotsController)
		{
			heroSlotsController.ResetSelection();
		}
		Focus(focus: true);
	}

	private void AssignHeroToPlayer(NetworkPlayer player)
	{
		if ((bool)heroSlotsController)
		{
			heroSlotsController.AssignPlayerToSelectedSlot(player, service.IsOnline(player));
		}
		Invalidate();
	}

	private void OnDeselectedHeroToAssign()
	{
		selectPlayerPopup.Hide();
	}

	private void OnSelectedHeroToAssign(UIMultiplayerMerchantSlot slot)
	{
		if (FFSNetwork.IsHost)
		{
			Focus(focus: false);
			selectPlayerPopup.Show(name, heroSlotsController.CurrentSlot.CharacterID, heroSlotsController.CurrentSlot.CharacterName, slot.SlotIndex, slot.Title, slot.AssignedPlayer, slot.transform as RectTransform, OnClosedSelectPlayer, AssignHeroToPlayer);
		}
	}

	private void OnSelectedPartSlotToAssign(UIPartySlot partySlot)
	{
		if (FFSNetwork.IsHost && !(partySlot == null))
		{
			Focus(focus: false);
			selectPlayerPopup.Show(name, partySlot.CurrentData.ClassID, partySlot.CurrentData.Name, partySlot.SlotId, LocalizationManager.GetTranslation("GUI_MULTIPLAYER_ASSIGN_SLOT"), null, partySlot.transform as RectTransform, OnClosedSelectPlayer, (NetworkPlayer player) =>
			{
				Invalidate();
			});
			PositionConstraint orAdd = selectPlayerPopup.gameObject.GetOrAdd<PositionConstraint>();
			if ((object)orAdd != null)
			{
				orAdd.SetSource(0, new ConstraintSource
				{
					sourceTransform = partySlot.transform,
					weight = 1f
				});
				orAdd.weight = 1f;
				orAdd.constraintActive = true;
			}
		}
	}

	private void OnShown()
	{
		INetworkSessionService networkSessionService = service;
		networkSessionService.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(networkSessionService.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
		if ((bool)heroSlotsController)
		{
			heroSlotsController.Init(AdventureState.MapState.MapParty.SelectedCharactersArray);
		}
		selectPlayerPopup.Hide();
		Focus(focus: true);
		ShowOnlineOptions(FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp);
		Invalidate();
	}

	private void OnHidden()
	{
		INetworkSessionService networkSessionService = service;
		networkSessionService.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(networkSessionService.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
		if ((bool)heroSlotsController)
		{
			heroSlotsController.ResetSelection();
		}
		selectPlayerPopup.Hide();
		invitePlayersWindow.Hide();
		signInConfirmationBox.Hide();
	}

	private void OnPlayerJoined(NetworkPlayer player)
	{
		if ((bool)heroSlotsController)
		{
			heroSlotsController.UpdateConnectionStateSlot(player, isOnline: true);
		}
		Invalidate();
	}

	private void OnControllableOwnersChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		if ((bool)heroSlotsController)
		{
			heroSlotsController.AssignPlayerTo(controllable.Slot, newController, service.IsOnline(newController));
		}
		Invalidate();
	}

	public void Refresh()
	{
		if (window.IsOpen)
		{
			Invalidate();
		}
	}

	public void RefreshAssignInteraction()
	{
		if ((bool)heroSlotsController && window.IsOpen)
		{
			heroSlotsController.RefreshInteractable();
		}
		Invalidate();
	}

	private void RefreshAssignedHeroes()
	{
		if ((bool)heroSlotsController)
		{
			for (int i = 0; i < 4; i++)
			{
				PartySlotID slot = PartySlotID.FromIndex(i);
				NetworkPlayer playerAssignedToSlot = PlayerRegistry.GetPlayerAssignedToSlot(slot);
				heroSlotsController.AssignPlayerTo(slot, playerAssignedToSlot, service.IsOnline(playerAssignedToSlot));
			}
		}
	}

	private void RefreshSlots()
	{
		if ((bool)partySlots)
		{
			partySlots.Init(AdventureState.MapState.MapParty.SelectedCharactersArray);
		}
		if (!playerSlots)
		{
			return;
		}
		playerSlots.Init(new UIMultiplayerPlayerRoster.Params
		{
			Players = PlayerRegistry.AllPlayers,
			OnInvite = OpenInviteOverlay,
			OnKick = (NetworkPlayer player) =>
			{
				if (player != null && player.IsClient)
				{
					confirmationBox.ShowConfirmation(player, () =>
					{
						heroAssignService.RemoveClient(player);
					});
				}
			}
		});
	}

	private void UpdateWarning()
	{
		if (!(warnings == null))
		{
			string text = string.Empty;
			bool flag = PlayerRegistry.AllPlayers.Any((NetworkPlayer player) => !player.IsSlotAssigned);
			bool flag2 = PlayerRegistry.GetSlots().Any((PartySlotID id) => !PlayerRegistry.SlotHasControllable(id) && PlayerRegistry.GetPlayerAssignedToSlot(id).IsClient);
			Log.GUIInfo($"Warring hasPlayerWithoutSlot:{flag}, hasPlayerWithEmptySlot{flag2}");
			if (flag)
			{
				text = text + LocalizationManager.GetTranslation("GUI_MULTIPLAYER_SESSION_WARNING_PLAYER_NOT_ASSIGNED_TO_SLOT") + "\n";
			}
			if (flag2)
			{
				text = text + LocalizationManager.GetTranslation("GUI_MULTIPLAYER_SESSION_WARNING_PLAYER_ASSIGNED_TO_EMPTY_SLOT") + "\n";
			}
			warnings.text = text;
		}
	}

	public void Invalidate()
	{
		invalidate = true;
	}

	private void LateUpdate()
	{
		if (invalidate)
		{
			invalidate = false;
			RefreshAssignedHeroes();
			RefreshSlots();
			selectPlayerPopup.RefreshPlayers();
			UpdateWarning();
		}
	}

	private void LoadCurrentCode()
	{
		if (service.GetInviteCode().IsNullOrEmpty())
		{
			copyCodeButton.gameObject.SetActive(value: false);
			generateCodeButton.gameObject.SetActive(value: true);
		}
		else
		{
			generateCodeButton.gameObject.SetActive(value: false);
			inviteCodeText.text = service.GetInviteCode();
			copyCodeButton.gameObject.SetActive(value: true);
		}
	}

	private void GenerateCode()
	{
		service.GenerateInviteCode();
		LoadCurrentCode();
		UIMultiplayerNotifications.ShowGenerateInviteCode();
	}

	private void CopyCode()
	{
		GUIUtility.systemCopyBuffer = service.GetInviteCode();
		UIMultiplayerNotifications.ShowShareInviteCode();
	}

	public void OpenInviteOverlay()
	{
		Debug.Log("OPEN OVERLAY");
		PlatformLayer.Instance.NewPlatformLayer.OpenInviteOverlay();
	}

	private void OpenEpicInvite()
	{
		Focus(focus: false);
		epicMask.SetAlpha(0f);
		if ((bool)heroSlotsController)
		{
			heroSlotsController.Focus(focus: false, interactable: false);
		}
		invitePlayersWindow.Show(() =>
		{
			Focus(focus: true);
		}, openEpicButton.transform as RectTransform);
	}

	private void EndSession()
	{
		if (FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_END_SESSION"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_END_SESSION"), service.EndSession, UIInfoTools.DefInstance.neutralActionTextColor, enableSoftlockReport: true);
		}
	}

	private void OnHostingEndedCallback()
	{
		selectPlayerPopup.Hide();
		ShowOnlineOptions(show: false);
		Hide();
	}

	public void StartSession()
	{
		service.StartSession(OnHostingStartedCallback, OnHostingEndedCallback);
	}

	private void OnHostingStartedCallback()
	{
		ShowOnlineOptions(show: true);
		UIMultiplayerNotifications.ShowStartedSession();
	}

	public void ShowOnlineOptions(bool show)
	{
		if (show)
		{
			LoadCurrentCode();
			RefreshAssignedHeroes();
			RefreshSlots();
		}
		else
		{
			invitePlayersWindow.Hide();
			signInConfirmationBox.Hide();
		}
		if ((bool)contentScroll)
		{
			contentScroll.verticalScrollbar.gameObject.SetActive(show);
			contentScroll.enabled = show;
		}
		if ((bool)startSessionButton)
		{
			startSessionButton.gameObject.SetActive(!show);
		}
		onlineOptionsGroup.alpha = (show ? 1 : 0);
		onlineOptionsGroup.interactable = show;
		onlineOptionsGroup.blocksRaycasts = show;
		SelectFirstElement();
	}
}

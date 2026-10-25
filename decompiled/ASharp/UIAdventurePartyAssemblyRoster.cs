using System;
using System.Collections.Generic;
using FFSNet;
using FFSNet.Backend;
using GLOOM;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAdventurePartyAssemblyRoster : MonoBehaviour
{
	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private Transform rosterHolder;

	[SerializeField]
	private UIAdventurePartyAssemblyRosterSlot slotPrefab;

	[SerializeField]
	private List<UIAdventurePartyAssemblyRosterSlot> rosterSlots;

	private Dictionary<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlots = new Dictionary<CMapCharacter, UIAdventurePartyAssemblyRosterSlot>();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterHover = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterUnhover = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterSelected = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterDeselected = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	private PlayersChangedEvent OnPlayerJoinedHandler;

	private CMapCharacter selectedCharacter;

	private Coroutine autoselectElementCoroutine;

	public void Show(List<CMapCharacter> characters, CMapCharacter selectedCharacter, RectTransform sourceUI, bool instant = false)
	{
		if (sourceUI != null)
		{
			verticalPointer.PointAt(sourceUI);
		}
		Refresh(characters, selectedCharacter);
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
		OnPlayerJoinedHandler = (NetworkPlayer player) =>
		{
			OnPlayerConnected();
		};
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, OnPlayerJoinedHandler);
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserDisconnected));
	}

	public void Refresh(List<CMapCharacter> characters, CMapCharacter selectedCharacter)
	{
		this.selectedCharacter = selectedCharacter;
		RefreshSlots(characters);
	}

	public void RefreshSlots(List<CMapCharacter> characters)
	{
		assignedSlots.Clear();
		HelperTools.NormalizePool(ref rosterSlots, slotPrefab.gameObject, rosterHolder, characters.Count);
		for (int i = 0; i < characters.Count; i++)
		{
			CMapCharacter cMapCharacter = characters[i];
			rosterSlots[i].Init(cMapCharacter, cMapCharacter == selectedCharacter);
			rosterSlots[i].OnCharacterHover.AddListener(OnHoveredCharacter);
			rosterSlots[i].OnCharacterUnhover.AddListener(OnUnhoveredCharacter);
			rosterSlots[i].OnCharacterSelected.AddListener(OnSelectedCharacter);
			rosterSlots[i].OnCharacterDeselected.AddListener(OnDeselectedCharacter);
			rosterSlots[i].DetermineInteractability(selectedCharacter);
			assignedSlots[cMapCharacter] = rosterSlots[i];
		}
	}

	public void Hide()
	{
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, OnPlayerJoinedHandler);
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserDisconnected));
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
			PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, OnPlayerJoinedHandler);
			PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserDisconnected));
		}
	}

	public void ShowCharacterSelected(CMapCharacter character)
	{
		ClearCharacterSelected();
		selectedCharacter = character;
		assignedSlots[character].SetSelected(isSelected: true);
		if (FFSNetwork.IsOnline)
		{
			DetermineSlotInteractability();
		}
	}

	public void ClearCharacterSelected()
	{
		if (selectedCharacter != null)
		{
			assignedSlots[selectedCharacter].SetSelected(isSelected: false);
			selectedCharacter = null;
			if (FFSNetwork.IsOnline)
			{
				DetermineSlotInteractability();
			}
		}
	}

	public void ClearIfCharacterSelected(CMapCharacter character)
	{
		if (selectedCharacter == character)
		{
			ClearCharacterSelected();
		}
	}

	public void RemoveCharacterOption(CMapCharacter character)
	{
		ClearIfCharacterSelected(character);
		assignedSlots[character].gameObject.SetActive(value: false);
		assignedSlots.Remove(character);
	}

	public void DeselectAllCharacters()
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.SetSelected(isSelected: false);
		}
	}

	private void OnSelectedCharacter(CMapCharacter character)
	{
		if (assignedSlots[character].IsLocked)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_PARTY_LOCKED_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_PARTY_LOCKED"), null, "GUI_CONTINUE");
		}
		else
		{
			OnCharacterSelected?.Invoke(character);
		}
	}

	private void OnDeselectedCharacter(CMapCharacter character)
	{
		OnCharacterDeselected?.Invoke(character);
	}

	private void OnHoveredCharacter(CMapCharacter character)
	{
		OnCharacterHover?.Invoke(character);
	}

	private void OnUnhoveredCharacter(CMapCharacter character)
	{
		OnCharacterUnhover?.Invoke(character);
	}

	public void Lock(CMapCharacter[] showLocks)
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> slot in assignedSlots)
		{
			slot.Value.SetLocked(isLocked: true, Array.Exists(showLocks, (CMapCharacter e) => e == slot.Key));
		}
	}

	public void DisableOptions(List<CMapCharacter> options)
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlot in assignedSlots)
		{
			if (options.Contains(assignedSlot.Key))
			{
				if (EventSystem.current.currentSelectedGameObject == assignedSlot.Value.gameObject)
				{
					_ = assignedSlot.Key;
				}
				assignedSlot.Value.DisableOption(disable: true, this);
			}
			else
			{
				assignedSlot.Value.DisableOption(disable: false, this);
			}
		}
	}

	public void DetermineSlotInteractability()
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.DetermineInteractability(selectedCharacter);
		}
	}

	public void OnPlayerConnected(NetworkBackendPlayer connection = null)
	{
		DetermineSlotInteractability();
	}

	private void OnJoiningUserDisconnected(NetworkBackendPlayer connection = null)
	{
		DetermineSlotInteractability();
	}
}

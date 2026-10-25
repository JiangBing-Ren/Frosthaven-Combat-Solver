using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FFSNet;
using FHRL;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewPartyDisplayUI : APartyDisplayUI
{
	public enum DisplayType
	{
		NONE,
		INVENTORY,
		CARDS,
		PERKS,
		CHARACTER_SELECTOR,
		LEVELUP,
		BATTLE_GOALS
	}

	[Flags]
	private enum FlagsInteraction
	{
		None = 0,
		DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT = 1,
		DISABLE_BUTTONS = 2,
		DISABLE_TOGGLE_OFF_CHARACTER = 4,
		DISABLE_EDIT_PARTY = 8,
		DISABLE_RESET_LEVELUP = 0x10
	}

	public delegate void SelectionEvent(bool selected, DisplayType display);

	[SerializeField]
	private UIWindow window;

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI partyNameText;

	[SerializeField]
	private GoldCounter partyGoldCounter;

	[SerializeField]
	private GUIAnimator warningPartyGoldAnimator;

	[SerializeField]
	private GameObject partyGoldContainer;

	[SerializeField]
	private List<NewPartyCharacterUI> charactersList;

	[SerializeField]
	private UIPartyCharacterAbilityCardsDisplay abilityCardsDisplay;

	[SerializeField]
	private ToggleGroup slotsToggleGroup;

	[SerializeField]
	private UIPerksWindow perkManager;

	[SerializeField]
	private UIAdventurePartyAssemblyWindow characterSelector;

	[SerializeField]
	private UIPartyCharacterEquipmentDisplay itemInventoryDisplay;

	[SerializeField]
	private ClickTrackerScopeElement clickTracker;

	[SerializeField]
	private UIPartyStats campaignPartyStats;

	[SerializeField]
	private UIPartyStats guildmasterPartyStats;

	[Header("Open/Close animation")]
	[SerializeField]
	private GUIAnimator showPanelAnimator;

	[SerializeField]
	private GUIAnimator hidePanelAnimator;

	[SerializeField]
	private CanvasGroup animLockInteraction;

	[SerializeField]
	private float openPanelDelay;

	private Coroutine delayedShowCoroutine;

	private NewPartyCharacterUI selectedCharacter;

	private DisplayType displayActive;

	private FlagsInteraction flagsInteraction;

	private Action<CMapCharacter> onCharacterSelectedCallback;

	private IPartyItemEquipmentService itemService;

	private CMapParty mapParty;

	private Action hideCallback;

	private bool initialised;

	private UIPartyStats partyStats;

	private HashSet<Component> disableClickTrackerRequests = new HashSet<Component>();

	private HashSet<object> hideRequests = new HashSet<object>();

	private HashSet<Component> disableAssignPlayerRequests = new HashSet<Component>();

	private HashSet<Component> disableCharacterSelectionRequests = new HashSet<Component>();

	private HashSet<Component> disableStatsRequests = new HashSet<Component>();

	public UICampaignAdventurePartyAssemblyWindow CampaignCharacterSelector
	{
		get
		{
			if (characterSelector != null && characterSelector is UICampaignAdventurePartyAssemblyWindow result)
			{
				return result;
			}
			return null;
		}
	}

	public int SelectedSlotIndex => charactersList.IndexOf(SelectedUISlot);

	public bool Initialised => initialised;

	public NewPartyCharacterUI SelectedUISlot => selectedCharacter;

	public int SelectedUISlotIndex => charactersList.IndexOf(selectedCharacter);

	public List<NewPartyCharacterUI> CharacterSlots => charactersList;

	public UnityEvent OnShown => window.onShown;

	public UnityEvent OnHidden => window.onHidden;

	public override APartyCharacterUI SelectedCharacter => selectedCharacter;

	public static NewPartyDisplayUI PartyDisplay => Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI;

	public bool IsOpen => window.IsOpen;

	public event SelectionEvent OnOpenedPanel;

	public event BasicEventHandler OnAbilityDeckUpdated;

	protected override void AwakeInit()
	{
		initialised = false;
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].OnClickedAssignRole.AddListener(OnMultiplayerAssignRoleSelected);
			NewPartyCharacterUI slot = charactersList[i];
			slot.OnHovered.AddListener(() =>
			{
				OnHoveredSlot(slot, hovered: true);
			});
			slot.OnUnhovered.AddListener(() =>
			{
				OnHoveredSlot(slot, hovered: false);
			});
		}
		showPanelAnimator.OnAnimationFinished.AddListener(() =>
		{
			animLockInteraction.blocksRaycasts = true;
		});
		hidePanelAnimator.OnAnimationFinished.AddListener(() =>
		{
			Log.GUIInfo(string.Format("Finish hide animation {0} ({1})", string.Join(",", hideRequests), hideCallback == null));
			animLockInteraction.blocksRaycasts = true;
			window.Hide();
			hideCallback?.Invoke();
		});
	}

	private void OnHoveredSlot(NewPartyCharacterUI slot, bool hovered)
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].ToggleHoverDim(hovered && charactersList[i] != slot);
		}
	}

	public void Init(CMapParty party)
	{
		mapParty = party;
		itemService = new AdventurePartyItemEquipmentService(party);
		onCharacterSelectedCallback = null;
		flagsInteraction = FlagsInteraction.None;
		characterSelector.Initialize(mapParty, OnSelectedNewCharacter, OnDeselectedCharacter);
		if (AdventureState.MapState.IsCampaign)
		{
			partyStats = campaignPartyStats;
			guildmasterPartyStats.gameObject.SetActive(value: false);
		}
		else
		{
			partyStats = guildmasterPartyStats;
			campaignPartyStats.gameObject.SetActive(value: false);
		}
		Open();
		initialised = true;
	}

	private void Open()
	{
		partyStats.Setup(mapParty);
		partyNameText.text = ((SaveData.Instance.Global.CurrentAdventureData != null) ? SaveData.Instance.Global.CurrentAdventureData.PartyName : string.Empty);
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			partyGoldContainer.SetActive(value: true);
			RefreshPartyGold(mapParty.PartyGold, animate: false);
		}
		else
		{
			partyGoldContainer.SetActive(value: false);
		}
		_ = SaveData.Instance.Global.GameMode;
		_ = 2;
		int num = (AdventureState.MapState.IsCampaign ? 4 : Mathf.Clamp(1, mapParty.CheckCharacters.Count, charactersList.Count));
		for (int i = 0; i < mapParty.SelectedCharactersArray.Count(); i++)
		{
			CMapCharacter cMapCharacter = mapParty.SelectedCharactersArray[i];
			if (cMapCharacter != null)
			{
				int controllableID = ControllableRegistry.GetControllableID(cMapCharacter);
				charactersList[i].Init(null, i, cMapCharacter, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, itemService.CountNewItems(mapParty.SelectedCharactersArray[i]), OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected, updateController: true);
			}
			else if (i < num)
			{
				charactersList[i].InitAvailable(null, i, slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
			else
			{
				charactersList[i].InitEmpty(null, i);
			}
		}
		ShowNewClassNotification(mapParty.NewUnlockedCharacterIDs.Count > 0);
		abilityCardsDisplay.Hide();
		perkManager.Hide();
		ShowDelayed();
	}

	private void OnXpChanged(string characterId, string characterName)
	{
		GetCharacterUI(characterId, characterName)?.OnXpChanged();
	}

	[ContextMenu("Show")]
	public void Show(object request)
	{
		hideRequests.Remove(request);
		Log.GUIInfo(string.Format("Show party panel requested by {0} (current hidding request: {1}) isOpen {2} isHidding {3}", request, string.Join(",", hideRequests), window.IsOpen, hidePanelAnimator.IsPlaying));
		if ((!window.IsOpen || hidePanelAnimator.IsPlaying) && hideRequests.Count <= 0)
		{
			CancelShowAnimations();
			window.Show();
			if (!window.HasGoneToStartingState)
			{
				window.onShown.Invoke();
			}
			showPanelAnimator.Play(fromStart: false);
			animLockInteraction.blocksRaycasts = false;
		}
	}

	private void ShowDelayed()
	{
		CancelShowAnimations();
		window.Hide();
		showPanelAnimator.GoInitState();
		delayedShowCoroutine = UIManager.Instance.StartCoroutine(DelayedShow());
	}

	private IEnumerator DelayedShow()
	{
		yield return Timekeeper.instance.WaitForSeconds(openPanelDelay);
		delayedShowCoroutine = null;
		Show(this);
	}

	[ContextMenu("Hide")]
	public void Hide(object request)
	{
		Hide(request, instant: false);
	}

	public void Hide(object request, bool instant, Action callback = null, bool deselectCurrentCharacter = true)
	{
		hideRequests.Add(request);
		if (!window.IsOpen)
		{
			Log.GUIInfo($"Hide party panel already closed, requested by {request} ({hideCallback == null})");
			if (delayedShowCoroutine != null)
			{
				CancelShowAnimations();
			}
			return;
		}
		if (deselectCurrentCharacter)
		{
			UnselectCurrentCharacter();
		}
		CancelShowAnimations();
		if (instant)
		{
			Log.GUIInfo($"Hide party panel immediatly requested by {request} ({hidePanelAnimator.IsPlaying}) ({hideCallback == null})");
			hidePanelAnimator.GoToFinishState();
			window.Hide();
			callback?.Invoke();
		}
		else
		{
			Log.GUIInfo($"Hide party panel animation requested by {request} ({hidePanelAnimator.IsPlaying}) ({hideCallback == null})");
			hideCallback = callback;
			animLockInteraction.blocksRaycasts = false;
			hidePanelAnimator.Play();
		}
	}

	private void CancelShowAnimations()
	{
		if (delayedShowCoroutine != null)
		{
			StopCoroutine(delayedShowCoroutine);
		}
		delayedShowCoroutine = null;
		showPanelAnimator.Stop(goToEnd: false);
		hidePanelAnimator.Stop(goToEnd: false);
	}

	private void OnUnlockedCharacter(CMapCharacter character)
	{
		int count = mapParty.CheckCharacters.Count;
		int num = charactersList.Where((NewPartyCharacterUI w) => w.State != PartySlotState.Empty).Count();
		if (num != charactersList.Count && count > num)
		{
			NewPartyCharacterUI newPartyCharacterUI = charactersList.FirstOrDefault((NewPartyCharacterUI f) => f.State == PartySlotState.Empty);
			if (newPartyCharacterUI != null)
			{
				newPartyCharacterUI.InitAvailable(mapParty, charactersList.IndexOf(newPartyCharacterUI), slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
		}
		if (!AdventureState.MapState.IsCampaign && mapParty.SelectedCharacters.Count() < charactersList.Count && mapParty.SelectedCharacters.Count() < 2)
		{
			NewPartyCharacterUI newPartyCharacterUI2 = charactersList.FirstOrDefault((NewPartyCharacterUI f) => f.State == PartySlotState.Available);
			if (newPartyCharacterUI2 != null)
			{
				int controllableID = ControllableRegistry.GetControllableID(character);
				newPartyCharacterUI2.Init(mapParty, charactersList.IndexOf(newPartyCharacterUI2), character, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, itemService.CountNewItems(character), OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected);
			}
		}
	}

	public override void UpdatePartyLevel()
	{
		partyStats.UpdateLevel();
	}

	private new void OnDestroy()
	{
		CancelShowAnimations();
	}

	private void OnDisable()
	{
		CancelShowAnimations();
	}

	private void OnNewItemAdded(CItem item)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.ShowNewItemsNotification(itemService.CountNewItems(characters.Data) > 0);
		}
	}

	private void OnItemBound(CItem item, CMapCharacter character)
	{
		if (item.IsNew)
		{
			RefreshNewItemsNotification();
		}
	}

	private void RefreshPartyGold(int gold, bool animate)
	{
		ShowWarningGold(show: false);
		if (animate)
		{
			partyGoldCounter.CountTo(gold);
		}
		else
		{
			partyGoldCounter.SetCount(gold);
		}
	}

	private void OnPartyGoldChanged(int money)
	{
		RefreshPartyGold(money, animate: true);
	}

	public void RefreshNewClassesNotification()
	{
		if (initialised)
		{
			try
			{
				ShowNewClassNotification(mapParty.NewUnlockedCharacterIDs.Count > 0);
			}
			catch (Exception ex)
			{
				Log.GUIException(ex, "Exception while trying to refresh new classes notification. MapParty is null: " + (mapParty == null) + " NewUnlockedCharacterIDs is null: " + (mapParty?.NewUnlockedCharacterIDs == null));
				throw ex;
			}
		}
	}

	private void RefreshNewItemsNotification(CItem item)
	{
		RefreshNewItemsNotification();
	}

	private void RefreshNewItemsNotification(CItem item, int slot)
	{
		RefreshNewItemsNotification();
	}

	public void UnselectCurrentCharacter()
	{
		if (selectedCharacter != null)
		{
			selectedCharacter.OnClick();
		}
	}

	public void Cancel()
	{
		if (window.IsOpen)
		{
			UnselectCurrentCharacter();
		}
	}

	private void OnCharacterSelect(bool isSelected, NewPartyCharacterUI characterUI, bool openDefault)
	{
		if (isSelected)
		{
			characterUI.ToggleSelect(active: true);
			selectedCharacter = characterUI;
			foreach (NewPartyCharacterUI characters in charactersList)
			{
				if (characters != characterUI)
				{
					characters.ToggleSelect(active: false);
					characters.ToggleDim(active: true);
				}
			}
			onCharacterSelectedCallback?.Invoke(characterUI.Data);
			return;
		}
		if (selectedCharacter == characterUI && flagsInteraction.HasFlag(FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER))
		{
			selectedCharacter.ToggleSelect(active: true);
			return;
		}
		selectedCharacter = null;
		abilityCardsDisplay.Hide();
		CloseQuickAccessAssignRole();
		if (slotsToggleGroup.AnyTogglesOn())
		{
			foreach (Toggle item in slotsToggleGroup.ActiveToggles())
			{
				if (item.isOn)
				{
					item.isOn = false;
				}
			}
		}
		foreach (NewPartyCharacterUI characters2 in charactersList)
		{
			characters2.ToggleDim(active: false);
		}
	}

	private void OnAbilityCardSelected(bool isSelected)
	{
		OnAbilityDeckUpdated?.Invoke();
	}

	private void OnCardsSelected(NewPartyCharacterUI data, bool isSelected)
	{
		OnCardsSelected(data, isSelected, animate: true);
	}

	private void OnCardsSelected(NewPartyCharacterUI data, bool isSelected, bool animate)
	{
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			displayActive = DisplayType.CARDS;
			abilityCardsDisplay.Display(data.Data, OnAbilityCardSelected, selectedCharacter.CardPointReference, disableStatsRequests.Count == 0, animate, () =>
			{
				OnLevelUpSelected(data);
			}, () =>
			{
				data.CloseCardsWindow();
			});
		}
		else
		{
			displayActive = DisplayType.NONE;
			abilityCardsDisplay.Hide(!slotsToggleGroup.AnyTogglesOn());
		}
		OnOpenedPanel?.Invoke(isSelected, DisplayType.CARDS);
	}

	public void RefreshAbilityCardsLevelUp(ICharacterService character)
	{
		if (abilityCardsDisplay.IsVisible && character != null && selectedCharacter?.Service == character)
		{
			abilityCardsDisplay.EnableLevelUp(disableStatsRequests.Count == 0 && character.CanLevelup());
		}
	}

	private void OnItemsSelected(NewPartyCharacterUI data, bool isSelected)
	{
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			displayActive = DisplayType.INVENTORY;
			itemInventoryDisplay.Display(itemService, data.Data, selectedCharacter.AddEquippedItemModifiers, (CItem item) =>
			{
				selectedCharacter.RefreshEquippedItemModifiers();
			}, data.ItemPointReference, OnNewItemsDisplayed, disableStatsRequests.Count == 0, data.CloseItemsWindow);
		}
		else
		{
			displayActive = DisplayType.NONE;
			itemInventoryDisplay.Hide(!slotsToggleGroup.AnyTogglesOn());
		}
		OnOpenedPanel?.Invoke(isSelected, DisplayType.INVENTORY);
	}

	private void OnLevelUpSelected(NewPartyCharacterUI characterUI)
	{
	}

	private void OnMultiplayerAssignRoleSelected(NewPartyCharacterUI characterUI)
	{
		Log.GUIInfo("OnMultiplayerAssignRoleSelected");
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost && characterUI.State != PartySlotState.Empty && disableAssignPlayerRequests.Count <= 0)
		{
			if (characterUI.State == PartySlotState.Assigned)
			{
				Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Show(name, characterUI.Data.CharacterID, characterUI.Data.CharacterName, characterUI.SlotIndex, characterUI.Data.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterUI.Data.CharacterYMLData.LocKey) : characterUI.Data.CharacterName, characterUI.Player, characterUI.ClassPointReference, null, null, (transform as RectTransform).rect.width);
			}
			else
			{
				Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Show(name, null, null, characterUI.SlotIndex, string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HERO_SLOT"), charactersList.IndexOf(characterUI) + 1), characterUI.Player, characterUI.ClassPointReference, null, null, (transform as RectTransform).rect.width);
			}
		}
	}

	private void OnNewItemsDisplayed(CItem item)
	{
		RefreshNewItemsNotification();
	}

	private void RefreshNewItemsNotification()
	{
		foreach (NewPartyCharacterUI item in charactersList.Where((NewPartyCharacterUI it) => it.Data != null))
		{
			item.ShowNewItemsNotification(itemService.CountNewItems(item.Data) > 0);
		}
	}

	private void OnPerksSelected(NewPartyCharacterUI ui, bool isSelected)
	{
		abilityCardsDisplay.Hide();
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			displayActive = DisplayType.PERKS;
			perkManager.Show(ui.Service, selectedCharacter.PerksPointReference, OnHoveredPerk, disableStatsRequests.Count == 0, ui.ClosePerksWindow);
		}
		else
		{
			displayActive = DisplayType.NONE;
			perkManager.Hide(!slotsToggleGroup.AnyTogglesOn());
		}
		OnOpenedPanel?.Invoke(isSelected, DisplayType.PERKS);
	}

	private void OnHoveredPerk(AttackModifierYMLData modifier)
	{
		selectedCharacter.Data.NewEquippedItemsWithModifiers?.Remove(modifier.Name);
		selectedCharacter.RefreshEquippedItemModifiers();
		SaveData.Instance.ManualSaveCurrentAdventureData();
	}

	private void OnBattleGoalSelected(NewPartyCharacterUI ui, bool isSelected)
	{
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			displayActive = DisplayType.BATTLE_GOALS;
		}
		else
		{
			displayActive = DisplayType.NONE;
		}
		OnOpenedPanel?.Invoke(isSelected, DisplayType.BATTLE_GOALS);
	}

	private void OnCharacterPickerSelected(NewPartyCharacterUI ui, bool isSelected, RectTransform point)
	{
		abilityCardsDisplay.Hide();
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			displayActive = DisplayType.CHARACTER_SELECTOR;
			characterSelector.Show(point, ui.Data, () =>
			{
				ui.CloseAssigWindow();
			}, !flagsInteraction.HasFlag(FlagsInteraction.DISABLE_EDIT_PARTY) && (!FFSNetwork.IsOnline || PlayerRegistry.GetPlayerAssignedToSlot(PartySlotID.FromIndex(ui.SlotIndex)) == PlayerRegistry.MyPlayer));
		}
		else
		{
			displayActive = DisplayType.NONE;
			characterSelector.Hide(instant: false, !slotsToggleGroup.AnyTogglesOn());
		}
		OnOpenedPanel?.Invoke(isSelected, DisplayType.CHARACTER_SELECTOR);
	}

	private void OnSelectedNewCharacter(CMapCharacter character, NewPartyCharacterUI slot)
	{
		if (slot != null)
		{
			AssignNewCharacterToUISlot(character, slot, PlayerRegistry.MyPlayer);
		}
		else
		{
			AssignNewCharacterToUISlot(character, selectedCharacter, PlayerRegistry.MyPlayer);
		}
	}

	public void AssignNewCharacterToUISlot(CMapCharacter character, NewPartyCharacterUI characterUISlot, NetworkPlayer initialController = null)
	{
		int newItemsNotification = itemService.CountNewItems(character);
		NewPartyCharacterUI newPartyCharacterUI = charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data == character);
		if (newPartyCharacterUI != null)
		{
			if (characterUISlot.Data == null)
			{
				Log.GUIInfo("Init Available " + newPartyCharacterUI.Data.CharacterID);
				newPartyCharacterUI.InitAvailable(mapParty, charactersList.IndexOf(newPartyCharacterUI), slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
			else
			{
				int controllableID = ControllableRegistry.GetControllableID(character);
				newPartyCharacterUI.Init(mapParty, charactersList.IndexOf(newPartyCharacterUI), characterUISlot.Data, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, newItemsNotification, OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected, updateController: true);
			}
		}
		characterUISlot.Init(mapParty, charactersList.IndexOf(characterUISlot), character, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, newItemsNotification, OnCharacterPickerSelected, OnLevelUpSelected, initialController, OnBattleGoalSelected, updateController: true);
		if (!FFSNetwork.IsOnline)
		{
			characterUISlot.ShowClassToggled();
		}
		if (characterUISlot == selectedCharacter)
		{
			onCharacterSelectedCallback?.Invoke(character);
		}
	}

	private void OnDeselectedCharacter(CMapCharacter character)
	{
		NewPartyCharacterUI newPartyCharacterUI = charactersList.FirstOrDefault((NewPartyCharacterUI x) => x.Data == character);
		if ((bool)newPartyCharacterUI)
		{
			newPartyCharacterUI.InitAvailable(mapParty, charactersList.IndexOf(newPartyCharacterUI), slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			if (!FFSNetwork.IsClient && slotsToggleGroup.AnyTogglesOn() && selectedCharacter == newPartyCharacterUI)
			{
				newPartyCharacterUI.ShowAssignToggled();
			}
			if (newPartyCharacterUI == selectedCharacter)
			{
				onCharacterSelectedCallback?.Invoke(null);
			}
		}
		else
		{
			Log.GUIWarning("Trying to remove the character but the character UI slot does not exist.");
		}
	}

	public override void CloseWindows()
	{
		perkManager.Hide();
		selectedCharacter?.CloseWindows();
		CloseQuickAccessAssignRole();
	}

	private void CloseQuickAccessAssignRole()
	{
		if (Singleton<UIMultiplayerSelectPlayerScreen>.Instance.IsOpen && Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Id == name)
		{
			Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Hide();
		}
	}

	public NewPartyCharacterUI GetCharacterUI(string characterID, string characterName = null)
	{
		if (characterName != null)
		{
			return charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data != null && it.Data.CharacterID == characterID && it.Data.CharacterName == characterName);
		}
		return charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data != null && it.Data.CharacterID == characterID);
	}

	public int GetCharacterSlot(NewPartyCharacterUI characterUI)
	{
		return charactersList.IndexOf(characterUI);
	}

	public override void OpenCardsPanel(string characterID, float durationOpen = -1f)
	{
		GetCharacterUI(characterID).ToggleCardsPanel(toggledOn: true);
		abilityCardsDisplay.Show();
	}

	public void OpenBattleGoalPanel(string characterID)
	{
		GetCharacterUI(characterID)?.ToggleBatteGoal(toggledOn: true);
	}

	public override void ToggleCardsPanel(string characterID)
	{
		NewPartyCharacterUI characterUI = GetCharacterUI(characterID);
		characterUI.ToggleCardsPanel(!characterUI.CardWindowSelected);
	}

	public void EnableEnhancementMode(Action<NewPartyCharacterUI> onSelectedCharacter)
	{
		flagsInteraction = flagsInteraction | FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER | FlagsInteraction.DISABLE_BUTTONS | FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT;
		charactersList.ForEach((NewPartyCharacterUI it) =>
		{
			it.DisableButtons(ignoreCardButton: true);
			it.EnableAutoOpenDefaultPanel(isEnabled: false);
		});
		onCharacterSelectedCallback = (CMapCharacter character) =>
		{
			onSelectedCharacter(selectedCharacter);
		};
		if (mapParty.SelectedCharacters.Count() > 0)
		{
			OnCharacterSelect(isSelected: true, (selectedCharacter == null || selectedCharacter.State != PartySlotState.Assigned) ? charactersList.First((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned) : selectedCharacter, openDefault: false);
		}
		DisableMapOptions();
	}

	public void DisableEnhancementMode()
	{
		flagsInteraction = flagsInteraction & ~FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER & ~FlagsInteraction.DISABLE_BUTTONS & ~FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT;
		charactersList.ForEach((NewPartyCharacterUI it) =>
		{
			it.EnableButtons(ignoreCardButton: true);
			it.EnableAutoOpenDefaultPanel(!flagsInteraction.HasFlag(FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT));
		});
		onCharacterSelectedCallback = null;
	}

	public void DisableAssignToEmptySlot()
	{
		foreach (NewPartyCharacterUI item in charactersList.Where((NewPartyCharacterUI it) => it.State == PartySlotState.Available))
		{
			item.DisableAssign();
		}
	}

	public void EnableSelectionMode(Action<NewPartyCharacterUI> onSelectedCharacter, bool disableButtons = true)
	{
		flagsInteraction = flagsInteraction | FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER | FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT;
		if (disableButtons)
		{
			flagsInteraction |= FlagsInteraction.DISABLE_BUTTONS;
		}
		charactersList.ForEach((NewPartyCharacterUI it) =>
		{
			if (disableButtons || it.State != PartySlotState.Assigned)
			{
				it.DisableButtons(ignoreCardButton: false);
			}
			it.EnableAutoOpenDefaultPanel(isEnabled: false);
			it.EnableSelectCharacter(enable: false);
		});
		EnableAssignPlayer(enable: false, this);
		onCharacterSelectedCallback = (CMapCharacter character) =>
		{
			if ((selectedCharacter == null || selectedCharacter.State != PartySlotState.Assigned) && mapParty.SelectedCharacters.Any())
			{
				OnCharacterSelect(isSelected: true, charactersList.First((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned), openDefault: false);
			}
			else
			{
				onSelectedCharacter(selectedCharacter);
			}
		};
		if (mapParty.SelectedCharacters.Any())
		{
			OnCharacterSelect(isSelected: true, (selectedCharacter == null || selectedCharacter.State != PartySlotState.Assigned) ? charactersList.First((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned) : selectedCharacter, openDefault: false);
		}
		DisableMapOptions();
	}

	public void DisableSelectionMode()
	{
		flagsInteraction = flagsInteraction & ~FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER & ~FlagsInteraction.DISABLE_BUTTONS & ~FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT;
		charactersList.ForEach((NewPartyCharacterUI it) =>
		{
			it.EnableButtons(ignoreCardButton: false);
			it.EnableSelectCharacter(enable: true);
			it.EnableAutoOpenDefaultPanel(!flagsInteraction.HasFlag(FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT));
		});
		onCharacterSelectedCallback = null;
		EnableAssignPlayer(enable: true, this);
	}

	public void DisableMapOptions()
	{
		EnableResetLevel(enableReset: false);
		if (selectedCharacter != null && selectedCharacter.State != PartySlotState.Assigned)
		{
			UnselectCurrentCharacter();
		}
		charactersList.ForEach((NewPartyCharacterUI it) =>
		{
			if (it.State == PartySlotState.Available)
			{
				it.DisableSelection();
			}
		});
		CloseWindows();
	}

	public void EnableMapOptions()
	{
		EnableResetLevel(enableReset: true);
		charactersList.ForEach((NewPartyCharacterUI it) =>
		{
			if (it.State == PartySlotState.Available)
			{
				it.EnableSelection();
			}
		});
	}

	public void EnableResetLevel(bool enableReset)
	{
		if (enableReset)
		{
			flagsInteraction &= ~FlagsInteraction.DISABLE_RESET_LEVELUP;
		}
		else
		{
			flagsInteraction |= FlagsInteraction.DISABLE_RESET_LEVELUP;
		}
	}

	public void EnableClickTracker(bool enable, Component request)
	{
		if (enable)
		{
			disableClickTrackerRequests.Remove(request);
		}
		else
		{
			disableClickTrackerRequests.Add(request);
		}
		clickTracker.enabled = disableClickTrackerRequests.Count == 0;
	}

	public void ShowRequiredCharactersWarning(int requiredCharacters, bool glow)
	{
		int num = Math.Min(charactersList.Count, mapParty.CheckCharacters.Count);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (charactersList[i].Data == null)
			{
				if (num2 < requiredCharacters)
				{
					charactersList[i].ShowAssignWarning(glow);
				}
				else
				{
					charactersList[i].HideAssignWarning();
				}
				num2++;
			}
		}
	}

	public void HideRequiredCharactersWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideAssignWarning();
		}
	}

	public void ShowClassCharactersWarning(bool glow)
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			if (charactersList[i].State == PartySlotState.Assigned)
			{
				charactersList[i].ShowClassWarning(glow);
			}
		}
	}

	public void HideClassCharactersWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideClassWarning();
		}
	}

	public void ShowCardsWarning(List<CMapCharacter> characters, bool glow)
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			if (charactersList[i].Data != null && characters.Contains(charactersList[i].Data))
			{
				charactersList[i].ShowCardsWarning(glow);
			}
		}
		for (int j = 0; j < charactersList.Count; j++)
		{
			if (charactersList[j].Data != null)
			{
				if (characters.Contains(charactersList[j].Data))
				{
					charactersList[j].ShowCardsWarning(glow);
				}
				else
				{
					charactersList[j].HideCardsWarning();
				}
			}
		}
	}

	public void HideCardsWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideCardsWarning();
		}
	}

	public void HideLevelUpUI()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].DisableUIForLevelingUp();
		}
	}

	public void ShowWarningGold(bool show, string characterId = null)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			if (!characterId.IsNOTNullOrEmpty())
			{
				foreach (NewPartyCharacterUI item in charactersList.Where((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned))
				{
					item.ShowWarningGold(show);
				}
				return;
			}
			GetCharacterUI(characterId).ShowWarningGold(show);
		}
		else if (show)
		{
			warningPartyGoldAnimator.Play();
		}
		else
		{
			warningPartyGoldAnimator.Stop();
		}
	}

	public void ShowNewClassNotification(bool show)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.ShowNewClassNotification(show);
		}
	}

	public void ShowPerksWarning(string characterID, bool show)
	{
		GetCharacterUI(characterID).ShowPerksWarning(show);
	}

	public void EnableCharacterSelection(bool enable, Component request)
	{
		if (enable)
		{
			disableCharacterSelectionRequests.Remove(request);
		}
		else
		{
			disableCharacterSelectionRequests.Add(request);
		}
		if (disableCharacterSelectionRequests.Count > 0)
		{
			flagsInteraction |= FlagsInteraction.DISABLE_EDIT_PARTY;
		}
		else
		{
			flagsInteraction &= ~FlagsInteraction.DISABLE_EDIT_PARTY;
		}
	}

	public void EnableEditStats(Component request, bool closeWindowsOnEnable)
	{
		if (disableStatsRequests.Remove(request) && disableStatsRequests.Count == 0)
		{
			if (closeWindowsOnEnable)
			{
				CloseWindows();
			}
			else if (abilityCardsDisplay.IsVisible)
			{
				abilityCardsDisplay.SetSelectable(isEditAllowed: true);
			}
		}
	}

	public void DisableEditStats(Component request, bool closeWindowsOnDisable)
	{
		disableStatsRequests.Add(request);
		if (closeWindowsOnDisable && disableStatsRequests.Count == 1)
		{
			CloseWindows();
		}
	}

	public void EnableAssignPlayer(bool enable, Component request)
	{
		if (enable)
		{
			disableAssignPlayerRequests.Remove(request);
		}
		else
		{
			disableAssignPlayerRequests.Add(request);
			CloseQuickAccessAssignRole();
		}
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.EnableAssignPlayer(disableAssignPlayerRequests.Count == 0);
		}
	}

	public void UpdateConnectionStatus(List<string> characterIDs)
	{
		for (int i = 0; i < characterIDs.Count; i++)
		{
			GetCharacterUI(characterIDs[i]).RefreshMultiplayerState();
		}
	}

	public void UpdateConnectionStatus()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].RefreshMultiplayerState();
		}
	}

	public void EnableBattleGoalsSelection(bool enable)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.EnableBattleGoalsSelection(enable);
		}
	}

	public void EnableBattleGoalsSelection(string characterId, bool interactable, string tooltip)
	{
		GetCharacterUI(characterId).EnableBattleGoalsSelection(enable: true, interactable, tooltip);
	}

	public void ShowBattleGoalWarning(List<string> charactersID, bool glow)
	{
		if (charactersID.IsNullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < charactersList.Count; i++)
		{
			if (charactersList[i].Data != null)
			{
				if (charactersID.Contains(charactersList[i].Data.CharacterID))
				{
					charactersList[i].ShowBattleGoalWarning(glow);
				}
				else
				{
					charactersList[i].HideBattleGoalWarning();
				}
			}
		}
	}

	public void HideBattleGoalWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideBattleGoalWarning();
		}
	}

	public bool RemoveCharacterFromSlot(string characterID, bool save)
	{
		bool executionFinished = false;
		return RemoveCharacterFromSlot(GetCharacterUI(characterID)?.Data, ref executionFinished, save);
	}

	private bool RemoveCharacterFromSlot(CMapCharacter character, ref bool executionFinished, bool save = true, bool isProxyCall = false, int switchingPlayerID = 0)
	{
		if (character == null)
		{
			executionFinished = true;
			return false;
		}
		return characterSelector.RemoveCharacterFromParty(character, ref executionFinished, networkIfOnline: false, save, isProxyCall, switchingPlayerID);
	}

	public void DisableSlots(List<CMapCharacter> characterSlots)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			if (characters.State == PartySlotState.Assigned && characterSlots.Contains(characters.Data))
			{
				characters.DisableButtons(ignoreCardButton: false);
				characters.DisableSelection();
			}
		}
	}

	public void HideSlots(List<CMapCharacter> characterSlots)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			if (characters.State == PartySlotState.Assigned && characterSlots.Contains(characters.Data))
			{
				characters.HideAssignedCharacter();
			}
		}
	}

	public void MPRemoveCharacter(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		string targetCharacterID = ControllableRegistry.GetCharacterIDByControllerID(action.ActorID);
		CMapCharacter cMapCharacter = mapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		if (cMapCharacter != null)
		{
			Log.NetworkInfo("Removing " + cMapCharacter.CharacterID + " from the party.");
			actionValid = RemoveCharacterFromSlot(cMapCharacter, ref executionFinished, save: true, isProxyCall: true, action.SupplementaryDataIDMax);
			return;
		}
		Log.GUIInfo("[IsSwitchingCharacter] False - Unable to find character to remove");
		executionFinished = true;
		actionValid = false;
		Log.NetworkWarning("Error trying to remove a character. Character returns null (ControllableID: " + action.ActorID + ").");
	}

	public void MPSwitchCharacter(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		CMapCharacter character = null;
		if (AdventureState.MapState.IsCampaign)
		{
			character = ControllableRegistry.GetMapCharacterByControllerID(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			character = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (character != null)
		{
			if (AdventureState.MapState.IsCampaign && mapParty.SelectedCharacters.Any((CMapCharacter it) => it.CharacterID == character.CharacterID && it.CharacterName != character.CharacterName && action.SupplementaryDataIDMed != mapParty.SelectedCharactersArray.IndexOf(it)))
			{
				Log.GUIInfo("Character class already in party");
				actionValid = false;
				executionFinished = true;
			}
			else
			{
				Log.NetworkInfo(PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax).Username + " switching to " + character.CharacterID);
				executionFinished = false;
				actionValid = characterSelector.SelectCharacter(charactersList[action.SupplementaryDataIDMed], character, ref executionFinished, PlayerRegistry.GetPlayer(action.PlayerID), networkActionIfOnline: false, action.SupplementaryDataIDMax == PlayerRegistry.MyPlayer.PlayerID.ID, isProxyCall: true, action.SupplementaryDataIDMax);
				CloseQuickAccessAssignRole();
			}
		}
		else
		{
			Log.GUIInfo("[IsSwitchingCharacter] False - Unable to find character to switch");
			actionValid = false;
			executionFinished = true;
			if (FFSNetwork.IsClient)
			{
				throw new Exception("Error switching character. Character returns null (ControllableID: " + action.ActorID + ").");
			}
		}
	}

	public void ProxyLevelUp(GameAction action)
	{
		ProxyIncreaseLevelNumber(action.ActorID, ((LevelToken)action.SupplementaryDataToken2).Level);
		ProxyUpdatePerkPoints(action.ActorID, ((PerkPointsToken)action.SupplementaryDataToken3).PerkPoints);
		ProxyModifyCardInventory(action.ActorID, (CardInventoryToken)action.SupplementaryDataToken);
		SaveData.Instance.ManualSaveCurrentAdventureData();
	}

	private void ProxyIncreaseLevelNumber(int controllableID, int newLevel)
	{
		CMapCharacter character = null;
		if (AdventureState.MapState.IsCampaign)
		{
			character = ControllableRegistry.GetMapCharacterByControllerID(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			character = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (character != null)
		{
			if (!mapParty.UnselectedCharacters.Contains(character))
			{
				Log.NetworkInfo("Modifying level for " + character.CharacterID);
				NewPartyCharacterUI newPartyCharacterUI = charactersList.First((NewPartyCharacterUI x) => x.Data == character);
				for (int num = newLevel - character.Level; num > 0; num--)
				{
					newPartyCharacterUI.LevelUp();
				}
			}
			return;
		}
		throw new Exception("Error increasing level number for a proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void ProxyModifyCardInventory(GameAction action)
	{
		ProxyModifyCardInventory(action.ActorID, (CardInventoryToken)action.SupplementaryDataToken);
		SaveData.Instance.ManualSaveCurrentAdventureData();
	}

	private void ProxyModifyCardInventory(int controllableID, CardInventoryToken inventoryToken)
	{
		CMapCharacter character = null;
		if (AdventureState.MapState.IsCampaign)
		{
			character = ControllableRegistry.GetMapCharacterByControllerID(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			character = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == character.CharacterID);
		if (character != null)
		{
			Log.NetworkInfo("Modifying card inventory for " + character.CharacterID);
			List<int> list = character.HandAbilityCardIDs.ToList();
			character.HandAbilityCardIDs.Clear();
			character.OwnedAbilityCardIDs.Clear();
			int i;
			for (i = 0; i < inventoryToken.SelectedCardIDs.Length; i++)
			{
				if (cCharacterClass.AbilityCardsPool.Find((CAbilityCard x) => x.ID == inventoryToken.SelectedCardIDs[i]) != null)
				{
					character.OwnedAbilityCardIDs.Add(inventoryToken.SelectedCardIDs[i]);
					character.HandAbilityCardIDs.Add(inventoryToken.SelectedCardIDs[i]);
				}
			}
			int i2;
			for (i2 = 0; i2 < inventoryToken.UnselectedCardIDs.Length; i2++)
			{
				if (cCharacterClass.AbilityCardsPool.Find((CAbilityCard x) => x.ID == inventoryToken.UnselectedCardIDs[i2]) != null)
				{
					character.OwnedAbilityCardIDs.Add(inventoryToken.UnselectedCardIDs[i2]);
				}
			}
			if (abilityCardsDisplay.IsVisible && displayActive == DisplayType.CARDS && selectedCharacter != null && selectedCharacter.Data == character)
			{
				OnCardsSelected(selectedCharacter, isSelected: true, animate: false);
			}
			NetworkPlayer controller = ControllableRegistry.GetController(controllableID);
			{
				foreach (int cardID in character.OwnedAbilityCardIDs)
				{
					CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.SingleOrDefault((CAbilityCard x) => x.ID == cardID);
					if (list.Contains(cardID))
					{
						if (!character.HandAbilityCardIDs.Contains(cardID))
						{
							Log.SimpleInfo(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " deselected card: " + cAbilityCard.StrictName + "(CardID: " + cAbilityCard.ID + ").");
						}
					}
					else if (character.HandAbilityCardIDs.Contains(cardID))
					{
						Log.SimpleInfo(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " selected card: " + cAbilityCard.StrictName + "(CardID: " + cAbilityCard.ID + ").");
					}
				}
				return;
			}
		}
		throw new Exception("Error modifying card inventory for proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void LogProxyModifyItemInventoryOnHost(int controllableID, ItemInventoryToken itemInventoryToken)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter == null)
		{
			return;
		}
		List<CItem> allUnlockedItems = mapParty.GetAllUnlockedItems(includeMultiplayerItemReserve: true);
		List<CItem> boundAndEquippedItems = cMapCharacter.GetBoundAndEquippedItems();
		List<CItem> list = cMapCharacter.CheckEquippedItems.ToList();
		List<CItem> list2 = new List<CItem>();
		List<CItem> list3 = new List<CItem>();
		List<CItem> list4 = new List<CItem>();
		List<CItem> list5 = new List<CItem>();
		Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Host logging proxy modify item inventory information. None of these changes are performed on the Host, this logging is just for comparing log files");
		Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + ". Item Count: " + itemInventoryToken.Items.Length + ". Listing items:");
		int i;
		for (i = 0; i < itemInventoryToken.Items.Length; i++)
		{
			CItem cItem = null;
			cItem = allUnlockedItems.SingleOrDefault((CItem x) => x.NetworkID == itemInventoryToken.Items[i].ItemNetworkID);
			if (cItem != null)
			{
				switch (itemInventoryToken.Items[i].ItemState)
				{
				case ItemInventoryToken.ItemState.Unbound:
					list4.Add(cItem);
					break;
				case ItemInventoryToken.ItemState.BoundButNotEquipped:
					list3.Add(cItem);
					break;
				case ItemInventoryToken.ItemState.Equipped:
					cItem.SlotIndex = itemInventoryToken.Items[i].SlotIndex;
					list2.Add(cItem);
					break;
				case ItemInventoryToken.ItemState.Sold:
					list5.Add(cItem);
					break;
				}
				Log.NetworkTrace("Name: " + cItem.Name + ". ItemState: " + itemInventoryToken.Items[i].ItemState.ToString() + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ".");
			}
			else
			{
				Log.NetworkWarning("No item found with NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", ItemState: " + itemInventoryToken.Items[i].ItemState, true);
			}
		}
		List<CItem> list6 = list2.Concat(list3).Concat(list4).Concat(list5)
			.ToList();
		foreach (CItem item in boundAndEquippedItems)
		{
			if (!list6.Exists((CItem x) => x.NetworkID == item.NetworkID) && !mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == item.NetworkID))
			{
				Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + item.Name + " NetworkID: " + item.NetworkID + ") added to Multiplayer item reserve.");
			}
		}
		foreach (CItem itemUpdatedInState in list6)
		{
			if (mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == itemUpdatedInState.NetworkID))
			{
				Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + itemUpdatedInState.Name + " NetworkID: " + itemUpdatedInState.NetworkID + ") removed from Multiplayer item reserve.");
			}
		}
		NetworkPlayer controller = ControllableRegistry.GetController(controllableID);
		foreach (CItem item2 in list)
		{
			if (!list2.Exists((CItem x) => x.NetworkID == item2.NetworkID))
			{
				Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " unequipped " + item2.YMLData.StringID + " (NetworkID: " + item2.NetworkID + ").", true);
			}
		}
		foreach (CItem item3 in list2)
		{
			if (!list.Exists((CItem x) => x.NetworkID == item3.NetworkID))
			{
				Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " equipped " + item3.YMLData.StringID + " (NetworkID: " + item3.NetworkID + ").", true);
			}
		}
		Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Finished processing " + cMapCharacter.CharacterID + ". Item count: " + (cMapCharacter.CheckEquippedItems.Count + cMapCharacter.CheckBoundItems.Count + mapParty.CheckUnboundItems.Count) + " (Equipped: " + cMapCharacter.CheckEquippedItems.Count + ". Inventory [bound]: " + cMapCharacter.CheckBoundItems.Count + ". Inventory [unbound]: " + mapParty.CheckUnboundItems.Count + ").");
	}

	private void ProxyModifyItemInventory(int controllableID, ItemInventoryToken itemInventoryToken)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			List<CItem> allUnlockedItems = mapParty.GetAllUnlockedItems(includeMultiplayerItemReserve: true);
			List<CItem> boundAndEquippedItems = cMapCharacter.GetBoundAndEquippedItems();
			List<CItem> checkUnboundItems = mapParty.CheckUnboundItems;
			List<CItem> allBoundPartyItems = mapParty.GetAllBoundPartyItems();
			List<CItem> checkEquippedItems = cMapCharacter.CheckEquippedItems;
			List<CItem> list = new List<CItem>();
			List<CItem> list2 = new List<CItem>();
			List<CItem> list3 = new List<CItem>();
			List<CItem> list4 = new List<CItem>();
			Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + ". Item Count: " + itemInventoryToken.Items.Length + ". Listing items:");
			int i;
			for (i = 0; i < itemInventoryToken.Items.Length; i++)
			{
				CItem cItem = null;
				cItem = allUnlockedItems.SingleOrDefault((CItem x) => x.NetworkID == itemInventoryToken.Items[i].ItemNetworkID);
				if (cItem != null)
				{
					switch (itemInventoryToken.Items[i].ItemState)
					{
					case ItemInventoryToken.ItemState.Unbound:
						list3.Add(cItem);
						break;
					case ItemInventoryToken.ItemState.BoundButNotEquipped:
						list2.Add(cItem);
						break;
					case ItemInventoryToken.ItemState.Equipped:
						cItem.SlotIndex = itemInventoryToken.Items[i].SlotIndex;
						list.Add(cItem);
						break;
					case ItemInventoryToken.ItemState.Sold:
						list4.Add(cItem);
						break;
					default:
						throw new Exception("[PROXY MODIFY ITEM INVENTORY]: Error modifying inventory. Uninitialized ItemState detected. ItemName: " + cItem.YMLData.StringID + ", ItemState: " + itemInventoryToken.Items[i].ItemState.ToString() + "NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ").");
					}
					Log.NetworkTrace("Name: " + cItem.Name + ". ItemState: " + itemInventoryToken.Items[i].ItemState.ToString() + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ".");
				}
				else
				{
					Log.NetworkWarning("No item found with NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", ItemState: " + itemInventoryToken.Items[i].ItemState, true);
				}
			}
			List<CItem> list5 = list.Concat(list2).Concat(list3).Concat(list4)
				.ToList();
			foreach (CItem item in boundAndEquippedItems.Concat(checkUnboundItems))
			{
				if (!list5.Exists((CItem x) => x.NetworkID == item.NetworkID) && !mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == item.NetworkID))
				{
					item.SlotIndex = int.MaxValue;
					item.IsSlotIndexSet = false;
					item.SlotState = CItem.EItemSlotState.None;
					mapParty.MultiplayerItemReserve.Add(item);
					Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + item.Name + " NetworkID: " + item.NetworkID + ") added to Multiplayer item reserve.");
				}
			}
			foreach (CItem itemUpdatedInState in list5)
			{
				if (mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == itemUpdatedInState.NetworkID))
				{
					mapParty.MultiplayerItemReserve.Remove(itemUpdatedInState);
					Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + itemUpdatedInState.Name + " NetworkID: " + itemUpdatedInState.NetworkID + ") removed from Multiplayer item reserve.");
				}
			}
			for (int num = list3.Count - 1; num >= 0; num--)
			{
				CItem newUnboundItem = list3[num];
				if (allBoundPartyItems.Exists((CItem x) => x.NetworkID == newUnboundItem.NetworkID) && !boundAndEquippedItems.Exists((CItem x) => x.NetworkID == newUnboundItem.NetworkID))
				{
					list3.Remove(newUnboundItem);
				}
			}
			cMapCharacter.ClearEquippedItems();
			cMapCharacter.NewEquippedItemsWithModifiers?.Clear();
			cMapCharacter.ClearBoundItems();
			mapParty.ClearUnboundItems();
			foreach (CItem item5 in list2)
			{
				cMapCharacter.AddItemToBoundItems(item5);
			}
			foreach (CItem item6 in list3)
			{
				mapParty.AddItemToUnboundItems(item6);
			}
			List<CMapCharacter> checkCharacters = mapParty.CheckCharacters;
			foreach (CItem item2 in list)
			{
				foreach (CMapCharacter item7 in checkCharacters)
				{
					if (item7 != cMapCharacter)
					{
						CItem cItem2 = item7.CheckEquippedItems.SingleOrDefault((CItem x) => x.ItemGuid == item2.ItemGuid);
						if (cItem2 != null)
						{
							item7.RemoveEquippedItem(cItem2);
						}
						CItem cItem3 = item7.CheckBoundItems.SingleOrDefault((CItem x) => x.ItemGuid == item2.ItemGuid);
						if (cItem3 != null)
						{
							item7.RemoveItemFromBoundItems(cItem3);
						}
					}
				}
				CItem cItem4 = checkUnboundItems.SingleOrDefault((CItem x) => x.ItemGuid == item2.ItemGuid);
				if (cItem4 != null)
				{
					mapParty.RemoveItemFromUnboundItems(cItem4);
					checkUnboundItems.Remove(cItem4);
				}
				cMapCharacter.EquipItem(item2, item2.SlotIndex, forceEquip: true);
			}
			cMapCharacter.GetSmallItemMax();
			List<CItem> list6 = new List<CItem>();
			foreach (CItem soldItem in list4)
			{
				if (!list6.Exists((CItem x) => x.NetworkID == soldItem.NetworkID))
				{
					Log.SimpleInfo("[PROXY MODIFY ITEM INVENTORY]: Local client user adding sold item back to merchant stock: " + soldItem.YMLData.StringID + " (NetworkID: " + soldItem.NetworkID + ").");
					list6.Add(soldItem);
					if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
					{
						cMapCharacter.ModifyGold(soldItem.SellPrice, useGoldModifier: true);
					}
				}
			}
			List<CItem> checkUnboundItems2 = mapParty.CheckUnboundItems;
			List<CItem> boundAndEquippedItems2 = cMapCharacter.GetBoundAndEquippedItems();
			for (int num2 = list6.Count - 1; num2 >= 0; num2--)
			{
				CItem cItem5 = list6[num2];
				if (checkUnboundItems2.Contains(cItem5) || boundAndEquippedItems2.Contains(cItem5))
				{
					Log.SimpleInfo("[PROXY MODIFY ITEM INVENTORY]: Local client user removing bought item from merchant stock: " + cItem5.YMLData.StringID + " (NetworkID: " + cItem5.NetworkID + ")" + ((cMapCharacter != null) ? (" for CharacterID: " + cMapCharacter.CharacterID) : ""));
					list6.Remove(cItem5);
					if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
					{
						cMapCharacter.ModifyGold(-(cItem5.YMLData.Cost + AdventureState.MapState.MapParty.ShopDiscount), useGoldModifier: true);
					}
				}
			}
			if (itemInventoryDisplay.IsVisible && selectedCharacter != null && selectedCharacter.Data == cMapCharacter)
			{
				itemInventoryDisplay.Refresh(cMapCharacter);
			}
			if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
			{
				mapParty.ModifyPartyGold(itemInventoryToken.PartyGold - mapParty.PartyGold, useGoldModifier: false);
			}
			NetworkPlayer controller = ControllableRegistry.GetController(controllableID);
			foreach (CItem item3 in checkEquippedItems)
			{
				if (!list.Exists((CItem x) => x.NetworkID == item3.NetworkID))
				{
					Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " unequipped " + item3.YMLData.StringID + " (NetworkID: " + item3.NetworkID + ").", true);
				}
			}
			foreach (CItem item4 in list)
			{
				if (!checkEquippedItems.Exists((CItem x) => x.NetworkID == item4.NetworkID))
				{
					Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " equipped " + item4.YMLData.StringID + " (NetworkID: " + item4.NetworkID + ").", true);
				}
			}
			Log.NetworkTrace("[PROXY MODIFY ITEM INVENTORY]: Finished processing " + cMapCharacter.CharacterID + ". Item count: " + (cMapCharacter.CheckEquippedItems.Count + cMapCharacter.CheckBoundItems.Count + mapParty.CheckUnboundItems.Count) + " (Equipped: " + cMapCharacter.CheckEquippedItems.Count + ". Inventory [bound]: " + cMapCharacter.CheckBoundItems.Count + ". Inventory [unbound]: " + mapParty.CheckUnboundItems.Count + ").");
			return;
		}
		throw new Exception("[PROXY MODIFY ITEM INVENTORY]: Error modifying item inventory for proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void ServerEquipItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = cMapCharacter.GetBoundAndEquippedItems().SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				if (!cMapCharacter.CheckEquippedItems.Contains(cItem) || cItem.SlotIndex != itemToken.ItemSlotIndex)
				{
					cMapCharacter.UnequipPreviouslyEquippedItems(cItem, itemToken.ItemSlotIndex);
					cMapCharacter.EquipItem(cItem, itemToken.ItemSlotIndex);
					FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
					NetworkPlayer controller = ControllableRegistry.GetController(ControllableRegistry.GetControllableID(cMapCharacter));
					Log.NetworkInfo(cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " equipped " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ").", true);
				}
				return;
			}
			throw new Exception("Error equipping an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
		}
		throw new Exception("Error equipping item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
	}

	public void ServerUnequipItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = cMapCharacter.CheckEquippedItems.SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem == null)
			{
				cItem = cMapCharacter.CheckBoundItems.SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			}
			if (cItem != null)
			{
				if (cMapCharacter.CheckEquippedItems.Contains(cItem))
				{
					cMapCharacter.UnequipItems(new List<CItem> { cItem });
					FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
					NetworkPlayer controller = ControllableRegistry.GetController(ControllableRegistry.GetControllableID(cMapCharacter));
					Log.NetworkInfo(cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " unequipped " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ").", true);
				}
				return;
			}
			throw new Exception("Error unequipping an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
		}
		throw new Exception("Error unequipping item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
	}

	public void ServerBindAndEquipItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = mapParty.CheckUnboundItems.SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				mapParty.BindItem(cMapCharacter.CharacterID, cMapCharacter.CharacterName, cItem);
				cMapCharacter.UnequipPreviouslyEquippedItems(cItem, itemToken.ItemSlotIndex);
				cMapCharacter.EquipItem(cItem, itemToken.ItemSlotIndex);
				FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
				NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
				NetworkPlayer controller = ControllableRegistry.GetController(ControllableRegistry.GetControllableID(cMapCharacter));
				Log.NetworkInfo(((player != null) ? player.Username : "NULL PLAYER") + " bound and equipped " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ") to " + cMapCharacter.CharacterID + " (Controller: " + ((controller != null) ? controller.Username : "NULL PLAYER") + ")", true);
			}
			else
			{
				Log.NetworkWarning("Error binding and equipping an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
			}
		}
		else
		{
			Log.NetworkWarning("Error binding and equipping an item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
		}
	}

	public void ServerUnbindItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = cMapCharacter.GetBoundAndEquippedItems().SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				switch (AdventureState.MapState.GoldMode)
				{
				case EGoldMode.PartyGold:
					if (mapParty.PartyGold < cItem.SellPrice)
					{
						return;
					}
					mapParty.ModifyPartyGold(-cItem.SellPrice, useGoldModifier: false);
					break;
				case EGoldMode.CharacterGold:
					if (cMapCharacter.CharacterGold < cItem.SellPrice)
					{
						return;
					}
					cMapCharacter.ModifyGold(-cItem.SellPrice, useGoldModifier: false);
					break;
				case EGoldMode.None:
					Log.NetworkError("ERROR_MULTIPLAYER_00004", "Error modifying item inventory. Gold mode not determined.");
					return;
				default:
					Log.NetworkError("ERROR_MULTIPLAYER_00005", "Error modifying item inventory. Unrecognized gold mode found.");
					return;
				}
				if (cMapCharacter.CheckEquippedItems.Contains(cItem))
				{
					cMapCharacter.UnequipItems(new List<CItem> { cItem });
				}
				cMapCharacter.UnbindItem(cItem);
				FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
			}
			else
			{
				Log.NetworkWarning("Error unbinding an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
			}
		}
		else
		{
			Log.NetworkWarning("Error unbinding an item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
		}
	}

	private void FinishInventoryModification(CMapCharacter character)
	{
		if (itemInventoryDisplay.IsVisible && selectedCharacter != null && selectedCharacter.Data == character)
		{
			itemInventoryDisplay.Refresh(character);
		}
		foreach (CMapCharacter checkCharacter in SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.CheckCharacters)
		{
			if (!AdventureState.MapState.IsCampaign || checkCharacter.PersonalQuest != null)
			{
				int controllableID = ControllableRegistry.GetControllableID(checkCharacter);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemInventoryToken(checkCharacter, mapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode));
			}
		}
		SaveData.Instance.ManualSaveCurrentAdventureData();
	}

	public void ProxyModifyPerks(GameAction action)
	{
		ProxyUpdatePerkPoints(action.ActorID, ((PerkPointsToken)action.SupplementaryDataToken).PerkPoints);
		ProxyModifyPerks(action.ActorID, (PerksToken)action.SupplementaryDataToken2);
		SaveData.Instance.ManualSaveCurrentAdventureData();
	}

	private void ProxyModifyPerks(int controllableID, PerksToken perksToken)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			Log.NetworkInfo("Modifying perks for " + cMapCharacter.CharacterID + " ( has perks: " + (cMapCharacter.Perks != null) + ")");
			List<CharacterPerk> list = cMapCharacter.Perks.FindAll((CharacterPerk x) => x.IsActive);
			Log.NetworkInfo("Already enabled perks " + list.Count);
			int[] activePerkIndexes = perksToken.ActivePerkIndexes;
			foreach (int num2 in activePerkIndexes)
			{
				if (cMapCharacter.Perks[num2] == null)
				{
					Log.GUIErrorFormat("Character perk index {0} is null", num2);
				}
				cMapCharacter.Perks[num2].IsActive = true;
				if (!list.Contains(cMapCharacter.Perks[num2]))
				{
					Log.SimpleInfo("MapCharacter: " + cMapCharacter.CharacterID + " enabled perk: " + cMapCharacter.Perks[num2].PerkID);
				}
			}
			if (perkManager.IsVisible)
			{
				if (selectedCharacter == null)
				{
					Log.GUIErrorFormat("Selected character is null");
					perkManager.Refresh(disableStatsRequests.Count == 0);
				}
				else if (selectedCharacter.Data == cMapCharacter)
				{
					perkManager.PerkDisplay.Display(selectedCharacter.Service, selectedCharacter.PerksPointReference, disableStatsRequests.Count == 0);
					perkManager.ModifierDisplay.Display(selectedCharacter.Service);
				}
			}
			return;
		}
		throw new Exception("Error modifying perks for proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	private void ProxyUpdatePerkPoints(int controllableID, int perkPoints)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = ControllableRegistry.GetMapCharacterByControllerID(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			Log.NetworkInfo("Updating perk points for " + cMapCharacter.CharacterID);
			cMapCharacter.UpdatePerkPoints(perkPoints);
			if (perkManager.IsVisible && selectedCharacter != null && selectedCharacter.Data == cMapCharacter)
			{
				if (perkManager.PerkDisplay == null)
				{
					Log.GUIError("PerkDisplay missing");
				}
				perkManager.PerkDisplay.Display(selectedCharacter.Service, selectedCharacter.PerksPointReference, disableStatsRequests.Count == 0);
			}
			return;
		}
		throw new Exception("Error updating perk points proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}
}

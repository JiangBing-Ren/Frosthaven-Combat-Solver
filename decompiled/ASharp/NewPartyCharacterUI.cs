using System;
using System.Collections.Generic;
using System.Linq;
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

public class NewPartyCharacterUI : APartyCharacterUI, IControllable, IGoldEventListener
{
	[Serializable]
	public class PartyCharacterEvent : UnityEvent<NewPartyCharacterUI>
	{
	}

	[SerializeField]
	private Image characterPortrait;

	[SerializeField]
	private Image characterPortraitHighlight;

	[SerializeField]
	private Image characterUnselectedImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private CanvasGroup slotInteraction;

	[SerializeField]
	private Color selectedColor = new Color(1f, 1f, 1f, 0.9f);

	[SerializeField]
	private Color highlightedColor = new Color(1f, 1f, 1f, 0.2f);

	[SerializeField]
	private Color selectedHighlightedColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private CanvasGroup buttonsCanvasGroup;

	[SerializeField]
	private CanvasGroup characterSlot;

	[SerializeField]
	private MultiplayerUserState multiplayerInfo;

	[SerializeField]
	private Button multiplayerAssignRoleButton;

	public int SlotIndex;

	[SerializeField]
	private List<GameObject> characterInformation;

	[Header("Gold")]
	[SerializeField]
	private GoldCounter individualGoldCounter;

	[SerializeField]
	private GUIAnimator warningGoldAnimator;

	[SerializeField]
	private GameObject individualGoldContainer;

	[Header("Free slot")]
	[SerializeField]
	private Sprite emptySlotSelectedSprite;

	[SerializeField]
	private CanvasGroup emptySlot;

	[SerializeField]
	private ExtendedToggle assignToggle;

	[SerializeField]
	private GameObject assignWarning;

	[SerializeField]
	private GUIAnimator assignWarningHighlight;

	private Image assignIcon;

	[SerializeField]
	private GameObject newAssignNotification;

	[Header("Level")]
	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private GameObject xpPanel;

	[SerializeField]
	private ImageProgressBar xpBar;

	[Header("LevelUp")]
	[SerializeField]
	private ExtendedToggle levelUpToggle;

	[SerializeField]
	private GameObject levelUpRoot;

	[Header("Class")]
	[SerializeField]
	private ExtendedToggle classToggle;

	[SerializeField]
	private Image classIcon;

	[SerializeField]
	private GameObject classWarning;

	[SerializeField]
	private GUIAnimator classWarningHighlight;

	[SerializeField]
	private GameObject newClassNotification;

	[Header("Cards")]
	[SerializeField]
	private ExtendedToggle cardsToggle;

	[SerializeField]
	private GameObject cardsWarning;

	[SerializeField]
	private GUIAnimator cardsWarningHighlight;

	private Image cardsIcon;

	[Header("Perks")]
	[SerializeField]
	private ExtendedToggle perksToggle;

	[SerializeField]
	private GameObject perkPointsHighlight;

	[SerializeField]
	private GameObject perkPointsNotification;

	[SerializeField]
	private GUIAnimator perksWarningHighlight;

	private Image perkIcon;

	[Header("Item modifiers")]
	[SerializeField]
	private ExtendedToggle itemsToggle;

	[SerializeField]
	private GUIAnimator itemModifiersHighlight;

	[SerializeField]
	private GameObject itemModifiersNotification;

	[SerializeField]
	private UINewNotificationTip newItemsNotification;

	private Image itemsIcon;

	[Header("Battle goal")]
	[SerializeField]
	private ExtendedToggle battleGoalToggle;

	[SerializeField]
	private GameObject battleGoalWarning;

	[SerializeField]
	private GUIAnimator battleGoalWarningHighlight;

	private Image battleGoalIcon;

	[SerializeField]
	private UITextTooltipTarget battleGoalTooltip;

	[Header("Mode custom stats")]
	[SerializeField]
	private UICampaignPartyCharacterStats campaignStats;

	[SerializeField]
	private UIGuildmasterPartyCharacterStats guildmasterStats;

	private UICustomPartyCharacterStats customStats;

	private PartySlotState state;

	private Action<NewPartyCharacterUI, bool> onItemsClickAction;

	private Action<NewPartyCharacterUI, bool> onCardsClickAction;

	private Action<NewPartyCharacterUI, bool> onPerksClickAction;

	private Action<bool, NewPartyCharacterUI, bool> onCharacterSelect;

	private Action<NewPartyCharacterUI, bool, RectTransform> onClickAssignAction;

	private Action<NewPartyCharacterUI> onClickLevelupAction;

	private Action<NewPartyCharacterUI, bool> onBattleGoalClickAction;

	private CMapCharacter characterData;

	private CMapCharacterService service;

	private bool initialised;

	public PartyCharacterEvent OnClickedAssignRole;

	private bool isSelected;

	private bool isHighlighted;

	private bool isDimmed;

	private bool isOtherHovered;

	private bool autoOpenDefaultPanel = true;

	private bool disabledCharacterSelection;

	public RectTransform ClassPointReference => classToggle.transform as RectTransform;

	public RectTransform CardPointReference => cardsToggle.transform as RectTransform;

	public bool CardWindowSelected => cardsToggle.isOn;

	public RectTransform PerksPointReference => perksToggle.transform as RectTransform;

	public RectTransform ItemPointReference => itemsToggle.transform as RectTransform;

	public PartySlotState State => state;

	public CMapCharacter Data => characterData;

	public override ICharacterService Service => service;

	public UnityEvent OnHovered => button.onMouseEnter;

	public UnityEvent OnUnhovered => button.onMouseExit;

	public bool IsInteractable
	{
		get
		{
			if (button.IsInteractable())
			{
				return AreButtonsInteractable;
			}
			return false;
		}
	}

	private bool AreButtonsInteractable
	{
		get
		{
			if (slotInteraction.interactable)
			{
				return buttonsCanvasGroup.interactable;
			}
			return false;
		}
	}

	public bool IsParticipant => true;

	public bool IsAlive => true;

	public NetworkPlayer Player { get; private set; }

	public bool IsFocused { get; private set; }

	private void Awake()
	{
		levelUpToggle.onValueChanged.AddListener(OnClickGainExpToLevel);
		perksToggle.onValueChanged.AddListener(OnClickPerks);
		itemsToggle.onValueChanged.AddListener(OnClickItem);
		cardsToggle.onValueChanged.AddListener(OnClickCards);
		assignToggle.onValueChanged.AddListener(OnClickAssign);
		classToggle.onValueChanged.AddListener(OnClickAssign);
		battleGoalToggle.onValueChanged.AddListener(OnClickBattleGoal);
		button.onClick.AddListener(() =>
		{
			if (!Singleton<InputFHManager>.Instance.UsingGamepad || !isSelected)
			{
				OnClick();
			}
		});
		CacheComponents();
		multiplayerAssignRoleButton.onClick.AddListener(OnClickAssignRole);
		if (AdventureState.MapState.IsCampaign)
		{
			customStats = campaignStats;
			guildmasterStats.Hide();
		}
		else
		{
			customStats = guildmasterStats;
			guildmasterStats.OnClickLevelUp.AddListener(OnClickLevelup);
			campaignStats.Hide();
		}
		customStats.Show();
	}

	private void CacheComponents()
	{
		if (!perkIcon)
		{
			perkIcon = perksToggle.GetComponent<Image>();
			itemsIcon = itemsToggle.GetComponent<Image>();
			assignIcon = assignToggle.GetComponent<Image>();
			cardsIcon = cardsToggle.GetComponent<Image>();
			battleGoalIcon = battleGoalToggle.GetComponent<Image>();
		}
	}

	public void Init(CMapParty party, int slot, CMapCharacter characterData, Action<NewPartyCharacterUI, bool> onCardsClickAction, Action<NewPartyCharacterUI, bool> onItemsClickAction, Action<bool, NewPartyCharacterUI, bool> onCharacterSelect, Action<NewPartyCharacterUI, bool> onPerksClickAction, ToggleGroup toggleGroup, int newItemsNotification, Action<NewPartyCharacterUI, bool, RectTransform> onClickSwitchCharacter = null, Action<NewPartyCharacterUI> onClickLevelupAction = null, NetworkPlayer initialController = null, Action<NewPartyCharacterUI, bool> onBattleGoalClickAction = null, bool updateController = false)
	{
		Log.GUIInfo("Initializing PartyCharacterUI");
		CacheComponents();
		ClearEvents();
		if (party != null)
		{
			party.SelectedCharactersArray[slot] = characterData;
		}
		bool forceUpdate = updateController && this.characterData != characterData;
		if (this.characterData != characterData)
		{
			CloseCharacterWindows();
		}
		this.characterData = characterData;
		service = new CMapCharacterService(characterData);
		this.onCharacterSelect = onCharacterSelect;
		this.onCardsClickAction = onCardsClickAction;
		this.onItemsClickAction = onItemsClickAction;
		this.onPerksClickAction = onPerksClickAction;
		onClickAssignAction = onClickSwitchCharacter;
		this.onClickLevelupAction = onClickLevelupAction;
		this.onBattleGoalClickAction = onBattleGoalClickAction;
		CharacterYMLData characterYMLData = characterData.CharacterYMLData;
		nameText.text = (characterData.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterYMLData.LocKey) : characterData.CharacterName);
		characterPortrait.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterYMLData.Model, highlighted: false, characterData.CharacterYMLData.CustomCharacterConfig);
		characterPortraitHighlight.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterYMLData.Model, highlighted: true, characterData.CharacterYMLData.CustomCharacterConfig);
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			individualGoldContainer.SetActive(value: true);
			individualGoldCounter.SetCount(characterData.CharacterGold);
			service.AddCallbackOnGoldChanged(this);
		}
		else
		{
			individualGoldContainer.SetActive(value: false);
		}
		service.AddCallbackOnPerkPointsChanged(UpdatePerkPoints);
		customStats?.Setup(service);
		cardsToggle.group = toggleGroup;
		perksToggle.group = toggleGroup;
		itemsToggle.group = toggleGroup;
		cardsToggle.group = toggleGroup;
		battleGoalToggle.group = toggleGroup;
		assignToggle.group = toggleGroup;
		classToggle.group = toggleGroup;
		initialised = true;
		UpdateXPTexts();
		UpdatePerkPoints();
		RefreshEquippedItemModifiers();
		CreateControllable(initialController, updateController, updateController, forceUpdate);
		ShowCanvasGroup(emptySlot, show: false);
		ShowCanvasGroup(characterSlot, show: true);
		state = PartySlotState.Assigned;
		EnableSelection();
		ShowNewItemsNotification(newItemsNotification > 0);
		ClearState();
		RefreshMultiplayerState();
		ShowNewClassNotification(party != null && party.NewUnlockedCharacterIDs.Count > 0);
	}

	public void CreateControllable(NetworkPlayer initialController, bool overrideBenched, bool updateIfExists = false, bool forceUpdate = false)
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			int controllableID = ControllableRegistry.GetControllableID(characterData);
			NetworkControllable controllable = ControllableRegistry.GetControllable(controllableID);
			if (((controllable == null) | forceUpdate) || (updateIfExists && controllable.Controller != initialController) || (controllable.ControllableObject != null && controllable.ControllableObject is CharacterManager))
			{
				ControllableRegistry.CreateControllable(controllableID, this, initialController.AssignedFreeSlots.First(), changeObjectIfControllableExists: true, releaseOldControllableObject: false);
			}
		}
		OnControlAssigned(initialController);
	}

	private void ClearState()
	{
		HideAssignWarning();
		HideCardsWarning();
		HideClassWarning();
		HideBattleGoalWarning();
		ShowPerksWarning(show: false);
		DisableCardButton(disable: false);
		ShowWarningGold(show: false);
		ShowNewClassNotification(show: false);
	}

	private void ShowCanvasGroup(CanvasGroup canvasGroup, bool show)
	{
		canvasGroup.alpha = (show ? 1 : 0);
		bool interactable = (canvasGroup.blocksRaycasts = show);
		canvasGroup.interactable = interactable;
	}

	public void InitEmpty(CMapParty party, int slot)
	{
		CacheComponents();
		ClearEvents();
		if (party != null)
		{
			party.SelectedCharactersArray[slot] = null;
		}
		ShowCanvasGroup(characterSlot, show: false);
		ShowCanvasGroup(emptySlot, show: false);
		state = PartySlotState.Empty;
		characterData = null;
		service = null;
		DisableSelection();
		customStats?.Clear();
		initialised = true;
		HideNewItemsNotification();
		ClearState();
	}

	public void InitAvailable(CMapParty party, int slot, ToggleGroup toggleGroup, Action<bool, NewPartyCharacterUI, bool> onCharacterSelect, Action<NewPartyCharacterUI, bool, RectTransform> onClickSwitchCharacter = null)
	{
		CacheComponents();
		if (characterData != null)
		{
			int controllableID = ControllableRegistry.GetControllableID(characterData);
			ControllableRegistry.AllControllables.SingleOrDefault((NetworkControllable e) => e.ID == controllableID);
		}
		ClearEvents();
		if (party != null)
		{
			party.SelectedCharactersArray[slot] = null;
		}
		assignToggle.group = toggleGroup;
		cardsToggle.group = toggleGroup;
		perksToggle.group = toggleGroup;
		itemsToggle.group = toggleGroup;
		battleGoalToggle.group = toggleGroup;
		cardsToggle.group = toggleGroup;
		classToggle.group = toggleGroup;
		DetermineInteractability();
		ShowCanvasGroup(characterSlot, show: false);
		ShowCanvasGroup(emptySlot, show: true);
		characterData = null;
		service = null;
		characterPortraitHighlight.sprite = emptySlotSelectedSprite;
		this.onCharacterSelect = onCharacterSelect;
		onClickAssignAction = onClickSwitchCharacter;
		customStats?.Clear();
		state = PartySlotState.Available;
		EnableSelection();
		initialised = true;
		HideNewItemsNotification();
		ClearState();
		RefreshMultiplayerState();
		ShowNewClassNotification(party != null && party.NewUnlockedCharacterIDs.Count > 0);
	}

	private void OnDestroy()
	{
		ClearEvents();
		initialised = false;
	}

	private void ClearEvents()
	{
		if (service != null)
		{
			service.RemoveCallbackOnPerkPointsChanged(UpdatePerkPoints);
			service.RemoveCallbackOnGoldChanged(this);
		}
	}

	public void OnUpdatedGold(int gold)
	{
		if (individualGoldContainer.activeSelf)
		{
			if (individualGoldContainer.activeInHierarchy)
			{
				individualGoldCounter.CountTo(gold);
			}
			else
			{
				individualGoldCounter.SetCount(gold);
			}
		}
	}

	public void OnXpChanged()
	{
		UpdateXPTexts(playProgressAnimation: true);
	}

	public void UpdateXPTexts(bool playProgressAnimation = false)
	{
		if (!initialised)
		{
			return;
		}
		levelText.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), Data.Level);
		if (Data.Level + 1 >= Data.XPTable.Count)
		{
			xpPanel.SetActive(value: false);
			return;
		}
		int num = Data.EXP - Data.GetXPThreshold(Data.Level);
		int num2 = Data.GetXPThreshold(Data.Level + 1) - Data.GetXPThreshold(Data.Level);
		xpPanel.SetActive(value: true);
		if (playProgressAnimation)
		{
			xpBar.PlayProgressTo(num, num2);
		}
		else
		{
			xpBar.SetAmount(num, num2);
		}
	}

	public void UpdatePerkPoints()
	{
		if (initialised)
		{
			perkPointsHighlight.SetActive(characterData.PerkPoints > 0);
			perkPointsNotification.SetActive(characterData.PerkPoints > 0);
		}
	}

	public void UpdateDisplay()
	{
	}

	public void OnClickItem(bool isSelected)
	{
		if (isSelected)
		{
			onCharacterSelect(arg1: true, this, arg3: true);
		}
		onItemsClickAction(this, isSelected);
	}

	private void OnClickAssign(bool isSelected)
	{
		if (buttonsCanvasGroup.interactable)
		{
			if (isSelected)
			{
				onCharacterSelect(arg1: true, this, arg3: false);
			}
			onClickAssignAction(this, isSelected, assignToggle.transform as RectTransform);
		}
	}

	private void OnClickBattleGoal(bool isSelected)
	{
		if (buttonsCanvasGroup.interactable)
		{
			if (isSelected)
			{
				onCharacterSelect(arg1: true, this, arg3: false);
			}
			onBattleGoalClickAction(this, isSelected);
		}
	}

	private void OnClickAssignRole()
	{
		if (buttonsCanvasGroup.interactable || !multiplayerAssignRoleButton.interactable)
		{
			cardsToggle.group.SetAllTogglesOff();
			onCharacterSelect(arg1: true, this, arg3: false);
			OnClickedAssignRole.Invoke(this);
		}
	}

	public void ShowClassToggled()
	{
		assignToggle.SetValue(value: false);
		classToggle.SetValue(value: true);
	}

	public void ShowAssignToggled()
	{
		classToggle.SetValue(value: false);
		assignToggle.SetValue(value: true);
	}

	public void CloseAssigWindow()
	{
		classToggle.isOn = false;
		assignToggle.isOn = false;
	}

	public void OnClickCards(bool isSelected)
	{
		if (buttonsCanvasGroup.interactable)
		{
			if (isSelected)
			{
				onCharacterSelect(arg1: true, this, arg3: true);
			}
			onCardsClickAction(this, isSelected);
		}
	}

	private void OnClickGainExpToLevel(bool isSelected)
	{
		if (isSelected)
		{
			onCharacterSelect(arg1: true, this, arg3: false);
		}
		CustomPartySetup.LevelUp(characterData);
		LevelUp();
		levelUpRoot.gameObject.SetActive(characterData.Level < 9);
	}

	public void OnClickPerks(bool isSelected)
	{
		if (isSelected)
		{
			onCharacterSelect(arg1: true, this, arg3: false);
		}
		onPerksClickAction(this, isSelected);
	}

	private void OnClickLevelup()
	{
		cardsToggle.group.SetAllTogglesOff();
		onCharacterSelect(arg1: true, this, arg3: false);
		onClickLevelupAction?.Invoke(this);
	}

	public void ToggleDim(bool active)
	{
		isDimmed = active;
		UpdateSprite();
	}

	public void ToggleHoverDim(bool active)
	{
		isOtherHovered = active;
		UpdateSprite();
	}

	public void OnClick()
	{
		if (!button.interactable || FFSNetwork.IsOnline)
		{
			return;
		}
		bool flag = !isSelected;
		if (characterData != null)
		{
			ToggleSelect(flag);
			onCharacterSelect(isSelected, this, arg3: true);
			if (!isSelected)
			{
				return;
			}
			if (autoOpenDefaultPanel && AreButtonsInteractable && !Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				if (classToggle.gameObject.activeSelf && classToggle.interactable)
				{
					classToggle.isOn = true;
				}
				else if (battleGoalToggle.gameObject.activeSelf && battleGoalToggle.IsInteractable())
				{
					battleGoalToggle.isOn = true;
				}
				else
				{
					classToggle.group.SetAllTogglesOff();
				}
			}
			else
			{
				classToggle.group.SetAllTogglesOff();
			}
		}
		else if (flag)
		{
			if (assignToggle.gameObject.activeSelf)
			{
				if (assignToggle.interactable && AreButtonsInteractable)
				{
					ToggleSelect(active: true);
					onCharacterSelect(isSelected, this, arg3: true);
					assignToggle.isOn = true;
				}
				else
				{
					assignToggle.group.SetAllTogglesOff();
				}
			}
			else
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			}
		}
		else
		{
			ToggleSelect(active: false);
			onCharacterSelect(isSelected, this, arg3: true);
		}
	}

	public void EnableAutoOpenDefaultPanel(bool isEnabled)
	{
		autoOpenDefaultPanel = isEnabled;
	}

	public void ToggleSelect(bool active)
	{
		isSelected = active;
		if (isSelected)
		{
			ToggleDim(active: false);
		}
		UpdateSprite();
	}

	public void OnHighlight(bool active)
	{
		isHighlighted = active;
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		characterUnselectedImage.enabled = !isHighlighted && (isDimmed || (isSelected && isOtherHovered));
		characterUnselectedImage.color = (isSelected ? highlightedColor : UIInfoTools.Instance.White);
		Image image = characterPortraitHighlight;
		Color color;
		if (isHighlighted)
		{
			color = (isSelected ? selectedHighlightedColor : highlightedColor);
		}
		else
		{
			color = (isSelected ? selectedColor : new Color(1f, 1f, 1f, 0f));
		}
		image.color = color;
	}

	public void LevelUp(Action onLevelUp = null)
	{
		if (service.CanLevelup())
		{
			bool num = !FFSNetwork.IsOnline || Data.IsMapCharacterUnderMyControl;
			service.LevelUp();
			Singleton<APartyDisplayUI>.Instance?.UpdatePartyLevel();
			UpdateXPTexts();
			if (num)
			{
				onLevelUp?.Invoke();
			}
			customStats.RefreshLevelUp();
			if (FFSNetwork.IsOnline && Data.IsMapCharacterUnderMyControl)
			{
				int controllableID = ControllableRegistry.GetControllableID(Data);
				Synchronizer.ReplicateControllableStateChange(GameActionType.LevelUp, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardInventoryToken(Data.HandAbilityCardIDs, Data.OwnedAbilityCardIDs), new LevelToken(Data.Level), new PerkPointsToken(Data.PerkPoints));
			}
			Log.SimpleInfo("MapCharacter: " + characterData.CharacterID + " levelled up");
		}
		else
		{
			customStats.RefreshLevelUp();
		}
	}

	public void ShowNewItemsNotification(bool show)
	{
		if (show)
		{
			newItemsNotification.Show();
		}
		else
		{
			HideNewItemsNotification();
		}
	}

	public void HideNewItemsNotification()
	{
		newItemsNotification.Hide();
	}

	public void ShowNewClassNotification(bool show)
	{
		newClassNotification.SetActive(show);
		newAssignNotification.SetActive(show);
	}

	public void CloseWindows()
	{
		Log.GUIInfo("CloseWindows");
		cardsToggle.isOn = false;
		perksToggle.isOn = false;
		assignToggle.isOn = false;
		classToggle.isOn = false;
		itemsToggle.isOn = false;
		battleGoalToggle.isOn = false;
	}

	private void CloseCharacterWindows()
	{
		cardsToggle.isOn = false;
		perksToggle.isOn = false;
		itemsToggle.isOn = false;
		battleGoalToggle.isOn = false;
	}

	public void EnableSelection()
	{
		button.interactable = true;
	}

	public void DisableSelection()
	{
		button.interactable = false;
	}

	public void EnableAssignPlayer(bool enable)
	{
		multiplayerAssignRoleButton.interactable = enable;
	}

	public void EnableSelectCharacter(bool enable)
	{
		disabledCharacterSelection = !enable;
		DetermineInteractability();
	}

	public void DisableButtons(bool ignoreCardButton)
	{
		buttonsCanvasGroup.interactable = false;
		buttonsCanvasGroup.blocksRaycasts = false;
		RefreshButtonsState();
		if (ignoreCardButton)
		{
			cardsIcon.material = (cardsToggle.interactable ? null : UIInfoTools.Instance.greyedOutMaterial);
		}
	}

	private void DisableCardButton(bool disable)
	{
		cardsToggle.interactable = !disable;
		cardsIcon.material = ((cardsToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
	}

	public void EnableButtons(bool ignoreCardButton)
	{
		buttonsCanvasGroup.interactable = true;
		buttonsCanvasGroup.blocksRaycasts = true;
		if (!ignoreCardButton)
		{
			DisableCardButton(disable: false);
		}
		RefreshButtonsState();
	}

	private void RefreshStateClassToggle()
	{
		classIcon.material = ((classToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		assignIcon.material = ((assignToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
	}

	private void RefreshButtonsState()
	{
		RefreshStateClassToggle();
		itemsIcon.material = ((itemsToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		perkIcon.material = ((perksToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		cardsIcon.material = ((cardsToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		battleGoalIcon.material = ((battleGoalToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
	}

	public void AddEquippedItemModifiers(CItem item)
	{
		if (item.YMLData.Data.AdditionalModifiers.Count != 0)
		{
			itemModifiersHighlight.Stop();
			itemModifiersHighlight.Play();
			RefreshEquippedItemModifiers();
		}
	}

	public void RefreshEquippedItemModifiers()
	{
		int num = characterData.NewEquippedItemsWithModifiers?.Count ?? 0;
		itemModifiersNotification.SetActive(num > 0);
	}

	public void ToggleCardsPanel(bool toggledOn)
	{
		cardsToggle.isOn = toggledOn;
	}

	public void ToggleBatteGoal(bool toggledOn)
	{
		if (battleGoalToggle.interactable || !toggledOn)
		{
			battleGoalToggle.isOn = toggledOn;
		}
	}

	public void ShowAssignWarning(bool glow)
	{
		if (glow)
		{
			assignWarningHighlight.Play();
		}
		else
		{
			assignWarning.SetActive(value: true);
		}
	}

	public void HideAssignWarning()
	{
		assignWarning.SetActive(value: false);
		assignWarningHighlight.Stop();
	}

	public void ShowClassWarning(bool glow)
	{
		if (glow)
		{
			classWarningHighlight.Play();
		}
		else
		{
			classWarning.SetActive(value: true);
		}
	}

	public void HideClassWarning()
	{
		classWarning.SetActive(value: false);
		classWarningHighlight.Stop();
	}

	public void ShowCardsWarning(bool glow)
	{
		cardsWarning.SetActive(value: true);
		if (glow)
		{
			cardsWarningHighlight.Play();
		}
	}

	public void HideCardsWarning()
	{
		cardsWarning.SetActive(value: false);
		cardsWarningHighlight.Stop();
	}

	public void ShowPerksWarning(bool show)
	{
		if (show)
		{
			perksWarningHighlight.Play();
		}
		else
		{
			perksWarningHighlight.Stop(goToEnd: true);
		}
	}

	private void DetermineInteractability(bool updateOpenWindows = true)
	{
		NetworkControllable networkControllable = null;
		if (characterData != null)
		{
			networkControllable = ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID(characterData));
		}
		slotInteraction.interactable = !FFSNetwork.IsOnline || characterData == null || networkControllable != null;
		if (assignToggle != null && classToggle != null)
		{
			assignToggle.interactable = !disabledCharacterSelection && (!FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer == PlayerRegistry.GetPlayerAssignedToSlot(new PartySlotID(SlotIndex))));
			classToggle.interactable = !disabledCharacterSelection && (!FFSNetwork.IsOnline || characterData == null || characterData.IsMapCharacterUnderMyControl);
			if (!UIAdventurePartyAssemblyWindow.IsChangingCharacter && PlayerRegistry.MyPlayer == null)
			{
				if ((assignToggle.isOn & updateOpenWindows) && (!assignToggle.interactable || !slotInteraction.interactable))
				{
					CloseAssigWindow();
				}
				if ((classToggle.isOn & updateOpenWindows) && (!classToggle.interactable || !slotInteraction.interactable))
				{
					CloseAssigWindow();
				}
			}
		}
		battleGoalToggle.interactable = !FFSNetwork.IsOnline || characterData == null || characterData.IsMapCharacterUnderMyControl;
		levelUpRoot.gameObject.SetActive(characterData != null && characterData.Level < 9);
		multiplayerAssignRoleButton.interactable = FFSNetwork.IsHost;
		RefreshButtonsState();
	}

	public void RefreshMultiplayerState(bool updateOpenWindows = true)
	{
		DetermineInteractability(updateOpenWindows);
		if (State == PartySlotState.Empty)
		{
			return;
		}
		if (FFSNetwork.IsOnline)
		{
			NetworkPlayer networkPlayer = PlayerRegistry.GetPlayerAssignedToSlot(new PartySlotID(SlotIndex));
			if (networkPlayer == null)
			{
				networkPlayer = PlayerRegistry.HostPlayer;
			}
			RefreshMultiplayerOwner(networkPlayer);
		}
		else
		{
			Player = null;
			multiplayerAssignRoleButton.gameObject.SetActive(value: false);
			multiplayerInfo.Hide();
		}
	}

	private void RefreshMultiplayerOwner(NetworkPlayer player)
	{
		if (State != PartySlotState.Empty)
		{
			Player = player;
			multiplayerInfo.Show(player, !PlayerRegistry.IsJoining(player));
			multiplayerAssignRoleButton.gameObject.SetActive(value: true);
		}
	}

	public void OnControlAssigned(NetworkPlayer player)
	{
		if (player == null || PlayerRegistry.MyPlayer == null)
		{
			return;
		}
		bool flag = true;
		Log.GUIInfo("OnControlAssigned " + characterData?.CharacterID + " " + PlayerRegistry.MyPlayer.PlayerID.ToString());
		if (characterData == null || !(PlayerRegistry.MyPlayer.PlayerID == player.PlayerID))
		{
			return;
		}
		if (ControllableRegistry.AllControllables.Where((NetworkControllable w) => w.GetCharacter == characterData.CharacterID).Count() <= 1)
		{
			ControllableRegistry.AllControllables.SingleOrDefault((NetworkControllable s) => s.GetCharacter == characterData.CharacterID);
		}
		if (!characterData.IsMapCharacterUnderMyControl)
		{
			Log.NetworkInfo(characterData.CharacterID + "(MAP) is now under my control.");
			characterData.IsMapCharacterUnderMyControl = true;
		}
		if (flag)
		{
			RefreshMultiplayerOwner(player);
			CloseCharacterWindows();
		}
		DetermineInteractability();
		EnableSelection();
	}

	public void OnControlReleased()
	{
		if (PlayerRegistry.MyPlayer != null)
		{
			if (characterData != null && characterData.IsMapCharacterUnderMyControl)
			{
				Log.GUIInfo("Relese " + characterData?.CharacterID);
				characterData.IsMapCharacterUnderMyControl = false;
				Log.NetworkInfo(characterData.CharacterID + "(MAP) is no longer under my control.");
			}
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
			{
				RefreshMultiplayerState(updateOpenWindows: false);
			}
		}
	}

	public string GetName()
	{
		if (characterData == null)
		{
			return "UNINITIALIZED";
		}
		return characterData.CharacterID;
	}

	public string GetID()
	{
		if (characterData == null)
		{
			return "UNINITIALIZED";
		}
		return characterData.CharacterID;
	}

	public void DisableUIForLevelingUp()
	{
		customStats.DisableUIForLevelingUp();
	}

	public void EnableBattleGoalsSelection(bool enable, bool interactable = true, string tooltip = null)
	{
		battleGoalToggle.gameObject.SetActive(enable);
		classToggle.gameObject.SetActive(!enable);
		battleGoalToggle.interactable = interactable;
		RefreshButtonsState();
		if (tooltip.IsNOTNullOrEmpty())
		{
			battleGoalTooltip.SetText(tooltip);
			battleGoalTooltip.enabled = true;
		}
		else
		{
			battleGoalTooltip.enabled = false;
		}
	}

	public void HideBattleGoalWarning()
	{
		battleGoalWarning.SetActive(value: false);
		battleGoalWarningHighlight.Stop();
	}

	public void ShowBattleGoalWarning(bool glow)
	{
		battleGoalWarning.SetActive(value: true);
		if (glow)
		{
			battleGoalWarningHighlight.Play();
		}
	}

	public void ShowWarningGold(bool show)
	{
		if (show)
		{
			warningGoldAnimator.Play();
		}
		else
		{
			warningGoldAnimator.Stop();
		}
	}

	public void DisableAssign()
	{
		assignToggle.gameObject.SetActive(value: false);
	}

	public void CloseCardsWindow()
	{
		cardsToggle.isOn = false;
	}

	public void CloseItemsWindow()
	{
		itemsToggle.isOn = false;
	}

	public void ClosePerksWindow()
	{
		perksToggle.isOn = false;
	}

	public void CloseBattleGoal()
	{
		battleGoalToggle.isOn = false;
	}

	public void HideAssignedCharacter()
	{
		characterPortraitHighlight.sprite = emptySlotSelectedSprite;
		characterPortrait.SetAlpha(0f);
		for (int i = 0; i < characterInformation.Count; i++)
		{
			characterInformation[i].SetActive(value: false);
		}
		DisableButtons(ignoreCardButton: false);
		DisableSelection();
		EnableBattleGoalsSelection(enable: false);
	}
}

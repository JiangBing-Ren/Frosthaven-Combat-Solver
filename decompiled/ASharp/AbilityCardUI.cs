using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.Common;
using FHRL;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

[StateDump(UseToString = true)]
public class AbilityCardUI : LocalizedListener, IComparable, IPooleable, IGeneralInfoProvider
{
	public int CardID;

	public int CardInstanceID;

	public string CardName;

	public CPlayerActor.EForm CardRequiredForm;

	[Header("Internal references")]
	[SerializeField]
	[StateDump(NoMembers = true)]
	public FullAbilityCard fullAbilityCard;

	[SerializeField]
	[StateDump(NoMembers = true)]
	public MiniAbilityCard miniAbilityCard;

	public CanvasGroup canvasGroup;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	public UIFX_MaterialFX_Control fxControls;

	[SerializeField]
	private AudioButtonProfile interactionAudioProfile;

	[Header("Display settings")]
	[SerializeField]
	private Vector2 fullCardDescriptionPadding;

	[SerializeField]
	private float previewCardHeight;

	[SerializeField]
	private float fullCardSpacing;

	[SerializeField]
	public float defaultHighlightScale = 1.2f;

	[SerializeField]
	public Vector3 defaultHoverMovement = new Vector3(10f, 0f, 0f);

	[SerializeField]
	public UIHoverMoveEffects _hoverMove;

	[SerializeField]
	public UIHoverScaleEffects _hoverScale;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactability;

	[SerializeField]
	private float unselectedCardOppacity = 0.2f;

	[SerializeField]
	private float unselectedDiscardedCardOppacity = 0.5f;

	private Transform fullCardHolder;

	private Func<RectTransform, Vector2> overrideFullPreviewCardPosition;

	private bool alwaysShowFullCard;

	private bool isSelectable;

	private bool isHovered;

	private bool isHighlighted;

	private bool isFaded;

	private bool isInFocus;

	private IndicatorType indicatorType;

	private InitiativeTrack.InitiativeEffects effect;

	private CardPileType cardType = CardPileType.Any;

	private CardPileType unselectedCardType = CardPileType.Any;

	private Action<AbilityCardUI, bool> onSelectedCallback;

	private Action<AbilityCardUI, bool> onDeselectedCallback;

	private Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback;

	private Action<AbilityCardUI> onCancelActiveAbilityCallback;

	private Action onActionCompleteCallback;

	private CardHandMode mode;

	private Action<bool, AbilityCardUI> onHoverAction;

	private Action<AbilityCardUI> onUnableToSelectAction;

	private bool disableFullCard;

	[StateDump(Skip = true)]
	private List<InteractabilityIsolatedUIControl> m_IsolatableCardControls = new List<InteractabilityIsolatedUIControl>();

	private CPlayerActor playerActor;

	[StateDump(Skip = true)]
	private List<EnhancedAreaHex> enhancedAreaHexs;

	private bool isValid = true;

	private Action onClickedInvalidCard;

	private float currentFullCardSpacing;

	private bool isEnabled;

	[StateDump(Skip = true)]
	private List<CEnhancement> _enhancements = new List<CEnhancement>(10);

	private bool isInOnDisable;

	private CAbilityCard abilityCard;

	private bool isInteractable;

	private bool displayDebugMessage;

	private bool previewingFullCard;

	public Selectable Selectable => button;

	public float PreviewCardHeight => previewCardHeight;

	public string ClassID { get; private set; }

	public CharacterID CharacterID { get; private set; }

	public string PrefabPath { get; set; }

	public MiniAbilityCard MiniAbilityCard => miniAbilityCard;

	public bool IsLongRest { get; private set; }

	public bool IsInPreview
	{
		get
		{
			CardHandMode cardHandMode = mode;
			return cardHandMode == CardHandMode.Preview || cardHandMode == CardHandMode.PreviewMiniCard;
		}
	}

	public bool IsGeneratedFromLayout { get; private set; }

	public IReadOnlyList<CEnhancement> Enhancements => _enhancements;

	public bool IsSelected
	{
		get
		{
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
			{
				if (Singleton<CardsHandManager>.Instance?.CurrentHand == null)
				{
					return false;
				}
				if (IsLongRest)
				{
					if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
					{
						return playerActor.IsLongRestSelected;
					}
					return playerActor.IsLongRestActionSelected;
				}
				if (Singleton<CardsHandManager>.Instance.CurrentHand.currentMode == CardHandMode.CardsSelection)
				{
					if (GameState.IsSelectingCardForMode(out var modeAbility))
					{
						return modeAbility.SelectedModeCard == abilityCard;
					}
					if (playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None || GameState.IsPerformingCardOutOfPhase)
					{
						return Singleton<CardsHandManager>.Instance.CurrentHand.IsCardSelected(CardID);
					}
					return playerActor.CharacterClass.RoundAbilityCards.Exists((CAbilityCard e) => e.ID == CardID && e.CardInstanceID == CardInstanceID);
				}
				if (Singleton<CardsHandManager>.Instance.CurrentHand.PlayerActor.Class.ID != playerActor.Class.ID)
				{
					return playerActor.CharacterClass.RoundAbilityCards.Exists((CAbilityCard e) => e.ID == CardID && e.CardInstanceID == CardInstanceID);
				}
				return Singleton<CardsHandManager>.Instance.CurrentHand.IsCardSelected(CardID);
			}
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
			{
				return SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == ClassID)?.HandAbilityCardIDs.Contains(CardID) ?? false;
			}
			if (CustomPartySetup.Instance != null)
			{
				CMapCharacter cMapCharacter = CustomPartySetup.Instance.CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == ClassID);
				if (cMapCharacter != null)
				{
					return cMapCharacter.HandAbilityCardIDs.Contains(CardID);
				}
				Log.GUIError("Unable to find Map Party for AbilityCardUI");
				return false;
			}
			Log.GUIError("Unable to find Map Party for AbilityCardUI");
			return false;
		}
	}

	public CardHandMode Mode => mode;

	public CardPileType CardType => cardType;

	public CAbilityCard AbilityCard => abilityCard;

	public CPlayerActor PlayerActor => playerActor;

	public bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			isInteractable = value;
			fullAbilityCard.SetInteractable(isInteractable);
		}
	}

	public List<EnhancementButton> EnhancementButtons => fullAbilityCard.topActionButton.EnhancementButtons.Concat(fullAbilityCard.bottomActionButton.EnhancementButtons).ToList();

	public List<EnhancedAreaHex> EnhancedAreaHexs
	{
		get
		{
			if (enhancedAreaHexs == null || enhancedAreaHexs.Count == 0)
			{
				enhancedAreaHexs = GetComponentsInChildren<EnhancedAreaHex>(includeInactive: true).ToList();
			}
			return enhancedAreaHexs;
		}
	}

	public bool IsSelectable => isSelectable;

	public bool IsValid => isValid;

	protected override void OnDisable()
	{
		base.OnDisable();
		isInOnDisable = true;
		OnPointerExit();
		isInOnDisable = false;
	}

	public override string ToString()
	{
		return GetType().Name + "(" + abilityCard?.Name + ")";
	}

	public IGeneralInfoData GetInfoData()
	{
		if (Choreographer.s_Choreographer == null)
		{
			return UIGeneralInfoUtils.GetAbilityCardInfo(this);
		}
		if (!IsInPreview && !IsLongRest)
		{
			return UIGeneralInfoUtils.GetAbilityCardInfo(this);
		}
		return null;
	}

	private bool GetIsInteractable()
	{
		if (interactability != null)
		{
			return interactability.cachedInteractable;
		}
		return true;
	}

	public void Toggle(bool enabled)
	{
		isEnabled = enabled;
		RefreshInteractability();
	}

	private void RefreshInteractability()
	{
		if (!(gameObject == null))
		{
			bool flag = isEnabled && (interactability == null || interactability.cachedEnabled);
			if (gameObject.scene.isLoaded && gameObject.activeSelf != flag && !isInOnDisable)
			{
				gameObject.SetActive(flag);
			}
			if (flag)
			{
				RefreshFade();
			}
		}
	}

	private void RefreshFade()
	{
		float fade = 1f;
		if (!IsInPreview && (isFaded || !GetIsInteractable() || !IsSelectable))
		{
			fade = ((cardType == CardPileType.Discarded || cardType == CardPileType.Permalost || cardType == CardPileType.Lost) ? unselectedDiscardedCardOppacity : unselectedCardOppacity);
		}
		button.SetFade(fade);
	}

	public void SetupInteractability(InteractabilityIsolatedUIControl control = null)
	{
		if (control == null)
		{
			control = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (control != null)
		{
			interactability = new InteractabilityIsolatedUIControl.InteractabilityReference
			{
				control = control,
				onInteractabilityChanged = () =>
				{
					RefreshInteractability();
				}
			};
		}
	}

	private void Awake()
	{
		fullAbilityCard.onFullCardSelected.AddListener(OnSelectCardFromAction);
		enhancedAreaHexs = GetComponentsInChildren<EnhancedAreaHex>(includeInactive: true).ToList();
		Toggle(gameObject.activeSelf);
	}

	private void Start()
	{
		OnLanguageChanged();
	}

	private void OnDestroy()
	{
		onSelectedCallback = null;
		onDeselectedCallback = null;
		onActionCallback = null;
		onCancelActiveAbilityCallback = null;
		onActionCompleteCallback = null;
		playerActor = null;
		onHoverAction = null;
		onUnableToSelectAction = null;
		onClickedInvalidCard = null;
		overrideFullPreviewCardPosition = null;
		m_IsolatableCardControls.Clear();
		enhancedAreaHexs?.Clear();
		fullCardHolder = null;
		fxControls = null;
		button.onClick.RemoveAllListeners();
		abilityCard = null;
		fullAbilityCard = null;
		miniAbilityCard = null;
	}

	private void ClearEnhancements()
	{
		_enhancements?.Clear();
		try
		{
			foreach (EnhancedAreaHex enhancedAreaHex in EnhancedAreaHexs)
			{
				enhancedAreaHex.RemoveEnhancement();
			}
			foreach (EnhancementButton enhancementButton in EnhancementButtons)
			{
				enhancementButton.UpdateEnhancement(EEnhancement.NoEnhancement);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred clearing enhacements on " + CardName);
		}
	}

	private void ApplyEnhancements(IEnumerable<CEnhancement> enhancements)
	{
		ClearEnhancements();
		_enhancements.AddRange(enhancements);
		bool activeSelf = gameObject.activeSelf;
		gameObject.SetActive(value: true);
		foreach (CEnhancement enhancement in _enhancements)
		{
			try
			{
				if (enhancement.AbilityCardID != CardID)
				{
					continue;
				}
				foreach (CEnhancement allEnhancement in AbilityCard.GetAllEnhancements())
				{
					if (!(allEnhancement.AbilityName != enhancement.AbilityName) && allEnhancement.EnhancementLine == enhancement.EnhancementLine && allEnhancement.EnhancementSlot == enhancement.EnhancementSlot)
					{
						allEnhancement.Enhancement = enhancement.Enhancement;
						allEnhancement.PaidPrice = enhancement.PaidPrice;
					}
				}
				if (enhancement.EnhancementLine == EEnhancementLine.AreaHex)
				{
					foreach (EnhancedAreaHex enhancedAreaHex in EnhancedAreaHexs)
					{
						if (!string.IsNullOrEmpty(enhancedAreaHex.AbilityName) && enhancedAreaHex.Ability != null)
						{
							if (enhancedAreaHex.Enhancement == null)
							{
								enhancedAreaHex.AbilityName = null;
							}
							else if (enhancedAreaHex.Enhancement.AbilityName == enhancement.AbilityName && enhancedAreaHex.Enhancement.EnhancementLine == enhancement.EnhancementLine && enhancedAreaHex.Enhancement.EnhancementSlot == enhancement.EnhancementSlot)
							{
								enhancedAreaHex.Init(enhancement);
							}
						}
					}
					continue;
				}
				foreach (EnhancementButton enhancementButton in EnhancementButtons)
				{
					if (!string.IsNullOrEmpty(enhancementButton.AbilityName) && enhancementButton.Ability != null)
					{
						if (enhancementButton.Ability.AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == enhancementButton.EnhancementLine && s.EnhancementSlot == enhancementButton.EnhancementSlot) == null)
						{
							enhancementButton.AbilityName = null;
						}
						else if (enhancementButton.Enhancement.AbilityName == enhancement.AbilityName && enhancementButton.Enhancement.EnhancementLine == enhancement.EnhancementLine && enhancementButton.Enhancement.EnhancementSlot == enhancement.EnhancementSlot)
						{
							enhancementButton.Init(enhancement);
						}
					}
				}
			}
			catch (Exception exception)
			{
				Log.Exception(exception, "An exception occurred applying enhancements on " + CardName);
			}
			finally
			{
				gameObject.SetActive(activeSelf);
			}
		}
	}

	public void OnAfterLoad()
	{
		playerActor = GameState.ResolvePlayerAfterLoad(playerActor);
		ApplyEnhancements(UIPlayerCharacterUtils.GetEnhancements(playerActor));
	}

	private void OnSelectCardFromAction()
	{
		if (PhaseManager.IsInCardsSelectionPhase || mode == CardHandMode.LoseCard || mode == CardHandMode.DiscardCard || mode == CardHandMode.SelectCardsWithActiveBonusTracker || mode == CardHandMode.SelectCardsWithTideActiveBonusesToKeep || mode == CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate || (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && IsLongRest))
		{
			OnClick();
		}
	}

	public void OnClick()
	{
		OnClick(ignoreHiglight: false);
	}

	public void OnClick(bool ignoreHiglight)
	{
		if ((isSelectable || isHighlighted || ignoreHiglight) && (!(Singleton<CardsHandManager>.Instance != null) || !Singleton<CardsHandManager>.Instance.ShouldAutoSelectPlayerCards()))
		{
			CAbilityModeTransfer modeAbility;
			if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
			{
				UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
				miniAbilityCard?.ShowWarning(show: true);
			}
			else if (!IsValid)
			{
				AudioControllerUtils.PlaySound(interactionAudioProfile.nonInteractableMouseDownAudioItem);
				miniAbilityCard.ShowWarning(show: true);
				onClickedInvalidCard?.Invoke();
			}
			else if (isSelectable && cardType == CardPileType.Active && mode != CardHandMode.SelectCardsWithActiveBonusTracker && mode != CardHandMode.SelectCardsWithTideActiveBonusesToKeep && mode != CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate && !GameState.IsSelectingCardForMode(out modeAbility) && onCancelActiveAbilityCallback != null)
			{
				AudioControllerUtils.PlaySound(interactionAudioProfile.mouseDownAudioItem);
				onCancelActiveAbilityCallback(this);
			}
			else
			{
				ToggleSelect(!IsSelected, highlight: true);
			}
		}
	}

	public void UpdateElements()
	{
		fullAbilityCard.UpdateElements();
	}

	public void OnPointerEnter()
	{
		isHovered = true;
		if (mode != CardHandMode.ActionSelection && !IsInPreview && !alwaysShowFullCard)
		{
			if (abilityCard != null && DebugMenu.displayCardsId)
			{
				displayDebugMessage = true;
				UIManager.Instance.DisplayDebugMessage(abilityCard.Name + " card ID: " + abilityCard.ID + " card instance ID: " + abilityCard.CardInstanceID);
			}
			ToggleFullCardPreview(isHighlighted: true, fullCardHolder);
			if (IsLongRest)
			{
				Singleton<CardsHandManager>.Instance?.CurrentHand?.ToggleHighlight(new List<CardPileType> { CardPileType.Discarded }, active: true, keepLongRest: true);
			}
			onHoverAction?.Invoke(arg1: true, this);
			if (mode == CardHandMode.CardsSelection && playerActor.CharacterClass.RoundAbilityCards.Count == 0 && isSelectable && !playerActor.CharacterClass.LongRest && playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count == 0)
			{
				PlayEffect(InitiativeTrack.InitiativeEffects.Hover);
			}
			if (GetIsInteractable() && IsSelectable)
			{
				button.SetFade(1f);
			}
		}
	}

	public void OnPointerExit()
	{
		isHovered = false;
		if (ScenarioRuleClient.ScenarioRuleClientStopped)
		{
			return;
		}
		if (mode != CardHandMode.ActionSelection && !IsInPreview && !alwaysShowFullCard)
		{
			ToggleFullCardPreview(isHighlighted: false, fullCardHolder);
		}
		if (displayDebugMessage)
		{
			displayDebugMessage = false;
			UIManager.Instance.HideDebugMessage();
		}
		if (IsLongRest && gameObject.activeInHierarchy && Singleton<CardsHandManager>.Instance?.CurrentHand != null)
		{
			Singleton<CardsHandManager>.Instance.CurrentHand.ToggleHighlight(new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			}, active: false);
		}
		onHoverAction?.Invoke(arg1: false, this);
		if (playerActor != null && playerActor.TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (mode == CardHandMode.CardsSelection && playerActor.CharacterClass.RoundAbilityCards.Count == 0 && !playerActor.CharacterClass.LongRest)
			{
				PlayEffect(InitiativeTrack.InitiativeEffects.None);
			}
		}
		else if (playerActor != null && playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count == 0)
		{
			PlayEffect(InitiativeTrack.InitiativeEffects.None);
		}
		if (isSelectable)
		{
			RefreshInteractability();
		}
	}

	public void Init(CAbilityCard abilityCard, CPlayerActor playerActor, CardPileType cardType, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction, Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback, Action onActionCompleteCallback, Action<AbilityCardUI> onCancelActiveAbility = null, bool isLongRest = false, Func<RectTransform, Vector2> overrideFullPreviewCardPosition = null, Transform fullCardHolder = null)
	{
		Toggle(enabled: true);
		ResetDefaultParameters();
		this.playerActor = playerActor;
		this.onActionCallback = onActionCallback;
		this.abilityCard = abilityCard;
		this.onSelectedCallback = onSelectedCallback;
		this.onDeselectedCallback = onDeselectedCallback;
		onUnableToSelectAction = onUnableToSelectCallback;
		this.onHoverAction = onHoverAction;
		this.onActionCompleteCallback = onActionCompleteCallback;
		onCancelActiveAbilityCallback = onCancelActiveAbility;
		this.fullCardHolder = fullCardHolder;
		this.overrideFullPreviewCardPosition = overrideFullPreviewCardPosition;
		IsLongRest = isLongRest;
		if (abilityCard != null && abilityCard.ClassModel != null)
		{
			SetSkin(abilityCard.ClassModel, abilityCard.ClassCharacterConfig);
		}
		else
		{
			SetSkin(playerActor.CharacterClass.AbilityCardArt, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
		SetType(cardType);
		SetDisplayMode(cardType);
		SetUpInteractabilityRelationship();
		SetUpFullCardElements(abilityCard);
		disableFullCard = false;
		ClassID = playerActor.Class.ID;
		if (abilityCard != null)
		{
			CardInstanceID = abilityCard.CardInstanceID;
		}
		ApplyEnhancements(UIPlayerCharacterUtils.GetEnhancements(playerActor, CardID));
	}

	public void Init(CAbilityCard abilityCard, Transform fullCardHolder, CMapCharacter character, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction, float highlightScale, Vector3 highlightDisplacement, float fullCardSpacing, bool disableFullCard = false)
	{
		Init(abilityCard, fullCardHolder, character.CharacterID, onSelectedCallback, onDeselectedCallback, onUnableToSelectCallback, onHoverAction, disableFullCard);
		SetHoverParams(highlightScale, highlightDisplacement);
		currentFullCardSpacing = fullCardSpacing;
		ApplyEnhancements(UIPlayerCharacterUtils.GetEnhancements(character, CardID));
	}

	public void Init(CAbilityCard abilityCard, Transform fullCardHolder, string character, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction, float highlightScale, Vector3 highlightDisplacement, float fullCardSpacing, bool disableFullCard = false)
	{
		Init(abilityCard, fullCardHolder, character, onSelectedCallback, onDeselectedCallback, onUnableToSelectCallback, onHoverAction, disableFullCard);
		SetHoverParams(highlightScale, highlightDisplacement);
		currentFullCardSpacing = fullCardSpacing;
	}

	public void Init(CAbilityCard abilityCard, Transform fullCardHolder, string characterID, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction = null, bool disableFullCard = false, Func<RectTransform, Vector2> overrideFullPreviewCardPosition = null)
	{
		Toggle(enabled: true);
		ResetDefaultParameters();
		this.onHoverAction = onHoverAction;
		this.abilityCard = abilityCard;
		this.onSelectedCallback = onSelectedCallback;
		this.onDeselectedCallback = onDeselectedCallback;
		onUnableToSelectAction = onUnableToSelectCallback;
		this.fullCardHolder = fullCardHolder;
		this.overrideFullPreviewCardPosition = overrideFullPreviewCardPosition;
		ClassID = characterID;
		if (abilityCard != null)
		{
			CardInstanceID = abilityCard.CardInstanceID;
		}
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID.Equals(characterID, StringComparison.InvariantCultureIgnoreCase));
		SetSkin(cCharacterClass.AbilityCardArt, cCharacterClass.CharacterYML.CustomCharacterConfig);
		SetMode(CardHandMode.DeckSelection, new List<CardPileType> { CardPileType.Any });
		SetType((!IsInPreview && IsSelected) ? CardPileType.Round : CardPileType.Hand);
		SetDisplayMode((!IsInPreview && IsSelected) ? CardPileType.Round : CardPileType.Hand);
		SetUpInteractabilityRelationship();
		SetUpFullCardElements(abilityCard);
		this.disableFullCard = disableFullCard;
	}

	public void Init(CAbilityCard abilityCard, CharacterID characterID, bool disableEventDetection = false)
	{
		CharacterID = characterID;
		Init(abilityCard, disableEventDetection, UIPlayerCharacterUtils.GetEnhancements(characterID));
	}

	public void Init(CAbilityCard abilityCard, bool disableEventDetection = false, IEnumerable<CEnhancement> enhancements = null)
	{
		Toggle(enabled: true);
		ResetDefaultParameters();
		this.abilityCard = abilityCard;
		SetSkin(abilityCard.ClassModel, abilityCard.ClassCharacterConfig);
		SetMode(CardHandMode.Preview, null);
		SetUpInteractabilityRelationship();
		disableFullCard = false;
		DisableEventDetection(disableEventDetection);
		if (enhancements != null)
		{
			ApplyEnhancements(enhancements);
		}
		else if (CharacterClassManager.AllAbilityCardsUnmodifiedForUI.Contains(abilityCard))
		{
			foreach (CEnhancement allEnhancement in AbilityCard.GetAllEnhancements())
			{
				allEnhancement.Enhancement = EEnhancement.NoEnhancement;
			}
		}
		fullAbilityCard.Init(null, abilityCard, abilityCard.ID, null, null);
		SetUpFullCardElements(abilityCard);
	}

	private void DisableEventDetection(bool disable)
	{
		button.enabled = !disable;
		if (fullAbilityCard.DefaultMoveButton != null && fullAbilityCard.DefaultAttackButton != null)
		{
			fullAbilityCard.bottomActionButton.actionButton.enabled = !disable;
			fullAbilityCard.topActionButton.actionButton.enabled = !disable;
			fullAbilityCard.topActionButton.enabled = !disable;
			fullAbilityCard.bottomActionButton.enabled = !disable;
		}
	}

	private void ResetDefaultParameters()
	{
		currentFullCardSpacing = fullCardSpacing;
		fullCardHolder = null;
		overrideFullPreviewCardPosition = null;
		SetHoverParams(defaultHighlightScale, defaultHoverMovement);
	}

	private void SetUpFullCardElements(CAbilityCard cAbilityCard)
	{
		fullAbilityCard.InitCardLevel(cAbilityCard?.GetAbilityCardYML);
		fullAbilityCard.UpdateCardSummary(cAbilityCard);
		fullAbilityCard.SetupClassElements(cAbilityCard);
	}

	public void EditorInit(CCharacterClass playerClass)
	{
		ResetDefaultParameters();
		SetSkin(playerClass.AbilityCardArt, playerClass.CharacterYML.CustomCharacterConfig);
		SetType(CardPileType.Active);
		SetDisplayMode(CardPileType.Active);
	}

	public void CreateCardsInit(string classModel)
	{
		ResetDefaultParameters();
		SetSkin(classModel);
		SetType(CardPileType.Active);
		SetDisplayMode(CardPileType.Active);
	}

	private void SetUpInteractabilityRelationship()
	{
		if (m_IsolatableCardControls.Count > 0)
		{
			return;
		}
		InteractabilityIsolatedUIControl interactabilityIsolatedUIControl = gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableCardControls.Add(interactabilityIsolatedUIControl);
		interactabilityIsolatedUIControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.CardSelection;
		interactabilityIsolatedUIControl.ControlIdentifier = CardName;
		interactabilityIsolatedUIControl.ExtendedButtonsToAllow.Add(button);
		interactabilityIsolatedUIControl.SpecifiedRectTransformToFollow = ((miniAbilityCard.HighlightRect == null) ? miniAbilityCard.GetComponent<RectTransform>() : miniAbilityCard.HighlightRect);
		interactabilityIsolatedUIControl.Initialise();
		InteractabilityIsolatedUIControl fullCardTopHalfSelectControl = fullAbilityCard.topActionButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableCardControls.Add(fullCardTopHalfSelectControl);
		fullCardTopHalfSelectControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.CardTopHalfSelection;
		fullCardTopHalfSelectControl.ControlIdentifier = CardName;
		fullCardTopHalfSelectControl.TrackedButtonsToAllow.Add(fullAbilityCard.topActionButton.actionButton.GetComponent<TrackedButton>());
		fullCardTopHalfSelectControl.FHButtonsToAllow.Add(fullAbilityCard.topActionButton.actionButton.GetComponent<FHButton>());
		fullCardTopHalfSelectControl.Initialise();
		fullCardTopHalfSelectControl.ValidateHighlight = () => fullAbilityCard.topActionButton.ActiveAbilityAction || InteractabilityManager.AllowInteractabilityHighlight(fullCardTopHalfSelectControl);
		TrackedButton trackedButton = fullAbilityCard.bottomActionButton?.actionButton?.GetComponent<TrackedButton>();
		if (trackedButton != null)
		{
			InteractabilityIsolatedUIControl fullCardBottomHalfSelectControl = fullAbilityCard.bottomActionButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(fullCardBottomHalfSelectControl);
			fullCardBottomHalfSelectControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.CardBottomHalfSelection;
			fullCardBottomHalfSelectControl.ControlIdentifier = CardName;
			fullCardBottomHalfSelectControl.TrackedButtonsToAllow.Add(trackedButton);
			fullCardBottomHalfSelectControl.Initialise();
			fullCardBottomHalfSelectControl.ValidateHighlight = () => fullAbilityCard.bottomActionButton.ActiveAbilityAction || InteractabilityManager.AllowInteractabilityHighlight(fullCardBottomHalfSelectControl);
		}
		if (fullAbilityCard.DefaultAttackButton != null)
		{
			InteractabilityIsolatedUIControl fullCardDefaultAttackControl = fullAbilityCard.topActionButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(fullCardDefaultAttackControl);
			fullCardDefaultAttackControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility;
			fullCardDefaultAttackControl.ControlIdentifier = CardName;
			fullCardDefaultAttackControl.TrackedButtonsToAllow.Add(fullAbilityCard.DefaultAttackButton);
			fullCardDefaultAttackControl.Initialise();
			fullCardDefaultAttackControl.ValidateHighlight = () => fullAbilityCard.topActionButton.ActiveDefaultAction || InteractabilityManager.AllowInteractabilityHighlight(fullCardDefaultAttackControl);
		}
		if (fullAbilityCard.DefaultMoveButton != null)
		{
			InteractabilityIsolatedUIControl fullCardDefaultMoveControl = fullAbilityCard.bottomActionButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(fullCardDefaultMoveControl);
			fullCardDefaultMoveControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility;
			fullCardDefaultMoveControl.ControlIdentifier = CardName;
			fullCardDefaultMoveControl.TrackedButtonsToAllow.Add(fullAbilityCard.DefaultMoveButton);
			fullCardDefaultMoveControl.Initialise();
			fullCardDefaultMoveControl.ValidateHighlight = () => fullAbilityCard.bottomActionButton.ActiveDefaultAction || InteractabilityManager.AllowInteractabilityHighlight(fullCardDefaultMoveControl);
		}
		for (int num = 0; num < fullAbilityCard.topActionButton.ConsumeButtons.Length; num++)
		{
			ConsumeButton consumeButton = fullAbilityCard.topActionButton.ConsumeButtons[num];
			consumeButton.SetupForAbilityCard(isTopAction: true, CardID, num);
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl2 = consumeButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(interactabilityIsolatedUIControl2);
			interactabilityIsolatedUIControl2.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardTop;
			interactabilityIsolatedUIControl2.ControlIdentifier = CardName;
			interactabilityIsolatedUIControl2.ControlIndex = num;
			interactabilityIsolatedUIControl2.SpecifiedRectTransformToFollow = consumeButton.transform.parent.GetComponent<RectTransform>();
			interactabilityIsolatedUIControl2.Initialise();
		}
		for (int num2 = 0; num2 < fullAbilityCard.bottomActionButton.ConsumeButtons.Length; num2++)
		{
			ConsumeButton consumeButton2 = fullAbilityCard.bottomActionButton.ConsumeButtons[num2];
			consumeButton2.SetupForAbilityCard(isTopAction: false, CardID, num2);
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl3 = consumeButton2.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(interactabilityIsolatedUIControl3);
			interactabilityIsolatedUIControl3.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardBottom;
			interactabilityIsolatedUIControl3.ControlIdentifier = CardName;
			interactabilityIsolatedUIControl3.ControlIndex = num2;
			interactabilityIsolatedUIControl3.SpecifiedRectTransformToFollow = consumeButton2.transform.parent.GetComponent<RectTransform>();
			interactabilityIsolatedUIControl3.Initialise();
		}
	}

	public void RemoveInteractabilityRelationship()
	{
		foreach (InteractabilityIsolatedUIControl isolatableCardControl in m_IsolatableCardControls)
		{
			UnityEngine.Object.Destroy(isolatableCardControl);
		}
		m_IsolatableCardControls.Clear();
	}

	private void SetSkin(string playerName, string custom = null)
	{
		SetSkin(UIInfoTools.Instance.GetCardSkin(playerName, IsAltFormCard(), custom));
		SetVisualsFromConfig(UIInfoTools.Instance.GetCharacterConfigUI(playerName, useDefault: true, custom));
	}

	public bool IsAltFormCard()
	{
		return CardRequiredForm == CPlayerActor.EForm.Range;
	}

	private void SetSkin(AbilityCardUISkin skin)
	{
		miniAbilityCard.SetSkin(skin);
		if (IsGeneratedFromLayout)
		{
			fullAbilityCard.SetSkin(skin);
		}
	}

	private void SetVisualsFromConfig(CharacterConfigUI config)
	{
		fullAbilityCard.SetVisualsFromConfig(config);
	}

	public void SetType(CardPileType type)
	{
		unselectedCardType = ((cardType == CardPileType.Round) ? CardPileType.Hand : cardType);
		cardType = type;
		if (!disableFullCard || mode != CardHandMode.DeckSelection)
		{
			fullAbilityCard.SetPile(CrossReference.CardPileTypeToECardPile(type));
		}
	}

	public void HiglightFullCard(bool active)
	{
		fullAbilityCard.Highlight(active);
		UpdateSize(mode);
		fullAbilityCard.SetPile(CrossReference.CardPileTypeToECardPile(cardType));
	}

	public bool LostAnimation(bool active, float cardHighlightTime)
	{
		if (!gameObject.activeInHierarchy)
		{
			return false;
		}
		StartCoroutine(miniAbilityCard.LostAnimation(active, cardHighlightTime));
		return true;
	}

	public void CancelLostAnimation()
	{
		StopAllCoroutines();
		miniAbilityCard.CancelLostAnimation();
	}

	public void UpdateCard()
	{
		if (GameState.CurrentOutOfPhaseCardSelectionAbility != null && playerActor.CharacterClass.RoundAbilityCards.Contains(abilityCard))
		{
			cardType = CardPileType.Hand;
		}
		SetDisplayMode(cardType);
		if (!disableFullCard || mode != CardHandMode.DeckSelection)
		{
			fullAbilityCard.SetPile(CrossReference.CardPileTypeToECardPile(cardType));
		}
	}

	public void Highlight(bool active, List<CardPileType> highlightCardTypes = null, bool showIndicator = false, bool keepLongRest = false, bool keepShortRest = false)
	{
		isHighlighted = active && ((!IsLongRest && highlightCardTypes != null && highlightCardTypes.Contains(cardType)) || ((IsLongRest && isSelectable) & keepLongRest));
		if (IsLongRest)
		{
			button.SetFade((!isSelectable || !keepLongRest || !GetIsInteractable()) ? 0.3f : 1f);
		}
		else if (GetIsInteractable() && isHighlighted)
		{
			button.SetFade(1f);
		}
		bool flag = keepLongRest | keepShortRest;
		bool flag2 = (isHighlighted | showIndicator) && !IsLongRest;
		SetDisplayMode(cardType, !flag2 && isHighlighted);
		if (flag2)
		{
			if (flag || indicatorType == IndicatorType.Burn)
			{
				miniAbilityCard.EnableGlowBurnSelected();
			}
			else
			{
				miniAbilityCard.EnableGlowSelected();
			}
		}
		else
		{
			miniAbilityCard.DisableGlowSelected();
		}
		miniAbilityCard.DisplayIndicator(flag2, flag ? IndicatorType.Burn : indicatorType);
	}

	public bool SwapInitiative()
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
		{
			UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			miniAbilityCard.ShowWarning(show: true);
			Singleton<HelpBox>.Instance.HighlightWarning();
			return false;
		}
		if (!PhaseManager.IsInCardsSelectionPhase || playerActor.CharacterClass.RoundAbilityCards.Count < 2 || !playerActor.CharacterClass.RoundAbilityCards.Contains(abilityCard))
		{
			if (mode == CardHandMode.CardsSelection && playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
			{
				miniAbilityCard.ShowWarning(show: true);
				Singleton<HelpBox>.Instance.HighlightWarning();
			}
			return false;
		}
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(button))
		{
			return false;
		}
		if (!isValid)
		{
			AudioControllerUtils.PlaySound(interactionAudioProfile.nonInteractableMouseDownAudioItem);
			miniAbilityCard.ShowWarning(show: true);
			onClickedInvalidCard?.Invoke();
			return false;
		}
		AudioControllerUtils.PlaySound(interactionAudioProfile.mouseDownAudioItem);
		if (playerActor.MaxRoundCardsCount == 1)
		{
			playerActor.CharacterClass.SetInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[0]);
		}
		else if (playerActor.MaxRoundCardsCount > 1)
		{
			CAbilityCard item = playerActor.CharacterClass.RoundAbilityCards[0];
			playerActor.CharacterClass.RoundAbilityCards.RemoveAt(0);
			playerActor.CharacterClass.RoundAbilityCards.Add(item);
			playerActor.CharacterClass.SetInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[0]);
			playerActor.CharacterClass.SetSubInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[1]);
		}
		InitiativeTrack.Instance.UpdateActors();
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(playerActor);
		hand.OnSelectedCardsNumberChanged();
		hand.NetworkSelectedRoundCards();
		return true;
	}

	public void SetSelectable(bool isOn, bool isHoverable = true)
	{
		isSelectable = isOn;
		button.interactable = isOn;
		button.scaleNonInteractable = isHoverable;
		RefreshInteractability();
	}

	public void SetValid(bool isValid, Action onClickedInvalidCard = null, Action onClickedInvalidFullCard = null)
	{
		this.isValid = isValid;
		this.onClickedInvalidCard = onClickedInvalidCard;
		if (isValid)
		{
			miniAbilityCard.ShowWarning(show: false);
		}
		fullAbilityCard.SetValid(isValid, onClickedInvalidFullCard);
	}

	public void PlayEffect(InitiativeTrack.InitiativeEffects effect)
	{
		if (!(fxControls == null) && (this.effect != effect || effect == InitiativeTrack.InitiativeEffects.None) && fxControls.gameObject.activeInHierarchy)
		{
			this.effect = effect;
			switch (effect)
			{
			case InitiativeTrack.InitiativeEffects.Active:
				fxControls.InitiativeActive();
				break;
			case InitiativeTrack.InitiativeEffects.Hover:
				fxControls.InitiativeHover();
				break;
			case InitiativeTrack.InitiativeEffects.None:
				fxControls.InitiativeNone();
				break;
			case InitiativeTrack.InitiativeEffects.Select:
				fxControls.InitiativeSelect();
				break;
			}
		}
	}

	public void SetMode(CardHandMode mode, List<CardPileType> selectableCardType, bool fadeUnselectableCards = false, bool highlightSelectableCard = false, bool longRestAvailable = false, Func<CAbilityCard, bool> cardSelectableFilter = null)
	{
		if (abilityCard != null && abilityCard.ClassModel != null)
		{
			SetSkin(abilityCard.ClassModel, abilityCard.ClassCharacterConfig);
		}
		else if (playerActor != null)
		{
			SetSkin(playerActor.CharacterClass.AbilityCardArt, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
		bool isOn = selectableCardType != null && !selectableCardType.Contains(CardPileType.None) && (selectableCardType.Contains(CardPileType.Any) || selectableCardType.Contains(cardType)) && (!IsLongRest | longRestAvailable) && (cardSelectableFilter == null || abilityCard == null || cardSelectableFilter(abilityCard)) && (mode != CardHandMode.ActionSelection || IsLongRest);
		SetSelectable(isOn);
		isFaded = ((!isSelectable & fadeUnselectableCards) || (IsLongRest && !isSelectable)) && !IsInPreview;
		isInFocus = (isSelectable & highlightSelectableCard) && !IsLongRest;
		indicatorType = ((mode == CardHandMode.LoseCard) ? IndicatorType.Burn : IndicatorType.General);
		if (isInFocus)
		{
			Highlight(active: true, selectableCardType, showIndicator: true);
		}
		else
		{
			Highlight(active: false);
		}
		RefreshInteractability();
		switch (mode)
		{
		case CardHandMode.CardsSelection:
		case CardHandMode.LoseCard:
		case CardHandMode.DeckSelection:
		case CardHandMode.RecoverLostCard:
		case CardHandMode.RecoverDiscardedCard:
		case CardHandMode.IncreaseCardLimit:
		case CardHandMode.DiscardCard:
		case CardHandMode.SelectCardsWithActiveBonusTracker:
		case CardHandMode.SelectCardsWithTideActiveBonusesToKeep:
		case CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate:
			this.mode = mode;
			this.fullAbilityCard.SetInteractable(active: true);
			ToggleFullCard(active: false);
			break;
		case CardHandMode.ActionSelection:
		{
			if (playerActor.IsTakingExtraTurn)
			{
				if (!playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(abilityCard))
				{
					return;
				}
			}
			else if (GameState.IsPerformingCardOutOfPhase)
			{
				if (!playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Contains(abilityCard))
				{
					return;
				}
			}
			else if (!playerActor.CharacterClass.RoundAbilityCards.Contains(abilityCard) && !playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(abilityCard) && (!playerActor.CharacterClass.LongRest || !IsLongRest))
			{
				if (IsLongRest && playerActor.CharacterClass.HasLongRested && mode == CardHandMode.ActionSelection)
				{
					this.mode = mode;
					ToggleFullCard(active: true);
				}
				return;
			}
			this.mode = mode;
			FullAbilityCard fullAbilityCard = this.fullAbilityCard;
			CPlayerActor cPlayerActor = playerActor;
			CAbilityCard cAbilityCard = abilityCard;
			int id;
			if (GameState.IsPerformingCardOutOfPhase)
			{
				id = playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.IndexOf(abilityCard);
			}
			else
			{
				id = (playerActor.IsTakingExtraTurn ? playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection.IndexOf(abilityCard) : playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.IndexOf(abilityCard));
			}
			fullAbilityCard.Init(cPlayerActor, cAbilityCard, id, onActionCallback, onActionCompleteCallback);
			ToggleFullCard(active: true);
			break;
		}
		case CardHandMode.PreviewMiniCard:
			this.mode = mode;
			ToggleFullCard(active: false);
			RefreshInteractability();
			break;
		case CardHandMode.Preview:
			this.mode = mode;
			ToggleFullCard(active: true);
			RefreshInteractability();
			break;
		}
		this.fullAbilityCard.SetMode(mode);
	}

	public void ToggleFullSelectionCard(bool active)
	{
		if (!IsLongRest)
		{
			alwaysShowFullCard = active;
			fullAbilityCard.EnableSelectCard(active);
			bool flag = mode == CardHandMode.ActionSelection && playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(abilityCard);
			bool flag2 = FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && cardType == CardPileType.Round && !playerActor.IsUnderMyControl() && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			fullAbilityCard.DisplaySelected(((IsSelected | flag) & active) && !flag2);
		}
	}

	public void ToggleFullCard(bool active)
	{
		if (active)
		{
			miniAbilityCard.gameObject.SetActive(value: false);
			fullAbilityCard.gameObject.SetActive(value: true);
			fullAbilityCard.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
			fullAbilityCard.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0.5f);
			fullAbilityCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			UpdateSize(CardHandMode.ActionSelection);
		}
		else
		{
			miniAbilityCard.gameObject.SetActive(value: true);
			fullAbilityCard.gameObject.SetActive(value: false);
			fullAbilityCard.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
			fullAbilityCard.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
			fullAbilityCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(currentFullCardSpacing, 0f);
			fullAbilityCard.Highlight(active: false);
			UpdateSize(mode);
		}
	}

	private void SetDisplayMode(CardPileType type)
	{
		SetDisplayMode(type, isHighlighted);
	}

	private void SetDisplayMode(CardPileType type, bool isHighlighted)
	{
		bool flag = type == CardPileType.Round;
		int num;
		if (flag && FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && PhaseManager.IsInCardsSelectionPhase && playerActor != null)
		{
			num = ((!playerActor.IsUnderMyControl()) ? 1 : 0);
			if (num != 0)
			{
				type = unselectedCardType;
				flag = false;
			}
		}
		else
		{
			num = 0;
		}
		int selectedIndex = -1;
		if (((num == 0) & flag) && PhaseManager.IsInCardsSelectionPhase && abilityCard != null && playerActor != null)
		{
			selectedIndex = playerActor.CharacterClass.RoundAbilityCards.IndexOf(abilityCard);
		}
		_ = CardRequiredForm;
		miniAbilityCard.SetDisplayMode(type, abilityCard, isHighlighted && isValid, isHovered && isValid, selectedIndex);
		SetHoverLock(flag);
	}

	public void SetHoverDetectionActive(bool active)
	{
		if (_hoverMove != null)
		{
			_hoverMove.enabled = active;
		}
		if (_hoverScale != null)
		{
			_hoverScale.enabled = active;
		}
	}

	private void SetHoverParams(float hoverScale, Vector3 hoverMove)
	{
		bool flag = false;
		if (_hoverScale != null)
		{
			_hoverScale.ScaleFactor = hoverScale;
			_hoverScale.ScaleFactorOnClick = hoverScale;
			flag = true;
		}
		if (_hoverMove != null)
		{
			_hoverMove.MoveOffset = hoverMove;
			_hoverMove.MoveOffsetOnClick = hoverMove;
			flag = true;
		}
		if (flag)
		{
			hoverScale = 1f;
			hoverMove = Vector3.zero;
		}
		button.highlightScaleFactor = hoverScale;
		button.hoverMovement = hoverMove;
	}

	private void SetHoverLock(bool isLocked)
	{
		if (_hoverScale != null)
		{
			UIHoverScaleEffects.LockType lockType = (isLocked ? UIHoverScaleEffects.LockType.ScaleFactor : UIHoverScaleEffects.LockType.None);
			_hoverScale.SetLock(lockType);
		}
		if (_hoverMove != null)
		{
			UIHoverMoveEffects.LockType lockType2 = (isLocked ? UIHoverMoveEffects.LockType.MoveOffset : UIHoverMoveEffects.LockType.None);
			_hoverMove.SetLock(lockType2);
		}
	}

	public void SetLocalScale(Vector3 scale)
	{
		Transform transform = base.transform;
		Transform transform2 = fullAbilityCard.transform;
		transform.localScale = scale;
		transform2.localScale = ((transform2.parent == transform) ? Vector3.one : scale);
	}

	public void ToggleFullCardPreview(bool isHighlighted, Transform holder = null)
	{
		if (previewingFullCard == isHighlighted || (disableFullCard && mode == CardHandMode.DeckSelection))
		{
			return;
		}
		previewingFullCard = isHighlighted;
		SetDisplayMode(cardType, isHighlighted);
		ToggleFullCard(active: false);
		fullAbilityCard.Highlight(active: false);
		fullAbilityCard.gameObject.SetActive(isHighlighted);
		fullAbilityCard.ToggleBurningAnimation(isHighlighted);
		fullAbilityCard.BlockRaycasts(holder == null || !isHighlighted);
		fullAbilityCard.transform.SetParent(((holder != null) & isHighlighted) ? holder : transform);
		if (isHighlighted)
		{
			RectTransform component = fullAbilityCard.GetComponent<RectTransform>();
			if (overrideFullPreviewCardPosition != null)
			{
				component.anchoredPosition = overrideFullPreviewCardPosition(component);
			}
			else if (holder != null)
			{
				fullAbilityCard.transform.position = new Vector3(holder.position.x, transform.position.y);
				component.anchoredPosition = new Vector2((0f - component.rect.width) / 2f - holder.GetComponent<RectTransform>().rect.width / 2f, component.anchoredPosition.y);
			}
			fullAbilityCard.transform.position += component.DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, 20f);
		}
	}

	private void UpdateSize(CardHandMode mode)
	{
		if (mode == CardHandMode.CardsSelection || mode == CardHandMode.LoseCard || mode == CardHandMode.DiscardCard || mode == CardHandMode.DeckSelection || mode == CardHandMode.RecoverDiscardedCard || mode == CardHandMode.RecoverLostCard || mode == CardHandMode.IncreaseCardLimit || mode == CardHandMode.SelectCardsWithActiveBonusTracker || mode == CardHandMode.SelectCardsWithTideActiveBonusesToKeep || mode == CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate || mode == CardHandMode.PreviewMiniCard)
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(miniAbilityCard.GetComponent<RectTransform>().sizeDelta.x, previewCardHeight);
		}
		else
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(fullAbilityCard.GetComponent<RectTransform>().sizeDelta.x * fullAbilityCard.transform.localScale.x, fullAbilityCard.GetComponent<RectTransform>().sizeDelta.y * fullAbilityCard.transform.localScale.y);
		}
	}

	public void SetFade(float fade)
	{
		button.SetFade(fade);
	}

	public void ToggleHighlight(bool active, bool highlight)
	{
		cardType = (active ? CardPileType.Round : unselectedCardType);
		SetDisplayMode(active ? CardPileType.Round : unselectedCardType, highlight);
		if (FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && !playerActor.IsUnderMyControl() && PhaseManager.IsInCardsSelectionPhase)
		{
			fullAbilityCard.DisplaySelected(isSelected: false);
		}
		else
		{
			fullAbilityCard.DisplaySelected(active);
		}
	}

	public void ToggleSelect(bool active, bool highlight = false, bool networkAction = true)
	{
		if (!active && !IsSelected)
		{
			Log.GUIError("ToggleSelect. Trying to deselect a deselected card (" + CardName + "). Returning.");
			return;
		}
		if (active && IsSelected)
		{
			Log.GUIError("ToggleSelect. Trying to select a selected card (" + CardName + "). Returning.");
			return;
		}
		bool flag;
		if (!active && IsSelected)
		{
			flag = true;
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			flag = mode != CardHandMode.CardsSelection || playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None || GameState.IsPerformingCardOutOfPhase || GameState.IsSelectingCardForMode() || IsLongRest || (playerActor.CharacterClass.RoundAbilityCards.Count < playerActor.MaxRoundCardsCount && !IsGivenToOtherPlayer());
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			CMapCharacter cMapCharacter = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == ClassID);
			if (cMapCharacter != null)
			{
				flag = cMapCharacter.HandAbilityCardIDs.Count < cMapCharacter.MaxCards;
			}
			else
			{
				Log.GUIError("Unable to find Map Party for AbilityCardUI");
				flag = false;
			}
		}
		else if (CustomPartySetup.Instance != null)
		{
			CMapCharacter cMapCharacter2 = CustomPartySetup.Instance.CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == ClassID);
			if (cMapCharacter2 != null)
			{
				flag = cMapCharacter2.HandAbilityCardIDs.Count < cMapCharacter2.MaxCards;
			}
			else
			{
				Log.GUIError("Unable to find Map Party for AbilityCardUI");
				flag = false;
			}
		}
		else
		{
			Log.GUIError("Unable to find Map Party for AbilityCardUI");
			flag = false;
		}
		if (!flag)
		{
			AudioControllerUtils.PlaySound(interactionAudioProfile.nonInteractableMouseDownAudioItem);
			onUnableToSelectAction?.Invoke(this);
			return;
		}
		int controllableID = ControllableRegistry.GetControllableID(ClassID);
		if (!FFSNetwork.IsOnline || PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable a) => a.ID == controllableID))
		{
			AudioControllerUtils.PlaySound(interactionAudioProfile.mouseDownAudioItem);
		}
		cardType = (active ? CardPileType.Round : unselectedCardType);
		SetDisplayMode(active ? CardPileType.Round : unselectedCardType, highlight);
		if (FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && !playerActor.IsUnderMyControl() && PhaseManager.IsInCardsSelectionPhase)
		{
			fullAbilityCard.DisplaySelected(isSelected: false);
		}
		else
		{
			fullAbilityCard.DisplaySelected(active);
		}
		if (IsLongRest)
		{
			if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				playerActor.IsLongRestSelected = active;
			}
			else
			{
				playerActor.IsLongRestActionSelected = active;
			}
		}
		if (active)
		{
			Log.NetworkTrace("[ABILITY CARD UI]: Toggle Select, selecting card " + CardName);
			onSelectedCallback(this, networkAction);
		}
		else
		{
			Log.NetworkTrace("[ABILITY CARD UI]: Toggle Select, deselecting a card " + CardName);
			onDeselectedCallback(this, networkAction);
		}
	}

	public bool IsGivenToOtherPlayer()
	{
		if (abilityCard.ClassID == playerActor.CharacterClass.CharacterID)
		{
			return ScenarioManager.Scenario.PlayerActors.Exists((CPlayerActor it) => it.ID != playerActor.ID && (it.CharacterClass.GivenCards.Exists((CAbilityCard card) => card.ID == abilityCard.ID) || it.CharacterClass.ActivatedAbilityCards.Exists((CAbilityCard card) => card.ID == abilityCard.ID)));
		}
		return false;
	}

	public bool IsMatchingCard(int cardInstanceID)
	{
		if (!IsLongRest || cardInstanceID != 0)
		{
			CAbilityCard cAbilityCard = AbilityCard;
			if (cAbilityCard == null)
			{
				return false;
			}
			return cAbilityCard.CardInstanceID == cardInstanceID;
		}
		return true;
	}

	public bool IsMatchingCardID(int cardID)
	{
		CAbilityCard cAbilityCard = AbilityCard;
		if (cAbilityCard == null)
		{
			return false;
		}
		return cAbilityCard.ID == cardID;
	}

	public int CompareTo(object obj)
	{
		AbilityCardUI abilityCardUI = (AbilityCardUI)obj;
		int num = ((cardType == CardPileType.Round && PhaseManager.IsInCardsSelectionPhase) ? CardPileType.Hand.ToSortedValue() : cardType.ToSortedValue());
		int num2 = ((abilityCardUI.cardType == CardPileType.Round && PhaseManager.IsInCardsSelectionPhase) ? CardPileType.Hand.ToSortedValue() : abilityCardUI.cardType.ToSortedValue());
		if (abilityCardUI.IsLongRest && !IsLongRest)
		{
			if (num < CardPileType.Discarded.ToSortedValue())
			{
				return -1;
			}
			return 1;
		}
		if (!abilityCardUI.IsLongRest && IsLongRest)
		{
			if (num2 < CardPileType.Discarded.ToSortedValue())
			{
				return 1;
			}
			return -1;
		}
		if (num2 < num)
		{
			return 1;
		}
		if (num2 > num)
		{
			return -1;
		}
		if (playerActor == null || playerActor.CharacterClass.CharacterID != "GeminateID")
		{
			return abilityCard.Initiative.CompareTo(abilityCardUI.abilityCard.Initiative);
		}
		CPlayerActor.EForm eForm = ((playerActor.Form == CPlayerActor.EForm.None) ? playerActor.PendingForm : playerActor.Form);
		int num3 = ((CardRequiredForm != eForm) ? ((int)CardRequiredForm * 100) : 0);
		int num4 = ((abilityCardUI.CardRequiredForm != eForm) ? ((int)abilityCardUI.CardRequiredForm * 100) : 0);
		num = abilityCard.Initiative + num3;
		num2 = abilityCardUI.abilityCard.Initiative + num4;
		return num.CompareTo(num2);
	}

	public void MakeAbilityCard(AbilityCardYMLData cardData, bool alwaysShow = false, bool isLongRest = false)
	{
		CardID = cardData.ID;
		CardName = cardData.Name;
		CardRequiredForm = cardData.RequiredForm;
		alwaysShowFullCard = alwaysShow;
		IsGeneratedFromLayout = true;
		miniAbilityCard.MakeMiniCard(CardName, cardData, isLongRest);
		fullAbilityCard.MakeFullCard(CardName, cardData, fullCardDescriptionPadding, isLongRest);
	}

	public void MakeAbilityCardContinued()
	{
		fullAbilityCard.MakeFullCardContinued();
	}

	public void OnReturnedToPool()
	{
		SetUnfocused(unfocused: false);
		fullAbilityCard.BlockRaycasts(block: true);
		fullAbilityCard.gameObject.SetActive(value: true);
		fullAbilityCard.ClearEffects();
		fullAbilityCard.EnableSelectCard(enabled: false);
		fullAbilityCard.ResetHighlightInCanvas();
		cardType = CardPileType.Any;
		unselectedCardType = CardPileType.Any;
		SetValid(isValid: true);
		RemoveInteractabilityRelationship();
		fullAbilityCard.ResetInteractable();
		disableFullCard = false;
		playerActor = null;
		fullAbilityCard.ResetConsumes(clearSelection: true, skipElementCheck: true);
		fullAbilityCard.HighlightAvailableConsumes(null);
		SetHoverParams(defaultHighlightScale, defaultHoverMovement);
		miniAbilityCard.Clear();
		DisableEventDetection(disable: false);
		ClearEnhancements();
	}

	public void OnRemovedFromPool()
	{
	}

	public static bool GetCustomCardImage(AbilityCardYMLData cardData, out Sprite sprite)
	{
		bool flag = !string.IsNullOrEmpty(cardData.CustomImage);
		sprite = null;
		if (flag)
		{
			sprite = AssetBundleManager.Instance.LoadAssetFromBundle<Sprite>("misc_cardimages", cardData.CustomImage, "cardSprites", "png", null, suppressError: true);
			return sprite != null;
		}
		return false;
	}

	protected override void OnLanguageChanged()
	{
		Localize();
	}

	public void Localize()
	{
		miniAbilityCard.SetCardName(CardName);
		fullAbilityCard.SetCardName(CardName);
		fullAbilityCard.OnLanguageChanged();
	}

	public void SetUnfocused(bool unfocused)
	{
		miniAbilityCard.SetUnfocused(unfocused);
		fullAbilityCard.SetUnfocused(unfocused);
	}

	public void SetLoseIndicatorVisibility(bool isVisible)
	{
		miniAbilityCard.SetLoseIndicatorVisibility(isVisible);
	}
}

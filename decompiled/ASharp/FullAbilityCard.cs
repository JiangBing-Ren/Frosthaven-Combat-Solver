using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[StateDump(UseToString = true)]
public class FullAbilityCard : MonoBehaviour, IAbilityCardUI, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private TextMeshProUGUI initiativeText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Image headerImage;

	[SerializeField]
	private Image customCardImage;

	[SerializeField]
	public CardEffects cardEffects;

	[Header("Actions")]
	[SerializeField]
	public FullAbilityCardAction topActionButton;

	[SerializeField]
	public FullAbilityCardAction bottomActionButton;

	[SerializeField]
	private AttackValueBreakdown attackValueBreakdown;

	[SerializeField]
	private string audioItemSelectCard = "PlaySound_CardUI_SelectCard";

	[SerializeField]
	private Image unfocusedMask;

	[Header("Default Actions")]
	public TrackedButton DefaultAttackButton;

	public AbilityCardDefaultAction DefaultAttackButtonAction;

	public TrackedButton DefaultMoveButton;

	public AbilityCardDefaultAction DefaultMoveButtonAction;

	public int id;

	private CAbilityCard abilityCard;

	private Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback;

	private Action onActionCompleteCallback;

	private CBaseCard.ECardPile cardPile = CBaseCard.ECardPile.Hand;

	private CardHandMode _mode;

	public CreateLayout topLayout;

	public CreateLayout bottomLayout;

	[SerializeField]
	private bool isLongRestCard;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	public AbilityCardUIAbilityRowPreview _abilityRowPreview;

	private CPlayerActor playerActor;

	private bool enableSelectCard;

	public UnityEvent onFullCardSelected = new UnityEvent();

	private bool isValid = true;

	private Action onClickedInvalidCard;

	private IAbilityCardClassUIElements[] _classElements;

	private Canvas canvas;

	private GraphicRaycaster graphicRaycaster;

	public float HighlightScale { get; set; } = 1f;

	public int CardID => id;

	public ObjectPool.ECardType CardType => ObjectPool.ECardType.Ability;

	public bool IsInPreview
	{
		get
		{
			CardHandMode mode = _mode;
			return mode == CardHandMode.Preview || mode == CardHandMode.PreviewMiniCard;
		}
	}

	public string Title => titleText.text;

	public CAbilityCard AbilityCard => abilityCard;

	public override string ToString()
	{
		return GetType().Name + "(" + abilityCard?.Name + ")";
	}

	private void Awake()
	{
		topActionButton.SetUIEvents(() =>
		{
			OnPointerEnter(isTopAbility: true);
		}, () =>
		{
			OnPointerExit(isTopAbility: true);
		}, Highlight);
		bottomActionButton.SetUIEvents(() =>
		{
			OnPointerEnter(isTopAbility: false);
		}, () =>
		{
			OnPointerExit(isTopAbility: false);
		}, Highlight);
		if (DefaultAttackButton != null && DefaultMoveButton != null)
		{
			DefaultAttackButton.onClick.RemoveAllListeners();
			DefaultAttackButton.onClick.AddListener(AttackAbilityClick);
			DefaultMoveButton.onClick.RemoveAllListeners();
			DefaultMoveButton.onClick.AddListener(MoveAbilityClick);
		}
		if (DefaultAttackButtonAction != null && DefaultMoveButtonAction != null)
		{
			DefaultAttackButtonAction.SetUIEvents(() =>
			{
				OnDefaultPointerEnter(isTopAbility: true);
			}, () =>
			{
				OnDefaultPointerExit(isTopAbility: true);
			}, () =>
			{
				OnDefaultSelect(isTopAbility: true);
			}, () =>
			{
				OnDefaultDeselect(isTopAbility: true);
			});
			DefaultMoveButtonAction.SetUIEvents(() =>
			{
				OnDefaultPointerEnter(isTopAbility: false);
			}, () =>
			{
				OnDefaultPointerExit(isTopAbility: false);
			}, () =>
			{
				OnDefaultSelect(isTopAbility: false);
			}, () =>
			{
				OnDefaultDeselect(isTopAbility: false);
			});
		}
	}

	private void OnDestroy()
	{
		DefaultMoveButton?.onClick.RemoveAllListeners();
		DefaultAttackButton?.onClick.RemoveAllListeners();
		onFullCardSelected.RemoveAllListeners();
		onClickedInvalidCard = null;
		playerActor = null;
		onActionCallback = null;
		onActionCompleteCallback = null;
		abilityCard = null;
		topActionButton = null;
		bottomActionButton = null;
		topLayout = null;
		bottomLayout = null;
		attackValueBreakdown = null;
	}

	private void OnEnable()
	{
		ResetAbilityPreview();
	}

	public void SetInteractable(bool active, CBaseCard.ActionType actionType)
	{
		Log.SimpleInfo("Set Interaction card: " + Title + " to " + active + " action " + actionType);
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.DefaultMoveAction)
		{
			bottomActionButton.SetInteractable(active, actionType == CBaseCard.ActionType.DefaultMoveAction);
		}
		else
		{
			topActionButton.SetInteractable(active, actionType == CBaseCard.ActionType.DefaultAttackAction);
		}
	}

	public void SetInteractable(bool active)
	{
		bottomActionButton.SetInteractable(active);
		topActionButton.SetInteractable(active);
	}

	public void ResetInteractable()
	{
		bottomActionButton.ResetInteractable();
		topActionButton.ResetInteractable();
	}

	public void BlockRaycasts(bool block)
	{
		if (canvasGroup == null)
		{
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
			canvasGroup.ignoreParentGroups = true;
		}
		canvasGroup.blocksRaycasts = block;
	}

	public bool IsInteractable(CBaseCard.ActionType actionType, bool considerSelection = true)
	{
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.DefaultMoveAction)
		{
			return bottomActionButton.IsInteractable(actionType, considerSelection);
		}
		return topActionButton.IsInteractable(actionType, considerSelection);
	}

	public bool HasActionInteractable(bool considerSelection = true)
	{
		if (!bottomActionButton.IsInteractable(considerSelection))
		{
			return topActionButton.IsInteractable(considerSelection);
		}
		return true;
	}

	public void ToggleHover(bool active, bool isTopSide)
	{
		if (isTopSide)
		{
			topActionButton.ToggleAnimateBurnIcon(active);
			topActionButton.ToggleHover(active, isDefaultAbility: true);
			topActionButton.ToggleHover(active, isDefaultAbility: false);
			topActionButton.ToggleHoverInfusion(active);
		}
		else
		{
			bottomActionButton.ToggleAnimateBurnIcon(active);
			bottomActionButton.ToggleHover(active, isDefaultAbility: true);
			bottomActionButton.ToggleHover(active, isDefaultAbility: false);
			bottomActionButton.ToggleHoverInfusion(active);
		}
	}

	public void ToggleHighlightHover(bool active, bool isTopSide, bool isDefault)
	{
		if (isTopSide)
		{
			topActionButton.ToggleHighlightHover(active, isDefault);
		}
		else
		{
			bottomActionButton.ToggleHighlightHover(active, isDefault);
		}
	}

	public void UntoggleHighlightHover(bool isTopSide)
	{
		if (isTopSide)
		{
			topActionButton.ToggleHighlightHover(active: false, isDefaultAbility: false);
			topActionButton.ToggleHighlightHover(active: false, isDefaultAbility: true);
		}
		else
		{
			bottomActionButton.ToggleHighlightHover(active: false, isDefaultAbility: false);
			bottomActionButton.ToggleHighlightHover(active: false, isDefaultAbility: true);
		}
	}

	public void Deselect()
	{
		bottomActionButton.ToggleSelect(active: false, isDefaultAction: true);
		bottomActionButton.ToggleSelect(active: false);
		topActionButton.ToggleSelect(active: false, isDefaultAction: true);
		topActionButton.ToggleSelect(active: false);
	}

	public void ToggleSelect(bool active, CBaseCard.ActionType actionType)
	{
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.DefaultMoveAction)
		{
			bottomActionButton.ToggleSelect(active, actionType == CBaseCard.ActionType.DefaultMoveAction);
		}
		else
		{
			topActionButton.ToggleSelect(active, actionType == CBaseCard.ActionType.DefaultAttackAction);
		}
	}

	public void SetSkin(AbilityCardUISkin skin)
	{
		if (!isLongRestCard)
		{
			if (cardEffects != null && !cardEffects.HasEffect(CardEffects.FXTask.BurnCard))
			{
				initiativeText.color = skin.initiativeColor;
			}
			if (!skin.__HACK_FHCards)
			{
				Image image = headerImage;
				Sprite sprite = (unfocusedMask.sprite = skin.titleSprite);
				image.sprite = sprite;
			}
		}
		topActionButton.SetSkin(skin, topAction: true, isLongRestCard);
		bottomActionButton.SetSkin(skin, topAction: false, isLongRestCard);
	}

	public void SetVisualsFromConfig(CharacterConfigUI config)
	{
		topActionButton.SetVisualsFromConfig(config);
		topLayout?.SetVisualsFromConfig(config);
		bottomActionButton.SetVisualsFromConfig(config);
		bottomLayout?.SetVisualsFromConfig(config);
	}

	public void Highlight(bool active)
	{
		if (!((!topActionButton.IsInteractable() && !bottomActionButton.IsInteractable()) & active) && active && abilityCard != null && DebugMenu.displayCardsId)
		{
			UIManager.Instance.DisplayDebugMessage(abilityCard.Name + " card ID: " + abilityCard.ID + " card instance ID: " + abilityCard.CardInstanceID);
		}
	}

	public void HighlightAvailableConsumes(List<ElementInfusionBoardManager.EElement> availableElements)
	{
		topActionButton.HighlightAvailableConsumes(availableElements);
		bottomActionButton.HighlightAvailableConsumes(availableElements);
	}

	public void ToggleBurningAnimation(bool active)
	{
		topActionButton.ToggleAnimateBurnIcon(active);
		bottomActionButton.ToggleAnimateBurnIcon(active);
	}

	public void SetPile(CBaseCard.ECardPile newCardPile)
	{
		if (cardPile != newCardPile && cardEffects != null)
		{
			if (newCardPile == CBaseCard.ECardPile.Discarded)
			{
				ToggleCardEffect(active: true, CardEffects.FXTask.DiscardMode);
			}
			if (newCardPile == CBaseCard.ECardPile.Lost || newCardPile == CBaseCard.ECardPile.PermanentlyLost)
			{
				ToggleCardEffect(active: true, CardEffects.FXTask.LostMode);
			}
			if (newCardPile == CBaseCard.ECardPile.Hand)
			{
				cardEffects.RestoreCard();
			}
		}
		cardPile = newCardPile;
		if (bottomActionButton != null)
		{
			bottomActionButton.UpdateView(cardPile);
		}
		if (topActionButton != null)
		{
			topActionButton.UpdateView(cardPile);
		}
	}

	public void RefreshPile()
	{
		if (cardEffects != null)
		{
			if (cardPile == CBaseCard.ECardPile.Discarded)
			{
				cardEffects.ToggleEffect(active: false, CardEffects.FXTask.DiscardMode);
			}
			if (cardPile == CBaseCard.ECardPile.Lost || cardPile == CBaseCard.ECardPile.PermanentlyLost)
			{
				cardEffects.ToggleEffect(active: false, CardEffects.FXTask.LostMode);
			}
			if (cardPile == CBaseCard.ECardPile.Hand)
			{
				cardEffects.RestoreCard();
			}
		}
	}

	public List<InfuseElement> UnselectedInfusions(CBaseCard.ActionType abilityType)
	{
		List<InfuseElement> list = new List<InfuseElement>();
		switch (abilityType)
		{
		case CBaseCard.ActionType.TopAction:
			list.AddRange(topActionButton.UnselectedInfusions());
			break;
		case CBaseCard.ActionType.BottomAction:
			list.AddRange(bottomActionButton.UnselectedInfusions());
			break;
		}
		return list;
	}

	public bool AreThereInfusionsToPick(CBaseCard.ActionType abilityType)
	{
		return UnselectedInfusions(abilityType).Count > 0;
	}

	private List<ElementInfusionBoardManager.EElement> SelectedAnyInfusionElemenents(CBaseCard.ActionType abilityType)
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		switch (abilityType)
		{
		case CBaseCard.ActionType.TopAction:
			list.AddRange(topActionButton.SelectedAnyInfusionElements());
			break;
		case CBaseCard.ActionType.BottomAction:
			list.AddRange(bottomActionButton.SelectedAnyInfusionElements());
			break;
		}
		return list;
	}

	public void Init(CPlayerActor playerActor, CAbilityCard abilityCard, int id, Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback, Action onActionCompleteCallback)
	{
		this.playerActor = playerActor;
		this.onActionCallback = onActionCallback;
		this.onActionCompleteCallback = onActionCompleteCallback;
		this.abilityCard = abilityCard;
		this.id = id;
		SetPile(CBaseCard.ECardPile.Hand);
		UpdateCardSummary(abilityCard);
		InitCardLevel(abilityCard?.GetAbilityCardYML);
		OnLanguageChanged();
		if (GameState.CurrentExtraTurnAbility != null && !GameState.CurrentExtraTurnAbility.IsPending())
		{
			if (AbilityCard != null && !GameState.CurrentExtraTurnAbility.IsValidAction(AbilityCard.TopAction))
			{
				ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			}
			if (AbilityCard != null && !GameState.CurrentExtraTurnAbility.IsValidAction(AbilityCard.BottomAction))
			{
				ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			}
			if (AbilityCard != null && !GameState.CurrentExtraTurnAbility.IsValidAction(AbilityCard.DefaultAttackAction))
			{
				ToggleActionInteractivity(active: false, CBaseCard.ActionType.DefaultAttackAction);
			}
			if (AbilityCard != null && !GameState.CurrentExtraTurnAbility.IsValidAction(AbilityCard.DefaultMoveAction))
			{
				ToggleActionInteractivity(active: false, CBaseCard.ActionType.DefaultMoveAction);
			}
		}
		CAbilitySelectAndPlayCard currentSelectAndPlayCardAbility = GameState.CurrentSelectAndPlayCardAbility;
		if (currentSelectAndPlayCardAbility != null && currentSelectAndPlayCardAbility.m_State == CAbilitySelectAndPlayCard.SelectAndPlayCardState.PlayCards)
		{
			if (AbilityCard != null && !GameState.CurrentSelectAndPlayCardAbility.IsValidAction(AbilityCard.TopAction, CBaseCard.ActionType.TopAction, AbilityCard))
			{
				ToggleActionInteractivity(active: false, CBaseCard.ActionType.TopAction);
			}
			if (AbilityCard != null && !GameState.CurrentSelectAndPlayCardAbility.IsValidAction(AbilityCard.BottomAction, CBaseCard.ActionType.BottomAction, AbilityCard))
			{
				ToggleActionInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			}
			if (AbilityCard != null && !GameState.CurrentSelectAndPlayCardAbility.IsValidAction(AbilityCard.DefaultAttackAction, CBaseCard.ActionType.DefaultAttackAction, AbilityCard))
			{
				ToggleActionInteractivity(active: false, CBaseCard.ActionType.DefaultAttackAction);
			}
			if (AbilityCard != null && !GameState.CurrentSelectAndPlayCardAbility.IsValidAction(AbilityCard.DefaultMoveAction, CBaseCard.ActionType.DefaultMoveAction, AbilityCard))
			{
				ToggleActionInteractivity(active: false, CBaseCard.ActionType.DefaultMoveAction);
			}
		}
		if (playerActor != null && !playerActor.CanUseDefaultActions)
		{
			ToggleActionInteractivity(active: false, CBaseCard.ActionType.DefaultAttackAction);
			ToggleActionInteractivity(active: false, CBaseCard.ActionType.DefaultMoveAction);
		}
		topActionButton.Init();
		bottomActionButton.Init();
	}

	public void InitCardLevel(AbilityCardYMLData yml)
	{
		if (!(levelText == null) && yml != null)
		{
			levelText.text = yml.Level;
			levelText.gameObject.SetActive(!isLongRestCard);
		}
	}

	public void UpdateCardSummary(CAbilityCard fallbackAbilityCard)
	{
		CAbilityCard cAbilityCard = abilityCard ?? fallbackAbilityCard;
		if (cAbilityCard != null && !isLongRestCard)
		{
			if (initiativeText != null)
			{
				initiativeText.text = cAbilityCard.Initiative.ToString();
			}
			if (topActionButton != null)
			{
				AbilityCardYMLData getAbilityCardYML = cAbilityCard.GetAbilityCardYML;
				topActionButton.InitSummary(getAbilityCardYML.TopActionCardData, getAbilityCardYML.TopIsLost, getAbilityCardYML.TopIsPermLost);
			}
			if (bottomActionButton != null)
			{
				AbilityCardYMLData getAbilityCardYML2 = cAbilityCard.GetAbilityCardYML;
				bottomActionButton.InitSummary(getAbilityCardYML2.BottomActionCardData, getAbilityCardYML2.BottomIsLost, getAbilityCardYML2.BottomIsPermLost);
			}
		}
	}

	public void MakeFullCard(string cardName, AbilityCardYMLData cardData, Vector2 contentPadding, bool isLongRest = false)
	{
		isLongRestCard = isLongRest;
		SetCardName(cardName);
		SetCustomCardImage(cardData);
		if (initiativeText != null)
		{
			initiativeText.text = cardData.Initiative.ToString();
		}
		InitCardLevel(cardData);
		topLayout = topActionButton.MakeAbilityAction(cardData.TopActionFullLayout?.ParentGroup, cardData, contentPadding, isLongRestCard, FullAbilityCardAction.CardHalf.Top);
		if (!isLongRestCard)
		{
			bottomLayout = bottomActionButton.MakeAbilityAction(cardData.BottomActionFullLayout?.ParentGroup, cardData, contentPadding, isLongRestCard, FullAbilityCardAction.CardHalf.Bottom);
		}
	}

	public void MakeFullCardContinued()
	{
		if (topLayout != null)
		{
			topActionButton.MakeAbilityActionContinued(topLayout);
		}
		if (!isLongRestCard && bottomLayout != null)
		{
			bottomActionButton.MakeAbilityActionContinued(bottomLayout);
		}
	}

	public void SetupClassElements(CAbilityCard card)
	{
		abilityCard = card;
		if (card?.GetAbilityCardYML != null)
		{
			if (_classElements == null)
			{
				_classElements = GetComponents<IAbilityCardClassUIElements>();
			}
			IAbilityCardClassUIElements[] classElements = _classElements;
			foreach (IAbilityCardClassUIElements obj in classElements)
			{
				obj.SetupCharacterClassElements(card.Name, card.GetAbilityCardYML);
				obj.UpdateCharacterClassElements(playerActor);
			}
		}
	}

	public void UpdateClassElements(CPlayerActor cPlayerActor)
	{
		if (_classElements != null && (playerActor == null || playerActor == cPlayerActor))
		{
			IAbilityCardClassUIElements[] classElements = _classElements;
			for (int i = 0; i < classElements.Length; i++)
			{
				classElements[i].UpdateCharacterClassElements(cPlayerActor);
			}
		}
	}

	public void MoveAbilityClick()
	{
		if (bottomActionButton.IsInteractable(CBaseCard.ActionType.DefaultMoveAction))
		{
			OnAbilityClick(CBaseCard.ActionType.DefaultMoveAction, isProxyAction: false);
		}
	}

	public void AttackAbilityClick()
	{
		if (topActionButton.IsInteractable(CBaseCard.ActionType.DefaultAttackAction))
		{
			OnAbilityClick(CBaseCard.ActionType.DefaultAttackAction, isProxyAction: false);
		}
	}

	public void OnAbilityClick(bool isTopAbility)
	{
		OnAbilityClick(isTopAbility ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction, isProxyAction: false);
	}

	public void OnPointerEnter(bool isTopAbility)
	{
		if (isValid && !(isTopAbility ? (topActionButton == null) : (bottomActionButton == null)) && (isTopAbility ? topActionButton.IsInteractable(considerSelection: false) : bottomActionButton.IsInteractable(considerSelection: false)) && abilityCard != null && playerActor != null && !(Singleton<CardsHandManager>.Instance == null))
		{
			if (isTopAbility)
			{
				topActionButton.ToggleAnimateBurnIcon(active: true);
				topActionButton.ToggleHoverInfusion(active: true);
			}
			else
			{
				bottomActionButton.ToggleHoverInfusion(active: true);
				bottomActionButton.ToggleAnimateBurnIcon(active: true);
			}
			Singleton<CardsHandManager>.Instance.cardsActionController.OnCardHighlight(isActive: true, this, isTopAbility, isDefaultAbility: false);
			CAction cAction = (isTopAbility ? abilityCard.TopAction : abilityCard.BottomAction);
			if (cAction.FindType(CAbility.EAbilityType.Attack) != null)
			{
				attackValueBreakdown.Init(playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack), GameState.CurrentActionSelectedAugmentations, cAction);
				attackValueBreakdown.Show();
			}
		}
	}

	public void OnPointerExit(bool isTopAbility)
	{
		Singleton<CardsHandManager>.Instance?.cardsActionController.OnCardHighlight(isActive: false, this, isTopAbility, isDefaultAbility: false);
		if (isTopAbility)
		{
			topActionButton.ToggleHoverInfusion(active: false);
			topActionButton.ToggleAnimateBurnIcon(active: false);
		}
		else
		{
			bottomActionButton.ToggleHoverInfusion(active: false);
			bottomActionButton.ToggleAnimateBurnIcon(active: false);
		}
		attackValueBreakdown?.Hide();
	}

	public void OnDefaultPointerEnter(bool isTopAbility)
	{
		if (isValid && (isTopAbility ? topActionButton.IsInteractable(considerSelection: false) : bottomActionButton.IsInteractable(considerSelection: false)) && abilityCard != null && playerActor != null && !(Singleton<CardsHandManager>.InstanceFast == null))
		{
			Singleton<CardsHandManager>.InstanceFast?.cardsActionController.OnCardHighlight(isActive: true, this, isTopAbility, isDefaultAbility: true);
		}
	}

	public void OnDefaultPointerExit(bool isTopAbility)
	{
		Singleton<CardsHandManager>.InstanceFast?.cardsActionController.OnCardHighlight(isActive: false, this, isTopAbility, isDefaultAbility: true);
	}

	public void OnDefaultSelect(bool isTopAbility)
	{
		OnDefaultPointerEnter(isTopAbility);
		Highlight(active: true);
	}

	public void OnDefaultDeselect(bool isTopAbility)
	{
		OnDefaultPointerExit(isTopAbility);
		Highlight(active: false);
	}

	public void TryPlayBurnAnimation(CBaseCard.ActionType abilityType)
	{
		if (cardPile == CBaseCard.ECardPile.Lost || cardPile == CBaseCard.ECardPile.PermanentlyLost || abilityCard == null)
		{
			return;
		}
		CAction cAction = null;
		switch (abilityType)
		{
		case CBaseCard.ActionType.BottomAction:
			cAction = abilityCard.BottomAction;
			break;
		case CBaseCard.ActionType.TopAction:
			cAction = abilityCard.TopAction;
			break;
		default:
			return;
		}
		if ((cAction.CardPile == CBaseCard.ECardPile.Lost && abilityCard.CurrentCardPile == CBaseCard.ECardPile.Lost) || (cAction.CardPile == CBaseCard.ECardPile.PermanentlyLost && abilityCard.CurrentCardPile == CBaseCard.ECardPile.PermanentlyLost))
		{
			if (abilityCard.ActionHasHappened)
			{
				if (cardEffects != null && !cardEffects.HasEffect(CardEffects.FXTask.BurnCard))
				{
					ToggleCardEffect(active: true, CardEffects.FXTask.BurnCard);
					AudioController.Play("PlaySound_CardUI_BurnedCard", transform, null, attachToParent: false);
				}
			}
			else if (cardEffects != null && !cardEffects.HasEffect(CardEffects.FXTask.DiscardMode) && !cardEffects.HasEffect(CardEffects.FXTask.BurnCard) && abilityCard.CurrentCardPile == CBaseCard.ECardPile.Discarded)
			{
				ToggleCardEffect(active: true, CardEffects.FXTask.DiscardMode);
				AudioController.Play("PlaySound_CardUI_BurnedCard", transform, null, attachToParent: false);
			}
		}
		else if (cardEffects != null && !cardEffects.HasEffect(CardEffects.FXTask.DiscardMode) && !cardEffects.HasEffect(CardEffects.FXTask.BurnCard) && (abilityType == CBaseCard.ActionType.DefaultAttackAction || abilityType == CBaseCard.ActionType.DefaultMoveAction || abilityCard.CurrentCardPile == CBaseCard.ECardPile.Discarded))
		{
			ToggleCardEffect(active: true, CardEffects.FXTask.DiscardMode);
			AudioController.Play("PlaySound_CardUI_DiscardedCard", transform, null, attachToParent: false);
		}
	}

	public void OnAbilityClick(CBaseCard.ActionType abilityType, bool isProxyAction, bool checkValid = true)
	{
		if (InfusionBoardUI.Instance == null)
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to InfusionBoardUI instance == null");
			return;
		}
		if (Singleton<CardsHandManager>.Instance.IsFullCardPreviewShowing && !isProxyAction)
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to CardsHandManager IsFullCardPreviewShowing and !isProxyAction");
			return;
		}
		if (!IsInteractable(abilityType, !isLongRestCard))
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to card not being interactable action type: " + abilityType);
			return;
		}
		if (!isProxyAction && Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.m_MessageQueue.Count > 0)
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to Choreographer busy");
			return;
		}
		if (!isProxyAction && FFSNetwork.IsOnline && Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.m_WaitState.m_State < Choreographer.ChoreographerStateType.Play)
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to Choreographer wait state: " + Choreographer.s_Choreographer.m_WaitState.m_State);
			return;
		}
		if (checkValid && !isValid)
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			onClickedInvalidCard?.Invoke();
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to card not being valid");
			return;
		}
		if (!isProxyAction && FFSNetwork.IsOnline && GHClientCallbacks.CurrentStreamMode != GHClientCallbacks.EStreamMode.NotSet)
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to being in the process of sending data to the host/other clients");
			return;
		}
		if (enableSelectCard)
		{
			AudioControllerUtils.PlaySound(audioItemSelectCard);
			onFullCardSelected.Invoke();
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action || PhaseManager.PhaseType == CPhase.PhaseType.InfuserMechanicSelection || (Singleton<CardsHandManager>.Instance != null && !Singleton<CardsHandManager>.Instance.cardsActionController.IsActionAvailable) || (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && Choreographer.s_Choreographer.CurrentActor != playerActor))
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to phase type");
			return;
		}
		if (cardPile != CBaseCard.ECardPile.Round && cardPile != CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase && PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && !isProxyAction)
		{
			Log.SimpleInfo("Returning from OnAbilityClick for card: " + titleText.text + " early due to UI card not being in the round pile at action selection phase");
			return;
		}
		if (!enableSelectCard)
		{
			AudioControllerUtils.PlaySound(audioItemSelectCard);
		}
		CBaseCard.ActionType actionType;
		UIEvent.EUIEventType eventType;
		switch (abilityType)
		{
		case CBaseCard.ActionType.TopAction:
			actionType = CBaseCard.ActionType.DefaultAttackAction;
			eventType = UIEvent.EUIEventType.CardTopHalfSelected;
			break;
		case CBaseCard.ActionType.DefaultAttackAction:
			actionType = CBaseCard.ActionType.TopAction;
			eventType = UIEvent.EUIEventType.DefaultAttackAbilityPressed;
			break;
		case CBaseCard.ActionType.BottomAction:
			actionType = CBaseCard.ActionType.DefaultMoveAction;
			eventType = UIEvent.EUIEventType.CardBottomHalfSelected;
			break;
		case CBaseCard.ActionType.DefaultMoveAction:
			actionType = CBaseCard.ActionType.BottomAction;
			eventType = UIEvent.EUIEventType.DefaultMoveAbilityPressed;
			break;
		default:
			Log.GUIError("Invalid ability type set in OnAbilityClick().");
			return;
		}
		Log.SimpleInfo("ToggleActionInteractivity to false for card: " + titleText.text + " after ability click for " + actionType);
		ToggleActionInteractivity(active: false, actionType);
		ElementInfusionBoardManager.SaveState();
		InfusionBoardUI.Instance?.SaveState();
		Singleton<UIUseItemsBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		_ = (CPlayerActor)Choreographer.s_Choreographer.m_CurrentActor;
		UIEventManager.AddUIEvent(new UIEvent(eventType, playerActor.GetPrefabName(), abilityCard.Name));
		Choreographer.s_Choreographer.onCharacterAbilityComplete = () =>
		{
			OnActionMade(abilityType);
		};
		onActionCallback(this, abilityType);
		GameState.PlayerSelectedAbilityCardAction(abilityCard, abilityType);
		Choreographer.s_Choreographer.Pass();
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
			Synchronizer.SendGameAction(GameActionType.SelectCardAbility, ActionSequenceTypeToActionPhaseType(GameState.GetCurrentActionSequenceType(playerActor.SelectedRoundCardsCount)), validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, supplementaryDataIDMax: abilityCard.CardInstanceID, supplementaryDataIDMin: (int)abilityType);
		}
	}

	public static ActionPhaseType ActionSequenceTypeToActionPhaseType(GameState.EActionSequenceType actionSequenceType)
	{
		if (GameState.CurrentOutOfPhaseCardSelectionAbility != null)
		{
			return ActionPhaseType.FirstAction;
		}
		return actionSequenceType switch
		{
			GameState.EActionSequenceType.FirstAction => ActionPhaseType.FirstAction, 
			GameState.EActionSequenceType.SecondAction => ActionPhaseType.SecondAction, 
			GameState.EActionSequenceType.ThirdAction => ActionPhaseType.ThirdAction, 
			_ => throw new ArgumentException($"Invalid action sequence type: '{actionSequenceType}'"), 
		};
	}

	public void SetMode(CardHandMode mode)
	{
		_mode = mode;
		if ((uint)mode <= 14u)
		{
			if (topActionButton != null)
			{
				topActionButton.ToggleDefaultActionVisible(val: true);
			}
			if (bottomActionButton != null)
			{
				bottomActionButton.ToggleDefaultActionVisible(val: true);
			}
			if (topActionButton != null)
			{
				topActionButton.SetInPreview(IsInPreview);
			}
			if (bottomActionButton != null)
			{
				bottomActionButton.SetInPreview(IsInPreview);
			}
			return;
		}
		throw new ArgumentOutOfRangeException("mode", mode, null);
	}

	public void UpdateElements()
	{
		if (abilityCard == null)
		{
			return;
		}
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		if (abilityCard.SelectedAction?.Infusions != null)
		{
			foreach (ElementInfusionBoardManager.EElement infusion in abilityCard.SelectedAction.Infusions)
			{
				if (infusion != ElementInfusionBoardManager.EElement.Any && abilityCard.SelectedAction.Abilities.Any((CAbility a) => a.AbilityHasHappened))
				{
					list.Add(infusion);
					if (abilityCard.SelectedAction?.ID == abilityCard.TopAction.ID)
					{
						topActionButton.AnimateGenerateInfusions();
					}
					else if (abilityCard.SelectedAction?.ID == abilityCard.BottomAction.ID)
					{
						bottomActionButton.AnimateGenerateInfusions();
					}
				}
			}
		}
		InfusionBoardUI.Instance?.UpdateBoard(list);
	}

	private void OnActionMade(CBaseCard.ActionType abilityType)
	{
		onActionCompleteCallback?.Invoke();
	}

	public void ToggleSideInteractivity(bool active, CBaseCard.ActionType actionType)
	{
		if (actionType == CBaseCard.ActionType.TopAction || actionType == CBaseCard.ActionType.DefaultAttackAction)
		{
			topActionButton.SetInteractable(active);
		}
		else
		{
			bottomActionButton.SetInteractable(active);
		}
	}

	public void ToggleActionInteractivity(bool active, CBaseCard.ActionType actionType)
	{
		switch (actionType)
		{
		case CBaseCard.ActionType.TopAction:
			topActionButton.SetInteractable(active, defaultAction: false);
			break;
		case CBaseCard.ActionType.DefaultAttackAction:
			topActionButton.SetInteractable(active, defaultAction: true);
			break;
		default:
			bottomActionButton.SetInteractable(active, actionType == CBaseCard.ActionType.DefaultMoveAction);
			break;
		}
	}

	public void ClearEffects()
	{
		ToggleBurningAnimation(active: false);
		if (cardEffects != null)
		{
			cardEffects.RestoreCard();
		}
		ResetHighlightInfusions();
	}

	public void HighlightInfusions(bool highlight, CBaseCard.ActionType actionType)
	{
		switch (actionType)
		{
		case CBaseCard.ActionType.BottomAction:
			bottomActionButton.ToggleSelectedHighlightInfusion(highlight);
			break;
		case CBaseCard.ActionType.TopAction:
			topActionButton.ToggleSelectedHighlightInfusion(highlight);
			break;
		default:
			topActionButton.ToggleSelectedHighlightInfusion(active: false);
			bottomActionButton.ToggleSelectedHighlightInfusion(active: false);
			break;
		}
	}

	public void ResetHighlightInfusions()
	{
		topActionButton.ToggleSelectedHighlightInfusion(active: false);
		bottomActionButton.ToggleSelectedHighlightInfusion(active: false);
	}

	public void PickUnselectedInfusions(CBaseCard.ActionType abilityType, Action<bool> onFinish, CActor actorPicking)
	{
		List<InfuseElement> infusions = UnselectedInfusions(abilityType);
		if (infusions.Count == 0)
		{
			onFinish?.Invoke(obj: false);
			return;
		}
		((Action)(() =>
		{
			ElementInfusionBoardManager.Infuse(SelectedAnyInfusionElemenents(abilityType), actorPicking);
			InfusionBoardUI.Instance?.UpdateBoard(SelectedAnyInfusionElemenents(abilityType).ToList());
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				foreach (InfuseElement item in infusions)
				{
					_ = item;
					((CPhaseAction)PhaseManager.CurrentPhase).ElementsToInfuse.Remove(ElementInfusionBoardManager.EElement.Any);
				}
			}
			onFinish?.Invoke(obj: true);
		}))();
	}

	public void EnableSelectCard(bool enabled)
	{
		enableSelectCard = enabled;
	}

	public void DisplaySelected(bool isSelected)
	{
		topActionButton.DisplaySelected(enableSelectCard & isSelected, topAction: true, isLongRestCard);
		bottomActionButton.DisplaySelected(enableSelectCard & isSelected, topAction: false, isLongRestCard);
	}

	public void ProxyToggleAugment(bool toggleOn, AbilityAugmentToken dataToken)
	{
		FullAbilityCardAction fullAbilityCardAction = ((dataToken.ActionTypeID == 1) ? topActionButton : bottomActionButton);
		if (dataToken.ConsumeIndex < fullAbilityCardAction.ConsumeButtons.Length && dataToken.ConsumeIndex >= 0)
		{
			Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugment(fullAbilityCardAction.ConsumeButtons[dataToken.ConsumeIndex], toggleOn, dataToken);
			return;
		}
		throw new Exception("Error trying to augment an ability. The provided consume index is out of bounds.");
	}

	public void SetCardName(string cardName)
	{
		titleText.text = LocalizationManager.GetTranslation(cardName);
	}

	public void SetCustomCardImage(AbilityCardYMLData cardData)
	{
		if (!(customCardImage == null))
		{
			bool flag = AbilityCardUI.GetCustomCardImage(cardData, out var sprite);
			customCardImage.enabled = flag;
			if (flag)
			{
				customCardImage.sprite = sprite;
			}
		}
	}

	public void SetValid(bool isValid, Action onClickedInvalidCard = null)
	{
		this.isValid = isValid;
		this.onClickedInvalidCard = onClickedInvalidCard;
		bottomActionButton.DisableHoverHighlight(!isValid);
		topActionButton.DisableHoverHighlight(!isValid);
	}

	public void ResetConsumes(bool clearSelection = true, bool skipElementCheck = false)
	{
		foreach (ConsumeButton item in bottomActionButton.ConsumeButtons.Concat(topActionButton.ConsumeButtons))
		{
			item.ResetState(clearSelection, overrideAvailable: false, skipElementCheck);
		}
	}

	public ConsumeButton FindConsumeButton(CAbility ability)
	{
		AbilityCardYMLData getAbilityCardYML = abilityCard.GetAbilityCardYML;
		ConsumeButton consumeButton = FindAbilityConsumeButton(getAbilityCardYML.TopConsumes, topActionButton.ConsumeButtons, ability);
		if (consumeButton != null)
		{
			return consumeButton;
		}
		return FindAbilityConsumeButton(getAbilityCardYML.BottomConsumes, bottomActionButton.ConsumeButtons, ability);
	}

	private ConsumeButton FindAbilityConsumeButton(List<AbilityConsume> consumes, IList<ConsumeButton> buttons, CAbility ability)
	{
		AbilityConsume consume = consumes.FirstOrDefault((AbilityConsume it) => it.ConsumeData != null && it.ConsumeData.AugmentationOps.Exists((CActionAugmentationOp cActionAugmentationOp) => cActionAugmentationOp.AbilityOverride != null && cActionAugmentationOp.AbilityOverride.ChooseAbilities.Contains(ability)));
		if (consume != null)
		{
			return buttons.FirstOrDefault((ConsumeButton it) => it.ConsumeName == consume.Name);
		}
		return null;
	}

	public void HighlightInCanvas(int order)
	{
		if (canvas == null)
		{
			canvas = gameObject.AddComponent<Canvas>();
		}
		if (graphicRaycaster == null)
		{
			graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
		}
		canvas.overrideSorting = true;
		canvas.sortingOrder = order;
	}

	public void ResetHighlightInCanvas()
	{
		if (graphicRaycaster != null)
		{
			UnityEngine.Object.Destroy(graphicRaycaster);
			graphicRaycaster = null;
		}
		if (canvas != null)
		{
			UnityEngine.Object.Destroy(canvas);
			canvas = null;
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		unfocusedMask.gameObject.SetActive(unfocused);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Highlight(active: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Highlight(active: false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Highlight(active: true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Highlight(active: false);
	}

	public void ToggleCardEffect(bool active, CardEffects.FXTask effect)
	{
		if (cardEffects != null)
		{
			cardEffects.ToggleEffect(active, effect);
		}
	}

	public void OnLanguageChanged()
	{
		if (abilityCard == null)
		{
			return;
		}
		if (topActionButton != null && abilityCard != null)
		{
			AbilityCardActionLocalizedListener[] componentsInChildren = topActionButton.GetComponentsInChildren<AbilityCardActionLocalizedListener>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnLanguageChanged(abilityCard.Name, abilityCard.TopAction);
			}
		}
		if (bottomActionButton != null && abilityCard != null)
		{
			AbilityCardActionLocalizedListener[] componentsInChildren = bottomActionButton.GetComponentsInChildren<AbilityCardActionLocalizedListener>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnLanguageChanged(abilityCard.Name, abilityCard.BottomAction);
			}
		}
	}

	public void OnAbilityStart(string abilityName)
	{
	}

	public void OnAbilityRestart(string abilityName)
	{
	}

	public void OnAbilityEnd(string abilityName)
	{
	}

	public void ResetAbilityPreview()
	{
	}
}

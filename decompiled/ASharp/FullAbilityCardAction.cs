using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FullAbilityCardAction : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public enum CardHalf
	{
		NA,
		Top,
		Bottom
	}

	[SerializeField]
	public Button actionButton;

	[SerializeField]
	private Button defaultActionButton;

	[SerializeField]
	private CardActionSummary[] actionSummaryContainers;

	[SerializeField]
	private RectTransform cardContentTransform;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private UIImageSpriteStateController actionBackground;

	[SerializeField]
	private CanvasGroup defCanvasGroup;

	[SerializeField]
	private UIImageSpriteStateController defActionBackground;

	[SerializeField]
	private CardActionHighlight highlightAction;

	[SerializeField]
	private CardActionHighlight highlightDefaultAction;

	[SerializeField]
	private UIImageSpriteStateController cardFrame;

	[SerializeField]
	private UIImageSpriteStateController cardDefActionFrame;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private float inactiveAlpha = 0.5f;

	public InteractabilityIsolatedUIControl.InteractabilityReference actionInteractability;

	public InteractabilityIsolatedUIControl.InteractabilityReference defaultActionInteractability;

	private bool isInteractable;

	private bool isInteractableDefaultAction;

	private bool isSelected;

	private bool isSelectedDefaultAction;

	private bool isInPreview;

	private Selectable.Transition _actionButtonTransition;

	private Selectable.Transition _defActionButtonTransition;

	[StateDump(Skip = true)]
	private List<EnhancementButton> enhancementButtons;

	private AbilityCardUISkin skin;

	private Action _onPointerEnter;

	private Action _onPointerExit;

	private Action<bool> _onHighlight;

	[field: SerializeField]
	public CardHalf CardHalfAction { get; private set; }

	public bool IsTop => CardHalfAction == CardHalf.Top;

	public bool IsBottom => CardHalfAction == CardHalf.Bottom;

	public bool ActiveAbilityAction { get; private set; }

	public bool ActiveDefaultAction { get; private set; }

	public bool VisibleDefaultAction { get; private set; }

	public ConsumeButton[] ConsumeButtons => GetComponentsInChildren<ConsumeButton>();

	public InfuseElement[] InfuseElements => GetComponentsInChildren<InfuseElement>();

	public List<EnhancementButton> EnhancementButtons
	{
		get
		{
			if (enhancementButtons == null || enhancementButtons.Count == 0)
			{
				enhancementButtons = (from e in GetComponentsInChildren<EnhancementButton>(includeInactive: true)
					where !e.AbilityName.IsNullOrEmpty()
					select e).ToList();
			}
			return enhancementButtons;
		}
	}

	public void SetupActionInteractability(InteractabilityIsolatedUIControl control = null)
	{
		if (control == null)
		{
			control = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (control != null)
		{
			actionInteractability = new InteractabilityIsolatedUIControl.InteractabilityReference
			{
				control = control,
				onInteractabilityChanged = () =>
				{
					RefreshInteractable(defaultAction: false);
				}
			};
		}
	}

	public void SetupDefaultActionInteractability(InteractabilityIsolatedUIControl control = null)
	{
		if (control == null)
		{
			control = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (!(control != null))
		{
			return;
		}
		defaultActionInteractability = new InteractabilityIsolatedUIControl.InteractabilityReference
		{
			control = control,
			onInteractabilityChanged = () =>
			{
				bool flag = VisibleDefaultAction && defaultActionInteractability.cachedEnabled;
				defaultActionButton.gameObject.SetActive(flag);
				if (flag)
				{
					RefreshInteractable(defaultAction: true);
				}
			}
		};
	}

	public void ToggleDefaultActionVisible(bool val)
	{
		VisibleDefaultAction = val;
		defaultActionInteractability?.onInteractabilityChanged();
	}

	public void Init()
	{
		if (enhancementButtons == null)
		{
			enhancementButtons = GetComponentsInChildren<EnhancementButton>(includeInactive: true).ToList();
		}
	}

	private void OnEnable()
	{
		if (enhancementButtons == null)
		{
			enhancementButtons = GetComponentsInChildren<EnhancementButton>(includeInactive: true).ToList();
		}
		InitTooltips();
		_actionButtonTransition = actionButton.transition;
		_defActionButtonTransition = defaultActionButton.transition;
		highlightAction?.Hide();
		highlightDefaultAction?.Hide();
		cardFrame?.SetState(IFHSelectable.SelectionState.Normal);
		cardDefActionFrame?.SetState(IFHSelectable.SelectionState.Normal);
	}

	private void InitTooltips()
	{
		UITextTooltipTarget[] componentsInChildren = cardContentTransform.GetComponentsInChildren<UITextTooltipTarget>(includeInactive: false);
		for (int i = 0; i < componentsInChildren?.Length; i++)
		{
			if (!(componentsInChildren[i] != null))
			{
				continue;
			}
			if (componentsInChildren[i].GetComponent<TextMeshProUGUI>() != null)
			{
				if (LocalizationGlossaryUtils.TryParseGlossaryTexts(componentsInChildren[i].gameObject, out var parsedText))
				{
					componentsInChildren[i].Initialize(UITooltipManager.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
					componentsInChildren[i].SetText(parsedText);
				}
				else
				{
					componentsInChildren[i].enabled = false;
				}
			}
			else if (componentsInChildren[i].GetComponent<RawImage>() != null)
			{
				componentsInChildren[i].Initialize(UITooltipManager.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
				componentsInChildren[i].SetText(CreateLayout.LocaliseText("$Glossary_HitArea$"));
			}
		}
	}

	private void OnDestroy()
	{
		actionButton.onClick.RemoveAllListeners();
		defaultActionButton.onClick.RemoveAllListeners();
		enhancementButtons = null;
	}

	private void ExecuteOnActiveSummaries(Action<CardActionSummary> action)
	{
		if (actionSummaryContainers == null)
		{
			return;
		}
		CardActionSummary[] array = actionSummaryContainers;
		foreach (CardActionSummary cardActionSummary in array)
		{
			if (!(cardActionSummary == null))
			{
				action(cardActionSummary);
			}
		}
	}

	public bool IsInteractable(CBaseCard.ActionType actionType, bool considerSelection = true)
	{
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.TopAction)
		{
			if (isInteractable && (!isSelected || !considerSelection))
			{
				if (actionInteractability != null)
				{
					return actionInteractability.cachedInteractable;
				}
				return true;
			}
			return false;
		}
		if (isInteractableDefaultAction && (!isSelectedDefaultAction || !considerSelection))
		{
			if (defaultActionInteractability != null)
			{
				return defaultActionInteractability.cachedInteractable;
			}
			return true;
		}
		return false;
	}

	public bool IsInteractable(bool considerSelection = true)
	{
		if (!IsInteractable(CBaseCard.ActionType.BottomAction, considerSelection))
		{
			return IsInteractable(CBaseCard.ActionType.DefaultMoveAction, considerSelection);
		}
		return true;
	}

	public void ToggleAnimateBurnIcon(bool active)
	{
		ExecuteOnActiveSummaries((CardActionSummary e) =>
		{
			e.ToggleAnimateBurnIcon(active);
		});
	}

	public void ToggleHoverInfusion(bool active)
	{
		InfuseElement[] infuseElements = InfuseElements;
		for (int i = 0; i < infuseElements.Length; i++)
		{
			infuseElements[i].ToggleHoverEffect(active);
		}
	}

	public void ToggleSelectedHighlightInfusion(bool active)
	{
		InfuseElement[] infuseElements = InfuseElements;
		for (int i = 0; i < infuseElements.Length; i++)
		{
			infuseElements[i].TogglePermanentHighlight(active);
		}
	}

	public void AnimateGenerateInfusions()
	{
		InfuseElement[] infuseElements = InfuseElements;
		for (int i = 0; i < infuseElements.Length; i++)
		{
			infuseElements[i].AnimateGeneratedElement();
		}
	}

	public void ToggleHighlightHover(bool active, bool isDefaultAbility)
	{
		if (active)
		{
			if (isDefaultAbility)
			{
				highlightDefaultAction?.ShowHover();
				cardDefActionFrame?.SetState(IFHSelectable.SelectionState.Highlighted);
			}
			else
			{
				highlightAction?.ShowHover();
				cardFrame?.SetState(IFHSelectable.SelectionState.Highlighted);
			}
		}
		else if (isDefaultAbility)
		{
			highlightDefaultAction?.Hide();
			cardDefActionFrame?.SetState(IFHSelectable.SelectionState.Normal);
		}
		else
		{
			highlightAction?.Hide();
			cardFrame?.SetState(IFHSelectable.SelectionState.Normal);
		}
	}

	public void HighlightAvailableConsumes(List<ElementInfusionBoardManager.EElement> availableElements)
	{
		for (int i = 0; i < ConsumeButtons.Length; i++)
		{
			ConsumeButton consumeButton = ConsumeButtons[i];
			consumeButton.SetAvailable(availableElements != null && consumeButton.CanConsumeRequiredElements(availableElements));
		}
	}

	public void ToggleHover(bool active, bool isDefaultAbility)
	{
		if (active)
		{
			if (isDefaultAbility)
			{
				defaultActionButton.OnPointerEnter(new PointerEventData(EventSystem.current));
				cardDefActionFrame?.SetState(IFHSelectable.SelectionState.Highlighted);
			}
			else
			{
				actionButton.OnPointerEnter(new PointerEventData(EventSystem.current));
				cardFrame?.SetState(IFHSelectable.SelectionState.Highlighted);
			}
		}
		else if (isDefaultAbility)
		{
			defaultActionButton.OnPointerExit(new PointerEventData(EventSystem.current));
			cardDefActionFrame?.SetState(IFHSelectable.SelectionState.Normal);
		}
		else
		{
			actionButton.OnPointerExit(new PointerEventData(EventSystem.current));
			cardFrame?.SetState(IFHSelectable.SelectionState.Normal);
		}
	}

	public void RefreshInteractable(bool defaultAction)
	{
		if (defaultAction)
		{
			ActiveDefaultAction = isInteractableDefaultAction && (defaultActionInteractability == null || defaultActionInteractability.cachedInteractable);
		}
		else
		{
			ActiveAbilityAction = isInteractable && (actionInteractability == null || actionInteractability.cachedInteractable);
		}
		bool flag = ActiveDefaultAction || ActiveAbilityAction;
		if (canvasGroup != null && !defaultAction)
		{
			canvasGroup.alpha = (ActiveAbilityAction ? 1f : inactiveAlpha);
		}
		if (defCanvasGroup != null && !defaultAction)
		{
			defCanvasGroup.alpha = (ActiveDefaultAction ? 1f : inactiveAlpha);
		}
		if (defaultAction)
		{
			defaultActionButton.interactable = ActiveDefaultAction;
		}
		else
		{
			actionButton.interactable = ActiveAbilityAction;
			InfuseElement[] infuseElements = InfuseElements;
			foreach (InfuseElement infuseElement in infuseElements)
			{
				if (flag)
				{
					infuseElement.Enable();
				}
				else
				{
					infuseElement.Disable();
				}
			}
			foreach (EnhancementButton item in EnhancementButtons.Where((EnhancementButton it) => it.InfuseElement != null))
			{
				if (flag)
				{
					item.InfuseElement.Enable();
				}
				else
				{
					item.InfuseElement.Disable();
				}
			}
		}
		if (isSelected && !flag)
		{
			ToggleSelect(active: false);
		}
		if (isSelectedDefaultAction && !flag)
		{
			ToggleSelect(active: false, isDefaultAction: true);
		}
		RefreshActionStateVisuals(defaultAction);
	}

	public void SetInteractable(bool active, bool defaultAction)
	{
		if (defaultAction)
		{
			isInteractableDefaultAction = active;
		}
		else
		{
			isInteractable = active;
		}
		RefreshInteractable(defaultAction);
	}

	public void SetInPreview(bool inPreview)
	{
		isInPreview = inPreview;
		RefreshInteractable(defaultAction: false);
		RefreshInteractable(defaultAction: true);
		if (isInPreview)
		{
			ConsumeButton[] consumeButtons = ConsumeButtons;
			for (int i = 0; i < consumeButtons.Length; i++)
			{
				consumeButtons[i].ResetState(clearSelection: true, overrideAvailable: true, skipElementCheck: true);
			}
		}
	}

	private void RefreshActionStateVisuals(bool defaultAction)
	{
		if (defaultAction)
		{
			if (defActionBackground != null)
			{
				if (isSelectedDefaultAction)
				{
					defActionBackground.SetState(IFHSelectable.SelectionState.Selected);
				}
				else
				{
					defActionBackground.SetState((!ActiveDefaultAction) ? IFHSelectable.SelectionState.Disabled : IFHSelectable.SelectionState.Normal);
				}
			}
			if (defCanvasGroup != null)
			{
				defCanvasGroup.alpha = ((ActiveDefaultAction || isSelectedDefaultAction || isInPreview) ? 1f : inactiveAlpha);
			}
			return;
		}
		if (actionBackground != null)
		{
			if (isSelected)
			{
				actionBackground.SetState(IFHSelectable.SelectionState.Selected);
			}
			else
			{
				actionBackground.SetState((!ActiveAbilityAction) ? IFHSelectable.SelectionState.Disabled : IFHSelectable.SelectionState.Normal);
			}
		}
		if (canvasGroup != null)
		{
			canvasGroup.alpha = ((ActiveAbilityAction || isSelected || isInPreview) ? 1f : canvasGroup.alpha);
		}
	}

	public void SetInteractable(bool active)
	{
		SetInteractable(active, defaultAction: true);
		SetInteractable(active, defaultAction: false);
	}

	public void ResetInteractable()
	{
		isInteractableDefaultAction = false;
		isInteractable = false;
		isSelectedDefaultAction = false;
		isSelected = false;
		highlightAction?.Hide();
		highlightDefaultAction?.Hide();
	}

	public void ToggleSelect(bool active, bool isDefaultAction = false)
	{
		if (isDefaultAction)
		{
			isSelectedDefaultAction = active;
			if (!active)
			{
				defaultActionButton.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
		else
		{
			isSelected = active;
			if (!active)
			{
				actionButton.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
		RefreshHighlight();
		RefreshActionStateVisuals(isDefaultAction);
	}

	private void RefreshHighlight()
	{
		if (isSelected)
		{
			highlightAction?.ShowSelected();
			highlightDefaultAction?.Hide();
			cardFrame?.SetState(IFHSelectable.SelectionState.Selected);
		}
		else if (isSelectedDefaultAction)
		{
			highlightDefaultAction?.ShowSelected();
			highlightAction?.Hide();
			cardFrame?.SetState(IFHSelectable.SelectionState.Normal);
		}
		else
		{
			highlightAction?.Hide();
			highlightDefaultAction?.Hide();
			cardFrame?.SetState(IFHSelectable.SelectionState.Normal);
		}
	}

	private void Update()
	{
		if (isSelectedDefaultAction)
		{
			defaultActionButton.OnSelect(new BaseEventData(EventSystem.current));
		}
		if (isSelected)
		{
			actionButton.OnSelect(new BaseEventData(EventSystem.current));
		}
	}

	public List<InfuseElement> UnselectedInfusions()
	{
		List<InfuseElement> list = new List<InfuseElement>();
		InfuseElement[] infuseElements = InfuseElements;
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (!infuseElement.IsSelected && !infuseElement.IsEnhancement)
			{
				list.Add(infuseElement);
			}
		}
		return list;
	}

	public List<ElementInfusionBoardManager.EElement> SelectedAnyInfusionElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		InfuseElement[] infuseElements = InfuseElements;
		foreach (InfuseElement infuseElement in infuseElements)
		{
			if (infuseElement.IsAnyElement)
			{
				list.Add(infuseElement.SelectedElement);
			}
		}
		return list;
	}

	public CreateLayout MakeAbilityAction(CardLayoutGroup parentGroup, AbilityCardYMLData card, Vector2 contentPadding, bool isLongRest, CardHalf cardHalf)
	{
		CardHalfAction = cardHalf;
		cardContentTransform.sizeDelta -= contentPadding;
		bool flag = cardHalf == CardHalf.Top;
		CAction action = (flag ? card.TopActionCardData : card.BottomActionCardData);
		InitSummary(action, flag ? card.TopIsLost : card.BottomIsLost, flag ? card.TopIsPermLost : card.BottomIsPermLost, doFit: true);
		if ((parentGroup != null) | isLongRest)
		{
			return new CreateLayout(parentGroup, cardContentTransform.rect, card.ID, isLongRest, inConsume: false, cardContentTransform, isItemCard: false, cardHalf);
		}
		return null;
	}

	public void MakeAbilityActionContinued(CreateLayout newLayout)
	{
		if (!newLayout.IsLongRest)
		{
			UIUtility.ForceRecalclulateLayouts((RectTransform)newLayout.FullLayout.transform);
			newLayout.GenerateFullLayout();
			GameObject fullLayout = newLayout.FullLayout;
			fullLayout.transform.SetParent(cardContentTransform);
			RectTransform obj = fullLayout.transform as RectTransform;
			obj.anchoredPosition = Vector2.zero;
			obj.localScale = new Vector3(1f, 1f, 1f);
		}
		newLayout = null;
	}

	public void SetSkin(AbilityCardUISkin skin, bool topAction, bool longRest)
	{
		this.skin = skin;
		if (skin != null)
		{
			if (longRest)
			{
				actionButton.image.sprite = skin.longRestActionRegularSprite;
				actionButton.spriteState = skin.GetLongRestActionSpriteState();
				return;
			}
			actionButton.image.sprite = (topAction ? skin.topActionRegularSprite : skin.bottomActionRegularSprite);
			actionButton.spriteState = skin.GetActionSpriteState(topAction);
			defaultActionButton.image.sprite = (topAction ? skin.defaultTopActionRegularSprite : skin.defaultBottomActionRegularSprite);
			defaultActionButton.spriteState = skin.GetDefaultActionSpriteState(topAction);
		}
	}

	public void UpdateView(CBaseCard.ECardPile cardPile)
	{
		ConsumeButton[] consumeButtons = ConsumeButtons;
		foreach (ConsumeButton consumeButton in consumeButtons)
		{
			if (isInteractable)
			{
				consumeButton.Enable();
			}
			else
			{
				consumeButton.Disable();
			}
		}
	}

	private CardActionSummary.Params GetActionSummaryParams(CAction actionData, bool isLost, bool isPermaLost, Func<CAbility, bool> abilityFilter = null)
	{
		IEnumerable<CAbility> source = CAbilityCard.GetActionAbilities(actionData);
		if (abilityFilter != null)
		{
			source = source.Where(abilityFilter);
		}
		source = source.ToArray();
		bool hasRoundBonus = source.Any((CAbility a) =>
		{
			AbilityData.ActiveBonusData activeBonusData = a.ActiveBonusData;
			if (activeBonusData != null && activeBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Round)
			{
				AbilityData.ActiveBonusData activeBonusData2 = a.ActiveBonusData;
				if (activeBonusData2 == null)
				{
					return true;
				}
				return !activeBonusData2.Hidden;
			}
			return false;
		});
		bool hasPermBonus = source.Any((CAbility a) =>
		{
			AbilityData.ActiveBonusData activeBonusData = a.ActiveBonusData;
			return (activeBonusData != null && activeBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent) || a is CAbilitySummon;
		});
		return new CardActionSummary.Params(isLost, isPermaLost, hasRoundBonus, hasPermBonus, actionData.ActionXP);
	}

	public void InitSummary(CAction action, bool isLost, bool isPermalost, bool doFit = false)
	{
		ExecuteOnActiveSummaries((CardActionSummary summary) =>
		{
			summary.Init(GetActionSummaryParams(action, isLost, isPermalost, summary.GetFilter()), doFit);
		});
	}

	public void DisplaySelected(bool selected, bool topAction, bool longRest)
	{
		if (skin != null)
		{
			if (longRest)
			{
				actionButton.image.sprite = (selected ? skin.longRestActionHighlightSprite : skin.longRestActionRegularSprite);
			}
			else if (selected)
			{
				actionButton.image.sprite = (topAction ? skin.topActionHighlightSprite : skin.bottomActionHighlightSprite);
				defaultActionButton.image.sprite = (topAction ? skin.defaultTopActionHighlightSprite : skin.defaultBottomActionHighlightSprite);
			}
			else
			{
				actionButton.image.sprite = (topAction ? skin.topActionRegularSprite : skin.bottomActionRegularSprite);
				defaultActionButton.image.sprite = (topAction ? skin.defaultTopActionRegularSprite : skin.defaultBottomActionRegularSprite);
			}
		}
	}

	public void DisableHoverHighlight(bool disable)
	{
		actionButton.transition = ((!disable) ? _actionButtonTransition : Selectable.Transition.None);
		defaultActionButton.transition = ((!disable) ? _defActionButtonTransition : Selectable.Transition.None);
		if (cardFrame != null)
		{
			cardFrame.enabled = !disable;
		}
		if (cardDefActionFrame != null)
		{
			cardDefActionFrame.enabled = !disable;
		}
	}

	public void SetVisualsFromConfig(CharacterConfigUI config)
	{
		ExecuteOnActiveSummaries((CardActionSummary e) =>
		{
			e.SetColor(config.questConfig.color);
		});
	}

	public void SetUIEvents(Action onPointerEnter, Action onPointerExit, Action<bool> onHighlight)
	{
		_onPointerEnter = onPointerEnter;
		_onPointerExit = onPointerExit;
		_onHighlight = onHighlight;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_onPointerEnter?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_onPointerExit?.Invoke();
	}

	public void OnSelect(BaseEventData eventData)
	{
		_onPointerEnter?.Invoke();
		_onHighlight?.Invoke(obj: true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_onPointerExit?.Invoke();
		_onHighlight?.Invoke(obj: false);
	}
}

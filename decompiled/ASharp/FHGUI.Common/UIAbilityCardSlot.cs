using System.Collections.Generic;
using System.Linq;
using FHGUI.Modules;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIAbilityCardSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public class UIAbilityCardSlotContext
	{
		public CPlayerActor PlayerActor;

		public List<CAbilityCard> SelectedCard;

		public ECharacter Fallback;

		public UIScenarioCardInteractionEvents Events;
	}

	[SerializeField]
	private Image _selectedHighlight;

	[SerializeField]
	private bool _isMiniCard = true;

	private UIAbilityCardSlotContext _context;

	private AbilityCardUI _cardUI;

	private CanvasGroup _cardCanvasGroup;

	private RectTransform _tooltipContainer;

	public void Init(CAbilityCard card, UIAbilityCardSlotContext context, RectTransform tooltipContainer = null)
	{
		if (_cardUI?.CardID != card.ID)
		{
			Clear();
		}
		_context = context;
		_tooltipContainer = tooltipContainer;
		if (_cardUI == null)
		{
			_cardUI = ObjectPool.SpawnCard(card.ID, ObjectPool.ECardType.Ability, transform, resetLocalScale: true, resetToMiddle: false, resetLocalRotation: true).GetComponent<AbilityCardUI>();
		}
		CardPileType cardType = CrossReference.ECardPileToCardPileType(card.CurrentCardPile);
		if (context?.PlayerActor != null)
		{
			_cardUI.Init(card, context.PlayerActor, cardType, null, null, null, null, null, null);
		}
		else if (context != null && context.Fallback != ECharacter.None)
		{
			_cardUI.Init(card, _tooltipContainer, CrossReference.ECharacterModelToClassId(context.Fallback), null, null, null, null, 1f, new Vector3(0f, 0f, 0f), 0f);
		}
		else
		{
			_cardUI.Init(card);
		}
		CardHandMode mode = (_isMiniCard ? CardHandMode.PreviewMiniCard : CardHandMode.Preview);
		_cardUI.fullAbilityCard.BlockRaycasts(_isMiniCard);
		_cardUI.SetMode(mode, null);
		_cardUI.SetHoverDetectionActive(active: false);
		_cardUI.transform.localPosition = Vector3.zero;
		_cardCanvasGroup = _cardUI.GetComponent<CanvasGroup>();
		if (_cardCanvasGroup != null)
		{
			_cardCanvasGroup.blocksRaycasts = false;
		}
		if ((bool)_selectedHighlight)
		{
			_selectedHighlight.enabled = context?.SelectedCard != null && context.SelectedCard.Any((CAbilityCard abilityCard) => abilityCard.ID == card.ID);
		}
		VerticalLayoutGroup orAdd = gameObject.GetOrAdd<VerticalLayoutGroup>();
		orAdd.childForceExpandHeight = true;
		orAdd.childForceExpandWidth = true;
	}

	public void Clear()
	{
		_context = null;
		_tooltipContainer = null;
		if (!(_cardUI == null))
		{
			if (_cardCanvasGroup != null)
			{
				_cardCanvasGroup.blocksRaycasts = true;
			}
			ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Ability, _cardUI.gameObject);
			_cardUI = null;
			_cardCanvasGroup = null;
		}
	}

	public bool TryGetCardUi(out AbilityCardUI cardUI)
	{
		cardUI = _cardUI;
		return cardUI != null;
	}

	public void OnClick()
	{
		_context?.Events?.OnClick?.Invoke(_cardUI);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isMiniCard && _cardUI != null)
		{
			_cardUI.ToggleFullCardPreview(isHighlighted: true, _tooltipContainer);
		}
		_context?.Events?.OnHover?.Invoke(_cardUI, isFlagRaised: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_isMiniCard && _cardUI != null)
		{
			_cardUI.ToggleFullCardPreview(isHighlighted: false, _tooltipContainer);
		}
		_context?.Events?.OnHover?.Invoke(_cardUI, isFlagRaised: false);
	}
}

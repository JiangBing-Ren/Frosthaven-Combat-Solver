using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Common;

public class UICompactCardListContainer : ListContainerBase<UICompactAbilityCard, UICardInteractionEvents, CardDTO>
{
	[SerializeField]
	private RectTransform _cardPivot;

	private AbilityCardUI _cardUI;

	private string _hints;

	public UICardInteractionEvents DefaultEvents => new UICardInteractionEvents
	{
		OnHover = OnHover
	};

	public void Init(UICardInteractionEvents events, IList<CardDTO> cards, string hints = null)
	{
		_hints = hints;
		base.Init(events, cards);
	}

	protected override void InitList(UICardInteractionEvents events, IList<CardDTO> listElementParams)
	{
		base.InitList(events, listElementParams);
	}

	private void OnHover(CardDTO target, bool hover)
	{
		if (hover)
		{
			if (_cardUI != null)
			{
				ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Ability, _cardUI.gameObject);
				_cardUI = null;
			}
			_cardUI = ObjectPool.SpawnCard(target.Id, ObjectPool.ECardType.Ability, _cardPivot).GetComponent<AbilityCardUI>();
			_cardUI.transform.localPosition = Vector3.zero;
			_cardUI.transform.localScale = Vector3.one;
			CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == target.Id);
			_cardUI.Init(abilityCard, disableEventDetection: true, target.Enhancements);
			_cardUI.SetHoverDetectionActive(active: false);
			_cardUI.ToggleFullCard(active: true);
			_cardUI.fullAbilityCard.BlockRaycasts(block: true);
			_cardUI.fullAbilityCard.SetInteractable(active: true);
		}
		else if (_cardUI != null)
		{
			ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Ability, _cardUI.gameObject);
			_cardUI = null;
		}
	}

	private void OnDisable()
	{
		if (_cardUI != null)
		{
			ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Ability, _cardUI.gameObject);
			_cardUI = null;
		}
	}

	protected override void InitElement(UICompactAbilityCard cardUI, CardDTO cardDto, UICardInteractionEvents events)
	{
		cardUI.Init(cardDto, events);
	}
}

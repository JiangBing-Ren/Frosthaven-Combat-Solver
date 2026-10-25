using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public abstract class CardListModuleBase : FHMapViewModule
{
	public delegate IEnumerable<UIPartySlotHints.HintedActionParams> CardHintAction(CardDTO cardDto);

	public class Params
	{
		public List<CardDTO> Cards;

		public UICardInteractionEvents InteractionEvents;

		public CardHintAction HintedActions;

		public int MinCount = -1;
	}

	[Header("Base Components")]
	public UIOutpostCardSlot TemplateCardSlot;

	public Transform ContainerCardsSlots;

	[Header("Base Settings")]
	public bool IsMiniCard;

	public bool FitCardInParrent;

	protected List<UIOutpostCardSlot> _slots = new List<UIOutpostCardSlot>();

	protected List<CardDTO> _orderedCards = new List<CardDTO>();

	private UICardInteractionEvents _cardInteractionEvents;

	protected override bool PlaySoundOnEnable => false;

	public Params Data { get; private set; }

	private void InitCache()
	{
		_slots.Clear();
		TemplateCardSlot.gameObject.SetActive(value: false);
	}

	public virtual void Init(Params @params)
	{
		Data = @params;
		_cardInteractionEvents = @params.InteractionEvents;
		if (_slots.Count == 0)
		{
			InitCache();
		}
		_orderedCards = GetOrderedCards().ToList();
		while (_orderedCards.Count < @params.MinCount)
		{
			_orderedCards.Add(null);
		}
		HelperTools.NormalizePool(ref _slots, TemplateCardSlot.gameObject, ContainerCardsSlots, _orderedCards.Count);
		for (int i = 0; i < _orderedCards.Count; i++)
		{
			_slots[i].Init(_orderedCards[i], IsMiniCard, _cardInteractionEvents, FitCardInParrent);
			if (@params.HintedActions != null)
			{
				UpdateInteractionHints(_slots[i].GetComponent<UIPartySlotHints>(), @params.HintedActions(_orderedCards[i]));
			}
		}
		if (_slots.Count > _orderedCards.Count)
		{
			for (int j = _orderedCards.Count; j < _slots.Count; j++)
			{
				_slots[j].Init(null, IsMiniCard, null, FitCardInParrent);
				_slots[j].gameObject.SetActive(value: false);
			}
		}
	}

	public void SelectCard(CardDTO card)
	{
		for (int i = 0; i < _slots.Count; i++)
		{
			UIOutpostCardSlot uIOutpostCardSlot = _slots[i];
			if (uIOutpostCardSlot.gameObject.activeInHierarchy)
			{
				uIOutpostCardSlot.SetSelected(uIOutpostCardSlot.CardDto == card);
			}
		}
	}

	protected void Refresh()
	{
		Init(Data);
	}

	protected virtual IEnumerable<CardDTO> GetOrderedCards()
	{
		return from c in Data.Cards
			orderby c.IsInHand descending, c.IsAltColor, c.Initiative
			select c;
	}

	public void Clear()
	{
		Init(new Params
		{
			Cards = new List<CardDTO>(),
			MinCount = -1
		});
	}

	private void UpdateInteractionHints(UIPartySlotHints component, IEnumerable<UIPartySlotHints.HintedActionParams> actions)
	{
		if ((object)component != null)
		{
			if (actions != null)
			{
				component.Init(actions);
			}
			else
			{
				component.Clear();
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using UnityEngine;

namespace FHGUI.Modules;

public class DeckCardListModule : CardListModuleBase
{
	public enum CategoryFilter
	{
		Name = 1,
		Initiative = 2,
		Level = 4
	}

	[Header("Deck Components")]
	public UIFilterGroup _categoryFilter;

	private bool m_Initialized;

	private void Awake()
	{
		InitCache();
	}

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			UIFilterGroup categoryFilter = _categoryFilter;
			categoryFilter.OnFilter = (Action<Enum>)Delegate.Combine(categoryFilter.OnFilter, new Action<Enum>(SetFilter));
			_categoryFilter.SetFilters(GetFilters());
			_categoryFilter.UpdateState(CategoryFilter.Name);
		}
	}

	protected override IEnumerable<CardDTO> GetOrderedCards()
	{
		InitCache();
		if ((CategoryFilter)(object)_categoryFilter.ActiveFilter == CategoryFilter.Name)
		{
			return from card in Data.Cards
				orderby LocalizationManager.GetTranslation(card.NameLocKey), card.Level
				select card;
		}
		if ((CategoryFilter)(object)_categoryFilter.ActiveFilter == CategoryFilter.Initiative)
		{
			return from card in Data.Cards
				orderby card.Initiative.ToString("{0:00}"), card.Level
				select card;
		}
		if ((CategoryFilter)(object)_categoryFilter.ActiveFilter == CategoryFilter.Level)
		{
			return from card in Data.Cards
				orderby card.Level descending, LocalizationManager.GetTranslation(card.NameLocKey)
				select card;
		}
		return Data.Cards;
	}

	private void SetFilter(Enum newFilter)
	{
		if (!object.Equals(_categoryFilter.ActiveFilter, newFilter))
		{
			_categoryFilter.UpdateState(newFilter);
			Refresh();
		}
	}

	private UIFilterGroup.FilterParam[] GetFilters()
	{
		return new UIFilterGroup.FilterParam[3]
		{
			new UIFilterGroup.FilterParam
			{
				TextKey = "GUI_ABILITY_CARDS_SORT_BY_NAME",
				TooltipKey = "GUI_ABILITY_CARDS_SORT_BY_NAME_DESC",
				Filter = CategoryFilter.Name
			},
			new UIFilterGroup.FilterParam
			{
				TextKey = "GUI_ABILITY_CARDS_SORT_BY_INITIATIVE",
				TooltipKey = "GUI_ABILITY_CARDS_SORT_BY_INITIATIVE_DESC",
				Filter = CategoryFilter.Initiative
			},
			new UIFilterGroup.FilterParam
			{
				TextKey = "GUI_ABILITY_CARDS_SORT_BY_LEVEL",
				TooltipKey = "GUI_ABILITY_CARDS_SORT_BY_LEVEL_DESC",
				Filter = CategoryFilter.Level
			}
		};
	}
}

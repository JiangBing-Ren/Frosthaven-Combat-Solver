using System;
using System.Collections.Generic;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class ShopViewModule : FHMapViewModule
{
	public struct Params
	{
		public IEnumerable<ItemDataDTO> Items;

		public FHItemTooltip.ItemTooltipParamsFunc TooltipData;

		public UIItemElementInteractionEvents ElementInteractionEvents;

		public Action<CategoryFilter> OnFilterStateUpdated;

		public CategoryFilter ActiveFilters;

		public int EnsureMinimumVisibleSlots;

		public bool FillFreeSpace;

		public bool DisableFilters;

		public string CaptionKey;

		public bool PreserveDefaultSelectionBehavior;

		public bool HideEmptySlots;

		public Color HeaderColor;

		public Sprite CategorySprite;

		public Sprite Illustration;

		public RangedValueDTO Charges;

		public bool ShowLimitReached;

		public string LimitDescriptionKey;

		public bool DontShowCharges;
	}

	[SerializeField]
	private TextLocalizedListener m_Caption;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private Image m_CategoryIcon;

	[SerializeField]
	private UIFilteredItemContainerElement _itemContainer;

	[SerializeField]
	private Image m_Header;

	[SerializeField]
	private Image m_HeaderOrnament;

	[SerializeField]
	private GameObject m_LimitReached;

	[SerializeField]
	private GameObject m_PurchaseGroup;

	[SerializeField]
	private FHStacksCounter m_PurchaseCharges;

	[SerializeField]
	private TextLocalizedListener m_LimitLabel;

	public void Init(Params @params)
	{
		UIItemContainerElement.Params setupData = new UIItemContainerElement.Params
		{
			Items = @params.Items,
			TooltipFunc = @params.TooltipData,
			ElementInteractionEvents = @params.ElementInteractionEvents,
			EnsureMinimumVisibleSlots = @params.EnsureMinimumVisibleSlots,
			FillFreeSpace = @params.FillFreeSpace,
			PreserveDefaultSelectionBehavior = @params.PreserveDefaultSelectionBehavior,
			HideEmpty = @params.HideEmptySlots
		};
		_itemContainer.Init(setupData);
		_itemContainer.InitFilters(@params.ActiveFilters, @params.OnFilterStateUpdated, @params.DisableFilters);
		if ((bool)m_Caption)
		{
			m_Caption.SetTextKey(@params.CaptionKey ?? "GUI_MERCHANT");
		}
		if ((bool)m_Header && @params.HeaderColor != default(Color))
		{
			m_Header.color = @params.HeaderColor;
		}
		if ((bool)m_HeaderOrnament && @params.HeaderColor != default(Color))
		{
			m_HeaderOrnament.color = @params.HeaderColor;
		}
		if (m_Illustration != null)
		{
			m_Illustration.overrideSprite = @params.Illustration;
		}
		if (m_CategoryIcon != null)
		{
			m_CategoryIcon.overrideSprite = @params.CategorySprite;
		}
		if (m_LimitReached != null)
		{
			m_LimitReached.gameObject.SetActive(@params.ShowLimitReached);
		}
		bool flag = @params.Charges.Max > 0 && !@params.DontShowCharges;
		if (m_PurchaseCharges != null)
		{
			m_PurchaseCharges.Init(@params.Charges.Value, @params.Charges.Max);
			m_PurchaseCharges.gameObject.SetActive(flag);
		}
		if (m_LimitLabel != null)
		{
			m_LimitLabel.ClearText();
			m_LimitLabel.SetTextKey(@params.LimitDescriptionKey);
		}
		if (m_PurchaseGroup != null)
		{
			m_PurchaseGroup.gameObject.SetActive(flag || @params.LimitDescriptionKey.IsNOTNullOrEmpty());
		}
	}

	public void SelectItem(ItemID itemItemID)
	{
		_itemContainer.SelectItem(itemItemID);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIFilteredItemContainerElement : UIItemContainerElement
{
	[SerializeField]
	private UIFilterGroup _categoryFilter;

	[SerializeField]
	private GameObject _categoryVisuals;

	[SerializeField]
	private bool hideFilterIfNotOverflowing = true;

	private Action<CategoryFilter> _onFilterStateUpdated;

	private bool _disableFilters;

	private bool _invalidateFilter;

	protected override void Setup()
	{
		if (!_setup)
		{
			base.Setup();
			UIFilterGroup categoryFilter = _categoryFilter;
			categoryFilter.OnFilter = (Action<Enum>)Delegate.Combine(categoryFilter.OnFilter, new Action<Enum>(SetFilter));
			UIFilterGroup categoryFilter2 = _categoryFilter;
			categoryFilter2.OnStateUpdated = (Action<Enum>)Delegate.Combine(categoryFilter2.OnStateUpdated, new Action<Enum>(CallOnStateUpdated));
			_categoryFilter.SetFilters(GetInventoryFilterParams());
			_categoryFilter.UpdateState(CategoryFilter.All);
		}
	}

	public override void Init(Params setupData)
	{
		base.Init(setupData);
		_invalidateFilter = true;
	}

	public void InitFilters(CategoryFilter filters, Action<CategoryFilter> onFilterStateUpdated, bool disableFilters)
	{
		_onFilterStateUpdated = null;
		SetFilters(filters);
		DisableFilters(disableFilters);
		_onFilterStateUpdated = onFilterStateUpdated;
	}

	public void ResetFilters()
	{
		SetFilters(CategoryFilter.All);
	}

	public void SetFilters(CategoryFilter filters)
	{
		_categoryFilter.UpdateState(filters);
	}

	private void UpdateFilter(bool showActiveFilter = false)
	{
		_categoryFilter.gameObject.SetActive(showActiveFilter);
		if (_categoryVisuals != null)
		{
			_categoryVisuals.SetActive(showActiveFilter);
		}
		CategoryFilter categoryFilter = (showActiveFilter ? ((CategoryFilter)(object)_categoryFilter.ActiveFilter) : CategoryFilter.All);
		List<ItemDataDTO> source = FilterItems(categoryFilter, _items);
		TooltipPositionParams value = new TooltipPositionParams
		{
			TransformAnchor = _tooltipPos,
			Anchor = _tooltipAnchor
		};
		UIOutpostItemSectionElement[] sections = Sections;
		foreach (UIOutpostItemSectionElement section in sections)
		{
			section.Init(source.Where((ItemDataDTO item) => item.ItemType == section.ItemType), _elementInteractionEvents, hideEmpty: _hideEmpty, ensureMinimumVisibleSlots: _ensureMinimumVisibleSlots, tooltipParamsFunc: _tooltipFunc, tooltipPositionParams: value);
		}
		sections = Sections;
		foreach (UIOutpostItemSectionElement uIOutpostItemSectionElement in sections)
		{
			bool showCategoryLabel;
			bool active = (showCategoryLabel = uIOutpostItemSectionElement.ItemCount > 0) && (categoryFilter == CategoryFilter.Purchasable || _categoryFilter.ActiveFilter.HasFlag(ItemTypeToFilter(uIOutpostItemSectionElement.ItemType)));
			uIOutpostItemSectionElement.gameObject.SetActive(active);
			uIOutpostItemSectionElement.ShowCategoryLabel = showCategoryLabel;
		}
		RefreshSelection();
		UpdateHints();
	}

	private void SetFilter(Enum newFilter)
	{
		if (!object.Equals(_categoryFilter.ActiveFilter, newFilter))
		{
			_categoryFilter.UpdateState(newFilter);
			_invalidateFilter = true;
		}
	}

	public void DisableFilters(bool disableFilters)
	{
		_disableFilters = disableFilters;
		_invalidateFilter = true;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_invalidateFilter && !CanvasUpdateRegistry.IsRebuildingLayout())
		{
			UpdateFilter(CanShowFilter());
			_invalidateFilter = false;
			_invalidateFreeSpace = true;
		}
	}

	private void CallOnStateUpdated(Enum activeFilter)
	{
		_onFilterStateUpdated?.Invoke((CategoryFilter)(object)activeFilter);
	}

	public static UIFilterGroup.FilterParam[] GetInventoryFilterParams()
	{
		return new UIFilterGroup.FilterParam[8]
		{
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_UI_ICO_ItemSlot_Any"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_UI_ICO_ItemSlot_Any_ON"),
				TextKey = "GUI_INVENTORY_SLOT_ALL",
				TooltipKey = "GUI_INVENTORY_SLOT_ALL",
				Filter = CategoryFilter.All
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Available_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Available_ON"),
				TooltipKey = "GUI_INVENTORY_AVAILABLE",
				Filter = CategoryFilter.Purchasable
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Head_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Head_ON"),
				TextKey = "GUI_ITEM_SLOT_HEAD",
				TooltipKey = "GUI_ITEM_SLOT_HEAD",
				Filter = CategoryFilter.Head
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Body_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Body_ON"),
				TextKey = "GUI_ITEM_SLOT_BODY",
				TooltipKey = "GUI_ITEM_SLOT_BODY",
				Filter = CategoryFilter.Body
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Feet_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Feet_ON"),
				TextKey = "GUI_ITEM_SLOT_LEGS",
				TooltipKey = "GUI_ITEM_SLOT_LEGS",
				Filter = CategoryFilter.Feet
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_OneHandWeapon_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_OneHandWeapon_ON"),
				TextKey = "GUI_ITEM_SLOT_ONEHAND",
				TooltipKey = "GUI_ITEM_SLOT_ONEHAND",
				Filter = CategoryFilter.OneHandWeapon
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_TwoHandWeapon_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_TwoHandWeapon_ON"),
				TextKey = "GUI_INVENTORY_SLOT_TwoHandItem",
				TooltipKey = "GUI_INVENTORY_SLOT_TwoHandItem",
				Filter = CategoryFilter.TwoHandWeapon
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = ItemDTOUtilities.GetItemTypeImage(ItemEquipType.Small),
				Icon = ItemDTOUtilities.GetItemTypeImageTintable(ItemEquipType.Small),
				TextKey = "GUI_ITEM_SLOT_SMALLITEM",
				TooltipKey = "GUI_ITEM_SLOT_SMALLITEM",
				Filter = CategoryFilter.Small
			}
		};
	}

	private bool CanShowFilter()
	{
		bool flag = (CategoryFilter)(object)_categoryFilter.ActiveFilter == CategoryFilter.All;
		if (!_disableFilters)
		{
			return !((hideFilterIfNotOverflowing && CanFitInContainerUnfiltered()) & flag);
		}
		return false;
	}

	protected bool CanFitInContainerUnfiltered()
	{
		if ((object)_scrollRect == null)
		{
			return true;
		}
		if (_scrollRect.content.GetComponent<LayoutGroup>() is VerticalLayoutGroup vertical)
		{
			return calcVertical(vertical);
		}
		return false;
		bool calcVertical(VerticalLayoutGroup verticalLayoutGroup)
		{
			float num = (float)_items.Select((ItemDataDTO dto) => dto.ItemType).Distinct().Count() * Sections[0].GetHeaderHeight();
			float num2 = _items.Count * Sections[0].GetItemHeight();
			return ((RectTransform)_scrollRect.transform).rect.height - (float)(verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom) >= num + num2;
		}
	}

	public static List<ItemDataDTO> FilterItems(CategoryFilter filter, List<ItemDataDTO> items)
	{
		List<ItemDataDTO> list = new List<ItemDataDTO>();
		foreach (ItemDataDTO item in items)
		{
			if (filter == CategoryFilter.Purchasable && item.CanAfford)
			{
				list.Add(item);
			}
			else if (filter.HasFlag(ItemTypeToFilter(item.ItemType)))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static CategoryFilter ItemTypeToFilter(ItemEquipType itemType)
	{
		return itemType switch
		{
			ItemEquipType.Small => CategoryFilter.Small, 
			ItemEquipType.Head => CategoryFilter.Head, 
			ItemEquipType.Body => CategoryFilter.Body, 
			ItemEquipType.Feet => CategoryFilter.Feet, 
			ItemEquipType.OneHandWeapon => CategoryFilter.OneHandWeapon, 
			ItemEquipType.TwoHandWeapon => CategoryFilter.TwoHandWeapon, 
			_ => CategoryFilter.None, 
		};
	}
}

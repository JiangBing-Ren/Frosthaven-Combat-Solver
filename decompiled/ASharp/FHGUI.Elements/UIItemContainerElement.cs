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

public class UIItemContainerElement : MonoBehaviour
{
	public struct Params
	{
		public IEnumerable<ItemDataDTO> Items;

		public FHItemTooltip.ItemTooltipParamsFunc TooltipFunc;

		public UIItemElementInteractionEvents ElementInteractionEvents;

		public int EnsureMinimumVisibleSlots;

		public bool FillFreeSpace;

		public bool HideEmpty;

		public bool PreserveDefaultSelectionBehavior;
	}

	[SerializeField]
	protected RectTransform _itemContainer;

	[SerializeField]
	private UIOutpostItemElement _emptySlotPrototype;

	[SerializeField]
	protected RectTransform _tooltipPos;

	[SerializeField]
	protected TextAnchor _tooltipAnchor;

	[SerializeField]
	private TextLocalizedListener _emptyContainerLabel;

	private UIOutpostItemSectionElement[] _sections;

	protected ScrollRect _scrollRect;

	protected bool _hideEmpty;

	protected int _ensureMinimumVisibleSlots;

	protected UIItemElementInteractionEvents _elementInteractionEvents;

	protected FHItemTooltip.ItemTooltipParamsFunc _tooltipFunc;

	protected List<ItemDataDTO> _items = new List<ItemDataDTO>();

	private List<UIOutpostItemElement> _emptySlots = new List<UIOutpostItemElement>();

	private ItemID? _selectedItemID;

	protected bool _fillFreeSpace;

	protected bool _preserveTooltipIfSelected;

	protected bool _invalidateFreeSpace;

	protected bool _setup;

	public UIOutpostItemElement SelectedItem { get; private set; }

	public UIOutpostItemSectionElement[] Sections
	{
		get
		{
			Setup();
			return _sections;
		}
	}

	protected virtual void Setup()
	{
		if (!_setup)
		{
			_sections = _itemContainer.GetComponentsInChildren<UIOutpostItemSectionElement>();
			_scrollRect = GetComponentInChildren<ScrollRect>();
			if (_scrollRect != null)
			{
				_scrollRect.verticalNormalizedPosition = 0f;
			}
			_setup = true;
		}
	}

	public virtual void Init(Params setupData)
	{
		Setup();
		_items.Clear();
		_items.AddRange(setupData.Items);
		_hideEmpty = setupData.HideEmpty;
		_tooltipFunc = setupData.TooltipFunc;
		if (_elementInteractionEvents != null)
		{
			UIItemElementInteractionEvents elementInteractionEvents = _elementInteractionEvents;
			elementInteractionEvents.OnClick = (UIInteractionEvents<UIOutpostItemElement>.OnUIEvent)Delegate.Remove(elementInteractionEvents.OnClick, new UIInteractionEvents<UIOutpostItemElement>.OnUIEvent(HandleOnItemSelect));
		}
		_elementInteractionEvents = setupData.ElementInteractionEvents ?? new UIItemElementInteractionEvents();
		if (setupData.PreserveDefaultSelectionBehavior)
		{
			UIItemElementInteractionEvents elementInteractionEvents2 = _elementInteractionEvents;
			elementInteractionEvents2.OnClick = (UIInteractionEvents<UIOutpostItemElement>.OnUIEvent)Delegate.Combine(elementInteractionEvents2.OnClick, new UIInteractionEvents<UIOutpostItemElement>.OnUIEvent(HandleOnItemSelect));
		}
		UIItemElementInteractionEvents elementInteractionEvents3 = _elementInteractionEvents;
		if (elementInteractionEvents3.OnHover == null)
		{
			elementInteractionEvents3.OnHover = HandleOnItemHover;
		}
		_ensureMinimumVisibleSlots = setupData.EnsureMinimumVisibleSlots;
		_fillFreeSpace = setupData.FillFreeSpace;
		TooltipPositionParams value = new TooltipPositionParams
		{
			Pos = _tooltipPos.position,
			Anchor = _tooltipAnchor
		};
		UIOutpostItemSectionElement[] sections = Sections;
		foreach (UIOutpostItemSectionElement section in sections)
		{
			section.Init(_items.Where((ItemDataDTO item) => item.ItemType == section.ItemType), _elementInteractionEvents, _tooltipFunc, value, _hideEmpty, _ensureMinimumVisibleSlots);
		}
		UpdateHints();
		ClearSelection();
		_invalidateFreeSpace = true;
	}

	private UIOutpostItemElement FindItemElementById(ItemID itemID)
	{
		UIOutpostItemSectionElement[] sections = _sections;
		for (int i = 0; i < sections.Length; i++)
		{
			foreach (UIOutpostItemElement item in sections[i].Items)
			{
				if (item.ItemData != null && item.ItemData.ID == itemID)
				{
					return item;
				}
			}
		}
		return null;
	}

	public void SelectItem(ItemID? itemID)
	{
		_selectedItemID = itemID;
		if (itemID.HasValue)
		{
			UIOutpostItemElement uIOutpostItemElement = FindItemElementById(itemID.Value);
			if ((object)uIOutpostItemElement != null)
			{
				SelectItem(uIOutpostItemElement);
			}
		}
	}

	public void SelectItem(UIOutpostItemElement itemElement)
	{
		if ((bool)SelectedItem)
		{
			SelectedItem.Deselect();
		}
		SelectedItem = itemElement;
		SelectedItem.Select();
	}

	public void ClearSelection()
	{
		if ((bool)SelectedItem)
		{
			SelectedItem.Deselect();
		}
		SelectedItem = null;
		_selectedItemID = null;
	}

	public void RefreshSelection()
	{
		SelectItem(_selectedItemID);
	}

	private void FillFreeSpaceWithEmptySlots()
	{
		for (int i = 0; i < _emptySlots.Count; i++)
		{
			_emptySlots[i].gameObject.SetActive(value: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(_itemContainer);
		int count = CalcNeededEmptySlots();
		if (_fillFreeSpace)
		{
			UIHelperUtil.UpdateUIList(count, _emptySlots, _emptySlotPrototype, _itemContainer, setItemData);
		}
		static void setItemData(int num, UIOutpostItemElement item)
		{
			item.SetEmpty();
		}
	}

	private int CalcNeededEmptySlots()
	{
		if ((object)_emptySlotPrototype == null)
		{
			return 0;
		}
		if ((object)_scrollRect == null)
		{
			return 0;
		}
		LayoutGroup component = _scrollRect.content.GetComponent<LayoutGroup>();
		if (!(component is HorizontalLayoutGroup horizontal))
		{
			if (!(component is VerticalLayoutGroup vertical))
			{
				if (component is GridLayoutGroup grid)
				{
					return calcGrid(grid);
				}
				return 0;
			}
			return calcVertical(vertical);
		}
		return calcHorizontal(horizontal);
		static int calcGrid(GridLayoutGroup gridLayoutGroup)
		{
			return 0;
		}
		int calcHorizontal(HorizontalLayoutGroup horizontalLayoutGroup)
		{
			float spacing = horizontalLayoutGroup.spacing;
			float num = (_emptySlotPrototype.transform as RectTransform).sizeDelta.x + spacing;
			float num2 = (_scrollRect.transform as RectTransform).rect.width - _itemContainer.rect.height;
			num2 -= (float)(horizontalLayoutGroup.padding.left + horizontalLayoutGroup.padding.right) + spacing;
			return Mathf.CeilToInt((num2 >= num) ? (num2 / num) : 0f) + 1;
		}
		int calcVertical(VerticalLayoutGroup verticalLayoutGroup)
		{
			float spacing = verticalLayoutGroup.spacing;
			float num = (_emptySlotPrototype.transform as RectTransform).sizeDelta.y + spacing;
			float num2 = (_scrollRect.transform as RectTransform).rect.height - _itemContainer.rect.height;
			num2 -= (float)(verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom) + spacing;
			return Mathf.CeilToInt((num2 >= num) ? (num2 / num) : 0f) + 1;
		}
	}

	protected void UpdateHints()
	{
		if (!(_emptyContainerLabel != null))
		{
			return;
		}
		bool active = true;
		UIOutpostItemSectionElement[] sections = Sections;
		for (int i = 0; i < sections.Length; i++)
		{
			if (sections[i].ItemCount > 0)
			{
				active = false;
				break;
			}
		}
		_emptyContainerLabel.gameObject.SetActive(active);
	}

	protected virtual void LateUpdate()
	{
		if (_invalidateFreeSpace)
		{
			_invalidateFreeSpace = false;
			FillFreeSpaceWithEmptySlots();
		}
	}

	public int GetItemsCount()
	{
		return _items.Count;
	}

	private void OnDisable()
	{
		_tooltipFunc = null;
		_elementInteractionEvents = null;
		SelectedItem = null;
		HideTooltip(immediate: true);
	}

	protected void HandleOnItemSelect(UIOutpostItemElement itemElement)
	{
		SelectItem(itemElement);
	}

	private void HideTooltip(bool immediate = false)
	{
	}

	private void ShowTooltip(ItemDataDTO itemData)
	{
	}

	protected void HandleOnItemHover(UIOutpostItemElement itemElement, bool hover)
	{
		if (!_preserveTooltipIfSelected || (itemElement.ItemData != null && !(SelectedItem == itemElement)))
		{
			if (hover)
			{
				ShowTooltip(itemElement.ItemData);
			}
			else
			{
				HideTooltip();
			}
		}
	}
}

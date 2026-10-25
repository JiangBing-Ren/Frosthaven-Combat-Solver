using System;
using System.Collections.Generic;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHRL;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIOutpostItemSectionElement : MonoBehaviour
{
	[SerializeField]
	private ItemEquipType _itemType;

	[SerializeField]
	private UIOutpostItemElement _itemPrefab;

	[SerializeField]
	private RectTransform _itemContainer;

	[SerializeField]
	private TMP_Text _totalItems;

	[SerializeField]
	private RectTransform _categoryContainer;

	[SerializeField]
	private TMP_Text _categoryLabel;

	[SerializeField]
	private Image _categoryIcon;

	private List<UIOutpostItemElement> _items = new List<UIOutpostItemElement>(12);

	private List<ItemDataDTO> _itemsData = new List<ItemDataDTO>(12);

	private bool _showCategoryLabels;

	private bool _setup;

	private int _maxUIItemsPerSection = 1;

	public ItemEquipType ItemType => _itemType;

	public IReadOnlyList<UIOutpostItemElement> Items => _items.AsReadOnly();

	public int ItemCount => _itemsData.Count;

	public bool ShowCategoryLabel
	{
		get
		{
			return _showCategoryLabels;
		}
		set
		{
			_showCategoryLabels = value;
			UpdateLabelVisibility();
		}
	}

	private void Awake()
	{
		if (!_setup)
		{
			Setup();
		}
	}

	private void Setup()
	{
		_items.AddRange(_itemContainer.GetComponentsInChildren<UIOutpostItemElement>());
		_maxUIItemsPerSection = 0;
		if ((object)_itemContainer != null)
		{
			GridLayoutGroup component = _itemContainer.GetComponent<GridLayoutGroup>();
			if ((object)component != null && component.constraint != GridLayoutGroup.Constraint.Flexible)
			{
				_maxUIItemsPerSection = component.constraintCount;
			}
		}
		if ((object)_categoryLabel != null)
		{
			string categoryLocalizationKey = GetCategoryLocalizationKey(_itemType);
			Localize component2 = _categoryLabel.GetComponent<Localize>();
			if ((object)component2 != null)
			{
				component2.SetTerm(categoryLocalizationKey);
			}
			else
			{
				_categoryLabel.text = LocalizationManager.GetTermTranslation(categoryLocalizationKey);
			}
		}
		if ((object)_categoryIcon != null)
		{
			_categoryIcon.sprite = ItemDTOUtilities.GetItemTypeImage(_itemType);
		}
		_setup = true;
	}

	public void Init(IEnumerable<ItemDataDTO> items, UIItemElementInteractionEvents elementInteractionEvents = null, FHItemTooltip.ItemTooltipParamsFunc tooltipParamsFunc = null, TooltipPositionParams? tooltipPositionParams = null, bool hideEmpty = true, int ensureMinimumVisibleSlots = 0)
	{
		if (!_setup)
		{
			Setup();
		}
		_itemsData.Clear();
		_itemsData.AddRange(items);
		_itemsData.Sort((ItemDataDTO x, ItemDataDTO y) => x.ID.CompareTo(y.ID));
		gameObject.SetActive(!hideEmpty || _itemsData.Count > 0);
		if (!hideEmpty || _itemsData.Count != 0)
		{
			int minItemCount = GetMinItemCount(ensureMinimumVisibleSlots);
			minItemCount = Mathf.Max(ensureMinimumVisibleSlots, minItemCount);
			UIHelperUtil.UpdateUIList(minItemCount, _items, _itemPrefab, _itemContainer, setItemData);
		}
		void setItemData(int i, UIOutpostItemElement item)
		{
			if (i < _itemsData.Count)
			{
				item.SetItem(_itemsData[i], elementInteractionEvents, tooltipParamsFunc?.Invoke(_itemsData[i]), tooltipPositionParams);
			}
			else
			{
				item.SetEmpty(elementInteractionEvents, _itemType);
			}
		}
	}

	private void UpdateLabelVisibility()
	{
		if (_categoryContainer != null)
		{
			_categoryContainer.gameObject.SetActive(_showCategoryLabels);
		}
	}

	private int GetMinItemCount(int ensureMinimum)
	{
		if (_maxUIItemsPerSection == 0)
		{
			return _itemsData.Count;
		}
		return Mathf.Max(Mathf.CeilToInt(_itemsData.Count / _maxUIItemsPerSection), 1) * _maxUIItemsPerSection;
	}

	private string GetCategoryLocalizationKey(ItemEquipType cat)
	{
		return cat switch
		{
			ItemEquipType.Head => "GUI_INVENTORY_SLOT_Head", 
			ItemEquipType.Body => "GUI_INVENTORY_SLOT_Body", 
			ItemEquipType.Feet => "GUI_INVENTORY_SLOT_Legs", 
			ItemEquipType.OneHandWeapon => "GUI_INVENTORY_SLOT_OneHandItem", 
			ItemEquipType.TwoHandWeapon => "GUI_INVENTORY_SLOT_TwoHandItem", 
			ItemEquipType.Small => "GUI_INVENTORY_SLOT_SmallItems", 
			_ => throw new ArgumentOutOfRangeException("cat", cat, null), 
		};
	}

	public float GetHeaderHeight()
	{
		return 64f;
	}

	public int GetItemHeight()
	{
		return 68;
	}
}

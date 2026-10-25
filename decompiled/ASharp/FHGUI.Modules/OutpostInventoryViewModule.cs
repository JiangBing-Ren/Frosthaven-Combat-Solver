using System;
using System.Collections.Generic;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Tweening;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class OutpostInventoryViewModule : FHMapViewModule
{
	public struct Params
	{
		public IEnumerable<ItemDataDTO> Items;

		public FHItemTooltip.ItemTooltipParamsFunc TooltipParams;

		public UIItemElementInteractionEvents ElementInteractionEvents;

		public Action<CategoryFilter> OnFilterStateUpdated;

		public CategoryFilter ActiveFilters;

		public int EnsureMinimumVisibleSlots;

		public bool FillFreeSpace;

		public bool DisableFilters;

		public Color TintColor;

		public Sprite Icon;

		public bool ShowResourceBar;

		public bool HideZeroResources;

		public OutpostResources Resources;

		public uint Gold;

		public bool ShowBonusStartingGoldHint;
	}

	[Header("Components")]
	[SerializeField]
	private ResourcesBarElement _resourceBar;

	[SerializeField]
	private GameObject _resourceGroup;

	[SerializeField]
	private UIFilteredItemContainerElement _itemContainer;

	[SerializeField]
	private GameObject _classContainer;

	[SerializeField]
	private Image _classColor;

	[SerializeField]
	private Image _classIcon;

	[SerializeField]
	private Image _classHeaderColor;

	[SerializeField]
	private GameObject _bonusStartingGoldHint;

	private RectTransform _rectTransform;

	private Vector2 _originalAnchoredPosition;

	protected override bool PlaySoundOnEnable => false;

	public UIOutpostItemElement SelectedItem => _itemContainer.SelectedItem;

	private void Awake()
	{
		_rectTransform = transform as RectTransform;
		_originalAnchoredPosition = _rectTransform.anchoredPosition;
	}

	public void Init(Params @params)
	{
		UIItemContainerElement.Params setupData = new UIItemContainerElement.Params
		{
			Items = @params.Items,
			TooltipFunc = @params.TooltipParams,
			ElementInteractionEvents = @params.ElementInteractionEvents,
			EnsureMinimumVisibleSlots = @params.EnsureMinimumVisibleSlots,
			FillFreeSpace = @params.FillFreeSpace,
			HideEmpty = false
		};
		_itemContainer.Init(setupData);
		_itemContainer.InitFilters(@params.ActiveFilters, @params.OnFilterStateUpdated, @params.DisableFilters);
		if (_resourceBar != null)
		{
			_resourceBar.gameObject.SetActive(@params.ShowResourceBar);
			if (@params.ShowResourceBar)
			{
				_resourceBar.Init((int)@params.Gold, @params.Resources, @params.HideZeroResources);
			}
		}
		if (_resourceGroup != null)
		{
			_resourceGroup.SetActive(@params.ShowResourceBar);
		}
		if ((bool)_classColor)
		{
			_classColor.color = @params.TintColor;
		}
		if ((bool)_classIcon)
		{
			_classIcon.sprite = @params.Icon;
		}
		if ((bool)_classHeaderColor)
		{
			_classHeaderColor.color = @params.TintColor;
		}
		if ((bool)_classContainer)
		{
			_classContainer.gameObject.SetActive(@params.Icon != null);
		}
		if ((bool)_bonusStartingGoldHint)
		{
			_bonusStartingGoldHint.SetActive(@params.ShowBonusStartingGoldHint);
		}
	}

	public void SelectItem(ItemID itemItemID)
	{
		_itemContainer.SelectItem(itemItemID);
	}

	public void ClearSelection()
	{
		_itemContainer.ClearSelection();
	}

	protected override void OnHide()
	{
		base.OnHide();
		SetOffset(Vector2.zero);
	}

	public void SetOffset(Vector2 offset)
	{
		if (!(_rectTransform == null))
		{
			_rectTransform.anchoredPosition = _originalAnchoredPosition + offset;
			UITweenEffects.UpdateInitialAnchoredPosition(_rectTransform);
		}
	}
}

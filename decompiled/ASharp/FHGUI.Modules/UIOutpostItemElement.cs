using System.Collections;
using FHGUI.Common;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIOutpostItemElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IGeneralInfoProvider
{
	[Header("Elements")]
	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _iconBackground;

	[SerializeField]
	private GameObject _iconContainer;

	[SerializeField]
	private Image _categoryIcon;

	[SerializeField]
	private GameObject _outline;

	[SerializeField]
	private Image _unavailable;

	[SerializeField]
	private Image _separator;

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _quantity;

	[SerializeField]
	private GameObject _quantityContainer;

	[SerializeField]
	private GameObject _equppiedIndicator;

	[SerializeField]
	private GameObject _newIndicator;

	[SerializeField]
	private GameObject _ownedIndicator;

	[SerializeField]
	private GameObject _forbiddenIndicator;

	[SerializeField]
	private GameObject _costContainer;

	[SerializeField]
	private TMP_Text _cost;

	[SerializeField]
	private UIOutpostResourcesCost _resourcesCost;

	[Header("Colors")]
	[SerializeField]
	private Color _unavailableColor = Color.red;

	[SerializeField]
	private Color _availableColor = Color.white;

	[SerializeField]
	private Color _cannotBuyColor = Color.red;

	[SerializeField]
	private Color _unavailableNameColor = Color.red;

	[SerializeField]
	private Color _availableNameColor = Color.white;

	[SerializeField]
	private Color _cannotBuyNameColor = Color.red;

	[Header("Sprites")]
	[SerializeField]
	private Sprite _emptySlotSprite;

	[SerializeField]
	private Sprite _populatedSlotSprite;

	[SerializeField]
	private Sprite _selectedSlotSprite;

	[SerializeField]
	private Sprite _emptyIconSlotSprite;

	[SerializeField]
	private Sprite _populatedIconSlotSprite;

	[SerializeField]
	private Sprite _selectedIconSlotSprite;

	private bool _isSelected;

	private bool _isHighlighted;

	private UIItemElementInteractionEvents _elementInteractionEvents;

	private Coroutine _onClickCrt;

	[field: SerializeField]
	public UIItemTooltipTarget Tooltip { get; private set; }

	public ItemDataDTO ItemData { get; private set; }

	public ItemID ItemID => ItemData.ID;

	public bool IsEmpty => ItemData == null;

	public void SetItem(ItemDataDTO itemData, UIItemElementInteractionEvents elementInteractionEvents = null, FHItemTooltip.ItemTooltipParams tooltipParams = null, TooltipPositionParams? tooltipPosParams = null)
	{
		if (itemData == null)
		{
			SetEmpty();
			return;
		}
		ResetState();
		_elementInteractionEvents = elementInteractionEvents;
		ItemData = itemData;
		bool flag = itemData.Quantity != 0;
		if (_background != null)
		{
			_background.sprite = _populatedSlotSprite;
		}
		if (_iconBackground != null)
		{
			_iconBackground.sprite = _populatedIconSlotSprite;
		}
		if (_iconContainer != null)
		{
			_iconContainer.SetActive(value: true);
		}
		if (_name != null)
		{
			_name.enabled = !string.IsNullOrWhiteSpace(itemData.NameLocalized);
			_name.text = itemData.NameLocalized;
			_name.color = (flag ? _availableNameColor : _unavailableNameColor);
			if (itemData.ShowCost)
			{
				bool flag2 = itemData.ResourceCost?.CanAfford ?? itemData.CanAfford;
				_name.color = (flag2 ? _name.color : _cannotBuyNameColor);
			}
		}
		if (_categoryIcon != null)
		{
			_categoryIcon.enabled = false;
		}
		if (_equppiedIndicator != null)
		{
			_equppiedIndicator.SetActive(itemData.IsEquipped);
		}
		if (_separator != null)
		{
			_separator.enabled = true;
		}
		if (_ownedIndicator != null)
		{
			_ownedIndicator.SetActive(itemData.OwnedInstance);
		}
		if (_forbiddenIndicator != null)
		{
			_forbiddenIndicator.SetActive(itemData.IsForbidden);
		}
		if (_icon != null)
		{
			bool newEnabledState = (object)itemData.Icon != null;
			UIHelperUtil.SetSpriteAndTweenIt(_icon, itemData.Icon, newEnabledState);
		}
		if ((bool)_quantity)
		{
			_quantity.enabled = itemData.MaxQuantity != -1;
			_quantity.text = $"{itemData.Quantity}/{itemData.MaxQuantity}";
		}
		if ((bool)_quantityContainer)
		{
			_quantityContainer.gameObject.SetActive(_quantity.enabled);
		}
		if ((bool)_costContainer)
		{
			_costContainer.SetActive(itemData.ShowCost && itemData.ResourceCost == null);
		}
		if ((bool)_newIndicator)
		{
			_newIndicator.SetActive(itemData.IsNew);
		}
		if (_cost != null)
		{
			_cost.text = itemData.CostLocalized;
			_cost.color = (flag ? _availableColor : _unavailableColor);
			_cost.color = (itemData.CanAfford ? _cost.color : _cannotBuyColor);
		}
		if (_unavailable != null)
		{
			_unavailable.enabled = !flag;
		}
		if (_resourcesCost != null)
		{
			bool flag3 = !(itemData.ResourceCost?.IsEmpty() ?? true);
			_resourcesCost.gameObject.SetActive(flag3);
			if (flag3)
			{
				_resourcesCost.Init(itemData.ResourceCost);
			}
		}
		if (Tooltip != null)
		{
			Tooltip.Initialize(tooltipParams, tooltipPosParams);
		}
		RefreshHover();
	}

	public void SetEmpty(UIItemElementInteractionEvents elementInteractionEvents = null, ItemEquipType equipType = ItemEquipType.Small)
	{
		if (Tooltip != null)
		{
			Tooltip.Initialize(null);
		}
		if (_background != null)
		{
			_background.sprite = _emptySlotSprite;
		}
		if (_iconBackground != null)
		{
			_iconBackground.sprite = _emptyIconSlotSprite;
		}
		if (_iconContainer != null)
		{
			_iconContainer.SetActive(value: false);
		}
		if ((object)_categoryIcon != null)
		{
			_categoryIcon.enabled = true;
			_categoryIcon.sprite = ItemDTOUtilities.GetItemTypeImage(equipType);
		}
		if (_name != null)
		{
			_name.enabled = false;
		}
		if (_separator != null)
		{
			_separator.enabled = false;
		}
		if (_equppiedIndicator != null)
		{
			_equppiedIndicator.SetActive(value: false);
		}
		if (_ownedIndicator != null)
		{
			_ownedIndicator.SetActive(value: false);
		}
		if (_quantity != null)
		{
			_quantity.enabled = false;
		}
		if (_unavailable != null)
		{
			_unavailable.enabled = false;
		}
		if (_costContainer != null)
		{
			_costContainer.SetActive(value: false);
		}
		if (_resourcesCost != null)
		{
			_resourcesCost.gameObject.SetActive(value: false);
		}
		ItemData = null;
		_elementInteractionEvents = elementInteractionEvents;
		ResetState();
	}

	private void UpdateHighlights()
	{
		if ((bool)_background && ItemData != null)
		{
			_background.sprite = (_isSelected ? _selectedSlotSprite : _populatedSlotSprite);
		}
		if ((bool)_iconBackground && ItemData != null)
		{
			_iconBackground.sprite = (_isSelected ? _selectedIconSlotSprite : _populatedIconSlotSprite);
		}
		if ((bool)_outline)
		{
			_outline.SetActive(_isSelected || _isHighlighted);
		}
	}

	private void RefreshHover()
	{
		if (_isHighlighted)
		{
			OnPointerEnter(null);
		}
	}

	private void OnDisable()
	{
		_elementInteractionEvents = null;
		_isHighlighted = false;
		CancelClick();
	}

	public void ResetState()
	{
		if ((bool)_newIndicator)
		{
			_newIndicator.SetActive(value: false);
		}
		Deselect();
	}

	public void Select()
	{
		_isSelected = true;
		UpdateHighlights();
	}

	public void Deselect()
	{
		_isSelected = false;
		UpdateHighlights();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isHighlighted = true;
		_elementInteractionEvents?.OnHover?.Invoke(this, isFlagRaised: true);
		UpdateHighlights();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHighlighted = false;
		_elementInteractionEvents?.OnHover?.Invoke(this, isFlagRaised: false);
		UpdateHighlights();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (_elementInteractionEvents?.OnDoubleClick == null)
			{
				OnClick();
			}
			else if (eventData.clickCount == 1)
			{
				_onClickCrt = StartCoroutine(OnClick_Crt());
			}
			else
			{
				OnDoubleClick();
			}
		}
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			OnRightClick();
		}
	}

	private void CancelClick()
	{
		if (_onClickCrt != null)
		{
			StopCoroutine(_onClickCrt);
			_onClickCrt = null;
		}
	}

	private IEnumerator OnClick_Crt()
	{
		yield return new WaitForSeconds(0.2f);
		OnClick();
	}

	public void OnClick()
	{
		_elementInteractionEvents?.OnClick?.Invoke(this);
	}

	public void OnDoubleClick()
	{
		CancelClick();
		_elementInteractionEvents?.OnDoubleClick?.Invoke(this);
	}

	public void OnRightClick()
	{
		_elementInteractionEvents?.OnRightClick?.Invoke(this);
	}

	public IGeneralInfoData GetInfoData()
	{
		return UIGeneralInfoUtils.GetItemInfo(ItemData);
	}
}

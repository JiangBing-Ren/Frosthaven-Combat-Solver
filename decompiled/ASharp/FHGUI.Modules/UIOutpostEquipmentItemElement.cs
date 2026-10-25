using System;
using System.Collections;
using FHGUI.Common;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIOutpostEquipmentItemElement : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IGeneralInfoProvider
{
	[Header("Components")]
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private GameObject _iconOrnament;

	[SerializeField]
	private GameObject _levelRequirementContainer;

	[SerializeField]
	private TMP_Text _levelRequirement;

	[SerializeField]
	private Image _border;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _selection;

	[SerializeField]
	private Image _type;

	[SerializeField]
	private Image _locked;

	[Header("Colors")]
	[SerializeField]
	private Color _borderColorRegular;

	[SerializeField]
	private Color _borderColorSpent;

	[SerializeField]
	private Color _borderColorUnavailable;

	[SerializeField]
	private Color _backgroundColorRegular;

	[SerializeField]
	private Color _backgroundColorUnavailable;

	[SerializeField]
	private Color _iconColorRegular = Color.white;

	[SerializeField]
	private Color _iconColorUnavailable = Color.white;

	[SerializeField]
	private Material _iconMaterialRegular;

	[SerializeField]
	private Material _iconMaterialUnavailable;

	[SerializeField]
	private Color _iconTypeColorRegular = Color.white;

	[SerializeField]
	private Color _iconTypeColorUnavailable = Color.white;

	[Header("Scenario")]
	[SerializeField]
	private Image _spent;

	[SerializeField]
	private Image _lost;

	[SerializeField]
	private ImageMaterialSwapper _iconMaterial;

	[Header("Misc")]
	[SerializeField]
	private bool _selectable = true;

	private Action<UIOutpostEquipmentItemElement> _onClick;

	private Action<UIOutpostEquipmentItemElement, bool> _onHover;

	private Action<UIOutpostEquipmentItemElement> _onDoubleClick;

	private Action<UIOutpostEquipmentItemElement> _onRightClick;

	private Coroutine _onClickCrt;

	public ItemDataDTO Data { get; private set; }

	[field: SerializeField]
	public UIItemTooltipTarget Tooltip { get; private set; }

	[field: SerializeField]
	public UITextTooltipTarget CategoryTooltip { get; private set; }

	public void SetItem(ItemDataDTO item, Action<UIOutpostEquipmentItemElement> onClick = null, Action<UIOutpostEquipmentItemElement, bool> onHover = null, Action<UIOutpostEquipmentItemElement> onDoubleClick = null, Action<UIOutpostEquipmentItemElement> onRightClick = null, FHItemTooltip.ItemTooltipParams tooltipParams = null, TooltipPositionParams? tooltipPositionParams = null, bool shared = true)
	{
		Data = item;
		UIHelperUtil.SetSpriteAndTweenIt(_icon, item.Icon, newEnabledState: true);
		bool flag = Data.ScenarioData?.IsLost ?? false;
		bool flag2 = Data.ScenarioData?.IsSpent ?? false;
		bool flag3 = Data.ScenarioData?.IsLocked ?? false;
		if (_iconOrnament != null)
		{
			_iconOrnament.gameObject.SetActive(value: true);
		}
		if ((object)_type != null)
		{
			_type.enabled = false;
		}
		if ((object)_locked != null)
		{
			_locked.enabled = flag3;
		}
		if ((object)_spent != null)
		{
			_spent.enabled = flag2;
		}
		if ((object)_lost != null)
		{
			_lost.enabled = flag;
		}
		if ((object)_iconMaterial != null)
		{
			if (flag | flag2)
			{
				_iconMaterial.SwapToTargetMaterial();
			}
			else
			{
				_iconMaterial.ResetToPreSwap();
			}
		}
		if (_levelRequirementContainer != null)
		{
			_levelRequirementContainer.SetActive(value: false);
		}
		bool flag4 = !flag & shared;
		if ((bool)_icon)
		{
			_icon.color = (flag4 ? _iconColorRegular : _iconColorUnavailable);
			_icon.material = (flag4 ? _iconMaterialRegular : _iconMaterialUnavailable);
		}
		if ((object)_background != null)
		{
			_background.color = (flag4 ? _backgroundColorRegular : _backgroundColorUnavailable);
		}
		if ((object)_border != null)
		{
			Image border = _border;
			Color color;
			if (flag4)
			{
				color = (flag2 ? _borderColorSpent : _borderColorRegular);
			}
			else
			{
				color = _borderColorUnavailable;
			}
			border.color = color;
		}
		if ((object)_locked != null)
		{
			_locked.enabled = flag3;
		}
		_onClick = onClick;
		_onHover = onHover;
		_onDoubleClick = onDoubleClick;
		_onRightClick = onRightClick;
		_selection.enabled = false;
		if (Tooltip != null)
		{
			Tooltip.Initialize(tooltipParams, tooltipPositionParams);
		}
		if (CategoryTooltip != null)
		{
			CategoryTooltip.SetText(null);
		}
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	public void SetEmpty(bool isAvailable, ItemEquipType type, int levelRequirement = 0, Action<UIOutpostEquipmentItemElement> onClick = null, Action<UIOutpostEquipmentItemElement, bool> onHover = null, Action<UIOutpostEquipmentItemElement> onDoubleClick = null, Action<UIOutpostEquipmentItemElement> onRightClick = null)
	{
		Data = null;
		_icon.enabled = false;
		if (_iconOrnament != null)
		{
			_iconOrnament.gameObject.SetActive(value: false);
		}
		if (_levelRequirementContainer != null)
		{
			_levelRequirementContainer.gameObject.SetActive(!isAvailable && levelRequirement > 0);
		}
		if (_levelRequirement != null)
		{
			_levelRequirement.text = levelRequirement.ToString();
		}
		if (_border != null)
		{
			_border.color = (isAvailable ? _borderColorRegular : _borderColorUnavailable);
		}
		if ((object)_spent != null)
		{
			_spent.enabled = false;
		}
		if ((object)_lost != null)
		{
			_lost.enabled = false;
		}
		if ((object)_iconMaterial != null)
		{
			_iconMaterial.ResetToPreSwap();
		}
		_background.color = (isAvailable ? _backgroundColorRegular : _backgroundColorUnavailable);
		_icon.color = (isAvailable ? _iconColorRegular : _iconColorUnavailable);
		if (_type != null)
		{
			_type.enabled = true;
			_type.sprite = ItemDTOUtilities.GetItemTypeImageTintable(type);
			_type.color = (isAvailable ? _iconTypeColorRegular : _iconTypeColorUnavailable);
		}
		if (_locked != null)
		{
			_locked.enabled = !isAvailable;
		}
		_onClick = onClick;
		_onHover = onHover;
		_onDoubleClick = onDoubleClick;
		_onRightClick = onRightClick;
		_selection.enabled = false;
		if (Tooltip != null)
		{
			Tooltip.Initialize(null);
		}
		if (CategoryTooltip != null)
		{
			string text = LocalizationManager.GetTranslation(FHUILocKeys.GetItemSlotKey(type));
			if (!isAvailable)
			{
				text = text + "\n" + FHUILocFormatting.PopulateParams("GUI_PETS_SLOT_UNLOCK", levelRequirement);
			}
			CategoryTooltip.SetText(text);
		}
	}

	public void SetType(ItemEquipType type)
	{
		if (_type != null)
		{
			_type.sprite = ItemDTOUtilities.GetItemTypeImageTintable(type);
		}
	}

	private void OnDisable()
	{
		_onClick = null;
		_onHover = null;
		_onDoubleClick = null;
		_onDoubleClick = null;
		_selection.enabled = false;
		Data = null;
	}

	public void Select()
	{
		if (_selectable)
		{
			_selection.enabled = true;
		}
	}

	public void Deselect()
	{
		_selection.enabled = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_onHover?.Invoke(this, arg2: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_onHover?.Invoke(this, arg2: false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (eventData.clickCount == 1)
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
		yield return new WaitForSeconds(0.3f);
		_onClick?.Invoke(this);
	}

	public void OnDoubleClick()
	{
		CancelClick();
		_onDoubleClick?.Invoke(this);
	}

	public void OnRightClick()
	{
		_onRightClick?.Invoke(this);
	}

	public IGeneralInfoData GetInfoData()
	{
		if (Data != null)
		{
			return UIGeneralInfoUtils.GetItemInfo(Data);
		}
		return null;
	}
}

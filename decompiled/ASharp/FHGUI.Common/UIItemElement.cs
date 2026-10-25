using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIItemElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IGeneralInfoProvider
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private Image _isActiveIndicator;

	[SerializeField]
	private Image _isLockedIndicator;

	[SerializeField]
	private Image _isNewIndicator;

	[SerializeField]
	private TMP_Text _quantity;

	[SerializeField]
	private UIItemTooltipTarget _tooltip;

	[SerializeField]
	private UITextTooltipTarget _textTooltip;

	private static Color _activeColor = Color.white;

	private static Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);

	private static Color _consumedColor = new Color(1f, 0.5f, 0.5f, 0.75f);

	private ItemDataDTO _item;

	public UIItemTooltipTarget Tooltip => _tooltip;

	public void Init(ItemDataDTO item, FHItemTooltip.ItemTooltipParams tooltipParams = null, TooltipPositionParams? tooltipPositionParams = null, bool manualShowHide = false)
	{
		_item = item;
		_icon.sprite = item.Icon;
		ref ItemDataDTO.ItemScenarioData? scenarioData = ref item.ScenarioData;
		bool flag = scenarioData.HasValue && scenarioData.GetValueOrDefault().SlotState == CItem.EItemSlotState.Active;
		ref ItemDataDTO.ItemScenarioData? scenarioData2 = ref item.ScenarioData;
		bool flag2 = scenarioData2.HasValue && scenarioData2.GetValueOrDefault().SlotState == CItem.EItemSlotState.Consumed;
		ref ItemDataDTO.ItemScenarioData? scenarioData3 = ref item.ScenarioData;
		bool flag3 = scenarioData3.HasValue && scenarioData3.GetValueOrDefault().SlotState == CItem.EItemSlotState.Spent;
		ref ItemDataDTO.ItemScenarioData? scenarioData4 = ref item.ScenarioData;
		bool flag4 = scenarioData4.HasValue && scenarioData4.GetValueOrDefault().SlotState == CItem.EItemSlotState.Locked;
		Image icon = _icon;
		Color color;
		if (flag2)
		{
			color = _consumedColor;
		}
		else
		{
			color = (flag3 ? _inactiveColor : _activeColor);
		}
		icon.color = color;
		_isLockedIndicator.enabled = flag4;
		_isActiveIndicator.enabled = flag;
		_isNewIndicator.enabled = item.IsNew;
		_highlight.enabled = false;
		if (_quantity != null)
		{
			_quantity.enabled = item.Quantity != 0;
			_quantity.text = item.Quantity.ToString();
		}
		if (_tooltip != null)
		{
			_tooltip.Initialize(tooltipParams, tooltipPositionParams, manualShowHide);
			_tooltip.enabled = true;
			if (_textTooltip != null)
			{
				_textTooltip.enabled = false;
			}
		}
		else if (_textTooltip != null)
		{
			_textTooltip.SetText(_item.NameLocalized);
			_textTooltip.enabled = true;
			if (_tooltip != null)
			{
				_tooltip.enabled = false;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_highlight.enabled = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_highlight.enabled = false;
	}

	public IGeneralInfoData GetInfoData()
	{
		if (_item != null)
		{
			return UIGeneralInfoUtils.GetItemInfo(_item);
		}
		return null;
	}
}

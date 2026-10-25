using FHGUI.DTO;
using FHGUI.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIScenarioBriefingSuggestibleSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[Header("Components")]
	[SerializeField]
	private Image m_ImageMain;

	[SerializeField]
	private Image m_ImageBackgroundNormal;

	[SerializeField]
	private Image m_ImageBackgroundDisabled;

	[SerializeField]
	private Image m_ImageSelected;

	[SerializeField]
	private Image m_ImageHighlightUnderlay;

	[SerializeField]
	private Image m_ImageHighlightFrame;

	[SerializeField]
	private TMP_Text m_TextName;

	[SerializeField]
	private TMP_Text m_TextInfo;

	[SerializeField]
	private TMP_Text m_TextPoints;

	[SerializeField]
	private UIUsersListContainer m_UsersListContainer;

	[SerializeField]
	private FHStacksCounter m_Charges;

	[Header("Tooltips")]
	[SerializeField]
	private UITextTooltipTarget m_Tooltip;

	[SerializeField]
	private UIPrefabTooltipTarget m_PrefabTooltip;

	[SerializeField]
	private GameObject m_CustomTooltipPrototype;

	[SerializeField]
	private RectTransform m_PickerTargetPoint;

	[Header("Settings")]
	[SerializeField]
	private TMP_ColorGradient m_CaptionColorGradientNormal;

	[SerializeField]
	private TMP_ColorGradient m_CaptionColorGradientSelected;

	private UISuggestibleSlotInteractionEvents _interactionEvents;

	private UIPrefabTooltipTarget.TooltipInitParams _tooltipInitParams;

	private SuggestibleSlotDTO _data;

	private bool _initialized;

	private bool _isHovered;

	public RectTransform PickerPoint
	{
		get
		{
			if (!(m_PickerTargetPoint != null))
			{
				return gameObject.transform as RectTransform;
			}
			return m_PickerTargetPoint;
		}
	}

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
		}
	}

	public void Init(SuggestibleSlotDTO data, UISuggestibleSlotInteractionEvents interactionEvents, UIPrefabTooltipTarget.TooltipInitParams tooltipInitParams = null)
	{
		InitCache();
		_data = data;
		_interactionEvents = interactionEvents;
		_tooltipInitParams = tooltipInitParams;
		if ((bool)m_ImageMain)
		{
			m_ImageMain.sprite = _data.Image;
			m_ImageMain.enabled = _data.Image != null;
		}
		if ((bool)m_TextName)
		{
			m_TextName.text = _data.NameLiteral;
			m_TextName.colorGradientPreset = (_data.IsSelected ? m_CaptionColorGradientSelected : m_CaptionColorGradientNormal);
		}
		if ((bool)m_TextInfo)
		{
			m_TextInfo.text = _data.InfoLiteral;
		}
		if ((bool)m_TextPoints)
		{
			m_TextPoints.text = _data.FooterLiteral;
		}
		if ((bool)m_ImageSelected)
		{
			m_ImageSelected.gameObject.SetActive(_data.IsSelected);
		}
		if ((bool)m_ImageBackgroundNormal)
		{
			m_ImageBackgroundNormal.gameObject.SetActive(_data.Interactable);
		}
		if ((bool)m_ImageBackgroundDisabled)
		{
			m_ImageBackgroundDisabled.gameObject.SetActive(!_data.Interactable);
		}
		if (m_Charges != null)
		{
			m_Charges.Init(_data.Charges, _data.MaxCharges);
		}
		SetupTooltip();
		if ((bool)m_UsersListContainer)
		{
			m_UsersListContainer.Init(_data.SuggestingUsers);
		}
		enabled = !data.SuppressInteraction;
		OnHover(_isHovered, raiseEvent: false);
	}

	private void SetupTooltip()
	{
		if (m_PrefabTooltip != null && _tooltipInitParams != null)
		{
			m_PrefabTooltip.enabled = true;
			if (m_CustomTooltipPrototype != null)
			{
				_tooltipInitParams.Prefab = m_CustomTooltipPrototype;
			}
			m_PrefabTooltip.Initialize(_tooltipInitParams.Prefab, _tooltipInitParams.OnObjectCreatedCallback);
			m_PrefabTooltip.Refresh();
			if (m_Tooltip != null)
			{
				m_Tooltip.enabled = false;
			}
		}
		else if ((bool)m_Tooltip && _data.TooltipLiteral.IsNOTNullOrEmpty())
		{
			m_Tooltip.enabled = true;
			m_Tooltip.SetText(_data.TooltipLiteral, refreshTooltip: true);
			if (m_PrefabTooltip != null)
			{
				m_PrefabTooltip.enabled = false;
			}
		}
		else
		{
			if (m_Tooltip != null)
			{
				m_Tooltip.enabled = false;
			}
			if (m_PrefabTooltip != null)
			{
				m_PrefabTooltip.enabled = false;
			}
		}
	}

	private void OnDisable()
	{
		_isHovered = false;
		OnHover(_isHovered, raiseEvent: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isHovered = true;
		OnHover(_isHovered, raiseEvent: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHovered = false;
		OnHover(_isHovered, raiseEvent: true);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			_interactionEvents?.OnClick?.Invoke(_data);
		}
	}

	private void OnHover(bool isHovered, bool raiseEvent)
	{
		if ((bool)m_ImageHighlightUnderlay)
		{
			Image imageHighlightUnderlay = m_ImageHighlightUnderlay;
			SuggestibleSlotDTO data = _data;
			imageHighlightUnderlay.enabled = (data != null && data.Interactable && (_data?.IsEmpty ?? false)) & isHovered;
		}
		if ((bool)m_ImageHighlightFrame)
		{
			m_ImageHighlightFrame.enabled = (_data?.Interactable ?? false) & isHovered;
		}
		if (raiseEvent)
		{
			_interactionEvents?.OnHover?.Invoke(_data, isHovered);
		}
	}
}

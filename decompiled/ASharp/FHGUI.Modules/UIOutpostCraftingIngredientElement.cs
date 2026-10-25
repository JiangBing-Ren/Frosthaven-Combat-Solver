using System;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIOutpostCraftingIngredientElement : MonoBehaviour
{
	public struct Params
	{
		public ItemDataDTO Item;

		public float NeededValue;

		public float AvailableValue;

		public string NameLocalized;

		public Sprite Icon;

		public bool Available;

		public UICraftingIngredientInteractionEvents Interactions;

		public object Type;

		public Action OnDeselect;

		public bool Interactable;

		public int Index;

		public FHItemTooltip.ItemTooltipParams tooltipParams;

		public TooltipPositionParams? tooltipPosParams;
	}

	[Header("Elements")]
	[SerializeField]
	private FHButton m_Button;

	[SerializeField]
	private UIOutpostEquipmentItemElement m_ItemIcon;

	[SerializeField]
	private Image m_ResourceIcon;

	[SerializeField]
	private TMP_Text m_ResourceValue;

	[SerializeField]
	private TMP_Text m_ResourceName;

	[SerializeField]
	private GameObject m_Empty;

	[SerializeField]
	private Image m_MissingIngredient;

	[SerializeField]
	private GameObject m_Selected;

	[SerializeField]
	private GameObject m_Highlight;

	[Header("Effects")]
	[SerializeField]
	private UITweenEffects m_OnSelectEffects;

	[Header("Settings")]
	[SerializeField]
	private Material m_GrayScaleMaterial;

	[Header("Tooltip")]
	[SerializeField]
	private UITextTooltipTarget m_TooltipTarget;

	public Action<UIOutpostCraftingIngredientElement> OnStateChanged;

	private bool m_SuppressActionCallbacks;

	private bool m_Initialized;

	public Params Data { get; private set; }

	public bool IsSelected { get; private set; }

	public bool IsHighlighted { get; private set; }

	public bool IsEmpty
	{
		get
		{
			if (Data.Item == null)
			{
				return (object)Data.Icon == null;
			}
			return false;
		}
	}

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if ((bool)m_Button)
			{
				m_Button.Selected += HandleOnSelect;
				m_Button.Deselected += HandleOnDeselect;
				m_Button.PointerEntered += HandleOnPointerEnter;
				m_Button.PointerExited += HandleOnPointerExit;
				m_Button.PointerEvent += HandleOnPointerEvent;
			}
			UpdateHighlights();
		}
	}

	public void Init(Params data)
	{
		InitCache();
		Data = data;
		bool flag = Data.Item != null;
		bool flag2 = !flag && (object)Data.Icon != null;
		if ((bool)m_ResourceIcon)
		{
			UIHelperUtil.SetSpriteAndTweenIt(m_ResourceIcon, Data.Icon, flag2);
			if (flag2)
			{
				m_ResourceIcon.material = ((!data.Available) ? m_GrayScaleMaterial : null);
			}
		}
		if (m_ItemIcon != null)
		{
			UIManager.RegisterLateActiveState(m_ItemIcon.gameObject, flag);
			if (flag)
			{
				m_ItemIcon.SetItem(Data.Item, null, null, null, null, Data.tooltipParams, Data.tooltipPosParams);
			}
		}
		if ((bool)m_Empty)
		{
			m_Empty.gameObject.SetActive(IsEmpty);
		}
		if ((bool)m_ResourceValue)
		{
			bool flag3 = flag2;
			m_ResourceValue.gameObject.SetActive(flag3);
			if (flag3)
			{
				m_ResourceValue.text = GetValueFormated(data);
			}
		}
		if ((bool)m_ResourceName)
		{
			m_ResourceName.enabled = !IsEmpty;
			if (flag)
			{
				m_ResourceName.text = Data.Item.NameLocalized;
			}
			if (flag2)
			{
				m_ResourceName.text = Data.NameLocalized;
			}
		}
		if ((bool)m_MissingIngredient)
		{
			m_MissingIngredient.enabled = !IsEmpty && !Data.Available;
		}
		if ((bool)m_Button)
		{
			m_Button.interactable = Data.Interactable;
		}
		SetupTooltipLiteral();
	}

	private void SetupTooltipLiteral()
	{
		if (!(m_TooltipTarget == null))
		{
			if (Data.Item != null)
			{
				m_TooltipTarget.SetText(string.Empty);
			}
			else
			{
				m_TooltipTarget.SetText(Data.NameLocalized);
			}
		}
	}

	private string GetValueFormated(Params @params)
	{
		if (@params.Item != null)
		{
			return string.Empty;
		}
		if (@params.NeededValue > 0f)
		{
			string text = $"{Mathf.Min(@params.AvailableValue, @params.NeededValue)}/{@params.NeededValue}";
			if (@params.AvailableValue < @params.NeededValue)
			{
				text = FHUILocFormatting.Color(text, FHUIColors.MISSING_RESOURCE);
			}
			return text;
		}
		return @params.NeededValue.ToString();
	}

	private void UpdateHighlights()
	{
		if ((bool)m_Selected)
		{
			m_Selected.SetActive(IsSelected);
		}
		if ((bool)m_Highlight)
		{
			m_Highlight.SetActive(Data.Interactable && IsHighlighted);
		}
	}

	public void Select()
	{
		if (!IsSelected)
		{
			m_SuppressActionCallbacks = true;
			IsSelected = true;
			if ((bool)m_Button)
			{
				m_Button.OnSelect(null);
			}
			m_SuppressActionCallbacks = false;
			UpdateHighlights();
			OnStateChanged?.Invoke(this);
			if (m_OnSelectEffects != null)
			{
				m_OnSelectEffects.Play();
			}
		}
	}

	public void Deselect()
	{
		if (IsSelected)
		{
			m_SuppressActionCallbacks = true;
			IsSelected = false;
			if ((bool)m_Button)
			{
				m_Button.OnDeselect(null);
			}
			m_SuppressActionCallbacks = false;
			UpdateHighlights();
			OnStateChanged?.Invoke(this);
			if (m_OnSelectEffects != null)
			{
				m_OnSelectEffects.Reset();
			}
		}
	}

	public void HandleOnSelect()
	{
	}

	public void HandleOnDeselect()
	{
		if (!m_SuppressActionCallbacks)
		{
			Data.OnDeselect?.Invoke();
		}
	}

	public void HandleOnPointerEnter()
	{
		IsHighlighted = true;
		UpdateHighlights();
		if (!m_SuppressActionCallbacks)
		{
			Data.Interactions?.OnHover?.Invoke(Data, isFlagRaised: true);
		}
		OnStateChanged?.Invoke(this);
	}

	public void HandleOnPointerExit()
	{
		IsHighlighted = false;
		UpdateHighlights();
		if (!m_SuppressActionCallbacks)
		{
			Data.Interactions?.OnHover?.Invoke(Data, isFlagRaised: false);
		}
		OnStateChanged?.Invoke(this);
	}

	private void OnClick()
	{
		if (!m_SuppressActionCallbacks)
		{
			Data.Interactions?.OnClick?.Invoke(Data);
		}
	}

	private void OnRightClick()
	{
		if (!m_SuppressActionCallbacks)
		{
			Data.Interactions?.OnRightClick?.Invoke(Data);
		}
	}

	public void HandleOnPointerEvent(PointerEventData eventData)
	{
		if (eventData.eligibleForClick)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				OnClick();
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				OnRightClick();
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_Button)
		{
			m_Button.Selected -= HandleOnSelect;
			m_Button.Deselected -= HandleOnDeselect;
			m_Button.PointerEntered -= HandleOnPointerEnter;
			m_Button.PointerExited -= HandleOnPointerExit;
			m_Button.PointerEvent -= HandleOnPointerEvent;
		}
	}
}

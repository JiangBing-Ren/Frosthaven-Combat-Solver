using System;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIDistributionCell : UIBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public struct Params
	{
		public int BaseValue;

		public int CurrentDelta;

		public int MaxDelta;

		public Sprite Icon;

		public bool IsGain;

		public bool Enabled;

		public bool IsEmpty;

		public bool IsDone;

		public bool HideValueString;

		public bool Owned;

		public bool IsPicked;

		public bool SuppressButtons;

		public bool NotEligible;

		public ItemDataDTO Item;

		public string TooltipLiteral;

		public UIPrefabTooltipTarget.TooltipInitParams PrefabTooltipParams;

		public Action<int> OnChange;

		public FHItemTooltip.ItemTooltipParamsFunc TooltipParamsFunc;

		public static Params Empty()
		{
			return new Params
			{
				IsEmpty = true
			};
		}
	}

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Text_Value;

	[SerializeField]
	private FHButton m_Button_Add;

	[SerializeField]
	private FHButton m_Button_Remove;

	[AudioEventName]
	[SerializeField]
	private string m_AudioItemAdd = "PlaySound_UIButtonPlusSelect";

	[AudioEventName]
	[SerializeField]
	private string m_AudioItemRemove = "PlaySound_UIButtonMinusSelect";

	[SerializeField]
	private GameObject m_Highlight;

	[SerializeField]
	private GameObject m_Picked;

	[SerializeField]
	private GameObject m_Owned;

	[SerializeField]
	private RectTransform m_HitBox;

	[Header("Settings")]
	[SerializeField]
	private Color m_TextValueNormal;

	[SerializeField]
	private Color m_TextValueFocused;

	[SerializeField]
	private Color m_IconNormal;

	[SerializeField]
	private Color m_IconNotEligible;

	[SerializeField]
	private float m_ShrinkInPixels;

	[Header("Events")]
	[SerializeField]
	private UIItemTooltipTarget m_ItemTooltip;

	[SerializeField]
	private RectTransform m_TooltipPos;

	[SerializeField]
	private TextAnchor m_TooltipAnchor;

	[Header("Tooltip")]
	[SerializeField]
	private UITextTooltipTarget m_TooltipTarget;

	[SerializeField]
	private UIPrefabTooltipTarget m_PrefabTooltip;

	[SerializeField]
	private GameObject m_CustomTooltipPrototype;

	private bool m_Focused;

	private bool m_IsHighlighted;

	public Params Data { get; private set; }

	public new void Awake()
	{
		m_Button_Remove.onClick.AddListener(HandleOnRemoveButton);
		m_Button_Add.onClick.AddListener(HandleOnAddButton);
	}

	public void Init(Params @params)
	{
		Clear();
		m_Focused = false;
		Data = @params;
		if (m_Icon != null)
		{
			m_Icon.sprite = @params.Icon;
			m_Icon.gameObject.SetActive(@params.Icon != null);
			m_Icon.color = (@params.NotEligible ? m_IconNotEligible : m_IconNormal);
		}
		if (m_Text_Value != null)
		{
			m_Text_Value.text = ((Data.IsEmpty || !HasAnyValidAction()) ? "-" : @params.CurrentDelta.ToString());
			m_Text_Value.enabled = !Data.HideValueString;
		}
		if (m_Owned != null)
		{
			m_Owned.gameObject.SetActive(Data.Owned);
		}
		if ((bool)m_Picked)
		{
			m_Picked.gameObject.SetActive(Data.IsPicked);
		}
		UpdateInteractions();
		UpdateHighlight();
		UpdateTooltip();
	}

	private void UpdateInteractions()
	{
		m_Button_Add.interactable = !Data.IsDone && Math.Abs(Data.CurrentDelta) < Data.MaxDelta;
		m_Button_Remove.interactable = !Data.IsDone && Data.CurrentDelta > 0;
		m_Button_Add.gameObject.SetActive(m_Focused && Data.Enabled && !Data.IsEmpty && !Data.SuppressButtons && HasAnyValidAction());
		m_Button_Remove.gameObject.SetActive(m_Focused && Data.Enabled && !Data.IsEmpty && !Data.SuppressButtons && HasAnyValidAction());
	}

	private bool HasAnyValidAction()
	{
		if (Math.Abs(Data.CurrentDelta) < Data.MaxDelta)
		{
			return true;
		}
		if (Data.CurrentDelta > 0)
		{
			return true;
		}
		return false;
	}

	private void UpdateHighlight()
	{
		m_Highlight.gameObject.SetActive(m_IsHighlighted && Data.Enabled);
		m_Text_Value.color = ((m_IsHighlighted && Data.Enabled) ? m_TextValueFocused : m_TextValueNormal);
	}

	private void UpdateTooltip()
	{
		if (m_TooltipTarget != null && !string.IsNullOrEmpty(Data.TooltipLiteral))
		{
			m_TooltipTarget.SetText(Data.TooltipLiteral);
			if ((bool)m_ItemTooltip)
			{
				m_ItemTooltip.enabled = false;
			}
			if ((bool)m_PrefabTooltip)
			{
				m_PrefabTooltip.enabled = false;
			}
		}
		else if (m_ItemTooltip != null && Data.Item != null && Data.TooltipParamsFunc != null)
		{
			TooltipPositionParams value = new TooltipPositionParams
			{
				TransformAnchor = m_TooltipPos,
				Anchor = m_TooltipAnchor
			};
			m_ItemTooltip.enabled = true;
			m_ItemTooltip.Initialize(Data.TooltipParamsFunc?.Invoke(Data.Item), value);
			if (m_IsHighlighted)
			{
				m_ItemTooltip.OnPointerEnter(null);
			}
			if ((bool)m_TooltipTarget)
			{
				m_TooltipTarget.SetText(string.Empty);
			}
			if ((bool)m_PrefabTooltip)
			{
				m_PrefabTooltip.enabled = false;
			}
		}
		else if (m_PrefabTooltip != null && Data.PrefabTooltipParams != null)
		{
			m_PrefabTooltip.enabled = true;
			m_PrefabTooltip.Initialize(Data.PrefabTooltipParams.Prefab ?? m_CustomTooltipPrototype, Data.PrefabTooltipParams.OnObjectCreatedCallback);
			if (m_IsHighlighted)
			{
				m_PrefabTooltip.OnPointerEnter(null);
			}
			m_PrefabTooltip.Refresh();
			if ((bool)m_TooltipTarget)
			{
				m_TooltipTarget.SetText(string.Empty);
			}
			if ((bool)m_ItemTooltip)
			{
				m_ItemTooltip.enabled = false;
			}
		}
		else
		{
			if ((bool)m_TooltipTarget)
			{
				m_TooltipTarget.SetText(string.Empty);
			}
			if ((bool)m_PrefabTooltip)
			{
				m_PrefabTooltip.enabled = false;
			}
			if ((bool)m_ItemTooltip)
			{
				m_ItemTooltip.enabled = false;
			}
		}
	}

	public void SetFocused(bool focused)
	{
		m_Focused = focused;
		UpdateInteractions();
	}

	private void HandleOnAddButton()
	{
		bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		Data.OnChange?.Invoke((!flag) ? 1 : (Data.MaxDelta - Data.CurrentDelta));
	}

	private void HandleOnRemoveButton()
	{
		bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		Data.OnChange?.Invoke(flag ? (-Data.CurrentDelta) : (-1));
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Data.Enabled)
		{
			bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			if (eventData.button == PointerEventData.InputButton.Left && m_Button_Add.interactable)
			{
				AudioControllerUtils.PlaySound(m_AudioItemAdd);
				Data.OnChange?.Invoke((!flag) ? 1 : (Data.MaxDelta - Data.CurrentDelta));
			}
			if (eventData.button == PointerEventData.InputButton.Right && m_Button_Remove.interactable)
			{
				AudioControllerUtils.PlaySound(m_AudioItemRemove);
				Data.OnChange?.Invoke(flag ? (-Data.CurrentDelta) : (-1));
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_IsHighlighted = true;
		UpdateHighlight();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_IsHighlighted = false;
		UpdateHighlight();
	}

	public void ShrinkHitArena(bool b)
	{
		m_HitBox.sizeDelta = new Vector2(b ? (0f - m_ShrinkInPixels) : 0f, 0f);
	}

	private void Clear()
	{
		if (Data.Item != null && m_ItemTooltip != null)
		{
			m_ItemTooltip.Initialize(null);
		}
	}
}

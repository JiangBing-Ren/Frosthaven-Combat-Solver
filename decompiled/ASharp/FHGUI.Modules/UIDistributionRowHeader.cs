using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIDistributionRowHeader : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public struct Params
	{
		public Sprite Icon;

		public string Text;

		public ItemDataDTO Item;

		public string TooltipLiteral;

		public FHItemTooltip.ItemTooltipParamsFunc TooltipParamsFunc;

		public UIPrefabTooltipTarget.TooltipInitParams PrefabTooltipParams;
	}

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_IconStandAlone;

	[SerializeField]
	private TMP_Text m_Text_Value;

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

	private bool m_IsHighlighted;

	public Params Data { get; private set; }

	public void Init(Params @params)
	{
		Clear();
		Data = @params;
		bool flag = string.IsNullOrEmpty(@params.Text);
		if (m_Icon != null)
		{
			m_Icon.sprite = @params.Icon;
			m_Icon.enabled = (bool)@params.Icon && !flag;
		}
		if (m_IconStandAlone != null)
		{
			m_IconStandAlone.sprite = @params.Icon;
			m_IconStandAlone.enabled = (bool)@params.Icon & flag;
		}
		if (m_Text_Value != null)
		{
			m_Text_Value.gameObject.SetActive(!string.IsNullOrEmpty(@params.Text));
			m_Text_Value.text = @params.Text;
		}
		if (m_ItemTooltip != null)
		{
			TooltipPositionParams value = new TooltipPositionParams
			{
				Pos = m_TooltipPos.position,
				Anchor = m_TooltipAnchor
			};
			m_ItemTooltip.Initialize((Data.Item == null) ? null : Data.TooltipParamsFunc?.Invoke(Data.Item), value);
		}
		UpdateTooltip();
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

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_IsHighlighted = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_IsHighlighted = false;
	}

	private void Clear()
	{
		if (Data.Item != null && m_ItemTooltip != null)
		{
			m_ItemTooltip.Initialize(null);
		}
	}
}

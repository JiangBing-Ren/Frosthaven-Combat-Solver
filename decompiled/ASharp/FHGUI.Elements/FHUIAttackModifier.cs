using System.Collections.Generic;
using FHGUI.DTO;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHUIAttackModifier : Selectable
{
	[Header("Components")]
	[SerializeField]
	protected Image m_ImageIcon;

	[SerializeField]
	protected Image m_ImageAdditionalEffect;

	[SerializeField]
	protected Image m_ImageValue;

	[SerializeField]
	protected TextMeshProUGUI m_TextValue;

	[SerializeField]
	protected GameObject m_ContentGroup;

	[SerializeField]
	protected Transform m_CountersContainer;

	[SerializeField]
	protected Image m_CountPipPrototype;

	[SerializeField]
	protected Image m_Highlight;

	[SerializeField]
	protected GameObject m_RollingIndication;

	[Header("Tooltip")]
	[SerializeField]
	private UIPrefabTooltipTarget m_TooltipTarget;

	[SerializeField]
	private GameObject TooltipPrototype;

	[Header("Settings")]
	[SerializeField]
	private Color m_AddColor;

	[SerializeField]
	private Color m_RemoveColor;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_IconNormalColor;

	[SerializeField]
	private Color m_IconDepletedColor;

	private List<Image> m_CountPipIcons = new List<Image>();

	private bool m_Initialized;

	public AttackModifierDTO Data { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if (m_CountersContainer != null)
			{
				m_CountersContainer.GetComponentsInChildren(includeInactive: true, m_CountPipIcons);
			}
		}
	}

	public void Init(AttackModifierDTO dto)
	{
		InitCache();
		Data = dto;
		bool flag = Data == null;
		if (m_ImageIcon != null && !flag)
		{
			m_ImageIcon.sprite = Data.BaseIcon;
			m_ImageIcon.enabled = Data.BaseIcon != null;
			m_ImageIcon.color = ((Data.Count > 0) ? m_IconNormalColor : m_IconDepletedColor);
		}
		if (m_TextValue != null && !flag)
		{
			m_TextValue.text = Data.LocalizedValue;
			m_TextValue.color = Data.ValueColor;
		}
		if (m_ImageValue != null && !flag)
		{
			m_ImageValue.sprite = Data.ValueIcon;
			m_ImageValue.enabled = Data.ValueIcon != null;
		}
		if (m_ImageAdditionalEffect != null && !flag)
		{
			m_ImageAdditionalEffect.sprite = Data.ValueIcon;
			m_ImageAdditionalEffect.enabled = Data.ValueIcon != null;
		}
		if (m_ContentGroup != null)
		{
			m_ContentGroup.SetActive(!flag);
		}
		if (m_RollingIndication != null)
		{
			m_RollingIndication.SetActive(!flag && Data.Rolling && !Data.IsPremadeIcon);
		}
		UpdateCount(Data?.Count ?? 0, Data?.CountChange ?? 0);
		SetupTooltip();
	}

	private void UpdateCount(int count, int change)
	{
		if (!(m_CountersContainer == null) && !(m_CountPipPrototype == null))
		{
			UIHelperUtil.UpdateUIList(count + Mathf.Abs(change), m_CountPipIcons, m_CountPipPrototype, m_CountersContainer, onShow);
		}
		void onShow(int index, Image item)
		{
			Color color;
			if (index < count)
			{
				color = m_NormalColor;
			}
			else
			{
				color = ((change > 0) ? m_AddColor : m_RemoveColor);
			}
			item.color = color;
		}
	}

	private void SetupTooltip()
	{
		if (Data == null || m_TooltipTarget == null || TooltipPrototype == null)
		{
			m_TooltipTarget.enabled = false;
			return;
		}
		m_TooltipTarget.enabled = true;
		m_TooltipTarget.Initialize(TooltipPrototype, (GameObject go) =>
		{
			UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
			{
				Icon = Data.BaseIcon,
				DescriptionLiteral = LocalizationManager.GetTranslation(Data.DescriptionLocKey),
				Hints = null
			};
			go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
		});
	}
}

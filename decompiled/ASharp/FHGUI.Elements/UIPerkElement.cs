using FHGUI.DTO;
using FHGUI.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPerkElement : Selectable
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text m_ItemName;

	[SerializeField]
	private TMP_Text m_ItemDescription;

	[SerializeField]
	private TMP_Text m_ActiveCount;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Cost;

	[SerializeField]
	private Image m_IconCost;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_IconBackground;

	[SerializeField]
	private GameObject m_Highlight;

	[SerializeField]
	private GameObject m_Completed;

	[SerializeField]
	private FHUIAttackModifier m_AttackModifier;

	[SerializeField]
	private TMP_Text m_Quantity;

	[SerializeField]
	private GameObject m_QuantityContainer;

	[SerializeField]
	private FHStacksPip[] m_StacksPips;

	[Header("Sprites")]
	[SerializeField]
	private Sprite m_BackgroundAvailable;

	[SerializeField]
	private Sprite m_BackgroundNotAvailable;

	[SerializeField]
	private Sprite m_BackgroundSelected;

	[SerializeField]
	private Sprite m_BackgroundIconAvailable;

	[SerializeField]
	private Sprite m_BackgroundIconNotAvailable;

	[SerializeField]
	private Sprite m_BackgroundIconSelected;

	[Header("Settings")]
	[SerializeField]
	private Color m_Color_GoldCostNormal;

	[SerializeField]
	private Color m_Color_GoldCostInsufficient;

	[SerializeField]
	private Color m_Color_NameTintAvailable;

	[SerializeField]
	private Color m_Color_NameTintCannotBuy;

	[Header("Tooltip")]
	[SerializeField]
	private UIPrefabTooltipTarget m_TooltipTarget;

	[SerializeField]
	private GameObject TooltipPrototype;

	private UIPerkInteractionEvents m_InteractionEvents;

	private bool m_IsHovering;

	public PerkDTO Data { get; private set; }

	public void Init(PerkDTO data, UIPerkInteractionEvents interactionEvents)
	{
		Data = data;
		m_InteractionEvents = interactionEvents;
		PopulateItem();
		UpdateHighlights();
		SetupTooltip();
	}

	private void PopulateItem()
	{
		if ((bool)m_ItemName)
		{
			m_ItemName.text = Data.NameLocalized;
			m_ItemName.color = (Data.Affordable ? m_Color_NameTintAvailable : m_Color_NameTintCannotBuy);
		}
		if ((bool)m_ItemDescription)
		{
			m_ItemDescription.text = Data.DescriptionLocalized;
		}
		if ((bool)m_Icon)
		{
			m_Icon.sprite = Data.Icon;
			m_Icon.enabled = Data.Icon != null;
		}
		if ((bool)m_ActiveCount)
		{
			m_ActiveCount.text = $"{Data.UnlockedCount}/{Data.MaxCount}";
		}
		if ((bool)m_Cost)
		{
			m_Cost.text = FHUILocFormatting.Color(Data.Cost.ToString(), Data.Affordable ? m_Color_GoldCostNormal : m_Color_GoldCostInsufficient);
			m_Cost.gameObject.SetActive(Data.Cost > 1);
		}
		if ((bool)m_IconCost)
		{
			m_IconCost.color = (Data.Affordable ? m_Color_GoldCostNormal : m_Color_GoldCostInsufficient);
		}
		if ((bool)m_Completed)
		{
			m_Completed.gameObject.SetActive(Data.IsComplete);
		}
		if ((bool)m_AttackModifier)
		{
			bool flag = Data.DisplayModifier != null;
			m_AttackModifier.gameObject.SetActive(flag);
			if (flag)
			{
				m_AttackModifier.Init(Data.DisplayModifier);
			}
		}
		if ((bool)m_Quantity)
		{
			m_Quantity.enabled = Data.MaxCount != -1;
			m_Quantity.text = $"{Data.MaxCount - Data.UnlockedCount}/{Data.MaxCount}";
		}
		if ((bool)m_QuantityContainer)
		{
			m_QuantityContainer.gameObject.SetActive(m_Quantity.enabled);
		}
		if (m_StacksPips != null)
		{
			for (int i = 0; i < m_StacksPips.Length; i++)
			{
				m_StacksPips[i].gameObject.SetActive(i < Data.MaxCount);
				m_StacksPips[i].SetActive(i < Data.UnlockedCount);
			}
		}
	}

	private void SetupTooltip()
	{
		if (!(m_TooltipTarget == null))
		{
			m_TooltipTarget.Initialize(TooltipPrototype, (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = Data.Icon,
					CaptionLiteral = Data.NameLocalized,
					DescriptionLiteral = Data.DescriptionLocalized,
					Hints = null
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			});
		}
	}

	private void UpdateHighlights()
	{
		if (m_Background != null)
		{
			m_Background.sprite = GetBackgroundSprite();
		}
		if (m_IconBackground != null)
		{
			m_IconBackground.sprite = GetIconBackgroundSprite();
		}
		if ((bool)m_Highlight)
		{
			m_Highlight.SetActive(currentSelectionState != SelectionState.Normal && currentSelectionState != SelectionState.Disabled);
		}
	}

	private Sprite GetBackgroundSprite()
	{
		if (Data == null)
		{
			return m_BackgroundAvailable;
		}
		if (!Data.Affordable)
		{
			return m_BackgroundNotAvailable;
		}
		if (currentSelectionState == SelectionState.Selected)
		{
			return m_BackgroundSelected;
		}
		return m_BackgroundAvailable;
	}

	private Sprite GetIconBackgroundSprite()
	{
		if (Data == null)
		{
			return m_BackgroundIconAvailable;
		}
		if (!Data.Affordable)
		{
			return m_BackgroundIconNotAvailable;
		}
		if (currentSelectionState == SelectionState.Selected)
		{
			return m_BackgroundIconSelected;
		}
		return m_BackgroundIconAvailable;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		m_InteractionEvents?.OnClick?.Invoke(Data);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		m_IsHovering = true;
		m_InteractionEvents?.OnHover?.Invoke(Data, m_IsHovering);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		m_IsHovering = false;
		m_InteractionEvents?.OnHover?.Invoke(Data, m_IsHovering);
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		UpdateHighlights();
	}
}

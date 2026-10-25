using FHGUI.DTO;
using FHGUI.Modules;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UICardEnhancementElement : Selectable
{
	public struct Params
	{
		public CardDTO card;

		public CardEnhancementDTO enhancement;

		public CardEnhancementSlotDTO slot;

		public bool IsEmpty
		{
			get
			{
				if (card != null && enhancement != null)
				{
					return slot == null;
				}
				return true;
			}
		}

		public bool IsInteractable
		{
			get
			{
				if (slot == null || slot.InUse)
				{
					return enhancement.Sloted;
				}
				return true;
			}
		}
	}

	[SerializeField]
	private EnhancementsUIDataSO m_DataSO;

	[Header("Components")]
	[SerializeField]
	private TMP_Text m_EnhancementName;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_SlotIcon;

	[SerializeField]
	private TMP_Text m_Cost;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_IconBackground;

	[SerializeField]
	private GameObject m_Highlight;

	[SerializeField]
	private GameObject m_Owned;

	[Header("Colors")]
	[SerializeField]
	private Color m_Color_GoldCostNormal;

	[SerializeField]
	private Color m_Color_GoldCostInsufficient;

	[SerializeField]
	private Color m_Color_NameTintAvailable;

	[SerializeField]
	private Color m_Color_NameTintCannotBuy;

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

	[Header("Tooltip")]
	[SerializeField]
	private UIPrefabTooltipTarget m_TooltipTarget;

	[SerializeField]
	private GameObject TooltipPrototype;

	private UIEnhancementInteractionEvents m_InteractionEvents;

	private bool m_IsHovering;

	public Params Data { get; private set; }

	public void Init(Params data, UIEnhancementInteractionEvents interactionEvents)
	{
		Data = data;
		m_InteractionEvents = interactionEvents;
		SetupDebugTooltip();
		if ((bool)m_EnhancementName)
		{
			m_EnhancementName.text = data.enhancement.NameLocalized;
			m_EnhancementName.color = (data.enhancement.Affordable ? m_Color_NameTintAvailable : m_Color_NameTintCannotBuy);
		}
		if ((bool)m_Cost)
		{
			m_Cost.enabled = !Data.slot.InUse;
			if (m_Cost.enabled)
			{
				m_Cost.text = GetCostString(data.enhancement.Affordable);
			}
		}
		if ((bool)m_Icon)
		{
			m_Icon.overrideSprite = m_DataSO.GetEnhancementInventoryIcon(CrossReference.EnhancementDTOToEEnhancement(data.enhancement));
		}
		if ((bool)m_SlotIcon)
		{
			m_SlotIcon.overrideSprite = m_DataSO.GetEnhancementSlotIcon(data.enhancement.Mark);
		}
		if ((bool)m_Background)
		{
			m_Background.sprite = GetBackgroundSprite();
		}
		if ((bool)m_IconBackground)
		{
			m_IconBackground.sprite = GetIconBackgroundSprite();
		}
		if ((bool)m_Owned)
		{
			m_Owned.SetActive(Data.enhancement.Sloted);
		}
		interactable = data.IsInteractable;
		UpdateHighlights();
	}

	private void SetupDebugTooltip()
	{
		if (!(m_TooltipTarget == null))
		{
			string translation = LocalizationManager.GetTranslation(Data.card.NameLocKey);
			string description = "";
			string hint = string.Empty;
			if (Data.enhancement.Sloted)
			{
				description += FHUILocFormatting.PopulateParams("GUI_ENHANCEMENTS_INVENTORY_SLOT_ENHANCED_IN_CARD", translation);
				description += "\n\n";
			}
			else if (!Data.slot.InUse)
			{
				description = description + LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST") + ": " + GetCostString(Data.enhancement.Affordable);
				hint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick) + " " + FHUILocFormatting.PopulateParams("GUI_ENHANCEMENTS_INVENTORY_SLOT_IMPROVECARD", LocalizationManager.GetTranslation(Data.card.NameLocKey));
			}
			m_TooltipTarget.Initialize(TooltipPrototype, (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = m_DataSO.GetEnhancementInventoryIcon(CrossReference.EnhancementDTOToEEnhancement(Data.enhancement)),
					CaptionLiteral = Data.enhancement.NameLocalized,
					DescriptionLiteral = description,
					Hints = hint
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

	private string GetCostString(bool affordable = true)
	{
		Color color = (affordable ? m_Color_GoldCostNormal : m_Color_GoldCostInsufficient);
		return FHUILocFormatting.Color(FHUILocFormatting.Gold((int)Data.enhancement.CurrentGoldCost), color);
	}

	private Sprite GetBackgroundSprite()
	{
		if (Data.IsEmpty)
		{
			return m_BackgroundAvailable;
		}
		if (Data.enhancement.Sloted)
		{
			return m_BackgroundSelected;
		}
		if (Data.slot.InUse)
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
		if (Data.IsEmpty)
		{
			return m_BackgroundIconAvailable;
		}
		if (Data.enhancement.Sloted)
		{
			return m_BackgroundIconSelected;
		}
		if (Data.slot.InUse)
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

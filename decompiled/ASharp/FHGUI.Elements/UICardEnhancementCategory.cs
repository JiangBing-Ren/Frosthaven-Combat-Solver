using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UICardEnhancementCategory : MonoBehaviour
{
	[SerializeField]
	private EnhancementsUIDataSO m_DataSO;

	[Header("Components")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private UICardEnhancementElement m_EnhancementIconPrototype;

	[SerializeField]
	private TMP_Text m_CategoryLabel;

	[SerializeField]
	private Image m_CategoryIcon;

	[Header("Settings")]
	[SerializeField]
	private Color m_textColorInUse;

	private List<UICardEnhancementElement> m_EnhancementIcons = new List<UICardEnhancementElement>();

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_Container.GetComponentsInChildren(includeInactive: true, m_EnhancementIcons);
		}
	}

	public void Init(CardDTO abilityCard, CardEnhancementSlotDTO slot, UIEnhancementInteractionEvents interactionEvents)
	{
		InitCache();
		UpdateCategoryLabel(abilityCard, slot);
		UIHelperUtil.UpdateUIList(slot.Enhancements?.Count ?? 0, m_EnhancementIcons, m_EnhancementIconPrototype, m_Container, onShow);
		void onShow(int index, UICardEnhancementElement item)
		{
			UICardEnhancementElement.Params data = new UICardEnhancementElement.Params
			{
				card = abilityCard,
				slot = slot,
				enhancement = slot.Enhancements[index]
			};
			item.Init(data, interactionEvents);
		}
	}

	private void UpdateCategoryLabel(CardDTO abilityCard, CardEnhancementSlotDTO slotData)
	{
		if (m_CategoryLabel != null)
		{
			string text = "<sprite name=" + slotData.AbilityType + "> " + LocalizationManager.GetTranslation(slotData.AbilityType);
			if (slotData.InUse)
			{
				text = text + " (" + LocalizationManager.GetTranslation("GUI_ENHANCEMENTS_INVENTORY_SLOT_ENHANCED") + ")";
				text = FHUILocFormatting.Color(text, m_textColorInUse);
			}
			m_CategoryLabel.text = text;
		}
		if (m_CategoryIcon != null)
		{
			m_CategoryIcon.overrideSprite = m_DataSO.GetEnhancementSlotIcon(slotData.Mark);
		}
	}

	public UICardEnhancementElement SelectEnhancement(CardEnhancementSlotDTO cardEnhancementSlotDto, string enhancementID)
	{
		for (int i = 0; i < m_EnhancementIcons.Count; i++)
		{
			UICardEnhancementElement uICardEnhancementElement = m_EnhancementIcons[i];
			if (uICardEnhancementElement.Data.slot == cardEnhancementSlotDto && (!enhancementID.IsNOTNullOrEmpty() || !(uICardEnhancementElement.Data.enhancement.ID != enhancementID)))
			{
				uICardEnhancementElement.Select();
				return uICardEnhancementElement;
			}
		}
		return null;
	}
}

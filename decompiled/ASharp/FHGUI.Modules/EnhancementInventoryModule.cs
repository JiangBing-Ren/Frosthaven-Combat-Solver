using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.EventChannels;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class EnhancementInventoryModule : FHMapViewModule
{
	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener m_Caption;

	[SerializeField]
	private TMP_Text m_BuildingLevel;

	[SerializeField]
	private UICardEnhancementContainer m_EnhancementsContianer;

	[Header("Events")]
	[SerializeField]
	private UIEnhancementHighlightEventChannel m_EnhancementHighlightEventChannel;

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			if ((bool)m_Caption)
			{
				m_Caption.SetTextKey("GUI_ENHANCEMENTS_INVENTORY_CAPTION");
			}
		}
	}

	public void Init(CardDTO card, uint level, List<CardEnhancementSlotDTO> enhancementSlots, UIEnhancementInteractionEvents interactionEvents)
	{
		InitCache();
		if (m_EnhancementsContianer != null)
		{
			m_EnhancementsContianer.Init(card, enhancementSlots, interactionEvents);
		}
		if ((bool)m_BuildingLevel)
		{
			m_BuildingLevel.text = level.ToString();
		}
	}

	public void HighlightEnhancement(UICardEnhancementElement.Params target, bool isHover)
	{
		m_EnhancementHighlightEventChannel?.EnhancementHighlighted(target, isHover);
	}

	public void SelectEnhancement(CardEnhancementSlotDTO cardEnhancementSlotDto, string enhancementID)
	{
		m_EnhancementsContianer.SelectEnhancement(cardEnhancementSlotDto, enhancementID);
	}
}

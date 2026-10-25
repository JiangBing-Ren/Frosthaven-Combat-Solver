using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UICardEnhancementContainer : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_CategoryContainer;

	[SerializeField]
	private UICardEnhancementCategory m_CategoryPrototype;

	[SerializeField]
	private ScrollRect m_ScrollRect;

	private List<UICardEnhancementCategory> m_EnhancementCategories = new List<UICardEnhancementCategory>();

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_CategoryContainer.GetComponentsInChildren(includeInactive: true, m_EnhancementCategories);
		}
	}

	public void Init(CardDTO abilityCard, List<CardEnhancementSlotDTO> slotData, UIEnhancementInteractionEvents interactionEvents)
	{
		InitCache();
		UIHelperUtil.UpdateUIList(slotData.Count, m_EnhancementCategories, m_CategoryPrototype, m_CategoryContainer, onShow);
		void onShow(int index, UICardEnhancementCategory item)
		{
			item.Init(abilityCard, slotData[index], interactionEvents);
		}
	}

	public void SelectEnhancement(CardEnhancementSlotDTO cardEnhancementSlotDto, string enhancementID)
	{
		RectTransform rectTransform = null;
		for (int i = 0; i < m_EnhancementCategories.Count; i++)
		{
			UICardEnhancementElement uICardEnhancementElement = m_EnhancementCategories[i].SelectEnhancement(cardEnhancementSlotDto, enhancementID);
			if (uICardEnhancementElement != null)
			{
				rectTransform = uICardEnhancementElement.transform as RectTransform;
			}
		}
		if ((bool)rectTransform)
		{
			m_ScrollRect.ScrollToFit(rectTransform);
		}
	}
}

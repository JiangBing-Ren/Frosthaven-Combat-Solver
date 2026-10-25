using System;
using FHGUI.Elements;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class EnhancementPurchaseModule : FHMapViewModule
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text m_Caption;

	[SerializeField]
	private UIOutpostCardSlot m_CardSlot;

	[SerializeField]
	private FHButton m_PurchaseButton;

	[SerializeField]
	private UICardEnhancementHighlighter m_EnhancementHighlighter;

	private Action<UICardEnhancementElement.Params> m_OnPurchase;

	private UICardEnhancementElement.Params Data;

	private UICardInteractionEvents m_cardInteractionEvents;

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_PurchaseButton.onClick.AddListener(HandleOnPurchase);
		}
	}

	public void Init(UICardEnhancementElement.Params data, Action<UICardEnhancementElement.Params> onPurchaseCallback)
	{
		InitCache();
		m_OnPurchase = onPurchaseCallback;
		Data = data;
		if ((bool)m_Caption)
		{
			UIHelperUtil.SetTextKey(m_Caption, data.card.NameLocKey);
		}
		if ((bool)m_CardSlot)
		{
			m_CardSlot.Init(data.card, isMiniCard: false, null, fitInParent: true);
		}
		if ((bool)m_PurchaseButton)
		{
			m_PurchaseButton.interactable = data.enhancement != null && !data.slot.InUse && data.enhancement.Affordable;
		}
		if ((bool)m_EnhancementHighlighter)
		{
			m_EnhancementHighlighter.Init(m_CardSlot, data);
		}
	}

	private void HandleOnPurchase()
	{
		m_OnPurchase?.Invoke(Data);
	}
}

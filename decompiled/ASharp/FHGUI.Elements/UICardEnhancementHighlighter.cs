using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.EventChannels;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UICardEnhancementHighlighter : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform HighlightAbilityParent;

	[SerializeField]
	private UIEnhancementButtonHighlight HighlightAbilityPrototype;

	[Header("Events")]
	[SerializeField]
	protected UIEnhancementHighlightEventChannel m_EnhancementEventsChannel;

	private UIOutpostCardSlot m_CardSlot;

	private UICardEnhancementElement.Params m_Param;

	private List<UIEnhancementButtonHighlight> n_HighlightAbilityPool = new List<UIEnhancementButtonHighlight>();

	private List<EnhancementButtonBase> m_SelectedEnhancementButtonsCache = new List<EnhancementButtonBase>();

	private bool m_InvalidateHighlights;

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if ((bool)m_EnhancementEventsChannel)
			{
				m_EnhancementEventsChannel.OnEnhancementHighlighted.AddListener(HighlightEnhancementSlot);
			}
			if ((bool)HighlightAbilityParent)
			{
				HighlightAbilityParent.GetComponentsInChildren(includeInactive: true, n_HighlightAbilityPool);
			}
		}
	}

	public void Init(UIOutpostCardSlot cardSlot, UICardEnhancementElement.Params dataParams)
	{
		InitCache();
		m_CardSlot = cardSlot;
		m_Param = dataParams;
		CacheEnhancementButtonsForSelected();
		m_InvalidateHighlights = true;
	}

	private void LateUpdate()
	{
		if (m_InvalidateHighlights)
		{
			m_InvalidateHighlights = false;
			StartCoroutine(HighlightCardAbilityLines());
		}
	}

	private IEnumerator HighlightCardAbilityLines()
	{
		yield return null;
		List<KeyValuePair<RectTransform, EnhancementButtonBase>> highLightedElements = GetHighlightableObjects(m_CardSlot.CardUI);
		ResetHighlights();
		UIHelperUtil.UpdateUIList(highLightedElements.Count, n_HighlightAbilityPool, HighlightAbilityPrototype, HighlightAbilityParent, onShow);
		void onShow(int index, UIEnhancementButtonHighlight item)
		{
			KeyValuePair<RectTransform, EnhancementButtonBase> keyValuePair = highLightedElements[index];
			RectTransform key = keyValuePair.Key;
			EnhancementButtonBase value = keyValuePair.Value;
			item.transform.SetParent(key);
			item.transform.localScale = Vector3.one;
			item.gameObject.SetActive(value: true);
			item.SetMode(isBuy: true);
			if (IsFocusedSlot(value))
			{
				item.HighlightSelected(key, value);
			}
			else
			{
				item.HighlightPreview(key, value);
			}
		}
	}

	private void PreviewSelectedEnhancementIcon()
	{
		if (m_Param.enhancement != null)
		{
			PreviewEnhancement(GetEnhancementButton(m_Param.slot), CrossReference.EnhancementDTOToEEnhancement(m_Param.enhancement));
		}
	}

	private void HighlightEnhancementSlot(UICardEnhancementElement.Params target, bool highlighted)
	{
		HighlightEnhancementSlot(target.card, target.slot, target.enhancement, highlighted);
	}

	public void HighlightEnhancementSlot(CardDTO targetCard, CardEnhancementSlotDTO enhancementSlot, CardEnhancementDTO enhancement, bool highlighted)
	{
		if (targetCard == null || m_CardSlot.CardUI == null || m_CardSlot.CardUI.CardID != targetCard.Id)
		{
			return;
		}
		for (int i = 0; i < m_SelectedEnhancementButtonsCache.Count; i++)
		{
			EnhancementButtonBase enhancementButtonBase = m_SelectedEnhancementButtonsCache[i];
			if (enhancementButtonBase.Ability != null && !(enhancementButtonBase.Ability.Name != enhancementSlot.AbilityName) && enhancementButtonBase.EnhancementLine == (EEnhancementLine)enhancementSlot.EnhancementLine && enhancementButtonBase.EnhancementSlot == (int)enhancementSlot.SlotIndex && enhancementButtonBase.Enhancement.Enhancement == EEnhancement.NoEnhancement)
			{
				(enhancementButtonBase as EnhancementButton)?.TogglePermanentHighlight(highlighted);
				break;
			}
		}
	}

	public void PreviewEnhancement(EnhancementButtonBase button, EEnhancement enhancement)
	{
		if (button is EnhancementButton enhancementButton)
		{
			enhancementButton.UpdateEnhancement(enhancement);
		}
		else if (button is EnhancedAreaHex enhancedAreaHex)
		{
			switch (enhancement)
			{
			case EEnhancement.NoEnhancement:
				enhancedAreaHex.RemoveEnhancement();
				break;
			case EEnhancement.Area:
				enhancedAreaHex.ApplyEnhancement();
				break;
			}
		}
	}

	private bool IsFocusedSlot(EnhancementButtonBase button)
	{
		if (button == null || m_Param.slot == null)
		{
			return false;
		}
		if (button.Ability.Name == m_Param.slot.AbilityName)
		{
			return button.EnhancementLine == (EEnhancementLine)m_Param.slot.EnhancementLine;
		}
		return false;
	}

	private EnhancementButtonBase GetEnhancementButton(CardEnhancementSlotDTO slot)
	{
		for (int i = 0; i < m_SelectedEnhancementButtonsCache.Count; i++)
		{
			EnhancementButtonBase enhancementButtonBase = m_SelectedEnhancementButtonsCache[i];
			if (enhancementButtonBase.Ability.Name == slot.AbilityName && enhancementButtonBase.EnhancementLine == (EEnhancementLine)slot.EnhancementLine)
			{
				return enhancementButtonBase;
			}
		}
		return null;
	}

	private List<KeyValuePair<RectTransform, EnhancementButtonBase>> GetHighlightableObjects(AbilityCardUI card)
	{
		IEnumerable<EnhancementButtonBase> enumerable;
		if (!(card == null))
		{
			enumerable = card.EnhancementButtons.Cast<EnhancementButtonBase>().Concat(card.EnhancedAreaHexs);
		}
		else
		{
			IEnumerable<EnhancementButtonBase> enumerable2 = new EnhancementButtonBase[0];
			enumerable = enumerable2;
		}
		Dictionary<RectTransform, EnhancementButtonBase> dictionary = new Dictionary<RectTransform, EnhancementButtonBase>();
		foreach (EnhancementButtonBase item in enumerable)
		{
			if (item.ParentContainer == null || !item.ParentContainer.gameObject.activeInHierarchy || item.ParentContainer.rect.width == 0f || item.ParentContainer.rect.height == 0f)
			{
				item.ParentContainer = FindParentContainer(item);
			}
			if (item.gameObject.activeSelf && !(item.ParentContainer == null) && item.Enhancement != null)
			{
				dictionary[item.ParentContainer] = item;
			}
		}
		return dictionary.ToList();
	}

	private RectTransform FindParentContainer(EnhancementButtonBase button)
	{
		AbilityCardUIAbilityRow abilityCardUIAbilityRow = UIUtility.FindInParents<AbilityCardUIAbilityRow>(button.gameObject);
		if (abilityCardUIAbilityRow != null)
		{
			return abilityCardUIAbilityRow.transform as RectTransform;
		}
		RectTransform rectTransform = button.transform.parent as RectTransform;
		while (!(rectTransform == null) && (!(rectTransform.rect.width > 0f) || !(rectTransform.rect.height > 0f)) && !rectTransform.name.Contains("topAction") && !rectTransform.name.Contains("bоttomAction"))
		{
			rectTransform = rectTransform.parent as RectTransform;
			if (!(rectTransform.parent != null))
			{
				break;
			}
		}
		return rectTransform;
	}

	private void CacheEnhancementButtonsForSelected()
	{
		m_SelectedEnhancementButtonsCache.Clear();
		if (!(m_CardSlot == null) && !(m_CardSlot.CardUI == null))
		{
			m_SelectedEnhancementButtonsCache.AddRange(m_CardSlot.CardUI.EnhancementButtons.Cast<EnhancementButtonBase>().Concat(m_CardSlot.CardUI.EnhancedAreaHexs));
		}
	}

	private void ResetHighlights()
	{
		for (int i = 0; i < n_HighlightAbilityPool.Count; i++)
		{
			n_HighlightAbilityPool[i].gameObject.SetActive(value: false);
			n_HighlightAbilityPool[i].transform.SetParent(HighlightAbilityParent);
		}
	}

	private void OnDisable()
	{
		ResetHighlights();
		m_SelectedEnhancementButtonsCache.Clear();
		m_CardSlot = null;
	}
}

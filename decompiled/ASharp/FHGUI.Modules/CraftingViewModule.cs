using System;
using System.Collections.Generic;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class CraftingViewModule : FHMapViewModule
{
	[Header("Components")]
	[SerializeField]
	private FHButton m_CraftButton;

	[SerializeField]
	private TMP_Text m_ItemName;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private TMP_Text m_ProcessDescription;

	[SerializeField]
	private UIOutpostItemElement m_ItemIcon;

	[SerializeField]
	private RectTransform Container_Ingredients;

	[SerializeField]
	private TMP_Text m_CraftHints;

	[SerializeField]
	private GameObject[] m_Links;

	[SerializeField]
	private GameObject[] m_Glows;

	[Header("Settings")]
	[SerializeField]
	private float m_ActiveCanvasAlpha = 1f;

	[SerializeField]
	private float m_InactiveCanvasAlpha = 0.2f;

	[Header("Events")]
	[SerializeField]
	private RectTransform m_TooltipPos;

	[SerializeField]
	private TextAnchor m_TooltipAnchor;

	private List<UIOutpostCraftingIngredientElement> m_IngredientsIcons;

	private Action<ItemDataDTO> m_OnCraftCallback;

	private TooltipPositionParams m_TooltipPosParams;

	private bool m_Initialized;

	public ItemDataDTO Item { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_CraftButton.onClick.AddListener(HandleOnCraft);
			m_IngredientsIcons = new List<UIOutpostCraftingIngredientElement>(Container_Ingredients.childCount);
			Container_Ingredients.GetComponentsInChildren(includeInactive: true, m_IngredientsIcons);
			m_TooltipPosParams = new TooltipPositionParams
			{
				Pos = m_TooltipPos.position,
				Anchor = m_TooltipAnchor
			};
		}
	}

	public void Init(ItemDataDTO item, Action<ItemDataDTO> onCraftCallback, IEnumerable<string> hints, bool canCraft)
	{
		InitCache();
		Item = item;
		m_OnCraftCallback = onCraftCallback;
		PopulateItem(Item);
		PopulateIngredients(Item);
		UpdateHints(hints);
		m_CraftButton.interactable = canCraft;
		if (m_ProcessDescription != null)
		{
			m_ProcessDescription.gameObject.SetActive(item == null);
		}
	}

	private void PopulateItem(ItemDataDTO item)
	{
		if ((bool)m_ItemIcon)
		{
			TooltipPositionParams value = new TooltipPositionParams
			{
				Pos = m_TooltipPos.position,
				Anchor = m_TooltipAnchor
			};
			m_ItemIcon.SetItem(item, null, new FHItemTooltip.ItemTooltipParams(item), value);
		}
		if ((bool)m_ItemName)
		{
			m_ItemName.text = item?.NameLocalized;
		}
		if ((bool)m_Description)
		{
			m_Description.text = item?.DescriptionLocalized;
		}
		GameObject[] glows = m_Glows;
		for (int i = 0; i < glows.Length; i++)
		{
			glows[i].SetActive(item != null);
		}
	}

	private void PopulateIngredients(ItemDataDTO item)
	{
		int num = 0;
		if (item != null)
		{
			if (item.CraftCost.GoldCost > 0)
			{
				UIOutpostCraftingIngredientElement.Params data = new UIOutpostCraftingIngredientElement.Params
				{
					Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
					NeededValue = item.CraftCost.GoldCost,
					AvailableValue = item.CraftCost.CharacterGold,
					Available = item.CraftCost.CanAffordGoldCost
				};
				m_IngredientsIcons[num].Init(data);
				ApplyAlphaEffect(m_IngredientsIcons[num], active: true);
				num++;
			}
			foreach (ItemDataDTO requiredItem in item.CraftCost.RequiredItems)
			{
				UIOutpostCraftingIngredientElement.Params data2 = new UIOutpostCraftingIngredientElement.Params
				{
					Item = requiredItem,
					tooltipParams = new FHItemTooltip.ItemTooltipParams(requiredItem),
					tooltipPosParams = m_TooltipPosParams,
					Available = true
				};
				m_IngredientsIcons[num].Init(data2);
				ApplyAlphaEffect(m_IngredientsIcons[num], active: true);
				num++;
			}
			foreach (ItemCraftCostDTO.MaterialCost item2 in item.CraftCost.GetMaterialCost())
			{
				UIOutpostCraftingIngredientElement.Params data3 = new UIOutpostCraftingIngredientElement.Params
				{
					Available = item2.Available,
					Icon = UIInfoTools.Instance.GetResourceConfig(item2.Type.ToString()).Icon,
					Type = item2.Type,
					NeededValue = item2.Amount,
					AvailableValue = item.CraftCost.CharacterResources.GetMaterialAmount(item2.Type),
					NameLocalized = LocalizationManager.GetTranslation(FHUILocKeys.GetMaterialLocalizationKey(item2.Type))
				};
				m_IngredientsIcons[num].Init(data3);
				ApplyAlphaEffect(m_IngredientsIcons[num], active: true);
				num++;
			}
			foreach (ItemCraftCostDTO.HerbCost item3 in item.CraftCost.GetHerbsCost())
			{
				UIOutpostCraftingIngredientElement.Params data4 = new UIOutpostCraftingIngredientElement.Params
				{
					Available = item3.Available,
					Icon = UIInfoTools.Instance.GetResourceConfig(item3.Type.ToString()).Icon,
					Type = item3.Type,
					NeededValue = item3.Amount,
					AvailableValue = item.CraftCost.OutpostResources.GetHerbAmount(item3.Type),
					NameLocalized = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(item3.Type))
				};
				m_IngredientsIcons[num].Init(data4);
				ApplyAlphaEffect(m_IngredientsIcons[num], active: true);
				num++;
			}
		}
		if (num < m_IngredientsIcons.Count)
		{
			for (int i = num; i < m_IngredientsIcons.Count; i++)
			{
				m_IngredientsIcons[i].Init(default);
				ApplyAlphaEffect(m_IngredientsIcons[i], active: false);
			}
		}
		if (m_Links != null)
		{
			for (int j = 0; j < m_Links.Length; j++)
			{
				m_Links[j].gameObject.SetActive(num > j);
			}
		}
		void ApplyAlphaEffect(UIOutpostCraftingIngredientElement icon, bool active)
		{
			CanvasGroup component = icon.GetComponent<CanvasGroup>();
			if ((bool)component)
			{
				component.alpha = (active ? m_ActiveCanvasAlpha : m_InactiveCanvasAlpha);
			}
		}
	}

	private void UpdateHints(IEnumerable<string> hints)
	{
		if (m_CraftHints == null)
		{
			return;
		}
		string text = string.Empty;
		foreach (string hint in hints)
		{
			text += hint;
		}
		bool flag = !string.IsNullOrEmpty(text);
		m_CraftHints.enabled = flag;
		if (flag)
		{
			m_CraftHints.text = text;
		}
	}

	private void HandleOnCraft()
	{
		m_OnCraftCallback(Item);
	}
}

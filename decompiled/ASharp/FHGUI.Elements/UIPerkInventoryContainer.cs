using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using UnityEngine;

namespace FHGUI.Elements;

public class UIPerkInventoryContainer : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_CategoryContainer;

	[SerializeField]
	private UIPerkCategory m_CategoryPrototype;

	[Header("Settings")]
	[SerializeField]
	private bool m_UseMultipleCategories;

	private List<UIPerkCategory> m_Categories = new List<UIPerkCategory>();

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_CategoryContainer.GetComponentsInChildren(includeInactive: true, m_Categories);
		}
	}

	public void Init(List<PerkDTO> perks, UIPerkInteractionEvents perkInteractions)
	{
		InitCache();
		List<(PerkDTO.ActionType, List<PerkDTO>)> categoryData = new List<(PerkDTO.ActionType, List<PerkDTO>)>();
		foreach (PerkDTO perk in perks)
		{
			PerkDTO.ActionType actionType = (m_UseMultipleCategories ? perk.GetActionType() : PerkDTO.ActionType.Learn);
			int num = categoryData.FindIndex(((PerkDTO.ActionType, List<PerkDTO>) tuple) => tuple.Item1 == actionType);
			if (num == -1)
			{
				categoryData.Add((actionType, new List<PerkDTO>()));
				num = categoryData.Count - 1;
			}
			categoryData[num].Item2.Add(perk);
		}
		UIHelperUtil.UpdateUIList(categoryData.Count, m_Categories, m_CategoryPrototype, m_CategoryContainer, onShow);
		void onShow(int index, UIPerkCategory item)
		{
			PerkDTO.ActionType item2 = categoryData[index].Item1;
			List<PerkDTO> item3 = categoryData[index].Item2;
			item.Init(item3, perkInteractions, GetCategoryLabelKey(item2));
		}
	}

	private static string GetCategoryLabelKey(PerkDTO.ActionType catType)
	{
		return catType switch
		{
			PerkDTO.ActionType.Learn => "GUI_PERKS_CAT_LEARN", 
			PerkDTO.ActionType.Improve => "GUI_PERKS_CAT_IMPROVE", 
			PerkDTO.ActionType.Forget => "GUI_PERKS_CAT_FORGET", 
			_ => throw new ArgumentOutOfRangeException("catType", catType, null), 
		};
	}
}

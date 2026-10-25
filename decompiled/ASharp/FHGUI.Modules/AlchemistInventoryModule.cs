using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class AlchemistInventoryModule : FHMapViewModule
{
	[Flags]
	public enum HerbsFilter
	{
		Arrowvine = 1,
		Axenut = 2,
		Corpsecap = 4,
		Flamefruit = 8,
		Rockroot = 0x10,
		Snowthistle = 0x20,
		Purchasable = 0x40,
		Researchable = 0x80,
		None = 0,
		All = Arrowvine | Axenut | Corpsecap | Flamefruit | Rockroot | Snowthistle
	}

	public class FilterParams
	{
		public Action<HerbsFilter> OnFilterStateUpdated;

		public HerbsFilter ActiveFilters;
	}

	[Header("Components")]
	[SerializeField]
	private AlchemistInventoryContainer m_Recipes;

	[SerializeField]
	private TextLocalizedListener m_CaptionLabel;

	[SerializeField]
	private TMP_Text m_BuildingLevel;

	[SerializeField]
	private TextLocalizedListener m_EmptyInvetoryLabel;

	[SerializeField]
	private UIFilterGroup m_HerbsFilter;

	private bool m_InvalidateFilter;

	private Action<HerbsFilter> _onFilterStateUpdated;

	private void Awake()
	{
		if ((bool)m_CaptionLabel)
		{
			m_CaptionLabel.SetTextKey("BUILDING_Name_35");
		}
		if ((bool)m_EmptyInvetoryLabel)
		{
			m_EmptyInvetoryLabel.SetTextKey("GUI_GENERIC_EMPTY_INVENTORY");
		}
		if ((bool)m_HerbsFilter)
		{
			UIFilterGroup herbsFilter = m_HerbsFilter;
			herbsFilter.OnFilter = (Action<Enum>)Delegate.Combine(herbsFilter.OnFilter, new Action<Enum>(SetFilter));
			UIFilterGroup herbsFilter2 = m_HerbsFilter;
			herbsFilter2.OnStateUpdated = (Action<Enum>)Delegate.Combine(herbsFilter2.OnStateUpdated, new Action<Enum>(CallOnFilterStateUpdated));
			m_HerbsFilter.SetFilters(GetInventoryFilterParams());
			m_HerbsFilter.UpdateState(HerbsFilter.All);
		}
	}

	public void Init(AlchemistInventoryContainer.ListParams listParams, List<AlchemistInventoryContainer.SlotData> researchItemsParams, List<AlchemistInventoryContainer.SlotData> recipesItemsParams, FilterParams filterParams)
	{
		if ((bool)m_BuildingLevel)
		{
			m_BuildingLevel.text = listParams.BuildingLevel.ToString();
		}
		List<AlchemistInventoryContainer.SlotData> list = new List<AlchemistInventoryContainer.SlotData>();
		list.AddRange(recipesItemsParams);
		list.AddRange(researchItemsParams);
		m_Recipes.Init(listParams, list);
		if ((bool)m_HerbsFilter)
		{
			_onFilterStateUpdated = null;
			m_HerbsFilter.UpdateState(filterParams.ActiveFilters);
			_onFilterStateUpdated = filterParams.OnFilterStateUpdated;
			UpdateFilter();
		}
		if ((bool)m_EmptyInvetoryLabel)
		{
			m_EmptyInvetoryLabel.gameObject.SetActive(!m_Recipes.Elements.Any((UIAlchemistInventoryElement element) => element.IsActive()));
		}
	}

	private void SetFilter(Enum newFilter)
	{
		if (!object.Equals(m_HerbsFilter.ActiveFilter, newFilter))
		{
			m_HerbsFilter.UpdateState(newFilter);
			m_InvalidateFilter = true;
		}
	}

	private void UpdateFilter()
	{
		if (m_HerbsFilter == null)
		{
			return;
		}
		HerbsFilter current = (HerbsFilter)(object)m_HerbsFilter.ActiveFilter;
		foreach (UIAlchemistInventoryElement element in m_Recipes.Elements)
		{
			element.gameObject.SetActive(CheckVisibility(element, current));
		}
		bool CheckVisibility(UIAlchemistInventoryElement e, HerbsFilter filter)
		{
			if (e == null)
			{
				return false;
			}
			if (e.Data == null)
			{
				return false;
			}
			if (current == HerbsFilter.All)
			{
				return true;
			}
			if (current == HerbsFilter.Researchable && e.Data.RecipeDto.Hidden)
			{
				return true;
			}
			if (current == HerbsFilter.Purchasable && e.Data.RecipeDto.Hidden)
			{
				return false;
			}
			if (current == HerbsFilter.Purchasable && e.Data.CanBeBrewed && !e.Data.Owned)
			{
				return true;
			}
			if (e.Data.RecipeDto.Herb1.ToString() == current.ToString())
			{
				return true;
			}
			if (e.Data.RecipeDto.Herb2.ToString() == current.ToString())
			{
				return true;
			}
			if (e.Data.RecipeDto.Herb3.ToString() == current.ToString())
			{
				return true;
			}
			return false;
		}
	}

	protected void LateUpdate()
	{
		if (m_InvalidateFilter && !CanvasUpdateRegistry.IsRebuildingLayout())
		{
			UpdateFilter();
			m_InvalidateFilter = false;
		}
	}

	public bool TrySelect(ItemID recipeID, int cloneIndex)
	{
		ClearSelection();
		if (m_Recipes.HasItem(recipeID))
		{
			m_Recipes.TrySelectItem(recipeID, cloneIndex);
			return true;
		}
		return false;
	}

	public bool TrySelect(BrewableRecipeDTO recipeDto)
	{
		ClearSelection();
		if (m_Recipes.HasItem(recipeDto))
		{
			m_Recipes.TrySelectItem(recipeDto);
			return true;
		}
		return false;
	}

	public void ClearSelection()
	{
		m_Recipes.ClearSelection();
	}

	public static UIFilterGroup.FilterParam[] GetInventoryFilterParams()
	{
		return new UIFilterGroup.FilterParam[9]
		{
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_UI_ICO_ItemSlot_Any"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_UI_ICO_ItemSlot_Any_ON"),
				TooltipKey = "GUI_INVENTORY_SLOT_ALL",
				Filter = HerbsFilter.All
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Available_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Available_ON"),
				TooltipKey = "GUI_INVENTORY_AVAILABLE",
				Filter = HerbsFilter.Purchasable
			},
			new UIFilterGroup.FilterParam
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Arrowvine.ToString()).CategorySelected,
				IconInactive = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Arrowvine.ToString()).CategoryNormal,
				TooltipKey = "RESOURCE_Arrowvine",
				Filter = HerbsFilter.Arrowvine
			},
			new UIFilterGroup.FilterParam
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Axenut.ToString()).CategorySelected,
				IconInactive = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Axenut.ToString()).CategoryNormal,
				TooltipKey = "RESOURCE_Axenut",
				Filter = HerbsFilter.Axenut
			},
			new UIFilterGroup.FilterParam
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Corpsecap.ToString()).CategorySelected,
				IconInactive = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Corpsecap.ToString()).CategoryNormal,
				TooltipKey = "RESOURCE_Corpsecap",
				Filter = HerbsFilter.Corpsecap
			},
			new UIFilterGroup.FilterParam
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Flamefruit.ToString()).CategorySelected,
				IconInactive = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Flamefruit.ToString()).CategoryNormal,
				TooltipKey = "RESOURCE_Flamefruit",
				Filter = HerbsFilter.Flamefruit
			},
			new UIFilterGroup.FilterParam
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Rockroot.ToString()).CategorySelected,
				IconInactive = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Rockroot.ToString()).CategoryNormal,
				TooltipKey = "RESOURCE_Rockroot",
				Filter = HerbsFilter.Rockroot
			},
			new UIFilterGroup.FilterParam
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Snowthistle.ToString()).CategorySelected,
				IconInactive = UIInfoTools.Instance.GetResourceConfig(HerbsFilter.Snowthistle.ToString()).CategoryNormal,
				TooltipKey = "RESOURCE_Snowthistle",
				Filter = HerbsFilter.Snowthistle
			},
			new UIFilterGroup.FilterParam
			{
				IconInactive = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Researchable_OFF"),
				Icon = UIInfoTools.Instance.GetGenericIcon("FH_Filter_Researchable_ON"),
				TooltipKey = "GUI_ALCHEMIST_RESEARCH",
				Filter = HerbsFilter.Researchable
			}
		};
	}

	private void CallOnFilterStateUpdated(Enum activeFilter)
	{
		_onFilterStateUpdated?.Invoke((HerbsFilter)(object)activeFilter);
	}

	protected override void OnHide()
	{
		base.OnHide();
	}
}

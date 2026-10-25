using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class AlchemistInventoryContainer : ListContainerBase<UIAlchemistInventoryElement, AlchemistInventoryContainer.ListParams, AlchemistInventoryContainer.SlotData>
{
	public class ListParams
	{
		public UIAlchemistItemElementInteractionEvents ElementInteractionEvents;

		public UIAlchemistInventoryElement.ItemTooltipHintsFunc TooltipHints;

		public UIAlchemistInventoryElement.ItemTooltipCostFunc TooltipCost;

		public Action<UIAlchemistInventoryElement.Params> OnInventoryItemDeselected;

		public int BuildingLevel;
	}

	public class SlotData
	{
		public OutpostResources AvailableResources;

		public BrewableRecipeDTO RecipeDto;

		public bool CanBeBrewed;

		public bool OwnedInstance;

		public bool HasResources;

		public int CloneIndex;
	}

	[Header("Elements")]
	[SerializeField]
	private ScrollRect m_ScrollRect;

	[Header("Tooltips")]
	[SerializeField]
	private RectTransform m_TooltipPos;

	[SerializeField]
	private TextAnchor m_TooltipAnchor;

	protected override void InitElement(UIAlchemistInventoryElement partySlotComponent, SlotData slotData, ListParams listParams)
	{
		UIAlchemistInventoryElement.Params slotParams = new UIAlchemistInventoryElement.Params
		{
			RecipeDto = slotData.RecipeDto,
			AvailableResources = slotData.AvailableResources,
			CanBeBrewed = slotData.CanBeBrewed,
			Owned = slotData.OwnedInstance,
			HasResources = slotData.HasResources,
			ElementInteractions = listParams.ElementInteractionEvents,
			TooltipHints = listParams.TooltipHints,
			TooltipCost = listParams.TooltipCost,
			OnDeselect = listParams.OnInventoryItemDeselected,
			CloneIndex = slotData.CloneIndex,
			PotionTooltipPosParams = new TooltipPositionParams
			{
				Pos = m_TooltipPos.position,
				Anchor = m_TooltipAnchor
			}
		};
		partySlotComponent.Init(slotParams);
	}

	public bool HasItem(ItemID recipeID)
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			if (listElement.Data != null && listElement.Data.RecipeDto.ID == recipeID)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasItem(BrewableRecipeDTO recipeDto)
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			if (listElement.Data != null && listElement.Data.RecipeDto == recipeDto)
			{
				return true;
			}
		}
		return false;
	}

	public bool TrySelectItem(ItemID recipeID, int cloneIndex, bool focusItem = true)
	{
		UIAlchemistInventoryElement uIAlchemistInventoryElement = FindItems(recipeID).Skip(cloneIndex).First();
		if (uIAlchemistInventoryElement == null)
		{
			return false;
		}
		SelectItem(uIAlchemistInventoryElement);
		return true;
	}

	public bool TrySelectItem(BrewableRecipeDTO recipeDto, bool focusItem = true)
	{
		UIAlchemistInventoryElement uIAlchemistInventoryElement = FindItem(recipeDto);
		if (uIAlchemistInventoryElement == null)
		{
			return false;
		}
		SelectItem(uIAlchemistInventoryElement, focusItem);
		return true;
	}

	private UIAlchemistInventoryElement FindItem(BrewableRecipeDTO recipeDto)
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			if (listElement.Data != null && listElement.Data.RecipeDto == recipeDto)
			{
				return listElement;
			}
		}
		return null;
	}

	private UIAlchemistInventoryElement FindItem(ItemID recipeID)
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			if (listElement.Data != null && listElement.Data.RecipeDto.ID == recipeID)
			{
				return listElement;
			}
		}
		return null;
	}

	private IEnumerable<UIAlchemistInventoryElement> FindItems(ItemID recipeID)
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			if (listElement.Data != null && listElement.Data.RecipeDto.ID == recipeID)
			{
				yield return listElement;
			}
		}
	}

	private void SelectItem(UIAlchemistInventoryElement selectee, bool focusItem = true)
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			if (listElement == selectee)
			{
				listElement.Select();
				if (focusItem)
				{
					m_ScrollRect?.ScrollToFit(listElement.transform as RectTransform);
				}
			}
			else
			{
				listElement.Deselect();
			}
		}
	}

	public void ClearSelection()
	{
		foreach (UIAlchemistInventoryElement listElement in m_ListElements)
		{
			listElement.Deselect();
		}
	}
}

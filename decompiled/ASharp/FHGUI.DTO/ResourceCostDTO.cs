using System.Collections.Generic;
using System.Linq;
using FHRL;
using UnityEngine;

namespace FHGUI.DTO;

public class ResourceCostDTO
{
	public struct ItemData
	{
		public Sprite Icon;

		public int Value;

		public bool Available;
	}

	private List<ItemData> Items;

	public ItemData this[int i] => Items[i];

	public int Count => Items?.Count ?? 0;

	public bool CanAfford
	{
		get
		{
			if (Items != null)
			{
				return !Items.Any((ItemData data) => !data.Available);
			}
			return true;
		}
	}

	public bool IsEmpty()
	{
		if (Items != null)
		{
			return Items.Count == 0;
		}
		return true;
	}

	public ResourceCostDTO()
	{
	}

	public ResourceCostDTO(ItemCraftCostDTO craftCost)
	{
		if (craftCost.GoldCost > 0)
		{
			AddGoldCost(craftCost.GoldCost, craftCost.CharacterGold);
		}
		foreach (ItemDataDTO requiredItem in craftCost.RequiredItems)
		{
			AddItemCost(requiredItem);
		}
		AddResourceCost(craftCost.ResourceCost, craftCost.CharacterResources + craftCost.OutpostResources);
	}

	public ResourceCostDTO(OutpostResources cost, OutpostResources available = default(OutpostResources))
	{
		AddResourceCost(cost, available);
	}

	private void AddResourceCost(OutpostResources cost, OutpostResources available = default(OutpostResources))
	{
		if (cost.Metal > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Metal").Icon, cost.Metal, cost.Metal <= available.Metal);
		}
		if (cost.Lumber > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Lumber").Icon, cost.Lumber, cost.Lumber <= available.Lumber);
		}
		if (cost.Hide > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Hide").Icon, cost.Hide, cost.Hide <= available.Hide);
		}
		if (cost.Arrowvine > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Arrowvine").Icon, cost.Arrowvine, cost.Arrowvine <= available.Arrowvine);
		}
		if (cost.Axenut > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Axenut").Icon, cost.Axenut, cost.Axenut <= available.Axenut);
		}
		if (cost.Corpsecap > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Corpsecap").Icon, cost.Corpsecap, cost.Corpsecap <= available.Corpsecap);
		}
		if (cost.Flamefruit > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Flamefruit").Icon, cost.Flamefruit, cost.Flamefruit <= available.Flamefruit);
		}
		if (cost.Rockroot > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Rockroot").Icon, cost.Rockroot, cost.Rockroot <= available.Rockroot);
		}
		if (cost.Snowthistle > 0)
		{
			AddElement(UIInfoTools.Instance.GetResourceConfig("Snowthistle").Icon, cost.Snowthistle, cost.Snowthistle <= available.Snowthistle);
		}
	}

	private void AddGoldCost(int cost, int available)
	{
		AddElement(UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized, cost, cost <= available);
	}

	private void AddItemCost(ItemDataDTO item)
	{
		AddElement(item.Icon, 1, available: true);
	}

	public void AddElement(Sprite sprite, int value, bool available, bool asStack = true)
	{
		if (Items == null)
		{
			Items = new List<ItemData>();
		}
		if (asStack)
		{
			Items.Add(new ItemData
			{
				Icon = sprite,
				Value = value,
				Available = available
			});
			return;
		}
		for (int i = 0; i < value; i++)
		{
			Items.Add(new ItemData
			{
				Icon = sprite,
				Value = 1,
				Available = available
			});
		}
	}
}

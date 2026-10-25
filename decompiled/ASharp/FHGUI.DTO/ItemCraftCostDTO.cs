using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO;

public class ItemCraftCostDTO
{
	public class MaterialCost
	{
		public OutpostMaterialType Type;

		public int Amount;

		public bool Available;

		public MaterialCost(OutpostMaterialType t, int amount, bool available)
		{
			Type = t;
			Amount = amount;
			Available = available;
		}

		public static MaterialCost Create(OutpostMaterialType t, int amount, OutpostResources availableResources)
		{
			return new MaterialCost(t, amount, availableResources.HasMaterial(t, amount));
		}
	}

	public class HerbCost
	{
		public OutpostHerbType Type;

		public int Amount;

		public bool Available;

		public HerbCost(OutpostHerbType t, int amount, bool available)
		{
			Type = t;
			Amount = amount;
			Available = available;
		}

		public static HerbCost Create(OutpostHerbType t, int amount, OutpostResources availableResources)
		{
			return new HerbCost(t, amount, availableResources.HasHerb(t, amount));
		}
	}

	public int GoldCost;

	public int CharacterGold;

	public bool CanAffordGoldCost;

	public OutpostResources ResourceCost;

	public OutpostResources CharacterResources;

	public OutpostResources OutpostResources;

	public List<ItemDataDTO> RequiredItems;

	public IEnumerable<MaterialCost> GetMaterialCost()
	{
		if (ResourceCost.Metal > 0)
		{
			yield return MaterialCost.Create(OutpostMaterialType.Metal, ResourceCost.Metal, CharacterResources);
		}
		if (ResourceCost.Lumber > 0)
		{
			yield return MaterialCost.Create(OutpostMaterialType.Lumber, ResourceCost.Lumber, CharacterResources);
		}
		if (ResourceCost.Hide > 0)
		{
			yield return MaterialCost.Create(OutpostMaterialType.Hide, ResourceCost.Hide, CharacterResources);
		}
	}

	public IEnumerable<HerbCost> GetHerbsCost()
	{
		if (ResourceCost.Arrowvine > 0)
		{
			yield return HerbCost.Create(OutpostHerbType.Arrowvine, ResourceCost.Arrowvine, OutpostResources);
		}
		if (ResourceCost.Axenut > 0)
		{
			yield return HerbCost.Create(OutpostHerbType.Axenut, ResourceCost.Axenut, OutpostResources);
		}
		if (ResourceCost.Corpsecap > 0)
		{
			yield return HerbCost.Create(OutpostHerbType.Corpsecap, ResourceCost.Corpsecap, OutpostResources);
		}
		if (ResourceCost.Flamefruit > 0)
		{
			yield return HerbCost.Create(OutpostHerbType.Flamefruit, ResourceCost.Flamefruit, OutpostResources);
		}
		if (ResourceCost.Rockroot > 0)
		{
			yield return HerbCost.Create(OutpostHerbType.Rockroot, ResourceCost.Rockroot, OutpostResources);
		}
		if (ResourceCost.Snowthistle > 0)
		{
			yield return HerbCost.Create(OutpostHerbType.Snowthistle, ResourceCost.Snowthistle, OutpostResources);
		}
	}
}

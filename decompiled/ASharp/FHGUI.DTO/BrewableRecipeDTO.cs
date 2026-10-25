using System.Collections.Generic;
using FHRL;
using UnityEngine;

namespace FHGUI.DTO;

public class BrewableRecipeDTO
{
	public ItemID ID;

	public Sprite Icon;

	public string NameKey;

	public ItemDataDTO Potion;

	public OutpostHerbType Herb1;

	public OutpostHerbType Herb2;

	public OutpostHerbType Herb3;

	public bool IsPoison;

	public bool Is3HerbRecipe;

	public bool CanBeDistilled;

	public int RemainingCrafts;

	public bool Hidden;

	public int TotalCrafts => Potion?.MaxQuantity ?? (-1);

	public OutpostResources HerbResources => new OutpostResources(GetHerbs());

	public string NameLocalized => Potion?.NameLocalized ?? "";

	public IEnumerable<OutpostHerbType> GetHerbs()
	{
		if (Herb1 != OutpostHerbType.None)
		{
			yield return Herb1;
		}
		if (Herb2 != OutpostHerbType.None)
		{
			yield return Herb2;
		}
		if (Herb3 != OutpostHerbType.None)
		{
			yield return Herb3;
		}
	}

	public BrewableRecipeDTO()
	{
	}

	public BrewableRecipeDTO(BrewableRecipe def, int remaining, bool hidden)
		: this(def, hidden)
	{
		RemainingCrafts = remaining;
	}

	public BrewableRecipeDTO(BrewableRecipe def, int remaining)
		: this(def)
	{
		RemainingCrafts = remaining;
	}

	public BrewableRecipeDTO(BrewableRecipe def, bool hidden)
		: this(def)
	{
		Hidden = hidden;
	}

	public BrewableRecipeDTO(BrewableRecipe def)
	{
		ID = def.Item.ID;
		Potion = new ItemDataDTO(def.Item);
		Herb1 = def.BrewingInfo.Herb1;
		Herb2 = def.BrewingInfo.Herb2;
		Herb3 = def.BrewingInfo.Herb3;
		Is3HerbRecipe = def.BrewingInfo.Is3HerbRecipe;
		CanBeDistilled = def.BrewingInfo.CanBeDistilled;
		NameKey = Potion.NameKey;
		Icon = Potion.Icon;
		IsPoison = def.IsAnyRecipe || (Herb1 == Herb2 && Herb1 != OutpostHerbType.None) || (Herb1 == Herb3 && Herb1 != OutpostHerbType.None) || (Herb2 == Herb3 && Herb1 != OutpostHerbType.None);
	}

	public OutpostHerbType GetHerbAt(int index)
	{
		return index switch
		{
			0 => Herb1, 
			1 => Herb2, 
			2 => Herb3, 
			_ => OutpostHerbType.None, 
		};
	}

	public override string ToString()
	{
		return "BrewableRecipeDTO [Potion: " + ((Potion != null) ? $"{Potion.NameKey}({Potion.ID})" : "None") + "," + $" Herbs: ({Herb1}, {Herb2}.{Herb3})," + $" Is Poison {IsPoison}, Distillable {CanBeDistilled}, RemainingCrafts {RemainingCrafts}]";
	}
}

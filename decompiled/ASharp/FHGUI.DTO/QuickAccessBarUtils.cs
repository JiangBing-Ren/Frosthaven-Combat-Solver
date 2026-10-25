using System;

namespace FHGUI.DTO;

public static class QuickAccessBarUtils
{
	public static MainViewMainCategories GetContext(MainViewSubCategories subcategory)
	{
		return subcategory switch
		{
			MainViewSubCategories.Craftsman => MainViewMainCategories.Outpost, 
			MainViewSubCategories.Merchant => MainViewMainCategories.Outpost, 
			MainViewSubCategories.TravellingMerchant => MainViewMainCategories.Outpost, 
			MainViewSubCategories.Alchemy => MainViewMainCategories.Outpost, 
			MainViewSubCategories.Enhancer => MainViewMainCategories.Outpost, 
			MainViewSubCategories.Defense => MainViewMainCategories.Outpost, 
			MainViewSubCategories.CharacterManagement => MainViewMainCategories.Characters, 
			MainViewSubCategories.HandManagement => MainViewMainCategories.Characters, 
			MainViewSubCategories.NewCards => MainViewMainCategories.Characters, 
			MainViewSubCategories.Perks => MainViewMainCategories.Characters, 
			MainViewSubCategories.WorldQuests => MainViewMainCategories.WorldMap, 
			MainViewSubCategories.PuzzleBook => MainViewMainCategories.WorldMap, 
			MainViewSubCategories.ActiveCharacters => MainViewMainCategories.Roster, 
			MainViewSubCategories.RetiredCharacters => MainViewMainCategories.Roster, 
			MainViewSubCategories.JobPostings => MainViewMainCategories.WorldMap, 
			MainViewSubCategories.None => MainViewMainCategories.Characters, 
			_ => throw new ArgumentOutOfRangeException("subcategory", subcategory, null), 
		};
	}

	public static MainViewSubCategories GetDefaultSubcategory(MainViewMainCategories context)
	{
		return context switch
		{
			MainViewMainCategories.None => MainViewSubCategories.None, 
			MainViewMainCategories.Outpost => MainViewSubCategories.None, 
			MainViewMainCategories.Characters => MainViewSubCategories.CharacterManagement, 
			MainViewMainCategories.WorldMap => MainViewSubCategories.WorldQuests, 
			MainViewMainCategories.Roster => MainViewSubCategories.ActiveCharacters, 
			_ => throw new ArgumentOutOfRangeException("context", context, null), 
		};
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using GLOOM;

namespace FHGUI.Services;

public class AlchemistViewService : FHMapViewService
{
	public bool IsDistillingEnabled => _view.GameController.CraftingController.IsDistillingEnabled;

	public bool IsBrewingEnabled => _view.GameController.CraftingController.IsBrewingEnabled;

	public bool IsAdvancedBrewingEnabled => _view.GameController.CraftingController.Is3HerbBrewingEnabled;

	public AlchemistViewService(FHMapView view)
		: base(view)
	{
	}

	public List<ItemDataDTO> GetOwnedPotions(CharacterState characterState)
	{
		List<ItemDataDTO> list = (from dto in _view.GetService<ItemViewService>().GetOwnedItems(characterState)
			where dto.IsPotion
			orderby dto.ID
			select dto).ToList();
		foreach (ItemDataDTO item in list)
		{
			ResourceCostDTO resourceCostDTO = new ResourceCostDTO();
			foreach (OutpostHerbType herb in DefRepo.Items[item.ID].Brewing.GetHerbs())
			{
				resourceCostDTO.AddElement(UIInfoTools.Instance.GetResourceConfig(herb.ToString()).IconStylized, 1, available: true);
			}
			if (item.CanBeDistilled)
			{
				item.ResourceCost = resourceCostDTO;
			}
		}
		return list;
	}

	public IEnumerable<OutpostHerbType> GetDistillableHerbOptions(ItemID id)
	{
		ItemDef def = DefRepo.Items[id];
		return GameController.CraftingController.GetDistillableHerbOptions(def);
	}

	public List<BrewableRecipeDTO> GetResearchRecipes()
	{
		List<BrewableRecipeDTO> list = new List<BrewableRecipeDTO>();
		foreach (ItemDef value in DefRepo.Items.Values)
		{
			if (!_view.GameController.IsItemValid(value))
			{
				continue;
			}
			foreach (ItemDef.BrewingInfo item2 in value.GetBrewingInfo())
			{
				BrewableRecipe def = new BrewableRecipe
				{
					Item = value,
					BrewingInfo = item2
				};
				if (!_view.GameController.CraftingController.IsPotionDiscovered(value))
				{
					BrewableRecipeDTO item = new BrewableRecipeDTO(def, hidden: true);
					list.Add(item);
				}
			}
		}
		list.Sort((BrewableRecipeDTO x, BrewableRecipeDTO y) => string.Compare(x.Potion.NameLocalized, y.Potion.NameLocalized, StringComparison.Ordinal));
		return list;
	}

	public IEnumerable<BrewableRecipeDTO> GetBrewableRecipes()
	{
		List<BrewableRecipe> brewableRecipies = GameController.CraftingController.GetBrewableRecipies();
		foreach (BrewableRecipe item in brewableRecipies)
		{
			if (!item.IsAnyRecipe)
			{
				yield return new BrewableRecipeDTO(item, _view.GameController.CraftingController.GetRecipieRemainingItems(item));
			}
		}
	}

	public List<AlchemistInventoryContainer.SlotData> GetResearchData()
	{
		bool is3HerbBrewingEnabled = _view.GameController.CraftingController.Is3HerbBrewingEnabled;
		List<AlchemistInventoryContainer.SlotData> list = new List<AlchemistInventoryContainer.SlotData>();
		foreach (BrewableRecipeDTO researchRecipe in GetResearchRecipes())
		{
			if ((is3HerbBrewingEnabled || !researchRecipe.Is3HerbRecipe) && !researchRecipe.IsPoison)
			{
				AlchemistInventoryContainer.SlotData slotData = new AlchemistInventoryContainer.SlotData();
				slotData.RecipeDto = researchRecipe;
				slotData.CanBeBrewed = CanResearch(GetBrewableRecipeFromDTO(researchRecipe));
				slotData.AvailableResources = _view.GameState.Outpost.Resources;
				slotData.HasResources = slotData.AvailableResources.Has(researchRecipe.HerbResources);
				list.Add(slotData);
			}
		}
		list.Sort((AlchemistInventoryContainer.SlotData x, AlchemistInventoryContainer.SlotData y) => x.RecipeDto.Is3HerbRecipe.CompareTo(y.RecipeDto.Is3HerbRecipe));
		return list;
	}

	public BrewableRecipe GetBrewableRecipeFromDTO(BrewableRecipeDTO dto)
	{
		return new BrewableRecipe
		{
			Item = DefRepo.Items[dto.Potion.ID],
			BrewingInfo = new ItemDef.BrewingInfo
			{
				Herb1 = dto.Herb1,
				Herb2 = dto.Herb2,
				Herb3 = dto.Herb3
			}
		};
	}

	public BrewableRecipe GetBrewableRecipeFromItemID(ItemID itemId)
	{
		BrewableRecipe result = default;
		result.Item = DefRepo.Items[itemId];
		result.BrewingInfo = result.Item.GetBrewingInfo().First();
		return result;
	}

	public List<AlchemistInventoryContainer.SlotData> GetAlchemistInventory(CharacterState characterState)
	{
		List<AlchemistInventoryContainer.SlotData> list = new List<AlchemistInventoryContainer.SlotData>();
		IEnumerable<BrewableRecipeDTO> brewableRecipes = GetBrewableRecipes();
		List<ItemDef> source = _view.GameController.CharacterController.GetOwnedItems(characterState).ToList();
		foreach (BrewableRecipeDTO recipe in brewableRecipes)
		{
			AlchemistInventoryContainer.SlotData slotData = new AlchemistInventoryContainer.SlotData();
			slotData.RecipeDto = recipe;
			slotData.AvailableResources = _view.GameState.Outpost.Resources;
			slotData.CanBeBrewed = CanBrew(null, recipe);
			slotData.OwnedInstance = source.FirstOrDefault((ItemDef i) => i.ID == recipe.Potion.ID) != null;
			slotData.HasResources = slotData.AvailableResources.Has(recipe.HerbResources);
			slotData.CloneIndex = list.Count((AlchemistInventoryContainer.SlotData slotData2) => slotData2.RecipeDto.ID == recipe.ID);
			list.Add(slotData);
		}
		list.Sort((AlchemistInventoryContainer.SlotData x, AlchemistInventoryContainer.SlotData y) => string.Compare(x.RecipeDto.NameLocalized, y.RecipeDto.NameLocalized, StringComparison.Ordinal));
		return list;
	}

	public void DistillHerb(CharacterState characterState, ItemDataDTO potionDTO, OutpostHerbType herb)
	{
		ItemDef item = DefRepo.Items[potionDTO.ID];
		DistillHerb(characterState, item, herb);
	}

	public IEnumerable<FHItemTooltip.ItemTooltipHint> GetOwnedPotionTooltipHints(ItemDataDTO itemData, CharacterState character)
	{
		ItemDef def = DefRepo.Items[itemData.ID];
		bool canBeDistilled = itemData.CanBeDistilled;
		bool canDistill = GameController.CraftingController.CanDistillItem(character, def);
		bool isDistillingEnabled = GameController.CraftingController.IsDistillingEnabled;
		if (!canBeDistilled)
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = LocalizationManager.GetTranslation("GUI_ALCHEMIST_POTION_NOTDISTILLABLE")
			};
		}
		if ((isDistillingEnabled & canBeDistilled) && !canDistill)
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = LocalizationManager.GetTranslation("GUI_ALCHEMIST_UNKNOWN_RECIPE")
			};
		}
		if (canDistill)
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = FHUILocFormatting.GetActionIconFormatLiteral(FHInputEventElement.UserInteractionType.DoubleClick, "GUI_ALCHEMIST_DISTILL")
			};
		}
	}

	public FHItemTooltip.ItemTooltipCost GetRecipeDTOTooltipCost(BrewableRecipeDTO recipe, CharacterState character)
	{
		return new FHItemTooltip.ItemTooltipCost
		{
			CostLiteral = BrewingCostToLocalizedString(character, recipe)
		};
	}

	public IEnumerable<FHItemTooltip.ItemTooltipHint> GetRecipeDTOTooltipHints(BrewableRecipeDTO recipe, CharacterState character)
	{
		ItemDef itemDef = DefRepo.Items[recipe.Potion.ID];
		if (!_view.GameState.Outpost.Resources.Has(recipe.HerbResources))
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = LocalizationManager.GetTranslation("GUI_ALCHEMIST_COST_NOT_MET")
			};
		}
		if (recipe.Is3HerbRecipe && !GameController.CraftingController.Is3HerbBrewingEnabled)
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = "Advanced brewing not unlocked"
			};
		}
		if (!_view.GameController.CharacterController.CanHaveItem(character, itemDef))
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = LocalizationManager.GetTranslation("GUI_ITEM_OWNED")
			};
		}
		if (CanBrew(character, recipe))
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = FHUILocFormatting.GetActionIconFormatLiteral(FHInputEventElement.UserInteractionType.LeftClick, "GUI_SELECT_BLUEPRINT")
			};
		}
		if (CanResearch(recipe))
		{
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ALCHEMIST_RESEARCH")
			};
		}
	}

	public IEnumerable<string> GetCraftHints(AlchemistModule.Params @params)
	{
		bool isDistill = @params.Potion != null;
		bool flag = @params.Recipe != null && @params.Recipe.Hidden;
		bool flag2 = @params.Recipe != null && !@params.Recipe.Hidden;
		IEnumerable<ItemDef> ownedItems = _view.GameController.CharacterController.GetOwnedItems(_view.SelectedCharacter);
		bool heroOwnsAnInstance = (flag | flag2) && ownedItems.FirstOrDefault((ItemDef i) => @params.Recipe != null && i.ID == @params.Recipe.ID) != null;
		bool missingCraftResources = @params.Recipe != null && !_view.GameState.Outpost.Resources.Has(@params.Recipe.HerbResources);
		if (isDistill && @params.Potion.CanBeDistilled && @params.SelectedDistillOption == OutpostHerbType.None)
		{
			yield return LocalizationManager.GetTranslation("GUI_ALCHEMIST_DISTILL_PICKHERBHINT");
		}
		if (isDistill && !@params.Potion.CanBeDistilled)
		{
			yield return LocalizationManager.GetTranslation("GUI_ALCHEMIST_POTION_NOTDISTILLABLE");
		}
		if (heroOwnsAnInstance)
		{
			yield return LocalizationManager.GetTranslation("GUI_ITEM_OWNED");
		}
		if (missingCraftResources)
		{
			yield return LocalizationManager.GetTranslation("GUI_ALCHEMIST_COST_NOT_MET");
		}
	}

	public string BrewingCostToLocalizedString(CharacterState character, BrewableRecipeDTO recipe)
	{
		OutpostResources herbResources = recipe.HerbResources;
		return string.Empty + CraftingViewService.LocalizeCost(herbResources, character.Resources + _view.GameState.Outpost.Resources);
	}

	public bool CanBrew(CharacterState state, BrewableRecipe recipe)
	{
		return _view.GameController.CraftingController.CanBrew(state, recipe);
	}

	public bool CanBrew(CharacterState state, OutpostResources resources)
	{
		return _view.GameController.CraftingController.CanBrew(state, resources);
	}

	public bool CanBrew(CharacterState state, BrewableRecipeDTO recipe)
	{
		return CanBrew(state, GetBrewableRecipeFromDTO(recipe));
	}

	public void BrewRecipe(CharacterState state, BrewableRecipeDTO recipe)
	{
		BrewPotion(state, recipe.HerbResources);
	}

	public bool CanResearch(ItemID itemId, OutpostResources HerbResources)
	{
		ItemDef item = DefRepo.Items[itemId];
		return _view.GameController.CraftingController.CanResearch(item, HerbResources);
	}

	public bool CanResearch(BrewableRecipeDTO recipe)
	{
		ItemDef item = DefRepo.Items[recipe.Potion.ID];
		return _view.GameController.CraftingController.CanResearch(item, recipe.HerbResources);
	}

	public bool CanResearch(BrewableRecipe recipe, bool throwException = false)
	{
		return _view.GameController.CraftingController.CanResearch(recipe.Item, recipe.BrewingInfo.HerbResources, throwException);
	}

	public void DiscoverPotion(CharacterState sourceState, CharacterState targetState, OutpostResources herbs, Action onComplete = null, Action onFail = null)
	{
		if (_view.GameController.CraftingController.CanBrew(targetState, herbs))
		{
			FHMultiplayerClient.Promise promise = Choreographer.ExecuteCommand(FHRPC.Campaign.Crafting.DiscoverPotion, sourceState.ID, targetState.ID, herbs, true, false);
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.brewPotionAudioItem);
			promise.OnComplete(onComplete);
			promise.OnFailed(onFail);
		}
	}

	public void BrewPotion(CharacterState state, OutpostResources herbs, Action onComplete = null, Action onFail = null)
	{
		if (_view.GameController.CraftingController.CanBrew(state, herbs))
		{
			FHMultiplayerClient.Promise promise = Choreographer.ExecuteCommand(FHRPC.Campaign.Crafting.Brew, state.ID, herbs, true, false);
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.brewPotionAudioItem);
			promise.OnComplete(onComplete);
			promise.OnFailed(onFail);
		}
	}

	public void DistillHerb(CharacterState state, ItemDef item, OutpostHerbType herb)
	{
		Choreographer.ExecuteCommand(FHRPC.Campaign.Crafting.DistillItem, state.ID, item, herb);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.distillPotionAudioItem);
	}

	public BrewableRecipeDTO GetRecipeFromHerbs(OutpostHerbType[] herbs)
	{
		if (herbs == null)
		{
			return null;
		}
		OutpostResources resources = new OutpostResources(herbs);
		ItemDef brewableItemForResources = _view.GameController.CraftingController.GetBrewableItemForResources(resources);
		if (brewableItemForResources == null)
		{
			return null;
		}
		bool hidden = !_view.GameController.CraftingController.IsPotionDiscovered(brewableItemForResources);
		ItemDef.BrewingInfo brewing = brewableItemForResources.Brewing;
		BrewableRecipe brewableRecipe = new BrewableRecipe
		{
			Item = brewableItemForResources
		};
		if (!brewing.IsAnyRecipe)
		{
			brewableRecipe.BrewingInfo = brewableItemForResources.GetBrewingInfo().SingleOrDefault((ItemDef.BrewingInfo x) => x.HerbResources == resources);
		}
		else
		{
			brewableRecipe.BrewingInfo = new ItemDef.BrewingInfo
			{
				Herb1 = ((herbs.Length != 0) ? herbs[0] : OutpostHerbType.None),
				Herb2 = ((herbs.Length > 1) ? herbs[1] : OutpostHerbType.None),
				Herb3 = ((herbs.Length > 2) ? herbs[2] : OutpostHerbType.None)
			};
		}
		int recipieRemainingItems = _view.GameController.CraftingController.GetRecipieRemainingItems(brewableRecipe);
		return new BrewableRecipeDTO(brewableRecipe, recipieRemainingItems, hidden);
	}
}

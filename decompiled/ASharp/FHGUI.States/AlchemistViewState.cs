using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class AlchemistViewState : FHMapViewState
{
	private readonly UIItemElementInteractionEvents _heroInventoryElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIAlchemistItemElementInteractionEvents _alchemistInventoryElementInteractions = new UIAlchemistItemElementInteractionEvents();

	private readonly AlchemistInventoryModule.FilterParams _alchemistFilterParams = new AlchemistInventoryModule.FilterParams
	{
		ActiveFilters = AlchemistInventoryModule.HerbsFilter.All
	};

	private int m_SelectedRecipeCloneIndex;

	private OutpostHerbType[] m_CurrentBrewHerbOptions = new OutpostHerbType[3];

	private ItemID? m_SelectedPotion;

	private OutpostHerbType? m_SelectedDistillOption;

	private int m_PickingSlotIndex;

	private CategoryFilter _characterActiveFilters = CategoryFilter.All;

	public override StateCategory Category => StateCategory.Shop;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private OutpostInventoryViewModule InventoryViewModule => View.Modules.Characters.FullInventoryView;

	[Preserve]
	private AlchemistModule AlchemistModule => View.Modules.Characters.AlchemistModule;

	[Preserve]
	private AlchemistInventoryModule AlchemistInventoryModule => View.Modules.Characters.AlchemistInventoryModule;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	private ItemViewService ItemService => View.GetService<ItemViewService>();

	private AlchemistViewService AlchemistService => View.GetService<AlchemistViewService>();

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private BuildingViewService BuildingService => View.GetService<BuildingViewService>();

	private FHItemTooltip.ItemTooltipParams OwnedPotionTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = AlchemistService.GetOwnedPotionTooltipHints(item, View.SelectedCharacter).Concat(ItemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, View.SelectedCharacter)).ToArray()
			}
		};
	}

	private FHItemTooltip.ItemTooltipHint[] RecipeTooltipHints(BrewableRecipeDTO item)
	{
		return AlchemistService.GetRecipeDTOTooltipHints(item, View.SelectedCharacter).ToArray();
	}

	private FHItemTooltip.ItemTooltipCost RecipeTooltipCost(BrewableRecipeDTO item)
	{
		return AlchemistService.GetRecipeDTOTooltipCost(item, View.SelectedCharacter);
	}

	public AlchemistViewState()
	{
		_heroInventoryElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			ToggleEquipInventoryItem(el.ItemID);
		};
		_heroInventoryElementInteractions.OnDoubleClick = (UIOutpostItemElement el) =>
		{
			OnPotionSelected(el.ItemID);
		};
		_alchemistInventoryElementInteractions.OnClick = OnInventoryItemSelected;
		_alchemistFilterParams.OnFilterStateUpdated = (AlchemistInventoryModule.HerbsFilter f) =>
		{
			_alchemistFilterParams.ActiveFilters = f;
		};
	}

	public override void OnShow()
	{
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		HeaderService.SetupHeader(HeaderModule);
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, MainViewSubCategories.Alchemy);
		PartyService.SetupPartyColumn(PartyModule);
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		List<ItemDataDTO> ownedPotions = AlchemistService.GetOwnedPotions(View.SelectedCharacter);
		ECharacter character = CrossReference.ClassIdToECharacterModel(View.SelectedCharacter.ClassID.ID);
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = ownedPotions,
			TooltipParams = OwnedPotionTooltipParams,
			ElementInteractionEvents = _heroInventoryElementInteractions,
			OnFilterStateUpdated = OnCharacterFiltersUpdated,
			ActiveFilters = _characterActiveFilters,
			TintColor = UIInfoTools.Instance.GetCharacterColor(character),
			Icon = UIInfoTools.Instance.GetCharacterConfigUI(character.ToString()).iconClassGold,
			DisableFilters = true,
			Resources = View.SelectedCharacter.Resources,
			Gold = View.SelectedCharacter.Gold
		};
		InventoryViewModule.Init(obj);
		AlchemistInventoryContainer.ListParams listParams = new AlchemistInventoryContainer.ListParams
		{
			BuildingLevel = (int)BuildingService.GetBuildingLevelById(new BuildingTypeID(35u)),
			ElementInteractionEvents = _alchemistInventoryElementInteractions,
			TooltipHints = RecipeTooltipHints,
			TooltipCost = RecipeTooltipCost
		};
		List<AlchemistInventoryContainer.SlotData> researchData = AlchemistService.GetResearchData();
		AlchemistInventoryModule.Init(listParams, researchData, AlchemistService.GetAlchemistInventory(View.SelectedCharacter), _alchemistFilterParams);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		EvaluateState();
		Background.SetEnabled(isEnabled: true, UIInfoTools.Instance.GetOutpostPanelIllustration("FH_UI_View_Alchemist_Background"));
	}

	private void OnCharacterFiltersUpdated(CategoryFilter filter)
	{
		_characterActiveFilters = filter;
	}

	public override void OnHide()
	{
		ResetPersistentStateData();
		CharacterDetailsScreenModule.ClearAll();
		AlchemistInventoryModule.ClearSelection();
	}

	private void EvaluateState()
	{
		AlchemistInventoryModule.ClearSelection();
		InventoryViewModule.ClearSelection();
		if (m_SelectedPotion.HasValue && View.GameController.CharacterController.GetOwnedItems(View.SelectedCharacter).FirstOrDefault((ItemDef i) => i.ID == m_SelectedPotion.Value) == null)
		{
			m_SelectedPotion = null;
			m_SelectedDistillOption = null;
		}
		AlchemistModule.Params obj = new AlchemistModule.Params
		{
			AvailableResources = View.GameState.Outpost.Resources,
			BasicBrewEnabled = AlchemistService.IsBrewingEnabled,
			AdvancedBrewEnabled = AlchemistService.IsAdvancedBrewingEnabled,
			InputOptions = m_CurrentBrewHerbOptions,
			OnHerbPicker = OnHerbPicker,
			OnBrewHerbPicked = OnBrewHerbSelected,
			CraftHint = AlchemistService.GetCraftHints,
			PickingIndex = m_PickingSlotIndex
		};
		if (m_SelectedPotion.HasValue)
		{
			IEnumerable<OutpostHerbType> distillableHerbOptions = AlchemistService.GetDistillableHerbOptions(m_SelectedPotion.Value);
			obj.Potion = ItemService.ItemDTOFromID(m_SelectedPotion.Value);
			obj.OutputOptions = distillableHerbOptions;
			obj.SelectedDistillOption = m_SelectedDistillOption.GetValueOrDefault();
			obj.OnDistillOptionSelected = OnDistillOptionSelected;
			obj.OnCraftCallback = OnDistillRequest;
			obj.IsValidCraft = AlchemistService.IsDistillingEnabled && obj.SelectedDistillOption != OutpostHerbType.None;
			AlchemistInventoryModule.ClearSelection();
			InventoryViewModule.SelectItem(m_SelectedPotion.Value);
		}
		BrewableRecipeDTO recipeFromHerbs = AlchemistService.GetRecipeFromHerbs(m_CurrentBrewHerbOptions);
		if (recipeFromHerbs != null)
		{
			obj.Recipe = recipeFromHerbs;
			obj.InputOptions = m_CurrentBrewHerbOptions;
			obj.OnCraftCallback = OnCraftRequest;
			obj.IsValidCraft = (recipeFromHerbs.Hidden ? AlchemistService.CanResearch(AlchemistService.GetBrewableRecipeFromDTO(recipeFromHerbs)) : AlchemistService.CanBrew(View.SelectedCharacter, recipeFromHerbs));
			AlchemistInventoryModule.TrySelect(recipeFromHerbs.Potion.ID, m_SelectedRecipeCloneIndex);
		}
		AlchemistModule.Init(obj);
	}

	private void ToggleEquipInventoryItem(ItemID itemID)
	{
		if (View.SelectedCharacter.EquipedItemIDs.Contains(itemID))
		{
			ItemService.UnequipItem(itemID, View.SelectedCharacter);
		}
		else
		{
			ItemService.EquipItem(itemID, View.SelectedCharacter);
		}
	}

	private void OnPotionSelected(ItemID potionId, OutpostHerbType selectedOption = OutpostHerbType.None)
	{
		if (AlchemistService.IsDistillingEnabled)
		{
			ResetPersistentStateData();
			m_SelectedPotion = potionId;
			m_SelectedDistillOption = selectedOption;
			EvaluateState();
		}
	}

	private void OnInventoryItemSelected(UIAlchemistInventoryElement.Params data)
	{
		ResetPersistentStateData();
		m_SelectedRecipeCloneIndex = data.CloneIndex;
		m_CurrentBrewHerbOptions[0] = data.RecipeDto.Herb1;
		m_CurrentBrewHerbOptions[1] = data.RecipeDto.Herb2;
		m_CurrentBrewHerbOptions[2] = data.RecipeDto.Herb3;
		EvaluateState();
	}

	private void OnCraftRequest(AlchemistModule.Params p)
	{
		if (p.Recipe.Hidden)
		{
			OutpostResources sourceHerbs = (p.IsPoison ? new OutpostResources(m_CurrentBrewHerbOptions) : p.Recipe.HerbResources);
			ResetPersistentStateData();
			View.PushState(new PotionDiscoveryDistributionViewState(p.Recipe.Potion.ID, sourceHerbs, View.SelectedCharacter.ID));
			View.PushState(new PotionDiscoveryViewState(p.Recipe.Potion, sourceHerbs));
		}
		else
		{
			AlchemistService.BrewRecipe(View.SelectedCharacter, p.Recipe);
			ResetPersistentStateData();
			m_CurrentBrewHerbOptions[0] = p.Recipe.Herb1;
			m_CurrentBrewHerbOptions[1] = p.Recipe.Herb2;
			m_CurrentBrewHerbOptions[2] = p.Recipe.Herb3;
			View.RefreshView();
		}
	}

	private void OnDistillOptionSelected(ItemID potionId, OutpostHerbType selectee)
	{
		OnPotionSelected(potionId, selectee);
	}

	private void OnBrewHerbSelected(int index, OutpostHerbType herb)
	{
		m_CurrentBrewHerbOptions[index] = herb;
		m_PickingSlotIndex = 0;
		EvaluateState();
	}

	private void OnHerbPicker(int slotIndex)
	{
		m_PickingSlotIndex = slotIndex;
		EvaluateState();
	}

	private void OnDistillRequest(AlchemistModule.Params p)
	{
		AlchemistService.DistillHerb(View.SelectedCharacter, p.Potion, p.SelectedDistillOption);
		ResetPersistentStateData();
	}

	private void ResetPersistentStateData()
	{
		m_SelectedPotion = null;
		m_SelectedDistillOption = null;
		m_PickingSlotIndex = 0;
		m_SelectedRecipeCloneIndex = 0;
		for (int i = 0; i < m_CurrentBrewHerbOptions.Length; i++)
		{
			m_CurrentBrewHerbOptions[i] = OutpostHerbType.None;
		}
	}
}

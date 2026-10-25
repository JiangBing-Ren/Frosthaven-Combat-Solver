using System;
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

public class CraftingViewState : FHMapViewState
{
	private readonly UIItemElementInteractionEvents _shopElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemElementInteractionEvents _heroInventoryElementInteractions = new UIItemElementInteractionEvents();

	private ItemID? _lastSelected;

	private CategoryFilter _shopActiveFilters = CategoryFilter.All;

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
	private CraftingViewModule CraftingViewModule => View.Modules.Characters.CraftingViewModule;

	[Preserve]
	private ShopViewModule ShopViewModule => View.Modules.Characters.ShopView;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	private ItemViewService ItemService => View.GetService<ItemViewService>();

	private CraftingViewService CraftingService => View.GetService<CraftingViewService>();

	private InputActionHintsService InputActionHintsService => View.GetService<InputActionHintsService>();

	private FHItemTooltip.ItemTooltipParams ShopTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = CraftingService.GetTooltipHints(item, View.SelectedCharacter).ToArray(),
				Cost = CraftingService.GetTooltipCosts(item, View.SelectedCharacter)
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams InventoryTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = ItemService.GetTooltipHints((UIItemScreenFunctionality)((item.IsEquipped ? 8 : 4) | 2), item, View.SelectedCharacter).ToArray(),
				Cost = ItemService.GetTooltipCost(UIItemScreenFunctionality.Sell, item, View.SelectedCharacter)
			}
		};
	}

	public CraftingViewState()
	{
		_shopElementInteractions.OnClick = (UIOutpostItemElement id) =>
		{
			HandleOnItemsSelected(id.ItemID, id.ItemData.MaxQuantity);
		};
		_heroInventoryElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			ClickItemInInventory(el.ItemID);
		};
		_heroInventoryElementInteractions.OnDoubleClick = (UIOutpostItemElement el) =>
		{
			DoubleClickItemInInventory(el.ItemID);
		};
	}

	public override void OnShow()
	{
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		CharacterState selectedCharacter = View.SelectedCharacter;
		HeaderService.SetupHeader(HeaderModule);
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, MainViewSubCategories.Craftsman);
		PartyService.SetupPartyColumn(PartyModule);
		InputActionHintsService.Setup(InputActionHints, this);
		IOrderedEnumerable<ItemDataDTO> items = from dto in ItemService.GetOwnedItems(View.SelectedCharacter, includeSellCost: true)
			orderby dto.ID
			select dto;
		ECharacter character = CrossReference.ClassIdToECharacterModel(selectedCharacter.ClassID.ID);
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = items,
			TooltipParams = InventoryTooltipParams,
			ElementInteractionEvents = _heroInventoryElementInteractions,
			OnFilterStateUpdated = OnCharacterFiltersUpdated,
			ActiveFilters = _characterActiveFilters,
			TintColor = UIInfoTools.Instance.GetCharacterColor(character),
			Icon = UIInfoTools.Instance.GetCharacterConfigUI(character.ToString()).iconClassGold,
			ShowResourceBar = false,
			Resources = View.SelectedCharacter.Resources,
			Gold = View.SelectedCharacter.Gold
		};
		InventoryViewModule.Init(obj);
		List<ItemDataDTO> craftableItems = CraftingService.GetCraftableItems(selectedCharacter);
		ShopViewModule.Params obj2 = new ShopViewModule.Params
		{
			Items = craftableItems,
			TooltipData = ShopTooltipParams,
			ElementInteractionEvents = _shopElementInteractions,
			OnFilterStateUpdated = OnShopFiltersUpdated,
			ActiveFilters = _shopActiveFilters,
			PreserveDefaultSelectionBehavior = true,
			CaptionKey = "GUI_CRAFTSMAN_NAME",
			HeaderColor = FHUIColors.CRAFTER_HEADER,
			CategorySprite = UIInfoTools.Instance.GetBuildingConfig(BuildingId.Workshop)?.StylizedIcon,
			Illustration = UIInfoTools.Instance.GetOutpostPanelIllustration("Craftsman")
		};
		ShopViewModule.Init(obj2);
		TrySelectPreviousOrDefault(craftableItems);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true, UIInfoTools.Instance.GetOutpostPanelIllustration("FH_UI_View_Craftsman_Background"));
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
		_lastSelected = null;
	}

	private void TrySelectPreviousOrDefault(List<ItemDataDTO> craftableItems)
	{
		if (craftableItems == null || craftableItems.Count == 0)
		{
			SetItem(null);
			return;
		}
		List<ItemDataDTO> list = UIFilteredItemContainerElement.FilterItems(_shopActiveFilters, craftableItems);
		int num = list.FindIndex((ItemDataDTO dto) =>
		{
			ItemID iD = dto.ID;
			ItemID? lastSelected = _lastSelected;
			return iD == lastSelected;
		});
		SetItem((num != -1) ? list[num] : null, highlightItem: true);
	}

	private void SetItem(ItemDataDTO itemDto, bool highlightItem = false)
	{
		CraftingViewModule craftingViewModule = CraftingViewModule;
		Action<ItemDataDTO> onCraftCallback = (ItemDataDTO result) =>
		{
			if (result != null)
			{
				CraftingService.CraftItem(View.SelectedCharacter, result);
				_lastSelected = null;
			}
		};
		IEnumerable<string> hints;
		if (itemDto != null)
		{
			hints = CraftingService.GetCraftHintsLocalized(View.SelectedCharacter, itemDto);
		}
		else
		{
			IEnumerable<string> enumerable = new List<string>();
			hints = enumerable;
		}
		craftingViewModule.Init(itemDto, onCraftCallback, hints, itemDto != null && CraftingService.CanCraftItem(View.SelectedCharacter, itemDto.ID));
		if (highlightItem && itemDto != null)
		{
			ShopViewModule.SelectItem(itemDto.ID);
		}
		_lastSelected = itemDto?.ID;
	}

	private void HandleOnItemsSelected(ItemID itemID, int index)
	{
		ItemDataDTO itemDto = CraftingService.FillItemCraftCost(itemID, View.SelectedCharacter);
		SetItem(itemDto);
	}

	private void ClickItemInInventory(ItemID itemID)
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

	private void DoubleClickItemInInventory(ItemID itemID)
	{
		ItemService.ShowSellItemDialog(itemID, View.SelectedCharacter);
	}

	private void OnShopFiltersUpdated(CategoryFilter filter)
	{
		_shopActiveFilters = filter;
		List<ItemDataDTO> craftableItems = CraftingService.GetCraftableItems(View.SelectedCharacter);
		TrySelectPreviousOrDefault(craftableItems);
	}

	private void OnCharacterFiltersUpdated(CategoryFilter filter)
	{
		_characterActiveFilters = filter;
	}
}

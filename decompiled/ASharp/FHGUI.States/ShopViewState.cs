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

public class ShopViewState : FHMapViewState
{
	private UIItemScreenFunctionality _shopFunctionality = UIItemScreenFunctionality.Shop;

	private UIItemScreenFunctionality _inventoryFunctionality = UIItemScreenFunctionality.Inventory | UIItemScreenFunctionality.Sell;

	private UIItemScreenFunctionality _equipmentFunctionality = UIItemScreenFunctionality.Sell | UIItemScreenFunctionality.Unequip;

	private readonly UIItemElementInteractionEvents _shopElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemElementInteractionEvents _heroInventoryElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemInteractionEvents _equipmentElementInteractions = new UIItemInteractionEvents();

	private CategoryFilter _shopActiveFilters = CategoryFilter.All;

	private CategoryFilter _characterActiveFilters = CategoryFilter.All;

	public override StateCategory Category => StateCategory.Shop;

	protected virtual MainViewSubCategories SubCategory => MainViewSubCategories.Merchant;

	[Preserve]
	protected MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	protected OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	protected PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	protected CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	protected OutpostInventoryViewModule InventoryViewModule => View.Modules.Characters.FullInventoryView;

	[Preserve]
	protected ShopViewModule ShopViewModule => View.Modules.Characters.ShopView;

	[Preserve]
	protected CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	[Preserve]
	protected EquipmentViewModule EquipmentViewModule => View.Modules.Characters.EquipmentView;

	[Preserve]
	protected InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	private ItemViewService ItemService => View.GetService<ItemViewService>();

	private BuildingViewService BuildingService => View.GetService<BuildingViewService>();

	private CharacterDetailsViewService CharacterDetailsViewService => View.GetService<CharacterDetailsViewService>();

	private FHItemTooltip.ItemTooltipParams ShopTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = ItemService.GetTooltipHints(UIItemScreenFunctionality.Shop, item, View.SelectedCharacter).ToArray(),
				Cost = ItemService.GetTooltipCost(UIItemScreenFunctionality.Shop, item, View.SelectedCharacter)
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams EquipmentTooltipData(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = ItemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, View.SelectedCharacter).ToArray(),
				Cost = ItemService.GetTooltipCost(UIItemScreenFunctionality.Sell, item, View.SelectedCharacter)
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams GetInventoryTooltipParams(ItemDataDTO item)
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

	public ShopViewState()
	{
		_shopElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			ClickItemInShop(el.ItemID);
		};
		_heroInventoryElementInteractions.OnDoubleClick = (UIOutpostItemElement el) =>
		{
			HandleSellItem(el.ItemID);
		};
		_heroInventoryElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			HandleToggleEquipItem(el.ItemID);
		};
		_equipmentElementInteractions.OnClick = (ItemDataDTO el) =>
		{
			HandleToggleEquipItem(el.ID);
		};
	}

	protected virtual ShopViewModule.Params GetParams()
	{
		ItemViewService itemService = ItemService;
		return new ShopViewModule.Params
		{
			Items = itemService.GetPurchasableItems(View.SelectedCharacter),
			ElementInteractionEvents = _shopElementInteractions,
			TooltipData = ShopTooltipParams,
			OnFilterStateUpdated = OnShopFiltersUpdated,
			ActiveFilters = _shopActiveFilters,
			CaptionKey = "GUI_MERCHANT",
			HeaderColor = FHUIColors.MERCHANT_HEADER,
			ShowLimitReached = ItemService.IsAtPurchaseLimit(),
			Charges = ItemService.GetPurchaseLimit(),
			CategorySprite = UIInfoTools.Instance.GetBuildingConfig(BuildingId.TradingPost)?.StylizedIcon,
			DontShowCharges = false,
			LimitDescriptionKey = "GUI_weekly_purchases_remaining"
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
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, SubCategory);
		PartyService.SetupPartyColumn(PartyModule);
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		ItemViewService itemService = ItemService;
		ECharacter character = CrossReference.ClassIdToECharacterModel(View.SelectedCharacter.ClassID.ID);
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = from dto in itemService.GetOwnedItems(View.SelectedCharacter, includeSellCost: true)
				orderby dto.ID
				select dto,
			TooltipParams = GetInventoryTooltipParams,
			OnFilterStateUpdated = OnCharacterFiltersUpdated,
			ActiveFilters = _characterActiveFilters,
			ElementInteractionEvents = _heroInventoryElementInteractions,
			EnsureMinimumVisibleSlots = 0,
			FillFreeSpace = false,
			TintColor = UIInfoTools.Instance.GetCharacterColor(character),
			Icon = UIInfoTools.Instance.GetCharacterConfigUI(character.ToString()).iconClassGold,
			ShowResourceBar = false,
			Gold = View.SelectedCharacter.Gold,
			Resources = View.SelectedCharacter.Resources
		};
		InventoryViewModule.Init(obj);
		EquipmentViewModule.Init(itemService.GetEquippedItems(View.SelectedCharacter, includeSellCost: true), View.SelectedCharacter.Level, View.GameController.CharacterController.GetMaxUnlockedSmallItems(View.SelectedCharacter), View.GameController.CharacterController.GetMaxSmallItems(View.SelectedCharacter), View.GameController.CharacterController.GetMaxOneHandSlots(View.SelectedCharacter), EquipmentTooltipData, _equipmentElementInteractions);
		ShopViewModule.Params obj2 = GetParams();
		ShopViewModule.Init(obj2);
		CharacterPreviewModule.Init(CharacterDetailsViewService.PopulateCharacterDTO(View.SelectedCharacter), null, showValues: false);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(PopThisState));
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.Equipment);
		View.OutpostSceneController.OpenCharacterScreen(View.SelectedCharacter);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void ClickItemInShop(ItemID itemID)
	{
		ItemService.ShowBuyItemDialog(itemID, View.SelectedCharacter);
	}

	private void HandleToggleEquipItem(ItemID itemID)
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

	private void HandleSellItem(ItemID itemID)
	{
		ItemService.ShowSellItemDialog(itemID, View.SelectedCharacter);
	}

	private void OnShopFiltersUpdated(CategoryFilter filter)
	{
		_shopActiveFilters = filter;
	}

	private void OnCharacterFiltersUpdated(CategoryFilter filter)
	{
		_characterActiveFilters = filter;
	}
}

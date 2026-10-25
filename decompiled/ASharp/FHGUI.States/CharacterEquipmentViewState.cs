using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class CharacterEquipmentViewState : FHMapViewState
{
	private readonly UIItemElementInteractionEvents _inventoryElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemInteractionEvents _equipmentElementInteractions = new UIItemInteractionEvents();

	private CategoryFilter _characterActiveFilters = CategoryFilter.All;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private MainHeaderModule MainHeader => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private SimpleHeaderModule SimpleHeader => View.Modules.Characters.Header;

	[Preserve]
	private CharactersSelectorModule PartyColumn => View.Modules.Characters.PartyColumn;

	[Preserve]
	private OutpostInventoryViewModule EquipmentInventoryView => View.Modules.Characters.EquipmentInventoryView;

	[Preserve]
	private CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	[Preserve]
	private EquipmentViewModule EquipmentViewModule => View.Modules.Characters.EquipmentView;

	private ItemViewService ItemService => View.GetService<ItemViewService>();

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private PartyViewService _partyService => View.GetService<PartyViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private CharacterDetailsViewService CharacterDetailsService => View.GetService<CharacterDetailsViewService>();

	private FHItemTooltip.ItemTooltipParams InventoryTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = ItemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, View.SelectedCharacter).ToArray()
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams EquipmentTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = ItemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, View.SelectedCharacter).ToArray()
			}
		};
	}

	public CharacterEquipmentViewState()
	{
		_inventoryElementInteractions.OnDoubleClick = (UIOutpostItemElement el) =>
		{
			DoubleClickItemInInventory(el.ItemID);
		};
		_equipmentElementInteractions.OnDoubleClick = (ItemDataDTO el) =>
		{
			DoubleClickItemInInventory(el.ID);
		};
	}

	public override void OnShow()
	{
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		bool flag = HeaderService.SetupCharacterDetailsHeader(MainHeader, SimpleHeader, "Equipment");
		ItemViewService itemService = ItemService;
		IOrderedEnumerable<ItemDataDTO> items = from dto in itemService.GetOwnedItems(View.SelectedCharacter)
			orderby dto.ID
			select dto;
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = items,
			TooltipParams = InventoryTooltipParams,
			ElementInteractionEvents = _inventoryElementInteractions,
			OnFilterStateUpdated = OnCharacterFiltersUpdated,
			ActiveFilters = _characterActiveFilters
		};
		EquipmentInventoryView.Init(obj);
		CharacterPreviewModule.Init(CharacterDetailsService.PopulateCharacterDTO(View.SelectedCharacter), null, showValues: false);
		EquipmentViewModule.Init(itemService.GetEquippedItems(View.SelectedCharacter), View.SelectedCharacter.Level, View.GameController.CharacterController.GetMaxUnlockedSmallItems(View.SelectedCharacter), View.GameController.CharacterController.GetMaxSmallItems(View.SelectedCharacter), View.GameController.CharacterController.GetMaxOneHandSlots(View.SelectedCharacter), EquipmentTooltipParams, _equipmentElementInteractions);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(flag ? CharacterDetailsScreenModule.Functionality.Equipment : CharacterDetailsScreenModule.Functionality.QuestPreparation);
		View.OutpostSceneController.OpenCharacterScreen(View.SelectedCharacter);
		if (flag)
		{
			CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(PopThisState));
			QuickAccessBar.gameObject.SetActive(value: false);
			PartyModule.gameObject.SetActive(value: false);
			List<PartySlotDTO> partySlotsData = View.GetService<PartyViewService>().GetPartySlotsData(View.SelectedCharacter);
			PartyColumn.Init(partySlotsData, View.GetService<PartyViewService>().SwitchToPartySlot);
		}
		else
		{
			QuickAccessBarService.Setup(MainViewMainCategories.Characters);
			_partyService.SetupPartyColumn(PartyModule);
			PartyColumn.gameObject.SetActive(value: false);
		}
	}

	public override void OnHide()
	{
		PartyColumn.ClearEvents();
		CharacterDetailsScreenModule.ClearAll();
	}

	private void DoubleClickItemInInventory(ItemID itemID)
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

	private void OnCharacterFiltersUpdated(CategoryFilter filter)
	{
		_characterActiveFilters = filter;
	}
}

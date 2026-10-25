using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class SpendStartingGoldViewState : FHMapViewState
{
	private readonly CharacterID _characterID;

	private readonly bool _isIntroCharacter;

	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly ItemViewService _itemService;

	private readonly PartyViewService _partyService;

	private readonly UIItemElementInteractionEvents _heroInventoryElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemElementInteractionEvents _shopElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemInteractionEvents _equipmentElementInteractions = new UIItemInteractionEvents();

	private readonly bool _creationProcess;

	private readonly bool _unassignedCharacter;

	private CategoryFilter _shopActiveFilters = CategoryFilter.All;

	private CategoryFilter _characterActiveFilters = CategoryFilter.All;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private EquipmentViewModule EquipmentViewModule => View.Modules.Characters.EquipmentView;

	[Preserve]
	private OutpostInventoryViewModule InventoryViewModule => View.Modules.Characters.FullInventoryView;

	[Preserve]
	private ShopViewModule ShopViewModule => View.Modules.Characters.ShopView;

	[Preserve]
	private CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	private FHItemTooltip.ItemTooltipParams ShopTooltipParams(ItemDataDTO item)
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return null;
		}
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = _itemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = _itemService.GetTooltipHints(UIItemScreenFunctionality.Shop, item, character).ToArray(),
				Cost = _itemService.GetTooltipCost(UIItemScreenFunctionality.Shop, item, character)
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams InventoryTooltipParams(ItemDataDTO item)
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return null;
		}
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = _itemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = _itemService.GetTooltipHints(item.IsEquipped ? (UIItemScreenFunctionality.Sell | UIItemScreenFunctionality.Unequip) : (UIItemScreenFunctionality.Sell | UIItemScreenFunctionality.Equip), item, character).ToArray(),
				Cost = _itemService.GetTooltipCost(UIItemScreenFunctionality.Refund, item, character)
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams EquipmentTooltipParams(ItemDataDTO item)
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return null;
		}
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = _itemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = _itemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, character).ToArray()
			}
		};
	}

	public SpendStartingGoldViewState(CharacterState character, bool creationProcess, bool unassignedCharacter)
	{
		_characterID = character.ID;
		_creationProcess = creationProcess;
		_unassignedCharacter = unassignedCharacter;
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_itemService = View.GetService<ItemViewService>();
		_partyService = View.GetService<PartyViewService>();
		_heroInventoryElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			ClickItemInInventory(el.ItemID);
		};
		_heroInventoryElementInteractions.OnDoubleClick = (UIOutpostItemElement el) =>
		{
			OnDoubleClickItemInInInventory(el.ItemID);
		};
		_shopElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			_itemService.Action_BuyAndEquipItem(el.ItemID, _characterID, unequipCurrent: false);
		};
		_equipmentElementInteractions.OnClick = (ItemDataDTO el) =>
		{
			ClickItemInInventory(el.ID);
		};
	}

	public override void OnShow()
	{
		if (!_unassignedCharacter && !_partyService.LocalPlayerOwnsCharacter(_characterID))
		{
			View.ResetView();
			return;
		}
		if ((_unassignedCharacter && !_partyService.HasCharacterInRoster(_characterID)) || (!_unassignedCharacter && !_partyService.LocalPlayerOwnsCharacter(_characterID)))
		{
			PopThisState();
			return;
		}
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character) || !View.GameController.CharacterController.IsCharacterNameValid(character))
		{
			PopThisState();
			return;
		}
		ItemViewService itemService = _itemService;
		IOrderedEnumerable<ItemDataDTO> items = from dto in itemService.GetOwnedItems(character)
			orderby dto.ID
			select dto;
		ECharacter character2 = CrossReference.ClassIdToECharacterModel(character.ClassID.ID);
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = items,
			TooltipParams = InventoryTooltipParams,
			ElementInteractionEvents = _heroInventoryElementInteractions,
			OnFilterStateUpdated = OnCharacterFiltersUpdated,
			ActiveFilters = _characterActiveFilters,
			TintColor = UIInfoTools.Instance.GetCharacterColor(character2),
			ShowResourceBar = true,
			HideZeroResources = true,
			Icon = UIInfoTools.Instance.GetCharacterConfigUI(character2.ToString()).iconClassGold,
			Gold = character.Gold,
			ShowBonusStartingGoldHint = View.GetService<PartyViewService>().HasBonusStartingGold()
		};
		InventoryViewModule.Init(obj);
		InventoryViewModule.SetOffset(new Vector2(-112f, 0f));
		ShopViewModule.Params obj2 = new ShopViewModule.Params
		{
			Items = itemService.GetPurchasableItems(character),
			TooltipData = ShopTooltipParams,
			ElementInteractionEvents = _shopElementInteractions,
			OnFilterStateUpdated = OnShopFiltersUpdated,
			ActiveFilters = _shopActiveFilters,
			CaptionKey = "GUI_MERCHANT",
			HeaderColor = FHUIColors.MERCHANT_HEADER,
			CategorySprite = UIInfoTools.Instance.GetBuildingConfig(BuildingId.TradingPost)?.StylizedIcon
		};
		ShopViewModule.Init(obj2);
		CharacterPreviewModule.Init(_characterDetailsService.PopulateCharacterDTO(character), null, showValues: false);
		EquipmentViewModule.Init(itemService.GetEquippedItems(character), character.Level, View.GameController.CharacterController.GetMaxUnlockedSmallItems(character), View.GameController.CharacterController.GetMaxSmallItems(character), View.GameController.CharacterController.GetMaxOneHandSlots(character), EquipmentTooltipParams, _equipmentElementInteractions);
		ButtonsBarParam[] buttonsBar = ((!_creationProcess) ? new ButtonsBarParam[2]
		{
			CharacterDetailsScreenModule.Button_Back(OnBack),
			ButtonsBarElement.Common_Reset(OnReset, CharacterHasItems())
		} : new ButtonsBarParam[2]
		{
			CharacterDetailsScreenModule.Button_Back(OnBack),
			CharacterDetailsScreenModule.Button_Create(OnContinue)
		});
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(buttonsBar);
		if (_creationProcess)
		{
			bool excludePersonalQuest = !_characterDetailsService.HasAvailablePersonalQuest(View.PlayerID);
			CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(excludePersonalQuest), 3);
		}
		CharacterDetailsScreenModule.SetCharacterClass(character.ClassDef);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.Equipment);
		View.OutpostSceneController.OpenCharacterScreen(character);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnBack()
	{
		PopThisState();
	}

	private void OnContinue()
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return;
		}
		if (character.Gold != 0)
		{
			View.PushState(new GenericModalViewState(LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_CONFIRMATION_DESCRIPTION"), LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_CONFIRMATION_CAPTION"), LocalizationManager.GetTranslation("GUI_CREATE"), (bool confirmed) =>
			{
				if (confirmed)
				{
					ShowBonusStartingLevelChoice();
				}
			}));
		}
		else
		{
			ShowBonusStartingLevelChoice();
		}
	}

	private void ShowBonusStartingLevelChoice()
	{
		uint num = View.GetService<PartyViewService>().NewCharacterBonusLevelFromProsperity();
		if (num > 1)
		{
			string text = FHUILocFormatting.PopulateParams("GUI_HERO_LEVEL_INFO", num) + "\n\n";
			text += LocalizationManager.GetTranslation("GUI_HERO_LEVEL_INFO_QUESTION");
			FHUILocFormatting.PopulateParams("GUI_HERO_LEVEL_INFO_CHOICE_LABEL", num);
			View.PushState(new GenericListModalViewState(LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_CONFIRMATION_CAPTION"), text, new List<GenericListModalViewState.ListItem>
			{
				new GenericListModalViewState.ListItem
				{
					Item = 1u,
					Text = FHUILocFormatting.PopulateParams("GUI_HERO_LEVEL_INFO_CHOICE_LABEL", 1)
				},
				new GenericListModalViewState.ListItem
				{
					Item = num,
					Text = FHUILocFormatting.PopulateParams("GUI_HERO_LEVEL_INFO_CHOICE_LABEL", num)
				}
			}, 1, LocalizationManager.GetTranslation("GUI_CREATE"), LocalizationManager.GetTranslation("GUI_CANCEL"), (GenericListModalViewState.ListItem result) =>
			{
				if (result != null)
				{
					FinalizeHeroCreation((uint)result.Item > 1);
				}
			}, backgroundEnabled: false, 2));
		}
		else
		{
			FinalizeHeroCreation(levelUpToProsperity: false);
		}
	}

	private void FinalizeHeroCreation(bool levelUpToProsperity)
	{
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			if (MultiplayerGameController.GetPersonalQuests(View.PlayerID).Any() && character.PersonalQuest != null)
			{
				Choreographer.ExecuteCommand("ClearPersonalQuests", View.PlayerID);
				Choreographer.ExecuteCommand(FHRPC.Campaign.Character.FinishSelectPersonalQuest, character.ID, false);
			}
			_partyService.SetInitialCreationFlowFinished(character);
			if (View.GameState.IsInPrologue)
			{
				View.ResetView();
				View.SetState(new ProloguePartyCreationViewState());
			}
			else
			{
				View.GetService<PartyViewService>().ConfirmCharacterCreation(_characterID, !_unassignedCharacter, levelUpToProsperity);
				View.ResetView();
				View.ReplaceState(new ManageRosterViewState());
			}
		}
	}

	private bool CharacterHasItems()
	{
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			return false;
		}
		if (!View.GameController.CharacterController.GetEquippedItems(character).Any())
		{
			return View.GameController.CharacterController.GetTuckedItems(character).Any();
		}
		return true;
	}

	private void OnReset()
	{
		View.Choreographer.ExecuteCommand(FHRPC.Campaign.Character.ResetNewCharacterItems, _characterID);
		OnShow();
	}

	private void ClickItemInInventory(ItemID itemID)
	{
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			if (character.EquipedItemIDs.Contains(itemID))
			{
				_itemService.UnequipItem(itemID, character);
			}
			else
			{
				_itemService.EquipItem(itemID, character);
			}
		}
	}

	private void OnDoubleClickItemInInInventory(ItemID itemID)
	{
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			_itemService.SellItem(itemID, character);
		}
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

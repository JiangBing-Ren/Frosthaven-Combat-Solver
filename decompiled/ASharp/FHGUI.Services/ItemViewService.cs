using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.States;
using FHRL;
using GLOOM;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI.Services;

public class ItemViewService : FHMapViewService
{
	private TradingController _tradingController => GameController.TradingController;

	private CharacterController _characterController => GameController.CharacterController;

	public bool MerchantEnabled => !GameController.State.GameMode.NoMerchant;

	public bool IsTravellingMerchantEnabled => _tradingController.IsBuyingItemsFromTravellingMerchantEnabled;

	public bool IsTravellingMerchant => _view.CurrentState is TravellingMerchantViewState;

	public ItemViewService(FHMapView view)
		: base(view)
	{
	}

	public IEnumerable<ItemDef> GetPurchasableItemDefs(bool isInCharacterCreation)
	{
		if (IsTravellingMerchant)
		{
			return _tradingController.ValidBuyableItems();
		}
		if (isInCharacterCreation)
		{
			return _tradingController.BuyableItemsForNewCharacters.Where((ItemDef item) => item.CanBePurchased && item.ID.ID < 10000);
		}
		return _tradingController.BuyableItemsFromMerchant.Where((ItemDef item) => item.CanBePurchased);
	}

	public IEnumerable<ItemDataDTO> GetPurchasableItems(CharacterState character, bool stacked = true)
	{
		IEnumerable<ItemDef> purchasableItemDefs = GetPurchasableItemDefs(character.PartyState == CharacterPartyStateType.CreationProcess);
		if (stacked)
		{
			return ItemDataDTO.FromItemDefStacked(purchasableItemDefs, character, showCost: true, showEquipped: false, (ItemDef def) => OverrideGoldCost(def, character));
		}
		return ItemDataDTO.FromItemDef(purchasableItemDefs, character, showCost: true, showEquipped: false, (ItemDef def) => OverrideGoldCost(def, character));
	}

	private uint OverrideGoldCost(ItemDef item, CharacterState character)
	{
		bool flag = character.PartyState != CharacterPartyStateType.CreationProcess && !IsTravellingMerchant;
		int buyItemGoldDiscount = GameController.State.BuyItemGoldDiscount;
		return (uint)_tradingController.GetDiscountedCost(item, flag ? buyItemGoldDiscount : 0, flag);
	}

	public IEnumerable<ItemDataDTO> GetOwnedItems(CharacterState character, bool includeSellCost = false)
	{
		return from item in _view.GameController.CharacterController.GetOwnedItems(character)
			select new ItemDataDTO(item, -1, showCost: false, isNew: false, canBuy: false, IsItemEquipped(item.ID, character), includeSellCost ? GetItemSellValue(item, inCreation: false) : 0, character.Gold >= item.GoldCost, character.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == item.ID));
	}

	public IEnumerable<ItemDataDTO> GetEquippedItems(CharacterState character, bool includeSellCost = false)
	{
		return from item in _view.GameController.CharacterController.GetEquippedItems(character)
			select new ItemDataDTO(item, -1, showCost: false, isNew: false, canBuy: false, isEquipped: true, includeSellCost ? GetItemSellValue(item, inCreation: false) : 0, character.Gold >= item.GoldCost, character.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == item.ID));
	}

	public bool IsItemEquipped(ItemID itemID, CharacterState character)
	{
		return character.EquipedItemIDs.Contains(itemID);
	}

	public ItemDataDTO ItemDTOFromID(ItemID id)
	{
		return new ItemDataDTO(DefRepo.Items[id]);
	}

	[CanBeNull]
	public ItemDataDTO GetFlipItem(ItemID id)
	{
		ItemDef itemDef = DefRepo.Items[id];
		if (!itemDef.FlipScenarioItemID.HasValue)
		{
			return null;
		}
		int scenarioID = itemDef.FlipScenarioItemID.Value;
		ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData i) => i.ID == scenarioID);
		if (itemCardYMLData == null)
		{
			return null;
		}
		return new ItemDataDTO(itemCardYMLData);
	}

	public ItemDataDTO ItemDTOFromScenarioID(int scenarioItemID)
	{
		ItemDef itemDef = DefRepo.Items.Values.FirstOrDefault((ItemDef i) => i.ScenarioItemID == scenarioItemID);
		if (itemDef != null)
		{
			return new ItemDataDTO(itemDef);
		}
		return null;
	}

	public int GetItemSellValue(ItemDef itemDef, bool inCreation)
	{
		return _tradingController.GetItemSellValue(itemDef, inCreation);
	}

	public bool IsAtPurchaseLimit()
	{
		return RemainingItemsToBuy(_view.SelectedCharacter) == 0;
	}

	public RangedValueDTO GetPurchaseLimit()
	{
		return new RangedValueDTO((int)RemainingItemsToBuy(_view.SelectedCharacter), (int)_tradingController.MaxItemsToBuy(IsTravellingMerchant));
	}

	public bool CanBuyItem(CharacterState character, ItemDef itemDef, bool throwException = false)
	{
		return _tradingController.CanBuyItem(character, itemDef, IsTravellingMerchant, throwException);
	}

	public uint RemainingItemsToBuy(CharacterState character)
	{
		return _tradingController.RemainingItemsToBuy(character, IsTravellingMerchant);
	}

	public void ShowBuyItemDialog(ItemID itemID, CharacterState character)
	{
		if (!_view.GameController.DefRepository.Items.TryGetValue(itemID, out var def) || !CanBuyItem(character, def))
		{
			return;
		}
		string text = FHUILocFormatting.PopulateParams("GUI_CONFIRMATION_BUYITEM", LocalizationManager.GetTranslation(def.NameLocKey), _tradingController.GetDiscountedCost(def, 0, !IsTravellingMerchant));
		GenericModalViewState state = new GenericModalViewState(text, LocalizationManager.GetTranslation("GUI_BUY_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRM"), (bool sellConfirmed) =>
		{
			if (sellConfirmed)
			{
				Action_BuyAndEquipItem(itemID, character, unequipCurrent: false);
			}
		});
		state.ValidationHandler = () => CanBuyItem(character, def);
		state.OnValidFail = () =>
		{
			_view.TryPopState(state);
		};
		_view.PushState(state);
	}

	public void ShowSellItemDialog(ItemID itemID, CharacterState character, bool inCreation = false)
	{
		if (!_view.GameController.DefRepository.Items.TryGetValue(itemID, out var value))
		{
			return;
		}
		bool flag = _view.SelectedCharacter.EquipedItemIDs.Any((ItemID id) => id == itemID);
		int itemSellValue = GetItemSellValue(value, inCreation);
		string text = (flag ? FHUILocFormatting.PopulateParams("GUI_CONFIRMATION_SELLITEM_EQUIPPED", LocalizationManager.GetTranslation(value.NameLocKey), _view.SelectedCharacter.Name, itemSellValue) : FHUILocFormatting.PopulateParams("GUI_CONFIRMATION_SELLITEM", LocalizationManager.GetTranslation(value.NameLocKey), itemSellValue));
		_view.PushState(new GenericModalViewState(text, LocalizationManager.GetTranslation("GUI_SELL_CONFIRMATION_TITLE"), (bool sellConfirmed) =>
		{
			if (sellConfirmed)
			{
				SellItem(itemID, _view.SelectedCharacter);
			}
		}));
	}

	public bool SellItem(ItemID itemID, CharacterState character, bool throwException = false)
	{
		ItemDef itemDef = DefRepo.Items[itemID];
		return SellItem(itemDef, character, throwException);
	}

	public bool BuyItem(ItemID itemID, CharacterState character, bool throwException = false)
	{
		ItemDef itemDef = DefRepo.Items[itemID];
		return BuyItem(itemDef, character, autoEquip: false, unequipCurrent: false, throwException);
	}

	public bool EquipItem(ItemID itemID, CharacterState character, bool unequipCurrent = true, bool throwException = false)
	{
		return EquipItem(DefRepo.Items[itemID], character, unequipCurrent, throwException);
	}

	public bool UnequipItem(ItemID itemID, CharacterState character, bool throwException = false)
	{
		return UnequipItem(DefRepo.Items[itemID], character, throwException);
	}

	private bool EquipItem(ItemDef itemDef, CharacterState character, bool unequipCurrent = true, bool throwException = false)
	{
		if (character.EquipedItems.Contains(itemDef))
		{
			return false;
		}
		FHMapChoreographer choreographer = Choreographer;
		if (unequipCurrent)
		{
			List<ItemDef> currentItemInSlotForUnequipping = choreographer.GameController.CharacterController.GetCurrentItemInSlotForUnequipping(character, itemDef);
			if (currentItemInSlotForUnequipping != null)
			{
				foreach (ItemDef item in currentItemInSlotForUnequipping)
				{
					choreographer.ExecuteCommand(FHRPC.Campaign.Character.UnequipItem, character.ID, item);
				}
			}
		}
		bool flag = _characterController.CanEquipItem(character, itemDef, throwException);
		if (flag)
		{
			choreographer.ExecuteCommand(FHRPC.Campaign.Character.EquipItem, character.ID, itemDef);
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.itemEquipAudioItem);
		}
		return flag;
	}

	private bool UnequipItem(ItemDef itemDef, CharacterState character, bool throwException = false)
	{
		FHMapChoreographer choreographer = _view.Choreographer;
		if (!character.EquipedItemIDs.Contains(itemDef.ID))
		{
			if (!throwException)
			{
				return false;
			}
			throw new FHGameException("Cannot unequip item");
		}
		choreographer.ExecuteCommand(FHRPC.Campaign.Character.UnequipItem, character.ID, itemDef);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.itemUnequipAudioItem);
		return true;
	}

	private bool BuyItem(ItemDef itemDef, CharacterState character, bool autoEquip = false, bool unequipCurrent = false, bool throwException = false)
	{
		FHMapChoreographer choreographer = _view.Choreographer;
		bool flag = CanBuyItem(character, itemDef, throwException);
		if (flag)
		{
			bool flag2 = autoEquip && (unequipCurrent || _characterController.HasFreeItemSlot(character, itemDef));
			choreographer.ExecuteCommand(FHRPC.Campaign.Trading.BuyItem, character.ID, itemDef, IsTravellingMerchant, autoEquip, unequipCurrent);
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.merchantBuyAudioItem);
			if (flag2)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance?.itemEquipAudioItem);
			}
		}
		return flag;
	}

	public bool Action_BuyAndEquipItem(ItemID itemID, CharacterID characterID, bool unequipCurrent, bool throwException = false)
	{
		if (!GameController.CharacterController.TryGetCharacter(characterID, out var character))
		{
			return false;
		}
		ItemDef itemDef = DefRepo.Items[itemID];
		return BuyItem(itemDef, character, autoEquip: true, unequipCurrent, throwException);
	}

	public bool Action_BuyAndEquipItem(ItemID itemID, CharacterState character, bool unequipCurrent, bool throwException = false)
	{
		ItemDef itemDef = DefRepo.Items[itemID];
		return BuyItem(itemDef, character, autoEquip: true, unequipCurrent, throwException);
	}

	private bool SellItem(ItemDef itemDef, CharacterState character, bool throwException = false)
	{
		if (!_tradingController.CanSellItem(character, itemDef, throwException))
		{
			return false;
		}
		Choreographer.ExecuteCommand(FHRPC.Campaign.Trading.SellItem, character.ID, itemDef);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.merchantSellAudioItem);
		return true;
	}

	public FHItemTooltip.ItemTooltipCost GetTooltipCost(UIItemScreenFunctionality screenFunctionality, ItemDataDTO itemData, CharacterState character)
	{
		if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Buy))
		{
			return new FHItemTooltip.ItemTooltipCost
			{
				CostLiteral = FHUILocFormatting.Gold((int)itemData.Cost, character.Gold >= itemData.Cost)
			};
		}
		if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Sell))
		{
			return new FHItemTooltip.ItemTooltipCost
			{
				CostLiteral = FHUILocFormatting.Gold(itemData.SellGoldValue)
			};
		}
		if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Refund))
		{
			return new FHItemTooltip.ItemTooltipCost
			{
				CostLiteral = FHUILocFormatting.Gold((int)itemData.Cost)
			};
		}
		return default;
	}

	public IEnumerable<FHItemTooltip.ItemTooltipHint> GetTooltipHints(UIItemScreenFunctionality screenFunctionality, ItemDataDTO itemData, CharacterState character)
	{
		if (character != null)
		{
			if (character != null && screenFunctionality.HasFlag(UIItemScreenFunctionality.Buy) && CanBuyItem(character, DefRepo.Items[itemData.ID]))
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_BUY"),
					IsAction = true
				};
			}
			if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Buy) && character.OwnedItemIDs.Contains(itemData.ID))
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_ITEM_OWNED"), FHUIColors.MISSING_RESOURCE)
				};
			}
			int discountedCost = _tradingController.GetDiscountedCost(DefRepo.Items[itemData.ID], _view.GameState.BuyItemGoldDiscount);
			if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Buy) && !CanBuyItem(character, DefRepo.Items[itemData.ID]) && discountedCost > character.Gold)
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_INSUFFICIENT_RESOURCES"), FHUIColors.MISSING_RESOURCE)
				};
			}
			if (character.PartyState != CharacterPartyStateType.CreationProcess && screenFunctionality.HasFlag(UIItemScreenFunctionality.Buy) && RemainingItemsToBuy(character) == 0)
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_weekly_limit_reached"), FHUIColors.MISSING_RESOURCE)
				};
			}
			if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Equip))
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_EQUIP"),
					IsAction = true
				};
			}
			if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Unequip))
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_UNEQUIP"),
					IsAction = true
				};
			}
			if (screenFunctionality.HasFlag(UIItemScreenFunctionality.Sell))
			{
				yield return new FHItemTooltip.ItemTooltipHint
				{
					Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.DoubleClick, "GUI_SELL"),
					IsAction = true
				};
			}
			yield return new FHItemTooltip.ItemTooltipHint
			{
				Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.MiddleClick, "GUI_INSPECT"),
				IsAction = true
			};
		}
	}
}

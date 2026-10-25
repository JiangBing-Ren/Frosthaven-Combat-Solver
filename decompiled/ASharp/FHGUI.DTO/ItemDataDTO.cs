using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.DTO;

public class ItemDataDTO
{
	public struct ItemEffectDTO
	{
		public Sprite Icon;

		public string Name;
	}

	public struct ItemScenarioData
	{
		public CItem.EItemSlot Slot;

		public CItem.EItemSlotState SlotState;

		public int SlotIndex;

		public bool IsSpent => SlotState == CItem.EItemSlotState.Spent;

		public bool IsLost => SlotState == CItem.EItemSlotState.Consumed;

		public bool IsLocked => SlotState == CItem.EItemSlotState.Locked;

		public ItemScenarioData(CItem item)
		{
			Slot = item.Slot;
			SlotState = item.SlotState;
			SlotIndex = item.SlotIndex;
		}

		public ItemScenarioData(ItemCardYMLData item)
		{
			Slot = item.Slot;
			SlotState = CItem.EItemSlotState.None;
			SlotIndex = 0;
		}
	}

	public ItemID ID;

	public int ScenarioItemID;

	public Sprite Icon;

	public string NameKey;

	public string NameLocalized;

	public string DescriptionKey;

	public string DescriptionLocalized;

	public ItemEquipType ItemType;

	public Sprite ItemTypeIcon;

	public uint Quantity;

	public int MaxQuantity;

	public uint Cost;

	public string CostLocalized;

	public int SellGoldValue;

	public ResourceCostDTO ResourceCost;

	public bool ShowCost;

	public bool CanBuy;

	public bool CanAfford;

	public bool IsEquipped;

	public bool IsPotion;

	public bool CanBeDistilled;

	public bool IsForbidden;

	public string OwnedBy;

	public bool OwnedInstance;

	public ItemCraftCostDTO CraftCost;

	public ItemEffectDTO[] Effects;

	public bool IsNew;

	public int? FlipScenarioItemID;

	public bool IsFlipItemSideFront;

	public ItemScenarioData? ScenarioData;

	public bool IsEquipment
	{
		get
		{
			if (ItemType != ItemEquipType.Body && ItemType != ItemEquipType.Feet)
			{
				return ItemType == ItemEquipType.Head;
			}
			return true;
		}
	}

	public bool IsWeapon
	{
		get
		{
			if (ItemType != ItemEquipType.OneHandWeapon)
			{
				return ItemType == ItemEquipType.TwoHandWeapon;
			}
			return true;
		}
	}

	public ItemDataDTO(string name, string description, Sprite icon, uint quantity)
	{
		NameLocalized = name;
		DescriptionLocalized = description;
		Icon = icon;
		Quantity = quantity;
	}

	public ItemDataDTO()
	{
	}

	public ItemDataDTO(ItemDef itemDef, uint? overrideCost = null)
	{
		ID = itemDef.ID;
		ScenarioItemID = itemDef.ScenarioItemID;
		NameKey = itemDef.NameLocKey ?? $"Empty NameKey [Item({itemDef.ID.ID})]";
		NameLocalized = ItemDTOUtilities.GetNameLocalized(itemDef);
		DescriptionKey = itemDef.DescriptionLocKey ?? $"Empty DescKey [Item({itemDef.ID.ID})]";
		DescriptionLocalized = ItemDTOUtilities.GetDescriptionLocalized(itemDef);
		ItemType = itemDef.Type;
		Icon = itemDef.GetItemImage();
		ItemTypeIcon = itemDef.GetItemTypeImage();
		Quantity = 1u;
		Cost = overrideCost ?? itemDef.GoldCost;
		CostLocalized = this.GetLocalizedCost();
		MaxQuantity = (int)itemDef.Quantity;
		ShowCost = false;
		CanBuy = false;
		IsEquipped = false;
		IsNew = false;
		IsPotion = itemDef.CanBeBrewed;
		CanBeDistilled = itemDef.CanBeDistilled;
		FlipScenarioItemID = itemDef.FlipScenarioItemID;
		IsFlipItemSideFront = itemDef.IsFlipItemSideFront;
		ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData item) => item.ID == itemDef.ScenarioItemID);
		if (itemCardYMLData != null)
		{
			Effects = GetItemEffects(itemCardYMLData).ToArray();
			ScenarioData = new ItemScenarioData(itemCardYMLData);
		}
	}

	public ItemDataDTO(ItemCardYMLData itemYml, uint? overrideCost = null)
	{
		ID = new ItemID(0u);
		ScenarioItemID = itemYml.ID;
		NameKey = itemYml.Name;
		NameLocalized = ItemDTOUtilities.GetNameLocalized(itemYml);
		DescriptionKey = ItemDTOUtilities.GetDescriptionKey(itemYml) ?? $"Empty DescKey [Item({itemYml.ID})]";
		DescriptionLocalized = ItemDTOUtilities.GetDescriptionLocalized(itemYml);
		ItemType = CrossReference.ItemEquipTypeToEItemSlot(itemYml.Slot);
		Icon = itemYml.GetItemImage();
		ItemTypeIcon = itemYml.GetItemTypeImage();
		Quantity = 1u;
		Cost = (uint)(((int?)overrideCost) ?? itemYml.Cost);
		CostLocalized = this.GetLocalizedCost();
		MaxQuantity = -1;
		ShowCost = false;
		CanBuy = false;
		IsEquipped = false;
		IsNew = false;
		FlipScenarioItemID = (itemYml.FlipItemID.IsNullOrEmpty() ? ((int?)null) : new int?(int.Parse(itemYml.FlipItemID)));
		IsFlipItemSideFront = itemYml.FlipItemSide == CItem.EFlipItemSide.Front;
		Effects = GetItemEffects(itemYml).ToArray();
		ScenarioData = new ItemScenarioData(itemYml);
	}

	public ItemDataDTO(CItem item)
		: this(item.YMLData)
	{
		ScenarioData = new ItemScenarioData(item);
	}

	private IEnumerable<ItemEffectDTO> GetItemEffects(ItemCardYMLData itemCard)
	{
		if (itemCard.Data?.AdditionalModifiers != null)
		{
			foreach (KeyValuePair<AttackModifierYMLData, int> additionalModifier in itemCard.Data.AdditionalModifiers)
			{
				yield return new ItemEffectDTO
				{
					Icon = (UICharacterUtils.IsBasicModifier(additionalModifier.Key) ? UIInfoTools.Instance.GetAttackModifierBaseIcon(additionalModifier.Key) : UIInfoTools.Instance.GetAttackModifierIcon(additionalModifier.Key)),
					Name = $"x{additionalModifier.Value}"
				};
			}
		}
		if (itemCard.ShowPersistentBonusIcon)
		{
			yield return new ItemEffectDTO
			{
				Icon = UIInfoTools.Instance.PersistentBonus
			};
		}
		if (itemCard.ShowRoundBonusIcon)
		{
			yield return new ItemEffectDTO
			{
				Icon = UIInfoTools.Instance.RoundBonus
			};
		}
		if (itemCard.Usage == CItem.EUsageType.Spent)
		{
			yield return new ItemEffectDTO
			{
				Icon = UIInfoTools.Instance.SpentIcon
			};
		}
		else if (itemCard.Usage == CItem.EUsageType.Consumed)
		{
			yield return new ItemEffectDTO
			{
				Icon = UIInfoTools.Instance.BurnIcon
			};
		}
	}

	public ItemDataDTO(ItemDataDTO item)
	{
		ID = item.ID;
		ScenarioItemID = item.ScenarioItemID;
		Icon = item.Icon;
		NameKey = item.NameKey;
		NameLocalized = item.NameLocalized;
		DescriptionKey = item.DescriptionKey;
		DescriptionLocalized = item.DescriptionLocalized;
		ItemType = item.ItemType;
		ItemTypeIcon = item.ItemTypeIcon;
		Quantity = item.Quantity;
		MaxQuantity = item.MaxQuantity;
		Cost = item.Cost;
		CostLocalized = item.CostLocalized;
		SellGoldValue = item.SellGoldValue;
		ResourceCost = item.ResourceCost;
		ShowCost = item.ShowCost;
		CanBuy = item.CanBuy;
		CanAfford = item.CanAfford;
		IsEquipped = item.IsEquipped;
		IsPotion = item.IsPotion;
		CanBeDistilled = item.CanBeDistilled;
		IsForbidden = item.IsForbidden;
		OwnedBy = item.OwnedBy;
		CraftCost = item.CraftCost;
		Effects = item.Effects.ToArray();
		IsNew = item.IsNew;
		FlipScenarioItemID = item.FlipScenarioItemID;
		IsFlipItemSideFront = item.IsFlipItemSideFront;
	}

	public ItemDataDTO(ItemDef itemDef, int maxQuantity, bool showCost, bool isNew, bool canBuy, bool isEquipped, int sellGoldValue, bool canAfford, bool isForbidden, uint? overrideCost = null)
		: this(itemDef, overrideCost)
	{
		MaxQuantity = maxQuantity;
		ShowCost = showCost;
		IsNew = isNew;
		CanBuy = canBuy;
		IsEquipped = isEquipped;
		SellGoldValue = sellGoldValue;
		CanAfford = canAfford;
		IsForbidden = isForbidden;
	}

	public static List<ItemDataDTO> FromItemDef(IEnumerable<ItemDef> items, Func<ItemDef, uint> overrideCost = null)
	{
		return items.Select((ItemDef item) => new ItemDataDTO(item, overrideCost?.Invoke(item))).ToList();
	}

	public static List<ItemDataDTO> FromItemDefStacked(IEnumerable<ItemDef> items, CharacterState characterState, bool showCost = false, bool showEquipped = true, Func<ItemDef, uint> overrideCost = null)
	{
		return (from i in items
			group i by i.ID).Select((IGrouping<ItemID, ItemDef> i) =>
		{
			ItemDef itemDef = items.First((ItemDef ii) => ii.ID == i.Key);
			int quantity = i.Count();
			uint num = overrideCost?.Invoke(itemDef) ?? itemDef.GoldCost;
			return new ItemDataDTO(itemDef, (int)itemDef.Quantity, showCost, isNew: false, num <= characterState.Gold && !characterState.OwnedItems.Contains(itemDef), showEquipped && characterState.EquipedItemIDs.Contains(itemDef.ID), 0, num <= characterState.Gold, characterState.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == itemDef.ID), num)
			{
				Quantity = (uint)quantity,
				OwnedInstance = characterState.OwnedItems.Contains(itemDef)
			};
		}).ToList();
	}

	public static List<ItemDataDTO> FromItemDef(IEnumerable<ItemDef> items, CharacterState characterState, bool showCost = false, bool showEquipped = true, Func<ItemDef, uint> overrideCost = null)
	{
		return items.OrderBy((ItemDef def) => def.ID).Select((ItemDef itemDef) =>
		{
			int num = 1;
			uint num2 = overrideCost?.Invoke(itemDef) ?? itemDef.GoldCost;
			return new ItemDataDTO(itemDef, num, showCost, isNew: false, num2 <= characterState.Gold && !characterState.OwnedItems.Contains(itemDef), showEquipped && characterState.EquipedItemIDs.Contains(itemDef.ID), 0, num2 <= characterState.Gold, characterState.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == itemDef.ID), num2)
			{
				Quantity = (uint)num
			};
		}).ToList();
	}

	public override string ToString()
	{
		return $"ItemDataDTO: {ID}, Name: {NameLocalized}";
	}
}

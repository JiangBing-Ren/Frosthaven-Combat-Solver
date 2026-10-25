using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class ItemCardYMLData
{
	private CItem m_Item;

	public int ID { get; set; }

	public string StringID { get; set; }

	public string Name { get; set; }

	public string Art { get; set; }

	public string Description { get; set; }

	public int TotalInGame { get; set; }

	public int Cost { get; set; }

	public CItem.EItemSlot Slot { get; set; }

	public string FlipItemID { get; set; }

	public CItem.EFlipItemSide FlipItemSide { get; set; }

	public bool FlipEndOfTurn { get; set; }

	public CItem.EUsageType Usage { get; set; }

	public bool PersistentBonus { get; set; }

	public bool ShowPersistentBonusIcon => PersistentBonus;

	public bool RoundBonus { get; set; }

	public bool ShowRoundBonusIcon
	{
		get
		{
			if (RoundBonus)
			{
				return true;
			}
			if (Data?.Abilities != null)
			{
				if (Data.Abilities.Any((CAbility ability) => ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Round && !ability.ActiveBonusData.Hidden))
				{
					return true;
				}
				if (Data.Abilities.Any((CAbility ability) => ability.SubAbilities.Any((CAbility subAbility) => subAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Round && !subAbility.ActiveBonusData.Hidden)))
				{
					return true;
				}
				if (Data.Abilities.Any((CAbility ability) => ability is CAbilityControlActor cAbilityControlActor && cAbilityControlActor.ControlActorData.ControlAbilities.Any((CAbility controlAbility) => controlAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Round && !controlAbility.ActiveBonusData.Hidden)))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool? UsedWhenEquipped { get; set; }

	public CItem.EItemTrigger Trigger { get; set; }

	public CItem.EItemRarity Rarity { get; set; }

	public CardLayout CardLayout { get; set; }

	public List<ElementInfusionBoardManager.EElement> Consumes { get; set; }

	public ItemData Data { get; set; }

	public CItem.EItemType ItemType { get; set; }

	public int ProsperityRequirement { get; set; }

	public List<string> ValidEquipCharacterClassIDs { get; set; }

	public string FileName { get; private set; }

	public bool? PermanentlyConsumed { get; set; }

	public bool Tradeable { get; set; }

	public string PetUnlockedBy { get; set; }

	public bool IgnoreImpair { get; set; }

	public int MoraleCost { get; set; }

	public int ItemInstanceCount { get; private set; }

	public CItem GetItem
	{
		get
		{
			if (m_Item == null)
			{
				m_Item = new CItem(ID, CBaseCard.ECardType.Item);
			}
			return m_Item?.Copy();
		}
	}

	public CItem GetPet
	{
		get
		{
			if (m_Item == null)
			{
				m_Item = new CItem(ID, CBaseCard.ECardType.Pet);
			}
			return m_Item?.Copy();
		}
	}

	public bool IsProsperityItem
	{
		get
		{
			if (ProsperityRequirement != int.MaxValue)
			{
				return ProsperityRequirement > 0;
			}
			return false;
		}
	}

	public void ModifyItemCount(int count)
	{
		ItemInstanceCount = count;
	}

	public ItemCardYMLData(string fileName)
	{
		FileName = fileName;
		m_Item = null;
		ID = int.MaxValue;
		StringID = null;
		Name = null;
		Description = null;
		Art = null;
		TotalInGame = int.MaxValue;
		Cost = int.MaxValue;
		Slot = CItem.EItemSlot.None;
		Usage = CItem.EUsageType.None;
		FlipItemID = null;
		FlipItemSide = CItem.EFlipItemSide.None;
		UsedWhenEquipped = null;
		Trigger = CItem.EItemTrigger.None;
		Rarity = CItem.EItemRarity.None;
		CardLayout = null;
		Consumes = new List<ElementInfusionBoardManager.EElement>();
		Data = null;
		ItemType = CItem.EItemType.None;
		ProsperityRequirement = int.MaxValue;
		ValidEquipCharacterClassIDs = new List<string>();
		PermanentlyConsumed = null;
		Tradeable = true;
		PetUnlockedBy = null;
		PersistentBonus = false;
		RoundBonus = false;
		IgnoreImpair = false;
		MoraleCost = 0;
	}

	public bool Validate()
	{
		bool result = true;
		if (StringID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No StringID entry for item in file " + FileName);
			result = false;
		}
		if (Name == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No name entry for item in file " + FileName);
			result = false;
		}
		else if (Slot == CItem.EItemSlot.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid slot for item in file " + FileName);
			result = false;
		}
		else if (Rarity == CItem.EItemRarity.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid/Unspecified Rarity for item in file " + FileName);
			result = false;
		}
		else if (Usage == CItem.EUsageType.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid usage for item in file " + FileName);
			result = false;
		}
		else if (Data == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No item data found for item in file " + FileName);
			result = false;
		}
		else if (Trigger == CItem.EItemTrigger.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Trigger specified item in file " + FileName);
			result = false;
		}
		else if ((Data.Abilities == null || Data.Abilities.Count == 0) && (Data.Overrides == null || Data.Overrides.Count == 0) && Data.ShieldValue == 0 && Data.RetaliateValue == 0 && Data.SmallSlots == 0 && (Data.Vars == null || Data.Vars.Count == 0))
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid item abilities, overrides, or shield/retaliate/loot values were found for item in file " + FileName);
			result = false;
		}
		else if (CardLayout == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid layout specified for item in file " + FileName);
			result = false;
		}
		else if (Usage == CItem.EUsageType.Flip && (string.IsNullOrEmpty(FlipItemID) || FlipItemSide == CItem.EFlipItemSide.None))
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No FlipItemID or FlipItemSide entry for Flip Usage for item in file " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(ItemCardYMLData itemData)
	{
		if (itemData.Name != null)
		{
			Name = itemData.Name;
		}
		if (itemData.Art != null)
		{
			Art = itemData.Art;
		}
		if (itemData.TotalInGame != int.MaxValue)
		{
			TotalInGame = itemData.TotalInGame;
		}
		if (itemData.Cost != int.MaxValue)
		{
			Cost = itemData.Cost;
		}
		if (itemData.Slot != CItem.EItemSlot.None)
		{
			Slot = itemData.Slot;
		}
		if (itemData.Usage != CItem.EUsageType.None)
		{
			Usage = itemData.Usage;
		}
		if (itemData.PersistentBonus != PersistentBonus)
		{
			PersistentBonus = itemData.PersistentBonus;
		}
		if (itemData.RoundBonus != RoundBonus)
		{
			RoundBonus = itemData.RoundBonus;
		}
		if (itemData.UsedWhenEquipped.HasValue)
		{
			UsedWhenEquipped = itemData.UsedWhenEquipped;
		}
		if (itemData.Trigger != CItem.EItemTrigger.None)
		{
			Trigger = itemData.Trigger;
		}
		if (itemData.Rarity != CItem.EItemRarity.None)
		{
			Rarity = itemData.Rarity;
		}
		if (itemData.CardLayout != null)
		{
			CardLayout = itemData.CardLayout;
		}
		if (itemData.Data != null)
		{
			Data = itemData.Data;
			Consumes = itemData.Consumes;
		}
		if (itemData.ItemType != CItem.EItemType.None)
		{
			ItemType = itemData.ItemType;
		}
		if (itemData.ProsperityRequirement != int.MaxValue)
		{
			ProsperityRequirement = itemData.ProsperityRequirement;
		}
		if (itemData.ValidEquipCharacterClassIDs != null)
		{
			ValidEquipCharacterClassIDs.AddRange(itemData.ValidEquipCharacterClassIDs);
		}
		if (itemData.PermanentlyConsumed.HasValue)
		{
			PermanentlyConsumed = itemData.PermanentlyConsumed;
		}
		if (itemData.Tradeable != Tradeable)
		{
			Tradeable = itemData.Tradeable;
		}
		if (itemData.PetUnlockedBy != PetUnlockedBy)
		{
			PetUnlockedBy = itemData.PetUnlockedBy;
		}
		if (itemData.IgnoreImpair != IgnoreImpair)
		{
			IgnoreImpair = itemData.IgnoreImpair;
		}
		if (itemData.MoraleCost != MoraleCost)
		{
			MoraleCost = itemData.MoraleCost;
		}
	}

	public override string ToString()
	{
		return $"{StringID} ({ID})";
	}
}

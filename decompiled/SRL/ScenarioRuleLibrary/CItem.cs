using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.Tools;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CItem : CBaseCard, ISerializable
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EItemSlotState
	{
		None,
		Equipped,
		Useable,
		Selected,
		Locked,
		Consumed,
		Spent,
		Active,
		TempLocked
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EUsageType
	{
		None,
		Spent,
		Consumed,
		Unrestricted,
		Flip
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EItemSlot
	{
		None,
		Head,
		Body,
		Legs,
		OneHand,
		TwoHand,
		SmallItem,
		QuestItem,
		Pet
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EFlipItemSide
	{
		None,
		Front,
		Back
	}

	[Serializable]
	[KronosEnumPolice(4)]
	[Flags]
	public enum EItemTrigger
	{
		None = 0,
		PassiveEffect = 1,
		AtStartOfRound = 2,
		DuringOwnTurn = 4,
		SingleTarget = 8,
		SingleAbility = 0x10,
		EntireAction = 0x20,
		OnAttacked = 0x40,
		AtEndOfTurn = 0x80,
		OnNegativeConditionFromEnemy = 0x100,
		OnPositiveConditionFromEnemy = 0x200,
		OnPreventDamageByLosingCards = 0x400,
		OnNegativeConditionGlobal = 0x800,
		OnPositiveConditionGlobal = 0x1000,
		BeforeInitiativeCalc = 0x2000,
		OnAttackTargetOneByOne = 0x4000,
		AfterAttackedIgnoringDamageTaken = 0x8000,
		BeforeShortRest = 0x10000,
		OnSummonKilled = 0x20000,
		OnAttackedIgnoringDamageTaken = 0x40000,
		AtStartOfTurn = 0x80000,
		OnActorMoved = 0x100000,
		OnAttackedBeforeAttackModifiers = 0x200000,
		OnAnyActorDeath = 0x400000,
		OnAbilityApplied = 0x800000,
		BeforeStartOfTurn = 0x1000000,
		AfterShortRest = 0x2000000,
		OnAbilityEnded = 0x4000000,
		AfterAttackOneByOne = 0x8000000,
		AfterMonsterCardsRevealed = 0x10000000,
		OnLootFromDeck = 0x20000000,
		OnAutomaticActivateProp = 0x40000000
	}

	[Serializable]
	[KronosEnumPolice(1)]
	public enum EItemType
	{
		None,
		Ability,
		Override,
		Vars
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EItemRarity
	{
		None,
		Common,
		Rare,
		Relic
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EPlayerExecutionOrder
	{
		None,
		ForEachOwner
	}

	public static readonly EItemSlotState[] ItemSlotStates = (EItemSlotState[])Enum.GetValues(typeof(EItemSlotState));

	public static readonly EUsageType[] UsageTypes = (EUsageType[])Enum.GetValues(typeof(EUsageType));

	public static readonly EItemSlot[] ItemSlots = (EItemSlot[])Enum.GetValues(typeof(EItemSlot));

	public static readonly EFlipItemSide[] FlipItemSides = (EFlipItemSide[])Enum.GetValues(typeof(EFlipItemSide));

	public static readonly EItemTrigger[] ItemTriggers = (EItemTrigger[])Enum.GetValues(typeof(EItemTrigger));

	public static readonly EItemType[] ItemTypes = (EItemType[])Enum.GetValues(typeof(EItemType));

	public static readonly EItemRarity[] ItemRarities = (EItemRarity[])Enum.GetValues(typeof(EItemRarity));

	public static EPlayerExecutionOrder[] PlayerExecutionOrders = (EPlayerExecutionOrder[])Enum.GetValues(typeof(EPlayerExecutionOrder));

	[StateDump(Skip = true)]
	private EItemSlotState _slotState;

	private ItemCardYMLData m_YMLData;

	[StateDump(Skip = true)]
	public string ItemGuid { get; private set; }

	[StateDump(Skip = true)]
	public EItemSlotState SlotState
	{
		get
		{
			return _slotState;
		}
		set
		{
			if (_slotState != value)
			{
				Log.GameplayInfo($"Setting {this} from slot state {_slotState} to slot state {value}");
				_slotState = value;
			}
		}
	}

	public int SlotIndex { get; set; }

	public int RoundUsed { get; set; }

	public bool HasBeenUsed => RoundUsed > 0;

	public bool IsSlotIndexSet { get; set; }

	public uint NetworkID { get; set; }

	[StateDump(Skip = true)]
	public bool IsNew { get; set; }

	[StateDump(Skip = true)]
	public List<ElementInfusionBoardManager.EElement> ChosenElement { get; set; } = new List<ElementInfusionBoardManager.EElement>();

	[StateDump(Skip = true)]
	public CActor SingleTarget { get; set; }

	public ItemCardYMLData YMLData
	{
		get
		{
			if (m_YMLData == null && CardType == ECardType.Item)
			{
				m_YMLData = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData s) => s.ID == ID);
			}
			if (m_YMLData == null && CardType == ECardType.Pet)
			{
				m_YMLData = ScenarioRuleClient.SRLYML.PetCards.SingleOrDefault((ItemCardYMLData s) => s.ID == ID);
			}
			if (m_YMLData == null)
			{
				Log.ErrorWithReport($"Error: Unable to find YML for {CardType} with ID " + ID + " but this item has been serialized within the Save Data.");
				throw new Exception($"Error: Unable to find YML for {CardType} with ID " + ID + " but this item has been serialized within the Save Data.");
			}
			return m_YMLData;
		}
	}

	public IEnumerable<CActiveBonus> MultiUseBonuses => ActiveBonuses.Where((CActiveBonus bonus) => !bonus.Ability.GetMiscVar("ConsumeItemInstantly").GetBool() && ((bonus.HasTracker && bonus.TrackerMaxIndex > 1) || bonus.Duration == CActiveBonus.EActiveBonusDurationType.Persistent));

	public EItemSlot Slot
	{
		get
		{
			if (CardType != ECardType.Pet)
			{
				return YMLData.Slot;
			}
			return EItemSlot.Pet;
		}
	}

	public int SellPrice => (int)((float)YMLData.Cost / 2f);

	public bool Tradeable => YMLData.Tradeable;

	public bool IgnoreImpair => YMLData.IgnoreImpair;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ItemGuid", ItemGuid);
		info.AddValue("SlotState", SlotState);
		info.AddValue("SlotIndex", SlotIndex);
		info.AddValue("RoundUsed", RoundUsed);
		info.AddValue("IsSlotIndexSet", IsSlotIndexSet);
		info.AddValue("NetworkID", NetworkID);
	}

	protected CItem(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ItemGuid":
					ItemGuid = info.GetString("ItemGuid");
					break;
				case "SlotState":
					SlotState = (EItemSlotState)info.GetValue("SlotState", typeof(EItemSlotState));
					break;
				case "SlotIndex":
					SlotIndex = info.GetInt32("SlotIndex");
					break;
				case "RoundUsed":
					RoundUsed = info.GetInt32("RoundUsed");
					break;
				case "IsSlotIndexSet":
					IsSlotIndexSet = info.GetBoolean("IsSlotIndexSet");
					break;
				case "NetworkID":
					NetworkID = (uint)info.GetValue("NetworkID", typeof(uint));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CItem(int id, ECardType type = ECardType.Item, uint networkID = 0u)
		: base(id, type, id.ToString())
	{
		ItemGuid = Guid.NewGuid().ToString();
		SlotState = EItemSlotState.None;
		SlotIndex = int.MaxValue;
		RoundUsed = 0;
		IsSlotIndexSet = false;
		IsNew = true;
		NetworkID = networkID;
	}

	public CItem(int id, ECardType type = ECardType.Item)
		: base(id, type, id.ToString())
	{
		ItemGuid = Guid.NewGuid().ToString();
		SlotState = EItemSlotState.None;
		SlotIndex = int.MaxValue;
		RoundUsed = 0;
		IsSlotIndexSet = false;
		IsNew = true;
		NetworkID = 0u;
	}

	public CItem(int id, string itemGuid, uint networkID, ECardType type = ECardType.Item)
		: base(id, type, id.ToString())
	{
		ItemGuid = itemGuid;
		SlotState = EItemSlotState.None;
		SlotIndex = int.MaxValue;
		RoundUsed = 0;
		IsSlotIndexSet = false;
		IsNew = true;
		NetworkID = networkID;
	}

	public CItem Copy()
	{
		return new CItem(ID, ItemGuid, NetworkID, CardType)
		{
			SlotState = SlotState,
			SlotIndex = SlotIndex,
			RoundUsed = RoundUsed,
			IsSlotIndexSet = IsSlotIndexSet,
			IsNew = IsNew
		};
	}

	public CItem Copy(Guid newGuid, uint networkID)
	{
		return Copy(newGuid.ToString(), networkID);
	}

	public CItem Copy(string newGuid, uint networkID)
	{
		return new CItem(ID, newGuid, networkID, CardType)
		{
			SlotState = SlotState,
			SlotIndex = SlotIndex,
			RoundUsed = RoundUsed,
			IsSlotIndexSet = IsSlotIndexSet,
			IsNew = IsNew
		};
	}

	public void GenerateNetworkID(int characterID)
	{
		if (NetworkID == 0 && characterID != 0)
		{
			NetworkID = (uint)(ID * 100 + characterID);
		}
	}

	public bool HasCondition(CCondition.EPositiveCondition condition)
	{
		if (YMLData.Data.Overrides != null && YMLData.Data.Overrides.Exists((CAbilityOverride it) => it.PositiveConditions != null && it.PositiveConditions.Contains(condition)))
		{
			return true;
		}
		if (YMLData.Data.Abilities != null && YMLData.Data.Abilities.Exists((CAbility it) => it.PositiveConditions != null && it.PositiveConditions.ContainsKey(condition)))
		{
			return true;
		}
		return false;
	}

	public bool HasCondition(CCondition.ENegativeCondition condition)
	{
		if (YMLData.Data.Overrides != null && YMLData.Data.Overrides.Exists((CAbilityOverride it) => it.NegativeConditions != null && it.NegativeConditions.Contains(condition)))
		{
			return true;
		}
		if (YMLData.Data.Abilities != null && YMLData.Data.Abilities.Exists((CAbility it) => it.NegativeConditions != null && it.NegativeConditions.ContainsKey(condition)))
		{
			return true;
		}
		return false;
	}

	public bool CanEquipItem(string characterClassID)
	{
		if (YMLData.ValidEquipCharacterClassIDs.Count > 0)
		{
			return YMLData.ValidEquipCharacterClassIDs.Contains(characterClassID);
		}
		return true;
	}

	public CAbility FindAbilityOnItemCard(string name, Guid? guid = null)
	{
		if (YMLData?.Data?.Abilities == null)
		{
			return null;
		}
		return GetAllAbilities(this).SingleOrDefault((CAbility s) => s.Name == name || (guid.HasValue && s.ID == guid.Value));
	}

	public static IEnumerable<CAbility> GetAllAbilities(CItem card)
	{
		foreach (CAbility ability in CAbilityCard.GetWithChildAbilities(card.YMLData.Data.Abilities))
		{
			yield return ability;
			if (ability.ActiveBonusData.CostAbility == null)
			{
				continue;
			}
			foreach (CAbility withChildAbility in CAbilityCard.GetWithChildAbilities(ability.ActiveBonusData.CostAbility))
			{
				yield return withChildAbility;
			}
		}
		if (card.YMLData.Data.Overrides == null)
		{
			yield break;
		}
		foreach (CAbility item in card.YMLData.Data.Overrides.SelectMany((CAbilityOverride x) => CAbilityCard.GetOverrideWithChildAbilities(x)))
		{
			yield return item;
		}
	}

	public CAbility FindAbilityOnItemCard(CAbility.EAbilityType type)
	{
		if (YMLData?.Data?.Abilities == null)
		{
			return null;
		}
		return YMLData.Data.Abilities.SingleOrDefault((CAbility s) => s.AbilityType == type);
	}

	public static List<Tuple<int, string>> Compare(CItem item1, CItem item2, string ownerType, string ownerGuid, string ownerID, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CBaseCard.Compare(item1, item2, isMPCompare));
			if (item1.ItemGuid != item2.ItemGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1301, "Item ItemGuid does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid }
				});
			}
			if (item1.SlotIndex != item2.SlotIndex)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1303, "Item SlotIndex does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3]
					{
						"Item SlotIndex",
						item1.SlotIndex.ToString(),
						item2.SlotIndex.ToString()
					}
				});
			}
			if (item1.IsSlotIndexSet != item2.IsSlotIndexSet)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1304, "Item IsSlotIndexSet does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3]
					{
						"Item IsSlotIndexSet",
						item1.IsSlotIndexSet.ToString(),
						item2.IsSlotIndexSet.ToString()
					}
				});
			}
			if (item1.NetworkID != item2.NetworkID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1306, "Item NetworkID does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3]
					{
						"Item NetworkID",
						item1.NetworkID.ToString(),
						item2.NetworkID.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1399, "Exception during Item State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}

	public TValue GetVar(string key)
	{
		if (YMLData.Data.Vars != null)
		{
			return YMLData.Data.Vars.SingleOrDefault(((string key, TValue value) x) => x.key == key).value;
		}
		return TValue.Unknown;
	}
}

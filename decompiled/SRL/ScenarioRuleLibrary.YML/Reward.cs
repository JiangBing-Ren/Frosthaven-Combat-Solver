using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class Reward : ISerializable
{
	public static readonly ETreasureType[] TreasureTypes = (ETreasureType[])Enum.GetValues(typeof(ETreasureType));

	public static readonly ETreasureDistributionType[] TreasureDistributionTypes = (ETreasureDistributionType[])Enum.GetValues(typeof(ETreasureDistributionType));

	public static readonly EGiveToCharacterType[] GiveToCharacterTypes = (EGiveToCharacterType[])Enum.GetValues(typeof(EGiveToCharacterType));

	public static readonly EGiveToCharacterRequirement[] GiveToCharacterRequirementTypes = (EGiveToCharacterRequirement[])Enum.GetValues(typeof(EGiveToCharacterRequirement));

	public static readonly ETreasureDistributionRestrictionType[] TreasureDistributionRestrictionTypes = (ETreasureDistributionRestrictionType[])Enum.GetValues(typeof(ETreasureDistributionRestrictionType));

	public ETreasureType Type;

	public int Amount;

	public int ItemID = -1;

	public LootCard.EType ResourceType;

	public List<int> CardIDs;

	public List<RewardCondition> Conditions;

	public List<RewardCondition> EnemyConditions;

	public int Durability = int.MaxValue;

	public List<ElementInfusionBoardManager.EElement> Infusions;

	public EEnhancement Enhancement;

	public string PerkName;

	public string CharacterID = "NoneID";

	public string UnlockName;

	public bool Unlock;

	public bool LevelUp;

	public ETreasureDistributionType TreasureDistributionType;

	public int SubChapter;

	public string GiveToCharacterID = "NoneID";

	public EGiveToCharacterType GiveToCharacterType;

	public EGiveToCharacterRequirement GiveToCharacterRequirement;

	public string GiveToCharacterRequirementCheckID;

	public string ConsumeSlot;

	public Dictionary<string, int> Modifiers;

	public ETreasureDistributionRestrictionType TreasureDistributionRestrictionType;

	public CItem Item { get; private set; }

	public List<CAbilityCard> Cards
	{
		get
		{
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (int id in CardIDs)
			{
				CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.SingleOrDefault((CAbilityCard x) => x.ID == id);
				if (cAbilityCard != null)
				{
					list.Add(cAbilityCard);
				}
			}
			return list;
		}
	}

	public PerksYMLData Perk => ScenarioRuleClient.SRLYML.Perks.SingleOrDefault((PerksYMLData s) => s.Name == PerkName);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Type", Type);
		info.AddValue("Amount", Amount);
		info.AddValue("ItemID", ItemID);
		info.AddValue("ResourceType", ResourceType);
		info.AddValue("CardIDs", CardIDs);
		info.AddValue("Conditions", Conditions);
		info.AddValue("EnemyConditions", EnemyConditions);
		info.AddValue("Durability", Durability);
		info.AddValue("Infusions", Infusions);
		info.AddValue("Enhancement", Enhancement);
		info.AddValue("PerkName", PerkName);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("UnlockName", UnlockName);
		info.AddValue("Unlock", Unlock);
		info.AddValue("LevelUp", LevelUp);
		info.AddValue("TreasureDistributionType", TreasureDistributionType);
		info.AddValue("SubChapter", SubChapter);
		info.AddValue("GiveToCharacterID", GiveToCharacterID);
		info.AddValue("GiveToCharacterType", GiveToCharacterType);
		info.AddValue("GiveToCharacterRequirement", GiveToCharacterRequirement);
		info.AddValue("GiveToCharacterRequirementCheckID", GiveToCharacterRequirementCheckID);
		info.AddValue("ConsumeSlot", ConsumeSlot);
		info.AddValue("Modifiers", Modifiers);
		info.AddValue("TreasureDistributionRestrictionType", TreasureDistributionRestrictionType);
	}

	protected Reward(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Type":
					Type = (ETreasureType)info.GetValue("Type", typeof(ETreasureType));
					break;
				case "Amount":
					Amount = info.GetInt32("Amount");
					break;
				case "ItemID":
					ItemID = info.GetInt32("ItemID");
					Item = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.ID == ItemID)?.GetItem;
					break;
				case "ResourceType":
					ResourceType = (LootCard.EType)info.GetValue("ResourceType", typeof(LootCard.EType));
					break;
				case "CardIDs":
					CardIDs = (List<int>)info.GetValue("CardIDs", typeof(List<int>));
					break;
				case "Conditions":
					Conditions = (List<RewardCondition>)info.GetValue("Conditions", typeof(List<RewardCondition>));
					break;
				case "EnemyConditions":
					EnemyConditions = (List<RewardCondition>)info.GetValue("Conditions", typeof(List<RewardCondition>));
					break;
				case "Durability":
					Durability = info.GetInt32("Durability");
					break;
				case "Infusions":
					Infusions = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Infusions", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "Enhancement":
					Enhancement = (EEnhancement)info.GetValue("Enhancement", typeof(EEnhancement));
					break;
				case "PerkName":
					PerkName = info.GetString("PerkName");
					break;
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "UnlockName":
					UnlockName = info.GetString("UnlockName");
					break;
				case "Unlock":
					Unlock = info.GetBoolean("Unlock");
					break;
				case "LevelUp":
					LevelUp = info.GetBoolean("LevelUp");
					break;
				case "TreasureDistributionType":
					TreasureDistributionType = (ETreasureDistributionType)info.GetValue("TreasureDistributionType", typeof(ETreasureDistributionType));
					break;
				case "TreasureDistributionRestrictionType":
					TreasureDistributionRestrictionType = (ETreasureDistributionRestrictionType)info.GetValue("TreasureDistributionRestrictionType", typeof(ETreasureDistributionRestrictionType));
					break;
				case "SubChapter":
					SubChapter = info.GetInt32("SubChapter");
					break;
				case "GiveToCharacterID":
					GiveToCharacterID = info.GetString("GiveToCharacterID");
					break;
				case "GiveToCharacterType":
					GiveToCharacterType = (EGiveToCharacterType)info.GetValue("GiveToCharacterType", typeof(EGiveToCharacterType));
					break;
				case "GiveToCharacterRequirement":
					GiveToCharacterRequirement = (EGiveToCharacterRequirement)info.GetValue("GiveToCharacterRequirement", typeof(EGiveToCharacterRequirement));
					break;
				case "GiveToCharacterRequirementCheckID":
					GiveToCharacterRequirementCheckID = info.GetString("GiveToCharacterRequirementCheckID");
					break;
				case "ConsumeSlot":
					ConsumeSlot = info.GetString("ConsumeSlot");
					break;
				case "Modifiers":
					Modifiers = (Dictionary<string, int>)info.GetValue("Modifiers", typeof(Dictionary<string, int>));
					break;
				case "Character":
					CharacterID = ((ECharacter)info.GetValue("Character", typeof(ECharacter))/*cast due to constrained. prefix*/).ToString() + "ID";
					break;
				case "GiveToCharacter":
					GiveToCharacterID = ((ECharacter)info.GetValue("GiveToCharacter", typeof(ECharacter))/*cast due to constrained. prefix*/).ToString() + "ID";
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize Reward entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public Reward(ETreasureType type, int amount, ETreasureDistributionType treasureDistributionType, string giveToCharacterID, ETreasureDistributionRestrictionType treasureDistributionRestrictionType = ETreasureDistributionRestrictionType.None)
	{
		if (type == ETreasureType.XP || type == ETreasureType.Gold || type == ETreasureType.Damage || type == ETreasureType.Prosperity || type == ETreasureType.Reputation || type == ETreasureType.PerkCheck || type == ETreasureType.EnhancementSlots || type == ETreasureType.Discard || type == ETreasureType.PerkPoint)
		{
			Type = type;
			Amount = amount;
			TreasureDistributionType = treasureDistributionType;
			GiveToCharacterID = giveToCharacterID;
			GiveToCharacterType = EGiveToCharacterType.Give;
			TreasureDistributionRestrictionType = treasureDistributionRestrictionType;
			return;
		}
		throw new Exception("Invalid reward type " + type.ToString() + " supplied with integer parameter.");
	}

	public Reward(LootCard.EType resourceType, int amount, ETreasureDistributionType treasureDistributionType, string giveToCharacterID, ETreasureDistributionRestrictionType treasureDistributionRestrictionType = ETreasureDistributionRestrictionType.None)
	{
		Type = ETreasureType.Resource;
		ResourceType = resourceType;
		Amount = amount;
		TreasureDistributionType = treasureDistributionType;
		GiveToCharacterID = giveToCharacterID;
		GiveToCharacterType = EGiveToCharacterType.Give;
		TreasureDistributionRestrictionType = treasureDistributionRestrictionType;
	}

	public Reward(ETreasureType type, int amount)
	{
		Type = type;
		Amount = amount;
		GiveToCharacterType = EGiveToCharacterType.Give;
	}

	public Reward(ETreasureType type, bool unlock)
	{
		if (type == ETreasureType.UnlockEnhancer || type == ETreasureType.UnlockMerchant || type == ETreasureType.UnlockTemple || type == ETreasureType.UnlockTrainer || type == ETreasureType.UnlockPartyUI || type == ETreasureType.UnlockMultiplayer)
		{
			Type = type;
			Unlock = unlock;
			return;
		}
		throw new Exception("Invalid reward type " + type.ToString() + " supplied with string name of unlock target parameter.");
	}

	public Reward(string characterID)
	{
		Type = ETreasureType.UnlockCharacter;
		CharacterID = characterID;
	}

	public Reward(ETreasureType type, string unlockName, ETreasureDistributionType treasureDistributionType = ETreasureDistributionType.None)
	{
		switch (type)
		{
		case ETreasureType.UnlockLocation:
		case ETreasureType.UnlockQuest:
		case ETreasureType.CityEvent:
		case ETreasureType.RoadEvent:
			Type = type;
			UnlockName = unlockName;
			break;
		case ETreasureType.ConsumeItem:
			Type = type;
			ConsumeSlot = unlockName;
			TreasureDistributionType = treasureDistributionType;
			break;
		default:
			throw new Exception("Invalid reward type " + type.ToString() + " supplied with string name of unlock target parameter.");
		}
	}

	public Reward(int itemID, int amount, ETreasureType treasureType, string giveToCharacterID, EGiveToCharacterType giveToCharacterType, EGiveToCharacterRequirement giveToCharacterRequirement = EGiveToCharacterRequirement.None, string giveToCharacterRequirementCheckID = "")
	{
		if (treasureType == ETreasureType.Item || treasureType == ETreasureType.SoldItem || treasureType == ETreasureType.ItemStock || treasureType == ETreasureType.ItemBlueprint || treasureType == ETreasureType.UnlockProsperityItem || treasureType == ETreasureType.UnlockProsperityItemStock || treasureType == ETreasureType.LoseItem)
		{
			Type = treasureType;
			ItemID = itemID;
			Amount = amount;
			GiveToCharacterID = giveToCharacterID;
			GiveToCharacterType = giveToCharacterType;
			GiveToCharacterRequirement = giveToCharacterRequirement;
			GiveToCharacterRequirementCheckID = giveToCharacterRequirementCheckID;
			Item = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.ID == ItemID)?.GetItem;
			if (string.IsNullOrEmpty(GiveToCharacterID) && Item != null && Item.YMLData.ValidEquipCharacterClassIDs.Count > 0)
			{
				GiveToCharacterID = Item.YMLData.ValidEquipCharacterClassIDs[0];
			}
		}
	}

	public Reward(List<RewardCondition> conditions, bool forEnemy, ETreasureDistributionType treasureDistributionType = ETreasureDistributionType.None)
	{
		if (forEnemy)
		{
			Type = ETreasureType.EnemyCondition;
			EnemyConditions = conditions;
		}
		else
		{
			Type = ETreasureType.Condition;
			Conditions = conditions;
			TreasureDistributionType = treasureDistributionType;
		}
	}

	public Reward(List<ElementInfusionBoardManager.EElement> infusions)
	{
		Type = ETreasureType.Infuse;
		Infusions = infusions;
	}

	public Reward(EEnhancement enhancement)
	{
		Type = ETreasureType.Enhancement;
		Enhancement = enhancement;
	}

	public Reward(ETreasureType type, Dictionary<string, int> modifiers, ETreasureDistributionType treasureDistributionType = ETreasureDistributionType.None)
	{
		Type = type;
		Modifiers = modifiers;
		TreasureDistributionType = treasureDistributionType;
	}

	public Reward(PerksYMLData perk)
	{
		Type = ETreasureType.Perk;
		PerkName = perk.Name;
	}

	public Reward(ETreasureType type, int chapter, int subChapter)
	{
		if (type == ETreasureType.UnlockChapter)
		{
			Type = type;
			Amount = chapter;
			SubChapter = subChapter;
			return;
		}
		throw new Exception("Invalid reward type " + type.ToString() + " supplied with 2 integer parameter.");
	}

	public Reward()
	{
	}

	public Reward Copy()
	{
		return new Reward
		{
			Type = Type,
			Amount = Amount,
			ItemID = ItemID,
			ResourceType = ResourceType,
			CardIDs = CardIDs?.ToList(),
			Conditions = Conditions?.ToList(),
			EnemyConditions = EnemyConditions?.ToList(),
			Durability = Durability,
			Infusions = Infusions?.ToList(),
			Enhancement = Enhancement,
			PerkName = PerkName,
			CharacterID = CharacterID,
			UnlockName = UnlockName,
			Unlock = Unlock,
			LevelUp = LevelUp,
			TreasureDistributionType = TreasureDistributionType,
			SubChapter = SubChapter,
			GiveToCharacterID = GiveToCharacterID,
			GiveToCharacterType = GiveToCharacterType,
			GiveToCharacterRequirement = GiveToCharacterRequirement,
			GiveToCharacterRequirementCheckID = GiveToCharacterRequirementCheckID,
			ConsumeSlot = ConsumeSlot,
			Modifiers = Modifiers?.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value),
			TreasureDistributionRestrictionType = TreasureDistributionRestrictionType,
			Item = Item
		};
	}

	public static string GenerateRewardsString(List<Reward> rewards, List<string> tableNames)
	{
		string text = "Treasure Table Rewards [ ";
		foreach (string tableName in tableNames)
		{
			text = text + tableName + " ";
		}
		text += "]:\n";
		foreach (Reward reward in rewards)
		{
			switch (reward.Type)
			{
			case ETreasureType.Gold:
				text = text + "   Gold: " + reward.Amount + "\n";
				break;
			case ETreasureType.XP:
				text = text + "   Party XP: " + reward.Amount + "\n";
				break;
			case ETreasureType.Resource:
				text = text + $"   {reward.ResourceType}: " + reward.Amount + "\n";
				break;
			case ETreasureType.Damage:
				text = text + "   Damage: " + reward.Amount + "\n";
				break;
			case ETreasureType.Item:
			case ETreasureType.UnlockProsperityItem:
				text = text + "   Item: " + reward.Item.Name + "\n";
				break;
			case ETreasureType.ItemStock:
			case ETreasureType.UnlockProsperityItemStock:
				text = text + "   Item Stock: " + reward.Item.Name + " Amount: " + reward.Amount + "\n";
				break;
			case ETreasureType.ItemBlueprint:
				text = text + "   Item Blueprint: " + reward.Item.Name + " Amount: " + reward.Amount + "\n";
				break;
			case ETreasureType.Condition:
				foreach (RewardCondition condition in reward.Conditions)
				{
					if (condition.PositiveCondition != CCondition.EPositiveCondition.NA)
					{
						text = text + "   Condition: " + condition.PositiveCondition.ToString() + "\n";
					}
					if (condition.NegativeCondition != CCondition.ENegativeCondition.NA)
					{
						text = text + "   Condition: " + condition.NegativeCondition.ToString() + "\n";
					}
				}
				break;
			case ETreasureType.Infuse:
				foreach (ElementInfusionBoardManager.EElement infusion in reward.Infusions)
				{
					text = text + "   Infuse: " + infusion.ToString() + "\n";
				}
				break;
			}
		}
		return text;
	}

	public bool Equals(Reward compareReward)
	{
		if (compareReward.Type != Type)
		{
			return false;
		}
		if (compareReward.Amount != Amount)
		{
			return false;
		}
		if (compareReward.ItemID != ItemID)
		{
			return false;
		}
		if (compareReward.ResourceType != ResourceType)
		{
			return false;
		}
		if (compareReward.CardIDs != CardIDs)
		{
			return false;
		}
		if (compareReward.Conditions != Conditions)
		{
			return false;
		}
		if (compareReward.EnemyConditions != EnemyConditions)
		{
			return false;
		}
		if (compareReward.Infusions != Infusions)
		{
			return false;
		}
		if (compareReward.Enhancement != Enhancement)
		{
			return false;
		}
		if (compareReward.PerkName != PerkName)
		{
			return false;
		}
		if (compareReward.CharacterID != CharacterID)
		{
			return false;
		}
		if (compareReward.UnlockName != UnlockName)
		{
			return false;
		}
		if (compareReward.Unlock != Unlock)
		{
			return false;
		}
		if (compareReward.TreasureDistributionType != TreasureDistributionType)
		{
			return false;
		}
		if (compareReward.SubChapter != SubChapter)
		{
			return false;
		}
		if (compareReward.GiveToCharacterID != GiveToCharacterID)
		{
			return false;
		}
		if (compareReward.ConsumeSlot != ConsumeSlot)
		{
			return false;
		}
		return true;
	}
}

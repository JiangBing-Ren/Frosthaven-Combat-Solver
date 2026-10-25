using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class TreasureTableGroup
{
	public List<int> GroupChance;

	public List<TreasureTableEntry> Entries;

	public List<TreasureTableGroup> Groups;

	public int RepeatCount;

	public bool AndLogic;

	public string GiveToCharacterID;

	public EGiveToCharacterType GiveToCharacterType;

	public EGiveToCharacterRequirement GiveToCharacterRequirementType;

	public string GiveToCharacterRequirementCheckID;

	public string GroupName;

	public bool UpdateGroup;

	public TreasureTableGroup(List<TreasureTableEntry> entries, List<TreasureTableGroup> groups, List<int> groupChance, int repeatCount, bool andLogic, string giveToCharacterID, EGiveToCharacterType giveToCharacterType, EGiveToCharacterRequirement giveToCharacterRequirementType, string giveToCharacterRequirementCheckID, string groupName, bool updateGroup)
	{
		if (entries != null && groups != null && (entries.Count > 0 || groups.Count > 0))
		{
			GroupChance = groupChance ?? new List<int> { 100, 100, 100, 100, 100, 100, 100, 100 };
			Entries = entries;
			Groups = groups;
			RepeatCount = repeatCount;
			AndLogic = andLogic;
			GiveToCharacterID = giveToCharacterID;
			GiveToCharacterType = giveToCharacterType;
			GiveToCharacterRequirementType = giveToCharacterRequirementType;
			GiveToCharacterRequirementCheckID = giveToCharacterRequirementCheckID;
			GroupName = groupName;
			UpdateGroup = updateGroup;
			return;
		}
		throw new Exception("Invalid entries supplied to Treasure Table Group");
	}

	public TreasureTableGroup Copy()
	{
		List<TreasureTableEntry> entries = Entries.Select((TreasureTableEntry s) => s.Copy()).ToList();
		List<TreasureTableGroup> groups = Groups.Select((TreasureTableGroup s) => s.Copy()).ToList();
		return new TreasureTableGroup(entries, groups, GroupChance, RepeatCount, AndLogic, GiveToCharacterID, GiveToCharacterType, GiveToCharacterRequirementType, GiveToCharacterRequirementCheckID, GroupName, UpdateGroup);
	}

	public List<CItem> GetTreasureTableGroupItems()
	{
		List<CItem> list = new List<CItem>();
		foreach (TreasureTableEntry entry in Entries)
		{
			if (entry.Type == ETreasureType.Item || entry.Type == ETreasureType.ItemStock || entry.Type == ETreasureType.ItemBlueprint || entry.Type == ETreasureType.UnlockProsperityItem || entry.Type == ETreasureType.UnlockProsperityItemStock)
			{
				list.Add(ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == entry.ItemID).GetItem);
			}
		}
		foreach (TreasureTableGroup group in Groups)
		{
			list.AddRange(group.GetTreasureTableGroupItems());
		}
		return list;
	}
}

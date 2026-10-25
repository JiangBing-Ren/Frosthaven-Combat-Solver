using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

[Serializable]
[DebuggerDisplay("Name:{Name}")]
public class TreasureTable
{
	public string Name;

	public string Description;

	public string GiveToCharacterID;

	public List<TreasureTableEntry> Entries;

	public List<TreasureTableGroup> Groups;

	public string FileName { get; private set; }

	public TreasureTable(string name, string giveToCharacterID, List<TreasureTableEntry> entries, List<TreasureTableGroup> groups, string fileName)
	{
		Name = name;
		GiveToCharacterID = giveToCharacterID;
		Entries = entries;
		Groups = groups;
		FileName = fileName;
	}

	public bool Validate()
	{
		bool result = true;
		if (Entries.Count == 0 && Groups.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Entries or Groups exist within Treasure Table " + Name + ".  File: " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(string giveToCharacterID, List<TreasureTableEntry> entries, List<TreasureTableGroup> groups)
	{
		if (giveToCharacterID != null)
		{
			GiveToCharacterID = giveToCharacterID;
		}
		Entries.AddRange(entries);
		for (int i = 0; i < groups.Count; i++)
		{
			if (groups[i].UpdateGroup)
			{
				if (i < Groups.Count)
				{
					GroupUpdateRecursive(groups[i], Groups[i]);
				}
				else
				{
					Groups.Add(groups[i]);
				}
			}
			else
			{
				Groups.Add(groups[i]);
			}
		}
	}

	public void GroupUpdateRecursive(TreasureTableGroup group, TreasureTableGroup groupToUpdate)
	{
		groupToUpdate.Entries.AddRange(group.Entries);
		for (int i = 0; i < group.Groups.Count; i++)
		{
			if (i < groupToUpdate.Groups.Count)
			{
				GroupUpdateRecursive(group.Groups[i], groupToUpdate.Groups[i]);
			}
			else
			{
				groupToUpdate.Groups.Add(group.Groups[i]);
			}
		}
	}

	public TreasureTable Copy()
	{
		List<TreasureTableEntry> entries = Entries.Select((TreasureTableEntry s) => s.Copy()).ToList();
		List<TreasureTableGroup> groups = Groups.Select((TreasureTableGroup s) => s.Copy()).ToList();
		return new TreasureTable(Name, GiveToCharacterID, entries, groups, FileName);
	}

	public List<CItem> GetTreasureTableItems()
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

using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class PerksYMLData
{
	public string ID { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string CharacterID { get; set; }

	public int Available { get; set; }

	public int Cost { get; set; }

	public List<string> CardsToAddStrings { get; set; }

	public List<string> CardsToRemoveStrings { get; set; }

	public EPerkFlag Flags { get; set; }

	public List<CAbility> Abilities { get; set; }

	public List<CAbilityOverride> Overrides { get; set; }

	public CAbilityCompare CompareAbility { get; set; }

	public string FileName { get; private set; }

	public List<AttackModifierYMLData> CardsToAdd => CardsToAddStrings?.Select((string it) => ScenarioRuleClient.SRLYML.AttackModifiers.First((AttackModifierYMLData w) => it == w.Name)).ToList() ?? new List<AttackModifierYMLData>();

	public List<AttackModifierYMLData> CardsToRemove => CardsToRemoveStrings?.Select((string it) => ScenarioRuleClient.SRLYML.AttackModifiers.First((AttackModifierYMLData w) => it == w.Name)).ToList() ?? new List<AttackModifierYMLData>();

	public PerksYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		Name = null;
		Description = null;
		CharacterID = null;
		Available = int.MaxValue;
		Cost = 1;
		CardsToAddStrings = null;
		CardsToRemoveStrings = null;
		Flags = EPerkFlag.None;
	}

	public override string ToString()
	{
		return "PerksYMLData(" + Name + ")";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PerksYMLData perksYMLData))
		{
			return false;
		}
		return ID == perksYMLData.ID;
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode();
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified for perk in file " + FileName);
			return false;
		}
		if (Name == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Name specified for perk in file " + FileName);
			return false;
		}
		if (Description == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Description specified for perk in file " + FileName);
			return false;
		}
		if (CharacterID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CharacterID specified for perk in file " + FileName);
			return false;
		}
		if (Available == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Available amount specified for perk in file " + FileName);
			return false;
		}
		if ((CardsToAddStrings == null || CardsToAddStrings.Count == 0) && (CardsToRemoveStrings == null || CardsToRemoveStrings.Count == 0) && (Flags & EPerkFlag.Passive) == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid perk.  It does nothing. File " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(PerksYMLData perkData)
	{
		if (perkData.Name != null)
		{
			Name = perkData.Name;
		}
		if (perkData.Description != null)
		{
			Description = perkData.Description;
		}
		if (perkData.CharacterID != null)
		{
			CharacterID = perkData.CharacterID;
		}
		if (perkData.Available != int.MaxValue)
		{
			Available = perkData.Available;
		}
		if (perkData.Cost != 1)
		{
			Cost = perkData.Cost;
		}
		if (perkData.CardsToAddStrings != null)
		{
			CardsToAddStrings = perkData.CardsToAddStrings;
		}
		if (perkData.CardsToRemoveStrings != null)
		{
			CardsToRemoveStrings = perkData.CardsToRemoveStrings;
		}
		if (perkData.Flags != EPerkFlag.None)
		{
			Flags = perkData.Flags;
		}
		if (perkData.Abilities != null)
		{
			Abilities = perkData.Abilities;
		}
		if (perkData.Overrides != null)
		{
			Overrides = perkData.Overrides;
		}
		if (perkData.CompareAbility != null)
		{
			CompareAbility = perkData.CompareAbility;
		}
	}
}

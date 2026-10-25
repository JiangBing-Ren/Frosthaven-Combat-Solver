using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class AttackModifierDeckYMLData
{
	public string Name { get; set; }

	public Dictionary<AttackModifierYMLData, int> AttackModifierDeck { get; set; }

	public bool? IsMonsterDeckNullable { get; set; }

	public bool IsMonsterDeck
	{
		get
		{
			if (!IsMonsterDeckNullable.HasValue)
			{
				return false;
			}
			return IsMonsterDeckNullable.Value;
		}
	}

	public bool? IsReducedRandomnessDeckNullable { get; set; }

	public bool IsReducedRandomnessDeck
	{
		get
		{
			if (!IsReducedRandomnessDeckNullable.HasValue)
			{
				return false;
			}
			return IsReducedRandomnessDeckNullable.Value;
		}
	}

	public string FileName { get; private set; }

	public List<AttackModifierYMLData> GetAllAttackModifiers
	{
		get
		{
			List<AttackModifierYMLData> list = new List<AttackModifierYMLData>();
			foreach (AttackModifierYMLData key in AttackModifierDeck.Keys)
			{
				for (int i = 0; i < AttackModifierDeck[key]; i++)
				{
					list.Add(key);
				}
			}
			return list;
		}
	}

	public AttackModifierDeckYMLData(string fileName)
	{
		FileName = fileName;
		Name = null;
		AttackModifierDeck = null;
		IsMonsterDeckNullable = null;
		IsReducedRandomnessDeckNullable = null;
	}

	public bool Validate()
	{
		if (Name == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Name specified for atack modifier deck in file " + FileName);
			return false;
		}
		if (AttackModifierDeck == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No AttackModifierDeck specified for atack modifier deck " + Name + " in file " + FileName);
			return false;
		}
		if (AttackModifierDeck.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No cards in Attack Modifier Deck in file " + FileName);
			return false;
		}
		bool result = true;
		if (AttackModifierDeck.Where((KeyValuePair<AttackModifierYMLData, int> w) => w.Key.IsBless).Count() > 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "More than one bless card exists in this deck.  There can only be one bless card.  File: " + FileName);
			result = false;
		}
		if (AttackModifierDeck.Where((KeyValuePair<AttackModifierYMLData, int> w) => w.Key.IsCurse).Count() > 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "More than one curse card exists in this deck.  There can only be one curse card.  File: " + FileName);
			result = false;
		}
		foreach (KeyValuePair<AttackModifierYMLData, int> item in AttackModifierDeck)
		{
			if (!item.Key.Rolling)
			{
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(FileName, "No non-rolling cards in Attack Modifier Deck in file " + FileName);
		return false;
	}

	public void UpdateData(AttackModifierDeckYMLData deck)
	{
		if (deck.AttackModifierDeck != null)
		{
			AttackModifierDeck = deck.AttackModifierDeck;
		}
		if (deck.IsMonsterDeckNullable.HasValue)
		{
			IsMonsterDeckNullable = deck.IsMonsterDeckNullable;
		}
		if (deck.IsReducedRandomnessDeckNullable.HasValue)
		{
			IsReducedRandomnessDeckNullable = deck.IsReducedRandomnessDeckNullable;
		}
	}
}

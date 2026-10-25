using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;

public class CSRLYML
{
	public static bool IsLoading;

	public static List<CAbility> AllAbilities = new List<CAbility>();

	public List<HeroSummonYMLData> HeroSummons;

	public List<CharacterYMLData> Characters;

	public List<AbilityCardYMLData> AbilityCards;

	public List<ItemCardYMLData> ItemCards;

	public List<ItemCardYMLData> PetCards;

	public List<PerksYMLData> Perks;

	public List<AttackModifierYMLData> AttackModifiers;

	public List<AttackModifierDeckYMLData> AttackModifierDecks;

	public List<MonsterCardYMLData> MonsterCards;

	public List<MonsterYMLData> Monsters;

	public List<TreasureTable> TreasureTables;

	public List<ScenarioDefinition> Scenarios;

	public List<ScenarioAbilitiesYMLData> ScenarioAbilities;

	public List<CharacterResourceData> CharacterResources;

	public Dictionary<string, byte[]> CustomLevels;

	public CSRLYMLModeData Data { get; private set; }

	[Obsolete]
	public EnhancementYML Enhancements => Data.Enhancements;

	public List<IconGlossaryYML.IconGlossaryEntry> IconGlossary => Data.IconGlossary.Entries;

	public MonsterDataYML MonsterData => Data.MonsterData;

	public ScenarioRoomsYML ScenarioRooms => Data.ScenarioRooms;

	public CSRLYML()
	{
		Data = new CSRLYMLModeData();
	}

	public void UploadNewData()
	{
		if (HeroSummons == null && Data.HeroSummons.LoadedYML.Count > 0)
		{
			HeroSummons = Data.HeroSummons.LoadedYML.ToList();
		}
		if (Characters == null && Data.Characters.LoadedYML.Count > 0)
		{
			Characters = Data.Characters.LoadedYML.ToList();
		}
		if (AbilityCards == null && Data.AbilityCards.LoadedYML.Count > 0)
		{
			AbilityCards = Data.AbilityCards.LoadedYML.OrderBy((AbilityCardYMLData c) => c.ProgressionOrder).ToList();
		}
		if (ItemCards == null && Data.ItemCards.LoadedYML.Count > 0)
		{
			ItemCards = Data.ItemCards.LoadedYML.ToList();
		}
		if (PetCards == null && Data.PetCards.LoadedYML.Count > 0)
		{
			PetCards = Data.PetCards.LoadedYML.ToList();
		}
		if (Perks == null && Data.Perks.LoadedYML.Count > 0)
		{
			Perks = Data.Perks.LoadedYML.ToList();
		}
		if (AttackModifiers == null && Data.AttackModifiers.LoadedYML.Count > 0)
		{
			AttackModifiers = Data.AttackModifiers.LoadedYML.ToList();
		}
		if (AttackModifierDecks == null && Data.AttackModifierDecks.LoadedYML.Count > 0)
		{
			AttackModifierDecks = Data.AttackModifierDecks.LoadedYML.ToList();
		}
		if (MonsterCards == null && Data.MonsterCards.LoadedYML.Count > 0)
		{
			MonsterCards = Data.MonsterCards.LoadedYML.ToList();
		}
		if (Monsters == null && Data.Monsters.LoadedYML.Count > 0)
		{
			Monsters = Data.Monsters.LoadedYML.ToList();
		}
		if (TreasureTables == null && Data.TreasureTables.LoadedYML.Count > 0)
		{
			TreasureTables = Data.TreasureTables.LoadedYML.ToList();
		}
		if (Scenarios == null && Data.Scenarios.LoadedYML.Count > 0)
		{
			Scenarios = Data.Scenarios.LoadedYML.ToList();
		}
		if (ScenarioAbilities == null && Data.ScenarioAbilities.LoadedYML.Count > 0)
		{
			ScenarioAbilities = Data.ScenarioAbilities.LoadedYML.ToList();
		}
		if (CharacterResources == null && Data.CharacterResources.LoadedYML.Count > 0)
		{
			CharacterResources = Data.CharacterResources.LoadedYML.ToList();
		}
		if (CustomLevels == null && Data.CustomLevels.Count > 0)
		{
			CustomLevels = Data.CustomLevels.ToDictionary((Tuple<string, byte[]> l) => l.Item1, (Tuple<string, byte[]> l) => l.Item2);
		}
	}

	public void FinishDataLoad()
	{
		HeroSummons = HeroSummons ?? new List<HeroSummonYMLData>();
		Characters = Characters ?? new List<CharacterYMLData>();
		AbilityCards = AbilityCards ?? new List<AbilityCardYMLData>();
		ItemCards = ItemCards ?? new List<ItemCardYMLData>();
		PetCards = PetCards ?? new List<ItemCardYMLData>();
		Perks = Perks ?? new List<PerksYMLData>();
		AttackModifiers = AttackModifiers ?? new List<AttackModifierYMLData>();
		AttackModifierDecks = AttackModifierDecks ?? new List<AttackModifierDeckYMLData>();
		MonsterCards = MonsterCards ?? new List<MonsterCardYMLData>();
		Monsters = Monsters ?? new List<MonsterYMLData>();
		TreasureTables = TreasureTables ?? new List<TreasureTable>();
		Scenarios = Scenarios ?? new List<ScenarioDefinition>();
		ScenarioAbilities = ScenarioAbilities ?? new List<ScenarioAbilitiesYMLData>();
		CharacterResources = CharacterResources ?? new List<CharacterResourceData>();
		CustomLevels = CustomLevels ?? new Dictionary<string, byte[]>();
	}

	public bool Validate()
	{
		bool result = true;
		IEnumerable<IGrouping<string, HeroSummonYMLData>> source = from g in HeroSummons
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("HeroSummonYML", "Duplicated Hero Summon YML: " + string.Join(", ", source.Select((IGrouping<string, HeroSummonYMLData> s) => s.Key)));
			result = false;
		}
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		foreach (HeroSummonYMLData heroSummon in HeroSummons)
		{
			if (!heroSummon.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("HeroSummonYML", "Failed to validate Hero Summon YML " + heroSummon.FileName);
				result = false;
			}
			int hashCode = heroSummon.ID.GetHashCode();
			if (dictionary.ContainsKey(hashCode))
			{
				SharedClient.ValidationRecord.RecordParseFailure("HeroSummonYML", "Failed to validate Hero Summon YML " + heroSummon.FileName + "\nID HashCode collision for IDs: " + heroSummon.ID + " and " + dictionary[hashCode]);
				result = false;
			}
			else
			{
				dictionary.Add(hashCode, heroSummon.ID);
			}
		}
		IEnumerable<IGrouping<string, CharacterYMLData>> source2 = from g in Characters
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source2.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("CharacterYML", "Duplicated Character YML: " + string.Join(", ", source2.Select((IGrouping<string, CharacterYMLData> s) => s.Key)));
			result = false;
		}
		if (Characters.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("CharacterYML", "The number of loaded characters is less than the required minimum (" + 1 + ")");
			result = false;
		}
		foreach (CharacterYMLData character in Characters)
		{
			if (!character.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("CharacterYML", "Failed to validate Character YML " + character.FileName);
				result = false;
			}
		}
		IEnumerable<IGrouping<int, AbilityCardYMLData>> source3 = from g in AbilityCards
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source3.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("AbilityCardYML", "Duplicated Ability Card YML: " + string.Join(", ", source3.Select((IGrouping<int, AbilityCardYMLData> s) => s.Key)));
			result = false;
		}
		if (AbilityCards.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("AbilityCardYML", "The number of loaded Ability Cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (AbilityCardYMLData abilityCard in AbilityCards)
			{
				if (!abilityCard.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("AbilityCardYML", "Failed to validate Ability Card YML " + abilityCard.FileName);
					result = false;
				}
			}
		}
		IEnumerable<IGrouping<int, ItemCardYMLData>> source4 = from g in ItemCards
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source4.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("ItemCardYML", "Duplicated Item Card YML: " + string.Join(", ", source4.Select((IGrouping<int, ItemCardYMLData> s) => s.Key)));
			result = false;
		}
		if (ItemCards.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("ItemCardYML", "The number of loaded Item Cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (ItemCardYMLData itemCard in ItemCards)
			{
				if (!itemCard.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("ItemCardYML", "Failed to validate Item Card YML " + itemCard.FileName);
					result = false;
				}
			}
		}
		IEnumerable<IGrouping<string, PerksYMLData>> source5 = from g in Perks
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source5.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("PerksYML", "Duplicated Perk YML: " + string.Join(", ", source5.Select((IGrouping<string, PerksYMLData> s) => s.Key)));
			result = false;
		}
		if (Perks.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("PerksYML", "The number of loaded Perks is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (PerksYMLData perk in Perks)
			{
				if (!perk.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("PerksYML", "Failed to validate Perk YML " + perk.FileName);
					result = false;
				}
			}
		}
		if (!Enhancements.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("EnhancementsYML", "Failed to validate Enhancements YML " + Enhancements.FileName);
			result = false;
		}
		if (IconGlossary.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryYML", "The number of loaded Icon Glossary Entries is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (IconGlossaryYML.IconGlossaryEntry item in IconGlossary)
			{
				if (!item.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryYML", "Failed to validate Icon Glossary YML " + item.FileName);
					result = false;
				}
			}
		}
		IEnumerable<IGrouping<int, AttackModifierYMLData>> source6 = from g in AttackModifiers
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source6.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "Duplicated Attack Modifier YML: " + string.Join(", ", source6.Select((IGrouping<int, AttackModifierYMLData> s) => s.Key)));
			result = false;
		}
		if (AttackModifiers.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "The number of Attack modifier cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
			foreach (AttackModifierYMLData attackModifier in AttackModifiers)
			{
				if (!attackModifier.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "Failed to validate Attack Modifier YML " + attackModifier.FileName);
					result = false;
				}
				int deterministicHashCode = YMLShared.GetDeterministicHashCode(attackModifier.Name);
				if (dictionary2.ContainsKey(deterministicHashCode))
				{
					SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "Failed to validate Attack Modifier YML " + attackModifier.FileName + "\nID HashCode collision for Names: " + attackModifier.Name + " and " + dictionary2[deterministicHashCode]);
					result = false;
				}
				else
				{
					dictionary2.Add(deterministicHashCode, attackModifier.Name);
				}
			}
		}
		if (AttackModifierDecks.Count() < 2)
		{
			SharedClient.ValidationRecord.RecordParseFailure("AttackModifierDecksYML", "The number of loaded Attack Modifier Decks is less than the required minimum (" + 2 + ")");
			result = false;
		}
		else
		{
			bool flag = false;
			foreach (AttackModifierDeckYMLData attackModifierDeck in AttackModifierDecks)
			{
				if (!attackModifierDeck.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("AttackModifierDecksYML", "Failed to validate Attack Modifier Deck YML " + attackModifierDeck.FileName);
					result = false;
				}
				if (attackModifierDeck.IsMonsterDeck)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				SharedClient.ValidationRecord.RecordParseFailure("AttackModifierDecksYML", "No Monster Attack Modifier deck was loaded.  One Monster Attack modifier deck must be defined.");
				result = false;
			}
		}
		IEnumerable<IGrouping<int, MonsterCardYMLData>> source7 = from g in MonsterCards
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source7.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonsterCardYML", "Duplicated Monster Card YML: " + string.Join(", ", source7.Select((IGrouping<int, MonsterCardYMLData> s) => s.Key)));
			result = false;
		}
		if (MonsterCards.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonsterCardYML", "The number of loaded Monster Cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (MonsterCardYMLData monsterCard in MonsterCards)
			{
				if (!monsterCard.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("MonsterCardYML", "Failed to validate Monster Card YML " + monsterCard.FileName);
					result = false;
				}
			}
		}
		IEnumerable<IGrouping<string, MonsterYMLData>> source8 = from g in Monsters
			group g by g.ID into g
			where g.Count() > 1
			select g;
		if (source8.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonstersYML", "Duplicated Monster YML: " + string.Join(", ", source8.Select((IGrouping<string, MonsterYMLData> s) => s.Key)));
			result = false;
		}
		if (Monsters.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonstersYML", "The number of loaded monsters is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (MonsterYMLData monster in Monsters)
			{
				if (!monster.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("MonstersYML", "Failed to validate Monster YML " + monster.FileName);
					result = false;
				}
			}
		}
		IEnumerable<IGrouping<string, TreasureTable>> source9 = from g in TreasureTables
			group g by g.Name into g
			where g.Count() > 1
			select g;
		if (source9.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("TreasureTablesYML", "Duplicated Treasure Table YML: " + string.Join(", ", source9.Select((IGrouping<string, TreasureTable> s) => s.Key)));
			result = false;
		}
		if (TreasureTables.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("TreasureTablesYML", "The number of loaded Treasure Tables is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (TreasureTable treasureTable in TreasureTables)
			{
				if (!treasureTable.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("TreasureTablesYML", "Failed to validate Treasure Table YML " + treasureTable.FileName);
					result = false;
				}
			}
		}
		if (Scenarios.Count() + CustomLevels.Count() < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("ScenarioYML", "The number of loaded scenarios is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (ScenarioDefinition scenario in Scenarios)
			{
				if (!scenario.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("ScenarioYML", "Failed to validate Scenario YML " + scenario.FileName);
					result = false;
				}
			}
		}
		if (!MonsterData.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonsterDataYML", "Failed to validate MonsterData YML " + MonsterData.FileName);
			result = false;
		}
		if (!ScenarioRooms.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("ScenarioRoomsYML", "Failed to validate ScenarioRooms YML " + ScenarioRooms.FileName);
			result = false;
		}
		IEnumerable<IGrouping<string, ScenarioAbilitiesYMLData>> source10 = from g in ScenarioAbilities
			group g by g.ScenarioAbilityID into g
			where g.Count() > 1
			select g;
		if (source10.Any())
		{
			SharedClient.ValidationRecord.RecordParseFailure("ScenarioAbilitiesYML", "Duplicated Scenario Ability YML: " + string.Join(", ", source10.Select((IGrouping<string, ScenarioAbilitiesYMLData> s) => s.Key)));
			result = false;
		}
		foreach (ScenarioAbilitiesYMLData scenarioAbility in ScenarioAbilities)
		{
			if (!scenarioAbility.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("ScenarioAbilitiesYML", "Failed to validate ScenarioAbilities YML " + scenarioAbility.FileName);
				result = false;
			}
		}
		return result;
	}

	public CCustomLevelData GetCustomLevel(string name)
	{
		CustomLevels.TryGetValue(name, out var value);
		if (value != null)
		{
			try
			{
				CCustomLevelData result = null;
				using (MemoryStream serializationStream = new MemoryStream(value))
				{
					result = new BinaryFormatter().Deserialize(serializationStream) as CCustomLevelData;
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Unable to get Custom Level Data object from path " + name + ".\n" + ex.Message + "\n" + ex.StackTrace);
				throw;
			}
		}
		return null;
	}
}

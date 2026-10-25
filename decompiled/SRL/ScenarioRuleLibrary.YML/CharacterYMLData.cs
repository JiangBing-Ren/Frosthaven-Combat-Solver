using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class CharacterYMLData
{
	public enum ECharacterSkin
	{
		NotSet,
		Default,
		Alternate
	}

	public string ID { get; set; }

	public string LocKey { get; set; }

	public ECharacter Model { get; set; }

	public string OverrideAbilityCardArt { get; set; }

	public int DefaultSkin { get; set; }

	public int[] HealthTable { get; set; }

	public int NumberAbilityCardsInBattle { get; set; }

	public string AttackModifierDeckString { get; set; }

	public string CompanionSummonID { get; set; }

	public string Description { get; set; }

	public string Role { get; set; }

	public string Difficulty { get; set; }

	public string Strengths { get; set; }

	public string Weaknesses { get; set; }

	public string Adventure_Description { get; set; }

	public bool NullCompanionSummonID { get; set; }

	public string ColourHTML { get; set; }

	public float Fatness { get; set; }

	public float VertexAnimIntensity { get; set; }

	public string CustomCharacterConfig { get; set; }

	public List<CharacterResourceData> CharacterResourceDatas { get; set; }

	public string FileName { get; private set; }

	public int TimeTokens { get; set; } = -1;

	public int TrophyTokens { get; set; } = -1;

	public int ResonanceTokens { get; set; } = -1;

	public string Race { get; set; }

	public string[] Traits { get; set; }

	public int Complexity { get; set; } = 1;

	public string[] ElementalAffinities { get; set; }

	public string[] MasteryIDs { get; set; }

	public string RetireCampaignSection { get; set; }

	public HeroClassStats HeroStats { get; set; }

	public List<CAbility> StartScenarioAbilities { get; set; }

	public List<string> OnlyValidInLevels { get; set; }

	public AttackModifierDeckYMLData AttackModifierDeck
	{
		get
		{
			if (!string.IsNullOrEmpty(AttackModifierDeckString))
			{
				return ScenarioRuleClient.SRLYML.AttackModifierDecks.SingleOrDefault((AttackModifierDeckYMLData s) => s.Name == AttackModifierDeckString);
			}
			if (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness))
			{
				return ScenarioRuleClient.SRLYML.AttackModifierDecks.SingleOrDefault((AttackModifierDeckYMLData s) => s.Name == "ReducedRandomness");
			}
			return ScenarioRuleClient.SRLYML.AttackModifierDecks.SingleOrDefault((AttackModifierDeckYMLData s) => s.Name == "Standard");
		}
	}

	public ECharacterSkin GetCharacterSkin
	{
		get
		{
			if (DefaultSkin != 1)
			{
				switch (Model)
				{
				case ECharacter.Brute:
				case ECharacter.Cragheart:
				case ECharacter.Mindthief:
				case ECharacter.Scoundrel:
				case ECharacter.Spellweaver:
				case ECharacter.Tinkerer:
					return ECharacterSkin.Alternate;
				}
			}
			return ECharacterSkin.Default;
		}
	}

	public CharacterYMLData(string filename)
	{
		FileName = filename;
		ID = null;
		LocKey = null;
		OverrideAbilityCardArt = null;
		Model = ECharacter.None;
		DefaultSkin = int.MaxValue;
		HealthTable = null;
		NumberAbilityCardsInBattle = int.MaxValue;
		AttackModifierDeckString = null;
		CompanionSummonID = null;
		Description = null;
		Role = null;
		Difficulty = null;
		Strengths = null;
		Weaknesses = null;
		Adventure_Description = null;
		NullCompanionSummonID = false;
		ColourHTML = "#FFFFFF";
		Fatness = 0f;
		VertexAnimIntensity = 0f;
		CustomCharacterConfig = null;
		CharacterResourceDatas = new List<CharacterResourceData>();
		StartScenarioAbilities = new List<CAbility>();
		OnlyValidInLevels = null;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Character ID specified for character in file " + FileName);
			return false;
		}
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid LocKey specified for character in file " + FileName);
			return false;
		}
		if (Model == ECharacter.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Character Model specified for character in file " + FileName);
			return false;
		}
		if (DefaultSkin == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid DefaultSkin specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (NumberAbilityCardsInBattle == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No NumberAblityCardsInBattle specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (HealthTable == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No HealthTable specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (Description == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Description specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (Role == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Role specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (Difficulty == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Difficulty specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (Strengths == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Strengths specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (Weaknesses == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Weaknesses specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		if (Adventure_Description == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Adventure Description specified for character " + Model.ToString() + " in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(CharacterYMLData newData)
	{
		if (newData.LocKey != null)
		{
			LocKey = newData.LocKey;
		}
		if (newData.OverrideAbilityCardArt != null)
		{
			OverrideAbilityCardArt = newData.OverrideAbilityCardArt;
		}
		if (newData.Model != ECharacter.None)
		{
			Model = newData.Model;
		}
		if (newData.DefaultSkin != int.MaxValue)
		{
			DefaultSkin = newData.DefaultSkin;
		}
		if (newData.HealthTable != null)
		{
			HealthTable = newData.HealthTable;
		}
		if (newData.NumberAbilityCardsInBattle != int.MaxValue)
		{
			NumberAbilityCardsInBattle = newData.NumberAbilityCardsInBattle;
		}
		if (newData.AttackModifierDeckString != null)
		{
			AttackModifierDeckString = newData.AttackModifierDeckString;
		}
		if (newData.CompanionSummonID != null)
		{
			CompanionSummonID = newData.CompanionSummonID;
		}
		if (newData.Description != null)
		{
			Description = newData.Description;
		}
		if (newData.Role != null)
		{
			Role = newData.Role;
		}
		if (newData.Difficulty != null)
		{
			Difficulty = newData.Difficulty;
		}
		if (newData.Strengths != null)
		{
			Strengths = newData.Strengths;
		}
		if (newData.Weaknesses != null)
		{
			Weaknesses = newData.Weaknesses;
		}
		if (newData.Adventure_Description != null)
		{
			Adventure_Description = newData.Adventure_Description;
		}
		if (newData.NullCompanionSummonID)
		{
			CompanionSummonID = null;
		}
		if (newData.ColourHTML != "#FFFFFF")
		{
			ColourHTML = newData.ColourHTML;
		}
		if (newData.Fatness != 0f)
		{
			Fatness = newData.Fatness;
		}
		if (newData.VertexAnimIntensity != 0f)
		{
			VertexAnimIntensity = newData.VertexAnimIntensity;
		}
		if (CustomCharacterConfig != null)
		{
			CustomCharacterConfig = newData.CustomCharacterConfig;
		}
		if (newData.CharacterResourceDatas != null)
		{
			CharacterResourceDatas.AddRange(newData.CharacterResourceDatas);
		}
		if (newData.Race != null)
		{
			Race = newData.Race;
		}
		if (newData.Traits != null)
		{
			Traits = newData.Traits.ToArray();
		}
		if (newData.HeroStats != null)
		{
			HeroStats = newData.HeroStats.Clone();
		}
		if (newData.StartScenarioAbilities != null)
		{
			StartScenarioAbilities = newData.StartScenarioAbilities.ToList();
		}
		if (newData.OnlyValidInLevels != null)
		{
			OnlyValidInLevels = newData.OnlyValidInLevels.ToList();
		}
	}

	public bool VerifyValidForLevel(string levelName)
	{
		if (OnlyValidInLevels == null)
		{
			return true;
		}
		return OnlyValidInLevels.Contains(levelName);
	}
}

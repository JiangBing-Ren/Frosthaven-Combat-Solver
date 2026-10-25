using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.PersonalQuests;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public class RequirementCheckResult : IRequirementCheckResult
{
	public enum StartingLocatinState
	{
		Valid,
		Locked,
		Missing
	}

	public int missingAmountCharacters;

	public List<string> missingRequiredCharacters;

	public List<Tuple<string, int>> charactersWithoutLevel;

	public List<string> missingRequiredItems;

	public List<string> missingRequiredPersonalQuests;

	public StartingLocatinState startingLocationState;

	private CQuestState quest;

	public bool IsQuestStateLocked => quest.QuestState == CQuestState.EQuestState.Locked;

	public RequirementCheckResult(CQuestState quest)
	{
		this.quest = quest;
		missingAmountCharacters = 0;
		charactersWithoutLevel = new List<Tuple<string, int>>();
		missingRequiredCharacters = new List<string>();
		missingRequiredItems = new List<string>();
		missingRequiredPersonalQuests = new List<string>();
		startingLocationState = StartingLocatinState.Valid;
	}

	public bool IsUnlocked()
	{
		if (ValidCharacterParty() && ValidSizeParty() && ValidStartingLocation() && !IsQuestStateLocked && IsValidLinkedQuestChoice())
		{
			return !HasRemainingCharactersToRetire();
		}
		return false;
	}

	public bool ValidCharacterParty()
	{
		if (missingRequiredCharacters.Count == 0 && charactersWithoutLevel.Count == 0 && missingRequiredItems.Count == 0)
		{
			return missingRequiredPersonalQuests.Count == 0;
		}
		return false;
	}

	public bool ValidStartingLocation()
	{
		return startingLocationState == StartingLocatinState.Valid;
	}

	public bool ValidSizeParty()
	{
		return missingAmountCharacters == 0;
	}

	public bool IsOnlyMissingCharacters()
	{
		if ((missingRequiredCharacters.Count > 0 || !ValidSizeParty()) && charactersWithoutLevel.Count == 0 && ValidStartingLocation())
		{
			return !IsQuestStateLocked;
		}
		return false;
	}

	public bool IsValidLinkedQuestChoice()
	{
		return true;
	}

	private bool HasRemainingCharactersToRetire()
	{
		return AdventureState.MapState.MapParty.ExistsCharacterToRetire();
	}

	public string ToString(string formatLine = null)
	{
		return string.Join("\n", ToStringList(formatLine));
	}

	public List<string> ToStringList(string formatLine = null)
	{
		List<string> list = new List<string>();
		if (IsQuestStateLocked)
		{
			list.Add((formatLine == null) ? LocalizationManager.GetTranslation("GUI_QUEST_LOCKED") : string.Format(formatLine, LocalizationManager.GetTranslation("GUI_QUEST_LOCKED")));
		}
		if (startingLocationState != StartingLocatinState.Locked && startingLocationState == StartingLocatinState.Missing)
		{
			string translation = LocalizationManager.GetTranslation("GUI_QUEST_STARTING_LOCATION_MISSING");
			list.Add((formatLine == null) ? translation : string.Format(formatLine, translation));
		}
		if (missingRequiredCharacters != null)
		{
			foreach (string requiredCharacter in missingRequiredCharacters)
			{
				string text = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_REQUIRED_HEROES"), LocalizationManager.GetTranslation(CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == requiredCharacter).LocKey));
				list.Add((formatLine == null) ? text : string.Format(formatLine, text));
			}
		}
		if (missingRequiredPersonalQuests != null)
		{
			foreach (string requiredPersonalQuest in missingRequiredPersonalQuests)
			{
				string text2 = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_REQUIRED_PERSONAL_QUEST"), LocalizationManager.GetTranslation(MapRuleLibraryClient.MRLYML.PersonalQuests.Single((PersonalQuestYMLData s) => s.ID == requiredPersonalQuest).LocalisedName));
				list.Add((formatLine == null) ? text2 : string.Format(formatLine, text2));
			}
		}
		if (missingRequiredItems != null)
		{
			foreach (string requiredItems in missingRequiredItems)
			{
				string text3 = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_REQUIRED_ITEMS"), LocalizationManager.GetTranslation(ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.StringID == requiredItems).Name));
				list.Add((formatLine == null) ? text3 : string.Format(formatLine, text3));
			}
		}
		if (charactersWithoutLevel != null)
		{
			foreach (Tuple<string, int> requiredCharacter2 in charactersWithoutLevel)
			{
				string text4 = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_REQUIRED_LEVEL_HEROE"), LocalizationManager.GetTranslation(CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == requiredCharacter2.Item1).LocKey), requiredCharacter2.Item2);
				list.Add((formatLine == null) ? text4 : string.Format(formatLine, text4));
			}
		}
		if (missingAmountCharacters > 0)
		{
			string text5 = string.Format(LocalizationManager.GetTranslation((missingAmountCharacters == 1) ? "GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROE" : "GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROES"), missingAmountCharacters);
			list.Add((formatLine == null) ? text5 : string.Format(formatLine, text5));
		}
		if (!IsValidLinkedQuestChoice())
		{
			string translation2 = LocalizationManager.GetTranslation("GUI_LINKED_QUEST_INVALID_CHOICE");
			list.Add((formatLine == null) ? translation2 : string.Format(formatLine, translation2));
		}
		if (HasRemainingCharactersToRetire())
		{
			string translation3 = LocalizationManager.GetTranslation("GUI_QUEST_SELECT_BLOCKED");
			list.Add((formatLine == null) ? translation3 : string.Format(formatLine, translation3));
		}
		return list;
	}
}

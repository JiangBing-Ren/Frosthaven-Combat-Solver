using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

public static class CQuestStateExtensions
{
	public static RequirementCheckResult CheckRequirements(this CQuestState quest)
	{
		RequirementCheckResult requirementCheckResult = new RequirementCheckResult(quest);
		if (quest.Quest.Type != EQuestType.Job)
		{
			_ = quest.Quest.StartingVillage;
			requirementCheckResult.startingLocationState = RequirementCheckResult.StartingLocatinState.Missing;
		}
		IEnumerable<CMapCharacter> selectedCharacters = AdventureState.MapState.MapParty.SelectedCharacters;
		if (quest.ScenarioState.CustomLevelData == null || (quest.ScenarioState.CustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnAtEntrance && quest.ScenarioState.CustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations))
		{
			int num = 0;
			if (quest.Quest.QuestCharacterRequirements.Any((QuestYML.CQuestCharacterRequirement x) => x.RequiredCharacterCount.HasValue))
			{
				foreach (QuestYML.CQuestCharacterRequirement questCharacterRequirement in quest.Quest.QuestCharacterRequirements)
				{
					if (questCharacterRequirement.RequiredCharacterCount.HasValue && questCharacterRequirement.RequiredCharacterCount.Value > num)
					{
						num = questCharacterRequirement.RequiredCharacterCount.Value;
					}
				}
			}
			else
			{
				num = 2;
			}
			if (selectedCharacters.Count() < num)
			{
				int count = AdventureState.MapState.MapParty.CheckCharacters.Count;
				if (count < num)
				{
					if (selectedCharacters.Count() < count)
					{
						requirementCheckResult.missingAmountCharacters = count;
					}
					else if (!quest.QuestCompletionRewardGroup.Rewards.Exists((Reward it) => it.Type == ETreasureType.UnlockCharacter))
					{
						requirementCheckResult.missingAmountCharacters = num;
					}
				}
				else
				{
					requirementCheckResult.missingAmountCharacters = num;
				}
			}
		}
		foreach (QuestYML.CQuestCharacterRequirement characterRequirement in quest.Quest.QuestCharacterRequirements)
		{
			if (characterRequirement.RequiredCharacterID != null)
			{
				CMapCharacter cMapCharacter = selectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == characterRequirement.RequiredCharacterID);
				if (cMapCharacter == null)
				{
					requirementCheckResult.missingRequiredCharacters.Add(characterRequirement.RequiredCharacterID);
				}
				else if (characterRequirement.RequiredLevel.HasValue && characterRequirement.RequiredLevel > cMapCharacter.Level)
				{
					requirementCheckResult.charactersWithoutLevel.Add(new Tuple<string, int>(characterRequirement.RequiredCharacterID, characterRequirement.RequiredLevel.Value));
				}
			}
			if (characterRequirement.RequiredItemID != null && !selectedCharacters.Any((CMapCharacter x) => x.CheckEquippedItems.Any((CItem y) => y.YMLData.StringID == characterRequirement.RequiredItemID)))
			{
				requirementCheckResult.missingRequiredItems.Add(characterRequirement.RequiredItemID);
			}
			if (characterRequirement.RequiredPersonalQuestID != null && !selectedCharacters.Any((CMapCharacter x) => x.PersonalQuest.ID == characterRequirement.RequiredPersonalQuestID))
			{
				requirementCheckResult.missingRequiredPersonalQuests.Add(characterRequirement.RequiredPersonalQuestID);
			}
		}
		return requirementCheckResult;
	}

	public static int CountTreasures(this CQuestState quest)
	{
		if (!AdventureState.MapState.IsCampaign)
		{
			return 0;
		}
		if (quest.ScenarioState.NonSerializedInitialState == null)
		{
			quest.ScenarioState.CheckForNonSerializedInitialScenario();
		}
		int num = 0;
		for (int num2 = quest.ScenarioState.NonSerializedInitialState.ChestProps.Count - 1; num2 >= 0; num2--)
		{
			CObjectChest cObjectChest = (CObjectChest)quest.ScenarioState.NonSerializedInitialState.ChestProps[num2];
			if (cObjectChest.ObjectType == ScenarioManager.ObjectImportType.Chest && (cObjectChest.ChestTreasureTablesID.IsNullOrEmpty() || !cObjectChest.ChestTreasureTablesID.Exists(AdventureState.MapState.AlreadyRewardedChestTreasureTableIDs.Contains)))
			{
				num++;
			}
		}
		return num;
	}
}

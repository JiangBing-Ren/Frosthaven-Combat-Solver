using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.YML;
using FHGUI;
using FHGUI.Common;
using FHRL;
using FHRL.Buildings;
using FHRL.Puzzles;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using SharedLibrary.Achievments;

[Serializable]
public class FHSaveData : ISerializable
{
	public const int LatestSaveVersion = 89;

	public int SaveVersion = 89;

	public FHScenarioResult ScenarioResult;

	public CampaignState GameState;

	public CMapState MapState;

	[Obsolete("Slot assignment by CharacterClassID is obsolete! Use SlotAssignments instead!")]
	public List<CharacterClassID> SlotAssignment_Old;

	public List<CharacterID> SlotAssignments = new List<CharacterID>();

	public CampaignStatRepository StatsRepo = new CampaignStatRepository();

	public ScenarioStatRepository ScenarioStatsRepo = new ScenarioStatRepository();

	public const string DefaultCampaignID = "Frosthaven";

	public string CampaignID = "Frosthaven";

	public ScenarioRuleLibrary.ScenarioState FastReloadState;

	public static EGoldMode GoldMode => EGoldMode.CharacterGold;

	public static EEnhancementMode EnhancementMode => EEnhancementMode.CharacterPersistent;

	public void ResolveDefs(DefRepository defRepo)
	{
		GameState?.ResolveDefs(defRepo);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		StateSerializer stateSerializer = new StateSerializer();
		info.AddValue("SaveVersion", SaveVersion);
		info.AddValue("CampaignID", CampaignID);
		info.AddValue("ScenarioResult", stateSerializer.BinarySerialize(ScenarioResult));
		info.AddValue("GameState", stateSerializer.BinarySerialize(GameState));
		info.AddValue("MapState", MapState);
		info.AddValue("SlotAssignments", SlotAssignments?.Select((CharacterID s) => s?.ToString()).ToArray());
		info.AddValue("StatsRepo", stateSerializer.BinarySerialize(StatsRepo));
		info.AddValue("ScenarioStatsRepo", stateSerializer.BinarySerialize(ScenarioStatsRepo));
		info.AddValue("FastReloadState", FastReloadState);
	}

	public FHSaveData()
	{
	}

	public FHSaveData(SerializationInfo info, StreamingContext context)
	{
		SaveVersion = 0;
		StateSerializer stateSerializer = new StateSerializer();
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "SaveVersion":
					SaveVersion = info.GetInt32("SaveVersion");
					break;
				case "CampaignID":
					CampaignID = info.GetString("CampaignID");
					break;
				case "ScenarioResult":
				{
					byte[] data4 = (byte[])info.GetValue("ScenarioResult", typeof(byte[]));
					ScenarioResult = (FHScenarioResult)stateSerializer.BinaryDeserialzie(data4, typeof(FHScenarioResult));
					break;
				}
				case "GameState":
				{
					byte[] data = (byte[])info.GetValue("GameState", typeof(byte[]));
					GameState = (CampaignState)stateSerializer.BinaryDeserialzie(data, typeof(CampaignState));
					break;
				}
				case "MapState":
					MapState = (CMapState)info.GetValue("MapState", typeof(CMapState));
					break;
				case "SlotAssignment":
				{
					string[] array2 = (string[])info.GetValue("SlotAssignment", typeof(string[]));
					if (array2 != null)
					{
						SlotAssignment_Old = array2.Select((string s) => new CharacterClassID(s)).ToList();
					}
					break;
				}
				case "StatsRepo":
				{
					byte[] data3 = (byte[])info.GetValue("StatsRepo", typeof(byte[]));
					StatsRepo = (CampaignStatRepository)stateSerializer.BinaryDeserialzie(data3, typeof(CampaignStatRepository));
					break;
				}
				case "ScenarioStatsRepo":
				{
					byte[] data2 = (byte[])info.GetValue("ScenarioStatsRepo", typeof(byte[]));
					ScenarioStatsRepo = (ScenarioStatRepository)stateSerializer.BinaryDeserialzie(data2, typeof(ScenarioStatRepository));
					break;
				}
				case "FastReloadState":
					FastReloadState = (ScenarioRuleLibrary.ScenarioState)info.GetValue("FastReloadState", typeof(ScenarioRuleLibrary.ScenarioState));
					break;
				case "SlotAssignments":
				{
					string[] array = (string[])info.GetValue("SlotAssignments", typeof(string[]));
					if (array != null)
					{
						SlotAssignments = array.Select((string s) => (s != null) ? new CharacterID(new Guid(s)) : null).ToList();
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				if (!TryHandleDeserialziationFailure(item.Name, ex))
				{
					Log.ErrorWithReport("Exception while trying to deserialize FHSaveData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}
	}

	public void CreateDefaultMapState(EAdventureDifficulty difficulty, SaveOwner owner, EGameMode gameMode)
	{
		MapState = new CMapState(difficulty, GoldMode, EnhancementMode, DLCRegistry.EDLCKey.None, isModded: false, owner.ConvertToMapStateSaveOwner(), PartyAdventureData.ConvertGameModeToDLLMode(gameMode), gameMode != EGameMode.Campaign);
	}

	private bool TryHandleDeserialziationFailure(string entry, Exception ex)
	{
		if (entry == "FastReloadState")
		{
			Log.Error("Exception while trying to deserialize FHSaveData entry " + entry + "\n" + ex.Message);
			MapState = null;
			return true;
		}
		if (entry == "MapState")
		{
			Log.Error("Exception while trying to deserialize FHSaveData entry " + entry + "\n" + ex.Message);
			FastReloadState = null;
			return true;
		}
		return false;
	}

	public void ApplyMigration(DefRepository repo)
	{
		if (SaveVersion < 84)
		{
			GameState.UnlockedScenarios.RemoveAll((FHRL.ScenarioState x) => x.ScenarioID.ID == 128);
		}
		if (SaveVersion < 2)
		{
			GameState.Outpost.NextOutpostAttackMod.LoseMoraleInsteadOfDamagingBuildingsInt = (GameState.Outpost.NextOutpostAttackMod.LoseMoraleInsteadOfDamagingBuildings ? 1 : 0);
			foreach (OutpostEventState activatedEvent in GameState.Outpost.ActivatedEvents)
			{
				if (activatedEvent.Attack != null)
				{
					activatedEvent.Attack.LoseMoraleInsteadOfDamagingBuildingsInt = (activatedEvent.Attack.LoseMoraleInsteadOfDamagingBuildings ? 1 : 0);
				}
			}
		}
		if (SaveVersion < 3)
		{
			GameState.Outpost.NextOutpostAttackMod.StopAttackOnSuccessfulDefenseInt = (GameState.Outpost.NextOutpostAttackMod.StopAttackOnSuccessfulDefense ? 1 : 0);
			foreach (OutpostEventState activatedEvent2 in GameState.Outpost.ActivatedEvents)
			{
				if (activatedEvent2.Attack != null)
				{
					activatedEvent2.Attack.StopAttackOnSuccessfulDefenseInt = (activatedEvent2.Attack.StopAttackOnSuccessfulDefense ? 1 : 0);
				}
			}
		}
		if (SaveVersion < 4 && GameState.ActiveScenario?.NextRandomScenario != null)
		{
			FHRL.ScenarioState scenarioState = GameState.UnlockedScenarios.SingleOrDefault((FHRL.ScenarioState x) => x.Def == GameState.ActiveScenario.NextRandomScenario);
			if (scenarioState != null)
			{
				GameState.UnlockedScenarios.Remove(scenarioState);
			}
		}
		if (SaveVersion < 6)
		{
			if (FastReloadState != null)
			{
				ApplyRandomItemFixToScenarioState(FastReloadState, repo);
			}
			ApplyRandomItemFixToScenarioState(MapState?.CurrentMapScenarioState?.CurrentState, repo);
		}
		if (SaveVersion < 7)
		{
			if (FastReloadState != null)
			{
				ApplyAlreadyLootedChestFixForScenario16(FastReloadState);
			}
			ApplyAlreadyLootedChestFixForScenario16(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 8)
		{
			foreach (PersonalQuestState personalQuest in GameState.CompletedPersonalQuests)
			{
				GameState.UnlockedScenarios.RemoveAll((FHRL.ScenarioState x) => !x.Completed && x.Def.RequiresPersonalQuest == personalQuest.ID);
			}
		}
		if (SaveVersion < 9)
		{
			if (GameState.GameMode.Name == "Frosthaven")
			{
				GameState.GameMode.OutpostMarkerLocation = "Frosthaven";
			}
			else if (GameState.GameMode.Name == "RoadToFrosthaven")
			{
				GameState.GameMode.OutpostMarkerLocation = "CampOutpost";
			}
			if (GameState.ActiveScenario != null)
			{
				if (GameState.Outpost.ActiveEvent != null)
				{
					GameState.PartyTravelState = CampaignState.PartyTravelStateType.WaitingEncounter;
				}
				else
				{
					GameState.PartyTravelState = CampaignState.PartyTravelStateType.WaitingInScenario;
				}
			}
			else
			{
				GameState.PartyTravelState = CampaignState.PartyTravelStateType.WaitingInOutpost;
			}
		}
		if (SaveVersion < 10)
		{
			foreach (FHRL.ScenarioState unlockedScenario in GameState.UnlockedScenarios)
			{
				if (unlockedScenario.ConclusionParameters == null)
				{
					unlockedScenario.ConclusionParameters = new List<object>();
				}
			}
		}
		if (SaveVersion < 11)
		{
			PersonalQuestState personalQuestState = GameState.CompletedPersonalQuests.SingleOrDefault((PersonalQuestState x) => x.ID.ID.Equals("PQ_23", StringComparison.InvariantCultureIgnoreCase));
			if (personalQuestState != null)
			{
				List<CampaignEffect> list = new List<CampaignEffect>();
				uint rewardID = personalQuestState.Def.GetAvailableRewardIDs().FirstOrDefault();
				personalQuestState.Def.GetRewards(rewardID, list);
				new CampaignController(GameState, repo).ApplyEffects(list);
			}
			foreach (ChallengeDef value2 in repo.Challenges.Values)
			{
				if (!GameState.CompletedChallenges.Contains(value2) && !GameState.ChallengeDeck.Contains(value2))
				{
					GameState.ChallengeDeckIDs.Add(value2.ID);
				}
			}
		}
		if (SaveVersion < 12)
		{
			if (FastReloadState != null)
			{
				ApplyScenario107ReactivatedObjectiveFix(FastReloadState);
			}
			ApplyScenario107ReactivatedObjectiveFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 13 && GameState.ActiveScenario != null && GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInOutpost)
		{
			string text = GameState.ActiveScenario.Def.ID.ToString();
			if (text != GameState.PartyMarkerLocation)
			{
				GameState.PrevPartyMarkerLocation = GameState.PartyMarkerLocation;
				GameState.PartyMarkerLocation = text;
			}
			GameState.PartyTravelState = CampaignState.PartyTravelStateType.TravelingToScenario;
		}
		if (SaveVersion < 14)
		{
			if (FastReloadState != null)
			{
				ApplyBannerSpearAttackModifierNameFix(FastReloadState);
			}
			ApplyBannerSpearAttackModifierNameFix(MapState?.CurrentMapScenarioState?.CurrentState);
			foreach (CharacterState allCharacter in GameState.AllCharacters)
			{
				for (int num = allCharacter.AttackModifierIDs.Count - 1; num >= 0; num--)
				{
					if (allCharacter.AttackModifierIDs[num].ID.Equals("AMC_Shield1_R_Banner_Spear", StringComparison.InvariantCultureIgnoreCase))
					{
						allCharacter.AttackModifierIDs[num] = new CharacterAttackModifierID("AMC_Shield1_R_BannerSpear");
					}
				}
			}
		}
		if (SaveVersion < 16)
		{
			foreach (CharacterState allCharacter2 in GameState.AllCharacters)
			{
				foreach (ItemID item5 in allCharacter2.EquipedItemIDs.Where((ItemID x) => x.ID == 113 || x.ID == 169).ToList())
				{
					allCharacter2.EquipedItemIDs.Remove(item5);
					allCharacter2.TuckedItemIDs.Add(item5);
				}
			}
			if (FastReloadState != null)
			{
				ApplyRemoveInvalidUpdate1Items(FastReloadState);
			}
			ApplyRemoveInvalidUpdate1Items(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 17)
		{
			if (FastReloadState != null)
			{
				ApplyRandomItemFixToScenarioState2(FastReloadState, repo);
			}
			ApplyRandomItemFixToScenarioState2(MapState?.CurrentMapScenarioState?.CurrentState, repo);
		}
		if (SaveVersion < 18)
		{
			if (FastReloadState != null)
			{
				ApplyPetFrozenfistDuplicateIDsFix(FastReloadState);
			}
			ApplyPetFrozenfistDuplicateIDsFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 19 && (MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_015_Ancient_Spire" || MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_082_Expedition_North"))
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 20)
		{
			GameState.Random.ShouldLog = true;
		}
		if (SaveVersion < 21)
		{
			CampaignSectionID item = new CampaignSectionID("175.1");
			if (GameState.Outpost.AddedTownGuardPerks.Count >= 5 && !GameState.Outpost.JobPostings.Contains(item) && GameState.Outpost.TownGuardPerksPoints == 0)
			{
				GameState.Outpost.JobPostings.Add(item);
			}
		}
		if (SaveVersion < 22 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_082_Expedition_North")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 22 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_095_To_Bury_the_Dead")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 23)
		{
			if (MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_114_Work_Freeze")
			{
				if (FastReloadState != null)
				{
					ApplyDetonationChargeRepositionBugfix(FastReloadState);
				}
				ApplyDetonationChargeRepositionBugfix(MapState?.CurrentMapScenarioState?.CurrentState);
			}
			foreach (BuildingState building2 in GameState.Outpost.Buildings)
			{
				if (building2.donationDistribution != null)
				{
					CampaignController campaignController = new CampaignController(GameState, repo);
					uint num2 = 0u;
					if (building2.ConstructionState == BuildingConstructionType.NewBuilding)
					{
						num2 = campaignController.BuildingsController.GetConstructMoraleCost(building2.Def);
					}
					else if (building2.ConstructionState == BuildingConstructionType.Upgrading)
					{
						num2 = campaignController.BuildingsController.GetUpgradeMoraleCost(building2, (int)(building2.Level - 1));
					}
					uint num3 = building2.donationDistribution.Morale - num2;
					if (num3 != 0)
					{
						building2.donationDistribution.Morale -= num3;
						GameState.Outpost.Morale += (int)num3;
					}
					if (building2.donationDistribution.ConstructionPerTurn != 0)
					{
						GameState.Outpost.ConstructTurnRemains += building2.donationDistribution.ConstructionPerTurn;
						building2.donationDistribution.ConstructionPerTurn = 0u;
					}
					if (building2.donationDistribution.ExtraConstructionPerTurn != 0)
					{
						GameState.Outpost.ExtraConstructTurnRemains += building2.donationDistribution.ExtraConstructionPerTurn;
						building2.donationDistribution.ExtraConstructionPerTurn = 0u;
					}
				}
			}
		}
		if (SaveVersion < 25 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_094_A_Grand_View")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 27 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_094_A_Grand_View" && GameState.PartyCharacters.Any((CharacterState x) => x.Perks.Any((PerkDef perk) => perk.Flags.HasFlag(EPerkFlag.IgnoreNegativeScenarioEffects))))
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 28 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_082_Expedition_North")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 28)
		{
			CampaignController campaignController2 = new CampaignController(GameState, repo);
			campaignController2.Cheat_BeginResult();
			List<PersonalQuestID> collection = GameState.PersonalQuestDeckIDs.Except(GameState.CompletedPersonalQuests.Select((PersonalQuestState x) => x.ID)).Distinct().ToList();
			GameState.PersonalQuestDeckIDs.Clear();
			GameState.PersonalQuestDeckIDs.AddRange(collection);
			foreach (CharacterState character in GameState.AllCharacters)
			{
				if (character.PersonalQuestDef == null)
				{
					continue;
				}
				if (!campaignController2.IsPersonalQuestValid(character.PersonalQuest.ID))
				{
					PersonalQuestID invalidQuest = character.PersonalQuest.ID;
					List<PersonalQuestID> list2 = campaignController2.ValidPersonalQuestIDs().ToList();
					if (list2.Count == 0)
					{
						character.PersonalQuest = null;
					}
					else
					{
						PersonalQuestID personalQuestID = list2[0];
						GameState.PersonalQuestDeckIDs.Remove(personalQuestID);
						character.PersonalQuest = new PersonalQuestState(personalQuestID, repo, campaignController2);
						character.PersonalQuest.Def.StartQuest(character.PersonalQuest, campaignController2);
					}
					if (!GameState.CompletedPersonalQuests.Any((PersonalQuestState x) => x.ID == invalidQuest))
					{
						GameState.PersonalQuestDeckIDs.Add(invalidQuest);
					}
				}
				else if (GameState.CompletedPersonalQuests.Any((PersonalQuestState x) => x.ID == character.PersonalQuestDef.ID))
				{
					List<PersonalQuestID> list3 = campaignController2.ValidPersonalQuestIDs().ToList();
					if (list3.Count == 0)
					{
						character.PersonalQuest = null;
						continue;
					}
					PersonalQuestID personalQuestID2 = list3[0];
					GameState.PersonalQuestDeckIDs.Remove(personalQuestID2);
					character.PersonalQuest = new PersonalQuestState(personalQuestID2, repo, campaignController2);
					character.PersonalQuest.Def.StartQuest(character.PersonalQuest, campaignController2);
				}
				else if (GameState.PersonalQuestDeckIDs.Any((PersonalQuestID x) => x == character.PersonalQuestDef.ID))
				{
					GameState.PersonalQuestDeckIDs.RemoveAll((PersonalQuestID x) => x == character.PersonalQuestDef.ID);
				}
			}
			foreach (CharacterState allCharacter3 in GameState.AllCharacters)
			{
				if (allCharacter3.PersonalQuestDef == null)
				{
					continue;
				}
				foreach (CharacterState allCharacter4 in GameState.AllCharacters)
				{
					if (allCharacter4 != allCharacter3 && allCharacter4.PersonalQuestDef != null && allCharacter4.PersonalQuest.ID == allCharacter3.PersonalQuest.ID)
					{
						List<PersonalQuestID> list4 = campaignController2.ValidPersonalQuestIDs().ToList();
						if (list4.Count == 0)
						{
							allCharacter4.PersonalQuest = null;
							continue;
						}
						PersonalQuestID personalQuestID3 = list4[0];
						GameState.PersonalQuestDeckIDs.Remove(personalQuestID3);
						allCharacter4.PersonalQuest = new PersonalQuestState(personalQuestID3, repo, campaignController2);
						allCharacter4.PersonalQuest.Def.StartQuest(allCharacter4.PersonalQuest, campaignController2);
					}
				}
			}
			campaignController2.Cheat_EndResult();
		}
		if (SaveVersion < 29)
		{
			foreach (ChallengeDef value3 in repo.Challenges.Values)
			{
				if (!GameState.CompletedChallenges.Contains(value3) && !GameState.ChallengeDeck.Contains(value3))
				{
					GameState.ChallengeDeckIDs.Add(value3.ID);
				}
			}
		}
		if (SaveVersion < 30)
		{
			CampaignController campaignController3 = new CampaignController(GameState, repo);
			BuildingTypeID id = new BuildingTypeID(42u);
			if (GameState.CompletedPersonalQuests.Any((PersonalQuestState x) => x.Def is PQ_2) && !campaignController3.BuildingsController.HasBuildingBeenUnlocked(id))
			{
				campaignController3.Cheat_ApplyEffect(new CampaignEffect(CampaignEffectType.OpenEnvelope, (int)id.ID));
			}
			foreach (CharacterState allCharacter5 in GameState.AllCharacters)
			{
				if (allCharacter5.PersonalQuestDef is PQ_2 && allCharacter5.PersonalQuest.ProgressTrackers.Length == 3)
				{
					uint[] progressTrackers = allCharacter5.PersonalQuest.ProgressTrackers;
					uint[] progressTrackers2 = new uint[4]
					{
						progressTrackers[0],
						progressTrackers[1],
						0u,
						progressTrackers[2]
					};
					allCharacter5.PersonalQuest.ProgressTrackers = progressTrackers2;
				}
				else if (allCharacter5.PersonalQuestDef is PQ_7 && allCharacter5.PersonalQuest.ProgressTrackers.Length == 4)
				{
					uint[] progressTrackers3 = allCharacter5.PersonalQuest.ProgressTrackers;
					uint[] progressTrackers4 = new uint[6]
					{
						progressTrackers3[0],
						0u,
						progressTrackers3[1],
						0u,
						progressTrackers3[2],
						progressTrackers3[3]
					};
					allCharacter5.PersonalQuest.ProgressTrackers = progressTrackers4;
				}
			}
			bool flag = false;
			int num4 = 0;
			do
			{
				flag = false;
				List<PuzzleState> list5 = GameState.UnlockedPuzzles.ToList();
				for (int num5 = num4; num5 < list5.Count; num5++)
				{
					PuzzleState puzzleState = GameState.UnlockedPuzzles[num5];
					if ((!puzzleState.IsCompleted || puzzleState.Def is LockMechanism_PuzzleDef) && ((puzzleState.Stages.Count > 0 && puzzleState.Stages.All((PuzzleState.StageState x) => x.Completed)) || campaignController3.PuzzleController.CanSolvePuzzle(puzzleState)))
					{
						campaignController3.PuzzleController.Cheat_CompletePuzzle(puzzleState);
						List<PuzzleState> list6 = GameState.UnlockedPuzzles.Except(list5).ToList();
						num4 = num5 + 1;
						for (int num6 = list6.Count - 1; num6 >= 0; num6--)
						{
							PuzzleState item2 = list6[num6];
							GameState.UnlockedPuzzles.Remove(item2);
							GameState.UnlockedPuzzles.Insert(num4, item2);
						}
						flag = true;
						break;
					}
				}
			}
			while (flag);
			foreach (CampaignEffect item6 in YMLDefRepository.Repo.GameModes.Items.FirstOrDefault((GameMode x) => x.Name.Equals(GameState.GameMode.Name, StringComparison.InvariantCultureIgnoreCase)).StartingCampaignEffects.Where((CampaignEffect x) => x.Type == CampaignEffectType.UnlockPuzzle))
			{
				if (GameState.GameMode.StartingCampaignEffects.Contains(item6))
				{
					continue;
				}
				GameState.GameMode.StartingCampaignEffects.Add(item6);
				string puzzleKey = item6.GetString(0);
				if (!GameState.UnlockedPuzzles.Any((PuzzleState x) => x.ID.ID.Equals(puzzleKey)))
				{
					PuzzleState puzzleState2 = new PuzzleState(new PuzzleID(puzzleKey), repo, GameState);
					GameState.UnlockedPuzzles.Add(puzzleState2);
					if (puzzleState2.Def.CanSolve(campaignController3))
					{
						campaignController3.PuzzleController.Cheat_CompletePuzzle(puzzleState2);
					}
				}
			}
			if (GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID == "160.2") && !GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID == "35.3") && !GameState.AdditionalLootCards.Any((LootCardState x) => x.ID == 1419) && !GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID == "128.5"))
			{
				campaignController3.Cheat_ApplyEffect(new CampaignEffect(CampaignEffectType.AddAdditionalLootCard, 1419));
			}
		}
		if (SaveVersion < 31)
		{
			List<LootCardState> list7 = new List<LootCardState>(0);
			foreach (LootCardState lootCard in GameState.AdditionalLootCards)
			{
				if (!list7.Any((LootCardState x) => x.ID == lootCard.ID))
				{
					list7.Add(lootCard);
				}
			}
			GameState.AdditionalLootCards.Clear();
			GameState.AdditionalLootCards.AddRange(list7);
		}
		if (SaveVersion < 32 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_032_Ravens_Roost")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 33)
		{
			List<ChallengeID> collection2 = GameState.CompletedChallengeIDs.Distinct().ToList();
			GameState.CompletedChallengeIDs.Clear();
			GameState.CompletedChallengeIDs.AddRange(collection2);
			List<ChallengeID> collection3 = GameState.ChallengeDeckIDs.Distinct().Except(GameState.CompletedChallengeIDs).ToList();
			GameState.ChallengeDeckIDs.Clear();
			GameState.ChallengeDeckIDs.AddRange(collection3);
		}
		if (SaveVersion < 34 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_041_Unfettered_Shard")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 35)
		{
			if (FastReloadState != null)
			{
				ApplyMissingOnSummonBonuses(FastReloadState);
			}
			ApplyMissingOnSummonBonuses(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 36 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_038_The_Way_Forward")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 37 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_110_Guardians_Temple")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 38)
		{
			if (FastReloadState != null)
			{
				ApplyBootsOfSpeedGatheringForceBugfix(FastReloadState);
			}
			ApplyBootsOfSpeedGatheringForceBugfix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 39 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_037_The_Dead_Mile")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 44)
		{
			OutpostEventID item3 = new OutpostEventID("WR-07");
			if (!GameState.Outpost.WinterRoadEventDeckIDs.Contains(item3))
			{
				GameState.Outpost.WinterRoadEventDeckIDs.Add(item3);
			}
		}
		if (SaveVersion < 40)
		{
			if (FastReloadState != null)
			{
				ApplyBeguilingForceFix(FastReloadState);
			}
			ApplyBeguilingForceFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 41 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_083_Rising_Brine")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 42)
		{
			new CampaignController(GameState, repo).Cheat_RefreshEventFilter();
		}
		if (SaveVersion < 43)
		{
			if (FastReloadState != null)
			{
				ApplySwordOfMasterChargesFix(FastReloadState);
			}
			ApplySwordOfMasterChargesFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 44)
		{
			if (MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_059_Automaton_Uprising")
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
			OutpostEventID item4 = new OutpostEventID("WR-07");
			if (!GameState.Outpost.WinterRoadEventDeckIDs.Contains(item4))
			{
				GameState.Outpost.WinterRoadEventDeckIDs.Add(item4);
			}
		}
		if (SaveVersion < 46)
		{
			if (FastReloadState != null)
			{
				ApplyMetalMosaicHardResetChanges(FastReloadState);
			}
			ApplyMetalMosaicHardResetChanges(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 47)
		{
			if (FastReloadState != null)
			{
				ApplyScenario67HealCoreChanges(FastReloadState);
			}
			ApplyScenario67HealCoreChanges(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 48)
		{
			foreach (CharacterState allCharacter6 in GameState.AllCharacters)
			{
				if (allCharacter6.ID == null || !allCharacter6.ID.IsValid)
				{
					allCharacter6.RerollID();
				}
			}
			foreach (CharacterState retiredCharacter in GameState.RetiredCharacters)
			{
				if (retiredCharacter.ID == null || !retiredCharacter.ID.IsValid)
				{
					retiredCharacter.RerollID();
				}
			}
		}
		if (SaveVersion < 49)
		{
			int num7 = GameState.Stickers.Count((CampaignStickerID x) => x.ID.Contains("Brummix"));
			CampaignSectionID campaignSectionID = new CampaignSectionID("40.1");
			if (num7 == 5 && !GameState.ActivatedCampaignSectionIDs.Contains(campaignSectionID))
			{
				GameState.NextScenarioModifiers.EffectsOnStart.RemoveAll((CampaignEffect x) => x.Type == CampaignEffectType.ActivateCampaignSection && x.Param1 == "40.1");
				new CampaignController(GameState, repo).Cheat_ActivateCampaignSection(campaignSectionID);
			}
		}
		if (SaveVersion < 50)
		{
			foreach (CalendarEventState calendarEvent in GameState.Outpost.CalendarEvents)
			{
				switch (calendarEvent.SectionID.ID)
				{
				case "32.3":
					calendarEvent.TriggerAtWeek = 5u;
					break;
				case "183.3":
					if (calendarEvent.TriggerAtWeek >= 30)
					{
						calendarEvent.TriggerAtWeek = 30u;
					}
					else if (calendarEvent.TriggerAtWeek >= 25)
					{
						calendarEvent.TriggerAtWeek = 25u;
					}
					else
					{
						calendarEvent.TriggerAtWeek = 10u;
					}
					break;
				case "21.4":
					calendarEvent.TriggerAtWeek = 11u;
					break;
				case "129.3":
					calendarEvent.TriggerAtWeek = 21u;
					break;
				case "184.1":
					calendarEvent.TriggerAtWeek = 41u;
					break;
				case "137.2":
					calendarEvent.TriggerAtWeek = 81u;
					break;
				}
			}
		}
		if (SaveVersion < 51)
		{
			foreach (ChallengeDef value4 in repo.Challenges.Values)
			{
				if (!GameState.CompletedChallengeIDs.Contains(value4.ID) && !GameState.ChallengeDeckIDs.Contains(value4.ID))
				{
					GameState.ChallengeDeckIDs.Add(value4.ID);
				}
			}
			if (GameState.ActiveScenario != null && GameState.ActiveScenario.DrawnChallengeIDs.Count == 0 && !GameState.ActiveScenario.HasEnteredScenario)
			{
				CampaignController campaignController4 = new CampaignController(GameState, repo);
				if (campaignController4.ScenarioController.CanUseChallenges)
				{
					campaignController4.ScenarioController.Action_DrawChallenges();
				}
			}
		}
		if (SaveVersion < 52)
		{
			ScenarioRuleLibrary.ScenarioState scenarioState2 = FastReloadState;
			if (scenarioState2 == null)
			{
				scenarioState2 = MapState?.CurrentMapScenarioState?.CurrentState;
			}
			if (scenarioState2 != null && scenarioState2.AllyMonsters.Where((EnemyState x) => !string.IsNullOrEmpty(x.SummonerGuid)).ToList().Count > 0 && (scenarioState2.ID == "FS_002_Algox_Scouting" || scenarioState2.ID == "FS_028_Summit_Meeting"))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 53)
		{
			if (FastReloadState != null)
			{
				ApplyMetalMosaicHardResetChanges(FastReloadState);
			}
			ApplyMetalMosaicHardResetChanges(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 54)
		{
			if (FastReloadState != null)
			{
				ApplyStatTargetTypeFix(FastReloadState);
			}
			ApplyStatTargetTypeFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 56 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_077_The_Fish_King's_Ascension")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 57)
		{
			RefundWrongEnhancementTypes(GameState, repo, SaveVersion);
		}
		if (SaveVersion < 58)
		{
			ScenarioRuleLibrary.ScenarioState scenarioState3 = FastReloadState;
			if (scenarioState3 == null)
			{
				scenarioState3 = MapState?.CurrentMapScenarioState?.CurrentState;
			}
			if (scenarioState3 != null && scenarioState3.AllyMonsters.Where((EnemyState x) => !string.IsNullOrEmpty(x.SummonerGuid)).ToList().Count > 0 && (scenarioState3.ID == "FS_065_A_Strong_Foundation" || scenarioState3.ID == "FS_018_Crystal_Fields" || scenarioState3.ID == "FS_112_Raised_By_Wolves" || scenarioState3.ID == "FS_092_Sinking_Ship"))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 59)
		{
			ScenarioRuleLibrary.ScenarioState scenarioState4 = FastReloadState;
			if (scenarioState4 == null)
			{
				scenarioState4 = MapState?.CurrentMapScenarioState?.CurrentState;
			}
			if (scenarioState4 != null && scenarioState4.Objects.Any((ObjectState x) => !x.IsDead && (x.Type == CActor.EType.Enemy || x.Type == CActor.EType.Enemy2)) && scenarioState4.ChallengeSerialization.Any(((uint id, string storage) x) => x.id == 1497))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 60)
		{
			ScenarioRuleLibrary.ScenarioState scenarioState5 = FastReloadState;
			if (scenarioState5 == null)
			{
				scenarioState5 = MapState?.CurrentMapScenarioState?.CurrentState;
			}
			if (scenarioState5 != null && scenarioState5.ChallengeSerialization.Any(((uint id, string storage) x) => x.id == 1499))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 61)
		{
			CampaignController campaignController5 = new CampaignController(GameState, repo);
			campaignController5.OutpostController.UseDefaultTownGuardDeck();
			foreach (TownGuardPerkID addedTownGuardPerk in GameState.Outpost.AddedTownGuardPerks)
			{
				TownGuardPerkDef townGuardPerkDef = repo.TownGuardPerkDefs[addedTownGuardPerk];
				foreach (TownGuardAttackModifierID item7 in townGuardPerkDef.TownGuardModsToAdd())
				{
					GameState.Outpost.TownGuardDeckIDs.Add(item7);
				}
				foreach (TownGuardAttackModifierID item8 in townGuardPerkDef.TownGuardModsToRemove())
				{
					GameState.Outpost.TownGuardDeckIDs.Remove(item8);
				}
			}
			IEnumerable<CampaignEffect> first = GameState.UnlockedScenarios.Where((FHRL.ScenarioState x) => x.Completed).SelectMany((FHRL.ScenarioState x) => x.Def.Conclusions[(int)x.ConclusionIndex].Effects);
			IEnumerable<CampaignEffect> second = GameState.ActivatedCampaignSectionIDs.SelectMany((CampaignSectionID x) => repo.CampaignSections[x].Effects);
			foreach (CampaignEffect item9 in first.Concat(second))
			{
				if (item9.Type == CampaignEffectType.AddTownGuardMod || item9.Type == CampaignEffectType.LoseTownGuardMod)
				{
					campaignController5.Cheat_ApplyEffect(item9);
				}
			}
			foreach (OutpostEventState activatedEvent3 in GameState.Outpost.ActivatedEvents)
			{
				foreach (CampaignEffect item10 in activatedEvent3.AppliedEffectsFromOutcome)
				{
					if (string.IsNullOrEmpty(item10.Param1))
					{
						continue;
					}
					TownGuardAttackModifierID key = new TownGuardAttackModifierID(item10.Param1);
					if (repo.TownGuardAttackModifierDefs.ContainsKey(key))
					{
						CampaignEffectType campaignEffectType = CampaignEffectType.None;
						switch (activatedEvent3.ID.ID)
						{
						case "WO-52":
						case "WO-65":
						case "WO-78":
							campaignEffectType = CampaignEffectType.LoseTownGuardMod;
							break;
						case "WO-69":
						case "SO-62":
						case "SO-54":
						case "SR-45":
							campaignEffectType = CampaignEffectType.AddTownGuardMod;
							break;
						}
						if (campaignEffectType != CampaignEffectType.None)
						{
							CampaignEffect effect = new CampaignEffect(campaignEffectType, item10.Param1);
							campaignController5.Cheat_ApplyEffect(effect);
						}
					}
				}
			}
		}
		if (SaveVersion < 62 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_131_The_Dancing_Iceberg")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 63 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_114_Work_Freeze")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 63 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_092_Sinking_Ship")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 63)
		{
			CampaignController campaignController6 = new CampaignController(GameState, repo);
			if (campaignController6.CountCampaignSticker("AncientCoin") >= 1)
			{
				CampaignSectionID tavernUpgradedSection = new CampaignSectionID("35.3");
				if (!GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID.Equals(tavernUpgradedSection.ID)))
				{
					campaignController6.Cheat_ActivateCampaignSection(tavernUpgradedSection);
				}
				if (campaignController6.CountCampaignSticker("AncientCoin") >= 3)
				{
					CampaignSectionID ancientCoinsCollectedSection = new CampaignSectionID("113.1");
					if (!GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID.Equals(ancientCoinsCollectedSection.ID)))
					{
						campaignController6.Cheat_ActivateCampaignSection(ancientCoinsCollectedSection);
					}
				}
			}
		}
		if (SaveVersion < 64)
		{
			CampaignController campaignController7 = new CampaignController(GameState, repo);
			foreach (KeyValuePair<ItemID, ItemDef> item11 in repo.Items)
			{
				ItemID key2 = item11.Key;
				ItemDef value = item11.Value;
				int num8 = campaignController7.CountItemInstances(value);
				if (num8 <= value.Quantity)
				{
					continue;
				}
				for (uint num9 = value.Quantity; num9 < num8; num9++)
				{
					GameState.Outpost.PurchasableItemIDs.Remove(key2);
				}
				num8 = campaignController7.CountItemInstances(value);
				if (num8 > value.Quantity)
				{
					for (uint num10 = value.Quantity; num10 < num8; num10++)
					{
						GameState.Outpost.CraftableItemIDs.Remove(key2);
					}
				}
			}
		}
		if (SaveVersion < 65)
		{
			if (FastReloadState != null)
			{
				ApplyGreedIsGoodFix(FastReloadState);
			}
			ApplyGreedIsGoodFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 66)
		{
			if (FastReloadState != null)
			{
				ApplyWardensRobeToggleFix(FastReloadState);
			}
			ApplyWardensRobeToggleFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 67)
		{
			ScenarioRuleLibrary.ScenarioState scenarioState6 = FastReloadState;
			if (scenarioState6 == null)
			{
				scenarioState6 = MapState?.CurrentMapScenarioState?.CurrentState;
			}
			if (scenarioState6 != null && scenarioState6.ChallengeSerialization.Any(((uint id, string storage) x) => x.id == 1518))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 68)
		{
			ScenarioRuleLibrary.ScenarioState scenarioState7 = FastReloadState;
			if (scenarioState7 == null)
			{
				scenarioState7 = MapState?.CurrentMapScenarioState?.CurrentState;
			}
			if (scenarioState7 != null && scenarioState7.ChallengeSerialization.Any(((uint id, string storage) x) => x.id == 1519))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 69)
		{
			foreach (ChallengeDef value5 in repo.Challenges.Values)
			{
				if (!GameState.CompletedChallengeIDs.Contains(value5.ID) && !GameState.ChallengeDeckIDs.Contains(value5.ID))
				{
					GameState.ChallengeDeckIDs.Add(value5.ID);
				}
			}
			if (GameState.ActiveScenario != null && GameState.ActiveScenario.DrawnChallengeIDs.Count == 0 && !GameState.ActiveScenario.HasEnteredScenario)
			{
				CampaignController campaignController8 = new CampaignController(GameState, repo);
				if (campaignController8.ScenarioController.CanUseChallenges)
				{
					campaignController8.ScenarioController.Action_DrawChallenges();
				}
			}
		}
		if (SaveVersion < 69 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_118_Lurker_Necromancy")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 70 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_083_Rising_Brine")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 71 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_072_A_Giant_Block_of_Ice")
		{
			if (FastReloadState != null)
			{
				ApplyIceBlockEnemy2FixFix(FastReloadState);
			}
			ApplyIceBlockEnemy2FixFix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 70)
		{
			if (FastReloadState != null)
			{
				ApplyEmpoweringNoteEmberEnergyRework(FastReloadState);
			}
			ApplyEmpoweringNoteEmberEnergyRework(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 71 && GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID == "129.3") && !GameState.Outpost.WinterRoadEventDeckIDs.Any((OutpostEventID x) => x.ID == "WR-21"))
		{
			GameState.Outpost.WinterRoadEventDeckIDs.Add(new OutpostEventID("WR-21"));
		}
		if (SaveVersion < 72 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_073_Flotsam")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 72)
		{
			ActiveScenarioState activeScenario = GameState.ActiveScenario;
			if (activeScenario != null)
			{
				List<MonsterCardID> monsterCardsDrawnForStrategyFavor = activeScenario.MonsterCardsDrawnForStrategyFavor;
				if (monsterCardsDrawnForStrategyFavor != null && monsterCardsDrawnForStrategyFavor.Count == 0)
				{
					CampaignController campaignController9 = new CampaignController(GameState, repo);
					GameState.UnlockedScenarios.FirstOrDefault((FHRL.ScenarioState s) => s.Def == GameState.ActiveScenario.Def);
					campaignController9.ScenarioController.Action_DrawMonsterCardsForStrategyFavor();
				}
			}
		}
		if (SaveVersion < 73)
		{
			int adjustedMonsterCount;
			if (FastReloadState != null)
			{
				LevelEditorController.ReplaceEels(FastReloadState, out adjustedMonsterCount);
			}
			LevelEditorController.ReplaceEels(MapState?.CurrentMapScenarioState?.CurrentState, out adjustedMonsterCount);
		}
		if (SaveVersion < 74 && GameState.CompletedPersonalQuests.FirstOrDefault((PersonalQuestState x) => x.ID.ID == "PQ_2") != null)
		{
			new CampaignController(GameState, repo).Cheat_ApplyEffect(new CampaignEffect(CampaignEffectType.UnlockScenario, (int)PQ_2.scenarioUnlockID));
		}
		if (SaveVersion < 75)
		{
			if (FastReloadState != null)
			{
				ApplyBeguilingForceFix(FastReloadState);
			}
			ApplyBeguilingForceFix(MapState?.CurrentMapScenarioState?.CurrentState);
			if (MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_065_A_Strong_Foundation")
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 76 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_077_The_Fish_King's_Ascension")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 77)
		{
			CampaignController campaignController10 = new CampaignController(GameState, repo);
			foreach (CharacterState allCharacter7 in GameState.AllCharacters)
			{
				foreach (ItemID ownedItemID in allCharacter7.OwnedItemIDs)
				{
					ItemDef itemDef = repo.Items[ownedItemID];
					if (itemDef.CanBeBrewed && !campaignController10.CraftingController.IsPotionDiscovered(itemDef))
					{
						campaignController10.Cheat_BeginResult();
						campaignController10.CraftingController.Cheat_DiscoverBrewableItem(itemDef);
						campaignController10.Cheat_EndResult();
						GameState.Outpost.CraftableItemIDs.Remove(ownedItemID);
					}
				}
			}
		}
		if (SaveVersion < 78 && SlotAssignments != null && SlotAssignments.Count > 0)
		{
			foreach (CharacterState character2 in GameState.AllCharacters)
			{
				if (character2.PartyState == CharacterPartyStateType.InParty && SlotAssignments.FirstOrDefault((CharacterID x) => x != null && x.ID == character2.ID.ID) == null)
				{
					character2.PartyState = CharacterPartyStateType.Removed;
				}
			}
		}
		if (SaveVersion < 79 && GameState.ActiveScenario != null)
		{
			foreach (ActiveScenarioState.CharacterSettings characterScenarioSetting in GameState.ActiveScenario.CharacterScenarioSettings)
			{
				characterScenarioSetting.MigrateFromClassId(GameState);
			}
		}
		if (SaveVersion < 80 && GameState.ActiveScenario != null)
		{
			if (FastReloadState != null && FastReloadState.HeroSummons.Any((HeroSummonState x) => x.ClassID == "BoneHordeID" && !x.IsDead))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
			if (MapState?.CurrentMapScenarioState?.CurrentState?.HeroSummons != null && MapState.CurrentMapScenarioState.CurrentState.HeroSummons.Any((HeroSummonState x) => x.ClassID == "BoneHordeID" && !x.IsDead))
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 81)
		{
			if (FastReloadState != null)
			{
				ApplyScenraio102MoveBuffBugfix(FastReloadState);
			}
			ApplyScenraio102MoveBuffBugfix(MapState?.CurrentMapScenarioState?.CurrentState);
		}
		if (SaveVersion < 82)
		{
			CampaignController campaignController11 = new CampaignController(GameState, repo);
			foreach (FHRL.ScenarioState item12 in GameState.UnlockedScenarios.Where((FHRL.ScenarioState x) => x.Completed))
			{
				for (int num11 = 2; num11 <= 4; num11++)
				{
					foreach (ScenarioDef.Monster item13 in campaignController11.ScenarioController.GetMonstersToShow(item12, num11))
					{
						FHUICodex.UnlockCodexTag(FHUICodexUtils.CodexActorIDToUnlockTag(item13.ID.ID));
					}
				}
			}
			if (GameState.ActiveScenario != null)
			{
				ScenarioRuleLibrary.ScenarioState scenarioState8 = MapState?.CurrentMapScenarioState?.CurrentState;
				if (FastReloadState != null)
				{
					scenarioState8 = FastReloadState;
				}
				if (scenarioState8 != null)
				{
					foreach (EnemyState item14 in scenarioState8.Monsters.Concat(scenarioState8.AllyMonsters).Concat(scenarioState8.NeutralMonsters).Concat(scenarioState8.Enemy2Monsters)
						.Concat(scenarioState8.Objects))
					{
						if (item14.IsRevealed)
						{
							FHUICodex.UnlockCodexTag(FHUICodexUtils.CodexActorIDToUnlockTag(item14.ClassID));
						}
					}
				}
			}
			foreach (BuildingState building3 in GameState.Outpost.Buildings)
			{
				FHUICodex.UnlockCodexTag(building3.ID);
			}
		}
		if (SaveVersion < 83 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "RFS_07_Assault_On_The_Warchief")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 85 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_094_A_Grand_View")
		{
			int roundNumber = MapState.CurrentMapScenarioState.CurrentState.RoundNumber;
			if (FastReloadState != null)
			{
				roundNumber = FastReloadState.RoundNumber;
			}
			if (roundNumber <= 1)
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
			}
		}
		if (SaveVersion < 86 && MapState?.CurrentMapScenarioState?.CurrentState?.ID == "FS_074_Gaps_in_the_Road_B")
		{
			UnityGameEditorRuntime.InvalidatedScenarioSave = true;
		}
		if (SaveVersion < 87 && (GameState.UnlockedFavors == null || GameState.UnlockedFavors.Count == 0))
		{
			CampaignController campaignController12 = new CampaignController(GameState, repo);
			if (campaignController12.DefRepository.Buildings.Values.FirstOrDefault((BuildingTypeDef b) => b is HallOfRevelry_BuildingTypeDef) is HallOfRevelry_BuildingTypeDef hallOfRevelry_BuildingTypeDef)
			{
				BuildingState building = campaignController12.BuildingsController.GetBuilding(hallOfRevelry_BuildingTypeDef.ID);
				if (building != null && building.ConstructionState == BuildingConstructionType.None)
				{
					CampaignEffect effect2 = hallOfRevelry_BuildingTypeDef.GetOnConstructEffects(building.Level, campaignController12).FirstOrDefault((CampaignEffect e) => e.Type == CampaignEffectType.FavorUnlockLevel);
					if (effect2.Type != CampaignEffectType.None)
					{
						campaignController12.Cheat_ApplyEffect(effect2);
					}
				}
			}
		}
		if (SaveVersion < 88)
		{
			CampaignController campaignController13 = new CampaignController(GameState, repo);
			if (campaignController13.CountCampaignSticker("AncientCoin") >= 1)
			{
				CampaignSectionID tavernUpgradedSection2 = new CampaignSectionID("35.3");
				if (!GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID.Equals(tavernUpgradedSection2.ID)))
				{
					campaignController13.Cheat_ActivateCampaignSection(tavernUpgradedSection2);
				}
				if (campaignController13.CountCampaignSticker("AncientCoin") >= 3)
				{
					CampaignSectionID ancientCoinsCollectedSection2 = new CampaignSectionID("113.1");
					if (!GameState.ActivatedCampaignSectionIDs.Any((CampaignSectionID x) => x.ID.Equals(ancientCoinsCollectedSection2.ID)))
					{
						campaignController13.Cheat_ActivateCampaignSection(ancientCoinsCollectedSection2);
					}
				}
			}
		}
		if (SaveVersion < 89)
		{
			bool flag2 = false;
			foreach (BuildingState building4 in GameState.Outpost.Buildings)
			{
				if (building4.donationDistribution != null && (building4.donationDistribution.goldDistributions.Any((GoldDistribution d) => d.Character != null && d.CharacterID == null) || building4.donationDistribution.materialDistributions.Any((ResourceDistribution d) => d.Character != null && d.CharacterID == null)))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				EnsureUsingCharacterIDs();
				foreach (BuildingState building5 in GameState.Outpost.Buildings)
				{
					if (building5.donationDistribution == null)
					{
						continue;
					}
					for (int num12 = 0; num12 < building5.donationDistribution.goldDistributions.Count; num12++)
					{
						GoldDistribution goldDistribution = building5.donationDistribution.goldDistributions[num12];
						if (goldDistribution.CharacterID == null && goldDistribution.Character != null)
						{
							CharacterState characterState = getCharacter(goldDistribution.Character);
							if (characterState != null)
							{
								building5.donationDistribution.goldDistributions[num12].CharacterID = characterState.ID;
							}
						}
					}
					for (int num13 = 0; num13 < building5.donationDistribution.materialDistributions.Count; num13++)
					{
						ResourceDistribution resourceDistribution = building5.donationDistribution.materialDistributions[num13];
						if (resourceDistribution.CharacterID == null && resourceDistribution.Character != null)
						{
							CharacterState characterState2 = getCharacter(resourceDistribution.Character);
							if (characterState2 != null)
							{
								building5.donationDistribution.materialDistributions[num13].CharacterID = characterState2.ID;
							}
						}
					}
				}
			}
		}
		if (SaveVersion >= 89)
		{
			return;
		}
		foreach (ChallengeDef value6 in repo.Challenges.Values)
		{
			if (!GameState.CompletedChallengeIDs.Contains(value6.ID) && !GameState.ChallengeDeckIDs.Contains(value6.ID))
			{
				GameState.ChallengeDeckIDs.Add(value6.ID);
			}
		}
		if (GameState.ActiveScenario != null && GameState.ActiveScenario.DrawnChallengeIDs.Count == 0 && !GameState.ActiveScenario.HasEnteredScenario)
		{
			CampaignController campaignController14 = new CampaignController(GameState, repo);
			if (campaignController14.ScenarioController.CanUseChallenges)
			{
				campaignController14.ScenarioController.Action_DrawChallenges();
			}
		}
		CharacterState getCharacter(CharacterClassDef classDef)
		{
			CharacterState characterState3 = GameState.PartyCharacters.FirstOrDefault((CharacterState c) => c.ClassDef.ID == classDef.ID);
			if (characterState3 != null)
			{
				return characterState3;
			}
			return null;
		}
	}

	private void ApplyEmpoweringNoteEmberEnergyRework(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			player.CheckPerkBonuses = new List<string> { "TBuff" };
		}
	}

	private List<CardEnhancementSlotState> RefundWrongEnhancementTypes(CampaignState GameState, DefRepository repo, int saveVersion)
	{
		List<CardEnhancementSlotState> list = new List<CardEnhancementSlotState>();
		CampaignController campaignController = new CampaignController(GameState, repo);
		foreach (ClassCardState cardModification in GameState.ClassCardModifications.ToList())
		{
			foreach (CardEnhancementSlotState slot in cardModification.EnhancedSlots.ToList())
			{
				if (slot.InvalidSlotID)
				{
					list.Add(slot);
					cardModification.EnhancedSlots.Remove(slot);
					if (cardModification.EnhancedSlots.Count == 0)
					{
						GameState.ClassCardModifications.Remove(cardModification);
					}
					uint num = 0u;
					if (cardModification.CardID.ID == 56 && saveVersion < 57)
					{
						num = slot.Enhancement.Cost + 150;
					}
					if (num != 0)
					{
						CharacterState characterState = GameState.ActiveCharacters.FirstOrDefault((CharacterState x) => x.ClassID == cardModification.CardDef.ClassID);
						if (characterState != null)
						{
							characterState.Gold += slot.Enhancement.Cost;
						}
					}
					continue;
				}
				CardEnhacementSlotQuery slot2 = new CardEnhacementSlotQuery(cardModification.CardDef, slot.SlotDef);
				if (!campaignController.EnhancementController.GetEnhancementsForSlot(slot2).ToList().Any((CardEnhancementTypeDef x) => x.ID == slot.EnhancementID))
				{
					list.Add(slot);
					cardModification.EnhancedSlots.Remove(slot);
					CharacterState characterState2 = GameState.ActiveCharacters.FirstOrDefault((CharacterState x) => x.ClassID == cardModification.CardDef.ClassID);
					if (characterState2 != null)
					{
						characterState2.Gold += (uint)campaignController.EnhancementController.GetEnhacementGoldCost(slot2, slot.Enhancement);
					}
					if (cardModification.EnhancedSlots.Count == 0)
					{
						GameState.ClassCardModifications.Remove(cardModification);
					}
				}
			}
		}
		return list;
	}

	private void ApplyMissingOnSummonBonuses(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (HeroSummonState obj in scenarioState.HeroSummons.Where((HeroSummonState x) => !x.IsDead))
		{
			if (obj.OnSummonActiveBonuses.Count == 0)
			{
				CHeroSummonClass cHeroSummonClass = CharacterClassManager.HeroSummonClasses.SingleOrDefault((CHeroSummonClass x) => x.ID == obj.ClassID);
				if (cHeroSummonClass.SummonYML.OnSummonAbilities != null && cHeroSummonClass.SummonYML.OnSummonAbilities.Count > 0)
				{
					obj.TempTryFixOnSummonBonuses = true;
				}
			}
		}
	}

	private void ApplyDetonationChargeRepositionBugfix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (ObjectState obj in scenarioState.Objects.Where((ObjectState x) => !x.IsDead && x.ClassID == "PropDummyObject"))
		{
			if (scenarioState.Props.SingleOrDefault((CObjectProp x) => x.PropGuid == obj.PropGuidAttachedTo) == null)
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
				break;
			}
		}
	}

	private void ApplyPetFrozenfistDuplicateIDsFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			List<int> ownedUnusedCardIDs = player.AbilityDeck.OwnedUnusedCardIDs;
			RemoveDuplicates(ownedUnusedCardIDs, player.AbilityDeck.DiscardedAbilityCardIDs);
			RemoveDuplicates(ownedUnusedCardIDs, player.AbilityDeck.LostAbilityCardIDs);
			RemoveDuplicates(ownedUnusedCardIDs, player.AbilityDeck.PermaLostAbilityCardIDs);
			RemoveDuplicates(ownedUnusedCardIDs, player.AbilityDeck.TakenCardIDs);
			RemoveDuplicatesTuple(ownedUnusedCardIDs, player.AbilityDeck.HandAbilityCardIDsAndInstanceIDs);
			RemoveDuplicatesTuple(ownedUnusedCardIDs, player.AbilityDeck.RoundAbilityCardIDsAndInstanceID);
			RemoveDuplicatesTuple(ownedUnusedCardIDs, player.AbilityDeck.RoundCardsSelectedInCardSelectionIDsAndInstanceID);
			RemoveDuplicatesTuple(ownedUnusedCardIDs, player.AbilityDeck.SelectedAbilityCardIDsAndInstanceIDs);
		}
		static void RemoveDuplicates(List<int> source, List<int> dest)
		{
			for (int num = source.Count - 1; num >= 0; num--)
			{
				if (dest.Contains(source[num]))
				{
					source.RemoveAt(num);
				}
			}
		}
		static void RemoveDuplicatesTuple(List<int> source, List<Tuple<int, int>> dest)
		{
			int i;
			for (i = source.Count - 1; i >= 0; i--)
			{
				if (dest.Any((Tuple<int, int> x) => x.Item1 == source[i]))
				{
					source.RemoveAt(i);
				}
			}
		}
	}

	private void ApplyBannerSpearAttackModifierNameFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			ReplaceModifierWithNames("AMC_Shield1_R_Banner_Spear", "AMC_Shield1_R_BannerSpear", player.AttackModifierDeck.AttackModifierCardsAvailable);
			ReplaceModifierWithNames("AMC_Shield1_R_Banner_Spear", "AMC_Shield1_R_BannerSpear", player.AttackModifierDeck.AttackModifierCardsPool);
			ReplaceModifierWithNames("AMC_Shield1_R_Banner_Spear", "AMC_Shield1_R_BannerSpear", player.AttackModifierDeck.AttackModifierCardsDiscarded);
			for (int num = player.AbilityDeck.ActivatedAbilityCardIDs.Count - 1; num >= 0; num--)
			{
				Tuple<int, CBaseCard.ECardType, string> tuple = player.AbilityDeck.ActivatedAbilityCardIDs[num];
				if (tuple.Item3.Equals("AMC_Shield1_R_Banner_Spear", StringComparison.InvariantCultureIgnoreCase))
				{
					player.AbilityDeck.ActivatedAbilityCardIDs[num] = new Tuple<int, CBaseCard.ECardType, string>(tuple.Item1, tuple.Item2, "AMC_Shield1_R_BannerSpear");
				}
			}
			for (int num2 = player.AbilityDeck.ActiveBonuses.Count - 1; num2 >= 0; num2--)
			{
				ActiveBonusState activeBonusState = player.AbilityDeck.ActiveBonuses[num2];
				if (activeBonusState.CardName.Equals("AMC_Shield1_R_Banner_Spear", StringComparison.InvariantCultureIgnoreCase))
				{
					activeBonusState.CardName = "AMC_Shield1_R_BannerSpear";
				}
			}
		}
	}

	private void ReplaceModifierWithNames(string originalName, string newName, List<string> modifiers)
	{
		for (int num = modifiers.Count - 1; num >= 0; num--)
		{
			if (modifiers[num].Equals(originalName, StringComparison.InvariantCultureIgnoreCase))
			{
				modifiers[num] = newName;
			}
		}
	}

	private void ApplyMetalMosaicHardResetChanges(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			if (player.ClassID == "MetalMosaicID")
			{
				player.CheckPerkBonuses = new List<string> { "MetalMosaic10" };
			}
		}
	}

	private void ApplyScenario67HealCoreChanges(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null || !(scenarioState.ID == "FS_067_Core_Attunement"))
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			player.CheckPerkBonuses = new List<string> { "FH067_HealCore" };
		}
	}

	private void ApplySwordOfMasterChargesFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			foreach (ActiveBonusState activeBonuse in player.AbilityDeck.ActiveBonuses)
			{
				if (activeBonuse.CardID == 1219)
				{
					activeBonuse.Remaining = 1;
				}
			}
		}
	}

	private void ApplyBeguilingForceFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (var item in scenarioState.ChallengeSerialization)
		{
			if (item.id == 1498)
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
				break;
			}
		}
	}

	private void ApplyGreedIsGoodFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (var item in scenarioState.ChallengeSerialization)
		{
			if (item.id == 1490)
			{
				UnityGameEditorRuntime.InvalidatedScenarioSave = true;
				break;
			}
		}
	}

	private void ApplyBootsOfSpeedGatheringForceBugfix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			foreach (ActiveBonusState activeBonuse in player.AbilityDeck.ActiveBonuses)
			{
				if (activeBonuse.CardID == 333 && activeBonuse.AbilityName != "TActiveReducePush" && activeBonuse.AbilityName != "TActiveInfuse")
				{
					UnityGameEditorRuntime.InvalidatedScenarioSave = true;
					return;
				}
			}
		}
	}

	private void ApplyScenario107ReactivatedObjectiveFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState != null && scenarioState.ID == "FS_107_My_Private_Empire")
		{
			List<CObjective> winObjectives = scenarioState.WinObjectives;
			CObjective cObjective = winObjectives[2];
			if (winObjectives[3].IsActive)
			{
				cObjective.SetActivation(active: false, fromMigration: true);
				cObjective.SetIsComplete(val: true);
			}
		}
	}

	private void ApplyIceBlockEnemy2FixFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		ObjectState objectState = scenarioState.Objects.FirstOrDefault((ObjectState x) => x.ClassID == "IceBlockID");
		if (objectState != null)
		{
			objectState.Type = CActor.EType.Enemy2;
		}
	}

	private void ApplyWardensRobeToggleFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			foreach (ActiveBonusState activeBonuse in player.AbilityDeck.ActiveBonuses)
			{
				if (activeBonuse.CardID == 1109)
				{
					activeBonuse.ToggledBonus = false;
					activeBonuse.RestrictedActorGuids.Clear();
				}
			}
		}
	}

	private void ApplyStatTargetTypeFix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState?.Stats == null)
		{
			return;
		}
		foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat in scenarioState.Stats.m_PlayerStats)
		{
			ApplyStatTargetTypeFix(scenarioState, playerStat.Value.Abilities);
		}
		ApplyStatTargetTypeFix(scenarioState, scenarioState.Stats.m_HeroSummonStats.Abilities);
		ApplyStatTargetTypeFix(scenarioState, scenarioState.Stats.m_MonsterStats.Abilities);
	}

	private void ApplyStatTargetTypeFix(ScenarioRuleLibrary.ScenarioState scenarioState, List<CPlayerStatsAbilities> abilityStats)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (CPlayerStatsAbilities abilityStat in abilityStats)
		{
			if (abilityStat.TargetGUIDs == null)
			{
				continue;
			}
			abilityStat.TargetTypes = new List<CActor.EType>();
			foreach (string guid in abilityStat.TargetGUIDs)
			{
				if (scenarioState.Monsters.Any((EnemyState x) => x.ActorGuid == guid))
				{
					abilityStat.TargetTypes.Add(CActor.EType.Enemy);
				}
				else if (scenarioState.Enemy2Monsters.Any((EnemyState x) => x.ActorGuid == guid))
				{
					abilityStat.TargetTypes.Add(CActor.EType.Enemy2);
				}
				else if (scenarioState.AllyMonsters.Any((EnemyState x) => x.ActorGuid == guid))
				{
					abilityStat.TargetTypes.Add(CActor.EType.Ally);
				}
				else if (scenarioState.NeutralMonsters.Any((EnemyState x) => x.ActorGuid == guid))
				{
					abilityStat.TargetTypes.Add(CActor.EType.Neutral);
				}
				else
				{
					abilityStat.TargetTypes.Add(CActor.EType.Player);
				}
			}
		}
	}

	private void ApplyAlreadyLootedChestFixForScenario16(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		List<string> lootedChestTreasureTableIDs = GameState.LootedChestTreasureTableIDs;
		scenarioState.ScenarioSetup.AlreadyLootedChests = lootedChestTreasureTableIDs.ToList();
		if (!(scenarioState.ID == "FS_016_Derelict_Elevator"))
		{
			return;
		}
		foreach (string chest in lootedChestTreasureTableIDs)
		{
			scenarioState.LootedChests.Remove(chest);
			List<CObjectProp> list = scenarioState.Props.Where((CObjectProp x) => x is CObjectChest { Type: var type } && type.ToString().Equals(chest, StringComparison.InvariantCultureIgnoreCase)).ToList();
			if (list.Count <= 0)
			{
				continue;
			}
			foreach (CObjectProp item in list)
			{
				scenarioState.Props.Remove(item);
				scenarioState.DestroyedProps.Add(item);
			}
		}
	}

	private void ApplyRemoveInvalidUpdate1Items(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			foreach (CItem item in player.Items.Where((CItem x) => x.ID == 1091 || x.ID == 1262).ToList())
			{
				player.Items.Remove(item);
			}
		}
	}

	private void ApplyScenraio102MoveBuffBugfix(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		if (scenarioState == null)
		{
			return;
		}
		foreach (PlayerState player in scenarioState.Players)
		{
			foreach (ActiveBonusState activeBonuse in player.AbilityDeck.ActiveBonuses)
			{
				if (activeBonuse.ToggledBonus && activeBonuse.AbilityName == "BBuffMove" && activeBonuse.CardID == 444)
				{
					activeBonuse.ToggledBonus = false;
				}
			}
		}
	}

	private void ApplyRandomItemFixToScenarioState(ScenarioRuleLibrary.ScenarioState scenarioState, DefRepository repo)
	{
		int nextRandomItem = scenarioState?.ScenarioSetup?.NextRandomItemID ?? (GameState?.ActiveScenario?.NextRandomItem?.ScenarioItemID).GetValueOrDefault();
		if (nextRandomItem != 0 && GameState.AllItems.Any((ItemDef x) => x.ScenarioItemID == nextRandomItem))
		{
			if (scenarioState != null)
			{
				List<ItemID> list = new CampaignController(GameState, repo).ValidRandomItemDeckIDs.ToList();
				ItemDef itemDef = null;
				if (list.Count > 0)
				{
					itemDef = repo.Items[list[0]];
				}
				scenarioState.ScenarioSetup.NextRandomItemID = itemDef?.ScenarioItemID ?? 0;
				foreach (PlayerState player in scenarioState.Players)
				{
					if (!player.ScenarioLoot.ItemIds.Contains(nextRandomItem))
					{
						continue;
					}
					player.ScenarioLoot.ItemIds.Remove(nextRandomItem);
					CItem cItem = player.Items.SingleOrDefault((CItem x) => x.ID == nextRandomItem);
					if (cItem != null)
					{
						player.Items.Remove(cItem);
					}
					if (player.ScenarioLoot.ResourceAmount(LootCard.EType.RandomItem) > 0)
					{
						player.ScenarioLoot.ResourceTypeToCount.Remove(LootCard.EType.RandomItem);
						player.ScenarioLoot.DrawnLootCardTypes.Remove(LootCard.EType.RandomItem);
						player.ScenarioLootFromLootCards.ResourceTypeToCount.Remove(LootCard.EType.RandomItem);
						player.ScenarioLootFromLootCards.DrawnLootCardTypes.Remove(LootCard.EType.RandomItem);
						LootCard element = CLootDeck.AllCards.SingleOrDefault((LootCard x) => x.Type == LootCard.EType.RandomItem);
						if (element.Type == LootCard.EType.RandomItem)
						{
							scenarioState.LootDeck.Add((byte)CLootDeck.AllCards.IndexOf(element));
						}
					}
					break;
				}
			}
			if (ScenarioResult != null && ScenarioResult.RandomItemLooted)
			{
				ItemDef itemDef2 = repo.Items.Values.SingleOrDefault((ItemDef x) => x.ScenarioItemID == nextRandomItem);
				if (itemDef2 != null)
				{
					foreach (FHScenarioResult.Character character in ScenarioResult.Characters)
					{
						if (character.NewItems.Contains((int)itemDef2.ID.ID))
						{
							character.NewItems.Remove((int)itemDef2.ID.ID);
							ScenarioResult.RandomItemLooted = false;
							break;
						}
					}
				}
			}
		}
		if (scenarioState == null)
		{
			return;
		}
		foreach (CObjectProp destroyedProp in scenarioState.DestroyedProps)
		{
			if (destroyedProp is CObjectChest cObjectChest && !scenarioState.LootedChests.Contains(cObjectChest.Type.ToString()))
			{
				scenarioState.LootedChests.Add(cObjectChest.Type.ToString());
			}
		}
	}

	private void ApplyRandomItemFixToScenarioState2(ScenarioRuleLibrary.ScenarioState scenarioState, DefRepository repo)
	{
		int nextRandomItemID = scenarioState?.ScenarioSetup?.NextRandomItemID ?? (GameState?.ActiveScenario?.NextRandomItem?.ScenarioItemID).GetValueOrDefault();
		if (nextRandomItemID == 0)
		{
			return;
		}
		ItemDef itemDef = repo.Items.Values.SingleOrDefault((ItemDef x) => x.ScenarioItemID == nextRandomItemID);
		CampaignController campaignController = new CampaignController(GameState, repo);
		if (itemDef == null || campaignController.IsItemValid(itemDef))
		{
			return;
		}
		if (scenarioState != null)
		{
			List<ItemID> list = campaignController.ValidRandomItemDeckIDs.ToList();
			ItemDef itemDef2 = null;
			if (list.Count > 0)
			{
				itemDef2 = repo.Items[list[0]];
			}
			scenarioState.ScenarioSetup.NextRandomItemID = itemDef2?.ScenarioItemID ?? 0;
			foreach (PlayerState player in scenarioState.Players)
			{
				if (!player.ScenarioLoot.ItemIds.Contains(nextRandomItemID))
				{
					continue;
				}
				player.ScenarioLoot.ItemIds.Remove(nextRandomItemID);
				CItem cItem = player.Items.SingleOrDefault((CItem x) => x.ID == nextRandomItemID);
				if (cItem != null)
				{
					player.Items.Remove(cItem);
				}
				if (player.ScenarioLoot.ResourceAmount(LootCard.EType.RandomItem) > 0)
				{
					player.ScenarioLoot.ResourceTypeToCount.Remove(LootCard.EType.RandomItem);
					player.ScenarioLoot.DrawnLootCardTypes.Remove(LootCard.EType.RandomItem);
					player.ScenarioLootFromLootCards.ResourceTypeToCount.Remove(LootCard.EType.RandomItem);
					player.ScenarioLootFromLootCards.DrawnLootCardTypes.Remove(LootCard.EType.RandomItem);
					LootCard element = CLootDeck.AllCards.SingleOrDefault((LootCard x) => x.Type == LootCard.EType.RandomItem);
					if (element.Type == LootCard.EType.RandomItem)
					{
						scenarioState.LootDeck.Add((byte)CLootDeck.AllCards.IndexOf(element));
					}
				}
				break;
			}
		}
		if (ScenarioResult == null || !ScenarioResult.RandomItemLooted || itemDef == null)
		{
			return;
		}
		foreach (FHScenarioResult.Character character in ScenarioResult.Characters)
		{
			if (character.NewItems.Contains((int)itemDef.ID.ID))
			{
				character.NewItems.Remove((int)itemDef.ID.ID);
				ScenarioResult.RandomItemLooted = false;
				break;
			}
		}
	}

	public static CampaignState NewGameState(YMLDefRepository defRepo, string campaignID = "Frosthaven", int seed = 0)
	{
		return new CampaignController(defRepo.GameModes.Items.First((GameMode gm) => gm.Name == campaignID), defRepo.DefRepository, seed).State;
	}

	public static CampaignState NewGameState(int seed, EAdventureDifficulty difficulty, YMLDefRepository defRepo, string gameModeID = "Frosthaven")
	{
		CampaignState campaignState = NewGameState(defRepo, gameModeID, seed);
		campaignState.Difficulty = difficulty.ToString();
		return campaignState;
	}

	public void EnsureUsingCharacterIDs()
	{
		if (SlotAssignment_Old == null || (SlotAssignments != null && SlotAssignments.Count != 0))
		{
			return;
		}
		SlotAssignments = new List<CharacterID>(new CharacterID[GameState.Settings.MaxPartySize]);
		for (int i = 0; i < Math.Min(SlotAssignment_Old.Count, (int)GameState.Settings.MaxPartySize); i++)
		{
			CharacterClassID slot = SlotAssignment_Old[i];
			if (slot.ID.IsNullOrEmpty())
			{
				SlotAssignments[i] = null;
				continue;
			}
			CharacterState characterState = GameState.PartyCharacters.FirstOrDefault((CharacterState p) => p.ClassID == slot);
			if (characterState == null)
			{
				Log.Warning("Cannot migrate character of class " + slot.ID);
			}
			SlotAssignments[i] = characterState?.ID;
		}
	}

	public void EnsureValidParty()
	{
		CharacterState[] array = GameState.AllCharacters.ToArray();
		foreach (CharacterState characterState in array)
		{
			if (characterState.PartyState == CharacterPartyStateType.InParty && !SlotAssignments.Contains(characterState.ID))
			{
				characterState.PartyState = CharacterPartyStateType.Removed;
			}
			if (SlotAssignments.Contains(characterState.ID) && characterState.PartyState != CharacterPartyStateType.InParty)
			{
				characterState.PartyState = CharacterPartyStateType.InParty;
			}
		}
		if (GameState.ActiveScenario == null)
		{
			return;
		}
		for (int num = GameState.ActiveScenario.CharacterScenarioSettings.Count - 1; num >= 0; num--)
		{
			ActiveScenarioState.CharacterSettings setting = GameState.ActiveScenario.CharacterScenarioSettings[num];
			if (GameState.PartyCharacters.FirstOrDefault((CharacterState p) => p.ID == setting.CharacterID) == null)
			{
				GameState.ActiveScenario.CharacterScenarioSettings.RemoveAt(num);
			}
		}
	}
}

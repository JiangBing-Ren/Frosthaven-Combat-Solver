using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FH.LevelComponents.ProcGen;
using FH.YML;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Scenario;
using FHGUI.Scenario.Services;
using FHRL;
using GLOOM;
using Libs.Utils;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using Newtonsoft.Json;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FHGUI.Services;

public static class ScenarioResultViewService
{
	private struct CharacterStats(CMapCharacter character, CPlayerStats stats, int goldLooted, int chestsLooted, int xpEarned, int itemsUsed, int randomItemBlueprintsLooted)
	{
		public int kills = stats?.Kills.Count() ?? 0;

		public int damageDone = stats?.DamageDealt.Sum((CPlayerStatsDamage it2) => it2.FinalDamageAmount) ?? 0;

		public int damageTaken = stats?.DamageReceived.Sum((CPlayerStatsDamage it2) => it2.FinalDamageAmount) ?? 0;

		public int healingDone = stats?.Heals.Sum((CPlayerStatsHeal it2) => it2.HealAmount) ?? 0;

		public int goldLooted = goldLooted;

		public int chestsLooted = chestsLooted;

		public int xpEarned = xpEarned;

		public int itemsUsed = itemsUsed;

		public int randomItemBlueprintsLooted = randomItemBlueprintsLooted;
	}

	public const int MAX_CHARACTER_SLOTS = 4;

	private const int MAX_CONSOLATION_TITLES = 12;

	private const int MAX_BADGE_TITLES = 10;

	public const string PARTY_VIEW_SCENE_NAME = "FHPartyView";

	private const float SCENE_LOAD_PERCENT = 0.9f;

	private const float _transitionSeconds = 2f;

	private static Tween _transitionTween;

	public static CMapParty Party => AdventureState.MapState?.MapParty;

	public static void PopulateScenarioDebriefingModule(ScenarioDebriefingModule debriefingModule, EResult outcome, Action EndGameCallback, Action OnRetryCallback)
	{
		ScenarioDebriefingModule.Params obj = new ScenarioDebriefingModule.Params
		{
			BattleResults = GetBattleResultDTO(outcome),
			ScenarioNameKey = GetCurrentScenarioNameKey(),
			OnReturnToMap = EndGameCallback,
			OnRetry = OnRetryCallback
		};
		debriefingModule.Init(obj, outcome);
	}

	public static BattleResultDTO GetBattleResultDTO(EResult outcome)
	{
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		CampaignState frosthavenCampaign = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign;
		DefRepository defRepository = YMLDefRepository.Repo.DefRepository;
		CampaignController campaignController = new CampaignController(frosthavenCampaign, defRepository);
		campaignController.Cheat_BeginResult();
		BattleResultDTO battleResultDTO = new BattleResultDTO();
		foreach (CPlayerActor playerActor in Singleton<FHScenarioViewController>.InstanceFast?.GetService<PlayerViewService>()?.GetPartySlotOrderedPlayerActors()?.ToList() ?? ScenarioManager.Scenario.AllPlayers)
		{
			CharacterState characterState = frosthavenCampaign.PartyCharacters.SingleOrDefault((CharacterState state) => state.ClassID.ID == playerActor.CharacterClass.CharacterID);
			BattleResultDTO.HeroStats heroStats = new BattleResultDTO.HeroStats
			{
				ClassID = playerActor.CharacterClass.CharacterID,
				BattleStats = GetBattleStats(playerActor.Class.ID),
				Gold = playerActor.LootAmount(LootCard.EType.Money),
				XP = playerActor.XP,
				BonusXP = GetScenarioXP(outcome) + GetFavorXP(outcome, playerActor) + GetChallengesXP(outcome) + GetScenarioLevelXP(outcome, campaignController),
				ExperienceChangeFromAbilityCards = (playerActor.TotalLoot.XpFromSources.TryGetValue("AbilityCards", out var value) ? value : 0)
			};
			heroStats.Exhausted = playerActor.IsDeadForObjectives;
			heroStats.Materials.Lumber += playerActor.LootAmount(LootCard.EType.Lumber);
			heroStats.Materials.Metal += playerActor.LootAmount(LootCard.EType.Metal);
			heroStats.Materials.Hide += playerActor.LootAmount(LootCard.EType.Hide);
			battleResultDTO.Herbs.Arrowvine += playerActor.LootAmount(LootCard.EType.Arrowvine);
			battleResultDTO.Herbs.Axenut += playerActor.LootAmount(LootCard.EType.Axenut);
			battleResultDTO.Herbs.Corpsecap += playerActor.LootAmount(LootCard.EType.Corpsecap);
			battleResultDTO.Herbs.Flamefruit += playerActor.LootAmount(LootCard.EType.Flamefruit);
			battleResultDTO.Herbs.Rockroot += playerActor.LootAmount(LootCard.EType.Rockroot);
			battleResultDTO.Herbs.Snowthistle += playerActor.LootAmount(LootCard.EType.Snowthistle);
			heroStats.ResourceChangeFromLootCards.Lumber = playerActor.LootAmount(LootCard.EType.Lumber, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Metal = playerActor.LootAmount(LootCard.EType.Metal, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Hide = playerActor.LootAmount(LootCard.EType.Hide, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Arrowvine = playerActor.LootAmount(LootCard.EType.Arrowvine, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Axenut = playerActor.LootAmount(LootCard.EType.Axenut, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Corpsecap = playerActor.LootAmount(LootCard.EType.Corpsecap, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Flamefruit = playerActor.LootAmount(LootCard.EType.Flamefruit, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Rockroot = playerActor.LootAmount(LootCard.EType.Rockroot, fromLootCards: true);
			heroStats.ResourceChangeFromLootCards.Snowthistle = playerActor.LootAmount(LootCard.EType.Snowthistle, fromLootCards: true);
			heroStats.LootCardTypesDrawn.Lumber = playerActor.LootAmount(LootCard.EType.Lumber, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Metal = playerActor.LootAmount(LootCard.EType.Metal, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Hide = playerActor.LootAmount(LootCard.EType.Hide, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Arrowvine = playerActor.LootAmount(LootCard.EType.Arrowvine, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Axenut = playerActor.LootAmount(LootCard.EType.Axenut, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Corpsecap = playerActor.LootAmount(LootCard.EType.Corpsecap, fromLootCards: true);
			heroStats.LootCardTypesDrawn.Flamefruit = playerActor.LootAmount(LootCard.EType.Flamefruit, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Rockroot = playerActor.LootAmount(LootCard.EType.Rockroot, fromLootCards: false, cardCount: true);
			heroStats.LootCardTypesDrawn.Snowthistle = playerActor.LootAmount(LootCard.EType.Snowthistle, fromLootCards: false, cardCount: true);
			foreach (uint blueprint in playerActor.TotalLoot.LootedItemBlueprintIDs)
			{
				CItem cItem = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.ID == blueprint)?.GetItem;
				if (cItem == null)
				{
					continue;
				}
				ItemDataDTO itemDataDTO = CItemToDTO(cItem, defRepository);
				if (itemDataDTO != null)
				{
					BattleResultDTO battleResultDTO2 = battleResultDTO;
					if (battleResultDTO2.UnlockedBlueprints == null)
					{
						battleResultDTO2.UnlockedBlueprints = new List<ItemDataDTO>();
					}
					battleResultDTO.UnlockedBlueprints.Add(itemDataDTO);
				}
			}
			foreach (int itemId in playerActor.TotalLoot.ItemIds)
			{
				CItem cItem2 = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.ID == itemId)?.GetItem;
				if (cItem2 != null)
				{
					BattleResultDTO.HeroStats heroStats2 = heroStats;
					if (heroStats2.Items == null)
					{
						heroStats2.Items = new List<ItemDataDTO>();
					}
					heroStats.Items.Add(CItemToDTO(cItem2, defRepository));
				}
			}
			battleResultDTO.InspirationFromLoot += playerActor.TotalLoot.NumEmptyRandomScenarioDraws + playerActor.TotalLoot.NumEmptyItemBlueprintDraws;
			foreach (Tuple<string, CBattleGoalState> chosenBattleGoalState in AdventureState.MapState.CurrentMapScenarioState.ChosenBattleGoalStates)
			{
				if (!(heroStats.ClassID != chosenBattleGoalState.Item1))
				{
					heroStats.BattleGoal = new GoalDTO(chosenBattleGoalState.Item2, ScenarioManager.Scenario.AllPlayers.Count);
					heroStats.BattleGoal.IsChecked = IsBattleGoalComplete(chosenBattleGoalState.Item2, outcome, playerActor);
					break;
				}
			}
			heroStats.Masteries = GetMasteries(defRepository, characterState, campaignController, outcome, playerActor);
			heroStats.CapturedPets = GetCapturedPets(defRepository, heroStats.ClassID);
			if (currentScenarioState?.Stats?.m_PlayerStats != null && currentScenarioState.Stats.m_PlayerStats.TryGetValue(playerActor.CharacterClass.ID, out var value2))
			{
				List<ScenarioResult.CharacterResult.KillStat> collection = value2.Kills.Select((CPlayerStatsKill killStat) => new ScenarioResult.CharacterResult.KillStat(killStat.ActorWhoDiedClassID, killStat.Items.ToList())).ToList();
				heroStats.KillStats.AddRange(collection);
			}
			battleResultDTO.IndividualRewards.Add(heroStats);
		}
		battleResultDTO.InspirationFromFromMissingCharacters = (int)(SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.Settings.WinScenarionBaseInspiration - ScenarioManager.Scenario.AllPlayers.Count);
		ScenarioDef value3 = defRepository.Scenarios.First((KeyValuePair<ScenarioID, ScenarioDef> pair) => pair.Value.LevelID == AdventureState.MapState.CurrentMapScenarioState.ScenarioID || pair.Value.AlternativeLevelID == AdventureState.MapState.CurrentMapScenarioState.ScenarioID).Value;
		UpdatePersonalQuest(defRepository, campaignController, battleResultDTO.IndividualRewards, outcome == EResult.Win, ScenarioManager.CurrentScenarioState.ScenarioSetup.WasCompleted, value3);
		campaignController.Cheat_EndResult();
		CalcAchievementBadges(battleResultDTO);
		battleResultDTO.InspirationFromScenario = GetScenarioInspiration();
		battleResultDTO.Challenges = GetChallenges(outcome);
		battleResultDTO.ChallengesTooltipParams = GetChallengeSlotTooltipParams(outcome);
		battleResultDTO.Effects = GetOtherEffects(outcome);
		return battleResultDTO;
	}

	private static int EXPtoCharacterLevel(CampaignController controller, int exp)
	{
		if (controller == null)
		{
			return 0;
		}
		List<uint> characterExperienceTable = controller.State.GameMode.CharacterExperienceTable;
		for (int i = 0; i < characterExperienceTable.Count; i++)
		{
			if (characterExperienceTable[i] > exp)
			{
				return i;
			}
		}
		return characterExperienceTable.Count;
	}

	private static RangedValueDTO GetProjectedLevelXP(CampaignController controller, int characterXP)
	{
		if (controller == null)
		{
			return default;
		}
		uint num = (uint)EXPtoCharacterLevel(controller, characterXP);
		uint experienceForLevel = controller.CharacterController.GetExperienceForLevel(num);
		uint num2 = ((num >= controller.CharacterController.GetMaxLevel()) ? experienceForLevel : controller.State.GameMode.CharacterExperienceTable[(int)num]);
		return new RangedValueDTO((int)(characterXP - experienceForLevel), (int)(num2 - experienceForLevel));
	}

	private static void UpdatePersonalQuest(DefRepository repo, CampaignController campaignController, List<BattleResultDTO.HeroStats> characters, bool scenarioWon, bool scenarioIsAlreadyCompleted, ScenarioDef scenarioDef)
	{
		CampaignState frosthavenCampaign = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign;
		foreach (BattleResultDTO.HeroStats heroData in characters)
		{
			CharacterState characterState = frosthavenCampaign.PartyCharacters.SingleOrDefault((CharacterState state) => state.ClassID.ID == heroData.ClassID);
			heroData.CharacterDTO = new CharacterDataDTO(characterState, null, null, null, campaignController.CharacterController.GetLevelFromProsperity());
			campaignController.CharacterController.GetExperienceProgress(characterState, out var currentProgress, out var maxProgress);
			heroData.CharacterDTO.Experience = new RangedValueDTO((int)currentProgress, (int)maxProgress);
			heroData.ProjectedLevel = EXPtoCharacterLevel(campaignController, (int)(characterState.Experience + heroData.TotalXP));
			heroData.ProjectedLevelExperience = GetProjectedLevelXP(campaignController, (int)(characterState.Experience + heroData.TotalXP));
			CharacterState characterState2 = CloneCharacterState(characterState, repo);
			OutpostResources outpostResources = heroData.Materials.ToMaterialResources();
			characterState2.Resources += outpostResources;
			characterState2.Gold += (uint)heroData.Gold;
			if (heroData.XP + heroData.BonusXP > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.GainExperience, (uint)heroData.ExperienceChangeFromAbilityCards);
			}
			if (heroData.LootCardTypesDrawn.Lumber > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootLumberLootCard, (uint)heroData.LootCardTypesDrawn.Lumber);
			}
			if (heroData.ResourceChangeFromLootCards.Flamefruit > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootFlamefruitFromLootCard, (uint)heroData.ResourceChangeFromLootCards.Flamefruit);
			}
			if (heroData.ResourceChangeFromLootCards.Arrowvine > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootArrowvineFromLootCard, (uint)heroData.ResourceChangeFromLootCards.Arrowvine);
			}
			if (heroData.ResourceChangeFromLootCards.Axenut > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootAxenutFromLootCard, (uint)heroData.ResourceChangeFromLootCards.Axenut);
			}
			if (heroData.ResourceChangeFromLootCards.Corpsecap > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootCorpsecapFromLootCard, (uint)heroData.ResourceChangeFromLootCards.Corpsecap);
			}
			if (heroData.ResourceChangeFromLootCards.Rockroot > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootRockrootFromLootCard, (uint)heroData.ResourceChangeFromLootCards.Rockroot);
			}
			if (heroData.ResourceChangeFromLootCards.Snowthistle > 0)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.LootSnowthistleFromLootCard, (uint)heroData.ResourceChangeFromLootCards.Snowthistle);
			}
			foreach (IGrouping<string, ScenarioResult.CharacterResult.KillStat> item in from x in heroData.KillStats
				group x by x.monsterClassID)
			{
				_ = item.Key;
				foreach (ScenarioResult.CharacterResult.KillStat item2 in item)
				{
					campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.KillMonster, 1u, item2.monsterClassID, item2.itemsUsed);
				}
			}
			if (scenarioWon)
			{
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.CapturePet, (uint)heroData.CapturedPets.Count);
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.HaveAlliesExhaustInWonScenarios, (uint)characters.Count((BattleResultDTO.HeroStats x) => x != heroData && x.Exhausted));
				if (heroData.BattleGoal != null && heroData.BattleGoal.IsChecked)
				{
					BattleGoalDef battleGoalDef = repo.BattleGoals[new BattleGoalID(heroData.BattleGoal.Id)];
					uint amount = battleGoalDef.PerkChecksReward;
					if (characters.Count >= 4 && battleGoalDef.PerkChecksRewardFullyParty != 0)
					{
						amount = battleGoalDef.PerkChecksRewardFullyParty;
					}
					campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.EarnPerkFromBattleGoal, amount);
				}
				if (!scenarioIsAlreadyCompleted && (scenarioDef.RequiresBoat || scenarioDef.RequiresClimbingGear || scenarioDef.RequiresSled))
				{
					campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.CompleteBoatSledRopeScenario, 1u);
				}
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.CompleteScenario, 1u, scenarioDef.ID.ID);
				campaignController.CharacterController.Replicate_IncrementPersonalQuest(characterState2, PersonalQuestDef.IncrementType.CompleteChallenge, (uint)ScenarioManager.CurrentScenarioState.Challenges.Count);
			}
			heroData.PersonalQuest = ((characterState2.PersonalQuestDef != null) ? new CharacterPersonalQuestDTO(characterState2, campaignController) : null);
			heroData.CanRetire = campaignController.CharacterController.IsPersonalQuestCompleted(characterState2);
		}
	}

	private static CharacterState CloneCharacterState(CharacterState stateToCopyFrom, DefRepository repository)
	{
		CharacterState characterState = new CharacterState(stateToCopyFrom.ClassDef, repository, stateToCopyFrom.ID)
		{
			PartyState = CharacterPartyStateType.CreationProcess
		};
		if (stateToCopyFrom.PersonalQuest != null)
		{
			characterState.PersonalQuest = new PersonalQuestState(stateToCopyFrom.PersonalQuest.ID, repository, null, isFakeCopy: true);
			characterState.PersonalQuest.ProgressTrackers = stateToCopyFrom.PersonalQuest.ProgressTrackers.ToArray();
		}
		characterState.CompletedMasteryIDs.AddRange(stateToCopyFrom.CompletedMasteryIDs.ToList());
		characterState.PerkIDs.AddRange(stateToCopyFrom.PerkIDs.ToList());
		characterState.AttackModifierIDs.AddRange(stateToCopyFrom.AttackModifierIDs.ToList());
		characterState.Name = stateToCopyFrom.Name;
		characterState.Experience = stateToCopyFrom.Experience;
		characterState.Level = stateToCopyFrom.Level;
		characterState.PerkChecks = stateToCopyFrom.PerkChecks;
		characterState.PerkMarks = stateToCopyFrom.PerkMarks;
		characterState.Gold = stateToCopyFrom.Gold;
		characterState.Resources = stateToCopyFrom.Resources;
		characterState.UnusedCardIDs.AddRange(stateToCopyFrom.UnusedCardIDs.ToList());
		characterState.SelectedCardIDs.AddRange(stateToCopyFrom.SelectedCardIDs.ToList());
		characterState.EquipedItemIDs.AddRange(stateToCopyFrom.EquipedItemIDs.ToList());
		characterState.TuckedItemIDs.AddRange(stateToCopyFrom.TuckedItemIDs.ToList());
		return characterState;
	}

	private static List<GoalDTO> GetMasteries(DefRepository repo, CharacterState characterState, CampaignController controller, EResult outcome, CPlayerActor playerActor)
	{
		List<Tuple<string, CBattleGoalState>> masteryBattleGoalStates = AdventureState.MapState.CurrentMapScenarioState.MasteryBattleGoalStates;
		List<GoalDTO> list = new List<GoalDTO>();
		if (controller.HasUnfinishedPrologue())
		{
			return list;
		}
		MasteryID[] masteryIDs = characterState.ClassDef.MasteryIDs;
		for (int i = 0; i < masteryIDs.Length; i++)
		{
			MasteryID masteryID = masteryIDs[i];
			MasteryDef value = repo.MasteryDefs.FirstOrDefault((KeyValuePair<MasteryID, MasteryDef> pair) => pair.Key == masteryID).Value;
			bool flag = characterState.CompletedMasteries.Contains(value);
			CBattleGoalState cBattleGoalState = masteryBattleGoalStates.FirstOrDefault((Tuple<string, CBattleGoalState> tuple) => tuple.Item2.ID == masteryID.ID)?.Item2;
			bool flag2 = cBattleGoalState != null && IsBattleGoalComplete(cBattleGoalState, outcome, playerActor);
			GoalDTO item = new GoalDTO(value, flag | flag2)
			{
				Highlihed = flag2
			};
			list.Add(item);
		}
		return list;
	}

	private static void CalcAchievementBadges(BattleResultDTO result)
	{
		System.Random rng = new System.Random(ScenarioManager.CurrentScenarioState.ID.GetHashCode());
		Dictionary<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> dictionary = new Dictionary<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>>();
		foreach (BattleResultDTO.AchievementBadge.BadgeCategory value in Enum.GetValues(typeof(BattleResultDTO.AchievementBadge.BadgeCategory)))
		{
			if (value == BattleResultDTO.AchievementBadge.BadgeCategory.Consolation)
			{
				continue;
			}
			foreach (BattleResultDTO.HeroStats individualReward in result.IndividualRewards)
			{
				HeroBattleStatDTO.BattleStat statByCategory = individualReward.BattleStats.GetStatByCategory(value);
				if (!(statByCategory == null) && statByCategory.HighScore)
				{
					if (!dictionary.ContainsKey(value))
					{
						dictionary.Add(value, new List<BattleResultDTO.HeroStats>());
					}
					dictionary[value].Add(individualReward);
				}
			}
		}
		foreach (KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> badge in dictionary)
		{
			if (badge.Value.Count <= 1)
			{
				continue;
			}
			BattleResultDTO.HeroStats heroStats = null;
			int num = 0;
			foreach (BattleResultDTO.HeroStats hero in badge.Value)
			{
				int num2 = dictionary.Where((KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> pair) => pair.Key != badge.Key).Count((KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> pair) => pair.Value.Contains(hero));
				if (num2 > num)
				{
					heroStats = hero;
					num = num2;
				}
			}
			if (heroStats == null)
			{
				continue;
			}
			badge.Value.Clear();
			badge.Value.Add(heroStats);
			foreach (KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> item in dictionary.Where((KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> pool) => pool.Key == badge.Key))
			{
				item.Value.Remove(heroStats);
			}
		}
		List<int> list = new List<int>();
		foreach (BattleResultDTO.HeroStats r in result.IndividualRewards)
		{
			KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> keyValuePair = dictionary.FirstOrDefault((KeyValuePair<BattleResultDTO.AchievementBadge.BadgeCategory, List<BattleResultDTO.HeroStats>> pair) => pair.Value.Contains(r));
			if (keyValuePair.Value == null || keyValuePair.Value.Count == 0)
			{
				BattleResultDTO.AchievementBadge.BadgeCategory badgeCategory2 = BattleResultDTO.AchievementBadge.BadgeCategory.Consolation;
				int num3 = rng.Range(1, 12);
				while (list.Contains(num3))
				{
					num3 = rng.Range(1, 12);
				}
				list.Add(num3);
				r.Badge = new BattleResultDTO.AchievementBadge
				{
					Category = BattleResultDTO.AchievementBadge.BadgeCategory.Consolation,
					Icon = UIInfoTools.Instance.GetDebriefingBadgeIcon(badgeCategory2.ToString()),
					NameKey = $"HeroPerformanceBadge_{badgeCategory2}_{num3:D3}"
				};
			}
			else
			{
				BattleResultDTO.AchievementBadge.BadgeCategory key = keyValuePair.Key;
				r.Badge = new BattleResultDTO.AchievementBadge
				{
					Category = key,
					Icon = UIInfoTools.Instance.GetDebriefingBadgeIcon(key.ToString()),
					NameKey = $"HeroPerformanceBadge_{key}_{rng.Range(1, 10):D3}"
				};
			}
		}
	}

	private static bool IsBattleGoalComplete(CBattleGoalState battleGoal, EResult result, CPlayerActor playerActor)
	{
		if (result != EResult.Win && !playerActor.CanBattleGoalsCompleteOnLoseScenario())
		{
			return false;
		}
		CUnlockConditionState battleGoalConditionState = battleGoal.BattleGoalConditionState;
		if (battleGoalConditionState.NegativeCondition)
		{
			return battleGoalConditionState.TotalConditionsAndTargets >= battleGoalConditionState.CurrentProgress && !battleGoalConditionState.Failed;
		}
		return battleGoalConditionState.TotalConditionsAndTargets <= battleGoalConditionState.CurrentProgress && !battleGoalConditionState.Failed;
	}

	private static int GetScenarioInspiration()
	{
		if (ScenarioManager.CurrentScenarioState.ScenarioSetup.WasCompleted)
		{
			return 0;
		}
		FHScenarioSetup scenarioSetup = ScenarioManager.CurrentScenarioState.ScenarioSetup;
		if (!YMLDefRepository.Repo.DefRepository.Scenarios.TryGetValue(new ScenarioID(scenarioSetup.ScenarioID), out var value))
		{
			return 0;
		}
		int conclusionIndex = ScenarioManager.CurrentScenarioState.GetConclusionIndex();
		return value.Conclusions[conclusionIndex].Effects.Where((CampaignEffect effect) => effect.Type == CampaignEffectType.GainInspiration).Sum((CampaignEffect effect) => effect.GetInt(0));
	}

	private static int GetScenarioLevelXP(EResult outcome, CampaignController campaignController)
	{
		if (outcome != EResult.Win)
		{
			return 0;
		}
		return campaignController.ScenarioController.GetScenarioLevelSetting(campaignController.ScenarioController.GetScenarioLevel()).BonusExperience;
	}

	private static int GetFavorXP(EResult outcome, CPlayerActor playerActor)
	{
		if (outcome != EResult.Win && !playerActor.CanBattleGoalsCompleteOnLoseScenario())
		{
			return 0;
		}
		bool flag = false;
		foreach (Tuple<string, CBattleGoalState> chosenBattleGoalState in AdventureState.MapState.CurrentMapScenarioState.ChosenBattleGoalStates)
		{
			string item = chosenBattleGoalState.Item1;
			CBattleGoalState item2 = chosenBattleGoalState.Item2;
			if (!(playerActor.Class.ID != item))
			{
				CUnlockConditionState battleGoalConditionState = item2.BattleGoalConditionState;
				flag = ((!battleGoalConditionState.NegativeCondition) ? (battleGoalConditionState.TotalConditionsAndTargets <= battleGoalConditionState.CurrentProgress && !battleGoalConditionState.Failed) : (battleGoalConditionState.TotalConditionsAndTargets >= battleGoalConditionState.CurrentProgress && !battleGoalConditionState.Failed));
				break;
			}
		}
		if (!flag)
		{
			return 0;
		}
		return (int)SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.ActiveScenario.Modifiers.Where((ScenarioModifier x) => x.Type == ScenarioModifierType.GanExperienceOnBattlegoal).Sum((ScenarioModifier x) => Convert.ToUInt32(x.Param1));
	}

	private static int GetChallengesXP(EResult outcome)
	{
		if (outcome != EResult.Win)
		{
			return 0;
		}
		CampaignState frosthavenCampaign = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign;
		return frosthavenCampaign.ActiveScenario.SelectedChallengeIDs.Count * (int)frosthavenCampaign.Settings.ExperiencePerScenarioChallenge;
	}

	private static int GetScenarioXP(EResult outcome)
	{
		if (outcome != EResult.Win)
		{
			return 0;
		}
		if (ScenarioManager.CurrentScenarioState.ScenarioSetup.WasCompleted)
		{
			return 0;
		}
		FHScenarioSetup scenarioSetup = ScenarioManager.CurrentScenarioState.ScenarioSetup;
		if (!YMLDefRepository.Repo.DefRepository.Scenarios.TryGetValue(new ScenarioID(scenarioSetup.ScenarioID), out var value))
		{
			return 0;
		}
		int conclusionIndex = ScenarioManager.CurrentScenarioState.GetConclusionIndex();
		return value.Conclusions[conclusionIndex].Effects.Where((CampaignEffect effect) => effect.Type == CampaignEffectType.AllCharactersGainExperience).Sum((CampaignEffect effect) => effect.GetInt(0));
	}

	private static ItemDataDTO CItemToDTO(CItem item, DefRepository repo)
	{
		return new ItemDataDTO
		{
			Icon = UIInfoTools.Instance.GetItemMiniSprite(item.YMLData.Art),
			NameKey = item.Name,
			NameLocalized = LocalizationManager.GetTranslation(item.Name),
			DescriptionLocalized = (item.Name.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(item.Name.Substring(0, item.Name.Length - 1) + "2")),
			ID = (FHGlue.GetScenarioItemDef(item, repo)?.ID ?? default(ItemID))
		};
	}

	private static List<ItemDataDTO> GetCapturedPets(DefRepository repo, string characterID)
	{
		List<ItemDataDTO> list = new List<ItemDataDTO>();
		foreach (var pair in ScenarioManager.CurrentScenarioState.CapturedEnemies)
		{
			if (!(pair.characterID != characterID))
			{
				CItem cItem = ScenarioRuleClient.SRLYML.PetCards.SingleOrDefault((ItemCardYMLData x) => x.ID == pair.petID)?.GetItem;
				if (cItem != null)
				{
					list.Add(CItemToDTO(cItem, repo));
				}
			}
		}
		return list;
	}

	public static List<SuggestibleSlotDTO> GetChallenges(EResult outcome)
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		if (outcome == EResult.Lose || outcome == EResult.Resign)
		{
			return list;
		}
		foreach (CChallenge challenge in ScenarioManager.CurrentScenarioState.Challenges)
		{
			list.Add(new SuggestibleSlotDTO
			{
				NameLiteral = LocalizationManager.GetTranslation(challenge.NameLocKey),
				InfoLiteral = GetChallengeDescription(challenge),
				FooterLiteral = FHUILocFormatting.PopulateParams("GUI_TOWNGUARD_PERK_COST_SHORT", 1),
				IsSelected = (outcome == EResult.Win),
				Image = UIInfoTools.Instance.GetChallengesSprite(challenge.Name)
			});
		}
		return list;
	}

	public static List<OutpostEventDTO.Effect> GetOtherEffects(EResult outcome)
	{
		List<CampaignEffect> list = new List<CampaignEffect>();
		CampaignState? campaignState = JsonConvert.DeserializeObject<CampaignState>(JsonConvert.SerializeObject(SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign));
		DefRepository defRepository = YMLDefRepository.Repo.DefRepository;
		campaignState.ResolveDefs(defRepository);
		new CampaignController(campaignState, defRepository).Cheat_ApplyEffects(PartyAdventureData.GetOrmiEffects(outcome), list);
		OutpostEventsService service = new OutpostEventsService(ScenarioManager.CurrentScenarioState);
		return (from e in service.CombineCampaignEffects(list)
			select service.GetFormattedEffect(e)).ToList();
	}

	public static List<SuggestibleSlotDTO> GetFavors()
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		List<ScenarioFavorToBeUsed> list2 = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign?.ActiveScenario?.FavorsToUse;
		if (list2 == null)
		{
			return list;
		}
		foreach (ScenarioFavorType favorType in list2.Select((ScenarioFavorToBeUsed f) => f.type).Distinct())
		{
			FavorConfigUI favorConfig = UIInfoTools.Instance.GetFavorConfig(favorType);
			int num = list2.Count((ScenarioFavorToBeUsed f) => f.type == favorType);
			list.Add(new SuggestibleSlotDTO
			{
				NameLiteral = LocalizationManager.GetTranslation(favorConfig.Name) + ((num > 1) ? $" x {num}" : string.Empty),
				InfoLiteral = LocalizationManager.GetTranslation(favorConfig.Description),
				Image = favorConfig.Icon
			});
		}
		return list;
	}

	public static List<UIPrefabTooltipTarget.TooltipInitParams> GetChallengeSlotTooltipParams(EResult outcome)
	{
		List<UIPrefabTooltipTarget.TooltipInitParams> list = new List<UIPrefabTooltipTarget.TooltipInitParams>();
		foreach (CChallenge challenge in ScenarioManager.CurrentScenarioState.Challenges)
		{
			UIPrefabTooltipTarget.TooltipInitParams item = new UIPrefabTooltipTarget.TooltipInitParams
			{
				Prefab = null,
				OnObjectCreatedCallback = (GameObject go) =>
				{
					UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
					{
						Icon = null,
						CaptionLiteral = LocalizationManager.GetTranslation(challenge.NameLocKey),
						DescriptionLiteral = GetChallengeDescription(challenge),
						Hints = ((outcome == EResult.Win) ? FHUILocFormatting.PopulateParams("GUI_TOWNGUARD_PERK_COST_SHORT", 1) : null)
					};
					go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
				}
			};
			list.Add(item);
		}
		return list;
	}

	public static string GetChallengeDescription(CChallenge challenge)
	{
		string text = LocalizationManager.GetTranslation(challenge.DescriptionLocKey);
		switch (challenge.ID)
		{
		case 1493u:
		{
			int num2 = ScenarioManager.CurrentScenarioState?.Level ?? 0;
			text = string.Format(text, Math.Ceiling((float)num2 / 2f).ToString());
			break;
		}
		case 1497u:
		{
			int num = ScenarioManager.CurrentScenarioState?.Level ?? 0;
			text = string.Format(text, (1.0 + Math.Ceiling((float)num / 4f)).ToString());
			break;
		}
		case 1510u:
		{
			int count = ScenarioManager.CurrentScenarioState.Players.Count;
			text = string.Format(text, count);
			break;
		}
		}
		return text;
	}

	private static HeroBattleStatDTO GetBattleStats(string classID)
	{
		return HeroBattleStatDTO.Populate(classID, ScenarioManager.CurrentScenarioState.Stats, Party.LastScenarioStats);
	}

	private static List<PartySlotDTO> GetPartySlotsData()
	{
		List<PartySlotDTO> list = new List<PartySlotDTO>();
		foreach (CPlayerActor item2 in Singleton<FHScenarioViewController>.InstanceFast?.GetService<PlayerViewService>()?.GetPartySlotOrderedPlayerActors()?.ToList() ?? ScenarioManager.Scenario.AllPlayers)
		{
			PartySlotDTO item = new PartySlotDTO
			{
				Character = item2.CharacterClass.CharacterModel,
				Name = LocalizationManager.GetTranslation(item2.ActorLocKey()),
				Illustration = UIInfoTools.Instance.GetCharacterConfigUI(item2.CharacterClass.CharacterModel).InfoIllustration,
				Level = (uint)item2.Level
			};
			list.Add(item);
		}
		while (list.Count < 4)
		{
			list.Add(PartySlotDTO.Empty());
		}
		return list;
	}

	public static string GetCurrentScenarioNameKey()
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return null;
		}
		int result = 0;
		if (ScenarioManager.CurrentScenarioState.ScenarioSetup != null && ScenarioManager.CurrentScenarioState.ScenarioSetup.ScenarioID != 0)
		{
			result = (int)ScenarioManager.CurrentScenarioState.ScenarioSetup.ScenarioID;
		}
		else
		{
			string text = ScenarioManager.CurrentScenarioState.ID.Split("_").Skip(1).FirstOrDefault();
			if (text != null)
			{
				int.TryParse(text, out result);
			}
		}
		return $"FS_{result}_NAME";
	}

	public static void UpdateBattleGoals(EResult result)
	{
		if (AdventureState.MapState != null)
		{
			if (result == EResult.Win || ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor x) => x.CanBattleGoalsCompleteOnLoseScenario()))
			{
				AdventureState.MapState.CurrentMapScenarioState?.CheckBattleGoals();
			}
			string soloID = "";
			AdventureState.MapState.SoloID = soloID;
		}
	}

	public static void CleanUI()
	{
		(AdventureState.MapState?.MapParty)?.LastScenarioStats.Clear();
	}

	public static void UpdateStats(StatsState stats, List<CMapCharacter> characters, List<CPlayerStatsScenario> lastScenarioStats, bool won, string soloID)
	{
		int i;
		for (i = 0; i < characters.Count; i++)
		{
			CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characters[i].CharacterID);
			CPlayerStatsScenario cPlayerStatsScenario = lastScenarioStats.SingleOrDefault((CPlayerStatsScenario x) => x.CharacterID == characters[i].CharacterID);
			if (cPlayerStatsScenario != null)
			{
				CharacterStats characterStats = CalculateStats(stats, characters[i], cPlayerStatsScenario);
				if (soloID == "" || soloID == characters[i].CharacterID)
				{
					characters[i].PlayerRecords.UpdatePlayerRecord(characterStats.kills, characterStats.damageDone, characterStats.damageTaken, characterStats.healingDone, characterStats.goldLooted, characterStats.chestsLooted, characterStats.xpEarned, characterStats.itemsUsed, cPlayerStatsScenario.RoundsPlayed, !cPlayerStatsScenario.PlayerSurvivedScenario, won);
				}
			}
		}
	}

	private static CharacterStats CalculateStats(StatsState statStorage, CMapCharacter character, CPlayerStatsScenario lastScenarioStats)
	{
		return new CharacterStats(character, lastScenarioStats, statStorage.m_GoldPilesLootedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? statStorage.m_GoldPilesLootedLastScenarioByCharacter[character.CharacterID] : 0, statStorage.m_ChestsLootedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? statStorage.m_ChestsLootedLastScenarioByCharacter[character.CharacterID] : 0, statStorage.m_TotalXPEarnedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? statStorage.m_TotalXPEarnedLastScenarioByCharacter[character.CharacterID] : 0, statStorage.m_ItemsUsedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? statStorage.m_ItemsUsedLastScenarioByCharacter[character.CharacterID] : 0, statStorage.m_RandomItemBlueprintsLootedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? statStorage.m_RandomItemBlueprintsLootedLastScenarioByCharacter[character.CharacterID] : 0);
	}

	public static DebriefingTableDTO BuildBattleStatsTable(BattleResultDTO battleResults)
	{
		DebriefingTableDTO debriefingTableDTO = new DebriefingTableDTO();
		debriefingTableDTO.ColumHeaders = new List<DebriefingTableDTO.Header>(battleResults.IndividualRewards.Count);
		foreach (BattleResultDTO.HeroStats individualReward in battleResults.IndividualRewards)
		{
			DebriefingTableDTO.Header item = new DebriefingTableDTO.Header
			{
				Icon = UIInfoTools.Instance.GetCharacterConfigUI(individualReward.ClassID).InfoIllustration
			};
			debriefingTableDTO.ColumHeaders.Add(item);
		}
		debriefingTableDTO.RowHeaders = new List<DebriefingTableDTO.Header>();
		foreach (HeroBattleStatDTO.BattleStat battleStat in battleResults.IndividualRewards[0].BattleStats.GetBattleStats())
		{
			DebriefingTableDTO.Header item2 = new DebriefingTableDTO.Header
			{
				Icon = battleStat.Icon,
				Text = LocalizationManager.GetTranslation(battleStat.NameKey)
			};
			debriefingTableDTO.RowHeaders.Add(item2);
		}
		debriefingTableDTO.Cells = new DebriefingTableDTO.Cell[debriefingTableDTO.RowHeaders.Count, debriefingTableDTO.ColumHeaders.Count];
		for (int i = 0; i < battleResults.IndividualRewards.Count; i++)
		{
			int num = 0;
			foreach (HeroBattleStatDTO.BattleStat battleStat2 in battleResults.IndividualRewards[i].BattleStats.GetBattleStats())
			{
				debriefingTableDTO.Cells[num, i] = new DebriefingTableDTO.Cell
				{
					Text = $"{battleStat2.Current}",
					IsHighlighted = battleStat2.HighScore
				};
				num++;
			}
		}
		return debriefingTableDTO;
	}

	public static DebriefingTableDTO BuildHeroRewardsTable(BattleResultDTO battleResults)
	{
		DebriefingTableDTO debriefingTableDTO = new DebriefingTableDTO();
		debriefingTableDTO.ColumHeaders = new List<DebriefingTableDTO.Header>(battleResults.IndividualRewards.Count);
		foreach (BattleResultDTO.HeroStats individualReward in battleResults.IndividualRewards)
		{
			DebriefingTableDTO.Header item = new DebriefingTableDTO.Header
			{
				Icon = UIInfoTools.Instance.GetCharacterConfigUI(individualReward.ClassID).InfoIllustration
			};
			debriefingTableDTO.ColumHeaders.Add(item);
		}
		debriefingTableDTO.RowHeaders = new List<DebriefingTableDTO.Header>();
		debriefingTableDTO.RowHeaders.Add(new DebriefingTableDTO.Header
		{
			Icon = UIInfoTools.Instance.GetResourceConfig("Gold").Icon,
			Text = LocalizationManager.GetTranslation("Gold")
		});
		foreach (OutpostMaterialType item5 in OutpostResources.AllMaterialTypes())
		{
			DebriefingTableDTO.Header item2 = new DebriefingTableDTO.Header
			{
				Icon = UIInfoTools.Instance.GetResourceConfig(item5.ToString()).IconStylized,
				Text = LocalizationManager.GetTranslation(FHUILocKeys.GetMaterialLocalizationKey(item5))
			};
			debriefingTableDTO.RowHeaders.Add(item2);
		}
		List<ItemDataDTO> lootedItems = new List<ItemDataDTO>();
		lootedItems.AddRange(from itemDataDTO in battleResults.IndividualRewards.Where((BattleResultDTO.HeroStats rewards) => rewards.Items != null).SelectMany((BattleResultDTO.HeroStats rewards) => rewards.Items)
			where !lootedItems.Any((ItemDataDTO dto) => dto.ID == itemDataDTO.ID)
			select itemDataDTO);
		foreach (ItemDataDTO item6 in lootedItems)
		{
			if (item6 != null)
			{
				DebriefingTableDTO.Header item3 = new DebriefingTableDTO.Header
				{
					Icon = item6.Icon,
					Text = item6.NameLocalized
				};
				debriefingTableDTO.RowHeaders.Add(item3);
			}
		}
		debriefingTableDTO.Cells = new DebriefingTableDTO.Cell[debriefingTableDTO.RowHeaders.Count, debriefingTableDTO.ColumHeaders.Count];
		int num = 0;
		for (int num2 = 0; num2 < debriefingTableDTO.ColumHeaders.Count; num2++)
		{
			int gold = battleResults.IndividualRewards[num2].Gold;
			debriefingTableDTO.Cells[num, num2] = new DebriefingTableDTO.Cell
			{
				Text = (((gold != 0) ? gold.ToString() : "-") ?? "")
			};
		}
		num++;
		foreach (OutpostMaterialType item7 in OutpostResources.AllMaterialTypes())
		{
			for (int num3 = 0; num3 < debriefingTableDTO.ColumHeaders.Count; num3++)
			{
				int materialAmount = battleResults.IndividualRewards[num3].Materials.GetMaterialAmount(item7);
				debriefingTableDTO.Cells[num, num3] = new DebriefingTableDTO.Cell
				{
					Text = (((materialAmount != 0) ? materialAmount.ToString() : "-") ?? "")
				};
			}
			num++;
		}
		foreach (ItemDataDTO item4 in lootedItems)
		{
			for (int num4 = 0; num4 < debriefingTableDTO.ColumHeaders.Count; num4++)
			{
				bool flag = battleResults.IndividualRewards[num4].Items != null && battleResults.IndividualRewards[num4].Items.Any((ItemDataDTO dto) => dto.ID == item4.ID);
				debriefingTableDTO.Cells[num, num4] = new DebriefingTableDTO.Cell
				{
					Icon = (flag ? UIInfoTools.Instance.GetBattleStatIcon("LootedItem_Highlighted") : null),
					Text = (flag ? string.Empty : "-"),
					IsHighlighted = flag
				};
			}
			num++;
		}
		return debriefingTableDTO;
	}

	public static void BeginRetryScenario(Action onFinish)
	{
		Log.GUIInfo("OnRetryScenario");
	}

	public static void BeginReturnToMap(Action onFinish)
	{
		Log.GUIInfo("OnReturnToMap");
	}

	public static IEnumerator ShowCurtain(CanvasGroup _transitionCanvasGroup)
	{
		_transitionTween = _transitionCanvasGroup.DOFade(1f, 2f).SetEase(Ease.InOutCubic).OnComplete(() =>
		{
			_transitionTween = null;
		});
		while (_transitionTween != null)
		{
			yield return null;
		}
	}

	public static IEnumerator HideCurtain(CanvasGroup _transitionCanvasGroup)
	{
		_transitionTween = _transitionCanvasGroup.DOFade(0f, 2f).SetEase(Ease.InOutCubic).OnComplete(() =>
		{
			_transitionCanvasGroup.blocksRaycasts = false;
			_transitionTween = null;
		});
		while (_transitionTween != null)
		{
			yield return null;
		}
	}

	private static List<T> FindObjectsByTypeInAllScenes<T>() where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				for (int j = 0; j < rootGameObjects.Length; j++)
				{
					T[] componentsInChildren = rootGameObjects[j].GetComponentsInChildren<T>(includeInactive: true);
					list.AddRange(componentsInChildren);
				}
			}
		}
		return list;
	}

	public static IEnumerator SetupPartyScene()
	{
		List<Ambience> list = FindObjectsByTypeInAllScenes<Ambience>();
		if (list != null)
		{
			foreach (Ambience item in list)
			{
				item.ApplyBlend(0f, 1f);
			}
		}
		yield return LoadScenePartyScreenCrt();
		yield return PopulateHeroesCrt();
		CameraController.s_CameraController.m_Camera.enabled = false;
		CanvasGroup component = Choreographer.s_Choreographer.m_WorldSpaceCanvas.GetComponent<CanvasGroup>();
		component.alpha = 0f;
		component.interactable = false;
		ClientScenarioManager.s_ClientScenarioManager.m_Board.gameObject.SetActive(value: false);
		ProceduralScenario component2 = Choreographer.s_Choreographer.m_MapSceneRoot.GetComponent<ProceduralScenario>();
		if (component2 != null)
		{
			component2.gameObject.SetActive(value: false);
		}
	}

	public static IEnumerator SetupPartyReactions(EResult result)
	{
		Scene sceneByName = SceneManager.GetSceneByName("FHPartyView");
		if (!sceneByName.IsValid())
		{
			yield break;
		}
		FHPartyViewScene component = sceneByName.GetComponent<FHPartyViewScene>(inChildren: true);
		if (component == null)
		{
			Log.Error("Cannot findFHPartyViewScene");
			yield break;
		}
		string text = null;
		bool freezeAnimation = false;
		switch (result)
		{
		case EResult.Win:
			text = "Celebration";
			break;
		case EResult.Lose:
		case EResult.Resign:
			text = "Defeat";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			component.Play3DAnimations(text, freezeAnimation);
		}
	}

	public static IEnumerator UnfreezePartyReactionAnimators()
	{
		Scene sceneByName = SceneManager.GetSceneByName("FHPartyView");
		if (sceneByName.IsValid())
		{
			FHPartyViewScene component = sceneByName.GetComponent<FHPartyViewScene>(inChildren: true);
			if (component == null)
			{
				Log.Error("Cannot findFHPartyViewScene");
			}
			else
			{
				component.Freeze3DAnimatiors();
			}
		}
		yield break;
	}

	private static IEnumerator PopulateHeroesCrt()
	{
		Scene sceneByName = SceneManager.GetSceneByName("FHPartyView");
		if (sceneByName.IsValid())
		{
			FHPartyViewScene component = sceneByName.GetComponent<FHPartyViewScene>(inChildren: true);
			if (component == null)
			{
				Log.Error("Cannot findFHPartyViewScene");
				yield break;
			}
			FHPartyViewScene.PartySceneParams partySceneParams = new FHPartyViewScene.PartySceneParams
			{
				SlotData = GetPartySlotsData()
			};
			component.Show(partySceneParams);
		}
	}

	private static IEnumerator LoadScenePartyScreenCrt()
	{
		AsyncOperation loadOp = SceneManager.LoadSceneAsync("FHPartyView", LoadSceneMode.Additive);
		loadOp.allowSceneActivation = false;
		while (!loadOp.isDone)
		{
			if (loadOp.progress >= 0.9f)
			{
				loadOp.allowSceneActivation = true;
			}
			yield return null;
		}
		FHPartyViewScene component = SceneManager.GetSceneByName("FHPartyView").GetComponent<FHPartyViewScene>(inChildren: true);
		if (component != null)
		{
			UniversalAdditionalCameraData component2 = component.Camera.GetComponent<UniversalAdditionalCameraData>();
			if (component2 != null)
			{
				component2.cameraStack.Clear();
				component2.cameraStack.Add(UIManager.Instance.UICamera);
			}
		}
		else
		{
			Log.Error("Cannot find ISceneRoot root in scene FHPartyView");
		}
	}

	public static void UnloadScenePartyScreen()
	{
		if (SceneManager.GetSceneByName("FHPartyView").isLoaded)
		{
			SceneManager.UnloadSceneAsync("FHPartyView");
		}
	}
}

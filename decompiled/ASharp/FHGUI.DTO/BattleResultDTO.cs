using System.Collections.Generic;
using System.Linq;
using FHRL;
using UnityEngine;

namespace FHGUI.DTO;

public class BattleResultDTO
{
	public class HeroStats
	{
		public CharacterDataDTO CharacterDTO;

		public string ClassID;

		public int Gold;

		public int XP;

		public int BonusXP;

		public HeroBattleStatDTO BattleStats;

		public OutpostResources Materials;

		public List<ItemDataDTO> Items;

		public GoalDTO BattleGoal;

		public List<GoalDTO> Masteries;

		public CharacterPersonalQuestDTO PersonalQuest;

		public List<ItemDataDTO> CapturedPets;

		public bool CanRetire;

		public OutpostResources LootCardTypesDrawn;

		public OutpostResources ResourceChangeFromLootCards;

		public bool Exhausted;

		public int ExperienceChangeFromAbilityCards;

		public List<ScenarioResult.CharacterResult.KillStat> KillStats = new List<ScenarioResult.CharacterResult.KillStat>();

		public AchievementBadge Badge;

		public int ProjectedLevel;

		public RangedValueDTO ProjectedLevelExperience;

		public int TotalXP => XP + BonusXP;
	}

	public class AchievementBadge
	{
		public enum BadgeCategory
		{
			EnemiesKilled,
			DamageDealt,
			DamageTaken,
			DamageHealed,
			LootCollected,
			ChestsLooted,
			ExperienceEarned,
			ItemsUsed,
			Consolation
		}

		public BadgeCategory Category;

		public string NameKey;

		public Sprite Icon;
	}

	public List<HeroStats> IndividualRewards = new List<HeroStats>();

	public int InspirationFromLoot;

	public int InspirationFromScenario;

	public int InspirationFromFromMissingCharacters;

	public OutpostResources Herbs;

	public List<ItemDataDTO> UnlockedBlueprints;

	public List<SuggestibleSlotDTO> Challenges;

	public List<UIPrefabTooltipTarget.TooltipInitParams> ChallengesTooltipParams;

	public List<OutpostEventDTO.Effect> Effects;

	public IEnumerable<ItemDataDTO> CapturedPets => IndividualRewards.SelectMany((HeroStats rewards) => rewards.CapturedPets);
}

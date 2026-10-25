using System.Collections.Generic;
using FHRL;

public class FHScenarioResult
{
	public class Character
	{
		public string ClassID;

		public string Name;

		public int GoldChange;

		public int ExperienceChange;

		public int ExperienceChangeFromAbilityCards;

		public uint PerkChecks;

		public OutpostResources ResourceChange;

		public List<int> NewItems = new List<int>();

		public bool BattleGoalCompleted;

		public List<string> MasteryGoalsCompletedIDs = new List<string>();

		public List<ScenarioResult.CharacterResult.KillStat> KillStats = new List<ScenarioResult.CharacterResult.KillStat>();

		public OutpostResources ResourceChangeFromLootCards;

		public List<uint> CapturedPets = new List<uint>();

		public bool Exhausted;

		public OutpostResources LootCardTypesDrawn;
	}

	public string ScenarioID;

	public EResult eResult;

	public List<Character> Characters = new List<Character>();

	public List<string> LootedTreasureTableIDs = new List<string>();

	public bool RandomItemLooted;

	public uint LootedRandomSideScenarioID;

	public uint LootedScenarioID;

	public uint LootedRandomItemBlueprintID;

	public int InspirationChange;

	public int ConclusionIndex;

	public List<object> ConclusionParameters = new List<object>();

	public List<CampaignEffect> Effects = new List<CampaignEffect>();

	public List<string> TriggeredCampaignSections = new List<string>();

	public int MoraleSpent;

	public bool HasBeenProcessed;
}

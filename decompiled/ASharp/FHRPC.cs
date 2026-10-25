public static class FHRPC
{
	public static class Campaign
	{
		public static class Garden
		{
			internal static readonly string Root = "CampaignController.GardenController";

			public static readonly string PlantHerbs = Root + ".PlantHerbs";
		}

		public static class Puzzle
		{
			internal static readonly string Root = "CampaignController.PuzzleController";

			public static readonly string SolvePuzzle = Root + ".SolvePuzzle";
		}

		public static class Buildings
		{
			internal static readonly string Root = "CampaignController.BuildingsController";

			public static readonly string BuildBuilding = Root + ".BuildBuilding";

			public static readonly string UpgradeBuilding = Root + ".UpgradeBuilding";

			public static readonly string RepairBuilding = Root + ".RepairBuilding";

			public static readonly string DontRepairBuilding = Root + ".DontRepairBuilding";

			public static readonly string Rebuild = Root + ".Rebuild";

			public static readonly string FinalizeBuildingDonations = Root + ".FinalizeBuildingDonaions";

			public static readonly string CancelBuildingConstruction = Root + ".CancelBuildingConstruction";

			public static readonly string DonateMaterialsToBuilding = Root + ".DonateMaterialsToBuilding";

			public static readonly string DonateGoldToBuilding = Root + ".DonateGoldToBuilding";

			public static readonly string RefundMaterialsFromBuilding = Root + ".RefundMaterialsFromBuilding";

			public static readonly string RefundGoldFromBuilding = Root + ".RefundGoldFromBuilding";
		}

		public static class Character
		{
			internal static readonly string Root = "CampaignController.CharacterController";

			public static readonly string LearnPerk = Root + ".LearnPerk";

			public static readonly string UnlockCard = Root + ".UnlockCard";

			public static readonly string CancelCreateNewCharacter = Root + ".CancelCreateNewCharacter";

			public static readonly string SetCreationFlowPassed = Root + ".SetCreationFlowPassed";

			public static readonly string CreateCopyOfCharacter = Root + ".CreateCopyOfCharacter";

			public static readonly string CancelPersonalQuestSelection = Root + ".CancelPersonalQuestSelection";

			public static readonly string StartCreateNewCharacter = Root + ".StartCreateNewCharacter";

			public static readonly string RenameCharacter = Root + ".RenameCharacter";

			public static readonly string SelectPersonalQuest = Root + ".SelectPersonalQuest";

			public static readonly string FinishSelectPersonalQuest = Root + ".FinishSelectPersonalQuest";

			public static readonly string StartRetire = Root + ".StartRetire";

			public static readonly string CancelRetire = Root + ".CancelRetire";

			public static readonly string FinishRetire = Root + ".FinishRetire";

			public static readonly string EquipItem = Root + ".EquipItem";

			public static readonly string UnequipItem = Root + ".UnequipItem";

			public static readonly string AddCharacterFromParty = Root + ".AddCharacterFromParty";

			public static readonly string RemoveCharacterFromParty = Root + ".RemoveCharacterFromParty";

			public static readonly string DeleteCharacter = Root + ".DeleteCharacter";

			public static readonly string ResolveChoseClass = Root + ".ResolveChoseClass";

			public static readonly string SelectCard = Root + ".SelectCard";

			public static readonly string UnselectCard = Root + ".UnselectCard";

			public static readonly string ResetNewCharacterItems = Root + ".ResetNewCharacterItems";

			public static readonly string FinalizeProsperityLevelUp = Root + ".FinalizeProsperityLevelUp";

			public static readonly string Respec = Root + ".Respec";
		}

		public static class Trading
		{
			internal static readonly string Root = "CampaignController.TradingController";

			public static readonly string BuyItem = Root + ".BuyItem";

			public static readonly string SellItem = Root + ".SellItem";

			public static readonly string BuyMaterials = Root + ".BuyMaterials";

			public static readonly string SellMaterials = Root + ".SellMaterials";

			public static readonly string BuyMaterialWithInspiration = Root + ".BuyMaterialWithInspiration";
		}

		public static class Crafting
		{
			internal static readonly string Root = "CampaignController.CraftingController";

			public static readonly string CraftItem = Root + ".CraftItem";

			public static readonly string DiscoverPotion = Root + ".DiscoverPotion";

			public static readonly string Brew = Root + ".Brew";

			public static readonly string DistillItem = Root + ".DistillItem";
		}

		public static class Enchancement
		{
			internal static readonly string Root = "CampaignController.EnhancementController";

			public static readonly string Enhance = Root + ".Enhance";
		}

		public static class Attack
		{
			internal static readonly string Root = "CampaignController.AttackController";

			public static readonly string StartAttack = Root + ".StartAttack";

			public static readonly string ModifySoldiers = Root + ".ModifySoldiers";

			public static readonly string ResolveCurrentTarget = Root + ".ResolveCurrentTarget";

			public static readonly string ResolveChooseAttackModifier = Root + ".ResolveChooseAttackModifier";

			public static readonly string EndCurrentAttack = Root + ".EndCurrentAttack";
		}

		public static class Outpost
		{
			internal static readonly string Root = "CampaignController.OutpostController";

			public static readonly string ResolveGainGold = Root + ".ResolveGainGold";

			public static readonly string ResolveLoseGold = Root + ".ResolveLoseGold";

			public static readonly string ResolveLoseMaterials = Root + ".ResolveLoseMaterials";

			public static readonly string ResolveGainMaterials = Root + ".ResolveGainMaterials";

			public static readonly string ResolveLoseAnyHerbs = Root + ".ResolveLoseAnyHerbs";

			public static readonly string ResolveGainAnyHerbs = Root + ".ResolveGainAnyHerbs";

			public static readonly string ResolveGainItems = Root + ".ResolveGainItems";

			public static readonly string ResolveLoseItem = Root + ".ResolveLoseItem";

			public static readonly string TrainSoldiers = Root + ".TrainSoldiers";

			public static readonly string BlessCharacter = Root + ".BlessCharacter";

			public static readonly string AddTownGuardPerk = Root + ".AddTownGuardPerk";

			public static readonly string AcceptJobPosting = Root + ".AcceptJobPosting";

			public static readonly string HoldFestival = Root + ".HoldFestival";
		}

		public static class OutpostEvent
		{
			internal static readonly string Root = "CampaignController.OutpostEventController";

			public static readonly string ResolveEvent_TradeLumberForMetal = Root + ".ResolveEvent_TradeLumberForMetal";

			public static readonly string ResolveEvent_EverybodyChooseLoseGold = Root + ".ResolveEvent_EverybodyChooseLoseGold";

			public static readonly string ResolveEvent_EverybodyChooseLoseUpToGold = Root + ".ResolveEvent_EverybodyChooseLoseUpToGold";

			public static readonly string ResolveEvent_LoseAnySnowthistle = Root + ".ResolveEvent_LoseAnySnowthistle";

			public static readonly string ResolveEvent_LoseMaterialsEach = Root + ".ResolveEvent_LoseMaterialsEach";

			public static readonly string ResolveEvent_EverybodyChooseLoseUpToResources = Root + ".ResolveEvent_EverybodyChooseLoseUpToMaterials";

			public static readonly string ResolveEvent_WR24OptionA = Root + ".ResolveEvent_WR24OptionA";

			public static readonly string ResolveEvent_SelectBuildingsToProtect = Root + ".ResolveEvent_SelectBuildingsToProtect";

			public static readonly string ResolveCurrentEvent = Root + ".ResolveCurrentEvent";

			public static readonly string ResolveGainOneItemForParty = Root + ".ResolveGainOneItemForParty";

			public static readonly string ResolveBuyItemsAtDiscount = Root + ".ResolveBuyItemsAtDiscount";

			public static readonly string ResolveEvent_GainAnyMaterialsEach = Root + ".ResolveEvent_GainAnyMaterialsEach";

			public static readonly string ContinueResolveCurrentEvent = Root + ".ContinueResolveCurrentEvent";

			public static readonly string ResolveChooseNextCampaignSection = Root + ".ResolveChooseNextCampaignSection";

			public static readonly string ResolveChooseUnlockMandatoryScenario = Root + ".ResolveChooseUnlockMandatoryScenario";

			public static readonly string ResolveChooseSingleCharacter = Root + ".ResolveChooseSingleCharacter";

			public static readonly string ResolveChooseMultipleCharacters = Root + ".ResolveChooseMultipleCharacters";

			public static readonly string ResolveChooseAlternativeScenarioLevel = Root + ".ResolveChooseAlternativeScenarioLevel";

			public static readonly string ResolveEvent_LosePet = Root + ".ResolveEvent_LosePet";

			public static readonly string ResolveChooseBuilding = Root + ".ResolveChooseBuilding";
		}

		public static class Phase
		{
			internal static readonly string Root = "CampaignController.PhaseController";

			public static readonly string ProcessPendingSections = Root + ".ProcessPendingSections";
		}

		public static class Scenario
		{
			internal static readonly string Root = "CampaignController.ScenarioController";

			public static readonly string SelectCharacterBattleGoal = Root + ".SelectCharacterBattleGoal";

			public static readonly string StartScenario = Root + ".StartScenario";

			public static readonly string CompleteActiveScenario = Root + ".CompleteActiveScenario";

			public static readonly string AbandonActiveScenario = Root + ".AbandonActiveScenario";

			public static readonly string EnterActiveScenario = Root + ".EnterActiveScenario";

			public static readonly string StartScenarioBriefing = Root + ".StartScenarioBriefing";

			public static readonly string SetFavors = Root + ".SetFavors";

			public static readonly string DrawMonsterCardsForStrategyFavor = Root + ".DrawMonsterCardsForStrategyFavor";

			public static readonly string SelectChallenge = Root + ".SelectChallenge";

			public static readonly string DeselectChallenge = Root + ".DeselectChallenge";

			public static readonly string AddChooseAlternativeScenarioLevelAction = Root + ".AddChooseAlternativeScenarioLevelAction";

			public static readonly string ResetLinkedScenarios = Root + ".ResetLinkedScenarios";
		}

		public static class Pets
		{
			internal static readonly string Root = "CampaignController.PetController";

			public static readonly string AddPet = Root + ".AddPet";

			public static readonly string DelPet = Root + ".DelPet";

			public static readonly string BringPetToScenario = Root + ".BringPetToScenario";

			public static readonly string RemovePetFromScenario = Root + ".RemovePetFromScenario";
		}

		internal const string Root = "CampaignController";

		public static readonly string NextPhase = "CampaignController.NextPhase";

		public static readonly string ApplyEffect = "CampaignController.ApplyEffect";

		public static readonly string UpdatePartyMarkerLocation = "CampaignController.UpdatePartyMarkerLocation";
	}

	public const string StartCreateNewCharacter = "StartCreateNewCharacter";

	public const string StartCreateNewCharacterWithoutSlot = "StartCreateNewCharacterWithoutSlot";

	public const string CreateCopyOfCharacter = "CreateCopyOfCharacter";

	public const string FinishCreateCharacters = "FinishCreateCharacters";

	public const string FinishCreateCharacter = "FinishCreateCharacter";

	public const string NewPlayer = "NewPlayer";

	public const string AssignCharacterToSlot = "AssignCharacterToSlot";

	public const string StartReadyCheck = "StartReadyCheck";

	public const string CancelReadyCheck = "CancelReadyCheck";

	public const string FinishReadyCheck = "FinishReadyCheck";

	public const string SetReady = "SetReady";

	public const string ResetGoldDistribution = "ResetGoldDistribution";

	public const string UpdateGoldDistribution = "UpdateGoldDistribution";

	public const string UpdateFrosthavenGoldDistribution = "UpdateFrosthavenGoldDistribution";

	public const string SetGoldDistribution = "SetGoldDistribution";

	public const string ResetMaterialDistribution = "ResetMaterialDistribution";

	public const string ResetHerbDistribution = "ResetHerbDistribution";

	public const string SetResourceDistribution = "SetResourceDistribution";

	public const string UpdateFrosthaveResourceDistribution = "UpdateFrosthaveResourceDistribution";

	public const string UpdateFrosthavenPetDistribution = "UpdateFrosthavenPetDistribution";

	public const string UpdateFrosthaveBuildingDistribution = "UpdateFrosthaveBuildingDistribution";

	public const string UpdateResourceDistribution = "UpdateResourceDistribution";

	public const string ResetItemDistribution = "ResetItemDistribution";

	public const string UpdateItemsDistribution = "UpdateItemsDistribution";

	public const string DrawPersonalQuests = "DrawPersonalQuests";

	public const string ClearPersonalQuests = "ClearPersonalQuests";

	public const string AddPlayerUITag = "AddPlayerUITag";

	public const string AddUITag = "AddUITag";

	public const string RemovePlayerUITag = "RemovePlayerUITag";

	public const string RemoveUITag = "RemoveUITag";

	public const string ClearPlayerUITags = "ClearPlayerUITags";

	public const string ClearUITags = "ClearUITags";
}

using System;
using FHGUI.Common;
using FHGUI.Elements;
using FHGUI.Modals;
using UnityEngine;

namespace FHGUI.Modules;

[Serializable]
public struct ModulesCatalogue
{
	[Serializable]
	public struct CharacterModules
	{
		[Header("Header")]
		public CharacterDetailsScreenModule MainScreen;

		public SimpleHeaderModule Header;

		public PartyManagementModule PartyManagementSelector;

		public PartyColumnModule PartyColumn;

		[Header("Character")]
		public CharactersSelectorModule ClassesList;

		public HeroClassStatsModule HeroClassStats;

		public HeroClassStatsModule UnlockedHeroClassStats;

		public CharacterBiographyModule CharacterBio;

		public CharacterRenameModule CharacterRename;

		public CharacterPreviewModule CharacterPreview;

		public EquipmentViewModule EquipmentView;

		public RosterManagementModule Roster;

		public CharacterRetireModule RetireModule;

		public RetirementStoryModule RetirementStoryModule;

		public AttackModifiersModule AttackModifiersModule;

		public PerksInventoryModule PerksInventoryModule;

		public UnlockCharacterModule UnlockCharacterModule;

		[Header("Cards")]
		public HandCardListModule AbilitiesHandCard;

		public HandCardListModule HandCardList;

		public DeckCardListModule DeckCardList;

		public NewCardsListModule NewCardsList;

		public HandCardListModule EnchancableCardList;

		public EnhancementInventoryModule EnhancementsInventoryModule;

		public EnhancementPurchaseModule EnhancementPurchaseModule;

		[Header("Inventory")]
		public OutpostInventoryViewModule EquipmentInventoryView;

		public OutpostInventoryViewModule FullInventoryView;

		[Header("Quests/Scenario")]
		public ScenarioBriefingPartyModule ScenarioBriefingParty;

		public CharacterGoalsModule CharacterMasteries;

		public CharacterPersonalQuestModule CharacterPersonalQuest;

		public PersonalQuestSelectionModule PersonalQuestSelection;

		[Header("Shop")]
		public ShopViewModule ShopView;

		public CraftingViewModule CraftingViewModule;

		public AlchemistModule AlchemistModule;

		public AlchemistInventoryModule AlchemistInventoryModule;

		public PotionDistributionModule PotionDistributionModule;

		public PotionDiscoveryModule PotionDiscoveryModule;

		public PerkPurchaseModule PerkPurchaseModule;

		[Header("Party")]
		public ImportPartyModule ImportParty;
	}

	[Serializable]
	public struct WorldMapModules
	{
		public WorldMapModule WorldMap;

		public ScenarioListModule ScenarioList;

		public ScenarioLauncherModule ScenarioLauncher;

		public TextModule TextHint;

		public ScenarioRewardsModule ScenarioRewards;

		public StoryGoalsModule StoryGoalsModule;

		public PuzzleBookModule PuzzleBook;

		public JobPostingsViewModule JobPostingsModule;
	}

	[Serializable]
	public struct OutpostMapModules
	{
		public OutpostVillageMapModule OutpostOverviewModule;

		public BuildingInfoModule BuildingInfoModule;
	}

	[Serializable]
	public struct ScenarioBriefingModules
	{
		public UIScenarioBriefingSuggestibleList PetsList;

		public UIScenarioBriefingSuggestibleList ChallengesList;

		public BattleFavorModule BattleFavorModule;

		public UIScenarioBriefingSummeryList ChallengesSummery;

		public UIScenarioBriefingSummeryList PetsSummery;

		public UIScenarioBriefingSummeryList FavorsSummery;

		public StrategyFavorModal StrategyFavorModal;

		public DiscoveryFavorModal DiscoveryFavorModal;
	}

	[Serializable]
	public struct GenericModalModules
	{
		public GenericModalViewModule Modal;

		public GenericListModalModule ListModal;

		public ResourcePickerModalModule ResourcePickerModal;

		public StandaloneVideoModule StandaloneVideoModal;
	}

	public UIEmptyModule EmptyModule;

	public UIGeneralInfoModule GeneralInfo;

	public UIContextualTutorialModule ContextualTutorial;

	public SimpleBackgroundModule Background;

	public GenericModalModules GenericModals;

	public CharacterModules Characters;

	public ScenarioBriefingModules BriefingModules;

	public WorldMapModules WorldMap;

	public OutpostMapModules OutpostMap;

	public MainHeaderModule MainHeaderModule;

	public OutpostQuickAccessBarModule QuickAccessBar;

	public SettingsShortcutsModule SettingsShortcuts;

	public DistributionModalModule DistributionModuleLegacy;

	public DistributionModule DistributionModule;

	public InputActionHintsModule InputActionHints;

	public DemoMarketingPopupModule DemoMarketingPopup;

	public ShowCreditsPopupModule ShowCreditsPopup;

	[Header("Notifications")]
	public OutpostNotificationModule NotificationModule;

	public OutpostEventLogModule EventLogModule;

	public ToastNotificationsContainer ToastNotifications;
}

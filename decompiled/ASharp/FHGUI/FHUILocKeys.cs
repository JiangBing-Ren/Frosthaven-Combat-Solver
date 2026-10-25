using System;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI;

public static class FHUILocKeys
{
	public const string HEALTH = "Health";

	public const string GUI_SETINGS = "GUI_Settings";

	public const string GUI_BACK = "GUI_BACK";

	public const string GUI_SELECT = "GUI_SELECT";

	public const string LOC_GUI_DESELECT = "GUI_DESELECT";

	public const string GUI_SELECTING = "GUI_SELECTING";

	public const string GUI_CONTINUE = "GUI_CONTINUE";

	public const string GUI_RESET = "GUI_RESET";

	public const string GUI_COMPLETE = "GUI_COMPLETE";

	public const string LOC_GUI_NEXT = "GUI_NEXT";

	public const string LOC_GUI_HOLD_TO_SKIP = "GUI_HOLD_TO_SKIP";

	public const string GUI_OK = "GUI_OK";

	public const string GUI_YES = "YES";

	public const string GUI_NO = "NO";

	public const string GUI_SKIP = "GUI_SKIP";

	public const string LOC_GUI_PROCEED = "GUI_PROCEED";

	public const string LOC_GUI_CREATE = "GUI_CREATE";

	public const string LOC_GUI_PURCHASE = "GUI_PURCHASE";

	public const string NONLOC_MISSING_LOC_KEY = "MISSING_KEY";

	public const string LOC_GUI_CONTRIBUTE = "GUI_CONTRIBUTE";

	public const string LOC_GUI_DONE = "GUI_DONE";

	public const string LOC_GUI_CLOSE = "GUI_CLOSE";

	public const string LOC_GUI_OUTPOST = "GUI_Outpost";

	public const string LOC_GUI_INSPECT = "GUI_INSPECT";

	public const string LOC_GUI_ADVANTAGE = "Advantage";

	public const string LOC_GUI_DISADVANTAGE = "Disadvantage";

	public const string LOC_GUI_EMPTY = "GUI_EMPTY";

	public const string LOC_GUI_STAY = "GUI_STAY";

	public const string LOC_GUI_LEAVE = "GUI_LEAVE";

	public const string LOC_GUI_NARRATION_MUTED = "GUI_NARRATION_DISABLED";

	public const string LOC_GUI_NARRATION_PLAY = "GUI_NARRATION_PLAY";

	public const string LOC_GUI_NARRATION_PAUSE = "GUI_NARRATION_PAUSE";

	public const string LOC_GUI_REQUIRES_LEVEL = "GUI_REQUIRES_LEVEL";

	public const string LOC_GUI_OPEN_INTRO_CINEMATIC = "GUI_OPEN_INTRO_CINEMATIC";

	public const string CHARACTER_ONE_HERO_PER_CLASS = "GUI_ONE_HERO_PER_CLASS";

	public const string CHARACTER_CREATE = "GUI_CREATE_CHARACTER_NAME_CONFIRM";

	public const string CHARACTER_DELETE = "GUI_DELETE_CHARACTER";

	public const string CHARACTER_DELETE_CONFIRMATION = "GUI_DELETE_CHARACTER_CONFIRMATION";

	public const string CHARACTER_RENAME = "GUI_RENAME";

	public const string CHARACTER_INVALID_NAME = "GUI_CHARACTER_INVALID_NAME";

	public const string CHARACTER_SELECT_PERSONAL_QUEST = "GUI_SELECT_QUEST";

	public const string CHARACTER_STARTING_CARDS = "GUI_CREATE_CHARACTER_CLASS_CARDS";

	public const string CHARACTER_PREVIEW_CARDS = "GUI_CHARACTER_PREVIEW_CARDS";

	public const string LOC_GUI_UNLOCKCARDS_CANNOTUNLOCK = "GUI_UNLOCKCARDS_CANNOTUNLOCK";

	public const string LOC_GUI_UNLOCKCARDS_CANUNLOCK = "GUI_UNLOCKCARDS_CANUNLOCK";

	public const string LOC_GUI_UNLOCKCARDS_MAXUNLOCK_REACHED = "GUI_UNLOCKCARDS_MAXUNLOCK_REACHED";

	public const string CHARACTER_UNLOCK_CARDS = "GUI_LEVELUP_NEW_CARDS";

	public const string CHARACTER_EDIT_HAND = "GUI_CHARACTER_EDIT_HAND";

	public const string CHARACTER_NOT_ENOUGH_CARDS = "GUI_CHARACTER_NOT_ENOUGH_CARDS";

	public const string CHARACTER_EQUIPMENT = "GUI_INVENTORY_ITEMS";

	public const string CHARACTER_INVENTORY = "GUI_INVENTORY";

	public const string CHARACTER_PERKS = "GUI_PERKS";

	public const string CHARACTER_LEVEL_UP = "GUI_LEVELUP_BUTTON";

	public const string CHARACTER_CAN_RETIRE = "GUI_CAN_RETIRE";

	public const string CHARACTER_RETIRED = "GUI_RETIRED";

	public const string LOC_GUI_CHARACTER_BIO_LABEL = "GUI_CHARACTER_BIO_LABEL";

	public const string LOC_GUI_CHARACTER_QUEST_LABEL = "GUI_CHARACTER_QUEST_LABEL";

	public const string LOC_GUI_CHARACTER_LEVEL = "GUI_LEVEL_NUM";

	public const string LOC_GUI_CHARACTER_MASTERIES_LABEL = "GUI_CHARACTER_MASTERIES_LABEL";

	public const string LOC_GUI_CHARACTER_RETIRE = "GUI_RETIRE";

	public const string LOC_GUI_CHARACTER_VIEW = "GUI_VIEW_CHARACTER";

	public const string LOC_GUI_CHARACTER_EDIT = "GUI_EDIT_CHARACTER";

	public const string LOC_GUI_ASSIGN_SLOT = "GUI_ASSIGN_SLOT";

	public const string LOC_GUI_CHARACTER_MAXLEVEL = "GUI_CHARACTER_MAXLEVEL";

	public const string LOC_GUI_CHARACTER_LEVEL_CU_INFO = "GUI_CHARACTER_LEVEL_CU_INFO";

	public const string LOC_GUI_CHARACTER_LEVEL_CU_PROMPT_DESC = "GUI_CHARACTER_LEVEL_CU_PROMPT_DESC";

	public const string LOC_GUI_CHARACTER_LEVEL_CU_PROMPT_TITLE = "GUI_CHARACTER_LEVEL_CU_PROMPT_TITLE";

	public const string PARTY_CREATE_TWO_CHARACTERS = "GUI_PARTY_CREATE_TWO_CHARACTERS";

	public const string PARTY_ADD_CHARACTER = "GUI_ACTION_ADD";

	public const string LOC_GUI_JOIN_PARTY = "GUI_JOIN_PARTY";

	public const string PARTY_REMOVE_CHARACTER = "GUI_LEAVE";

	public const string PARTY_IMPORT = "GUI_IMPORT_PARTY";

	public const string LOC_GUI_CHARACTER_CREATION_SELECTCLASS = "GUI_CHARACTER_CREATION_SelectClass";

	public const string LOC_GUI_CHARACTER_CREATION_CHOOSENAME = "GUI_CHARACTER_CREATION_ChooseName";

	public const string LOC_GUI_CHARACTER_CREATION_SELECTQUEST = "GUI_CHARACTER_CREATION_SelectQuest";

	public const string LOC_GUI_CHARACTER_CREATION_SELECTEQUIPMENT = "GUI_CHARACTER_CREATION_SelectEquipment";

	public const string LOC_GUI_CHARACTER_CREATION_CONFIRMATION_CAPTION = "GUI_CHARACTER_CREATION_CONFIRMATION_CAPTION";

	public const string LOC_GUI_CHARACTER_CREATION_CONFIRMATION_DESCRIPTION = "GUI_CHARACTER_CREATION_CONFIRMATION_DESCRIPTION";

	public const string LOC_GUI_HERO_STAT_MELEE_NAME = "GUI_HERO_STAT_Melee_Name";

	public const string LOC_GUI_HERO_STAT_DEFENSE_NAME = "GUI_HERO_STAT_Defense_Name";

	public const string LOC_GUI_HERO_STAT_CONTROL_NAME = "GUI_HERO_STAT_Control_Name";

	public const string LOC_GUI_HERO_STAT_SUPPORT_NAME = "GUI_HERO_STAT_Support_Name";

	public const string LOC_GUI_HERO_STAT_MOBILITY_NAME = "GUI_HERO_STAT_Mobility_Name";

	public const string LOC_GUI_HERO_STAT_RANGED_NAME = "GUI_HERO_STAT_Ranged_Name";

	public const string LOC_GUI_HERO_LEVEL_INFO = "GUI_HERO_LEVEL_INFO";

	public const string LOC_GUI_HERO_LEVEL_INFO_QUESTION = "GUI_HERO_LEVEL_INFO_QUESTION";

	public const string LOC_HERO_LEVEL_INFO_CHOICE_LABEL = "GUI_HERO_LEVEL_INFO_CHOICE_LABEL";

	public const string LOC_GUI_EQUIP = "GUI_EQUIP";

	public const string LOC_GUI_UNEQUIP = "GUI_UNEQUIP";

	public const string LOC_GUI_BUY = "GUI_BUY";

	public const string LOC_GUI_SELL = "GUI_SELL";

	public const string LOC_GUI_BUY_CONFIRMATION_TITLE = "GUI_BUY_CONFIRMATION_TITLE";

	public const string LOC_GUI_CONFIRMATION_BUYITEM = "GUI_CONFIRMATION_BUYITEM";

	public const string LOC_GUI_SELL_CONFIRMATION_TITLE = "GUI_SELL_CONFIRMATION_TITLE";

	public const string LOC_GUI_CONFIRMATION_SELLITEM_EQUIPPED = "GUI_CONFIRMATION_SELLITEM_EQUIPPED";

	public const string LOC_GUI_CONFIRMATION_SELLITEM = "GUI_CONFIRMATION_SELLITEM";

	public const string LOC_GUI_BUY_AND_EQUIP = "GUI_BUY_AND_EQUIP";

	public const string LOC_GUI_MERCHANT = "GUI_MERCHANT";

	public const string LOC_GUI_MERCHANT_PURCHASES_LIMIT = "GUI_weekly_purchases_remaining";

	public const string LOC_GUI_TRAVELLING_MERCHANT = "GUI_TRAVELLING_MERCHANT";

	public const string LOC_GUI_TRAVELLING_MERCHANT_ITEM_LIMIT = "GUI_TRAVELLING_MERCHANT_ITEM_LIMIT";

	public const string LOC_GUI_NOTIFICATION_UNASSIGNED_PLAYER_SLOT = "GUI_Notification_unassigned_player_slot";

	public const string LOC_GUI_NOTIFICATION_NEW_JOB_POSTING = "GUI_Notification_new_job_posting";

	public const string LOC_GUI_NOTIFICATION_FULL_STABLES = "GUI_NOTIFICATION_OUTPOST_STABLES_FULL_TT";

	public const string LOC_GUI_NOTIFICATION_WRECKED_BUILDING = "GUI_Notification_wrecked_building";

	public const string LOC_GUI_NOTIFICATION_EMPTY_GARDEN_PLOT = "GUI_NOTIFICATION_OUTPOST_EMPTY_GARDEN_TT";

	public const string LOC_GUI_NOTIFICATION_CONSTRUCTION_AVAILABLE = "GUI_NOTIFICATION_OUTPOST_CONSTRUCTION_AVAILABLE_TT";

	public const string LOC_GUI_NOTIFICATION_EVENT_LOG = "GUI_EventLog";

	public const string LOC_GUI_CALENDAR_UNKNOWN_EVENT = "GUI_UNKNOWN_EVENT";

	public const string LOC_GUI_NOTIFICATION_CHARACTER_LEVELUP = "GUI_NOTIFICATION_CHARACTER_LEVELUP_TT";

	public const string LOC_GUI_NOTIFICATION_BUILDING_UNLOCKED = "GUI_NOTIFICATION_OUTPOST_NEW_BUILDINGS_TT";

	public const string LOC_GUI_NOTIFICATION_BUILDING_BUILT = "GUI_NOTIFICATION_OUTPOST_BUILDINGS_BUILT_TT";

	public const string LOC_GUI_NOTIFICATION_CHARACTER_READYRETIRE = "GUI_NOTIFICATION_CHARACTER_READYRETIRE_TT";

	public const string LOC_GUI_NOTIFICATION_NEW_TRADER_ITEMS = "GUI_NOTIFICATION_OUTPOST_NEW_TRADER_ITEMS_TT";

	public const string LOC_GUI_NOTIFICATION_NEW_CRAFTING = "GUI_NOTIFICATION_OUTPOST_NEW_CRAFTING_TT";

	public const string LOC_GUI_NOTIFICATION_CHARACTER_RETIRED = "GUI_NOTIFICATION_CHARACTER_RETIRED_TT";

	public const string LOC_GUI_NOTIFICATION_UNSPENT_PERK = "GUI_NOTIFICATION_CHARACTER_UNSPENT_PERKS_TT";

	public const string LOC_GUI_NOTIFICATION_TOWNGUARD_UNSPENT_PERK = "GUI_NOTIFICATION_OUTPOST_TOWNGUARD_UNSPENT_PERKS_TT";

	public const string LOC_GUI_NOTIFICATION_CHARACTER_UNSELECTED_CARDS = "GUI_NOTIFICATION_CHARACTER_UNSELECTED_CARDS_TT";

	public const string LOC_GUI_NOTIFICATION_CHARACTER_UNEQUIPPED_ITEMS = "GUI_NOTIFICATION_CHARACTER_UNEQUIPPED_ITEMS_TT";

	public const string LOC_GUI_NOTIFICATION_SECTION_TRIGGERED = "GUI_NOTIFICATION_OUTPOST_SECTION_TRIGGERED";

	public const string LOC_GUI_EVENTLOG_QUEST_STARTED = "GUI_EVENTLOG_QUEST_STARTED";

	public const string LOC_GUI_EVENTLOG_CLASS_UNLOCKED = "GUI_EVENTLOG_CLASS_UNLOCKED";

	public const string LOC_GUI_EVENTLOG_BUILDING_UNLOCKED = "GUI_EVENTLOG_BUILDING_UNLOCKED";

	public const string LOC_GUI_EVENTLOG_ITEMS_TRADER = "GUI_EVENTLOG_ITEMS_TRADER";

	public const string LOC_GUI_EVENTLOG_NEW_CRAFTING = "GUI_EVENTLOG_NEW_CRAFTING";

	public const string LOC_GUI_EVENTLOG_HERO_RETIRED = "GUI_EVENTLOG_HERO_RETIRED";

	public const string LOC_GUI_EVENTLOG_HERO_LEVELUP = "GUI_EVENTLOG_HERO_LEVELUP";

	public const string LOC_GUI_EVENTLOG_HERO_PQ_COMPLETED = "GUI_EVENTLOG_HERO_PQ_COMPLETED";

	public const string LOC_GUI_EVENTLOG_DONATION_STARTED = "GUI_EVENTLOG_DONATION_STARTED";

	public const string LOC_GUI_EVENTLOG_DONATION_FINISHED = "GUI_EVENTLOG_DONATION_FINISHED";

	public const string LOC_GUI_EVENTLOG_DONATION_CANCELLED = "GUI_EVENTLOG_DONATION_CANCELLED";

	public const string LOC_GUI_EVENTLOG_DONATION_EVENT = "GUI_EVENTLOG_DONATION_EVENT";

	public const string LOC_GUI_EVENTLOG_CONSTRUCTION_STARTED = "GUI_EVENTLOG_CONSTRUCTION_STARTED";

	public const string LOC_GUI_EVENTLOG_CONSTRUCTION_FINISHED = "GUI_EVENTLOG_CONSTRUCTION_FINISHED";

	public const string LOC_GUI_EVENTLOG_TOWN_ATTACK_STARTED = "GUI_EVENTLOG_TOWN_ATTACK_STARTED";

	public const string LOC_GUI_EVENTLOG_BUILDING_ATTACKED = "GUI_EVENTLOG_BUILDING_ATTACKED";

	public const string LOC_GUI_EVENTLOG_BUILDING_DEFENDED = "GUI_EVENTLOG_BUILDING_DEFENDED";

	public const string LOC_GUI_EVENTLOG_BUILDING_DAMAGED = "GUI_EVENTLOG_BUILDING_DAMAGED";

	public const string LOC_GUI_EVENTLOG_BUILDING_REPAIRED = "GUI_EVENTLOG_BUILDING_REPAIRED";

	public const string LOC_GUI_EVENTLOG_BUILDING_WRECKED = "GUI_EVENTLOG_BUILDING_WRECKED";

	public const string LOC_GUI_EVENTLOG_TOWN_ATTACK_FINISHED = "GUI_EVENTLOG_TOWN_ATTACK_FINISHED";

	public const string LOC_GUI_EVENTLOG_BUILDING_REBUILT = "GUI_EVENTLOG_BUILDING_REBUILT";

	public const string LOC_GUI_EVENTLOG_GARDEN_HERBS = "GUI_EVENTLOG_GARDEN_HERBS";

	public const string LOC_GUI_EVENTLOG_PROSPERITY_GAINED = "GUI_EVENTLOG_PROSPERITY_GAINED";

	public const string LOC_GUI_EVENTLOG_PROSPERITY_LOST = "GUI_EVENTLOG_PROSPERITY_LOST";

	public const string LOC_GUI_EVENTLOG_TOWN_LEVEL_INCREASED = "GUI_EVENTLOG_TOWN_LEVEL_INCREASED";

	public const string LOC_GUI_EVENTLOG_JOBPOSTING_ADDED = "GUI_EVENTLOG_JOBPOSTING_ADDED";

	public const string LOC_GUI_EVENTLOG_PET_CAPTURED = "GUI_EVENTLOG_PET_CAPTURED";

	public const string LOC_GUI_EVENTLOG_SEASON_CHANGED = "GUI_EVENTLOG_SEASON_CHANGED";

	public const string LOC_GUI_EVENTLOG_WEEK_STARTED = "GUI_EVENTLOG_WEEK_STARTED";

	public const string LOC_GUI_BUILD_BUILDING = "GUI_Build";

	public const string LOC_GUI_UPGRADE_BUILDING = "GUI_UpgradeBuilding";

	public const string LOC_GUI_USE_BUILDING = "GUI_UseBuilding";

	public const string LOC_GUI_REBUILD_BUILDING = "GUI_RebuildBuilding";

	public const string LOC_GUI_HERB_PICKER = "GUI_herb_picker_title";

	public const string LOC_GUI_DONATE = "GUI_DONATE";

	public const string GUI_OUTPOST_SECONDBUILD_CONFIRMATION = "GUI_SECOND_BUILD_CONFIRMATION";

	public const string LOC_GUI_SECONDBUILD_CONFIRMATION_TITLE = "GUI_SECOND_BUILD_CONFIRMATION_TITLE";

	public const string LOC_GUI_SECONDBUILD_TOOLTIP = "GUI_SECOND_BUILD_TOOLTIP";

	public const string LOC_GUI_BUILDING_STATUS_DONATING = "GUI_BUILDING_STATUS_DONATING";

	public const string LOC_GUI_BUILDING_STATUS_CONSTRUCTING = "GUI_BUILDING_STATUS_CONSTRUCTING";

	public const string LOC_GUI_PROSPERITY_REQUIREMENT = "GUI_prosperity_requirement";

	public const string LOC_GUI_PROSPERITY_LEVEL_REQUIREMENT = "GUI_prosperity_level_requirement";

	public const string LOC_GUI_BUILDING_LIMIT_REACHED = "GUI_building_limit_reached";

	public const string LOC_GUI_TRADE = "GUI_Trade";

	public const string LOC_GUI_ENHANCE = "GUI_ENHANCE";

	public const string LOC_GUI_CRAFT = "GUI_CRAFT";

	public const string LOC_GUI_ACTION_HINT = "GUI_open_building_message";

	public const string LOC_GUI_ICON_METAL = "Metal";

	public const string LOC_GUI_ICON_HIDE = "Hide";

	public const string LOC_GUI_ICON_LUMBER = "Lumber";

	public const string LOC_GUI_BUILDING_NOACTIVCEEFFECT = "GUI_building_no_active_effect";

	public const string LOC_GUI_MATERIAL_DESC = "GUI_Material_Desc";

	public const string LOC_GUI_HERB_DESC = "GUI_Herb_Desc";

	public const string LOC_GUI_ICON_ARROWVINE = "Arrowvine";

	public const string LOC_GUI_ICON_AXENUT = "Axenut";

	public const string LOC_GUI_ICON_CORPSECAP = "Corpsecap";

	public const string LOC_GUI_ICON_FLAMEFRUIT = "Flamefruit";

	public const string LOC_GUI_ICON_SNOWTHISTLE = "Snowthistle";

	public const string LOC_GUI_ICON_ROCKROOT = "Rockroot";

	public const string LOC_GUI_BUILD_PARTY_LEADER_WARNING = "GUI_building_party_leader_warning";

	public const string LOC_GUI_START_SCENARIO = "GUI_START";

	public const string LOC_GUI_READY = "GUI_READY";

	public const string LOC_GUI_CANCEL = "GUI_CANCEL";

	public const string LOC_GUI_CONFIRM = "GUI_CONFIRM";

	public const string LOC_GUI_CONTINUE = "GUI_CONTINUE";

	public const string LOC_GUI_REREAD = "GUI_REREAD";

	public const string LOC_GUI_START_VOTE = "GUI_START_VOTE";

	public const string LOC_GUI_WAITING_PARTY_LEADER = "GUI_WAITING_PARTY_LEADER";

	public const string LOC_GUI_WAITING_PARTY_PLAYERS = "GUI_WAITING_PARTY_PLAYERS";

	public const string LOC_GUI_BATTLE_BRIEFING_START = "GUI_BATTLE_BRIEFING_START";

	public const string LOC_GUI_PREREQUISITE_GOALS = "GUI_BATTLE_BRIEFING_PREREQUISITE_GOALS";

	public const string LOC_GUI_PREREQUISITE_FULL_CARDHAND = "GUI_PREREQUISITE_FULL_CARDHAND";

	public const string LOC_GUI_BATTLE_GOALS = "GUI_BATTLE_GOALS";

	public const string LOC_GUI_BATTLE_GOALS_TITLE = "GUI_BATTLE_GOALS_TITLE";

	public const string LOC_GUI_BATTLE_GOALS_DESCRIPTION = "GUI_BATTLE_GOALS_DESCRIPTION";

	public const string LOC_GUI_CHALLENGES = "GUI_CHALLENGES";

	public const string LOC_GUI_CHALLENGES_TITLE = "GUI_CHALLENGES_TITLE";

	public const string LOC_GUI_CHALLENGES_DESCRIPTION = "GUI_CHALLENGES_DESCRIPTION";

	public const string LOC_GUI_PETS = "GUI_PETS";

	public const string LOC_GUI_PETS_TITLE = "GUI_PETS_TITLE";

	public const string GUI_PETS_SLOT_UNLOCK = "GUI_PETS_SLOT_UNLOCK";

	public const string GUI_PETS_EMPTY = "PETS_EMPTY";

	public const string LOC_GUI_PETS_DESCRIPTION = "GUI_PETS_DESCRIPTION";

	public const string LOC_GUI_FAVORS = "GUI_FAVORS";

	public const string LOC_GUI_FAVORS_TITLE = "GUI_FAVORS_TITLE";

	public const string LOC_GUI_FAVORS_DESCRIPTION = "GUI_FAVORS_DESCRIPTION";

	public const string LOC_GUI_SUMMARY = "GUI_SUMMARY";

	public const string LOC_GUI_SUMMARY_DESCRIPTION = "GUI_SUMMARY_DESCRIPTION";

	public const string LOC_GUI_ABANDON_QUEST = "GUI_ABANDON_QUEST";

	public const string LOC_GUI_ABANDON_CONFIRM = "GUI_ABANDON_CONFIRM";

	public const string LOC_GUI_ACTION_ADD = "GUI_ACTION_ADD";

	public const string LOC_GUI_ACTION_REMOVE = "GUI_ACTION_REMOVE";

	public const string LOC_GUI_EDIT = "GUI_EDIT";

	public const string LOC_GUI_PREREQUISITE_CANNOT_USE_POTIONS = "GUI_PREREQUISITE_CANNOT_USE_POTIONS";

	public const string LOC_MODAL_CONFIRMATION_UNLOCK_CARD_TITLE = "GUI_UNLOCK_CARD_TITLE";

	public const string LOC_MODAL_CONFIRMATION_UNLOCK_CARD_DESC = "GUI_UNLOCK_CARD_DESC";

	public const string NONLOC_SELECT_RESOURCE_TYPE = "Select resource type";

	public const string NONLOC_SELECT_MONSTER_TYPE = "Select monster type";

	public const string NONLOC_SELECT_MONSTER_CARD = "Select monster card";

	public const string NONLOC_SELECT_CHARACTER = "Select character";

	public const string NONLOC_CHOOSE_CLASS_TO_UNLOCK = "Choose class to unlock";

	public const string LOC_GUI_HANDMANAGEMENT_DESCRIPTION = "GUI_HANDMANAGEMENT_DESCRIPTION";

	public const string NONLOC_GUI_HANDMANAGEMENT_INCOMPLETEDECK = "Hand is not full. Add more cards to be able to save the changes";

	public const string NONLOC_GUI_HANDMANAGEMENT_WARNING_UNSAVEDCHANGES = "Are you sure you want to exit the screen? Unsaved changes will be lost.";

	public const string LOC_GUI_HANDMANAGEMENT_WARNING_UNSAVEDCHANGES = "GUI_HANDMANAGEMENT_WARNING_UNSAVEDCHANGES";

	public const string GUI_HANDMANAGEMENT_INVALID_TITLE = "GUI_HANDMANAGEMENT_INVALID_TITLE";

	public const string GUI_HANDMANAGEMENT_INVALID_DESCRIPTION = "GUI_HANDMANAGEMENT_INVALID_DESC";

	public const string LOC_GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_GENERIC = "GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_GENERIC";

	public const string LOC_GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_HIVE = "GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_HIVE";

	public const string LOC_GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_TITLE = "GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_TITLE";

	public const string LOC_GUI_RESPEC = "GUI_RESPEC";

	public const string LOC_GUI_RESPEC_CONFIRM = "GUI_RESPEC_CONFIRM";

	public const string NONLOC_GUI_UNLOCKCARDS_CANNOTUNLOCK = "Preview cards that can be unlocked for your hero. Level up to be unlock new cards.";

	public const string NONLOC_GUI_UNLOCKCARDS_CANUNLOCK = "Choose new cards to add to your card pool.";

	public const string NONLOC_GUI_UNLOCKCARDS_MAXUNLOCK_REACHED = "Your hero has reached the maximum level. No new cards can be added to the pool.";

	public const string NONLOC_GUI_UNLOCKCARDS_CONFIRMATION_UNLOCK = "Do you want to permanently add this card to your card pool?";

	public const string LOC_GUI_PERKS_INVENTORY_CAPTION = "GUI_PERKS_INVENTORY_CAPTION";

	public const string LOC_GUI_PERKS_CAT_LEARN = "GUI_PERKS_CAT_LEARN";

	public const string LOC_GUI_PERKS_CAT_IMPROVE = "GUI_PERKS_CAT_IMPROVE";

	public const string LOC_GUI_PERKS_CAT_FORGET = "GUI_PERKS_CAT_FORGET";

	public const string LOC_GUI_PERKS_ALL_HEROPERKS_UNLOCKED = "GUI_PERKS_ALL_HEROPERKS_UNLOCKED";

	public const string LOC_GUI_PERKS_INSUFFICIENT_PERKPOINTS = "GUI_PERKS_INSUFFICIENT_PERKPOINTS";

	public const string LOC_GUI_PERKS_POINTS_DESCRIPTION = "GUI_PERKS_POINTS_DESCRIPTION";

	public const string LOC_GUI_PERKS_POINTS_VALUE = "GUI_PERKS_POINTS_VALUE";

	public const string LOC_GUI_PERKS_OWNED = "GUI_PERKS_ALREADY_OWNED";

	public const string LOC_GUI_TOWNGUARD_PERK_COST = "GUI_TOWNGUARD_PERK_COST";

	public const string LOC_GUI_TOWNGUARD_PERK_COST_SHORT = "GUI_TOWNGUARD_PERK_COST_SHORT";

	public const string LOC_GUI_TOWNGUARD_PERKS_INVENTORY_CAPTION = "GUI_TOWNGUARD_PERKS_INVENTORY_CAPTION";

	public const string LOC_GUI_TOWNGUARD_ALL_PERKS_UNLOCKED = "GUI_TOWNGUARD_ALL_PERKS_UNLOCKED";

	public const string LOC_GUI_TOWNGUARD_POINTS_VALUE = "GUI_TOWNGUARD_POINTS_VALUE";

	public const string LOC_GUI_TOWNGUARD_LEADERONLY_UNLOCK = "GUI_TOWNGUARD_LEADERONLY_UNLOCK";

	public const string LOC_GUI_TOWNGUARD_DESCRIPTION = "GUI_TOWNGUARD_DESCRIPTION";

	public const string LOC_GUI_ATTACKMODIFIERS_BASE = "GUI_ATTACKMODIFIERS_BASE";

	public const string LOC_GUI_ATTACKMODIFIERS_SPECIAL = "GUI_ATTACKMODIFIERS_SPECIAL";

	public const string LOC_GUI_ATTACKMODIFIERS_TACTICAL = "GUI_ATTACKMODIFIERS_TACTICAL";

	public const string LOC_GUI_ATTACKMODIFIERS_BASE_TOOLTIP = "GUI_ATTACKMODIFIERS_BASE_TT";

	public const string LOC_GUI_ATTACKMODIFIERS_LABEL = "GUI_ATTACKMODIFIERS_LABEL";

	public const string LOC_GUI_ENHANCEMENTS_INVENTORY_CAPTION = "GUI_ENHANCEMENTS_INVENTORY_CAPTION";

	public const string LOC_GUI_QUEST_REQUIREMENTS_NOT_MET = "GUI_REQUIREMENTS_NOT_MET";

	public const string LOC_GUI_QUEST_COMPLETED_REWARDS = "GUI_QUEST_COMPLETED_REWARDS";

	public const string LOC_GUI_QUEST_DESCRIPTION = "GUI_PERSONALQUEST_DESCRIPTION";

	public const string LOC_GUI_RETURN_TO_OUTPOST = "GUI_ReturnToOutpost";

	public const string LOC_GUI_RETURN_TO_OUTPOST_DESC = "GUI_ReturnToOutpost_Desc";

	public const string LOC_GUI_LINKED_SCENARIO_CHOICE_HINT = "GUI_OptionalScenarioChoice_Hint";

	public const string LOC_GUI_INVALID_PARTY_SIZE = "GUI_QUESTINFO_INVALID_PARTY_SIZE";

	public const string LOC_GUI_QUEST_PARTY_MEMBER_READY_TO_RETIRE = "GUI_QUESTINFO_PARTY_MEMBER_READY_TO_RETIRE";

	public const string LOC_GUI_QUEST_CANT_BE_REPLAYED = "GUI_QUESTINFO_CANT_BE_REPLAYED";

	public const string LOC_GUI_QUESTINFO_START_LEADERONLY = "GUI_QUESTINFO_START_LEADERONLY";

	public const string LOC_GUI_QUEST_MISSING_PERSONAL_QUEST = "GUI_QUEST_MISSING_PERSONAL_QUEST";

	public const string LOC_GUI_QUEST_MISSING_CORAL_SHARDS = "GUI_QUEST_MISSING_CORAL_SHARDS";

	public const string LOC_GUI_QUEST_MISSING_BOAT = "GUI_QUEST_MISSING_BOAT";

	public const string LOC_GUI_QUEST_MISSING_SLED = "GUI_QUEST_MISSING_SLED";

	public const string LOC_GUI_QUEST_MISSING_CLIMBING_GEAR = "GUI_QUEST_MISSING_CLIMBING_GEAR";

	public const string LOC_GUI_QUEST_MONSTERS = "GUI_QUEST_MONSTERS";

	public const string LOC_GUI_QUESTINFO_REWARDS_AND_LOOT = "GUI_QUESTINFO_REWARDS_AND_LOOT";

	public const string LOC_GUI_CRAFTSMAN_INVENTORY_CAPTION = "GUI_CRAFTSMAN_NAME";

	public const string LOC_GUI_CRAFTSMAN_CRAFT_ITEM = "GUI_CRAFT_ITEM";

	public const string LOC_GUI_CRAFTSMAN_MISSING_INGREDIENTS = "GUI_CRAFTSMAN_MISSING_INGREDIENTS";

	public const string LOC_GUI_INVENTORY_ALL = "GUI_INVENTORY_SLOT_ALL";

	public const string LOC_GUI_INVENTORY_SLOT_BODY = "GUI_ITEM_SLOT_BODY";

	public const string LOC_GUI_INVENTORY_SLOT_HEAD = "GUI_ITEM_SLOT_HEAD";

	public const string LOC_GUI_INVENTORY_SLOT_LEGS = "GUI_ITEM_SLOT_LEGS";

	public const string LOC_GUI_INVENTORY_SLOT_ONEHAND = "GUI_ITEM_SLOT_ONEHAND";

	public const string LOC_GUI_INVENTORY_SLOT_TWOHAND = "GUI_INVENTORY_SLOT_TwoHandItem";

	public const string LOC_GUI_INVENTORY_SLOT_SMALLITEM = "GUI_ITEM_SLOT_SMALLITEM";

	public const string NONLOC_GUI_INVENTORY_AVAILABLE = "GUI_INVENTORY_AVAILABLE";

	public const string LOC_GUI_INVENTORY_CATEGORY_BODY = "GUI_INVENTORY_SLOT_Body";

	public const string LOC_GUI_INVENTORY_CATEGORY_HEAD = "GUI_INVENTORY_SLOT_Head";

	public const string LOC_GUI_INVENTORY_CATEGORY_LEGS = "GUI_INVENTORY_SLOT_Legs";

	public const string LOC_GUI_INVENTORY_CATEGORY_ONEHAND = "GUI_INVENTORY_SLOT_OneHandItem";

	public const string LOC_GUI_INVENTORY_CATEGORY_TWOHAND = "GUI_INVENTORY_SLOT_TwoHandItem";

	public const string LOC_GUI_INVENTORY_CATEGORY_SMALLITEM = "GUI_INVENTORY_SLOT_SmallItems";

	public const string LOC_GUI_ITEM_CATEGORY = "Item";

	public const string LOC_GUI_ITEM_CATEGORY_QUEST = "GUI_ItemType_Quest";

	public const string LOC_GUI_ITEM_CATEGORY_PET = "GUI_ItemType_Pet";

	public const string LOC_GUI_RESOURCE_LUMBER = "RESOURCE_Lumber";

	public const string LOC_GUI_RESOURCE_METAL = "RESOURCE_Metal";

	public const string LOC_GUI_RESOURCE_HIDE = "RESOURCE_Hide";

	public const string LOC_GUI_RESOURCE_ARROWVINE = "RESOURCE_Arrowvine";

	public const string LOC_GUI_RESOURCE_AXENUT = "RESOURCE_Axenut";

	public const string LOC_GUI_RESOURCE_CORPSECAP = "RESOURCE_Corpsecap";

	public const string LOC_GUI_RESOURCE_FLAMEFRUIT = "RESOURCE_Flamefruit";

	public const string LOC_GUI_RESOURCE_ROCKROOT = "RESOURCE_Rockroot";

	public const string LOC_GUI_RESOURCE_SNOWTHISTLE = "RESOURCE_Snowthistle";

	public const string LOC_GUI_RESOURCE_ANY = "RESOURCE_Any";

	public const string LOC_GUI_RESOURCE_GOLD = "Gold";

	public const string LOC_GUI_GOLD_DESC = "GUI_Gold_Desc";

	public const string LOC_GUI_ALCHEMIST_INVENTORY_CAPTION = "BUILDING_Name_35";

	public const string LOC_GUI_ALCHEMIST_POTION_NOTDISTILLABLE = "GUI_ALCHEMIST_POTION_NOTDISTILLABLE";

	public const string LOC_GUI_ALCHEMIST_UNKNOWN_RECIPE = "GUI_ALCHEMIST_UNKNOWN_RECIPE";

	public const string LOC_GUI_ALCHEMIST_UNKNOWN_RECIPE_DESCRIPTION = "GUI_ALCHEMIST_UNKNOWN_RECIPE_DESCRIPTION";

	public const string LOC_GUI_ALCHEMIST_DISTILL_POTION = "GUI_ALCHEMIST_DISTILL_POTION";

	public const string LOC_GUI_ALCHEMIST_RECIPES = "GUI_ALCHEMIST_RECIPES";

	public const string LOC_GUI_ALCHEMIST_BREW = "GUI_ALCHEMIST_BREW";

	public const string LOC_GUI_ALCHEMIST_RESEARCH = "GUI_ALCHEMIST_RESEARCH";

	public const string LOC_GUI_ALCHEMIST_DISTILL = "GUI_ALCHEMIST_DISTILL";

	public const string LOC_GUI_ALCHEMIST_DISTILL_DESCRIPTION = "ALCHEMIST_DISTILL_DESCRIPTION";

	public const string LOC_GUI_ALCHEMIST_BREW_DESCRIPTION = "GUI_ALCHEMIST_BREW_DESCRIPTION";

	public const string LOC_GUI_ALCHEMIST_DISTILL_PICKHERBHINT = "GUI_ALCHEMIST_DISTILL_PICKHERBHINT";

	public const string LOC_GUI_ALCHEMIST_COST_NOT_MET = "GUI_ALCHEMIST_COST_NOT_MET";

	public const string NONLOC_GUI_ALCHEMIST_ADVANCED_NOT_UNLOCKED = "Advanced brewing not unlocked";

	public const string LOC_GUI_SELECT_BLUEPRINT = "GUI_SELECT_BLUEPRINT";

	public const string LOC_GUI_ALCHEMIST_UNHEALTHYMIXTURE = "GUI_ALCHEMIST_UNHEALTHYMIXTURE";

	public const string LOC_GUI_ALCHEMIST_DEADLYMIXTURE = "GUI_ALCHEMIST_DEADLYMIXTURE";

	public const string LOC_GUI_ALCHEMIST_PICKHERB = "GUI_ALCHEMIST_PICKHERB";

	public const string LOC_GUI_TRAIN_SOLDIER = "GUI_TRAIN_SOLDIER";

	public const string GUI_EVENT_OUTPOST_ATTACK_COMPLETED = "GUI_EVENT_OUTPOST_ATTACK_COMPLETED";

	public const string LOC_GUI_EVENT_ROAD_SUMMER = "GUI_EVENT_ROAD_SUMMER";

	public const string LOC_GUI_EVENT_ROAD_WINTER = "GUI_EVENT_ROAD_WINTER";

	public const string LOC_GUI_EVENT_BOAT = "GUI_EVENT_BOAT";

	public const string LOC_GUI_EVENT_OUTPOST = "GUI_EVENT_OUTPOST";

	public const string LOC_GUI_EVENT_CAMPAIGN = "GUI_EVENT_CAMPAIGN";

	public const string LOC_GUI_EVENT_COMPLETED = "GUI_EVENT_COMPLETED";

	public const string LOC_GUI_OUTPOST_ATTACK_ADVANTAGE_HINT_HOST = "GUI_OUTPOST_ATTACK_ADVANTAGE_HINT_HOST";

	public const string LOC_GUI_OUTPOST_ATTACK_ADVANTAGE_HINT_CLIENT = "GUI_OUTPOST_ATTACK_ADVANTAGE_HINT_CLIENT";

	public const string LOC_GUI_VOTE = "GUI_VOTE";

	public const string LOC_GUI_VOTE_PENDING = "GUI_VOTE_PENDING";

	public const string LOC_GUI_VOTE_REVOKE = "GUI_VOTE_REVOKE";

	public const string LOC_GUI_MULTIPLAYER_CHOOSING = "GUI_MULTIPLAYER_CHOOSING";

	public const string LOC_GUI_MULTIPLAYER_WAITINGFORCHARASSIGNMENT = "GUI_MULTIPLAYER_WARNING_TEXT_WaitingForCharAssignment";

	public const string LOC_GUI_OUTPOSTDEFENCE_SUCCESS = "GUI_OUTPOSTDEFENCE_SUCCESS";

	public const string LOC_GUI_OUTPOSTDEFENCE_FAIL = "GUI_OUTPOSTDEFENCE_FAIL";

	public const string LOC_GUI_OUTPOSTDEFENCE_CANNOT_USE_SOLDIERS = "GUI_SOLDIERS_FORBIDDEN";

	public const string LOC_GUI_DEFEND = "GUI_DEFEND";

	public const string LOC_GUI_DRAW_MODIFIER = "GUI_DRAW_MODIFIER";

	public const string LOC_GUI_REPAIR_SPENDMORALE = "GUI_REPAIR_SPENDMORALE";

	public const string LOC_GUI_REPAIR_SPENDMATERIAL = "GUI_REPAIR_SPENDMATERIAL";

	public const string LOC_GUI_OUTPOSTDEFENCE_SOLDIER_DESCRIPTION = "GUI_OUTPOSTDEFENCE_SOLDIER_DESCRIPTION";

	public const string LOC_GUI_OUTPOSTDEFENCE_BUILDING_REPAIRCOST = "GUI_OUTPOSTDEFENCE_BUILDING_REPAIRCOST";

	public const string LOC_GUI_OUTPOSTDEFENCE_SUCCESS_DESC = "GUI_OUTPOSTDEFENCE_SUCCESS_DESC";

	public const string LOC_GUI_OUTPOSTDEFENCE_BUILDING_WRECKED = "GUI_OUTPOSTDEFENCE_BUILDING_WRECKED";

	public const string LOC_GUI_OUTPOSTDEFENCE_BUILDING_DAMAGED = "GUI_OUTPOSTDEFENCE_BUILDING_DAMAGED";

	public const string LOC_GUI_OUTPOSTDEFENCE_BUILDING_REPAIRED = "GUI_OUTPOSTDEFENCE_BUILDING_REPAIRED";

	public const string LOC_GUI_OUTPOST_ATTACK_SOLDIERS_DEPLETED = "GUI_OUTPOST_ATTACK_SOLDIERS_DEPLETED";

	public const string LOC_GUI_OUTPOST_ATTACK_SOLDIERS_FORBIDDEN_BARRACK_WRECKED = "GUI_OUTPOST_ATTACK_SOLDIERS_FORBIDDEN_BARRACK_WRECKED";

	public const string LOC_GUI_RETIRE_ANNOUNCE_CAPTION = "GUI_RETIRE_ANNOUNCE_CAPTION";

	public const string LOC_GUI_RETIRE_CONFIRMATION = "GUI_RETIRE_CONFIRMATION";

	public const string LOC_GUI_RETIRE_ANNOUNCE_DESCRIPTION = "GUI_RETIRE_ANNOUNCE_DESCRIPTION";

	public const string LOC_GUI_RETIRE_PERSONALQUEST_CAPTION = "GUI_SELECT_PERSONAL_QUEST";

	public const string LOC_GUI_RETIRE_COMPLETE_ADDITIONAL_QUEST_DESCRIPTION = "GUI_RETIRE_COMPLETE_ADDITIONAL_QUEST_DESCRIPTION";

	public const string LOC_GUI_RETIRE_INSUFFICIENT_INSPIRATION = "GUI_RETIRE_INSUFFUCENT_INSPIRATION";

	public const string LOC_GUI_RESOURCE_ALLOCATION_CAPTION = "GUI_RESOURCE_ALLOCATION_CAPTION";

	public const string LOC_GUI_CHARACTER_CATEGORY = "GUI_CharacterLabel";

	public const string LOC_GUI_VIEW_SOURCE = "GUI_ACTION_VIEW_SOURCE";

	public const string LOC_GUI_DEACTIVATE = "GUI_ACTION_DEACTIVATE";

	public const string GUI_WEEK = "GUI_WEEK";

	public const string GUI_YEAR = "GUI_YEAR";

	public const string GUI_SUMMER = "GUI_SUMMER";

	public const string GUI_WINTER = "GUI_Winter";

	public const string LOC_GUI_ITEM_OWNED = "GUI_ITEM_OWNED";

	public const string LOC_GUI_MISSING_RESOURCES = "GUI_MISSING_RESOURCES";

	public const string LOC_GUI_PERK_COST = "GUI_PERK_COST";

	public const string LOC_GUI_PERK_COST_SHORT = "GUI_PERK_COST_SHORT";

	public const string LOC_GUI_PERKS_DESCRIPTION = "GUI_PERKS_DESCRIPTION";

	public const string LOC_GUI_WEEKLY_LIMIT_REACHED = "GUI_weekly_limit_reached";

	public const string LOC_GUI_GENERIC_EMPTY_INVENTORY = "GUI_GENERIC_EMPTY_INVENTORY";

	public const string LOC_GUI_PURCHASE_LIMIT_REACHED = "GUI_PURCHASE_LIMIT_REACHED";

	public const string LOC_GUI_QUEST_REWARD_PERKPOINTS = "GUI_QUEST_REWARD_PerkPoints";

	public const string LOC_GUI_QUEST_REWARD_PERKCHECKS = "GUI_QUEST_REWARD_PerkCheck";

	public const string LOC_GUI_REWARD = "GUI_ADV_SIDE_SCENARIO_REWARD";

	public const string LOC_GUI_INSUFFICIENT_RESOURCES = "GUI_INSUFFICIENT_RESOURCES";

	public const string LOC_GUI_CHOOSE_CHARACTER = "GUI_CHOOSE_CHARACTER";

	public const string LOC_GUI_CHOOSE_CHARACTERS = "GUI_CHOOSE_CHARACTERS";

	public const string LOC_GUI_AVAILABLE_MATERIALS = "GUI_AVAILABLE_MATERIALS";

	public const string LOC_GUI_SELECT_RESOURCE = "GUI_SELECT_RESOURCE";

	public const string LOC_GUI_BUILDING_WRECKED = "GUI_WRECKED";

	public const string LOC_GUI_COMING_SOON = "GUI_COMING_SOON";

	public const string LOC_GUI_SOLVE_PUZZLE = "GUI_PUZZLE_SOLVE_CURRENT_PUZZLE_GOAL";

	public const string LOC_GUI_LOCKED_PUZZLE = "GUI_PUZZLE_LOCKED";

	public const string LOC_GUI_WALL = "WALL_TOOLTIP";

	public const string LOC_GUI_OBSTACLE = "OBSTACLE_TOOLTIP";

	public const string LOC_GUI_OBSTACLE_DESC = "OBSTACLE_DESC";

	public const string LOC_GUI_SPAWNER = "GUI_SPAWNER";

	public const string LOC_GUI_DIFFICULT_TERRAIN = "DIFFICULT_TERRAIN_TOOLTIP";

	public const string LOC_GUI_DIFFICULT_TERRAIN_DESC = "DIFFICULT_TERRAIN_DESC";

	public const string LOC_GUI_HAZARDOUS_TERRAIN = "HAZARDOUS_TERRAIN_TOOLTIP";

	public const string LOC_GUI_HAZARDOUS_TERRAIN_DESC = "HAZARDOUS_TERRAIN_DESC";

	public const string LOC_GUI_HAZARDOUS_DIFFICULT_TERRAIN = "HAZARDOUS_DIFFICULT_TERRAIN_TOOLTIP";

	public const string LOC_GUI_PRESSURE_PLATE = "PRESSURE_PLATE_TOOLTIP";

	public const string LOC_GUI_PRESSURE_PLATE_DESC = "PRESSURE_PLATE_DESC";

	public const string LOC_GUI_ICE_TERRAIN = "ICE_TERRAIN_TOOLTIP";

	public const string LOC_GUI_ICE_TERRAIN_DESC = "ICY_TERRAIN_DESC";

	public const string LOC_GUI_PORTAL = "PORTAL_TOOLTIP";

	public const string LOC_GUI_PORTAL_DESC = "PORTAL_DESCR_TOOLTIP";

	public const string LOC_GUI_OPEN_DOOR = "DOORWAY_TOOLTIP";

	public const string LOC_GUI_CLOSED_DOOR = "CLOSED_DOOR_TOOLTIP";

	public const string LOC_GUI_LOCKED_DOOR = "LOCKED_DOOR_TOOLTIP";

	public const string LOC_GUI_DOOR_DESC = "DOOR_DESC";

	public const string LOC_GUI_IMMOVABLE_OBSTACLE = "IMMOVABLE_OBSTACLE_DESCR_TOOLTIP";

	public const string LOC_GUI_GOAL_CHEST = "GOAL_CHEST_TOOLTIP";

	public const string LOC_GUI_GOAL_CHEST_DESC = "GOAL_CHEST_DESC";

	public const string LOC_GUI_TREASURE_CHEST = "TREASURE_CHEST_TOOLTIP";

	public const string LOC_GUI_TREASURE_CHEST_DESC = "TREASURE_CHEST_DESC";

	public const string LOC_GUI_RANDOM_ITEM = "RANDOM_ITEM_TOOLTIP";

	public const string LOC_GUI_SHADOW_PROP = "FH/SHADOW_TOOLTIP";

	public const string LOC_GUI_SHADOW_PROP_DESC = "FH/SHADOW_DESC";

	public const string LOC_GUI_TRAP_DESC = "TRAP_DESC";

	public const string LOC_GUI_DUNGEON_EXIT = "DUNGEON_EXIT_TOOLTIP";

	public const string LOC_GUI_EXIT_TILE = "GUI_Exit_Tile";

	public const string LOC_GUI_LOOT_TOKEN = "LOOT_TOKEN_TOOLTIP";

	public const string LOC_GUI_LOOT_TOKEN_DESC = "LOOT_TOKEN_DESC";

	public const string LOC_GUI_CAN_BE_LOOTED_BY = "GUI_PROP_CAN_BE_LOOTED_BY";

	public const string LOC_GUI_MOVE_COST = "MoveCost";

	public const string LOC_GUI_DAMAGE = "Damage";

	public const string LOC_GUI_HEAL = "Heal";

	public const string LOC_PROP_CREATED_BY = "GUI_PROP_CREATED_BY";

	public const string LOC_GUI_ABILIY_INNATE = "Innate";

	public const string LOC_GUI_ABILITY_MIND_CONTROL = "GUI_MIND_CONTROL";

	public const string LOC_GUI_PQ_PROGRESS = "PQ_ProgressBasic";

	public const string LOC_GUI_PQ_PROGRESS_TIMES = "PQ_ProgressTimes";

	public const string LOC_GUI_PQ_PROGRESS_TYPES = "PQ_ProgressTypes";

	public const string LOC_GUI_PQ_PROGRESS_GOLD = "PQ_ProgressGold";

	public static string GetMaterialLocalizationKey(OutpostMaterialType type)
	{
		return type switch
		{
			OutpostMaterialType.Metal => "RESOURCE_Metal", 
			OutpostMaterialType.Hide => "RESOURCE_Hide", 
			OutpostMaterialType.Lumber => "RESOURCE_Lumber", 
			_ => throw new Exception($"Material type {type} has no localization key"), 
		};
	}

	public static string GetMaterialIconLocalizationKey(OutpostMaterialType type)
	{
		return type switch
		{
			OutpostMaterialType.Metal => "Metal", 
			OutpostMaterialType.Hide => "Hide", 
			OutpostMaterialType.Lumber => "Lumber", 
			_ => throw new Exception($"Material type {type} has no icon key"), 
		};
	}

	public static string GetHerbLocalizationKey(OutpostHerbType type)
	{
		return type switch
		{
			OutpostHerbType.Arrowvine => "RESOURCE_Arrowvine", 
			OutpostHerbType.Axenut => "RESOURCE_Axenut", 
			OutpostHerbType.Corpsecap => "RESOURCE_Corpsecap", 
			OutpostHerbType.Flamefruit => "RESOURCE_Flamefruit", 
			OutpostHerbType.Rockroot => "RESOURCE_Rockroot", 
			OutpostHerbType.Snowthistle => "RESOURCE_Snowthistle", 
			OutpostHerbType.Any => "RESOURCE_Any", 
			_ => throw new Exception($"Herb type {type} has no localization key"), 
		};
	}

	public static string GetMaterialTMPIcon(OutpostMaterialType type)
	{
		return type switch
		{
			OutpostMaterialType.Lumber => "Lumber", 
			OutpostMaterialType.Metal => "Metal", 
			OutpostMaterialType.Hide => "Hide", 
			_ => throw new Exception($"Material type {type} has no TMP icon localization key"), 
		};
	}

	public static string GetHerbTMPIcon(OutpostHerbType type)
	{
		return type switch
		{
			OutpostHerbType.Arrowvine => "Arrowvine", 
			OutpostHerbType.Axenut => "Axenut", 
			OutpostHerbType.Corpsecap => "Corpsecap", 
			OutpostHerbType.Flamefruit => "Flamefruit", 
			OutpostHerbType.Rockroot => "Rockroot", 
			OutpostHerbType.Snowthistle => "Snowthistle", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static string GetItemSlotKey(ItemEquipType slot)
	{
		return slot switch
		{
			ItemEquipType.Small => "GUI_ITEM_SLOT_SMALLITEM", 
			ItemEquipType.Head => "GUI_ITEM_SLOT_HEAD", 
			ItemEquipType.Body => "GUI_ITEM_SLOT_BODY", 
			ItemEquipType.Feet => "GUI_ITEM_SLOT_LEGS", 
			ItemEquipType.OneHandWeapon => "GUI_ITEM_SLOT_ONEHAND", 
			ItemEquipType.TwoHandWeapon => "GUI_ITEM_SLOT_ONEHAND", 
			_ => throw new ArgumentOutOfRangeException("slot", slot, null), 
		};
	}

	public static string GetItemSlotKey(CItem.EItemSlot slot)
	{
		return slot switch
		{
			CItem.EItemSlot.None => "", 
			CItem.EItemSlot.Head => "GUI_ITEM_SLOT_HEAD", 
			CItem.EItemSlot.Body => "GUI_ITEM_SLOT_BODY", 
			CItem.EItemSlot.Legs => "GUI_ITEM_SLOT_LEGS", 
			CItem.EItemSlot.OneHand => "GUI_ITEM_SLOT_ONEHAND", 
			CItem.EItemSlot.TwoHand => "GUI_INVENTORY_SLOT_TwoHandItem", 
			CItem.EItemSlot.SmallItem => "GUI_ITEM_SLOT_SMALLITEM", 
			CItem.EItemSlot.QuestItem => "GUI_ItemType_Quest", 
			CItem.EItemSlot.Pet => "GUI_ItemType_Pet", 
			_ => throw new ArgumentOutOfRangeException("slot", slot, null), 
		};
	}

	public static string GetActorTypeKey(CActor.EType actorType)
	{
		return actorType switch
		{
			CActor.EType.Player => string.Empty, 
			CActor.EType.Enemy => "GUI_ENEMY", 
			CActor.EType.HeroSummon => string.Empty, 
			CActor.EType.Ally => "GUI_ALLY", 
			CActor.EType.Enemy2 => "GUI_ENEMY2", 
			CActor.EType.Neutral => "GUI_NEUTRAL", 
			CActor.EType.Unknown => string.Empty, 
			_ => throw new ArgumentOutOfRangeException("actorType", actorType, null), 
		};
	}

	public static string GetMonsterTypeKey(EMonsterType monsterType)
	{
		return monsterType switch
		{
			EMonsterType.None => string.Empty, 
			EMonsterType.Monster => "GUI_MONSTER_CARD", 
			EMonsterType.Boss => "Boss", 
			EMonsterType.Object => "Object", 
			_ => throw new ArgumentOutOfRangeException("monsterType", monsterType, null), 
		};
	}

	public static string GetLootTypeKey(LootCard.EType lootType)
	{
		return lootType switch
		{
			LootCard.EType.None => string.Empty, 
			LootCard.EType.Money => "Gold", 
			LootCard.EType.Lumber => "RESOURCE_Lumber", 
			LootCard.EType.Metal => "RESOURCE_Metal", 
			LootCard.EType.Hide => "RESOURCE_Hide", 
			LootCard.EType.Arrowvine => "RESOURCE_Arrowvine", 
			LootCard.EType.Axenut => "RESOURCE_Axenut", 
			LootCard.EType.Corpsecap => "RESOURCE_Corpsecap", 
			LootCard.EType.Flamefruit => "RESOURCE_Flamefruit", 
			LootCard.EType.Rockroot => "RESOURCE_Rockroot", 
			LootCard.EType.Snowthistle => "RESOURCE_Snowthistle", 
			LootCard.EType.RandomItem => string.Empty, 
			_ => throw new ArgumentOutOfRangeException("lootType", lootType, null), 
		};
	}

	public static string GetClassTMPIcon(string classID)
	{
		return GetClassTMPIcon(CrossReference.ClassIdToECharacterModel(classID));
	}

	public static string GetClassTMPIcon(CharacterClassID classID)
	{
		return GetClassTMPIcon(CrossReference.ClassIdToECharacterModel(classID.ID));
	}

	public static string GetClassTMPIcon(ECharacter character)
	{
		return character switch
		{
			ECharacter.Infuser => "Infuser", 
			ECharacter.Blinkblade => "Blinkblade", 
			ECharacter.Boneshaper => "Boneshaper", 
			ECharacter.BannerSpear => "BannerSpear", 
			ECharacter.Pyroclast => "Pyroclast", 
			ECharacter.Drifter => "Drifter", 
			ECharacter.Deathwalker => "Deathwalker", 
			ECharacter.Frozenfist => "FrozenFist", 
			ECharacter.Snowdancer => "Snowdancer", 
			ECharacter.Deepwraith => "Deepwraith", 
			ECharacter.Trapper => "Trapper", 
			ECharacter.Crashingtide => "CrashingTide", 
			ECharacter.Geminate => "Geminate", 
			ECharacter.HIVE => "HIVE", 
			ECharacter.Metalmosaic => "MetalMosaic", 
			ECharacter.Painconduit => "PainConduit", 
			ECharacter.Shattersong => "Shattersong", 
			_ => string.Empty, 
		};
	}

	public static string GetFavorName(ScenarioFavorType favor)
	{
		return favor switch
		{
			ScenarioFavorType.Knowledge => "FAVOR_Knowledge_Name", 
			ScenarioFavorType.Wealth => "FAVOR_Wealth_Name", 
			ScenarioFavorType.Strategy => "FAVOR_Strategy_Name", 
			ScenarioFavorType.Capacity => "FAVOR_Knowledge_Name", 
			ScenarioFavorType.Discovery => "FAVOR_Discovery_Name", 
			_ => string.Empty, 
		};
	}

	public static string GetFavorDescription(ScenarioFavorType favor)
	{
		return favor switch
		{
			ScenarioFavorType.Knowledge => "FAVOR_Knowledge_Desc", 
			ScenarioFavorType.Wealth => "FAVOR_Wealth_Desc", 
			ScenarioFavorType.Strategy => "FAVOR_Strategy_Desc", 
			ScenarioFavorType.Capacity => "FAVOR_Knowledge_Desc", 
			ScenarioFavorType.Discovery => "FAVOR_Discovery_Desc", 
			_ => string.Empty, 
		};
	}
}

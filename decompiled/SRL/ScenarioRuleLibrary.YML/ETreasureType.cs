using System;
using FH.Tools;

namespace ScenarioRuleLibrary.YML;

[Serializable]
[KronosEnumPolice(1)]
public enum ETreasureType
{
	None,
	Gold,
	Item,
	Card,
	XP,
	Condition,
	Enhancement,
	Perk,
	Infuse,
	Damage,
	Prosperity,
	UnlockLocation,
	UnlockMerchant,
	UnlockEnhancer,
	UnlockTrainer,
	UnlockTemple,
	UnlockQuest,
	UnlockChapter,
	UnlockCharacter,
	ItemStock,
	EnhancementSlots,
	UnlockPartyUI,
	PerkPoint,
	UnlockProsperityItem,
	UnlockProsperityItemStock,
	UnlockMultiplayer,
	EnemyCondition,
	Reputation,
	PerkCheck,
	Discard,
	CityEvent,
	RoadEvent,
	ConsumeItem,
	LoseItem,
	AddModifiers,
	ItemDesign,
	Resource,
	ItemBlueprint,
	SoldItem,
	EmptyLootDeck,
	EmpyRandomItemDeck,
	EmptyBlueprintDeck,
	EmptyScenarioDeck
}

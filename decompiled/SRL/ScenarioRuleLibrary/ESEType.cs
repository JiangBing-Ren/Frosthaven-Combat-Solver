using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESEType
{
	None,
	Internal,
	Phase,
	Actor,
	Item,
	Action,
	Ability,
	AttackModifier,
	ObjectProp,
	Element,
	Donate,
	Perk,
	Enhancement,
	PersonalQuest,
	Round,
	LoseCard,
	DiscardCard,
	RandomSideScenarioLooted,
	CampaignSectionTriggered,
	CustomSEvent
}

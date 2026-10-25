using System;

namespace ScenarioRuleLibrary.YML;

[Flags]
public enum EPerkFlag
{
	None = 0,
	Passive = 1,
	IgnoreNegativeItemEffects = 2,
	IgnoreMinusOneItemEffects = 4,
	IgnoreNegativeScenarioEffects = 8,
	ExecuteAbilitiesAfterLongRest = 0x10,
	ExecuteAbilitiesAfterRoomRevealed = 0x20,
	ApplyActiveBonusesAtScenarioStart = 0x40,
	ApplyOverridesAfterRoomRevealed = 0x80,
	OnNegativeCondition = 0x100,
	OnNegativeConditionCastBySelf = 0x200,
	AddOneAdditionalOneHandSlot = 0x400,
	ExecuteAbilitiesAfterShortRest = 0x800,
	IgnoreIceSlideEffect = 0x1000,
	ChooseToNotTriggerTrap = 0x2000,
	GetLootCardEndOfScenario = 0x4000,
	All = -1
}

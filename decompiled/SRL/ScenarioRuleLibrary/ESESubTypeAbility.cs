using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(1)]
public enum ESESubTypeAbility
{
	None,
	AbilityStart,
	AbilityPerform,
	AbilityPerformImmediate,
	AbilityTileSelected,
	AbilityComplete,
	AbilityEnded,
	AbilityReset,
	AbilityRestart,
	AbilityApplySingleTargetItem,
	AbilityOverrideAbilityValues,
	AbilityUndoOverride,
	ApplyToActor,
	ActorIsApplying,
	AbilityApplySingleTargetActiveBonus,
	AbilityTileDeselected,
	AbilityTargetsChanged
}

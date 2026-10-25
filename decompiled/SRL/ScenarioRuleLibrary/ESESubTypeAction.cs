using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESESubTypeAction
{
	None,
	ActionInitialized,
	ActionPerform,
	ActionUndoItemAbility,
	ActionNextStep,
	ActionEndAbilitySynchronise,
	ActionOnTileSelected,
	ActionOnStepComplete,
	ActionInjectAbility,
	ActionInjectXP,
	ActionInjectInfusions,
	ActionStackItemAbilities,
	ActionStackInlineSubAbilities,
	ActionStackNextAbilities,
	ActionUnstackItemAbilities,
	ActionAddConsumeAbilities,
	ActionOnFirstAbilityStarted,
	ActionUpdateItemsUsedInPhase,
	ActionRemoveConsumeAbilities,
	ActionOnTileDeselected,
	ActionOnApplySingleTarget,
	ActionEnded
}
